using System;
using System.Collections.Generic;
using Neko.Sim.Core;
using Neko.Sim.Maids;

namespace Neko.Sim.Progress
{
    public sealed class SkillDef
    {
        public SkillId Id;
        public string DisplayName;
        public SkillBranch Branch;
        public int RequiredLevel;

        /// <summary>Portuguese: this is read by the player, in the maid's detail screen.</summary>
        public string Description;
    }

    /// <summary>
    /// Eighteen skills, six per branch, three slots at most.
    /// </summary>
    /// <remarks>
    /// Split in two deliberately. The ones that are pure multipliers are applied here, in
    /// <see cref="Apply"/>, which the maid maths calls through a hook; the ones that change
    /// what the shift <em>does</em> — carrying two plates, a free first ticket — are read by
    /// the shift with <see cref="Has"/>. Trying to express the second kind as a multiplier is
    /// what turns a skill tree into a list of percentages.
    /// </remarks>
    public static class SkillTable
    {
        public const int MaxSlots = 3;

        /// <summary>A slot every fourth level, which is three by level twelve.</summary>
        public static int SlotsAt(int level) => Curves.ClampInt(level / 4, 0, MaxSlots);

        private static readonly SkillDef[] Defs =
        {
            // Floor
            new SkillDef
            {
                Id = SkillId.QuickFeet, DisplayName = "Pés Rápidos", Branch = SkillBranch.Floor,
                RequiredLevel = 1, Description = "Anda 15% mais rápido pelo salão.",
            },
            new SkillDef
            {
                Id = SkillId.Usher, DisplayName = "Recepção", Branch = SkillBranch.Floor,
                RequiredLevel = 4, Description = "Acomoda os clientes 40% mais rápido.",
            },
            new SkillDef
            {
                Id = SkillId.TableSense, DisplayName = "Olho de Salão", Branch = SkillBranch.Floor,
                RequiredLevel = 8, Description = "Anota pedidos 30% mais rápido.",
            },
            new SkillDef
            {
                Id = SkillId.DoubleTray, DisplayName = "Bandeja Dupla", Branch = SkillBranch.Floor,
                RequiredLevel = 8, Description = "Leva dois pratos de uma vez.",
            },
            new SkillDef
            {
                Id = SkillId.CleanSweep, DisplayName = "Limpeza Relâmpago",
                Branch = SkillBranch.Floor, RequiredLevel = 12,
                Description = "Limpa mesas 60% mais rápido.",
            },
            new SkillDef
            {
                Id = SkillId.Multitask, DisplayName = "Multitarefa", Branch = SkillBranch.Floor,
                RequiredLevel = 16, Description = "Tudo o que ela faz é 10% mais rápido.",
            },

            // Kitchen
            new SkillDef
            {
                Id = SkillId.SteadyHands, DisplayName = "Mão Firme", Branch = SkillBranch.Kitchen,
                RequiredLevel = 1, Description = "Estraga metade dos pratos que estragaria.",
            },
            new SkillDef
            {
                Id = SkillId.Prep, DisplayName = "Pré-preparo", Branch = SkillBranch.Kitchen,
                RequiredLevel = 4, Description = "Cozinha 15% mais rápido.",
            },
            new SkillDef
            {
                Id = SkillId.HeatControl, DisplayName = "Controle de Fogo",
                Branch = SkillBranch.Kitchen, RequiredLevel = 8,
                Description = "Cozinha mais 25% mais rápido.",
            },
            new SkillDef
            {
                Id = SkillId.MiseEnPlace, DisplayName = "Mise en Place",
                Branch = SkillBranch.Kitchen, RequiredLevel = 8,
                Description = "O primeiro prato do turno sai na hora.",
            },
            new SkillDef
            {
                Id = SkillId.BatchCooking, DisplayName = "Produção em Série",
                Branch = SkillBranch.Kitchen, RequiredLevel = 12,
                Description = "Com três ou mais comandas na fila, cozinha 35% mais rápido.",
            },
            new SkillDef
            {
                Id = SkillId.Plating, DisplayName = "Empratamento",
                Branch = SkillBranch.Kitchen, RequiredLevel = 16,
                Description = "Os pratos dela rendem 15% a mais de gorjeta.",
            },

            // Charm
            new SkillDef
            {
                Id = SkillId.WarmWelcome, DisplayName = "Bem-vindo!", Branch = SkillBranch.Charm,
                RequiredLevel = 1, Description = "Quem ela acomoda senta com 15% mais paciência.",
            },
            new SkillDef
            {
                Id = SkillId.SweetTalk, DisplayName = "Lábia", Branch = SkillBranch.Charm,
                RequiredLevel = 4, Description = "15% a mais de gorjeta.",
            },
            new SkillDef
            {
                Id = SkillId.Patience, DisplayName = "Presença", Branch = SkillBranch.Charm,
                RequiredLevel = 8, Description = "Quem está perto dela recupera paciência devagar.",
            },
            new SkillDef
            {
                Id = SkillId.Regulars, DisplayName = "Freguesia", Branch = SkillBranch.Charm,
                RequiredLevel = 8, Description = "Quem ela atende volta amanhã gastando 20% a mais.",
            },
            new SkillDef
            {
                Id = SkillId.SecondWind, DisplayName = "Segundo Fôlego",
                Branch = SkillBranch.Charm, RequiredLevel = 12,
                Description = "Uma vez por turno, recupera o ânimo sozinha.",
            },
            new SkillDef
            {
                Id = SkillId.Photogenic, DisplayName = "Fotogênica", Branch = SkillBranch.Charm,
                RequiredLevel = 16, Description = "Os fãs pagam o dobro por uma foto com ela.",
            },
        };

