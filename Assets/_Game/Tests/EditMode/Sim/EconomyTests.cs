using System.Linq;
using Neko.Sim.Core;
using Neko.Sim.Economy;
using Neko.Sim.Maids;
using Neko.Sim.Progress;
using Neko.Sim.Save;
using Neko.Sim.Shift;
using NUnit.Framework;

namespace Neko.Tests.Sim
{
    public sealed class WalletTests
    {
        [Test]
        public void SpendingExactlyTheBalanceSucceedsAndLeavesZero()
        {
            var wallet = new Wallet(100);

            Assert.IsTrue(wallet.Spend(100));
            Assert.AreEqual(0, wallet.Money);
        }

        [Test]
        public void SpendingOneMoreThanTheBalanceIsRefusedAndChangesNothing()
        {
            var wallet = new Wallet(100);

            Assert.IsFalse(wallet.Spend(101));
            Assert.AreEqual(100, wallet.Money);
        }

        [Test]
        public void MoneyNeverGoesNegative()
        {
            var wallet = new Wallet(-500);

            Assert.AreEqual(0, wallet.Money);
            Assert.IsFalse(wallet.Spend(1));
        }

        [Test]
        public void SpendingNothingIsFreeAndSpendingANegativeIsRefused()
        {
            var wallet = new Wallet(10);

            Assert.IsTrue(wallet.Spend(0));
            Assert.IsFalse(wallet.Spend(-5), "a negative price would be a way to print money");
            Assert.AreEqual(10, wallet.Money);
        }

        [Test]
        public void EarningNothingOrLessChangesNothing()
        {
            var wallet = new Wallet(10);
            wallet.Earn(0);
            wallet.Earn(-50);

            Assert.AreEqual(10, wallet.Money);
        }

        [Test]
        public void ReputationOnlyEverClimbs()
        {
            var wallet = new Wallet(0, 100);
            wallet.AddReputation(-60);

            Assert.AreEqual(100, wallet.Reputation, "a bad day must not take away a level");

            wallet.AddReputation(25);
            Assert.AreEqual(125, wallet.Reputation);
        }
    }

    public sealed class CafeLevelTests
    {
        [Test]
        public void LevelRisesWithReputationAndStartsAtOne()
        {
            Assert.AreEqual(1, CafeLevelTable.LevelFor(0));
            Assert.AreEqual(2, CafeLevelTable.LevelFor(CafeLevelTable.ReputationForLevel(2)));
            Assert.AreEqual(CafeLevelTable.MaxLevel,
                CafeLevelTable.LevelFor(CafeLevelTable.ReputationForLevel(10) + 50_000));
        }

        [Test]
        public void TheThresholdsAreStrictlyIncreasingAfterLevelOne()
        {
            var previous = -1;
            for (var level = 2; level <= CafeLevelTable.MaxLevel; level++)
            {
                var needed = CafeLevelTable.ReputationForLevel(level);
                Assert.Greater(needed, previous, "level " + level);
                previous = needed;
            }
        }

        [Test]
        public void TheRoomRosterAndShiftOnlyEverGrow()
        {
            var previous = CafeLevelTable.UnlocksAt(1);

            for (var level = 2; level <= CafeLevelTable.MaxLevel; level++)
            {
                var now = CafeLevelTable.UnlocksAt(level);

                Assert.GreaterOrEqual(now.GridWidth, previous.GridWidth, "level " + level);
                Assert.GreaterOrEqual(now.GridHeight, previous.GridHeight, "level " + level);
                Assert.GreaterOrEqual(now.RosterCap, previous.RosterCap, "level " + level);
                Assert.GreaterOrEqual(now.ShiftSlots, previous.ShiftSlots, "level " + level);
                Assert.LessOrEqual(now.ShiftSlots, now.RosterCap,
                    "level " + level + " would roster more maids than it can hire");

                previous = now;
            }
        }

