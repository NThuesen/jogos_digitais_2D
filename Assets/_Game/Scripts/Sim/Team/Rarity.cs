using System;
using System.Collections.Generic;
using Neko.Sim.Core;
using Neko.Sim.Maids;

namespace Neko.Sim.Team
{
    public enum Rarity
    {
        Common = 0,
        Rare,
        Epic,
        Legendary,
    }

    public sealed class RarityProfile
    {
        public Rarity Kind;
        public string DisplayName;

        /// <summary>Range each starting stat is rolled in.</summary>
        public int StatMin;

        public int StatMax;

        /// <summary>How many traits she comes out of the box with.</summary>
        public int Traits;

        /// <summary>Daily wage. A legendary on the bench still costs every single day.</summary>
        public int Salary;

        /// <summary>Fichas returned when she is let go.</summary>
        public int SeverancePoints;

        public string TintHex;
    }

    /// <summary>
    /// What a card is worth before anything is done to it.
    /// </summary>
    /// <remarks>
    /// Salary rising steeply with rarity is the whole reason the lineup is a budget decision
    /// rather than "play the best four". A squad of legendaries that cannot cover its own
    /// wage bill goes bankrupt holding the best cards in the game.
    /// </remarks>
    public static class RarityTable
    {
        private static readonly RarityProfile[] Profiles =
        {
            new RarityProfile
            {
                Kind = Rarity.Common, DisplayName = "Comum",
                StatMin = 18, StatMax = 32, Traits = 0,
                Salary = 18, SeverancePoints = 5, TintHex = "#A89078",
            },
            new RarityProfile
            {
                Kind = Rarity.Rare, DisplayName = "Rara",
                StatMin = 28, StatMax = 46, Traits = 1,
                Salary = 44, SeverancePoints = 20, TintHex = "#6BAED0",
            },
            new RarityProfile
            {
                Kind = Rarity.Epic, DisplayName = "Épica",
                StatMin = 40, StatMax = 62, Traits = 2,
                Salary = 95, SeverancePoints = 70, TintHex = "#9940B5",
            },
            new RarityProfile
            {
                Kind = Rarity.Legendary, DisplayName = "Lendária",
                StatMin = 55, StatMax = 78, Traits = 3,
                Salary = 210, SeverancePoints = 220, TintHex = "#C9B04A",
            },
        };

        public static IReadOnlyList<RarityProfile> All => Profiles;

        public static RarityProfile For(Rarity rarity)
        {
            for (var i = 0; i < Profiles.Length; i++)
            {
                if (Profiles[i].Kind == rarity) return Profiles[i];
            }

            throw new ArgumentOutOfRangeException(nameof(rarity));
        }

        /// <summary>Rolls the four starting stats inside this rarity's band.</summary>
        public static Stats RollStats(Rarity rarity, Archetype archetype, Rng rng)
        {
            var band = For(rarity);
            var weights = ArchetypeTable.For(archetype).GrowthWeights;
            var total = Math.Max(1, weights.Serve + weights.Cook + weights.Charm + weights.Stamina);

            int Roll(int weight)
            {
                // Her personality tilts the roll: a Mukuchi rolls high in the kitchen even as
                // a Common, which is what makes the archetype readable on the card.
                var tilt = weight / (float)total * 4f;
                var low = Curves.Lerp(band.StatMin, band.StatMax, 0.15f * tilt);
                var high = Curves.Lerp(low, band.StatMax, 0.6f);
                return Curves.ClampInt((int)rng.Range(low, high + 1f), Stats.Min, Stats.Max);
            }

            return new Stats(Roll(weights.Serve), Roll(weights.Cook), Roll(weights.Charm),
                Roll(weights.Stamina));
        }
    }
}
