using System;
using System.Collections;
using System.Collections.Generic;
using Neko.Game.Core;
using Neko.Game.Feel;
using Neko.Game.Ui;
using NUnit.Framework;
using CafeLevelTable = Neko.Sim.Economy.CafeLevelTable;
using Rarity = Neko.Sim.Team.Rarity;
using ShopItemKind = Neko.Sim.Economy.ShopItemKind;
using TraitId = Neko.Sim.Team.TraitId;

namespace Neko.Tests.PlayMode.Layout
{
    /// <summary>
    /// One Audit_State of the requirements' Audit States table: the design's "Audit state
    /// definition" (Data Models), plus the <see cref="Views"/> that the audit loop measures inside
    /// the state's one load.
    /// </summary>
    /// <remarks>
    /// The audit loop runs a state in this order: <see cref="StateContext.Begin"/>, then
    /// <see cref="Build"/>, then for each of <see cref="Views"/> <see cref="StateContext.Enter"/>,
    /// <see cref="SessionFactory.Settle"/> and the measurements at 960×600, 1280×720 and 960×600
    /// again, each with every registered rule and <see cref="Checks"/>. It attributes the records
    /// and names the captures with <see cref="Id"/> and the view's <see cref="AuditView.Screen"/>,
    /// and its <c>[TearDown]</c> calls <see cref="StateContext.End"/>. Neither <see cref="Build"/>
    /// nor a view's Enter settles the game; the loop does, once per view.
    ///
    /// Most states have one view: the state as <see cref="Build"/> leaves it, named
    /// <see cref="Screen"/>. A state measured several ways inside one load lists its views: LJ-4
    /// one per Loja tab at café level 10 ("Loja (Móveis)", "Loja (Roupas)", "Loja (Presentes)"),
    /// each entered by selecting its tab and scrolling the list to the end; SL-4 one per café level
    /// ("Salão (nível 1)" to "Salão (nível 10)"), each entered by raising the loaded session to its
    /// level.
    /// </remarks>
    public sealed class AuditState
    {
        /// <summary>
        /// The state of the Audit States row <paramref name="id"/>. Without <paramref name="views"/>,
        /// its one view is the state as <paramref name="build"/> leaves it, at the first of
        /// <paramref name="cafeLevels"/>, named <paramref name="screen"/>.
        /// </summary>
        /// <remarks>
        /// Fails, naming the state, when a member is missing, a café level does not exist, a view
        /// shows a café level that is not one of the state's, a café level has no view, or two
        /// views would write captures of the same name.
        /// </remarks>
        public AuditState(string id, string screen, Func<StateContext, IEnumerator> build,
            Action<Measurement, List<Violation>> checks, int[] cafeLevels, params AuditView[] views)
        {
            Assert.IsFalse(string.IsNullOrEmpty(id), "AuditState: an Audit_State needs its id, such as EM-1");
            Assert.IsFalse(string.IsNullOrEmpty(screen), "AuditState: " + id + " needs the Screen it shows, such as Empresa");
            Assert.IsTrue(build != null, "AuditState: " + id + " needs the Build that puts the game in it");
            Assert.IsTrue(checks != null,
                "AuditState: " + id + " needs its Checks; a state without state checks takes AuditState.NoChecks");
            Assert.IsTrue(cafeLevels != null && cafeLevels.Length > 0, "AuditState: " + id + " needs the café level it is measured at");

            foreach (var level in cafeLevels)
            {
                Assert.IsTrue(level >= 1 && level <= CafeLevelTable.MaxLevel,
                    "AuditState: " + id + " is measured at café level " + level + "; the levels run from 1 to " +
                    CafeLevelTable.MaxLevel);
            }

            var own = views == null || views.Length == 0
                ? new[] { new AuditView(screen, cafeLevels[0], AuditView.Stay) }
                : (AuditView[])views.Clone();

            var captures = new HashSet<string>(StringComparer.Ordinal);
            foreach (var view in own)
            {
                Assert.IsTrue(view != null, "AuditState: " + id + " has a null view");

                var slug = LayoutReport.Slug(view.Screen);
                Assert.IsTrue(captures.Add(slug),
                    "AuditState: two views of " + id + " would write their captures as " + slug + "; the second is " +
                    view.Screen);
                Assert.IsTrue(Array.IndexOf(cafeLevels, view.CafeLevel) >= 0,
                    "AuditState: the view " + view.Screen + " of " + id + " shows café level " + view.CafeLevel +
                    ", which is not one of the levels " + id + " is measured at (" + string.Join(", ", cafeLevels) + ")");
            }

            foreach (var level in cafeLevels)
            {
                var shown = false;
                foreach (var view in own) shown |= view.CafeLevel == level;
                Assert.IsTrue(shown, "AuditState: " + id + " is measured at café level " + level + ", but none of its views shows it");
            }

            Id = id;
            Screen = screen;
            Build = build;
            Checks = checks;
            CafeLevels = (int[])cafeLevels.Clone();
            Views = Array.AsReadOnly(own);
        }

        /// <summary>The id of the Audit States row, such as "LJ-1".</summary>
        public string Id { get; }

        /// <summary>
        /// The Screen or layer the state shows, such as "Loja (Móveis)": the name of its one view,
        /// or, for a state with several views, the Screen they share, such as "Loja".
        /// </summary>
        public string Screen { get; }

