using System.Linq;
using Neko.Sim.Core;
using Neko.Sim.Economy;
using Neko.Sim.Maids;
using Neko.Sim.Progress;
using Neko.Sim.Save;
using Neko.Sim.Shift;
using Neko.Sim.Team;
using NUnit.Framework;

namespace Neko.Tests.EditMode.Sim
{
    /// <summary>
    /// ECON-03: the collection has no cap, and only the maids standing in a slot of today's lineup are paid.
    /// </summary>
    public sealed class ActiveWagesTests
    {
        /// <summary>The roster cap that used to be: level 1 held two maids.</summary>
        private const int OldLevelOneCap = 2;

        private static Profile Shop(uint seed = 7u) => Profile.NewGame(seed);

        /// <summary>A level 4 café: the opening team stands in the lineup and some slots are still free.</summary>
        private static Profile RoomyShop()
        {
            var profile = Shop();
            profile.Wallet.AddReputation(CafeLevelTable.ReputationForLevel(4));
            profile.ApplyUnlocks();
            Assert.IsTrue(profile.Lineup.All.Any(s => s.Empty), "a free slot to work with");
            Assert.IsTrue(profile.FieldedMaids().Count >= 3);
            return profile;
        }

        /// <summary>Puts maids of free identities on the roster without fielding them.</summary>
        private static void Collect(Profile profile, int count, int level = 1)
        {
            foreach (var identity in MaidCatalog.FreeReleased(profile.Maids).Take(count))
            {
                profile.Add(Maid.Create(profile.NextMaidId++, identity, level));
            }
        }

        private static int Wages(params Maid[] maids) => maids.Sum(m => m.Salary);

        private static ShiftResult QuietDay() => new ShiftResult { Satisfaction = 1f };

        // ---- the wage bill -------------------------------------------------------------------

        [Test]
        public void OnlyTheFieldedMaidsDrawAWage()
        {
            var profile = Shop();
            var fielded = profile.FieldedMaids().ToArray();
            Assert.GreaterOrEqual(fielded.Length, 3, "the opening team stands in the lineup");

            Collect(profile, 9);
            Assert.GreaterOrEqual(profile.Maids.Count, 12, "many owned maids, most of them benched");

            Assert.AreEqual(Wages(fielded), Books.WagesFor(profile));
            Assert.AreEqual(Wages(fielded) + Books.RentFor(profile), Books.FixedCostsFor(profile));
        }

        [Test]
        public void AddingAMaidToTheCollectionWithoutFieldingHerDoesNotChangeTheFixedCosts()
        {
            var profile = Shop();
            var before = Books.FixedCostsFor(profile);

            Collect(profile, 1, level: 8);
            var legendary = Lootbox.Mint(profile, Rarity.Legendary, new Rng(3));
            profile.Add(legendary);

            Assert.IsFalse(profile.Lineup.Contains(legendary.Id));
            Assert.AreEqual(before, Books.FixedCostsFor(profile), "a legendary on the bench is free to keep");
        }

        [Test]
        public void ReplacingAFieldedMaidWithAnotherOneChangesTheBillByTheDifference()
        {
            var profile = Shop();
            var leaving = profile.FieldedMaids()[0];
            var slot = profile.Lineup.All.First(s => s.MaidId == leaving.Id);
            var arriving = Lootbox.Mint(profile, Rarity.Legendary, new Rng(3));
            profile.Add(arriving);
            Assert.IsNotNull(profile.MaidById(arriving.Id), "the legendary joined the collection");
            Assert.AreNotEqual(leaving.Salary, arriving.Salary, "the test needs two different wages");

            var before = Books.WagesFor(profile);
            Assert.IsTrue(profile.Lineup.Assign(slot.Role, slot.Index, arriving.Id));

            Assert.AreEqual(before - leaving.Salary + arriving.Salary, Books.WagesFor(profile));
            Assert.IsTrue(profile.Lineup.Contains(arriving.Id));
            Assert.IsFalse(profile.Lineup.Contains(leaving.Id), "she is on the bench now, and costs nothing");
        }

        [Test]
        public void TakingAMaidOutOfTheLineupStopsHerWage()
        {
            var profile = Shop();
            var maid = profile.FieldedMaids()[0];
            var before = Books.WagesFor(profile);

            Assert.IsTrue(profile.Lineup.Remove(maid.Id));

            Assert.AreEqual(before - maid.Salary, Books.WagesFor(profile));
            Assert.IsNotNull(profile.MaidById(maid.Id), "she is still in the collection");
        }

