using Neko.Sim.Cafe;
using Neko.Sim.Maids;
using Neko.Sim.Team;

namespace Neko.Sim.Shift
{
    /// <summary>
    /// One maid, at one position, for one day.
    /// </summary>
    /// <remarks>
    /// There is no player avatar any more. Every worker on the floor is a card the player put
    /// in a slot before the doors opened, and nothing they do during the day can be
    /// countermanded — which is what makes the lineup screen the whole game rather than a
    /// menu attached to one.
    /// </remarks>
    public sealed class Worker
    {
        /// <summary>Within one tile counts as "there", so a worker never jitters on a target.</summary>
        public const float ArrivalRadius = 0.6f;

        public const float BreakSeconds = 18f;

        /// <summary>What a maid comes back from the break room with.</summary>
        public const float MoodAfterBreak = 0.4f;

        public int Id;
        public Maid Maid;

        /// <summary>The slot she was put in. Not necessarily where she belongs.</summary>
        public Role Role;

        /// <summary>Her chemistry in this lineup, nought to ten.</summary>
        public int Chemistry;

        public bool InPosition;

        /// <summary>
        /// Where she stands when there is nothing to do.
        /// </summary>
        /// <remarks>
        /// Her own spot, not her position's. Sending every cook to the same tile drew a
        /// six-strong squad as a café with two people in it — the views were all there,
        /// stacked exactly on top of one another.
        /// </remarks>
        public float StationX;

        public float StationY;

        public float X;
        public float Y;

        public ShiftTask Claimed;

        /// <summary>0..1 through the current task, once she has actually arrived.</summary>
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

        /// <summary>Mise en Place and Second Wind each fire once a day, and this is how.</summary>
        public bool UsedMiseEnPlace;

        public bool UsedSecondWind;

        public float EncoreCooldown;

        public string Name => Maid != null ? Maid.Name : "?";

        public RoleProfile Position => RoleTable.For(Role);

        /// <summary>Stage and Management shape the day without touching a single task.</summary>
        public bool TakesTasks => Position.TakesTasks;

        /// <summary>Ojou-sama will not, and that is a real cost of fielding her.</summary>
        public bool WillBusTables => Maid == null || Maid.Profile.WillBusTables;

        public bool HasEncore => Maid != null && Maid.Profile.HasEncore;

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

        /// <summary>
        /// Whether this position is allowed to do this kind of work.
        /// </summary>
        /// <remarks>
        /// Hard boundaries, not preferences. A cook who wandered out to clear tables when the
        /// floor got busy would quietly undo the one decision the player made, and the lineup
        /// would stop mattering the moment the café did.
        /// </remarks>
        public bool Allows(TaskKind kind)
        {
            if (!TakesTasks) return false;

            switch (Role)
            {
                case Role.Reception:
                    return kind == TaskKind.Greet;

                case Role.Floor:
                    return kind == TaskKind.TakeOrder || kind == TaskKind.Deliver ||
                           (kind == TaskKind.Bus && WillBusTables);

                case Role.Kitchen:
                    return kind == TaskKind.Cook;

                default:
                    return false;
            }
        }
    }
}
