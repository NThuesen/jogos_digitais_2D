using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Sim.Cafe;
using UnityEngine;

namespace Neko.Game.View
{
    /// <summary>
    /// The café around the floor: a papered wall with a wainscot, windows that throw daylight
    /// onto the boards, framed pictures, a clock, hanging lamps and pools of light. All of it is
    /// drawn from the same shapes as the rest of the game, behind everything that moves.
    /// </summary>
    public sealed partial class RoomView
    {
        private const float WallHeight = 4.2f;
        private const float Reach = 16f;

        private static readonly Color Paper = Palette.Hex("#F3DCD2");
        private static readonly Color PaperStripe = Palette.Hex("#EBCDC4");
        private static readonly Color Wainscot = Palette.Hex("#B5707A");
        private static readonly Color Moulding = Palette.Hex("#F8EBDD");
        private static readonly Color Frame = Palette.Hex("#8A5A3C");
        private static readonly Color OutsideRug = Palette.Hex("#7A5646");

        private SpriteRenderer Block(string name, float centreX, float centreY, float width, float height,
            Color colour, int layer, float depth, float degrees = 0f)
        {
            var go = Make(name, new Vector3(centreX, centreY, 0f), _sprites.Solid(colour), layer, depth);
            go.transform.localScale = new Vector3(width, height, 1f);
            go.transform.rotation = Quaternion.Euler(0f, 0f, degrees);
            return go.GetComponent<SpriteRenderer>();
        }

        private SpriteRenderer Piece(string name, Vector3 position, ShapeSpec spec, Vector2 size, int layer,
            float depth)
        {
            var go = Make(name, position, _sprites.Get(spec), layer, depth);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            return go.GetComponent<SpriteRenderer>();
        }

        private void BuildAmbience(CafeGrid grid)
        {
            var left = -0.5f;
            var middle = (grid.Width - 1) * 0.5f;
            var wallBottom = grid.Height - 0.5f;
            var wallMid = wallBottom + WallHeight * 0.5f;
            var span = Reach * 2f + grid.Width;

            // Beyond the room: a darker rug, so the playable floor reads as the lit part.
            Block("Outside", middle, (grid.Height - 1) * 0.5f, span, Reach * 2f, OutsideRug, Layers.Floor, 40f);

            // The wall, in two tones: papered above, panelled below.
            Block("Wall", middle, wallMid, span, WallHeight, Paper, Layers.Floor, 39f);
            for (var x = -(int)Reach; x < grid.Width + Reach; x += 1)
            {
                Block("Stripe", x + 0.5f, wallMid + 0.55f, 0.5f, WallHeight - 1.1f, PaperStripe,
                    Layers.Floor, 38.5f);
            }

            var panelTop = wallBottom + 0.6f;
            Block("Wainscot", middle, wallBottom + 0.3f, span, 0.6f, Wainscot, Layers.Floor, 38f);
            Block("Rail", middle, panelTop, span, 0.13f, Moulding, Layers.Floor, 37.8f);
            Block("Skirting", middle, wallBottom + 0.07f, span, 0.14f, Palette.Darken(Wainscot, 0.25f),
                Layers.Floor, 37.8f);
            Block("Shade", middle, wallBottom - 0.06f, grid.Width + 0.6f, 0.24f,
                Palette.WithAlpha(Palette.Ink, 0.22f), Layers.Rug, grid.Height + 0.5f);

            var panels = Mathf.CeilToInt(span / 1.9f);
            for (var i = 0; i < panels; i++)
            {
                Piece("Panel", new Vector3(-Reach + 0.6f + i * 1.9f, wallBottom + 0.32f, 0f),
                    new ShapeSpec(ShapeId.RoundedRect, Palette.Lighten(Wainscot, 0.08f),
                        Palette.Darken(Wainscot, 0.2f), 0.06f, 0.2f), new Vector2(1.5f, 0.4f),
                    Layers.Floor, 37.9f);
            }

            // Windows, evenly spaced over the room's width.
            var windows = Mathf.Max(2, Mathf.RoundToInt(grid.Width / 3.2f));
            var spacing = grid.Width / windows;
            for (var i = 0; i < windows; i++)
            {
                var x = left + spacing * (i + 0.5f);
                BuildWindow(x, wallBottom + 1.32f);
                BuildPool(grid, x, i);
            }

            BuildWallDecor(grid, wallBottom);
            BuildLamps(grid, wallBottom);
        }

