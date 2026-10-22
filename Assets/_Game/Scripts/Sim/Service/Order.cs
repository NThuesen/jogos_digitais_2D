using System.Collections.Generic;

namespace Neko.Sim.Service
{
    /// <summary>What one customer asked for, and how much of it has arrived.</summary>
    public sealed class Order
    {
        private readonly List<Recipe> _dishes;

        public Order(List<Recipe> dishes)
        {
            _dishes = dishes;
        }

        public IReadOnlyList<Recipe> Dishes => _dishes;

        public int ServedCount { get; private set; }

        public bool Complete => ServedCount >= _dishes.Count;

        public int BasePrice
        {
            get
            {
                var total = 0;
                for (var i = 0; i < _dishes.Count; i++) total += _dishes[i].Price;
                return total;
            }
        }

        /// <summary>The dish the kitchen owes next, or null when the order is complete.</summary>
        public Recipe NextDish => Complete ? null : _dishes[ServedCount];

        /// <summary>
        /// Records one more dish on the table. Serving past the end is ignored rather than
        /// thrown: two maids can finish the last plate on the same tick, and that is a race,
        /// not a bug worth ending a shift over.
        /// </summary>
        public void Serve()
        {
            if (!Complete) ServedCount++;
        }
    }
}