        public static IReadOnlyList<SkillDef> All => Defs;

        public static SkillDef ById(SkillId id)
        {
            for (var i = 0; i < Defs.Length; i++)
            {
                if (Defs[i].Id == id) return Defs[i];
            }

            throw new KeyNotFoundException("No skill called " + id + ".");
        }

        public static IReadOnlyList<SkillDef> Branch(SkillBranch branch)
        {
            var found = new List<SkillDef>();
            for (var i = 0; i < Defs.Length; i++)
            {
                if (Defs[i].Branch == branch) found.Add(Defs[i]);
            }

            return found;
        }

        /// <summary>What this maid is high enough level to learn and has not already learned.</summary>
        public static IReadOnlyList<SkillDef> AvailableTo(Maid maid)
        {
            var found = new List<SkillDef>();
            for (var i = 0; i < Defs.Length; i++)
            {
                if (Defs[i].RequiredLevel <= maid.Level && !maid.Skills.Contains(Defs[i].Id))
                {
                    found.Add(Defs[i]);
                }
            }

            return found;
        }

        public static bool Has(Maid maid, SkillId id) => maid != null && maid.Skills.Contains(id);

        /// <summary>
        /// Installs <see cref="Apply"/> into the maid maths. Called once at startup.
        /// </summary>
        /// <remarks>
        /// A hook rather than a direct call so that <c>Maids</c> does not depend on
        /// <c>Progress</c>; the simulation's dependencies run one way.
        /// </remarks>
        public static void Install() => MaidMath.SkillHook = Apply;

        /// <summary>The multiplier half of the skill list.</summary>
        public static MaidDerived Apply(Maid maid, MaidDerived derived, ShiftSnapshot moment)
        {
            if (maid == null || maid.Skills.Count == 0) return derived;

            var move = 1f;
            var greet = 1f;
            var serve = 1f;
            var cook = 1f;
            var tip = 1f;
            var fumble = derived.FumbleChance;

            for (var i = 0; i < maid.Skills.Count; i++)
            {
                switch (maid.Skills[i])
                {
                    case SkillId.QuickFeet:
                        move *= 1.15f;
                        break;

                    case SkillId.Usher:
                        greet *= 1.4f;
                        break;

                    case SkillId.TableSense:
                        if (moment.Work == WorkKind.TakeOrder) serve *= 1.3f;
                        break;

                    case SkillId.CleanSweep:
                        if (moment.Work == WorkKind.Bus) serve *= 1.6f;
                        break;

                    case SkillId.Multitask:
                        move *= 1.1f;
                        greet *= 1.1f;
                        serve *= 1.1f;
                        cook *= 1.1f;
                        break;

                    case SkillId.SteadyHands:
                        fumble *= 0.5f;
                        break;

                    case SkillId.Prep:
                        cook *= 1.15f;
                        break;

                    case SkillId.HeatControl:
                        cook *= 1.25f;
                        break;

                    case SkillId.BatchCooking:
                        if (moment.QueuedTickets >= 3) cook *= 1.35f;
                        break;

                    case SkillId.Plating:
                        tip *= 1.15f;
                        break;

                    case SkillId.SweetTalk:
                        tip *= 1.15f;
                        break;
                }
            }

            return derived.Scaled(move, greet, serve, cook, tip).WithFumble(fumble, derived.DropChance);
        }
    }
}
