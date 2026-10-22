using System;
using System.Collections.Generic;

namespace Neko.Sim.Core
{
    /// <summary>
    /// The only source of randomness in the simulation.
    /// </summary>
    /// <remarks>
    /// Everything random goes through here so that a shift can be replayed exactly from its
    /// seed. That is not a nicety: the balance tests run two hundred seeded shifts and compare
    /// outcomes, and they would be meaningless against a generator the caller cannot pin.
    /// </remarks>
    public sealed class Rng
    {
        // xorshift32 latches permanently at zero, which would silently turn every later draw
        // into the same number. Golden ratio constant chosen only because it is not zero.
        private const uint NonZeroSeed = 0x9E3779B9u;

        private uint _state;

        public Rng(uint seed)
        {
            _state = seed == 0u ? NonZeroSeed : seed;
        }

        public uint State => _state;

        public uint NextUInt()
        {
            var x = _state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _state = x;
            return x;
        }

        /// <summary>A draw in [0, 1). The 24-bit mantissa is what keeps it below 1.</summary>
        public float NextFloat() => (NextUInt() >> 8) * (1f / 16777216f);

        public int NextInt(int maxExclusive)
        {
            if (maxExclusive <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxExclusive), "A range of " + maxExclusive + " has nothing to draw from.");
            }

            return (int)(NextFloat() * maxExclusive);
        }

        public int RangeInt(int minInclusive, int maxExclusive) =>
            minInclusive + NextInt(Math.Max(1, maxExclusive - minInclusive));

        public float Range(float minInclusive, float maxExclusive) =>
            minInclusive + NextFloat() * (maxExclusive - minInclusive);

        public bool Chance(float probability)
        {
            if (probability <= 0f) return false;
            if (probability >= 1f) return true;
            return NextFloat() < probability;
        }

        public T Pick<T>(IReadOnlyList<T> items)
        {
            RequireItems(items);
            return items[NextInt(items.Count)];
        }

        /// <summary>
        /// Picks by weight, ignoring negative weights, and falls back to uniform when every
        /// weight is zero — a weight table that has not been filled in yet should give a
        /// playable game, not an exception from inside a shift.
        /// </summary>
        public T PickWeighted<T>(IReadOnlyList<T> items, Func<T, float> weight)
        {
            RequireItems(items);

            var total = 0f;
            for (var i = 0; i < items.Count; i++)
            {
                var w = weight(items[i]);
                if (w > 0f) total += w;
            }

            if (total <= 0f) return items[NextInt(items.Count)];

            var roll = NextFloat() * total;
            for (var i = 0; i < items.Count; i++)
            {
                var w = weight(items[i]);
                if (w <= 0f) continue;

                roll -= w;
                if (roll < 0f) return items[i];
            }

            return items[items.Count - 1];
        }

        /// <summary>
        /// A child stream, so one system's draws cannot shift another's. Deterministic: the
        /// same parent seed always forks the same child.
        /// </summary>
        /// <remarks>
        /// The drawn value is mixed rather than used directly. xorshift is a pure function of
        /// its state, so seeding a child with the parent's next draw hands the child the
        /// parent's own future — the two streams come out identical, which a test caught.
        /// </remarks>
        public Rng Fork()
        {
            var seed = NextUInt();
            seed ^= seed >> 16;
            seed *= 0x7FEB352Du;
            seed ^= seed >> 15;
            seed *= 0x846CA68Bu;
            seed ^= seed >> 16;
            return new Rng(seed);
        }

        private static void RequireItems<T>(IReadOnlyList<T> items)
        {
            if (items == null || items.Count == 0)
            {
                throw new ArgumentException("Nothing to pick from.", nameof(items));
            }
        }
    }
}
