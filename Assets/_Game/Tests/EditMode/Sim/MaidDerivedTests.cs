using Neko.Sim.Maids;
using NUnit.Framework;

namespace Neko.Tests.Sim
{
    public sealed class MaidDerivedTests
    {
        private static Maid Fresh(Archetype kind, int level = 1) => Maid.Create(1, "Teste", kind, level);

        private static ShiftSnapshot Floor(float occupancy = 0f, bool idle = false) =>
            new ShiftSnapshot(occupancy, false, idle, 0f, false);

        private static ShiftSnapshot AtStation(float occupancy = 0f) =>
            new ShiftSnapshot(occupancy, true, false, 0f, false);

        [Test]
        public void GenkiMovesThirtyFivePercentFasterThanABaselineOfTheSameStats()
        {
            var genki = Fresh(Archetype.Genki);
            var idol = Fresh(Archetype.Idol);
            genki.Base = idol.Base;

            var ratio = MaidMath.Derive(genki, Floor()).MoveSpeed /
                        MaidMath.Derive(idol, Floor()).MoveSpeed;

            Assert.That(ratio, Is.EqualTo(1.35f).Within(1e-4f));
        }

        [Test]
        public void KuudereNeverFumblesEvenWithTheWorstCookStat()
        {
            var kuudere = Fresh(Archetype.Kuudere);
            kuudere.Base = new Stats(1, 1, 1, 1);

            var derived = MaidMath.Derive(kuudere, AtStation());

            Assert.AreEqual(0f, derived.FumbleChance);
            Assert.AreEqual(0f, derived.DropChance);
        }

        [Test]
        public void DojikkoDropsRegardlessOfSkillBecauseItIsNotASkillProblem()
        {
            var novice = Fresh(Archetype.Dojikko);
            novice.Base = new Stats(1, 1, 1, 1);
            var expert = Fresh(Archetype.Dojikko);
            expert.Base = new Stats(100, 100, 100, 100);

            Assert.AreEqual(0.12f, MaidMath.Derive(novice, Floor()).DropChance);
            Assert.AreEqual(0.12f, MaidMath.Derive(expert, Floor()).DropChance);
        }

        [Test]
        public void FumbleChanceFallsAsTheCookStatRises()
        {
            var novice = Fresh(Archetype.Genki);
            novice.Base = new Stats(50, 1, 50, 50);
            var expert = Fresh(Archetype.Genki);
            expert.Base = new Stats(50, 100, 50, 50);

            Assert.Greater(MaidMath.Derive(novice, AtStation()).FumbleChance,
                           MaidMath.Derive(expert, AtStation()).FumbleChance);
        }

        [Test]
        public void TsundereSpeedsUpOnlyOnceTheRoomIsCrowded()
        {
            var maid = Fresh(Archetype.Tsundere);

            var quiet = MaidMath.Derive(maid, Floor(0.59f)).ServeSpeed;
            var busy = MaidMath.Derive(maid, Floor(0.60f)).ServeSpeed;

            Assert.That(busy / quiet, Is.EqualTo(1.30f).Within(1e-4f));
        }

        [Test]
        public void TsundereLosesTipsWhileIdle()
        {
            var maid = Fresh(Archetype.Tsundere);

            var working = MaidMath.Derive(maid, Floor(0.2f)).TipMultiplier;
            var idle = MaidMath.Derive(maid, Floor(0.2f, idle: true)).TipMultiplier;

            Assert.That(idle / working, Is.EqualTo(0.85f).Within(1e-4f));
        }

        [Test]
        public void OnlyTsundereCaresAboutTheCrowd()
        {
            foreach (var profile in ArchetypeTable.All)
            {
                if (profile.Kind == Archetype.Tsundere) continue;

                var maid = Fresh(profile.Kind);
                Assert.That(MaidMath.Derive(maid, Floor(0.9f)).ServeSpeed,
                    Is.EqualTo(MaidMath.Derive(maid, Floor(0f)).ServeSpeed).Within(1e-4f),
                    profile.Kind.ToString());
            }
        }

        [Test]
        public void MukuchiIsSlowToGreetAndFastToCook()
        {
            var mukuchi = Fresh(Archetype.Mukuchi);
            var idol = Fresh(Archetype.Idol);
            mukuchi.Base = idol.Base;

            var m = MaidMath.Derive(mukuchi, AtStation());
            var i = MaidMath.Derive(idol, AtStation());

            Assert.That(m.GreetSpeed / i.GreetSpeed, Is.EqualTo(0.6f).Within(1e-4f));
            Assert.That(m.CookSpeed / i.CookSpeed, Is.EqualTo(1.5f).Within(1e-4f));
        }