        /// <summary>
        /// Crafts the session, boots the game on it and walks to the Screen, at
        /// <see cref="StateContext.CafeLevel"/>, which is the first of <see cref="CafeLevels"/>
        /// while it runs. It keeps what <see cref="Checks"/> read with <see cref="StateContext.Keep{T}"/>.
        /// </summary>
        public Func<StateContext, IEnumerator> Build { get; }

        /// <summary>
        /// The state's containment, separation and click criteria (Requirements 8 to 17), run with
        /// the rules at every measurement of every view; they append their records to the list.
        /// </summary>
        /// <remarks>
        /// They reach the view, the café level and the values <see cref="Build"/> kept through
        /// <see cref="StateContext.Current"/>.
        /// </remarks>
        public Action<Measurement, List<Violation>> Checks { get; }

        /// <summary>The café levels the state is measured at, in order: 1 to 10 for SL-4, one level for every other state.</summary>
        public int[] CafeLevels { get; }

        /// <summary>
        /// The views the audit loop measures inside the state's one load, in order, each under its
        /// own Screen name; every café level of <see cref="CafeLevels"/> is shown by at least one.
        /// </summary>
        public IReadOnlyList<AuditView> Views { get; }

        /// <summary>The <see cref="Checks"/> of a state without state checks of its own; the rules still run.</summary>
        public static void NoChecks(Measurement measurement, List<Violation> violations)
        {
        }
    }

    /// <summary>
    /// One view of an <see cref="AuditState"/>: a Loja tab of LJ-4, a café level of SL-4, or the
    /// state as its Build leaves it. The audit loop measures each view under its own Screen name.
    /// </summary>
    public sealed class AuditView
    {
        /// <param name="screen">The Screen name of the view's records and captures, such as "Salão (nível 3)".</param>
        /// <param name="cafeLevel">The café level the view shows; one of the state's <see cref="AuditState.CafeLevels"/>.</param>
        /// <param name="enter">Switches the built state to the view; <see cref="Stay"/> when there is nothing to switch.</param>
        public AuditView(string screen, int cafeLevel, Func<StateContext, IEnumerator> enter)
        {
            Assert.IsFalse(string.IsNullOrEmpty(screen), "AuditView: a view needs its Screen name, such as \"Loja (Móveis)\"");
            Assert.IsTrue(enter != null,
                "AuditView: the view " + screen + " needs the Enter that switches to it; a view of the state as built takes AuditView.Stay");

            Screen = screen;
            CafeLevel = cafeLevel;
            Enter = enter;
        }

        /// <summary>The Screen name of the view's records and captures, such as "Loja (Móveis)" or "Salão (nível 3)".</summary>
        public string Screen { get; }

        /// <summary>The café level the view shows, which <see cref="StateContext.Enter"/> makes the context's.</summary>
        public int CafeLevel { get; }

        /// <summary>
        /// Switches the built state to the view inside the same load, without a scene load and
        /// without settling: selects a tab, scrolls, raises the café level. Run through
        /// <see cref="StateContext.Enter"/>.
        /// </summary>
        public Func<StateContext, IEnumerator> Enter { get; }

        /// <summary>The Enter of a view that shows the game as it already is.</summary>
        public static IEnumerator Stay(StateContext context)
        {
            yield break;
        }
    }

    /// <summary>
    /// What one run of an Audit_State shares between its <see cref="AuditState.Build"/>, its views
    /// and its <see cref="AuditState.Checks"/>: the game, the view and café level being measured,
    /// and the values the builder kept for the checks, such as the maid it selected.
    /// </summary>
    /// <remarks>
    /// The design's <see cref="AuditState.Checks"/> take only the measurement and the record list,
    /// so they reach the context through <see cref="Current"/>, as tests reach NUnit's
    /// <c>TestContext.CurrentContext</c>. The audit loop makes a context current with
    /// <see cref="Begin"/> and clears it with <see cref="End"/>; Unity runs one test at a time, so
    /// one current context is enough.
    /// </remarks>
    public sealed class StateContext
    {
        private static StateContext _current;

        private readonly Dictionary<string, object> _kept = new Dictionary<string, object>(StringComparer.Ordinal);

        private StateContext(AuditState state)
        {
            State = state;
            CafeLevel = state.CafeLevels[0];
        }

        /// <summary>The context of the Audit_State being audited; fails when there is none.</summary>
        public static StateContext Current
        {
            get
            {
                Assert.IsTrue(_current != null,
                    "StateContext.Current: no Audit_State is being audited; the audit loop starts one with StateContext.Begin");
                return _current;
            }
        }

        /// <summary>The audited state.</summary>
        public AuditState State { get; }

        /// <summary>The view being measured; null while <see cref="AuditState.Build"/> runs.</summary>
        public AuditView View { get; private set; }

        /// <summary>
        /// The café level being built or measured: the first of the state's
        /// <see cref="AuditState.CafeLevels"/> while its Build runs, then the level of <see cref="View"/>.
        /// </summary>
        public int CafeLevel { get; private set; }

        /// <summary>The game; fails when there is none. Valid once the state's Build has booted it.</summary>
        public GameRoot Root
        {
            get
            {
                var root = GameRoot.Instance;
                Assert.IsTrue(root != null, "StateContext: there is no game for " + State.Id + "; its Build boots one with SessionFactory.Boot");
                return root;
            }
        }

