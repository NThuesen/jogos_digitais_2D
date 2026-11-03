using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using BoxScreen = Neko.Game.Ui.BoxScreen;
using GameRoot = Neko.Game.Core.GameRoot;
using UiRoot = Neko.Game.Ui.UiRoot;

namespace Neko.Tests.PlayMode.Layout
{
    /// <summary>
    /// The Layout_Check's model of what uGUI draws, in Canvas_Units.
    /// </summary>
    /// <remarks>
    /// Canvas_Units are measured in the root canvas's own space, with the origin at the canvas's
    /// bottom-left corner. A window pixel is a Canvas_Unit times <c>canvas.scaleFactor</c>, and
    /// pixel rows count from the bottom, as <see cref="ResolutionRig.Frame"/> returns them.
    ///
    /// The model reads meshes. <see cref="Mesh"/> asks a graphic for the vertices it gives
    /// uGUI, through the protected method that uGUI itself calls, and moves them into
    /// Canvas_Units; <see cref="SampleAlpha"/> reads a sprite's alpha as the GPU filters it. The
    /// prototype probes of task 1.3 (<c>LayoutCheckTests.ProbesAgreeWithTheRenderedFrame</c>)
    /// compared both with rendered frames on Empresa at 960×600 and at 1280×720, and recorded the
    /// two findings below. The comparison holds for a Text only once uGUI has regenerated it at
    /// the window's scale factor, which <see cref="ResolutionRig.Apply"/> makes sure of.
    ///
    /// <see cref="Measure"/> builds the element model on those meshes: the Active_Layer's
    /// elements in draw order, classified from the design's Baseline hierarchy facts (plates,
    /// composites and their background pieces, Decor, Containers and Container_Chains), and its
    /// Interactive_Controls. It recognises the Game_UI only by the names and components the
    /// Baseline already sets, so it runs unchanged on the Baseline and on the fixed interface.
    ///
    /// Each element has a <see cref="VisibleShape"/>: for an Image with a readable sprite, the
    /// part of its mesh where the filtered texture alpha reaches 0.5; for a text, its
    /// Text_Bounds; cut to its <c>RectMask2D</c> ancestors' clip rect. Each measurement also
    /// holds the Room_Grid_Area (<see cref="Measurement.RoomGridArea"/>), the rect that the
    /// room's floor tiles cover on screen.
    /// </remarks>
    public static class LayoutModel
    {
        /// <summary>
        /// Whether uGUI's text generator emits a glyph quad for a whitespace character. It does not.
        /// </summary>
        /// <remarks>
        /// Measured by task 1.3 on Empresa at 960×600 and at 1280×720: 62 labels with text at each
        /// size. The 6 with whitespace ("600 ¥", "198 ¥", "Custo fixo por dia", the level line, the
        /// chemistry line and "Abrir o dia") had one quad for each non-whitespace character and none
        /// for their spaces; each of the other 56 had one quad per character. Rule R3.3 therefore
        /// expects one glyph quad per non-whitespace character. The probe fails if this changes.
        /// </remarks>
        public const bool WhitespaceEmitsGlyphQuads = false;

        /// <summary>
        /// Whether <c>EventSystem.RaycastAll</c> returns results in a batch-mode PlayMode run. It does.
        /// </summary>
        /// <remarks>
        /// Measured by task 1.3 on Empresa: RaycastAll returned results at all 14 control centres
        /// (the 7 planks at 960×600 and at 1280×720), and its first result was the topmost raycast
        /// target in draw order at all 14. In the Editor, when <c>Display.RelativeMouseAt</c>
        /// returns zero, as it did, uGUI puts the pointer on <c>Display.activeEditorGameViewTarget</c>;
        /// batch mode reported 0, the event camera's display, so GraphicRaycaster tested the
        /// graphics. <see cref="Clicks"/> uses RaycastAll directly.
        /// The probe logs a warning, and does not fail, when a run measures something else.
        /// </remarks>
        public const bool RaycastAllAnswersInBatchMode = true;

        private const string SceneryName = "Scenery";
        private const string SpaceName = "Space";
        private const string PanelName = "Panel";
        private const string EarName = "CatEar";

        /// <summary>The name <c>RoomView</c> gives each floor tile's object.</summary>
        private const string TileName = "Tile";

        /// <summary>How far, in Canvas_Units, a full-stretch rect's offsets may sit inside its parent.</summary>
        private const float StretchTolerance = 0.5f;

        /// <summary>A factory <c>ShapeId.RoundedRect</c> sprite: <c>&lt;size&gt;#2|…</c>.</summary>
        private static readonly Regex PlateSpriteName = new Regex(@"^\d+#2\|", RegexOptions.CultureInvariant);

        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        private static MethodInfo _populateMesh;

        /// <summary>
        /// Measures the Active_Layer of <paramref name="ui"/> in the window that
        /// <see cref="ResolutionRig.Apply"/> gave the game: its elements in draw order, classified,
        /// and its Interactive_Controls.
        /// </summary>
        /// <remarks>
        /// Reads the hierarchy as it is laid out now, so the caller settles the Screen first. A
        /// Dialog closed in this frame is destroyed only at the end of the frame, so it keeps the
        /// dialog layer the Active_Layer until the next one.
        ///
        /// The elements are the active, enabled graphics of the Active_Layer in depth-first
        /// hierarchy order, which is uGUI's draw order on one canvas: a TrackedLabel is one
        /// element in its letters' place, and a Text without visible characters is left out. A
        /// graphic that a <c>RectMask2D</c> culls (<c>canvasRenderer.cull</c>) stays in the list,
        /// marked <see cref="MeasuredElement.Culled"/> and measured from its RectTransform.
        /// </remarks>
        public static Measurement Measure(UiRoot ui)
        {
            Assert.IsTrue(ui != null, "LayoutModel.Measure: there is no UiRoot to measure");
            Assert.IsTrue(ResolutionRig.Window != Vector2Int.zero,
                "LayoutModel.Measure: no ResolutionRig window is applied, so there is no window to measure in");

            return new MeasurementBuilder(ActiveLayer(ui), ResolutionRig.Window).Build();
        }

        /// <summary>
        /// The Active_Layer: the dialog layer while a Dialog is open, else the active <c>Reveal</c>
        /// of the <c>BoxScreen</c>, else the current Screen.
        /// </summary>
        public static Transform ActiveLayer(UiRoot ui)
        {
            Assert.IsTrue(ui != null, "LayoutModel.ActiveLayer: there is no UiRoot");

            if (ui.DialogOpen)
            {
                var dialogs = ui.transform.Find("Dialogs");
                Assert.IsTrue(dialogs != null, "LayoutModel: a Dialog is open but " + PathOf(ui.transform) +
                                               "/Dialogs was not found");
                return dialogs;
            }

            var box = ui.Get<BoxScreen>();
            if (box != null)
            {
                var reveal = box.transform.Find("Reveal");
                if (reveal != null && reveal.gameObject.activeInHierarchy) return reveal;
            }

            var current = ui.Current;
            Assert.IsTrue(current != null, "LayoutModel: no Screen is showing, so there is no Active_Layer");
            return current.transform;
        }

        /// <summary>
        /// Whether <paramref name="graphic"/> is a plate: an Image whose sprite has a 9-slice border
        /// or is a factory rounded rect, named <c>&lt;size&gt;#2|…</c>.
        /// </summary>
        public static bool IsPlate(Graphic graphic)
        {
            var image = graphic as Image;
            if (image == null || image.sprite == null) return false;
            return image.sprite.border != Vector4.zero || PlateSpriteName.IsMatch(image.sprite.name);
        }

        /// <summary>
        /// Whether <paramref name="node"/> is a TrackedLabel: a row without a graphic, laid out by a
        /// <c>HorizontalLayoutGroup</c> and a <c>ContentSizeFitter</c>, whose children are
        /// one-character Texts or <c>Space</c> objects.
        /// </summary>
        public static bool IsTrackedLabel(Transform node)
        {
            if (node == null || node.GetComponent<Graphic>() != null) return false;
            if (node.GetComponent<HorizontalLayoutGroup>() == null) return false;
            if (node.GetComponent<ContentSizeFitter>() == null) return false;

            var letters = 0;
            for (var i = 0; i < node.childCount; i++)
            {
                var child = node.GetChild(i);
                if (child.name == SpaceName) continue;

                var letter = child.GetComponent<Text>();
                if (letter == null || letter.text == null || letter.text.Length != 1) return false;
                letters++;
            }

            return letters > 0;
        }

        /// <summary>
        /// The composite whose root is <paramref name="node"/>, recognised by the name the Game_UI
        /// gives it, or false.
        /// </summary>
        public static bool TryCompositeKind(Transform node, out CompositeKind kind)
        {
            kind = default;
            if (node == null) return false;

            switch (node.name)
            {
                case "Plaque": kind = CompositeKind.Plaque; return true;
                case "Plank": kind = CompositeKind.Plank; return true;
                case "Button": kind = CompositeKind.Button; return true;
                case "Bar": kind = CompositeKind.Bar; return true;
                case "Logo": kind = CompositeKind.Logo; return true;
                case "Monogram": kind = CompositeKind.Monogram; return true;
            }

            if (node.name.StartsWith("Card ", StringComparison.Ordinal))
            {
                kind = CompositeKind.Card;
                return true;
            }

            if (IsCrate(node))
            {
                kind = CompositeKind.Crate;
                return true;
            }

            return false;
        }

        /// <summary>
        /// The clip rect that <c>RectMask2D</c> applies to <paramref name="graphic"/> in
        /// Canvas_Units, or null when no mask clips it.
        /// </summary>
        /// <remarks>
        /// As <c>MaskUtilities.GetRectMaskForClippable</c> and <c>Clipping.FindCullAndClipWorldRect</c>
        /// do: the intersection of the rects of the active masks on the graphic's strict ancestors,
        /// each from its corners 0 and 2 minus its padding. A mask does not clip its own graphic.
        /// </remarks>
        public static Rect? ClipRectOf(Graphic graphic)
        {
            var maskable = graphic as MaskableGraphic;
            if (maskable == null || !maskable.maskable) return null;
            return AncestorClipRect(graphic.transform);
        }

