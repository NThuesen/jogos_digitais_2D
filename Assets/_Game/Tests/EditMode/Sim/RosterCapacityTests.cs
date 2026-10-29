using System.Linq;
using Neko.Sim.Core;
using Neko.Sim.Economy;
using Neko.Sim.Save;
using Neko.Sim.Team;
using NUnit.Framework;

namespace Neko.Tests.Sim
{
    public sealed class RosterCapacityTests
    {
        [TestCase(1, 4)] [TestCase(2, 5)] [TestCase(3, 6)] [TestCase(4, 7)]
        [TestCase(5, 8)] [TestCase(6, 9)] [TestCase(7, 10)] [TestCase(8, 11)]
        [TestCase(9, 11)] [TestCase(10, 11)]
        public void EachLevelHasRoomForItsLineupAndOneAdditionalMaid(int level, int expected)
        {
            Assert.AreEqual(expected, CafeLevelTable.UnlocksAt(level).RosterCap);
            Assert.AreEqual(Lineup.SlotsFor(level).Total + 1, CafeLevelTable.UnlocksAt(level).RosterCap);
        }

        [Test]
        public void FirstSimpleBoxAddsFourthMaidWithoutChangingAssignmentsAndPaysHerWages()
        {
            var p = Profile.NewGame(1);
            Assert.AreEqual(3, p.Maids.Count);
            Assert.AreEqual(4, p.Unlocks.RosterCap);
            var assignments = Assignments(p);
            int wages = Books.WagesFor(p), money = p.Wallet.Money;
            var result = Lootbox.Open(p, "box_simple", new Rng(73));
            Assert.IsTrue(result.Ok);
            Assert.IsFalse(result.Cards.Single().Duplicate);
            var maid = result.Cards.Single().Maid;
            Assert.AreEqual(4, p.Maids.Count);
            Assert.AreSame(maid, p.MaidById(maid.Id));
            CollectionAssert.AreEqual(assignments, Assignments(p));
            Assert.IsFalse(p.Lineup.Contains(maid.Id));
            Assert.AreEqual(wages + maid.Salary, Books.WagesFor(p));
            Assert.AreEqual(money, p.Wallet.Money);
            Assert.AreEqual(0, p.Wallet.Points);
        }

        [Test]
        public void MultiCardBoxAddsOnlyAvailableCardsAndConvertsRemainderWithoutChangingDraws()
        {
            var room = Profile.NewGame(29);
            var full = Profile.NewGame(29);
            full.Add(Lootbox.Mint(full, Rarity.Common, new Rng(31)));
            room.Wallet.AddPoints(1000);
            full.Wallet.AddPoints(1000);
            var a = new Rng(991); var b = new Rng(991);
            var mixed = Lootbox.Open(room, "box_silver", a);
            var converted = Lootbox.Open(full, "box_silver", b);
            Assert.IsTrue(mixed.Ok); Assert.IsTrue(converted.Ok);
            Assert.AreEqual(4, room.Maids.Count); Assert.AreEqual(4, full.Maids.Count);
            Assert.IsFalse(mixed.Cards[0].Duplicate);
            Assert.IsTrue(mixed.Cards.Skip(1).All(c => c.Duplicate));
            Assert.IsTrue(converted.Cards.All(c => c.Duplicate));
            CollectionAssert.AreEqual(mixed.Cards.Select(c => c.Maid.Rarity), converted.Cards.Select(c => c.Maid.Rarity));
            Assert.AreEqual(a.State, b.State);
            Assert.AreEqual(1040 - Lootbox.ById("box_silver").PricePoints + mixed.Cards.Sum(c => c.PointsBack), room.Wallet.Points);
        }

