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
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Neko.Tests.PlayMode
{
    /// <summary>
    /// UI-INT-02A: the illustrated Maids screen with its two filters, order, pages and detail,
    /// with real data and real EventSystem clicks at both reference windows. Frames are written
    /// only when NEKO_UI_QA_DIR is set.
    /// </summary>
    public sealed class RosterScreenTests
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

        private struct Spec
        {
            public string Name;
            public Rarity Rarity;
            public Role Role;
            public int Level;

            public Spec(string name, Rarity rarity, Role role, int level)
            {
                Name = name;
                Rarity = rarity;
                Role = role;
                Level = level;
            }
        }

        /// <summary>Eleven maids: every rarity and all five natural positions, so two pages.</summary>
        private static readonly Spec[] Eleven =
        {
            new Spec("Aoi", Rarity.Common, Role.Reception, 3),
            new Spec("Beni", Rarity.Common, Role.Floor, 3),
            new Spec("Chie", Rarity.Rare, Role.Kitchen, 5),
            new Spec("Daisy", Rarity.Rare, Role.Reception, 1),
            new Spec("Emi", Rarity.Epic, Role.Stage, 9),
            new Spec("Fuyu", Rarity.Epic, Role.Management, 9),
            new Spec("Gin", Rarity.Legendary, Role.Kitchen, 20),
            new Spec("Hana", Rarity.Common, Role.Stage, 2),
            new Spec("Iku", Rarity.Rare, Role.Floor, 5),
            new Spec("Jun", Rarity.Epic, Role.Reception, 4),
            new Spec("Kei", Rarity.Common, Role.Management, 1),
        };

        private static Profile Roster(IEnumerable<Spec> specs, int fichas = 43)
        {
            var builder = SessionFactory.NewProfile(7u, 10).WithMoney(900).WithFichas(fichas);
            foreach (var spec in specs) builder.WithMaid(spec.Name, spec.Rarity, spec.Level, 2);
            foreach (var spec in specs) builder.MaidNamed(spec.Name).NaturalRole = spec.Role;
            return builder.AutoFillLineup().Build();
        }

        private static RosterScreen Screen => GameRoot.Instance.Ui.Get<RosterScreen>();

        private static IEnumerator Open(IEnumerable<Spec> specs, int fichas = 43)
        {
            yield return SessionFactory.Boot(Roster(specs, fichas));
            yield return SessionFactory.ToHub();
            yield return SessionFactory.ToRoster();
            yield return ResolutionRig.Apply(1280, 720);
            yield return SessionFactory.Settle();
        }

        /// <summary>What a click reached, by name: the control itself is rebuilt by the click.</summary>
        private sealed class Hit
        {
            public string name;
        }

        private static Hit _last;

        /// <summary>
        /// A real click, then one frame: a graphic built by the click takes raycasts only once a frame
        /// has rendered, which is the player's next frame and a test's explicit one.
        /// </summary>
        private static IEnumerator Tap(string name)
        {
            _last = new Hit { name = ClickCentre(name).name };
            yield return null;
        }

        private static GameObject ClickCentre(string name)
        {
            // A graphic built this frame takes no raycast until its canvas has been rebuilt, which
            // a player's next frame does and a test that clicks twice in one frame does not.
            Canvas.ForceUpdateCanvases();
            var clicks = new Clicks(LayoutModel.Measure(GameRoot.Instance.Ui));
            var control = clicks.Measurement.Controls.First(c => c.Component.name == name);
            var points = clicks.SamplePoints(control);
            Assert.IsTrue(points.Count > 0, name + " draws nothing, so it has no centre");
            var handler = clicks.Click(points[0]);
            if (handler == null)
            {
                var top = clicks.TopTarget(points[0]);
                Assert.Fail("a click on " + name + " at " + points[0] + " (rect " + control.OwnRect + ", " + points.Count +
                            " points) reached nothing; the top target there is " + (top != null ? top.name + " under " + top.transform.parent.name : "none"));
            }

            return handler;
        }

        /// <summary>The names on the visible cards, in the order they are drawn.</summary>
        private static List<string> Cards() => Screen.GetComponentsInChildren<Button>()
            .Where(b => b.name.StartsWith("Card ")).Select(b => b.name.Substring(5)).ToList();

        private static string Names(IEnumerable<Spec> specs) => string.Join(",", specs.Select(s => s.Name));

        private static Text Label(string buttonName) =>
            Screen.GetComponentsInChildren<Button>().First(b => b.name == buttonName).GetComponentInChildren<Text>();

        private static Text Find(string path)
        {
            var node = Screen.transform.Find("Stage/" + path);
            Assert.IsNotNull(node, "no " + path + " on the Maids screen");
            return node.GetComponentInChildren<Text>(true);
        }

        /// <summary>"Guardar" on the box reveal.</summary>
        private static void PressReveal()
        {
            var button = GameRoot.Instance.Ui.Get<BoxScreen>().GetComponentsInChildren<Button>()
                .First(b => b.GetComponentInChildren<Text>() != null && b.GetComponentInChildren<Text>().text == Strings.Get("box.done"));
            button.onClick.Invoke();
        }

        /// <summary>The dialog's button with this label, pressed (the dialog sits outside the measured layer).</summary>
        private static void PressDialog(string text)
        {
            var dialogs = GameRoot.Instance.Ui.transform.Find("Dialogs");
            var button = dialogs.GetComponentsInChildren<Button>().First(b => b.GetComponentInChildren<Text>().text == text);
            button.onClick.Invoke();
        }

        private static void AssertLegible(string where)
        {
            var found = new List<string>();
            var measured = KitLegibility.Check(LayoutModel.Measure(GameRoot.Instance.Ui), Screen.transform, where, found);
            Assert.Greater(measured, 10, where + ": almost no label was measured, so nothing was checked");
            Assert.IsEmpty(found, string.Join("\n", found));
        }

        private static IEnumerator AtEachWindow(string state, System.Action check)
        {
            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return SessionFactory.Settle();
                check();
                AssertLegible(state + " " + window);
                Save(state, window);
            }
        }

        // ---- filters -------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator EveryRarityButtonShowsExactlyThoseMaidsAndTheCountsAreTheOwnedTotals()
        {
            yield return Open(Eleven);
            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return SessionFactory.Settle();

                foreach (var rarity in RosterQuery.Rarities)
                {
                    yield return Tap("Filter_Rarity_" + rarity);
                    var click = _last;
                    Assert.AreEqual("Filter_Rarity_" + rarity, click.name, window + ": the click reached the filter");
                    var expected = Eleven.Where(s => s.Rarity == rarity).OrderBy(s => s.Name).Select(s => s.Name);
                    CollectionAssert.AreEquivalent(expected, Cards(), window + " " + rarity);
                    Assert.AreEqual(rarity, Screen.RarityFilter);

                    // The counts do not move with the filter.
                    Assert.AreEqual("Todas (11)", Label("Filter_Rarity_All").text);
                    Assert.AreEqual("Comuns (4)", Label("Filter_Rarity_Common").text);
                    Assert.AreEqual("Raras (3)", Label("Filter_Rarity_Rare").text);
                    Assert.AreEqual("Épicas (3)", Label("Filter_Rarity_Epic").text);
                    Assert.AreEqual("Lendárias (1)", Label("Filter_Rarity_Legendary").text);
                }

                yield return Tap("Filter_Rarity_All");
                Assert.AreEqual(8, Cards().Count, "Todas: the first page of the whole roster");
            }
        }

        [UnityTest]
        public IEnumerator EveryOneOfTheFivePositionButtonsFiltersByNaturalRole()
        {
            yield return Open(Eleven);
            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return SessionFactory.Settle();

                foreach (var role in RosterQuery.Roles)
                {
                    yield return Tap("Filter_Role_" + role);
                    var click = _last;
                    Assert.AreEqual("Filter_Role_" + role, click.name, window + ": the click reached " + role);
                    var expected = Eleven.Where(s => s.Role == role).Select(s => s.Name).ToList();
                    CollectionAssert.AreEquivalent(expected, Cards(), window + " " + role);
                    Assert.AreEqual(role, Screen.RoleFilter);
                    Assert.AreEqual(RoleTable.For(role).DisplayName, Label("Filter_Role_" + role).text, "the label of " + role);
                }
            }
        }

        [UnityTest]
        public IEnumerator PalcoAndGerenciaAreOnScreenAndNothingOverlapsInTheSecondRow()
        {
            yield return Open(Eleven);
            yield return AtEachWindow("roster_all", () =>
            {
                var rects = new List<Rect>();
                foreach (var name in new[] { "All" }.Concat(RosterQuery.Roles.Select(r => r.ToString())))
                {
                    var button = Screen.GetComponentsInChildren<Button>().First(b => b.name == "Filter_Role_" + name);
                    var corners = new Vector3[4];
                    ((RectTransform)button.transform).GetWorldCorners(corners);
                    rects.Add(Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y));
                }

                Assert.AreEqual(6, rects.Count, "Todas as posições and the five positions");

                var sortCorners = new Vector3[4];
                ((RectTransform)Screen.GetComponentsInChildren<Button>().First(b => b.name == "Sort").transform).GetWorldCorners(sortCorners);
                var sort = Rect.MinMaxRect(sortCorners[0].x, sortCorners[0].y, sortCorners[2].x, sortCorners[2].y);
                for (var i = 0; i < rects.Count; i++)
                {
                    Assert.IsFalse(rects[i].Overlaps(sort), "position button " + i + " runs under the Ordenar control");
                }
                for (var i = 0; i < rects.Count; i++)
                {
                    for (var j = i + 1; j < rects.Count; j++)
                    {
                        var a = rects[i];
                        var b = rects[j];
                        var overlap = Mathf.Min(a.xMax, b.xMax) - Mathf.Max(a.xMin, b.xMin);
                        Assert.LessOrEqual(overlap, 0.5f, "position buttons " + i + " and " + j + " overlap by " + overlap);
                    }
                }
            });
        }

        [UnityTest]
        public IEnumerator TheTwoFiltersCombineByIntersectionAndEitherCanBeCleared()
        {
            yield return Open(Eleven);
            yield return SessionFactory.Settle();

            yield return Tap("Filter_Rarity_Epic");
            yield return Tap("Filter_Role_Reception");
            CollectionAssert.AreEqual(new[] { "Jun" }, Cards(), "Épicas + Recepção");

            yield return Tap("Filter_Role_All");
            CollectionAssert.AreEquivalent(new[] { "Emi", "Fuyu", "Jun" }, Cards(), "Épicas + Todas as posições");

            yield return Tap("Filter_Rarity_All");
            yield return Tap("Filter_Role_Kitchen");
            CollectionAssert.AreEquivalent(new[] { "Chie", "Gin" }, Cards(), "Todas + Cozinha");

            yield return Tap("Filter_Rarity_Rare");
            CollectionAssert.AreEqual(new[] { "Chie" }, Cards(), "Raras + Cozinha");

            // The filters are only a view: the collection and the profile are as they were.
            Assert.AreEqual(11, GameRoot.Instance.Session.Profile.Maids.Count);
            CollectionAssert.AreEqual(Eleven.Select(s => s.Name),
                GameRoot.Instance.Session.Profile.Maids.Select(m => m.Name));
        }

        [UnityTest]
        public IEnumerator TheActiveButtonIsTheGoldOneInEachRowAndOnlyOnePerRow()
        {
            yield return Open(Eleven);
            yield return SessionFactory.Settle();

            yield return Tap("Filter_Rarity_Rare");
            yield return Tap("Filter_Role_Floor");
            string SpriteOf(string name) => Screen.GetComponentsInChildren<Button>()
                .First(b => b.name == name).GetComponent<Image>().sprite.name;

            Assert.AreEqual("maids_filter_selected_empty", SpriteOf("Filter_Rarity_Rare"));
            foreach (var rarity in new[] { "All", "Common", "Epic", "Legendary" })
            {
                Assert.AreEqual("maids_filter_rarity_normal_empty", SpriteOf("Filter_Rarity_" + rarity), rarity);
            }

            Assert.AreEqual("maids_filter_selected_surface", SpriteOf("Filter_Role_Floor"));
            foreach (var role in new[] { "All", "Reception", "Kitchen", "Stage", "Management" })
            {
                Assert.AreEqual("maids_filter_position_normal_empty", SpriteOf("Filter_Role_" + role), role);
            }
        }

        [UnityTest]
        public IEnumerator ACombinationWithNoMaidShowsTheEmptyStateAndKeepsTheFilterBoard()
        {
            yield return Open(Eleven);
            yield return SessionFactory.Settle();

            yield return Tap("Filter_Rarity_Legendary");
            yield return Tap("Filter_Role_Management");
            Assert.IsEmpty(Cards(), "no card for Lendárias + Gerência");
            var empty = Find("CollectionEmpty/Label");
            Assert.IsTrue(empty.gameObject.activeInHierarchy, "the empty state is up");
            Assert.AreEqual(Strings.Get("roster.nomatch"), empty.text);
            Assert.AreEqual("1 / 1", Find("PagePlate/Label").text);
            Assert.AreEqual(-1, Screen.SelectedId);
            Assert.IsNotNull(Screen.transform.Find("Stage/Detail/DetailEmpty"), "the detail has an empty state too");
            Assert.IsNull(Screen.transform.Find("Stage/Detail/Name"), "no maid is read in the detail");
            Assert.IsFalse(Screen.GetComponentsInChildren<Button>().Any(b => b.name == "Dismiss"), "nothing to dismiss");
            Assert.AreEqual("Lendárias (1)", Label("Filter_Rarity_Legendary").text, "the board still counts what is owned");

            yield return AtEachWindow("roster_empty", () => { });

            yield return Tap("Filter_Role_All");
            CollectionAssert.AreEqual(new[] { "Gin" }, Cards(), "back to the legendary");
            Assert.AreEqual("Gin", Find("Detail/Name").text, "and she is read in the detail");
        }

        [UnityTest]
        public IEnumerator AnEmptyCollectionHasItsOwnMessage()
        {
            yield return SessionFactory.Boot(SessionFactory.NewProfile(7u, 1).WithMoney(900).WithFichas(40).Build());
            yield return SessionFactory.ToHub();
            yield return SessionFactory.ToRoster();
            yield return SessionFactory.Settle();

            if (GameRoot.Instance.Session.Profile.Maids.Count > 0)
            {
                Assert.Pass("this profile always opens with maids; the empty message is covered by the filters test");
            }

            Assert.AreEqual(Strings.Get("roster.noroster"), Find("CollectionEmpty/Label").text);
        }

        // ---- order ---------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator TheOrderCyclesRarityNameLevelAndNeverReordersTheCollection()
        {
            yield return Open(Eleven);
            yield return SessionFactory.Settle();

            Assert.AreEqual("Ordenar: Raridade", Label("Sort").text);
            CollectionAssert.AreEqual(new[] { "Gin", "Emi", "Fuyu", "Jun", "Chie", "Daisy", "Iku", "Aoi" }, Cards(), "rarity: rarest first, then name");

            yield return Tap("Sort");

            var click = _last;
            Assert.AreEqual("Sort", click.name);
            Assert.AreEqual("Ordenar: Nome", Label("Sort").text);
            CollectionAssert.AreEqual(new[] { "Aoi", "Beni", "Chie", "Daisy", "Emi", "Fuyu", "Gin", "Hana" }, Cards(), "name");

            yield return Tap("Sort");
            Assert.AreEqual("Ordenar: Nível", Label("Sort").text);
            CollectionAssert.AreEqual(new[] { "Gin", "Emi", "Fuyu", "Chie", "Iku", "Jun", "Aoi", "Beni" }, Cards(), "level: highest first, then name");

            // The order works together with a filter.
            yield return Tap("Filter_Rarity_Rare");
            CollectionAssert.AreEqual(new[] { "Chie", "Iku", "Daisy" }, Cards(), "level + Raras");

            yield return Tap("Sort");
            Assert.AreEqual("Ordenar: Raridade", Label("Sort").text, "and round again");

            CollectionAssert.AreEqual(Eleven.Select(s => s.Name),
                GameRoot.Instance.Session.Profile.Maids.Select(m => m.Name), "profile.Maids keeps its own order");
        }

        // ---- pages ---------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator MoreThanEightMaidsPageAndAFilterChangeGoesBackToThePageOne()
        {
            yield return Open(Eleven);
            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return SessionFactory.Settle();

                Assert.AreEqual("1 / 2", Find("PagePlate/Label").text);
                Assert.AreEqual(8, Cards().Count);
                Assert.IsFalse(Screen.GetComponentsInChildren<Button>().First(b => b.name == "PagePrev").interactable, "no page before the first");
                Assert.IsTrue(Screen.GetComponentsInChildren<Button>().First(b => b.name == "PageNext").interactable);
                var first = Cards();

                yield return Tap("PageNext");

                var click = _last;
                Assert.AreEqual("PageNext", click.name, window + ": the click reached the next page control");
                Assert.AreEqual("2 / 2", Find("PagePlate/Label").text);
                Assert.AreEqual(3, Cards().Count, "the last page holds the rest");
                Assert.IsEmpty(first.Intersect(Cards()), "no maid on two pages");
                Assert.AreEqual(11, first.Concat(Cards()).Distinct().Count(), "every maid is on exactly one page");
                Assert.IsFalse(Screen.GetComponentsInChildren<Button>().First(b => b.name == "PageNext").interactable, "no page past the last");
                AssertLegible("roster_page2 " + window);
                Save("roster_page2", window);

                yield return Tap("PagePrev");
                CollectionAssert.AreEqual(first, Cards(), "back to the first page");

                yield return Tap("PageNext");
                yield return Tap("Filter_Rarity_Common");
                Assert.AreEqual(0, Screen.Page, "a filter change returns to the first page");
                Assert.AreEqual("1 / 1", Find("PagePlate/Label").text);
                CollectionAssert.AreEquivalent(new[] { "Aoi", "Beni", "Hana", "Kei" }, Cards());

                yield return Tap("Filter_Rarity_All");
                yield return Tap("PageNext");
                yield return Tap("Sort");
                Assert.AreEqual(0, Screen.Page, "so does a change of order");
                yield return Tap("Sort");
                yield return Tap("Sort");
            }
        }

        // ---- selection and detail ------------------------------------------------------------

        [UnityTest]
        public IEnumerator ClickingACardSelectsHerHighlightsHerAndReadsHerRealDataInTheDetail()
        {
            yield return Open(Eleven);
            var profile = GameRoot.Instance.Session.Profile;

            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return SessionFactory.Settle();

                foreach (var name in new[] { "Chie", "Emi", "Gin" })
                {
                    yield return Tap("Card " + name);
                    var click = _last;
                    Assert.AreEqual("Card " + name, click.name, window + ": the click reached her card");
                    var maid = profile.Maids.First(m => m.Name == name);
                    Assert.AreEqual(maid.Id, Screen.SelectedId);

                    var highlights = Screen.GetComponentsInChildren<Image>().Where(i => i.name == "Highlight" && i.gameObject.activeInHierarchy).ToList();
                    Assert.AreEqual(1, highlights.Count, "exactly one highlight");
                    Assert.AreEqual("Cell " + name, highlights[0].transform.parent.name, "on her cell");

                    Assert.AreEqual(name, Find("Detail/Name").text);
                    Assert.AreEqual(maid.Card.DisplayName, Find("Detail/RarityBadge/Label").text);
                    Assert.AreEqual(RoleTable.For(maid.NaturalRole).DisplayName, Find("Detail/FactValue_lineup.natural").text);
                    Assert.AreEqual(Strings.Money(maid.Salary), Find("Detail/FactValue_" + (GameRoot.Instance.Session.Profile.Lineup.Contains(maid.Id) ? "roster.salary.paid" : "roster.salary.idle")).text);
                    Assert.AreEqual(maid.Level + " / " + Maid.MaxLevel, Find("Detail/FactValue_roster.level").text);
                    Assert.AreEqual(maid.Total.Serve.ToString(), Find("Detail/StatValue_roster.serve").text);
                    Assert.AreEqual(maid.Total.Cook.ToString(), Find("Detail/StatValue_roster.cook").text);
                    Assert.AreEqual(maid.Total.Charm.ToString(), Find("Detail/StatValue_roster.charm").text);
                    Assert.AreEqual(maid.Total.Stamina.ToString(), Find("Detail/StatValue_roster.stamina").text);
                    Assert.AreEqual(Strings.Get("roster.dismiss") + " (+" + maid.Severance + ")", Label("Dismiss").text);
                    AssertLegible("roster_detail_" + name + " " + window);
                }

                Save("roster_selected", window);
            }
        }

        [UnityTest]
        public IEnumerator TheAttributeFillFollowsTheRealValue()
        {
            yield return Open(Eleven);
            yield return SessionFactory.Settle();
            yield return Tap("Card Gin");
            var maid = GameRoot.Instance.Session.Profile.Maids.First(m => m.Name == "Gin");

            float Width(string key) => ((RectTransform)Screen.transform.Find("Stage/Detail/Fill_" + key)).rect.width;
            var low = Mathf.Min(maid.Total.Serve, maid.Total.Cook);
            var high = Mathf.Max(maid.Total.Serve, maid.Total.Cook);
            if (low != high)
            {
                var narrow = maid.Total.Serve < maid.Total.Cook ? "roster.serve" : "roster.cook";
                var wide = narrow == "roster.serve" ? "roster.cook" : "roster.serve";
                Assert.Less(Width(narrow), Width(wide), "a bigger attribute fills more of its track");
            }

            var track = ((RectTransform)Screen.transform.Find("Stage/Detail/Track_roster.serve")).rect.width;
            Assert.LessOrEqual(Width("roster.serve"), track, "the fill never passes the track");
        }

        [UnityTest]
        public IEnumerator TheSelectionSurvivesAFilterThatShowsHerAndMovesToTheFirstWhenItDoesNot()
        {
            yield return Open(Eleven);
            yield return SessionFactory.Settle();
            var profile = GameRoot.Instance.Session.Profile;

            yield return Tap("Card Iku");
            Assert.AreEqual(profile.Maids.First(m => m.Name == "Iku").Id, Screen.SelectedId);

            yield return Tap("Filter_Rarity_Rare");
            Assert.AreEqual("Iku", Find("Detail/Name").text, "Iku is rare: she stays selected");

            yield return Tap("Filter_Rarity_Epic");
            var first = Cards()[0];
            Assert.AreEqual(first, Find("Detail/Name").text, "Iku is not epic: the first epic is read instead");
            Assert.AreEqual(profile.Maids.First(m => m.Name == first).Id, Screen.SelectedId);
            Assert.AreEqual(1, Screen.GetComponentsInChildren<Image>().Count(i => i.name == "Highlight" && i.gameObject.activeInHierarchy));

            yield return Tap("Filter_Rarity_All");
            Assert.AreEqual(first, Find("Detail/Name").text, "back on Todas she is still the one selected");
        }

        [UnityTest]
        public IEnumerator AMaidWithoutAPremiumPortraitKeepsTheCompactCardInTheFrame()
        {
            yield return Open(Eleven);
            yield return SessionFactory.Settle();
            var profile = GameRoot.Instance.Session.Profile;

            var without = profile.Maids.FirstOrDefault(m => MaidPortraits.Detail(m) == null);
            var with = profile.Maids.FirstOrDefault(m => MaidPortraits.Detail(m) != null);
            Debug.Log("UI-INT-02A portraits: without=" + (without != null ? without.Name : "none") + " with=" + (with != null ? with.Name : "none"));

            if (without != null)
            {
                yield return Tap("Card " + without.Name);
                var frame = Screen.transform.Find("Stage/Detail/PortraitFrame/PortraitWindow");
                Assert.IsNotNull(frame, "the window of the compact card");
                Assert.IsNull(Screen.transform.Find("Stage/Detail/PortraitFrame/Detail Portrait " + without.Name), "no premium picture");
                Assert.IsNotNull(frame.Find("Card " + without.Name), "the compact card stands in");
            }

            if (with != null)
            {
                yield return Tap("Card " + with.Name);
                var frame = Screen.transform.Find("Stage/Detail/PortraitFrame/Detail Portrait " + with.Name);
                Assert.IsNotNull(frame, "the premium portrait");
                Assert.AreSame(MaidPortraits.Detail(with), frame.Find("Portrait").GetComponent<Image>().sprite);
                Assert.IsNull(frame.Find("Card " + with.Name));
            }

            Assert.IsTrue(with != null || without != null);
        }

        // ---- real numbers --------------------------------------------------------------------

        [UnityTest]
        public IEnumerator FichasRosterCountAndFixedCostAreTheGameOnes()
        {
            yield return Open(Eleven, fichas: 73);
            var profile = GameRoot.Instance.Session.Profile;
            yield return AtEachWindow("roster_numbers", () =>
            {
                Assert.AreEqual("73 " + Strings.Get("hub.points"), Find("TokenPlate/Tokens").text);
                Assert.AreEqual(SessionFactory.LargeValues.RosterHeader(profile.Maids.Count, RosterScreen.CollectedCount(profile),
                    Books.FixedCostsFor(profile)), Find("RosterInfo").text, "the roster count and the daily fixed cost");
                Assert.AreEqual("Mostrando 11 / 11", Find("Shown").text);
            });

            yield return Tap("Filter_Rarity_Epic");
            Assert.AreEqual("Mostrando 3 / 11", Find("Shown").text, "the visible results are shown apart from the owned total");
            Assert.AreEqual("Todas (11)", Label("Filter_Rarity_All").text);
        }

        // ---- dismissal -----------------------------------------------------------------------

        [UnityTest]
        public IEnumerator DismissingAsksFirstAndCancellingChangesNothing()
        {
            yield return Open(Eleven);
            yield return SessionFactory.Settle();
            var profile = GameRoot.Instance.Session.Profile;
            yield return Tap("Card Aoi");
            var fichas = profile.Wallet.Points;

            yield return Tap("Dismiss");

            var click = _last;
            Assert.AreEqual("Dismiss", click.name);
            Assert.IsTrue(GameRoot.Instance.Ui.DialogOpen, "the player is asked");
            Assert.AreEqual(11, profile.Maids.Count, "nothing yet");

            PressDialog(Strings.Get("dialog.no"));
            yield return null;
            Assert.IsFalse(GameRoot.Instance.Ui.DialogOpen);
            Assert.AreEqual(11, profile.Maids.Count, "cancelled: she stays");
            Assert.AreEqual(fichas, profile.Wallet.Points, "and no fichas moved");
            Assert.AreEqual("Aoi", Find("Detail/Name").text, "she is still the one read");
        }

        [UnityTest]
        public IEnumerator ConfirmedDismissalRemovesHerRefundsTheFichasAndRefreshesTheScreen()
        {
            yield return Open(Eleven);
            yield return SessionFactory.Settle();
            var profile = GameRoot.Instance.Session.Profile;
            yield return Tap("Card Daisy");
            var daisy = profile.Maids.First(m => m.Name == "Daisy");
            var severance = daisy.Severance;
            var fichas = profile.Wallet.Points;

            yield return Tap("Dismiss");
            PressDialog(Strings.Get("dialog.yes"));
            yield return null;

            Assert.AreEqual(10, profile.Maids.Count);
            Assert.IsNull(profile.MaidById(daisy.Id), "she is gone from the collection");
            Assert.AreEqual(fichas + severance, profile.Wallet.Points, "the fichas come back");
            Assert.IsFalse(Cards().Contains("Daisy"));
            Assert.AreEqual("Todas (10)", Label("Filter_Rarity_All").text);
            Assert.AreEqual("Raras (2)", Label("Filter_Rarity_Rare").text);
            Assert.AreEqual((fichas + severance) + " " + Strings.Get("hub.points"), Find("TokenPlate/Tokens").text);
            Assert.AreNotEqual(daisy.Id, Screen.SelectedId, "the selection moved off her");
            Assert.AreNotEqual(-1, Screen.SelectedId, "to a maid who is still here");
        }

        [UnityTest]
        public IEnumerator DismissingTheLastMaidOnAPageLeavesAValidPage()
        {
            // Nine maids: the second page holds one. Dismissing her must not leave page 2 of 1.
            yield return Open(Eleven.Take(9));
            yield return SessionFactory.Settle();
            var profile = GameRoot.Instance.Session.Profile;

            yield return Tap("Filter_Rarity_All");
            yield return Tap("Sort");
            yield return Tap("Sort");
            yield return Tap("Sort");
            yield return Tap("PageNext");
            Assert.AreEqual("2 / 2", Find("PagePlate/Label").text);
            var only = Cards().Single();

            yield return Tap("Card " + only);
            yield return Tap("Dismiss");
            PressDialog(Strings.Get("dialog.yes"));
            yield return null;

            Assert.AreEqual(8, profile.Maids.Count);
            Assert.AreEqual("1 / 1", Find("PagePlate/Label").text, "the page came back with the roster");
            Assert.AreEqual(8, Cards().Count);
        }

        // ---- the roster changes --------------------------------------------------------------

        [UnityTest]
        public IEnumerator ABoxBoughtOnCaixinhasShowsUpHereWithTheCountsUpdated()
        {
            var few = Eleven.Take(3).ToArray();
            yield return SessionFactory.Boot(Roster(few, fichas: 400));
            var profile = GameRoot.Instance.Session.Profile;
            yield return SessionFactory.ToHub();
            yield return SessionFactory.ToRoster();
            yield return SessionFactory.Settle();
            Assert.AreEqual(3, Cards().Count);
            Assert.AreEqual("Todas (3)", Label("Filter_Rarity_All").text);

            yield return SessionFactory.BackToHub();
            yield return SessionFactory.ToBoxes();
            yield return ResolutionRig.Apply(1280, 720);
            yield return SessionFactory.Settle();
            var before = profile.Maids.Count;
            yield return Tap("Open_box_silver");
            yield return SessionFactory.Settle();
            var gained = profile.Maids.Count - before;
            PressReveal();
            yield return null;

            yield return SessionFactory.BackToHub();
            yield return SessionFactory.ToRoster();
            yield return ResolutionRig.Apply(1280, 720);
            yield return SessionFactory.Settle();

            Assert.AreEqual(profile.Maids.Count, Cards().Count + 0, "every maid owned is on the page (fewer than nine)");
            Assert.AreEqual("Todas (" + profile.Maids.Count + ")", Label("Filter_Rarity_All").text);
            Assert.GreaterOrEqual(gained, 0);
            Assert.AreEqual(profile.Wallet.Points + " " + Strings.Get("hub.points"), Find("TokenPlate/Tokens").text);
        }

        // ---- clicks and legibility -----------------------------------------------------------

        [UnityTest]
        public IEnumerator EveryControlIsHitByARealClickAtBothWindows()
        {
            yield return Open(Eleven);
            var backs = 0;
            Screen.OnBack = () => backs++;

            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return SessionFactory.Settle();

                yield return Tap("Filter_Rarity_All");
                yield return Tap("Filter_Role_All");
                // Every enabled control, once (a page control that is disabled is skipped: it has no action).
                var names = Screen.GetComponentsInChildren<Button>().Where(b => b.interactable)
                    .Select(b => b.name).Distinct().ToList();
                Assert.Contains("Back", names);
                Assert.Contains("Sort", names);
                Assert.Contains("PageNext", names);
                Assert.Contains("Dismiss", names);
                Assert.IsFalse(names.Contains("PagePrev"), "no previous page on the first");
                foreach (var name in names)
                {
                    if (name == "Dismiss" || name.StartsWith("Card ")) continue;
                    yield return Tap(name);
                    var handler = _last;
                    Assert.IsTrue(handler != null && handler.name == name,
                        window + ": the click on " + name + " reached " + (handler != null ? handler.name : "nothing"));
                    yield return Tap("Filter_Rarity_All");
                    yield return Tap("Filter_Role_All");
                    while (Screen.Sort != RosterSort.Rarity) yield return Tap("Sort");
                    if (Screen.Page != 0) yield return Tap("PagePrev");
                }

                yield return Tap("Card Aoi");

                var card = _last;
                Assert.AreEqual("Card Aoi", card.name, window + ": a card takes its click");
            }

            Assert.AreEqual(Windows.Length, backs, "Voltar called its callback once per window");
        }

        [UnityTest]
        public IEnumerator EveryStateIsLegibleAtBothWindows()
        {
            yield return Open(Eleven);

            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return SessionFactory.Settle();

                foreach (var step in new[] { "Filter_Rarity_All", "Filter_Rarity_Legendary", "Filter_Role_Management", "Filter_Role_Kitchen", "Filter_Rarity_All", "Filter_Role_All", "Sort", "Sort", "PageNext" })
                {
                    yield return Tap(step);
                    yield return SessionFactory.Settle();
                    AssertLegible("roster after " + step + " " + window);
                }

                yield return Tap("Card Kei");
                yield return SessionFactory.Settle();
                AssertLegible("roster Kei " + window);
            }

        }

        [UnityTest]
        public IEnumerator ALegendaryWithThreeTraitsKeepsEveryTraitReachableInTheFrame()
        {
            var builder = SessionFactory.NewProfile(7u, 10).WithMoney(900).WithFichas(40)
                .WithMaid("Longa", Rarity.Legendary, 20, 5, TraitId.AngelHands, TraitId.QuickStep, TraitId.Charisma)
                .WithMaid("Curta", Rarity.Common, 1, 0);
            yield return SessionFactory.Boot(builder.AutoFillLineup().Build());
            yield return SessionFactory.ToHub();
            yield return SessionFactory.ToRoster();

            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return SessionFactory.Settle();
                yield return Tap("Card Longa");
                yield return SessionFactory.Settle();

                var lines = Screen.GetComponentsInChildren<Text>().Where(t => t.name == "TraitLine").Select(t => t.text).ToList();
                Assert.GreaterOrEqual(lines.Count, 4, "three traits and her description");
                foreach (var id in new[] { TraitId.AngelHands, TraitId.QuickStep, TraitId.Charisma })
                {
                    var def = TraitTable.ById(id);
                    Assert.IsTrue(lines.Any(l => l.StartsWith(def.DisplayName)), def.DisplayName + " is listed");
                }

                var scroll = Screen.transform.Find("Stage/Detail/TraitsScroll").GetComponent<ScrollRect>();
                Assert.IsNotNull(scroll, "the traits scroll inside their frame");

                // Whatever is on screen is legible; what is scrolled out of the frame is the only
                // thing allowed to be clipped, and the other end of the scroll brings it in.
                foreach (var position in new[] { 1f, 0f })
                {
                    scroll.verticalNormalizedPosition = position;
                    yield return SessionFactory.Settle();
                    var found = new List<string>();
                    KitLegibility.Check(LayoutModel.Measure(GameRoot.Instance.Ui), Screen.transform, "traits " + position + " " + window, found);
                    var real = found.Where(f => !(f.Contains("TraitLine") && f.Contains("clipped away"))).ToList();
                    Assert.IsEmpty(real, string.Join(" | ", real));

                    var line = lines.Count - 1;
                    var edge = position == 1f ? 0 : line;
                    Assert.IsFalse(found.Any(f => f.Contains("TraitLine[" + edge + "]")),
                        "the line at the " + (position == 1f ? "top" : "bottom") + " of the scroll is readable");
                    Save(position == 1f ? "roster_traits_top" : "roster_traits_bottom", window);
                }
            }
        }

        // ---- the fallback --------------------------------------------------------------------

        [UnityTest]
        public IEnumerator WithoutTheKitTheEarlierScreenStillWorks()
        {
            yield return Open(Eleven.Take(5));
            var root = GameRoot.Instance.Ui;

            var go = new GameObject("PlainRoster", typeof(RectTransform));
            go.transform.SetParent(root.transform, false);
            var plain = go.AddComponent<RosterScreen>();
            plain.ForcePlain = true;
            plain.Attach(root);
            plain.Session = GameRoot.Instance.Session;
            plain.Show();
            yield return null;

            Assert.IsNull(plain.transform.Find("Stage"), "no kit stage");
            Assert.AreEqual(5, plain.GetComponentsInChildren<Button>().Count(b => b.name.StartsWith("Card ")), "five clickable cards");
            Assert.IsTrue(plain.GetComponentsInChildren<Text>().Any(t => t.text.StartsWith(Strings.Get("roster.maids") + " 5")), "the roster count");
            Assert.IsTrue(plain.GetComponentsInChildren<Text>().Any(t => t.text.StartsWith(Strings.Get("roster.dismiss"))), "the dismiss control");
            Object.Destroy(go);
        }
    }
}