        [Test]
        public void AnEmptyLineupPaysNoWagesButStillPaysRent()
        {
            var profile = Shop();
            Collect(profile, 5);

            profile.Lineup.Clear();

            Assert.AreEqual(0, Books.WagesFor(profile));
            Assert.AreEqual(Books.RentFor(profile), Books.FixedCostsFor(profile));
        }

        [Test]
        public void AnEmptySlotAndASlotNamingNobodyOnTheBooksPayNothing()
        {
            var profile = RoomyShop();
            var fielded = profile.FieldedMaids().ToArray();
            var free = profile.Lineup.All.First(s => s.Empty);

            // A hand-edited or half-migrated save: a slot pointing at a maid who is not on the books.
            free.MaidId = 424242;

            Assert.AreEqual(Wages(fielded), Books.WagesFor(profile), "no phantom wage");
        }

        [Test]
        public void TheSameMaidIsNeverPaidTwice()
        {
            var profile = RoomyShop();
            var first = profile.Lineup.All.First(s => !s.Empty);
            var second = profile.Lineup.All.First(s => s.Empty);
            var before = Books.WagesFor(profile);

            second.MaidId = first.MaidId; // the lineup itself forbids this; a corrupted save could not

            Assert.AreEqual(before, Books.WagesFor(profile));
        }

        [Test]
        public void WagesNeverTouchTheMaidsOwnSalary()
        {
            var profile = Shop();
            Collect(profile, 4);
            var salaries = profile.Maids.Select(m => m.Salary).ToArray();

            Books.WagesFor(profile);
            profile.Lineup.Clear();
            profile.Lineup.AutoFill(profile.Maids);

            CollectionAssert.AreEqual(salaries, profile.Maids.Select(m => m.Salary).ToArray());
        }

        [Test]
        public void FixedCostsGrowWithTheLineupSlotsOfTheCafeLevelNotWithAThree()
        {
            var profile = Shop();
            Collect(profile, 10);
            profile.Wallet.AddReputation(CafeLevelTable.ReputationForLevel(6));
            profile.ApplyUnlocks();
            profile.Lineup.AutoFill(profile.Maids);

            var slots = Lineup.SlotsFor(profile.CafeLevel).Total;
            var fielded = profile.FieldedMaids();

            Assert.Greater(slots, 3, "later levels unlock more positions");
            Assert.AreEqual(slots, fielded.Count, "the whole team is fielded from a deep collection");
            Assert.AreEqual(fielded.Sum(m => m.Salary), Books.WagesFor(profile));
        }

        // ---- the day's books -----------------------------------------------------------------

        [Test]
        public void TheLedgerChargesTheFieldedTeamExactlyAndTheDisplayedCostMatches()
        {
            var profile = Shop();
            Collect(profile, 8);
            var expected = Books.WagesFor(profile);
            var shown = Books.FixedCostsFor(profile);
            var before = profile.Wallet.Money;

            var ledger = Books.Close(profile, QuietDay());

            Assert.AreEqual(expected, ledger.Wages, "the ledger shows only the active wages");
            Assert.AreEqual(shown, ledger.Wages + ledger.Rent, "what was shown is what was charged");
            Assert.AreEqual(before - ledger.Costs, ledger.BalanceAfter);
            Assert.AreEqual(ledger.Wages + ledger.Rent + ledger.Ingredients, ledger.Costs);
        }

        [Test]
        public void ChangingTheTeamBetweenDaysChangesTheNextLedger()
        {
            var profile = Shop();
            Collect(profile, 6);
            var one = Books.Close(profile, QuietDay());

            var maid = profile.FieldedMaids()[0];
            profile.Lineup.Remove(maid.Id);
            var two = Books.Close(profile, QuietDay());

            Assert.AreEqual(one.Wages - maid.Salary, two.Wages, "she was benched, so she is not paid");
        }

        [Test]
        public void TheCashWarningAndBankruptcyFollowTheActiveCostsAndNotTheBench()
        {
            // Exactly the active costs in the bank: the day ends at zero, which is bankruptcy.
            var broke = Shop();
            Collect(broke, 10);
            broke.Wallet.Pay(broke.Wallet.Money - Books.FixedCostsFor(broke));
            Assert.IsTrue(Books.Close(broke, QuietDay()).Bankrupt);

            // One coin more survives; the old rule (bench included) would have ended it.
            var survives = Shop();
            Collect(survives, 10);
            survives.Wallet.Pay(survives.Wallet.Money - Books.FixedCostsFor(survives) - 1);
            var ledger = Books.Close(survives, QuietDay());
            Assert.IsFalse(ledger.Bankrupt);
            Assert.IsTrue(ledger.Warning, "one coin left is under two days of the active costs");

            // Comfortably above two days of the active costs: no warning, bench or not.
            var fine = Shop();
            Collect(fine, 10);
            var need = Books.FixedCostsFor(fine);
            fine.Wallet.Pay(fine.Wallet.Money - (need * (Books.WarningDays + 2)));
            var calm = Books.Close(fine, QuietDay());
            Assert.IsFalse(calm.Bankrupt);
            Assert.IsFalse(calm.Warning);
        }

