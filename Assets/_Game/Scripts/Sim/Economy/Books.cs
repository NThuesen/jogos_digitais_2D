using System;
using Neko.Sim.Core;
using Neko.Sim.Progress;
using Neko.Sim.Save;
using Neko.Sim.Shift;
using Neko.Sim.Team;

namespace Neko.Sim.Economy
{
    /// <summary>
    /// Closes the day: pays everyone, counts the fichas, and decides whether the business
    /// is still standing.
    /// </summary>
    /// <remarks>
    /// Separate from the shift on purpose. The floor produces plates and goodwill; this
    /// turns them into money and a verdict. Keeping the two apart is what lets a lineup be
    /// tested against two hundred seeded days without a save anywhere near it.
    /// </remarks>
    public static class Books
    {
        /// <summary>Daily rent, before the room's size is taken into account.</summary>
        public const int BaseRent = 60;

        /// <summary>Added per tile of floor. A bigger café is a bigger bill.</summary>
        public const float RentPerTile = 2.4f;

        /// <summary>What a dish costs to make, as a fraction of what it sells for.</summary>
        public const float IngredientFraction = 0.22f;

        /// <summary>Fichas for a party served and happy about it.</summary>
        public const int PointsPerHappyParty = 2;

        /// <summary>And what one wrong plate takes back off.</summary>
        public const int PointsPerWrongOrder = 3;

        /// <summary>A whole day without a single mistake is worth going for.</summary>
        public const int FlawlessDayBonus = 15;

        /// <summary>Below this many days of fixed costs in the bank, the game says so.</summary>
        public const int WarningDays = 2;

        public static int RentFor(Profile profile)
        {
            if (profile == null) return BaseRent;

            var tiles = profile.Grid.Width * profile.Grid.Height;
            return BaseRent + (int)Math.Round(tiles * RentPerTile);
        }

        /// <summary>
        /// Every maid on the books, not only the ones who worked.
        /// </summary>
        /// <remarks>
        /// Paying for the bench is what makes a deep squad a decision instead of a hoard, and
        /// it is the only pressure that ever makes a player let a Legendary go.
        /// </remarks>
        public static int WagesFor(Profile profile)
        {
            if (profile == null) return 0;

            var total = 0;
            for (var i = 0; i < profile.Maids.Count; i++) total += profile.Maids[i].Salary;
            return total;
        }

        /// <summary>What it costs to keep the doors open tomorrow, whatever happens today.</summary>
        public static int FixedCostsFor(Profile profile) => WagesFor(profile) + RentFor(profile);

        /// <summary>
        /// Turns a finished day into the books and applies it to the profile.
        /// </summary>
        public static DayLedger Close(Profile profile, ShiftResult result)
        {
            if (profile == null) return new DayLedger();

            var ledger = new DayLedger
            {
                Day = profile.Day,
                BalanceBefore = profile.Wallet.Money,
                Wages = WagesFor(profile),
                Rent = RentFor(profile),
            };

            if (result != null)
            {
                ledger.Dishes = result.DishRevenue;
                ledger.Tips = result.Tips;
                ledger.Served = result.Served;
                ledger.WrongOrders = result.WrongOrders;
                ledger.Burnt = result.Burnt;
                ledger.LeftAngry = result.LeftAngry;
                ledger.TurnedAway = result.TurnedAway;
                ledger.DishesCooked = result.DishesCooked;
                ledger.Satisfaction = result.Satisfaction;
                ledger.ChemistryScore = result.ChemistryScore;
                ledger.FailureReason = result.FailureReason;

                // Charged on everything the kitchen made, including the plates that burnt and
                // the ones that went to the wrong table. Mistakes cost ingredients.
                ledger.Ingredients = (int)Math.Round(
                    result.DishesCooked * AverageDishCost(profile.CafeLevel));

                foreach (var pair in result.XpByMaid) ledger.XpByMaid[pair.Key] = pair.Value;
            }

            ledger.PointsEarned = PointsFor(profile, ledger);
            profile.Wallet.AddReputation(DayPlan.ReputationFor(result));

            ApplyXp(profile, ledger);
            Settle(profile, ledger);
            return ledger;
        }

        /// <summary>
        /// Fichas are paid on quality, never on volume.
        /// </summary>
        /// <remarks>
        /// Deliberately pulling against the money. A day that takes a fortune by rushing
        /// everybody out of the door should buy fewer boxes than a quiet one served properly,
        /// so that the player has two things to want and cannot have both at once.
        /// </remarks>
        public static int PointsFor(Profile profile, DayLedger ledger)
        {
            if (ledger == null) return 0;

            var happy = (int)Math.Round(ledger.Served * ledger.Satisfaction);
            var points = happy * PointsPerHappyParty - ledger.WrongOrders * PointsPerWrongOrder;

            if (ledger.Served > 0 && ledger.WrongOrders == 0 && ledger.LeftAngry == 0)
            {
                points += FlawlessDayBonus;
            }

            if (profile != null)
            {
                for (var i = 0; i < profile.Maids.Count; i++)
                {
                    var maid = profile.Maids[i];
                    if (!profile.Lineup.Contains(maid.Id)) continue;

                    for (var t = 0; t < maid.Traits.Count; t++)
                    {
                        points += TraitTable.ById(maid.Traits[t]).PointsPerDay;
                    }
                }
            }

            return Math.Max(0, points);
        }

        private static void ApplyXp(Profile profile, DayLedger ledger)
        {
            foreach (var pair in ledger.XpByMaid)
            {
                var maid = profile.MaidById(pair.Key);
                if (maid == null) continue;

                var before = maid.Level;
                MaidProgression.GrantXp(maid, pair.Value, out _);

                var gained = maid.Level - before;
                if (gained > 0) ledger.LevelUps[maid.Id] = gained;
            }
        }

        private static void Settle(Profile profile, DayLedger ledger)
        {
            profile.Wallet.Earn(ledger.Revenue);

            // Spend refuses what it cannot cover, so the overdraft is taken explicitly: going
            // under is not an error here, it is the losing condition.
            profile.Wallet.Pay(ledger.Costs);

            profile.Wallet.AddPoints(ledger.PointsEarned);
            ledger.BalanceAfter = profile.Wallet.Money;

            for (var i = 0; i < profile.Maids.Count; i++)
            {
                var maid = profile.Maids[i];
                MaidProgression.RecoverBetweenDays(maid, profile.Lineup.Contains(maid.Id));
            }

            profile.Records.TotalCustomersServed += ledger.Served;
            profile.Records.TotalDaysPlayed++;
            if (ledger.Revenue > profile.Records.BestRevenue)
            {
                profile.Records.BestRevenue = ledger.Revenue;
            }

            if (profile.Day > profile.Records.BestDay) profile.Records.BestDay = profile.Day;

            ledger.Bankrupt = profile.Wallet.Money <= 0;
            ledger.Warning = !ledger.Bankrupt &&
                             profile.Wallet.Money < FixedCostsFor(profile) * WarningDays;

            if (ledger.Bankrupt)
            {
                profile.Bankrupt = true;
                return;
            }

            profile.Day++;
            profile.ApplyUnlocks();
        }

        private static float AverageDishCost(int cafeLevel)
        {
            var menu = Service.RecipeBook.UnlockedAt(cafeLevel);
            if (menu.Count == 0) return 0f;

            var total = 0;
            for (var i = 0; i < menu.Count; i++) total += menu[i].Price;

            return total / (float)menu.Count * IngredientFraction;
        }
    }
}
