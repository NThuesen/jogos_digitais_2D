using Neko.Sim.Cafe;
using UnityEngine;

namespace Neko.Game.Art
{
    /// <summary>One painted piece of furniture, ready to become a texture.</summary>
    public readonly struct FurniturePicture
    {
        /// <summary>Bottom row first, the way <c>Texture2D.SetPixels32</c> wants it.</summary>
        public readonly Color32[] Pixels;

        public readonly int Width;
        public readonly int Height;

        /// <summary>The centre of the footprint, normalised. Anything taller rises above it.</summary>
        public readonly Vector2 Pivot;

        public FurniturePicture(Color32[] pixels, int width, int height, Vector2 pivot)
        {
            Pixels = pixels;
            Width = width;
            Height = height;
            Pivot = pivot;
        }
    }

    /// <summary>
    /// Every placeable in the café, painted as pixel art in code.
    /// </summary>
    /// <remarks>
    /// Seen from three-quarters above, like the characters: a lit top, a front face, and
    /// anything tall rising past the top of its own footprint, so a fern overlaps the tile
    /// behind it the way a fern would. The footprint is always the bottom of the picture and
    /// the pivot is its centre, which is what lets the room view place a piece by its cells
    /// and sort it by its front row without knowing how tall it is.
    ///
    /// Drawn at <see cref="TilePixels"/> to a tile and blown up by a whole number, never by a
    /// fraction: pixel art scaled by 2.6 has pixels of two different widths, and that is the
    /// first thing an eye catches. The tint from the catalog is the base of each piece's ramp,
    /// so the data table still decides what colour a thing is.
    ///
    /// Nothing is imported. Like the shapes, this is a pure function of its inputs, so every
    /// piece can be rendered and checked in a headless test.
    /// </remarks>
    public static class FurnitureArt
    {
        /// <summary>Logical pixels to a tile. Thirty-two leaves room for a teacup.</summary>
        public const int TilePixels = 32;

        /// <summary>Texels per logical pixel. 32 × 3 = 96, the project's pixels per unit.</summary>
        public const int Scale = 3;

        private static readonly Color Ink = Palette.Ink;
        private static readonly Color Paper = Palette.Panel;
        private static readonly Color Cream = Palette.Cream;

        /// <summary>Muted on purpose: the saturated gold is the focus colour and decor never gets it.</summary>
        private static readonly Color Brass = Palette.Hex("#C9A563");

        private static readonly Color DarkWood = Palette.Hex("#8C5A3C");
        private static readonly Color Terracotta = Palette.Hex("#C27350");
        private static readonly Color Leaf = Palette.Hex("#6FA864");
        private static readonly Color Cushion = Palette.Hex("#C97B86");
        private static readonly Color Teal = Palette.Hex("#5E9C99");
        private static readonly Color Iron = Palette.Hex("#5C5862");
        private static readonly Color Flame = Palette.Hex("#FFE8A8");
        private static readonly Color Ember = Palette.Hex("#EE8A4E");
        private static readonly Color Glass = Palette.Hex("#CFE6EE");
        private static readonly Color Copper = Palette.Hex("#C27A4E");
        private static readonly Color Berry = Palette.Hex("#D2525A");

        public static FurniturePicture Paint(PlaceableDef def, int widthTiles, int heightTiles) =>
            def == null
                ? Paint("", Palette.Wood, widthTiles, heightTiles)
                : Paint(def.Id, Palette.Hex(def.TintHex), widthTiles, heightTiles);

        public static FurniturePicture Paint(string id, Color tint, int widthTiles, int heightTiles)
        {
            var canvas = PaintCanvas(id, tint, widthTiles, heightTiles);
            var footprint = Mathf.Max(1, heightTiles) * TilePixels;
            var pivot = new Vector2(0.5f, footprint * 0.5f / canvas.Height);

            return new FurniturePicture(canvas.Export(Scale), canvas.Width * Scale,
                canvas.Height * Scale, pivot);
        }

        /// <summary>The logical-resolution canvas, before scaling. What the tests look at.</summary>
        public static PixelCanvas PaintCanvas(string id, Color tint, int widthTiles, int heightTiles)
        {
            var w = Mathf.Max(1, widthTiles);
            var h = Mathf.Max(1, heightTiles);
            var top = HeadroomFor(id);
            var canvas = new PixelCanvas(w * TilePixels, h * TilePixels + top);

            Draw(canvas, id ?? "", tint, w, h, top);
            return canvas;
        }

        /// <summary>Flat things lie under everything, including the furniture.</summary>
        public static bool LiesFlat(string id) => id == "rug" || id == "koi_pond";

        /// <summary>How far above its footprint a piece is allowed to rise, in logical pixels.</summary>
        public static int HeadroomFor(string id)
        {
            switch (id)
            {
                case "table_2":
                case "table_4": return 8;
                case "stool": return 4;
                case "stove": return 14;
                case "teabar": return 14;
                case "dessert": return 18;
                case "bakery": return 16;
                case "plant": return 16;
                case "window": return 20;
                case "rug": return 0;
                case "cat_tower": return 22;
                case "stage": return 24;
                case "chandelier": return 26;
                case "koi_pond": return 2;
                case "lantern_sakura": return 22;
                case "sakura_tree": return 28;
                case "gothic_candelabra": return 22;
                case "gothic_mirror": return 24;
                case "star_lamp": return 24;
                case "planetarium": return 18;
                case "showa_poster": return 20;
                case "showa_jukebox": return 18;
                case "door": return 24;
                default: return 8;
            }
        }

        private static void Draw(PixelCanvas c, string id, Color tint, int w, int h, int f)
        {
            switch (id)
            {
                case "table_2": Table2(c, tint, w, h, f); break;
                case "stool": Stool(c, tint, f); break;
                case "table_4": Table4(c, tint, f); break;
                case "stove": Stove(c, tint, f); break;
                case "teabar": TeaBar(c, tint, w, h, f); break;
                case "dessert": Dessert(c, tint, f); break;
                case "bakery": Bakery(c, tint, w, h, f); break;
                case "plant": Fern(c, tint, f); break;
                case "window": Window(c, tint, f); break;
                case "rug": Rug(c, tint, f); break;
                case "cat_tower": CatTower(c, tint, w, h, f); break;
                case "stage": Stage(c, tint, f); break;
                case "chandelier": Chandelier(c, tint, f); break;
                case "koi_pond": KoiPond(c, tint, f); break;
                case "lantern_sakura": Lantern(c, tint, f); break;
                case "sakura_tree": SakuraTree(c, tint, f); break;
                case "gothic_candelabra": Candelabra(c, tint, f); break;
                case "gothic_mirror": Mirror(c, tint, f); break;
                case "star_lamp": StarLamp(c, tint, f); break;
                case "planetarium": Planetarium(c, tint, f); break;
                case "showa_poster": Poster(c, tint, f); break;
                case "showa_jukebox": Jukebox(c, tint, f); break;
                case "door": Door(c, tint, f); break;
                default: Crate(c, tint, f); break;
            }
        }

        // -------------------------------------------------------------------------------
        // Tables and seats
        // -------------------------------------------------------------------------------

        /// <summary>
        /// A table for two, with the chairs behind it.
        /// </summary>
        /// <remarks>
        /// Customers are drawn standing on their seat's cell, feet at its centre, so the chair
        /// sits in the back half of each cell and the table top in the front half: a seated
        /// party then reads as sitting behind the table instead of on top of it.
        /// </remarks>
        private static void Table2(PixelCanvas c, Color tint, int w, int h, int f)
        {
            if (h > w)
            {
                TableLong(c, tint, f, h);
                return;
            }

            var width = c.Width;
            Shadow(c, width * 0.5f, f + 28, width * 0.42f, 3.5f);

            for (var seat = 0; seat < w; seat++)
            {
                ChairFacing(c, seat * 32 + 16, f + 13, Tones.Shift(tint, -1), Cushion);
            }

            TableTop(c, 3, f + 14, width - 6, 10, 3, tint, f + 30);

            for (var seat = 0; seat < w; seat++) TeaSetting(c, seat * 32 + 16, f + 18);
            Vase(c, width / 2, f + 18);

            c.Outline(Ink);
        }

        /// <summary>The same table turned on end: chairs down the left, table down the right.</summary>
        private static void TableLong(PixelCanvas c, Color tint, int f, int h)
        {
            var length = h * 32;
            Shadow(c, 20f, f + length - 5, 11f, 3f);

            for (var seat = 0; seat < h; seat++)
            {
                var cy = f + seat * 32 + 16;
                ChairSide(c, 4, cy, Tones.Shift(tint, -1), Cushion);
            }

            var top = f + 4;
            var depth = length - 14;
            c.Rect(14, top, 15, depth, Tones.Shift(tint, 1));
            Grain(c, 14, top, 15, depth, tint, 3);
            c.VLine(14, top, top + depth - 1, Tones.Shift(tint, 2));
            c.Rect(14, top + depth, 15, 3, tint);
            c.HLine(14, 28, top + depth + 2, Tones.Shift(tint, -1));
            Leg(c, 15, top + depth + 3, top + depth + 7, tint);
            Leg(c, 26, top + depth + 3, top + depth + 7, tint);

            for (var seat = 0; seat < h; seat++) TeaSetting(c, 21, f + seat * 32 + 14);
            Vase(c, 22, f + length / 2 + 1);

            c.Outline(Ink);
        }

        private static void Table4(PixelCanvas c, Color tint, int f)
        {
            var width = c.Width;
            var cx = width / 2;
            Shadow(c, cx, f + 47, width * 0.43f, 4f);

            var chair = Tones.Shift(tint, -1);
            ChairFacing(c, 16, f + 13, chair, Cushion);
            ChairFacing(c, width - 16, f + 13, chair, Cushion);

            TableTop(c, 4, f + 14, width - 8, 24, 3, tint, f + 46);

            // A lace runner with the tea things on it.
            c.Rect(4, f + 21, width - 8, 9, Paper);
            for (var x = 4; x < width - 4; x += 2)
            {
                c.Set(x, f + 21, Cream);
                c.Set(x + 1, f + 29, Cream);
            }

            c.HLine(4, width - 5, f + 30, Tones.Shift(Paper, -1));
            Teapot(c, cx, f + 26, Teal);
            TeaSetting(c, 14, f + 25);
            TeaSetting(c, width - 14, f + 25);
            Vase(c, cx + 13, f + 20);

            ChairAway(c, 16, f + 41, chair, Cushion);
            ChairAway(c, width - 16, f + 41, chair, Cushion);

            c.Outline(Ink);
        }

