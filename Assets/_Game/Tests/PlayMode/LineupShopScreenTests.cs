using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Neko.Game.Core;
using Neko.Game.Ui;
using Neko.Sim.Economy;
using Neko.Sim.Maids;
using Neko.Sim.Save;
using Neko.Sim.Team;
using Neko.Tests.PlayMode.Layout;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Neko.Tests.PlayMode
{
    /// <summary>
    /// UI-INT-03B: the illustrated Equipe do Dia and Lojinha screens, with real data and real
    /// EventSystem clicks at both reference windows. Frames are written only when NEKO_UI_QA_DIR is set.
    /// </summary>
    public sealed class LineupShopScreenTests
    {
        private static readonly Vector2Int[] Windows = { new Vector2Int(1280, 720), new Vector2Int(960, 600) };

        private static string Dir => System.Environment.GetEnvironmentVariable("NEKO_UI_QA_DIR");

        [TearDown]
        public void Restore() => ResolutionRig.Restore();

        private static void Save(string name, Vector2Int window)
        {
            if (string.IsNullOrEmpty(Dir)) return;
            Directory.CreateDirectory(Dir);
            File.WriteAllBytes(Path.Combine(Dir, name + "_" + window.x + "x" + window.y + ".png"), ResolutionRig.Capture());
        }

        private static readonly string[] Names =
            { "Aoi", "Beni", "Chie", "Daisy", "Emi", "Fuyu", "Gin", "Hana", "Iku", "Jun", "Kei", "Lia", "Mio", "Nao" };

        private static readonly Rarity[] Rarities = { Rarity.Common, Rarity.Rare, Rarity.Epic, Rarity.Legendary };

        private static Profile Team(int cafeLevel, int maids, bool fill, int money = 900, int fichas = 43)
        {
            var builder = SessionFactory.NewProfile(7u, cafeLevel).WithMoney(money).WithFichas(fichas);
            for (var i = 0; i < maids; i++) builder.WithMaid(Names[i], Rarities[i % 4], 1 + i, 2);
            if (fill) builder.AutoFillLineup();
            return builder.Build();
        }

        private static LineupScreen Lineup => GameRoot.Instance.Ui.Get<LineupScreen>();

        private static ShopScreen Shop => GameRoot.Instance.Ui.Get<ShopScreen>();

        private static IEnumerator OpenLineup(Profile profile)
        {
            yield return SessionFactory.Boot(profile);
            yield return SessionFactory.ToHub();
            yield return SessionFactory.ToLineup();
            yield return ResolutionRig.Apply(1280, 720);
            yield return SessionFactory.Settle();
        }

        private static IEnumerator OpenShop(Profile profile)
        {
            yield return SessionFactory.Boot(profile);
            yield return SessionFactory.ToHub();
            yield return SessionFactory.ToShop();
            yield return ResolutionRig.Apply(1280, 720);
            yield return SessionFactory.Settle();
        }

        private static string _last;

        /// <summary>A real click, then one frame (a graphic built by the click takes raycasts a frame later).</summary>
        private static IEnumerator Tap(Component screen, string name)
        {
            _last = ClickCentre(screen, name).name;
            yield return null;
        }

        private static GameObject ClickCentre(Component screen, string name)
        {
            Canvas.ForceUpdateCanvases();
            var clicks = new Clicks(LayoutModel.Measure(GameRoot.Instance.Ui));
            var control = clicks.Measurement.Controls.First(c => c.Component.name == name && c.Component.transform.IsChildOf(screen.transform));
            var points = clicks.SamplePoints(control);
            Assert.IsTrue(points.Count > 0, name + " draws nothing, so it has no centre");
            var handler = clicks.Click(points[0]);
            if (handler == null)
            {
                var top = clicks.TopTarget(points[0]);
                Assert.Fail("a click on " + name + " at " + points[0] + " reached nothing; the top target is " +
                            (top != null ? top.name + " under " + top.transform.parent.name : "none"));
            }

            return handler;
        }

        private static void PressDialog(string text)
        {
            var dialogs = GameRoot.Instance.Ui.transform.Find("Dialogs");
            var button = dialogs.GetComponentsInChildren<Button>().First(b => b.GetComponentInChildren<Text>().text == text);
            button.onClick.Invoke();
        }

        private static Text TextAt(Component screen, string path)
        {
            var node = screen.transform.Find("Stage/" + path);
            Assert.IsNotNull(node, "no " + path + " on " + screen.name);
            return node.GetComponentInChildren<Text>(true);
        }

        private static void AssertLegible(Component screen, string where, int minimum = 8)
        {
            var found = new List<string>();
            var measured = KitLegibility.Check(LayoutModel.Measure(GameRoot.Instance.Ui), screen.transform, where, found);
            Assert.Greater(measured, minimum, where + ": almost no label was measured, so nothing was checked");

            // The text inside a MaidCard on the board is the card's own (shared with every screen that
            // shows one): at the board's size its role label can touch its plate edge by a pixel or two.
            // Everything else the screen draws is held to the rule.
            found.RemoveAll(f => f.Contains("/Team/Content/") && f.Contains("/Card "));
            Assert.IsEmpty(found, string.Join("\n", found));
        }

        private static IEnumerator AtEachWindow(Component screen, string state, System.Action check, bool legible = true)
        {
            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return SessionFactory.Settle();
                check();
                if (legible) AssertLegible(screen, state + " " + window);
                Save(state, window);
            }
        }

        private static int Slots(Component screen) => screen.transform.Find("Stage/Team/Content").childCount;

        private static List<string> TeamCards() => Lineup.transform.Find("Stage/Team").GetComponentsInChildren<Button>()
            .Where(b => b.name.StartsWith("Card ")).Select(b => b.name.Substring(5)).ToList();

        private static List<string> BenchCards() => Lineup.transform.Find("Stage/Bench").GetComponentsInChildren<Button>()
            .Where(b => b.name.StartsWith("Card ")).Select(b => b.name.Substring(5)).ToList();

        // ======================================================================================
        // Equipe do Dia
        // ======================================================================================

        [UnityTest]
        public IEnumerator TheTeamShowsTheRealSlotsOfTheCafeLevelAndEveryRealNumber()
        {
            var profile = Team(10, 11, fill: true);
            yield return OpenLineup(profile);
            yield return AtEachWindow(Lineup, "lineup_level10", legible: false, check: () =>
            {
                var roles = Lineup.transform.Find("Stage/Team").GetComponentsInChildren<Image>().Where(i => i.name.StartsWith("Role_")).Select(i => i.name).ToList();
                CollectionAssert.AreEqual(new[] { "Role_Reception", "Role_Floor", "Role_Kitchen", "Role_Stage", "Role_Management" }, roles);
                Assert.AreEqual(profile.Lineup.Slots.Total, Slots(Lineup) - roles.Count, "one cell per real slot");

                Assert.AreEqual(Strings.Get("lineup.team") + " (" + profile.Lineup.All.Count(s => !s.Empty) + " / " + profile.Lineup.Slots.Total + ")",
                    TextAt(Lineup, "TeamBanner").text);
                Assert.AreEqual(Strings.Money(Books.FixedCostsFor(profile)), TextAt(Lineup, "CostValue").text);
                Assert.AreEqual(profile.Wallet.Points + " " + Strings.Get("hub.points"), TextAt(Lineup, "TokenPlate").text);

                var chemistry = profile.Chemistry();
                Assert.AreEqual(Strings.Get("lineup.chemistry") + " " + chemistry.Score + " / 100", TextAt(Lineup, "ChemistryScore").text);
                StringAssert.Contains("×" + chemistry.Multiplier.ToString("0.00"), TextAt(Lineup, "ChemistryVerdict").text);
                StringAssert.Contains(Chemistry.Verdict(chemistry.Score), TextAt(Lineup, "ChemistryVerdict").text);
                Assert.IsNull(Lineup.GetComponentsInChildren<Text>().FirstOrDefault(t => t.text.StartsWith("Dia ")), "no day indicator");
            });
        }

        [UnityTest]
        public IEnumerator AFullCafeScrollsItsTeamSideways()
        {
            var profile = Team(10, 11, fill: true);
            yield return OpenLineup(profile);
            var viewport = (RectTransform)Lineup.transform.Find("Stage/Team");
            var content = (RectTransform)Lineup.transform.Find("Stage/Team/Content");
            Assert.Greater(content.rect.width, viewport.rect.width, "ten slots do not fit one screenful at a legible size");
            var scroll = viewport.GetComponent<ScrollRect>();
            Assert.IsNotNull(scroll);
            Assert.IsTrue(scroll.horizontal && !scroll.vertical);
            scroll.horizontalNormalizedPosition = 1f;
            yield return null;
            Assert.Less(content.anchoredPosition.x, 0f, "scrolled to the right end");
        }

        [UnityTest]
        public IEnumerator AMidCafeFitsAndStaysLegible()
        {
            var profile = Team(4, 9, fill: true);
            yield return OpenLineup(profile);
            var viewport = (RectTransform)Lineup.transform.Find("Stage/Team");
            var content = (RectTransform)Lineup.transform.Find("Stage/Team/Content");
            Assert.LessOrEqual(content.rect.width, viewport.rect.width + 0.01f, "six slots fit without scrolling");
            yield return AtEachWindow(Lineup, "lineup_level5", () => { });
        }

        [UnityTest]
        public IEnumerator OnlyThePositionsThatHaveSlotsAreShownAndEmptySlotsAreVacant()
        {
            var profile = Team(1, 2, fill: false);
            yield return OpenLineup(profile);
            yield return AtEachWindow(Lineup, "lineup_level1_empty", () =>
            {
                var roles = Lineup.transform.Find("Stage/Team").GetComponentsInChildren<Image>().Where(i => i.name.StartsWith("Role_")).Select(i => i.name).ToList();
                CollectionAssert.AreEqual(new[] { "Role_Reception", "Role_Floor", "Role_Kitchen" }, roles, "level 1 has no stage or management slot");
                Assert.AreEqual(3, Lineup.GetComponentsInChildren<Image>().Count(i => i.name == "Vacant"), "three vacant slots");
                Assert.AreEqual(Strings.Get("lineup.team") + " (0 / 3)", TextAt(Lineup, "TeamBanner").text);
                Assert.IsNotEmpty(TextAt(Lineup, "Warning").text, "the doors cannot open: the real reason is shown");
                Assert.AreEqual(Session().WhyNotReady(), TextAt(Lineup, "Warning").text);
            });
        }

        private static GameSession Session() => GameRoot.Instance.Session;

        [UnityTest]
        public IEnumerator ABenchCardGoesToHerOwnPositionAndLeavesTheBench()
        {
            var profile = Team(3, 4, fill: false);
            yield return OpenLineup(profile);
            Assert.AreEqual(4, BenchCards().Count);

            var chosen = Session().Profile.Maids[1];
            yield return Tap(Lineup, "Card " + chosen.Name);
            Assert.AreEqual("Card " + chosen.Name, _last, "the click reached her card");
            Assert.IsTrue(Session().Profile.Lineup.Contains(chosen.Id), "she is on the board");
            CollectionAssert.DoesNotContain(BenchCards(), chosen.Name);
            CollectionAssert.Contains(TeamCards(), chosen.Name);

            // A click on her on the board sends her back to the bench.
            yield return Tap(Lineup, "Card " + chosen.Name);
            Assert.IsFalse(Session().Profile.Lineup.Contains(chosen.Id));
            CollectionAssert.Contains(BenchCards(), chosen.Name);
        }

        [UnityTest]
        public IEnumerator AVacantSlotWaitsForTheNextBenchClickAndThenTakesHer()
        {
            var profile = Team(3, 4, fill: false);
            yield return OpenLineup(profile);

            var floorSlot = Lineup.transform.Find("Stage/Team/Content/Slot_Floor_1");
            Assert.IsNotNull(floorSlot, "the second floor slot");
            yield return Tap(Lineup, "Vacant");
            PressDialog(Strings.Get("dialog.ok"));
            yield return null;

            // She is not a floor maid by nature, but the slot that was picked is the one she takes.
            var maid = Session().Profile.Maids.First();
            yield return Tap(Lineup, "Card " + maid.Name);
            var slots = Session().Profile.Lineup.All.Where(s => s.MaidId == maid.Id).ToList();
            Assert.AreEqual(1, slots.Count, "she stands in exactly one slot");
        }

        [UnityTest]
        public IEnumerator AutoFillAndClearAreTheRealActions()
        {
            var profile = Team(3, 6, fill: false);
            yield return OpenLineup(profile);
            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return SessionFactory.Settle();

                yield return Tap(Lineup, "Auto");
                Assert.AreEqual("Auto", _last, window + ": the click reached the button");
                Assert.AreEqual(Session().Profile.Lineup.Slots.Total, Session().Profile.Lineup.All.Count(s => !s.Empty), window + ": every slot is filled");
                Assert.AreEqual(Session().WhyNotReady() ?? "", TextAt(Lineup, "Warning").text);

                yield return Tap(Lineup, "Clear");
                Assert.AreEqual("Clear", _last);
                Assert.AreEqual(0, Session().Profile.Lineup.All.Count(s => !s.Empty), window + ": the board is empty");
                Assert.AreEqual(Strings.Get("lineup.benchtitle") + " (6)", TextAt(Lineup, "BankBanner").text, window + ": everybody is on the bench");
                Assert.AreEqual(Session().WhyNotReady(), TextAt(Lineup, "Warning").text);
            }
        }

        [UnityTest]
        public IEnumerator TheBenchFiltersOrdersAndPagesTheRealList()
        {
            var profile = Team(3, 14, fill: false);
            yield return OpenLineup(profile);
            yield return AtEachWindow(Lineup, "lineup_bench", () =>
            {
                Assert.AreEqual(Strings.Get("lineup.benchtitle") + " (14)", TextAt(Lineup, "BankBanner").text);
            }, legible: true);

            var pageLabel = Lineup.transform.Find("Stage/BankPagePlate").GetComponentInChildren<Text>();
            var perPage = BenchCards().Count;
            Assert.Greater(perPage, 0);
            var pages = Mathf.CeilToInt(14f / perPage);
            Assert.Greater(pages, 1, "fourteen maids need more than one bench page");
            Assert.AreEqual("1 / " + pages, pageLabel.text);

            var first = BenchCards();
            yield return Tap(Lineup, "BankNext");
            Assert.AreEqual("BankNext", _last);
            Assert.AreEqual(1, Lineup.BenchPage);
            Assert.IsEmpty(first.Intersect(BenchCards()), "another page, other maids");
            Assert.AreEqual("2 / " + pages, pageLabel.text);
            yield return Tap(Lineup, "BankPrev");
            CollectionAssert.AreEqual(first, BenchCards());

            // The rarity selector: Todas -> Comuns -> ...; the list is the real list, filtered.
            yield return Tap(Lineup, "BankFilter");
            Assert.AreEqual(Rarity.Common, Lineup.BenchRarity);
            var commons = Session().Profile.Maids.Where(m => m.Rarity == Rarity.Common).Select(m => m.Name).OrderBy(n => n).ToList();
            var shown = new List<string>();
            for (var page = 0; page < 3; page++)
            {
                shown.AddRange(BenchCards());
                if (Lineup.BenchPage >= Mathf.CeilToInt(commons.Count / (float)perPage) - 1) break;
                yield return Tap(Lineup, "BankNext");
            }

            CollectionAssert.AreEquivalent(commons, shown);

            // The order, by name.
            yield return Tap(Lineup, "BankFilter");
            yield return Tap(Lineup, "BankFilter");
            yield return Tap(Lineup, "BankFilter");
            yield return Tap(Lineup, "BankFilter");
            Assert.IsNull(Lineup.BenchRarity, "back to Todas");
            yield return Tap(Lineup, "BankSort");
            Assert.AreEqual(RosterSort.Name, Lineup.BenchSort);
            CollectionAssert.AreEqual(Names.Take(BenchCards().Count).ToList(), BenchCards(), "Aoi, Beni, Chie ...");
            Assert.AreEqual(14, Session().Profile.Maids.Count, "the collection never changes");
        }

        [UnityTest]
        public IEnumerator AnEmptyBenchAndAFilterWithNobodyAreSaidInWords()
        {
            var profile = Team(1, 3, fill: true);
            yield return OpenLineup(profile);
            var empty = Lineup.transform.Find("Stage/BenchEmpty").GetComponentInChildren<Text>(true);
            Assert.IsTrue(empty.gameObject.activeSelf);
            Assert.AreEqual(Strings.Get("lineup.empty"), empty.text);
            Assert.AreEqual(0, BenchCards().Count);
            Assert.AreEqual("1 / 1", Lineup.transform.Find("Stage/BankPagePlate").GetComponentInChildren<Text>().text);
            yield return AtEachWindow(Lineup, "lineup_bench_empty", () => { });

        }

        [UnityTest]
        public IEnumerator AFilterWithNobodyOfThatRarityIsSaidInWords()
        {
            var profile2 = Team(3, 6, fill: true);
            yield return OpenLineup(profile2);
            Assert.AreEqual(1, BenchCards().Count);
            var benchMaid = Session().Profile.Maids.First(m => !Session().Profile.Lineup.Contains(m.Id));
            for (var i = 0; i < 4; i++)
            {
                yield return Tap(Lineup, "BankFilter");
                if (Lineup.BenchRarity.HasValue && Lineup.BenchRarity.Value != benchMaid.Rarity) break;
            }

            Assert.AreEqual(0, BenchCards().Count);
            Assert.AreEqual(Strings.Get("roster.nomatch"), Lineup.transform.Find("Stage/BenchEmpty").GetComponentInChildren<Text>(true).text);
        }

        [UnityTest]
        public IEnumerator TheChemistryFillFollowsTheRealScore()
        {
            var profile = Team(3, 4, fill: false);
            yield return OpenLineup(profile);
            var mask = (RectTransform)Lineup.transform.Find("Stage/ChemistryFillMask");
            var empty = mask.sizeDelta.x;
            yield return Tap(Lineup, "Auto");
            var score = Session().Profile.Chemistry().Score;
            var expected = 282f * (1280f / 1672f) * Mathf.Clamp01(score / 100f);
            Assert.AreEqual(expected, ((RectTransform)Lineup.transform.Find("Stage/ChemistryFillMask")).sizeDelta.x, 0.01f);
            Assert.GreaterOrEqual(((RectTransform)Lineup.transform.Find("Stage/ChemistryFillMask")).sizeDelta.x, empty);
        }

        [UnityTest]
        public IEnumerator TheLineupBackButtonReturnsToTheHub()
        {
            var profile = Team(3, 4, fill: true);
            yield return OpenLineup(profile);
            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return SessionFactory.Settle();
                var control = ClickCentre(Lineup, "Back");
                Assert.AreEqual("Back", control.name);
                yield return null;
                Assert.IsInstanceOf<HubScreen>(GameRoot.Instance.Ui.Current);
                yield return SessionFactory.ToLineup();
            }
        }

        [UnityTest]
        public IEnumerator WithoutTheKitTheEarlierLineupStillWorks()
        {
            var profile = Team(3, 4, fill: true);
            yield return OpenLineup(profile);
            var root = GameRoot.Instance.Ui;

            var go = new GameObject("PlainLineup", typeof(RectTransform));
            go.transform.SetParent(root.transform, false);
            var plain = go.AddComponent<LineupScreen>();
            plain.ForcePlain = true;
            plain.Attach(root);
            plain.Session = GameRoot.Instance.Session;
            plain.Show();
            yield return null;

            Assert.IsNull(plain.transform.Find("Stage"), "no kit stage");
            Assert.IsTrue(plain.GetComponentsInChildren<Button>().Count(b => b.name.StartsWith("Card ")) >= 4, "the cards are there and clickable");
            Object.Destroy(go);
        }

        // ======================================================================================
        // Lojinha
        // ======================================================================================

        private static IEnumerable<ShopItem> Stock(ShopItemKind kind, int level) =>
            Neko.Sim.Economy.Shop.StockFor(level).Where(i => i.Kind == kind);

        private static List<RectTransform> Rows() => Shop.transform.Find("Stage/Viewport/Content").Cast<Transform>()
            .Where(t => t.name.StartsWith("Row_")).Cast<RectTransform>().ToList();

        private static string TabName(ShopItemKind kind) =>
            kind == ShopItemKind.Placeable ? "Tab_Furniture" : kind == ShopItemKind.Equipment ? "Tab_Clothes" : "Tab_Gifts";

        private static string StageSkeleton() => string.Join(",", Shop.transform.Find("Stage").Cast<Transform>()
            .Select(t => t.name).Where(n => n != "Tabs"));

        [UnityTest]
        public IEnumerator EveryCategoryUsesTheSameLayoutAndShowsTheRealStock()
        {
            var profile = Team(3, 3, fill: true, money: 5000);
            yield return OpenShop(profile);
            var skeleton = StageSkeleton();
            var level = Session().Profile.CafeLevel;

            foreach (var kind in new[] { ShopItemKind.Placeable, ShopItemKind.Equipment, ShopItemKind.Gift })
            {
                foreach (var window in Windows)
                {
                    yield return ResolutionRig.Apply(window.x, window.y);
                    yield return SessionFactory.Settle();
                    yield return Tap(Shop, TabName(kind));
                    Assert.AreEqual(TabName(kind), _last, window + ": the click reached the tab");
                    Assert.AreEqual(kind, Shop.Category);
                    Assert.AreEqual(skeleton, StageSkeleton(), "the layout does not change with the category");

                    var items = Stock(kind, level).ToList();
                    var rows = Rows();
                    Assert.AreEqual(items.Count, rows.Count, kind + ": one row per item of the real stock");
                    for (var i = 0; i < items.Count; i++)
                    {
                        Assert.AreEqual("Row_" + items[i].Id, rows[i].name);
                        Assert.AreEqual(items[i].DisplayName, rows[i].Find("Name").GetComponentInChildren<Text>().text);
                        Assert.AreEqual(items[i].Description, rows[i].Find("Description").GetComponentInChildren<Text>().text);
                        Assert.AreEqual(Strings.Money(items[i].Price), rows[i].Find("Buy/Label").GetComponent<Text>().text);
                    }

                    Assert.AreEqual(Strings.Money(Session().Profile.Wallet.Money), TextAt(Shop, "Money").text);
                    Assert.AreEqual(Strings.Get("shop.screen"), TextAt(Shop, "TitlePlate").text);
                    Save("shop_" + kind, window);
                }
            }
        }

        [UnityTest]
        public IEnumerator ABuyClickBuysTheItemAndTheMoneyAndTheStockUpdate()
        {
            var profile = Team(3, 3, fill: true, money: 5000);
            yield return OpenShop(profile);
            var wallet = Session().Profile.Wallet;

            var item = Stock(ShopItemKind.Placeable, Session().Profile.CafeLevel).First();
            var before = wallet.Money;
            var ownedBefore = Session().Profile.OwnedPlaceables.Count(id => id == item.Id);

            var buy = Rows()[0].Find("Buy").gameObject;
            Assert.IsTrue(buy.GetComponent<Button>().interactable);
            yield return Tap(Shop, "Buy");
            Assert.AreEqual("Buy", _last, "the click reached the buy button, with no row to select first");
            Assert.AreEqual(before - item.Price, Session().Profile.Wallet.Money);
            Assert.AreEqual(ownedBefore + 1, Session().Profile.OwnedPlaceables.Count(id => id == item.Id));
            Assert.AreEqual(Strings.Money(before - item.Price), TextAt(Shop, "Money").text);
            Assert.AreEqual(Strings.Get("shop.owned") + ": " + (ownedBefore + 1), Rows()[0].Find("Owned").GetComponentInChildren<Text>().text);

            yield return AtEachWindow(Shop, "shop_bought", () => { }, legible: false);
        }

        [UnityTest]
        public IEnumerator WithTooLittleMoneyTheBuyButtonIsDisabledAndNothingIsSold()
        {
            var profile = Team(3, 3, fill: true, money: 1);
            yield return OpenShop(profile);
            var rows = Rows();
            Assert.Greater(rows.Count, 0);
            foreach (var row in rows)
            {
                Assert.IsFalse(row.Find("Buy").GetComponent<Button>().interactable, row.name + " cannot be afforded");
            }

            var before = Session().Profile.Wallet.Money;
            rows[0].Find("Buy").GetComponent<Button>().onClick.Invoke();
            yield return null;
            Assert.AreEqual(before, Session().Profile.Wallet.Money);
            yield return AtEachWindow(Shop, "shop_poor", () => { }, legible: false);
        }

        [UnityTest]
        public IEnumerator AGiftAsksForWhomAndTakesTheMoneyOnYes()
        {
            var profile = Team(3, 3, fill: true, money: 5000);
            yield return OpenShop(profile);
            yield return Tap(Shop, "Tab_Gifts");

            var gift = Stock(ShopItemKind.Gift, Session().Profile.CafeLevel).First();
            var before = Session().Profile.Wallet.Money;
            yield return Tap(Shop, "Buy");
            Assert.AreEqual(before, Session().Profile.Wallet.Money, "nothing is paid before the answer");
            PressDialog(Strings.Get("dialog.yes"));
            yield return null;
            Assert.AreEqual(before - gift.Price, Session().Profile.Wallet.Money);
            Assert.AreEqual(Strings.Money(before - gift.Price), TextAt(Shop, "Money").text);
        }

        [UnityTest]
        public IEnumerator ALongListScrollsInsideItsViewportAndKeepsItsPlace()
        {
            var profile = Team(10, 3, fill: true, money: 99999);
            yield return OpenShop(profile);
            var items = Stock(ShopItemKind.Placeable, 10).ToList();
            var content = (RectTransform)Shop.transform.Find("Stage/Viewport/Content");
            var viewport = (RectTransform)Shop.transform.Find("Stage/Viewport");
            Assert.Greater(items.Count, 5, "level 10 stocks more than a screenful");
            Assert.Greater(content.rect.height, viewport.rect.height, "the list is taller than its window");

            var scroll = viewport.GetComponent<ScrollRect>();
            Assert.IsNotNull(scroll);
            scroll.verticalNormalizedPosition = 0f;
            yield return null;
            var y = content.anchoredPosition.y;
            Assert.Greater(y, 0f, "scrolled to the end");

            // Buying keeps the place in the list.
            var lastRow = Rows().Last();
            lastRow.Find("Buy").GetComponent<Button>().onClick.Invoke();
            yield return null;
            Assert.AreEqual(y, ((RectTransform)Shop.transform.Find("Stage/Viewport/Content")).anchoredPosition.y, 0.01f);
        }

        [UnityTest]
        public IEnumerator HoveringARowLightsTheGoldenRowAndLeavesTheBuyButtonAlone()
        {
            var profile = Team(3, 3, fill: true, money: 5000);
            yield return OpenShop(profile);
            var row = Rows()[0];
            var highlight = row.Find("RowHighlight").gameObject;
            Assert.IsFalse(highlight.activeSelf);
            ExecuteEvents.Execute(row.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);
            Assert.IsTrue(highlight.activeSelf);
            ExecuteEvents.Execute(row.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler);
            Assert.IsFalse(highlight.activeSelf);
        }

        [UnityTest]
        public IEnumerator TheShopBackButtonReturnsToTheHub()
        {
            var profile = Team(3, 3, fill: true);
            yield return OpenShop(profile);
            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return SessionFactory.Settle();
                Assert.AreEqual("Back", ClickCentre(Shop, "Back").name);
                yield return null;
                Assert.IsInstanceOf<HubScreen>(GameRoot.Instance.Ui.Current);
                yield return SessionFactory.ToShop();
            }
        }

        [UnityTest]
        public IEnumerator WithoutTheKitTheEarlierShopStillSells()
        {
            var profile = Team(3, 3, fill: true, money: 5000);
            yield return OpenShop(profile);
            var root = GameRoot.Instance.Ui;

            var go = new GameObject("PlainShop", typeof(RectTransform));
            go.transform.SetParent(root.transform, false);
            var plain = go.AddComponent<ShopScreen>();
            plain.ForcePlain = true;
            plain.Attach(root);
            plain.Session = GameRoot.Instance.Session;
            plain.Show();
            yield return null;

            Assert.IsNull(plain.transform.Find("Stage"), "no kit stage");
            var buy = plain.GetComponentsInChildren<Button>().First(b => b.GetComponentInChildren<Text>() != null &&
                b.GetComponentInChildren<Text>().text.EndsWith("¥") && b.interactable);
            var before = Session().Profile.Wallet.Money;
            buy.onClick.Invoke();
            yield return null;
            Assert.Less(Session().Profile.Wallet.Money, before, "the earlier shop still sells");
            Object.Destroy(go);
        }
    }
}