        // ---- the collection has no cap -------------------------------------------------------

        [Test]
        public void HiringAboveTheOldRosterCapIsAllowed()
        {
            var profile = Shop();
            profile.Wallet.Earn(100000);
            var rng = new Rng(11u);
            var board = new ApplicantBoard();

            Assert.Greater(profile.Maids.Count, OldLevelOneCap, "the new game already starts above the old cap");
            var hired = 0;
            for (var round = 0; round < 6 && hired < 8; round++)
            {
                board.Refresh(rng, profile.CafeLevel, profile.NextMaidId, profile.Maids);
                for (var i = 0; i < ApplicantBoard.Slots; i++)
                {
                    if (board.Candidates[i] == null) continue;
                    var result = board.Hire(profile, i);
                    Assert.IsTrue(result.Ok, result.Refusal);
                    hired++;
                }
            }

            Assert.GreaterOrEqual(hired, 6);
            Assert.GreaterOrEqual(profile.Maids.Count, 3 + hired);
            Assert.AreEqual(profile.Maids.Count, profile.Maids.Select(m => m.Id).Distinct().Count(), "ids stay unique");
            Assert.AreEqual(profile.Maids.Count, profile.Maids.Select(m => m.IdentityId).Distinct().Count());
        }

        [Test]
        public void HiringStillChargesTheFeeConsumesTheBoardSlotAndRefusesWhenBroke()
        {
            var profile = Shop();
            profile.Wallet.Earn(100000);
            var board = new ApplicantBoard();
            board.Refresh(new Rng(5u), profile.CafeLevel, profile.NextMaidId, profile.Maids);
            var index = Enumerable.Range(0, ApplicantBoard.Slots).First(i => board.Candidates[i] != null);
            var price = board.Candidates[index].Price;
            var money = profile.Wallet.Money;

            Assert.IsTrue(board.Hire(profile, index).Ok);
            Assert.AreEqual(money - price, profile.Wallet.Money, "the hiring fee is unchanged");
            Assert.IsNull(board.Candidates[index], "the slot of the board is consumed");
            Assert.IsFalse(board.Hire(profile, index).Ok, "an empty slot cannot be hired twice");

            var poor = Shop();
            var poorBoard = new ApplicantBoard();
            poorBoard.Refresh(new Rng(5u), poor.CafeLevel, poor.NextMaidId, poor.Maids);
            poor.Wallet.Pay(poor.Wallet.Money);
            var refused = poorBoard.Hire(poor, index);
            Assert.IsFalse(refused.Ok);
            StringAssert.Contains("Faltam", refused.Refusal);
        }

        [Test]
        public void HiringRefusesAnIdentityShePossesses()
        {
            var profile = Shop();
            profile.Wallet.Earn(100000);
            var board = new ApplicantBoard();
            board.Refresh(new Rng(5u), profile.CafeLevel, profile.NextMaidId, profile.Maids);
            var index = Enumerable.Range(0, ApplicantBoard.Slots).First(i => board.Candidates[i] != null);

            // She acquires the very identity on offer some other way (a box) before hiring.
            var identity = MaidCatalog.ById(board.Candidates[index].Maid.IdentityId);
            profile.Add(Maid.Create(profile.NextMaidId++, identity));
            var count = profile.Maids.Count;

            var result = board.Hire(profile, index);

            Assert.IsFalse(result.Ok);
            Assert.AreEqual(count, profile.Maids.Count);
        }

        [Test]
        public void ABoxAboveTheOldRosterCapAddsTheNewIdentity()
        {
            var profile = Shop();
            profile.Wallet.AddPoints(100000);
            Collect(profile, 6);
            var before = profile.Maids.Count;
            Assert.Greater(before, OldLevelOneCap);
            Assert.Greater(MaidCatalog.FreeReleased(profile.Maids).Count, 0);

            var open = Lootbox.Open(profile, "box_gold", new Rng(8u));

            Assert.IsTrue(open.Ok);
            var added = open.Cards.Count(c => !c.Duplicate);
            Assert.Greater(added, 0, "a new identity joins the collection however many she owns");
            Assert.AreEqual(before + added, profile.Maids.Count);
            foreach (var card in open.Cards.Where(c => !c.Duplicate))
            {
                Assert.IsNotNull(profile.MaidById(card.Maid.Id));
            }
        }