        private static void Stool(PixelCanvas c, Color tint, int f)
        {
            Shadow(c, 16f, f + 28, 9f, 2.5f);

            // Back legs first, thinner and in shadow; the footrest ring between the pairs.
            var back = Tones.Shift(tint, -2);
            c.Line(13, f + 16, 12, f + 25, back);
            c.Line(19, f + 16, 20, f + 25, back);
            c.HLine(12, 20, f + 20, back);

            c.Line(10, f + 16, 8, f + 28, tint);
            c.Line(11, f + 16, 9, f + 28, Tones.Shift(tint, -1));
            c.Line(21, f + 16, 23, f + 28, Tones.Shift(tint, -1));
            c.Line(22, f + 16, 24, f + 28, Tones.Shift(tint, -1));
            c.HLine(10, 22, f + 23, tint);
            c.HLine(11, 21, f + 24, Tones.Shift(tint, -1));

            // Seat: a wooden rim and a buttoned cushion on it.
            c.Ellipse(16f, f + 15f, 9f, 3f, Tones.Shift(tint, -1));
            c.Ellipse(16f, f + 14f, 9f, 2.8f, tint);
            c.Ellipse(16f, f + 12.5f, 8f, 3f, Tones.Shift(Teal, -1));
            c.Ellipse(16f, f + 12f, 7.5f, 2.6f, Teal);
            c.Ellipse(14.5f, f + 11.5f, 4f, 1.2f, Tones.Shift(Teal, 1));
            c.HLine(12, 14, f + 10, Tones.Shift(Teal, 3));
            c.Set(16, f + 12, Tones.Shift(Teal, -2));

            c.Outline(Ink);
        }

        private static void TableTop(PixelCanvas c, int x, int y, int width, int depth, int apron,
            Color tint, int floor)
        {
            // The line where the top meets the chairs behind it.
            c.HLine(x + 1, x + width - 2, y - 1, Tones.Mix(Tones.Shift(tint, -3), Ink, 0.4f));

            c.RoundRect(x, y, width, depth, Tones.Shift(tint, 1), 1);
            Grain(c, x + 1, y + 1, width - 2, depth - 2, tint, 7);
            c.HLine(x + 1, x + width - 2, y + depth - 1, Tones.Shift(tint, 2));
            c.Rect(x, y + depth, width, apron, tint);
            c.HLine(x, x + width - 1, y + depth + apron - 1, Tones.Shift(tint, -1));
            c.VLine(x + width - 1, y + depth, y + depth + apron - 1, Tones.Shift(tint, -1));

            var legTop = y + depth + apron;
            Leg(c, x + 2, legTop, floor, tint);
            Leg(c, x + width - 4, legTop, floor, tint);
        }

        private static void Leg(PixelCanvas c, int x, int y0, int y1, Color tint)
        {
            c.VLine(x, y0, y1, Tones.Shift(tint, -1));
            c.VLine(x + 1, y0, y1, Tones.Shift(tint, -2));
        }

        /// <summary>
        /// A chair behind a table, facing the room: a wooden frame round an upholstered back.
        /// </summary>
        /// <remarks>
        /// Upholstered rather than slatted. The gaps of a slatted back are one or two pixels
        /// wide, and the outline pass fills every one of them, so slats read as a dark panel
        /// with stripes; a padded back also puts the café's pink above the table top, where
        /// it is seen.
        /// </remarks>
        private static void ChairFacing(PixelCanvas c, int cx, int seatY, Color wood, Color cushion)
        {
            var left = cx - 8;
            var top = seatY - 17;

            c.RoundRect(left, top, 16, 17, wood, 3);
            c.HLine(left + 3, left + 12, top, Tones.Shift(wood, 1));
            c.VLine(left + 15, top + 3, seatY - 1, Tones.Shift(wood, -1));

            c.RoundRect(left + 3, top + 3, 10, 12, cushion, 2);
            c.HLine(left + 5, left + 10, top + 3, Tones.Shift(cushion, 2));
            c.VLine(left + 3, top + 5, top + 12, Tones.Shift(cushion, 1));
            c.HLine(left + 5, left + 10, top + 14, Tones.Shift(cushion, -1));
            c.Set(cx, top + 8, Tones.Shift(cushion, -2));

            c.RoundRect(cx - 8, seatY - 2, 16, 5, Tones.Shift(cushion, -1), 1);
            c.HLine(cx - 6, cx + 5, seatY - 2, Tones.Shift(cushion, 1));
        }

        /// <summary>A chair in front of a table, seen from behind.</summary>
        private static void ChairAway(PixelCanvas c, int cx, int seatY, Color wood, Color cushion)
        {
            c.RoundRect(cx - 7, seatY, 14, 4, cushion, 1);
            c.HLine(cx - 5, cx + 4, seatY, Tones.Shift(cushion, 2));

            var top = seatY + 3;
            c.RoundRect(cx - 8, top, 16, 4, wood, 2);
            c.HLine(cx - 6, cx + 5, top, Tones.Shift(wood, 1));
            c.Rect(cx - 7, top + 3, 2, 9, wood);
            c.Rect(cx + 5, top + 3, 2, 9, Tones.Shift(wood, -1));
            c.Rect(cx - 4, top + 5, 8, 2, Tones.Shift(wood, -1));
            Leg(c, cx - 7, top + 12, top + 14, wood);
            Leg(c, cx + 5, top + 12, top + 14, wood);
        }

        /// <summary>A chair seen side-on, for a table turned lengthways.</summary>
        private static void ChairSide(PixelCanvas c, int x, int cy, Color wood, Color cushion)
        {
            c.Rect(x, cy - 13, 2, 15, wood);
            c.VLine(x, cy - 13, cy, Tones.Shift(wood, 1));
            c.RoundRect(x, cy - 1, 10, 4, cushion, 1);
            c.HLine(x + 1, x + 8, cy - 1, Tones.Shift(cushion, 2));
            Leg(c, x, cy + 3, cy + 8, wood);
            Leg(c, x + 8, cy + 3, cy + 8, wood);
        }

        private static void TeaSetting(PixelCanvas c, int cx, int cy)
        {
            c.Ellipse(cx, cy + 1.5f, 4f, 1.5f, Tones.Shift(Paper, -1));
            c.HLine(cx - 3, cx + 2, cy + 1, Paper);
            c.Rect(cx - 2, cy - 2, 4, 3, Paper);
            c.HLine(cx - 1, cx, cy - 2, Palette.Hex("#B37A4E"));
            c.Set(cx + 2, cy - 1, Tones.Shift(Paper, -1));
            c.VLine(cx + 1, cy - 1, cy, Tones.Shift(Paper, -1));
        }

        private static void Vase(PixelCanvas c, int cx, int bottom)
        {
            c.Rect(cx - 1, bottom - 5, 3, 5, Teal);
            c.VLine(cx - 1, bottom - 5, bottom - 1, Tones.Shift(Teal, 2));
            c.VLine(cx, bottom - 9, bottom - 6, Leaf);
            c.Set(cx + 1, bottom - 8, Leaf);
            Blossom(c, cx, bottom - 10, Palette.Hex("#F2A7BC"));
        }

        private static void Blossom(PixelCanvas c, int cx, int cy, Color petal)
        {
            c.Set(cx, cy - 1, petal);
            c.Set(cx - 1, cy, petal);
            c.Set(cx + 1, cy, petal);
            c.Set(cx, cy + 1, Tones.Shift(petal, -1));
            c.Set(cx, cy, Palette.Hex("#F7E3A0"));
        }

        private static void Teapot(PixelCanvas c, int cx, int cy, Color body)
        {
            c.Ellipse(cx, cy, 4.5f, 3.5f, Paper);
            c.HLine(cx - 4, cx + 3, cy, body);
            c.HLine(cx - 3, cx + 2, cy + 1, Tones.Shift(body, -1));
            c.Rect(cx - 2, cy - 5, 4, 2, Tones.Shift(Paper, -1));
            c.Set(cx - 1, cy - 6, body);
            c.Set(cx, cy - 6, body);
            c.Line(cx + 4, cy - 1, cx + 6, cy - 3, Paper);
            c.VLine(cx - 5, cy - 2, cy, Tones.Shift(Paper, -1));
            c.Set(cx - 4, cy - 3, Tones.Shift(Paper, -1));
            c.Set(cx - 2, cy - 2, Color.white);
        }

        // -------------------------------------------------------------------------------
        // Kitchen
        // -------------------------------------------------------------------------------

        private static void Stove(PixelCanvas c, Color tint, int f)
        {
            Shadow(c, 16f, f + 29, 13f, 3f);

            // Flue at the back left, clear of the pot and its steam.
            c.Rect(5, 0, 4, f + 3, Iron);
            c.VLine(5, 0, f + 2, Tones.Shift(Iron, 1));
            c.HLine(4, 9, 3, Brass);

            var body = tint;
            c.Rect(3, f + 1, 26, 9, Tones.Shift(body, 1));
            c.HLine(3, 28, f + 9, Tones.Shift(body, 2));
            c.Rect(3, f + 10, 26, 18, body);
            c.VLine(28, f + 10, f + 27, Tones.Shift(body, -1));
            c.HLine(3, 28, f + 27, Tones.Shift(body, -2));

            // Burners: one free, one with the pot on it.
            Burner(c, 10, f + 5);
            Burner(c, 21, f + 5);

            // A round pot with a lid and two handles.
            c.RoundRect(16, f - 3, 10, 8, Copper, 2);
            c.VLine(17, f - 2, f + 3, Tones.Shift(Copper, 2));
            c.VLine(24, f - 2, f + 3, Tones.Shift(Copper, -1));
            c.HLine(18, 23, f + 4, Tones.Shift(Copper, -1));
            c.Ellipse(21f, f - 3f, 5f, 1.5f, Tones.Shift(Copper, -1));
            c.Ellipse(21f, f - 3.5f, 4f, 1f, Tones.Shift(Copper, 1));
            c.Rect(20, f - 6, 2, 2, Tones.Shift(Copper, -2));
            c.Set(15, f - 1, Tones.Shift(Copper, -1));
            c.Set(26, f - 1, Tones.Shift(Copper, -1));

            // Knobs, the oven door and its window.
            for (var k = 0; k < 3; k++) c.Rect(8 + k * 7, f + 11, 2, 2, Brass);

            c.Rect(7, f + 14, 18, 12, Tones.Shift(body, -1));
            c.HLine(8, 23, f + 15, Brass);
            c.Rect(11, f + 17, 10, 5, Palette.Hex("#4A3036"));
            c.Rect(12, f + 19, 8, 3, Ember);
            c.HLine(13, 18, f + 21, Flame);

            // A tea towel over the handle.
            c.Rect(20, f + 15, 4, 7, Cream);
            c.HLine(20, 23, f + 18, Berry);

            c.Rect(4, f + 28, 3, 2, Iron);
            c.Rect(25, f + 28, 3, 2, Iron);

            c.Outline(Ink);

            Steam(c, 22, f - 8);
        }

