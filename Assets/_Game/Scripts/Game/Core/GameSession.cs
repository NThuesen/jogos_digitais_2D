using System;
using Neko.Sim.Core;
using Neko.Sim.Economy;
using Neko.Sim.Progress;
using Neko.Sim.Save;
using Neko.Sim.Shift;

namespace Neko.Game.Core
{
    /// <summary>
    /// One player's run: the profile, the applicants, and the day turning over.
    /// </summary>
    /// <remarks>
    /// Sits between the screens and the simulation so that no screen ever mutates a profile
    /// directly. The rule that pays for itself is <see cref="Touch"/>: anything that changes
    /// the profile saves it, so a browser tab closed mid-shop does not cost the player the
    /// thing they just bought.
    /// </remarks>
    public sealed class GameSession
    {
        private readonly Rng _rng;

        public GameSession(Profile profile)
        {
            Profile = profile ?? Profile.NewGame(1u);
            _rng = new Rng(Profile.Seed == 0 ? 1u : Profile.Seed);
            Applicants = new ApplicantBoard();
            RefreshApplicants();
        }

        public Profile Profile { get; }

        public ApplicantBoard Applicants { get; }

        public ShiftResult LastResult { get; private set; }

        /// <summary>Levels gained by each maid on the last day, for the results screen.</summary>
        public System.Collections.Generic.Dictionary<int, int> LastLevelUps { get; } =
            new System.Collections.Generic.Dictionary<int, int>();

        public int LastCafeLevel { get; private set; }

        public bool CafeLevelledUp { get; private set; }

        public event Action Changed;

        public static GameSession Load()
        {
            var result = SaveBridge.Load();
            return new GameSession(result.Profile);
        }

        public Sim.Shift.Shift StartDay()
        {
            LastCafeLevel = Profile.CafeLevel;
            var setup = DayPlan.Build(Profile, _rng.NextUInt());
            return new Sim.Shift.Shift(setup);
        }

        /// <summary>
        /// Folds a finished day into the profile and saves.
        /// </summary>
        /// <remarks>
        /// Level-ups are recorded before the result is folded in, because the results screen
        /// wants to say which maid levelled and the profile no longer knows once it has.
        /// </remarks>
        public void FinishDay(ShiftResult result)
        {
            LastResult = result;
            LastLevelUps.Clear();

            if (result == null) return;

            foreach (var pair in result.XpByMaid)
            {
                var maid = Profile.MaidById(pair.Key);
                if (maid == null) continue;

                var before = maid.Level;
                MaidProgression.GrantXp(maid, pair.Value, out _);
                var gained = maid.Level - before;
                if (gained > 0) LastLevelUps[maid.Id] = gained;
            }

            // Settle hands out XP as well; the grant above already did it for this result, so
            // the copy it is given has none left to award.
            var settled = new ShiftResult
            {
                Day = result.Day,
                Earned = result.Earned,
                Quota = result.Quota,
                QuotaMet = result.QuotaMet,
                Served = result.Served,
                Angry = result.Angry,
                TurnedAway = result.TurnedAway,
                ReputationDelta = result.ReputationDelta,
                SlowDay = result.SlowDay,
                FailureReason = result.FailureReason,
            };

            DayPlan.Settle(Profile, settled);

            CafeLevelledUp = Profile.CafeLevel > LastCafeLevel;
            RefreshApplicants();
            Touch();
        }

        public void RefreshApplicants() =>
            Applicants.Refresh(_rng.Fork(), Profile.CafeLevel, Profile.NextMaidId);

        /// <summary>Something changed. Save it and tell the screens.</summary>
        public void Touch()
        {
            SaveBridge.Save(Profile);
            Changed?.Invoke();
        }
    }
}