        [Test]
        public void EveryLevelAfterTheFirstBringsSomething()
        {
            for (var level = 2; level <= CafeLevelTable.MaxLevel; level++)
            {
                var now = CafeLevelTable.UnlocksAt(level);
                var before = CafeLevelTable.UnlocksAt(level - 1);

                var grew = now.GridWidth > before.GridWidth || now.GridHeight > before.GridHeight ||
                           now.RosterCap > before.RosterCap || now.ShiftSlots > before.ShiftSlots;

                Assert.IsTrue(grew || now.NewThisLevel().Count > 0,
                    "café level " + level + " unlocks nothing at all");
            }
        }

        [Test]
        public void ToNextLevelCountsDownAndIsZeroAtTheCap()
        {
            Assert.AreEqual(CafeLevelTable.ReputationForLevel(2), CafeLevelTable.ToNextLevel(0));
            Assert.AreEqual(0,
                CafeLevelTable.ToNextLevel(CafeLevelTable.ReputationForLevel(10) + 1));
        }

        [Test]
        public void TheStartingRoomFitsInsideTheStartingUnlocks()
        {
            var unlocks = CafeLevelTable.UnlocksAt(1);
            var grid = Neko.Sim.Cafe.CafeGrid.StartingRoom();

            Assert.LessOrEqual(grid.Width, unlocks.GridWidth);
            Assert.LessOrEqual(grid.Height, unlocks.GridHeight);
        }
    }

    public sealed class DayPlanTests
    {
        [Test]
        public void TheQuotaIsKeyedOnTheCafeLevel()
        {
            Assert.AreEqual(95, DayPlan.QuotaFor(1, 1));
            Assert.AreEqual((int)System.Math.Round(95 * System.Math.Pow(5, DayPlan.QuotaExponent)),
                DayPlan.QuotaFor(1, 5));
        }

        [Test]
        public void TheQuotaRisesWithTheLevelAndNudgesWithTheStep()
        {
            var previous = 0;
            for (var level = 1; level <= CafeLevelTable.MaxLevel; level++)
            {
                var quota = DayPlan.QuotaFor(1, level);
                Assert.Greater(quota, previous, "level " + level);
                previous = quota;
            }

            Assert.Greater(DayPlan.QuotaFor(10, 3), DayPlan.QuotaFor(1, 3));
        }

        [Test]
        public void ARepeatedDayGetsOnlyALittleHarder()
        {
            // The step climb is a nudge, not a curve. A player parked at one café level
            // should be able to keep earning, not be squeezed out of their own save.
            Assert.Less(DayPlan.QuotaFor(10, 2) / (float)DayPlan.QuotaFor(1, 2), 1.25f);
        }

        [Test]
        public void ArrivalsAndPatienceGetHarderEachDay()
        {
            Assert.Greater(DayPlan.ArrivalsPerSecondFor(20, 1, 4),
                DayPlan.ArrivalsPerSecondFor(1, 1, 4));
            Assert.Less(DayPlan.PatienceScaleFor(20), DayPlan.PatienceScaleFor(1));
        }

        [Test]
        public void ABiggerCafeIsBusierAndOpenLonger()
        {
            Assert.Greater(DayPlan.ArrivalsPerSecondFor(1, 8, 14),
                DayPlan.ArrivalsPerSecondFor(1, 1, 4),
                "a room with more seats has to fill them");
            Assert.Greater(DayPlan.DurationFor(8), DayPlan.DurationFor(1));
        }

        [Test]
        public void TheCurvesAreClampedSoDayAThousandIsStillAGame()
        {
            Assert.LessOrEqual(DayPlan.ArrivalsPerSecondFor(1000, 10, 40), 1.1f);
            Assert.GreaterOrEqual(DayPlan.PatienceScaleFor(1000), 0.6f);
            Assert.LessOrEqual(DayPlan.DurationFor(100), 180f);
        }

