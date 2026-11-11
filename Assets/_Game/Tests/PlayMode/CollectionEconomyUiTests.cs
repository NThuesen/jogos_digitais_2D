using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Neko.Game.Core;
using Neko.Game.Ui;
using Neko.Sim.Economy;
using Neko.Sim.Maids;
using Neko.Sim.Team;
using Neko.Sim.Save;
using Neko.Tests.PlayMode.Layout;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Neko.Tests.PlayMode
{
    /// <summary>
    /// ECON-03 on the screens: a collection with no cap (more maids than the old eight), the Maids header
    /// that says how many she really has, and the fixed cost of the Equipe do Dia that follows the team.
    /// Real clicks, both reference windows.
    /// </summary>
    public sealed class CollectionEconomyUiTests
    {
        private static readonly Vector2Int[] Windows = { new Vector2Int(1280, 720), new Vector2Int(960, 600) };

        private static readonly string[] Names =
        {
            "Aoi", "Beni", "Chie", "Daisy", "Emi", "Fuyu", "Gin", "Hana", "Iku", "Jun", "Kei", "Lia", "Mio", "Nao",
            "Ora", "Pia", "Qui", "Rin", "Sui", "Tae",
        };

        private static readonly Rarity[] Rarities = { Rarity.Common, Rarity.Rare, Rarity.Epic, Rarity.Legendary };

        [TearDown]
        public void Restore() => ResolutionRig.Restore();

        /// <summary>Level 10 café, so the lineup is wide and the old cap was eight.</summary>
        private static Profile Collection(int maids)
        {
            var builder = SessionFactory.NewProfile(7u, 10).WithMoney(900).WithFichas(43);
            for (var i = 0; i < maids; i++) builder.WithMaid(Names[i], Rarities[i % 4], 1 + i % 9, 2);
            return builder.AutoFillLineup().Build();
        }

        private static RosterScreen Roster => GameRoot.Instance.Ui.Get<RosterScreen>();

        private static LineupScreen Lineup => GameRoot.Instance.Ui.Get<LineupScreen>();

        private static IEnumerator OpenRoster(Profile profile)
        {
            yield return SessionFactory.Boot(profile);
            yield return SessionFactory.ToHub();
            yield return SessionFactory.ToRoster();
            yield return ResolutionRig.Apply(1280, 720);
            yield return SessionFactory.Settle();
        }

        private static IEnumerator OpenLineup(Profile profile)
        {
            yield return SessionFactory.Boot(profile);
            yield return SessionFactory.ToHub();
            yield return SessionFactory.ToLineup();
            yield return ResolutionRig.Apply(1280, 720);
            yield return SessionFactory.Settle();
        }

        private static IEnumerator AtEachWindow(System.Action check)
        {
            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return SessionFactory.Settle();
                check();
            }
        }

        private static IEnumerator Tap(Component screen, string name)
        {
            Canvas.ForceUpdateCanvases();
            var clicks = new Clicks(LayoutModel.Measure(GameRoot.Instance.Ui));
            var control = clicks.Measurement.Controls.First(c => c.Component.name == name);
            var points = clicks.SamplePoints(control);
            Assert.IsTrue(points.Count > 0, name + " draws nothing, so it has no centre");
            Assert.IsNotNull(clicks.Click(points[0]), "a click on " + name + " reached nothing");
            yield return null;
        }

        private static Text TextAt(Component screen, string path) =>
            screen.transform.Find("Stage/" + path).GetComponentInChildren<Text>(true);

        /// <summary>The profile the running game uses (Boot works on its own copy, not on the one a test built).</summary>
        private static Profile Live => GameRoot.Instance.Session.Profile;

        private static List<string> Cards() => Roster.GetComponentsInChildren<Button>()
            .Where(b => b.name.StartsWith("Card ")).Select(b => b.name.Substring(5)).ToList();

        // ---- Maids ---------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator TheMaidsHeaderCountsTheRealCollectionAndHasNoCap()
        {
            var profile = Collection(17);
            Assert.Greater(profile.Maids.Count, 8, "more maids than the old cap of the café level");
            yield return OpenRoster(profile);
            profile = Live;

            yield return AtEachWindow(() =>
            {
                var shown = TextAt(Roster, "RosterInfo").text;
                Assert.AreEqual(SessionFactory.LargeValues.RosterHeader(17, RosterScreen.CollectedCount(profile),
                    Books.FixedCostsFor(profile)), shown);
                StringAssert.Contains("17", shown);
                StringAssert.DoesNotContain(" / ", shown, "there is no limit to show");
            });
        }

        [UnityTest]
        public IEnumerator TheFixedCostShownIsTheFieldedTeamPlusRentAndNotTheBench()
        {
            var profile = Collection(17);
            var fielded = profile.FieldedMaids().Sum(m => m.Salary);
            var everyone = profile.Maids.Sum(m => m.Salary);
            Assert.Less(fielded, everyone, "a bench exists");
            yield return OpenRoster(profile);
            profile = Live;

            var expected = Strings.Money(Books.RentFor(profile) + fielded);
            StringAssert.Contains(expected, TextAt(Roster, "RosterInfo").text);
            Assert.AreNotEqual(Books.RentFor(profile) + everyone, Books.FixedCostsFor(profile));
        }

        [UnityTest]
        public IEnumerator ACollectionOfMoreThanEightKeepsItsPagesAndFilters()
        {
            var profile = Collection(19);
            yield return OpenRoster(profile);
            profile = Live;

            yield return AtEachWindow(() =>
            {
                Assert.AreEqual(8, Cards().Count, "a page is four by two");
                Assert.AreEqual("Mostrando 19 / 19", TextAt(Roster, "Shown").text);
            });

            yield return Tap(Roster, "PageNext");
            Assert.AreEqual(8, Cards().Count, "the second page is full too");
            yield return Tap(Roster, "PageNext");
            Assert.AreEqual(3, Cards().Count, "and the third holds the last three");

            var epics = profile.Maids.Count(m => m.Rarity == Rarity.Epic);
            yield return Tap(Roster, "Filter_Rarity_Epic");
            Assert.AreEqual("Mostrando " + epics + " / 19", TextAt(Roster, "Shown").text,
                "a filter narrows the visible list, never the owned total");
            Assert.AreEqual(epics, Cards().Count);
            Assert.AreEqual("Todas (19)", Roster.GetComponentsInChildren<Button>().First(b => b.name == "Filter_Rarity_All")
                .GetComponentInChildren<Text>().text);
        }

        [UnityTest]
        public IEnumerator DismissingABenchedMaidDoesNotChangeTheFixedCost()
        {
            var profile = Collection(14);
            var benched = profile.Maids.First(m => !profile.Lineup.Contains(m.Id));
            var before = Books.FixedCostsFor(profile);
            var points = profile.Wallet.Points;

            Assert.IsTrue(profile.Dismiss(benched.Id));

            Assert.AreEqual(before, Books.FixedCostsFor(profile), "she was free to keep, so letting her go saves nothing");
            Assert.Greater(profile.Wallet.Points, points, "the refund in fichas is unchanged");
            yield return null;
        }

        // ---- Equipe do Dia -------------------------------------------------------------------

        [UnityTest]
        public IEnumerator TheLineupCostFollowsTheTeamAsItIsEmptiedAndFilledAgain()
        {
            var profile = Collection(14);
            yield return OpenLineup(profile);
            profile = Live;
            var full = Books.FixedCostsFor(profile);
            Assert.Greater(profile.Maids.Count, profile.Lineup.All.Count(s => !s.Empty), "a bench exists");

            yield return AtEachWindow(() =>
                Assert.AreEqual(Strings.Money(Books.FixedCostsFor(profile)), TextAt(Lineup, "CostValue").text));

            yield return Tap(Lineup, "Clear");
            Assert.AreEqual(Books.RentFor(profile), Books.FixedCostsFor(profile), "an empty team costs only the rent");
            Assert.AreEqual(Strings.Money(Books.RentFor(profile)), TextAt(Lineup, "CostValue").text);

            yield return Tap(Lineup, "Auto");
            Assert.AreEqual(Strings.Money(Books.FixedCostsFor(profile)), TextAt(Lineup, "CostValue").text);
            Assert.AreEqual(full, Books.FixedCostsFor(profile), "the same team costs the same again");
        }

        [UnityTest]
        public IEnumerator FieldingABenchedMaidRaisesTheCostByHerWageAndNobodyElses()
        {
            var profile = Collection(14);
            profile.Lineup.Remove(profile.FieldedMaids()[0].Id); // leave a free slot to field her into
            yield return OpenLineup(profile);
            profile = Live;
            var before = Books.FixedCostsFor(profile);
            var benched = profile.Maids.Where(m => !profile.Lineup.Contains(m.Id)).OrderByDescending(m => m.Salary).First();

            yield return Tap(Lineup, "Card " + benched.Name);

            Assert.IsTrue(profile.Lineup.Contains(benched.Id), "the click fielded her");
            Assert.AreEqual(before + benched.Salary, Books.FixedCostsFor(profile));
            Assert.AreEqual(Strings.Money(Books.FixedCostsFor(profile)), TextAt(Lineup, "CostValue").text);
        }
    }
}
