using System;
using System.Collections.Generic;
using Neko.Sim.Maids;

namespace Neko.Sim.Team
{
    /// <summary>
    /// The five places a maid can be put.
    /// </summary>
    /// <remarks>
    /// The whole game is this enum. The player has no controls during a day; every decision
    /// they make is which maid goes in which of these, so each one has to do something
    /// visibly different and each one has to be the best home for somebody.
    /// </remarks>
    public enum Role
    {
        /// <summary>Greets whoever comes in and walks them to a table.</summary>
        Reception = 0,

        /// <summary>Takes orders, carries plates, clears tables.</summary>
        Floor,

        /// <summary>Cooks the tickets.</summary>
        Kitchen,

        /// <summary>Performs. Holds the whole room's patience up and does no tasks.</summary>
        Stage,

        /// <summary>Runs the till. Raises tips and the day's points, and does no tasks.</summary>
        Management,
    }

    public sealed class RoleProfile
    {
        public Role Kind;
        public string DisplayName;

        /// <summary>Portuguese. The player reads this on the lineup screen.</summary>
        public string Description;

        /// <summary>The stat this position is actually graded on.</summary>
        public StatKind Primary;

        public StatKind Secondary;

        /// <summary>False for Stage and Management: they shape the day without touching it.</summary>
        public bool TakesTasks;

        public string ShapeId;
        public string TintHex;
    }

    public static class RoleTable
    {
        /// <summary>What playing out of position costs. A third of her, and all of the bonus.</summary>
        public const float OutOfPositionPenalty = 0.35f;

        private static readonly RoleProfile[] Profiles =
        {
            new RoleProfile
            {
                Kind = Role.Reception, DisplayName = "Recepção",
                Description = "Recebe quem chega e leva à mesa. Charme e salão.",
                Primary = StatKind.Charm, Secondary = StatKind.Serve, TakesTasks = true,
                ShapeId = "Door", TintHex = "#D68946",
            },
            new RoleProfile
            {
                Kind = Role.Floor, DisplayName = "Salão",
                Description = "Anota pedidos, leva pratos e limpa mesas. Salão acima de tudo.",
                Primary = StatKind.Serve, Secondary = StatKind.Stamina, TakesTasks = true,
                ShapeId = "Plate", TintHex = "#6BAED0",
            },
            new RoleProfile
            {
                Kind = Role.Kitchen, DisplayName = "Cozinha",
                Description = "Cozinha as comandas. Cozinha, e fôlego para aguentar o turno.",
                Primary = StatKind.Cook, Secondary = StatKind.Stamina, TakesTasks = true,
                ShapeId = "Teardrop", TintHex = "#C9B04A",
            },
            new RoleProfile
            {
                Kind = Role.Stage, DisplayName = "Palco",
                Description = "Não atende ninguém: segura a paciência do salão inteiro.",
                Primary = StatKind.Charm, Secondary = StatKind.Stamina, TakesTasks = false,
                ShapeId = "Note", TintHex = "#9940B5",
            },
            new RoleProfile
            {
                Kind = Role.Management, DisplayName = "Gerência",
                Description = "Cuida do caixa: mais gorjeta e mais fichas no fim do dia.",
                Primary = StatKind.Stamina, Secondary = StatKind.Charm, TakesTasks = false,
                ShapeId = "Ring", TintHex = "#636AC5",
            },
        };

        public static IReadOnlyList<RoleProfile> All => Profiles;

        public static RoleProfile For(Role role)
        {
            for (var i = 0; i < Profiles.Length; i++)
            {
                if (Profiles[i].Kind == role) return Profiles[i];
            }

            throw new ArgumentOutOfRangeException(nameof(role), role, "No profile for this role.");
        }

        /// <summary>
        /// Where each personality belongs.
        /// </summary>
        /// <remarks>
        /// Read straight off the archetype rules rather than rolled: Mukuchi is bad at the
        /// door and fast at the stove, so the kitchen is her home, and the player can work
        /// that out by reading her card instead of by losing a day.
        /// </remarks>
        public static Role NaturalFor(Archetype archetype)
        {
            switch (archetype)
            {
                case Archetype.Genki: return Role.Floor;
                case Archetype.Kuudere: return Role.Kitchen;
                case Archetype.Tsundere: return Role.Floor;
                case Archetype.Dojikko: return Role.Reception;
                case Archetype.Ojousama: return Role.Reception;
                case Archetype.Oneesan: return Role.Management;
                case Archetype.Idol: return Role.Stage;
                case Archetype.Gourmand: return Role.Kitchen;
                default: return Role.Kitchen;
            }
        }
    }
}
