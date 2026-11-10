using Neko.Sim.Cafe;

namespace Neko.Game.Art
{
    /// <summary>
    /// The placeables that have illustrated art, and how that art sits on the grid.
    /// </summary>
    /// <remarks>
    /// The procedural drawing in <see cref="FurnitureArt"/> stays the fallback: a piece is drawn
    /// from here only when it has an entry, its footprint is the one the art was made for, and
    /// its PNG loads. Anything else, including a deleted file, quietly draws as before.
    ///
    /// Every entry is calibrated from the illustrated area itself (the PNGs are cropped to it
    /// by <c>tools/art/furniture_pilot/make_pilot_assets.py</c>), never from a generation canvas:
    /// <see cref="WidthUnits"/> is how wide that area is in tiles, and the sprite's pivot is the
    /// bottom centre of the art. The art sits with its bottom on the footprint's front edge,
    /// raised by <see cref="BottomLift"/>, so it rises through the rows behind it the way the
    /// procedural pieces do, and <see cref="View.DepthSort"/> keys it by that same edge.
    /// </remarks>
    public sealed class IllustratedFurniture
    {
        private IllustratedFurniture(string id, int footprintWidth, int footprintHeight,
            string resource, string raisedResource, int pixelWidth, int pixelHeight,
            float widthUnits, float bottomLift)
        {
            Id = id;
            FootprintWidth = footprintWidth;
            FootprintHeight = footprintHeight;
            Resource = resource;
            RaisedResource = raisedResource;
            PixelWidth = pixelWidth;
            PixelHeight = pixelHeight;
            WidthUnits = widthUnits;
            BottomLift = bottomLift;
        }

        public string Id { get; }

        /// <summary>The footprint the art was made for. A rotated piece falls back to procedural.</summary>
        public int FootprintWidth { get; }

        public int FootprintHeight { get; }

        /// <summary>The whole picture, or the flat base when <see cref="RaisedResource"/> is set.</summary>
        public string Resource { get; }

        /// <summary>
        /// Optional second picture, same size and same pivot, holding only what stands up: it is
        /// drawn with the solids while <see cref="Resource"/> lies flat beneath everyone.
        /// </summary>
        public string RaisedResource { get; }

        /// <summary>Size of the PNG, so a platform that pads textures cannot change the maths.</summary>
        public int PixelWidth { get; }

        public int PixelHeight { get; }

        /// <summary>Width of the illustrated area in tiles; the pixels per unit follow from it.</summary>
        public float WidthUnits { get; }

        /// <summary>How far above the footprint's front edge the bottom of the art sits.</summary>
        public float BottomLift { get; }

        public float PixelsPerUnit => PixelWidth / WidthUnits;

        public bool HasRaisedLayer => !string.IsNullOrEmpty(RaisedResource);

        private static readonly IllustratedFurniture[] Entries =
        {
            // The whole platform is solid and fills its 2x2: 2.08 tiles wide, 2.09 tall.
            new IllustratedFurniture("stage", 2, 2, "Furniture/stage", null, 532, 534, 2.08f, 0f),

            // Water lies flat, rocks, lantern and spout stand. The two PNGs are cut from one
            // picture so they line up to the pixel. 1.62 tall in a 2-row footprint: centred.
            new IllustratedFurniture("koi_pond", 2, 2, "Furniture/koi_pond",
                "Furniture/koi_pond_raised", 543, 416, 2.12f, 0.2f),

            // A free-standing frame on feet. The feet touch the floor a little inside the cell.
            new IllustratedFurniture("window", 1, 1, "Furniture/window", null, 236, 412, 0.92f,
                0.06f),
        };

        /// <summary>
        /// Switches every illustrated piece back to its procedural drawing. For visual comparison
        /// and as a kill switch; nothing sets it in the game.
        /// </summary>
        public static bool Enabled { get; set; } = true;

        /// <summary>The entry for this piece at this footprint, or null for procedural art.</summary>
        public static IllustratedFurniture For(PlaceableDef def, int widthTiles, int heightTiles)
        {
            if (def == null || !Enabled) return null;

            for (var i = 0; i < Entries.Length; i++)
            {
                var entry = Entries[i];
                if (entry.Id == def.Id && entry.FootprintWidth == widthTiles &&
                    entry.FootprintHeight == heightTiles)
                {
                    return entry;
                }
            }

            return null;
        }
    }
}
