namespace Neko.Sim.Shift
{
    public enum TaskKind
    {
        /// <summary>Walk a party at the door to a free seat.</summary>
        Greet = 0,

        /// <summary>Take the order of a seated party with its hand up.</summary>
        TakeOrder,

        /// <summary>Cook one ticket at a station that supports it.</summary>
        Cook,

        /// <summary>Carry a finished dish from its station to the table that ordered it.</summary>
        Deliver,

        /// <summary>Clear a table so somebody else can sit at it.</summary>
        Bus,
    }

    /// <summary>One piece of work on the floor, and who has it.</summary>
    public sealed class ShiftTask
    {
        public readonly int Id;
        public readonly TaskKind Kind;

        /// <summary>A customer id, a ticket id or a seat index, depending on the kind.</summary>
        public readonly int TargetId;

        public int ClaimedBy = -1;

        /// <summary>Refreshed every tick by the shift, because urgency changes under it.</summary>
        public float Priority;

        public ShiftTask(int id, TaskKind kind, int targetId)
        {
            Id = id;
            Kind = kind;
            TargetId = targetId;
        }

        public bool Claimed => ClaimedBy >= 0;
    }
}
