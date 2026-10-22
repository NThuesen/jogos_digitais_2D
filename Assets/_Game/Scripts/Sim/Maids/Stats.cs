using System;

namespace Neko.Sim.Maids
{
    public enum StatKind
    {
        Serve = 0,
        Cook,
        Charm,
        Stamina,
    }

    /// <summary>The four numbers a maid is made of. Always in 1..100 once clamped.</summary>
    public readonly struct Stats : IEquatable<Stats>
    {
        public const int Min = 1;
        public const int Max = 100;

        public readonly int Serve;
        public readonly int Cook;
        public readonly int Charm;
        public readonly int Stamina;

        public Stats(int serve, int cook, int charm, int stamina)
        {
            Serve = serve;
            Cook = cook;
            Charm = charm;
            Stamina = stamina;
        }

        public static Stats Zero => new Stats(0, 0, 0, 0);

        public static Stats Uniform(int value) => new Stats(value, value, value, value);

        public int this[StatKind kind]
        {
            get
            {
                switch (kind)
                {
                    case StatKind.Serve: return Serve;
                    case StatKind.Cook: return Cook;
                    case StatKind.Charm: return Charm;
                    case StatKind.Stamina: return Stamina;
                    default: throw new ArgumentOutOfRangeException(nameof(kind));
                }
            }
        }

        public Stats Plus(Stats other) =>
            new Stats(Serve + other.Serve, Cook + other.Cook, Charm + other.Charm,
                Stamina + other.Stamina);

        public Stats Clamped(int min = Min, int max = Max) =>
            new Stats(Clamp(Serve, min, max), Clamp(Cook, min, max), Clamp(Charm, min, max),
                Clamp(Stamina, min, max));

        /// <summary>Each stat as 0..1, which is the form every derived speed is built from.</summary>
        public float Normalised(StatKind kind) => (this[kind] - Min) / (float)(Max - Min);

        private static int Clamp(int value, int min, int max) =>
            value < min ? min : value > max ? max : value;

        public bool Equals(Stats other) =>
            Serve == other.Serve && Cook == other.Cook && Charm == other.Charm &&
            Stamina == other.Stamina;

        public override bool Equals(object obj) => obj is Stats other && Equals(other);

        public override int GetHashCode() =>
            Serve ^ (Cook << 8) ^ (Charm << 16) ^ (Stamina << 24);

        public override string ToString() =>
            "Serve " + Serve + " / Cook " + Cook + " / Charm " + Charm + " / Stamina " + Stamina;
    }
}
