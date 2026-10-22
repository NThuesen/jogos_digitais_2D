using System.Collections.Generic;
using System.Linq;
using Neko.Sim.Cafe;
using Neko.Sim.Maids;
using Neko.Sim.Service;
using Neko.Sim.Shift;
using NUnit.Framework;

namespace Neko.Tests.Sim
{
    public sealed class ShiftTests
    {
        private static CafeGrid Room()
        {
            var grid = new CafeGrid(7, 5);
            grid.Place(PlaceableCatalog.ById("table_2"), 3, 1);
            grid.Place(PlaceableCatalog.ById("table_2"), 3, 3);
            grid.Place(PlaceableCatalog.ById("stove"), 5, 2);
            return grid;
        }

        private static ShiftSetup Setup(
            IEnumerable<Maid> roster = null,
            IEnumerable<ScriptedArrival> arrivals = null,
            CafeGrid grid = null,
            float duration = 90f,
            int quota = 120) =>
            new ShiftSetup
            {
                Day = 1,
                CafeLevel = 1,
                Grid = grid ?? Room(),
                Roster = (roster ?? new Maid[0]).ToList(),
                DurationSeconds = duration,
                ArrivalsPerSecond = 0f,
                Quota = quota,
                Seed = 1234u,
                ScriptedArrivals = (arrivals ?? new[] { new ScriptedArrival(1f, CustomerType.Regular) })
                    .ToList(),
            };

        private static ShiftResult RunToEnd(Shift shift, float dt = 0.05f, float maxSeconds = 600f)
        {
            var spent = 0f;
            while (!shift.Finished && spent < maxSeconds)
            {
                shift.Tick(dt);
                spent += dt;
            }

            return shift.Finish();
        }

        private static List<Maid> Roster(params Archetype[] kinds)
        {
            var roster = new List<Maid>();
            for (var i = 0; i < kinds.Length; i++)
            {
                roster.Add(Maid.Create(i + 1, kinds[i].ToString(), kinds[i]));
            }

            return roster;
        }

        [Test]
        public void ASingleCustomerIsServedEndToEnd()
        {
            var shift = new Shift(Setup(Roster(Archetype.Genki, Archetype.Mukuchi)));

            var result = RunToEnd(shift);

            Assert.AreEqual(1, result.Served, "the one scripted customer must be served");
            Assert.AreEqual(0, result.Angry);
            Assert.Greater(result.Earned, 0);
        }

        [Test]
        public void TheWholeLoopIsVisibleInTheEventStream()
        {
            var shift = new Shift(Setup(Roster(Archetype.Genki, Archetype.Mukuchi)));
            RunToEnd(shift);

            var kinds = shift.Events.Select(e => e.Kind).ToList();

            foreach (var expected in new[]
                     {
                         ShiftEventKind.CustomerArrived, ShiftEventKind.Seated, ShiftEventKind.Ordered,
                         ShiftEventKind.TicketQueued, ShiftEventKind.CookStarted,
                         ShiftEventKind.PlateReady, ShiftEventKind.Served, ShiftEventKind.Paid,
                         ShiftEventKind.Bussed, ShiftEventKind.ShiftEnded,
                     })
            {
                CollectionAssert.Contains(kinds, expected);
            }
        }

        [Test]
        public void TheSameSeedProducesTheSameResult()
        {
            var first = RunToEnd(new Shift(Rush()));
            var second = RunToEnd(new Shift(Rush()));

            Assert.AreEqual(first.Earned, second.Earned);
            Assert.AreEqual(first.Served, second.Served);
            Assert.AreEqual(first.Angry, second.Angry);
            Assert.AreEqual(first.TurnedAway, second.TurnedAway);

            ShiftSetup Rush() => new ShiftSetup
            {
                Grid = Room(),
                Roster = Roster(Archetype.Genki, Archetype.Mukuchi),
                DurationSeconds = 60f,
                ArrivalsPerSecond = 0.5f,
                Quota = 300,
                Seed = 99u,
                CafeLevel = 2,
            };
        }

