using System.Collections.Generic;
using System.Linq;
using Neko.Sim.Cafe;
using Neko.Sim.Maids;
using Neko.Sim.Service;
using Neko.Sim.Shift;
using NUnit.Framework;

namespace Neko.Tests.Sim
{
    /// <summary>
    /// The inputs the design implies but a feature test would never reach. Every one of these
    /// is a way a player could be left stuck, which rubric 11 calls a dead end.
    /// </summary>
    public sealed class ShiftEdgeCaseTests
    {
        private static ShiftSetup Setup(CafeGrid grid, IEnumerable<Maid> roster = null) =>
            new ShiftSetup
            {
                Grid = grid,
                Roster = (roster ?? new Maid[0]).ToList(),
                DurationSeconds = 30f,
                ArrivalsPerSecond = 0f,
                Quota = 100,
                Seed = 5u,
                ScriptedArrivals = new[] { new ScriptedArrival(0.5f, CustomerType.Regular) },
            };

        [Test]
        public void AShiftWithNoTablesEndsImmediatelyWithAReasonInPortuguese()
        {
            var grid = new CafeGrid(7, 5);
            grid.Place(PlaceableCatalog.ById("stove"), 2, 2);

            var shift = new Shift(Setup(grid));

            Assert.IsTrue(shift.Finished);
            var result = shift.Finish();
            Assert.IsTrue(result.SlowDay);
            Assert.IsNotNull(result.FailureReason);
            StringAssert.Contains("mesa", result.FailureReason.ToLowerInvariant());
        }

        [Test]
        public void AShiftWithNoStationEndsImmediatelyWithAReason()
        {
            var grid = new CafeGrid(7, 5);
            grid.Place(PlaceableCatalog.ById("table_2"), 2, 2);

            var shift = new Shift(Setup(grid));
            var result = shift.Finish();

            Assert.IsTrue(shift.Finished);
            Assert.IsNotNull(result.FailureReason);
            StringAssert.Contains("cozinha", result.FailureReason.ToLowerInvariant());
        }

        [Test]
        public void AStationThatCannotCookAnythingOnTheMenuIsNoStationAtAll()
        {
            // The bakery exists but nothing it bakes is unlocked at café level 1.
            var grid = new CafeGrid(9, 7);
            grid.Place(PlaceableCatalog.ById("table_2"), 2, 2);
            grid.Place(PlaceableCatalog.ById("bakery"), 6, 2);

            var shift = new Shift(Setup(grid));

            Assert.IsTrue(shift.Finished);
            Assert.IsNotNull(shift.Finish().FailureReason);
        }

        [Test]
        public void NobodyEverOrdersADishThisCafeCannotCook()
        {
            // Café level 3 has unlocked parfaits and matcha, but this room owns only a stove.
            // Letting somebody order one would strand the ticket, run their patience out and
            // leave the player with no move that fixed it.
            var grid = new CafeGrid(9, 7);
            grid.Place(PlaceableCatalog.ById("table_2"), 3, 1);
            grid.Place(PlaceableCatalog.ById("stove"), 6, 3);

            var setup = Setup(grid, new[] { Maid.Create(1, "A", Archetype.Mukuchi, 12) });
            setup.CafeLevel = 3;
            setup.DurationSeconds = 150f;
            setup.ArrivalsPerSecond = 0.5f;
            setup.ScriptedArrivals = null;

            var shift = new Shift(setup);
            while (!shift.Finished) shift.Tick(0.05f);

            foreach (var ticket in shift.Tickets)
            {
                Assert.AreEqual(StationKind.Stove, ticket.Recipe.Station,
                    ticket.Recipe.DisplayName + " was ordered in a café with no station for it");
            }

            Assert.Greater(shift.Finish().Served, 0);
        }

