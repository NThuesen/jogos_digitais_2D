using System;

namespace Neko.Sim.Economy
{
    /// <summary>
    /// The two currencies, and the one that can kill you.
    /// </summary>
    /// <remarks>
    /// Money is allowed to go under, and <see cref="Pay"/> is the only way it ever does. That
    /// is the whole losing condition: the business runs out of cash and closes. Fichas never
    /// go under, because a player who could not afford a box has simply not earned one yet.
    ///
    /// Reputation only ever climbs. A bad day costs the day's gain, not yesterday's café —
    /// taking back a room the player has already filled is not something a game should do.
    /// </remarks>
    public sealed class Wallet
    {
        public Wallet(int money = 0, int reputation = 0, int points = 0)
        {
            Money = money;
            Reputation = Math.Max(0, reputation);
            Points = Math.Max(0, points);
        }

        /// <summary>Can go negative, exactly once, on the day the business fails.</summary>
        public int Money { get; private set; }

        public int Reputation { get; private set; }

        /// <summary>Fichas. Boxes, and nothing else.</summary>
        public int Points { get; private set; }

        public bool CanAfford(int price) => price >= 0 && Money >= price;

        public bool CanAffordPoints(int price) => price >= 0 && Points >= price;

        /// <summary>A purchase. Refuses rather than allowing debt.</summary>
        public bool Spend(int price)
        {
            if (!CanAfford(price)) return false;

            Money -= price;
            return true;
        }

        public bool SpendPoints(int price)
        {
            if (!CanAffordPoints(price)) return false;

            Points -= price;
            return true;
        }

        /// <summary>
        /// A bill. Taken whether or not it can be covered.
        /// </summary>
        /// <remarks>
        /// Wages and rent are not optional, and refusing them would quietly make the game
        /// unloseable. This is the only path to a negative balance in the whole game.
        /// </remarks>
        public void Pay(int amount)
        {
            if (amount > 0) Money -= amount;
        }

        public void Earn(int amount)
        {
            if (amount > 0) Money += amount;
        }

        public void AddPoints(int amount)
        {
            if (amount > 0) Points += amount;
        }

        public void AddReputation(int delta)
        {
            if (delta > 0) Reputation += delta;
        }

        public void Restore(int money, int reputation, int points)
        {
            Money = money;
            Reputation = Math.Max(0, reputation);
            Points = Math.Max(0, points);
        }
    }
}