        [Test]
        public void AnAbandonedCustomerLeavesAngryAndTheTableStillNeedsBussing()
        {
            // Nobody on shift and no commands given: the manager never moves on his own.
            var setup = Setup(arrivals: new[] { new ScriptedArrival(0.5f, CustomerType.Vip) });
            setup.DurationSeconds = 120f;
            var shift = new Shift(setup);

            // Seat the customer by hand, then walk away from it.
            Advance(shift, 1f);
            shift.CommandManager(TaskKind.Greet, 1);
            Advance(shift, 8f);
            Assert.GreaterOrEqual(shift.CustomerById(1).SeatIndex, 0,
                "precondition: the customer got a seat");

            Advance(shift, 60f);

            Assert.IsTrue(shift.Seats.Any(s => s.Dirty), "an angry customer still leaves a mess");
            Assert.AreEqual(1, shift.Finish().Angry);
        }

        [Test]
        public void AShiftWithNoMaidsIsPlayableByTheManagerAlone()
        {
            var shift = new Shift(Setup());

            var spent = 0f;
            while (!shift.Finished && spent < 300f)
            {
                DriveTheManager(shift);
                shift.Tick(0.05f);
                spent += 0.05f;
            }

            var result = shift.Finish();

            Assert.AreEqual(1, result.Served, "a new game starts with nobody hired; it must be winnable");
            Assert.IsNull(result.FailureReason);
        }

        [Test]
        public void BetterMaidsServeMoreInTheSameTime()
        {
            var novice = RunToEnd(new Shift(Busy(1)));
            var veteran = RunToEnd(new Shift(Busy(18)));

            Assert.Greater(veteran.Served, novice.Served,
                "if levelling did not show up here the upgrade fantasy is a lie");

            ShiftSetup Busy(int level) => new ShiftSetup
            {
                Grid = Room(),
                Roster = new[]
                {
                    Maid.Create(1, "A", Archetype.Genki, level),
                    Maid.Create(2, "B", Archetype.Mukuchi, level),
                },
                DurationSeconds = 90f,
                ArrivalsPerSecond = 0.6f,
                Quota = 400,
                Seed = 7u,
                CafeLevel = 2,
            };
        }

        [Test]
        public void AmbienceRaisesWhatACustomerPays()
        {
            var plainRoom = Room();
            var cosyRoom = Room();
            for (var i = 0; i < 4; i++) cosyRoom.Place(PlaceableCatalog.ById("plant"), i, 0);

            var plain = RunToEnd(new Shift(Setup(Roster(Archetype.Genki, Archetype.Mukuchi), grid: plainRoom)));
            var cosy = RunToEnd(new Shift(Setup(Roster(Archetype.Genki, Archetype.Mukuchi), grid: cosyRoom)));

            Assert.AreEqual(plain.Served, cosy.Served, "precondition: both served the same customer");
            Assert.Greater(cosy.Earned, plain.Earned);
        }

        [Test]
        public void AFullQueueTurnsArrivalsAway()
        {
            var setup = Setup(arrivals: Enumerable.Range(0, 10)
                .Select(i => new ScriptedArrival(0.1f * i, CustomerType.Student)));
            setup.DoorCapacity = 2;
            setup.DurationSeconds = 20f;

            var shift = new Shift(setup);
            Advance(shift, 2f);

            Assert.Greater(shift.TurnedAway, 0, "a door with room for two cannot hold ten");
        }

        [Test]
        public void RushSpeedsAMaidUpAndCostsHerMood()
        {
            var maid = Maid.Create(1, "A", Archetype.Genki);
            var shift = new Shift(Setup(new[] { maid }));
            var worker = shift.Workers.First(w => !w.IsManager);
            var moodBefore = maid.Mood;

            Assert.IsTrue(shift.Rush(worker.Id));

            Assert.Greater(worker.RushRemaining, 0f);
            Assert.Less(maid.Mood, moodBefore);
        }

        [Test]
        public void TheManagerCannotBeRushed()
        {
            var shift = new Shift(Setup());

            Assert.IsFalse(shift.Rush(shift.Manager.Id));
        }

