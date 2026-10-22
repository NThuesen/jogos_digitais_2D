using System.Linq;
using Neko.Sim.Core;
using Neko.Sim.Maids;
using Neko.Sim.Progress;
using NUnit.Framework;

namespace Neko.Tests.Sim
{
    public sealed class MaidTests
    {
        private static Maid Fresh(Archetype kind, int level = 1) =>
            Maid.Create(1, "Teste", kind, level);

        [Test]
        public void StatsAddAndClampAtOneHundred()
        {
            var a = new Stats(90, 90, 90, 90);
            var b = new Stats(30, 30, 30, 30);

            var sum = a.Plus(b).Clamped();

            Assert.AreEqual(100, sum.Serve);
            Assert.AreEqual(100, sum.Stamina);
        }

        [Test]
        public void StatsNeverFallBelowOne()
        {
            var clamped = new Stats(-20, 0, 1, 5).Clamped();

            Assert.AreEqual(1, clamped.Serve);
            Assert.AreEqual(1, clamped.Cook);
        }

        [Test]
        public void XpCurveStartsAtZeroAndIsStrictlyIncreasingToTwenty()
        {
            Assert.AreEqual(0, MaidMath.XpForLevel(1));

            var previous = -1;
            for (var level = 1; level <= Maid.MaxLevel; level++)
            {
                var needed = MaidMath.XpForLevel(level);
                Assert.Greater(needed, previous, "level " + level);
                previous = needed;
            }
        }

        [Test]
        public void GrowthSpendsExactlyFourPointsPerLevel()
        {
            foreach (var profile in ArchetypeTable.All)
            {
                var maid = Fresh(profile.Kind);
                var growth = MaidMath.GrowthFor(maid, 5);
                var spent = growth.Serve + growth.Cook + growth.Charm + growth.Stamina;

                Assert.AreEqual(5 * MaidMath.PointsPerLevel, spent, profile.Kind.ToString());
            }
        }

        [Test]
        public void GrowthFollowsTheArchetypeWeights()
        {
            var mukuchi = MaidMath.GrowthFor(Fresh(Archetype.Mukuchi), 10);
            var genki = MaidMath.GrowthFor(Fresh(Archetype.Genki), 10);

            Assert.Greater(mukuchi.Cook, mukuchi.Serve, "Mukuchi must level into the kitchen");
            Assert.Greater(genki.Serve, genki.Cook, "Genki must level into the floor");
        }

        [Test]
        public void GrowthIsDeterministic()
        {
            var once = MaidMath.GrowthFor(Fresh(Archetype.Tsundere), 7);
            var twice = MaidMath.GrowthFor(Fresh(Archetype.Tsundere), 7);

            Assert.AreEqual(once, twice);
        }

        [Test]
        public void CreateAtALevelAlreadyCarriesThatLevelsGrowth()
        {
            var one = Fresh(Archetype.Genki);
            var ten = Fresh(Archetype.Genki, 10);

            Assert.Greater(ten.Base.Serve, one.Base.Serve);
            Assert.AreEqual(10, ten.Level);
        }

        [Test]
        public void TotalIncludesGearAndBond()
        {
            var maid = Fresh(Archetype.Genki);
            var bare = maid.Total;

            maid.Gear[(int)GearSlot.Headwear] =
                new Equipment("test_hat", "Chapéu", GearSlot.Headwear, new Stats(0, 0, 7, 0),
                    ThemeTag.None, 10, 1, "Bow", "#ffffff");
            maid.Bond = 3;

            Assert.AreEqual(bare.Charm + 7 + 3, maid.Total.Charm);
        }

        [Test]
        public void MoodStartsFullAndIsClampedBothWays()
        {
            var maid = Fresh(Archetype.Genki);
            Assert.AreEqual(1f, maid.Mood);

            maid.Mood = 4f;
            Assert.AreEqual(1f, maid.Mood);

            maid.Mood = -2f;
            Assert.AreEqual(0f, maid.Mood);
        }

        [Test]
        public void EveryMaidHasFourGearSlotsAndNoSkillsAtBirth()
        {
            var maid = Fresh(Archetype.Idol);

            Assert.AreEqual(4, maid.Gear.Length);
            Assert.IsTrue(maid.Gear.All(g => g == null));
            Assert.IsEmpty(maid.Skills);
        }
    }
}
