using System;
using System.Collections.Generic;
using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Sim.Cafe;
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
        private Image _coin;
        private readonly List<(Button Button, ShopItemKind Kind)> _tabs =
            new List<(Button, ShopItemKind)>();
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

            // The back control is the approved secondary button (UI-005 NORMAL/HOVER/PRESSED/DISABLED).
            var back = UiArt.StateButton(transform, Strings.Get("dialog.back"), new Vector2(120f, 44f),
                () => OnBack?.Invoke(), "btn2", Typeface.Small);
            UiKit.Anchor(back.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(-26f, -18f));

            _coin = UiArt.Has("icon_coin")
                ? UiArt.Picture(transform, "icon_coin", new Vector2(34f, 34f))
                : null;
            if (_coin != null)
            {
                UiKit.Anchor(_coin.rectTransform, new Vector2(1f, 1f), new Vector2(-300f, -26f));
            }

            var tabs = UiKit.Row(transform, 10f);
            UiKit.Anchor(tabs, new Vector2(0.5f, 1f), new Vector2(0f, -82f));
            tabs.sizeDelta = new Vector2(700f, 48f);

            Tab(tabs, "shop.furniture", ShopItemKind.Placeable);
            Tab(tabs, "shop.clothes", ShopItemKind.Equipment);
            Tab(tabs, "shop.gifts", ShopItemKind.Gift);

            _list = UiKit.ScrollList(transform, new Vector2(880f, 460f), 8f, exact: true);
            UiKit.Anchor((RectTransform)_list.parent, new Vector2(0.5f, 0f), new Vector2(0f, 24f));
        }

        private void Tab(Transform parent, string key, ShopItemKind kind)
        {
            var tab = UiArt.Tab(parent, Strings.Get(key), new Vector2(210f, 44f), () =>
            {
                _tab = kind;
                Refresh();
            }, Typeface.Small);
            _tabs.Add((tab, kind));
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
            PlaceCoin();

            for (var i = 0; i < _tabs.Count; i++)
            {
                UiArt.SetSelected(_tabs[i].Button, _tabs[i].Kind == _tab);
            }

            foreach (var item in Shop.StockFor(Session.Profile.CafeLevel))
            {
                if (item.Kind != _tab) continue;
                BuildRow(item);
            }
        }

        /// <summary>Keeps the coin icon just left of the right-aligned amount, whatever its width.</summary>
        private void PlaceCoin()
        {
            if (_coin == null) return;

            // Both pivots are top-right, so the coin's right edge sits a few units left of the text.
            var money = _money.rectTransform.anchoredPosition;
            _coin.rectTransform.anchoredPosition = new Vector2(
                money.x - _money.preferredWidth - 8f, money.y - 3f);
        }

        private void BuildRow(ShopItem item)
        {
            var affordable = Session.Profile.Wallet.CanAfford(item.Price);
            var row = UiKit.Panel(_list, new Vector2(850f, 62f),
                Palette.WithAlpha(affordable ? Palette.Panel : Palette.Wall, 0.92f), 0.2f, exact: true);
            _rows.Add(row.gameObject);

            // One universal frame (UI-017 v02, 9-sliced) for all three tabs. The state tint stays
            // in Unity: a row the player cannot afford is dimmed instead of recoloured.
            var frame = UiArt.Get("ui017_shop_row");
            if (frame != null)
            {
                var skin = row.GetComponent<Image>();
                skin.sprite = frame;
                skin.type = Image.Type.Sliced;
                skin.pixelsPerUnitMultiplier = frame.rect.height / 62f;
                skin.color = affordable ? Color.white : new Color(0.78f, 0.74f, 0.72f, 1f);
            }

            // Furniture shows the piece itself. The panel is 9-sliced, so the row's drawn edge is its
            // rect edge (-425). The text moves right past the icon and keeps its right edge.
            var indent = 0f;
            if (item.Kind == ShopItemKind.Placeable)
            {
                var def = PlaceableCatalog.ById(item.Id);
                var icon = new GameObject("Icon", typeof(RectTransform)).AddComponent<Image>();
                icon.transform.SetParent(row, false);
                icon.rectTransform.sizeDelta = new Vector2(60f, 50f);
                icon.rectTransform.anchoredPosition = new Vector2(-322f, 0f);
                icon.raycastTarget = false;
                icon.sprite = UiKit.Sprites.Furniture(def, def.Width, def.Height);
                icon.preserveAspect = true;
                indent = 118f;
            }
            else
            {
                var badge = item.Kind == ShopItemKind.Gift ? "icon_gift" : "icon_cat_paw";
                if (UiArt.Has(badge))
                {
                    var icon = UiArt.Picture(row, badge, new Vector2(40f, 40f));
                    icon.rectTransform.anchoredPosition = new Vector2(-352f, 0f);
                    indent = 80f;
                }
            }

            var name = UiKit.Label(row, item.DisplayName, Typeface.Body, Palette.Text,
                TextAnchor.MiddleLeft, bold: true);
            name.rectTransform.sizeDelta = new Vector2(320f, 26f);
            name.rectTransform.anchoredPosition = new Vector2(-240f + indent, 12f);

            var description = UiKit.Label(row, item.Description, Typeface.Tiny, Palette.TextDim,
                TextAnchor.MiddleLeft);
            description.rectTransform.sizeDelta = new Vector2(470f - indent, 22f);
            description.rectTransform.anchoredPosition = new Vector2(-165f + indent * 0.5f, -12f);

            var owned = Owned(item);
            if (owned > 0)
            {
                var stock = UiKit.Label(row, Strings.Get("shop.owned") + ": " + owned,
                    Typeface.Tiny, Logo.Brass, TextAnchor.MiddleRight, bold: true);
                // Right edge at 250: the buy plank starts at 265, so the count is never under it.
                stock.rectTransform.sizeDelta = new Vector2(170f, 22f);
                stock.rectTransform.anchoredPosition = new Vector2(165f, 14f);
            }

            var buy = UiArt.StateButton(row, Strings.Money(item.Price), new Vector2(150f, 44f),
                () => Buy(item), "btn1", Typeface.Small);

            UiKit.Anchor(buy.GetComponent<RectTransform>(), new Vector2(1f, 0.5f),
                new Vector2(-10f, 0f));
            buy.interactable = affordable;
            // DISABLED must read as disabled in Unity too, not only in the art: dim, cooler text.
            buy.GetComponentInChildren<Text>().color = affordable
                ? Palette.Hex("#3A2A1A")
                : new Color(0.42f, 0.38f, 0.36f, 0.75f);
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