        [Test]
        public void OnlyTheIdolHasAnEncoreAndItGoesOnCooldown()
        {
            var shift = new Shift(Setup(Roster(Archetype.Idol, Archetype.Genki)));
            var idol = shift.Workers.First(w => !w.IsManager && w.Maid.Archetype == Archetype.Idol);
            var genki = shift.Workers.First(w => !w.IsManager && w.Maid.Archetype == Archetype.Genki);

            Assert.IsFalse(shift.UseEncore(genki.Id));
            Assert.IsTrue(shift.UseEncore(idol.Id));
            Assert.IsFalse(shift.UseEncore(idol.Id), "it has to cost something to be a decision");
        }

        [Test]
        public void EncoreGivesTheRoomItsPatienceBack()
        {
            var setup = Setup(Roster(Archetype.Idol), new[] { new ScriptedArrival(0.5f, CustomerType.Vip) });
            setup.DurationSeconds = 120f;
            var shift = new Shift(setup);

            Advance(shift, 1f);
            shift.CommandManager(TaskKind.Greet, 1);
            Advance(shift, 12f);

            var customer = shift.CustomerById(1);
            Assert.Less(customer.Patience, 0.7f, "precondition: they have been waiting a while");

            var idol = shift.Workers.First(w => !w.IsManager);
            shift.UseEncore(idol.Id);

            Assert.That(customer.Patience, Is.GreaterThanOrEqualTo(0.7f));
        }

        [Test]
        public void ClosingTimeStopsArrivalsAndSendsTheQueueHome()
        {
            var setup = Setup(arrivals: Enumerable.Range(0, 6)
                .Select(i => new ScriptedArrival(0.2f * i, CustomerType.Student)));
            setup.DurationSeconds = 2f;

            var shift = new Shift(setup);
            Advance(shift, 3f);

            Assert.IsTrue(shift.ClosingTime);
            Assert.IsFalse(shift.Customers.Any(c => c.State == CustomerState.Queueing));
        }

        [Test]
        public void TheShiftEndsOnceTheLastCustomerIsGone()
        {
            var shift = new Shift(Setup(Roster(Archetype.Genki, Archetype.Mukuchi), duration: 30f));

            RunToEnd(shift);

            Assert.IsTrue(shift.Finished);
            Assert.IsEmpty(shift.Customers);
        }

        [Test]
        public void TheGracePeriodEndsAShiftThatWouldOtherwiseHang()
        {
            // One VIP, nobody to serve them, and a door that closes almost immediately.
            var setup = Setup(arrivals: new[] { new ScriptedArrival(0.1f, CustomerType.Cat) });
            setup.DurationSeconds = 1f;
            setup.ClosingGraceSeconds = 5f;
            var shift = new Shift(setup);

            // The cat is never impatient, so only the grace period can end this.
            var spent = 0f;
            while (!shift.Finished && spent < 60f)
            {
                shift.Tick(0.1f);
                spent += 0.1f;
            }

            Assert.IsTrue(shift.Finished, "a customer who never gets impatient must not hang the day");
            Assert.Less(spent, 30f);
        }

        [Test]
        public void MeetingTheQuotaIsNotASlowDay()
        {
            var shift = new Shift(Setup(Roster(Archetype.Genki, Archetype.Mukuchi), quota: 1));

            var result = RunToEnd(shift);

            Assert.IsTrue(result.QuotaMet);
            Assert.IsFalse(result.SlowDay);
        }

        [Test]
        public void MissingTheQuotaIsASlowDayAndNotAGameOver()
        {
            var shift = new Shift(Setup(Roster(Archetype.Genki), quota: 100000));

            var result = RunToEnd(shift);

            Assert.IsFalse(result.QuotaMet);
            Assert.IsTrue(result.SlowDay);
            Assert.IsNull(result.FailureReason, "a hard day is not a broken café");
        }

        [Test]
        public void ServingEarnsTheMaidsXp()
        {
            var shift = new Shift(Setup(Roster(Archetype.Genki, Archetype.Mukuchi)));

            var result = RunToEnd(shift);

            Assert.IsNotEmpty(result.XpByMaid);
            Assert.IsTrue(result.XpByMaid.Values.All(xp => xp > 0));
        }

