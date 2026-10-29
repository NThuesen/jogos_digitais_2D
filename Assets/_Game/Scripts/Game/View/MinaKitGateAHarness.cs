#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using Neko.Game.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Game.View
{
    /// <summary>Explicit temporary scene. Never bootstraps GameRoot, a session or PlayerPrefs.</summary>
    public sealed class MinaKitGateAHarness : MonoBehaviour
    {
        [Serializable] private sealed class JointEvidence
        {
            public string name, parent;
            public Vector3 localPosition, localEulerAngles, worldPosition;
        }
        [Serializable] private sealed class SceneEvidence
        {
            public string pose;
            public int width, height, renderers;
            public Vector3 rootPosition, rootScale;
            public JointEvidence[] joints;
        }
        private Camera _camera;
        private Canvas _canvas;
        private Font _font;
        private MinaModularKit _small, _large;
        private GameObject _board;
        private Sprite _reference;
        private float _deadline;
        private string _output;

        [MenuItem("Neko/Experiments/ANIM-01 Gate A preview")]
        public static void Preview()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit PlayMode before opening Gate A.");
            if (EditorSceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Save the current scene before opening Gate A.");
            PrepareImports();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("ANIM-01 Gate A harness").AddComponent<MinaKitGateAHarness>();
            EditorApplication.isPlaying = true;
        }

        private static void PrepareImports()
        {
            AssetDatabase.Refresh();
            var files = Directory.GetFiles("Assets/_Game/Resources/Anim01GateA", "*.png");
            foreach (var file in files)
            {
                var importer = AssetImporter.GetAtPath(file.Replace('\\', '/')) as TextureImporter;
                if (importer == null) throw new InvalidOperationException("No importer for " + file);
                importer.textureType = TextureImporterType.Default;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.SaveAndReimport();
            }
        }

        public static void Begin()
        {
            if (!Application.isBatchMode || !SaveBridge.Isolated ||
                string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NEKO_GATE_A_OUTPUT")))
                throw new InvalidOperationException("Gate A capture requires batch, isolated save and explicit output.");
            // Let the Editor finish startup callbacks before changing scenes/entering play.
            // Immediate PlayMode on a fresh worktree races Unity Search initialization.
            var ready = EditorApplication.timeSinceStartup + 5d;
            EditorApplication.update += WaitForStartup;
            void WaitForStartup()
            {
                if (EditorApplication.timeSinceStartup < ready) return;
                EditorApplication.update -= WaitForStartup;
                Preview();
            }
        }

        private void Start()
        {
            _deadline = Time.realtimeSinceStartup + 240f;
            _output = Environment.GetEnvironmentVariable("NEKO_GATE_A_OUTPUT");
            Application.logMessageReceived += OnLog;
            if (FindObjectsByType<GameRoot>(FindObjectsInactive.Include).Length != 0)
                throw new InvalidOperationException("Gate A must not contain GameRoot.");
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var cameraObject = new GameObject("Gate A orthographic camera");
            _camera = cameraObject.AddComponent<Camera>();
            _camera.orthographic = true;
            _camera.transform.position = new Vector3(0f, 0f, -10f);
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color32(184, 160, 140, 255);
            _reference = Sprite.Create(Resources.Load<Texture2D>("Anim01GateA/reference"),
                new Rect(0, 0, 1254, 1254), new Vector2(650f / 1254f, 1f - 1215f / 1254f), 1175f);
            Layout(1280, 720);
            SetPose(0);
            if (!string.IsNullOrEmpty(_output)) StartCoroutine(Capture());
        }

        private void Update()
        {
            if (!string.IsNullOrEmpty(_output) && Time.realtimeSinceStartup > _deadline)
                Fail("Gate A capture deadline exceeded.");
        }

        private void OnLog(string message, string trace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception) Fail(message + "\n" + trace);
        }

        private void Fail(string reason)
        {
            if (!string.IsNullOrEmpty(_output))
            {
                Directory.CreateDirectory(_output);
                File.WriteAllText(Path.Combine(_output, "failure.txt"), reason);
            }
            if (Application.isBatchMode) EditorApplication.Exit(1);
        }

        private Vector3 Pixel(float x, float y, int width, int height) =>
            new Vector3(x - width * .5f, y - height * .5f, 0f);

        private void Layout(int width, int height)
        {
            if (_board != null) DestroyImmediate(_board);
            _board = new GameObject("Gate A board — disposable");
            _camera.orthographicSize = height * .5f;
            _camera.aspect = (float)width / height;
            _canvas = new GameObject("Gate A labels").AddComponent<Canvas>();
            _canvas.transform.SetParent(_board.transform, false);
            _canvas.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas.worldCamera = _camera;
            _canvas.planeDistance = 1f;
            _canvas.sortingOrder = 10000;
            var scale = _canvas.gameObject.AddComponent<CanvasScaler>();
            scale.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            var ground = new GameObject("Stable floor reference", typeof(RectTransform)).AddComponent<Image>();
            ground.transform.SetParent(_canvas.transform, false);
            ground.color = new Color(0.25f, 0.15f, 0.08f, .3f);
            ground.raycastTarget = false;
            ground.rectTransform.anchorMin = ground.rectTransform.anchorMax = new Vector2(0f, 0f);
            ground.rectTransform.pivot = Vector2.zero;
            ground.rectTransform.anchoredPosition = new Vector2(16f, 99f);
            ground.rectTransform.sizeDelta = new Vector2(width - 32f, 1f);

            var zoom = height == 720 ? 440f : 350f;
            var sx1 = width * .09f;
            var sx2 = width * .23f;
            var lx1 = width * .53f;
            var lx2 = width * .82f;
            ReferenceAt(sx1, 100f, 100f, width, height);
            ReferenceAt(lx1, 100f, zoom, width, height);
            _small = KitAt(sx2, 100f, 100f, width, height);
            _large = KitAt(lx2, 100f, zoom, width, height);
            Label("ANIM-01 / Gate A — poses estáticas no Unity", 16f, 12f, width - 32f, 28f, 20);
            Label("Original · 100 px", sx1 - 65f, height - 230f, 140f, 30f, 14);
            Label("Modular · 100 px", sx2 - 65f, height - 230f, 150f, 30f, 14);
            Label("Original · ampliado", lx1 - 90f, 78f, 200f, 28f, 16);
            Label("Modular · ampliado", lx2 - 90f, 78f, 200f, 28f, 16);
            Label("Referência de chão: mesma origem em todas as poses. Sem caminhada, bob ou squash.",
                16f, height - 64f, width - 32f, 26f, 14);
        }

        private void ReferenceAt(float x, float y, float scale, int width, int height)
        {
            var renderer = new GameObject("Unmodified keyed reference").AddComponent<SpriteRenderer>();
            renderer.transform.SetParent(_board.transform, false);
            renderer.transform.position = Pixel(x, y, width, height);
            renderer.transform.localScale = Vector3.one * scale;
            renderer.sprite = _reference;
        }

        private MinaModularKit KitAt(float x, float y, float scale, int width, int height)
        {
            var kit = MinaModularKit.Create(_board.transform);
            kit.transform.position = Pixel(x, y, width, height);
            kit.transform.localScale = Vector3.one * scale;
            return kit;
        }

        private void Label(string text, float left, float top, float width, float height, int size,
            Action click = null)
        {
            var label = new GameObject(text, typeof(RectTransform)).AddComponent<Text>();
            label.transform.SetParent(_canvas.transform, false);
            var rect = label.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(left, -top);
            rect.sizeDelta = new Vector2(width, height);
            label.font = _font;
            label.text = text;
            label.fontSize = size;
            label.color = new Color32(50, 31, 23, 255);
            label.raycastTarget = click != null;
            if (click != null) label.gameObject.AddComponent<Button>().onClick.AddListener(() => click());
        }

        private void SetPose(int index)
        {
            _small.ApplyPose(index);
            _large.ApplyPose(index);
            Debug.Log("GATE_A_POSE " + _small.Data.poses[index].id);
        }

        private IEnumerator Capture()
        {
            Directory.CreateDirectory(_output);
            // Warm up actual graphics. Fixed poses, no fixed-time performance claims.
            for (var i = 0; i < 18; i++) yield return null;
            foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(960, 600) })
            {
                Layout(size.x, size.y);
                for (var pose = 0; pose < _small.Data.poses.Length; pose++)
                {
                    SetPose(pose);
                    var definition = _small.Data.poses[pose];
                    Label(definition.label + " — parâmetros experimentais no rig.json", 16f, 44f,
                        size.x - 32f, 26f, 16);
                    for (var i = 0; i < 3; i++) yield return null;
                    yield return Shot(definition.id + "_" + size.x + "x" + size.y, size.x, size.y);
                    // Rebuild board also checks deterministic reassembly and resource disposal.
                    Layout(size.x, size.y);
                }
            }
            File.WriteAllText(Path.Combine(_output, "definition.json"), JsonUtility.ToJson(_small.Data, true));
            File.WriteAllText(Path.Combine(_output, "result.txt"),
                "PASS: 7 static articulated poses x 2 resolutions; independent temporary scene; no GameRoot or save calls.\n" +
                "Aesthetic approval pending. No walking or equipment integration.\n");
            Debug.Log("GATE_A_CAPTURE_COMPLETE");
            EditorApplication.Exit(0);
        }

        private IEnumerator Shot(string name, int width, int height)
        {
            var target = new RenderTexture(width, height, 24);
            var pixels = new Texture2D(width, height, TextureFormat.RGB24, false);
            var previous = RenderTexture.active;
            try
            {
                _camera.targetTexture = target;
                // Discard real target renders, not merely unrendered elapsed frames.
                // The first offscreen frame on this driver had incorrect texture bindings.
                for (var warm = 0; warm < 12; warm++)
                {
                    Canvas.ForceUpdateCanvases();
                    _camera.Render();
                    yield return null;
                }
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                pixels.Apply();
                File.WriteAllBytes(Path.Combine(_output, name + ".png"), pixels.EncodeToPNG());
                var evidence = new SceneEvidence
                {
                    pose = name, width = width, height = height, renderers = _small.RendererCount,
                    rootPosition = _small.transform.position, rootScale = _small.transform.localScale,
                    joints = new JointEvidence[_small.Data.nodes.Length],
                };
                for (var i = 0; i < _small.Data.nodes.Length; i++)
                {
                    var joint = _small.Joint(_small.Data.nodes[i].id);
                    evidence.joints[i] = new JointEvidence
                    {
                        name = joint.name, parent = joint.parent.name,
                        localPosition = joint.localPosition, localEulerAngles = joint.localEulerAngles,
                        worldPosition = joint.position,
                    };
                }
                File.WriteAllText(Path.Combine(_output, name + ".json"), JsonUtility.ToJson(evidence, true));
            }
            finally
            {
                _camera.targetTexture = null;
                RenderTexture.active = previous;
                target.Release();
                Destroy(target);
                Destroy(pixels);
            }
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= OnLog;
            if (_reference != null) Destroy(_reference);
        }
    }
}
#endif
