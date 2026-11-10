// Editor-only capture probe for ART-INT-01. Not part of the game: copy to Assets/Editor/ to run,
//   Unity -batchmode -projectPath <project> -executeMethod DepthProbe.Run
// Output: <project>/qa_output/*.png. Builds the real RoomView and CustomerView on a 13x9 room and
// puts characters at chosen floor positions with the same DepthSort rule the views use.
using System;
using System.IO;
using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Game.View;
using Neko.Sim.Cafe;
using Neko.Sim.Service;
using UnityEditor;
using UnityEngine;

public static class DepthProbe
{
    private static readonly int[][] Sizes = { new[] { 960, 600 }, new[] { 1280, 720 } };

    public static void Run()
    {
        try
        {
            // Illustrated pieces, then the procedural look of the same room for comparison.
            Scene("pilot", true);
            Scene("procedural", false);
            Debug.Log("DEPTH_PROBE_COMPLETED");
        }
        catch (Exception e)
        {
            Debug.LogError("DEPTH_PROBE_FAILED: " + e);
            EditorApplication.Exit(1);
            return;
        }

        EditorApplication.Exit(0);
    }

    private static void Scene(string label, bool illustrated)
    {
        IllustratedFurniture.Enabled = illustrated;

        var grid = new CafeGrid(13, 9);
        Place(grid, "stage", 2, 3);
        Place(grid, "koi_pond", 6, 3);
        Place(grid, "window", 10, 3);
        Place(grid, "table_4", 9, 6);
        Place(grid, "plant", 5, 6);

        var sprites = new SpriteFactory();
        var room = new GameObject("QA Room").AddComponent<RoomView>();
        room.Bind(sprites);
        room.Build(grid);

        var cameraGo = new GameObject("QA Camera");
        var cam = cameraGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Palette.Darken(Palette.Wall, .45f);
        cam.nearClipPlane = -20;
        cam.farClipPlane = 50;
        cam.transform.position = new Vector3(6, 4, -10);
        cam.orthographicSize = Mathf.Max((9 + 1.8f) / 2f, (13 + 1.8f) / 2f / (4f / 3f));

        var entry = MaidAnimCatalog.ById("suzu");
        var frames = MaidAnimCatalog.Load(sprites, entry);
        if (frames == null) throw new Exception("maid sheet unavailable");

        // Each spot: x, feet y. Chosen around stage (cells 2-3 x 3-4), pond (6-7 x 3-4), window (10, 3).
        var spots = new[]
        {
            new Vector2(2.5f, 1.8f),   // in front of the stage
            new Vector2(2.5f, 5.6f),   // behind the stage
            new Vector2(2.5f, 3.6f),   // on the stage's cells
            new Vector2(6.5f, 1.8f),   // in front of the pond
            new Vector2(5.6f, 3.4f),   // beside the pond, overlapping its left rocks
            new Vector2(6.5f, 5.6f),   // behind the pond
            new Vector2(10f, 1.9f),    // in front of the window
            new Vector2(10f, 4.9f),    // behind the window
            new Vector2(9.5f, 6.6f),   // seated at the table
            new Vector2(5f, 7.2f),     // behind the fern
            new Vector2(5f, 5.5f),     // in front of the fern
        };

        foreach (var spot in spots)
        {
            var feet = new Vector3(spot.x, spot.y, 0f);
            var maid = new GameObject("QA Maid").AddComponent<SpriteRenderer>();
            maid.sprite = frames[entry.idle_frame];
            maid.transform.position = feet + entry.BodyOffset;
            Layers.ApplyActor(maid, DepthSort.ActorKey(grid, feet));
        }

        foreach (var size in Sizes) Capture(cam, label, size[0], size[1]);
        // The very first render is discarded as shader warm-up; capture twice is cheap.
        foreach (var size in Sizes) Capture(cam, label, size[0], size[1]);

        UnityEngine.Object.DestroyImmediate(room.gameObject);
        UnityEngine.Object.DestroyImmediate(cameraGo);
        foreach (var go in GameObject.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
        {
            if (go != null) UnityEngine.Object.DestroyImmediate(go.gameObject);
        }

        sprites.Clear();
        IllustratedFurniture.Enabled = true;
    }

    private static void Place(CafeGrid grid, string id, int x, int y)
    {
        if (!grid.Place(PlaceableCatalog.ById(id), x, y)) throw new Exception("place " + id);
    }

    private static void Capture(Camera cam, string label, int w, int h)
    {
        var target = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
        var prev = RenderTexture.active;
        cam.targetTexture = target;
        cam.aspect = (float)w / h;
        cam.Render();
        RenderTexture.active = target;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();
        var dir = Path.GetFullPath(Path.Combine(Application.dataPath, "../qa_output"));
        Directory.CreateDirectory(dir);
        File.WriteAllBytes(Path.Combine(dir, label + "_" + w + "x" + h + ".png"), tex.EncodeToPNG());
        cam.targetTexture = null;
        RenderTexture.active = prev;
        UnityEngine.Object.DestroyImmediate(tex);
        target.Release();
        UnityEngine.Object.DestroyImmediate(target);
    }
}