        [Test]
        public void TheManagerEarnsNoXpBecauseHeIsAlreadyTheCeiling()
        {
            var shift = new Shift(Setup());
            var spent = 0f;
            while (!shift.Finished && spent < 300f)
            {
                DriveTheManager(shift);
                shift.Tick(0.05f);
                spent += 0.05f;
            }

            Assert.IsFalse(shift.Finish().XpByMaid.ContainsKey(0));
        }

        [Test]
        public void OneesanGivesPartOfHerXpToTheWeakestMaid()
        {
            var onee = Maid.Create(1, "Onee", Archetype.Oneesan, 10);
            var rookie = Maid.Create(2, "Rookie", Archetype.Mukuchi);
            var shift = new Shift(Setup(new[] { onee, rookie }));

            var result = RunToEnd(shift);

            Assert.IsTrue(result.XpByMaid.ContainsKey(rookie.Id));
            Assert.Greater(result.XpByMaid[rookie.Id], 0);
        }

        [Test]
        public void TheOtakuPaysFarMoreWhenHisMaidIsOnShift()
        {
            var without = RunToEnd(new Shift(Fan(Archetype.Kuudere)));
            var with = RunToEnd(new Shift(Fan(Archetype.Genki)));

            Assert.AreEqual(1, without.Served);
            Assert.AreEqual(1, with.Served);
            Assert.Greater(with.Earned, without.Earned * 1.5f);

            ShiftSetup Fan(Archetype rostered)
            {
                // Seed chosen so the otaku asks for a Genki; the rostered maid is what varies.
                var setup = Setup(
                    new[] { Maid.Create(1, "A", rostered, 12), Maid.Create(2, "B", Archetype.Mukuchi, 12) },
                    new[] { new ScriptedArrival(0.5f, CustomerType.Otaku) });
                setup.CafeLevel = 3;
                setup.Seed = 3u;
                return setup;
            }
        }

        [Test]
        public void ADroppedPlateCheersTheWholeRoomUp()
        {
            // Dojikko drops one in eight; over a busy shift at a fixed seed, at least one lands.
            var setup = new ShiftSetup
            {
                Grid = Room(),
                Roster = Roster(Archetype.Dojikko, Archetype.Dojikko),
                DurationSeconds = 120f,
                ArrivalsPerSecond = 0.5f,
                Quota = 500,
                Seed = 21u,
                CafeLevel = 2,
            };

            var shift = new Shift(setup);
            RunToEnd(shift);

            CollectionAssert.Contains(shift.Events.Select(e => e.Kind).ToList(),
                ShiftEventKind.PlateDropped);
        }

        [Test]
        public void NothingStaysClaimedAfterTheShiftEnds()
        {
            var shift = new Shift(Setup(Roster(Archetype.Genki, Archetype.Mukuchi)));
            RunToEnd(shift);

            Assert.IsTrue(shift.OpenTasks.All(t => !t.Claimed));
            Assert.IsTrue(shift.Workers.All(w => w.Claimed == null));
        }

        [Test]
        public void FinishIsIdempotent()
        {
            var shift = new Shift(Setup(Roster(Archetype.Genki)));
            RunToEnd(shift);

            Assert.AreSame(shift.Finish(), shift.Finish());
        }

        [Test]
        public void TickingAFinishedShiftDoesNothing()
        {
            var shift = new Shift(Setup(Roster(Archetype.Genki)));
            var result = RunToEnd(shift);
            var earned = result.Earned;

            for (var i = 0; i < 100; i++) shift.Tick(0.1f);

            Assert.AreEqual(earned, shift.Finish().Earned);
        }

        // Advances the shift by a number of seconds in small steps.
        private static void Advance(Shift shift, float seconds, float dt = 0.05f)
        {
            var spent = 0f;
            while (spent < seconds && !shift.Finished)
            {
                shift.Tick(dt);
                spent += dt;
            }
        }

        // Stands in for a player clicking: hands the manager whatever is most urgent.
        private static void DriveTheManager(Shift shift)
        {
            if (shift.Manager.Claimed != null) return;

            var best = shift.OpenTasks
                .Where(t => !t.Claimed)
                .OrderByDescending(t => shift.UrgencyOf(t))
                .FirstOrDefault();

            if (best != null) shift.CommandManager(best.Kind, best.TargetId);
        }
    }
}
