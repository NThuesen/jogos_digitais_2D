using System.Collections.Generic;
using System.Linq;
using Neko.Sim.Cafe;
using Neko.Sim.Economy;
using Neko.Sim.Save;
using Neko.Sim.Core;
using Neko.Sim.Maids;
using Neko.Sim.Service;
using Neko.Sim.Shift;
using Neko.Sim.Team;
using NUnit.Framework;

namespace Neko.Tests.Sim
{
    /// <summary>
    /// The owner's hands on the floor: helping never changes a day nobody touches, and a
    /// helpful tap is always at least as good as leaving the team alone.
    /// </summary>
    public sealed class PlayerAssistTests
    {
        private const float Step = 0.1f;

        private static CafeGrid Room()
        {
            var grid = new CafeGrid(9, 7);
            grid.Place(PlaceableCatalog.ById("table_2"), 3, 1);
            grid.Place(PlaceableCatalog.ById("table_2"), 3, 4);
            grid.Place(PlaceableCatalog.ById("stove"), 7, 3);
            return grid;
        }

        private static Maid Card(int id, Archetype archetype)
        {
            var maid = Maid.Create(id, archetype.ToString(), archetype, 8);
            maid.House = House.Hanabi;
            maid.Origin = ThemeTag.Sakura;
            return maid;
        }

        private static Shift NewShift(uint seed, float arrivals = 0.35f)
        {
            var roster = new List<Maid>
            {
                Card(1, Archetype.Ojousama), Card(2, Archetype.Genki), Card(3, Archetype.Mukuchi),
            };

            var lineup = new Lineup(Lineup.SlotsFor(1));
            lineup.Assign(Role.Reception, 0, 1);
            lineup.Assign(Role.Floor, 0, 2);
            lineup.Assign(Role.Kitchen, 0, 3);

            return new Shift(new ShiftSetup
            {
                Day = 4,
                CafeLevel = 1,
                Grid = Room(),
                Roster = roster,
                Lineup = lineup,
                Chemistry = Chemistry.Evaluate(lineup, roster, ThemeTag.None),
                DurationSeconds = 110f,
                ArrivalsPerSecond = arrivals,
                Seed = seed,
            });
        }

        private static void RunUntil(Shift shift, System.Func<Shift, bool> done, float maxSeconds = 120f)
        {
            for (var t = 0f; t < maxSeconds && !shift.Finished && !done(shift); t += Step) shift.Tick(Step);
        }

        /// <summary>A player who plays every help correctly, as often as they can.</summary>
        private static void Assist(Shift shift)
        {
            foreach (var customer in shift.Customers.ToList())
            {
                if (customer.State == CustomerState.Queueing) shift.TryGreet(customer.Id);
                else if (customer.State == CustomerState.Ordering) shift.TryTakeOrder(customer.Id);
                else if (customer.Patience < 0.45f) shift.TryPamper(customer.Id);
            }

            foreach (var seat in shift.Seats.ToList())
            {
                if (seat.Dirty) shift.TryClean(seat.Index);
            }

            foreach (var ticket in shift.Tickets.ToList())
            {
                if (shift.CookProgressOf(ticket) >= Shift.PerfectMin + 0.02f) shift.TryPerfectCook(ticket.Id);
            }
        }

        private static ShiftResult Play(uint seed, bool helping)
        {
            var shift = NewShift(seed);
            return Play(shift, helping);
        }

        private static ShiftResult Play(Shift shift, bool helping)
        {
            while (!shift.Finished)
            {
                shift.Tick(Step);
                if (helping) Assist(shift);
            }

            return shift.Finish();
        }

        /// <summary>The real starter café on a given day, built exactly as the game builds it.</summary>
        private static Shift RealDay(uint seed, int day)
        {
            var profile = Profile.NewGame(seed);
            profile.Day = day;
            return new Shift(DayPlan.Build(profile, seed));
        }

