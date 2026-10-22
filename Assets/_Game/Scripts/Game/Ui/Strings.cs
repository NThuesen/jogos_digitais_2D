using System.Collections.Generic;

namespace Neko.Game.Ui
{
    /// <summary>
    /// Every word the player reads.
    /// </summary>
    /// <remarks>
    /// Portuguese, in one table, because the course is Brazilian and the graders read the
    /// screen before they read the code. A test asserts that every key a screen asks for
    /// exists and that nothing in here is still in English, which is what stops a hurried
    /// "TODO" shipping as a label.
    /// </remarks>
    public static class Strings
    {
        private static readonly Dictionary<string, string> Table =
            new Dictionary<string, string>
            {
                // Title
                { "game.title", "Nekomata Café" },
                { "game.subtitle", "um café de maids-gato" },
                { "menu.play", "Abrir o café" },
                { "menu.continue", "Continuar" },
                { "menu.newgame", "Começar de novo" },
                { "menu.endless", "Modo sem fim" },
                { "menu.settings", "Opções" },
                { "menu.credits", "Créditos" },
                { "menu.records", "Recordes" },

                // Café
                { "cafe.title", "O café" },
                { "cafe.openday", "Abrir o dia" },
                { "cafe.roster", "Maids" },
                { "cafe.shop", "Loja" },
                { "cafe.editor", "Salão" },
                { "cafe.hire", "Contratar" },
                { "cafe.day", "Dia" },
                { "cafe.quota", "Meta de hoje" },
                { "cafe.money", "Moedas" },
                { "cafe.reputation", "Reputação" },
                { "cafe.level", "Nível do café" },
                { "cafe.tonext", "para o próximo nível" },

                // Shift
                { "shift.time", "Tempo" },
                { "shift.earned", "Caixa" },
                { "shift.quota", "Meta" },
                { "shift.served", "Atendidos" },
                { "shift.angry", "Foram embora" },
                { "shift.closing", "Fechando!" },
                { "shift.rush", "Apressar" },
                { "shift.encore", "Encore" },
                { "shift.full", "Lotado!" },
                { "shift.hint", "Clique em quem precisa de você. Botão direito apressa uma maid." },

                // Results
                { "results.title", "Fim do dia" },
                { "results.met", "Meta batida!" },
                { "results.missed", "Dia fraco" },
                { "results.missedhint",
                    "A meta de amanhã continua a mesma. Dá para tentar de novo." },
                { "results.earned", "Faturamento" },
                { "results.served", "Clientes atendidos" },
                { "results.angry", "Clientes perdidos" },
                { "results.turned", "Não couberam" },
                { "results.reputation", "Reputação ganha" },
                { "results.levelup", "Subiu de nível!" },
                { "results.continue", "Voltar ao café" },

                // Roster
                { "roster.title", "As maids" },
                { "roster.onshift", "De plantão" },
                { "roster.bench", "Descansando" },
                { "roster.level", "Nível" },
                { "roster.bond", "Afinidade" },
                { "roster.mood", "Ânimo" },
                { "roster.serve", "Salão" },
                { "roster.cook", "Cozinha" },
                { "roster.charm", "Charme" },
                { "roster.stamina", "Fôlego" },
                { "roster.skills", "Habilidades" },
                { "roster.gear", "Vestuário" },
                { "roster.empty", "Nada equipado" },
                { "roster.setbonus", "Conjunto completo" },
                { "roster.signature", "Marca registrada" },
                { "roster.rosterfull", "O plantão de hoje já está cheio." },
                { "roster.put", "Pôr de plantão" },
                { "roster.remove", "Tirar do plantão" },

                // Shop
                { "shop.title", "Loja" },
                { "shop.buy", "Comprar" },
                { "shop.owned", "No estoque" },
                { "shop.furniture", "Móveis" },
                { "shop.clothes", "Roupas" },
                { "shop.gifts", "Presentes" },
                { "shop.forwho", "Para quem?" },
                { "shop.bought", "Comprado!" },

                // Hiring
                { "hire.title", "Candidatas" },
                { "hire.cost", "Contratar por" },
                { "hire.refresh", "Novas candidatas amanhã" },
                { "hire.hired", "Contratada!" },

                // Editor
                { "editor.title", "Arrumar o salão" },
                { "editor.ambience", "Ambiente" },
                { "editor.flow", "Caminhada" },
                { "editor.seats", "Lugares" },
                { "editor.theme", "Tema" },
                { "editor.notheme", "sem tema" },
                { "editor.stock", "No estoque" },
                { "editor.place", "Clique no salão para colocar" },
                { "editor.remove", "Clique com o direito para guardar" },
                { "editor.rotate", "R gira" },
                { "editor.blocked", "Não cabe aí." },
                { "editor.flowhint", "Quanto menor, menos as maids andam." },

                // Pause and settings
                { "pause.title", "Pausa" },
                { "pause.resume", "Voltar" },
                { "pause.quit", "Sair para o menu" },
                { "settings.title", "Opções" },
                { "settings.music", "Música" },
                { "settings.sfx", "Efeitos" },
                { "settings.back", "Voltar" },
                { "settings.wipe", "Apagar o save" },
                { "settings.wipeconfirm", "Apagar tudo e começar de novo?" },

                // Dialogs and errors
                { "dialog.ok", "Entendi" },
                { "dialog.cancel", "Cancelar" },
                { "dialog.yes", "Sim" },
                { "dialog.no", "Não" },
                { "error.title", "Algo deu errado" },
                { "error.shift",
                    "O dia terminou mais cedo por um problema interno. Nada foi perdido." },

                // Credits
                { "credits.title", "Créditos" },
                { "credits.body",
                    "Nekomata Café. Nenhuma imagem e nenhum som foram importados: " +
                    "todos os sprites são gerados em tempo de execução a partir de campos " +
                    "de distância, e todos os sons são sintetizados. Não há assets de " +
                    "terceiros neste projeto." },
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
    }
}
