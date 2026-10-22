using Neko.Sim.Cafe;

namespace Neko.Sim.Service
{
    /// <summary>One dish. Time at a station, money at the table.</summary>
    public sealed class Recipe
    {
        public readonly string Id;
        public readonly string DisplayName;

        /// <summary>Seconds at a station for a maid with an average Cook stat.</summary>
        public readonly float Seconds;

        public readonly int Price;
        public readonly StationKind Station;
        public readonly int UnlockCafeLevel;

        /// <summary>What the view draws on the plate.</summary>
        public readonly string ShapeId;

        public readonly string TintHex;

        public Recipe(string id, string displayName, float seconds, int price, StationKind station,
            int unlockCafeLevel, string shapeId, string tintHex)
        {
            Id = id;
            DisplayName = displayName;
            Seconds = seconds;
            Price = price;
            Station = station;
            UnlockCafeLevel = unlockCafeLevel;
            ShapeId = shapeId;
            TintHex = tintHex;
        }
    }
}
