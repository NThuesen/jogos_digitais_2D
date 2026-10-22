using System;
using System.Linq;
using Neko.Sim.Core;
using Neko.Sim.Maids;
using Neko.Sim.Progress;
using NUnit.Framework;

namespace Neko.Tests.Sim
{
    public sealed class SkillTests
    {
        [SetUp]
        public void Install() => SkillTable.Install();

        [TearDown]
        public void Uninstall() => MaidMath.SkillHook = null;

        private static Maid Maid20(Archetype kind = Archetype.Genki) =>
            Maid.Create(1, "Teste", kind, 20);

        [Test]
        public void ThereAreSixSkillsPerBranchAndEighteenInAll()
        {
            Assert.AreEqual(18, SkillTable.All.Count);

            foreach (SkillBranch branch in Enum.GetValues(typeof(SkillBranch)))
            {
                Assert.AreEqual(6, SkillTable.Branch(branch).Count, branch.ToString());
            }
        }

        [Test]
        public void EverySkillIdExceptNoneIsInTheTable()
        {
            foreach (SkillId id in Enum.GetValues(typeof(SkillId)))
            {
                if (id == SkillId.None) continue;
                Assert.DoesNotThrow(() => SkillTable.ById(id), id.ToString());
            }
        }

        [Test]
        public void EverySkillHasAPortugueseDescription()
        {
            foreach (var def in SkillTable.All)
            {
                Assert.IsNotEmpty(def.DisplayName, def.Id.ToString());
                Assert.IsNotEmpty(def.Description, def.Id.ToString());
                Assert.That(def.RequiredLevel, Is.InRange(1, Maid.MaxLevel), def.Id.ToString());
            }
        }

        [Test]
        public void SlotsOpenEveryFourthLevelToThreeAtTwelve()
        {
            Assert.AreEqual(0, SkillTable.SlotsAt(1));
            Assert.AreEqual(1, SkillTable.SlotsAt(4));
            Assert.AreEqual(2, SkillTable.SlotsAt(8));
            Assert.AreEqual(3, SkillTable.SlotsAt(12));
            Assert.AreEqual(3, SkillTable.SlotsAt(20), "three is the cap");
        }

        [Test]
        public void EquippingAFourthSkillIsRefusedWithAReason()
        {
            var maid = Maid20();
            MaidProgression.EquipSkill(maid, SkillId.QuickFeet, out _);
            MaidProgression.EquipSkill(maid, SkillId.Usher, out _);
            MaidProgression.EquipSkill(maid, SkillId.Prep, out _);

            Assert.IsFalse(MaidProgression.EquipSkill(maid, SkillId.SweetTalk, out var refusal));
            Assert.IsNotEmpty(refusal);
            Assert.AreEqual(3, maid.Skills.Count);
        }

        [Test]
        public void ASkillAboveTheMaidsLevelIsRefused()
        {
            var rookie = Maid.Create(1, "Teste", Archetype.Genki, 4);

            Assert.IsFalse(MaidProgression.EquipSkill(rookie, SkillId.Multitask, out var refusal));
            StringAssert.Contains("16", refusal);
        }

        [Test]
        public void TheSameSkillCannotBeTakenTwice()
        {
            var maid = Maid20();
            MaidProgression.EquipSkill(maid, SkillId.QuickFeet, out _);

            Assert.IsFalse(MaidProgression.EquipSkill(maid, SkillId.QuickFeet, out _));
            Assert.AreEqual(1, maid.Skills.Count);
        }

        [Test]
        public void AvailableToHidesWhatSheAlreadyKnowsAndWhatSheCannotLearn()
        {
            var maid = Maid.Create(1, "Teste", Archetype.Genki, 8);
            MaidProgression.EquipSkill(maid, SkillId.QuickFeet, out _);

            var available = SkillTable.AvailableTo(maid).Select(d => d.Id).ToList();

            CollectionAssert.DoesNotContain(available, SkillId.QuickFeet);
            CollectionAssert.DoesNotContain(available, SkillId.Multitask);
            CollectionAssert.Contains(available, SkillId.Usher);
        }

