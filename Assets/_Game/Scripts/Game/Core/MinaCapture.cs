#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Neko.Game.Ui;
using Neko.Game.View;
using Neko.Sim.Progress;
using Neko.Sim.Team;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Neko.Game.Core
{
    // Explicit, Editor-only evidence run. Never enabled in the player or ordinary boot.
    public sealed class MinaCapture : MonoBehaviour
    {
        private float _deadline;
        private void Awake()
        {
            _deadline = Time.realtimeSinceStartup + 150f;
            Application.logMessageReceived += OnLog;
        }
        private void OnDestroy() => Application.logMessageReceived -= OnLog;
        private void Update()
        {
            if (Time.realtimeSinceStartup > _deadline) EditorApplication.Exit(1);
        }
        private void OnLog(string message, string stack, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error) EditorApplication.Exit(1);
        }

        public static void Begin()
        {
            if (!Application.isBatchMode || !SaveBridge.Isolated)
                throw new InvalidOperationException("Capture requires isolated Editor batch mode.");
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Boot.unity");
            EditorApplication.isPlaying = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartCapture()
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NEKO_MINA_CAPTURE_DIR"))) return;
            if (!Application.isBatchMode || !SaveBridge.Isolated)
                throw new InvalidOperationException("Refusing capture without save isolation.");
            new GameObject("Mina evidence capture").AddComponent<MinaCapture>().StartCoroutine(Run());
        }

        private static IEnumerator Run()
        {
            for (var i = 0; i < 12; i++) yield return null;
            var root = GameRoot.Instance;
            root.Ui.Get<MenuScreen>().OnNewGame.Invoke();
            root.Ui.Get<MenuScreen>().OnPlay.Invoke();
            var maid = root.Session.Profile.Maids.First(m => m.NaturalRole == Role.Floor);
            maid.Name = "Mina";
            for (var i = 0; i < maid.Gear.Length; i++) maid.Gear[i] = null;
            var companion = root.Session.Profile.Maids.FirstOrDefault(m => m != maid);
            if (companion != null) companion.Name = "Suzu";
            root.Ui.Show<HubScreen>();
            root.Ui.Get<HubScreen>().OnOpenDay.Invoke();
            yield return new WaitForSeconds(1f);
            var view = UnityEngine.Object.FindObjectsByType<MaidView>(FindObjectsInactive.Exclude)
                .First(v => v.Worker.Maid == maid);
            // Natural simulation evidence precedes controlled visual probes.
            for (var frame = 0; frame < 120; frame++)
            {
                if (frame % 20 == 0) yield return Shot("natural_" + frame, root.Camera.Camera);
                yield return null;
            }
            root.enabled = false; // Suspend simulation for controlled visual probes.
            Clocks.Pause(true);
            var worker = view.Worker;
            worker.Claimed = null;
            worker.Carrying = false;
            worker.OnBreak = false;
            maid.Mood = 1f;
            var directions = new[] { Vector2.right, Vector2.left, Vector2.up, Vector2.down,
                new Vector2(1, 1), new Vector2(1, 0.95f), Vector2.zero };
            var labels = new[] { "right", "left", "back", "front", "diagonal", "diagonal_near", "stop" };
            for (var i = 0; i < directions.Length; i++)
            {
                // Controlled positions are in-memory probes, not proof of pathfinding.
                Clocks.Pause(false);
                for (var until = Time.realtimeSinceStartup + 0.5f; Time.realtimeSinceStartup < until;)
                {
                    worker.X += directions[i].x * Clocks.Game;
                    worker.Y += directions[i].y * Clocks.Game;
                    view.Sync();
                    yield return null;
                }
                Clocks.Pause(true);
                var body = view.transform.Find("Body").GetComponent<SpriteRenderer>();
                var expected = new[] { "Mina_side", "Mina_side", "Mina_back", "Mina_walk", "Mina_back", "Mina_back", "Mina_back" }[i];
                if (body.sprite.name != expected || (i < 2 && body.flipX != (i == 1)))
                    throw new InvalidOperationException("Direction probe failed: " + labels[i] + " " + body.sprite.name);
                yield return Shot("probe_" + labels[i], root.Camera.Camera);
            }
            worker.X = 1.5f;
            worker.Y = 2.7f;
            for (var until = Time.realtimeSinceStartup + 0.5f; Time.realtimeSinceStartup < until;)
            {
                view.Sync();
                yield return null;
            }
            worker.Carrying = true;
            view.Sync();
            yield return Shot("probe_carry", root.Camera.Camera);
            worker.Carrying = false;
            worker.Claimed = new Neko.Sim.Shift.ShiftTask(-1, Neko.Sim.Shift.TaskKind.Cook, -1);
            view.Sync();
            yield return Shot("probe_cook", root.Camera.Camera);
            worker.Claimed = null;
            worker.OnBreak = true;
            view.Sync();
            yield return Shot("probe_tired", root.Camera.Camera);
            Debug.Log("MINA_CAPTURE_COMPLETE: isolated save; natural frames and controlled probes.");
            EditorApplication.Exit(0);
        }

        private static IEnumerator Shot(string name, Camera camera)
        {
            foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(960, 600) })
            {
                var target = new RenderTexture(size.x, size.y, 24);
                var texture = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);
                var previous = RenderTexture.active;
                var previousTarget = camera.targetTexture;

                try
                {
                    camera.targetTexture = target;
                    camera.aspect = (float)size.x / size.y;
                    for (var i = 0; i < 12; i++) yield return null;
                    Canvas.ForceUpdateCanvases();
                    camera.Render();
                    RenderTexture.active = target;
                    texture.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
                    texture.Apply();
                    var dir = Environment.GetEnvironmentVariable("NEKO_MINA_CAPTURE_DIR");
                    Directory.CreateDirectory(dir);
                    File.WriteAllBytes(Path.Combine(dir, name + "_" + size.x + "x" + size.y + ".png"), texture.EncodeToPNG());
                    foreach (var view in UnityEngine.Object.FindObjectsByType<MaidView>(FindObjectsInactive.Exclude))
                    {
                        var body = view.transform.Find("Body").GetComponent<SpriteRenderer>();
                        Debug.Log("MINA_EVIDENCE " + name + " " + view.Worker.Name + " sprite=" + body.sprite.name + " flip=" + body.flipX + " position=" + view.transform.position);
                    }
                }
                finally
                {
                    camera.targetTexture = previousTarget;
                    camera.ResetAspect();
                    RenderTexture.active = previous;
                    UnityEngine.Object.Destroy(texture);
                    UnityEngine.Object.Destroy(target);
                }
            }
        }
    }
}
#endif
