namespace Neko.Sim.Maids
{
    /// <summary>
    /// One archetype's rule, as data.
    /// </summary>
    /// <remarks>
    /// Every field here is read by <see cref="MaidMath"/> or by the shift; none of them is
    /// decoration. The reason it is a table rather than nine subclasses is that the balance
    /// tests want to loop over all nine and assert things like "no archetype is useless" —
    /// which is only possible if the rules are values.
    /// </remarks>
    public sealed class ArchetypeProfile
    {
        public Archetype Kind;
        public string DisplayName;

        /// <summary>Where she starts, before any levelling.</summary>
        public Stats StartingStats;

        /// <summary>Relative pull on each stat when she levels. Need not sum to anything.</summary>
        public Stats GrowthWeights;

        public float MoveMultiplier = 1f;
        public float GreetMultiplier = 1f;
        public float ServeMultiplier = 1f;
        public float CookMultiplier = 1f;

        /// <summary>How fast a shift wears her down, relative to everyone else.</summary>
        public float MoodDrainMultiplier = 1f;

        /// <summary>Chance of spoiling a dish, before the Cook stat reduces it.</summary>
        public float BaseFumbleChance;

        /// <summary>
        /// Dojikko's signature. Deliberately not reduced by Cook: clumsiness is not a skill
        /// problem, and a maxed Dojikko who stopped dropping plates would lose her identity
        /// along with the patience bonus every drop gives the room.
        /// </summary>
        public float ClumsyDropChance;

        public bool NeverFumbles;

        public bool WillBusTables = true;

        /// <summary>Tsundere: the occupancy at which she stops sulking and starts working.</summary>
        public float CrowdedThreshold = 1.1f;

        public float CrowdedSpeedBonus;

        public float IdleTipPenalty;

        /// <summary>Onee-san: what she grants to maids working near her.</summary>
        public float NeighbourSpeedAura;

        /// <summary>Onee-san: the share of her XP that goes to the roster's weakest maid.</summary>
        public float XpShareFraction;

        /// <summary>Ojou-sama: how much more often a VIP walks in.</summary>
        public float VipWeightMultiplier = 1f;

        /// <summary>Gourmand: the chance a finished dish never reaches the table.</summary>
        public float EatChance;

        /// <summary>Mukuchi: plating counts her Charm twice while she is at a station.</summary>
        public bool CharmDoublesAtStation;

        /// <summary>Idol: the active that refills the room's patience.</summary>
        public bool HasEncore;

        public float EncoreCooldownSeconds;
        public float EncorePatienceRestored;

        /// <summary>The colour that marks her everywhere she appears.</summary>
        public string AccentHex;

        /// <summary>What breaks her outline, which is all a 64-pixel figure has.</summary>
        public string SilhouetteTell;

        /// <summary>One line of flavour, shown on the roster. Portuguese: the player reads it.</summary>
        public string Flavour;
    }
}
