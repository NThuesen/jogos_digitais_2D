using System;
using Neko.Sim.Core;
using Neko.Sim.Save;
using Neko.Sim.Shift;

namespace Neko.Sim.Economy
{
    /// <summary>
    /// What today asks of the player.
    /// </summary>
    /// <remarks>
    /// Four dials move together — the quota, how fast people arrive, how patient they are and
    /// how long the day runs. Keeping them in one place is what makes the difficulty curve
    /// something that can be read, checked against the balance server and argued with, rather
    /// than something scattered across the systems that happen to use it.
    /// </remarks>
    public static class DayPlan
    {
        public const float QuotaScale = 120f;
        public const float QuotaExponent = 1.35f;

        public static int QuotaFor(int step) =>
            (int)Math.Round(Curves.Power(QuotaScale, QuotaExponent, Math.Max(1, step)));

        /// <summary>
        /// Parties per second. Rises with the day and with the room's capacity, because a
        /// bigger café that never got busier would get easier every time it grew.
        /// </summary>
        public static float ArrivalsPerSecondFor(int day, int cafeLevel) =>
            Curves.Clamp(0.16f + 0.015f * (day - 1) + 0.035f * (cafeLevel - 1), 0.16f, 1.1f);

        /// <summary>Everybody gets a little more impatient, down to a floor of 60%.</summary>
        public static float PatienceScaleFor(int day) =>
            Curves.Clamp(1f - 0.012f * (day - 1), 0.6f, 1f);

        public static float DurationFor(int cafeLevel) =>
            Curves.Clamp(75f + 11f * (cafeLevel - 1), 75f, 180f);

        public static int DoorCapacityFor(int cafeLevel) =>
            Curves.ClampInt(3 + cafeLevel / 2, 3, 8);

        public static ShiftSetup Build(Profile profile, uint seed)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            var level = profile.CafeLevel;

            return new ShiftSetup
            {
                Day = profile.Day,
                CafeLevel = level,
                Grid = profile.Grid,
                Roster = profile.RosteredMaids(),
                DurationSeconds = DurationFor(level),
                ArrivalsPerSecond = ArrivalsPerSecondFor(profile.Day, level),
                Quota = QuotaFor(profile.QuotaStep),
                Seed = seed,
                DoorCapacity = DoorCapacityFor(level),
                PatienceScale = PatienceScaleFor(profile.Day),

                // Day one is scripted: three easy parties, spaced out, so the loop teaches
                // itself before the curve has any say.
                ScriptedArrivals = profile.Day == 1 && profile.QuotaStep == 1
                    ? new[]
                    {
                        new ScriptedArrival(1.5f, Service.CustomerType.Regular),
                        new ScriptedArrival(18f, Service.CustomerType.Student),
                        new ScriptedArrival(34f, Service.CustomerType.Regular),
                        new ScriptedArrival(50f, Service.CustomerType.Student),
                    }
                    : null,
            };
        }

        /// <summary>
        /// Folds a finished day back into the profile.
        /// </summary>
        /// <remarks>
        /// The day always advances; the quota step only advances when the quota was met. That
        /// one asymmetry is the whole of the no-dead-end promise: a player who is behind keeps
        /// replaying the same difficulty, earning, until they are not.
        /// </remarks>
        public static void Settle(Profile profile, ShiftResult result)
        {
            if (profile == null || result == null) return;

            profile.Wallet.Earn(result.Earned);
            profile.Wallet.AddReputation(result.ReputationDelta);

            foreach (var pair in result.XpByMaid)
            {
                Progress.MaidProgression.GrantXp(profile.MaidById(pair.Key), pair.Value, out _);
            }

            for (var i = 0; i < profile.Maids.Count; i++)
            {
                var maid = profile.Maids[i];
                Progress.MaidProgression.RecoverBetweenDays(maid,
                    profile.ShiftRoster.Contains(maid.Id));
            }

            profile.Records.TotalCustomersServed += result.Served;
            profile.Records.TotalDaysPlayed++;
            if (result.Earned > profile.Records.BestEarnings)
            {
                profile.Records.BestEarnings = result.Earned;
            }

            if (profile.Day > profile.Records.BestDay) profile.Records.BestDay = profile.Day;

            profile.Day++;
            if (result.QuotaMet) profile.QuotaStep++;

            if (profile.QuotaStep > 20) profile.EndlessUnlocked = true;

            profile.ApplyUnlocks();
        }
    }
}
