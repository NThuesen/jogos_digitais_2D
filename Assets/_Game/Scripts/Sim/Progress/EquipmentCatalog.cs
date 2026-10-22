using System;
using System.Collections.Generic;
using Neko.Sim.Core;
using Neko.Sim.Maids;

namespace Neko.Sim.Progress
{
    /// <summary>
    /// Everything a maid can wear.
    /// </summary>
    /// <remarks>
    /// Every piece carries a shape and a tint, because the design's first pillar is that an
    /// upgrade the player cannot see is a bug. Equipping something swaps a layer on her
    /// sprite; the view needs no second table to know what to draw.
    /// </remarks>
    public static class EquipmentCatalog
    {
        /// <summary>Pieces of one theme needed before the set starts paying.</summary>
        public const int SetBonusThreshold = 3;

        private static readonly Equipment[] Items =
        {
            // Outfit
            Make("outfit_basic", "Avental simples", GearSlot.Outfit, 1, 1, 1, 1, ThemeTag.None, 0, 1,
                "Apron", "#F2EDE4"),
            Make("outfit_sakura", "Vestido sakura", GearSlot.Outfit, 4, 0, 8, 0, ThemeTag.Sakura,
                320, 2, "Apron", "#F2B6C6"),
            Make("outfit_gothic", "Vestido gótico", GearSlot.Outfit, 0, 4, 8, 0, ThemeTag.Gothic,
                340, 3, "Apron", "#4E4458"),
            Make("outfit_chef", "Dólmã de chef", GearSlot.Outfit, 0, 10, 0, 2, ThemeTag.None, 380, 3,
                "Apron", "#FFFFFF"),
            Make("outfit_starry", "Vestido estelar", GearSlot.Outfit, 6, 0, 6, 2, ThemeTag.Starry,
                520, 5, "Apron", "#6C7FD1"),
            Make("outfit_showa", "Uniforme Shōwa", GearSlot.Outfit, 8, 2, 2, 2, ThemeTag.Showa,
                500, 5, "Apron", "#C77A3C"),

            // Headwear
            Make("head_band", "Tiara simples", GearSlot.Headwear, 0, 0, 3, 0, ThemeTag.None, 90, 1,
                "Bow", "#E8E2D6"),
            Make("head_sakura", "Flor de sakura", GearSlot.Headwear, 0, 0, 6, 0, ThemeTag.Sakura,
                210, 2, "Bow", "#E890AB"),
            Make("head_gothic", "Touca gótica", GearSlot.Headwear, 0, 3, 5, 0, ThemeTag.Gothic,
                230, 3, "Bow", "#6B5A78"),
            Make("head_chef", "Chapéu de chef", GearSlot.Headwear, 0, 7, 0, 0, ThemeTag.None, 260, 3,
                "Capsule", "#FFFFFF"),
            Make("head_starry", "Coroa estelar", GearSlot.Headwear, 2, 0, 7, 0, ThemeTag.Starry,
                400, 5, "Star", "#8FA8E8"),
            Make("head_showa", "Laço Shōwa", GearSlot.Headwear, 4, 0, 4, 0, ThemeTag.Showa, 380, 5,
                "Bow", "#D9B04A"),

            // Accessory
            Make("acc_apron", "Avental de babado", GearSlot.Accessory, 2, 2, 2, 0, ThemeTag.None,
                110, 1, "Ring", "#F5EFE2"),
            Make("acc_sakura", "Fita sakura", GearSlot.Accessory, 3, 0, 5, 0, ThemeTag.Sakura, 240,
                2, "Ring", "#F2B6C6"),
            Make("acc_gothic", "Camafeu", GearSlot.Accessory, 0, 0, 8, 0, ThemeTag.Gothic, 260, 3,
                "Ring", "#4E4458"),
            Make("acc_timer", "Cronômetro", GearSlot.Accessory, 0, 6, 0, 3, ThemeTag.None, 300, 4,
                "Circle", "#8C7B5A"),
            Make("acc_starry", "Broche estelar", GearSlot.Accessory, 0, 3, 6, 0, ThemeTag.Starry,
                420, 5, "Star", "#6C7FD1"),
            Make("acc_showa", "Rádio de bolso", GearSlot.Accessory, 5, 0, 3, 2, ThemeTag.Showa, 400,
                5, "RoundedRect", "#C77A3C"),

            // Shoes
            Make("shoes_flats", "Sapatilha", GearSlot.Shoes, 3, 0, 0, 2, ThemeTag.None, 100, 1,
                "Capsule", "#8C7B5A"),
            Make("shoes_sakura", "Geta de sakura", GearSlot.Shoes, 4, 0, 3, 2, ThemeTag.Sakura, 230,
                2, "Capsule", "#E890AB"),
            Make("shoes_gothic", "Botina", GearSlot.Shoes, 2, 0, 2, 6, ThemeTag.Gothic, 250, 3,
                "Capsule", "#4E4458"),
            Make("shoes_running", "Tênis", GearSlot.Shoes, 8, 0, 0, 4, ThemeTag.None, 320, 4,
                "Capsule", "#D9D4C8"),
            Make("shoes_starry", "Sandália estelar", GearSlot.Shoes, 5, 0, 4, 3, ThemeTag.Starry,
                430, 5, "Capsule", "#8FA8E8"),
            Make("shoes_showa", "Sapato Shōwa", GearSlot.Shoes, 6, 0, 2, 3, ThemeTag.Showa, 410, 5,
                "Capsule", "#D9B04A"),
        };

