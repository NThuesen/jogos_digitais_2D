using System;
using System.Collections.Generic;
using Neko.Sim.Core;
using Neko.Sim.Maids;

namespace Neko.Sim.Team
{
    /// <summary>Where she trained. The closest thing this game has to a club badge.</summary>
    public enum House
    {
        Tsukimi = 0,
        Hanabi,
        Kurogane,
        Shirayuki,
        Momiji,
        Hoshizora,
    }

    /// <summary>How she works. Derived from her personality, never rolled.</summary>
    public enum Temperament
    {
        Energetic = 0,
        Serene,
        Dramatic,
    }

    public static class ChemistryTags
    {
        private static readonly string[] HouseNames =
            { "Tsukimi", "Hanabi", "Kurogane", "Shirayuki", "Momiji", "Hoshizora" };

        private static readonly string[] TemperamentNames = { "Enérgica", "Serena", "Dramática" };

        public static string NameOf(House house) => HouseNames[(int)house];

        public static string NameOf(Temperament temperament) => TemperamentNames[(int)temperament];

        public static string NameOf(ThemeTag origin)
        {
            switch (origin)
            {
                case ThemeTag.Sakura: return "Sakura";
                case ThemeTag.Gothic: return "Gótico";
                case ThemeTag.Starry: return "Estelar";
                case ThemeTag.Showa: return "Shōwa";
                default: return "—";
            }
        }

        /// <summary>
        /// Temperament follows the personality rather than being rolled beside it.
        /// </summary>
        /// <remarks>
        /// Two tags out of three are random, which is what makes a squad a puzzle; a third
        /// that the player can predict from the archetype is what makes it a solvable one.
        /// </remarks>
        public static Temperament TemperamentFor(Archetype archetype)
        {
            switch (archetype)
            {
                case Archetype.Genki:
                case Archetype.Dojikko:
                case Archetype.Tsundere:
                    return Temperament.Energetic;

                case Archetype.Kuudere:
                case Archetype.Mukuchi:
                case Archetype.Oneesan:
                    return Temperament.Serene;

                default:
                    return Temperament.Dramatic;
            }
        }

        public static House RollHouse(Rng rng) => (House)rng.NextInt(HouseNames.Length);

        public static ThemeTag RollOrigin(Rng rng) =>
            (ThemeTag)(1 + rng.NextInt(Enum.GetValues(typeof(ThemeTag)).Length - 1));

        /// <summary>How much two maids have in common, 0 to 3.</summary>
        public static int SharedTags(Maid a, Maid b)
        {
            if (a == null || b == null) return 0;

            var shared = 0;
            if (a.House == b.House) shared++;
            if (a.Temperament == b.Temperament) shared++;
            if (a.Origin == b.Origin && a.Origin != ThemeTag.None) shared++;
            return shared;
        }

        public static IReadOnlyList<string> Describe(Maid maid) => maid == null
            ? new string[0]
            : new[]
            {
                NameOf(maid.House), NameOf(maid.Temperament), NameOf(maid.Origin),
            };
    }
}
