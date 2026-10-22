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
        /// <summary>Quota at café level one, before any step climb.</summary>
        public const float QuotaScale = 95f;

        /// <summary>
        /// Measured, not chosen. At 1.25 the quota passed what a well-built café earns
        /// somewhere around level seven and never came back.
        /// </summary>
        public const float QuotaExponent = 1.12f;

        /// <summary>How much harder each repeated day at the same café level gets.</summary>
        public const float QuotaStepClimb = 0.02f;

        /// <summary>
        /// What today has to earn.
        /// </summary>
        /// <remarks>
        /// Keyed on the café level rather than on the day, and that was measured rather than
        /// guessed. A café's takings barely move from one day to the next — they move when it
        /// grows — so a quota that climbed with the day outran the café immediately: the first
        /// curve multiplied the target by two and a half between day one and day two, and
        /// every seeded run of day two lost. The step still adds a couple of per cent, so a
        /// player sitting at one café level is nudged rather than parked.
        /// </remarks>
        public static int QuotaFor(int step, int cafeLevel)
        {
            var level = Curves.ClampInt(cafeLevel, 1, CafeLevelTable.MaxLevel);
            var baseline = Curves.Power(QuotaScale, QuotaExponent, level);
            var climb = 1f + QuotaStepClimb * Math.Max(0, step - 1);

            return (int)Math.Round(baseline * climb);
        }

        /// <summary>
        /// How fast people come in.
        /// </summary>
        /// <remarks>
        /// Driven by how many seats there are, not by the day. Arrivals beyond what a room can
        /// hold are pure downside — they earn nothing and cost goodwill — so a curve that
        /// raised them while the room stayed the same size made every later day strictly
        /// worse with nothing to show for it.
        /// </remarks>
        public static float ArrivalsPerSecondFor(int day, int cafeLevel, int seats) =>
            Curves.Clamp(0.055f * Math.Max(1, seats) + 0.008f * (day - 1), 0.12f, 1.1f);

        /// <summary>Everybody gets a little more impatient, down to a floor of 75%.</summary>
        public static float PatienceScaleFor(int day) =>
            Curves.Clamp(1f - 0.006f * (day - 1), 0.75f, 1f);

        public static float DurationFor(int cafeLevel) =>
            Curves.Clamp(75f + 11f * (cafeLevel - 1), 75f, 180f);

        public static int DoorCapacityFor(int cafeLevel) =>
            Curves.ClampInt(3 + cafeLevel / 2, 3, 8);

        public static ShiftSetup Build(Profile profile, uint seed)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            var level = profile.CafeLevel;
            var seats = profile.Grid.Metrics().Seats;

            return new ShiftSetup
            {
                Day = profile.Day,
                CafeLevel = level,
                Grid = profile.Grid,
                Roster = profile.RosteredMaids(),
                DurationSeconds = DurationFor(level),
                ArrivalsPerSecond = ArrivalsPerSecondFor(profile.Day, level, seats),
                Quota = QuotaFor(profile.QuotaStep, level),
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
