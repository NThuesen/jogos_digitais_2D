using Neko.Sim.Cafe;
using Neko.Sim.Core;
using NUnit.Framework;

namespace Neko.Tests.Sim
{
    public sealed class CafeMetricsTests
    {
        private static PlaceableDef Def(string id) => PlaceableCatalog.ById(id);

        [Test]
        public void AnEmptyRoomHasNoAmbienceNoSeatsAndNoFlow()
        {
            var metrics = new CafeGrid(7, 5).Metrics();

            Assert.AreEqual(0f, metrics.Ambience);
            Assert.AreEqual(0, metrics.Seats);
            Assert.AreEqual(0f, metrics.Flow, "flow of a room with no seats must be defined, not a divide by zero");
            Assert.AreEqual(1f, metrics.PatienceMultiplier);
            Assert.AreEqual(1f, metrics.TipMultiplier);
        }

        [Test]
        public void SeatsCountEverySeatOfEveryTable()
        {
            var grid = new CafeGrid(9, 7);
            grid.Place(Def("table_4"), 2, 2);
            grid.Place(Def("table_2"), 5, 2);
            grid.Place(Def("stool"), 7, 5);

            Assert.AreEqual(7, grid.Metrics().Seats);
        }

        [Test]
        public void AmbienceIsScaledByRoomSizeSoASmallRoomIsNotTriviallyCosy()
        {
            var small = new CafeGrid(7, 5);
            var large = new CafeGrid(13, 9);
            small.Place(Def("lantern_sakura"), 1, 1);
            large.Place(Def("lantern_sakura"), 1, 1);

            Assert.Greater(small.Metrics().Ambience, large.Metrics().Ambience,
                "one lantern must do less for a hall than for a parlour");
        }

        [Test]
        public void MoreDecorRaisesAmbience()
        {
            var grid = new CafeGrid(9, 7);
            grid.Place(Def("plant"), 1, 1);
            var one = grid.Metrics().Ambience;

            grid.Place(Def("plant"), 3, 1);

            Assert.Greater(grid.Metrics().Ambience, one);
        }

        [Test]
        public void FlowFallsWhenAStationMovesNearTheTables()
        {
            var far = new CafeGrid(13, 9);
            far.Place(Def("table_2"), 1, 1);
            far.Place(Def("stove"), 12, 8);

            var near = new CafeGrid(13, 9);
            near.Place(Def("table_2"), 1, 1);
            near.Place(Def("stove"), 3, 1);

            Assert.Greater(far.Metrics().Flow, near.Metrics().Flow,
                "layout has to have teeth or the editor is a dress-up screen");
        }

        [Test]
        public void FlowCountsTheWalkFromTheDoorToo()
        {
            var nearDoor = new CafeGrid(13, 9);
            nearDoor.Place(Def("table_2"), 1, 4);
            nearDoor.Place(Def("stove"), 3, 4);

            var farFromDoor = new CafeGrid(13, 9);
            farFromDoor.Place(Def("table_2"), 10, 4);
            farFromDoor.Place(Def("stove"), 12, 4);

            Assert.Greater(farFromDoor.Metrics().Flow, nearDoor.Metrics().Flow);
        }

        [Test]
        public void FlowIsDefinedWhenThereIsNoStation()
        {
            var grid = new CafeGrid(7, 5);
            grid.Place(Def("table_2"), 2, 2);

            Assert.Greater(grid.Metrics().Flow, 0f);
            Assert.IsFalse(float.IsNaN(grid.Metrics().Flow));
            Assert.IsFalse(float.IsInfinity(grid.Metrics().Flow));
        }

        [Test]
        public void ThemeBonusNeedsSeventyPercentOfTheDecor()
        {
            var two = new CafeGrid(13, 9);
            two.Place(Def("lantern_sakura"), 1, 1);
            two.Place(Def("lantern_sakura"), 3, 1);
            two.Place(Def("plant"), 5, 1);

            Assert.IsFalse(two.Metrics().ThemeBonusActive, "two in three is 67%, which is not enough");

            var seven = new CafeGrid(13, 9);
            for (var i = 0; i < 7; i++) seven.Place(Def("lantern_sakura"), i, 1);
            for (var i = 0; i < 3; i++) seven.Place(Def("plant"), i, 3);

            Assert.IsTrue(seven.Metrics().ThemeBonusActive, "seven in ten is 70%, which is");
            Assert.AreEqual(ThemeTag.Sakura, seven.Metrics().DominantTheme);
        }

        [Test]
        public void ARoomOfUnthemedDecorHasNoDominantTheme()
        {
            var grid = new CafeGrid(13, 9);
            for (var i = 0; i < 5; i++) grid.Place(Def("plant"), i, 1);

            Assert.AreEqual(ThemeTag.None, grid.Metrics().DominantTheme);
            Assert.IsFalse(grid.Metrics().ThemeBonusActive);
        }

        [Test]
        public void TwoDecorPiecesAreTooFewToCountAsATheme()
        {
            var grid = new CafeGrid(13, 9);
            grid.Place(Def("lantern_sakura"), 1, 1);
            grid.Place(Def("lantern_sakura"), 3, 1);

            Assert.IsFalse(grid.Metrics().ThemeBonusActive,
                "a corner with two lanterns is not a themed café");
        }

        [Test]
        public void PatienceAndTipMultipliersRiseWithAmbience()
        {
            var bare = new CafeGrid(9, 7);
            bare.Place(Def("table_2"), 1, 1);

            var cosy = new CafeGrid(9, 7);
            cosy.Place(Def("table_2"), 1, 1);
            for (var i = 0; i < 4; i++) cosy.Place(Def("plant"), i, 3);

            Assert.Greater(cosy.Metrics().PatienceMultiplier, bare.Metrics().PatienceMultiplier);
            Assert.Greater(cosy.Metrics().TipMultiplier, bare.Metrics().TipMultiplier);
        }

        [Test]
        public void TheMultipliersAreClampedBetweenOneAndTwo()
        {
            var grid = new CafeGrid(7, 5);
            for (var x = 0; x < 7; x++)
            {
                for (var y = 0; y < 5; y++) grid.Place(Def("planetarium"), x, y);
            }

            var metrics = grid.Metrics();

            Assert.That(metrics.PatienceMultiplier, Is.InRange(1f, 2f));
            Assert.That(metrics.TipMultiplier, Is.InRange(1f, 2f));
        }

        [Test]
        public void TheThemeBonusIsWorthSomethingOnTipsOnItsOwn()
        {
            // Both rooms hold ten pieces worth 6 ambience each, so the only difference
            // between them is that one of them commits to a single theme.
            var mixed = new CafeGrid(13, 9);
            for (var i = 0; i < 6; i++) mixed.Place(Def("lantern_sakura"), i, 1);
            for (var i = 0; i < 4; i++) mixed.Place(Def("gothic_candelabra"), i, 3);

            var themed = new CafeGrid(13, 9);
            for (var i = 0; i < 10; i++) themed.Place(Def("lantern_sakura"), i, 1);

            Assert.That(themed.Metrics().Ambience, Is.EqualTo(mixed.Metrics().Ambience).Within(1e-4f));
            Assert.IsFalse(mixed.Metrics().ThemeBonusActive, "six in ten is 60%");
            Assert.IsTrue(themed.Metrics().ThemeBonusActive);
            Assert.Greater(themed.Metrics().TipMultiplier, mixed.Metrics().TipMultiplier);
        }
    }
}