        private static void Burner(PixelCanvas c, int cx, int cy)
        {
            c.Ellipse(cx, cy, 4.5f, 2f, Palette.Hex("#3E3A42"));
            c.Ellipse(cx, cy, 2.5f, 1f, Palette.Hex("#57525E"));
        }

        private static void TeaBar(PixelCanvas c, Color tint, int w, int h, int f)
        {
            var wood = Palette.Wood;

            if (h > w)
            {
                var length = h * 32;
                Shadow(c, 16f, f + length - 4, 13f, 3f);
                c.Rect(5, f + 2, 22, length - 13, Tones.Shift(wood, 1));
                Grain(c, 6, f + 3, 20, length - 15, wood, 5);
                c.VLine(5, f + 2, f + length - 12, Tones.Shift(wood, 2));
                c.Rect(5, f + length - 11, 22, 9, tint);
                c.HLine(5, 26, f + length - 11, Tones.Shift(tint, 2));
                c.HLine(5, 26, f + length - 3, Tones.Shift(tint, -2));
                c.Rect(8, f + length - 9, 16, 5, Tones.Shift(tint, 1));
                Kettle(c, 15, f + 12);
                Teapot(c, 16, f + 28, Teal);
                Jar(c, 10, f + 40, Leaf);
                Jar(c, 16, f + 40, Palette.Hex("#B37A4E"));
                Jar(c, 22, f + 40, Palette.Hex("#D9B060"));
                c.Outline(Ink);
                Steam(c, 13, f + 4);
                return;
            }

            var width = c.Width;
            Shadow(c, width * 0.5f, f + 29, width * 0.45f, 3f);

            c.Rect(2, f + 6, width - 4, 8, Tones.Shift(wood, 1));
            Grain(c, 3, f + 7, width - 6, 6, wood, 11);
            c.HLine(2, width - 3, f + 13, Tones.Shift(wood, 2));

            c.Rect(2, f + 14, width - 4, 16, tint);
            c.HLine(2, width - 3, f + 14, Tones.Shift(tint, -1));
            c.VLine(width - 3, f + 14, f + 29, Tones.Shift(tint, -1));
            c.Rect(2, f + 28, width - 4, 2, Tones.Shift(tint, -2));

            var panels = Mathf.Max(1, (width - 8) / 18);
            var panelWidth = (width - 8 - (panels - 1) * 3) / panels;
            for (var p = 0; p < panels; p++)
            {
                var px = 4 + p * (panelWidth + 3);
                c.Rect(px, f + 17, panelWidth, 9, Tones.Shift(tint, -1));
                c.Rect(px + 1, f + 18, panelWidth - 2, 7, Tones.Shift(tint, 1));
                c.HLine(px + 1, px + panelWidth - 2, f + 18, Tones.Shift(tint, 2));
            }

            Kettle(c, 11, f + 6);
            TeaSetting(c, 22, f + 10);
            Teapot(c, 32, f + 7, Teal);
            Jar(c, 43, f + 9, Leaf);
            Jar(c, 49, f + 9, Palette.Hex("#B37A4E"));
            Jar(c, 55, f + 9, Palette.Hex("#D9B060"));

            c.Outline(Ink);

            Steam(c, 8, f - 3);
        }

        private static void Kettle(PixelCanvas c, int cx, int cy)
        {
            // Handle first, an arch over the lid.
            var handle = Palette.Hex("#5C4038");
            c.VLine(cx - 3, cy - 5, cy - 3, handle);
            c.VLine(cx + 3, cy - 5, cy - 3, handle);
            c.Set(cx - 2, cy - 6, handle);
            c.Set(cx + 2, cy - 6, handle);
            c.HLine(cx - 1, cx + 1, cy - 7, handle);

            c.Ellipse(cx, cy, 4.5f, 3.5f, Copper);
            c.Ellipse(cx - 1.5f, cy - 1.5f, 2f, 1.2f, Tones.Shift(Copper, 2));
            c.HLine(cx - 3, cx + 3, cy + 3, Tones.Shift(Copper, -1));
            c.Rect(cx - 1, cy - 4, 3, 1, Tones.Shift(Copper, -1));
            c.Set(cx, cy - 5, Tones.Shift(Copper, -2));
            c.Line(cx - 4, cy, cx - 7, cy - 3, Copper);
            c.Set(cx - 7, cy - 4, Tones.Shift(Copper, 1));
        }

        private static void Jar(PixelCanvas c, int cx, int bottom, Color contents)
        {
            c.Rect(cx - 2, bottom - 7, 5, 7, Glass);
            c.Rect(cx - 1, bottom - 5, 3, 4, contents);
            c.VLine(cx - 2, bottom - 6, bottom - 2, Color.white);
            c.Rect(cx - 2, bottom - 9, 5, 2, Palette.Hex("#B98C5E"));
        }

        private static void Dessert(PixelCanvas c, Color tint, int f)
        {
            Shadow(c, 16f, f + 29, 13f, 3f);

            // Marble top, pink counter, lace trim.
            c.Rect(2, f + 8, 28, 7, Tones.Shift(Paper, -1));
            c.Rect(3, f + 9, 26, 5, Paper);
            c.Line(6, f + 10, 11, f + 13, Tones.Shift(Paper, -1));
            c.HLine(2, 29, f + 14, Color.white);
            c.Rect(2, f + 15, 28, 15, tint);
            for (var x = 4; x < 29; x += 4) c.VLine(x, f + 18, f + 28, Tones.Shift(tint, 1));
            for (var x = 2; x < 30; x += 3)
            {
                c.HLine(x, x + 1, f + 15, Paper);
                c.Set(x, f + 16, Paper);
            }

            c.VLine(29, f + 15, f + 29, Tones.Shift(tint, -1));
            c.HLine(2, 29, f + 29, Tones.Shift(tint, -2));

            // A cake on a stand.
            c.Ellipse(13f, f + 11.5f, 8f, 2f, Tones.Shift(Paper, -1));
            c.Rect(8, f + 3, 11, 8, Palette.Hex("#F2D3A2"));
            c.HLine(8, 18, f + 6, Paper);
            c.HLine(8, 18, f + 8, Tones.Shift(tint, 1));
            c.VLine(18, f + 3, f + 10, Palette.Hex("#D9B585"));
            c.Ellipse(13f, f + 3f, 5.5f, 1.8f, Tones.Shift(tint, 2));
            c.Rect(10, f, 2, 2, Berry);
            c.Rect(15, f, 2, 2, Berry);
            c.Set(10, f, Tones.Shift(Berry, 2));
            c.Set(15, f, Tones.Shift(Berry, 2));

            // Macarons.
            Macaron(c, 24, f + 12, Palette.Hex("#A8D8B9"));
            Macaron(c, 24, f + 9, Tones.Shift(tint, 1));
            Macaron(c, 24, f + 6, Palette.Hex("#F2DE8C"));

            c.Outline(Ink);

            // The glass dome goes on after the outline: glass is seen through, not around.
            DomeGlass(c, 13f, f + 4f, 8.5f, 9.5f);
        }

        private static void Macaron(PixelCanvas c, int cx, int cy, Color colour)
        {
            c.HLine(cx - 2, cx + 2, cy, colour);
            c.HLine(cx - 2, cx + 2, cy + 1, Cream);
            c.HLine(cx - 2, cx + 2, cy + 2, Tones.Shift(colour, -1));
            c.Set(cx - 1, cy, Tones.Shift(colour, 2));
        }

        private static void DomeGlass(PixelCanvas c, float cx, float cy, float rx, float ry)
        {
            var rim = Tones.Alpha(Palette.Hex("#E9F5FA"), 0.85f);
            for (var a = 0; a < 64; a++)
            {
                var t = Mathf.PI * a / 63f;
                var x = Mathf.RoundToInt(cx + Mathf.Cos(t) * rx - 0.5f);
                var y = Mathf.RoundToInt(cy - Mathf.Sin(t) * ry);
                c.Set(x, y, rim);
            }

            c.Line(Mathf.RoundToInt(cx - rx * 0.55f), Mathf.RoundToInt(cy - ry * 0.55f),
                Mathf.RoundToInt(cx - rx * 0.3f), Mathf.RoundToInt(cy - ry * 0.85f),
                Tones.Alpha(Color.white, 0.9f));
            c.Rect(Mathf.RoundToInt(cx) - 1, Mathf.RoundToInt(cy - ry) - 1, 2, 2, Brass);
        }