        [Test]
        public void AddingAStationWidensWhatPeopleOrder()
        {
            var stoveOnly = MenuSeenAt(withTeaBar: false);
            var withTea = MenuSeenAt(withTeaBar: true);

            CollectionAssert.IsSubsetOf(stoveOnly, withTea);
            Assert.Greater(withTea.Count, stoveOnly.Count, "buying a station has to open the menu");

            HashSet<string> MenuSeenAt(bool withTeaBar)
            {
                var grid = new CafeGrid(9, 7);
                grid.Place(PlaceableCatalog.ById("table_2"), 3, 1);
                grid.Place(PlaceableCatalog.ById("stove"), 6, 3);
                if (withTeaBar) grid.Place(PlaceableCatalog.ById("teabar"), 6, 5);

                var setup = Setup(grid, new[] { Maid.Create(1, "A", Archetype.Mukuchi, 15) });
                setup.CafeLevel = 2;
                setup.DurationSeconds = 150f;
                setup.ArrivalsPerSecond = 0.6f;
                setup.ScriptedArrivals = null;

                var shift = new Shift(setup);
                var seen = new HashSet<string>();
                while (!shift.Finished)
                {
                    shift.Tick(0.05f);
                    foreach (var ticket in shift.Tickets) seen.Add(ticket.Recipe.Id);
                }

                return seen;
            }
        }

        [Test]
        public void AnImpossibleShiftAwardsNoReputationEitherWay()
        {
            var grid = new CafeGrid(7, 5);
            grid.Place(PlaceableCatalog.ById("stove"), 2, 2);

            Assert.AreEqual(0, new Shift(Setup(grid)).Finish().ReputationDelta);
        }

        [Test]
        public void AMaidWhoseMoodEmptiesFinishesHerTaskAndThenRests()
        {
            var maid = Maid.Create(1, "Cansada", Archetype.Genki);
            maid.Mood = 0.001f;

            var shift = new Shift(Setup(PlayableRoom(), new[] { maid }));
            var worker = shift.Workers.First(w => !w.IsManager);

            var spent = 0f;
            while (!worker.OnBreak && spent < 60f && !shift.Finished)
            {
                shift.Tick(0.05f);
                spent += 0.05f;
            }

            Assert.IsTrue(worker.OnBreak, "a maid at zero mood has to rest, not grind on forever");
            Assert.IsNull(worker.Claimed);
        }

        [Test]
        public void NoTaskStaysClaimedByAWorkerOnBreak()
        {
            var maid = Maid.Create(1, "Cansada", Archetype.Genki);
            maid.Mood = 0.001f;

            var shift = new Shift(Setup(PlayableRoom(), new[] { maid }));
            var worker = shift.Workers.First(w => !w.IsManager);

            for (var i = 0; i < 1200 && !shift.Finished; i++)
            {
                shift.Tick(0.05f);
                if (!worker.OnBreak) continue;

                Assert.IsTrue(shift.OpenTasks.All(t => t.ClaimedBy != worker.Id),
                    "a task held by somebody in the break room can never be done");
            }
        }

        [Test]
        public void AMaidComesBackFromTheBreakRoomAndNeverQuits()
        {
            var maid = Maid.Create(1, "Cansada", Archetype.Genki);
            maid.Mood = 0.001f;

            var setup = Setup(PlayableRoom(), new[] { maid });
            setup.DurationSeconds = 200f;
            setup.ArrivalsPerSecond = 0.3f;
            setup.ScriptedArrivals = null;

            var shift = new Shift(setup);
            var worker = shift.Workers.First(w => !w.IsManager);
            var rested = false;

            for (var i = 0; i < 6000 && !shift.Finished; i++)
            {
                shift.Tick(0.05f);
                if (!worker.OnBreak && maid.Mood >= Worker.MoodAfterBreak) rested = true;
            }

            Assert.IsTrue(rested, "the break room has to give her something back");
            CollectionAssert.Contains(shift.Events.Select(e => e.Kind).ToList(),
                ShiftEventKind.BackFromBreak);
        }

        [Test]
        public void ARosterOfOjousamasStillClearsTablesBecauseTheManagerCan()
        {
            var setup = Setup(PlayableRoom(), new[]
            {
                Maid.Create(1, "A", Archetype.Ojousama),
                Maid.Create(2, "B", Archetype.Ojousama),
            });
            setup.DurationSeconds = 120f;

            var shift = new Shift(setup);

            for (var i = 0; i < 4000 && !shift.Finished; i++)
            {
                var bus = shift.OpenTasks.FirstOrDefault(t => t.Kind == TaskKind.Bus && !t.Claimed);
                if (bus != null) shift.CommandManager(TaskKind.Bus, bus.TargetId);
                shift.Tick(0.05f);
            }

            Assert.IsTrue(shift.Finished);
            Assert.IsFalse(shift.Seats.Any(s => s.Dirty),
                "the player always has a way to clear a table, whoever is on shift");
        }

