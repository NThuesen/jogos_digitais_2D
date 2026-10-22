using System.Collections.Generic;
using Neko.Sim.Cafe;
using Neko.Sim.Core;
using Neko.Sim.Maids;
using Neko.Sim.Service;
using Neko.Sim.Team;

namespace Neko.Sim.Shift
{
    /// <summary>A customer who arrives at a fixed moment, rather than off the curve.</summary>
    /// <remarks>
    /// Written for the tests — "one Regular at two seconds" is a far clearer setup than an
    /// arrival rate and a seed — and then kept, because the first day of a new game wants
    /// exactly the same thing: a scripted, gentle opening the player cannot lose.
    /// </remarks>
    public readonly struct ScriptedArrival
    {
        public readonly float AtSeconds;
        public readonly CustomerType Kind;

        public ScriptedArrival(float atSeconds, CustomerType kind)
        {
            AtSeconds = atSeconds;
            Kind = kind;
        }
    }

    /// <summary>
    /// Everything the day needs, fixed before the doors open.
    /// </summary>
    /// <remarks>
    /// Nothing in here changes once a <see cref="Shift"/> has been built from it. That is the
    /// pivot in one sentence: the player's whole turn happens before this object exists.
    /// </remarks>
    public sealed class ShiftSetup
    {
        public int Day = 1;
        public int CafeLevel = 1;
        public CafeGrid Grid;

        /// <summary>Who is on the card today. Only maids standing in a slot appear here.</summary>
        public IReadOnlyList<Maid> Roster = new Maid[0];

        public Lineup Lineup;

        /// <summary>Worked out from the lineup before the day starts, and then held.</summary>
        public TeamChemistry Chemistry;

        public float DurationSeconds = 90f;
        public float ArrivalsPerSecond = 0.25f;
        public uint Seed = 1u;

        /// <summary>How many parties will wait at the door before the rest are turned away.</summary>
        public int DoorCapacity = 4;

        /// <summary>Day difficulty: below 1 means everybody is in more of a hurry.</summary>
        public float PatienceScale = 1f;

        /// <summary>When set, arrivals come from here instead of from the curve.</summary>
        public IReadOnlyList<ScriptedArrival> ScriptedArrivals;

        /// <summary>Seconds after closing before the shift gives up on its last customers.</summary>
        public float ClosingGraceSeconds = 45f;

        public ThemeTag CafeTheme = ThemeTag.None;
    }
}
