using UnityEngine;

namespace Neko.Game.Art
{
    /// <summary>
    /// Draws the shape vocabulary into a pixel buffer, as signed distance fields.
    /// </summary>
    /// <remarks>
    /// Distance fields rather than scanline polygons for three reasons that all matter here:
    /// the edge stays clean at icon sizes without any supersampling, an outline is a second
    /// threshold on the same number and therefore free, and the whole thing is a pure
    /// function of its inputs — so generated art is reviewable and testable without a GPU.
    ///
    /// Deliberately returns a <see cref="Color32"/> buffer rather than a texture: this half
    /// runs in a headless unit test, and <c>Texture2D.Apply</c> wants a graphics device.
    /// </remarks>
    public static class ShapeRasteriser
    {
        /// <summary>Half a pixel of smoothing. Less aliases; more looks blurred at 32 px.</summary>
        private const float EdgeSoftness = 1.2f;

        public static Color32[] Render(ShapeSpec spec, int size)
        {
            var pixels = new Color32[size * size];
            Blend(pixels, size, spec);
            return pixels;
        }

        /// <summary>Draws one shape over whatever is already in the buffer.</summary>
        public static void Blend(Color32[] pixels, int size, ShapeSpec spec)
        {
            if (pixels == null || size <= 0) return;

            // One pixel, expressed in the [-1, 1] space the distance functions work in.
            var pixel = EdgeSoftness * 2f / size;

            var radians = -spec.RotationDegrees * Mathf.Deg2Rad;
            var cos = Mathf.Cos(radians);
            var sin = Mathf.Sin(radians);

            var scaleX = Mathf.Max(1e-3f, spec.Scale.x);
            var scaleY = Mathf.Max(1e-3f, spec.Scale.y);

            for (var y = 0; y < size; y++)
            {
                // v runs bottom-up so the buffer matches Unity's texture orientation.
                var v = (y + 0.5f) / size * 2f - 1f;

                for (var x = 0; x < size; x++)
                {
                    var u = (x + 0.5f) / size * 2f - 1f;

                    var px = (u - spec.Offset.x) / scaleX;
                    var py = (v - spec.Offset.y) / scaleY;

                    var rx = px * cos - py * sin;
                    var ry = px * sin + py * cos;

                    var distance = SignedDistance(spec.Shape, new Vector2(rx, ry), spec.Detail);

                    // The field is measured in the unscaled space, so the softening has to be
                    // scaled back or a squashed shape gets a fuzzy edge on one axis only.
                    var softness = pixel / Mathf.Min(scaleX, scaleY);

                    var inside = 1f - Mathf.Clamp01((distance + softness * 0.5f) / softness);
                    if (inside <= 0.001f) continue;

                    var colour = spec.GradientTo.HasValue
                        ? Color.Lerp(spec.GradientTo.Value, spec.Fill, (v + 1f) * 0.5f)
                        : spec.Fill;

                    if (spec.Outline.HasValue && spec.OutlineWidth > 0f)
                    {
                        // Inside the band nearest the edge, the outline wins. One extra
                        // comparison on a number that was already computed.
                        var edge = Mathf.Clamp01(
                            (distance + spec.OutlineWidth) / Mathf.Max(softness, 1e-5f));
                        colour = Color.Lerp(spec.Outline.Value, colour, 1f - edge);
                    }

                    Over(pixels, y * size + x, colour, inside * colour.a);
                }
            }
        }

        private static void Over(Color32[] pixels, int index, Color colour, float alpha)
        {
            var dst = pixels[index];
            var dstAlpha = dst.a / 255f;
            var outAlpha = alpha + dstAlpha * (1f - alpha);
            if (outAlpha <= 0f) return;

            var r = (colour.r * alpha + dst.r / 255f * dstAlpha * (1f - alpha)) / outAlpha;
            var g = (colour.g * alpha + dst.g / 255f * dstAlpha * (1f - alpha)) / outAlpha;
            var b = (colour.b * alpha + dst.b / 255f * dstAlpha * (1f - alpha)) / outAlpha;

            pixels[index] = new Color32(
                (byte)Mathf.Clamp(r * 255f, 0f, 255f),
                (byte)Mathf.Clamp(g * 255f, 0f, 255f),
                (byte)Mathf.Clamp(b * 255f, 0f, 255f),
                (byte)Mathf.Clamp(outAlpha * 255f, 0f, 255f));
        }

