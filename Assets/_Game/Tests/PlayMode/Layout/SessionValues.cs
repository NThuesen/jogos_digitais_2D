using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Neko.Game.Ui;
using Neko.Sim.Cafe;
using Neko.Sim.Core;
using Neko.Sim.Economy;
using Neko.Sim.Maids;
using Neko.Sim.Progress;
using Neko.Sim.Team;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Tests.PlayMode.Layout
{
    // The value providers of the session builders: the Large_Values, the Longest_Values, and the
    // display replacement for the Large_Values that no data reaches.
    public static partial class SessionFactory
    {
        // -------------------------------------------------------------------------------
        // Large_Values
        // -------------------------------------------------------------------------------

        /// <summary>
        /// The glossary's Large_Values: for each number the UI shows, the largest value it must fit.
        /// </summary>
        /// <remarks>
        /// A state puts the values a save holds (money, fichas, day, records, stock) into its
        /// profile and those a day produces into the <c>DayLedger</c> or the <c>Shift</c>; the game
        /// formats them itself. The texts below are the same values formatted the way the
        /// Baseline formats them, for messages and expectations. Salary, fixed cost and severance
        /// are computed by <c>Neko.Sim</c> from the roster, so no data reaches them: a state
        /// replaces their display with
        /// <see cref="ReplaceDisplay(Transform, Func{int, string}, int, int)"/> and the
        /// compositions below.
        /// </remarks>
        public static class LargeValues
        {
            /// <summary>Balances: the Empresa "Caixa", the Loja balance, "Caixa depois". 9.999.999 ¥.</summary>
            public const int Balance = 9999999;

            /// <summary>
            /// Per-day amounts: ledger rows, "Receita", "Despesa", "Lucro" and "Prejuízo", "Caixa do
            /// dia", "Custo fixo por dia", "Melhor faturamento". 99.999 ¥.
            /// </summary>
            public const int DayAmount = 99999;

            /// <summary>Fichas. 99.999.</summary>
            public const int Fichas = 99999;

            /// <summary>"Dia" and "Melhor dia". 999.</summary>
            public const int Day = 999;

            /// <summary>Shift counts and Balanço tallies. 999.</summary>
            public const int Tally = 999;

            /// <summary>"Clientes atendidos". 999.999.</summary>
            public const int CustomersServed = 999999;

            /// <summary>"Lendárias encontradas". 99.</summary>
            public const int LegendariesFound = 99;

            /// <summary>"Empresas falidas". 99.</summary>
            public const int BusinessesLost = 99;

            /// <summary>Owned stock, per item. 99.</summary>
            public const int StockPerItem = 99;

            /// <summary>A maid's salary. 9.999 ¥.</summary>
            public const int Salary = 9999;

            /// <summary>A maid's severance, in fichas. 999.</summary>
            public const int Severance = 999;

            /// <summary>Salão "Lugares": two integer digits.</summary>
            public const int Seats = 99;

            /// <summary>Salão "Ambiente": two integer digits and the one decimal the Salão writes.</summary>
            public const float Ambience = 99.9f;

            /// <summary>Salão "Caminhada": two integer digits and the one decimal the Salão writes.</summary>
            public const float Walk = 99.9f;

            /// <summary>
            /// "9.999.999 ¥": <see cref="Balance"/> through <c>Strings.Money</c>, as the Empresa, the
            /// Loja and "Caixa depois" write a balance.
            /// </summary>
            public static string BalanceText => Strings.Money(Balance);

            /// <summary>
            /// "99.999 ¥": <see cref="DayAmount"/> through <c>Strings.Money</c>, as the ledger rows,
            /// "Caixa do dia", "Custo fixo por dia" and "Melhor faturamento" write an amount.
            /// </summary>
            public static string DayAmountText => Strings.Money(DayAmount);

            /// <summary>"+99.999 ¥": a profit of <see cref="DayAmount"/> through <c>Strings.Signed</c>, as "Lucro" writes it.</summary>
            public static string ProfitText => Strings.Signed(DayAmount);

            /// <summary>"−99.999 ¥": a loss of <see cref="DayAmount"/> through <c>Strings.Signed</c>, as "Prejuízo" writes it.</summary>
            public static string LossText => Strings.Signed(-DayAmount);

            /// <summary>"9.999 ¥": <see cref="Salary"/> through <c>Strings.Money</c>.</summary>
            public static string SalaryText => Strings.Money(Salary);

            /// <summary>The Elenco detail's "Salário" value, as <c>RosterScreen</c> composes it: "1.234 ¥".</summary>
            public static string SalaryFact(int salary) => Strings.Money(salary);

            /// <summary>
            /// The salary line of a card that shows its salary, on the Elenco grid, as
            /// <c>MaidCard</c> composes it: "Salário 1.234 ¥".
            /// </summary>
            public static string CardSalary(int salary) => Strings.Get("roster.salary") + " " + Strings.Money(salary);

            /// <summary>The Empresa "Custo fixo por dia" figure, as <c>HubScreen</c> composes it: "1.234 ¥".</summary>
            public static string HubFixedCost(int fixedCost) => Strings.Money(fixedCost);

            /// <summary>
            /// The Elenco header, as <c>RosterScreen</c> composes it: "8 / 8   ·   Custo fixo por dia
            /// 1.234 ¥".
            /// </summary>
            public static string RosterHeader(int maids, int rosterCap, int fixedCost) =>
                maids + " / " + rosterCap + "   ·   " + Strings.Get("hub.fixedcosts") + " " + Strings.Money(fixedCost);

            /// <summary>The Elenco "Dispensar" plank label, as <c>RosterScreen</c> composes it: "Dispensar (+120)".</summary>
            public static string DismissPlank(int severance) => Strings.Get("roster.dismiss") + " (+" + severance + ")";
        }

        // -------------------------------------------------------------------------------
        // Longest_Values
        // -------------------------------------------------------------------------------

        /// <summary>
        /// The glossary's Longest_Values: for each text field, the candidate from the game's own
        /// tables that renders widest in <c>Typeface.Font</c> at the size the Game_UI draws that
        /// field.
        /// </summary>
        /// <remarks>
        /// Picked at test time, so a longer entry added to a table is measured without a change
        /// here. A provider lists the field's candidates in table order, measures each with
        /// <see cref="RenderedWidth"/> and ranks them widest first. The dynamic font's advances are
        /// whole pixels at scale factor 1, so widths are whole units and ties are common; a tie
        /// keeps table order. A provider whose tables give no candidate fails with its name. Each
        /// provider names its
        /// size: the one the Baseline passes to <c>UiKit.Label</c> for that field, at card scale 1
        /// for a card label. A field drawn at several sizes is measured at the one named.
        ///
        /// Shift failure reasons, save messages and purchase refusals are not tables; the
        /// Audit_State builders get them from the real code paths.
        /// </remarks>
        public static class Longest
        {
            /// <summary>
            /// The card's name label at card scale 1, bold: <c>MaidCard</c> draws it at 20 × the
            /// card scale.
            /// </summary>
            public const int CardNameSize = 20;

            /// <summary>The card's archetype label at card scale 1: <c>MaidCard</c> draws it at 13 × the card scale.</summary>
            public const int CardArchetypeSize = 13;

            /// <summary>
            /// The card's position tab at card scale 1, bold: <c>MaidCard</c> draws it at 12 × the
            /// card scale.
            /// </summary>
            public const int CardPositionSize = 12;

            /// <summary>The private name pool of <c>Lootbox</c>, which <c>Lootbox.PickName</c> draws from.</summary>
            public const string NamePoolField = "Names"; // historical: the pool is now MaidCatalog

            private static TextGenerator _generator;

            /// <summary>
            /// A maid's name, the numbered fallback included: the closed maid catalog,
            /// read by reflection, and each of its names as <see cref="Numbered"/> numbers it. The
            /// pick is the widest pool name + " 31", for example "Kaede 31". Measured as the card's
            /// name label, <see cref="CardNameSize"/>, bold.
            /// </summary>
            public static LongestPick<string> MaidName()
            {
                var pool = NamePool("MaidName");
                return Widest("MaidName", pool, name => name, CardNameSize, true);
            }

            /// <summary>
            /// A maid's name as a box gives it while the pool lasts: the private <c>Lootbox</c> name
            /// pool alone, for a state with several maids, which takes distinct names from
            /// <see cref="LongestPick{T}.Top"/>. Measured as <see cref="MaidName"/>.
            /// </summary>
            public static LongestPick<string> PoolName() =>
                Widest("PoolName", NamePool("PoolName"), name => name, CardNameSize, true);

            /// <summary><paramref name="name"/> as <c>Lootbox.PickName</c> numbers it when the pool is used up.</summary>
            /// <remarks>
            /// <c>PickName</c> falls back only when every name of the pool is taken, so when the
            /// roster holds at least <paramref name="poolSize"/> maids, and it numbers the new maid
            /// <c>Maids.Count + 1</c>. The first number it can give is <paramref name="poolSize"/> + 1,
            /// 31 for the pool of 30.
            /// </remarks>
            public static string Numbered(string name, int poolSize) => name + " " + (poolSize + 1);

            /// <summary>
            /// An archetype's name: <c>ArchetypeTable.All</c>, <c>DisplayName</c>. Measured as the
            /// card's archetype label, <see cref="CardArchetypeSize"/>.
            /// </summary>
            public static LongestPick<ArchetypeProfile> ArchetypeName() =>
                Widest("ArchetypeName", ArchetypeTable.All, profile => profile.DisplayName, CardArchetypeSize, false);

            /// <summary>
            /// A position's name: <c>RoleTable.All</c>, <c>DisplayName</c>. Measured as the card's
            /// position tab, <see cref="CardPositionSize"/>, bold; the Escalação headers and the Menu
            /// plates draw it at <c>Typeface.Tiny</c>, bold, and the O dia chips at
            /// <c>Typeface.Tiny</c>.
            /// </summary>
            public static LongestPick<RoleProfile> PositionName() =>
                Widest("PositionName", RoleTable.All, role => role.DisplayName, CardPositionSize, true);

            /// <summary>
            /// A position's description: <c>RoleTable.All</c>, <c>Description</c>. Measured as the
            /// body of the Escalação's "Escolha quem fica aqui" Dialog, <c>Typeface.Body</c>.
            /// </summary>
            public static LongestPick<RoleProfile> PositionDescription() =>
                Widest("PositionDescription", RoleTable.All, role => role.Description, Typeface.Body, false);

            /// <summary>
            /// A maid's chemistry tags as the Elenco detail's "Etiquetas" value writes them,
            /// <c>ChemistryTags.Describe</c> joined with " · ": every house, temperament and origin
            /// through <c>ChemistryTags.NameOf</c>, the origins without <c>ThemeTag.None</c>, which
            /// <c>ChemistryTags.RollOrigin</c> never gives. Measured as that value,
            /// <c>Typeface.Small</c>, bold.
            /// </summary>
            /// <remarks>
            /// A save holds the house and the origin. The temperament is not stored: it follows the
            /// archetype through <c>ChemistryTags.TemperamentFor</c>.
            /// </remarks>
            public static LongestPick<(House House, Temperament Temperament, ThemeTag Origin)> ChemistryTagLine()
            {
                var candidates = new List<(House House, Temperament Temperament, ThemeTag Origin)>();
                foreach (House house in Enum.GetValues(typeof(House)))
                {
                    foreach (Temperament temperament in Enum.GetValues(typeof(Temperament)))
                    {
                        foreach (var origin in Themes()) candidates.Add((house, temperament, origin));
                    }
                }

                return Widest("ChemistryTagLine", candidates,
                    tags => string.Join(" · ", ChemistryTags.NameOf(tags.House), ChemistryTags.NameOf(tags.Temperament),
                        ChemistryTags.NameOf(tags.Origin)),
                    Typeface.Small, true);
            }

            /// <summary>
            /// A trait line as the Elenco detail writes it, name, " — " and description:
            /// <c>TraitTable.All</c>, at <c>Typeface.Tiny</c>.
            /// </summary>
            public static LongestPick<TraitDef> TraitLine() =>
                Widest("TraitLine", TraitTable.All, def => def.DisplayName + " — " + def.Description, Typeface.Tiny, false);

            /// <summary>
            /// A bond signature line as the Elenco detail writes it at bond 5, "Marca registrada: "
            /// and <c>BondTable.SignatureFor(archetype, BondTable.MaxLevel)</c>, for each archetype of
            /// <c>ArchetypeTable.All</c> that has a signature, at <c>Typeface.Tiny</c>, bold.
            /// </summary>
            public static LongestPick<Archetype> SignatureLine() =>
                Widest("SignatureLine", ArchetypeTable.All.Select(profile => profile.Kind), kind =>
                {
                    var signature = BondTable.SignatureFor(kind, BondTable.MaxLevel);
                    return string.IsNullOrEmpty(signature) ? null : Strings.Get("roster.signature") + ": " + signature;
                }, Typeface.Tiny, true);

            /// <summary>
            /// An archetype's flavour line: <c>ArchetypeTable.All</c>, <c>Flavour</c>. Measured as
            /// the Elenco detail draws it, <c>Typeface.Tiny</c>.
            /// </summary>
            public static LongestPick<ArchetypeProfile> FlavourLine() =>
                Widest("FlavourLine", ArchetypeTable.All, profile => profile.Flavour, Typeface.Tiny, false);

            /// <summary>
            /// The name of an item on the Loja tab <paramref name="tab"/>: the items of that kind in
            /// <c>Shop.StockFor(cafeLevel)</c>, <c>DisplayName</c>. Measured as the Loja row's name,
            /// <c>Typeface.Body</c>, bold. At <see cref="CafeLevelTable.MaxLevel"/> every item is for
            /// sale.
            /// </summary>
            public static LongestPick<ShopItem> ShopItemName(ShopItemKind tab, int cafeLevel = CafeLevelTable.MaxLevel) =>
                Widest("ShopItemName(" + tab + ", café level " + cafeLevel + ")", StockOn(tab, cafeLevel),
                    item => item.DisplayName, Typeface.Body, true);

            /// <summary>
            /// The description of an item on the Loja tab <paramref name="tab"/>: the items of that
            /// kind in <c>Shop.StockFor(cafeLevel)</c>, <c>Description</c>, which <c>Shop</c> composes
            /// for furniture and clothes. Measured as the Loja row's description,
            /// <c>Typeface.Tiny</c>.
            /// </summary>
            public static LongestPick<ShopItem> ShopItemDescription(ShopItemKind tab, int cafeLevel = CafeLevelTable.MaxLevel) =>
                Widest("ShopItemDescription(" + tab + ", café level " + cafeLevel + ")", StockOn(tab, cafeLevel),
                    item => item.Description, Typeface.Tiny, false);

            /// <summary>
            /// A box's description, which states its odds: <c>Lootbox.All</c>, <c>Description</c>.
            /// Measured as the Caixas draw it, <c>Typeface.Tiny</c>.
            /// </summary>
            public static LongestPick<BoxDef> BoxDescription() =>
                Widest("BoxDescription", Lootbox.All, box => box.Description, Typeface.Tiny, false);

            /// <summary>
            /// A placeable's name: every placeable that
            /// <c>PlaceableCatalog.ForSaleAt(CafeLevelTable.MaxLevel)</c> sells, so not the door,
            /// which is a fixture. Measured as the Salão stock card draws it before its " ×" count,
            /// <c>Typeface.Tiny</c>; the Loja draws the same names through <see cref="ShopItemName"/>.
            /// </summary>
            public static LongestPick<PlaceableDef> PlaceableName() =>
                Widest("PlaceableName", PlaceableCatalog.ForSaleAt(CafeLevelTable.MaxLevel), def => def.DisplayName,
                    Typeface.Tiny, false);

            /// <summary>
            /// A theme's name as the Salão metrics line writes it while the theme bonus is on,
            /// <c>CafeMetrics.DominantTheme.ToString()</c>: every <c>ThemeTag</c> but None, at
            /// <c>Typeface.Small</c>. These are the enum names, "Showa" for example, not the
            /// Portuguese names <c>ChemistryTags.NameOf</c> gives the origin tags.
            /// </summary>
            public static LongestPick<ThemeTag> ThemeName() =>
                Widest("ThemeName", Themes(), theme => theme.ToString(), Typeface.Small, false);

            /// <summary>
            /// Ranks the candidates <paramref name="text"/> gives for <paramref name="items"/> by
            /// their <see cref="RenderedWidth"/> at <paramref name="size"/>, widest first, a tie in
            /// table order; fails with the provider's name, <paramref name="field"/>, when there is no
            /// candidate.
            /// </summary>
            /// <param name="text">The text the field shows for an item; null or empty leaves the item out.</param>
            public static LongestPick<T> Widest<T>(string field, IEnumerable<T> items, Func<T, string> text, int size,
                bool bold)
            {
                var name = "Longest." + field;
                Assert.IsTrue(items != null && text != null, "SessionFactory." + name + ": there is no table to pick from");

                var candidates = new List<LongestCandidate<T>>();
                foreach (var item in items)
                {
                    var shown = text(item);
                    if (string.IsNullOrEmpty(shown)) continue;

                    candidates.Add(new LongestCandidate<T>(item, shown, RenderedWidth(shown, size, bold)));
                }

                Assert.IsTrue(candidates.Count > 0,
                    "SessionFactory." + name + ": the game's tables give it no candidate, so it has no longest value");

                // OrderByDescending is a stable sort, so a tie keeps table order.
                return new LongestPick<T>(name, size, bold, candidates.OrderByDescending(candidate => candidate.Width).ToList());
            }

            /// <summary>
            /// The width of <paramref name="text"/> on one line in <c>Typeface.Font</c> at
            /// <paramref name="size"/>, in Canvas_Units at canvas scale factor 1 (the 1280×720
            /// canvas).
            /// </summary>
            /// <remarks>
            /// <c>TextGenerator.GetPreferredWidth</c> with the font, size, style and rich text that a
            /// <c>UiKit.Label</c> gives its <c>Text</c>, which is how uGUI computes a Text's preferred
            /// width.
            /// </remarks>
            public static float RenderedWidth(string text, int size, bool bold)
            {
                var font = Typeface.Font;
                Assert.IsTrue(font != null, "SessionFactory.Longest: Typeface.Font resolved no font, so no text can be measured");

                var settings = new TextGenerationSettings
                {
                    font = font,
                    fontSize = size,
                    fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
                    lineSpacing = 1f,
                    richText = true,
                    scaleFactor = 1f,
                    color = Color.white,
                    textAnchor = TextAnchor.MiddleCenter,
                    pivot = new Vector2(0.5f, 0.5f),
                    generationExtents = Vector2.zero,
                    horizontalOverflow = HorizontalWrapMode.Overflow,
                    verticalOverflow = VerticalWrapMode.Overflow,
                };

                _generator ??= new TextGenerator();
                return _generator.GetPreferredWidth(text, settings);
            }

            /// <summary>The closed maid catalog, through <see cref="RequireMember{T}"/>.</summary>
            private static List<string> NamePool(string field)
            {
                var pool = new List<string>();
                foreach (var identity in Neko.Sim.Maids.MaidCatalog.All) pool.Add(identity.Name);
                Assert.IsTrue(pool.Count > 0, "SessionFactory.Longest." + field + ": the catalog holds no names");
                return pool;
            }

            /// <summary>Every <c>ThemeTag</c> but None, in enum order.</summary>
            private static List<ThemeTag> Themes()
            {
                var themes = new List<ThemeTag>();
                foreach (ThemeTag theme in Enum.GetValues(typeof(ThemeTag)))
                {
                    if (theme != ThemeTag.None) themes.Add(theme);
                }

                return themes;
            }

            /// <summary>The items of kind <paramref name="tab"/> that the Loja sells at <paramref name="cafeLevel"/>, in its order.</summary>
            private static IEnumerable<ShopItem> StockOn(ShopItemKind tab, int cafeLevel) =>
                Shop.StockFor(cafeLevel).Where(item => item.Kind == tab);
        }

        // -------------------------------------------------------------------------------
        // Display replacement
        // -------------------------------------------------------------------------------

        /// <summary>
        /// Sets the one active label under <paramref name="layer"/> that reads
        /// <paramref name="current"/> to <paramref name="replacement"/>, and returns it; fails
        /// unless exactly one label reads it, listing the labels that do.
        /// </summary>
        /// <remarks>
        /// For a Large_Value that only <c>Neko.Sim</c> computes. A state calls it after the
        /// Screen's last <c>Refresh</c> and before <see cref="Settle"/>: a later <c>Refresh</c>
        /// rebuilds the label from the session and puts the real value back, which
        /// <see cref="Settle"/> reports as a changed text. A label matches when its whole text
        /// equals <paramref name="current"/>, so a label that only contains it, the card's
        /// "Salário 1.234 ¥" for the detail's "1.234 ¥", does not match; when none matches, the
        /// failure lists the labels that contain it. A TrackedLabel holds one letter per Text, so
        /// it never matches.
        /// </remarks>
        public static Text ReplaceDisplay(Transform layer, string current, string replacement)
        {
            Assert.IsTrue(layer != null, "SessionFactory.ReplaceDisplay: there is no layer to find \"" + current + "\" in");
            Assert.IsFalse(string.IsNullOrEmpty(current),
                "SessionFactory.ReplaceDisplay: there is no display string to replace under " + LayoutModel.PathOf(layer));
            Assert.IsFalse(string.IsNullOrEmpty(replacement),
                "SessionFactory.ReplaceDisplay: \"" + current + "\" under " + LayoutModel.PathOf(layer) + " has no replacement");

            var matches = new List<Text>();
            var containing = new List<Text>();
            foreach (var label in layer.GetComponentsInChildren<Text>())
            {
                if (!label.isActiveAndEnabled || string.IsNullOrEmpty(label.text)) continue;

                if (label.text == current) matches.Add(label);
                else if (label.text.Contains(current)) containing.Add(label);
            }

            if (matches.Count == 1)
            {
                matches[0].text = replacement;
                return matches[0];
            }

            var where = LayoutModel.PathOf(layer);
            Assert.Fail(matches.Count > 1
                ? "SessionFactory.ReplaceDisplay: " + matches.Count + " labels under " + where + " read \"" + current +
                  "\", so \"" + replacement + "\" has no single target: " + DescribeLabels(layer, matches)
                : "SessionFactory.ReplaceDisplay: no active label under " + where + " reads \"" + current + "\", so \"" +
                  replacement + "\" has no target" +
                  (containing.Count > 0 ? "; these contain it: " + DescribeLabels(layer, containing) : ""));
            return null;
        }

        /// <summary>
        /// <see cref="ReplaceDisplay(Transform, string, string)"/> with both strings from
        /// <paramref name="compose"/>: the one label that shows <paramref name="shown"/> is composed
        /// again, the same way, from <paramref name="envelope"/>.
        /// </summary>
        /// <example>
        /// <c>ReplaceDisplay(roster.transform, LargeValues.SalaryFact, maid.Salary, LargeValues.Salary)</c>
        /// turns the Elenco detail's "Salário" value into "9.999 ¥".
        /// </example>
        public static Text ReplaceDisplay(Transform layer, Func<int, string> compose, int shown, int envelope)
        {
            Assert.IsTrue(compose != null,
                "SessionFactory.ReplaceDisplay: there is no composition to show " + envelope + " instead of " + shown + " with");
            return ReplaceDisplay(layer, compose(shown), compose(envelope));
        }

        /// <summary>The paths of <paramref name="labels"/> from <paramref name="layer"/>, each with its text.</summary>
        private static string DescribeLabels(Transform layer, List<Text> labels)
        {
            var parts = new List<string>();
            foreach (var label in labels) parts.Add(LayoutModel.PathFrom(layer, label.transform) + " (\"" + label.text + "\")");
            return string.Join(", ", parts);
        }
    }

    /// <summary>
    /// What a <see cref="SessionFactory.Longest"/> provider measured: every candidate of one field,
    /// widest first, and the size and style it measured them at.
    /// </summary>
    public sealed class LongestPick<T>
    {
        internal LongestPick(string field, int size, bool bold, List<LongestCandidate<T>> ranking)
        {
            Field = field;
            Size = size;
            Bold = bold;
            Ranking = ranking;
        }

        /// <summary>The provider, for example "Longest.MaidName".</summary>
        public string Field { get; }

        /// <summary>The font size the candidates were measured at.</summary>
        public int Size { get; }

        /// <summary>Whether they were measured bold.</summary>
        public bool Bold { get; }

        /// <summary>Every candidate, widest first; a tie keeps table order. Never empty.</summary>
        public IReadOnlyList<LongestCandidate<T>> Ranking { get; }

        /// <summary>The table entry that gives the widest text.</summary>
        public T Item => Ranking[0].Item;

        /// <summary>The widest text: the field's Longest_Value.</summary>
        public string Text => Ranking[0].Text;

        /// <summary>Its rendered width, in Canvas_Units at canvas scale factor 1.</summary>
        public float Width => Ranking[0].Width;

        /// <summary>
        /// The items of the <paramref name="count"/> widest candidates, widest first; fails when the
        /// field has fewer.
        /// </summary>
        public List<T> Top(int count)
        {
            Assert.IsTrue(count >= 1 && count <= Ranking.Count,
                "SessionFactory." + Field + ": there is no top " + count + " of its " + Ranking.Count + " candidates");

            var top = new List<T>();
            for (var i = 0; i < count; i++) top.Add(Ranking[i].Item);
            return top;
        }

        /// <summary>The pick, its width and size, and the runner-up, for the log.</summary>
        public override string ToString()
        {
            var line = Field + ": \"" + Text + "\", " + Units(Width) + " at size " + Size + (Bold ? " bold" : "") +
                       ", the widest of " + Ranking.Count;
            return Ranking.Count > 1 ? line + "; next \"" + Ranking[1].Text + "\", " + Units(Ranking[1].Width) : line;
        }

        private static string Units(float width) => width.ToString("0.##", CultureInfo.InvariantCulture) + " units";
    }

    /// <summary>
    /// One candidate of a <see cref="LongestPick{T}"/>: a table entry, the text its field shows for
    /// it, and the rendered width of that text.
    /// </summary>
    public readonly struct LongestCandidate<T>
    {
        public LongestCandidate(T item, string text, float width)
        {
            Item = item;
            Text = text;
            Width = width;
        }

        public T Item { get; }

        public string Text { get; }

        /// <summary>In Canvas_Units at canvas scale factor 1.</summary>
        public float Width { get; }
    }
}