        private static void Bakery(PixelCanvas c, Color tint, int w, int h, int f)
        {
            var width = c.Width;
            var height = c.Height;
            var cx = width / 2;
            var mortar = Tones.Mix(Paper, tint, 0.35f);

            Shadow(c, cx, height - 3, width * 0.44f, 3f);

            // Chimney.
            var flue = cx + (w > h ? 12 : 4);
            c.Rect(flue - 2, 0, 6, f + 10, Tones.Shift(tint, -1));
            c.HLine(flue - 3, flue + 4, 0, Iron);
            c.HLine(flue - 3, flue + 4, 1, Iron);

            // Dome and body.
            var domeY = f + (h > w ? 14 : 6);
            var rx = width * 0.5f - 3f;
            c.Ellipse(cx, domeY, rx, 13f, tint);
            c.Rect(3, domeY, width - 6, height - domeY - 3, tint);

            Bricks(c, 3, domeY - 13, width - 6, height - domeY + 10, tint, mortar);

            c.VLine(width - 4, domeY, height - 4, Tones.Shift(tint, -1));
            c.HLine(3, width - 4, height - 4, Tones.Shift(tint, -2));

            // Mouth: the fire at the back, a loaf on the floor in front of it.
            var mouthTop = height - 30;
            var mouthWidth = Mathf.Min(16, width - 12);
            var mx = cx - mouthWidth / 2;
            var soot = Palette.Hex("#3C2A2E");
            c.Rect(mx - 1, mouthTop + 3, mouthWidth + 2, 17, Tones.Shift(tint, -2));
            c.Ellipse(cx - 0.5f, mouthTop + 5f, mouthWidth * 0.5f + 1f, 5f, Tones.Shift(tint, -2));
            c.Rect(mx, mouthTop + 4, mouthWidth, 15, soot);
            c.Ellipse(cx - 0.5f, mouthTop + 5f, mouthWidth * 0.5f, 4f, soot);

            for (var x = mx + 1; x < mx + mouthWidth - 1; x++)
            {
                var flicker = Hash(x, 5) % 3;
                c.VLine(x, mouthTop + 9 - flicker, mouthTop + 10, flicker == 0 ? Flame : Ember);
                c.Set(x, mouthTop + 11, Berry);
            }

            c.Rect(mx, mouthTop + 12, mouthWidth, 7, Palette.Hex("#4A3238"));
            c.Ellipse(cx - 0.5f, mouthTop + 15f, 5f, 2.2f, Palette.Hex("#C98B4F"));
            c.Ellipse(cx - 1f, mouthTop + 14.5f, 3.5f, 1.2f, Palette.Hex("#E8B270"));
            c.Set(cx - 2, mouthTop + 15, Palette.Hex("#A86F3E"));
            c.Set(cx + 1, mouthTop + 15, Palette.Hex("#A86F3E"));
            c.HLine(mx - 1, mx + mouthWidth, mouthTop + 19, Palette.Hex("#9A9088"));

            // A small stack of logs at the foot of the oven.
            var logX = w > h ? 8 : width - 9;
            Log(c, logX - 2, height - 7);
            Log(c, logX + 2, height - 7);
            Log(c, logX, height - 10);

            c.Outline(Ink);

            Glow(c, cx, mouthTop + 10f, mouthWidth * 0.75f, 5f, Ember, 0.2f);
        }

        /// <summary>The end of a log: bark ring, pale wood, a growth ring.</summary>
        private static void Log(PixelCanvas c, int cx, int cy)
        {
            c.Ellipse(cx + 0.5f, cy + 0.5f, 2.6f, 2.4f, Palette.Hex("#5C3B2E"));
            c.Ellipse(cx + 0.5f, cy + 0.5f, 1.7f, 1.5f, Palette.Hex("#D9A66A"));
            c.Set(cx, cy, Palette.Hex("#B5834F"));
        }

        private static void Bricks(PixelCanvas c, int x, int y, int width, int height, Color tint,
            Color mortar)
        {
            for (var row = 0; row * 4 < height; row++)
            {
                var by = y + row * 4;
                for (var i = x; i < x + width; i++) c.SetOnSolid(i, by, mortar);

                var offset = row % 2 == 0 ? 0 : 3;
                for (var i = x + offset; i < x + width; i += 6)
                {
                    for (var j = by + 1; j < by + 4; j++) c.SetOnSolid(i, j, mortar);
                }

                for (var i = x + offset + 1; i < x + width; i += 12)
                {
                    c.SetOnSolid(i, by + 1, Tones.Shift(tint, 1));
                }
            }
        }

        // -------------------------------------------------------------------------------
        // Décor
        // -------------------------------------------------------------------------------

        private static void Fern(PixelCanvas c, Color tint, int f)
        {
            Shadow(c, 16f, f + 29, 9f, 2.5f);

            Pot(c, 16, f + 19, f + 29, 8, 6, Terracotta);
            c.Ellipse(16f, f + 19.5f, 6.5f, 1.4f, Palette.Hex("#5C3F34"));

            // Seven fronds arching out of the pot like a fountain, with daylight between
            // them: a fern reads by its silhouette, and a solid mound of leaves reads as a
            // bush. Back fronds darker, front ones lighter, because the light is overhead.
            var dark = Tones.Shift(tint, -1);
            var light = Tones.Shift(tint, 1);
            var bx = 16f;
            var by = f + 18f;

            Frond(c, bx, by, 10f, f - 6f, 4f, f + 3f, dark);
            Frond(c, bx, by, 22f, f - 6f, 28f, f + 3f, dark);
            Frond(c, bx, by, 15f, f - 10f, 13f, f - 9f, tint);
            Frond(c, bx, by, 5f, f + 2f, 1f, f + 14f, tint);
            Frond(c, bx, by, 27f, f + 2f, 31f, f + 14f, tint);
            Frond(c, bx, by, 12f, f - 2f, 7f, f + 8f, light);
            Frond(c, bx, by, 20f, f - 2f, 25f, f + 8f, light);

            c.Outline(Ink, 0.45f);
        }

        /// <summary>
        /// One fern frond along a quadratic curve: thick at the base, tapering to the tip,
        /// with a leaflet notched out on alternate sides at every other step.
        /// </summary>
        private static void Frond(PixelCanvas c, float x0, float y0, float cx, float cy,
            float x1, float y1, Color colour)
        {
            var tip = Tones.Shift(colour, 1);
            var shade = Tones.Shift(colour, -1);
            const int steps = 26;

            for (var i = 0; i <= steps; i++)
            {
                var t = i / (float)steps;
                var u = 1f - t;
                var x = u * u * x0 + 2f * u * t * cx + t * t * x1;
                var y = u * u * y0 + 2f * u * t * cy + t * t * y1;

                var tx = 2f * u * (cx - x0) + 2f * t * (x1 - cx);
                var ty = 2f * u * (cy - y0) + 2f * t * (y1 - cy);
                var norm = Mathf.Max(0.001f, Mathf.Sqrt(tx * tx + ty * ty));
                var nx = -ty / norm;
                var ny = tx / norm;

                var px = Mathf.RoundToInt(x);
                var py = Mathf.RoundToInt(y);
                var body = t > 0.8f ? tip : colour;
                c.Set(px, py, body);

                // The width tapers from two pixels either side down to the bare tip.
                var half = t < 0.35f ? 1 : t < 0.75f ? 1 : 0;
                for (var r = 1; r <= half; r++)
                {
                    c.Set(Mathf.RoundToInt(x + nx * r), Mathf.RoundToInt(y + ny * r), body);
                    c.Set(Mathf.RoundToInt(x - nx * r), Mathf.RoundToInt(y - ny * r), shade);
                }

                if (i % 3 != 1 || t < 0.2f || t > 0.9f) continue;

                // Leaflets: one pixel proud of the frond, alternating sides.
                var side = (i / 3) % 2 == 0 ? 1f : -1f;
                var reach = half + 1;
                c.Set(Mathf.RoundToInt(x + nx * reach * side), Mathf.RoundToInt(y + ny * reach * side),
                    side > 0 ? tip : colour);
            }
        }

        private static void Pot(PixelCanvas c, int cx, int top, int bottom, int topHalf,
            int bottomHalf, Color colour)
        {
            for (var y = top; y <= bottom; y++)
            {
                var t = (y - top) / (float)Mathf.Max(1, bottom - top);
                var half = Mathf.RoundToInt(Mathf.Lerp(topHalf, bottomHalf, t));
                c.HLine(cx - half, cx + half - 1, y, colour);
                c.Set(cx - half + 1, y, Tones.Shift(colour, 1));
                c.Set(cx + half - 1, y, Tones.Shift(colour, -1));
                c.Set(cx + half - 2, y, Tones.Shift(colour, -1));
            }

            c.Rect(cx - topHalf - 1, top - 1, topHalf * 2 + 2, 3, Tones.Shift(colour, 1));
            c.HLine(cx - topHalf - 1, cx + topHalf, top + 1, colour);
            c.HLine(cx - bottomHalf, cx + bottomHalf - 1, bottom, Tones.Shift(colour, -2));
        }

        private static void Window(PixelCanvas c, Color tint, int f)
        {
            Shadow(c, 16f, f + 29, 12f, 2.5f);

            var frame = DarkWood;
            c.Rect(7, f + 25, 3, 4, Tones.Shift(frame, -1));
            c.Rect(22, f + 25, 3, 4, Tones.Shift(frame, -1));

            // Arched frame and four panes.
            c.Ellipse(16f, 9f, 12f, 8f, frame);
            c.Rect(4, 9, 24, f + 17, frame);
            c.Ellipse(16f, 9.5f, 9.5f, 6f, tint);
            c.Rect(7, 9, 18, f + 13, tint);

            // Sky in the glass: brightest at the top, falling off towards the sill.
            for (var y = 4; y < f + 22; y++)
            {
                var light = y < 14 ? 2 : y < f ? 1 : 0;
                for (var x = 7; x < 25; x++) c.SetOnSolid(x, y, Tones.Shift(tint, light));
            }

            c.Ellipse(16f, 9.5f, 9.5f, 6f, Tones.Shift(tint, 2));
            c.Rect(7, 10, 18, 4, Tones.Shift(tint, 2));
            c.Rect(15, 4, 2, f + 18, frame);
            c.Rect(7, f - 2, 18, 2, frame);
            c.Line(9, 20, 13, 16, Color.white);
            c.Line(9, 22, 11, 20, Color.white);
            c.Line(19, f + 6, 22, f + 3, Color.white);

            // Curtains tied back at either side.
            var curtain = Palette.Hex("#E7B7B9");
            Curtain(c, 4, 3, f + 4, curtain, 1);
            Curtain(c, 27, 3, f + 4, curtain, -1);
            c.Rect(4, 2, 24, 3, Tones.Shift(curtain, -1));
            for (var x = 4; x < 28; x += 2) c.Set(x, 5, Tones.Shift(curtain, -1));

            // Window box with flowers.
            c.Rect(5, f + 19, 22, 5, Terracotta);
            c.HLine(5, 26, f + 19, Tones.Shift(Terracotta, 1));
            c.HLine(5, 26, f + 23, Tones.Shift(Terracotta, -1));
            for (var x = 7; x < 26; x += 4)
            {
                c.Set(x, f + 18, Leaf);
                c.Set(x + 1, f + 17, Leaf);
                Blossom(c, x + 1, f + 16, x % 8 == 7 ? Palette.Hex("#F2A7BC") : Palette.Hex("#F2DE8C"));
            }

            c.Outline(Ink);
        }

