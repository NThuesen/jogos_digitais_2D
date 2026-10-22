namespace Neko.Sim.Maids
{
    /// <summary>
    /// What the room looks like from one maid's position, right now.
    /// </summary>
    /// <remarks>
    /// The archetype rules are situational — Tsundere only works when it is busy, Mukuchi
    /// only plates well at a station — so deriving a speed needs the moment as well as the
    /// maid. Passing a snapshot rather than the whole shift keeps <see cref="MaidMath"/>
    /// testable with four numbers.
    /// </remarks>
    public readonly struct ShiftSnapshot
    {
        /// <summary>Fraction of seats taken, 0..1.</summary>
        public readonly float TableOccupancy;

        public readonly bool AtStation;

        public readonly bool Idle;

        /// <summary>Summed speed aura of the maids working near her. Usually zero.</summary>
        public readonly float NeighbourAura;

        public readonly bool Rushing;

        public ShiftSnapshot(float tableOccupancy, bool atStation, bool idle, float neighbourAura,
            bool rushing)
        {
            TableOccupancy = tableOccupancy;
            AtStation = atStation;
            Idle = idle;
            NeighbourAura = neighbourAura;
            Rushing = rushing;
        }

        public static ShiftSnapshot Still => new ShiftSnapshot(0f, false, true, 0f, false);
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

        public MaidDerived WithSpeeds(float moveSpeed, float greetSpeed, float serveSpeed,
            float cookSpeed) =>
            new MaidDerived(moveSpeed, greetSpeed, serveSpeed, cookSpeed, TipMultiplier,
                FumbleChance, DropChance, MoodDrainPerSecond);

        public MaidDerived WithTip(float tipMultiplier) =>
            new MaidDerived(MoveSpeed, GreetSpeed, ServeSpeed, CookSpeed, tipMultiplier,
                FumbleChance, DropChance, MoodDrainPerSecond);

        public MaidDerived WithFumble(float fumbleChance, float dropChance) =>
            new MaidDerived(MoveSpeed, GreetSpeed, ServeSpeed, CookSpeed, TipMultiplier,
                fumbleChance, dropChance, MoodDrainPerSecond);
    }
}