        [Test]
        public void HiringAtCapacityCostsNothingAndDismissalReopensTheSameVacancy()
        {
            var p = Profile.NewGame(41);
            p.Wallet.Earn(20000);
            var board = new ApplicantBoard();
            board.Refresh(new Rng(43), p.CafeLevel, p.NextMaidId);
            var assignments = Assignments(p);
            var before = p.Wallet.Money;
            var price = board.Candidates[0].Price;
            var hired = board.Hire(p, 0);
            Assert.IsTrue(hired.Ok); Assert.AreEqual(4, p.Maids.Count);
            Assert.AreEqual(before - price, p.Wallet.Money);
            CollectionAssert.AreEqual(assignments, Assignments(p));
            Assert.IsFalse(p.Lineup.Contains(hired.Maid.Id));
            var candidate = board.Candidates[1];
            before = p.Wallet.Money;
            int nextId = p.NextMaidId;
            var refused = board.Hire(p, 1);
            Assert.IsFalse(refused.Ok); StringAssert.Contains("4", refused.Refusal);
            Assert.AreEqual(before, p.Wallet.Money); Assert.AreEqual(nextId, p.NextMaidId);
            Assert.AreSame(candidate, board.Candidates[1]);
            int points = p.Wallet.Points, wages = Books.WagesFor(p);
            Assert.IsTrue(p.Dismiss(hired.Maid.Id));
            Assert.AreEqual(points + hired.Maid.Severance, p.Wallet.Points);
            Assert.AreEqual(wages - hired.Maid.Salary, Books.WagesFor(p));
            Assert.IsTrue(board.Hire(p, 1).Ok);
            Assert.AreEqual(before - candidate.Price, p.Wallet.Money);
            CollectionAssert.AreEqual(assignments, Assignments(p));
            CollectionAssert.AllItemsAreUnique(p.Maids.Select(m => m.Id));
        }

        [Test]
        public void UnlockingLevelsIncreasesCapacityWithoutAssigningBenchOrFillingEmptyPositions()
        {
            var p = Profile.NewGame(59);
            p.Lineup.Clear();
            p.Lineup.Assign(Role.Floor, 0, p.Maids[1].Id);
            var ids = p.Maids.Select(m => m.Id).ToArray();
            for (int level = 1; level <= 10; level++)
            {
                p.Wallet.AddReputation(CafeLevelTable.ReputationForLevel(level) - p.Wallet.Reputation);
                p.ApplyUnlocks();
                Assert.AreEqual(level, p.CafeLevel);
                Assert.AreEqual(p.Lineup.All.Count + 1, p.Unlocks.RosterCap);
                CollectionAssert.AreEqual(ids, p.Maids.Select(m => m.Id));
                CollectionAssert.AreEqual(new[] { ids[1] }, p.Lineup.MaidIds());
            }
        }

        [Test]
        public void SaveWrittenBeforeCapacityChangePreservesRosterEmptySlotsAndBenchWithoutMigration()
        {
            // Generated by revision 6187683 before changing the cap: seed 73, L1 cap 2,
            // three maids, only the floor maid assigned. Payload is frozen, not re-encoded input.
            const string legacy = "NEKO2:c2VlZD03MwpkYXk9MQptb25leT02MDAKcmVwPTAKcG9pbnRzPTQwCmJhbmtydXB0PTAKbmV4dG1haWQ9NApncmlkPTd4NQppdGVtPXRhYmxlXzIsMywxLDAKaXRlbT10YWJsZV8yLDMsMywwCml0ZW09c3RvdmUsNSwyLDAKaXRlbT1wbGFudCwxLDAsMAptYWlkPTF8WXVraXxEb2ppa2tvfDF8MHwwfDB8MjgsMjMsMjcsMjJ8LCwsfHwxfDB8MHwxfDF8Cm1haWQ9MnxTYXlhfEdlbmtpfDF8MHwwfDB8MjksMjAsMjcsMjF8LCwsfHwxfDB8MXwxfDN8Cm1haWQ9M3xLdXJvfEt1dWRlcmV8MXwwfDB8MHwyNywyMywyNSwyMXwsLCx8fDF8MHwyfDR8M3wKc2xvdD0xLDAsMgpwbGFjZT0KZ2Vhcj0KcmVjb3Jkcz0wLDAsMCwwLDAsMAo=:04d9c747";
            var loaded = SaveCodec.Decode(legacy);
            Assert.AreEqual(LoadStatus.Ok, loaded.Status);
            Assert.AreEqual(4, loaded.Profile.Unlocks.RosterCap);
            Assert.AreEqual(3, loaded.Profile.Maids.Count);
            CollectionAssert.AreEqual(new[] { 2 }, loaded.Profile.Lineup.MaidIds());
            Assert.AreEqual(2, loaded.Profile.Maids.Count(m => !loaded.Profile.Lineup.Contains(m.Id)));
            Assert.AreEqual(legacy, SaveCodec.Encode(loaded.Profile));
        }

        private static int[] Assignments(Profile p) => p.Lineup.All.Select(s => s.MaidId).ToArray();
    }
}
