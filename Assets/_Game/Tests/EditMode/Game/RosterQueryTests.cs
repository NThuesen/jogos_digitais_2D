using System.Collections.Generic;
using System.Linq;
using Neko.Game.Ui;
using Neko.Sim.Maids;
using Neko.Sim.Team;
using NUnit.Framework;

namespace Neko.Tests.Game
{
    /// <summary>
    /// UI-INT-02A: what the Maids screen shows of the collection. Presentation only: the filters
    /// and the order never touch the collection they read.
    /// </summary>
    public sealed class RosterQueryTests
    {
        private static Maid Make(int id, string name, Rarity rarity, Role role, int level)
        {
            var maid = Maid.Create(id, name, Archetype.Genki, level);
            maid.Rarity = rarity;
            maid.NaturalRole = role;
            return maid;
        }

        /// <summary>Eleven maids covering every rarity and all five natural positions.</summary>
        private static List<Maid> Collection() => new List<Maid>
        {
            Make(1, "Aoi", Rarity.Common, Role.Reception, 3),
            Make(2, "Beni", Rarity.Common, Role.Floor, 3),
            Make(3, "Chie", Rarity.Rare, Role.Kitchen, 5),
            Make(4, "Daisy", Rarity.Rare, Role.Reception, 1),
            Make(5, "Emi", Rarity.Epic, Role.Stage, 9),
            Make(6, "Fuyu", Rarity.Epic, Role.Management, 9),
            Make(7, "Gin", Rarity.Legendary, Role.Kitchen, 20),
            Make(8, "Hana", Rarity.Common, Role.Stage, 2),
            Make(9, "Iku", Rarity.Rare, Role.Floor, 5),
            Make(10, "Jun", Rarity.Epic, Role.Reception, 4),
            Make(11, "Kei", Rarity.Common, Role.Management, 1),
        };

        private static string Names(IEnumerable<Maid> maids) => string.Join(",", maids.Select(m => m.Name));

        [Test]
        public void EveryRarityFilterKeepsOnlyThatRarity()
        {
            var all = Collection();
            foreach (var rarity in RosterQuery.Rarities)
            {
                var shown = RosterQuery.Visible(all, rarity, null, RosterSort.Name);
                Assert.IsNotEmpty(shown, rarity + " has maids in this collection");
                Assert.IsTrue(shown.All(m => m.Rarity == rarity), rarity + ": " + Names(shown));
                Assert.AreEqual(all.Count(m => m.Rarity == rarity), shown.Count, rarity.ToString());
            }
        }

        [Test]
        public void EveryOneOfTheFivePositionsFiltersByNaturalRole()
        {
            var all = Collection();
            Assert.AreEqual(5, RosterQuery.Roles.Length, "the game has five natural positions");
            foreach (var role in RosterQuery.Roles)
            {
                var shown = RosterQuery.Visible(all, null, role, RosterSort.Name);
                Assert.IsNotEmpty(shown, role + " has maids in this collection");
                Assert.IsTrue(shown.All(m => m.NaturalRole == role), role + ": " + Names(shown));
            }

            Assert.AreEqual("Chie,Gin", Names(RosterQuery.Visible(all, null, Role.Kitchen, RosterSort.Name)));
        }

        [Test]
        public void TheTwoFiltersIntersect()
        {
            var all = Collection();
            Assert.AreEqual("Chie,Daisy,Iku", Names(RosterQuery.Visible(all, Rarity.Rare, null, RosterSort.Name)));
            Assert.AreEqual("Chie,Gin", Names(RosterQuery.Visible(all, null, Role.Kitchen, RosterSort.Name)));
            Assert.AreEqual("Jun", Names(RosterQuery.Visible(all, Rarity.Epic, Role.Reception, RosterSort.Name)));
            Assert.AreEqual("Gin", Names(RosterQuery.Visible(all, Rarity.Legendary, Role.Kitchen, RosterSort.Name)));
            Assert.AreEqual(all.Count, RosterQuery.Visible(all, null, null, RosterSort.Name).Count,
                "Todas + Todas is the whole roster");
        }

        [Test]
        public void ACombinationWithNoMaidIsEmpty()
        {
            var all = Collection();
            Assert.IsEmpty(RosterQuery.Visible(all, Rarity.Legendary, Role.Management, RosterSort.Rarity));
            Assert.AreEqual(1, RosterQuery.Pages(0), "an empty result is one empty page");
            Assert.AreEqual(0, RosterQuery.ClampPage(5, 0));
            Assert.AreEqual(-1, RosterQuery.Resolve(3, new List<Maid>()), "nothing is selected when nothing is shown");
        }