        /// <summary>
        /// The intersection of the active <c>RectMask2D</c> rects on the strict ancestors of
        /// <paramref name="transform"/>, in Canvas_Units, or null when there is none.
        /// </summary>
        public static Rect? AncestorClipRect(Transform transform)
        {
            Rect? clip = null;
            for (var node = transform.parent; node != null; node = node.parent)
            {
                var mask = node.GetComponent<RectMask2D>();
                if (mask == null || !mask.isActiveAndEnabled) continue;

                var rect = MaskRect(mask);
                clip = clip.HasValue ? Intersect(clip.Value, rect) : rect;
            }

            return clip;
        }

        /// <summary>The rect that <paramref name="mask"/> clips to, minus its padding, in Canvas_Units.</summary>
        public static Rect MaskRect(RectMask2D mask)
        {
            var toCanvas = LocalToCanvas(mask.transform);
            var rect = mask.rectTransform.rect;
            Vector2 first = toCanvas.MultiplyPoint3x4(new Vector3(rect.xMin, rect.yMin, 0f));
            Vector2 third = toCanvas.MultiplyPoint3x4(new Vector3(rect.xMax, rect.yMax, 0f));
            var padding = mask.padding;

            return Span(Mathf.Min(first.x, third.x) + padding.x, Mathf.Min(first.y, third.y) + padding.y,
                Mathf.Max(first.x, third.x) - padding.z, Mathf.Max(first.y, third.y) - padding.w);
        }

        /// <summary>
        /// The overlap of two rects; zero-sized, at the overlap's corner, when they do not overlap.
        /// </summary>
        public static Rect Intersect(Rect a, Rect b) =>
            Span(Mathf.Max(a.xMin, b.xMin), Mathf.Max(a.yMin, b.yMin), Mathf.Min(a.xMax, b.xMax),
                Mathf.Min(a.yMax, b.yMax));

        /// <summary>The smallest rect that holds both rects.</summary>
        public static Rect Union(Rect a, Rect b) =>
            Rect.MinMaxRect(Mathf.Min(a.xMin, b.xMin), Mathf.Min(a.yMin, b.yMin), Mathf.Max(a.xMax, b.xMax),
                Mathf.Max(a.yMax, b.yMax));

