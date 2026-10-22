using System;
using System.Collections.Generic;
using System.Linq;
using Neko.Sim.Core;
using NUnit.Framework;

namespace Neko.Tests.Sim
{
    public sealed class RngTests
    {
        [Test]
        public void SameSeedGivesSameSequence()
        {
            var a = new Rng(1234);
            var b = new Rng(1234);

            for (var i = 0; i < 100; i++)
            {
                Assert.AreEqual(a.NextFloat(), b.NextFloat(), "diverged at draw " + i);
            }
        }

        [Test]
        public void DifferentSeedsDiverge()
        {
            var a = new Rng(1);
            var b = new Rng(2);
            var same = 0;

            for (var i = 0; i < 100; i++)
            {
                if (Math.Abs(a.NextFloat() - b.NextFloat()) < 1e-9f) same++;
            }

            Assert.Less(same, 5, "two seeds produced essentially the same stream");
        }

        [Test]
        public void SeedZeroIsStillAUsefulStream()
        {
            var rng = new Rng(0);
            var draws = Enumerable.Range(0, 50).Select(_ => rng.NextFloat()).ToList();

            Assert.AreNotEqual(0, rng.State, "xorshift latched to zero and the stream is dead");
            Assert.Greater(draws.Distinct().Count(), 40);
        }

        [Test]
        public void NextFloatStaysInRange()
        {
            var rng = new Rng(7);

            for (var i = 0; i < 10000; i++)
            {
                var v = rng.NextFloat();
                Assert.GreaterOrEqual(v, 0f);
                Assert.Less(v, 1f);
            }
        }

        [Test]
        public void NextIntNeverReturnsMax()
        {
            var rng = new Rng(11);
            var seen = new HashSet<int>();

            for (var i = 0; i < 10000; i++)
            {
                var v = rng.NextInt(3);
                Assert.GreaterOrEqual(v, 0);
                Assert.Less(v, 3);
                seen.Add(v);
            }

            CollectionAssert.AreEquivalent(new[] { 0, 1, 2 }, seen);
        }

        [Test]
        public void NextIntOfOneIsAlwaysZeroAndOfZeroIsRefused()
        {
            var rng = new Rng(3);

            Assert.AreEqual(0, rng.NextInt(1));
            Assert.Throws<ArgumentOutOfRangeException>(() => rng.NextInt(0));
        }

        [Test]
        public void RangeRespectsBothEnds()
        {
            var rng = new Rng(5);

            for (var i = 0; i < 5000; i++)
            {
                var v = rng.Range(2.5f, 4.5f);
                Assert.GreaterOrEqual(v, 2.5f);
                Assert.Less(v, 4.5f);
            }
        }

        [Test]
        public void PickWeightedRespectsWeights()
        {
            var rng = new Rng(99);
            var items = new[] { "rare", "common" };
            var weights = new Dictionary<string, float> { { "rare", 1f }, { "common", 9f } };
            var rare = 0;
            const int draws = 20000;

            for (var i = 0; i < draws; i++)
            {
                if (rng.PickWeighted(items, x => weights[x]) == "rare") rare++;
            }

            Assert.That(rare / (float)draws, Is.EqualTo(0.10f).Within(0.02f));
        }

        [Test]
        public void PickWeightedWithAllZeroWeightsIsUniformAndNeverNull()
        {
            var rng = new Rng(13);
            var items = new[] { "a", "b", "c" };
            var seen = new HashSet<string>();

            for (var i = 0; i < 3000; i++)
            {
                var pick = rng.PickWeighted(items, _ => 0f);
                Assert.IsNotNull(pick);
                seen.Add(pick);
            }

            CollectionAssert.AreEquivalent(items, seen);
        }

        [Test]
        public void PickWeightedIgnoresNegativeWeights()
        {
            var rng = new Rng(17);
            var items = new[] { "never", "always" };

            for (var i = 0; i < 1000; i++)
            {
                Assert.AreEqual("always", rng.PickWeighted(items, x => x == "never" ? -5f : 1f));
            }
        }

        [Test]
        public void PickFromAnEmptyListIsRefused()
        {
            var rng = new Rng(19);

            Assert.Throws<ArgumentException>(() => rng.Pick(new string[0]));
            Assert.Throws<ArgumentException>(() => rng.PickWeighted(new string[0], _ => 1f));
        }

        [Test]
        public void ChanceIsHonestAtTheEdges()
        {
            var rng = new Rng(23);

            for (var i = 0; i < 2000; i++)
            {
                Assert.IsFalse(rng.Chance(0f));
                Assert.IsTrue(rng.Chance(1f));
            }
        }

        [Test]
        public void ForkDivergesFromTheParentButIsReproducible()
        {
            var parent = new Rng(31);
            var fork = parent.Fork();
            var forkDraws = Enumerable.Range(0, 20).Select(_ => fork.NextFloat()).ToList();
            var parentDraws = Enumerable.Range(0, 20).Select(_ => parent.NextFloat()).ToList();

            CollectionAssert.AreNotEqual(parentDraws, forkDraws);

            var again = new Rng(31);
            var againFork = again.Fork();
            CollectionAssert.AreEqual(
                forkDraws,
                Enumerable.Range(0, 20).Select(_ => againFork.NextFloat()).ToList());
        }
    }
}
