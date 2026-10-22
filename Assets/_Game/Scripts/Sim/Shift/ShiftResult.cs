using System.Collections.Generic;

namespace Neko.Sim.Shift
{
    /// <summary>What a day came to.</summary>
    public sealed class ShiftResult
    {
        public int Day;
        public int Earned;
        public int Quota;
        public bool QuotaMet;
        public int Served;
        public int Angry;
        public int TurnedAway;
        public int ReputationDelta;

        /// <summary>XP earned, by maid id. The manager earns none; he is already the ceiling.</summary>
        public readonly Dictionary<int, int> XpByMaid = new Dictionary<int, int>();

        /// <summary>
        /// A day that could not be won, as opposed to one that was lost.
        /// </summary>
        /// <remarks>
        /// Missing a quota is a slow day, not a game over: the bonus is lost and the curve
        /// does not advance, so a player who is behind can always grind their way forward.
        /// The design calls a dead end an anti-goal, and this flag is how that promise is kept.
        /// </remarks>
        public bool SlowDay;

        /// <summary>Set when the café could not possibly serve anyone. Shown to the player.</summary>
        public string FailureReason;
    }
}
