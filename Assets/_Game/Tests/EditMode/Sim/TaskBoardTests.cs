using System.Collections.Generic;
using Neko.Sim.Cafe;
using Neko.Sim.Maids;
using Neko.Sim.Shift;
using NUnit.Framework;

namespace Neko.Tests.Sim
{
    public sealed class TaskBoardTests
    {
        /// <summary>Five lines of world, so claiming can be tested without a running café.</summary>
        private sealed class FakeContext : ITaskContext
        {
            public readonly Dictionary<int, Cell> Cells = new Dictionary<int, Cell>();
            public readonly Dictionary<int, float> Urgency = new Dictionary<int, float>();

            public Cell CellOf(ShiftTask task) =>
                Cells.TryGetValue(task.Id, out var cell) ? cell : new Cell(0, 0);

            public float UrgencyOf(ShiftTask task) =>
                Urgency.TryGetValue(task.Id, out var value) ? value : 0f;

            public bool CanDo(Worker worker, ShiftTask task) => worker.Allows(task.Kind);
        }

        private static Worker MaidWorker(Archetype archetype = Archetype.Genki, float x = 0f,
            float y = 0f, Neko.Sim.Team.Role role = Neko.Sim.Team.Role.Floor) =>
            new Worker
            {
                Id = 1,
                Maid = Maid.Create(1, "Teste", archetype),
                Role = role,
                X = x,
                Y = y,
            };

        private static Worker Second(Neko.Sim.Team.Role role = Neko.Sim.Team.Role.Floor) =>
            new Worker
            {
                Id = 2,
                Maid = Maid.Create(2, "Outra", Archetype.Kuudere),
                Role = role,
            };

        [Test]
        public void AddingTheSameTaskTwiceReturnsTheSameTask()
        {
            var board = new TaskBoard();

            var first = board.Add(TaskKind.Bus, 3);
            var second = board.Add(TaskKind.Bus, 3);

            Assert.AreSame(first, second);
            Assert.AreEqual(1, board.Open.Count);
        }

        [Test]
        public void ClaimBestPrefersTheMostUrgentCustomerOverADirtyTable()
        {
            var board = new TaskBoard();
            var context = new FakeContext();
            var bus = board.Add(TaskKind.Bus, 0);
            var order = board.Add(TaskKind.TakeOrder, 7);
            context.Urgency[order.Id] = 0.9f;

            var claimed = board.ClaimBest(MaidWorker(), context);

            Assert.AreSame(order, claimed);
            Assert.IsFalse(bus.Claimed);
        }

        [Test]
        public void ATaskIsClaimedByExactlyOneWorker()
        {
            var board = new TaskBoard();
            var context = new FakeContext();
            board.Add(TaskKind.TakeOrder, 1);

            Assert.IsNotNull(board.ClaimBest(MaidWorker(), context));
            Assert.IsNull(board.ClaimBest(Second(), context), "the only task was already taken");
        }

        [Test]
        public void OjousamaIsNeverOfferedABusTask()
        {
            var board = new TaskBoard();
            board.Add(TaskKind.Bus, 0);

            Assert.IsNull(board.ClaimBest(
                MaidWorker(Archetype.Ojousama), new FakeContext()));
            Assert.IsNotNull(board.ClaimBest(MaidWorker(), new FakeContext()));
        }

        [Test]
        public void ReleasedTasksBecomeClaimableAgain()
        {
            var board = new TaskBoard();
            var context = new FakeContext();
            var task = board.Add(TaskKind.Cook, 1);
            board.ClaimBest(MaidWorker(), context);

            board.Release(task);

            Assert.IsFalse(task.Claimed);
            Assert.IsNotNull(board.ClaimBest(Second(Neko.Sim.Team.Role.Kitchen), context));
        }

        [Test]
        public void ClaimBestBreaksTiesByDistance()
        {
            var board = new TaskBoard();
            var context = new FakeContext();
            var near = board.Add(TaskKind.Bus, 0);
            var far = board.Add(TaskKind.Bus, 1);
            context.Cells[near.Id] = new Cell(1, 0);
            context.Cells[far.Id] = new Cell(9, 0);

            Assert.AreSame(near, board.ClaimBest(MaidWorker(), context));
        }

        [Test]
        public void UrgencyOutranksDistance()
        {
            var board = new TaskBoard();
            var context = new FakeContext();
            var near = board.Add(TaskKind.TakeOrder, 0);
            var farButDying = board.Add(TaskKind.TakeOrder, 1);
            context.Cells[near.Id] = new Cell(1, 0);
            context.Cells[farButDying.Id] = new Cell(12, 0);
            context.Urgency[farButDying.Id] = 1f;

            Assert.AreSame(farButDying, board.ClaimBest(MaidWorker(), context));
        }

        [Test]
        public void ReleaseAllClaimedByFreesOnlyThatWorkersTasks()
        {
            var board = new TaskBoard();
            var context = new FakeContext();
            var mine = board.Add(TaskKind.Cook, 1);
            var theirs = board.Add(TaskKind.Cook, 2);
            mine.ClaimedBy = 1;
            theirs.ClaimedBy = 2;

            board.ReleaseAllClaimedBy(1);

            Assert.IsFalse(mine.Claimed);
            Assert.IsTrue(theirs.Claimed);
            Assert.IsNotNull(context);
        }

        [Test]
        public void CompleteTakesATaskOffTheBoard()
        {
            var board = new TaskBoard();
            var task = board.Add(TaskKind.Bus, 0);

            board.Complete(task);

            Assert.IsEmpty(board.Open);
        }

        [Test]
        public void PruneDropsTasksWhoseTargetIsGone()
        {
            var board = new TaskBoard();
            board.Add(TaskKind.Bus, 0);
            board.Add(TaskKind.Bus, 1);

            board.Prune(task => task.TargetId == 0);

            Assert.AreEqual(1, board.Open.Count);
            Assert.AreEqual(0, board.Open[0].TargetId);
        }

    }
}
