using Neko.Sim.Core;
using Neko.Sim.Maids;

namespace Neko.Sim.Progress
{
    /// <summary>
    /// The four axes, as operations. Every one of them refuses rather than throws.
    /// </summary>
    /// <remarks>
    /// Refusing with a reason instead of throwing is deliberate: these are driven straight
    /// from buttons, and a UI that has to guard every click to avoid an exception grows a
    /// second copy of the rules that then drifts from this one.
    /// </remarks>
    public static class MaidProgression
    {
        /// <summary>Mood recovered overnight by a maid who worked today.</summary>
        public const float RecoveryOnShift = 0.45f;

        /// <summary>And by one who was left on the bench, which is why benching is a move.</summary>
        public const float RecoveryOffShift = 0.85f;

        /// <summary>Gift points a day of work is worth on its own.</summary>
        public const int BondPointsPerShift = 2;

        public static bool GrantXp(Maid maid, int xp, out int levelsGained)
        {
            levelsGained = 0;
            if (maid == null || xp <= 0) return false;

            maid.Xp += xp;

            while (maid.Level < Maid.MaxLevel && maid.Xp >= MaidMath.XpForLevel(maid.Level + 1))
            {
                maid.Level++;
                levelsGained++;
            }

            if (levelsGained > 0)
            {
                maid.Base = maid.Base.Plus(MaidMath.GrowthFor(maid, levelsGained)).Clamped();
            }

            // At the cap, XP keeps accumulating rather than being thrown away: it is what the
            // roster screen shows as lifetime service, and discarding it would make a
            // maxed-out maid look idle.
            return levelsGained > 0;
        }

        public static bool EquipSkill(Maid maid, SkillId id, out string refusal)
        {
            refusal = null;
            if (maid == null)
            {
                refusal = "Nenhuma maid selecionada.";
                return false;
            }

            if (id == SkillId.None)
            {
                refusal = "Escolha uma habilidade.";
                return false;
            }

            if (maid.Skills.Contains(id))
            {
                refusal = "Ela já tem essa habilidade.";
                return false;
            }

            var def = SkillTable.ById(id);
            if (def.RequiredLevel > maid.Level)
            {
                refusal = "Precisa de nível " + def.RequiredLevel + ".";
                return false;
            }

            if (maid.Skills.Count >= SkillTable.SlotsAt(maid.Level))
            {
                refusal = "Sem espaço: ela tem " + SkillTable.SlotsAt(maid.Level) + " slots.";
                return false;
            }

            maid.Skills.Add(id);
            return true;
        }

        public static bool ForgetSkill(Maid maid, SkillId id) =>
            maid != null && maid.Skills.Remove(id);

        /// <summary>
        /// Puts a piece on, and hands back whatever came off so the caller can keep it.
        /// </summary>
        public static bool Equip(Maid maid, Equipment item, out Equipment removed)
        {
            removed = null;
            if (maid == null || item == null) return false;

            var slot = (int)item.Slot;
            removed = maid.Gear[slot];
            maid.Gear[slot] = item;
            RefreshSetBonus(maid);
            return true;
        }

        public static bool Unequip(Maid maid, GearSlot slot, out Equipment removed)
        {
            removed = null;
            if (maid == null) return false;

            removed = maid.Gear[(int)slot];
            if (removed == null) return false;

            maid.Gear[(int)slot] = null;
            RefreshSetBonus(maid);
            return true;
        }

        public static void RefreshSetBonus(Maid maid)
        {
            if (maid != null) maid.SetBonus = EquipmentCatalog.SetBonus(maid);
        }

        public static bool GiveGift(Maid maid, int points, out int bondLevelsGained)
        {
            bondLevelsGained = 0;
            if (maid == null || points <= 0 || maid.Bond >= BondTable.MaxLevel) return false;

            maid.BondPoints += points;
            var level = BondTable.LevelForPoints(maid.BondPoints);
            bondLevelsGained = level - maid.Bond;
            maid.Bond = level;
            return true;
        }

        /// <summary>
        /// Overnight recovery. A maid who sat out comes back nearly fresh, which is what makes
        /// benching somebody a decision rather than a punishment.
        /// </summary>
        public static void RecoverBetweenDays(Maid maid, bool wasOnShift)
        {
            if (maid == null) return;

            maid.Mood = Curves.Clamp01(maid.Mood +
                                       (wasOnShift ? RecoveryOnShift : RecoveryOffShift));

            if (wasOnShift) GiveGift(maid, BondPointsPerShift, out _);
        }
    }
}