        [Test]
        public void ForgettingASkillFreesTheSlot()
        {
            var maid = Maid20();
            MaidProgression.EquipSkill(maid, SkillId.QuickFeet, out _);

            Assert.IsTrue(MaidProgression.ForgetSkill(maid, SkillId.QuickFeet));
            Assert.IsFalse(MaidProgression.ForgetSkill(maid, SkillId.QuickFeet));
            Assert.IsEmpty(maid.Skills);
        }

        [Test]
        public void QuickFeetShowsUpInTheMoveSpeed()
        {
            var plain = Maid20();
            var swift = Maid20();
            MaidProgression.EquipSkill(swift, SkillId.QuickFeet, out _);

            var moment = new ShiftSnapshot(0f, WorkKind.Deliver);

            Assert.That(
                MaidMath.Derive(swift, moment).MoveSpeed / MaidMath.Derive(plain, moment).MoveSpeed,
                Is.EqualTo(1.15f).Within(1e-4f));
        }

        [Test]
        public void TableSenseOnlyHelpsWithOrders()
        {
            var maid = Maid20();
            MaidProgression.EquipSkill(maid, SkillId.TableSense, out _);
            var plain = Maid20();

            Assert.Greater(
                MaidMath.Derive(maid, new ShiftSnapshot(0f, WorkKind.TakeOrder)).ServeSpeed,
                MaidMath.Derive(plain, new ShiftSnapshot(0f, WorkKind.TakeOrder)).ServeSpeed);

            Assert.That(
                MaidMath.Derive(maid, new ShiftSnapshot(0f, WorkKind.Bus)).ServeSpeed,
                Is.EqualTo(MaidMath.Derive(plain, new ShiftSnapshot(0f, WorkKind.Bus)).ServeSpeed)
                    .Within(1e-4f));
        }

        [Test]
        public void BatchCookingOnlyPaysWhenTheRailIsBackedUp()
        {
            var maid = Maid20();
            MaidProgression.EquipSkill(maid, SkillId.BatchCooking, out _);

            var quiet = MaidMath.Derive(maid, new ShiftSnapshot(0f, WorkKind.Cook, 0f, false, 2));
            var busy = MaidMath.Derive(maid, new ShiftSnapshot(0f, WorkKind.Cook, 0f, false, 3));

            Assert.That(busy.CookSpeed / quiet.CookSpeed, Is.EqualTo(1.35f).Within(1e-4f));
        }

        [Test]
        public void SteadyHandsHalvesTheFumbleChance()
        {
            var plain = Maid.Create(1, "Teste", Archetype.Genki, 20);
            plain.Base = new Stats(50, 20, 50, 50);
            var steady = Maid.Create(1, "Teste", Archetype.Genki, 20);
            steady.Base = plain.Base;
            MaidProgression.EquipSkill(steady, SkillId.SteadyHands, out _);

            var moment = new ShiftSnapshot(0f, WorkKind.Cook);

            Assert.That(
                MaidMath.Derive(steady, moment).FumbleChance,
                Is.EqualTo(MaidMath.Derive(plain, moment).FumbleChance * 0.5f).Within(1e-5f));
        }

        [Test]
        public void SweetTalkAndPlatingBothRaiseTheTip()
        {
            var maid = Maid20();
            MaidProgression.EquipSkill(maid, SkillId.SweetTalk, out _);
            MaidProgression.EquipSkill(maid, SkillId.Plating, out _);
            var plain = Maid20();

            var moment = new ShiftSnapshot(0f, WorkKind.Deliver);

            Assert.That(
                MaidMath.Derive(maid, moment).TipMultiplier,
                Is.EqualTo(MaidMath.Derive(plain, moment).TipMultiplier * 1.15f * 1.15f)
                    .Within(1e-4f));
        }

        [Test]
        public void AMaidWithNoSkillsIsUnaffectedByTheHook()
        {
            var maid = Maid20();
            var moment = new ShiftSnapshot(0.5f, WorkKind.Cook);

            var withHook = MaidMath.Derive(maid, moment);
            MaidMath.SkillHook = null;
            var without = MaidMath.Derive(maid, moment);

            Assert.AreEqual(without.CookSpeed, withHook.CookSpeed);
        }