        [Test]
        public void ADayNobodyTouchesIsExactlyTheDayItAlwaysWas()
        {
            for (uint seed = 1; seed <= 4; seed++)
            {
                var a = Play(seed, false);
                var b = Play(seed, false);
                Assert.AreEqual(a.Revenue, b.Revenue, "seed " + seed);
                Assert.AreEqual(0, a.Assists);
                Assert.AreEqual(0, a.PerfectDishes);
                Assert.AreEqual(0, a.BestCombo);
            }
        }

        [Test]
        public void TapsOnNothingChangeNothing()
        {
            var shift = NewShift(3u);
            shift.Tick(0.5f);

            Assert.AreEqual(AssistResult.Rejected, shift.TryGreet(9999));
            Assert.AreEqual(AssistResult.Rejected, shift.TryTakeOrder(9999));
            Assert.AreEqual(AssistResult.Rejected, shift.TryClean(0));
            Assert.AreEqual(AssistResult.Rejected, shift.TryClean(-1));
            Assert.AreEqual(AssistResult.Rejected, shift.TryPamper(9999));
            Assert.AreEqual(AssistResult.Rejected, shift.TryPerfectCook(9999));
            Assert.AreEqual(0, shift.Assists);
            Assert.AreEqual(0, shift.Combo);
        }

        [Test]
        public void GreetingSeatsAQueuedPartyAndTakingTheOrderRaisesTheTickets()
        {
            var shift = NewShift(5u);
            RunUntil(shift, s => s.Customers.Any(c => c.State == CustomerState.Queueing));
            var party = shift.Customers.First(c => c.State == CustomerState.Queueing);

            Assert.AreEqual(AssistResult.Done, shift.TryGreet(party.Id));
            Assert.AreEqual(CustomerState.Seating, party.State);
            Assert.GreaterOrEqual(party.SeatIndex, 0);
            Assert.AreEqual(AssistResult.Rejected, shift.TryGreet(party.Id), "already seated");

            RunUntil(shift, s => party.State == CustomerState.Ordering);
            Assert.AreEqual(CustomerState.Ordering, party.State);

            var tickets = shift.Tickets.Count;
            Assert.AreEqual(AssistResult.Done, shift.TryTakeOrder(party.Id));
            Assert.AreEqual(CustomerState.Waiting, party.State);
            Assert.Greater(shift.Tickets.Count, tickets);
            Assert.AreEqual(2, shift.Assists);
            Assert.AreEqual(2, shift.Combo);
        }

        [Test]
        public void CleaningClearsADirtyTableAndANeverTouchedOneIsRejected()
        {
            var shift = NewShift(7u);
            RunUntil(shift, s => s.Seats.Any(seat => seat.Dirty), 110f);
            var dirty = shift.Seats.FirstOrDefault(seat => seat.Dirty);
            if (dirty == null) Assert.Inconclusive("no table got dirty in this seed");

            Assert.AreEqual(AssistResult.Done, shift.TryClean(dirty.Index));
            Assert.IsFalse(dirty.Dirty);
            Assert.AreEqual(AssistResult.Rejected, shift.TryClean(dirty.Index));
        }

        [Test]
        public void ADishStoppedOnTheGoldenMarkIsReadyAtOnceNeverBurntAndSpendsMore()
        {
            var shift = NewShift(5u, 0.5f);
            RunUntil(shift, s => s.Tickets.Any(t => s.CookProgressOf(t) >= Shift.PerfectMin + 0.02f));
            var ticket = shift.Tickets.FirstOrDefault(t => shift.CookProgressOf(t) >= Shift.PerfectMin);
            Assert.IsNotNull(ticket, "a dish reached the golden mark");

            var customer = shift.CustomerById(ticket.CustomerId);
            var spend = customer.SpendBonus;

            Assert.AreEqual(AssistResult.Done, shift.TryPerfectCook(ticket.Id));
            Assert.AreEqual(TicketState.Ready, ticket.State);
            Assert.AreEqual(1, shift.PerfectDishes);
            Assert.AreEqual(spend * Shift.PerfectSpendBonus, customer.SpendBonus, 1e-4f);
            Assert.AreEqual(AssistResult.Rejected, shift.TryPerfectCook(ticket.Id), "already done");
        }

