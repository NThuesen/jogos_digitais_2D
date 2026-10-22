using System.Linq;
using Neko.Sim.Cafe;
using NUnit.Framework;

namespace Neko.Tests.Sim
{
    public sealed class CafeGridTests
    {
        private static CafeGrid Room(int width = 7, int height = 5) => new CafeGrid(width, height);

        private static PlaceableDef Def(string id) => PlaceableCatalog.ById(id);

        [Test]
        public void ANewRoomIsEmptyExceptForItsDoor()
        {
            var grid = Room();

            Assert.IsEmpty(grid.Items);
            Assert.AreEqual(0, grid.Door.X);
            Assert.AreEqual(grid.Height / 2, grid.Door.Y);
        }

        [Test]
        public void PlacingInsideTheRoomWorks()
        {
            var grid = Room();

            Assert.IsTrue(grid.Place(Def("table_2"), 3, 2));
            Assert.AreEqual(1, grid.Items.Count);
        }

        [Test]
        public void PlacingOutsideTheGridIsRefused()
        {
            var grid = Room();

            Assert.IsFalse(grid.Place(Def("table_2"), -1, 0));
            Assert.IsFalse(grid.Place(Def("table_2"), 6, 0), "a 2-wide table must not hang off the edge");
            Assert.IsFalse(grid.Place(Def("table_2"), 0, 5));
            Assert.IsEmpty(grid.Items);
        }

        [Test]
        public void OverlappingPlacementIsRefused()
        {
            var grid = Room();
            grid.Place(Def("table_2"), 2, 2);

            Assert.IsFalse(grid.Place(Def("stove"), 2, 2));
            Assert.IsFalse(grid.Place(Def("stove"), 3, 2), "the second cell of a 2-wide table is taken too");
            Assert.IsTrue(grid.Place(Def("stove"), 4, 2));
        }

        [Test]
        public void NothingMayBePlacedOnTheDoor()
        {
            var grid = Room();

            Assert.IsFalse(grid.Place(Def("stove"), grid.Door.X, grid.Door.Y));
        }

        [Test]
        public void RotatingSwapsTheFootprint()
        {
            var grid = Room();

            Assert.IsFalse(grid.Place(Def("table_2"), 6, 2), "2x1 does not fit against the right wall");
            Assert.IsTrue(grid.Place(Def("table_2"), 6, 2, rotated: true), "1x2 does");
        }

        [Test]
        public void RemovingFreesTheWholeFootprint()
        {
            var grid = Room();
            grid.Place(Def("table_2"), 2, 2);

            Assert.IsTrue(grid.Remove(3, 2), "removing from either cell of the footprint works");
            Assert.IsEmpty(grid.Items);
            Assert.IsTrue(grid.Place(Def("stove"), 2, 2));
        }

        [Test]
        public void RemovingEmptyFloorIsFalseNotAnError()
        {
            Assert.IsFalse(Room().Remove(4, 4));
        }

        [Test]
        public void ItemAtFindsTheItemFromAnyCellOfItsFootprint()
        {
            var grid = Room();
            grid.Place(Def("table_4"), 2, 2);

            Assert.IsNotNull(grid.ItemAt(2, 2));
            Assert.IsNotNull(grid.ItemAt(3, 3));
            Assert.IsNull(grid.ItemAt(5, 1));
        }

        [Test]
        public void ResizeKeepsItemsThatStillFitAndDropsTheRest()
        {
            var grid = Room(9, 7);
            grid.Place(Def("stove"), 1, 1);
            grid.Place(Def("stove"), 8, 6);

            grid.Resize(7, 5);

            Assert.AreEqual(1, grid.Items.Count);
            Assert.IsNotNull(grid.ItemAt(1, 1));
        }

        [Test]
        public void ResizeMovesTheDoorToTheNewWall()
        {
            var grid = Room(7, 5);
            grid.Resize(13, 9);

            Assert.AreEqual(4, grid.Door.Y);
        }

        [Test]
        public void SeatPositionsGiveOneCellPerSeat()
        {
            var grid = Room();
            grid.Place(Def("table_2"), 2, 2);
            grid.Place(Def("stool"), 5, 1);

            Assert.AreEqual(3, grid.SeatPositions().Count);
        }

        [Test]
        public void StationsReportWhatTheyCook()
        {
            var grid = Room();
            grid.Place(Def("stove"), 1, 1);

            var stations = grid.Stations();

            Assert.AreEqual(1, stations.Count);
            Assert.AreEqual(StationKind.Stove, stations[0].Kind);
        }

        [Test]
        public void TheCatalogIsConsistent()
        {
            foreach (var def in PlaceableCatalog.All)
            {
                Assert.IsNotEmpty(def.DisplayName, def.Id);
                Assert.Greater(def.Width, 0, def.Id);
                Assert.Greater(def.Height, 0, def.Id);
                Assert.GreaterOrEqual(def.Price, 0, def.Id);
                Assert.GreaterOrEqual(def.UnlockCafeLevel, 1, def.Id);

                if (def.Kind == PlaceableKind.Table) Assert.Greater(def.Seats, 0, def.Id);
                if (def.Kind == PlaceableKind.Station) Assert.IsTrue(def.Station.HasValue, def.Id);
                if (def.Kind == PlaceableKind.Decor) Assert.Greater(def.Ambience, 0f, def.Id);
            }
        }

        [Test]
        public void EveryCatalogIdIsUnique()
        {
            CollectionAssert.AllItemsAreUnique(PlaceableCatalog.All.Select(d => d.Id));
        }

        [Test]
        public void EveryStationKindIsBuildable()
        {
            foreach (StationKind kind in System.Enum.GetValues(typeof(StationKind)))
            {
                Assert.IsTrue(
                    PlaceableCatalog.All.Any(d => d.Station == kind),
                    kind + " has no placeable, so its recipes could never be cooked");
            }
        }

        [Test]
        public void UnlockedAtGrowsWithCafeLevelAndNeverShrinks()
        {
            var previous = 0;

            for (var level = 1; level <= 10; level++)
            {
                var count = PlaceableCatalog.UnlockedAt(level).Count;
                Assert.GreaterOrEqual(count, previous, "level " + level);
                previous = count;
            }

            Assert.Greater(PlaceableCatalog.UnlockedAt(10).Count, PlaceableCatalog.UnlockedAt(1).Count);
        }

        [Test]
        public void ByIdRefusesAnUnknownId()
        {
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(
                () => PlaceableCatalog.ById("nao_existe"));
        }

        [Test]
        public void ADefaultRoomCanAlreadyServeSomebody()
        {
            var grid = CafeGrid.StartingRoom();

            Assert.Greater(grid.Metrics().Seats, 0, "day one must be playable out of the box");
            Assert.Greater(grid.Metrics().StationCount, 0);
        }
    }
}
