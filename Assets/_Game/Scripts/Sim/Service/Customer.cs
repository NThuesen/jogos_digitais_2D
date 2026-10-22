using System.Collections.Generic;
using Neko.Sim.Core;
using Neko.Sim.Maids;

namespace Neko.Sim.Service
{
    public enum CustomerState
    {
        /// <summary>At the door, waiting to be greeted. Draining.</summary>
        Queueing = 0,

        /// <summary>Being walked to a table. Not draining: something is happening for them.</summary>
        Seating,

        /// <summary>Reading the menu. Brief, and not draining.</summary>
        Thinking,

        /// <summary>Hand up, waiting for somebody to take the order. Draining.</summary>
        Ordering,

        /// <summary>Order taken, waiting for food. Draining.</summary>
        Waiting,

        Eating,
        Paying,
        Leaving,
        Angry,
    }

    /// <summary>One party at one table, and the clock running against it.</summary>
    public sealed class Customer
    {
        public int Id;
        public CustomerType Kind;
        public int PartySize = 1;
        public CustomerState State = CustomerState.Queueing;

        /// <summary>How much of their goodwill is left, 1 down to 0.</summary>
        public float Patience = 1f;

        /// <summary>Seconds of waiting their full patience is worth.</summary>
        public float PatienceSeconds = 45f;

        /// <summary>Index into the shift's seat list; -1 while still at the door.</summary>
        public int SeatIndex = -1;

        public Order Order;

        /// <summary>Who the otaku came to see.</summary>
        public Archetype? WantedArchetype;

        /// <summary>Counts down the current state when the state is just a wait.</summary>
        public float StateTimer;

        /// <summary>Set when they pay, so the result can report what the day actually earned.</summary>
        public int Paid;

        public CustomerProfile Profile => CustomerTable.For(Kind);

        public bool DrainsPatience =>
            State == CustomerState.Queueing ||
            State == CustomerState.Ordering ||
            State == CustomerState.Waiting;

        public static Customer Spawn(int id, CustomerType kind, Rng rng, int cafeLevel,
            float patienceMultiplier)
        {
            var profile = CustomerTable.For(kind);

            var customer = new Customer
            {
                Id = id,
                Kind = kind,
                PartySize = rng.RangeInt(profile.PartySizeMin, profile.PartySizeMax + 1),
                PatienceSeconds = profile.PatienceSeconds * Curves.Clamp(patienceMultiplier, 0.1f, 4f),
                WantedArchetype = profile.WantsAnArchetype
                    ? CustomerTable.WantedArchetype(rng)
                    : (Archetype?)null,
            };

            customer.Order = RollOrder(profile, rng, cafeLevel);
            return customer;
        }

        /// <summary>
        /// Burns patience for <paramref name="seconds"/> of waiting, softened by the room.
        /// </summary>
        /// <remarks>
        /// Ambience divides the drain rather than adding to the pool, so a cosy room makes
        /// every wait proportionally easier instead of buying one fixed grace period that
        /// stops mattering once the café is busy.
        /// </remarks>
        public void DrainPatience(float seconds, float ambiencePatienceMultiplier)
        {
            if (Profile.NeverImpatient || !DrainsPatience) return;

            var effective = PatienceSeconds * Curves.Clamp(ambiencePatienceMultiplier, 0.1f, 4f);
            Patience = Curves.Clamp01(Patience - seconds / effective);
        }

        private static Order RollOrder(CustomerProfile profile, Rng rng, int cafeLevel)
        {
            var dishes = new List<Recipe>();

            if (!string.IsNullOrEmpty(profile.OnlyOrders))
            {
                dishes.Add(RecipeBook.ById(profile.OnlyOrders));
                return new Order(dishes);
            }

            var menu = RecipeBook.UnlockedAt(cafeLevel);
            var count = rng.RangeInt(profile.DishesMin, profile.DishesMax + 1);

            for (var i = 0; i < count; i++) dishes.Add(rng.Pick(menu));

            return new Order(dishes);
        }
    }
}