        /// <summary>The game's UI, as <see cref="LayoutModel.Measure"/> takes it.</summary>
        public UiRoot Ui => Root.Ui;

        /// <summary>A fresh context for <paramref name="state"/>, made <see cref="Current"/>, at its first café level.</summary>
        public static StateContext Begin(AuditState state)
        {
            Assert.IsTrue(state != null, "StateContext.Begin: there is no Audit_State to begin");
            return _current = new StateContext(state);
        }

        /// <summary>Leaves no context current. For <c>[TearDown]</c>.</summary>
        public static void End() => _current = null;

        /// <summary>
        /// Makes <paramref name="view"/>, one of the state's views, the one being measured, takes
        /// its café level, and runs its <see cref="AuditView.Enter"/>.
        /// </summary>
        public IEnumerator Enter(AuditView view)
        {
            var own = false;
            foreach (var other in State.Views) own |= ReferenceEquals(other, view);
            Assert.IsTrue(own, "StateContext.Enter: " + (view != null ? view.Screen : "no view") + " is not a view of " + State.Id);

            View = view;
            CafeLevel = view.CafeLevel;

            var routine = view.Enter(this);
            Assert.IsTrue(routine != null, "StateContext.Enter: the Enter of " + view.Screen + " in " + State.Id + " returned no routine");
            yield return routine;
        }

        /// <summary>Keeps <paramref name="value"/> under <paramref name="name"/> for the state's Checks, replacing what was kept there.</summary>
        public void Keep<T>(string name, T value)
        {
            Assert.IsFalse(string.IsNullOrEmpty(name), "StateContext.Keep: " + State.Id + " keeps a value without a name");
            Assert.IsTrue(value != null, "StateContext.Keep: " + State.Id + " keeps null under \"" + name + "\"");
            _kept[name] = value;
        }

        /// <summary>
        /// The value kept under <paramref name="name"/>; fails when nothing was kept there, listing
        /// what was, or when it is not a <typeparamref name="T"/>.
        /// </summary>
        public T Kept<T>(string name)
        {
            object value = null;
            if (name == null || !_kept.TryGetValue(name, out value))
            {
                Assert.Fail("StateContext.Kept: " + State.Id + " kept nothing under \"" + name + "\"; it kept " +
                            (_kept.Count > 0 ? string.Join(", ", _kept.Keys) : "nothing"));
            }

            if (!(value is T))
            {
                Assert.Fail("StateContext.Kept: " + State.Id + " kept a " + value.GetType().Name + " under \"" + name +
                            "\", not a " + typeof(T).Name);
            }

            return (T)value;
        }
    }

    /// <summary>
    /// The Audit_States the Layout_Check measures: the implemented states in the order of the
    /// requirements' Audit States table, and that table's 40 ids.
    /// </summary>
    /// <remarks>
    /// A new state goes into <see cref="Create"/> at its place in the table; registering fails when
    /// an id is not in <see cref="TableIds"/>, comes twice or is out of table order.
    /// </remarks>
    public static class AuditStates
    {
        /// <summary>
        /// The seed of the new game EM-1 shows: <c>Profile.NewGame</c> rolls its three openers from
        /// it, and <c>GameSession</c> seeds its <c>Rng</c> from <c>Profile.Seed</c>, so box openings
        /// and days replay exactly. The value spells "NEW1" in ASCII, as
        /// <see cref="SessionFactory.MintSeed"/> spells "NEKO".
        /// </summary>
        public const uint NewGameSeed = 0x4E455731u;

        /// <summary>The 40 ids of the requirements' Audit States table, in table order.</summary>
        public static readonly IReadOnlyList<string> TableIds = Array.AsReadOnly(new[]
        {
            "MN-1", "MN-2", "MN-3",
            "EM-1", "EM-2", "EM-3", "EM-4",
            "ES-1", "ES-2", "ES-3", "ES-4",
            "EL-1", "EL-2", "EL-3", "EL-4", "EL-5",
            "CX-1", "CX-2", "CX-3", "CX-4",
            "LJ-1", "LJ-2", "LJ-3", "LJ-4", "LJ-5", "LJ-6",
            "SL-1", "SL-2", "SL-3", "SL-4",
            "OD-1", "OD-2", "OD-3",
            "BL-1", "BL-2", "BL-3", "BL-4",
            "FL-1",
            "DG-1", "DG-2",
        });

        private static IReadOnlyList<AuditState> _all;
        private static IReadOnlyList<string> _ids;

        /// <summary>The implemented Audit_States, in table order.</summary>
        public static IReadOnlyList<AuditState> All => _all ?? (_all = Register(Create()));

        /// <summary>The ids of <see cref="All"/>, in table order: the <c>[ValueSource]</c> of the audit test.</summary>
        public static IReadOnlyList<string> Ids
        {
            get
            {
                if (_ids != null) return _ids;

                var ids = new List<string>();
                foreach (var state in All) ids.Add(state.Id);
                return _ids = ids.AsReadOnly();
            }
        }