        [Test]
        public void DayOneIsScriptedAndDayTwoIsNot()
        {
            var profile = Profile.NewGame(1u);

            Assert.IsNotNull(DayPlan.Build(profile, 1u).ScriptedArrivals,
                "the first day teaches the loop; it does not roll dice");

            profile.Day = 2;
            profile.QuotaStep = 2;
            Assert.IsNull(DayPlan.Build(profile, 1u).ScriptedArrivals);
        }

        [Test]
        public void BuildUsesTheProfilesOwnRoomAndRoster()
        {
            var profile = Profile.NewGame(1u);
            var setup = DayPlan.Build(profile, 7u);

            Assert.AreSame(profile.Grid, setup.Grid);
            Assert.AreEqual(profile.ShiftRoster.Count, setup.Roster.Count);
            Assert.AreEqual(7u, setup.Seed);
        }

        [Test]
        public void AMetQuotaAdvancesTheCurveAndAMissedOneDoesNot()
        {
            var met = Profile.NewGame(1u);
            var missed = Profile.NewGame(1u);

            DayPlan.Settle(met, new ShiftResult { Earned = 500, Quota = 120, QuotaMet = true });
            DayPlan.Settle(missed, new ShiftResult { Earned = 10, Quota = 120, QuotaMet = false });

            Assert.AreEqual(2, met.Day);
            Assert.AreEqual(2, missed.Day, "the day always advances");

            Assert.AreEqual(2, met.QuotaStep);
            Assert.AreEqual(1, missed.QuotaStep, "a slow day must not make tomorrow harder");
            Assert.AreEqual(DayPlan.QuotaFor(1, 1), DayPlan.QuotaFor(missed.QuotaStep, 1));
        }

        [Test]
        public void SettlingPaysTheMoneyAndTheReputation()
        {
            var profile = Profile.NewGame(1u);
            var before = profile.Wallet.Money;

            DayPlan.Settle(profile, new ShiftResult
            {
                Earned = 400, Quota = 120, QuotaMet = true, ReputationDelta = 70, Served = 5,
            });

            Assert.AreEqual(before + 400, profile.Wallet.Money);
            Assert.AreEqual(70, profile.Wallet.Reputation);
            Assert.AreEqual(5, profile.Records.TotalCustomersServed);
        }

        [Test]
        public void SettlingLevelsTheMaidsWhoEarnedIt()
        {
            var profile = Profile.NewGame(1u);
            var maid = profile.Maids[0];
            var result = new ShiftResult { Earned = 400, Quota = 120, QuotaMet = true };
            result.XpByMaid[maid.Id] = MaidMath.XpForLevel(4);

            DayPlan.Settle(profile, result);

            Assert.AreEqual(4, maid.Level);
        }

        [Test]
        public void SettlingGrowsTheRoomOnceTheReputationIsThere()
        {
            var profile = Profile.NewGame(1u);
            var before = profile.Grid.Width;

            DayPlan.Settle(profile, new ShiftResult
            {
                Earned = 9999, Quota = 1, QuotaMet = true,
                ReputationDelta = CafeLevelTable.ReputationForLevel(4),
            });

            Assert.Greater(profile.Grid.Width, before);
            Assert.AreEqual(CafeLevelTable.UnlocksAt(4).GridWidth, profile.Grid.Width);
        }

        [Test]
        public void SettlingANullResultIsIgnoredRatherThanThrown()
        {
            Assert.DoesNotThrow(() => DayPlan.Settle(Profile.NewGame(1u), null));
            Assert.DoesNotThrow(() => DayPlan.Settle(null, new ShiftResult()));
        }

        [Test]
        public void EndlessUnlocksAfterTwentyGoodDays()
        {
            var profile = Profile.NewGame(1u);

            for (var i = 0; i < 20; i++)
            {
                DayPlan.Settle(profile, new ShiftResult { Earned = 99999, QuotaMet = true });
            }

            Assert.IsTrue(profile.EndlessUnlocked);
        }
    }