        private static void Curtain(PixelCanvas c, int edge, int top, int bottom, Color colour,
            int direction)
        {
            for (var y = top; y <= bottom; y++)
            {
                // Narrow at the tie, flaring below it.
                var t = (y - top) / (float)Mathf.Max(1, bottom - top);
                var width = t < 0.55f ? Mathf.RoundToInt(Mathf.Lerp(5f, 2f, t / 0.55f))
                    : Mathf.RoundToInt(Mathf.Lerp(2f, 4f, (t - 0.55f) / 0.45f));
                for (var i = 0; i < width; i++)
                {
                    var shade = i == width - 1 ? -1 : i == 0 ? 1 : 0;
                    c.Set(edge + i * direction, y, Tones.Shift(colour, shade));
                }
            }

            var tieY = top + Mathf.RoundToInt((bottom - top) * 0.55f);
            c.Set(edge, tieY, Teal);
            c.Set(edge + direction, tieY, Teal);
            c.Set(edge + 2 * direction, tieY, Teal);
        }

        private static void Rug(PixelCanvas c, Color tint, int f)
        {
            var width = c.Width;
            var height = c.Height;
            var x0 = 3;
            var y0 = f + 5;
            var rw = width - 6;
            var rh = height - f - 10;

            // Fringe on the short ends.
            for (var y = y0 + 1; y < y0 + rh - 1; y += 2)
            {
                c.HLine(x0 - 3, x0 - 1, y, Cream);
                c.HLine(x0 + rw, x0 + rw + 2, y, Cream);
            }

            c.RoundRect(x0, y0, rw, rh, Tones.Shift(tint, -1), 2);
            c.RoundRect(x0 + 2, y0 + 2, rw - 4, rh - 4, Cream, 1);
            c.RoundRect(x0 + 3, y0 + 3, rw - 6, rh - 6, tint, 1);

            // A diamond medallion and a dotted inner border.
            var cx = width / 2f - 0.5f;
            var cy = y0 + rh / 2f - 0.5f;
            Diamond(c, cx, cy, rw * 0.3f, rh * 0.32f, Tones.Shift(tint, -1));
            Diamond(c, cx, cy, rw * 0.24f, rh * 0.25f, Cream);
            Diamond(c, cx, cy, rw * 0.19f, rh * 0.19f, Tones.Shift(tint, 1));
            Diamond(c, cx, cy, rw * 0.07f, rh * 0.07f, Teal);

            for (var x = x0 + 6; x < x0 + rw - 6; x += 3)
            {
                c.Set(x, y0 + 5, Tones.Shift(tint, 1));
                c.Set(x, y0 + rh - 6, Tones.Shift(tint, 1));
            }

            for (var y = y0 + 6; y < y0 + rh - 6; y += 3)
            {
                c.Set(x0 + 5, y, Tones.Shift(tint, 1));
                c.Set(x0 + rw - 6, y, Tones.Shift(tint, 1));
            }

            Diamond(c, x0 + 9, y0 + 9, 3f, 3f, Cream);
            Diamond(c, x0 + rw - 10, y0 + 9, 3f, 3f, Cream);
            Diamond(c, x0 + 9, y0 + rh - 10, 3f, 3f, Cream);
            Diamond(c, x0 + rw - 10, y0 + rh - 10, 3f, 3f, Cream);

            c.Outline(Ink, 0.35f);
        }

        private static void Diamond(PixelCanvas c, float cx, float cy, float rx, float ry, Color colour)
        {
            var x0 = Mathf.FloorToInt(cx - rx);
            var x1 = Mathf.CeilToInt(cx + rx);
            var y0 = Mathf.FloorToInt(cy - ry);
            var y1 = Mathf.CeilToInt(cy + ry);
            for (var y = y0; y <= y1; y++)
            {
                for (var x = x0; x <= x1; x++)
                {
                    var d = Mathf.Abs(x + 0.5f - cx - 0.5f) / Mathf.Max(0.5f, rx) +
                            Mathf.Abs(y + 0.5f - cy - 0.5f) / Mathf.Max(0.5f, ry);
                    if (d <= 1f) c.Set(x, y, colour);
                }
            }
        }

        private static void CatTower(PixelCanvas c, Color tint, int w, int h, int f)
        {
            var width = c.Width;
            var height = c.Height;
            var cx = width / 2;
            var sisal = Palette.Hex("#D8C397");

            Shadow(c, cx, height - 3, width * 0.42f, 3f);

            // Base.
            c.Rect(3, height - 12, width - 6, 4, Tones.Shift(tint, 1));
            c.Rect(3, height - 8, width - 6, 5, tint);
            c.HLine(3, width - 4, height - 4, Tones.Shift(tint, -1));

            var tall = h > w;
            var posts = tall ? new[] { cx - 3 } : new[] { 11, width - 17 };
            var platformY = tall ? f + 26 : f + 2;
            var topY = tall ? f - 2 : f - 8;

            foreach (var px in posts) Post(c, px, topY + 2, height - 12, sisal);

            if (tall)
            {
                c.Rect(6, platformY, width - 12, 3, Tones.Shift(tint, 1));
                c.Rect(6, platformY + 3, width - 12, 3, tint);
                c.HLine(6, width - 7, platformY + 5, Tones.Shift(tint, -1));
                // A toy on a string from the middle platform.
                c.Ellipse(width - 8f, platformY + 15f, 2f, 2f, Berry);
            }
            else
            {
                // A hammock slung between the two posts, so the wide tower does not read as
                // a table with a cat on it.
                var left = posts[0] + 6;
                var right = posts[1] - 1;
                for (var x = left; x <= right; x++)
                {
                    var t = (x - left) / (float)Mathf.Max(1, right - left);
                    var sag = Mathf.RoundToInt(Mathf.Sin(t * Mathf.PI) * 6f);
                    var y = f + 6 + sag;
                    c.VLine(x, y, y + 2, Cushion);
                    c.Set(x, y, Tones.Shift(Cushion, 2));
                    c.Set(x, y + 3, Tones.Shift(Cushion, -1));
                }

                c.Ellipse(cx + 8f, f + 16f, 2f, 2f, Berry);
            }

            // Top platform with a bed, and somebody asleep in it.
            c.Rect(4, topY, width - 8, 3, Tones.Shift(tint, 1));
            c.Rect(4, topY + 3, width - 8, 3, tint);
            c.HLine(4, width - 5, topY + 5, Tones.Shift(tint, -1));

            c.Ellipse(cx, topY - 1f, 10f, 3.5f, Tones.Shift(Cushion, -1));
            c.Ellipse(cx, topY - 2f, 8f, 2.5f, Cushion);
            Cat(c, cx, topY - 4);

            c.Outline(Ink);

            if (tall)
            {
                c.VLine(width - 8, platformY + 6, platformY + 12, Tones.Alpha(Ink, 0.8f));
            }
        }

        private static void Post(PixelCanvas c, int x, int top, int bottom, Color sisal)
        {
            for (var y = top; y <= bottom; y++)
            {
                var band = y % 2 == 0 ? sisal : Tones.Shift(sisal, -1);
                c.HLine(x, x + 5, y, band);
                c.Set(x, y, Tones.Shift(sisal, 1));
                c.Set(x + 5, y, Tones.Shift(sisal, -2));
            }
        }

        /// <summary>A cat, curled up asleep. Every café of this name needs one somewhere.</summary>
        private static void Cat(PixelCanvas c, int cx, int cy)
        {
            var fur = Palette.Hex("#E3A462");
            c.Ellipse(cx, cy, 6.5f, 3f, fur);
            c.Ellipse(cx - 3.5f, cy - 1.5f, 3f, 2.5f, fur);
            c.Set(cx - 6, cy - 4, fur);
            c.Set(cx - 5, cy - 4, fur);
            c.Set(cx - 2, cy - 4, fur);
            c.Set(cx - 1, cy - 4, fur);
            c.Set(cx - 5, cy - 3, Tones.Shift(fur, 1));
            c.HLine(cx - 5, cx - 4, cy - 1, Ink);
            c.HLine(cx - 2, cx - 1, cy - 1, Ink);
            c.Set(cx - 3, cy, Palette.InnerEar);
            for (var x = cx; x <= cx + 4; x += 2) c.VLine(x, cy - 2, cy - 1, Tones.Shift(fur, -1));
            c.HLine(cx + 2, cx + 7, cy + 2, Tones.Shift(fur, -1));
            c.Set(cx + 7, cy + 1, Tones.Shift(fur, -1));
        }

        private static void Stage(PixelCanvas c, Color tint, int f)
        {
            var width = c.Width;
            var height = c.Height;
            var plank = Palette.Hex("#B98458");

            Shadow(c, width * 0.5f, height - 3, width * 0.47f, 3f);

            // Backdrop: velvet folds, a scalloped pelmet, curtains tied back.
            c.Rect(3, 4, width - 6, f + 18, tint);
            for (var x = 3; x < width - 3; x++)
            {
                var fold = (x / 3) % 3;
                var shade = fold == 0 ? -1 : fold == 2 ? 1 : 0;
                c.VLine(x, 6, f + 21, Tones.Shift(tint, shade));
            }

            c.Rect(1, 0, width - 2, 5, Tones.Shift(tint, -1));
            for (var x = 2; x < width - 2; x += 6) c.Ellipse(x + 2.5f, 5f, 3f, 2f, Tones.Shift(tint, -1));
            c.HLine(1, width - 2, 1, Brass);
            for (var x = 3; x < width - 3; x += 6) c.Set(x + 2, 7, Brass);

            Swag(c, 3, 6, f + 21, tint, 1);
            Swag(c, width - 4, 6, f + 21, tint, -1);

            // Platform.
            var top = f + 20;
            var front = height - 13;
            c.Rect(1, top, width - 2, front - top, Tones.Shift(plank, 1));
            for (var y = top + 4; y < front; y += 5) c.HLine(1, width - 2, y, plank);
            for (var y = top; y < front; y += 5)
            {
                var seam = 6 + (y * 7) % 20;
                for (var x = seam; x < width - 2; x += 22) c.VLine(x, y + 1, Mathf.Min(y + 3, front - 1), plank);
            }

            c.HLine(1, width - 2, front - 1, Tones.Shift(plank, 2));
            c.Rect(1, front, width - 2, 9, Tones.Shift(tint, -1));
            c.HLine(1, width - 2, front, Brass);
            for (var x = 4; x < width - 3; x += 4) c.VLine(x, front + 2, front + 7, Tones.Shift(tint, -2));

            // A microphone, waiting.
            var mx = width / 2;
            c.Ellipse(mx, front - 8f, 3f, 1.2f, Iron);
            c.VLine(mx, top + 3, front - 9, Iron);
            c.Rect(mx - 1, top, 3, 4, Palette.Hex("#4A4650"));
            c.Set(mx - 1, top, Tones.Shift(Iron, 2));

            c.Outline(Ink);

            for (var x = 6; x < width - 4; x += 9)
            {
                c.HLine(x, x + 1, front - 2, Flame);
                Glow(c, x + 1, front - 3f, 3f, 2f, Flame, 0.25f);
            }

            Sparkle(c, 10, 14, Brass);
            Sparkle(c, width - 11, 12, Brass);
        }

