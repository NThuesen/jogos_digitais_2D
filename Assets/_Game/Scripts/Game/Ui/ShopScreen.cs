using System.Collections.Generic;
using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Sim.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Game.Ui
{
    /// <summary>Where the day's takings turn into furniture, clothes and goodwill.</summary>
    public sealed class ShopScreen : Screen
    {
        private readonly List<GameObject> _rows = new List<GameObject>();

        private RectTransform _list;
        private Text _money;
        private ShopItemKind _tab = ShopItemKind.Placeable;

        public GameSession Session;
        public System.Action OnBack;

        protected override void Build()
        {
            Backdrop(Palette.Cream);

            var title = UiKit.Label(transform, Strings.Get("shop.title"), Typeface.Heading,
                Palette.Text, TextAnchor.MiddleLeft, bold: true);
            UiKit.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(24f, -16f));
            title.rectTransform.sizeDelta = new Vector2(300f, 44f);

            _money = UiKit.Label(transform, "", Typeface.Heading, Palette.Focus,
                TextAnchor.MiddleRight, bold: true);
            UiKit.Anchor(_money.rectTransform, new Vector2(1f, 1f), new Vector2(-90f, -16f));
            _money.rectTransform.sizeDelta = new Vector2(300f, 44f);

            var back = UiKit.Button(transform, "<", new Vector2(56f, 48f), Palette.Wall,
                () => OnBack?.Invoke());
            UiKit.Anchor(back.GetComponent<RectTransform>(), new Vector2(1f, 1f),
                new Vector2(-16f, -16f));

            var tabs = UiKit.Row(transform, 10f);
            UiKit.Anchor(tabs, new Vector2(0.5f, 1f), new Vector2(0f, -76f));
            tabs.sizeDelta = new Vector2(700f, 48f);

            Tab(tabs, "shop.furniture", ShopItemKind.Placeable);
            Tab(tabs, "shop.clothes", ShopItemKind.Equipment);
            Tab(tabs, "shop.gifts", ShopItemKind.Gift);

            _list = UiKit.ScrollList(transform, new Vector2(820f, 480f));
            UiKit.Anchor((RectTransform)_list.parent, new Vector2(0.5f, 0f), new Vector2(0f, 20f));
        }

        private void Tab(Transform parent, string key, ShopItemKind kind)
        {
            UiKit.Button(parent, Strings.Get(key), new Vector2(210f, 44f), Palette.Wall, () =>
            {
                _tab = kind;
                Refresh();
            });
        }

        public override void Refresh()
        {
            if (Session == null) return;

            for (var i = 0; i < _rows.Count; i++)
            {
                if (_rows[i] != null) Destroy(_rows[i]);
            }

            _rows.Clear();
            _money.text = Session.Profile.Wallet.Money + " " +
                          Strings.Get("cafe.money").ToLowerInvariant();

            foreach (var item in Shop.StockFor(Session.Profile.CafeLevel))
            {
                if (item.Kind != _tab) continue;
                BuildRow(item);
            }
        }

        private void BuildRow(ShopItem item)
        {
            var affordable = Session.Profile.Wallet.CanAfford(item.Price);
            var row = UiKit.Panel(_list, new Vector2(790f, 62f),
                affordable ? Palette.Panel : Palette.WithAlpha(Palette.Wall, 0.5f));

            var name = UiKit.Label(row, item.DisplayName, Typeface.Body, Palette.Text,
                TextAnchor.MiddleLeft, bold: true);
            name.rectTransform.sizeDelta = new Vector2(300f, 26f);
            name.rectTransform.anchoredPosition = new Vector2(-220f, 12f);

            var description = UiKit.Label(row, item.Description, Typeface.Tiny, Palette.TextDim,
                TextAnchor.MiddleLeft);
            description.rectTransform.sizeDelta = new Vector2(440f, 22f);
            description.rectTransform.anchoredPosition = new Vector2(-150f, -12f);

            var owned = Owned(item);
            if (owned > 0)
            {
                var stock = UiKit.Label(row, Strings.Get("shop.owned") + ": " + owned,
                    Typeface.Tiny, Palette.Focus, TextAnchor.MiddleRight, bold: true);
                stock.rectTransform.sizeDelta = new Vector2(160f, 22f);
                stock.rectTransform.anchoredPosition = new Vector2(190f, 12f);
            }

            var buy = UiKit.Button(row, item.Price + "", new Vector2(150f, 44f),
                affordable ? Palette.Focus : Palette.Wall, () => Buy(item));

            UiKit.Anchor(buy.GetComponent<RectTransform>(), new Vector2(1f, 0.5f),
                new Vector2(-10f, 0f));
            buy.interactable = affordable;

            _rows.Add(row.gameObject);
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

        /// <summary>
        /// Buys, or says why not.
        /// </summary>
        /// <remarks>
        /// A gift needs somebody to give it to, so it asks. Everything else goes straight into
        /// the inventory, and placing or wearing it is a separate decision taken on a screen
        /// built for it.
        /// </remarks>
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

            // One maid per press, cycling: a picker here would be a third screen for a
            // decision the player makes by looking at the roster anyway.
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
