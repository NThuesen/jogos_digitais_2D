using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Tests.PlayMode.Layout
{
    /// <summary>
    /// Whether the text of an illustrated screen (UI-INT-01) can actually be read, measured from the
    /// rendered meshes of the model rather than from the strings.
    /// </summary>
    /// <remarks>
    /// For every visible label under a screen: its glyphs must lie inside the Viewport; they must lie
    /// inside the <i>drawn</i> bounds of the nearest plate it sits on (the sprite's alpha, not its
    /// rect, so a label cannot hang over a plate's transparent margin or ornament); and no Image drawn
    /// later may overlap the glyphs, because a later image paints over them.
    /// </remarks>
    public static class KitLegibility
    {
        /// <summary>
        /// How far, in Canvas_Units, a glyph box may stick out of a plate's drawn bounds: the one
        /// <see cref="VisibleShape.SampleStep"/> by which <c>DrawnBounds</c> can fall short of the true
        /// drawn edge, plus the glyph quad's own rounding to a pixel. Far below a label that is cut off.
        /// </summary>
        public const float Tolerance = 1f + VisibleShape.SampleStep;

        /// <summary>Overlap, in Canvas_Units, below which a later image is only touching the label.</summary>
        public const float OverlapTolerance = 1f;

        /// <summary>Adds to <paramref name="found"/> every label under <paramref name="screen"/> that is not legible.</summary>
        /// <returns>How many labels were measured.</returns>
        public static int Check(Measurement measurement, Transform screen, string where, List<string> found)
        {
            var measured = 0;
            foreach (var element in measurement.Elements)
            {
                if (!element.IsText || !element.Transform.IsChildOf(screen)) continue;

                var text = element.Graphics.Count > 0 ? element.Graphics[0] as Text : null;
                if (text == null || string.IsNullOrWhiteSpace(text.text)) continue;

                measured++;
                var name = where + ": \"" + text.text + "\" (" + element.Path + ")";
                var bounds = element.Shape.Bounds;
                if (element.Shape.IsEmpty || bounds.width <= 0f || bounds.height <= 0f)
                {
                    found.Add(name + " draws no glyph, or is clipped away");
                    continue;
                }

                if (!Contains(measurement.Viewport, bounds, Tolerance))
                {
                    found.Add(name + " lies at " + LayoutModel.Describe(bounds) + ", outside the Viewport " +
                              LayoutModel.Describe(measurement.Viewport));
                }

                var plate = PlateOf(measurement, element);
                if (plate != null)
                {
                    var drawn = plate.Shape.DrawnBounds;
                    if (!drawn.HasValue)
                    {
                        found.Add(name + " sits on " + plate.Path + ", which draws nothing");
                    }
                    else if (!Contains(drawn.Value, bounds, Tolerance))
                    {
                        found.Add(name + " lies at " + LayoutModel.Describe(bounds) + ", outside the drawn bounds " +
                                  LayoutModel.Describe(drawn.Value) + " of its plate " + plate.Path);
                    }
                }

                foreach (var other in measurement.Elements)
                {
                    if (other.Order <= element.Order || other.Kind != ElementKind.Image) continue;
                    if (other.Shape.IsEmpty) continue;

                    var area = other.Shape.DrawnBounds ?? other.Shape.Bounds;
                    var width = Mathf.Min(area.xMax, bounds.xMax) - Mathf.Max(area.xMin, bounds.xMin);
                    var height = Mathf.Min(area.yMax, bounds.yMax) - Mathf.Max(area.yMin, bounds.yMin);
                    if (width > OverlapTolerance && height > OverlapTolerance)
                    {
                        found.Add(name + " is covered by " + other.Path + " (drawn later) over " +
                                  LayoutModel.Describe(area));
                    }
                }
            }

            return measured;
        }

        /// <summary>The nearest ancestor of a label that is an Image element: the plate it is drawn on.</summary>
        private static MeasuredElement PlateOf(Measurement measurement, MeasuredElement label)
        {
            for (var node = label.Transform.parent; node != null; node = node.parent)
            {
                var candidate = measurement.ElementOf(node);
                if (candidate != null && candidate.Kind == ElementKind.Image) return candidate;
                if (node == measurement.Layer) break;
            }

            return null;
        }

        private static bool Contains(Rect outer, Rect inner, float tolerance) =>
            inner.xMin >= outer.xMin - tolerance && inner.xMax <= outer.xMax + tolerance &&
            inner.yMin >= outer.yMin - tolerance && inner.yMax <= outer.yMax + tolerance;
    }
}