        public static IReadOnlyList<Equipment> All => Items;

        public static Equipment ById(string id)
        {
            for (var i = 0; i < Items.Length; i++)
            {
                if (Items[i].Id == id) return Items[i];
            }

            throw new KeyNotFoundException("No equipment called '" + id + "'.");
        }

        public static IReadOnlyList<Equipment> ForSlot(GearSlot slot, int cafeLevel)
        {
            var found = new List<Equipment>();
            for (var i = 0; i < Items.Length; i++)
            {
                if (Items[i].Slot == slot && Items[i].UnlockCafeLevel <= cafeLevel)
                {
                    found.Add(Items[i]);
                }
            }

            return found;
        }

        public static IReadOnlyList<Equipment> UnlockedAt(int cafeLevel)
        {
            var found = new List<Equipment>();
            for (var i = 0; i < Items.Length; i++)
            {
                if (Items[i].UnlockCafeLevel <= cafeLevel) found.Add(Items[i]);
            }

            return found;
        }

        /// <summary>
        /// What a committed outfit is worth: three matching pieces, and more for all four.
        /// </summary>
        public static Stats SetBonus(Maid maid)
        {
            if (maid == null) return Stats.Zero;

            var counts = new int[Enum.GetValues(typeof(ThemeTag)).Length];
            for (var i = 0; i < maid.Gear.Length; i++)
            {
                var piece = maid.Gear[i];
                if (piece != null && piece.Theme != ThemeTag.None) counts[(int)piece.Theme]++;
            }

            var best = 0;
            for (var i = 1; i < counts.Length; i++)
            {
                if (counts[i] > best) best = counts[i];
            }

            if (best < SetBonusThreshold) return Stats.Zero;
            return Stats.Uniform(best >= maid.Gear.Length ? 8 : 4);
        }

        /// <summary>The theme a maid is wearing enough of to count, or None.</summary>
        public static ThemeTag DominantTheme(Maid maid)
        {
            if (maid == null) return ThemeTag.None;

            var counts = new int[Enum.GetValues(typeof(ThemeTag)).Length];
            for (var i = 0; i < maid.Gear.Length; i++)
            {
                var piece = maid.Gear[i];
                if (piece != null && piece.Theme != ThemeTag.None) counts[(int)piece.Theme]++;
            }

            var best = ThemeTag.None;
            var bestCount = SetBonusThreshold - 1;
            for (var i = 1; i < counts.Length; i++)
            {
                if (counts[i] <= bestCount) continue;
                bestCount = counts[i];
                best = (ThemeTag)i;
            }

            return best;
        }

        private static Equipment Make(string id, string name, GearSlot slot, int serve, int cook,
            int charm, int stamina, ThemeTag theme, int price, int unlock, string shape,
            string tint) =>
            new Equipment(id, name, slot, new Stats(serve, cook, charm, stamina), theme, price,
                unlock, shape, tint);
    }
}