        [Test]
        public void MukuchiCharmCountsDoubleAtAStation()
        {
            var maid = Fresh(Archetype.Mukuchi);
            maid.Base = new Stats(50, 50, 50, 50);

            var onFloor = MaidMath.Derive(maid, Floor()).TipMultiplier;
            var atStation = MaidMath.Derive(maid, AtStation()).TipMultiplier;

            Assert.Greater(atStation, onFloor);
            Assert.That(atStation - 1f, Is.EqualTo((onFloor - 1f) * 2f).Within(1e-4f));
        }

        [Test]
        public void GourmandCooksAlmostTwiceAsFast()
        {
            var gourmand = Fresh(Archetype.Gourmand);
            var idol = Fresh(Archetype.Idol);
            gourmand.Base = idol.Base;

            Assert.That(
                MaidMath.Derive(gourmand, AtStation()).CookSpeed / MaidMath.Derive(idol, AtStation()).CookSpeed,
                Is.EqualTo(1.8f).Within(1e-4f));
        }

        [Test]
        public void TheNeighbourAuraLiftsEverySpeed()
        {
            var maid = Fresh(Archetype.Idol);

            var alone = MaidMath.Derive(maid, new ShiftSnapshot(0f, false, false, 0f, false));
            var beside = MaidMath.Derive(maid, new ShiftSnapshot(0f, false, false, 0.30f, false));

            Assert.That(beside.MoveSpeed / alone.MoveSpeed, Is.EqualTo(1.30f).Within(1e-4f));
            Assert.That(beside.ServeSpeed / alone.ServeSpeed, Is.EqualTo(1.30f).Within(1e-4f));
        }

        [Test]
        public void RushingIsWorthSixtyPercent()
        {
            var maid = Fresh(Archetype.Idol);

            var calm = MaidMath.Derive(maid, new ShiftSnapshot(0f, false, false, 0f, false));
            var rushed = MaidMath.Derive(maid, new ShiftSnapshot(0f, false, false, 0f, true));

            Assert.That(rushed.ServeSpeed / calm.ServeSpeed, Is.EqualTo(1.6f).Within(1e-4f));
        }

        [Test]
        public void ATiredMaidWorksSlowerButNeverStops()
        {
            var maid = Fresh(Archetype.Idol);
            var fresh = MaidMath.Derive(maid, Floor()).ServeSpeed;

            maid.Mood = 0f;
            var spent = MaidMath.Derive(maid, Floor()).ServeSpeed;

            Assert.Less(spent, fresh);
            Assert.Greater(spent, 0f, "a maid at zero mood must still be able to finish a task");
        }

        [Test]
        public void StaminaSlowsTheMoodDrain()
        {
            var weak = Fresh(Archetype.Idol);
            weak.Base = new Stats(50, 50, 50, 1);
            var strong = Fresh(Archetype.Idol);
            strong.Base = new Stats(50, 50, 50, 100);

            Assert.Greater(MaidMath.Derive(weak, Floor()).MoodDrainPerSecond,
                           MaidMath.Derive(strong, Floor()).MoodDrainPerSecond);
        }

        [Test]
        public void GenkiBurnsMoodFasterThanHerStatsAloneExplain()
        {
            var genki = Fresh(Archetype.Genki);
            var idol = Fresh(Archetype.Idol);
            genki.Base = idol.Base;

            Assert.That(
                MaidMath.Derive(genki, Floor()).MoodDrainPerSecond /
                MaidMath.Derive(idol, Floor()).MoodDrainPerSecond,
                Is.EqualTo(1.6f).Within(1e-4f));
        }

        [Test]
        public void EverySpeedStaysPositiveForEveryArchetypeAtEveryExtreme()
        {
            foreach (var profile in ArchetypeTable.All)
            {
                foreach (var stats in new[] { new Stats(1, 1, 1, 1), new Stats(100, 100, 100, 100) })
                {
                    var maid = Fresh(profile.Kind);
                    maid.Base = stats;
                    maid.Mood = 0f;

                    var d = MaidMath.Derive(maid, AtStation(1f));

                    Assert.Greater(d.MoveSpeed, 0f, profile.Kind.ToString());
                    Assert.Greater(d.GreetSpeed, 0f, profile.Kind.ToString());
                    Assert.Greater(d.ServeSpeed, 0f, profile.Kind.ToString());
                    Assert.Greater(d.CookSpeed, 0f, profile.Kind.ToString());
                    Assert.GreaterOrEqual(d.TipMultiplier, 1f, profile.Kind.ToString());
                    Assert.That(d.FumbleChance, Is.InRange(0f, 1f), profile.Kind.ToString());
                }
            }
        }
    }
}