        [Test]
        public void ARosterOfOjousamasNeverClaimsABusTaskOnItsOwn()
        {
            var setup = Setup(PlayableRoom(), new[] { Maid.Create(1, "A", Archetype.Ojousama) });
            setup.DurationSeconds = 120f;
            var shift = new Shift(setup);
            var worker = shift.Workers.First(w => !w.IsManager);

            for (var i = 0; i < 4000 && !shift.Finished; i++)
            {
                shift.Tick(0.05f);
                Assert.IsFalse(
                    worker.Claimed != null && worker.Claimed.Kind == TaskKind.Bus,
                    "her refusal has to be real or the archetype is paint");
            }
        }

        [Test]
        public void CommandingTheManagerToSomethingThatDoesNotExistIsRefusedNotThrown()
        {
            var shift = new Shift(Setup(PlayableRoom()));

            Assert.IsFalse(shift.CommandManager(TaskKind.Bus, 9999));
            Assert.IsFalse(shift.CommandManager(TaskKind.Cook, -1));
        }

        [Test]
        public void RushingAWorkerThatDoesNotExistIsRefusedNotThrown()
        {
            var shift = new Shift(Setup(PlayableRoom()));

            Assert.IsFalse(shift.Rush(9999));
            Assert.IsFalse(shift.UseEncore(9999));
        }

        [Test]
        public void ATickOfZeroOrNegativeTimeChangesNothing()
        {
            var shift = new Shift(Setup(PlayableRoom(), new[] { Maid.Create(1, "A", Archetype.Genki) }));
            var before = shift.TimeRemaining;

            shift.Tick(0f);
            shift.Tick(-5f);

            Assert.AreEqual(before, shift.TimeRemaining);
        }

        [Test]
        public void AVeryLargeTickDoesNotBreakTheDay()
        {
            var shift = new Shift(Setup(PlayableRoom(), new[] { Maid.Create(1, "A", Archetype.Genki) }));

            Assert.DoesNotThrow(() => shift.Tick(500f));
            Assert.IsTrue(shift.Finished);
        }

        [Test]
        public void AShiftWithOneSeatAndManyArrivalsNeverDeadlocks()
        {
            var grid = new CafeGrid(7, 5);
            grid.Place(PlaceableCatalog.ById("stool"), 3, 2);
            grid.Place(PlaceableCatalog.ById("stove"), 5, 2);

            var setup = Setup(grid, new[]
            {
                Maid.Create(1, "A", Archetype.Genki, 10),
                Maid.Create(2, "B", Archetype.Mukuchi, 10),
            });
            setup.DurationSeconds = 120f;
            setup.ArrivalsPerSecond = 0.8f;
            setup.ScriptedArrivals = null;

            var shift = new Shift(setup);
            var spent = 0f;
            while (!shift.Finished && spent < 400f)
            {
                shift.Tick(0.05f);
                spent += 0.05f;
            }

            Assert.IsTrue(shift.Finished, "one seat and a crowd must still reach the end of the day");
            Assert.Greater(shift.Finish().Served, 0);
        }

        [Test]
        public void EveryEventCarriesAPositionInsideTheRoom()
        {
            var shift = new Shift(Setup(PlayableRoom(), new[]
            {
                Maid.Create(1, "A", Archetype.Genki, 10),
                Maid.Create(2, "B", Archetype.Mukuchi, 10),
            }));

            while (!shift.Finished) shift.Tick(0.05f);

            foreach (var e in shift.Events)
            {
                Assert.That(e.X, Is.InRange(-1f, shift.Grid.Width + 1f), e.Kind.ToString());
                Assert.That(e.Y, Is.InRange(-1f, shift.Grid.Height + 1f), e.Kind.ToString());
            }
        }

        private static CafeGrid PlayableRoom()
        {
            var grid = new CafeGrid(7, 5);
            grid.Place(PlaceableCatalog.ById("table_2"), 3, 1);
            grid.Place(PlaceableCatalog.ById("table_2"), 3, 3);
            grid.Place(PlaceableCatalog.ById("stove"), 5, 2);
            return grid;
        }
    }
}
