using System;
using Neko.Sim.Core;

namespace Neko.Sim.Maids
{
    /// <summary>
    /// Turns a maid plus a moment into the speeds the shift runs on, and owns the XP curve.
    /// </summary>
    public static class MaidMath
    {
        /// <summary>Tiles per second for an average maid with nothing helping her.</summary>
        public const float BaseWalkSpeed = 2.2f;

        /// <summary>Stat points a level is worth, spread across the archetype's weights.</summary>
        public const int PointsPerLevel = 4;

        /// <summary>What a maid at zero mood is still worth. Never zero: she must finish.</summary>
        public const float MoodFloor = 0.55f;

        public const float RushBonus = 0.6f;

        /// <summary>Full mood to empty in this many seconds of continuous work, at average stamina.</summary>
        public const float MoodSecondsAtAverageStamina = 240f;

        private const float TipPerCharm = 0.6f;

        /// <summary>
        /// Where equipped skills get to change the result.
        /// </summary>
        /// <remarks>
        /// A delegate rather than a direct call so that <c>Maids</c> does not have to know
        /// about <c>Progress</c>. The skill table installs itself at startup; with nothing
        /// installed the game is simply a game without skills, which is what the early tests
        /// want.
        /// </remarks>
        public static Func<Maid, MaidDerived, ShiftSnapshot, MaidDerived> SkillHook;

        public static MaidDerived Derive(Maid maid, ShiftSnapshot moment)
        {
            var profile = maid.Profile;
            var total = maid.Total;

            var serve = total.Normalised(StatKind.Serve);
            var cook = total.Normalised(StatKind.Cook);
            var charm = total.Normalised(StatKind.Charm);
            var stamina = total.Normalised(StatKind.Stamina);

            // A spent maid is slower, never stopped. Going to zero would strand whatever task
            // she had claimed, which is the one thing a shift must not do.
            var mood = Curves.Lerp(MoodFloor, 1f, maid.Mood);

            var situational = 1f + moment.NeighbourAura;
            if (moment.TableOccupancy >= profile.CrowdedThreshold)
            {
                situational += profile.CrowdedSpeedBonus;
            }

            if (moment.Rushing) situational += RushBonus;

            var common = situational * mood;

            var move = BaseWalkSpeed * profile.MoveMultiplier * (0.7f + 0.6f * serve) * common;
            var greet = profile.GreetMultiplier * (0.6f + 0.9f * serve) * common;
            var serveSpeed = profile.ServeMultiplier * (0.6f + 0.9f * serve) * common;
            var cookSpeed = profile.CookMultiplier * (0.6f + 1.0f * cook) * common;

            var effectiveCharm = charm;
            if (profile.CharmDoublesAtStation && moment.AtStation) effectiveCharm *= 2f;

            var tip = 1f + effectiveCharm * TipPerCharm;
            if (moment.Idle) tip *= 1f - profile.IdleTipPenalty;

            var fumble = profile.NeverFumbles
                ? 0f
                : Curves.Clamp01(profile.BaseFumbleChance * (1f - 0.8f * cook));
            var drop = profile.NeverFumbles ? 0f : profile.ClumsyDropChance;

            var drain = 1f / MoodSecondsAtAverageStamina
                        * profile.MoodDrainMultiplier
                        * (1.4f - 0.8f * stamina);

            var derived = new MaidDerived(move, greet, serveSpeed, cookSpeed, tip, fumble, drop,
                drain);

            var hook = SkillHook;
            return hook == null ? derived : hook(maid, derived, moment);
        }

        /// <summary>Cumulative XP needed to be at <paramref name="level"/>. Level 1 costs nothing.</summary>
        public static int XpForLevel(int level) =>
            (int)Math.Round(Curves.Power(40f, 1.6f, Curves.ClampInt(level, 1, Maid.MaxLevel) - 1));

        /// <summary>
        /// The stat points <paramref name="levelsGained"/> levels are worth, spread across the
        /// archetype's growth weights by largest remainder.
        /// </summary>
        /// <remarks>
        /// Largest remainder rather than rounding each stat independently, because rounding
        /// four numbers independently spends either three points or five and the drift
        /// compounds over twenty levels.
        /// </remarks>
        public static Stats GrowthFor(Maid maid, int levelsGained)
        {
            if (levelsGained <= 0) return Stats.Zero;

            var weights = maid.Profile.GrowthWeights;
            var weightSum = weights.Serve + weights.Cook + weights.Charm + weights.Stamina;
            var points = PointsPerLevel * levelsGained;

            if (weightSum <= 0) return Stats.Uniform(points / 4);

            var exact = new float[4];
            var awarded = new int[4];
            for (var i = 0; i < 4; i++)
            {
                exact[i] = points * weights[(StatKind)i] / (float)weightSum;
                awarded[i] = (int)exact[i];
            }

            var spent = awarded[0] + awarded[1] + awarded[2] + awarded[3];
            while (spent < points)
            {
                var best = -1;
                var bestRemainder = -1f;
                for (var i = 0; i < 4; i++)
                {
                    var remainder = exact[i] - awarded[i];
                    if (remainder > bestRemainder)
                    {
                        bestRemainder = remainder;
                        best = i;
                    }
                }

                awarded[best]++;
                exact[best] = awarded[best]; // spent; it must not win the next round too
                spent++;
            }

            return new Stats(awarded[0], awarded[1], awarded[2], awarded[3]);
        }
    }
}
