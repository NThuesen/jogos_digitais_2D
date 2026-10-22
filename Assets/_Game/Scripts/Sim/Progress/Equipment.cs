using Neko.Sim.Core;
using Neko.Sim.Maids;

namespace Neko.Sim.Progress
{
    public enum GearSlot
    {
        Outfit = 0,
        Headwear,
        Accessory,
        Shoes,
    }

    /// <summary>
    /// One wearable piece.
    /// </summary>
    /// <remarks>
    /// It carries its own drawing instructions — <see cref="LayerShapeId"/> and
    /// <see cref="TintHex"/> — rather than leaving the view to look them up in a second
    /// table. An upgrade the player cannot see is a bug, and one table is one place for that
    /// to go wrong instead of two.
    /// </remarks>
    public sealed class Equipment
    {
        public readonly string Id;
        public readonly string DisplayName;
        public readonly GearSlot Slot;
        public readonly Stats Bonus;
        public readonly ThemeTag Theme;
        public readonly int Price;
        public readonly int UnlockCafeLevel;
        public readonly string LayerShapeId;
        public readonly string TintHex;

        public Equipment(
            string id,
            string displayName,
            GearSlot slot,
            Stats bonus,
            ThemeTag theme,
            int price,
            int unlockCafeLevel,
            string layerShapeId,
            string tintHex)
        {
            Id = id;
            DisplayName = displayName;
            Slot = slot;
            Bonus = bonus;
            Theme = theme;
            Price = price;
            UnlockCafeLevel = unlockCafeLevel;
            LayerShapeId = layerShapeId;
            TintHex = tintHex;
        }
    }
}
