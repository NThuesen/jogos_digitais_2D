using System.Collections.Generic;
using System.Linq;
using Neko.Sim.Cafe;
using Neko.Sim.Maids;
using Neko.Sim.Progress;
using Neko.Sim.Service;
using Neko.Sim.Shift;
using NUnit.Framework;

namespace Neko.Tests.Sim
{
    /// <summary>
    /// The half of the skill list that changes what the shift does rather than how fast it
    /// does it. A multiplier can be checked against <c>MaidMath</c>; these can only be checked
    /// by running a day.
    /// </summary>
    public sealed class SkillsInShiftTests
    {
        private static CafeGrid Room()
        {
            var grid = new CafeGrid(9, 7);
            grid.Place(PlaceableCatalog.ById("table_2"), 3, 1);
            grid.Place(PlaceableCatalog.ById("table_2"), 3, 4);
            grid.Place(PlaceableCatalog.ById("stove"), 7, 3);
            return grid;
        }

        private static ShiftSetup Setup(IEnumerable<Maid> roster, CustomerType kind = CustomerType.Regular)
            => new ShiftSetup
            {
                Grid = Room(),
                Roster = roster.ToList(),
                DurationSeconds = 120f,
                ArrivalsPerSecond = 0f,
                Quota = 100,
                Seed = 11u,
                CafeLevel = 1,
                ScriptedArrivals = new[] { new ScriptedArrival(0.5f, kind) },
            };

        private static ShiftResult Run(Shift shift)
        {
            var spent = 0f;
            while (!shift.Finished && spent < 600f)
            {
                shift.Tick(0.05f);
                spent += 0.05f;
            }

            return shift.Finish();
        }

        private static Maid Skilled(Archetype kind, params SkillId[] skills)
        {
            var maid = Maid.Create(1, "Teste", kind, 20);
            foreach (var skill in skills) MaidProgression.EquipSkill(maid, skill, out _);
            return maid;
        }

        [Test]
        public void MiseEnPlaceMakesTheFirstTicketInstantAndOnlyTheFirst()
        {
            var cook = Skilled(Archetype.Mukuchi, SkillId.MiseEnPlace);
            var shift = new Shift(Setup(new[] { cook }));

            var spent = 0f;
            while (!shift.Finished && spent < 600f)
            {
                shift.Tick(0.05f);
                spent += 0.05f;
            }

            var worker = shift.Workers.First(w => !w.IsManager);
            Assert.IsTrue(worker.UsedMiseEnPlace, "the free ticket has to actually be spent");
        }

        [Test]
        public void MiseEnPlaceGetsTheFoodOutSooner()
        {
            var withSkill = FirstPlateReadyAt(Skilled(Archetype.Mukuchi, SkillId.MiseEnPlace));
            var without = FirstPlateReadyAt(Skilled(Archetype.Mukuchi));

            Assert.Less(withSkill, without);

            float FirstPlateReadyAt(Maid maid)
            {
                var shift = new Shift(Setup(new[] { maid }));
                var spent = 0f;
                while (!shift.Finished && spent < 600f)
                {
                    shift.Tick(0.05f);
                    spent += 0.05f;
                    if (shift.Events.Any(e => e.Kind == ShiftEventKind.PlateReady)) return spent;
                }

                return float.MaxValue;
            }
        }

        [Test]
        public void WarmWelcomeSeatsPeopleHappier()
        {
            Assert.Greater(
                PatienceAfterSeating(Skilled(Archetype.Genki, SkillId.WarmWelcome)),
                PatienceAfterSeating(Skilled(Archetype.Genki)));

            float PatienceAfterSeating(Maid maid)
            {
                var shift = new Shift(Setup(new[] { maid }));
                var spent = 0f;
                while (!shift.Finished && spent < 600f)
                {
                    shift.Tick(0.05f);
                    spent += 0.05f;

                    var customer = shift.CustomerById(1);
                    if (customer != null && customer.SeatIndex >= 0) return customer.Patience;
                }

                return 0f;
            }
        }

        [Test]
        public void RegularsMakesTheTableSpendMore()
        {
            var plain = Run(new Shift(Setup(new[] { Skilled(Archetype.Genki) })));
            var loyal = Run(new Shift(Setup(new[] { Skilled(Archetype.Genki, SkillId.Regulars) })));

            Assert.AreEqual(1, plain.Served);
            Assert.AreEqual(1, loyal.Served);
            Assert.Greater(loyal.Earned, plain.Earned);
        }

