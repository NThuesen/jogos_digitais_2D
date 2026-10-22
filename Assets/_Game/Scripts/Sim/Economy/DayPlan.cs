using System;
using Neko.Sim.Core;
using Neko.Sim.Save;
using Neko.Sim.Shift;

namespace Neko.Sim.Economy
{
    /// <summary>
    /// What today looks like, before anybody walks in.
    /// </summary>
    /// <remarks>
    /// There is no quota any more. The day does not have a target to beat; it has a wage bill
    /// to cover, and whether it did is a line on the dashboard rather than a verdict the
    /// simulation hands down. What is left here is the shape of the day: how long it runs,
    /// how fast people arrive, and how patient they are.
    /// </remarks>
    public static class DayPlan
    {
        /// <summary>
        /// How fast people come in.
        /// </summary>
        /// <remarks>
        /// Driven by how many seats there are, not by the day. Arrivals beyond what a room can
        /// hold are pure downside — they earn nothing and cost goodwill — so a curve that
        /// raised them while the room stayed the same size made every later day strictly worse
        /// with nothing to show for it.
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

        /// <summary>
        /// Reputation for a day served well. What grows the café.
        /// </summary>
        /// <remarks>
        /// Paid on satisfaction rather than on takings, like the fichas and unlike the money.
        /// A café grows because it is good, not because it is busy.
        /// </remarks>
        public static int ReputationFor(ShiftResult result)
        {
            if (result == null || result.Impossible) return 0;

            var earned = (int)Math.Round(result.Served * result.Satisfaction * 2f);
            return Math.Max(0, earned - result.LeftAngry);
        }

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
                Roster = profile.FieldedMaids(),
                Lineup = profile.Lineup,
                Chemistry = profile.Chemistry(),
                CafeTheme = profile.CafeTheme,
                DurationSeconds = DurationFor(level),
                ArrivalsPerSecond = ArrivalsPerSecondFor(profile.Day, level, seats),
                Seed = seed,
                DoorCapacity = DoorCapacityFor(level),
                PatienceScale = PatienceScaleFor(profile.Day),

                // Day one is scripted: four easy parties, spaced out, so the loop teaches
                // itself before the curve has any say.
                ScriptedArrivals = profile.Day == 1
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
    }
}
