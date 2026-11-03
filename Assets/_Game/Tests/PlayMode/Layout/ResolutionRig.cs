using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Neko.Game.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Tests.PlayMode.Layout
{
    /// <summary>
    /// Gives the game a window of a chosen size in batch mode, and checks that the canvas took it.
    /// </summary>
    /// <remarks>
    /// Batch mode has no window to resize, so the main camera gets a render texture of the
    /// wanted size and keeps it for the whole measurement. The camera's aspect follows the
    /// texture, so CameraRig frames the room as it would in a real window of that size.
    /// Whether the Screen Space - Camera canvas follows the texture as well is decided in
    /// native code that the uGUI source does not show. That is why every measurement is
    /// compared with the scaler model (Requirement 20.2) before anything is measured on it.
    ///
    /// This is the primary path of the design's prototype: the texture stays assigned and the
    /// normal player loop sizes the canvas. The prototype's decision point kept it, so it is the
    /// only path the rig runs, and <see cref="ActivePath"/> names it. Every applied window also
    /// regenerates the labels, which uGUI does not do for a scale change alone (task 1.3).
    /// Restore gives the camera its previous target back (Requirement 20.7).
    /// </remarks>
    public static class ResolutionRig
    {
        /// <summary>Largest accepted difference between the canvas scale factor and the model.</summary>
        /// <remarks>uGUI itself ignores scale changes below 0.000005.</remarks>
        public const float ScaleTolerance = 0.0001f;

        /// <summary>Largest accepted difference between the canvas size and the model, in Canvas_Units.</summary>
        public const float SizeTolerance = 0.01f;

        /// <summary>Frames of the normal player loop run with the texture assigned before measuring.</summary>
        private const int SettleFrames = 2;

        /// <summary>The number of Text components <see cref="Apply"/> last regenerated.</summary>
        public static int RegeneratedTexts { get; private set; }

        private static bool _applied;
        private static Camera _camera;
        private static RenderTexture _previousTarget;
        private static Vector2Int _previousViewport;
        private static RenderTexture _target;

        /// <summary>The window currently applied, in pixels; zero when none is.</summary>
        public static Vector2Int Window { get; private set; }

        /// <summary>The canvas as the last <see cref="Apply"/> measured it.</summary>
        public static CanvasMeasurement Last { get; private set; }

        /// <summary>The path of the design's prototype that sizes the canvas, for the report header.</summary>
        /// <remarks>
        /// The decision point (task 1.2) kept the primary path: in batch mode its canvas matched
        /// the scaler model at 960×600, at 1280×720 and at 960×600 again, and each capture showed
        /// the whole window. <see cref="Apply"/> therefore runs the primary path and sets Primary.
        /// </remarks>
        public static RigPath ActivePath { get; private set; } = RigPath.Primary;

        /// <summary>
        /// Gives the main camera a <paramref name="width"/> by <paramref name="height"/> target
        /// and measures the game canvas in it.
        /// </summary>
        /// <remarks>
        /// Runs two frames of the normal player loop with the texture assigned, marks every
        /// active Text on the canvas dirty, calls <see cref="Canvas.ForceUpdateCanvases"/>, runs
        /// one more frame, and records the canvas in <see cref="Last"/>. The comparison with the
        /// model is <see cref="AssertMatchesModel"/>,
        /// called once every window of a test is measured, so a failing run still reports each
        /// window.
        ///
        /// The Texts are marked dirty because a window opened at this size rasterises every
        /// label at its scale factor, but uGUI regenerates a Text only when the Text is dirtied,
        /// and a new scale factor alone does not dirty it: a label whose rect keeps its size keeps
        /// the glyphs it was generated with in an earlier window. The probes of task 1.3 measured
        /// that on Empresa. Without this step the chemistry line drew a line 430 pixels wide at
        /// 960×600, where its mesh at 960×600 is 447 pixels wide. Of the graphics, only Text
        /// depends on the scale factor, and no layout of the game sizes itself from a Text's
        /// preferred size, so this moves no layout.
        /// </remarks>
        public static IEnumerator Apply(int width, int height)
        {
            var camera = Camera.main;
            Assert.IsTrue(camera != null,
                "ResolutionRig: there is no main camera to give a " + Size(width, height) + " window to");

            var canvas = GameCanvas();
            Assert.AreEqual(RenderMode.ScreenSpaceCamera, canvas.renderMode,
                "ResolutionRig: the game canvas does not render through a camera, so a camera target cannot size it");
            Assert.IsTrue(canvas.worldCamera == camera,
                "ResolutionRig: the game canvas renders through " + canvas.worldCamera + ", not the main camera");

            if (!_applied)
            {
                _camera = camera;
                _previousTarget = camera.targetTexture;
                _previousViewport = new Vector2Int(camera.pixelWidth, camera.pixelHeight);
                _applied = true;
            }
            else
            {
                Assert.IsTrue(_camera == camera, "ResolutionRig: the main camera changed during a measurement");
            }

            var replaced = _target;
            _target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
            {
                name = "ResolutionRig " + Size(width, height),
            };
            camera.targetTexture = _target;
            ActivePath = RigPath.Primary;
            Release(replaced);
            Window = new Vector2Int(width, height);

            for (var i = 0; i < SettleFrames; i++) yield return null;

            var texts = canvas.GetComponentsInChildren<Text>();
            foreach (var text in texts) text.SetAllDirty();
            RegeneratedTexts = texts.Length;
            Canvas.ForceUpdateCanvases();
            yield return null;

            Last = Measure(canvas, camera);
        }

        /// <summary>
        /// Fails, naming every mismatch, when a measurement differs from the scaler model.
        /// </summary>
        public static void AssertMatchesModel(IEnumerable<CanvasMeasurement> measurements)
        {
            var mismatches = new List<string>();
            foreach (var measurement in measurements)
            {
                if (!measurement.MatchesModel) mismatches.Add(measurement.ToString());
            }

            if (mismatches.Count == 0) return;

            Assert.Fail("The canvas differs from the scaler model in " + mismatches.Count +
                        " measurement(s):\n" + string.Join("\n", mismatches));
        }

        /// <summary>
        /// Renders the camera once into the applied window and returns the frame as a PNG.
        /// </summary>
        /// <remarks>
        /// Batch mode never reaches the end of a frame, so there is no back buffer to read. The
        /// camera renders on demand into its target instead, as in CaptureTests.
        /// </remarks>
        public static byte[] Capture()
        {
            var frame = Render("Capture");

            try
            {
                var png = ImageConversion.EncodeToPNG(frame);
                Assert.IsTrue(png != null && png.Length > 0,
                    "ResolutionRig.Capture: the " + Size(frame.width, frame.height) + " frame did not encode");
                return png;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(frame);
            }
        }

        /// <summary>
        /// Renders the camera once into the applied window and returns its pixels, bottom row
        /// first, as <see cref="Capture"/> would encode them.
        /// </summary>
        /// <remarks>
        /// For measurements that compare two frames pixel by pixel. Alpha is 255 everywhere.
        /// </remarks>
        public static Color32[] Frame()
        {
            var frame = Render("Frame");

            try
            {
                return frame.GetPixels32();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(frame);
            }
        }

        /// <summary>Renders the camera into the applied window and reads the frame back.</summary>
        /// <remarks>The caller destroys the returned texture.</remarks>
        private static Texture2D Render(string caller)
        {
            Assert.IsTrue(_applied && _target != null, "ResolutionRig." + caller + ": no window is applied");
            Assert.IsTrue(_camera != null,
                "ResolutionRig." + caller + ": the camera of the " + Size(Window.x, Window.y) + " window is gone");

            var width = _target.width;
            var height = _target.height;

            // RGB24, because a window has no alpha: blended interface pixels would otherwise
            // come out see-through in the file.
            var frame = new Texture2D(width, height, TextureFormat.RGB24, false);
            var previousActive = RenderTexture.active;
            var read = false;

            try
            {
                _camera.Render();

                RenderTexture.active = _target;
                frame.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                frame.Apply(false, false);
                read = true;
                return frame;
            }
            finally
            {
                RenderTexture.active = previousActive;
                if (!read) UnityEngine.Object.DestroyImmediate(frame);
            }
        }

        /// <summary>
        /// Gives the camera back the target it had before the first <see cref="Apply"/>, releases
        /// the texture and asserts the restore.
        /// </summary>
        /// <remarks>
        /// Does nothing when no window is applied, so [TearDown] can always call it. The texture
        /// is released in a finally block, even when the restore throws.
        /// </remarks>
        public static void Restore()
        {
            if (!_applied) return;

            var camera = _camera;
            var previousTarget = _previousTarget;
            var previousViewport = _previousViewport;

            try
            {
                if (camera != null) camera.targetTexture = previousTarget;
            }
            finally
            {
                Release(_target);
                _target = null;
                _camera = null;
                _previousTarget = null;
                _applied = false;
                Window = Vector2Int.zero;
                Last = null;
            }

            // A camera destroyed with its scene no longer holds a texture of the rig.
            if (camera == null) return;

            Assert.IsTrue(camera.targetTexture == previousTarget,
                "ResolutionRig.Restore: the camera did not get its previous target back");
            Assert.AreEqual(previousViewport, new Vector2Int(camera.pixelWidth, camera.pixelHeight),
                "ResolutionRig.Restore: the camera's viewport is not the size it had before the first window");
        }

        /// <summary>
        /// The scale factor that uGUI's CanvasScaler.HandleScaleWithScreenSize computes for a
        /// window of the given size (com.unity.ugui 2.5.0, display 0).
        /// </summary>
        public static float ModelScaleFactor(Vector2 referenceResolution,
            CanvasScaler.ScreenMatchMode mode, float matchWidthOrHeight, Vector2 window)
        {
            switch (mode)
            {
                case CanvasScaler.ScreenMatchMode.MatchWidthOrHeight:
                {
                    // Averaged in log space, base 2, as uGUI does.
                    var logWidth = Mathf.Log(window.x / referenceResolution.x, 2f);
                    var logHeight = Mathf.Log(window.y / referenceResolution.y, 2f);
                    return Mathf.Pow(2f, Mathf.Lerp(logWidth, logHeight, matchWidthOrHeight));
                }
                case CanvasScaler.ScreenMatchMode.Expand:
                    return Mathf.Min(window.x / referenceResolution.x, window.y / referenceResolution.y);
                case CanvasScaler.ScreenMatchMode.Shrink:
                    return Mathf.Max(window.x / referenceResolution.x, window.y / referenceResolution.y);
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(mode), mode,
                        "ResolutionRig: no scaler model for this match mode");
            }
        }

        internal static string Size(int width, int height) => width + "x" + height;

        private static CanvasMeasurement Measure(Canvas canvas, Camera camera)
        {
            var scaler = canvas.GetComponent<CanvasScaler>();
            Assert.IsTrue(scaler != null, "ResolutionRig: the game canvas has no CanvasScaler to model");
            Assert.AreEqual(CanvasScaler.ScaleMode.ScaleWithScreenSize, scaler.uiScaleMode,
                "ResolutionRig: the scaler model covers ScaleWithScreenSize only");

            // Read from the scaler now rather than copied from UiRoot, so the model follows the game.
            var window = new Vector2(Window.x, Window.y);
            var modelScale = ModelScaleFactor(scaler.referenceResolution, scaler.screenMatchMode,
                scaler.matchWidthOrHeight, window);

            return new CanvasMeasurement(
                Window,
                canvas.scaleFactor,
                ((RectTransform)canvas.transform).rect.size,
                modelScale,
                window / modelScale,
                canvas.renderingDisplaySize,
                new Vector2Int(camera.pixelWidth, camera.pixelHeight),
                new Vector2Int(UnityEngine.Screen.width, UnityEngine.Screen.height));
        }

        private static Canvas GameCanvas()
        {
            var root = GameRoot.Instance;
            Assert.IsTrue(root != null, "ResolutionRig: GameRoot is not running, so there is no game canvas");

            var canvas = root.Ui != null ? root.Ui.GetComponent<Canvas>() : null;
            Assert.IsTrue(canvas != null, "ResolutionRig: the game interface has no Canvas");
            return canvas;
        }

        private static void Release(RenderTexture texture)
        {
            if (texture == null) return;

            texture.Release();
            UnityEngine.Object.DestroyImmediate(texture);
        }
    }

    /// <summary>The two ways the design's prototype can give the canvas a window's size in batch mode.</summary>
    public enum RigPath
    {
        /// <summary>The texture stays assigned and the normal player loop sizes the canvas.</summary>
        Primary = 0,

        /// <summary>
        /// The scaler is disabled, the scale factor is set from the scaler model, and the canvas
        /// follows the texture on <c>camera.Render()</c>. Not implemented, because the primary
        /// path passed.
        /// </summary>
        Fallback = 1,
    }

    /// <summary>
    /// The game canvas in one applied window, next to what the scaler model expects there.
    /// </summary>
    public sealed class CanvasMeasurement
    {
        public CanvasMeasurement(Vector2Int window, float scaleFactor, Vector2 canvasSize,
            float modelScaleFactor, Vector2 modelCanvasSize, Vector2 renderingDisplaySize,
            Vector2Int cameraPixels, Vector2Int screenPixels)
        {
            Window = window;
            ScaleFactor = scaleFactor;
            CanvasSize = canvasSize;
            ModelScaleFactor = modelScaleFactor;
            ModelCanvasSize = modelCanvasSize;
            RenderingDisplaySize = renderingDisplaySize;
            CameraPixels = cameraPixels;
            ScreenPixels = screenPixels;
        }

        /// <summary>The applied window, in pixels.</summary>
        public Vector2Int Window { get; }

        /// <summary>The measured <c>canvas.scaleFactor</c>.</summary>
        public float ScaleFactor { get; }

        /// <summary>The measured canvas rect size, in Canvas_Units.</summary>
        public Vector2 CanvasSize { get; }

        public float ModelScaleFactor { get; }

        /// <summary>The window divided by the model scale factor, in Canvas_Units.</summary>
        public Vector2 ModelCanvasSize { get; }

        /// <summary>The size the scaler read. With the camera and Screen sizes, it shows where a wrong size came from.</summary>
        public Vector2 RenderingDisplaySize { get; }

        public Vector2Int CameraPixels { get; }

        public Vector2Int ScreenPixels { get; }

        public bool MatchesModel =>
            Mathf.Abs(ScaleFactor - ModelScaleFactor) <= ResolutionRig.ScaleTolerance &&
            Mathf.Abs(CanvasSize.x - ModelCanvasSize.x) <= ResolutionRig.SizeTolerance &&
            Mathf.Abs(CanvasSize.y - ModelCanvasSize.y) <= ResolutionRig.SizeTolerance;

        public override string ToString() =>
            ResolutionRig.Size(Window.x, Window.y) +
            " (" + ResolutionRig.ActivePath.ToString().ToLowerInvariant() + " path): scaleFactor " +
            Scale(ScaleFactor) + ", model " + Scale(ModelScaleFactor) +
            "; canvas " + Units(CanvasSize) + ", model " + Units(ModelCanvasSize) +
            "; renderingDisplaySize " + Units(RenderingDisplaySize) +
            ", camera " + ResolutionRig.Size(CameraPixels.x, CameraPixels.y) + " px" +
            ", Screen " + ResolutionRig.Size(ScreenPixels.x, ScreenPixels.y) + " px";

        private static string Scale(float value) => value.ToString("0.#####", CultureInfo.InvariantCulture);

        private static string Units(Vector2 value) =>
            value.x.ToString("0.##", CultureInfo.InvariantCulture) + "x" +
            value.y.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
