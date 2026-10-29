using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

// Editor-only study: no production scene, sim, save or runtime renderer references.
[InitializeOnLoad]
public static class SuzuStudy
{
    public static Camera StudyCamera;
    static SuzuStudy() { AssemblyReloadEvents.beforeAssemblyReload += Close; }
    public static void Close()
    {
        if (StudyCamera == null) return;
        var scene = StudyCamera.gameObject.scene;
        foreach (var root in scene.GetRootGameObjects())
            foreach (var renderer in root.GetComponentsInChildren<SpriteRenderer>()) {
                Object.DestroyImmediate(renderer.sprite.texture);
                Object.DestroyImmediate(renderer.sprite);
                Object.DestroyImmediate(renderer.sharedMaterial);
            }
        EditorSceneManager.ClosePreviewScene(scene);
        StudyCamera = null;
    }
    public static void Capture(string name, int width = 960, int height = 600)
    {
        Close();
        Open();
        var rt = new RenderTexture(width,height,24);
        var previous = RenderTexture.active;
        var texture = new Texture2D(width,height,TextureFormat.RGBA32,false);
        try {
            StudyCamera.targetTexture = rt;
            StudyCamera.Render();
            RenderTexture.active = rt;
            texture.ReadPixels(new Rect(0,0,width,height),0,0);
            texture.Apply();
            Directory.CreateDirectory("docs/art/suzu-modular/evidence");
            File.WriteAllBytes("docs/art/suzu-modular/evidence/"+name+".png",texture.EncodeToPNG());
        } finally {
            StudyCamera.targetTexture=null;
            RenderTexture.active=previous;
            Object.DestroyImmediate(texture);
            Object.DestroyImmediate(rt);
        }
    }
    public static void Open()
    {
        Close();
        var scene = EditorSceneManager.NewPreviewScene();

        var cameraGo = new GameObject("Suzu Study Camera");
        SceneManager.MoveGameObjectToScene(cameraGo, scene);
        var camera = cameraGo.AddComponent<Camera>();
        StudyCamera = camera;
        camera.scene = scene;
        camera.orthographic = true;
        camera.orthographicSize = 3;
        camera.transform.position = new Vector3(0, 0, -10);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.22f, .18f, .20f);
        camera.cullingMask = 1 << 30;
        var files = Directory.GetFiles("Assets/Experiments/SuzuModular/Parts", "*.png");
        System.Array.Sort(files, System.StringComparer.Ordinal);
        for (int sample = 0; sample < 3; sample++)
        {
            var root = new GameObject("Suzu " + sample);
            SceneManager.MoveGameObjectToScene(root, scene);
            root.transform.position = new Vector3(-2.5f + sample * 2.5f, -1.7f, 0);
            root.transform.localScale = Vector3.one * (sample == 0 ? 3 : sample == 1 ? 1 : .67f);
            for (int i = 0; i < files.Length; i++)
            {
                var texture = new Texture2D(2,2);
                texture.LoadImage(File.ReadAllBytes(files[i]));
                texture.filterMode = FilterMode.Point;
                texture.hideFlags = HideFlags.HideAndDontSave;
                var name = Path.GetFileNameWithoutExtension(files[i]);
                var pivot = name.Contains("leg") ? new Vector2(50,77) : name.Contains("arm") ? new Vector2(40,50) : name.Contains("tail") ? new Vector2(38,64) : name.Contains("skirt") ? new Vector2(50,61) : new Vector2(49,46);
                var sprite = Sprite.Create(texture, new Rect(0,0,192,224), new Vector2(pivot.x/96, 1-pivot.y/112),224);
                sprite.hideFlags = HideFlags.HideAndDontSave;
                var part = new GameObject(Path.GetFileNameWithoutExtension(files[i]));
                part.layer = 30;
                part.transform.SetParent(root.transform, false);
                part.transform.localPosition = new Vector3((pivot.x-48)/112,(106-pivot.y)/112,0);
                var renderer = part.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingOrder = i;
                renderer.sharedMaterial = new Material(Shader.Find("Sprites/Default")) {hideFlags=HideFlags.HideAndDontSave};
            }
        }
    }
}
