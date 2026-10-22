using System;
using System.Collections.Generic;
using System.Linq;
using Neko.Sim.Core;
using Neko.Sim.Service;
using NUnit.Framework;

namespace Neko.Tests.Sim
{
    public sealed class CustomerTests
    {
        private static Customer Spawn(CustomerType kind, int cafeLevel = 10) =>
            Customer.Spawn(1, kind, new Rng(42), cafeLevel, 1f);

        [Test]
        public void EverySevenTypesAreInTheTable()
        {
            var kinds = (CustomerType[])Enum.GetValues(typeof(CustomerType));

            Assert.AreEqual(7, kinds.Length);
            foreach (var kind in kinds) Assert.IsNotNull(CustomerTable.For(kind));
        }

        [Test]
        public void EveryProfileIsConsistent()
        {
            foreach (var profile in CustomerTable.All)
            {
                Assert.IsNotEmpty(profile.DisplayName, profile.Kind.ToString());
                Assert.Greater(profile.PatienceSeconds, 0f, profile.Kind.ToString());
                Assert.Greater(profile.SpendMultiplier, 0f, profile.Kind.ToString());
                Assert.GreaterOrEqual(profile.PartySizeMax, profile.PartySizeMin, profile.Kind.ToString());
                Assert.GreaterOrEqual(profile.PartySizeMin, 1, profile.Kind.ToString());
                Assert.Greater(profile.ArrivalWeight, 0f, profile.Kind.ToString());
            }
        }

        [Test]
        public void VipHasTheLeastPatienceAndTheMostSpend()
        {
            var impatient = CustomerTable.All
                .Where(p => !p.NeverImpatient)
                .OrderBy(p => p.PatienceSeconds)
                .First();

            Assert.AreEqual(CustomerType.Vip, impatient.Kind);
            Assert.AreEqual(CustomerType.Vip, CustomerTable.All.OrderByDescending(p => p.SpendMultiplier).First().Kind);
        }

        [Test]
        public void OnlyTheCatIsNeverImpatient()
        {
            foreach (var profile in CustomerTable.All)
            {
                Assert.AreEqual(profile.Kind == CustomerType.Cat, profile.NeverImpatient,
                    profile.Kind.ToString());
            }
        }

        [Test]
        public void TheCatOnlyEverOrdersMilk()
        {
            for (var seed = 1u; seed < 50u; seed++)
            {
                var cat = Customer.Spawn(1, CustomerType.Cat, new Rng(seed), 10, 1f);

                CollectionAssert.AreEqual(
                    new[] { "milk" },
                    cat.Order.Dishes.Select(d => d.Id).Distinct().ToList());
            }
        }

        [Test]
        public void TheCatRaisesAmbienceJustBySittingThere()
        {
            Assert.Greater(CustomerTable.For(CustomerType.Cat).AmbienceWhileSeated, 0f);
        }

        [Test]
        public void OnlyTheOtakuWantsAParticularMaid()
        {
            foreach (var profile in CustomerTable.All)
            {
                Assert.AreEqual(profile.Kind == CustomerType.Otaku, profile.WantsAnArchetype,
                    profile.Kind.ToString());
            }
        }

        [Test]
        public void AnOtakuAlwaysNamesSomebody()
        {
            for (var seed = 1u; seed < 30u; seed++)
            {
                var otaku = Customer.Spawn(1, CustomerType.Otaku, new Rng(seed), 10, 1f);
                Assert.IsTrue(otaku.WantedArchetype.HasValue);
            }
        }

        [Test]
        public void PatienceDrainsWhileWaitingAndNotWhileEating()
        {
            var drains = new[] { CustomerState.Queueing, CustomerState.Ordering, CustomerState.Waiting };
            var rests = new[]
            {
                CustomerState.Seating, CustomerState.Thinking, CustomerState.Eating,
                CustomerState.Paying, CustomerState.Leaving, CustomerState.Angry,
            };

            var customer = Spawn(CustomerType.Regular);

            foreach (var state in drains)
            {
                customer.State = state;
                Assert.IsTrue(customer.DrainsPatience, state.ToString());
            }

            foreach (var state in rests)
            {
                customer.State = state;
                Assert.IsFalse(customer.DrainsPatience, state.ToString());
            }
        }

        [Test]
        public void EveryStateIsEitherDrainingOrResting()
        {
            // The one test that stops a new state being added and silently never draining.
            var customer = Spawn(CustomerType.Regular);

            foreach (CustomerState state in Enum.GetValues(typeof(CustomerState)))
            {
                customer.State = state;
                Assert.DoesNotThrow(() => { var _ = customer.DrainsPatience; }, state.ToString());
            }
        }