        [Test]
        public void AnIdentityShePossessesStillConvertsToFichas()
        {
            var profile = Shop();
            profile.Wallet.AddPoints(100000);
            var rng = new Rng(2u);
            var sawDuplicate = false;

            for (var i = 0; i < 400 && !sawDuplicate; i++)
            {
                var points = profile.Wallet.Points;
                var count = profile.Maids.Count;
                var open = Lootbox.Open(profile, "box_simple", rng);
                var price = Lootbox.ById("box_simple").PricePoints;
                foreach (var card in open.Cards.Where(c => c.Duplicate))
                {
                    sawDuplicate = true;
                    Assert.AreEqual(card.Maid.Severance, card.PointsBack);
                    Assert.Greater(card.PointsBack, 0);
                }

                var back = open.Cards.Where(c => c.Duplicate).Sum(c => c.PointsBack);
                Assert.AreEqual(points - price + back, profile.Wallet.Points, "the box price and the refund are unchanged");
                Assert.AreEqual(count + open.Cards.Count(c => !c.Duplicate), profile.Maids.Count);
                Assert.AreEqual(profile.Maids.Count, profile.Maids.Select(m => m.IdentityId).Distinct().Count());
            }

            Assert.IsTrue(sawDuplicate, "after enough boxes a card repeats and converts");
        }

        [Test]
        public void TheWholeReleasedCatalogCanBeCollectedAndThenEveryCardConverts()
        {
            var profile = Shop();
            profile.Wallet.AddPoints(10000000);
            var rng = new Rng(77u);

            for (var i = 0; i < 4000 && MaidCatalog.FreeReleased(profile.Maids).Count > 0; i++)
            {
                Assert.IsTrue(Lootbox.Open(profile, "box_gold", rng).Ok);
            }

            Assert.AreEqual(0, MaidCatalog.FreeReleased(profile.Maids).Count, "every released identity is owned");
            Assert.AreEqual(MaidCatalog.Released.Count, profile.Maids.Count);
            Assert.AreEqual(MaidCatalog.Released.Count, profile.Maids.Select(m => m.IdentityId).Distinct().Count());
            Assert.IsTrue(profile.Maids.All(m => MaidCatalog.ById(m.IdentityId).Released), "nobody planned is obtainable");

            var open = Lootbox.Open(profile, "box_gold", rng);
            Assert.IsTrue(open.Cards.All(c => c.Duplicate), "with everyone collected a box can only convert");
            Assert.AreEqual(MaidCatalog.Released.Count, profile.Maids.Count);
        }

        [Test]
        public void ACollectionLargerThanTheOldCapSurvivesASaveAndKeepsTheLineup()
        {
            var profile = Shop(21u);
            Collect(profile, 14, level: 3);
            profile.Lineup.AutoFill(profile.Maids);
            var names = profile.Maids.Select(m => m.Id + ":" + m.IdentityId + ":" + m.Level).ToArray();
            var team = profile.Lineup.All.Select(s => s.Role + "/" + s.Index + "=" + s.MaidId).ToArray();
            var wages = Books.WagesFor(profile);
            var total = profile.Maids.Count;

            var loaded = SaveCodec.Decode(SaveCodec.Encode(profile));

            Assert.AreEqual(LoadStatus.Ok, loaded.Status, loaded.Message);
            var back = loaded.Profile;
            Assert.AreEqual(total, back.Maids.Count, "nobody disappears");
            Assert.Greater(back.Maids.Count, 8);
            CollectionAssert.AreEqual(names, back.Maids.Select(m => m.Id + ":" + m.IdentityId + ":" + m.Level).ToArray());
            CollectionAssert.AreEqual(team, back.Lineup.All.Select(s => s.Role + "/" + s.Index + "=" + s.MaidId).ToArray());
            Assert.AreEqual(back.Maids.Count, back.Maids.Select(m => m.IdentityId).Distinct().Count());
            Assert.IsTrue(back.Lineup.MaidIds().All(id => back.MaidById(id) != null), "lineup ids stay valid");
            Assert.AreEqual(wages, Books.WagesFor(back), "the wage is recomputed from the saved lineup");
        }
    }
}
