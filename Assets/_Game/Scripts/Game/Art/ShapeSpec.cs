using System.Globalization;
using UnityEngine;

namespace Neko.Game.Art
{
    /// <summary>
    /// The shape vocabulary the whole game is drawn from.
    /// </summary>
    /// <remarks>
    /// Nothing is imported — no texture file exists in this project — so this list is the art.
    /// Keeping it short is deliberate: a figure sixty pixels tall is a silhouette and nothing
    /// else, and a silhouette made of six recognisable shapes reads better than one made of
    /// thirty vague ones.
    /// </remarks>
    public enum ShapeId
    {
        Circle = 0,
        Capsule,
        RoundedRect,
        Triangle,
        Star,
        Ring,
        Heart,
        Teardrop,
        Crescent,
        CatEar,
        Bow,
        Apron,
        Plate,
        Steam,
        Door,
        Sparkle,
        Note,
    }

    /// <summary>One shape, with everything needed to draw it.</summary>
    public readonly struct ShapeSpec
    {
        public readonly ShapeId Shape;
        public readonly Color Fill;
        public readonly Color? GradientTo;
        public readonly Color? Outline;
        public readonly float OutlineWidth;

        /// <summary>Ring thickness, corner radius, star sharpness — depends on the shape.</summary>
        public readonly float Detail;

        public readonly float RotationDegrees;

        /// <summary>Offset within the sprite, in units of half the sprite. 0 is centred.</summary>
        public readonly Vector2 Offset;

        /// <summary>Scale within the sprite. 1 fills it.</summary>
        public readonly Vector2 Scale;

        public ShapeSpec(ShapeId shape, Color fill, Color? outline = null, float outlineWidth = 0f,
            float detail = 0.5f, float rotationDegrees = 0f, Vector2 offset = default,
            Vector2 scale = default, Color? gradientTo = null)
        {
            Shape = shape;
            Fill = fill;
            Outline = outline;
            OutlineWidth = outlineWidth;
            Detail = detail;
            RotationDegrees = rotationDegrees;
            Offset = offset;
            Scale = scale == default ? Vector2.one : scale;
            GradientTo = gradientTo;
        }

        public ShapeSpec With(Color fill) =>
            new ShapeSpec(Shape, fill, Outline, OutlineWidth, Detail, RotationDegrees, Offset,
                Scale, GradientTo);

        public ShapeSpec Outlined(Color outline, float width = 0.055f) =>
            new ShapeSpec(Shape, Fill, outline, width, Detail, RotationDegrees, Offset, Scale,
                GradientTo);

        public ShapeSpec At(Vector2 offset, Vector2 scale) =>
            new ShapeSpec(Shape, Fill, Outline, OutlineWidth, Detail, RotationDegrees, offset,
                scale, GradientTo);

        public ShapeSpec Rotated(float degrees) =>
            new ShapeSpec(Shape, Fill, Outline, OutlineWidth, Detail, degrees, Offset, Scale,
                GradientTo);

        /// <summary>
        /// Everything that changes the pixels, as text. The sprite cache is keyed on this.
        /// </summary>
        /// <remarks>
        /// Invariant culture throughout: on a machine whose decimal separator is a comma, a
        /// key built with the local one would collide across different scales and hand back
        /// the wrong sprite.
        /// </remarks>
        public string CacheKey()
        {
            const string f = "0.###";
            var c = CultureInfo.InvariantCulture;

            return (int)Shape + "|" + Rgba(Fill) + "|" +
                   (GradientTo.HasValue ? Rgba(GradientTo.Value) : "-") + "|" +
                   (Outline.HasValue ? Rgba(Outline.Value) : "-") + "|" +
                   OutlineWidth.ToString(f, c) + "|" + Detail.ToString(f, c) + "|" +
                   RotationDegrees.ToString(f, c) + "|" +
                   Offset.x.ToString(f, c) + "," + Offset.y.ToString(f, c) + "|" +
                   Scale.x.ToString(f, c) + "," + Scale.y.ToString(f, c);
        }

        private static string Rgba(Color colour) =>
            ((int)(colour.r * 255) << 24 | (int)(colour.g * 255) << 16 |
             (int)(colour.b * 255) << 8 | (int)(colour.a * 255))
            .ToString("x8", CultureInfo.InvariantCulture);
    }
}
