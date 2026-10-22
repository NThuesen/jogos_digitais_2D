using System;
using System.Linq;
using Neko.Sim.Core;
using Neko.Sim.Maids;
using Neko.Sim.Team;
using NUnit.Framework;

namespace Neko.Tests.Sim
{
    public sealed class RoleTests
    {
        [Test]
        public void ThereAreFivePositionsAndEveryOneIsDescribed()
        {
            var kinds = (Role[])Enum.GetValues(typeof(Role));
            Assert.AreEqual(5, kinds.Length);

            foreach (var profile in RoleTable.All)
            {
                Assert.IsNotEmpty(profile.DisplayName, profile.Kind.ToString());
                Assert.IsNotEmpty(profile.Description, profile.Kind.ToString());
                Assert.IsNotEmpty(profile.ShapeId, profile.Kind.ToString());
            }

            CollectionAssert.AreEquivalent(kinds, RoleTable.All.Select(p => p.Kind));
        }

        [Test]
        public void OnlyTheStageAndTheTillStandIdle()
        {
            foreach (var profile in RoleTable.All)
            {
                var passive = profile.Kind == Role.Stage || profile.Kind == Role.Management;
                Assert.AreEqual(!passive, profile.TakesTasks, profile.Kind.ToString());
            }
        }

        [Test]
        public void EveryPositionIsSomebodysHome()
        {
            // A position nobody naturally belongs in would be a slot the player can only ever
            // fill badly, which is a tax rather than a decision.
            var homes = ArchetypeTable.All.Select(p => RoleTable.NaturalFor(p.Kind)).Distinct();

            CollectionAssert.AreEquivalent((Role[])Enum.GetValues(typeof(Role)), homes.ToList());
        }

        [Test]
        public void EveryPositionGradesADifferentPairOfStats()
        {
            var pairs = RoleTable.All
                .Select(p => p.Primary + "/" + p.Secondary)
                .ToList();

            CollectionAssert.AllItemsAreUnique(pairs);
        }

        [Test]
        public void TheNaturalPositionFollowsTheArchetypeRules()
        {
            // Mukuchi is bad at the door and fast at the stove, so the kitchen is her home.
            Assert.AreEqual(Role.Kitchen, RoleTable.NaturalFor(Archetype.Mukuchi));
            Assert.AreEqual(Role.Stage, RoleTable.NaturalFor(Archetype.Idol));
            Assert.AreEqual(Role.Management, RoleTable.NaturalFor(Archetype.Oneesan));
            Assert.AreEqual(Role.Floor, RoleTable.NaturalFor(Archetype.Genki));
        }

