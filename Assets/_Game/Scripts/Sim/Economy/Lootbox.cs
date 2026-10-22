using System;
using System.Collections.Generic;
using Neko.Sim.Core;
using Neko.Sim.Maids;
using Neko.Sim.Save;
using Neko.Sim.Team;

namespace Neko.Sim.Economy
{
    public sealed class BoxDef
    {
        public string Id;
        public string DisplayName;

        /// <summary>Portuguese, and it states the odds. The player is told, not teased.</summary>
        public string Description;

        public int PricePoints;

        /// <summary>How many cards come out.</summary>
        public int Cards;

        /// <summary>Weights per rarity, in enum order. Normalised at roll time.</summary>
        public float[] Weights;

        /// <summary>The worst card in the box is at least this. A box is never a total loss.</summary>
        public Rarity Floor;
    }

    /// <summary>One card that came out of a box.</summary>
    public sealed class BoxCard
    {
        public Maid Maid;

        /// <summary>She was already on the books; the card became fichas instead.</summary>
        public bool Duplicate;

        public int PointsBack;
    }

    public sealed class BoxResult
    {
        public bool Ok;
        public string Refusal;
        public BoxDef Box;
        public readonly List<BoxCard> Cards = new List<BoxCard>();

        public static BoxResult No(string reason) => new BoxResult { Refusal = reason };
    }

    /// <summary>
    /// Where new maids come from.
    /// </summary>
    /// <remarks>
    /// Bought with fichas and never with money, so the one currency that can bankrupt the
    /// business is never the one being gambled. The odds are printed on every box, and a
    /// floor rarity means no box is ever a complete waste — a run of bad luck should feel
    /// unlucky rather than broken.
    /// </remarks>
    public static class Lootbox
    {
        private static readonly string[] Names =
        {
            "Mina", "Yuki", "Rina", "Suzu", "Kaede", "Nana", "Hoshi", "Chiyo", "Momo", "Aoi",
            "Rei", "Sora", "Hana", "Kiki", "Tama", "Nori", "Mio", "Shiro", "Kuro", "Umi",
            "Hina", "Sumi", "Yuzu", "Kana", "Riko", "Nagi", "Emi", "Saya", "Toko", "Mei",
        };

        private static readonly BoxDef[] Boxes =
        {
            new BoxDef
            {
                Id = "box_simple", DisplayName = "Caixa Simples",
                Description = "1 carta. 78% comum · 18% rara · 3,6% épica · 0,4% lendária.",
                PricePoints = 40, Cards = 1,
                Weights = new[] { 78f, 18f, 3.6f, 0.4f },
                Floor = Rarity.Common,
            },
            new BoxDef
            {
                Id = "box_silver", DisplayName = "Caixa Prata",
                Description = "3 cartas, nenhuma abaixo de rara. 62% rara · 32% épica · 6% lendária.",
                PricePoints = 180, Cards = 3,
                Weights = new[] { 0f, 62f, 32f, 6f },
                Floor = Rarity.Rare,
            },
            new BoxDef
            {
                Id = "box_gold", DisplayName = "Caixa Dourada",
                Description = "3 cartas, nenhuma abaixo de épica. 78% épica · 22% lendária.",
                PricePoints = 520, Cards = 3,
                Weights = new[] { 0f, 0f, 78f, 22f },
                Floor = Rarity.Epic,
            },
        };

        public static IReadOnlyList<BoxDef> All => Boxes;

        public static BoxDef ById(string id)
        {
            for (var i = 0; i < Boxes.Length; i++)
            {
                if (Boxes[i].Id == id) return Boxes[i];
            }

            return null;
        }

        /// <summary>The printed odds, as percentages, for the box's own description.</summary>
        public static IReadOnlyList<float> Odds(BoxDef box)
        {
            var total = 0f;
            for (var i = 0; i < box.Weights.Length; i++) total += Math.Max(0f, box.Weights[i]);

            var odds = new float[box.Weights.Length];
            if (total <= 0f) return odds;

            for (var i = 0; i < odds.Length; i++)
            {
                odds[i] = Math.Max(0f, box.Weights[i]) / total * 100f;
            }

            return odds;
        }

        public static BoxResult Open(Profile profile, string boxId, Rng rng)
        {
            if (profile == null) return BoxResult.No("Nenhum perfil carregado.");

            var box = ById(boxId);
            if (box == null) return BoxResult.No("Essa caixa não existe.");

            if (!profile.Wallet.CanAffordPoints(box.PricePoints))
            {
                return BoxResult.No("Faltam " + (box.PricePoints - profile.Wallet.Points) +
                                    " fichas.");
            }

            profile.Wallet.SpendPoints(box.PricePoints);

            var result = new BoxResult { Ok = true, Box = box };

            for (var i = 0; i < box.Cards; i++)
            {
                var rarity = RollRarity(box, rng);
                var card = Mint(profile, rarity, rng);

                // Over the roster cap she is converted on the spot rather than lost. A box
                // that silently ate a Legendary because the squad was full would be the
                // single worst thing this system could do to somebody.
                if (profile.Maids.Count >= profile.Unlocks.RosterCap)
                {
                    var back = RarityTable.For(rarity).SeverancePoints;
                    profile.Wallet.AddPoints(back);
                    result.Cards.Add(new BoxCard
                    {
                        Maid = card, Duplicate = true, PointsBack = back,
                    });
                    continue;
                }

                profile.Add(card);
                result.Cards.Add(new BoxCard { Maid = card });
            }

            return result;
        }

        public static Rarity RollRarity(BoxDef box, Rng rng)
        {
            var kinds = new List<Rarity>();
            foreach (Rarity rarity in Enum.GetValues(typeof(Rarity)))
            {
                if (rarity >= box.Floor) kinds.Add(rarity);
            }

            return rng.PickWeighted(kinds, r =>
            {
                var index = (int)r;
                return index < box.Weights.Length ? Math.Max(0f, box.Weights[index]) : 0f;
            });
        }

        /// <summary>Rolls a whole new card: personality, stats, tags and traits.</summary>
        public static Maid Mint(Profile profile, Rarity rarity, Rng rng)
        {
            var archetype = rng.Pick(ArchetypeTable.All).Kind;
            var id = profile?.NextMaidId ?? 1;
            if (profile != null) profile.NextMaidId++;

            var maid = Maid.Create(id, rng.Pick(Names), archetype);
            maid.Rarity = rarity;
            maid.Base = RarityTable.RollStats(rarity, archetype, rng);
            maid.House = ChemistryTags.RollHouse(rng);
            maid.Origin = ChemistryTags.RollOrigin(rng);

            maid.Traits.Clear();
            maid.Traits.AddRange(TraitTable.Roll(RarityTable.For(rarity).Traits, rng));

            Progress.MaidProgression.RefreshSetBonus(maid);
            return maid;
        }
    }
}
