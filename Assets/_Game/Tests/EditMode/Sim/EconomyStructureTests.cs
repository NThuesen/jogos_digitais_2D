using System.Collections.Generic;
using System.Linq;
using Neko.Sim.Cafe;
using Neko.Sim.Core;
using Neko.Sim.Economy;
using Neko.Sim.Maids;
using Neko.Sim.Save;
using Neko.Sim.Service;
using Neko.Sim.Shift;
using Neko.Sim.Team;
using NUnit.Framework;

namespace Neko.Tests.Sim
{
    /// <summary>
    /// ECON-02b: the room is capacity, not demand. A table pays when the team and the kitchen can
    /// turn it, and costs when they cannot; the books balance; the same seed gives the same career.
    /// </summary>
    public sealed class EconomyStructureTests
    {
        private const float Step = 0.1f;

        [Test]
        public void DemandDoesNotDependOnTheNumberOfSeatsOrTheDay()
        {
            var reference = DayPlan.ArrivalsPerSecondFor(2, 1, 4);
            foreach (var seats in new[] { 1, 4, 8, 16 })
            {
                foreach (var day in new[] { 1, 10, 40 })
                {
                    Assert.AreEqual(reference, DayPlan.ArrivalsPerSecondFor(day, 1, seats), 1e-6f);
                }
            }
        }

        [Test]
        public void ReputationGrowsWithHappyPartiesAndNeverGoesNegative()
        {
            var few = new ShiftResult { Served = 4, Satisfaction = 0.5f };
            var many = new ShiftResult { Served = 10, Satisfaction = 0.5f };
            var angry = new ShiftResult { Served = 1, Satisfaction = 0.1f, LeftAngry = 9 };
            Assert.Greater(DayPlan.ReputationFor(many), DayPlan.ReputationFor(few));
            Assert.AreEqual(0, DayPlan.ReputationFor(angry));
        }

        [Test]
        public void TheBooksBalanceEveryDay()
        {
            var profile = Profile.NewGame(1234567u);
            var rng = new Rng(11u);
            for (var day = 1; day <= 6 && !profile.Bankrupt; day++)
            {
                var shift = new Shift(DayPlan.Build(profile, rng.NextUInt()));
                while (!shift.Finished) shift.Tick(Step);

                var ledger = Books.Close(profile, shift.Finish());
                Assert.AreEqual(ledger.BalanceBefore + ledger.Revenue - ledger.Costs, ledger.BalanceAfter, "day " + day);
                Assert.GreaterOrEqual(ledger.Costs, Books.BaseRent, "rent is always due");
                Assert.AreEqual(ledger.Wages + ledger.Rent + ledger.Ingredients, ledger.Costs);
            }
        }

        [Test]
        public void TheSameSeedPlaysTheSameCareer()
        {
            Assert.AreEqual(Career(5, 8, Setup.None), Career(5, 8, Setup.None));
            Assert.AreEqual(Career(5, 8, Setup.StoveAndTables), Career(5, 8, Setup.StoveAndTables));
        }

        [Test]
        public void ATeamThatCanServeItEarnsMoreFromAStoveAndTwoTables()
        {
            var control = Mean(Setup.None, Rarity.Epic);
            var staffed = Mean(Setup.StoveAndTables, Rarity.Epic);
            Assert.Greater(staffed, control * 1.05, "stove + 2 tables should pay for an epic team: " + staffed + " vs " + control);
        }

        [Test]
        public void TablesWithoutAKitchenToFeedThemAreAMistake()
        {
            var control = Mean(Setup.None, Rarity.Common);
            var tablesOnly = Mean(Setup.TablesOnly, Rarity.Common);
            Assert.Less(tablesOnly, control, "extra tables on a common team with one stove should not pay: " + tablesOnly + " vs " + control);
        }

        private enum Setup { None, StoveAndTables, TablesOnly }

        private static double Mean(Setup setup, Rarity rarity)
        {
            double total = 0;
            const int Seeds = 16;
            for (uint seed = 1; seed <= Seeds; seed++) total += Career(seed, 6, setup, rarity);
            return total / Seeds;
        }

        private static double Career(uint seed, int days, Setup setup, Rarity rarity = Rarity.Common)
        {
            var profile = Profile.NewGame(seed * 2654435761u);
            if (rarity != Rarity.Common) Upgrade(profile, rarity, seed);
            Place(profile, setup);

            var rng = new Rng(seed * 977u + 3u);
            double revenue = 0;
            for (var day = 1; day <= days && !profile.Bankrupt; day++)
            {
                profile.Lineup.AutoFill(profile.Maids);
                var shift = new Shift(DayPlan.Build(profile, rng.NextUInt()));
                while (!shift.Finished)
                {
                    shift.Tick(Step);
                    Tend(shift);
                }

                var ledger = Books.Close(profile, shift.Finish());
                if (day >= 2) revenue += ledger.Revenue;
            }

            return revenue;
        }

        private static void Upgrade(Profile profile, Rarity rarity, uint seed)
        {
            var rng = new Rng(seed + 500u);
            var roles = profile.Maids.Select(m => m.NaturalRole).ToList();
            profile.Maids.Clear();
            profile.Lineup.Clear();
            foreach (var role in roles)
            {
                var pool = MaidCatalog.FreeReleased(profile.Maids).Where(i => RoleTable.NaturalFor(i.Archetype) == role).ToList();
                var maid = Lootbox.Mint(profile, rarity, rng, rng.Pick(pool));
                maid.NaturalRole = role;
                profile.Add(maid);
            }
        }

        private static void Place(Profile profile, Setup setup)
        {
            if (setup == Setup.None) return;
            if (setup == Setup.StoveAndTables) PlaceAnywhere(profile, "stove");
            PlaceAnywhere(profile, "table_2");
            PlaceAnywhere(profile, "table_2");
        }

        private static void PlaceAnywhere(Profile profile, string id)
        {
            var def = PlaceableCatalog.ById(id);
            for (var y = 0; y < profile.Grid.Height; y++)
            {
                for (var x = 2; x < profile.Grid.Width; x++)
                {
                    if (profile.Grid.Place(def, x, y)) return;
                }
            }
        }

        private static void Tend(Shift shift)
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
    }
}