        [Test]
        public void ForRefusesAnInventedPosition()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RoleTable.For((Role)99));
        }
    }

    public sealed class RarityTests
    {
        [Test]
        public void TheFourRaritiesGetStrictlyBetterAndStrictlyDearer()
        {
            RarityProfile previous = null;

            foreach (Rarity rarity in Enum.GetValues(typeof(Rarity)))
            {
                var profile = RarityTable.For(rarity);
                Assert.IsNotEmpty(profile.DisplayName, rarity.ToString());
                Assert.Less(profile.StatMin, profile.StatMax, rarity.ToString());

                if (previous != null)
                {
                    Assert.Greater(profile.StatMin, previous.StatMin, rarity.ToString());
                    Assert.Greater(profile.Salary, previous.Salary, rarity.ToString());
                    Assert.GreaterOrEqual(profile.Traits, previous.Traits, rarity.ToString());
                }

                previous = profile;
            }
        }

        [Test]
        public void ALegendaryCostsMoreThanTenCommonsToKeep()
        {
            // The whole reason the lineup is a budget decision rather than "field the best".
            Assert.Greater(RarityTable.For(Rarity.Legendary).Salary,
                RarityTable.For(Rarity.Common).Salary * 10);
        }

        [Test]
        public void RolledStatsStayInsideTheRarityBand()
        {
            foreach (Rarity rarity in Enum.GetValues(typeof(Rarity)))
            {
                var band = RarityTable.For(rarity);

                for (var seed = 1u; seed < 60u; seed++)
                {
                    var stats = RarityTable.RollStats(rarity, Archetype.Genki, new Rng(seed));

                    foreach (StatKind kind in Enum.GetValues(typeof(StatKind)))
                    {
                        Assert.That(stats[kind], Is.InRange(band.StatMin, band.StatMax),
                            rarity + " " + kind + " seed " + seed);
                    }
                }
            }
        }

        [Test]
        public void ARollerTiltsTowardsThePersonalitysOwnStat()
        {
            var cookTotal = 0;
            var serveTotal = 0;

            for (var seed = 1u; seed < 200u; seed++)
            {
                var stats = RarityTable.RollStats(Rarity.Epic, Archetype.Mukuchi, new Rng(seed));
                cookTotal += stats.Cook;
                serveTotal += stats.Serve;
            }

            Assert.Greater(cookTotal, serveTotal,
                "a Mukuchi card has to read as a cook before the player levels her");
        }

        [Test]
        public void RollingIsDeterministic()
        {
            Assert.AreEqual(
                RarityTable.RollStats(Rarity.Rare, Archetype.Idol, new Rng(7)),
                RarityTable.RollStats(Rarity.Rare, Archetype.Idol, new Rng(7)));
        }
    }

    public sealed class TraitTests
    {
        [Test]
        public void EveryTraitIdExceptNoneIsInTheTable()
        {
            foreach (TraitId id in Enum.GetValues(typeof(TraitId)))
            {
                if (id == TraitId.None) continue;
                Assert.DoesNotThrow(() => TraitTable.ById(id), id.ToString());
            }
        }

        [Test]
        public void EveryTraitDoesSomethingAndSaysSoInPortuguese()
        {
            foreach (var def in TraitTable.All)
            {
                Assert.IsNotEmpty(def.DisplayName, def.Id.ToString());
                Assert.IsNotEmpty(def.Description, def.Id.ToString());

                var inert = Math.Abs(def.SpeedMultiplier - 1f) < 1e-4f &&
                            Math.Abs(def.TipMultiplier - 1f) < 1e-4f &&
                            Math.Abs(def.MistakeMultiplier - 1f) < 1e-4f &&
                            Math.Abs(def.MoodDrainMultiplier - 1f) < 1e-4f &&
                            def.ChemistryBonus == 0 && def.PointsPerDay == 0 &&
                            Math.Abs(def.SalaryDiscount) < 1e-4f;

                Assert.IsFalse(inert, def.Id + " changes nothing at all");
            }
        }

        [Test]
        public void AngelHandsReallyNeverErrs()
        {
            Assert.AreEqual(0f, TraitTable.ById(TraitId.AngelHands).MistakeMultiplier);
        }

        [Test]
        public void RollingNeverRepeatsATraitOnOneCard()
        {
            for (var seed = 1u; seed < 80u; seed++)
            {
                var rolled = TraitTable.Roll(3, new Rng(seed));

                Assert.AreEqual(3, rolled.Count, "seed " + seed);
                CollectionAssert.AllItemsAreUnique(rolled);
            }
        }

        [Test]
        public void RollingNothingGivesNothing()
        {
            Assert.IsEmpty(TraitTable.Roll(0, new Rng(1)));
            Assert.IsEmpty(TraitTable.Roll(-3, new Rng(1)));
        }

        [Test]
        public void RollingMoreThanExistIsCappedRatherThanLooping()
        {
            Assert.AreEqual(TraitTable.All.Count, TraitTable.Roll(500, new Rng(1)).Count);
        }
    }

    public sealed class LineupTests
    {
        private static Maid Card(int id, Archetype archetype) =>
            Maid.Create(id, archetype.ToString(), archetype);

        [Test]
        public void TheOpeningBoardIsOneGreeterOneWaitressAndOneCook()
        {
            var slots = Lineup.SlotsFor(1);

            Assert.AreEqual(1, slots.Reception);
            Assert.AreEqual(1, slots.Floor);
            Assert.AreEqual(1, slots.Kitchen);
            Assert.AreEqual(0, slots.Stage);
            Assert.AreEqual(0, slots.Management);
        }

        [Test]
        public void TheBoardOnlyEverGrows()
        {
            var previous = Lineup.SlotsFor(1);

            for (var level = 2; level <= 10; level++)
            {
                var now = Lineup.SlotsFor(level);

                foreach (Role role in Enum.GetValues(typeof(Role)))
                {
                    Assert.GreaterOrEqual(now.For(role), previous.For(role),
                        "level " + level + " " + role);
                }

                previous = now;
            }
        }

        [Test]
        public void NobodyCanStandInTwoPlacesAtOnce()
        {
            var lineup = new Lineup(Lineup.SlotsFor(5));

            lineup.Assign(Role.Floor, 0, 7);
            lineup.Assign(Role.Kitchen, 0, 7);

            Assert.AreEqual(1, lineup.MaidIds().Count);
            Assert.AreEqual(Role.Kitchen, lineup.RoleOf(7));
        }

        [Test]
        public void AssigningToASlotThatDoesNotExistIsRefused()
        {
            var lineup = new Lineup(Lineup.SlotsFor(1));

            Assert.IsFalse(lineup.Assign(Role.Stage, 0, 1), "café level one has no stage");
            Assert.IsFalse(lineup.Assign(Role.Floor, 4, 1));
        }

        [Test]
        public void GrowingTheBoardKeepsWhoeverWasAlreadyStanding()
        {
            var lineup = new Lineup(Lineup.SlotsFor(1));
            lineup.Assign(Role.Kitchen, 0, 3);

            lineup.Rebuild(Lineup.SlotsFor(8));

            Assert.AreEqual(Role.Kitchen, lineup.RoleOf(3));
        }

        [Test]
        public void ShrinkingTheBoardDropsWhoeverNoLongerHasASlot()
        {
            var lineup = new Lineup(Lineup.SlotsFor(8));
            lineup.Assign(Role.Kitchen, 2, 9);

            lineup.Rebuild(Lineup.SlotsFor(1));

            Assert.IsFalse(lineup.Contains(9));
        }

        [Test]
        public void AutoFillPutsEverybodyWhereTheyBelong()
        {
            var roster = new[]
            {
                Card(1, Archetype.Genki), Card(2, Archetype.Mukuchi), Card(3, Archetype.Ojousama),
                Card(4, Archetype.Idol), Card(5, Archetype.Oneesan),
            };

            var lineup = new Lineup(Lineup.SlotsFor(5));
            lineup.AutoFill(roster);

            Assert.AreEqual(Role.Floor, lineup.RoleOf(1));
            Assert.AreEqual(Role.Kitchen, lineup.RoleOf(2));
            Assert.AreEqual(Role.Reception, lineup.RoleOf(3));
            Assert.AreEqual(Role.Stage, lineup.RoleOf(4));
            Assert.AreEqual(Role.Management, lineup.RoleOf(5));
        }

        [Test]
        public void AutoFillNeverPlaysTheSameMaidTwice()
        {
            var roster = new[] { Card(1, Archetype.Genki), Card(2, Archetype.Mukuchi) };

            var lineup = new Lineup(Lineup.SlotsFor(9));
            lineup.AutoFill(roster);

            CollectionAssert.AllItemsAreUnique(lineup.MaidIds());
            Assert.AreEqual(2, lineup.MaidIds().Count);
        }

        [Test]
        public void ClearingEmptiesTheWholeBoard()
        {
            var lineup = new Lineup(Lineup.SlotsFor(5));
            lineup.AutoFill(new[] { Card(1, Archetype.Genki) });

            lineup.Clear();

            Assert.IsEmpty(lineup.MaidIds());
        }
    }

    public sealed class ChemistryTests
    {
        private static Maid Card(int id, Archetype archetype, House house, ThemeTag origin)
        {
            var maid = Maid.Create(id, archetype.ToString(), archetype);
            maid.House = house;
            maid.Origin = origin;
            return maid;
        }

        [Test]
        public void AnEmptyLineupHasNoChemistryAndTheWorstMultiplier()
        {
            var chemistry = Chemistry.Evaluate(new Lineup(Lineup.SlotsFor(5)),
                new Maid[0], ThemeTag.None);

            Assert.AreEqual(0, chemistry.Score);
            Assert.AreEqual(Chemistry.LowMultiplier, chemistry.Multiplier);
        }

        [Test]
        public void StandingWhereSheBelongsIsWorthSomething()
        {
            var maid = Card(1, Archetype.Mukuchi, House.Tsukimi, ThemeTag.Sakura);
            var roster = new[] { maid };

            var home = new Lineup(Lineup.SlotsFor(5));
            home.Assign(Role.Kitchen, 0, 1);

            var away = new Lineup(Lineup.SlotsFor(5));
            away.Assign(Role.Floor, 0, 1);

            Assert.Greater(
                Chemistry.Evaluate(home, roster, ThemeTag.None).Score,
                Chemistry.Evaluate(away, roster, ThemeTag.None).Score);
        }

        [Test]
        public void SharedTagsLinkTwoMaidsInTheSamePosition()
        {
            var alike = new[]
            {
                Card(1, Archetype.Genki, House.Hanabi, ThemeTag.Sakura),
                Card(2, Archetype.Tsundere, House.Hanabi, ThemeTag.Sakura),
            };

            var apart = new[]
            {
                Card(1, Archetype.Genki, House.Hanabi, ThemeTag.Sakura),
                Card(2, Archetype.Kuudere, House.Kurogane, ThemeTag.Gothic),
            };

            Assert.Greater(Score(alike), Score(apart));

            int Score(Maid[] roster)
            {
                var lineup = new Lineup(Lineup.SlotsFor(7));
                lineup.Assign(Role.Floor, 0, 1);
                lineup.Assign(Role.Floor, 1, 2);
                return Chemistry.Evaluate(lineup, roster, ThemeTag.None).Score;
            }
        }

        [Test]
        public void MaidsInDifferentPositionsDoNotLink()
        {
            var roster = new[]
            {
                Card(1, Archetype.Genki, House.Hanabi, ThemeTag.Sakura),
                Card(2, Archetype.Mukuchi, House.Hanabi, ThemeTag.Sakura),
            };

            var lineup = new Lineup(Lineup.SlotsFor(5));
            lineup.Assign(Role.Floor, 0, 1);
            lineup.Assign(Role.Kitchen, 0, 2);

            var chemistry = Chemistry.Evaluate(lineup, roster, ThemeTag.None);

            foreach (var maid in chemistry.PerMaid)
            {
                Assert.AreEqual(0, maid.FromLinks,
                    "a cook and a waitress on opposite sides of the room do not link");
            }
        }

        [Test]
        public void DecoratingToMatchYourMaidsPaysOff()
        {
            var roster = new[] { Card(1, Archetype.Genki, House.Hanabi, ThemeTag.Sakura) };
            var lineup = new Lineup(Lineup.SlotsFor(5));
            lineup.Assign(Role.Floor, 0, 1);

            Assert.Greater(
                Chemistry.Evaluate(lineup, roster, ThemeTag.Sakura).Score,
                Chemistry.Evaluate(lineup, roster, ThemeTag.Gothic).Score);
        }

        [Test]
        public void TheMultiplierSpansTheWholeAdvertisedRange()
        {
            Assert.AreEqual(Chemistry.LowMultiplier, Chemistry.MultiplierFor(0));
            Assert.AreEqual(Chemistry.HighMultiplier, Chemistry.MultiplierFor(100));
            Assert.That(Chemistry.MultiplierFor(50), Is.EqualTo(1f).Within(1e-4f));
        }

        [Test]
        public void APerfectLineupBeatsAScatteredOneByAThird()
        {
            // The claim the whole pivot rests on, as a number.
            Assert.That(Chemistry.HighMultiplier / Chemistry.LowMultiplier,
                Is.GreaterThan(1.3f));
        }

        [Test]
        public void NobodysChemistryExceedsTheCap()
        {
            var maid = Card(1, Archetype.Mukuchi, House.Tsukimi, ThemeTag.Sakura);
            maid.Traits.Add(TraitId.Hostess);
            maid.Traits.Add(TraitId.Magnetic);

            var roster = new[] { maid };
            var lineup = new Lineup(Lineup.SlotsFor(5));
            lineup.Assign(Role.Kitchen, 0, 1);

            var chemistry = Chemistry.Evaluate(lineup, roster, ThemeTag.Sakura);

            Assert.LessOrEqual(chemistry.PerMaid[0].Score, Chemistry.MaxPerMaid);
            Assert.LessOrEqual(chemistry.Score, 100);
        }

        [Test]
        public void EveryVerdictIsPortugueseAndTheyDiffer()
        {
            var verdicts = new[] { 0, 25, 50, 70, 95 }.Select(Chemistry.Verdict).ToList();

            CollectionAssert.AllItemsAreUnique(verdicts);
            Assert.IsTrue(verdicts.All(v => !string.IsNullOrEmpty(v)));
        }

        [Test]
        public void TemperamentIsReadOffThePersonalityAndNeverStored()
        {
            Assert.AreEqual(Temperament.Energetic,
                ChemistryTags.TemperamentFor(Archetype.Genki));
            Assert.AreEqual(Temperament.Serene, ChemistryTags.TemperamentFor(Archetype.Kuudere));
            Assert.AreEqual(Temperament.Dramatic, ChemistryTags.TemperamentFor(Archetype.Idol));
        }

        [Test]
        public void SharedTagsCountsAllThree()
        {
            var a = Card(1, Archetype.Genki, House.Hanabi, ThemeTag.Sakura);
            var b = Card(2, Archetype.Tsundere, House.Hanabi, ThemeTag.Sakura);

            Assert.AreEqual(3, ChemistryTags.SharedTags(a, b));
            Assert.AreEqual(0, ChemistryTags.SharedTags(a, null));
        }
    }
}