        private static void Swag(PixelCanvas c, int edge, int top, int bottom, Color tint,
            int direction)
        {
            for (var y = top; y <= bottom; y++)
            {
                var t = (y - top) / (float)Mathf.Max(1, bottom - top);
                var width = Mathf.RoundToInt(Mathf.Lerp(8f, 3f, Mathf.Sin(t * Mathf.PI * 0.85f)));
                for (var i = 0; i < width; i++)
                {
                    var shade = i == width - 1 ? -2 : i % 3 == 1 ? 1 : -1;
                    c.Set(edge + i * direction, y, Tones.Shift(tint, shade));
                }
            }
        }

        private static void Chandelier(PixelCanvas c, Color tint, int f)
        {
            // The light it throws on the floor, under everything.
            c.Ellipse(16f, f + 21f, 13f, 4.5f, Tones.Alpha(Flame, 0.22f));
            c.Ellipse(16f, f + 21f, 8f, 2.5f, Tones.Alpha(Flame, 0.2f));

            for (var y = 0; y < 9; y++) c.Set(y % 2 == 0 ? 15 : 16, y, Brass);

            // Arms, candles and the ring.
            for (var i = 0; i < 5; i++)
            {
                var x = 5 + i * 5 + (i > 2 ? 1 : 0);
                var lift = i == 2 ? 2 : i == 1 || i == 3 ? 1 : 0;
                c.Rect(x, 11 - lift, 2, 5, Cream);
                c.Set(x, 11 - lift, Paper);
                c.HLine(x - 1, x + 2, 16 - lift, Brass);
            }

            c.Ellipse(16f, 17.5f, 12f, 3.2f, Brass);
            c.Ellipse(16f, 17f, 9.5f, 2f, tint);
            c.HLine(7, 24, 18, Tones.Shift(Brass, 1));
            c.Rect(14, 8, 4, 9, Tones.Shift(Brass, -1));
            c.VLine(14, 8, 16, Tones.Shift(Brass, 1));
            c.Ellipse(16f, 23f, 2.5f, 2f, Glass);

            // Crystal drops.
            for (var x = 6; x <= 26; x += 4)
            {
                var drop = 21 + (x / 4) % 2;
                c.VLine(x, 20, drop, Tones.Shift(Glass, -1));
                c.Set(x, drop + 1, Glass);
            }

            c.Outline(Ink, 0.45f);

            for (var i = 0; i < 5; i++)
            {
                var x = 5 + i * 5 + (i > 2 ? 1 : 0);
                var lift = i == 2 ? 2 : i == 1 || i == 3 ? 1 : 0;
                c.Set(x, 9 - lift, Flame);
                c.Set(x + 1, 10 - lift, Ember);
                c.Set(x, 10 - lift, Flame);
                Glow(c, x + 0.5f, 9.5f - lift, 2.5f, 2.5f, Flame, 0.22f);
            }

            Sparkle(c, 8, 24, Color.white);
            Sparkle(c, 24, 23, Color.white);
        }

        private static void KoiPond(PixelCanvas c, Color tint, int f)
        {
            var width = c.Width;
            var height = c.Height;
            var cx = width / 2f - 0.5f;
            var cy = f + (height - f) / 2f - 1f;
            var rx = width * 0.47f;
            var ry = (height - f) * 0.43f;
            var stone = Palette.Hex("#A8A196");

            c.Ellipse(cx, cy, rx, ry, stone);

            // Stones around the rim, each a slightly different grey.
            for (var i = 0; i < 18; i++)
            {
                var t = i / 18f * Mathf.PI * 2f;
                var sx = cx + Mathf.Cos(t) * (rx - 2.5f);
                var sy = cy + Mathf.Sin(t) * (ry - 2.5f);
                var shade = i % 3 == 0 ? 1 : i % 3 == 1 ? 0 : -1;
                c.Ellipse(sx, sy, 4.2f, 3.2f, Tones.Shift(stone, shade));
                c.Set(Mathf.RoundToInt(sx - 1.5f), Mathf.RoundToInt(sy - 1.5f), Tones.Shift(stone, 2));
            }

            c.Ellipse(cx, cy, rx - 5f, ry - 5f, Tones.Shift(tint, -1));
            c.Ellipse(cx - 1f, cy - 1f, rx - 7f, ry - 7f, tint);
            c.Ellipse(cx - 6f, cy - 5f, rx * 0.45f, ry * 0.35f, Tones.Shift(tint, 1));
            c.Ellipse(cx + 3f, cy + 4f, rx * 0.35f, ry * 0.3f, Tones.Shift(tint, -1));

            Koi(c, cx - 8f, cy + 3f, 1, Palette.Hex("#F08A4B"));
            Koi(c, cx + 9f, cy - 4f, -1, Paper);
            Koi(c, cx + 4f, cy + 8f, 1, Palette.Hex("#F2B36A"));

            LilyPad(c, cx - 14f, cy - 6f, true);
            LilyPad(c, cx + 14f, cy + 5f, false);

            c.Outline(Ink, 0.35f);

            c.HLine(Mathf.RoundToInt(cx - 10), Mathf.RoundToInt(cx - 6), Mathf.RoundToInt(cy - 9),
                Tones.Alpha(Color.white, 0.75f));
            c.HLine(Mathf.RoundToInt(cx + 2), Mathf.RoundToInt(cx + 5), Mathf.RoundToInt(cy + 1),
                Tones.Alpha(Color.white, 0.6f));
        }

        private static void Koi(PixelCanvas c, float x, float y, int direction, Color colour)
        {
            c.Ellipse(x, y, 3.5f, 1.6f, colour);
            var tx = Mathf.RoundToInt(x - direction * 4f);
            var ty = Mathf.RoundToInt(y);
            c.Set(tx, ty - 1, colour);
            c.Set(tx, ty + 1, colour);
            c.Set(tx + direction, ty, colour);
            c.Set(Mathf.RoundToInt(x + direction * 1f), ty - 1, Berry);
            c.Set(Mathf.RoundToInt(x + direction * 2.5f), ty, Ink);
        }

        private static void LilyPad(PixelCanvas c, float x, float y, bool flower)
        {
            c.Ellipse(x, y, 4f, 2.4f, Leaf);
            c.Ellipse(x - 0.5f, y - 0.5f, 2.5f, 1.2f, Tones.Shift(Leaf, 1));
            c.Clear(Mathf.RoundToInt(x + 2f), Mathf.RoundToInt(y - 1f));
            c.Clear(Mathf.RoundToInt(x + 3f), Mathf.RoundToInt(y - 1f));
            if (flower) Blossom(c, Mathf.RoundToInt(x - 1f), Mathf.RoundToInt(y - 1f), Palette.Hex("#F2A7BC"));
        }

        private static void Lantern(PixelCanvas c, Color tint, int f)
        {
            Shadow(c, 16f, f + 29, 8f, 2f);

            var wood = DarkWood;
            c.Rect(10, 14, 2, f + 15, wood);
            c.Rect(20, 14, 2, f + 15, Tones.Shift(wood, -1));
            c.Rect(9, f + 12, 14, 2, wood);
            c.Rect(8, f + 27, 5, 2, Tones.Shift(wood, -1));
            c.Rect(19, f + 27, 5, 2, Tones.Shift(wood, -1));

            // Paper body, lit from inside.
            c.Rect(7, 4, 18, f + 6, tint);
            c.Rect(10, 6, 12, f + 2, Tones.Shift(tint, 2));
            c.Rect(12, 8, 8, f - 2, Tones.Shift(tint, 3));
            c.VLine(7, 4, f + 9, Tones.Shift(tint, -1));
            c.VLine(24, 4, f + 9, Tones.Shift(tint, -1));
            c.VLine(16, 4, f + 9, Tones.Shift(tint, -1));
            c.HLine(7, 24, 4 + (f + 6) / 2, Tones.Shift(tint, -1));
            c.Rect(5, 1, 22, 3, wood);
            c.HLine(6, 25, 1, Tones.Shift(wood, 1));
            c.Rect(6, f + 10, 20, 2, wood);

            Sakura(c, 12, 12, Tones.Shift(tint, -2));
            Sakura(c, 20, f + 2, Tones.Shift(tint, -2));

            c.Outline(Ink);

            Glow(c, 16f, f - 4f, 14f, 16f, Tones.Shift(tint, 2), 0.22f);
            Glow(c, 16f, f + 22f, 9f, 3f, Tones.Shift(tint, 2), 0.18f);
        }

        /// <summary>A five-petalled cherry blossom, five pixels across.</summary>
        private static void Sakura(PixelCanvas c, int cx, int cy, Color colour)
        {
            c.Set(cx, cy - 2, colour);
            c.Set(cx - 2, cy - 1, colour);
            c.Set(cx + 2, cy - 1, colour);
            c.Set(cx - 1, cy + 1, colour);
            c.Set(cx + 1, cy + 1, colour);
            c.Set(cx, cy, Tones.Shift(colour, -1));
        }

