using System.Collections.Generic;
using Neko.Sim.Cafe;

namespace Neko.Sim.Shift
{
    /// <summary>
    /// What the board needs to know about the world to rank a task.
    /// </summary>
    /// <remarks>
    /// An interface rather than a reference to the shift so that claiming can be tested with
    /// five lines of fake instead of a running café.
    /// </remarks>
    public interface ITaskContext
    {
        Cell CellOf(ShiftTask task);

        /// <summary>0 for "whenever", 1 for "this customer is about to walk out".</summary>
        float UrgencyOf(ShiftTask task);

        bool CanDo(Worker worker, ShiftTask task);
    }

    /// <summary>
    /// The open work, and the rule for who takes what.
    /// </summary>
    /// <remarks>
    /// Maids claim from here on their own. The player's lever is which maids are on shift and
    /// how good they are — not micro-commanding each one — which is what makes upgrading them
    /// the way the game gets easier.
    /// </remarks>
    public sealed class TaskBoard
    {
        /// <summary>
        /// Base worth of each kind of task, before urgency and distance.
        /// </summary>
        /// <remarks>
        /// Delivering outranks cooking because a cooked dish going cold is a customer already
        /// waiting, and bussing is last because a dirty table costs nothing until somebody
        /// needs it.
        /// </remarks>
        private static readonly Dictionary<TaskKind, float> BasePriority = new Dictionary<TaskKind, float>
        {
            { TaskKind.Deliver, 50f },
            { TaskKind.Cook, 45f },
            { TaskKind.TakeOrder, 40f },
            { TaskKind.Greet, 35f },
            { TaskKind.Bus, 10f },
        };

        /// <summary>How much a customer at the end of their patience is worth jumping for.</summary>
        public const float UrgencyWeight = 100f;

        /// <summary>Tie-break only: a step away should not outrank a real emergency.</summary>
        public const float DistancePenalty = 2f;

        private readonly List<ShiftTask> _open = new List<ShiftTask>();
        private int _nextId = 1;

        public IReadOnlyList<ShiftTask> Open => _open;

        public ShiftTask Add(TaskKind kind, int targetId)
        {
            var existing = Find(kind, targetId);
            if (existing != null) return existing;

            var task = new ShiftTask(_nextId++, kind, targetId);
            _open.Add(task);
            return task;
        }

        public ShiftTask Find(TaskKind kind, int targetId)
        {
            for (var i = 0; i < _open.Count; i++)
            {
                if (_open[i].Kind == kind && _open[i].TargetId == targetId) return _open[i];
            }

            return null;
        }

        /// <summary>Hands a task back, unclaimed, so somebody else can pick it up.</summary>
        public void Release(ShiftTask task)
        {
            if (task != null) task.ClaimedBy = -1;
        }

        /// <summary>Takes a finished — or no longer meaningful — task off the board.</summary>
        public void Complete(ShiftTask task)
        {
            if (task != null) _open.Remove(task);
        }

        /// <summary>Drops every task whose target has stopped existing.</summary>
        public void Prune(System.Func<ShiftTask, bool> stillMeaningful)
        {
            for (var i = _open.Count - 1; i >= 0; i--)
            {
                if (stillMeaningful(_open[i])) continue;
                _open.RemoveAt(i);
            }
        }

        public void ReleaseAllClaimedBy(int workerId)
        {
            for (var i = 0; i < _open.Count; i++)
            {
                if (_open[i].ClaimedBy == workerId) _open[i].ClaimedBy = -1;
            }
        }

        public void ReleaseAll()
        {
            for (var i = 0; i < _open.Count; i++) _open[i].ClaimedBy = -1;
        }

        public float Score(Worker worker, ShiftTask task, ITaskContext context)
        {
            var baseline = BasePriority.TryGetValue(task.Kind, out var value) ? value : 0f;
            var urgency = context.UrgencyOf(task) * UrgencyWeight;
            var distance = worker.DistanceTo(context.CellOf(task)) * DistancePenalty;
            return baseline + urgency - distance;
        }

        /// <summary>
        /// Gives the worker the best thing they are allowed to do, or null if there is nothing.
        /// </summary>
        public ShiftTask ClaimBest(Worker worker, ITaskContext context)
        {
            ShiftTask best = null;
            var bestScore = float.NegativeInfinity;

            for (var i = 0; i < _open.Count; i++)
            {
                var task = _open[i];
                if (task.Claimed) continue;
                if (!context.CanDo(worker, task)) continue;

                var score = Score(worker, task, context);
                task.Priority = score;

                if (score <= bestScore) continue;
                bestScore = score;
                best = task;
            }

            if (best == null) return null;

            best.ClaimedBy = worker.Id;
            worker.Claimed = best;
            worker.Progress = 0f;
            worker.Carrying = false;
            return best;
        }

        /// <summary>
        /// Forces a task onto a worker, which is what a click from the player means.
        /// </summary>
        public bool Assign(Worker worker, ShiftTask task, ITaskContext context)
        {
            if (task == null || !context.CanDo(worker, task)) return false;

            if (task.Claimed && task.ClaimedBy != worker.Id)
            {
                // Taking it off whoever had it is the point: the player is overriding.
                task.ClaimedBy = -1;
            }

            ReleaseAllClaimedBy(worker.Id);
            worker.Drop();

            task.ClaimedBy = worker.Id;
            worker.Claimed = task;
            return true;
        }
    }
}