        /// <summary>The implemented Audit_State <paramref name="id"/>; fails with the id when there is none.</summary>
        public static AuditState Get(string id)
        {
            foreach (var state in All)
            {
                if (string.Equals(state.Id, id, StringComparison.Ordinal)) return state;
            }

            Assert.Fail("AuditStates.Get: " +
                        (TableIndex(id) >= 0
                            ? id + " is in the Audit States table but is not implemented yet"
                            : "the Audit States table has no Audit_State " + (id == null ? "(null)" : "\"" + id + "\"")) +
                        "; the implemented states are " + string.Join(", ", Ids));
            return null;
        }

        /// <summary>The implemented states, in table order.</summary>
        private static AuditState[] Create() => new[]
        {
            // UI-01B. Menu: a fresh business, the largest records, and the "Nova empresa" confirmation.
            new AuditState("MN-1", "Menu", context => MenuRoutine(MenuKind.Fresh), AuditState.NoChecks, new[] { 1 }),
            new AuditState("MN-2", "Menu", context => MenuRoutine(MenuKind.Records), MenuShowsTheSavedRecords, new[] { 10 }),
            new AuditState("MN-3", "Menu", context => MenuRoutine(MenuKind.Confirm), DialogBlocksTheLayerBelow, new[] { 10 }),

            // Empresa. Task 6.1 adds the Empresa checks (Requirement 11). EM-2: the largest values.
            new AuditState("EM-1", "Empresa", BuildNewGameDayOne, AuditState.NoChecks, new[] { 1 }),
            new AuditState("EM-2", "Empresa", context => HubRoutine(), AuditState.NoChecks, new[] { 10 }),

            // UI-01B. Escalação: a full lineup, and a legitimate partial one (empty positions, maids on the bench).
            new AuditState("ES-1", "Escalação", context => LineupRoutine(true), AuditState.NoChecks, new[] { 10 }),
            new AuditState("ES-2", "Escalação", context => LineupRoutine(false), AuditState.NoChecks, new[] { 10 }),

            // Elenco (UI-01A). EL-1 holds eight maids, the first selected; EL-2 puts the longest
            // values in the detail of the selected one; EL-3 a maid with no traits.
            new AuditState("EL-1", "Elenco", context => RosterRoutine(RosterKind.Full), AuditState.NoChecks, new[] { 10 }),
            new AuditState("EL-2", "Elenco", context => RosterRoutine(RosterKind.Longest), AuditState.NoChecks, new[] { 10 }),
            new AuditState("EL-3", "Elenco", context => RosterRoutine(RosterKind.NoTraits), AuditState.NoChecks, new[] { 10 }),

            // UI-01B. Caixas: the shelf with the largest fichas, and the reveal after opening one.
            new AuditState("CX-1", "Caixas", context => BoxesRoutine(), AuditState.NoChecks, new[] { 10 }),
            new AuditState("CX-2", "Caixas (resultado)", context => BoxesRoutine(), AuditState.NoChecks, new[] { 10 },
                new AuditView("Caixas (resultado)", 10, OpenTheFirstBox)),

            // Loja (UI-01A), café level 10 stock, one tab each.
            new AuditState("LJ-1", "Loja (Móveis)", context => ShopRoutine(true), AuditState.NoChecks, new[] { 10 }),
            new AuditState("LJ-2", "Loja (Roupas)", context => ShopRoutine(false), AuditState.NoChecks, new[] { 10 },
                new AuditView("Loja (Roupas)", 10, context => PressTab(context, "shop.clothes"))),
            new AuditState("LJ-3", "Loja (Presentes)", context => ShopRoutine(false), AuditState.NoChecks, new[] { 10 },
                new AuditView("Loja (Presentes)", 10, context => PressTab(context, "shop.gifts"))),
            new AuditState("LJ-4", "Loja (Móveis, fim da lista)", context => ShopRoutine(true), LastRowIsReachable, new[] { 10 },
                new AuditView("Loja (Móveis, fim da lista)", 10, ScrollToTheEnd)),

            // UI-01B. Salão with the stock shelf full, then with one piece picked up; both check that no UI
            // sits over a floor tile (a click on a tile places a piece only when nothing blocks it).
            new AuditState("SL-1", "Salão", context => HallRoutine(), TilesAreClickable, new[] { 10 }),
            new AuditState("SL-2", "Salão (peça escolhida)", context => HallRoutine(), TilesAreClickable, new[] { 10 },
                new AuditView("Salão (peça escolhida)", 10, PickTheFirstPiece)),

            // UI-01B. O dia, frozen on its first frame with a full lineup.
            new AuditState("OD-1", "O dia", context => DayRoutine(), AuditState.NoChecks, new[] { 10 }),

            // Balanço (UI-01A): a profitable day, a loss day, and every tally with eight level-ups.
            new AuditState("BL-1", "Balanço", context => BooksRoutine(BooksKind.Profit), AuditState.NoChecks, new[] { 10 }),
            new AuditState("BL-2", "Balanço", context => BooksRoutine(BooksKind.Loss), AuditState.NoChecks, new[] { 10 }),
            new AuditState("BL-3", "Balanço", context => BooksRoutine(BooksKind.Everything), AuditState.NoChecks, new[] { 10 }),

            // UI-01B. Falência, and the Dialogs: a long text with both buttons, and an error over the Salão.
            new AuditState("FL-1", "Falência", context => BustRoutine(), AuditState.NoChecks, new[] { 10 }),
            new AuditState("DG-1", "Diálogo (confirmar/cancelar)", context => DialogRoutine(false), DialogBlocksTheLayerBelow, new[] { 10 }),
            new AuditState("DG-2", "Diálogo (sobre o Salão)", context => DialogRoutine(true), DialogBlocksTheLayerBelow, new[] { 10 }),
        };

