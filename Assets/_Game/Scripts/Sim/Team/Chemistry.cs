using System.Collections.Generic;
using Neko.Sim.Core;
using Neko.Sim.Maids;

namespace Neko.Sim.Team
{
    /// <summary>One maid's standing in the lineup.</summary>
    public readonly struct MaidChemistry
    {
        public readonly int MaidId;
        public readonly Role Role;
        public readonly bool InPosition;

        /// <summary>Zero to ten.</summary>
        public readonly int Score;

        public readonly int FromPosition;
        public readonly int FromLinks;
        public readonly int FromOrigin;
        public readonly int FromTraits;

        public MaidChemistry(int maidId, Role role, bool inPosition, int fromPosition,
            int fromLinks, int fromOrigin, int fromTraits)
        {
            MaidId = maidId;
            Role = role;
            InPosition = inPosition;
            FromPosition = fromPosition;
            FromLinks = fromLinks;
            FromOrigin = fromOrigin;
            FromTraits = fromTraits;
            Score = Curves.ClampInt(fromPosition + fromLinks + fromOrigin + fromTraits, 0,
                Chemistry.MaxPerMaid);
        }
    }

    /// <summary>The whole lineup's standing.</summary>
    public readonly struct TeamChemistry
    {
        /// <summary>Zero to a hundred.</summary>
        public readonly int Score;

        /// <summary>What every maid's effective stats are multiplied by.</summary>
        public readonly float Multiplier;

        public readonly IReadOnlyList<MaidChemistry> PerMaid;

        public TeamChemistry(int score, float multiplier, IReadOnlyList<MaidChemistry> perMaid)
        {
            Score = score;
            Multiplier = multiplier;
            PerMaid = perMaid;
        }

        public MaidChemistry? For(int maidId)
        {
            for (var i = 0; i < PerMaid.Count; i++)
            {
                if (PerMaid[i].MaidId == maidId) return PerMaid[i];
            }

            return null;
        }
    }

    /// <summary>
    /// How well a lineup fits together.
    /// </summary>
    /// <remarks>
    /// The mechanic the whole pivot rests on. A squad of legendaries thrown into the wrong
    /// positions should lose to a squad of commons that belong where they stand — otherwise
    /// the boxes decide the game and the lineup screen is decoration.
    ///
    /// The swing is deliberately large: a multiplier from 0.75 to 1.25 on every stat in the
    /// café is a third more café between a bad lineup and a good one holding the same cards.
    /// </remarks>
    public static class Chemistry
    {
        public const int MaxPerMaid = 10;

        /// <summary>Standing where she belongs.</summary>
        public const int PositionBonus = 3;

        /// <summary>Sharing the room's decorating theme.</summary>
        public const int OriginBonus = 3;

        /// <summary>Most one link between two maids can be worth.</summary>
        public const int MaxPerLink = 2;

        /// <summary>Most the links inside one position can add up to.</summary>
        public const int MaxFromLinks = 4;

        public const float LowMultiplier = 0.75f;
        public const float HighMultiplier = 1.25f;

        public static TeamChemistry Evaluate(Lineup lineup, IReadOnlyList<Maid> roster,
            ThemeTag cafeTheme)
        {
            var perMaid = new List<MaidChemistry>();
            if (lineup == null || roster == null)
            {
                return new TeamChemistry(0, LowMultiplier, perMaid);
            }

            Maid Find(int id)
            {
                for (var i = 0; i < roster.Count; i++)
                {
                    if (roster[i].Id == id) return roster[i];
                }

                return null;
            }

            foreach (var slot in lineup.All)
            {
                if (slot.Empty) continue;

                var maid = Find(slot.MaidId);
                if (maid == null) continue;

                var inPosition = maid.NaturalRole == slot.Role;
                var position = inPosition ? PositionBonus : 0;

                // Links are counted inside a position only. Two cooks who trained together
                // work together; a cook and a greeter on opposite sides of the room do not.
                var links = 0;
                foreach (var otherId in lineup.MaidIdsIn(slot.Role))
                {
                    if (otherId == maid.Id) continue;

                    var other = Find(otherId);
                    if (other == null) continue;

                    links += Curves.ClampInt(ChemistryTags.SharedTags(maid, other), 0, MaxPerLink);
                }

                links = Curves.ClampInt(links, 0, MaxFromLinks);

                var origin = cafeTheme != ThemeTag.None && maid.Origin == cafeTheme
                    ? OriginBonus
                    : 0;

                var traits = 0;
                for (var i = 0; i < maid.Traits.Count; i++)
                {
                    traits += TraitTable.ById(maid.Traits[i]).ChemistryBonus;
                }

                perMaid.Add(new MaidChemistry(maid.Id, slot.Role, inPosition, position, links,
                    origin, traits));
            }

            if (perMaid.Count == 0) return new TeamChemistry(0, LowMultiplier, perMaid);

            var total = 0;
            for (var i = 0; i < perMaid.Count; i++) total += perMaid[i].Score;

            var score = Curves.ClampInt(
                (int)System.Math.Round(total / (float)perMaid.Count * 10f), 0, 100);

            return new TeamChemistry(score, MultiplierFor(score), perMaid);
        }

        public static float MultiplierFor(int score) =>
            Curves.Lerp(LowMultiplier, HighMultiplier, Curves.Clamp01(score / 100f));

        /// <summary>Portuguese, for the lineup screen. Terse on purpose.</summary>
        public static string Verdict(int score)
        {
            if (score >= 85) return "Entrosadíssimo";
            if (score >= 65) return "Entrosado";
            if (score >= 40) return "Dá para o gasto";
            if (score >= 20) return "Desencontrado";
            return "Não se falam";
        }
    }
}
