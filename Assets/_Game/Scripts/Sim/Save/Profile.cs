using System.Collections.Generic;
using Neko.Sim.Cafe;
using Neko.Sim.Core;
using Neko.Sim.Economy;
using Neko.Sim.Maids;
using Neko.Sim.Progress;
using Neko.Sim.Team;

namespace Neko.Sim.Save
{
    /// <summary>Bests worth keeping across businesses. They survive a bankruptcy.</summary>
    public sealed class Records
    {
        public int BestDay;
        public int BestRevenue;
        public int TotalCustomersServed;
        public int TotalDaysPlayed;
        public int BusinessesLost;
        public int LegendariesFound;
    }

    /// <summary>
    /// One business. The thing that gets saved, and the thing that goes under.
    /// </summary>
    /// <remarks>
    /// Café level is a property of reputation rather than a stored field, so the two can never
    /// disagree — a saved level that had drifted from the reputation that earned it would hand
    /// somebody a room they cannot fill or take away one they had.
    /// </remarks>
    public sealed class Profile
    {
        /// <summary>
        /// Starting money.
        /// </summary>
        /// <remarks>
        /// About four days of fixed costs for the opening squad. Enough to survive a couple of
        /// bad lineups and learn something, not enough to ignore the books.
        /// </remarks>
        public const int StartingMoney = 600;

        /// <summary>Enough for one box on day one, so the shop is not a locked door.</summary>
        public const int StartingPoints = 40;

        public uint Seed = 1u;

        /// <summary>Days survived. There is no quota and no step: there is only the balance.</summary>
        public int Day = 1;

        public readonly Wallet Wallet = new Wallet(StartingMoney, 0, StartingPoints);

        public readonly List<Maid> Maids = new List<Maid>();

        /// <summary>Bought but not placed. The salão screen takes from here.</summary>
        public readonly List<string> OwnedPlaceables = new List<string>();

        /// <summary>Bought but not worn.</summary>
        public readonly List<string> OwnedEquipment = new List<string>();

        public CafeGrid Grid = CafeGrid.StartingRoom();

        /// <summary>Who plays where. The player's entire turn lives in this object.</summary>
        public Lineup Lineup = new Lineup(Team.Lineup.SlotsFor(1));

        public readonly Records Records = new Records();

        /// <summary>The business failed. The save is kept for the records and nothing else.</summary>
        public bool Bankrupt;

        public int NextMaidId = 1;

        public int CafeLevel => CafeLevelTable.LevelFor(Wallet.Reputation);

        public CafeUnlocks Unlocks => CafeLevelTable.UnlocksAt(CafeLevel);

        /// <summary>The theme the room is decorated in. Maids who match it gain chemistry.</summary>
        public ThemeTag CafeTheme
        {
            get
            {
                var metrics = Grid.Metrics();
                return metrics.ThemeBonusActive ? metrics.DominantTheme : ThemeTag.None;
            }
        }

        public static Profile NewGame(uint seed)
        {
            var profile = new Profile { Seed = seed };
            var rng = new Rng(seed == 0 ? 1u : seed);

            // Three cards to open with, one for each position the first café has. Rolled
            // rather than fixed, so no two businesses start the same.
            profile.Add(Opener(profile, Role.Reception, rng));
            profile.Add(Opener(profile, Role.Floor, rng));
            profile.Add(Opener(profile, Role.Kitchen, rng));

            profile.Lineup.Rebuild(Team.Lineup.SlotsFor(profile.CafeLevel));
            profile.Lineup.AutoFill(profile.Maids);
            return profile;
        }

        /// <summary>A starting card guaranteed to belong in the position it is rolled for.</summary>
        private static Maid Opener(Profile profile, Role role, Rng rng)
        {
            var candidates = new List<Archetype>();
            foreach (var archetype in ArchetypeTable.All)
            {
                if (RoleTable.NaturalFor(archetype.Kind) == role) candidates.Add(archetype.Kind);
            }

            var kind = candidates.Count > 0 ? rng.Pick(candidates) : Archetype.Genki;
            var maid = Lootbox.Mint(profile, Rarity.Common, rng);
            maid.Archetype = kind;
            maid.NaturalRole = role;
            maid.Base = RarityTable.RollStats(Rarity.Common, kind, rng);
            return maid;
        }

        public Maid MaidById(int id)
        {
            for (var i = 0; i < Maids.Count; i++)
            {
                if (Maids[i].Id == id) return Maids[i];
            }

            return null;
        }

        /// <summary>The maids standing in a slot today, in lineup order.</summary>
        public IReadOnlyList<Maid> FieldedMaids()
        {
            var fielded = new List<Maid>();
            foreach (var id in Lineup.MaidIds())
            {
                var maid = MaidById(id);
                if (maid != null) fielded.Add(maid);
            }

            return fielded;
        }

        public TeamChemistry Chemistry() =>
            Team.Chemistry.Evaluate(Lineup, Maids, CafeTheme);

        public void Add(Maid maid)
        {
            if (maid == null) return;

            Maids.Add(maid);
            MaidProgression.RefreshSetBonus(maid);

            if (maid.Rarity == Rarity.Legendary) Records.LegendariesFound++;
        }

        /// <summary>
        /// Lets a maid go, for part of what she is worth in fichas.
        /// </summary>
        /// <remarks>
        /// The one move that always cuts the wage bill. Without it a player who over-hired
        /// would be watching a countdown with nothing to do about it, which is a dead end with
        /// extra steps.
        /// </remarks>
        public bool Dismiss(int maidId)
        {
            var maid = MaidById(maidId);
            if (maid == null) return false;

            Lineup.Remove(maidId);
            Maids.Remove(maid);
            Wallet.AddPoints(maid.Severance);
            return true;
        }

        /// <summary>
        /// Brings the room and the lineup up to what the café level has earned.
        /// </summary>
        public void ApplyUnlocks()
        {
            var unlocks = Unlocks;

            if (Grid.Width < unlocks.GridWidth || Grid.Height < unlocks.GridHeight)
            {
                Grid.Resize(unlocks.GridWidth, unlocks.GridHeight);
            }

            Lineup.Rebuild(Team.Lineup.SlotsFor(CafeLevel));

            // A maid who was let go must not keep standing in a slot.
            foreach (var slot in Lineup.All)
            {
                if (!slot.Empty && MaidById(slot.MaidId) == null) slot.MaidId = -1;
            }
        }
    }
}