        private enum MenuKind { Fresh, Records, Confirm }

        private enum RosterKind { Full, Longest, NoTraits }

        private enum BooksKind { Profit, Loss, Everything }

        private static readonly string[] CastNames = { "Suzu", "Akari", "Momo", "Reina", "Kaede", "Nene", "Ichika" };

        /// <summary>Eight maids at café level 10, a booted game walked to Elenco.</summary>
        private static IEnumerator RosterRoutine(RosterKind kind)
        {
            var builder = SessionFactory.NewProfile(NewGameSeed, 10).WithDay(1)
                .WithMoney(SessionFactory.LargeValues.Balance).EmptyRoom();
            var name = SessionFactory.Longest.MaidName().Item;

            if (kind == RosterKind.Longest) builder.WithMaid(name, Rarity.Legendary, 20, 5, TraitId.AngelHands, TraitId.QuickStep, TraitId.Charisma);
            else if (kind == RosterKind.NoTraits) builder.WithMaid(name, Rarity.Common, 1, 0);
            else builder.WithMaid("Hina", Rarity.Rare, 12, 3, TraitId.Lucky);

            var rarities = new[] { Rarity.Common, Rarity.Rare, Rarity.Epic, Rarity.Legendary };
            for (var i = 0; i < CastNames.Length; i++)
            {
                builder.WithMaid(CastNames[i], rarities[i % 4], 3 + i * 2, i % 6, TraitId.Hostess);
            }

            yield return SessionFactory.Boot(builder.Build());
            yield return SessionFactory.ToHub();
            yield return SessionFactory.ToRoster();
        }

        /// <summary>Café level 10 with every item owned many times, a booted game walked to the Shop.</summary>
        private static IEnumerator ShopRoutine(bool large)
        {
            var builder = SessionFactory.NewProfile(NewGameSeed, 10).WithDay(1)
                .WithMoney(large ? SessionFactory.LargeValues.Balance : 1500);
            builder.WithMaid(SessionFactory.Longest.MaidName().Item, Rarity.Rare, 5, 1, TraitId.Lucky);
            foreach (var item in Neko.Sim.Economy.Shop.StockFor(10))
            {
                if (item.Kind != ShopItemKind.Gift) builder.WithStock(item.Id, SessionFactory.LargeValues.StockPerItem);
            }

            yield return SessionFactory.Boot(builder.Build());
            yield return SessionFactory.ToHub();
            yield return SessionFactory.ToShop();
        }

        /// <summary>Scrolls the Screen's scroll area to its end, by the same position a drag to the end reaches.</summary>
        private static IEnumerator ScrollToTheEnd(StateContext context)
        {
            var scroll = context.Ui.Current.GetComponentInChildren<UnityEngine.UI.ScrollRect>(false);
            Assert.IsTrue(scroll != null, "ScrollToTheEnd: " + context.State.Id + " has no scroll area");

            UnityEngine.Canvas.ForceUpdateCanvases();
            scroll.verticalNormalizedPosition = 0f;
            yield return null;
        }

        /// <summary>
        /// The check of LJ-4: with the list scrolled to its end, the last row's button lies wholly
        /// inside the scroll area, so every item of the list can be bought.
        /// </summary>
        private static void LastRowIsReachable(Measurement measurement, List<Violation> violations)
        {
            InteractiveControl last = null;
            InteractiveControl area = null;
            foreach (var control in measurement.Controls)
            {
                if (control.Kind == ControlKind.ScrollArea) area = control;
                else if (control.Path.Contains("/Content/")) last = control;
            }

            if (area == null || last == null)
            {
                violations.Add(new Violation(measurement, "LJ-4", "the list has " + (area == null ? "no scroll area" : "no row button"),
                    "a scroll area and a button in its last row", measurement.LayerPath));
                return;
            }

            var visible = area.ClipRect ?? area.OwnRect;
            var rect = last.OwnRect;
            var slack = 0.5f;
            if (rect.yMin < visible.yMin - slack || rect.yMax > visible.yMax + slack ||
                rect.xMin < visible.xMin - slack || rect.xMax > visible.xMax + slack)
            {
                violations.Add(new Violation(measurement, "LJ-4",
                    "the last row's button " + LayoutReport.Box(rect) + " is not inside the scroll area " + LayoutReport.Box(visible),
                    "the last button wholly inside the scroll area", last.Path));
            }
        }

        /// <summary>Presses the Shop tab labelled by <paramref name="key"/>, as a player would.</summary>
        private static IEnumerator PressTab(StateContext context, string key)
        {
            var label = Strings.Get(key);
            foreach (var button in context.Ui.Current.GetComponentsInChildren<UnityEngine.UI.Button>(false))
            {
                var text = button.GetComponentInChildren<UnityEngine.UI.Text>();
                if (text == null || !text.text.StartsWith(label, StringComparison.Ordinal)) continue;

                button.onClick.Invoke();
                yield break;
            }

            Assert.Fail("PressTab: the Shop has no tab \"" + label + "\"");
        }

