using System;
using System.Collections.Generic;
using Neko.Sim.Core;

namespace Neko.Sim.Team
{
    /// <summary>
    /// The extra modifiers a card comes out of a box with.
    /// </summary>
    /// <remarks>
    /// Explicit values, because a trait id is written into the save file and renumbering the
    /// enum would hand every stored maid somebody else's abilities.
    ///
    /// Traits exist so that two Legendary Genkis are different cards. Without them, rarity is
    /// only a bigger number and a box is only a slot machine for the same prize.
    /// </remarks>
    public enum TraitId
    {
        None = 0,

        AngelHands = 10,
        QuickStep = 11,
        Charisma = 12,
        IronStomach = 13,
        Metronome = 14,
        Hostess = 15,
        Prodigy = 16,
        NightOwl = 17,
        Perfectionist = 18,
        Lucky = 19,
        Veteran = 20,
        Magnetic = 21,
    }

    public sealed class TraitDef
    {
        public TraitId Id;
        public string DisplayName;

        /// <summary>Portuguese. Shown on her card.</summary>
        public string Description;

        /// <summary>Multiplier on every speed. 1 for traits that do something else.</summary>
        public float SpeedMultiplier = 1f;

        public float TipMultiplier = 1f;

        /// <summary>Multiplier on the chance she hands over the wrong plate.</summary>
        public float MistakeMultiplier = 1f;

        /// <summary>Multiplier on how fast the day wears her down.</summary>
        public float MoodDrainMultiplier = 1f;

        /// <summary>Flat chemistry, before any link is counted.</summary>
        public int ChemistryBonus;

        /// <summary>Extra fichas at the end of a day she worked.</summary>
        public int PointsPerDay;

        /// <summary>Discount on her own wage, as a fraction.</summary>
        public float SalaryDiscount;
    }

    public static class TraitTable
    {
        private static readonly TraitDef[] Defs =
        {
            new TraitDef
            {
                Id = TraitId.AngelHands, DisplayName = "Mão de Anjo",
                Description = "Nunca entrega o prato errado.",
                MistakeMultiplier = 0f,
            },
            new TraitDef
            {
                Id = TraitId.QuickStep, DisplayName = "Pé Ligeiro",
                Description = "Tudo o que ela faz é 15% mais rápido.",
                SpeedMultiplier = 1.15f,
            },
            new TraitDef
            {
                Id = TraitId.Charisma, DisplayName = "Carisma",
                Description = "20% a mais de gorjeta.",
                TipMultiplier = 1.2f,
            },
            new TraitDef
            {
                Id = TraitId.IronStomach, DisplayName = "Fôlego de Ferro",
                Description = "Cansa 40% mais devagar.",
                MoodDrainMultiplier = 0.6f,
            },
            new TraitDef
            {
                Id = TraitId.Metronome, DisplayName = "Metrônomo",
                Description = "Erra metade do que erraria.",
                MistakeMultiplier = 0.5f,
            },
            new TraitDef
            {
                Id = TraitId.Hostess, DisplayName = "Anfitriã",
                Description = "+2 de química, com quem quer que seja.",
                ChemistryBonus = 2,
            },
            new TraitDef
            {
                Id = TraitId.Prodigy, DisplayName = "Prodígio",
                Description = "10% mais rápida e 10% mais de gorjeta.",
                SpeedMultiplier = 1.1f, TipMultiplier = 1.1f,
            },
            new TraitDef
            {
                Id = TraitId.NightOwl, DisplayName = "Notívaga",
                Description = "Rende 20% mais, mas cansa 30% mais rápido.",
                SpeedMultiplier = 1.2f, MoodDrainMultiplier = 1.3f,
            },
            new TraitDef
            {
                Id = TraitId.Perfectionist, DisplayName = "Perfeccionista",
                Description = "Quase não erra, mas trabalha 10% mais devagar.",
                MistakeMultiplier = 0.25f, SpeedMultiplier = 0.9f,
            },
            new TraitDef
            {
                Id = TraitId.Lucky, DisplayName = "Sortuda",
                Description = "+3 fichas por dia trabalhado.",
                PointsPerDay = 3,
            },
            new TraitDef
            {
                Id = TraitId.Veteran, DisplayName = "Veterana",
                Description = "Custa 25% menos de salário.",
                SalaryDiscount = 0.25f,
            },
            new TraitDef
            {
                Id = TraitId.Magnetic, DisplayName = "Magnética",
                Description = "+1 de química e 10% a mais de gorjeta.",
                ChemistryBonus = 1, TipMultiplier = 1.1f,
            },
        };

        public static IReadOnlyList<TraitDef> All => Defs;

        public static TraitDef ById(TraitId id)
        {
            for (var i = 0; i < Defs.Length; i++)
            {
                if (Defs[i].Id == id) return Defs[i];
            }

            throw new KeyNotFoundException("No trait called " + id + ".");
        }

        /// <summary>Rolls distinct traits for a new card.</summary>
        public static List<TraitId> Roll(int count, Rng rng)
        {
            var chosen = new List<TraitId>();
            if (count <= 0) return chosen;

            var pool = new List<TraitDef>(Defs);
            for (var i = 0; i < count && pool.Count > 0; i++)
            {
                var index = rng.NextInt(pool.Count);
                chosen.Add(pool[index].Id);
                pool.RemoveAt(index);
            }

            return chosen;
        }
    }
}