    public sealed class ShopTests
    {
        private static Profile Rich(int money = 100000)
        {
            var profile = Profile.NewGame(1u);
            profile.Wallet.Earn(money);
            return profile;
        }

        [Test]
        public void StockGrowsWithTheCafeLevel()
        {
            Assert.Greater(Shop.StockFor(10).Count, Shop.StockFor(1).Count);
        }

        [Test]
        public void EveryItemHasANameAPriceAndADescription()
        {
            foreach (var item in Shop.StockFor(CafeLevelTable.MaxLevel))
            {
                Assert.IsNotEmpty(item.DisplayName, item.Id);
                Assert.IsNotEmpty(item.Description, item.Id);
                Assert.GreaterOrEqual(item.Price, 0, item.Id);
            }
        }

        [Test]
        public void BuyingALockedItemIsRefusedWithAReason()
        {
            var profile = Rich();

            var result = Shop.Buy(profile, "bakery");

            Assert.IsFalse(result.Ok);
            Assert.IsNotEmpty(result.Refusal);
        }

        [Test]
        public void BuyingSomethingThatDoesNotExistIsRefusedNotThrown()
        {
            Assert.IsFalse(Shop.Buy(Rich(), "nao_existe").Ok);
            Assert.IsFalse(Shop.Buy(null, "plant").Ok);
        }

        [Test]
        public void BuyingFurniturePutsItInTheInventoryAndChargesOnce()
        {
            var profile = Rich();
            var price = Shop.Find(profile.CafeLevel, "plant").Price;
            var before = profile.Wallet.Money;

            Assert.IsTrue(Shop.Buy(profile, "plant").Ok);

            Assert.AreEqual(before - price, profile.Wallet.Money);
            CollectionAssert.Contains(profile.OwnedPlaceables, "plant");
        }

        [Test]
        public void ARefusedPurchaseCostsNothing()
        {
            var profile = Profile.NewGame(1u);
            var before = profile.Wallet.Money;

            Assert.IsFalse(Shop.Buy(profile, "stage").Ok);

            Assert.AreEqual(before, profile.Wallet.Money);
        }

        [Test]
        public void BuyingWithExactlyEnoughMoneySucceeds()
        {
            var profile = Profile.NewGame(1u);
            var price = Shop.Find(1, "plant").Price;
            profile.Wallet.Restore(price, 0);

            Assert.IsTrue(Shop.Buy(profile, "plant").Ok);
            Assert.AreEqual(0, profile.Wallet.Money);
        }

        [Test]
        public void BuyingOneCoinShortIsRefused()
        {
            var profile = Profile.NewGame(1u);
            profile.Wallet.Restore(Shop.Find(1, "plant").Price - 1, 0);

            var result = Shop.Buy(profile, "plant");

            Assert.IsFalse(result.Ok);
            StringAssert.Contains("1", result.Refusal);
        }

        [Test]
        public void AGiftNeedsSomebodyToGiveItTo()
        {
            var profile = Rich();

            Assert.IsFalse(Shop.Buy(profile, "gift_small").Ok);
            Assert.IsFalse(Shop.Buy(profile, "gift_small", 9999).Ok);
        }

        [Test]
        public void AGiftRaisesBondAndChargesOnce()
        {
            var profile = Rich();
            var maid = profile.Maids[0];
            var before = profile.Wallet.Money;

            Assert.IsTrue(Shop.Buy(profile, "gift_small", maid.Id).Ok);

            Assert.AreEqual(8, maid.BondPoints);
            Assert.AreEqual(before - 80, profile.Wallet.Money);
        }

        [Test]
        public void ClothesGoIntoTheWardrobeRatherThanOntoAMaid()
        {
            var profile = Rich();

            Assert.IsTrue(Shop.Buy(profile, "head_band").Ok);

            CollectionAssert.Contains(profile.OwnedEquipment, "head_band");
            Assert.IsTrue(profile.Maids.All(m => m.Gear.All(g => g == null)));
        }
    }