        /// <summary>A booted game whose day closes with a crafted result, with the Balanço on the screen.</summary>
        private static IEnumerator BooksRoutine(BooksKind kind)
        {
            var builder = SessionFactory.NewProfile(NewGameSeed, 10).WithDay(1)
                .WithMoney(kind == BooksKind.Loss ? 40 : SessionFactory.LargeValues.Balance).EmptyRoom();
            builder.WithMaid("Hina", Rarity.Common, 1, 0);
            foreach (var n in CastNames) builder.WithMaid(n, Rarity.Common, 1, 0);
            var profile = builder.Build();

            yield return SessionFactory.Boot(profile);
            yield return SessionFactory.ToHub();

            var result = new Neko.Sim.Shift.ShiftResult { Day = 1, Satisfaction = 0.9f, ChemistryScore = 80 };
            if (kind == BooksKind.Profit)
            {
                result.DishRevenue = 4200;
                result.Tips = 600;
                result.Served = 30;
                result.DishesCooked = 32;
            }
            else if (kind == BooksKind.Loss)
            {
                result.DishRevenue = 20;
                result.Served = 1;
                result.DishesCooked = 5;
                result.Satisfaction = 0.2f;
                result.ChemistryScore = 10;
            }
            else
            {
                result.DishRevenue = SessionFactory.LargeValues.DayAmount;
                result.Tips = SessionFactory.LargeValues.DayAmount;
                result.Served = SessionFactory.LargeValues.Tally;
                result.DishesCooked = SessionFactory.LargeValues.Tally;
                result.WrongOrders = result.Burnt = result.LeftAngry = result.TurnedAway = SessionFactory.LargeValues.Tally;
                foreach (var maid in profile.Maids) result.XpByMaid[maid.Id] = 99999;
            }

            var root = GameRoot.Instance;
            root.Session.FinishDay(result);
            root.Ui.Show<BooksScreen>();
            yield return null;
        }


        // -------------------------------------------------------------------------------
        // UI-01B: Menu, Empresa, Escalação, Caixas, Salão, O dia, Falência, Dialogs
        // -------------------------------------------------------------------------------

        /// <summary>The Menu as boot leaves it: a fresh business, the largest records, or the "Nova empresa" Dialog open.</summary>
        private static IEnumerator MenuRoutine(MenuKind kind)
        {
            var builder = SessionFactory.NewProfile(NewGameSeed, kind == MenuKind.Fresh ? 1 : 10).WithDay(1);
            if (kind != MenuKind.Fresh)
            {
                builder.WithRecords(SessionFactory.LargeValues.Day, SessionFactory.LargeValues.DayAmount,
                    SessionFactory.LargeValues.CustomersServed, SessionFactory.LargeValues.Day,
                    SessionFactory.LargeValues.BusinessesLost, SessionFactory.LargeValues.LegendariesFound);
            }

            yield return SessionFactory.Boot(builder.Build());

            // The sign and the parade sway forever on tweens, which Settled cannot wait out: stop them and
            // level what they rotated, as SessionFactory.Settle documents for the Menu.
            Tween.KillAll();
            foreach (var rect in GameRoot.Instance.Ui.Current.GetComponentsInChildren<UnityEngine.RectTransform>(true))
            {
                rect.localRotation = UnityEngine.Quaternion.identity;
            }

            if (kind == MenuKind.Confirm) PressButtonText(Strings.Get("menu.newgame"));
            yield return null;
        }

        /// <summary>
        /// MN-2: the Menu of a business with records says "Continuar" and lists the records on one line each,
        /// as the game binds the session before the Menu is first shown (UI-01B D1).
        /// </summary>
        private static void MenuShowsTheSavedRecords(Measurement measurement, List<Violation> violations)
        {
            var texts = new List<string>();
            foreach (var text in GameRoot.Instance.Ui.Current.GetComponentsInChildren<UnityEngine.UI.Text>(false)) texts.Add(text.text);

            var continues = texts.Contains(Strings.Get("menu.continue"));
            var records = texts.Exists(t => t.StartsWith(Strings.Get("menu.records") + "\n", StringComparison.Ordinal) &&
                                            t.Contains(Strings.Get("menu.bestday") + ": " + SessionFactory.LargeValues.Day));
            if (continues && records) return;

            violations.Add(new Violation(measurement, "MN-2",
                (continues ? "" : "no \"Continuar\" button; ") + (records ? "" : "no records text with real line breaks"),
                "\"Continuar\" and the records lines", measurement.LayerPath));
        }

        /// <summary>Empresa at café level 10 with the largest money, fichas, day and an eight-maid roster.</summary>
        private static IEnumerator HubRoutine()
        {
            var builder = SessionFactory.NewProfile(NewGameSeed, 10).WithDay(SessionFactory.LargeValues.Day)
                .WithMoney(SessionFactory.LargeValues.Balance).WithFichas(SessionFactory.LargeValues.Fichas).EmptyRoom();
            builder.WithMaid(SessionFactory.Longest.MaidName().Item, Rarity.Legendary, 20, 5, TraitId.AngelHands);
            foreach (var n in CastNames) builder.WithMaid(n, Rarity.Common, 3, 1);
            builder.AutoFillLineup();

            yield return SessionFactory.Boot(builder.Build());
            yield return SessionFactory.ToHub();
        }

