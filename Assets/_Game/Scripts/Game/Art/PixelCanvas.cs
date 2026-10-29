using UnityEngine;

namespace Neko.Game.Art
{
    /// <summary>
    /// A small drawing surface for pixel art painted in code.
    /// </summary>
    /// <remarks>
    /// Authored top-left first, y running down, which is how pixel art is drawn and read;
    /// <see cref="Export"/> flips it into Unity's bottom-up order. Everything here works on
    /// whole pixels on purpose: the signed-distance shapes are smooth at any size, and this is
    /// the opposite tool, for pieces that should look hand-placed at one fixed resolution.
    /// Pure arithmetic on a buffer, so a headless test can render and inspect it.
    /// </remarks>
    public sealed class PixelCanvas
    {
        /// <summary>Alpha at or above which a pixel counts as part of the object.</summary>
        public const byte SolidAlpha = 200;

        private readonly Color32[] _pixels;

        public PixelCanvas(int width, int height)
        {
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
            _pixels = new Color32[Width * Height];
        }

        public int Width { get; }

        public int Height { get; }

        public bool Inside(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

        public Color32 Get(int x, int y) => Inside(x, y) ? _pixels[y * Width + x] : default;

        public bool IsSolid(int x, int y) => Inside(x, y) && _pixels[y * Width + x].a >= SolidAlpha;

        /// <summary>Paints one pixel, compositing anything translucent over what is there.</summary>
        public void Set(int x, int y, Color colour)
        {
            if (!Inside(x, y) || colour.a <= 0f) return;

            var index = y * Width + x;
            if (colour.a >= 0.999f)
            {
                _pixels[index] = colour;
                return;
            }

            var dst = (Color)_pixels[index];
            var outAlpha = colour.a + dst.a * (1f - colour.a);
            if (outAlpha <= 0f) return;

            var r = (colour.r * colour.a + dst.r * dst.a * (1f - colour.a)) / outAlpha;
            var g = (colour.g * colour.a + dst.g * dst.a * (1f - colour.a)) / outAlpha;
            var b = (colour.b * colour.a + dst.b * dst.a * (1f - colour.a)) / outAlpha;
            _pixels[index] = new Color(r, g, b, outAlpha);
        }

        /// <summary>Paints only where something solid already is. For texture on a shape.</summary>
        public void SetOnSolid(int x, int y, Color colour)
        {
            if (IsSolid(x, y)) Set(x, y, colour);
        }

        public void Clear(int x, int y)
        {
            if (Inside(x, y)) _pixels[y * Width + x] = default;
        }

        public void Rect(int x, int y, int width, int height, Color colour)
        {
            for (var j = y; j < y + height; j++)
            {
                for (var i = x; i < x + width; i++) Set(i, j, colour);
            }
        }

        public void HLine(int x0, int x1, int y, Color colour)
        {
            if (x1 < x0) (x0, x1) = (x1, x0);
            for (var x = x0; x <= x1; x++) Set(x, y, colour);
        }

        public void VLine(int x, int y0, int y1, Color colour)
        {
            if (y1 < y0) (y0, y1) = (y1, y0);
            for (var y = y0; y <= y1; y++) Set(x, y, colour);
        }

        /// <summary>Bresenham. One pixel wide, no gaps, no doubled corners.</summary>
        public void Line(int x0, int y0, int x1, int y1, Color colour)
        {
            var dx = Mathf.Abs(x1 - x0);
            var dy = -Mathf.Abs(y1 - y0);
            var sx = x0 < x1 ? 1 : -1;
            var sy = y0 < y1 ? 1 : -1;
            var error = dx + dy;

            while (true)
            {
                Set(x0, y0, colour);
                if (x0 == x1 && y0 == y1) return;

                var twice = 2 * error;
                if (twice >= dy)
                {
                    error += dy;
                    x0 += sx;
                }

                if (twice <= dx)
                {
                    error += dx;
                    y0 += sy;
                }
            }
        }

        /// <summary>
        /// A filled ellipse, centred between pixels when the centre is a half value.
        /// </summary>
        /// <remarks>
        /// Tested at each pixel's centre, so an ellipse of radius 3.5 centred on 4.0 is seven
        /// pixels across and symmetric — the usual off-by-one that makes code-drawn pixel
        /// circles lopsided.
        /// </remarks>
        public void Ellipse(float cx, float cy, float rx, float ry, Color colour)
        {
            if (rx <= 0f || ry <= 0f) return;

            var x0 = Mathf.FloorToInt(cx - rx);
            var x1 = Mathf.CeilToInt(cx + rx);
            var y0 = Mathf.FloorToInt(cy - ry);
            var y1 = Mathf.CeilToInt(cy + ry);

            for (var y = y0; y <= y1; y++)
            {
                for (var x = x0; x <= x1; x++)
                {
                    var nx = (x + 0.5f - cx) / rx;
                    var ny = (y + 0.5f - cy) / ry;
                    if (nx * nx + ny * ny <= 1f) Set(x, y, colour);
                }
            }
        }

        /// <summary>A rectangle with its four corner pixels knocked off.</summary>
        public void RoundRect(int x, int y, int width, int height, Color colour, int corner = 1)
        {
            for (var j = 0; j < height; j++)
            {
                for (var i = 0; i < width; i++)
                {
                    var dx = Mathf.Min(i, width - 1 - i);
                    var dy = Mathf.Min(j, height - 1 - j);
                    if (dx + dy < corner) continue;
                    Set(x + i, y + j, colour);
                }
            }
        }

        /// <summary>
        /// Draws a one-pixel outline around everything solid, in a dark shade of whatever it
        /// is outlining.
        /// </summary>
        /// <remarks>
        /// Coloured rather than black: a plant gets a deep green edge and a cushion a deep
        /// rose one, which is what keeps code-drawn pixel art from looking like a colouring
        /// book. Translucent pixels — shadows, glows — count as background, so they are drawn
        /// over rather than outlined.
        /// </remarks>
        public void Outline(Color ink, float inkWeight = 0.55f)
        {
            var edge = new Color32[_pixels.Length];
            var mark = new bool[_pixels.Length];

            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    if (IsSolid(x, y)) continue;

                    var found = false;
                    var neighbour = default(Color32);
                    if (IsSolid(x, y - 1)) { neighbour = Get(x, y - 1); found = true; }
                    else if (IsSolid(x, y + 1)) { neighbour = Get(x, y + 1); found = true; }
                    else if (IsSolid(x - 1, y)) { neighbour = Get(x - 1, y); found = true; }
                    else if (IsSolid(x + 1, y)) { neighbour = Get(x + 1, y); found = true; }

                    if (!found) continue;

                    var dark = Tones.Shift(neighbour, -3);
                    edge[y * Width + x] = Color.Lerp(dark, ink, inkWeight);
                    mark[y * Width + x] = true;
                }
            }