        [Test]
        public void TappingTheGaugeTooEarlyBreaksTheComboAndCostsProgress()
        {
            var shift = NewShift(5u, 0.5f);
            RunUntil(shift, s => s.Tickets.Any(t =>
            {
                var p = s.CookProgressOf(t);
                return p > 0.1f && p < Shift.PerfectMin - 0.1f;
            }));

            var ticket = shift.Tickets.FirstOrDefault(t =>
            {
                var p = shift.CookProgressOf(t);
                return p > 0.1f && p < Shift.PerfectMin - 0.1f;
            });
            Assert.IsNotNull(ticket);

            // Build a combo first, with any help available, so there is something to break.
            RunUntil(shift, s => s.Customers.Any(c => c.State == CustomerState.Queueing), 5f);
            foreach (var c in shift.Customers.Where(c => c.State == CustomerState.Queueing).ToList()) shift.TryGreet(c.Id);

            var before = shift.CookProgressOf(ticket);
            Assert.AreEqual(AssistResult.TooEarly, shift.TryPerfectCook(ticket.Id));
            Assert.Less(shift.CookProgressOf(ticket), before);
            Assert.AreEqual(0, shift.Combo);
            Assert.AreEqual(TicketState.Cooking, ticket.State);
        }

        /// <summary>A dish being cooked, with its progress pinned to a chosen value.</summary>
        private static (Shift shift, Ticket ticket) DishAt(float progress)
        {
            var shift = NewShift(5u, 0.5f);
            RunUntil(shift, s => s.Tickets.Any(t => s.CookProgressOf(t) > 0.05f));
            var ticket = shift.Tickets.First(t => shift.CookProgressOf(t) > 0.05f);
            var worker = shift.Workers.First(w => w.Claimed != null && w.Claimed.Kind == TaskKind.Cook &&
                                                  w.Claimed.TargetId == ticket.Id);
            worker.Progress = progress;
            return (shift, ticket);
        }

        [Test]
        public void TheGoldenMarkIncludesBothItsEdges()
        {
            foreach (var edge in new[] { Shift.PerfectMin, Shift.PerfectMin + 0.001f, Shift.PerfectMax - 0.001f, Shift.PerfectMax })
            {
                var (shift, ticket) = DishAt(edge);
                Assert.AreEqual(AssistResult.Done, shift.TryPerfectCook(ticket.Id), "progress " + edge);
                Assert.AreEqual(1, shift.PerfectDishes);
            }
        }

        [Test]
        public void JustBeforeTheMarkIsTooEarlyAndJustAfterItIsTooLate()
        {
            var (early, earlyTicket) = DishAt(Shift.PerfectMin - 0.001f);
            Assert.AreEqual(AssistResult.TooEarly, early.TryPerfectCook(earlyTicket.Id));
            Assert.AreEqual(0, early.PerfectDishes);

            var (late, lateTicket) = DishAt(Shift.PerfectMax + 0.001f);
            Assert.AreEqual(AssistResult.TooLate, late.TryPerfectCook(lateTicket.Id));
            Assert.AreEqual(0, late.PerfectDishes);
            Assert.AreEqual(TicketState.Cooking, lateTicket.State, "a late tap does not finish the dish");
        }

        [Test]
        public void ALateTapCostsNothingNotEvenTheCombo()
        {
            var (shift, ticket) = DishAt(Shift.PerfectMax + 0.02f);
            RunUntil(shift, s => s.Customers.Any(c => c.State == CustomerState.Queueing), 3f);
            var greeted = shift.Customers.Where(c => c.State == CustomerState.Queueing).ToList();
            foreach (var c in greeted) shift.TryGreet(c.Id);
            var combo = shift.Combo;
            var spend = shift.CustomerById(ticket.CustomerId).SpendBonus;

            Assert.AreEqual(AssistResult.TooLate, shift.TryPerfectCook(ticket.Id));
            Assert.AreEqual(combo, shift.Combo);
            Assert.AreEqual(spend, shift.CustomerById(ticket.CustomerId).SpendBonus, 1e-6f);
        }

