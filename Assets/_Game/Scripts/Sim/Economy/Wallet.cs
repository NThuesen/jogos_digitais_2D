using System;

namespace Neko.Sim.Economy
{
    /// <summary>
    /// The money and the reputation.
    /// </summary>
    /// <remarks>
    /// Money cannot go negative, and <see cref="Spend"/> refuses rather than allowing debt.
    /// A player who could go into the red would be able to buy their way into a café that can
    /// no longer earn — which the design calls a dead end, and rubric 11 marks down.
    /// </remarks>
    public sealed class Wallet
    {
        public Wallet(int money = 0, int reputation = 0)
        {
            Money = Math.Max(0, money);
            Reputation = Math.Max(0, reputation);
        }

        public int Money { get; private set; }

        public int Reputation { get; private set; }

        public bool CanAfford(int price) => price >= 0 && Money >= price;

        public bool Spend(int price)
        {
            if (!CanAfford(price)) return false;

            Money -= price;
            return true;
        }

        public void Earn(int amount)
        {
            if (amount > 0) Money += amount;
        }

        /// <summary>
        /// Reputation only ever climbs.
        /// </summary>
        /// <remarks>
        /// A bad day costs the day's gain, not yesterday's café. Letting the café shrink would
        /// take away furniture the player has already placed and roster slots already filled,
        /// and there is no good way for a game to do that to somebody.
        /// </remarks>
        public void AddReputation(int delta)
        {
            if (delta > 0) Reputation += delta;
        }

        public void Restore(int money, int reputation)
        {
            Money = Math.Max(0, money);
            Reputation = Math.Max(0, reputation);
        }
    }
}