        [Test]
        public void DrainingTakesTheWholePatienceSecondsToEmpty()
        {
            var customer = Spawn(CustomerType.Regular);
            customer.State = CustomerState.Waiting;
            var seconds = CustomerTable.For(CustomerType.Regular).PatienceSeconds;

            customer.DrainPatience(seconds * 0.5f, 1f);
            Assert.That(customer.Patience, Is.EqualTo(0.5f).Within(0.01f));

            customer.DrainPatience(seconds * 0.5f, 1f);
            Assert.AreEqual(0f, customer.Patience);
        }

        [Test]
        public void AmbienceBuysPatience()
        {
            var plain = Spawn(CustomerType.Regular);
            var cosy = Spawn(CustomerType.Regular);
            plain.State = cosy.State = CustomerState.Waiting;

            plain.DrainPatience(10f, 1f);
            cosy.DrainPatience(10f, 2f);

            Assert.Greater(cosy.Patience, plain.Patience);
        }

        [Test]
        public void TheCatNeverRunsOut()
        {
            var cat = Spawn(CustomerType.Cat);
            cat.State = CustomerState.Waiting;

            cat.DrainPatience(10000f, 1f);

            Assert.AreEqual(1f, cat.Patience);
        }

        [Test]
        public void PatienceNeverGoesBelowZero()
        {
            var customer = Spawn(CustomerType.Regular);
            customer.State = CustomerState.Waiting;

            customer.DrainPatience(100000f, 1f);

            Assert.AreEqual(0f, customer.Patience);
        }

        [Test]
        public void AnOrderIsCompleteOnlyWhenEveryDishIsServed()
        {
            var customer = Spawn(CustomerType.Student);
            var order = customer.Order;

            Assert.Greater(order.Dishes.Count, 0);

            for (var i = 0; i < order.Dishes.Count; i++)
            {
                Assert.IsFalse(order.Complete, "complete after " + i + " of " + order.Dishes.Count);
                order.Serve();
            }

            Assert.IsTrue(order.Complete);
        }

        [Test]
        public void ServingMoreThanWasOrderedIsIgnored()
        {
            var order = Spawn(CustomerType.Regular).Order;
            for (var i = 0; i < 20; i++) order.Serve();

            Assert.AreEqual(order.Dishes.Count, order.ServedCount);
        }

        [Test]
        public void OrdersAreNeverEmptyAndNeverLongerThanThree()
        {
            for (var seed = 1u; seed < 200u; seed++)
            {
                foreach (CustomerType kind in Enum.GetValues(typeof(CustomerType)))
                {
                    var customer = Customer.Spawn(1, kind, new Rng(seed), 10, 1f);

                    Assert.That(customer.Order.Dishes.Count, Is.InRange(1, 3),
                        kind + " seed " + seed);
                }
            }
        }

        [Test]
        public void ACustomerOnlyEverOrdersUnlockedDishes()
        {
            var allowed = new HashSet<string>(RecipeBook.UnlockedAt(2).Select(r => r.Id));

            for (var seed = 1u; seed < 200u; seed++)
            {
                var customer = Customer.Spawn(1, CustomerType.Regular, new Rng(seed), 2, 1f);

                foreach (var dish in customer.Order.Dishes)
                {
                    CollectionAssert.Contains(allowed, dish.Id, "seed " + seed);
                }
            }
        }

        [Test]
        public void LockedCustomerTypesNeverRoll()
        {
            var rng = new Rng(77);
            var allowed = CustomerTable.All
                .Where(p => p.UnlockCafeLevel <= 2)
                .Select(p => p.Kind)
                .ToList();

            for (var i = 0; i < 5000; i++)
            {
                CollectionAssert.Contains(allowed, CustomerTable.Roll(rng, 2, 1f));
            }
        }

        [Test]
        public void OjousamaDoublesTheVipArrivalWeight()
        {
            Assert.Greater(VipShare(2f), VipShare(1f) * 1.7f);
        }

        [Test]
        public void EveryUnlockedTypeEventuallyShowsUp()
        {
            var rng = new Rng(101);
            var seen = new HashSet<CustomerType>();

            for (var i = 0; i < 20000; i++) seen.Add(CustomerTable.Roll(rng, 10, 1f));

            CollectionAssert.AreEquivalent(CustomerTable.All.Select(p => p.Kind), seen);
        }

        [Test]
        public void PartySizeNeverExceedsTheProfile()
        {
            for (var seed = 1u; seed < 300u; seed++)
            {
                foreach (var profile in CustomerTable.All)
                {
                    var customer = Customer.Spawn(1, profile.Kind, new Rng(seed), 10, 1f);

                    Assert.That(customer.PartySize,
                        Is.InRange(profile.PartySizeMin, profile.PartySizeMax),
                        profile.Kind + " seed " + seed);
                }
            }
        }

        private static float VipShare(float multiplier)
        {
            var rng = new Rng(55);
            var vips = 0;
            const int draws = 20000;

            for (var i = 0; i < draws; i++)
            {
                if (CustomerTable.Roll(rng, 10, multiplier) == CustomerType.Vip) vips++;
            }

            return vips / (float)draws;
        }
    }
}
