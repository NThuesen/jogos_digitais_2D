namespace Neko.Sim.Maids
{
    /// <summary>
    /// What a worker is doing, in terms the maid rules can read.
    /// </summary>
    /// <remarks>
    /// A plain mirror of the shift's own task kinds, declared here so that archetype and skill
    /// rules can depend on the work without <c>Maids</c> depending on <c>Shift</c>. The
    /// simulation's dependencies run one way on purpose.
    /// </remarks>
    public enum WorkKind
    {
        None = 0,
        Greet,
        TakeOrder,
        Cook,
        Deliver,
        Bus,
    }

    /// <summary>
    /// What the room looks like from one maid's position, right now.
    /// </summary>
    /// <remarks>
    /// The archetype and skill rules are situational — Tsundere only works when it is busy,
    /// Mukuchi only plates well at a station, Clean Sweep only helps with tables — so deriving
    /// a speed needs the moment as well as the maid. Passing a snapshot rather than the whole
    /// shift keeps <see cref="MaidMath"/> testable with five numbers.
    /// </remarks>
    public readonly struct ShiftSnapshot
    {
        /// <summary>Fraction of seats taken, 0..1.</summary>
        public readonly float TableOccupancy;

        public readonly WorkKind Work;

        /// <summary>Summed speed aura of the maids working near her. Usually zero.</summary>
        public readonly float NeighbourAura;

        public readonly bool Rushing;

        /// <summary>Tickets on the rail. Batch Cooking pays off exactly when this is high.</summary>
        public readonly int QueuedTickets;

        public ShiftSnapshot(float tableOccupancy, WorkKind work, float neighbourAura = 0f,
            bool rushing = false, int queuedTickets = 0)
        {
            TableOccupancy = tableOccupancy;
            Work = work;
            NeighbourAura = neighbourAura;
            Rushing = rushing;
            QueuedTickets = queuedTickets;
        }

        public bool AtStation => Work == WorkKind.Cook;

        public bool Idle => Work == WorkKind.None;

        public static ShiftSnapshot Still => new ShiftSnapshot(0f, WorkKind.None);
    }

    /// <summary>
    /// The numbers the shift actually uses. Speeds are multipliers on a task's duration,
    /// except <see cref="MoveSpeed"/>, which is tiles per second.
    /// </summary>
    public readonly struct MaidDerived
    {
        public readonly float MoveSpeed;
        public readonly float GreetSpeed;
        public readonly float ServeSpeed;
        public readonly float CookSpeed;
        public readonly float TipMultiplier;

        /// <summary>Chance of spoiling a dish. Falls as the Cook stat rises.</summary>
        public readonly float FumbleChance;

        /// <summary>Chance of dropping a carried plate. Dojikko's, and nobody else's.</summary>
        public readonly float DropChance;

        public readonly float MoodDrainPerSecond;

        public MaidDerived(float moveSpeed, float greetSpeed, float serveSpeed, float cookSpeed,
            float tipMultiplier, float fumbleChance, float dropChance, float moodDrainPerSecond)
        {
            MoveSpeed = moveSpeed;
            GreetSpeed = greetSpeed;
            ServeSpeed = serveSpeed;
            CookSpeed = cookSpeed;
            TipMultiplier = tipMultiplier;
            FumbleChance = fumbleChance;
            DropChance = dropChance;
            MoodDrainPerSecond = moodDrainPerSecond;
        }

        public MaidDerived Scaled(float move = 1f, float greet = 1f, float serve = 1f,
            float cook = 1f, float tip = 1f) =>
            new MaidDerived(MoveSpeed * move, GreetSpeed * greet, ServeSpeed * serve,
                CookSpeed * cook, TipMultiplier * tip, FumbleChance, DropChance,
                MoodDrainPerSecond);

        public MaidDerived WithFumble(float fumbleChance, float dropChance) =>
            new MaidDerived(MoveSpeed, GreetSpeed, ServeSpeed, CookSpeed, TipMultiplier,
                fumbleChance, dropChance, MoodDrainPerSecond);
    }
}
