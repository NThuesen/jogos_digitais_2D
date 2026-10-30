using System.Collections.Generic;

namespace Neko.Game.Ui
{
    /// <summary>
    /// Every word the player reads.
    /// </summary>
    /// <remarks>
    /// Portuguese, in one table, because the course is Brazilian and the graders read the
    /// screen before they read the code. A test asserts that every key a screen asks for
    /// exists, which is what stops a hurried placeholder shipping as a label.
    /// </remarks>
    public static class Strings
    {
        private static readonly Dictionary<string, string> Table =
            new Dictionary<string, string>
            {
                // Identity
                { "game.title", "Neko Maid Inc." },
                { "game.subtitle", "escale suas maids. o dia corre sozinho." },

                // Menu
                { "menu.play", "Abrir a empresa" },
                { "menu.continue", "Continuar" },
                { "menu.newgame", "Nova empresa" },
                { "menu.newgameask", "Começar uma empresa nova? A atual se perde." },
                { "menu.records", "Recordes" },
                { "menu.bestday", "Melhor dia" },
                { "menu.bestrevenue", "Melhor faturamento" },
                { "menu.servedtotal", "Clientes atendidos" },
                { "menu.lost", "Empresas falidas" },
                { "menu.legendaries", "Lendárias encontradas" },

                // The hub
                { "hub.title", "A empresa" },
                { "hub.open", "Abrir o dia" },
                { "hub.lineup", "Escalação" },
                { "hub.roster", "Elenco" },
                { "hub.shop", "Loja" },
                { "hub.boxes", "Caixas" },
                { "hub.hall", "Salão" },
                { "hub.day", "Dia" },
                { "hub.money", "Caixa" },
                { "hub.points", "Fichas" },
                { "hub.level", "Nível" },
                { "hub.fixedcosts", "Custo fixo por dia" },
                { "hub.danger", "O caixa não cobre dois dias de custo. Corte despesa ou lucre hoje." },
                { "hub.tonext", "para o próximo nível" },

                // Lineup
                { "lineup.title", "Escalação de hoje" },
                { "lineup.chemistry", "Química" },
                { "lineup.bench", "Banco" },
                { "lineup.empty", "vazio" },
                { "lineup.auto", "Escalar sozinho" },
                { "lineup.clear", "Limpar" },
                { "lineup.pick", "Escolha quem fica aqui" },
                { "lineup.remove", "Tirar" },
                { "lineup.inposition", "Em posição" },
                { "lineup.outofposition", "Fora de posição" },
                { "lineup.natural", "Posição natural" },
                { "lineup.links", "Entrosamento" },
                { "lineup.theme", "Combina com o salão" },
                { "lineup.needall", "Recepção, salão e cozinha precisam de alguém." },
                { "lineup.ready", "Pronto para abrir" },

                // Positions
                { "role.reception", "Recepção" },
                { "role.floor", "Salão" },
                { "role.kitchen", "Cozinha" },
                { "role.stage", "Palco" },
                { "role.management", "Gerência" },

                // Watching the day
                { "day.title", "O dia" },
                { "day.clock", "Relógio" },
                { "day.till", "Caixa do dia" },
                { "day.served", "Atendidos" },
                { "day.wrong", "Pedidos errados" },
                { "day.angry", "Foram embora" },
                { "day.closing", "Fechando!" },
                { "day.speed", "Velocidade" },
                { "day.skip", "Pular para o fim" },
                { "day.watching", "Você não pode interferir. O dia é da escalação." },

                // Dashboard
                { "books.title", "Balanço do dia" },
                { "books.in", "Entrou" },
                { "books.out", "Saiu" },
                { "books.dishes", "Pratos servidos" },
                { "books.tips", "Gorjetas" },
                { "books.wages", "Salários" },
                { "books.rent", "Aluguel" },
                { "books.ingredients", "Ingredientes" },
                { "books.revenue", "Receita" },
                { "books.costs", "Despesa" },
                { "books.profit", "Lucro" },
                { "books.loss", "Prejuízo" },
                { "books.balance", "Caixa depois" },
                { "books.points", "Fichas ganhas" },
                { "books.wrong", "Pedidos errados" },
                { "books.burnt", "Pratos queimados" },
                { "books.angry", "Clientes perdidos" },
                { "books.turned", "Não couberam" },
                { "books.satisfaction", "Satisfação" },
                { "books.chemistry", "Química do time" },
                { "books.levelup", "Subiu de nível!" },
                { "books.continue", "Voltar à empresa" },
                { "books.warning", "Atenção: o caixa está apertado." },

                // Bankruptcy
                { "bust.title", "Falência" },
                { "bust.body",
                    "O caixa zerou e a Neko Maid Inc. fechou as portas. Seus recordes ficam; " +
                    "a empresa, não." },
                { "bust.restart", "Abrir outra empresa" },

                // Roster
                { "roster.title", "Elenco" },
                { "roster.level", "Nível" },
                { "roster.salary", "Salário" },
                { "roster.rarity", "Raridade" },
                { "roster.traits", "Traços" },
                { "roster.notraits", "Nenhum traço" },
                { "roster.tags", "Etiquetas" },
                { "roster.serve", "Salão" },
                { "roster.cook", "Cozinha" },
                { "roster.charm", "Charme" },
                { "roster.stamina", "Fôlego" },
                { "roster.mood", "Ânimo" },
                { "roster.bond", "Afinidade" },
                { "roster.dismiss", "Dispensar" },
                { "roster.dismissask", "Dispensar e receber fichas de volta?" },
                { "roster.gear", "Vestuário" },
                { "roster.empty", "Nada equipado" },
                { "roster.setbonus", "Conjunto completo" },
                { "roster.signature", "Marca registrada" },
                { "roster.skills", "Habilidades" },
                { "roster.full", "O elenco está no limite deste nível." },

                // Shop
                { "shop.title", "Loja" },
                { "shop.buy", "Comprar" },
                { "shop.owned", "No estoque" },
                { "shop.furniture", "Móveis" },
                { "shop.clothes", "Roupas" },
                { "shop.gifts", "Presentes" },
                { "shop.forwho", "Para quem?" },

                // Boxes
                { "box.title", "Caixas" },
                { "box.open", "Abrir" },
                { "box.odds", "Chances" },
                { "box.got", "Veio na caixa" },
                { "box.duplicate", "Elenco cheio — virou ficha" },
                { "box.again", "Abrir outra" },
                { "box.done", "Guardar" },
                { "box.nopoints", "Fichas insuficientes." },

                // Hall
                { "hall.title", "Arrumar o salão" },
                { "hall.ambience", "Ambiente" },
                { "hall.flow", "Caminhada" },
                { "hall.seats", "Lugares" },
                { "hall.theme", "Tema" },
                { "hall.notheme", "sem tema" },
                { "hall.stock", "No estoque" },
                { "hall.place", "Clique no salão para colocar" },
                { "hall.rotate", "R gira" },
                { "hall.blocked", "Não cabe aí." },

                // Shared
                { "dialog.ok", "Entendi" },
                { "dialog.cancel", "Cancelar" },
                { "dialog.yes", "Sim" },
                { "dialog.no", "Não" },
                { "dialog.back", "Voltar" },
                { "error.title", "Algo deu errado" },
                { "error.day", "O dia terminou mais cedo por um problema interno." },
                { "settings.music", "Música" },
                { "settings.sfx", "Efeitos" },

                { "credits.body",
                    "Neko Maid Inc. O chibi Suzu usa pixel art original deste projeto. " +
                    "Os demais gráficos são gerados em tempo de execução e todos os sons " +
                    "são sintetizados. Não há assets de terceiros neste projeto." },
            };

        public static IReadOnlyDictionary<string, string> All => Table;

        /// <summary>
        /// The text for a key, or the key itself in brackets.
        /// </summary>
        /// <remarks>
        /// Visibly wrong rather than empty: a missing label that renders as nothing looks like
        /// a layout bug and gets chased for an hour.
        /// </remarks>
        public static string Get(string key) =>
            key != null && Table.TryGetValue(key, out var text) ? text : "[" + key + "]";

        public static bool Has(string key) => key != null && Table.ContainsKey(key);

        /// <summary>Money, the way every screen writes it.</summary>
        public static string Money(int amount) => amount.ToString("N0", Culture) + " ¥";

        public static string Signed(int amount) =>
            (amount >= 0 ? "+" : "−") + System.Math.Abs(amount).ToString("N0", Culture) + " ¥";

        private static readonly System.Globalization.CultureInfo Culture =
            new System.Globalization.CultureInfo("pt-BR");
    }
}