        private void BuildWindow(float x, float y)
        {
            Piece("WindowFrame", new Vector3(x, y, 0f),
                new ShapeSpec(ShapeId.RoundedRect, Frame, Palette.Darken(Frame, 0.3f), 0.05f, 0.18f),
                new Vector2(1.2f, 1.3f), Layers.Floor, 37.5f);

            Piece("Glass", new Vector3(x, y, 0f),
                new ShapeSpec(ShapeId.RoundedRect, Palette.Hex("#BFE3F2"), null, 0f, 0.14f,
                    gradientTo: Palette.Hex("#F6EBD3")),
                new Vector2(1.0f, 1.1f), Layers.Floor, 37.4f);

            Block("BarV", x, y, 0.06f, 1.1f, Frame, Layers.Floor, 37.3f);
            Block("BarH", x, y, 1.0f, 0.06f, Frame, Layers.Floor, 37.3f);
            Block("Sill", x, y - 0.7f, 1.35f, 0.1f, Moulding, Layers.Floor, 37.3f);

            for (var side = -1; side <= 1; side += 2)
            {
                Piece("Curtain", new Vector3(x + side * 0.66f, y + 0.02f, 0f),
                    new ShapeSpec(ShapeId.Teardrop, Palette.Hex("#E9A0AE"), Palette.Hex("#B5707A"), 0.04f,
                        0.5f, 180f), new Vector2(0.4f, 1.25f), Layers.Floor, 37.2f);
            }
        }

        // Daylight on the boards: a soft pool under each window, laid under the furniture.
        private void BuildPool(CafeGrid grid, float x, int index)
        {
            var pool = Piece("Pool", new Vector3(x - 0.3f, grid.Height - 1.4f, 0f),
                new ShapeSpec(ShapeId.Circle, Palette.WithAlpha(new Color(1f, 0.96f, 0.82f), 0.16f)),
                new Vector2(2.8f, 1.9f), Layers.Rug, grid.Height + 0.2f);
            pool.sortingOrder -= index;
        }

        private void BuildWallDecor(CafeGrid grid, float wallBottom)
        {
            var y = wallBottom + 1.32f;
            var clockX = (grid.Width - 1) * 0.5f;

            Piece("ClockRim", new Vector3(clockX, y + 0.2f, 0f),
                new ShapeSpec(ShapeId.Circle, Moulding, Frame, 0.09f), new Vector2(0.7f, 0.7f),
                Layers.Floor, 37.2f);
            Block("HandA", clockX, y + 0.3f, 0.05f, 0.24f, Palette.Ink, Layers.Floor, 37.1f);
            Block("HandB", clockX + 0.06f, y + 0.2f, 0.18f, 0.05f, Palette.Ink, Layers.Floor, 37.1f);

            var specs = new[]
            {
                new ShapeSpec(ShapeId.Heart, Palette.InnerEar),
                new ShapeSpec(ShapeId.CatEar, Palette.Hex("#D68946")),
                new ShapeSpec(ShapeId.Star, Palette.Focus),
            };

            var picture = 0;
            for (var x = -0.4f; x < grid.Width - 0.5f; x += 3.1f)
            {
                var px = x + 1.6f;
                if (px > grid.Width - 0.6f || Mathf.Abs(px - clockX) < 0.9f) continue;

                Piece("PicFrame", new Vector3(px, y - 0.1f, 0f),
                    new ShapeSpec(ShapeId.RoundedRect, Moulding, Frame, 0.07f, 0.1f),
                    new Vector2(0.62f, 0.8f), Layers.Floor, 37.2f);
                Piece("Pic", new Vector3(px, y - 0.1f, 0f), specs[picture++ % specs.Length],
                    new Vector2(0.4f, 0.4f), Layers.Floor, 37.1f);
            }
        }

        private void BuildLamps(CafeGrid grid, float wallBottom)
        {
            var lamps = Mathf.Max(2, Mathf.RoundToInt(grid.Width / 3.2f));
            var spacing = grid.Width / lamps;
            for (var i = 1; i < lamps; i++)
            {
                var x = -0.5f + spacing * i;
                var top = wallBottom + 2.05f;

                Block("Cord", x, top - 0.2f, 0.03f, 0.4f, Palette.Ink, Layers.Floor, 36.9f);
                Piece("LampShade", new Vector3(x, top - 0.55f, 0f),
                    new ShapeSpec(ShapeId.Teardrop, Palette.Hex("#F2C14E"),
                        Palette.Darken(Palette.Focus, 0.3f), 0.05f), new Vector2(0.46f, 0.46f),
                    Layers.Floor, 36.8f);
                Piece("LampGlow", new Vector3(x, top - 0.65f, 0f),
                    new ShapeSpec(ShapeId.Circle, Palette.WithAlpha(Palette.Focus, 0.2f)),
                    new Vector2(2.6f, 2.6f), Layers.Floor, 36.7f);
            }
        }
    }
}
