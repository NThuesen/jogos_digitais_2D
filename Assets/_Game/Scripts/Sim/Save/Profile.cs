using System.Collections.Generic;
using Neko.Sim.Cafe;
using Neko.Sim.Economy;
using Neko.Sim.Maids;
using Neko.Sim.Progress;

namespace Neko.Sim.Save
{
    /// <summary>Bests worth keeping across runs. Shown on the menu.</summary>
    public sealed class Records
    {
        public int BestDay;
        public int BestEarnings;
        public int BestEndlessDay;
        public int TotalCustomersServed;
        public int TotalDaysPlayed;
    }

    /// <summary>
    /// Everything one player has. The thing that gets saved.
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
        /// Deliberately below the price of a second stove and above the price of a table: the
        /// first purchase has to be a choice. At 250 every single thing on sale at café level
        /// one was affordable, which a test caught and which would have made day one a
        /// shopping trip rather than a decision.
        /// </remarks>
        public const int StartingMoney = 150;

        public uint Seed = 1u;

        /// <summary>Days played. Always advances, even after a slow day.</summary>
        public int Day = 1;

        /// <summary>
        /// The step the quota curve is on.
        /// </summary>
        /// <remarks>
        /// Separate from <see cref="Day"/> because a missed quota must not make tomorrow
        /// harder. A player who is behind can replay the same difficulty until they are not,
        /// which is the promise that there is always a way forward.
        /// </remarks>
        public int QuotaStep = 1;

        public readonly Wallet Wallet = new Wallet(StartingMoney);

        public readonly List<Maid> Maids = new List<Maid>();

        /// <summary>Ids of the maids working today, in order.</summary>
        public readonly List<int> ShiftRoster = new List<int>();

        /// <summary>Bought but not placed. The editor takes from here.</summary>
        public readonly List<string> OwnedPlaceables = new List<string>();

        /// <summary>Bought but not worn.</summary>
        public readonly List<string> OwnedEquipment = new List<string>();

        public CafeGrid Grid = CafeGrid.StartingRoom();

        public readonly Records Records = new Records();

        public bool EndlessUnlocked;

        public int NextMaidId = 1;

        public int CafeLevel => CafeLevelTable.LevelFor(Wallet.Reputation);

        public CafeUnlocks Unlocks => CafeLevelTable.UnlocksAt(CafeLevel);

        public static Profile NewGame(uint seed)
        {
            var profile = new Profile { Seed = seed };

            // Two maids to start: one for the floor, one for the kitchen, so the split the
            // whole roster system is built on is legible on day one.
            profile.Add(Maid.Create(profile.NextMaidId++, "Mina", Archetype.Genki));
            profile.Add(Maid.Create(profile.NextMaidId++, "Yuki", Archetype.Mukuchi));

            return profile;
        }

        public Maid MaidById(int id)
        {
            for (var i = 0; i < Maids.Count; i++)
            {
                if (Maids[i].Id == id) return Maids[i];
            }

            return null;
        }

        public IReadOnlyList<Maid> RosteredMaids()
        {
            var roster = new List<Maid>();
            for (var i = 0; i < ShiftRoster.Count; i++)
            {
                var maid = MaidById(ShiftRoster[i]);
                if (maid != null) roster.Add(maid);
            }

            return roster;
        }

        /// <summary>Adds a maid and puts her to work if there is room on today's shift.</summary>
        public void Add(Maid maid)
        {
            if (maid == null) return;

            Maids.Add(maid);
            MaidProgression.RefreshSetBonus(maid);

            if (ShiftRoster.Count < Unlocks.ShiftSlots) ShiftRoster.Add(maid.Id);
        }

        public bool SetRostered(int maidId, bool rostered)
        {
            if (MaidById(maidId) == null) return false;

            if (!rostered) return ShiftRoster.Remove(maidId);
            if (ShiftRoster.Contains(maidId)) return true;
            if (ShiftRoster.Count >= Unlocks.ShiftSlots) return false;

            ShiftRoster.Add(maidId);
            return true;
        }

        /// <summary>
        /// Brings the room up to the size this café level has earned, and trims a roster that
        /// a shrunken save would have left too long.
        /// </summary>
        public void ApplyUnlocks()
        {
            var unlocks = Unlocks;

            if (Grid.Width < unlocks.GridWidth || Grid.Height < unlocks.GridHeight)
            {
                Grid.Resize(unlocks.GridWidth, unlocks.GridHeight);
            }

            while (ShiftRoster.Count > unlocks.ShiftSlots)
            {
                ShiftRoster.RemoveAt(ShiftRoster.Count - 1);
            }
        }
    }
}
