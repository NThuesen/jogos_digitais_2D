using System;
using System.Linq;
using Neko.Sim.Maids;
using NUnit.Framework;

namespace Neko.Tests.Sim
{
    public sealed class ArchetypeTests
    {
        [Test]
        public void AllNineArchetypesAreInTheTable()
        {
            var kinds = (Archetype[])Enum.GetValues(typeof(Archetype));

            Assert.AreEqual(9, kinds.Length);
            CollectionAssert.AreEquivalent(kinds, ArchetypeTable.All.Select(p => p.Kind));
        }

        [Test]
        public void EveryProfileHasANameAnAccentAndATell()
        {
            foreach (var profile in ArchetypeTable.All)
            {
                Assert.IsNotEmpty(profile.DisplayName, profile.Kind.ToString());
                Assert.IsTrue(profile.AccentHex.StartsWith("#"), profile.Kind + " accent");
                Assert.IsNotEmpty(profile.SilhouetteTell, profile.Kind + " tell");
            }
        }

        [Test]
        public void EveryAccentIsDistinctSoTwoMaidsAreNeverTheSameColour()
        {
            var accents = ArchetypeTable.All.Select(p => p.AccentHex.ToLowerInvariant()).ToList();

            CollectionAssert.AllItemsAreUnique(accents);
        }

        [Test]
        public void EveryProfileGrowsSomething()
        {
            foreach (var profile in ArchetypeTable.All)
            {
                var weights = profile.GrowthWeights;
                Assert.Greater(
                    weights.Serve + weights.Cook + weights.Charm + weights.Stamina,
                    0,
                    profile.Kind + " would never gain a stat");
            }
        }

        [Test]
        public void GenkiMovesFasterAndTiresFaster()
        {
            var genki = ArchetypeTable.For(Archetype.Genki);

            Assert.AreEqual(1.35f, genki.MoveMultiplier);
            Assert.AreEqual(1.6f, genki.MoodDrainMultiplier);
        }

        [Test]
        public void KuudereNeverFumblesAndIsSlow()
        {
            var kuudere = ArchetypeTable.For(Archetype.Kuudere);

            Assert.IsTrue(kuudere.NeverFumbles);
            Assert.AreEqual(0.8f, kuudere.MoveMultiplier);
        }

        [Test]
        public void OjousamaRefusesToBusTablesAndPullsVips()
        {
            var ojou = ArchetypeTable.For(Archetype.Ojousama);

            Assert.IsFalse(ojou.WillBusTables);
            Assert.AreEqual(2f, ojou.VipWeightMultiplier);
        }

        [Test]
        public void EveryOtherArchetypeWillBusTables()
        {
            foreach (var profile in ArchetypeTable.All.Where(p => p.Kind != Archetype.Ojousama))
            {
                Assert.IsTrue(profile.WillBusTables, profile.Kind + " must still bus tables");
            }
        }

        [Test]
        public void OnlyDojikkoDropsPlatesOnPurpose()
        {
            foreach (var profile in ArchetypeTable.All)
            {
                var expected = profile.Kind == Archetype.Dojikko ? 0.12f : 0f;
                Assert.AreEqual(expected, profile.ClumsyDropChance, profile.Kind.ToString());
            }
        }

        [Test]
        public void OnlyIdolHasEncore()
        {
            foreach (var profile in ArchetypeTable.All)
            {
                Assert.AreEqual(profile.Kind == Archetype.Idol, profile.HasEncore, profile.Kind.ToString());
            }
        }

        [Test]
        public void OnlyOneesanSharesXpAndGrantsAnAura()
        {
            foreach (var profile in ArchetypeTable.All)
            {
                var isOnee = profile.Kind == Archetype.Oneesan;
                Assert.AreEqual(isOnee ? 0.25f : 0f, profile.XpShareFraction, profile.Kind.ToString());
                Assert.AreEqual(isOnee ? 0.15f : 0f, profile.NeighbourSpeedAura, profile.Kind.ToString());
            }
        }

        [Test]
        public void OnlyGourmandEatsTheStock()
        {
            foreach (var profile in ArchetypeTable.All)
            {
                Assert.AreEqual(profile.Kind == Archetype.Gourmand ? 0.2f : 0f, profile.EatChance,
                    profile.Kind.ToString());
            }
        }

        [Test]
        public void ForIsDefinedForEveryEnumValueAndRefusesAnInventedOne()
        {
            foreach (Archetype kind in Enum.GetValues(typeof(Archetype)))
            {
                Assert.IsNotNull(ArchetypeTable.For(kind));
            }

            Assert.Throws<ArgumentOutOfRangeException>(() => ArchetypeTable.For((Archetype)99));
        }
    }
}
