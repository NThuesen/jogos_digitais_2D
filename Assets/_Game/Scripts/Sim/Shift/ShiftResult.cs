using System.Collections.Generic;

namespace Neko.Sim.Shift
{
    /// <summary>
    /// What happened on the floor today, before any money is counted.
    /// </summary>
    /// <remarks>
    /// Operational numbers only. Wages, rent and whether the business survived are the books'
    /// business, not the shift's — keeping them apart is what lets two hundred seeded days be
    /// run against one lineup without touching a save.
    /// </remarks>
    public sealed class ShiftResult
    {
        public int Day;

        /// <summary>Money taken at the table, before costs.</summary>
        public int DishRevenue;

        public int Tips;

        public int Revenue => DishRevenue + Tips;

        public int Served;

        /// <summary>Parties who got the wrong plate at least once.</summary>
        public int WrongOrders;

        /// <summary>Dishes spoiled at the station.</summary>
        public int Burnt;

        /// <summary>Everything the kitchen put on a plate, including what never got eaten.</summary>
        public int DishesCooked;

        public int LeftAngry;
        public int TurnedAway;

        /// <summary>The lineup's chemistry on the day, nought to a hundred.</summary>
        public int ChemistryScore;

        /// <summary>
        /// How well the room was treated, nought to one.
        /// </summary>
        /// <remarks>
        /// Quality rather than volume, and it is what fichas are paid on. A day that takes a
        /// fortune by hustling everybody out of the door should buy fewer boxes than a quiet
        /// one served properly — the two currencies are supposed to pull apart.
        /// </remarks>
        public float Satisfaction;

        /// <summary>XP earned, by maid id.</summary>
        public readonly Dictionary<int, int> XpByMaid = new Dictionary<int, int>();

        /// <summary>Set when the café could not possibly serve anyone. Shown to the player.</summary>
        public string FailureReason;

        public bool Impossible => !string.IsNullOrEmpty(FailureReason);
    }
}