        [Test]
        public void TheCountsAreTheOwnedTotalWhateverTheOtherFilterIs()
        {
            var all = Collection();
            Assert.AreEqual(11, RosterQuery.Count(all, (Rarity?)null));
            Assert.AreEqual(4, RosterQuery.Count(all, (Rarity?)Rarity.Common));
            Assert.AreEqual(3, RosterQuery.Count(all, (Rarity?)Rarity.Rare));
            Assert.AreEqual(3, RosterQuery.Count(all, (Rarity?)Rarity.Epic));
            Assert.AreEqual(1, RosterQuery.Count(all, (Rarity?)Rarity.Legendary));
            Assert.AreEqual(3, RosterQuery.Count(all, (Role?)Role.Reception));
            Assert.AreEqual(2, RosterQuery.Count(all, (Role?)Role.Management));
            Assert.AreEqual(11, RosterQuery.Count(all, (Role?)null));
        }

        [Test]
        public void PagesHoldEightAndTheLastHoldsTheRest()
        {
            var all = Collection();
            var visible = RosterQuery.Visible(all, null, null, RosterSort.Name);
            Assert.AreEqual(2, RosterQuery.Pages(visible.Count));
            Assert.AreEqual(8, RosterQuery.Page(visible, 0).Count);
            Assert.AreEqual(3, RosterQuery.Page(visible, 1).Count);
            Assert.AreEqual(3, RosterQuery.Page(visible, 99).Count, "a page past the end is the last page");
            Assert.AreEqual(8, RosterQuery.Page(visible, -4).Count, "a page before the start is the first");
            Assert.AreEqual(1, RosterQuery.Pages(8));
            Assert.AreEqual(2, RosterQuery.Pages(9));
        }

        [Test]
        public void TheOrdersAreDeterministicWithTiesFallingToNameThenId()
        {
            var all = Collection();
            Assert.AreEqual("Gin,Emi,Fuyu,Jun,Chie,Daisy,Iku,Aoi,Beni,Hana,Kei",
                Names(RosterQuery.Visible(all, null, null, RosterSort.Rarity)), "rarest first, then name");
            Assert.AreEqual("Aoi,Beni,Chie,Daisy,Emi,Fuyu,Gin,Hana,Iku,Jun,Kei",
                Names(RosterQuery.Visible(all, null, null, RosterSort.Name)));
            Assert.AreEqual("Gin,Emi,Fuyu,Chie,Iku,Jun,Aoi,Beni,Hana,Daisy,Kei",
                Names(RosterQuery.Visible(all, null, null, RosterSort.Level)), "highest level first, then name");

            // The same input in any order gives the same output.
            var shuffled = all.OrderByDescending(m => m.Id).ToList();
            foreach (RosterSort sort in System.Enum.GetValues(typeof(RosterSort)))
            {
                Assert.AreEqual(Names(RosterQuery.Visible(all, null, null, sort)),
                    Names(RosterQuery.Visible(shuffled, null, null, sort)), sort.ToString());
            }
        }

        [Test]
        public void TwoMaidsWithTheSameNameAreOrderedById()
        {
            var twins = new List<Maid>
            {
                Make(9, "Mina", Rarity.Rare, Role.Floor, 4),
                Make(2, "Mina", Rarity.Rare, Role.Floor, 4),
            };
            foreach (RosterSort sort in System.Enum.GetValues(typeof(RosterSort)))
            {
                CollectionAssert.AreEqual(new[] { 2, 9 },
                    RosterQuery.Visible(twins, null, null, sort).Select(m => m.Id), sort.ToString());
            }
        }

        [Test]
        public void FilteringAndSortingNeverChangeTheCollection()
        {
            var all = Collection();
            var before = Names(all);
            RosterQuery.Visible(all, Rarity.Epic, Role.Stage, RosterSort.Level);
            RosterQuery.Visible(all, null, null, RosterSort.Name);
            Assert.AreEqual(before, Names(all));
        }

        [Test]
        public void TheSelectionStaysOnTheSameMaidWhileSheIsShownElseTheFirstElseNone()
        {
            var all = Collection();
            var rares = RosterQuery.Visible(all, Rarity.Rare, null, RosterSort.Name);
            Assert.AreEqual(9, RosterQuery.Resolve(9, rares), "Iku is rare: kept");
            Assert.AreEqual(rares[0].Id, RosterQuery.Resolve(1, rares), "Aoi is common: the first rare instead");
            Assert.AreEqual(-1, RosterQuery.Resolve(1, new List<Maid>()));
        }

        [Test]
        public void TheSortCyclesThroughAllThree()
        {
            var sort = RosterSort.Rarity;
            var seen = new HashSet<RosterSort>();
            for (var i = 0; i < 3; i++)
            {
                seen.Add(sort);
                sort = RosterQuery.Next(sort);
            }

            Assert.AreEqual(3, seen.Count);
            Assert.AreEqual(RosterSort.Rarity, sort, "and comes back round");
        }
    }
}
