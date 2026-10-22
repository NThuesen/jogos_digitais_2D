using System.Collections.Generic;
using Neko.Sim.Core;
using Neko.Sim.Maids;
using Neko.Sim.Save;

namespace Neko.Sim.Economy
{
    public sealed class Applicant
    {
        public Maid Maid;
        public int Price;
    }

    public readonly struct HireResult
    {
        public readonly bool Ok;
        public readonly string Refusal;
        public readonly Maid Maid;

        private HireResult(bool ok, string refusal, Maid maid)
        {
            Ok = ok;
            Refusal = refusal;
            Maid = maid;
        }

        public static HireResult Hired(Maid maid) => new HireResult(true, null, maid);

        public static HireResult No(string reason) => new HireResult(false, reason, null);
    }

    /// <summary>
    /// Three candidates, refreshed every morning.
    /// </summary>
    /// <remarks>
    /// Three rather than a list of everybody, so that hiring is a decision taken under a
    /// constraint instead of shopping. The board always offers at least one maid the player
    /// could afford if they saved, which is a deliberate floor: a board of nothing but
    /// unaffordable candidates reads as broken rather than as hard.
    /// </remarks>
    public sealed class ApplicantBoard
    {
        public const int Slots = 3;

        private static readonly string[] Names =
        {
            "Mina", "Yuki", "Rina", "Suzu", "Kaede", "Nana", "Hoshi", "Chiyo", "Momo", "Aoi",
            "Rei", "Sora", "Hana", "Kiki", "Tama", "Nori", "Mio", "Shiro", "Kuro", "Umi",
        };

        private readonly List<Applicant> _slots = new List<Applicant>();

        public IReadOnlyList<Applicant> Candidates => _slots;

        public void Refresh(Rng rng, int cafeLevel, int nextMaidId)
        {
            _slots.Clear();

            for (var i = 0; i < Slots; i++)
            {
                var archetype = rng.Pick(ArchetypeTable.All).Kind;

                // Candidates arrive a little below the café they are applying to, so hiring
                // is never a shortcut past levelling the maids already on the roster.
                var level = Curves.ClampInt(rng.RangeInt(1, 2 + cafeLevel / 2), 1, Maid.MaxLevel);
                var maid = Maid.Create(nextMaidId + i, rng.Pick(Names), archetype, level);

                _slots.Add(new Applicant { Maid = maid, Price = PriceOf(maid) });
            }
        }

        public static int PriceOf(Maid maid)
        {
            var stats = maid.Total;
            var worth = stats.Serve + stats.Cook + stats.Charm + stats.Stamina;
            return 180 + maid.Level * 110 + worth * 2;
        }

        public HireResult Hire(Profile profile, int slotIndex)
        {
            if (profile == null) return HireResult.No("Nenhum perfil carregado.");
            if (slotIndex < 0 || slotIndex >= _slots.Count) return HireResult.No("Vaga inválida.");

            var applicant = _slots[slotIndex];
            if (applicant == null) return HireResult.No("Essa vaga já foi preenchida.");

            if (profile.Maids.Count >= profile.Unlocks.RosterCap)
            {
                return HireResult.No("O café só comporta " + profile.Unlocks.RosterCap +
                                     " maids por enquanto.");
            }

            if (!profile.Wallet.CanAfford(applicant.Price))
            {
                return HireResult.No("Faltam " + (applicant.Price - profile.Wallet.Money) +
                                     " moedas.");
            }

            profile.Wallet.Spend(applicant.Price);

            var maid = applicant.Maid;
            maid.Id = profile.NextMaidId++;
            profile.Add(maid);

            _slots[slotIndex] = null;
            return HireResult.Hired(maid);
        }
    }
}
