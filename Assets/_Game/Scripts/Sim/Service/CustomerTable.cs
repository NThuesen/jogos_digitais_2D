using System;
using System.Collections.Generic;
using Neko.Sim.Core;
using Neko.Sim.Maids;

namespace Neko.Sim.Service
{
    public enum CustomerType
    {
        Regular = 0,
        Student,
        Salaryman,
        Cat,
        Otaku,
        Critic,
        Vip,
    }

    /// <summary>One kind of customer, as the rules that make them feel different to serve.</summary>
    public sealed class CustomerProfile
    {
        public CustomerType Kind;
        public string DisplayName;

        /// <summary>Seconds of waiting before they walk out, at ambience 1.</summary>
        public float PatienceSeconds;

        public float SpendMultiplier;
        public int PartySizeMin = 1;
        public int PartySizeMax = 1;
        public float ArrivalWeight;
        public int UnlockCafeLevel = 1;
        public int DishesMin = 1;
        public int DishesMax = 2;

        /// <summary>The cat. Sits there forever and makes the room nicer.</summary>
        public bool NeverImpatient;

        public float AmbienceWhileSeated;

        /// <summary>The otaku. Tips enormously if the maid he came for is on shift.</summary>
        public bool WantsAnArchetype;

        public float WantedArchetypeTipBonus;

        /// <summary>The cat again: one dish, always the same one.</summary>
        public string OnlyOrders;

        /// <summary>The critic scores the service and moves reputation at the end of the day.</summary>
        public bool Judges;

        public string AccentHex;
        public string Flavour;
    }

    public static class CustomerTable
    {
        private static readonly CustomerProfile[] Profiles =
        {
            new CustomerProfile
            {
                Kind = CustomerType.Regular,
                DisplayName = "Cliente",
                PatienceSeconds = 45f,
                SpendMultiplier = 1f,
                PartySizeMin = 1, PartySizeMax = 2,
                ArrivalWeight = 10f,
                UnlockCafeLevel = 1,
                AccentHex = "#A89078",
                Flavour = "Veio pelo café.",
            },
            new CustomerProfile
            {
                Kind = CustomerType.Student,
                DisplayName = "Estudante",
                PatienceSeconds = 62f,
                SpendMultiplier = 0.7f,
                PartySizeMin = 1, PartySizeMax = 3,
                ArrivalWeight = 6f,
                UnlockCafeLevel = 1,
                DishesMin = 1, DishesMax = 2,
                AccentHex = "#7FA8C4",
                Flavour = "Fica horas. Gasta pouco. Não reclama.",
            },
            new CustomerProfile
            {
                Kind = CustomerType.Salaryman,
                DisplayName = "Assalariado",
                PatienceSeconds = 30f,
                SpendMultiplier = 1.4f,
                ArrivalWeight = 5f,
                UnlockCafeLevel = 2,
                DishesMin = 1, DishesMax = 2,
                AccentHex = "#6E6A80",
                Flavour = "Tem vinte minutos de almoço e sabe disso.",
            },
            new CustomerProfile
            {
                Kind = CustomerType.Cat,
                DisplayName = "Gato",
                PatienceSeconds = 60f,
                SpendMultiplier = 0.4f,
                ArrivalWeight = 3f,
                UnlockCafeLevel = 2,
                DishesMin = 1, DishesMax = 1,
                NeverImpatient = true,
                AmbienceWhileSeated = 4f,
                OnlyOrders = "milk",
                AccentHex = "#C7B299",
                Flavour = "Pede leite. Fica. O salão inteiro melhora.",
            },
            new CustomerProfile
            {
                Kind = CustomerType.Otaku,
                DisplayName = "Fã",
                PatienceSeconds = 50f,
                SpendMultiplier = 1f,
                PartySizeMin = 1, PartySizeMax = 2,
                ArrivalWeight = 4f,
                UnlockCafeLevel = 3,
                DishesMin = 1, DishesMax = 3,
                WantsAnArchetype = true,
                WantedArchetypeTipBonus = 1.8f,
                AccentHex = "#C47FD1",
                Flavour = "Veio por uma maid específica. Paga muito bem se ela estiver.",
            },
            new CustomerProfile
            {
                Kind = CustomerType.Critic,
                DisplayName = "Crítico",
                PatienceSeconds = 35f,
                SpendMultiplier = 1.2f,
                ArrivalWeight = 2f,
                UnlockCafeLevel = 4,
                DishesMin = 2, DishesMax = 3,
                Judges = true,
                AccentHex = "#8C7B5A",
                Flavour = "Anota tudo. A reputação do dia depende dele.",
            },
            new CustomerProfile
            {
                Kind = CustomerType.Vip,
                DisplayName = "VIP",
                PatienceSeconds = 22f,
                SpendMultiplier = 3f,
                PartySizeMin = 1, PartySizeMax = 2,
                ArrivalWeight = 2f,
                UnlockCafeLevel = 6,
                DishesMin = 2, DishesMax = 3,
                AccentHex = "#D1A54F",
                Flavour = "Gasta uma fortuna e não espera nada.",
            },
        };

        public static IReadOnlyList<CustomerProfile> All => Profiles;

        public static CustomerProfile For(CustomerType kind)
        {
            for (var i = 0; i < Profiles.Length; i++)
            {
                if (Profiles[i].Kind == kind) return Profiles[i];
            }

            throw new ArgumentOutOfRangeException(nameof(kind), kind, "No profile for this customer.");
        }

        public static IReadOnlyList<CustomerProfile> UnlockedAt(int cafeLevel)
        {
            var found = new List<CustomerProfile>();
            for (var i = 0; i < Profiles.Length; i++)
            {
                if (Profiles[i].UnlockCafeLevel <= cafeLevel) found.Add(Profiles[i]);
            }

            return found;
        }

        /// <summary>
        /// Who walks in next. <paramref name="vipWeightMultiplier"/> is how an Ojou-sama on
        /// shift changes the clientele — her rule reaches outside her own speeds, which is
        /// what makes rostering her a decision rather than a stat comparison.
        /// </summary>
        public static CustomerType Roll(Rng rng, int cafeLevel, float vipWeightMultiplier)
        {
            var available = UnlockedAt(cafeLevel);

            return rng.PickWeighted(available, profile =>
                profile.Kind == CustomerType.Vip
                    ? profile.ArrivalWeight * Math.Max(0f, vipWeightMultiplier)
                    : profile.ArrivalWeight).Kind;
        }

        /// <summary>Every archetype an otaku might have come to see.</summary>
        public static Archetype WantedArchetype(Rng rng)
        {
            var all = ArchetypeTable.All;
            return all[rng.NextInt(all.Count)].Kind;
        }
    }
}