        [Test]
        public void HasReportsWhatTheShiftNeedsToKnow()
        {
            var maid = Maid20();
            MaidProgression.EquipSkill(maid, SkillId.DoubleTray, out _);

            Assert.IsTrue(SkillTable.Has(maid, SkillId.DoubleTray));
            Assert.IsFalse(SkillTable.Has(maid, SkillId.MiseEnPlace));
            Assert.IsFalse(SkillTable.Has(null, SkillId.DoubleTray));
        }
    }

    public sealed class EquipmentTests
    {
        private static Maid Fresh() => Maid.Create(1, "Teste", Archetype.Genki, 5);

        [Test]
        public void TheCatalogIsConsistent()
        {
            foreach (var item in EquipmentCatalog.All)
            {
                Assert.IsNotEmpty(item.DisplayName, item.Id);
                Assert.IsNotEmpty(item.LayerShapeId, item.Id);
                Assert.IsTrue(item.TintHex.StartsWith("#"), item.Id);
                Assert.GreaterOrEqual(item.Price, 0, item.Id);
                Assert.GreaterOrEqual(item.UnlockCafeLevel, 1, item.Id);
            }

            CollectionAssert.AllItemsAreUnique(EquipmentCatalog.All.Select(i => i.Id));
        }

        [Test]
        public void EverySlotHasSomethingAtCafeLevelOne()
        {
            foreach (GearSlot slot in Enum.GetValues(typeof(GearSlot)))
            {
                Assert.IsNotEmpty(EquipmentCatalog.ForSlot(slot, 1), slot.ToString());
            }
        }

        [Test]
        public void EveryThemeHasAFullSetSoASetBonusIsReachable()
        {
            foreach (ThemeTag theme in Enum.GetValues(typeof(ThemeTag)))
            {
                if (theme == ThemeTag.None) continue;

                var slots = EquipmentCatalog.All
                    .Where(i => i.Theme == theme)
                    .Select(i => i.Slot)
                    .Distinct()
                    .Count();

                Assert.AreEqual(4, slots, theme + " cannot be worn as a complete set");
            }
        }

        [Test]
        public void EquippingChangesTotalStatsAndHandsBackWhatCameOff()
        {
            var maid = Fresh();
            var before = maid.Total.Charm;
            var hat = EquipmentCatalog.ById("head_sakura");

            Assert.IsTrue(MaidProgression.Equip(maid, hat, out var removed));

            Assert.IsNull(removed);
            Assert.AreEqual(before + hat.Bonus.Charm, maid.Total.Charm);

            Assert.IsTrue(MaidProgression.Equip(maid, EquipmentCatalog.ById("head_band"), out removed));
            Assert.AreSame(hat, removed, "the old hat has to come back to the wardrobe");
        }

        [Test]
        public void UnequippingAnEmptySlotIsRefusedNotThrown()
        {
            Assert.IsFalse(MaidProgression.Unequip(Fresh(), GearSlot.Shoes, out _));
        }

        [Test]
        public void ThreeMatchingPiecesGrantTheSetBonusAndFourGrantMore()
        {
            var maid = Fresh();
            MaidProgression.Equip(maid, EquipmentCatalog.ById("outfit_sakura"), out _);
            MaidProgression.Equip(maid, EquipmentCatalog.ById("head_sakura"), out _);

            Assert.AreEqual(Stats.Zero, maid.SetBonus, "two is not a set");

            MaidProgression.Equip(maid, EquipmentCatalog.ById("acc_sakura"), out _);
            Assert.AreEqual(4, maid.SetBonus.Serve);
            Assert.AreEqual(ThemeTag.Sakura, EquipmentCatalog.DominantTheme(maid));

            MaidProgression.Equip(maid, EquipmentCatalog.ById("shoes_sakura"), out _);
            Assert.AreEqual(8, maid.SetBonus.Serve, "committing the whole outfit is worth more");
        }