    public sealed class ApplicantBoardTests
    {
        [Test]
        public void RefreshAlwaysOffersThreeCandidates()
        {
            var board = new ApplicantBoard();
            board.Refresh(new Rng(1), 1, 10);

            Assert.AreEqual(ApplicantBoard.Slots, board.Candidates.Count);
            Assert.IsTrue(board.Candidates.All(c => c != null && c.Maid != null && c.Price > 0));
        }

        [Test]
        public void ABetterCandidateCostsMore()
        {
            var rookie = Maid.Create(1, "A", Archetype.Genki);
            var veteran = Maid.Create(2, "B", Archetype.Genki, 15);

            Assert.Greater(ApplicantBoard.PriceOf(veteran), ApplicantBoard.PriceOf(rookie));
        }

        [Test]
        public void CandidatesArriveBelowTheCafeTheyApplyTo()
        {
            var board = new ApplicantBoard();

            for (var seed = 1u; seed < 60u; seed++)
            {
                board.Refresh(new Rng(seed), 4, 10);
                foreach (var candidate in board.Candidates)
                {
                    Assert.That(candidate.Maid.Level, Is.InRange(1, 3),
                        "hiring must not be a shortcut past levelling the roster");
                }
            }
        }

        [Test]
        public void RefreshIsDeterministic()
        {
            var first = new ApplicantBoard();
            var second = new ApplicantBoard();
            first.Refresh(new Rng(42), 3, 10);
            second.Refresh(new Rng(42), 3, 10);

            CollectionAssert.AreEqual(
                first.Candidates.Select(c => c.Maid.Archetype).ToList(),
                second.Candidates.Select(c => c.Maid.Archetype).ToList());
        }

        [Test]
        public void HiringTakesTheMoneyAndAddsHerToTheRoster()
        {
            var profile = Profile.NewGame(1u);
            profile.Wallet.Earn(50000);
            profile.Wallet.AddReputation(CafeLevelTable.ReputationForLevel(5));
            var board = new ApplicantBoard();
            board.Refresh(new Rng(3), 5, profile.NextMaidId);

            var before = profile.Maids.Count;
            var price = board.Candidates[0].Price;
            var money = profile.Wallet.Money;

            var result = board.Hire(profile, 0);

            Assert.IsTrue(result.Ok, result.Refusal);
            Assert.AreEqual(before + 1, profile.Maids.Count);
            Assert.AreEqual(money - price, profile.Wallet.Money);
            Assert.IsNull(board.Candidates[0], "a hired candidate leaves the board");
        }

        [Test]
        public void HiringTwiceFromTheSameSlotIsRefused()
        {
            var profile = Profile.NewGame(1u);
            profile.Wallet.Earn(50000);
            profile.Wallet.AddReputation(CafeLevelTable.ReputationForLevel(5));
            var board = new ApplicantBoard();
            board.Refresh(new Rng(3), 5, profile.NextMaidId);
            board.Hire(profile, 0);

            Assert.IsFalse(board.Hire(profile, 0).Ok);
        }

        [Test]
        public void HiringOverTheRosterCapIsRefusedWithAReason()
        {
            var profile = Profile.NewGame(1u);
            profile.Wallet.Earn(50000);
            var board = new ApplicantBoard();
            board.Refresh(new Rng(3), 1, profile.NextMaidId);

            // Café level 1 holds two maids, and a new game already has two.
            var result = board.Hire(profile, 0);

            Assert.IsFalse(result.Ok);
            StringAssert.Contains("2", result.Refusal);
        }

        [Test]
        public void HiringWithoutTheMoneyIsRefusedAndCostsNothing()
        {
            var profile = Profile.NewGame(1u);
            profile.Wallet.AddReputation(CafeLevelTable.ReputationForLevel(5));
            var before = profile.Wallet.Money;
            var board = new ApplicantBoard();
            board.Refresh(new Rng(3), 5, profile.NextMaidId);

            var result = board.Hire(profile, 0);

            Assert.IsFalse(result.Ok);
            Assert.AreEqual(before, profile.Wallet.Money);
        }

