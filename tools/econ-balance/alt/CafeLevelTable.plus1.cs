using System.Collections.Generic;
using Neko.Sim.Cafe;
using Neko.Sim.Core;
using Neko.Sim.Progress;
using Neko.Sim.Service;
using Neko.Sim.Team;

namespace Neko.Sim.Economy
{
    /// <summary>What a café level is worth.</summary>
    public readonly struct CafeUnlocks
    {
        public readonly int Level;
        public readonly int GridWidth;
        public readonly int GridHeight;
        public readonly int RosterCap;
        public readonly int ShiftSlots;

        public CafeUnlocks(int level, int gridWidth, int gridHeight, int rosterCap, int shiftSlots)
        {
            Level = level;
            GridWidth = gridWidth;
            GridHeight = gridHeight;
            RosterCap = rosterCap;
            ShiftSlots = shiftSlots;
        }

        /// <summary>Recipes, furniture and clothes that arrive exactly at this level.</summary>
        public IReadOnlyList<string> NewThisLevel()
        {
            var names = new List<string>();

            foreach (var recipe in RecipeBook.All)
            {
                if (recipe.UnlockCafeLevel == Level) names.Add(recipe.DisplayName);
            }

            foreach (var def in PlaceableCatalog.All)
            {
                if (def.UnlockCafeLevel == Level && def.Kind != PlaceableKind.Fixture)
                {
                    names.Add(def.DisplayName);
                }
            }

            foreach (var item in EquipmentCatalog.All)
            {
                if (item.UnlockCafeLevel == Level) names.Add(item.DisplayName);
            }

            return names;
        }
    }

    /// <summary>
    /// Ten levels, each of which has to be worth reaching.
    /// </summary>
    /// <remarks>
    /// A test walks every level and asserts it brings something — a bigger room, another maid
    /// on the roster, or new things to cook, build and wear. A level that unlocked nothing
    /// would be a progress bar filling for its own sake.
    /// </remarks>
    public static class CafeLevelTable
    {
        public const int MaxLevel = 10;

        /// <summary>Reputation needed to reach each level. Index is the level.</summary>
        private static readonly int[] Thresholds =
            { 0, 0, 60, 160, 320, 560, 900, 1350, 1950, 2700, 3700 };

        private static readonly CafeUnlocks[] Levels =
        {
            default,
            CreateUnlocks(1, 7, 5, 2),
            CreateUnlocks(2, 8, 6, 3),
            CreateUnlocks(3, 9, 6, 3),
            CreateUnlocks(4, 10, 7, 4),
            CreateUnlocks(5, 11, 7, 4),
            CreateUnlocks(6, 11, 8, 5),
            CreateUnlocks(7, 12, 8, 5),
            CreateUnlocks(8, 12, 9, 6),
            CreateUnlocks(9, 13, 9, 6),
            CreateUnlocks(10, 13, 9, 6),
        };

        private static CafeUnlocks CreateUnlocks(int level, int width, int height, int shiftSlots) =>
            new CafeUnlocks(level, width, height, Lineup.SlotsFor(level).Total + 1, shiftSlots);

        public static int LevelFor(int reputation)
        {
            var level = 1;
            for (var i = 1; i <= MaxLevel; i++)
            {
                if (reputation >= Thresholds[i]) level = i;
            }

            return level;
        }

        public static int ReputationForLevel(int level) =>
            Thresholds[Curves.ClampInt(level, 1, MaxLevel)];

        /// <summary>Reputation still to go, or 0 at the cap.</summary>
        public static int ToNextLevel(int reputation)
        {
            var level = LevelFor(reputation);
            return level >= MaxLevel ? 0 : Thresholds[level + 1] - reputation;
        }

        public static CafeUnlocks UnlocksAt(int level) =>
            Levels[Curves.ClampInt(level, 1, MaxLevel)];
    }
}
