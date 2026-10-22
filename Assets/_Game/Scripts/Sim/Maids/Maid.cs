using System.Collections.Generic;
using Neko.Sim.Core;
using Neko.Sim.Progress;

namespace Neko.Sim.Maids
{
    /// <summary>
    /// One cat. The thing the whole game is about improving.
    /// </summary>
    /// <remarks>
    /// Four axes move independently — level, skills, equipment, bond — which is what makes a
    /// level-12 Tsundere in a full Sakura set a different worker from the same Tsundere at
    /// bond zero. The class is a plain mutable record on purpose: it is saved, and a save
    /// that needs a constructor call per field is a save that breaks on the next change.
    /// </remarks>
    public sealed class Maid
    {
        public const int MaxLevel = 20;
        public const int MaxBond = 5;

        /// <summary>Flat stat gain per bond level, in every stat. Small on purpose.</summary>
        public const int StatsPerBondLevel = 1;

        private float _mood = 1f;

        public int Id;
        public string Name;
        public Archetype Archetype;
        public int Level = 1;
        public int Xp;
        public int Bond;

        /// <summary>Gift points earned. Bond level is a function of this, never set directly.</summary>
        public int BondPoints;

        /// <summary>Stats before gear, bond and set bonuses.</summary>
        public Stats Base;

        /// <summary>Filled in by the equipment catalog when gear changes; zero otherwise.</summary>
        public Stats SetBonus = Stats.Zero;

        public readonly Equipment[] Gear = new Equipment[4];
        public readonly List<SkillId> Skills = new List<SkillId>();

        /// <summary>How much she has left in her today, 0..1.</summary>
        public float Mood
        {
            get => _mood;
            set => _mood = Curves.Clamp01(value);
        }

        public ArchetypeProfile Profile => ArchetypeTable.For(Archetype);

        public Stats GearBonus
        {
            get
            {
                var total = Stats.Zero;
                for (var i = 0; i < Gear.Length; i++)
                {
                    if (Gear[i] != null) total = total.Plus(Gear[i].Bonus);
                }

                return total;
            }
        }

        public Stats BondBonus => Stats.Uniform(Bond * StatsPerBondLevel);

        public Stats Total => Base.Plus(GearBonus).Plus(SetBonus).Plus(BondBonus).Clamped();

        public static Maid Create(int id, string name, Archetype archetype, int level = 1)
        {
            var profile = ArchetypeTable.For(archetype);
            var maid = new Maid
            {
                Id = id,
                Name = name,
                Archetype = archetype,
                Level = Curves.ClampInt(level, 1, MaxLevel),
                Base = profile.StartingStats,
            };

            // A maid created at level 10 has to already carry nine levels of growth, or the
            // applicant board would hand out high-level maids with starting stats.
            maid.Base = maid.Base.Plus(MaidMath.GrowthFor(maid, maid.Level - 1)).Clamped();
            maid.Xp = MaidMath.XpForLevel(maid.Level);
            return maid;
        }
    }
}