        [Test]
        public void HiringAnInvalidSlotIsRefusedNotThrown()
        {
            var board = new ApplicantBoard();
            board.Refresh(new Rng(1), 1, 1);

            Assert.IsFalse(board.Hire(Profile.NewGame(1u), -1).Ok);
            Assert.IsFalse(board.Hire(Profile.NewGame(1u), 99).Ok);
            Assert.IsFalse(board.Hire(null, 0).Ok);
        }

        [Test]
        public void EveryArchetypeCanTurnUpOnTheBoard()
        {
            var board = new ApplicantBoard();
            var seen = new System.Collections.Generic.HashSet<Archetype>();

            for (var seed = 1u; seed < 400u; seed++)
            {
                board.Refresh(new Rng(seed), 5, 1);
                foreach (var candidate in board.Candidates) seen.Add(candidate.Maid.Archetype);
            }

            CollectionAssert.AreEquivalent(ArchetypeTable.All.Select(p => p.Kind), seen);
        }
    }

    public sealed class ProfileTests
    {
        [Test]
        public void ANewGameStartsWithAFloorMaidAndAKitchenMaid()
        {
            var profile = Profile.NewGame(1u);

            Assert.AreEqual(2, profile.Maids.Count);
            CollectionAssert.Contains(profile.Maids.Select(m => m.Archetype), Archetype.Genki);
            CollectionAssert.Contains(profile.Maids.Select(m => m.Archetype), Archetype.Mukuchi);
            Assert.AreEqual(2, profile.ShiftRoster.Count);
        }

        [Test]
        public void ANewGameCanAffordSomethingButNotEverything()
        {
            var profile = Profile.NewGame(1u);
            var stock = Shop.StockFor(profile.CafeLevel);

            Assert.IsTrue(stock.Any(i => profile.Wallet.CanAfford(i.Price)));
            Assert.IsTrue(stock.Any(i => !profile.Wallet.CanAfford(i.Price)));
        }

        [Test]
        public void CafeLevelIsAlwaysWhatTheReputationSays()
        {
            var profile = Profile.NewGame(1u);
            Assert.AreEqual(1, profile.CafeLevel);

            profile.Wallet.AddReputation(CafeLevelTable.ReputationForLevel(5));
            Assert.AreEqual(5, profile.CafeLevel);
        }

        [Test]
        public void TheRosterCannotGrowPastTheShiftSlots()
        {
            var profile = Profile.NewGame(1u);
            var extra = Maid.Create(99, "Extra", Archetype.Idol);
            profile.Maids.Add(extra);

            Assert.IsFalse(profile.SetRostered(99, true), "café level 1 fields two maids");

            profile.Wallet.AddReputation(CafeLevelTable.ReputationForLevel(3));
            Assert.IsTrue(profile.SetRostered(99, true));
        }

        [Test]
        public void BenchingAndRosteringAreBothReversible()
        {
            var profile = Profile.NewGame(1u);
            var id = profile.Maids[0].Id;

            Assert.IsTrue(profile.SetRostered(id, false));
            CollectionAssert.DoesNotContain(profile.ShiftRoster, id);

            Assert.IsTrue(profile.SetRostered(id, true));
            CollectionAssert.Contains(profile.ShiftRoster, id);
        }

        [Test]
        public void RosteringSomebodyWhoDoesNotWorkHereIsRefused()
        {
            Assert.IsFalse(Profile.NewGame(1u).SetRostered(12345, true));
        }

        [Test]
        public void ApplyUnlocksNeverShrinksTheRoom()
        {
            var profile = Profile.NewGame(1u);
            profile.Grid.Resize(13, 9);

            profile.ApplyUnlocks();

            Assert.AreEqual(13, profile.Grid.Width, "a room must never be taken away");
        }
    }
}
