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
            if (!ForcePlain && KitArt.Has(KitSprites))
            {
                _kit = true;
                BuildKit();
                return;
            }

            BuildPlain();
        }

        private void BuildPlain()
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

            if (_kit)
            {
                RefreshKit();
                return;
            }

            RefreshPlain();
        }

        private void RefreshPlain()
        {
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

        // ========================================================================================
        // The illustrated screen (UI-INT-03B)
        // ========================================================================================
        // One layout for Móveis, Roupas and Presentes: only the list, its names, descriptions, prices
        // and icons change. Placed on a 1280x720 stage in pixels of the approved mockup (KitStage).
        // Every word and number is Unity Text over a blank plate. The row is only a skin: the buy
        // button is its own Button wired straight to Buy(item), never behind a selection.

        private const float S = KitStage.S;

        private static readonly string[] KitSprites =
        {
            "shop_background", "shop_title_empty", "shop_money_panel_empty", "shop_tab_normal_empty",
            "shop_tab_selected_empty", "shop_tab_underline", "shop_main_panel_empty", "shop_panel_top_bow",
            "shop_bottom_paw_ornament", "shop_product_row_normal", "shop_product_row_highlight", "shop_buy_empty",
            "back_blank", "bow_gold",
        };

        private static readonly float[] TabX = { 357f, 685f, 1014f };
        private static readonly Color Gold = Palette.Hex("#F6C85A");
        private static readonly Color OwnedGold = Palette.Hex("#8A5A06");

        private const float RowPitch = 119f;

        private bool _kit;

        /// <summary>Asks for the earlier procedural layout even when the kit is present (tests, tools).</summary>
        public bool ForcePlain;

        private RectTransform _tabsLayer;
        private RectTransform _content;
        private RectTransform _viewport;
        private Text _empty;
        private ShopItemKind _shownTab = ShopItemKind.Placeable;

        /// <summary>The category on show, for tests.</summary>
        public ShopItemKind Category => _tab;

        private void BuildKit()
        {
            KitArt.Backdrop(transform, "shop_background");
            var stage = KitStage.Make(transform);

            if (UiArt.Has("ui020_logo_wordmark"))
            {
                var logo = UiArt.Picture(stage, "ui020_logo_wordmark", new Vector2(176f, 86f));
                UiKit.Anchor(logo.rectTransform, new Vector2(0f, 1f), new Vector2(12f, -9f));
            }

            var title = KitStage.At(stage, "shop_title_empty", 576, 56, 1090, 155, "TitlePlate");
            KitArt.Fitted(title.transform, Strings.Get("shop.screen"), 38, 18, KitArt.Brown,
                KitStage.Region(576, 56, 1090, 155, 716, 72, 950, 140));
            var bow = KitArt.Picture(stage, "bow_gold", 118f);
            bow.gameObject.name = "Bow";
            UiKit.Anchor(bow.rectTransform, new Vector2(0.5f, 1f), new Vector2((833f - 836f) * S, -22f));

            // The money in the till: ¥, never fichas.
            KitStage.At(stage, "shop_money_panel_empty", 1202, 64, 1427, 133, "MoneyPlate");
            if (UiArt.Has("icon_coin"))
            {
                var coin = UiArt.Picture(stage, "icon_coin", new Vector2(36f, 36f));
                coin.gameObject.name = "Coin";
                UiKit.Anchor(coin.rectTransform, new Vector2(0f, 1f), new Vector2(1222f * S, -81f * S));
            }

            _money = KitArt.Fitted(KitStage.Slot(stage, "Money", 1262, 76, 1412, 122), "", 30, 12, Gold,
                Vector4.zero, TextAnchor.MiddleRight);

            var back = KitArt.Button(stage, "back_blank", Strings.Get("dialog.back"), new Vector2(150f, 64f),
                () => OnBack?.Invoke(), 22, 12, KitArt.Brown, KitArt.Margins("back_blank", 86, 27, 90, 35));
            back.name = "Back";
            UiKit.Anchor(back.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(-16f, -14f));

            _tabsLayer = KitStage.Layer(stage, "Tabs");

            KitStage.At(stage, "shop_main_panel_empty", 236, 268, 1437, 907, "ListPlate");
            KitStage.At(stage, "shop_panel_top_bow", 718, 271, 953, 336, "TopBow");

            _viewport = KitStage.Slot(stage, "Viewport", 282, 350, 1391, 825);
            _viewport.gameObject.AddComponent<RectMask2D>();
            var catcher = _viewport.gameObject.AddComponent<Image>();
            catcher.color = new Color(1f, 1f, 1f, 0f);
            catcher.raycastTarget = true;

            _content = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
            _content.SetParent(_viewport, false);
            _content.anchorMin = new Vector2(0f, 1f);
            _content.anchorMax = new Vector2(1f, 1f);
            _content.pivot = new Vector2(0.5f, 1f);
            _content.anchoredPosition = Vector2.zero;
            _content.sizeDelta = new Vector2(0f, 0f);

            var scroll = _viewport.gameObject.AddComponent<ScrollRect>();
            scroll.content = _content;
            scroll.viewport = _viewport;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;

            KitStage.At(stage, "shop_bottom_paw_ornament", 715, 840, 959, 906, "BottomOrnament");

            _empty = KitArt.Fitted(KitStage.Slot(stage, "ListEmpty", 400, 560, 1270, 640), "", 24, 12, KitArt.Brown,
                Vector4.zero);
            _empty.gameObject.SetActive(false);
        }

        private void RefreshKit()
        {
            _money.text = Strings.Money(Session.Profile.Wallet.Money);

            BuildTabsKit();

            var keep = _shownTab == _tab ? _content.anchoredPosition.y : 0f;
            _shownTab = _tab;

            KitStage.Discard(_content);

            var index = 0;
            foreach (var item in Shop.StockFor(Session.Profile.CafeLevel))
            {
                if (item.Kind != _tab) continue;
                BuildRowKit(item, index++);
            }

            _empty.gameObject.SetActive(index == 0);
            if (index == 0) _empty.text = Strings.Get("lineup.empty");

            var contentHeight = index * RowPitch * S + 4f;
            _content.sizeDelta = new Vector2(0f, contentHeight);
            var overflow = Mathf.Max(0f, contentHeight - _viewport.rect.height);
            _content.anchoredPosition = new Vector2(0f, Mathf.Clamp(keep, 0f, overflow));
        }

        private void BuildTabsKit()
        {
            KitStage.Discard(_tabsLayer);

            var kinds = new[] { ShopItemKind.Placeable, ShopItemKind.Equipment, ShopItemKind.Gift };
            var keys = new[] { "shop.furniture", "shop.clothes", "shop.gifts" };
            var names = new[] { "Furniture", "Clothes", "Gifts" };

            for (var i = 0; i < kinds.Length; i++)
            {
                var kind = kinds[i];
                var selected = kind == _tab;
                var x = TabX[i];
                var width = selected ? 304f : 300f;

                var button = KitStage.PlateButton(_tabsLayer, selected ? "shop_tab_selected_empty" : "shop_tab_normal_empty",
                    "Tab_" + names[i], x, 166, x + width, 255, () =>
                    {
                        _tab = kind;
                        RefreshKit();
                    });
                var label = KitArt.Fitted(button.transform, Strings.Get(keys[i]), 32, 14, KitArt.Brown,
                    new Vector4(0.12f, 0.24f, 0.12f, 0.22f));
                label.gameObject.name = "Label";

                if (selected)
                {
                    KitStage.At(_tabsLayer, "shop_tab_underline", x + 72f, 252, x + 72f + 161f, 267, "TabUnderline");
                }
            }
        }

        private void BuildRowKit(ShopItem item, int index)
        {
            var affordable = Session.Profile.Wallet.CanAfford(item.Price);

            var row = new GameObject("Row_" + item.Id, typeof(RectTransform)).GetComponent<RectTransform>();
            row.SetParent(_content, false);
            UiKit.Anchor(row, new Vector2(0f, 1f), new Vector2(7f * S, -(2f + index * RowPitch) * S));
            row.sizeDelta = new Vector2(1094f * S, 107f * S);

            var normal = KitArt.Plate(row, "shop_product_row_normal", new Vector2((1094f + 16f) * S, (107f + 16f) * S));
            normal.gameObject.name = "RowSkin";
            UiKit.Anchor(normal.rectTransform, new Vector2(0f, 1f), new Vector2(-8f * S, 8f * S));
            normal.color = affordable ? Color.white : new Color(0.84f, 0.8f, 0.78f, 1f);

            // The golden row is the hover: it grows a few pixels past the logical row, so nothing moves.
            var highlight = KitArt.Plate(row, "shop_product_row_highlight",
                new Vector2((1094f + 5f + 7f + 16f) * S, (107f + 4f + 1f + 16f) * S));
            highlight.gameObject.name = "RowHighlight";
            UiKit.Anchor(highlight.rectTransform, new Vector2(0f, 1f), new Vector2((-5f - 8f) * S, (4f + 8f) * S));
            highlight.gameObject.SetActive(false);

            var hit = row.gameObject.AddComponent<Image>();
            hit.color = new Color(1f, 1f, 1f, 0f);
            hit.raycastTarget = true;
            row.gameObject.AddComponent<RowHover>().Highlight = highlight.gameObject;

            // The icon: the furniture itself, or the existing badge. Never a recut of the mockup.
            var window = KitStage.Slot(row, "IconWindow", 64, 14, 184, 93);
            if (item.Kind == ShopItemKind.Placeable)
            {
                var def = PlaceableCatalog.ById(item.Id);
                var icon = new GameObject("Icon", typeof(RectTransform)).AddComponent<Image>();
                icon.transform.SetParent(window, false);
                UiKit.Stretch(icon.rectTransform);
                icon.raycastTarget = false;
                icon.sprite = UiKit.Sprites.Furniture(def, def.Width, def.Height);
                icon.preserveAspect = true;
            }
            else
            {
                var badge = item.Kind == ShopItemKind.Gift ? "icon_gift" : "icon_cat_paw";
                if (UiArt.Has(badge))
                {
                    var size = Mathf.Min(window.sizeDelta.x, window.sizeDelta.y);
                    var icon = UiArt.Picture(window, badge, new Vector2(size, size));
                    icon.gameObject.name = "Icon";
                    UiKit.Anchor(icon.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero);
                    icon.raycastTarget = false;
                }
            }

            var owned = Owned(item);
            var nameRight = owned > 0 ? 640f : 835f;
            var nameText = KitArt.Fitted(KitStage.Slot(row, "Name", 204, 12, nameRight, 52), item.DisplayName, 30, 12,
                KitArt.Brown, Vector4.zero, TextAnchor.MiddleLeft);
            nameText.horizontalOverflow = HorizontalWrapMode.Wrap;

            if (owned > 0)
            {
                KitArt.Fitted(KitStage.Slot(row, "Owned", 645, 12, 835, 52), Strings.Get("shop.owned") + ": " + owned,
                    19, 9, OwnedGold, Vector4.zero, TextAnchor.MiddleRight);
            }

            KitArt.Fitted(KitStage.Slot(row, "Description", 204, 56, 835, 101), item.Description, 20, 9,
                SoftBrown, Vector4.zero, TextAnchor.UpperLeft);

            var buy = KitStage.PlateButton(row, "shop_buy_empty", "Buy", 857f + 0f, 19f, 857f + 212f, 19f + 70f,
                () => Buy(item));
            var label = KitArt.Fitted(buy.transform, Strings.Money(item.Price), 30, 12, KitArt.Brown,
                KitStage.Region(1146, 371, 1358, 441, 1166, 380, 1340, 432));
            label.gameObject.name = "Label";
            buy.interactable = affordable;
            label.color = affordable ? KitArt.Brown : new Color(0.42f, 0.38f, 0.36f, 0.75f);
        }

        private static readonly Color SoftBrown = new Color(0.43f, 0.3f, 0.2f);

        /// <summary>Lights the golden row while the pointer is over a row.</summary>
        public sealed class RowHover : MonoBehaviour, UnityEngine.EventSystems.IPointerEnterHandler,
            UnityEngine.EventSystems.IPointerExitHandler
        {
            public GameObject Highlight;

            public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
            {
                if (Highlight != null) Highlight.SetActive(true);
            }

            public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
            {
                if (Highlight != null) Highlight.SetActive(false);
            }

            private void OnDisable()
            {
                if (Highlight != null) Highlight.SetActive(false);
            }
        }
    }
}
