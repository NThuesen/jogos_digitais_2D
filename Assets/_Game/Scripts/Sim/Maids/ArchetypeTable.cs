using System;
using System.Collections.Generic;

namespace Neko.Sim.Maids
{
    /// <summary>
    /// The nine rules, written down once.
    /// </summary>
    /// <remarks>
    /// The rules are deliberately uneven. Ojou-sama's refusal to bus tables is a real cost
    /// and Dojikko is high variance on purpose, because a roster that is only ever "take the
    /// highest numbers" is not a composition problem and the personalities would be paint.
    /// Accents sit on a ladder of lightness four points apart, not just on a spread of hues:
    /// nine colours at the same value differ only in a cue a small sprite barely carries, and
    /// palette_check reported eleven collisions for the first attempt. Neko.Game.Art.Palette
    /// has the measurements.
    /// </remarks>
    public static class ArchetypeTable
    {
        private static readonly ArchetypeProfile[] Profiles =
        {
            new ArchetypeProfile
            {
                Kind = Archetype.Genki,
                DisplayName = "Genki",
                StartingStats = new Stats(32, 14, 24, 18),
                GrowthWeights = new Stats(5, 1, 2, 2),
                MoveMultiplier = 1.35f,
                MoodDrainMultiplier = 1.6f,
                BaseFumbleChance = 0.10f,
                AccentHex = "#D68946",
                SilhouetteTell = "twin tails, high",
                Flavour = "Corre o salão inteiro em dez segundos. E cansa em cinco minutos.",
            },
            new ArchetypeProfile
            {
                Kind = Archetype.Kuudere,
                DisplayName = "Kuudere",
                StartingStats = new Stats(18, 32, 14, 26),
                GrowthWeights = new Stats(2, 5, 1, 2),
                MoveMultiplier = 0.8f,
                NeverFumbles = true,
                AccentHex = "#6BAED0",
                SilhouetteTell = "straight bob, half-lidded eyes",
                Flavour = "Nunca derruba nada. Nunca tem pressa. Nunca comenta.",
            },
            new ArchetypeProfile
            {
                Kind = Archetype.Tsundere,
                DisplayName = "Tsundere",
                StartingStats = new Stats(28, 20, 22, 20),
                GrowthWeights = new Stats(4, 2, 3, 1),
                BaseFumbleChance = 0.08f,
                CrowdedThreshold = 0.60f,
                CrowdedSpeedBonus = 0.30f,
                IdleTipPenalty = 0.15f,
                AccentHex = "#D34F4F",
                SilhouetteTell = "side ponytail, large ribbon",
                Flavour = "Só trabalha direito quando o salão enche. Não é por você, é pelo café.",
            },
            new ArchetypeProfile
            {
                Kind = Archetype.Dojikko,
                DisplayName = "Dojikko",
                StartingStats = new Stats(24, 18, 30, 16),
                GrowthWeights = new Stats(3, 2, 4, 1),
                BaseFumbleChance = 0.08f,
                ClumsyDropChance = 0.12f,
                AccentHex = "#4DA33F",
                SilhouetteTell = "oversized apron bow, ahoge",
                Flavour = "Derruba um prato a cada oito. A mesa inteira acha adorável.",
            },
            new ArchetypeProfile
            {
                Kind = Archetype.Ojousama,
                DisplayName = "Ojou-sama",
                StartingStats = new Stats(16, 12, 38, 18),
                GrowthWeights = new Stats(2, 1, 6, 1),
                BaseFumbleChance = 0.06f,
                WillBusTables = false,
                VipWeightMultiplier = 2f,
                AccentHex = "#A42F79",
                SilhouetteTell = "drill curls, tiara",
                Flavour = "Atrai a clientela importante. Limpar mesa, jamais.",
            },
            new ArchetypeProfile
            {
                Kind = Archetype.Oneesan,
                DisplayName = "Onee-san",
                StartingStats = new Stats(24, 24, 24, 22),
                GrowthWeights = new Stats(3, 3, 2, 2),
                BaseFumbleChance = 0.07f,
                NeighbourSpeedAura = 0.15f,
                XpShareFraction = 0.25f,
                AccentHex = "#636AC5",
                SilhouetteTell = "long hair, shawl",
                Flavour = "Quem trabalha perto dela rende mais. E aprende mais rápido.",
            },
            new ArchetypeProfile
            {
                Kind = Archetype.Idol,
                DisplayName = "Idol",
                StartingStats = new Stats(20, 12, 36, 20),
                GrowthWeights = new Stats(2, 1, 5, 2),
                BaseFumbleChance = 0.09f,
                HasEncore = true,
                EncoreCooldownSeconds = 90f,
                EncorePatienceRestored = 0.70f,
                AccentHex = "#9940B5",
                SilhouetteTell = "star hairpin, cape",
                Flavour = "Sobe no palco e o salão inteiro esquece que estava esperando.",
            },
            new ArchetypeProfile
            {
                Kind = Archetype.Gourmand,
                DisplayName = "Gourmand",
                StartingStats = new Stats(14, 36, 18, 22),
                GrowthWeights = new Stats(1, 6, 1, 2),
                CookMultiplier = 1.8f,
                BaseFumbleChance = 0.05f,
                EatChance = 0.2f,
                AccentHex = "#C9B04A",
                SilhouetteTell = "round face, chef scarf",
                Flavour = "Cozinha quase no dobro da velocidade. Come um prato a cada cinco.",
            },
            new ArchetypeProfile
            {
                Kind = Archetype.Mukuchi,
                DisplayName = "Mukuchi",
                StartingStats = new Stats(12, 34, 20, 24),
                GrowthWeights = new Stats(1, 5, 3, 1),
                GreetMultiplier = 0.6f,
                CookMultiplier = 1.5f,
                BaseFumbleChance = 0.05f,
                CharmDoublesAtStation = true,
                AccentHex = "#32957D",
                SilhouetteTell = "hair over one eye, mask down",
                Flavour = "Péssima na porta, excelente no fogão. O prato dela sai bonito.",
            },
        };

        public static IReadOnlyList<ArchetypeProfile> All => Profiles;

        public static ArchetypeProfile For(Archetype kind)
        {
            for (var i = 0; i < Profiles.Length; i++)
            {
                if (Profiles[i].Kind == kind) return Profiles[i];
            }

            throw new ArgumentOutOfRangeException(nameof(kind), kind, "No profile for this archetype.");
        }
    }
}
