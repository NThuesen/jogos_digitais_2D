using System.Collections.Generic;
using System.Linq;
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
    /// The shape of the money curve, played end to end on real starter companies: a café left
    /// alone bleeds slowly, a café the owner tends to earns, and no archetype is a trap.
    /// </summary>
    public sealed class EconomyBalanceTests
    {
        private const float Step = 0.1f;

        private static void FillNatural(Profile profile)
        {
            profile.Lineup.Clear();
            foreach (var slot in profile.Lineup.All)
            {
                var maid = profile.Maids
                    .Where(m => m.NaturalRole == slot.Role && !profile.Lineup.Contains(m.Id))
                    .OrderByDescending(m => m.Total[RoleTable.For(m.NaturalRole).Primary])
                    .FirstOrDefault();
                if (maid != null) profile.Lineup.Assign(slot.Role, slot.Index, maid.Id);
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

        /// <summary>Plays <paramref name="days"/> days and returns the ledgers.</summary>
        private static List<DayLedger> Career(uint seed, int days, bool tending)
        {
            var profile = Profile.NewGame(seed * 2654435761u);
            var rng = new Rng(profile.Seed == 0 ? 1u : profile.Seed);
            var ledgers = new List<DayLedger>();

            for (var day = 1; day <= days && !profile.Bankrupt; day++)
            {
                FillNatural(profile);
                var shift = new Shift(DayPlan.Build(profile, rng.NextUInt()));
                while (!shift.Finished)
                {
                    shift.Tick(Step);
                    if (tending) Tend(shift);
                }

                ledgers.Add(Books.Close(profile, shift.Finish()));
            }

            return ledgers;
        }

        [Test]
        public void ACafeLeftAloneBleedsSlowlyInsteadOfDyingInAWeek()
        {
            for (uint seed = 1; seed <= 8; seed++)
            {
                var ledgers = Career(seed, 4, false);
                Assert.AreEqual(4, ledgers.Count, "seed " + seed + " went bankrupt inside four days");
                Assert.Less(-ledgers[0].Profit, 170, "seed " + seed + ": day one should not cost most of the cash");
            }
        }

        [Test]
        public void ACafeTheOwnerTendsPaysItsWayAndGrows()
        {
            var profitable = 0;
            for (uint seed = 1; seed <= 8; seed++)
            {
                var ledgers = Career(seed, 12, true);
                Assert.AreEqual(12, ledgers.Count, "seed " + seed + " went bankrupt while being tended");
                if (ledgers.Skip(2).Sum(l => l.Profit) > 0) profitable++;
            }

            Assert.GreaterOrEqual(profitable, 7, "tending should make nearly every café profitable");
        }

        [Test]
        public void TendingIsWorthMoreThanLeavingTheTeamAlone()
        {
            for (uint seed = 1; seed <= 6; seed++)
            {
                var idle = Career(seed, 5, false).Sum(l => l.Profit);
                var tended = Career(seed, 5, true).Sum(l => l.Profit);
                Assert.Greater(tended, idle, "seed " + seed);
            }
        }

        [Test]
        public void NoKitchenArchetypeIsAnOutlier()
        {
            var kitchen = new[] { Archetype.Kuudere, Archetype.Gourmand, Archetype.Mukuchi };
            var means = new List<double>();
            foreach (var archetype in kitchen)
            {
                double revenue = 0;
                const int Runs = 24;
                for (uint seed = 1; seed <= Runs; seed++)
                {
                    var profile = new Profile { Seed = seed };
                    var rng = new Rng(seed * 7919u);
                    foreach (var role in new[] { Role.Reception, Role.Floor, Role.Kitchen })
                    {
                        var pool = MaidCatalog.Released.Where(i => RoleTable.NaturalFor(i.Archetype) == role).ToList();
                        var identity = role == Role.Kitchen
                            ? pool.Where(i => i.Archetype == archetype).OrderBy(i => i.Id).First()
                            : rng.Pick(pool);
                        var maid = Lootbox.Mint(profile, Rarity.Common, rng, identity);
                        maid.NaturalRole = role;
                        profile.Add(maid);
                    }

                    profile.Lineup.Rebuild(Lineup.SlotsFor(1));
                    profile.Lineup.AutoFill(profile.Maids);
                    profile.Day = 3;

                    var shift = new Shift(DayPlan.Build(profile, seed * 31u));
                    while (!shift.Finished) shift.Tick(Step);
                    revenue += shift.Finish().Revenue;
                }

                means.Add(revenue / Runs);
            }

            Assert.Less(means.Max() / means.Min(), 1.35, "a kitchen archetype pulls clearly ahead or behind: "
                                                         + string.Join(" / ", means.Select(m => m.ToString("F0"))));
        }
    }
}