            for (var i = 0; i < _pixels.Length; i++)
            {
                if (mark[i]) _pixels[i] = edge[i];
            }
        }

        /// <summary>
        /// Unity's order — bottom row first — at an integer scale, nearest-neighbour.
        /// </summary>
        public Color32[] Export(int scale)
        {
            scale = Mathf.Max(1, scale);
            var width = Width * scale;
            var height = Height * scale;
            var output = new Color32[width * height];

            for (var y = 0; y < height; y++)
            {
                var source = Height - 1 - y / scale;
                for (var x = 0; x < width; x++)
                {
                    output[y * width + x] = _pixels[source * Width + x / scale];
                }
            }

            return output;
        }
    }

    /// <summary>
    /// Shading ramps with a hue shift, so shadows go cool and light goes warm.
    /// </summary>
    /// <remarks>
    /// Darkening by multiplying the value alone is what makes a code-drawn object look dead:
    /// every shade is the same hue, just dimmer. Moving the hue a little towards violet as it
    /// darkens and towards gold as it lightens is the standard pixel-art ramp, and it is what
    /// the accepted direction asks for — brown-violet shadows, never black.
    /// </remarks>
    public static class Tones
    {
        private const float ShadowHue = 0.74f;
        private const float LightHue = 0.13f;

        public static Color Shift(Color colour, int step)
        {
            if (step == 0) return colour;

            Color.RGBToHSV(colour, out var h, out var s, out var v);
            var n = Mathf.Abs(step);

            // A grey has no hue to speak of; pushing it round the wheel tints it instead.
            var hueWeight = Mathf.Clamp01(s * 2.5f);

            if (step < 0)
            {
                h = TowardHue(h, ShadowHue, 0.028f * n * hueWeight);
                s = Mathf.Clamp01(s + 0.05f * n + (s < 0.15f ? 0.03f * n : 0f));
                v = Mathf.Clamp01(v * Mathf.Pow(0.8f, n));
            }
            else
            {
                h = TowardHue(h, LightHue, 0.022f * n * hueWeight);
                s = Mathf.Clamp01(s * Mathf.Pow(0.8f, n));
                v = Mathf.Clamp01(v + (1f - v) * (1f - Mathf.Pow(0.62f, n)) + 0.03f * n);
            }

            var shifted = Color.HSVToRGB(h, s, v);
            shifted.a = colour.a;
            return shifted;
        }

        public static Color Mix(Color a, Color b, float t) => Color.Lerp(a, b, Mathf.Clamp01(t));

        public static Color Alpha(Color colour, float alpha) =>
            new Color(colour.r, colour.g, colour.b, alpha);

        private static float TowardHue(float hue, float target, float amount)
        {
            var delta = Mathf.Repeat(target - hue + 0.5f, 1f) - 0.5f;
            var step = Mathf.Clamp(delta, -amount, amount);
            return Mathf.Repeat(hue + step, 1f);
        }
    }
}