        /// <summary>Escalação with eight maids; <paramref name="full"/> fills the lineup, otherwise the lineup stays empty.</summary>
        private static IEnumerator LineupRoutine(bool full)
        {
            var builder = SessionFactory.NewProfile(NewGameSeed, 10).WithDay(1)
                .WithMoney(SessionFactory.LargeValues.Balance).EmptyRoom();
            builder.WithMaid(SessionFactory.Longest.MaidName().Item, Rarity.Legendary, 20, 5, TraitId.AngelHands, TraitId.QuickStep, TraitId.Charisma);
            foreach (var n in CastNames) builder.WithMaid(n, Rarity.Rare, 8, 2, TraitId.Hostess);
            if (full) builder.AutoFillLineup();

            yield return SessionFactory.Boot(builder.Build());
            yield return SessionFactory.ToHub();
            yield return SessionFactory.ToLineup();
        }

        /// <summary>Caixas with the largest fichas, a booted game walked to the shelf.</summary>
        private static IEnumerator BoxesRoutine()
        {
            var builder = SessionFactory.NewProfile(NewGameSeed, 10).WithDay(1)
                .WithMoney(SessionFactory.LargeValues.Balance).WithFichas(SessionFactory.LargeValues.Fichas).EmptyRoom();
            builder.WithMaid("Hina", Rarity.Rare, 12, 3, TraitId.Lucky);

            yield return SessionFactory.Boot(builder.Build());
            yield return SessionFactory.ToHub();
            yield return SessionFactory.ToBoxes();
        }

        /// <summary>Presses the first "Abrir" button of the shelf, as a player does; the Reveal then is the Active_Layer.</summary>
        private static IEnumerator OpenTheFirstBox(StateContext context)
        {
            PressButtonText(Strings.Get("box.open"));
            yield return null;
        }

        /// <summary>Salão at café level 10 with every placeable in stock and an empty room.</summary>
        private static IEnumerator HallRoutine()
        {
            var builder = SessionFactory.NewProfile(NewGameSeed, 10).WithDay(1)
                .WithMoney(SessionFactory.LargeValues.Balance).EmptyRoom();
            builder.WithMaid("Hina", Rarity.Rare, 5, 1, TraitId.Lucky);
            foreach (var item in Neko.Sim.Economy.Shop.StockFor(10))
            {
                if (item.Kind == ShopItemKind.Placeable) builder.WithStock(item.Id, 3);
            }

            yield return SessionFactory.Boot(builder.Build());
            yield return SessionFactory.ToHub();
            yield return SessionFactory.ToHall();
        }

        /// <summary>Picks up the first piece of the stock shelf, as a click on its card does.</summary>
        private static IEnumerator PickTheFirstPiece(StateContext context)
        {
            foreach (var button in context.Ui.Current.GetComponentsInChildren<UnityEngine.UI.Button>(false))
            {
                var text = button.GetComponentInChildren<UnityEngine.UI.Text>();
                if (text == null || !text.text.Contains("×")) continue;

                button.onClick.Invoke();
                yield break;
            }

            Assert.Fail("PickTheFirstPiece: the Salão has no stock card");
        }

        /// <summary>O dia: a running day under the DayHud with a full lineup, its clock paused.</summary>
        private static IEnumerator DayRoutine()
        {
            var builder = SessionFactory.NewProfile(NewGameSeed, 10).WithDay(1)
                .WithMoney(SessionFactory.LargeValues.Balance);
            builder.WithMaid("Hina", Rarity.Rare, 5, 1, TraitId.Lucky);
            foreach (var n in CastNames) builder.WithMaid(n, Rarity.Common, 3, 1);
            builder.AutoFillLineup();

            yield return SessionFactory.Boot(builder.Build());
            yield return SessionFactory.ToHub();
            yield return SessionFactory.ToDay();
        }

        /// <summary>Falência, shown as <c>GameRoot</c> shows it after a day that bankrupts the business.</summary>
        private static IEnumerator BustRoutine()
        {
            var builder = SessionFactory.NewProfile(NewGameSeed, 10).WithDay(1).WithMoney(40).EmptyRoom();
            builder.WithMaid("Hina", Rarity.Common, 1, 0);

            yield return SessionFactory.Boot(builder.Build());
            yield return SessionFactory.ToHub();

            GameRoot.Instance.Ui.Show<BustScreen>();
            yield return null;
        }

        /// <summary>A Dialog with a long title and body: both buttons over Empresa, or an error over the Salão.</summary>
        private static IEnumerator DialogRoutine(bool overHall)
        {
            var builder = SessionFactory.NewProfile(NewGameSeed, 10).WithDay(1)
                .WithMoney(SessionFactory.LargeValues.Balance).EmptyRoom();
            builder.WithMaid("Hina", Rarity.Common, 1, 0);

            yield return SessionFactory.Boot(builder.Build());
            yield return SessionFactory.ToHub();
            if (overHall) yield return SessionFactory.ToHall();

            var body = string.Join(" ", System.Linq.Enumerable.Repeat("Não foi possível guardar a empresa neste aparelho.", 4));
            var ui = GameRoot.Instance.Ui;
            if (overHall) ui.Dialog("Algo deu errado", body);
            else ui.Dialog("Uma pergunta bem comprida para a janela", body, "Sim, continuar", () => { }, "Não, voltar", () => { });
            yield return null;
        }