        [Test]
        public void MixingThemesLosesTheSetBonus()
        {
            var maid = Fresh();
            MaidProgression.Equip(maid, EquipmentCatalog.ById("outfit_sakura"), out _);
            MaidProgression.Equip(maid, EquipmentCatalog.ById("head_sakura"), out _);
            MaidProgression.Equip(maid, EquipmentCatalog.ById("acc_sakura"), out _);
            Assert.AreNotEqual(Stats.Zero, maid.SetBonus);

            MaidProgression.Equip(maid, EquipmentCatalog.ById("head_gothic"), out _);

            Assert.AreEqual(Stats.Zero, maid.SetBonus);
        }

        [Test]
        public void UnthemedPiecesNeverCountTowardsASet()
        {
            var maid = Fresh();
            MaidProgression.Equip(maid, EquipmentCatalog.ById("outfit_basic"), out _);
            MaidProgression.Equip(maid, EquipmentCatalog.ById("head_band"), out _);
            MaidProgression.Equip(maid, EquipmentCatalog.ById("acc_apron"), out _);
            MaidProgression.Equip(maid, EquipmentCatalog.ById("shoes_flats"), out _);

            Assert.AreEqual(Stats.Zero, maid.SetBonus);
            Assert.AreEqual(ThemeTag.None, EquipmentCatalog.DominantTheme(maid));
        }

        [Test]
        public void TotalStatsAreStillClampedAtOneHundred()
        {
            var maid = Maid.Create(1, "Teste", Archetype.Genki, 20);
            maid.Base = Stats.Uniform(100);
            MaidProgression.Equip(maid, EquipmentCatalog.ById("outfit_showa"), out _);

            Assert.AreEqual(100, maid.Total.Serve);
        }
    }

    public sealed class BondTests
    {
        private static Maid Fresh() => Maid.Create(1, "Teste", Archetype.Tsundere);

        [Test]
        public void GiftsRaiseBondAndTheThresholdsAreIncreasing()
        {
            var previous = -1;
            for (var level = 0; level <= BondTable.MaxLevel; level++)
            {
                var points = BondTable.PointsForLevel(level);
                Assert.Greater(points, previous, "level " + level);
                previous = points;
            }
        }

        [Test]
        public void AGiftCanCrossSeveralBondLevelsAtOnce()
        {
            var maid = Fresh();

            Assert.IsTrue(MaidProgression.GiveGift(maid, 30, out var gained));

            Assert.AreEqual(2, gained);
            Assert.AreEqual(2, maid.Bond);
        }

        [Test]
        public void BondStopsAtFive()
        {
            var maid = Fresh();
            MaidProgression.GiveGift(maid, 10000, out _);

            Assert.AreEqual(BondTable.MaxLevel, maid.Bond);
            Assert.IsFalse(MaidProgression.GiveGift(maid, 100, out _), "nothing left to buy");
        }

        [Test]
        public void BondRaisesEveryStat()
        {
            var maid = Fresh();
            var before = maid.Total;
            MaidProgression.GiveGift(maid, 150, out _);

            Assert.Greater(maid.Total.Serve, before.Serve);
            Assert.Greater(maid.Total.Cook, before.Cook);
        }

        [Test]
        public void EveryArchetypeHasItsOwnSignatureAndItOnlyArrivesAtBondFive()
        {
            var signatures = Enum.GetValues(typeof(Archetype)).Cast<Archetype>()
                .Select(a => BondTable.SignatureFor(a, BondTable.MaxLevel))
                .ToList();

            Assert.IsTrue(signatures.All(s => !string.IsNullOrEmpty(s)));
            CollectionAssert.AllItemsAreUnique(signatures);

            foreach (Archetype archetype in Enum.GetValues(typeof(Archetype)))
            {
                Assert.IsEmpty(BondTable.SignatureFor(archetype, 4), archetype.ToString());
            }
        }

        [Test]
        public void EveryArchetypeHasALineForEveryBondLevel()
        {
            foreach (Archetype archetype in Enum.GetValues(typeof(Archetype)))
            {
                for (var level = 1; level <= BondTable.MaxLevel; level++)
                {
                    Assert.IsNotEmpty(BondTable.LineFor(archetype, level),
                        archetype + " at bond " + level);
                }
            }
        }

