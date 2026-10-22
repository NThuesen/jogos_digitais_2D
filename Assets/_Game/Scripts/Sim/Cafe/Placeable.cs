using Neko.Sim.Core;

namespace Neko.Sim.Cafe
{
    public enum PlaceableKind
    {
        Table = 0,
        Station,
        Decor,
        Fixture,
    }

    public enum StationKind
    {
        Stove = 0,
        Dessert,
        TeaBar,
        Bakery,
    }

    /// <summary>One thing that can stand on the café floor.</summary>
    public sealed class PlaceableDef
    {
        public readonly string Id;
        public readonly string DisplayName;
        public readonly PlaceableKind Kind;
        public readonly int Width;
        public readonly int Height;

        /// <summary>Seats, for a table. Each seat is one customer.</summary>
        public readonly int Seats;

        /// <summary>What it cooks, for a station.</summary>
        public readonly StationKind? Station;

        public readonly float Ambience;
        public readonly ThemeTag Theme;
        public readonly int Price;
        public readonly int UnlockCafeLevel;

        /// <summary>What the view draws it as. Lives here so there is one table, not two.</summary>
        public readonly string ShapeId;

        public readonly string TintHex;

        public PlaceableDef(string id, string displayName, PlaceableKind kind, int width, int height,
            string shapeId, string tintHex, int price, int unlockCafeLevel, int seats = 0,
            StationKind? station = null, float ambience = 0f, ThemeTag theme = ThemeTag.None)
        {
            Id = id;
            DisplayName = displayName;
            Kind = kind;
            Width = width;
            Height = height;
            ShapeId = shapeId;
            TintHex = tintHex;
            Price = price;
            UnlockCafeLevel = unlockCafeLevel;
            Seats = seats;
            Station = station;
            Ambience = ambience;
            Theme = theme;
        }
    }

    /// <summary>One <see cref="PlaceableDef"/> standing at a spot on the grid.</summary>
    public sealed class PlacedItem
    {
        public readonly PlaceableDef Def;
        public readonly int X;
        public readonly int Y;

        /// <summary>Quarter turn. All it does is swap the footprint, which is all it needs to do.</summary>
        public readonly bool Rotated;

        public PlacedItem(PlaceableDef def, int x, int y, bool rotated)
        {
            Def = def;
            X = x;
            Y = y;
            Rotated = rotated;
        }

        public int Width => Rotated ? Def.Height : Def.Width;

        public int Height => Rotated ? Def.Width : Def.Height;

        public bool Covers(int x, int y) =>
            x >= X && x < X + Width && y >= Y && y < Y + Height;
    }
}
