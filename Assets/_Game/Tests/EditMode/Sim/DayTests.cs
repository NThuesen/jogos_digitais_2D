using System;
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
    /// The day that runs itself.
    /// </summary>
    /// <remarks>
    /// Every one of these builds a lineup, opens the doors and never touches anything again.
    /// That is the whole shape of the game now, and it is also why these tests can be
    /// trusted: there is no player input to model, so a seed really does determine the day.
    /// </remarks>
    public sealed class DayTests
    {
        private static CafeGrid Room()
        {
            var grid = new CafeGrid(9, 7);
            grid.Place(PlaceableCatalog.ById("table_2"), 3, 1);
            grid.Place(PlaceableCatalog.ById("table_2"), 3, 4);
            grid.Place(PlaceableCatalog.ById("stove"), 7, 3);
            return grid;
        }

        private static Maid Card(int id, Archetype archetype, int level = 8)
        {
            var maid = Maid.Create(id, archetype.ToString(), archetype, level);
            maid.House = House.Hanabi;
            maid.Origin = ThemeTag.Sakura;
            return maid;
        }

        /// <summary>A squad with everybody standing where they belong.</summary>
        private static ShiftSetup Setup(out List<Maid> roster, CafeGrid grid = null,
            bool inPosition = true, uint seed = 11u)
        {
            roster = new List<Maid>
            {
                Card(1, Archetype.Ojousama), Card(2, Archetype.Genki), Card(3, Archetype.Mukuchi),
            };

            var lineup = new Lineup(Lineup.SlotsFor(1));

            if (inPosition)
            {
                lineup.Assign(Role.Reception, 0, 1);
                lineup.Assign(Role.Floor, 0, 2);
                lineup.Assign(Role.Kitchen, 0, 3);
            }
            else
            {
                // Everybody somewhere they do not belong.
                lineup.Assign(Role.Reception, 0, 3);
                lineup.Assign(Role.Floor, 0, 1);
                lineup.Assign(Role.Kitchen, 0, 2);
            }

            return new ShiftSetup
            {
                Day = 4,
                CafeLevel = 1,
                Grid = grid ?? Room(),
                Roster = roster,
                Lineup = lineup,
                Chemistry = Chemistry.Evaluate(lineup, roster, ThemeTag.None),
                DurationSeconds = 110f,
                ArrivalsPerSecond = 0.35f,
                Seed = seed,
            };
        }

        private static ShiftResult Run(Shift shift, float maxSeconds = 600f)
        {
            var spent = 0f;
            while (!shift.Finished && spent < maxSeconds)
            {
                shift.Tick(0.05f);
                spent += 0.05f;
            }

            return shift.Finish();
        }

        // -------------------------------------------------------------------------------

        [Test]
        public void ADayRunsToTheEndWithNobodyTouchingIt()
        {
            var result = Run(new Shift(Setup(out _)));

            Assert.Greater(result.Served, 0);
            Assert.Greater(result.Revenue, 0);
            Assert.IsNull(result.FailureReason);
        }

        [Test]
        public void TheSameSeedProducesTheSameDay()
        {
            var first = Run(new Shift(Setup(out _)));
            var second = Run(new Shift(Setup(out _)));

            Assert.AreEqual(first.Revenue, second.Revenue);
            Assert.AreEqual(first.Served, second.Served);
            Assert.AreEqual(first.WrongOrders, second.WrongOrders);
            Assert.AreEqual(first.LeftAngry, second.LeftAngry);
        }

        [Test]
        public void EverySlotStaysInItsOwnLane()
        {
            var shift = new Shift(Setup(out _));

            for (var i = 0; i < 2000 && !shift.Finished; i++)
            {
                shift.Tick(0.05f);

                foreach (var worker in shift.Workers)
                {
                    if (worker.Claimed == null) continue;

                    Assert.IsTrue(worker.Allows(worker.Claimed.Kind),
                        worker.Role + " picked up a " + worker.Claimed.Kind +
                        "; the lineup stops mattering the moment a position wanders");
                }
            }
        }

        [Test]
        public void TheStageAndTheTillNeverTouchATask()
        {
            var roster = new List<Maid>
            {
                Card(1, Archetype.Ojousama), Card(2, Archetype.Genki), Card(3, Archetype.Mukuchi),
                Card(4, Archetype.Idol), Card(5, Archetype.Oneesan),
            };

            var lineup = new Lineup(Lineup.SlotsFor(5));
            lineup.AutoFill(roster);

            var shift = new Shift(new ShiftSetup
            {
                Day = 4, CafeLevel = 2, Grid = Room(), Roster = roster, Lineup = lineup,
                Chemistry = Chemistry.Evaluate(lineup, roster, ThemeTag.None),
                DurationSeconds = 110f, ArrivalsPerSecond = 0.4f, Seed = 5u,
            });

            for (var i = 0; i < 2600 && !shift.Finished; i++)
            {
                shift.Tick(0.05f);

                foreach (var worker in shift.Workers)
                {
                    if (worker.Role != Role.Stage && worker.Role != Role.Management) continue;
                    Assert.IsNull(worker.Claimed, worker.Role + " picked up work");
                }
            }
        }

        [Test]
        public void StandingEverybodyOutOfPositionCostsRealMoney()
        {
            var home = Run(new Shift(Setup(out _)));
            var away = Run(new Shift(Setup(out _, inPosition: false)));

            Assert.Greater(home.Revenue, away.Revenue,
                "a squad in the wrong slots took " + away.Revenue + " against " + home.Revenue);
        }

        [Test]
        public void ChemistryIsWorthAboutWhatItAdvertises()
        {
            var good = 0;
            var bad = 0;

            for (var seed = 1u; seed <= 12u; seed++)
            {
                good += Run(new Shift(Setup(out _, seed: seed))).Revenue;
                bad += Run(new Shift(Setup(out _, inPosition: false, seed: seed))).Revenue;
            }

            Assert.Greater(good, bad * 1.15f,
                "in position " + good + " against " + bad + " out of it");
        }

        [Test]
        public void AWrongPlateCostsTheTableAndIsCounted()
        {
            // A cook standing in the salão. She moves well enough to actually carry plates —
            // a maid with Serve 1 is so slow she never completes a delivery to get wrong —
            // but she is in the wrong position and the squad has almost no chemistry.
            var roster = new List<Maid>
            {
                Card(1, Archetype.Mukuchi, 6), Card(2, Archetype.Mukuchi, 6),
                Card(3, Archetype.Mukuchi, 6),
            };

            foreach (var maid in roster) maid.Base = new Stats(16, 48, 20, 45);

            var lineup = new Lineup(Lineup.SlotsFor(1));
            lineup.Assign(Role.Reception, 0, 3);
            lineup.Assign(Role.Floor, 0, 1);
            lineup.Assign(Role.Kitchen, 0, 2);

            var shift = new Shift(new ShiftSetup
            {
                Day = 6, CafeLevel = 1, Grid = Room(), Roster = roster, Lineup = lineup,
                Chemistry = Chemistry.Evaluate(lineup, roster, ThemeTag.None),
                DurationSeconds = 220f, ArrivalsPerSecond = 0.3f, Seed = 3u,
            });

            var waitress = shift.Workers.First(w => w.Role == Role.Floor);
            Assert.Greater(shift.MistakeChance(waitress), 0.2f,
                "precondition: a cook in the salão is supposed to be unreliable");

            var result = Run(shift, 900f);

            Assert.Greater(result.DishesCooked, 0, "precondition: the kitchen made something");
            Assert.Greater(result.WrongOrders, 0,
                "cooked " + result.DishesCooked + " with " + result.WrongOrders + " mistakes");

            CollectionAssert.Contains(shift.Events.Select(e => e.Kind).ToList(),
                ShiftEventKind.WrongOrder);

            // And the squad is so badly put together that it loses the room entirely, which
            // is the lesson the dashboard is supposed to teach in one screen.
            Assert.AreEqual(0, result.Served);
            Assert.Greater(result.LeftAngry, 5);
        }

        [Test]
        public void AGoodWaitressBarelyEverErrs()
        {
            var roster = new List<Maid>
            {
                Card(1, Archetype.Genki, 20), Card(2, Archetype.Ojousama),
                Card(3, Archetype.Mukuchi),
            };

            roster[0].Base = new Stats(100, 50, 50, 50);
            roster[0].Traits.Add(TraitId.AngelHands);

            var lineup = new Lineup(Lineup.SlotsFor(1));
            lineup.Assign(Role.Reception, 0, 2);
            lineup.Assign(Role.Floor, 0, 1);
            lineup.Assign(Role.Kitchen, 0, 3);

            var shift = new Shift(new ShiftSetup
            {
                Day = 4, CafeLevel = 1, Grid = Room(), Roster = roster, Lineup = lineup,
                Chemistry = Chemistry.Evaluate(lineup, roster, ThemeTag.None),
                Seed = 1u,
            });

            var waitress = shift.Workers.First(w => w.Role == Role.Floor);
            Assert.AreEqual(0f, shift.MistakeChance(waitress));
        }

        [Test]
        public void SatisfactionFallsWhenTheRoomIsTreatedBadly()
        {
            var good = Run(new Shift(Setup(out _)));
            var bad = Run(new Shift(Setup(out _, inPosition: false)));

            Assert.That(good.Satisfaction, Is.InRange(0f, 1f));
            Assert.Greater(good.Satisfaction, bad.Satisfaction);
        }

        [Test]
        public void ADayWithNobodyScaledRefusesToOpenAndSaysSo()
        {
            var roster = new List<Maid>();
            var setup = new ShiftSetup
            {
                Day = 1, CafeLevel = 1, Grid = Room(), Roster = roster,
                Lineup = new Lineup(Lineup.SlotsFor(1)),
                Chemistry = Chemistry.Evaluate(new Lineup(Lineup.SlotsFor(1)), roster, ThemeTag.None),
            };

            var shift = new Shift(setup);

            Assert.IsTrue(shift.Finished);
            StringAssert.Contains("escala", shift.Finish().FailureReason.ToLowerInvariant());
        }

        [Test]
        public void ADayWithNoCookRefusesToOpenAndNamesTheFix()
        {
            var roster = new List<Maid> { Card(1, Archetype.Genki), Card(2, Archetype.Ojousama) };
            var lineup = new Lineup(Lineup.SlotsFor(1));
            lineup.Assign(Role.Reception, 0, 2);
            lineup.Assign(Role.Floor, 0, 1);

            var shift = new Shift(new ShiftSetup
            {
                Day = 1, CafeLevel = 1, Grid = Room(), Roster = roster, Lineup = lineup,
                Chemistry = Chemistry.Evaluate(lineup, roster, ThemeTag.None),
            });

            Assert.IsTrue(shift.Finished);
            StringAssert.Contains("cozinha", shift.Finish().FailureReason.ToLowerInvariant());
        }

        [Test]
        public void ADayWithNoGreeterRefusesToOpen()
        {
            // Without a reception nobody is ever seated and the whole day is a queue at the
            // door. That reads as a broken game rather than a lineup mistake, so it is caught
            // before the doors open and named.
            var roster = new List<Maid> { Card(1, Archetype.Genki), Card(2, Archetype.Mukuchi) };
            var lineup = new Lineup(Lineup.SlotsFor(1));
            lineup.Assign(Role.Floor, 0, 1);
            lineup.Assign(Role.Kitchen, 0, 2);

            var shift = new Shift(new ShiftSetup
            {
                Day = 1, CafeLevel = 1, Grid = Room(), Roster = roster, Lineup = lineup,
                Chemistry = Chemistry.Evaluate(lineup, roster, ThemeTag.None),
            });

            Assert.IsTrue(shift.Finished);
            StringAssert.Contains("recepção", shift.Finish().FailureReason.ToLowerInvariant());
        }

        [Test]
        public void ADayWithNoTablesRefusesToOpen()
        {
            var grid = new CafeGrid(7, 5);
            grid.Place(PlaceableCatalog.ById("stove"), 3, 2);

            var shift = new Shift(Setup(out _, grid));

            Assert.IsTrue(shift.Finished);
            StringAssert.Contains("sentar", shift.Finish().FailureReason);
        }

        [Test]
        public void NoTaskStaysClaimedAfterTheDayEnds()
        {
            var shift = new Shift(Setup(out _));
            Run(shift);

            Assert.IsTrue(shift.OpenTasks.All(t => !t.Claimed));
            Assert.IsTrue(shift.Workers.All(w => w.Claimed == null));
        }

        [Test]
        public void EveryEventCarriesAPositionInsideTheRoom()
        {
            var shift = new Shift(Setup(out _));
            Run(shift);

            foreach (var e in shift.Events)
            {
                Assert.That(e.X, Is.InRange(-1f, shift.Grid.Width + 1f), e.Kind.ToString());
                Assert.That(e.Y, Is.InRange(-1f, shift.Grid.Height + 1f), e.Kind.ToString());
            }
        }

        [Test]
        public void AVeryLargeTickDoesNotBreakTheDay()
        {
            var shift = new Shift(Setup(out _));

            Assert.DoesNotThrow(() => shift.Tick(500f));
            Assert.IsTrue(shift.Finished);
        }

        [Test]
        public void FinishIsIdempotent()
        {
            var shift = new Shift(Setup(out _));
            Run(shift);

            Assert.AreSame(shift.Finish(), shift.Finish());
        }

        [Test]
        public void TheIdolFiresHerOwnEncoreWhenTheRoomNeedsIt()
        {
            // Nobody left to press the button, so she has to decide for herself.
            var roster = new List<Maid>
            {
                Card(1, Archetype.Idol), Card(2, Archetype.Mukuchi, 1),
                Card(3, Archetype.Mukuchi, 1), Card(4, Archetype.Ojousama, 1),
            };

            var lineup = new Lineup(Lineup.SlotsFor(2));
            lineup.Assign(Role.Stage, 0, 1);
            lineup.Assign(Role.Floor, 0, 2);
            lineup.Assign(Role.Kitchen, 0, 3);
            lineup.Assign(Role.Reception, 0, 4);

            var shift = new Shift(new ShiftSetup
            {
                Day = 10, CafeLevel = 2, Grid = Room(), Roster = roster, Lineup = lineup,
                Chemistry = Chemistry.Evaluate(lineup, roster, ThemeTag.None),
                DurationSeconds = 160f, ArrivalsPerSecond = 0.6f, Seed = 9u,
            });

            Run(shift);

            CollectionAssert.Contains(shift.Events.Select(e => e.Kind).ToList(),
                ShiftEventKind.EncoreUsed);
        }
    }

    public sealed class BooksTests
    {
        private static Profile Shop()
        {
            var profile = Profile.NewGame(7u);
            profile.Grid.Place(PlaceableCatalog.ById("stove"), 5, 0);
            return profile;
        }

        private static ShiftResult Day(int dishes = 500, int tips = 90, int served = 10,
            int wrong = 0, int angry = 0, float satisfaction = 1f) =>
            new ShiftResult
            {
                DishRevenue = dishes, Tips = tips, Served = served, WrongOrders = wrong,
                LeftAngry = angry, Satisfaction = satisfaction, DishesCooked = served * 2,
            };

        [Test]
        public void TheBooksAddUp()
        {
            var ledger = Books.Close(Shop(), Day());

            Assert.AreEqual(ledger.Dishes + ledger.Tips, ledger.Revenue);
            Assert.AreEqual(ledger.Wages + ledger.Rent + ledger.Ingredients, ledger.Costs);
            Assert.AreEqual(ledger.Revenue - ledger.Costs, ledger.Profit);
        }

        [Test]
        public void EverybodyOnTheBooksIsPaidWhetherOrNotSheWorked()
        {
            var profile = Shop();
            var wagesWithThree = Books.WagesFor(profile);

            profile.Add(Lootbox.Mint(profile, Rarity.Legendary, new Rng(3)));

            Assert.Greater(Books.WagesFor(profile), wagesWithThree,
                "a legendary on the bench still costs every day");
        }

        [Test]
        public void ABiggerRoomIsABiggerBill()
        {
            var small = Shop();
            var large = Shop();
            large.Grid.Resize(13, 9);

            Assert.Greater(Books.RentFor(large), Books.RentFor(small));
        }

        [Test]
        public void IngredientsAreChargedOnEveryPlateIncludingTheWastedOnes()
        {
            var careful = Books.Close(Shop(), Day());
            var wasteful = Books.Close(Shop(), new ShiftResult
            {
                DishRevenue = 500, Tips = 90, Served = 10, Satisfaction = 1f, DishesCooked = 60,
            });

            Assert.Greater(wasteful.Ingredients, careful.Ingredients);
        }

        [Test]
        public void FichasArePaidOnQualityAndNotOnTakings()
        {
            var quiet = Books.Close(Shop(), Day(dishes: 120, served: 6, satisfaction: 1f));
            var frantic = Books.Close(Shop(),
                Day(dishes: 900, served: 20, wrong: 9, angry: 7, satisfaction: 0.25f));

            Assert.Greater(frantic.Revenue, quiet.Revenue, "precondition: the busy day earned more");
            Assert.Greater(quiet.PointsEarned, frantic.PointsEarned,
                "the two currencies are supposed to pull apart");
        }

        [Test]
        public void AFlawlessDayIsWorthGoingFor()
        {
            var flawless = Books.Close(Shop(), Day(wrong: 0, angry: 0));
            var nearly = Books.Close(Shop(), Day(wrong: 1, angry: 0, satisfaction: 0.95f));

            Assert.Greater(flawless.PointsEarned, nearly.PointsEarned + 10);
        }

        [Test]
        public void FichasNeverGoNegative()
        {
            var ledger = Books.Close(Shop(),
                Day(dishes: 10, served: 2, wrong: 40, angry: 20, satisfaction: 0f));

            Assert.GreaterOrEqual(ledger.PointsEarned, 0);
        }

        [Test]
        public void ABadEnoughDayEndsTheBusiness()
        {
            var profile = Shop();
            profile.Wallet.Restore(50, 0, 0);

            var ledger = Books.Close(profile, Day(dishes: 0, tips: 0, served: 0));

            Assert.IsTrue(ledger.Bankrupt);
            Assert.IsTrue(profile.Bankrupt);
            Assert.AreEqual("Falência", ledger.Verdict());
        }

        [Test]
        public void TheDayDoesNotAdvancePastABankruptcy()
        {
            var profile = Shop();
            profile.Wallet.Restore(10, 0, 0);
            var before = profile.Day;

            Books.Close(profile, Day(dishes: 0, tips: 0, served: 0));

            Assert.AreEqual(before, profile.Day);
        }

        [Test]
        public void AThinBalanceIsWarnedAboutBeforeItIsFatal()
        {
            var profile = Shop();
            var fixedCosts = Books.FixedCostsFor(profile);
            profile.Wallet.Restore(fixedCosts + 40, 0, 0);

            var ledger = Books.Close(profile, Day(dishes: fixedCosts, tips: 0, served: 4));

            Assert.IsFalse(ledger.Bankrupt);
            Assert.IsTrue(ledger.Warning, "a player about to lose has to be told");
        }

        [Test]
        public void AGoodDayAdvancesTheDayAndPaysReputation()
        {
            var profile = Shop();
            var before = profile.Wallet.Reputation;

            var ledger = Books.Close(profile, Day());

            Assert.AreEqual(2, profile.Day);
            Assert.Greater(profile.Wallet.Reputation, before);
            Assert.IsFalse(ledger.Bankrupt);
        }

        [Test]
        public void ClosingWithoutAResultStillPaysTheBills()
        {
            var profile = Shop();
            var before = profile.Wallet.Money;

            var ledger = Books.Close(profile, null);

            Assert.Greater(ledger.Costs, 0);
            Assert.Less(profile.Wallet.Money, before);
        }

        [Test]
        public void ClosingANullProfileIsIgnoredRatherThanThrown()
        {
            Assert.DoesNotThrow(() => Books.Close(null, Day()));
        }

        [Test]
        public void EveryVerdictIsPortugueseAndTheyDiffer()
        {
            var verdicts = new List<string>
            {
                Books.Close(Shop(), Day()).Verdict(),
                Books.Close(Shop(), Day(dishes: 0, tips: 0, served: 0)).Verdict(),
                Books.Close(Shop(), Day(dishes: 900, served: 20, wrong: 8, satisfaction: 0.3f))
                    .Verdict(),
            };

            Assert.IsTrue(verdicts.All(v => !string.IsNullOrEmpty(v)));
            Assert.Greater(verdicts.Distinct().Count(), 1);
        }
    }

    public sealed class LootboxTests
    {
        private static Profile Rich()
        {
            var profile = Profile.NewGame(5u);
            profile.Wallet.AddPoints(100000);
            profile.Wallet.AddReputation(CafeLevelTable.ReputationForLevel(9));
            profile.ApplyUnlocks();
            return profile;
        }

        [Test]
        public void EveryBoxPrintsItsOddsAndTheyAddToOneHundred()
        {
            foreach (var box in Lootbox.All)
            {
                Assert.IsNotEmpty(box.DisplayName, box.Id);
                Assert.IsNotEmpty(box.Description, box.Id);
                Assert.Greater(box.PricePoints, 0, box.Id);
                Assert.Greater(box.Cards, 0, box.Id);

                var odds = Lootbox.Odds(box);
                Assert.That(odds.Sum(), Is.EqualTo(100f).Within(0.01f), box.Id);
            }
        }

        [Test]
        public void ADearerBoxIsAlwaysABetterBox()
        {
            var ordered = Lootbox.All.OrderBy(b => b.PricePoints).ToList();

            for (var i = 1; i < ordered.Count; i++)
            {
                Assert.GreaterOrEqual((int)ordered[i].Floor, (int)ordered[i - 1].Floor,
                    ordered[i].Id + " costs more and guarantees no more");
            }
        }

        [Test]
        public void NoBoxEverRollsBelowItsOwnFloor()
        {
            foreach (var box in Lootbox.All)
            {
                for (var seed = 1u; seed < 300u; seed++)
                {
                    var rarity = Lootbox.RollRarity(box, new Rng(seed));
                    Assert.GreaterOrEqual((int)rarity, (int)box.Floor, box.Id + " seed " + seed);
                }
            }
        }

        [Test]
        public void ASimpleBoxIsMostlyCommonAndOccasionallyNot()
        {
            var box = Lootbox.ById("box_simple");
            var rng = new Rng(42);
            var counts = new Dictionary<Rarity, int>();

            for (var i = 0; i < 20000; i++)
            {
                var rarity = Lootbox.RollRarity(box, rng);
                counts.TryGetValue(rarity, out var count);
                counts[rarity] = count + 1;
            }

            Assert.That(counts[Rarity.Common] / 20000f, Is.EqualTo(0.78f).Within(0.03f));
            Assert.IsTrue(counts.ContainsKey(Rarity.Legendary), "the long shot has to exist");
        }

        [Test]
        public void OpeningCostsFichasAndNeverMoney()
        {
            var profile = Rich();
            var money = profile.Wallet.Money;
            var points = profile.Wallet.Points;

            var result = Lootbox.Open(profile, "box_simple", new Rng(1));

            Assert.IsTrue(result.Ok, result.Refusal);
            Assert.AreEqual(money, profile.Wallet.Money, "a box must never risk the rent");
            Assert.Less(profile.Wallet.Points, points);
        }

        [Test]
        public void ACardOutOfABoxJoinsTheRoster()
        {
            var profile = Rich();
            var before = profile.Maids.Count;

            var result = Lootbox.Open(profile, "box_silver", new Rng(2));

            Assert.AreEqual(3, result.Cards.Count);
            Assert.AreEqual(before + 3, profile.Maids.Count);
            Assert.IsTrue(result.Cards.All(c => c.Maid != null));
        }

        [Test]
        public void EveryCardComesOutWithTheTraitsItsRarityPromises()
        {
            var profile = Rich();

            for (var seed = 1u; seed < 40u; seed++)
            {
                var maid = Lootbox.Mint(profile, Rarity.Legendary, new Rng(seed));

                Assert.AreEqual(RarityTable.For(Rarity.Legendary).Traits, maid.Traits.Count);
                CollectionAssert.AllItemsAreUnique(maid.Traits);
                Assert.AreNotEqual(ThemeTag.None, maid.Origin);
            }
        }

        [Test]
        public void ACardArrivesInAPositionItBelongsIn()
        {
            var profile = Rich();

            for (var seed = 1u; seed < 40u; seed++)
            {
                var maid = Lootbox.Mint(profile, Rarity.Rare, new Rng(seed));

                Assert.AreEqual(RoleTable.NaturalFor(maid.Archetype), maid.NaturalRole);
            }
        }

        [Test]
        public void ABoxWithoutTheFichasIsRefusedAndCostsNothing()
        {
            var profile = Profile.NewGame(1u);
            var points = profile.Wallet.Points;

            var result = Lootbox.Open(profile, "box_gold", new Rng(1));

            Assert.IsFalse(result.Ok);
            Assert.IsNotEmpty(result.Refusal);
            Assert.AreEqual(points, profile.Wallet.Points);
        }

        [Test]
        public void ABoxThatDoesNotExistIsRefusedNotThrown()
        {
            Assert.IsFalse(Lootbox.Open(Rich(), "box_imaginaria", new Rng(1)).Ok);
            Assert.IsFalse(Lootbox.Open(null, "box_simple", new Rng(1)).Ok);
        }

        [Test]
        public void AFullRosterTurnsACardIntoFichasRatherThanLosingIt()
        {
            var profile = Profile.NewGame(3u);
            profile.Wallet.AddPoints(100000);

            var rng = new Rng(3);
            while (profile.Maids.Count < profile.Unlocks.RosterCap)
                profile.Add(Lootbox.Mint(profile, Rarity.Common, rng));
            Assert.AreEqual(profile.Unlocks.RosterCap, profile.Maids.Count);

            var before = profile.Wallet.Points;
            var result = Lootbox.Open(profile, "box_simple", new Rng(4));

            Assert.IsTrue(result.Ok);
            Assert.IsTrue(result.Cards[0].Duplicate);
            Assert.Greater(result.Cards[0].PointsBack, 0);
            Assert.Greater(profile.Wallet.Points,
                before - Lootbox.ById("box_simple").PricePoints);
        }

        [Test]
        public void LettingSomebodyGoPaysSomethingBackAndCutsTheWageBill()
        {
            var profile = Rich();
            var maid = profile.Maids[0];
            var wages = Books.WagesFor(profile);
            var points = profile.Wallet.Points;

            Assert.IsTrue(profile.Dismiss(maid.Id));

            Assert.Less(Books.WagesFor(profile), wages);
            Assert.Greater(profile.Wallet.Points, points);
            Assert.IsFalse(profile.Lineup.Contains(maid.Id));
        }

        [Test]
        public void DismissingSomebodyWhoDoesNotWorkHereIsRefused()
        {
            Assert.IsFalse(Rich().Dismiss(99999));
        }
    }
}
