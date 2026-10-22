using Neko.Sim.Cafe;
using Neko.Sim.Maids;

namespace Neko.Sim.Shift
{
    public enum WorkerKind
    {
        Maid = 0,

        /// <summary>The player. One of him, competent at everything, never tires.</summary>
        Manager,
    }

    /// <summary>Somebody who can carry out a task. A maid, or the player's own avatar.</summary>
    /// <remarks>
    /// The manager is modelled as a worker rather than as a special case so that a shift with
    /// an empty roster is still a playable shift. That is not a hypothetical: it is where
    /// every new game starts.
    /// </remarks>
    public sealed class Worker
    {
        /// <summary>Within one tile counts as "there", so a worker never jitters on a target.</summary>
        public const float ArrivalRadius = 0.6f;

        public const float BreakSeconds = 18f;

        /// <summary>What a maid comes back from the break room with.</summary>
        public const float MoodAfterBreak = 0.4f;

        public const float RushSeconds = 6f;

        public const float RushMoodCost = 0.15f;

        public int Id;
        public WorkerKind Kind;
        public Maid Maid;

        public float X;
        public float Y;

        public ShiftTask Claimed;

        /// <summary>0..1 through the current task, once the worker has actually arrived.</summary>
        public float Progress;

        /// <summary>Deliver runs in two legs: fetch the plate, then carry it to the table.</summary>
        public bool Carrying;

        public bool OnBreak;

        /// <summary>
        /// Her mood hit zero while she was holding something.
        /// </summary>
        /// <remarks>
        /// She finishes the task and then rests, rather than dropping it where she stands.
        /// Releasing mid-task would strand a cooked plate at a station and lose the customer
        /// it was for, which punishes the player for a maid being tired twice over.
        /// </remarks>
        public bool NeedsBreak;

        public float BreakRemaining;
        public float RushRemaining;
        public float EncoreCooldown;

        /// <summary>Mise en Place and Second Wind each fire once a day, and this is how.</summary>
        public bool UsedMiseEnPlace;

        public bool UsedSecondWind;

        public string Name => Kind == WorkerKind.Manager ? "Gerente" : Maid.Name;

        public bool IsManager => Kind == WorkerKind.Manager;

        /// <summary>Ojou-sama will not, and that is a real cost of rostering her.</summary>
        public bool WillBusTables => IsManager || Maid.Profile.WillBusTables;

        public bool HasEncore => !IsManager && Maid.Profile.HasEncore;

        public bool Available => !OnBreak;

        public Cell Cell => new Cell((int)System.Math.Round(X), (int)System.Math.Round(Y));

        public float DistanceTo(Cell cell)
        {
            var dx = cell.X - X;
            var dy = cell.Y - Y;
            return (float)System.Math.Sqrt(dx * dx + dy * dy);
        }

        public void Drop()
        {
            Claimed = null;
            Progress = 0f;
            Carrying = false;
        }
    }
}