        [Test]
        public void TheComboStopsAtTheCapSoTheScreenMatchesThePay()
        {
            var shift = NewShift(5u, 0.9f);
            for (var t = 0f; t < 90f && !shift.Finished; t += Step)
            {
                shift.Tick(Step);
                Assist(shift);
                Assert.LessOrEqual(shift.Combo, Shift.ComboCap);
            }

            Assert.LessOrEqual(shift.BestCombo, Shift.ComboCap);
            Assert.AreEqual(Shift.ComboCap, shift.BestCombo, "a busy attentive day reaches the cap");
            Assert.AreEqual(Shift.ComboCap * Shift.ComboTipPerStep, shift.ComboTipBonus, 1e-6f);
        }

        [Test]
        public void PamperingCalmsATableOnceThenRecharges()
        {
            var shift = NewShift(5u);
            RunUntil(shift, s => s.Customers.Any(c => c.DrainsPatience && c.Patience < 0.8f && c.Profile.Kind != CustomerType.Cat));
            var customer = shift.Customers.First(c => c.DrainsPatience && c.Patience < 0.8f && c.Profile.Kind != CustomerType.Cat);
            var before = customer.Patience;

            Assert.AreEqual(AssistResult.Done, shift.TryPamper(customer.Id));
            Assert.Greater(customer.Patience, before);

            var other = shift.Customers.FirstOrDefault(c => c != customer && c.DrainsPatience && c.Patience < 0.95f);
            if (other != null) Assert.AreEqual(AssistResult.Cooldown, shift.TryPamper(other.Id));

            Assert.Greater(shift.PamperCooldownRemaining, 0f);
            shift.Tick(Shift.PamperCooldownSeconds + 0.5f);
            Assert.AreEqual(0f, shift.PamperCooldownRemaining);
        }

        [Test]
        public void TheComboExpiresWhenTheOwnerStopsHelping()
        {
            var shift = NewShift(5u);
            RunUntil(shift, s => s.Customers.Any(c => c.State == CustomerState.Queueing));
            shift.TryGreet(shift.Customers.First(c => c.State == CustomerState.Queueing).Id);
            Assert.AreEqual(1, shift.Combo);
            Assert.Greater(shift.ComboTipBonus, 0f);

            for (var t = 0f; t < Shift.ComboWindowSeconds + 1f; t += Step) shift.Tick(Step);
            Assert.AreEqual(0, shift.Combo);
            Assert.AreEqual(0f, shift.ComboTipBonus);
            Assert.AreEqual(1, shift.BestCombo, "the best run is remembered");
        }

        [Test]
        public void AttentivePlayAlwaysAtLeastMatchesAnIdleDayAndStaysAModestBoost()
        {
            double idle = 0, helped = 0;
            var better = 0;
            var runs = 0;
            for (var day = 1; day <= 4; day++)
            {
                for (uint seed = 1; seed <= 6; seed++)
                {
                    var a = Play(RealDay(seed, day), false);
                    var b = Play(RealDay(seed, day), true);
                    idle += a.Revenue;
                    helped += b.Revenue;
                    runs++;
                    if (b.Revenue >= a.Revenue) better++;
                    if (a.Served + b.Served > 0) Assert.Greater(b.Assists, 0, "day " + day + " seed " + seed);
                }
            }

            TestContext.WriteLine("idle " + idle + " helped " + helped + " gain " + (helped / idle - 1.0));
            Assert.GreaterOrEqual(better, runs - 2, "helping does not make the day worse");
            Assert.Greater(helped, idle, "helping pays");
            Assert.Less(helped / idle, 3.2, "even a flawless player does not replace the lineup");
        }

        [Test]
        public void ResultCarriesWhatThePlayerDid()
        {
            var shift = NewShift(2u);
            while (!shift.Finished)
            {
                shift.Tick(Step);
                Assist(shift);
            }

            var result = shift.Finish();
            Assert.AreEqual(shift.Assists, result.Assists);
            Assert.AreEqual(shift.PerfectDishes, result.PerfectDishes);
            Assert.AreEqual(shift.BestCombo, result.BestCombo);
            Assert.Greater(result.Assists, 0);
        }
    }
}
