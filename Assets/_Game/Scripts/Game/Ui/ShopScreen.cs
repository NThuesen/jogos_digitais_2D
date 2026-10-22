using System;
using System.Collections.Generic;
using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Sim.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Game.Ui
{
    /// <summary>Furniture and clothes, bought with money. Cards are not sold here.</summary>
    /// <remarks>
    /// Kept strictly apart from the boxes. Money pays the rent and can end the business;
    /// fichas buy cards and cannot. Mixing the two tills would let a player gamble the wage
    /// bill, which is the one thing the design does not allow.
    /// </remarks>
    public sealed class ShopScreen : Screen
    {
        private readonly List<GameObject> _rows = new List<GameObject>();

        private RectTransform _list;
        private Text _money;
        private ShopItemKind _tab = ShopItemKind.Placeable;

        public GameSession Session;
        public Action OnBack;

        protected override void Build()
        {
            Scenery.Build(transform, withFloor: false);

            var title = UiKit.TrackedLabel(transform, "LOJA", Typeface.Heading, Palette.Cream, 6f);
            UiKit.Anchor(title, new Vector2(0f, 1f), new Vector2(26f, -18f));

            _money = UiKit.Label(transform, "", Typeface.Heading, Logo.Brass,
                TextAnchor.MiddleRight, bold: true);
            UiKit.Anchor(_money.rectTransform, new Vector2(1f, 1f), new Vector2(-170f, -26f));
            _money.rectTransform.sizeDelta = new Vector2(260f, 40f);

            Scenery.PlankAt(transform, Strings.Get("dialog.back"), new Vector2(120f, 44f),
                new Vector2(1f, 1f), new Vector2(-26f, -18f), () => OnBack?.Invoke(),
                Typeface.Small);

            var tabs = UiKit.Row(transform, 10f);
            UiKit.Anchor(tabs, new Vector2(0.5f, 1f), new Vector2(0f, -82f));
            tabs.sizeDelta = new Vector2(700f, 48f);

            Tab(tabs, "shop.furniture", ShopItemKind.Placeable);
            Tab(tabs, "shop.clothes", ShopItemKind.Equipment);
            Tab(tabs, "shop.gifts", ShopItemKind.Gift);

            _list = UiKit.ScrollList(transform, new Vector2(880f, 460f));
            UiKit.Anchor((RectTransform)_list.parent, new Vector2(0.5f, 0f), new Vector2(0f, 24f));
        }

        private void Tab(Transform parent, string key, ShopItemKind kind)
        {
            Scenery.Plank(parent, Strings.Get(key), new Vector2(210f, 44f), () =>
            {
                _tab = kind;
                Refresh();
            }, Typeface.Small);
        }

        public override void Refresh()
        {
            if (Session == null) return;

            for (var i = 0; i < _rows.Count; i++)
            {
                if (_rows[i] != null) Destroy(_rows[i]);
            }

            _rows.Clear();
            _money.text = Strings.Money(Session.Profile.Wallet.Money);

            foreach (var item in Shop.StockFor(Session.Profile.CafeLevel))
            {
                if (item.Kind != _tab) continue;
                BuildRow(item);
            }
        }

        private void BuildRow(ShopItem item)
        {
            var affordable = Session.Profile.Wallet.CanAfford(item.Price);
            var row = UiKit.Panel(_list, new Vector2(850f, 62f),
                Palette.WithAlpha(affordable ? Palette.Panel : Palette.Wall, 0.92f), 0.2f);
            _rows.Add(row.gameObject);

            var name = UiKit.Label(row, item.DisplayName, Typeface.Body, Palette.Text,
                TextAnchor.MiddleLeft, bold: true);
            name.rectTransform.sizeDelta = new Vector2(320f, 26f);
            name.rectTransform.anchoredPosition = new Vector2(-240f, 12f);

            var description = UiKit.Label(row, item.Description, Typeface.Tiny, Palette.TextDim,
                TextAnchor.MiddleLeft);
            description.rectTransform.sizeDelta = new Vector2(470f, 22f);
            description.rectTransform.anchoredPosition = new Vector2(-165f, -12f);

            var owned = Owned(item);
            if (owned > 0)
            {
                var stock = UiKit.Label(row, Strings.Get("shop.owned") + ": " + owned,
                    Typeface.Tiny, Logo.Brass, TextAnchor.MiddleRight, bold: true);
                stock.rectTransform.sizeDelta = new Vector2(170f, 22f);
                stock.rectTransform.anchoredPosition = new Vector2(215f, 14f);
            }

            var buy = Scenery.Plank(row, Strings.Money(item.Price), new Vector2(150f, 44f),
                () => Buy(item), Typeface.Small, affordable ? Logo.Brass : Logo.Board);

            UiKit.Anchor(buy.GetComponent<RectTransform>(), new Vector2(1f, 0.5f),
                new Vector2(-10f, 0f));
            buy.interactable = affordable;
            if (affordable) buy.GetComponentInChildren<Text>().color = Palette.Hex("#3A2A1A");
        }

        private int Owned(ShopItem item)
        {
            var list = item.Kind == ShopItemKind.Placeable
                ? Session.Profile.OwnedPlaceables
                : item.Kind == ShopItemKind.Equipment
                    ? Session.Profile.OwnedEquipment
                    : null;

            if (list == null) return 0;

            var count = 0;
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i] == item.Id) count++;
            }

            return count;
        }

        private void Buy(ShopItem item)
        {
            if (item.Kind != ShopItemKind.Gift)
            {
                Report(Shop.Buy(Session.Profile, item.Id));
                return;
            }

            if (Session.Profile.Maids.Count == 0)
            {
                Root.Dialog(Strings.Get("shop.gifts"), Strings.Get("roster.empty"));
                return;
            }

            var maid = Session.Profile.Maids[_giftTarget % Session.Profile.Maids.Count];
            _giftTarget++;

            Root.Dialog(Strings.Get("shop.forwho"), maid.Name + " — " + item.DisplayName,
                Strings.Get("dialog.yes"),
                () => Report(Shop.Buy(Session.Profile, item.Id, maid.Id)),
                Strings.Get("dialog.no"));
        }

        private int _giftTarget;

        private void Report(PurchaseResult result)
        {
            if (!result.Ok)
            {
                Root.Dialog(Strings.Get("shop.title"), result.Refusal);
                return;
            }

            Session.Touch();
            Refresh();
        }
    }
}