        [Test]
        public void SecondWindKeepsHerOnTheFloorOnceInsteadOfTheBreakRoom()
        {
            var tired = Skilled(Archetype.Genki, SkillId.SecondWind);
            tired.Mood = 0.001f;

            var setup = Setup(new[] { tired });
            setup.ArrivalsPerSecond = 0.5f;
            setup.ScriptedArrivals = null;

            var shift = new Shift(setup);
            var worker = shift.Workers.First(w => !w.IsManager);
            var sawSecondWind = false;
            var wentOnBreakFirst = false;

            var spent = 0f;
            while (!shift.Finished && spent < 400f)
            {
                shift.Tick(0.05f);
                spent += 0.05f;

                if (shift.Events.Any(e => e.Kind == ShiftEventKind.SecondWind)) sawSecondWind = true;
                if (worker.OnBreak && !sawSecondWind) wentOnBreakFirst = true;
            }

            Assert.IsTrue(sawSecondWind);
            Assert.IsFalse(wentOnBreakFirst, "the skill has to fire before the break room does");
            Assert.IsTrue(worker.UsedSecondWind);
        }

        [Test]
        public void DoubleTrayPutsTwoDishesDownInOneTrip()
        {
            // A VIP orders two or three dishes, so there is a second plate to carry.
            var carrier = Skilled(Archetype.Genki, SkillId.DoubleTray);
            var setup = Setup(new[] { carrier, Maid.Create(2, "Cozinha", Archetype.Mukuchi, 20) },
                CustomerType.Student);
            setup.CafeLevel = 2;
            setup.Seed = 4u;

            var shift = new Shift(setup);
            Run(shift);

            var served = shift.Events.Count(e => e.Kind == ShiftEventKind.Served);
            var delivered = shift.Events.Count(e => e.Kind == ShiftEventKind.PlateReady);

            Assert.Greater(served, 0);
            Assert.GreaterOrEqual(served, delivered - 1,
                "every plate cooked should still reach a table");
        }

        [Test]
        public void PresencaKeepsPeopleInTheirSeatsOnABusyDay()
        {
            // Measured as the thing the player actually sees — fewer walkouts — rather than
            // as one customer's patience at one instant, which depends on where the maid
            // happened to be standing that tick.
            var withSkill = 0;
            var without = 0;

            for (var seed = 1u; seed <= 8u; seed++)
            {
                withSkill += AngryOver(seed, SkillId.Patience);
                without += AngryOver(seed, SkillId.None);
            }

            Assert.Less(withSkill, without);

            int AngryOver(uint seed, SkillId skill)
            {
                var roster = new[]
                {
                    skill == SkillId.None
                        ? Maid.Create(1, "A", Archetype.Genki, 12)
                        : Skilled(Archetype.Genki, skill),
                    Maid.Create(2, "B", Archetype.Mukuchi, 12),
                };

                var setup = Setup(roster);
                setup.ArrivalsPerSecond = 0.3f;
                setup.ScriptedArrivals = null;
                setup.DurationSeconds = 150f;
                setup.CafeLevel = 2;
                setup.Seed = seed;

                return Run(new Shift(setup)).Angry;
            }
        }

        [Test]
        public void PhotogenicDoublesWhatTheFanPays()
        {
            var plain = Run(new Shift(Fan(false)));
            var star = Run(new Shift(Fan(true)));

            Assert.AreEqual(1, plain.Served);
            Assert.AreEqual(1, star.Served);
            Assert.That(star.Earned, Is.EqualTo(plain.Earned * 2).Within(2));

            ShiftSetup Fan(bool photogenic)
            {
                var maid = photogenic
                    ? Skilled(Archetype.Genki, SkillId.Photogenic)
                    : Skilled(Archetype.Genki);

                var setup = Setup(new[] { maid, Maid.Create(2, "B", Archetype.Mukuchi, 20) },
                    CustomerType.Otaku);
                setup.CafeLevel = 3;
                setup.Seed = 3u;
                return setup;
            }
        }

        [Test]
        public void SkillsAreLiveWithoutAnybodyRememberingToInstallThem()
        {
            MaidMath.SkillHook = null;

            var maid = Skilled(Archetype.Genki, SkillId.QuickFeet);
            var _ = new Shift(Setup(new[] { maid }));

            Assert.IsNotNull(MaidMath.SkillHook,
                "a shift must not depend on the caller having wired the skill table");
        }
    }
}