        /// <summary>The axis-aligned bounds of <paramref name="points"/>.</summary>
        public static Rect BoundsOf(IReadOnlyList<Vector2> points)
        {
            if (points == null || points.Count == 0) return Rect.zero;

            var min = points[0];
            var max = min;
            for (var i = 1; i < points.Count; i++)
            {
                min = Vector2.Min(min, points[i]);
                max = Vector2.Max(max, points[i]);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        /// <summary>
        /// The mesh that <paramref name="graphic"/> gives uGUI before mesh effects, in Canvas_Units.
        /// </summary>
        /// <remarks>
        /// Calls the protected <c>Graphic.OnPopulateMesh(VertexHelper)</c> through reflection,
        /// as <c>Graphic.DoMeshGeneration</c> does, including its rule that a rect with a
        /// negative size draws nothing. Mesh modifiers such as <c>Shadow</c> and <c>Outline</c>
        /// run after that call, in <c>ModifyMesh</c>, so their copies are not in the result: that
        /// keeps drop shadows out of Text_Bounds. Each vertex goes through the graphic's transform
        /// into the root canvas, so rotation and scale are applied.
        /// </remarks>
        public static GraphicMesh Mesh(Graphic graphic)
        {
            Assert.IsTrue(graphic != null, "LayoutModel.Mesh: there is no graphic to read");

            var toCanvas = LocalToCanvas(graphic.transform);
            var helper = new VertexHelper();

            try
            {
                var rect = graphic.rectTransform.rect;
                if (rect.width >= 0f && rect.height >= 0f) Populate(graphic, helper);
                else helper.Clear();

                var vertex = new UIVertex();
                var vertices = new MeshVertex[helper.currentVertCount];
                for (var i = 0; i < vertices.Length; i++)
                {
                    helper.PopulateUIVertex(ref vertex, i);
                    vertices[i] = ToCanvas(toCanvas, vertex);
                }

                var stream = new List<UIVertex>(helper.currentIndexCount);
                helper.GetUIVertexStream(stream);
                var corners = new MeshVertex[stream.Count];
                for (var i = 0; i < corners.Length; i++) corners[i] = ToCanvas(toCanvas, stream[i]);

                return new GraphicMesh(graphic, vertices, corners);
            }
            finally
            {
                helper.Dispose();
            }
        }

        /// <summary>
        /// The alpha that the GPU's bilinear filter reads from <paramref name="texture"/> at
        /// <paramref name="uv"/>.
        /// </summary>
        /// <remarks>
        /// <c>Texture2D.GetPixelBilinear</c> puts UV (0, 0) on the centre of the bottom-left
        /// texel, so it centres texel i at i / width. The GPU centres it at (i + 0.5) / width, so
        /// the sample is taken half a texel lower on each axis. The probes of task 1.3 showed the
        /// difference on the test-local sliced image at 1280×720, where one texel is one pixel:
        /// sampled without the shift, its first opaque texel column and row fell one pixel left of
        /// and below where the GPU drew them.
        /// </remarks>
        public static float SampleAlpha(Texture2D texture, Vector2 uv) =>
            texture.GetPixelBilinear(uv.x - 0.5f / texture.width, uv.y - 0.5f / texture.height).a;

        /// <summary>
        /// Writes <paramref name="measurement"/> to <c>&lt;label&gt;_&lt;W&gt;x&lt;H&gt;.txt</c> in the
        /// directory that <see cref="DumpVariable"/> names and returns the file's absolute path, or
        /// does nothing and returns null when the variable is unset.
        /// </summary>
        /// <remarks>
        /// One line per element, in draw order: draw index, path from the Active_Layer, kind and
        /// classification, rect, Visible_Shape bounds, drawn bounds (<see cref="VisibleShape.DrawnBounds"/>)
        /// and Container path, <c>Viewport</c> when the Container is the Viewport. The composites
        /// and the Interactive_Controls follow, one line each. Rects are in Canvas_Units. A relative
        /// directory is taken from the working directory, the project root in a test run; a
        /// directory under <c>Assets/</c> is refused, because Unity would import the files.
        /// </remarks>
        public static string Dump(Measurement measurement, string label)
        {
            var directory = Environment.GetEnvironmentVariable(DumpVariable);
            if (string.IsNullOrEmpty(directory)) return null;

            Assert.IsTrue(measurement != null, "LayoutModel.Dump: there is no measurement to write");
            Assert.IsFalse(string.IsNullOrEmpty(label), "LayoutModel.Dump: the dump needs a label");

            directory = System.IO.Path.GetFullPath(directory);
            var assets = System.IO.Path.GetFullPath(Application.dataPath);
            Assert.IsFalse(IsUnder(directory, assets),
                "LayoutModel.Dump: " + DumpVariable + " names " + directory + ", which is under " + assets +
                "; give it a directory under Logs/");
            System.IO.Directory.CreateDirectory(directory);

            var file = System.IO.Path.Combine(directory,
                label + "_" + measurement.Window.x + "x" + measurement.Window.y + ".txt");
            System.IO.File.WriteAllText(file, Describe(measurement, label), new UTF8Encoding(false));
            return file;
        }

        /// <summary>The environment variable that names the directory <see cref="Dump"/> writes to.</summary>
        public const string DumpVariable = "NEKO_LAYOUT_DUMP";

        /// <summary>The text that <see cref="Dump"/> writes for <paramref name="measurement"/>.</summary>
        public static string Describe(Measurement measurement, string label)
        {
            var text = new StringBuilder();
            text.Append("Layout_Check model dump: ").Append(label).Append('\n');
            text.Append("layer ").Append(measurement.LayerPath).Append('\n');
            text.Append("window ").Append(measurement.Window.x).Append('x').Append(measurement.Window.y)
                .Append(", scale factor ").Append(Number(measurement.ScaleFactor))
                .Append(", viewport ").Append(Describe(measurement.Viewport)).Append('\n');
            // The projection itself, unchecked: the dump reports a room that does not match the grid.
            var room = measurement.Room;
            text.Append("room grid area ").Append(Describe(room.Area)).Append(", ").Append(room.Tiles)
                .Append(" floor tiles, grid ")
                .Append(room.Grid.HasValue ? room.Grid.Value.x + "x" + room.Grid.Value.y : "none").Append('\n');

            text.Append('\n').Append("elements (").Append(measurement.Elements.Count)
                .Append("): index | path | kind and classification | rect | shape | drawn | container\n");
            foreach (var element in measurement.Elements)
            {
                text.Append(element.Order).Append(" | ").Append(element.Path).Append(" | ")
                    .Append(Classification(element)).Append(" | rect ").Append(Describe(element.OwnRect))
                    .Append(" | shape ").Append(element.Shape.IsEmpty ? "empty" : Describe(element.Shape.Bounds))
                    .Append(" | drawn ").Append(Describe(element.Shape.DrawnBounds))
                    .Append(" | container ").Append(element.Container != null ? element.Container.Path : "Viewport")
                    .Append('\n');
            }

            text.Append('\n').Append("composites (").Append(measurement.Composites.Count)
                .Append("): kind | path | background pieces | shape | drawn\n");
            foreach (var composite in measurement.Composites)
            {
                var pieces = new List<string>();
                foreach (var piece in composite.BackgroundPieces) pieces.Add(piece.Order.ToString(Invariant));

                text.Append(composite.Kind).Append(" | ").Append(composite.Path).Append(" | pieces ")
                    .Append(pieces.Count > 0 ? string.Join(", ", pieces) : "none")
                    .Append(" | shape ").Append(composite.Shape.IsEmpty ? "empty" : Describe(composite.Shape.Bounds))
                    .Append(" | drawn ").Append(Describe(composite.Shape.DrawnBounds)).Append('\n');
            }

            text.Append('\n').Append("controls (").Append(measurement.Controls.Count)
                .Append("): kind | path | rect | clip | interactable\n");
            foreach (var control in measurement.Controls)
            {
                text.Append(control.Kind).Append(" | ").Append(control.Path).Append(" | rect ")
                    .Append(Describe(control.OwnRect)).Append(" | clip ")
                    .Append(control.ClipRect.HasValue ? Describe(control.ClipRect.Value) : "none")
                    .Append(control.Interactable ? " | interactable" : " | not interactable").Append('\n');
            }

            return text.ToString();
        }

        /// <summary>A rect of Canvas_Units, as the dump writes it.</summary>
        public static string Describe(Rect rect) =>
            "x " + Number(rect.xMin) + ".." + Number(rect.xMax) + ", y " + Number(rect.yMin) + ".." +
            Number(rect.yMax);

        private static string Describe(Rect? rect) => rect.HasValue ? Describe(rect.Value) : "none";

        private static string Classification(MeasuredElement element)
        {
            var parts = new List<string> { element.Kind.ToString() };
            if (element.Content != null) parts.Add(Quote(element.Content));
            if (element.Culled) parts.Add("culled");
            if (element.IsPlate) parts.Add("plate");
            if (element.IsBackgroundPiece) parts.Add("piece");
            if (element.IsDecor) parts.Add("decor");
            if (element.Composite != null) parts.Add("in " + element.Composite.Kind + " " + element.Composite.Path);
            return string.Join(" ", parts);
        }

        private static string Quote(string content)
        {
            var line = content.Replace("\r", "").Replace("\n", "\\n");
            return "\"" + (line.Length > 40 ? line.Substring(0, 40) + "…" : line) + "\"";
        }

        private static string Number(float value) => value.ToString("0.##", Invariant);

        private static bool IsUnder(string path, string directory)
        {
            var root = directory.TrimEnd(System.IO.Path.DirectorySeparatorChar) + System.IO.Path.DirectorySeparatorChar;
            var full = path.TrimEnd(System.IO.Path.DirectorySeparatorChar) + System.IO.Path.DirectorySeparatorChar;
            return full.StartsWith(root, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// The Room_Grid_Area: the rect, in Canvas_Units, that the room's floor tiles cover on the
        /// screen of <paramref name="canvas"/>; null when there is no room or it draws no tile.
        /// </summary>
        /// <remarks>
        /// The tiles are the active, enabled <c>SpriteRenderer</c>s named "Tile" under
        /// <c>GameRoot.Instance.Room</c>; <c>RoomView</c> builds one for each cell of the grid it is
        /// given. The corners of each tile's bounds go through <c>canvas.worldCamera.WorldToScreenPoint</c>
        /// and are divided by <c>canvas.scaleFactor</c>, which puts them in Canvas_Units with the
        /// origin at the bottom left. The result is the union of the tiles' projected bounds, not cut
        /// to the Viewport. It fails when the room draws a different number of tiles from the cells
        /// of <c>Session.Profile.Grid</c> (Width × Height), because the area would then belong to a
        /// room the session does not hold. <see cref="Measure"/> takes the same rect in the measured
        /// window and keeps it as <see cref="Measurement.RoomGridArea"/>.
        /// </remarks>
        public static Rect? RoomGridArea(Canvas canvas)
        {
            var room = ProjectRoom(canvas);
            room.AssertMatchesGrid();
            return room.Area;
        }

        /// <summary>The room's floor tiles projected onto <paramref name="canvas"/>, with the session's grid size.</summary>
        internal static RoomProjection ProjectRoom(Canvas canvas)
        {
            Assert.IsTrue(canvas != null, "LayoutModel.RoomGridArea: there is no canvas to project the room onto");

            var root = GameRoot.Instance;
            if (root == null) return new RoomProjection(null, 0, null);

            Vector2Int? grid = null;
            var profile = root.Session != null ? root.Session.Profile : null;
            if (profile != null && profile.Grid != null) grid = new Vector2Int(profile.Grid.Width, profile.Grid.Height);
            if (root.Room == null) return new RoomProjection(null, 0, grid);

            var camera = canvas.worldCamera;
            Rect? area = null;
            var tiles = 0;
            foreach (var tile in root.Room.GetComponentsInChildren<SpriteRenderer>())
            {
                if (!tile.enabled || tile.name != TileName) continue;

                Assert.IsTrue(camera != null, "LayoutModel.RoomGridArea: the canvas " + PathOf(canvas.transform) +
                                              " has no worldCamera to project the room through");
                var bounds = ProjectBounds(camera, canvas.scaleFactor, tile.bounds);
                area = area.HasValue ? Union(area.Value, bounds) : bounds;
                tiles++;
            }

            return new RoomProjection(area, tiles, grid);
        }

        /// <summary>The screen rect of a world-space box, in Canvas_Units, as <paramref name="camera"/> draws it.</summary>
        private static Rect ProjectBounds(Camera camera, float scaleFactor, Bounds box)
        {
            // The canvas covers the camera's pixel rect, which starts at (0, 0) for the rig's camera.
            var origin = camera.pixelRect.min;
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            for (var corner = 0; corner < 8; corner++)
            {
                var world = new Vector3((corner & 1) == 0 ? box.min.x : box.max.x,
                    (corner & 2) == 0 ? box.min.y : box.max.y, (corner & 4) == 0 ? box.min.z : box.max.z);
                var point = ((Vector2)camera.WorldToScreenPoint(world) - origin) / scaleFactor;
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        /// <summary>
        /// Whether <paramref name="graphic"/> is an Image that draws its whole sprite: Simple, or
        /// Sliced with its centre, rather than Tiled, Filled or a hollow frame.
        /// </summary>
        private static bool DrawsWhole(Graphic graphic)
        {
            var image = graphic as Image;
            return image != null &&
                   (image.type == Image.Type.Simple || (image.type == Image.Type.Sliced && image.fillCenter));
        }

        /// <summary>The matrix from <paramref name="transform"/>'s local space to Canvas_Units.</summary>
        public static Matrix4x4 LocalToCanvas(Transform transform)
        {
            var canvas = RootCanvas(transform);
            return Matrix4x4.Translate(-(Vector3)canvas.rect.min) * canvas.worldToLocalMatrix *
                   transform.localToWorldMatrix;
        }

        /// <summary>A point of <paramref name="transform"/>'s local space, in Canvas_Units.</summary>
        public static Vector2 ToCanvas(Transform transform, Vector2 local) =>
            LocalToCanvas(transform).MultiplyPoint3x4(local);

        /// <summary>
        /// The corners of <paramref name="rect"/> in Canvas_Units: bottom-left, top-left,
        /// top-right, bottom-right.
        /// </summary>
        public static Vector2[] RectCorners(RectTransform rect)
        {
            var toCanvas = LocalToCanvas(rect);
            var r = rect.rect;

            return new Vector2[]
            {
                toCanvas.MultiplyPoint3x4(new Vector3(r.xMin, r.yMin, 0f)),
                toCanvas.MultiplyPoint3x4(new Vector3(r.xMin, r.yMax, 0f)),
                toCanvas.MultiplyPoint3x4(new Vector3(r.xMax, r.yMax, 0f)),
                toCanvas.MultiplyPoint3x4(new Vector3(r.xMax, r.yMin, 0f)),
            };
        }

        /// <summary>
        /// The hierarchy path of <paramref name="transform"/>, with a sibling index where
        /// siblings share a name, for example <c>Ui/Screens/HubScreen/Plaque/Panel[1]</c>.
        /// </summary>
        public static string PathOf(Transform transform)
        {
            if (transform == null) return "(none)";

            var segments = new List<string>();
            for (var node = transform; node != null; node = node.parent) segments.Add(Segment(node));
            segments.Reverse();
            return string.Join("/", segments);
        }

        /// <summary>
        /// The hierarchy path of <paramref name="transform"/> from <paramref name="root"/>, for
        /// example <c>HubScreen/Plaque/Panel[1]</c>; the full path when it is not under the root.
        /// </summary>
        public static string PathFrom(Transform root, Transform transform)
        {
            if (transform == null) return "(none)";

            var segments = new List<string>();
            for (var node = transform; node != null; node = node.parent)
            {
                segments.Add(Segment(node));
                if (node != root) continue;

                segments.Reverse();
                return string.Join("/", segments);
            }

            return PathOf(transform);
        }

        /// <summary>
        /// Finds a method the model calls through reflection, or fails with the member's name.
        /// </summary>
        internal static MethodInfo RequireMethod(Type type, string name, BindingFlags flags,
            params Type[] parameters)
        {
            var method = type.GetMethod(name, flags, null, parameters, null);
            if (method != null) return method;

            var names = new string[parameters.Length];
            for (var i = 0; i < parameters.Length; i++) names[i] = parameters[i].Name;

            Assert.Fail("LayoutModel: the member " + type.FullName + "." + name + "(" +
                        string.Join(", ", names) + ") was not found, so the Layout_Check cannot " +
                        "read what uGUI draws");
            return null;
        }

        private static void Populate(Graphic graphic, VertexHelper helper)
        {
            if (_populateMesh == null)
            {
                _populateMesh = RequireMethod(typeof(Graphic), "OnPopulateMesh",
                    BindingFlags.Instance | BindingFlags.NonPublic, typeof(VertexHelper));
            }

            try
            {
                // Virtual dispatch: an Image or a Text runs its own override.
                _populateMesh.Invoke(graphic, new object[] { helper });
            }
            catch (TargetInvocationException error) when (error.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(error.InnerException).Throw();
            }
        }

        private static MeshVertex ToCanvas(Matrix4x4 toCanvas, UIVertex vertex) =>
            new MeshVertex(toCanvas.MultiplyPoint3x4(vertex.position),
                new Vector2(vertex.uv0.x, vertex.uv0.y), vertex.color);

        private static RectTransform RootCanvas(Transform transform)
        {
            Assert.IsTrue(transform != null, "LayoutModel: there is no transform to place on the canvas");

            var canvas = transform.GetComponentInParent<Canvas>(true);
            Assert.IsTrue(canvas != null, "LayoutModel: " + PathOf(transform) + " is not on a canvas");
            return (RectTransform)canvas.rootCanvas.transform;
        }

        private static string Segment(Transform node)
        {
            var parent = node.parent;
            if (parent == null) return node.name;

            var index = 0;
            var count = 0;
            for (var i = 0; i < parent.childCount; i++)
            {
                var sibling = parent.GetChild(i);
                if (sibling.name != node.name) continue;
                if (sibling == node) index = count;
                count++;
            }

            return count > 1 ? node.name + "[" + index + "]" : node.name;
        }

        private static Rect Span(float xMin, float yMin, float xMax, float yMax) =>
            new Rect(xMin, yMin, Mathf.Max(0f, xMax - xMin), Mathf.Max(0f, yMax - yMin));

        private static bool IsCrate(Transform node)
        {
            if (node.name != PanelName || node.GetComponent<Image>() == null) return false;

            var monogram = false;
            var band = false;
            for (var i = 0; i < node.childCount; i++)
            {
                var child = node.GetChild(i);
                if (child.name == "Monogram") monogram = true;
                else if (child.name == PanelName && child.GetComponent<Image>() != null) band = true;
            }

            return monogram && band;
        }

        /// <summary>Whether <paramref name="rect"/> stretches over the whole of its parent, as <c>UiKit.Stretch</c> does.</summary>
        private static bool CoversParent(RectTransform rect) =>
            rect.anchorMin == Vector2.zero && rect.anchorMax == Vector2.one &&
            rect.offsetMin.x <= StretchTolerance && rect.offsetMin.y <= StretchTolerance &&
            rect.offsetMax.x >= -StretchTolerance && rect.offsetMax.y >= -StretchTolerance;

        /// <summary>A zero-sized rect at the centre of <paramref name="rect"/>: the bounds of a mesh that draws nothing.</summary>
        private static Rect Nothing(Rect rect) => new Rect(rect.center, Vector2.zero);

        /// <summary>Builds one <see cref="Measurement"/> of an Active_Layer.</summary>
        private sealed class MeasurementBuilder
        {
            private readonly Transform _layer;
            private readonly Vector2Int _window;
            private readonly List<MeasuredElement> _elements = new List<MeasuredElement>();
            private readonly Dictionary<Transform, MeasuredElement> _byTransform =
                new Dictionary<Transform, MeasuredElement>();
            private readonly List<CompositeElement> _composites = new List<CompositeElement>();
            private readonly Dictionary<Transform, CompositeElement> _compositeByRoot =
                new Dictionary<Transform, CompositeElement>();
            private readonly HashSet<MeasuredElement> _pieces = new HashSet<MeasuredElement>();
            private readonly List<InteractiveControl> _controls = new List<InteractiveControl>();

            /// <summary>One alpha cache per sprite texture, shared by every element of this measurement.</summary>
            private readonly Dictionary<Texture2D, AlphaCache> _alphaCaches = new Dictionary<Texture2D, AlphaCache>();

            public MeasurementBuilder(Transform layer, Vector2Int window)
            {
                Assert.IsTrue(layer != null, "LayoutModel.Measure: there is no Active_Layer");
                _layer = layer;
                _window = window;
            }

            public Measurement Build()
            {
                var canvasRect = RootCanvas(_layer);
                var canvas = canvasRect.GetComponent<Canvas>();
                var viewport = new Rect(0f, 0f, canvasRect.rect.width, canvasRect.rect.height);

                Walk(_layer);

                // Background pieces first: Decor and the Containers are read from them.
                foreach (var composite in _composites)
                {
                    var pieces = BackgroundPiecesOf(composite);
                    composite.BackgroundPieces = pieces;
                    composite.Shape = VisibleShape.Union(pieces.ConvertAll(piece => piece.Shape),
                        ToCanvas(composite.Root, Vector2.zero));
                    foreach (var piece in pieces) _pieces.Add(piece);
                }

                foreach (var element in _elements)
                {
                    element.Composite = InnermostComposite(element.Transform);
                    element.IsBackgroundPiece = _pieces.Contains(element);
                    element.IsDecor = IsDecor(element);
                }

                foreach (var element in _elements) element.Container = ContainerOf(element);
                foreach (var element in _elements) element.Chain = ChainOf(element);

                return new Measurement(_layer, PathOf(_layer), _window, viewport, canvas.scaleFactor, _elements,
                    _composites, _controls, _byTransform, ProjectRoom(canvas));
            }

            private void Walk(Transform node)
            {
                if (!node.gameObject.activeInHierarchy) return;

                CompositeKind kind;
                if (TryCompositeKind(node, out kind))
                {
                    var composite = new CompositeElement(kind, node, PathFrom(_layer, node));
                    _composites.Add(composite);
                    _compositeByRoot[node] = composite;
                }

                AddControls(node);

                if (IsTrackedLabel(node))
                {
                    AddTrackedLabel(node);
                    return;
                }

                var graphic = node.GetComponent<Graphic>();
                if (graphic != null && graphic.isActiveAndEnabled) AddGraphic(graphic);

                for (var i = 0; i < node.childCount; i++) Walk(node.GetChild(i));
            }

            private void AddGraphic(Graphic graphic)
            {
                var text = graphic as Text;
                if (text != null && string.IsNullOrWhiteSpace(text.text)) return;

                var rectTransform = graphic.rectTransform;
                var ownRect = BoundsOf(RectCorners(rectTransform));
                var culled = graphic.canvasRenderer.cull;
                var clipRect = ClipRectOf(graphic);
                var isPlate = IsPlate(graphic);

                // A culled graphic is not drawn, so it is measured from its RectTransform and shows nothing.
                var meshes = Array.Empty<GraphicMesh>();
                var bounds = ownRect;
                var glyphs = 0;
                var shape = VisibleShape.Nothing(ownRect.position);
                if (!culled)
                {
                    var mesh = Mesh(graphic);
                    meshes = new[] { mesh };
                    bounds = mesh.Vertices.Count > 0 ? mesh.Bounds : Nothing(ownRect);
                    if (text != null) glyphs = mesh.QuadCount;

                    // A text shows its Text_Bounds, so the font texture is not sampled.
                    if (mesh.Vertices.Count == 0) shape = VisibleShape.Nothing(bounds.position);
                    else if (text != null) shape = VisibleShape.OfRect(bounds, clipRect);
                    else shape = VisibleShape.OfMesh(mesh, clipRect, AlphaOf(graphic), isPlate && DrawsWhole(graphic));
                }

                var element = new MeasuredElement(_elements.Count, PathFrom(_layer, graphic.transform),
                    text != null ? ElementKind.Text : ElementKind.Image, rectTransform, new[] { graphic },
                    meshes, culled, bounds, ownRect, clipRect, text != null ? text.text : null,
                    text != null ? bounds : Rect.zero, glyphs, isPlate, shape);
                _elements.Add(element);
                _byTransform[graphic.transform] = element;
            }

            /// <summary>
            /// The alpha cache of the sprite texture that <paramref name="graphic"/> draws, one per
            /// texture in this measurement; null when it draws no sprite the CPU can read.
            /// </summary>
            /// <remarks>
            /// An Image without a sprite, an Image whose texture is not readable, and any graphic
            /// other than an Image keep their whole mesh, cut to the clip rect, as their
            /// Visible_Shape: there is no alpha to read. <c>overrideSprite</c> returns the sprite
            /// that <c>OnPopulateMesh</c> draws, the override or else <c>sprite</c>.
            /// </remarks>
            private AlphaCache AlphaOf(Graphic graphic)
            {
                var image = graphic as Image;
                var sprite = image != null ? image.overrideSprite : null;
                var texture = sprite != null ? sprite.texture : null;
                if (texture == null || !texture.isReadable) return null;

                AlphaCache cache;
                if (!_alphaCaches.TryGetValue(texture, out cache))
                {
                    cache = new AlphaCache(texture);
                    _alphaCaches.Add(texture, cache);
                }

                return cache;
            }

            private void AddTrackedLabel(Transform node)
            {
                var row = node as RectTransform;
                Assert.IsTrue(row != null, "LayoutModel: the TrackedLabel " + PathOf(node) + " has no RectTransform");

                var letters = new List<Graphic>();
                var meshes = new List<GraphicMesh>();
                var content = new StringBuilder();
                var culled = true;
                Rect? glyphBounds = null;
                var glyphs = 0;

                for (var i = 0; i < row.childCount; i++)
                {
                    var child = row.GetChild(i);
                    if (!child.gameObject.activeInHierarchy) continue;

                    if (child.name == SpaceName)
                    {
                        content.Append(' ');
                        continue;
                    }

                    var letter = child.GetComponent<Text>();
                    if (letter == null || !letter.isActiveAndEnabled) continue;

                    letters.Add(letter);
                    content.Append(letter.text);
                    if (letter.canvasRenderer.cull) continue;

                    culled = false;
                    var mesh = Mesh(letter);
                    meshes.Add(mesh);
                    if (mesh.Vertices.Count == 0) continue;

                    glyphs += mesh.QuadCount;
                    glyphBounds = glyphBounds.HasValue ? Union(glyphBounds.Value, mesh.Bounds) : mesh.Bounds;
                }

                if (letters.Count == 0) return;

                // The row is culled only when every letter is; its own rect is the row's.
                var ownRect = BoundsOf(RectCorners(row));
                var bounds = culled ? ownRect : glyphBounds ?? Nothing(ownRect);
                var clipRect = ClipRectOf(letters[0]);
                var shape = culled ? VisibleShape.Nothing(ownRect.position) : VisibleShape.OfRect(bounds, clipRect);
                var element = new MeasuredElement(_elements.Count, PathFrom(_layer, row), ElementKind.TrackedText,
                    row, letters, meshes, culled, bounds, ownRect, clipRect, content.ToString(),
                    bounds, glyphs, false, shape);
                _elements.Add(element);
                _byTransform[row] = element;
                foreach (var letter in letters) _byTransform[letter.transform] = element;
            }

            private void AddControls(Transform node)
            {
                var rect = node as RectTransform;
                if (rect == null) return;

                foreach (var selectable in node.GetComponents<Selectable>())
                {
                    if (!selectable.IsActive()) continue;
                    _controls.Add(new InteractiveControl(ControlKind.Selectable, selectable, PathFrom(_layer, node),
                        BoundsOf(RectCorners(rect)), AncestorClipRect(node), selectable.IsInteractable()));
                }

                var scroll = node.GetComponent<ScrollRect>();
                if (scroll != null && scroll.IsActive())
                {
                    _controls.Add(new InteractiveControl(ControlKind.ScrollArea, scroll, PathFrom(_layer, node),
                        BoundsOf(RectCorners(rect)), AncestorClipRect(node), true));
                }
            }

            /// <summary>
            /// The plates a composite draws behind its content, as the glossary lists them: plaque
            /// layers; plank (and button) root, face and shine; bar track; card frame, face and
            /// wash; logo board layers; crate lid and band. A monogram has none.
            /// </summary>
            private List<MeasuredElement> BackgroundPiecesOf(CompositeElement composite)
            {
                var pieces = new List<MeasuredElement>();
                var root = composite.Root;

                switch (composite.Kind)
                {
                    case CompositeKind.Plaque:
                    case CompositeKind.Plank:
                    case CompositeKind.Button:
                        // Root, then the plates in front of it before any content: edge and inner
                        // panel, or face and shine. A Baseline Button is its own face.
                        AddPlate(pieces, root);
                        for (var i = 0; i < root.childCount && i < 2; i++)
                        {
                            if (!AddPlate(pieces, root.GetChild(i))) break;
                        }

                        break;
                    case CompositeKind.Bar:
                        AddPlate(pieces, root);
                        break;
                    case CompositeKind.Card:
                        // Frame (the root), face (its first child) and wash (the face's first child).
                        AddPlate(pieces, root);
                        var face = root.childCount > 0 ? root.GetChild(0) : null;
                        if (AddPlate(pieces, face) && face.childCount > 0) AddPlate(pieces, face.GetChild(0));
                        break;
                    case CompositeKind.Logo:
                        // The board layers: the shadow and board drawn after the ears, and the
                        // plates inset into the board. The hanger before the ears is Decor.
                        var afterEar = root.Find(EarName) == null;
                        for (var i = 0; i < root.childCount; i++)
                        {
                            var child = root.GetChild(i);
                            if (child.name == EarName) afterEar = true;
                            if (!afterEar || !AddPlate(pieces, child)) continue;

                            for (var j = 0; j < child.childCount; j++) AddPlate(pieces, child.GetChild(j));
                        }

                        break;
                    case CompositeKind.Crate:
                        // The lid (the root) and the band across it.
                        AddPlate(pieces, root);
                        for (var i = 0; i < root.childCount; i++)
                        {
                            var child = root.GetChild(i);
                            if (child.name == PanelName && AddPlate(pieces, child)) break;
                        }

                        break;
                }

                return pieces;
            }

            /// <summary>Adds the plate that <paramref name="node"/> draws, if it draws one.</summary>
            private bool AddPlate(List<MeasuredElement> pieces, Transform node)
            {
                MeasuredElement element;
                if (node == null || !_byTransform.TryGetValue(node, out element) || !element.IsPlate ||
                    element.Transform != node)
                    return false;

                if (!pieces.Contains(element)) pieces.Add(element);
                return true;
            }

            private CompositeElement InnermostComposite(Transform transform)
            {
                for (var node = transform; node != null; node = node.parent)
                {
                    CompositeElement composite;
                    if (_compositeByRoot.TryGetValue(node, out composite)) return composite;
                    if (node == _layer) break;
                }

                return null;
            }

            private bool IsDecor(MeasuredElement element)
            {
                for (var node = element.Transform; node != null; node = node.parent)
                {
                    if (node.name == SceneryName) return true;
                    if (node == _layer) break;
                }

                // A backdrop or shade: a full-stretch plate directly on the Screen, the dialog layer
                // or Reveal, which are the three kinds of Active_Layer root.
                if (element.IsPlate && element.Transform.parent == _layer && CoversParent(element.RectTransform))
                    return true;

                for (var node = element.Transform; node != null && node != _layer; node = node.parent)
                {
                    var parent = node.parent;
                    CompositeElement composite;
                    if (parent != null && _compositeByRoot.TryGetValue(parent, out composite) &&
                        composite.Kind == CompositeKind.Logo && IsBeforeFirstEar(parent, node))
                        return true;
                }

                return false;
            }

            private static bool IsBeforeFirstEar(Transform logo, Transform child)
            {
                for (var i = 0; i < logo.childCount; i++)
                {
                    var sibling = logo.GetChild(i);
                    if (sibling.name == EarName) return false;
                    if (sibling == child) return true;
                }

                return false;
            }

            /// <summary>
            /// The nearest ancestor plate; for content directly on a plank or button, its face; null
            /// (the Viewport) when there is no plate, or when the nearest one is Decor.
            /// </summary>
            private MeasuredElement ContainerOf(MeasuredElement element)
            {
                var parent = element.Transform.parent;

                // The label of a plank is a sibling of the face it is drawn on, not its child.
                CompositeElement composite;
                if (parent != null && !element.IsBackgroundPiece &&
                    _compositeByRoot.TryGetValue(parent, out composite) &&
                    (composite.Kind == CompositeKind.Plank || composite.Kind == CompositeKind.Button))
                {
                    var face = FaceOf(composite);
                    if (face != null) return face;
                }

                for (var node = parent; node != null; node = node.parent)
                {
                    MeasuredElement candidate;
                    if (_byTransform.TryGetValue(node, out candidate) && candidate.IsPlate &&
                        candidate.Transform == node)
                        return candidate.IsDecor ? null : candidate;
                    if (node == _layer) break;
                }

                return null;
            }

            /// <summary>The face of a plank or button: its first background piece in front of the root.</summary>
            private static MeasuredElement FaceOf(CompositeElement composite)
            {
                foreach (var piece in composite.BackgroundPieces)
                {
                    if (piece.Transform != composite.Root) return piece;
                }

                return null;
            }

            /// <summary>
            /// The Containers up to the Viewport, each followed by the background pieces of the
            /// composite it belongs to.
            /// </summary>
            private static List<MeasuredElement> ChainOf(MeasuredElement element)
            {
                var chain = new List<MeasuredElement>();
                var visited = new HashSet<MeasuredElement>();
                for (var container = element.Container; container != null && visited.Add(container);
                     container = container.Container)
                {
                    if (!chain.Contains(container)) chain.Add(container);
                    if (container.Composite == null) continue;

                    foreach (var piece in container.Composite.BackgroundPieces)
                    {
                        if (!chain.Contains(piece)) chain.Add(piece);
                    }
                }

                return chain;
            }
        }
    }

    /// <summary>One vertex of a <see cref="GraphicMesh"/>.</summary>
    public readonly struct MeshVertex
    {
        public MeshVertex(Vector2 position, Vector2 uv, Color32 colour)
        {
            Position = position;
            Uv = uv;
            Colour = colour;
        }

        /// <summary>The position in Canvas_Units.</summary>
        public Vector2 Position { get; }

        /// <summary>The texture coordinate uGUI samples at this vertex.</summary>
        public Vector2 Uv { get; }

        /// <summary>The vertex colour, which the graphic's colour sets.</summary>
        public Color32 Colour { get; }
    }

    /// <summary>A graphic's mesh in Canvas_Units: its vertices and its triangles.</summary>
    public sealed class GraphicMesh
    {
        private readonly MeshVertex[] _vertices;
        private readonly MeshVertex[] _corners;

        internal GraphicMesh(Graphic graphic, MeshVertex[] vertices, MeshVertex[] corners)
        {
            Graphic = graphic;
            _vertices = vertices;
            _corners = corners;
            Bounds = BoundsOf(vertices, 0, vertices.Length);
        }

        public Graphic Graphic { get; }

        /// <summary>The vertices, in the order the graphic added them.</summary>
        public IReadOnlyList<MeshVertex> Vertices => _vertices;

        /// <summary>The triangles, three corners each, in index order.</summary>
        public IReadOnlyList<MeshVertex> TriangleCorners => _corners;

        /// <summary>The array behind <see cref="TriangleCorners"/>, for the shape tests' inner loop. Not to be changed.</summary>
        internal MeshVertex[] Corners => _corners;

        /// <summary>
        /// Whether the mesh is a list of quads of four vertices and two triangles each, as text
        /// glyphs and image cells are.
        /// </summary>
        public bool IsQuadList => _vertices.Length % 4 == 0 && _corners.Length == _vertices.Length / 4 * 6;

        /// <summary>The number of quads, for a mesh that <see cref="IsQuadList"/>.</summary>
        public int QuadCount
        {
            get
            {
                Assert.IsTrue(IsQuadList, "GraphicMesh: the mesh of " + LayoutModel.PathOf(Graphic.transform) +
                                          " is not a list of quads (" + _vertices.Length + " vertices, " +
                                          _corners.Length / 3 + " triangles)");
                return _vertices.Length / 4;
            }
        }

        /// <summary>The bounds of every vertex in Canvas_Units; zero-sized for an empty mesh.</summary>
        public Rect Bounds { get; }

        /// <summary>The bounds of one quad in Canvas_Units.</summary>
        public Rect QuadBounds(int quad) => BoundsOf(_vertices, quad * 4, 4);

        private static Rect BoundsOf(MeshVertex[] vertices, int start, int count)
        {
            if (count <= 0 || start + count > vertices.Length) return Rect.zero;

            var min = vertices[start].Position;
            var max = min;
            for (var i = start + 1; i < start + count; i++)
            {
                min = Vector2.Min(min, vertices[i].Position);
                max = Vector2.Max(max, vertices[i].Position);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
    }

    /// <summary>What an element of a <see cref="Measurement"/> is drawn as.</summary>
    public enum ElementKind
    {
        /// <summary>A <c>Text</c> with text.</summary>
        Text,

        /// <summary>A TrackedLabel: a row of one-character Texts, measured as one element.</summary>
        TrackedText,

        /// <summary>Any other graphic, in practice an <c>Image</c>.</summary>
        Image,
    }

    /// <summary>The composites that the Layout_Check recognises by their root's name.</summary>
    public enum CompositeKind
    {
        Plaque,
        Plank,
        Button,
        Bar,
        Card,
        Logo,
        Monogram,

        /// <summary>A Panel that holds a band Panel and a <c>Monogram</c>.</summary>
        Crate,
    }

    /// <summary>What an Interactive_Control is.</summary>
    public enum ControlKind
    {
        /// <summary>An active <c>Selectable</c>, such as a <c>Button</c>.</summary>
        Selectable,

        /// <summary>An active <c>ScrollRect</c>: a scroll area.</summary>
        ScrollArea,
    }

    /// <summary>
    /// The Visible_Shape of an element: where its pixels can show, in Canvas_Units.
    /// </summary>
    /// <remarks>
    /// A point is inside when the element draws it and every <c>RectMask2D</c> ancestor's clip rect
    /// holds it:
    /// <list type="bullet">
    /// <item>An Image whose sprite texture the CPU can read draws a point of its mesh when the
    /// texture's alpha, filtered bilinearly at the point's interpolated UV as the GPU filters it
    /// (<see cref="LayoutModel.SampleAlpha"/>), is at least <see cref="AlphaThreshold"/>. The
    /// Image's colour is not part of the test. A Baseline <c>UiKit.Panel</c> stretches a rounded
    /// rect that is smaller than its texture, so its shape is smaller than its rect (cause C1).</item>
    /// <item>A Text or a TrackedLabel draws its Text_Bounds. Font textures are not sampled.</item>
    /// <item>Any other graphic, such as an Image without a sprite or with a texture the CPU cannot
    /// read, draws its whole mesh, cut to the clip rect.</item>
    /// </list>
    /// A culled element draws nothing. A Composite_Element's shape is the union of its background
    /// pieces' shapes (<see cref="CompositeElement.Shape"/>).
    ///
    /// <see cref="Bounds"/> come from the mesh, so they hold the shape but can be larger than it,
    /// by a sprite's transparent margin; <see cref="DrawnBounds"/> leave that margin out. The rules
    /// test shapes with <see cref="Contains(Rect)"/>,
    /// a rectangle inside a shape, and <see cref="Intersection"/>, the overlap of two shapes.
    /// </remarks>
    public sealed class VisibleShape
    {
        /// <summary>The filtered texture alpha at or above which an Image draws a point.</summary>
        public const float AlphaThreshold = 0.5f;

        /// <summary>The spacing, in Canvas_Units, of the points at which a shape test samples a shape.</summary>
        public const float SampleStep = 0.5f;

        /// <summary>How far outside a triangle, in barycentric weight, a point on its edge may fall by rounding.</summary>
        private const float EdgeTolerance = 1e-5f;

        /// <summary>Twice the area, in square Canvas_Units, below which a triangle covers nothing.</summary>
        private const float DegenerateArea = 1e-8f;

        /// <summary>The mesh's triangles, three corners each; null for a rectangle or a union.</summary>
        private readonly MeshVertex[] _triangles;

        /// <summary>The sprite texture's alpha; null when the whole mesh counts.</summary>
        private readonly AlphaCache _alpha;

        /// <summary>The shapes of a union; null for one element's shape.</summary>
        private readonly VisibleShape[] _parts;

        /// <summary>The cached <see cref="DrawnBounds"/>, valid once <see cref="_drawnKnown"/> is set.</summary>
        private Rect? _drawn;

        private bool _drawnKnown;

        /// <summary>A rectangle in Canvas_Units, such as a Text's clipped Text_Bounds.</summary>
        internal VisibleShape(Rect bounds) : this(bounds, true, null, null, null)
        {
        }

        private VisibleShape(Rect bounds, bool convex, MeshVertex[] triangles, AlphaCache alpha, VisibleShape[] parts)
        {
            Bounds = bounds;
            IsConvex = convex;
            _triangles = triangles;
            _alpha = alpha;
            _parts = parts;
            IsEmpty = bounds.width <= 0f || bounds.height <= 0f || (triangles != null && triangles.Length < 3) ||
                      (parts != null && parts.Length == 0);
        }

        /// <summary>
        /// The bounds of the shape: the mesh bounds, or the Text_Bounds of a text, cut to the clip
        /// rect. They hold every point of the shape, and a sprite's transparent margin as well.
        /// </summary>
        public Rect Bounds { get; }

        /// <summary>
        /// Whether nothing of the element can show: it is culled, draws no mesh, or is clipped away.
        /// A sprite that is transparent everywhere is not empty, but holds no point.
        /// </summary>
        public bool IsEmpty { get; }

        /// <summary>
        /// Whether the shape is convex, so that it holds a rectangle when it holds the rectangle's
        /// four corners: a rectangle, or a plate drawn whole (Simple, or Sliced with its centre).
        /// </summary>
        /// <remarks>
        /// A plate's sprite is a filled factory panel: the Baseline's stretched 96-pixel rounded rect
        /// or a sliced panel. Its alpha-0.5 outline is a rounded rect, which stays convex when it is
        /// stretched, rotated or cut to a clip rect.
        /// </remarks>
        public bool IsConvex { get; }

        /// <summary>
        /// The bounds of the points the shape draws, in Canvas_Units: the alpha-tight bounds, which
        /// leave out a sprite's transparent margin; null when the shape draws no point.
        /// </summary>
        /// <remarks>
        /// <see cref="Bounds"/> come from the mesh, so a Baseline panel's still hold the clear
        /// margin of its texture (cause C1). These bounds are those of the points where
        /// <see cref="Contains(Vector2)"/> holds, for the rules that test where an element shows
        /// against a rect, such as a plate's edge against the edge it should sit inside:
        /// <list type="bullet">
        /// <item>A rectangle shape, such as a text's Text_Bounds, draws the whole of its bounds.</item>
        /// <item>A union draws the union of its parts' drawn bounds.</item>
        /// <item>A mesh shape is sampled at points at most <see cref="SampleStep"/> apart over its
        /// bounds, edges and corners included, so each side lies on a drawn point and falls short of
        /// the true drawn edge by less than one step, unless a feature thinner than a step sticks out
        /// between two sample points. A rule that needs the drawn edge to keep a clearance d from a
        /// line therefore tests d + <see cref="SampleStep"/>.</item>
        /// </list>
        /// Computed on first use and kept, so a shape is sampled once per measurement.
        /// </remarks>
        public Rect? DrawnBounds
        {
            get
            {
                if (!_drawnKnown)
                {
                    _drawn = FindDrawnBounds();
                    _drawnKnown = true;
                }

                return _drawn;
            }
        }

        private bool IsRectangle => _triangles == null && _parts == null;

        /// <summary>Whether <paramref name="point"/>, in Canvas_Units, is inside the shape. Edges count as inside.</summary>
        public bool Contains(Vector2 point)
        {
            if (IsEmpty || !Holds(Bounds, point)) return false;

            if (_parts != null)
            {
                for (var i = 0; i < _parts.Length; i++)
                {
                    if (_parts[i].Contains(point)) return true;
                }

                return false;
            }

            return _triangles == null || MeshDraws(point);
        }

        /// <summary>Whether the whole of <paramref name="rect"/>, in Canvas_Units, is inside the shape.</summary>
        /// <remarks>
        /// A rect that leaves <see cref="Bounds"/> is outside. A rectangle shape holds every rect
        /// inside its bounds, and a convex shape (<see cref="IsConvex"/>) holds a rect when it holds
        /// its four corners. Any other shape is tested at points at most <see cref="SampleStep"/>
        /// apart over the rect, its edges and corners included.
        /// </remarks>
        public bool Contains(Rect rect)
        {
            var min = Vector2.Min(rect.min, rect.max);
            var max = Vector2.Max(rect.min, rect.max);
            if (IsEmpty || !Holds(Bounds, min) || !Holds(Bounds, max)) return false;
            if (IsRectangle) return true;

            if (IsConvex)
            {
                return Contains(min) && Contains(new Vector2(min.x, max.y)) && Contains(max) &&
                       Contains(new Vector2(max.x, min.y));
            }

            var columns = Intervals(min.x, max.x);
            var rows = Intervals(min.y, max.y);
            for (var row = 0; row <= rows; row++)
            {
                var y = Tick(min.y, max.y, row, rows);
                for (var column = 0; column <= columns; column++)
                {
                    if (!Contains(new Vector2(Tick(min.x, max.x, column, columns), y))) return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The overlap of this shape and <paramref name="other"/>: the bounds of the points inside
        /// both, in Canvas_Units, or null when no point is.
        /// </summary>
        /// <remarks>
        /// Two rectangle shapes overlap exactly. Otherwise the overlap is sampled at the points of a
        /// grid laid on the canvas, at x and y multiples of <see cref="SampleStep"/>, inside both
        /// shapes' bounds. The result is the bounds of the grid points inside both shapes, so it can
        /// be up to one step short of the true overlap on each side, and an overlap that holds no
        /// grid point is not seen. Shapes that only touch give at most a zero-width or zero-height
        /// rect, which the rules' Overlap_Tolerance ignores.
        /// </remarks>
        public Rect? Intersection(VisibleShape other)
        {
            Assert.IsTrue(other != null, "VisibleShape.Intersection: there is no shape to intersect with");
            if (IsEmpty || other.IsEmpty) return null;

            var xMin = Mathf.Max(Bounds.xMin, other.Bounds.xMin);
            var yMin = Mathf.Max(Bounds.yMin, other.Bounds.yMin);
            var xMax = Mathf.Min(Bounds.xMax, other.Bounds.xMax);
            var yMax = Mathf.Min(Bounds.yMax, other.Bounds.yMax);
            if (xMin > xMax || yMin > yMax) return null;
            if (IsRectangle && other.IsRectangle) return Rect.MinMaxRect(xMin, yMin, xMax, yMax);

            // A rectangle answers from its bounds, so it is asked first.
            var first = other.IsRectangle ? other : this;
            var second = first == this ? other : this;

            var found = false;
            var low = Vector2.zero;
            var high = Vector2.zero;
            var columnMin = Mathf.CeilToInt(xMin / SampleStep);
            var columnMax = Mathf.FloorToInt(xMax / SampleStep);
            var rowMax = Mathf.FloorToInt(yMax / SampleStep);
            for (var row = Mathf.CeilToInt(yMin / SampleStep); row <= rowMax; row++)
            {
                for (var column = columnMin; column <= columnMax; column++)
                {
                    var point = new Vector2(column * SampleStep, row * SampleStep);
                    if (!first.Contains(point) || !second.Contains(point)) continue;

                    low = found ? Vector2.Min(low, point) : point;
                    high = found ? Vector2.Max(high, point) : point;
                    found = true;
                }
            }

            return found ? Rect.MinMaxRect(low.x, low.y, high.x, high.y) : (Rect?)null;
        }

        /// <summary>The shape of an element that draws nothing, placed at <paramref name="at"/>.</summary>
        internal static VisibleShape Nothing(Vector2 at) => new VisibleShape(new Rect(at, Vector2.zero));

        /// <summary>A rectangle cut to <paramref name="clip"/>: the shape of a Text_Bounds.</summary>
        internal static VisibleShape OfRect(Rect rect, Rect? clip) =>
            new VisibleShape(clip.HasValue ? LayoutModel.Intersect(rect, clip.Value) : rect);

        /// <summary>
        /// The triangles of <paramref name="mesh"/> cut to <paramref name="clip"/>; with
        /// <paramref name="alpha"/>, only where the texture's alpha reaches <see cref="AlphaThreshold"/>.
        /// </summary>
        internal static VisibleShape OfMesh(GraphicMesh mesh, Rect? clip, AlphaCache alpha, bool convex)
        {
            var bounds = clip.HasValue ? LayoutModel.Intersect(mesh.Bounds, clip.Value) : mesh.Bounds;
            return new VisibleShape(bounds, convex, mesh.Corners, alpha, null);
        }

        /// <summary>
        /// The union of <paramref name="parts"/>, or an empty shape at <paramref name="at"/> when
        /// none of them shows.
        /// </summary>
        internal static VisibleShape Union(IEnumerable<VisibleShape> parts, Vector2 at)
        {
            var shown = new List<VisibleShape>();
            var bounds = Rect.zero;
            foreach (var part in parts)
            {
                if (part == null || part.IsEmpty) continue;

                bounds = shown.Count == 0 ? part.Bounds : LayoutModel.Union(bounds, part.Bounds);
                shown.Add(part);
            }

            if (shown.Count == 0) return Nothing(at);
            if (shown.Count == 1) return shown[0];
            return new VisibleShape(bounds, false, null, null, shown.ToArray());
        }

        /// <summary>Samples <see cref="DrawnBounds"/>.</summary>
        /// <remarks>
        /// Every drawn point lies between the lowest and the highest sample row that holds one, so
        /// the columns are scanned between those two rows only, and each scan stops at the first
        /// row or column, from its side, that holds a drawn point.
        /// </remarks>
        private Rect? FindDrawnBounds()
        {
            if (IsEmpty) return null;
            if (IsRectangle) return Bounds;

            if (_parts != null)
            {
                Rect? union = null;
                foreach (var part in _parts)
                {
                    var drawn = part.DrawnBounds;
                    if (!drawn.HasValue) continue;
                    union = union.HasValue ? LayoutModel.Union(union.Value, drawn.Value) : drawn;
                }

                return union;
            }

            // A sample never exceeds the most opaque texel, so a texture that stays below the
            // threshold, such as the scenery's soft lamplight, draws no point and needs no scan.
            if (_alpha != null && _alpha.MaxAlpha < AlphaThreshold) return null;

            var columns = Intervals(Bounds.xMin, Bounds.xMax);
            var rows = Intervals(Bounds.yMin, Bounds.yMax);

            var bottom = -1;
            for (var row = 0; row <= rows && bottom < 0; row++)
            {
                if (DrawsInRow(row, rows, 0, columns, columns)) bottom = row;
            }

            if (bottom < 0) return null;

            var top = bottom;
            for (var row = rows; row > bottom; row--)
            {
                if (!DrawsInRow(row, rows, 0, columns, columns)) continue;
                top = row;
                break;
            }

            // The bottom row holds a drawn point, so some column between the two rows does.
            var left = 0;
            for (var column = 0; column <= columns; column++)
            {
                if (!DrawsInColumn(column, columns, bottom, top, rows)) continue;
                left = column;
                break;
            }

            var right = left;
            for (var column = columns; column > left; column--)
            {
                if (!DrawsInColumn(column, columns, bottom, top, rows)) continue;
                right = column;
                break;
            }

            return Rect.MinMaxRect(Tick(Bounds.xMin, Bounds.xMax, left, columns),
                Tick(Bounds.yMin, Bounds.yMax, bottom, rows), Tick(Bounds.xMin, Bounds.xMax, right, columns),
                Tick(Bounds.yMin, Bounds.yMax, top, rows));
        }

        /// <summary>Whether the shape draws a sample point of <paramref name="row"/> from column <paramref name="first"/> to <paramref name="last"/>.</summary>
        private bool DrawsInRow(int row, int rows, int first, int last, int columns)
        {
            var y = Tick(Bounds.yMin, Bounds.yMax, row, rows);
            for (var column = first; column <= last; column++)
            {
                if (Contains(new Vector2(Tick(Bounds.xMin, Bounds.xMax, column, columns), y))) return true;
            }

            return false;
        }

        /// <summary>Whether the shape draws a sample point of <paramref name="column"/> from row <paramref name="first"/> to <paramref name="last"/>.</summary>
        private bool DrawsInColumn(int column, int columns, int first, int last, int rows)
        {
            var x = Tick(Bounds.xMin, Bounds.xMax, column, columns);
            for (var row = first; row <= last; row++)
            {
                if (Contains(new Vector2(x, Tick(Bounds.yMin, Bounds.yMax, row, rows)))) return true;
            }

            return false;
        }

        /// <summary>Whether a triangle of the mesh covers <paramref name="point"/> where its texture is opaque enough.</summary>
        private bool MeshDraws(Vector2 point)
        {
            for (var i = 0; i + 2 < _triangles.Length; i += 3)
            {
                var a = _triangles[i];
                var b = _triangles[i + 1];
                var c = _triangles[i + 2];
                var ab = b.Position - a.Position;
                var ac = c.Position - a.Position;
                var ap = point - a.Position;
                var area = ab.x * ac.y - ac.x * ab.y;
                if (Mathf.Abs(area) < DegenerateArea) continue;

                // Barycentric weights of b and c; a's is what remains.
                var wb = (ap.x * ac.y - ac.x * ap.y) / area;
                var wc = (ab.x * ap.y - ap.x * ab.y) / area;
                var wa = 1f - wb - wc;
                if (wa < -EdgeTolerance || wb < -EdgeTolerance || wc < -EdgeTolerance) continue;
                if (_alpha == null) return true;

                // The GPU interpolates UVs linearly across a flat UI triangle.
                var uv = a.Uv * wa + b.Uv * wb + c.Uv * wc;
                if (_alpha.Sample(uv) >= AlphaThreshold) return true;
            }

            return false;
        }

        private static bool Holds(Rect rect, Vector2 point) =>
            point.x >= rect.xMin && point.x <= rect.xMax && point.y >= rect.yMin && point.y <= rect.yMax;

        /// <summary>The number of intervals, none longer than <see cref="SampleStep"/>, that split [min, max].</summary>
        private static int Intervals(float min, float max) =>
            Mathf.Max(0, Mathf.CeilToInt((max - min) / SampleStep - 1e-4f));

        /// <summary>Point <paramref name="index"/> of <paramref name="intervals"/> + 1 evenly spaced points from min to max.</summary>
        private static float Tick(float min, float max, int index, int intervals) =>
            intervals == 0 ? min : index == intervals ? max : min + (max - min) * index / intervals;
    }

    /// <summary>
    /// The alpha of one sprite texture, read from its CPU copy once and filtered as
    /// <see cref="LayoutModel.SampleAlpha"/> filters it.
    /// </summary>
    /// <remarks>
    /// The shape tests sample a sprite many times, and <see cref="LayoutModel.SampleAlpha"/> asks the
    /// engine for four texels at each call. The cache keeps the texels' alpha and applies the same
    /// filter in managed code: texel i centred at (i + 0.5) / width, the four nearest texels blended
    /// bilinearly, and the edge texels repeated outside the texture, as Clamp does. Factory sprite
    /// textures are RGBA32, Bilinear, Clamp and readable. A texture that does not clamp on both axes
    /// is sampled with <see cref="LayoutModel.SampleAlpha"/> itself. <see cref="LayoutModel.Measure"/>
    /// makes one cache per texture for each measurement and reads the texels on first use.
    /// </remarks>
    internal sealed class AlphaCache
    {
        private readonly Texture2D _texture;
        private bool _read;
        private byte[] _alpha;
        private int _width;
        private int _height;
        private float _maxAlpha = 1f;

        internal AlphaCache(Texture2D texture)
        {
            Assert.IsTrue(texture != null && texture.isReadable,
                "AlphaCache: only a texture the CPU can read has an alpha to sample");
            _texture = texture;
        }

        public Texture2D Texture => _texture;

        /// <summary>
        /// The largest alpha that <see cref="Sample"/> can return, from 0 to 1: that of the most
        /// opaque texel, which a bilinear blend of four texels never exceeds; 1 for a texture that
        /// is sampled through <see cref="LayoutModel.SampleAlpha"/>.
        /// </summary>
        public float MaxAlpha
        {
            get
            {
                if (!_read) Read();
                return _maxAlpha;
            }
        }

        /// <summary>The filtered alpha at <paramref name="uv"/>, from 0 to 1.</summary>
        public float Sample(Vector2 uv)
        {
            if (!_read) Read();
            if (_alpha == null) return LayoutModel.SampleAlpha(_texture, uv);

            var x = uv.x * _width - 0.5f;
            var y = uv.y * _height - 0.5f;
            var left = Mathf.FloorToInt(x);
            var bottom = Mathf.FloorToInt(y);
            var across = x - left;
            var up = y - bottom;

            var x0 = Mathf.Clamp(left, 0, _width - 1);
            var x1 = Mathf.Clamp(left + 1, 0, _width - 1);
            var row0 = Mathf.Clamp(bottom, 0, _height - 1) * _width;
            var row1 = Mathf.Clamp(bottom + 1, 0, _height - 1) * _width;

            var lower = Mathf.Lerp(_alpha[row0 + x0], _alpha[row0 + x1], across);
            var upper = Mathf.Lerp(_alpha[row1 + x0], _alpha[row1 + x1], across);
            return Mathf.Lerp(lower, upper, up) / 255f;
        }

        private void Read()
        {
            _read = true;
            _width = _texture.width;
            _height = _texture.height;
            if (_texture.wrapModeU != TextureWrapMode.Clamp || _texture.wrapModeV != TextureWrapMode.Clamp) return;

            // Rows run bottom to top, as UVs do.
            var pixels = _texture.GetPixels32();
            _alpha = new byte[pixels.Length];
            byte max = 0;
            for (var i = 0; i < pixels.Length; i++)
            {
                _alpha[i] = pixels[i].a;
                if (pixels[i].a > max) max = pixels[i].a;
            }

            _maxAlpha = max / 255f;
        }
    }

    /// <summary>
    /// A composite of the Game_UI, such as a Plaque or a Plank: a root recognised by name and the
    /// background pieces drawn behind its content.
    /// </summary>
    public sealed class CompositeElement
    {
        internal CompositeElement(CompositeKind kind, Transform root, string path)
        {
            Kind = kind;
            Root = root;
            Path = path;
            BackgroundPieces = Array.Empty<MeasuredElement>();
            Shape = VisibleShape.Nothing(Vector2.zero);
        }

        public CompositeKind Kind { get; }

        public Transform Root { get; }

        /// <summary>The root's hierarchy path from the Active_Layer root.</summary>
        public string Path { get; }

        /// <summary>The composite's background pieces, in draw order.</summary>
        public IReadOnlyList<MeasuredElement> BackgroundPieces { get; internal set; }

        /// <summary>
        /// The composite's Visible_Shape: the union of its background pieces' shapes. It is empty,
        /// at the root's position, when no piece shows, as for a Monogram, which has none.
        /// </summary>
        public VisibleShape Shape { get; internal set; }

        public override string ToString() => Kind + " " + Path;
    }

    /// <summary>One element of a <see cref="Measurement"/>: a graphic, or a TrackedLabel.</summary>
    public sealed class MeasuredElement
    {
        internal MeasuredElement(int order, string path, ElementKind kind, RectTransform rectTransform,
            IReadOnlyList<Graphic> graphics, IReadOnlyList<GraphicMesh> meshes, bool culled, Rect bounds,
            Rect ownRect, Rect? clipRect, string content, Rect textBounds, int glyphCount, bool isPlate,
            VisibleShape shape)
        {
            Assert.IsTrue(shape != null, "MeasuredElement: " + path + " has no Visible_Shape");

            Order = order;
            Path = path;
            Kind = kind;
            RectTransform = rectTransform;
            Graphics = graphics;
            Meshes = meshes;
            Culled = culled;
            Bounds = bounds;
            OwnRect = ownRect;
            ClipRect = clipRect;
            Content = content;
            TextBounds = textBounds;
            GlyphCount = glyphCount;
            IsPlate = isPlate;
            Shape = shape;
            Chain = Array.Empty<MeasuredElement>();
        }

        /// <summary>The draw order index: an element is drawn over every element with a lower one.</summary>
        public int Order { get; }

        /// <summary>The hierarchy path from the Active_Layer root.</summary>
        public string Path { get; }

        public ElementKind Kind { get; }

        /// <summary>The graphic's RectTransform, or the row of a TrackedLabel.</summary>
        public RectTransform RectTransform { get; }

        public Transform Transform => RectTransform;

        /// <summary>The graphic; null for a TrackedLabel, whose letters are in <see cref="Graphics"/>.</summary>
        public Graphic Graphic => Kind == ElementKind.TrackedText ? null : Graphics[0];

        /// <summary>The graphic, or the letters of a TrackedLabel.</summary>
        public IReadOnlyList<Graphic> Graphics { get; }

        /// <summary>The meshes of <see cref="Graphics"/> that are drawn; empty when culled.</summary>
        public IReadOnlyList<GraphicMesh> Meshes { get; }

        /// <summary>
        /// Whether a <c>RectMask2D</c> culls the element, as when it is scrolled out of view. A
        /// culled element is not drawn; its bounds come from its RectTransform.
        /// </summary>
        public bool Culled { get; }

        /// <summary>The mesh bounds in Canvas_Units, or the RectTransform's bounds when culled.</summary>
        public Rect Bounds { get; }

        /// <summary>The bounds of the RectTransform in Canvas_Units: the row of a TrackedLabel.</summary>
        public Rect OwnRect { get; }

        /// <summary>The intersection of the <c>RectMask2D</c> ancestors' rects; null when none clips.</summary>
        public Rect? ClipRect { get; }

        /// <summary>
        /// The Visible_Shape: where the element shows, from its sprite's alpha, its Text_Bounds or
        /// its whole mesh, cut to <see cref="ClipRect"/>; empty when culled.
        /// </summary>
        public VisibleShape Shape { get; }

        public bool IsText => Kind != ElementKind.Image;

        /// <summary>The string shown by a Text or a TrackedLabel; null for an image.</summary>
        public string Content { get; }

        /// <summary>
        /// The union of the glyph quads, without <c>Shadow</c> copies; for a Text or a TrackedLabel
        /// only, from the RectTransform when culled.
        /// </summary>
        public Rect TextBounds { get; }

        /// <summary>The number of glyph quads drawn; 0 for an image or when culled.</summary>
        public int GlyphCount { get; }

        /// <summary>Whether the element is a plate: an Image with a panel sprite.</summary>
        public bool IsPlate { get; }

        /// <summary>The innermost composite that holds the element, or null.</summary>
        public CompositeElement Composite { get; internal set; }

        /// <summary>Whether the element is a background piece of a composite.</summary>
        public bool IsBackgroundPiece { get; internal set; }

        /// <summary>Whether the element is Decor.</summary>
        public bool IsDecor { get; internal set; }

        /// <summary>The plate that frames the element; null when its Container is the Viewport.</summary>
        public MeasuredElement Container { get; internal set; }

        /// <summary>
        /// The Container_Chain: the Containers up to the Viewport, plus every background piece of the
        /// composites on that chain.
        /// </summary>
        public IReadOnlyList<MeasuredElement> Chain { get; internal set; }

        public override string ToString() => Order + " " + Path + " (" + Kind + ")";
    }

    /// <summary>An Interactive_Control of the Active_Layer: a <c>Selectable</c> or a <c>ScrollRect</c>.</summary>
    public sealed class InteractiveControl
    {
        internal InteractiveControl(ControlKind kind, Behaviour component, string path, Rect ownRect,
            Rect? clipRect, bool interactable)
        {
            Kind = kind;
            Component = component;
            Path = path;
            OwnRect = ownRect;
            ClipRect = clipRect;
            Interactable = interactable;
        }

        public ControlKind Kind { get; }

        /// <summary>The <c>Selectable</c> or the <c>ScrollRect</c>.</summary>
        public Behaviour Component { get; }

        public RectTransform RectTransform => (RectTransform)Component.transform;

        /// <summary>The hierarchy path from the Active_Layer root.</summary>
        public string Path { get; }

        /// <summary>The bounds of the control's RectTransform in Canvas_Units.</summary>
        public Rect OwnRect { get; }

        /// <summary>The intersection of the <c>RectMask2D</c> ancestors' rects; null when none clips.</summary>
        public Rect? ClipRect { get; }

        /// <summary>Whether the control accepts input now.</summary>
        public bool Interactable { get; }

        public override string ToString() => Kind + " " + Path;
    }

    /// <summary>The room's floor tiles as projected onto the canvas, with the size of the session's grid.</summary>
    internal readonly struct RoomProjection
    {
        internal RoomProjection(Rect? area, int tiles, Vector2Int? grid)
        {
            Area = area;
            Tiles = tiles;
            Grid = grid;
        }

        /// <summary>The union of the tiles' projected bounds in Canvas_Units; null without tiles.</summary>
        public Rect? Area { get; }

        /// <summary>The number of floor tiles the room draws.</summary>
        public int Tiles { get; }

        /// <summary>The Width and Height of <c>Session.Profile.Grid</c>; null without a session.</summary>
        public Vector2Int? Grid { get; }

        /// <summary>Fails when the room draws tiles, but not one for each cell of the session's grid.</summary>
        public void AssertMatchesGrid()
        {
            if (Tiles == 0 || !Grid.HasValue) return;

            Assert.IsTrue(Tiles == Grid.Value.x * Grid.Value.y,
                "LayoutModel: the room draws " + Tiles + " floor tiles but Session.Profile.Grid is " +
                Grid.Value.x + "x" + Grid.Value.y + ", so the Room_Grid_Area would be another room's");
        }
    }

    /// <summary>One measurement of the Active_Layer, made by <see cref="LayoutModel.Measure"/>.</summary>
    public sealed class Measurement
    {
        private readonly Dictionary<Transform, MeasuredElement> _byTransform;
        private readonly RoomProjection _room;

        internal Measurement(Transform layer, string layerPath, Vector2Int window, Rect viewport, float scaleFactor,
            List<MeasuredElement> elements, List<CompositeElement> composites, List<InteractiveControl> controls,
            Dictionary<Transform, MeasuredElement> byTransform, RoomProjection room)
        {
            _room = room;
            Layer = layer;
            LayerPath = layerPath;
            Window = window;
            Viewport = viewport;
            ScaleFactor = scaleFactor;
            Elements = elements;
            Composites = composites;
            Controls = controls;
            _byTransform = byTransform;
        }

        /// <summary>The root of the Active_Layer.</summary>
        public Transform Layer { get; }

        /// <summary>The full hierarchy path of the Active_Layer root.</summary>
        public string LayerPath { get; }

        /// <summary>The <see cref="ResolutionRig"/> window measured in, in pixels.</summary>
        public Vector2Int Window { get; }

        /// <summary>The Viewport: the whole canvas, in Canvas_Units.</summary>
        public Rect Viewport { get; }

        /// <summary>The canvas's scale factor: window pixels per Canvas_Unit.</summary>
        public float ScaleFactor { get; }

        /// <summary>The elements in draw order, culled ones included.</summary>
        public IReadOnlyList<MeasuredElement> Elements { get; }

        /// <summary>The composites, in hierarchy order.</summary>
        public IReadOnlyList<CompositeElement> Composites { get; }

        /// <summary>The Interactive_Controls, in hierarchy order.</summary>
        public IReadOnlyList<InteractiveControl> Controls { get; }

        /// <summary>
        /// The Room_Grid_Area in the measured window: the rect, in Canvas_Units, that the room's
        /// floor tiles covered on screen when the measurement was made; null when there was no room
        /// or it drew no tile.
        /// </summary>
        /// <remarks>
        /// Projected as <see cref="LayoutModel.RoomGridArea"/> projects it. The measurement takes it
        /// on every Screen, but checks it only when it is read: reading it fails when the room drew
        /// a different number of tiles from the cells of <c>Session.Profile.Grid</c>, as a room not
        /// yet rebuilt for the session's grid would.
        /// </remarks>
        public Rect? RoomGridArea
        {
            get
            {
                _room.AssertMatchesGrid();
                return _room.Area;
            }
        }

        /// <summary>The room's projection as it was measured, without the grid check of <see cref="RoomGridArea"/>.</summary>
        internal RoomProjection Room => _room;

        /// <summary>The element drawn by <paramref name="transform"/>, or null.</summary>
        public MeasuredElement ElementOf(Transform transform)
        {
            MeasuredElement element;
            return transform != null && _byTransform.TryGetValue(transform, out element) ? element : null;
        }
    }
}
