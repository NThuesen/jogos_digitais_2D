using System.Collections.Generic;
using Neko.Sim.Maids;

namespace Neko.Sim.Progress
{
    /// <summary>
    /// What knowing a maid better is worth, and what she says about it.
    /// </summary>
    /// <remarks>
    /// The fourth scaling axis, and the only one that is not bought with money. Bond five
    /// grants a Signature: a passive that only that archetype can ever have, which is what
    /// stops a fully-equipped roster converging on the same maid in nine colours.
    /// </remarks>
    public static class BondTable
    {
        public const int MaxLevel = 5;

        /// <summary>Cumulative gift points to reach each bond level.</summary>
        private static readonly int[] Thresholds = { 0, 10, 25, 50, 90, 150 };

        private static readonly Dictionary<Archetype, string> Signatures =
            new Dictionary<Archetype, string>
            {
                { Archetype.Genki, "Nunca cansa de verdade: o ânimo dela cai 30% mais devagar." },
                { Archetype.Kuudere, "Não erra nem com pressa: imune ao custo de ânimo do grito." },
                { Archetype.Tsundere, "O bônus de salão cheio começa com 40% das mesas ocupadas." },
                { Archetype.Dojikko, "Cada prato derrubado vale o dobro de paciência para o salão." },
                { Archetype.Ojousama, "Clientes VIP gastam mais 25% quando ela está de plantão." },
                { Archetype.Oneesan, "A aura dela alcança o salão inteiro." },
                { Archetype.Idol, "O Encore recarrega em metade do tempo." },
                { Archetype.Gourmand, "Ela passou a dividir: come metade do que comia." },
                { Archetype.Mukuchi, "Tudo que ela empresta ao prato conta em dobro na gorjeta." },
            };

        private static readonly Dictionary<Archetype, string[]> Lines =
            new Dictionary<Archetype, string[]>
            {
                {
                    Archetype.Genki, new[]
                    {
                        "Bom dia, chefe! Hoje eu corro mais!",
                        "Já decorei onde fica cada mesa!",
                        "A gente faz um bom time, né?",
                        "Se o salão encher eu dou conta. Confia.",
                        "Esse café é meu também agora.",
                    }
                },
                {
                    Archetype.Kuudere, new[]
                    {
                        "...Bom dia.",
                        "O fogão estava sujo. Resolvi.",
                        "Você trabalha demais.",
                        "Eu fico até fechar. Não precisa pedir.",
                        "Eu gosto daqui. Não repita isso.",
                    }
                },
                {
                    Archetype.Tsundere, new[]
                    {
                        "Não é por você que eu vim, tá?",
                        "O salão estava cheio. Só isso.",
                        "Eu... guardei um pedaço de bolo pra você.",
                        "Para de me olhar. Eu estou trabalhando.",
                        "Tudo bem. Eu gosto daqui. Satisfeito?",
                    }
                },
                {
                    Archetype.Dojikko, new[]
                    {
                        "Desculpa pelo prato de ontem!",
                        "Hoje eu só derrubei dois!",
                        "Os clientes acham graça, acho que ajuda...",
                        "Eu treinei! Olha só! ...ops.",
                        "Obrigada por nunca ter gritado comigo.",
                    }
                },
                {
                    Archetype.Ojousama, new[]
                    {
                        "O estabelecimento é... aceitável.",
                        "Trouxe minha própria xícara.",
                        "Os clientes importantes vêm por minha causa.",
                        "Eu poderia trabalhar em qualquer lugar. Escolhi aqui.",
                        "Considere-se honrado. Eu fico.",
                    }
                },
                {
                    Archetype.Oneesan, new[]
                    {
                        "Deixa que eu ensino as meninas.",
                        "A novata está indo bem, viu?",
                        "Você também precisa descansar.",
                        "Eu cuido do salão. Vai comer alguma coisa.",
                        "Esse lugar virou minha casa.",
                    }
                },
                {
                    Archetype.Idol, new[]
                    {
                        "Palco pequeno, público fiel!",
                        "Ensaiei uma música nova.",
                        "Hoje teve pedido de bis!",
                        "Eu canto melhor quando você está vendo.",
                        "Nunca vou trocar esse salão por um teatro.",
                    }
                },
                {
                    Archetype.Gourmand, new[]
                    {
                        "Eu só provei. Juro.",
                        "O curry de hoje estava ótimo. Eu sei.",
                        "Fiz um a mais. É pra você.",
                        "Comer o que eu cozinho é controle de qualidade.",
                        "Eu divido. Agora eu divido.",
                    }
                },
                {
                    Archetype.Mukuchi, new[]
                    {
                        "...",
                        "(Ela aponta para o fogão.)",
                        "...obrigada.",
                        "O prato ficou bonito hoje. ...Achei.",
                        "Eu fico. Não precisa perguntar de novo.",
                    }
                },
            };

        public static int PointsForLevel(int bondLevel) =>
            Thresholds[Core.Curves.ClampInt(bondLevel, 0, MaxLevel)];

        /// <summary>The bond level a maid with this many gift points has reached.</summary>
        public static int LevelForPoints(int points)
        {
            var level = 0;
            for (var i = 1; i <= MaxLevel; i++)
            {
                if (points >= Thresholds[i]) level = i;
            }

            return level;
        }

        /// <summary>Her unique passive, unlocked at bond five. Empty below that.</summary>
        public static string SignatureFor(Archetype archetype, int bondLevel) =>
            bondLevel >= MaxLevel && Signatures.TryGetValue(archetype, out var text) ? text : "";

        public static string LineFor(Archetype archetype, int bondLevel)
        {
            if (!Lines.TryGetValue(archetype, out var lines)) return "";

            var index = Core.Curves.ClampInt(bondLevel, 1, MaxLevel) - 1;
            return lines[index];
        }
    }
}
