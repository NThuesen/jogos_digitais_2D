using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Neko.Tests.PlayMode.Layout
{
    /// <summary>
    /// The Layout_Check's clicks on one <see cref="Measurement"/>: the raycast targets of the
    /// Active_Layer, the Hit_Area and the sample points of each Interactive_Control, the topmost
    /// raycast target and handler at a point, and pointer clicks. Points are in Canvas_Units.
    /// </summary>
    /// <remarks>
    /// The raycast targets are the graphics that <c>GraphicRaycaster</c> tests: active and enabled,
    /// with <c>raycastTarget</c> on, not culled, and placed by the canvas (<c>depth != -1</c>), in
    /// draw order. Each one's rect is cut to its <c>RectMask2D</c> ancestors, and to a
    /// <c>RectMask2D</c> on its own object, which <c>GraphicRaycaster</c> also asks. The game uses
    /// no <c>CanvasGroup</c>, <c>Mask</c> or alpha hit threshold, and raycast padding is 0, so a
    /// clipped rect is where <c>GraphicRaycaster</c> hits its target. Rules R6.1, R6.3 and R14.5
    /// do rect math on these rects; rule R6.2 asks <see cref="TopHandler"/> at
    /// <see cref="SamplePoints"/>.
    ///
    /// <see cref="TopTarget"/>, <see cref="TopHandler"/> and <see cref="Click"/> ask
    /// <c>EventSystem.RaycastAll</c>, whose first result task 1.3 found to be the topmost raycast
    /// target in draw order, also in batch mode. The point goes in as a window pixel, converted as
    /// task 1.3 converts it: Canvas_Units times <c>canvas.scaleFactor</c>. Those three read the
    /// canvas as it is when they are called. The raycast targets, Hit_Areas and sample points come
    /// from the hierarchy and the measurement as they were when the helper was made, so make a new
    /// helper after the layout changes, as after a click that shows another Screen.
    /// </remarks>
    public sealed class Clicks
    {
        /// <summary>Overlap_Tolerance: 1 Canvas_Unit. The sample points stay this far inside a control's drawn bounds.</summary>
        public const float OverlapTolerance = 1f;

        /// <summary>The largest spacing, in Canvas_Units, between neighbouring points of a control's sample grid.</summary>
        public const float SampleSpacing = 4f;

        private readonly Canvas _canvas;
        private readonly List<RaycastTarget> _targets = new List<RaycastTarget>();
        private readonly List<RaycastResult> _results = new List<RaycastResult>();

        /// <summary>Reads the raycast targets of the Active_Layer that <paramref name="measurement"/> measured.</summary>
        public Clicks(Measurement measurement)
        {
            Assert.IsTrue(measurement != null, "Clicks: there is no measurement to click on");

            var canvas = measurement.Layer != null ? measurement.Layer.GetComponentInParent<Canvas>(true) : null;
            Assert.IsTrue(canvas != null, "Clicks: the Active_Layer " + measurement.LayerPath + " is not on a canvas");

            Measurement = measurement;
            _canvas = canvas.rootCanvas;
            Collect(measurement.Layer);
        }

        /// <summary>The measurement whose Active_Layer, controls and shapes the helper reads.</summary>
        public Measurement Measurement { get; }

        /// <summary>The raycast targets of the Active_Layer in draw order, each with its clipped rect.</summary>
        public IReadOnlyList<RaycastTarget> Targets => _targets;

        /// <summary>
        /// The object that receives a click on <paramref name="target"/>, or a scroll for
        /// <see cref="ControlKind.ScrollArea"/>: the nearest object, from the target up, with an active
        /// <c>IPointerClickHandler</c> (<c>IScrollHandler</c>), as <c>ExecuteEvents.GetEventHandler</c>
        /// finds it; null when there is none.
        /// </summary>
        public static GameObject HandlerOf(GameObject target, ControlKind kind)
        {
            if (target == null) return null;

            return kind == ControlKind.ScrollArea
                ? ExecuteEvents.GetEventHandler<IScrollHandler>(target)
                : ExecuteEvents.GetEventHandler<IPointerClickHandler>(target);
        }

        /// <summary>
        /// The Hit_Area of <paramref name="control"/>: the raycast targets whose handler
        /// (<see cref="HandlerOf"/> for the control's kind) is the control, and the union of their
        /// clipped rects.
        /// </summary>
        /// <remarks>A target that its masks clip to no area takes no click, so it is left out.</remarks>
        public HitArea HitAreaOf(InteractiveControl control)
        {
            Assert.IsTrue(control != null && control.Component != null, "Clicks.HitAreaOf: there is no control");

            var owner = control.Component.gameObject;
            var targets = new List<RaycastTarget>();
            Rect? bounds = null;
            foreach (var target in _targets)
            {
                var handler = control.Kind == ControlKind.ScrollArea ? target.ScrollHandler : target.ClickHandler;
                var rect = target.ClippedRect;
                if (handler != owner || rect.width <= 0f || rect.height <= 0f) continue;

                targets.Add(target);
                bounds = bounds.HasValue ? LayoutModel.Union(bounds.Value, rect) : rect;
            }

            return new HitArea(control, targets, bounds);
        }

        /// <summary>
        /// The points at which rule R6.2 clicks <paramref name="control"/>, in Canvas_Units: first the
        /// centre of the drawn bounds of its Visible_Shape, then every point of a grid over those
        /// bounds, inset by <see cref="OverlapTolerance"/>, that the shape holds. Empty when the
        /// control draws nothing.
        /// </summary>
        /// <remarks>
        /// The Visible_Shape is the composite's when the control is the root of a Composite_Element,
        /// such as a Plank; else the shape of the element that the control's own graphic draws, or,
        /// for a <c>Selectable</c> without one, its target graphic's. The grid points lie at most
        /// <see cref="SampleSpacing"/> apart, and the inset rect's edges and corners are among them.
        /// </remarks>
        public IReadOnlyList<Vector2> SamplePoints(InteractiveControl control)
        {
            Assert.IsTrue(control != null && control.Component != null, "Clicks.SamplePoints: there is no control");

            var points = new List<Vector2>();
            var shape = ShapeOf(control);
            var drawn = shape != null ? shape.DrawnBounds : null;
            if (!drawn.HasValue) return points;

            var bounds = drawn.Value;
            var centre = bounds.center;
            points.Add(centre);

            var xMin = bounds.xMin + OverlapTolerance;
            var xMax = bounds.xMax - OverlapTolerance;
            var yMin = bounds.yMin + OverlapTolerance;
            var yMax = bounds.yMax - OverlapTolerance;
            if (xMax < xMin || yMax < yMin) return points;

            var columns = Intervals(xMin, xMax);
            var rows = Intervals(yMin, yMax);
            for (var row = 0; row <= rows; row++)
            {
                var y = Tick(yMin, yMax, row, rows);
                for (var column = 0; column <= columns; column++)
                {
                    var point = new Vector2(Tick(xMin, xMax, column, columns), y);
                    if (point != centre && shape.Contains(point)) points.Add(point);
                }
            }

            return points;
        }

        /// <summary>
        /// The object of the topmost <c>RaycastAll</c> result at <paramref name="point"/>, in
        /// Canvas_Units: what a click there lands on, or what blocks it; null when nothing takes a
        /// raycast there.
        /// </summary>
        public GameObject TopTarget(Vector2 point)
        {
            var top = Raycast(point, nameof(TopTarget), out _, out _);
            return top.HasValue ? top.Value.gameObject : null;
        }

        /// <summary>
        /// The handler (<see cref="HandlerOf"/>) of the topmost <c>RaycastAll</c> result at
        /// <paramref name="point"/>, in Canvas_Units: the object that a click there reaches, or that a
        /// scroll there reaches for <see cref="ControlKind.ScrollArea"/>; null when there is none.
        /// </summary>
        public GameObject TopHandler(Vector2 point, ControlKind kind = ControlKind.Selectable)
        {
            var top = Raycast(point, nameof(TopHandler), out _, out _);
            return top.HasValue ? HandlerOf(top.Value.gameObject, kind) : null;
        }

        /// <summary>
        /// Clicks at <paramref name="point"/>, in Canvas_Units, as the input module does on a left
        /// click: the click handler of the topmost <c>RaycastAll</c> result receives a left-button
        /// <c>PointerEventData</c> at the point's window pixel, with that result as its raycast.
        /// Returns the handler, or null when the click reaches none, as when a plain panel takes it.
        /// </summary>
        /// <remarks>
        /// The handler reacts as to a real click: a <c>Button</c> runs its <c>onClick</c> only when it
        /// is active and interactable.
        /// </remarks>
        public GameObject Click(Vector2 point)
        {
            var top = Raycast(point, nameof(Click), out var system, out var pixel);
            if (!top.HasValue) return null;

            var result = top.Value;
            var handler = HandlerOf(result.gameObject, ControlKind.Selectable);
            if (handler == null) return null;

            var click = new PointerEventData(system)
            {
                button = PointerEventData.InputButton.Left,
                position = pixel,
                pressPosition = pixel,
                pointerCurrentRaycast = result,
                pointerPressRaycast = result,
                rawPointerPress = result.gameObject,
                pointerPress = handler,
                pointerClick = handler,
                eligibleForClick = true,
                clickCount = 1,
                clickTime = Time.unscaledTime,
            };

            ExecuteEvents.Execute(handler, click, ExecuteEvents.pointerClickHandler);
            return handler;
        }

        /// <summary>
        /// The topmost <c>RaycastAll</c> result at <paramref name="point"/>, in Canvas_Units, or null.
        /// Fails when there is no <c>EventSystem.current</c>, or when the canvas has changed scale
        /// since the measurement, so that the point would land elsewhere.
        /// </summary>
        private RaycastResult? Raycast(Vector2 point, string caller, out EventSystem system, out Vector2 pixel)
        {
            system = EventSystem.current;
            Assert.IsTrue(system != null, "Clicks." + caller + ": there is no EventSystem.current, so nothing can be " +
                                          "raycast or clicked at " + LayoutReport.Point(point));

            var scale = _canvas.scaleFactor;
            Assert.IsTrue(Mathf.Approximately(scale, Measurement.ScaleFactor),
                "Clicks." + caller + ": the canvas scale factor is " + Scale(scale) + " but " + Measurement.LayerPath +
                " was measured at " + Scale(Measurement.ScaleFactor) + "; measure again after the window changes");

            pixel = point * scale;
            _results.Clear();
            system.RaycastAll(new PointerEventData(system) { position = pixel }, _results);
            return _results.Count > 0 ? _results[0] : (RaycastResult?)null;
        }

        /// <summary>The Visible_Shape that <see cref="SamplePoints"/> samples, or null when the control draws no element.</summary>
        private VisibleShape ShapeOf(InteractiveControl control)
        {
            var node = control.Component.transform;
            foreach (var composite in Measurement.Composites)
            {
                if (composite.Root == node) return composite.Shape;
            }

            var element = Measurement.ElementOf(node);
            var selectable = control.Component as Selectable;
            if (element == null && selectable != null && selectable.targetGraphic != null)
            {
                element = Measurement.ElementOf(selectable.targetGraphic.transform);
            }

            return element != null ? element.Shape : null;
        }

        private void Collect(Transform node)
        {
            if (!node.gameObject.activeInHierarchy) return;

            var graphic = node.GetComponent<Graphic>();
            if (graphic != null && graphic.isActiveAndEnabled && graphic.raycastTarget &&
                !graphic.canvasRenderer.cull && graphic.depth != -1)
            {
                var clip = LayoutModel.AncestorClipRect(node);
                var own = node.GetComponent<RectMask2D>();
                if (own != null && own.isActiveAndEnabled)
                {
                    var mask = LayoutModel.MaskRect(own);
                    clip = clip.HasValue ? LayoutModel.Intersect(clip.Value, mask) : mask;
                }

                _targets.Add(new RaycastTarget(graphic, LayoutModel.PathFrom(Measurement.Layer, node),
                    LayoutModel.BoundsOf(LayoutModel.RectCorners(graphic.rectTransform)), clip));
            }

            for (var i = 0; i < node.childCount; i++) Collect(node.GetChild(i));
        }

        /// <summary>The number of intervals, none longer than <see cref="SampleSpacing"/>, that split [min, max].</summary>
        private static int Intervals(float min, float max) =>
            Mathf.Max(0, Mathf.CeilToInt((max - min) / SampleSpacing - 1e-4f));

        /// <summary>Point <paramref name="index"/> of <paramref name="intervals"/> + 1 evenly spaced points from min to max.</summary>
        private static float Tick(float min, float max, int index, int intervals) =>
            intervals == 0 ? min : index == intervals ? max : min + (max - min) * index / intervals;

        private static string Scale(float value) => value.ToString("0.####", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// A raycast target of the Active_Layer: a graphic that <c>GraphicRaycaster</c> tests, with the
    /// rect where it takes raycasts and the objects its clicks and scrolls reach.
    /// </summary>
    public sealed class RaycastTarget
    {
        internal RaycastTarget(Graphic graphic, string path, Rect ownRect, Rect? clipRect)
        {
            Graphic = graphic;
            Path = path;
            OwnRect = ownRect;
            ClipRect = clipRect;
            ClippedRect = clipRect.HasValue ? LayoutModel.Intersect(ownRect, clipRect.Value) : ownRect;
            ClickHandler = Clicks.HandlerOf(graphic.gameObject, ControlKind.Selectable);
            ScrollHandler = Clicks.HandlerOf(graphic.gameObject, ControlKind.ScrollArea);
        }

        public Graphic Graphic { get; }

        /// <summary>The hierarchy path from the Active_Layer root.</summary>
        public string Path { get; }

        /// <summary>The bounds of the graphic's RectTransform in Canvas_Units.</summary>
        public Rect OwnRect { get; }

        /// <summary>The intersection of the <c>RectMask2D</c> rects that cut the target's raycasts; null when none does.</summary>
        public Rect? ClipRect { get; }

        /// <summary>The own rect cut to the clip rect: where <c>GraphicRaycaster</c> hits the target, in Canvas_Units.</summary>
        public Rect ClippedRect { get; }

        /// <summary>The object that a click on the target reaches, or null.</summary>
        public GameObject ClickHandler { get; }

        /// <summary>The object that a scroll on the target reaches, or null.</summary>
        public GameObject ScrollHandler { get; }

        public override string ToString() => Path + " at " + LayoutModel.Describe(ClippedRect);
    }

    /// <summary>
    /// The Hit_Area of an Interactive_Control: the raycast targets through which a click, or a
    /// scroll for a scroll area, reaches the control, and the union of their clipped rects.
    /// </summary>
    public sealed class HitArea
    {
        internal HitArea(InteractiveControl control, IReadOnlyList<RaycastTarget> targets, Rect? bounds)
        {
            Control = control;
            Targets = targets;
            Bounds = bounds;
        }

        public InteractiveControl Control { get; }

        /// <summary>The raycast targets whose handler is the control, in draw order.</summary>
        public IReadOnlyList<RaycastTarget> Targets { get; }

        /// <summary>
        /// The union of the targets' clipped rects, the smallest rect that holds them all, in
        /// Canvas_Units; null when no raycast target reaches the control.
        /// </summary>
        public Rect? Bounds { get; }

        public override string ToString() =>
            Control + ": " + (Bounds.HasValue
                ? LayoutModel.Describe(Bounds.Value) + " from " + Targets.Count + " raycast targets"
                : "no raycast target");
    }
}