        /// <summary>
        /// Distance to the shape's edge. Negative inside.
        /// </summary>
        /// <remarks>
        /// Shapes are sized to sit inside roughly 0.86 of the half-extent, which leaves room
        /// for an outline and for a dropped shadow without either being clipped by the sprite
        /// border.
        /// </remarks>
        public static float SignedDistance(ShapeId shape, Vector2 p, float detail)
        {
            switch (shape)
            {
                case ShapeId.Circle:
                    return p.magnitude - 0.86f;

                case ShapeId.Capsule:
                    return Capsule(p, 0.34f, 0.52f);

                case ShapeId.RoundedRect:
                    return RoundedBox(p, new Vector2(0.84f, 0.84f),
                        Mathf.Lerp(0.04f, 0.42f, Mathf.Clamp01(detail)));

                case ShapeId.Triangle:
                    return Triangle(p * 1.05f);

                case ShapeId.Star:
                    return Star(p, 5, Mathf.Lerp(0.3f, 0.6f, 1f - Mathf.Clamp01(detail)));

                case ShapeId.Sparkle:
                    return Star(p, 4, 0.22f);

                case ShapeId.Ring:
                    return Mathf.Abs(p.magnitude - 0.68f) -
                           Mathf.Lerp(0.06f, 0.3f, Mathf.Clamp01(detail));

                case ShapeId.Heart:
                    return Heart(p);

                case ShapeId.Teardrop:
                    return Teardrop(p);

                case ShapeId.Crescent:
                    return Mathf.Max(p.magnitude - 0.86f,
                        -(new Vector2(p.x + 0.42f, p.y).magnitude - 0.72f));

                case ShapeId.CatEar:
                    return Mathf.Max(Triangle(new Vector2(p.x, p.y) * 1.1f), -0.98f - p.y);

                case ShapeId.Bow:
                    return Bow(p);

                case ShapeId.Apron:
                    return Trapezoid(p, 0.84f, 0.46f, 0.84f);

                case ShapeId.Plate:
                    return RoundedBox(new Vector2(p.x, p.y * 2.6f), new Vector2(0.86f, 0.86f), 0.42f);

                case ShapeId.Steam:
                    return Steam(p);

                case ShapeId.Door:
                    return Mathf.Min(
                        RoundedBox(new Vector2(p.x, p.y - 0.08f), new Vector2(0.7f, 0.92f), 0.3f),
                        new Vector2(p.x - 0.42f, p.y).magnitude - 0.12f);

                case ShapeId.Note:
                    return Mathf.Min(
                        new Vector2(p.x + 0.28f, (p.y + 0.5f) * 1.4f).magnitude - 0.3f,
                        RoundedBox(new Vector2(p.x - 0.16f, p.y - 0.2f), new Vector2(0.1f, 0.78f),
                            0.06f));

                default:
                    return p.magnitude - 0.86f;
            }
        }

        // -------------------------------------------------------------------------------
        // The fields themselves. Each is the usual closed form for its shape.
        // -------------------------------------------------------------------------------

        private static float RoundedBox(Vector2 p, Vector2 half, float radius)
        {
            var r = Mathf.Min(radius, Mathf.Min(half.x, half.y));
            var d = new Vector2(Mathf.Abs(p.x) - (half.x - r), Mathf.Abs(p.y) - (half.y - r));
            return new Vector2(Mathf.Max(d.x, 0f), Mathf.Max(d.y, 0f)).magnitude +
                   Mathf.Min(Mathf.Max(d.x, d.y), 0f) - r;
        }

        private static float Capsule(Vector2 p, float radius, float halfHeight)
        {
            var y = Mathf.Abs(p.y) - halfHeight;
            return new Vector2(p.x, Mathf.Max(y, 0f)).magnitude - radius;
        }

        private static float Triangle(Vector2 p)
        {
            // Equilateral, point up.
            const float k = 1.7320508f;
            var x = Mathf.Abs(p.x) - 0.86f;
            var y = p.y + 0.86f / k;

            if (x + k * y > 0f)
            {
                var nx = (x - k * y) / 2f;
                var ny = (-k * x - y) / 2f;
                x = nx;
                y = ny;
            }

            x -= Mathf.Clamp(x, -1.72f, 0f);
            return -new Vector2(x, y).magnitude * Mathf.Sign(y);
        }