        /// <summary>Presses the first active button whose label is <paramref name="label"/>; fails when there is none.</summary>
        private static void PressButtonText(string label)
        {
            var layer = LayoutModel.ActiveLayer(GameRoot.Instance.Ui);
            foreach (var button in layer.GetComponentsInChildren<UnityEngine.UI.Button>(false))
            {
                var text = button.GetComponentInChildren<UnityEngine.UI.Text>();
                if (text == null || !text.text.StartsWith(label, StringComparison.Ordinal)) continue;

                button.onClick.Invoke();
                return;
            }

            Assert.Fail("PressButtonText: " + LayoutModel.PathOf(layer) + " has no button \"" + label + "\"");
        }

        /// <summary>
        /// A Dialog takes every click: a click at any point of the window lands on the Dialog's own
        /// graphics (its shade or its box), so nothing of the Screen below can be pressed through it.
        /// </summary>
        private static void DialogBlocksTheLayerBelow(Measurement measurement, List<Violation> violations)
        {
            var clicks = new Clicks(measurement);
            var viewport = measurement.Viewport;
            var leaks = 0;
            string first = null;
            for (var i = 1; i < 16; i++)
            {
                for (var j = 1; j < 10; j++)
                {
                    var point = new UnityEngine.Vector2(viewport.xMin + viewport.width * i / 16f, viewport.yMin + viewport.height * j / 10f);
                    var top = clicks.TopTarget(point);
                    if (top != null && top.transform.IsChildOf(measurement.Layer)) continue;

                    leaks++;
                    if (first == null) first = (top != null ? LayoutModel.PathOf(top.transform) : "nothing") + " at " + point;
                }
            }

            if (leaks > 0)
            {
                violations.Add(new Violation(measurement, "DG-1",
                    leaks + " of 135 points of the window do not land on the Dialog; first " + first,
                    "every click lands on the Dialog", measurement.LayerPath));
            }
        }

        /// <summary>
        /// Salão: no raycast target lies over a floor tile that the window shows, so a click on that
        /// tile reaches the room (the game ignores a click while the pointer is over any UI graphic).
        /// </summary>
        private static void TilesAreClickable(Measurement measurement, List<Violation> violations)
        {
            var root = GameRoot.Instance;
            var canvas = root.Ui.GetComponentInParent<UnityEngine.Canvas>();
            var camera = canvas.worldCamera;
            var clicks = new Clicks(measurement);
            var blocked = 0;
            var shown = 0;
            string first = null;
            foreach (var tile in root.Room.GetComponentsInChildren<UnityEngine.SpriteRenderer>())
            {
                if (!tile.enabled || tile.name != "Tile") continue;

                var pixel = (UnityEngine.Vector2)camera.WorldToScreenPoint(tile.bounds.center) - camera.pixelRect.min;
                var point = pixel / canvas.scaleFactor;
                if (!measurement.Viewport.Contains(point)) continue;

                shown++;
                var top = clicks.TopTarget(point);
                if (top == null) continue;

                blocked++;
                if (first == null) first = LayoutModel.PathOf(top.transform) + " over the tile at " + point;
            }

            if (blocked > 0)
            {
                violations.Add(new Violation(measurement, "SL-1",
                    blocked + " of " + shown + " visible floor tiles are covered by UI; first " + first,
                    "no UI over a floor tile", measurement.LayerPath));
            }
        }

        private static IReadOnlyList<AuditState> Register(AuditState[] states)
        {
            var previous = -1;
            foreach (var state in states)
            {
                var index = TableIndex(state.Id);
                if (index < 0) Assert.Fail("AuditStates: " + state.Id + " is not an id of the Audit States table");
                if (index == previous) Assert.Fail("AuditStates: " + state.Id + " is registered twice");
                if (index < previous)
                {
                    Assert.Fail("AuditStates: " + state.Id + " is registered after " + TableIds[previous] +
                                ", but the Audit States table lists it first");
                }

                previous = index;
            }

            return Array.AsReadOnly(states);
        }

        private static int TableIndex(string id)
        {
            for (var i = 0; i < TableIds.Count; i++)
            {
                if (string.Equals(TableIds[i], id, StringComparison.Ordinal)) return i;
            }

            return -1;
        }

        // -------------------------------------------------------------------------------
        // Empresa
        // -------------------------------------------------------------------------------

        /// <summary>
        /// EM-1, a new game on day 1: <c>Profile.NewGame(NewGameSeed)</c> as it comes, with its
        /// money, fichas, room and the three openers it rolls, booted on the Menu and walked to the
        /// Empresa through the play plank.
        /// </summary>
        /// <remarks>
        /// <see cref="SessionFactory.NewProfile"/> pins the café level, which is 1, and
        /// <see cref="ProfileBuilder.WithDay"/> the day; no <c>WithMaid</c> is called, so the openers
        /// stay as <c>NewGame</c> rolled them.
        /// </remarks>
        private static IEnumerator BuildNewGameDayOne(StateContext context)
        {
            var profile = SessionFactory.NewProfile(NewGameSeed, context.CafeLevel).WithDay(1).Build();
            yield return SessionFactory.Boot(profile);
            yield return SessionFactory.ToHub();
        }
    }
}