        [Test]
        public void AGiftOfNothingIsRefused()
        {
            Assert.IsFalse(MaidProgression.GiveGift(Fresh(), 0, out _));
            Assert.IsFalse(MaidProgression.GiveGift(null, 10, out _));
        }
    }

    public sealed class MaidProgressionTests
    {
        private static Maid Fresh() => Maid.Create(1, "Teste", Archetype.Mukuchi);

        [Test]
        public void GrantXpCanCrossSeveralLevelsAtOnce()
        {
            var maid = Fresh();

            Assert.IsTrue(MaidProgression.GrantXp(maid, MaidMath.XpForLevel(5), out var gained));

            Assert.AreEqual(4, gained);
            Assert.AreEqual(5, maid.Level);
        }

        [Test]
        public void LevellingRaisesStatsAlongTheArchetypeWeights()
        {
            var maid = Fresh();
            var before = maid.Base;

            MaidProgression.GrantXp(maid, MaidMath.XpForLevel(10), out _);

            Assert.Greater(maid.Base.Cook, before.Cook);
            Assert.Greater(maid.Base.Cook - before.Cook, maid.Base.Serve - before.Serve);
        }

        [Test]
        public void LevelIsCappedAtTwentyAndExcessXpIsKept()
        {
            var maid = Fresh();
            MaidProgression.GrantXp(maid, 10_000_000, out _);

            Assert.AreEqual(Maid.MaxLevel, maid.Level);
            Assert.Greater(maid.Xp, MaidMath.XpForLevel(Maid.MaxLevel));
            Assert.IsFalse(MaidProgression.GrantXp(maid, 500, out var gained));
            Assert.AreEqual(0, gained);
        }

        [Test]
        public void GrantingNoXpChangesNothing()
        {
            var maid = Fresh();

            Assert.IsFalse(MaidProgression.GrantXp(maid, 0, out _));
            Assert.IsFalse(MaidProgression.GrantXp(maid, -50, out _));
            Assert.AreEqual(0, maid.Xp);
        }

        [Test]
        public void MoodRecoversFasterOffShift()
        {
            var worked = Fresh();
            var rested = Fresh();
            worked.Mood = rested.Mood = 0f;

            MaidProgression.RecoverBetweenDays(worked, true);
            MaidProgression.RecoverBetweenDays(rested, false);

            Assert.Greater(rested.Mood, worked.Mood, "benching somebody has to be a real move");
        }

        [Test]
        public void MoodNeverRecoversAboveFull()
        {
            var maid = Fresh();
            maid.Mood = 0.9f;

            MaidProgression.RecoverBetweenDays(maid, false);

            Assert.AreEqual(1f, maid.Mood);
        }

        [Test]
        public void ADayOfWorkIsWorthALittleBondOnItsOwn()
        {
            var maid = Fresh();

            MaidProgression.RecoverBetweenDays(maid, true);

            Assert.AreEqual(MaidProgression.BondPointsPerShift, maid.BondPoints);
        }

        [Test]
        public void SittingOutEarnsNoBond()
        {
            var maid = Fresh();

            MaidProgression.RecoverBetweenDays(maid, false);

            Assert.AreEqual(0, maid.BondPoints);
        }

        [Test]
        public void EveryOperationToleratesANullMaid()
        {
            Assert.DoesNotThrow(() =>
            {
                MaidProgression.GrantXp(null, 10, out _);
                MaidProgression.EquipSkill(null, SkillId.QuickFeet, out _);
                MaidProgression.ForgetSkill(null, SkillId.QuickFeet);
                MaidProgression.Equip(null, null, out _);
                MaidProgression.Unequip(null, GearSlot.Shoes, out _);
                MaidProgression.GiveGift(null, 5, out _);
                MaidProgression.RecoverBetweenDays(null, true);
                MaidProgression.RefreshSetBonus(null);
            });
        }
    }
}