        private static void SakuraTree(PixelCanvas c, Color tint, int f)
        {
            Shadow(c, 16f, f + 29, 9f, 2.5f);

            var glaze = Palette.Hex("#4F5B7C");
            Pot(c, 16, f + 20, f + 29, 7, 5, glaze);
            c.HLine(10, 21, f + 23, Tones.Shift(glaze, 2));

            var bark = Palette.Hex("#7A5646");
            ThickLine(c, 16, f + 20, 14, f + 6, bark);
            ThickLine(c, 14, f + 8, 8, f - 6, bark);
            ThickLine(c, 15, f + 4, 23, f - 9, bark);
            c.Line(15, f + 18, 15, f + 8, Tones.Shift(bark, 1));

            // Blossom clouds: dark undersides, light tops.
            var clouds = new[]
            {
                new Vector4(9f, f - 9f, 7.5f, 6f), new Vector4(23f, f - 11f, 7.5f, 6f),
                new Vector4(16f, f - 18f, 9f, 7f), new Vector4(15f, f - 6f, 6f, 4.5f),
                new Vector4(5f, f - 14f, 4f, 3.5f), new Vector4(27f, f - 17f, 4f, 3.5f),
            };

            foreach (var cloud in clouds)
            {
                c.Ellipse(cloud.x, cloud.y + 1f, cloud.z, cloud.w, Tones.Shift(tint, -1));
            }

            foreach (var cloud in clouds)
            {
                c.Ellipse(cloud.x - 0.5f, cloud.y - 0.5f, cloud.z - 1.2f, cloud.w - 1.4f, tint);
                c.Ellipse(cloud.x - 1.5f, cloud.y - 2f, cloud.z * 0.5f, cloud.w * 0.45f,
                    Tones.Shift(tint, 2));
            }

            for (var i = 0; i < 14; i++)
            {
                var x = 3 + Hash(i, 3) % 26;
                var y = f - 24 + Hash(i, 7) % 24;
                c.SetOnSolid(x, y, i % 2 == 0 ? Paper : Tones.Shift(tint, 3));
            }

            c.Outline(Ink, 0.45f);

            c.Set(5, f + 4, Tones.Shift(tint, 1));
            c.Set(26, f + 1, tint);
            c.Set(23, f + 12, Tones.Shift(tint, 1));
            c.Set(8, f + 14, tint);
        }

        private static void Candelabra(PixelCanvas c, Color tint, int f)
        {
            Shadow(c, 16f, f + 29, 8f, 2f);

            var iron = tint;
            ThickLine(c, 15, f + 21, 9, f + 28, Tones.Shift(iron, -1));
            ThickLine(c, 16, f + 21, 22, f + 28, Tones.Shift(iron, -1));
            c.Ellipse(16f, f + 21f, 4.5f, 1.8f, iron);
            c.Rect(15, 12, 2, f + 9, iron);
            c.VLine(15, 12, f + 20, Tones.Shift(iron, 1));
            c.Ellipse(16f, f + 6f, 2.2f, 1.6f, Tones.Shift(iron, 1));
            c.Ellipse(16f, f - 6f, 2.2f, 1.6f, Tones.Shift(iron, 1));

            // Two curling arms.
            for (var side = -1; side <= 1; side += 2)
            {
                var x0 = 16 + side * 1;
                c.Line(x0, 16, x0 + side * 6, 16, iron);
                c.Line(x0 + side * 6, 16, x0 + side * 9, 13, iron);
                c.Line(x0 + side * 9, 13, x0 + side * 10, 10, iron);
                c.Set(x0 + side * 3, 17, Tones.Shift(iron, -1));
            }

            // Candles, with wax running down them.
            CandleStick(c, 15, 4, 8);
            CandleStick(c, 5, 5, 5);
            CandleStick(c, 25, 5, 5);

            c.Outline(Ink, 0.4f);

            CandleFlame(c, 15, 3);
            CandleFlame(c, 5, 4);
            CandleFlame(c, 25, 4);
        }

        private static void CandleStick(PixelCanvas c, int x, int top, int height)
        {
            c.Rect(x - 1, top + height, 4, 2, Brass);
            c.Rect(x, top, 2, height, Palette.Hex("#F2E6D2"));
            c.Set(x, top, Paper);
            c.Set(x + 1, top + 2, Paper);
            c.VLine(x + 1, top + 1, top + height - 1, Palette.Hex("#DCCDB6"));
        }

        private static void CandleFlame(PixelCanvas c, int x, int baseY)
        {
            c.Set(x, baseY - 1, Flame);
            c.Set(x + 1, baseY - 2, Flame);
            c.Set(x, baseY - 2, Ember);
            c.Set(x, baseY - 3, Flame);
            Glow(c, x + 0.5f, baseY - 2f, 3f, 3f, Flame, 0.2f);
        }

        private static void Mirror(PixelCanvas c, Color tint, int f)
        {
            Shadow(c, 16f, f + 29, 10f, 2.5f);

            var frame = tint;
            c.Rect(5, 10, 2, f + 17, frame);
            c.Rect(25, 10, 2, f + 17, Tones.Shift(frame, -1));
            c.Rect(3, f + 26, 7, 2, Tones.Shift(frame, -1));
            c.Rect(22, f + 26, 7, 2, Tones.Shift(frame, -1));
            c.Rect(7, f + 16, 18, 2, frame);

            var cy = f - 6f;
            c.Ellipse(16f, cy, 10f, 15f, frame);
            c.Ellipse(16f, cy, 8.5f, 13.5f, Brass);
            c.Ellipse(16f, cy, 7f, 12f, Palette.Hex("#AEBBD0"));
            c.Ellipse(14.5f, cy - 2f, 5f, 9f, Palette.Hex("#C9D4E3"));
            c.Line(10, Mathf.RoundToInt(cy + 2), 15, Mathf.RoundToInt(cy - 6), Color.white);
            c.Line(12, Mathf.RoundToInt(cy + 6), 18, Mathf.RoundToInt(cy - 4), Tones.Alpha(Color.white, 0.8f));

            // Crest and pivots.
            c.Rect(13, 1, 6, 4, Brass);
            c.Set(16, 0, Brass);
            c.Set(15, 0, Brass);
            c.HLine(12, 19, 4, Tones.Shift(Brass, -1));
            c.Rect(4, Mathf.RoundToInt(cy) - 1, 3, 3, Brass);
            c.Rect(25, Mathf.RoundToInt(cy) - 1, 3, 3, Brass);
            c.Rect(14, f + 8, 4, 3, Brass);

            c.Outline(Ink, 0.45f);

            Sparkle(c, 19, Mathf.RoundToInt(cy - 7), Color.white);
        }

        private static void StarLamp(PixelCanvas c, Color tint, int f)
        {
            Shadow(c, 16f, f + 29, 7f, 2f);

            c.Ellipse(16f, f + 26.5f, 6f, 1.8f, Tones.Shift(tint, -2));
            c.HLine(12, 19, f + 26, Tones.Shift(tint, -1));
            c.Rect(15, 20, 2, f + 6, Brass);
            c.VLine(15, 20, f + 25, Tones.Shift(Brass, 1));

            Star(c, 16f, 12.5f, 11f, 4.8f, Tones.Shift(tint, -1));
            Star(c, 16f, 12.5f, 9f, 3.9f, tint);
            Star(c, 15.5f, 12f, 5.5f, 2.6f, Tones.Shift(tint, 2));
            c.Set(15, 11, Paper);
            c.Set(16, 12, Paper);

            c.Outline(Ink, 0.45f);

            Glow(c, 16f, 12.5f, 15f, 14f, Tones.Shift(tint, 2), 0.12f);
            Sparkle(c, 4, 4, Paper);
            Sparkle(c, 28, 7, Paper);
            Sparkle(c, 27, 22, Tones.Shift(tint, 2));
        }

        private static void Star(PixelCanvas c, float cx, float cy, float outer, float inner, Color colour)
        {
            var x0 = Mathf.FloorToInt(cx - outer);
            var x1 = Mathf.CeilToInt(cx + outer);
            var y0 = Mathf.FloorToInt(cy - outer);
            var y1 = Mathf.CeilToInt(cy + outer);

            for (var y = y0; y <= y1; y++)
            {
                for (var x = x0; x <= x1; x++)
                {
                    var px = x + 0.5f - cx;
                    var py = y + 0.5f - cy;
                    var angle = Mathf.Atan2(-py, px) - Mathf.PI * 0.5f;
                    var segment = Mathf.PI * 2f / 5f;
                    var local = Mathf.Repeat(angle, segment) / segment;
                    var t = local < 0.5f ? local * 2f : (1f - local) * 2f;
                    var radius = Mathf.Lerp(outer, inner, t);
                    if (px * px + py * py <= radius * radius) c.Set(x, y, colour);
                }
            }
        }

        private static void Planetarium(PixelCanvas c, Color tint, int f)
        {
            Shadow(c, 16f, f + 29, 10f, 2.5f);

            var wood = Palette.Hex("#7A5244");
            c.Ellipse(16f, f + 27.5f, 6f, 1.5f, Tones.Shift(wood, -1));
            c.Rect(14, f + 15, 4, 12, wood);
            c.VLine(14, f + 15, f + 26, Tones.Shift(wood, 1));
            c.Ellipse(16f, f + 13f, 12f, 3.6f, Tones.Shift(wood, -1));
            c.Ellipse(16f, f + 12f, 12f, 3.4f, Tones.Shift(wood, 1));
            c.HLine(6, 25, f + 14, wood);

            // An orrery: a pale sun, rings of wire, and planets on them.
            c.Ellipse(16f, f + 11f, 4f, 1.2f, Brass);
            c.VLine(16, f - 4, f + 10, Brass);
            c.Ellipse(16f, f - 6f, 3.6f, 3.6f, Palette.Hex("#F2DC9A"));
            c.Ellipse(15f, f - 7f, 1.6f, 1.6f, Paper);

            for (var a = 0; a < 80; a++)
            {
                var t = a / 80f * Mathf.PI * 2f;
                c.Set(Mathf.RoundToInt(16f + Mathf.Cos(t) * 12f - 0.5f),
                    Mathf.RoundToInt(f - 4f + Mathf.Sin(t) * 3.4f), Tones.Shift(Brass, -1));
            }

            c.Ellipse(5f, f - 3f, 2.6f, 2.6f, tint);
            c.Ellipse(4.5f, f - 3.5f, 1.2f, 1.2f, Tones.Shift(tint, 2));
            c.HLine(1, 9, f - 3, Tones.Shift(Brass, 1));
            c.Ellipse(26f, f - 6f, 1.8f, 1.8f, Palette.Hex("#D98068"));
            c.Ellipse(21f, f - 1f, 1.3f, 1.3f, Palette.Hex("#9DC8B4"));

            c.Outline(Ink, 0.45f);

            Sparkle(c, 6, 3, Paper);
            Sparkle(c, 25, 2, Tones.Shift(tint, 2));
            c.Set(12, 1, Paper);
        }

