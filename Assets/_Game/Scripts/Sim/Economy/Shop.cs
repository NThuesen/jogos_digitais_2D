using System.Collections.Generic;
using Neko.Sim.Cafe;
using Neko.Sim.Progress;
using Neko.Sim.Save;

namespace Neko.Sim.Economy
{
    public enum ShopItemKind
    {
        Placeable = 0,
        Equipment,
        Gift,
    }

    public sealed class ShopItem
    {
        public readonly string Id;
        public readonly ShopItemKind Kind;
        public readonly string DisplayName;
        public readonly string Description;
        public readonly int Price;
        public readonly int UnlockCafeLevel;

        /// <summary>Gift points, for a gift. Zero otherwise.</summary>
        public readonly int BondPoints;

        public ShopItem(string id, ShopItemKind kind, string displayName, string description,
            int price, int unlockCafeLevel, int bondPoints = 0)
        {
            Id = id;
            Kind = kind;
            DisplayName = displayName;
            Description = description;
            Price = price;
            UnlockCafeLevel = unlockCafeLevel;
            BondPoints = bondPoints;
        }
    }

    public readonly struct PurchaseResult
    {
        public readonly bool Ok;

        /// <summary>Portuguese, and shown to the player verbatim.</summary>
        public readonly string Refusal;

        private PurchaseResult(bool ok, string refusal)
        {
            Ok = ok;
            Refusal = refusal;
        }

        public static PurchaseResult Bought => new PurchaseResult(true, null);

        public static PurchaseResult No(string reason) => new PurchaseResult(false, reason);
    }

    /// <summary>
    /// Where money turns into furniture, clothes and goodwill.
    /// </summary>
    /// <remarks>
    /// Every refusal comes back as a sentence rather than an exception, because these calls
    /// are driven straight from buttons and a UI that has to pre-check every click grows a
    /// second copy of the rules that then drifts from this one.
    /// </remarks>
    public static class Shop
    {
        private static readonly ShopItem[] Gifts =
        {
            new ShopItem("gift_small", ShopItemKind.Gift, "Bombom", "Um agrado. +8 de afinidade.",
                80, 1, 8),
            new ShopItem("gift_medium", ShopItemKind.Gift, "Buquê",
                "Ela vai levar para casa. +25 de afinidade.", 200, 2, 25),
            new ShopItem("gift_large", ShopItemKind.Gift, "Dia de folga",
                "O maior presente que existe. +60 de afinidade.", 450, 4, 60),
        };

        public static IReadOnlyList<ShopItem> StockFor(int cafeLevel)
        {
            var stock = new List<ShopItem>();

            foreach (var def in PlaceableCatalog.ForSaleAt(cafeLevel))
            {
                stock.Add(new ShopItem(def.Id, ShopItemKind.Placeable, def.DisplayName,
                    DescribePlaceable(def), def.Price, def.UnlockCafeLevel));
            }

            foreach (var item in EquipmentCatalog.UnlockedAt(cafeLevel))
            {
                stock.Add(new ShopItem(item.Id, ShopItemKind.Equipment, item.DisplayName,
                    DescribeEquipment(item), item.Price, item.UnlockCafeLevel));
            }

            foreach (var gift in Gifts)
            {
                if (gift.UnlockCafeLevel <= cafeLevel) stock.Add(gift);
            }

            return stock;
        }

        public static ShopItem Find(int cafeLevel, string itemId)
        {
            var stock = StockFor(cafeLevel);
            for (var i = 0; i < stock.Count; i++)
            {
                if (stock[i].Id == itemId) return stock[i];
            }

            return null;
        }

        public static PurchaseResult Buy(Profile profile, string itemId, int maidId = -1)
        {
            if (profile == null) return PurchaseResult.No("Nenhum perfil carregado.");

            var item = Find(profile.CafeLevel, itemId);
            if (item == null) return PurchaseResult.No("Isso ainda não está à venda.");

            if (item.Kind == ShopItemKind.Gift && profile.MaidById(maidId) == null)
            {
                return PurchaseResult.No("Escolha para quem é o presente.");
            }

            if (!profile.Wallet.CanAfford(item.Price))
            {
                return PurchaseResult.No("Faltam " + (item.Price - profile.Wallet.Money) + " moedas.");
            }

            // Charged only once everything else has been checked, so a refusal never costs
            // anything.
            profile.Wallet.Spend(item.Price);

            switch (item.Kind)
            {
                case ShopItemKind.Placeable:
                    profile.OwnedPlaceables.Add(item.Id);
                    break;

                case ShopItemKind.Equipment:
                    profile.OwnedEquipment.Add(item.Id);
                    break;

                case ShopItemKind.Gift:
                    MaidProgression.GiveGift(profile.MaidById(maidId), item.BondPoints, out _);
                    break;
            }

            return PurchaseResult.Bought;
        }

        private static string DescribePlaceable(PlaceableDef def)
        {
            switch (def.Kind)
            {
                case PlaceableKind.Table:
                    return def.Seats + (def.Seats == 1 ? " lugar." : " lugares.");

                case PlaceableKind.Station:
                    return "Estação de cozinha.";

                default:
                    return "+" + def.Ambience.ToString("0") + " de ambiente" +
                           (def.Theme == Core.ThemeTag.None ? "." : " · tema " + def.Theme + ".");
            }
        }

        private static string DescribeEquipment(Equipment item)
        {
            var bonus = item.Bonus;
            var parts = new List<string>();
            if (bonus.Serve > 0) parts.Add("+" + bonus.Serve + " Salão");
            if (bonus.Cook > 0) parts.Add("+" + bonus.Cook + " Cozinha");
            if (bonus.Charm > 0) parts.Add("+" + bonus.Charm + " Charme");
            if (bonus.Stamina > 0) parts.Add("+" + bonus.Stamina + " Fôlego");

            var text = string.Join(", ", parts);
            return item.Theme == Core.ThemeTag.None ? text : text + " · tema " + item.Theme;
        }
    }
}