        private static float Star(Vector2 p, int points, float inner)
        {
            var angle = Mathf.Atan2(p.y, p.x);
            var segment = Mathf.PI * 2f / points;
            var local = Mathf.Repeat(angle + Mathf.PI * 0.5f, segment) / segment;

            // Triangle wave: 0 at a point, 1 between two of them.
            var t = local < 0.5f ? local * 2f : (1f - local) * 2f;
            var radius = Mathf.Lerp(0.86f, inner, t);
            return p.magnitude - radius;
        }

        /// <summary>
        /// Inigo Quilez's heart field, scaled and shifted so it sits in the middle.
        /// </summary>
        /// <remarks>
        /// The scale and shift were solved numerically rather than nudged by eye: the first
        /// attempt put the heart's centre of mass at 0.79 of the sprite's height, which a test
        /// caught, and a sprite whose contents are not centred cannot be positioned by its
        /// transform.
        /// </remarks>
        private static float Heart(Vector2 p)
        {
            const float scale = 1.35f;
            const float shift = 0.65f;

            var x = Mathf.Abs(p.x) / scale;
            var y = p.y / scale + shift;

            if (y + x > 1f)
            {
                return new Vector2(x - 0.25f, y - 0.75f).magnitude - 0.35355339f;
            }

            var lobe = new Vector2(x, y - 1f).sqrMagnitude;
            var m = 0.5f * Mathf.Max(x + y, 0f);
            var cleft = new Vector2(x - m, y - m).sqrMagnitude;

            return Mathf.Sqrt(Mathf.Min(lobe, cleft)) * Mathf.Sign(x - y);
        }

        private static float Teardrop(Vector2 p)
        {
            var circle = new Vector2(p.x, p.y + 0.24f).magnitude - 0.6f;
            var cone = Triangle(new Vector2(p.x * 1.6f, p.y * 1.05f - 0.05f));
            return Mathf.Min(circle, cone);
        }

        /// <summary>
        /// Inigo Quilez's trapezoid, bottom width first.
        /// </summary>
        /// <remarks>
        /// The first version had the sign of one edge vector flipped and drew a narrow spike
        /// instead of a skirt — which only showed up once the shapes were rendered and looked
        /// at, not in any assertion about coverage.
        /// </remarks>
        private static float Trapezoid(Vector2 p, float bottomHalf, float topHalf, float halfHeight)
        {
            var k1 = new Vector2(topHalf, halfHeight);
            var k2 = new Vector2(topHalf - bottomHalf, 2f * halfHeight);

            var q = new Vector2(Mathf.Abs(p.x), p.y);

            var ca = new Vector2(
                q.x - Mathf.Min(q.x, q.y < 0f ? bottomHalf : topHalf),
                Mathf.Abs(q.y) - halfHeight);

            var cb = q - k1 + k2 * Mathf.Clamp01(Vector2.Dot(k1 - q, k2) / Vector2.Dot(k2, k2));

            var s = cb.x < 0f && ca.y < 0f ? -1f : 1f;
            return s * Mathf.Sqrt(Mathf.Min(Vector2.Dot(ca, ca), Vector2.Dot(cb, cb)));
        }

        /// <summary>Two triangles pointing outwards, with a knot over the join.</summary>
        private static float Bow(Vector2 p)
        {
            var left = Wing(p, -0.40f, 90f);
            var right = Wing(p, 0.40f, -90f);
            var knot = p.magnitude - 0.24f;
            return Mathf.Min(knot, Mathf.Min(left, right));
        }

        private static float Wing(Vector2 p, float pivotX, float degrees)
        {
            var local = Rotate(new Vector2(p.x - pivotX, p.y), degrees);
            return Triangle(new Vector2(local.x * 1.5f, local.y * 1.9f));
        }

        private static Vector2 Rotate(Vector2 p, float degrees)
        {
            var radians = degrees * Mathf.Deg2Rad;
            var cos = Mathf.Cos(radians);
            var sin = Mathf.Sin(radians);
            return new Vector2(p.x * cos - p.y * sin, p.x * sin + p.y * cos);
        }

        private static float Steam(Vector2 p)
        {
            var a = new Vector2(p.x - 0.22f, p.y - 0.5f).magnitude - 0.34f;
            var b = new Vector2(p.x + 0.26f, p.y + 0.02f).magnitude - 0.38f;
            var c = new Vector2(p.x - 0.1f, p.y + 0.52f).magnitude - 0.3f;
            return Mathf.Min(a, Mathf.Min(b, c));
        }
    }
}