        private static void Poster(PixelCanvas c, Color tint, int f)
        {
            Shadow(c, 16f, f + 29, 11f, 2.5f);

            var wood = DarkWood;
            ThickLine(c, 9, 8, 6, f + 28, Tones.Shift(wood, -1));
            ThickLine(c, 22, 8, 25, f + 28, Tones.Shift(wood, -1));
            c.HLine(8, 23, f + 22, wood);

            c.Rect(5, 2, 22, f + 19, wood);
            c.HLine(5, 26, 2, Tones.Shift(wood, 1));

            // The poster: a sunburst, a sun, a cat sitting in front of it.
            var x0 = 7;
            var y0 = 4;
            var pw = 18;
            var ph = f + 15;
            c.Rect(x0, y0, pw, ph, tint);
            var cx = x0 + pw / 2f;
            var cy = y0 + ph * 0.62f;
            for (var y = y0; y < y0 + ph; y++)
            {
                for (var x = x0; x < x0 + pw; x++)
                {
                    var angle = Mathf.Atan2(y + 0.5f - cy, x + 0.5f - cx);
                    if (Mathf.Repeat(angle * 8f / Mathf.PI, 2f) < 1f) c.Set(x, y, Tones.Shift(tint, 1));
                }
            }

            c.Ellipse(cx, cy, 6f, 6f, Palette.Hex("#C9533F"));
            c.Ellipse(cx - 1.5f, cy - 1.5f, 3f, 3f, Palette.Hex("#DE7A5E"));

            var teal = Palette.Hex("#3F7D7B");
            c.Rect(x0, y0 + ph - 7, pw, 7, teal);
            for (var x = x0 + 2; x < x0 + pw - 2; x += 3) c.Set(x, y0 + ph - 3, Cream);

            // Cat silhouette.
            var catX = Mathf.RoundToInt(cx) - 1;
            var catY = y0 + ph - 8;
            c.Ellipse(catX + 0.5f, catY - 1f, 3f, 3.5f, Ink);
            c.Ellipse(catX + 0.5f, catY - 6f, 2.2f, 2f, Ink);
            c.Set(catX - 1, catY - 9, Ink);
            c.Set(catX + 2, catY - 9, Ink);
            c.Line(catX + 3, catY + 1, catX + 5, catY - 3, Ink);

            c.Outline(Ink);
        }

        private static void Jukebox(PixelCanvas c, Color tint, int f)
        {
            Shadow(c, 16f, f + 29, 12f, 3f);

            var body = tint;
            c.Ellipse(16f, 11f, 12.5f, 10f, body);
            c.Rect(4, 11, 25, f + 16, body);
            c.VLine(28, 11, f + 26, Tones.Shift(body, -1));
            c.VLine(4, 11, f + 26, Tones.Shift(body, 1));
            c.HLine(4, 28, f + 26, Tones.Shift(body, -2));

            // Bands of coloured light round the arch.
            var bands = new[]
            {
                Palette.Hex("#E39AA6"), Palette.Hex("#F2DE9C"), Palette.Hex("#A8D8B9"),
                Palette.Hex("#9CC3E0"),
            };

            for (var i = 0; i < bands.Length; i++)
            {
                var r = 10.5f - i * 1.2f;
                for (var a = 0; a <= 60; a++)
                {
                    var t = Mathf.PI * a / 60f;
                    c.Set(Mathf.RoundToInt(16f + Mathf.Cos(t) * r - 0.5f),
                        Mathf.RoundToInt(11f - Mathf.Sin(t) * r * 0.82f), bands[i]);
                }
            }

            // The record window and a record in it.
            c.Rect(9, 10, 14, 8, Palette.Hex("#3B3346"));
            c.Ellipse(16f, 14.5f, 5f, 2.2f, Palette.Hex("#26222C"));
            c.HLine(13, 18, 14, Palette.Hex("#47404F"));
            c.Rect(15, 14, 2, 1, Berry);
            c.HLine(9, 22, 10, Tones.Alpha(Color.white, 0.5f));

            for (var x = 9; x < 23; x += 3) c.Rect(x, 20, 2, 1, Cream);

            // Grille.
            c.Rect(8, 23, 16, f + 1, Tones.Shift(body, -1));
            for (var x = 9; x < 23; x += 2) c.VLine(x, 24, f + 22, Tones.Shift(body, -2));
            c.HLine(8, 23, 22, Tones.Shift(Paper, -1));
            c.HLine(8, 23, f + 24, Tones.Shift(Paper, -1));

            c.Rect(5, f + 27, 4, 2, Iron);
            c.Rect(24, f + 27, 4, 2, Iron);

            c.Outline(Ink);

            Glow(c, 16f, 8f, 13f, 9f, bands[1], 0.12f);
        }

        // -------------------------------------------------------------------------------
        // Fixtures
        // -------------------------------------------------------------------------------

        private static void Door(PixelCanvas c, Color tint, int f)
        {
            // Doormat first, then the door standing behind it.
            var mat = Palette.Hex("#B5715E");
            c.RoundRect(4, f + 22, 24, 8, mat, 1);
            c.HLine(6, 25, f + 24, Tones.Shift(mat, 1));
            c.HLine(6, 25, f + 27, Tones.Shift(mat, -1));
            Paw(c, 16, f + 25, Cream);

            var frame = Tones.Shift(tint, -1);
            c.Rect(3, 1, 26, f + 22, frame);
            c.Ellipse(16f, 4f, 13f, 4f, frame);
            c.Rect(5, 3, 22, f + 19, tint);
            c.Ellipse(16f, 5f, 11f, 3f, tint);
            for (var x = 9; x < 27; x += 5) c.VLine(x, 3, f + 21, Tones.Shift(tint, -1));
            c.VLine(5, 3, f + 21, Tones.Shift(tint, 1));

            // A little arched window with warm light behind it.
            c.Rect(10, 7, 12, 8, Tones.Shift(tint, -2));
            c.Ellipse(16f, 7.5f, 6f, 3f, Tones.Shift(tint, -2));
            c.Rect(11, 8, 10, 6, Palette.Hex("#F5D9A0"));
            c.Ellipse(16f, 8f, 5f, 2f, Palette.Hex("#F5D9A0"));
            c.VLine(16, 6, 13, Tones.Shift(tint, -2));
            c.HLine(11, 20, 10, Tones.Shift(tint, -2));

            // Knob and a cat-shaped sign on a string.
            c.Rect(22, f + 5, 2, 2, Brass);
            c.Set(22, f + 5, Paper);
            c.Line(13, 17, 16, 15, Tones.Shift(tint, -2));
            c.Line(19, 17, 16, 15, Tones.Shift(tint, -2));
            c.RoundRect(11, 18, 10, 7, Cream, 1);
            c.Set(11, 17, Cream);
            c.Set(12, 17, Cream);
            c.Set(19, 17, Cream);
            c.Set(20, 17, Cream);
            c.Set(14, 21, Ink);
            c.Set(17, 21, Ink);
            c.Set(15, 22, Palette.InnerEar);
            c.Set(16, 22, Palette.InnerEar);

            c.Outline(Ink);
        }

        private static void Paw(PixelCanvas c, int cx, int cy, Color colour)
        {
            c.Rect(cx - 1, cy, 3, 2, colour);
            c.Set(cx - 2, cy - 1, colour);
            c.Set(cx, cy - 2, colour);
            c.Set(cx + 2, cy - 1, colour);
        }

        /// <summary>A plain wooden crate, for anything the catalog has that this file does not.</summary>
        private static void Crate(PixelCanvas c, Color tint, int f)
        {
            var width = c.Width;
            var height = c.Height;
            Shadow(c, width * 0.5f, height - 3, width * 0.4f, 3f);

            c.Rect(3, f - 2, width - 6, 8, Tones.Shift(tint, 1));
            c.Rect(3, f + 6, width - 6, height - f - 9, tint);
            for (var y = f + 10; y < height - 4; y += 6) c.HLine(4, width - 5, y, Tones.Shift(tint, -1));
            c.Line(4, f + 7, width - 5, height - 5, Tones.Shift(tint, -1));
            c.VLine(width - 4, f + 6, height - 4, Tones.Shift(tint, -1));

            c.Outline(Ink);
        }

        // -------------------------------------------------------------------------------
        // Shared strokes
        // -------------------------------------------------------------------------------

        private static void Shadow(PixelCanvas c, float cx, float cy, float rx, float ry)
        {
            c.Ellipse(cx, cy, rx, ry, Tones.Alpha(Ink, 0.24f));
        }

        private static void ThickLine(PixelCanvas c, int x0, int y0, int x1, int y1, Color colour)
        {
            c.Line(x0, y0, x1, y1, colour);
            c.Line(x0 + 1, y0, x1 + 1, y1, Tones.Shift(colour, -1));
        }

        /// <summary>Short streaks of the base tone across a lit surface. Wood, at thirty pixels.</summary>
        private static void Grain(PixelCanvas c, int x, int y, int width, int height, Color tint, int seed)
        {
            for (var j = 1; j < height; j += 2)
            {
                var dash = 0;
                for (var k = 0; k < width; k += 6 + dash)
                {
                    dash = Hash(j + seed, k) % 5;
                    var start = x + k + Hash(k, j * 3 + seed) % 4;
                    var length = 2 + dash;
                    for (var i = start; i < start + length && i < x + width; i++)
                    {
                        c.SetOnSolid(i, y + j, tint);
                    }
                }
            }
        }

        private static void Steam(PixelCanvas c, int x, int y)
        {
            var puff = Tones.Alpha(Paper, 0.55f);
            c.Ellipse(x, y, 2.2f, 1.8f, puff);
            c.Ellipse(x + 2f, y - 4f, 1.8f, 1.6f, Tones.Alpha(Paper, 0.4f));
            c.Ellipse(x - 0.5f, y - 7.5f, 1.4f, 1.2f, Tones.Alpha(Paper, 0.28f));
        }

        private static void Glow(PixelCanvas c, float cx, float cy, float rx, float ry, Color colour,
            float alpha)
        {
            c.Ellipse(cx, cy, rx, ry, Tones.Alpha(colour, alpha * 0.5f));
            c.Ellipse(cx, cy, rx * 0.6f, ry * 0.6f, Tones.Alpha(colour, alpha * 0.5f));
        }

        private static void Sparkle(PixelCanvas c, int x, int y, Color colour)
        {
            c.Set(x, y, colour);
            c.Set(x - 1, y, Tones.Alpha(colour, 0.6f));
            c.Set(x + 1, y, Tones.Alpha(colour, 0.6f));
            c.Set(x, y - 1, Tones.Alpha(colour, 0.6f));
            c.Set(x, y + 1, Tones.Alpha(colour, 0.6f));
        }

        private static int Hash(int a, int b)
        {
            unchecked
            {
                var h = a * 73856093 ^ b * 19349663;
                h ^= h >> 13;
                h *= 0x5bd1e995;
                h ^= h >> 15;
                return h & 0x7fffffff;
            }
        }
    }
}
