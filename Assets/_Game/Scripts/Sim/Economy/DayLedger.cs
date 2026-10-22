using System.Collections.Generic;

namespace Neko.Sim.Economy
{
    /// <summary>One line on the dashboard.</summary>
    public readonly struct LedgerLine
    {
        public readonly string Label;
        public readonly int Amount;

        /// <summary>True for a cost, so the view can colour and sign it without guessing.</summary>
        public readonly bool Outgoing;

        public LedgerLine(string label, int amount, bool outgoing = false)
        {
            Label = label;
            Amount = amount;
            Outgoing = outgoing;
        }
    }

    /// <summary>
    /// The day's books, as the player reads them.
    /// </summary>
    /// <remarks>
    /// This is the screen the game is judged on. The brief asked for a dashboard the player
    /// can look at and decide whether the day was good or bad, so nothing here is a score:
    /// every line is money, and the only verdict is the one at the bottom.
    /// </remarks>
    public sealed class DayLedger
    {
        public int Day;

        // Takings ----------------------------------------------------------------------

        public int Dishes;
        public int Tips;

        public int Revenue => Dishes + Tips;

        // Costs ------------------------------------------------------------------------

        public int Wages;
        public int Rent;
        public int Ingredients;

        public int Costs => Wages + Rent + Ingredients;

        public int Profit => Revenue - Costs;

        // What happened ----------------------------------------------------------------

        public int Served;
        public int WrongOrders;
        public int Burnt;
        public int LeftAngry;
        public int TurnedAway;
        public int DishesCooked;

        /// <summary>Nought to one. What the fichas were paid on.</summary>
        public float Satisfaction;

        public int ChemistryScore;

        // Afterwards -------------------------------------------------------------------

        public int PointsEarned;
        public int BalanceBefore;
        public int BalanceAfter;

        /// <summary>The café could not open at all. Shown instead of the numbers.</summary>
        public string FailureReason;

        /// <summary>The business did not survive the day.</summary>
        public bool Bankrupt;

        /// <summary>Still standing, but one more day like this would end it.</summary>
        public bool Warning;

        public readonly Dictionary<int, int> XpByMaid = new Dictionary<int, int>();

        /// <summary>Maid id to levels gained today, for the dashboard.</summary>
        public readonly Dictionary<int, int> LevelUps = new Dictionary<int, int>();

        public IReadOnlyList<LedgerLine> Incoming() => new[]
        {
            new LedgerLine("Pratos servidos", Dishes),
            new LedgerLine("Gorjetas", Tips),
        };

        public IReadOnlyList<LedgerLine> Outgoing() => new[]
        {
            new LedgerLine("Salários", Wages, true),
            new LedgerLine("Aluguel", Rent, true),
            new LedgerLine("Ingredientes", Ingredients, true),
        };

        /// <summary>Portuguese, and the first thing on the screen.</summary>
        public string Verdict()
        {
            if (Bankrupt) return "Falência";
            if (!string.IsNullOrEmpty(FailureReason)) return "O café não abriu";
            if (Profit <= 0) return "Prejuízo";
            if (Satisfaction >= 0.85f && Profit > 0) return "Dia excelente";
            if (Satisfaction < 0.5f) return "Lucrou no sufoco";
            return "Dia bom";
        }
    }
}
