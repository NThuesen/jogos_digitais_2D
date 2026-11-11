using System;
using System.Collections.Generic;
using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Game.View;
using Neko.Sim.Cafe;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Game.Ui
{
    /// <summary>
    /// Arranging the room, with the four numbers it is worth shown live.
    /// </summary>
    /// <remarks>
    /// Decoration with consequences. Ambience and Flow feed the day directly, and the room's
    /// dominant theme feeds the lineup's chemistry — so a player who commits the whole room
    /// to one style and then hires maids from it is making a real, compounding decision.
    ///
    /// UI-INT-05: the screen is drawn from the Hall kit — an office desk with a drawing board, a metrics
    /// plate and a stock tray. The board's opening shows the real, live <see cref="RoomView"/> (the one
    /// camera behind the canvas, framed into <see cref="RoomWindow"/>); clicks count only inside that
    /// window. If a kit sprite is missing the earlier procedural layout is built instead, unchanged.
    /// </remarks>
    public sealed class HallScreen : Screen
    {
        // Mockup (1625x968) pixels to the 1280x720 stage; every cut-out sprite carries 8 px of padding.
        private const float K = 720f / 968f;
        private const float MockupWidth = 1625f;
        private const float MockupHeight = 968f;

        // The part of the desk that is opened up for the room: everything outside it is the office.
        private static readonly UnityEngine.Rect Opening = UnityEngine.Rect.MinMaxRect(431f, 200f, 1158f, 703f);

        // The safe rectangle inside the board's opening where the room is framed and clicks count.
        private static readonly UnityEngine.Rect Safe = UnityEngine.Rect.MinMaxRect(452f, 204f, 1142f, 698f);

        private static readonly string[] KitSprites =
        {
            "hall_office_background", "hall_board_frame", "hall_title_empty",
            "hall_metrics_plate_empty", "hall_metrics_divider", "hall_icon_seat", "hall_icon_ambience",
            "hall_icon_flow", "hall_icon_theme", "hall_instruction_strip_empty", "hall_inventory_tray_empty",
            "hall_inventory_cell_empty", "hall_inventory_cell_selected", "hall_inventory_paw",
            "hall_stock_label_empty", "back_blank",
        };

        /// <summary>For tests: build the earlier procedural Hall even though the kit is present.</summary>
        public static bool ForcePlain;

        private readonly List<GameObject> _stock = new List<GameObject>();
        private readonly Dictionary<string, Image> _cells = new Dictionary<string, Image>();

        private RectTransform _stockList;
        private Text _metrics;
        private Text _picked;
        private string _holding;
        private bool _rotated;

        // The kit layout.
        private bool _kit;
        private ContainStage _stage;
        private RoomWindow _window;
        private Text _seats;
        private Text _ambience;
        private Text _flow;
        private Text _theme;
        private Image _strip;
        private RectTransform _pickedSlot;
        private Image _tray;
        private Text _trayLabel;
        private Image _stockPlate;
        private Text _stockPlateLabel;
        private RectTransform _cellView;

        // What the camera clears to behind the room while the board is open: the paper of the drawing board.
        private static readonly Color Paper = new Color32(0xF2, 0xE6, 0xD2, 0xFF);
        private Color _clear;
        private bool _tinted;

        public GameSession Session;
        public RoomView Room;
        public CameraRig Camera;
        public Action OnBack;

        private void OnEnable()
        {
            if (!_kit || Camera == null || Camera.Camera == null || _tinted) return;

            _clear = Camera.Camera.backgroundColor;
            Camera.Camera.backgroundColor = Paper;
            _tinted = true;
        }

        private void OnDisable()
        {
            if (!_tinted) return;

            _tinted = false;
            if (Camera != null && Camera.Camera != null) Camera.Camera.backgroundColor = _clear;
        }

        /// <summary>True when this screen was built from the kit (the room sits in the board's opening).</summary>
        public bool IsKit => _kit;

        /// <summary>The window of the screen that shows the room, in camera pixels (kit layout only).</summary>
        public RoomWindow Window => _window;

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

        // ========================================================================================
        // The kit layout
        // ========================================================================================

        private void BuildKit()
        {
            var stage = _stage = ContainStage.Make(transform, MockupWidth, MockupHeight, K, 8f);
            stage.Letterbox(Palette.Hex("#3A2418"));
            var root = stage.Rect;

            // In front of the room: the office, with the opening left out. The same background picture, cut
            // into four strips around the opening by texture coordinates, so nothing is redrawn or edited.
            var office = KitArt.Get("hall_office_background");
            var scenery = stage.Scenery();
            Strip(scenery, office, 0f, 0f, MockupWidth, Opening.yMin, "OfficeTop");
            Strip(scenery, office, 0f, Opening.yMax, MockupWidth, MockupHeight, "OfficeBottom");
            Strip(scenery, office, 0f, Opening.yMin, Opening.xMin, Opening.yMax, "OfficeLeft");
            Strip(scenery, office, Opening.xMax, Opening.yMin, MockupWidth, Opening.yMax, "OfficeRight");

            // The board, with its real alpha opening.
            stage.Exact(root, "hall_board_frame", 292f, 118f, 1348f, 832f, "Board");

            // The window the room is framed into and clicks are accepted in.
            var window = stage.Slot(root, "RoomWindow", Safe.xMin, Safe.yMin, Safe.xMax, Safe.yMax);
            _window = window.gameObject.AddComponent<RoomWindow>();
            _window.Changed += OnWindowChanged;

            BuildHeader(stage, root);
            BuildInstruction(stage, root);
            BuildTray(stage, root);
        }

        private static void Strip(Transform parent, Sprite sprite, float x0, float y0, float x1, float y1, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var raw = go.AddComponent<RawImage>();
            raw.texture = sprite.texture;
            raw.raycastTarget = false;
            raw.uvRect = new Rect(x0 / MockupWidth, 1f - y1 / MockupHeight, (x1 - x0) / MockupWidth,
                (y1 - y0) / MockupHeight);

            var rect = raw.rectTransform;
            UiKit.Anchor(rect, new Vector2(0f, 1f), new Vector2(x0 * K, -y0 * K));
            rect.sizeDelta = new Vector2((x1 - x0) * K, (y1 - y0) * K);
        }

        private void BuildHeader(ContainStage stage, RectTransform root)
        {
            var title = stage.At(root, "hall_title_empty", 115f, 12f, 494f, 88f, "TitlePlate");
            KitArt.Fitted(title.transform, Strings.Get("hall.title"), 26, 14, KitArt.Brown,
                stage.Region(115f, 12f, 494f, 88f, 168f, 26f, 442f, 76f));

            stage.At(root, "hall_metrics_plate_empty", 498f, 13f, 1346f, 88f, "MetricsPlate");

            stage.At(root, "hall_icon_seat", 551f, 31f, 586f, 71f, "SeatIcon");
            stage.At(root, "hall_icon_ambience", 720f, 31f, 760f, 71f, "AmbienceIcon");
            stage.At(root, "hall_icon_flow", 908f, 30f, 951f, 71f, "FlowIcon");
            stage.At(root, "hall_icon_theme", 1115f, 30f, 1158f, 72f, "ThemeIcon");

            foreach (var x in new[] { 699f, 887f, 1094f })
            {
                stage.At(root, "hall_metrics_divider", x, 37f, x + 9f, 64f, "Divider");
            }

            _seats = Segment(stage, root, "Seats", 594f, 690f);
            _ambience = Segment(stage, root, "Ambience", 766f, 880f);
            _flow = Segment(stage, root, "Flow", 958f, 1086f);
            _theme = Segment(stage, root, "Theme", 1164f, 1336f);

            var back = KitArt.Button(root, "back_blank", Strings.Get("dialog.back"), new Vector2(140f, 60f),
                () => OnBack?.Invoke(), 22, 12, KitArt.Brown, KitArt.Margins("back_blank", 86, 27, 90, 35));
            back.name = "Back";
            UiKit.Anchor(back.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(-14f, -4f));
        }

        private static Text Segment(ContainStage stage, RectTransform root, string name, float x0, float x1)
        {
            var slot = stage.Slot(root, name, x0, 30f, x1, 72f);
            var text = KitArt.Fitted(slot, "", 18, 11, KitArt.Brown, Vector4.zero, TextAnchor.MiddleLeft);
            text.gameObject.name = name + "Text";
            return text;
        }

        private void BuildInstruction(ContainStage stage, RectTransform root)
        {
            _strip = stage.At(root, "hall_instruction_strip_empty", 587f, 714f, 1009f, 761f, "InstructionStrip");
            _strip.pixelsPerUnitMultiplier = 1f / K;

            _pickedSlot = new GameObject("InstructionText", typeof(RectTransform)).GetComponent<RectTransform>();
            _pickedSlot.SetParent(_strip.transform, false);
            _pickedSlot.anchorMin = Vector2.zero;
            _pickedSlot.anchorMax = Vector2.one;
            _pickedSlot.offsetMin = new Vector2(69f * K, 8f * K);
            _pickedSlot.offsetMax = new Vector2(-63f * K, -8f * K);

            _picked = KitArt.Fitted(_pickedSlot, Strings.Get("hall.place"), 19, 11, KitArt.Brown, Vector4.zero);
            _picked.gameObject.name = "Picked";
            _picked.fontStyle = FontStyle.Normal;
        }

        private void BuildTray(ContainStage stage, RectTransform root)
        {
            _tray = stage.At(root, "hall_inventory_tray_empty", 374f, 822f, 1234f, 930f, "InventoryTray");
            _tray.pixelsPerUnitMultiplier = 1f / K;

            // Empty: the stock plate with its cube and the count. (The kit also draws a "Loja" button here;
            // the Hall has no shop callback, so that art is not imported and no button is faked.)
            _stockPlate = stage.At(root, "hall_stock_label_empty", 640f, 841f, 970f, 908f, "StockPlate");
            _stockPlate.pixelsPerUnitMultiplier = 1f / K;
            var plateText = new GameObject("StockText", typeof(RectTransform)).GetComponent<RectTransform>();
            plateText.SetParent(_stockPlate.transform, false);
            plateText.anchorMin = Vector2.zero;
            plateText.anchorMax = Vector2.one;
            plateText.offsetMin = new Vector2(66f * K, 14f * K);
            plateText.offsetMax = new Vector2(-20f * K, -14f * K);
            _stockPlateLabel = KitArt.Fitted(plateText, "", 19, 11, KitArt.Brown, Vector4.zero, TextAnchor.MiddleLeft);
            _stockPlateLabel.fontStyle = FontStyle.Normal;

            // Populated: a heading over the cells.
            var heading = stage.Slot(root, "StockHeading", 556f, 804f, 1052f, 836f);
            _trayLabel = KitArt.Fitted(heading, "", 20, 12, KitArt.Brown, Vector4.zero);

            // The cells scroll sideways when there are more kinds than fit.
            _cellView = stage.Slot(root, "StockViewport", 438f, 836f, 1170f, 940f);
            _cellView.gameObject.AddComponent<RectMask2D>();
            var scroll = _cellView.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = true;
            scroll.vertical = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 28f;

            _stockList = UiKit.Row(_cellView, 8f * K);
            _stockList.gameObject.name = "Content";
            _stockList.anchorMin = new Vector2(0f, 0f);
            _stockList.anchorMax = new Vector2(0f, 1f);
            _stockList.pivot = new Vector2(0f, 0.5f);
            _stockList.sizeDelta = Vector2.zero;
            _stockList.anchoredPosition = Vector2.zero;
            _stockList.gameObject.AddComponent<ContentSizeFitter>().horizontalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = _cellView;
            scroll.content = _stockList;
        }

        private void OnWindowChanged(UnityEngine.Rect pixels)
        {
            if (Session == null) return;

            var grid = Session.Profile.Grid;
            Camera?.FrameRoomInWindow(grid.Width, grid.Height, pixels);
        }

        // ========================================================================================
        // The earlier procedural layout: the fallback when a kit sprite is missing
        // ========================================================================================

        private void BuildPlain()
        {
            var bar = Scenery.Plaque(transform, new Vector2(1100f, 58f), null, exact: true);
            UiKit.Anchor((RectTransform)bar.parent, new Vector2(0.5f, 1f), new Vector2(0f, -8f));

            var title = UiKit.Label(bar, Strings.Get("hall.title"), Typeface.Body, Palette.Text,
                TextAnchor.MiddleLeft, bold: true);
            title.rectTransform.sizeDelta = new Vector2(300f, 28f);
            title.rectTransform.anchoredPosition = new Vector2(-390f, 0f);

            _metrics = UiKit.Label(bar, "", Typeface.Small, Palette.Text, TextAnchor.MiddleRight);
            _metrics.rectTransform.sizeDelta = new Vector2(740f, 28f);
            _metrics.rectTransform.anchoredPosition = new Vector2(120f, 0f);

            Scenery.PlankAt(transform, Strings.Get("dialog.back"), new Vector2(120f, 44f),
                new Vector2(1f, 1f), new Vector2(-26f, -78f), () => OnBack?.Invoke(),
                Typeface.Small);

            _picked = UiKit.Label(transform, Strings.Get("hall.place"), Typeface.Small,
                Palette.WithAlpha(Palette.Cream, 0.85f));
            UiKit.Anchor(_picked.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 150f));
            _picked.rectTransform.sizeDelta = new Vector2(900f, 26f);

            _stockList = UiKit.ScrollRow(transform, new Vector2(1100f, 120f), 8f);
            UiKit.Anchor((RectTransform)_stockList.parent, new Vector2(0.5f, 0f),
                new Vector2(0f, 16f));
        }

        // ========================================================================================
        // Behaviour (shared by both layouts)
        // ========================================================================================

        public override void Refresh()
        {
            if (Session == null) return;

            Room?.Build(Session.Profile.Grid);
            Room?.SetFlowOverlay(true);

            if (_kit && _window != null)
            {
                // Into the board's opening, uniformly scaled and centred; again whenever the window moves.
                _window.Reframe();
            }
            else
            {
                // Between the title bar and the stock shelf, so neither covers a floor tile.
                Camera?.FrameRoomBetweenHud(Session.Profile.Grid.Width, Session.Profile.Grid.Height);
            }

            SyncMetrics();
            BuildStock();
        }

        public void SyncMetrics()
        {
            var metrics = Session.Profile.Grid.Metrics();
            var theme = metrics.ThemeBonusActive
                ? metrics.DominantTheme.ToString()
                : Strings.Get("hall.notheme");

            if (_kit)
            {
                _seats.text = Strings.Get("hall.seats") + " " + metrics.Seats;
                _ambience.text = Strings.Get("hall.ambience") + " " + metrics.Ambience.ToString("0.0");
                _flow.text = Strings.Get("hall.flow") + " " + metrics.Flow.ToString("0.0");
                _theme.text = Strings.Get("hall.theme") + " " + theme;
                return;
            }

            _metrics.text =
                Strings.Get("hall.seats") + " " + metrics.Seats + "    " +
                Strings.Get("hall.ambience") + " " + metrics.Ambience.ToString("0.0") + "    " +
                Strings.Get("hall.flow") + " " + metrics.Flow.ToString("0.0") + "    " +
                Strings.Get("hall.theme") + " " + theme;
        }

        private void BuildStock()
        {
            for (var i = 0; i < _stock.Count; i++)
            {
                if (_stock[i] == null) continue;

                // Out of the layout now: a click may be what asked for this rebuild.
                _stock[i].SetActive(false);
                _stock[i].transform.SetParent(null, false);
                Destroy(_stock[i]);
            }

            _stock.Clear();
            _cells.Clear();

            var counts = new Dictionary<string, int>();
            foreach (var id in Session.Profile.OwnedPlaceables)
            {
                counts.TryGetValue(id, out var count);
                counts[id] = count + 1;
            }

            if (_kit) ShowTray(counts);

            foreach (var pair in counts)
            {
                var def = PlaceableCatalog.ById(pair.Key);
                var card = _kit ? KitCell(def, pair.Value) : PlainCard(def, pair.Value);

                var id = pair.Key;
                card.GetComponent<Button>().onClick.AddListener(() =>
                {
                    _holding = id;
                    _rotated = false;
                    SetPicked(def.DisplayName + " — " + Strings.Get("hall.place") +
                              " · " + Strings.Get("hall.rotate"));
                    MarkHeld();
                });

                _stock.Add(card.gameObject);
            }

            MarkHeld();

            if (counts.Count != 0 || _kit) return;

            var empty = UiKit.Label(_stockList,
                Strings.Get("hall.stock") + ": 0 — " + Strings.Get("shop.title"),
                Typeface.Small, Palette.TextDim);
            empty.rectTransform.sizeDelta = new Vector2(600f, 40f);
            _stock.Add(empty.gameObject);
        }

        private RectTransform PlainCard(PlaceableDef def, int count)
        {
            var card = UiKit.Panel(_stockList, new Vector2(112f, 100f), Palette.Panel, 0.2f, exact: true);
            card.GetComponent<Image>().raycastTarget = true;

            var icon = new GameObject("Icon", typeof(RectTransform)).AddComponent<Image>();
            icon.transform.SetParent(card, false);
            icon.rectTransform.sizeDelta = new Vector2(64f, 58f);
            icon.rectTransform.anchoredPosition = new Vector2(0f, 14f);
            icon.raycastTarget = false;

            // The same picture the room draws, not a stand-in shape, so what is picked up
            // here is visibly what goes down on the floor.
            icon.sprite = UiKit.Sprites.Furniture(def, def.Width, def.Height);
            icon.preserveAspect = true;

            var label = UiKit.Label(card, def.DisplayName + " ×" + count, Typeface.Tiny,
                Palette.Text);
            label.rectTransform.sizeDelta = new Vector2(104f, 32f);
            label.rectTransform.anchoredPosition = new Vector2(0f, -30f);

            var button = card.gameObject.AddComponent<Button>();
            button.targetGraphic = card.GetComponent<Image>();
            return card;
        }

        private RectTransform KitCell(PlaceableDef def, int count)
        {
            var cell = KitArt.Plate(_stockList, "hall_inventory_cell_empty",
                new Vector2((136f + 16f) * K, (100f + 16f) * K));
            cell.gameObject.name = "Cell_" + def.Id;
            cell.pixelsPerUnitMultiplier = 1f / K;
            cell.raycastTarget = true;
            _stage.TrimHitArea(cell);
            _cells[def.Id] = cell;

            var button = cell.gameObject.AddComponent<Button>();
            button.targetGraphic = cell;
            button.transition = Selectable.Transition.ColorTint;
            var colours = button.colors;
            colours.normalColor = Color.white;
            colours.highlightedColor = new Color(1.06f, 1.04f, 1.02f, 1f);
            colours.pressedColor = new Color(0.86f, 0.82f, 0.78f, 1f);
            colours.selectedColor = Color.white;
            colours.fadeDuration = 0.06f;
            button.colors = colours;

            var icon = new GameObject("Icon", typeof(RectTransform)).AddComponent<Image>();
            icon.transform.SetParent(cell.transform, false);
            icon.rectTransform.sizeDelta = new Vector2(44f, 34f);
            UiKit.Anchor(icon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -9f));
            icon.raycastTarget = false;

            // The same picture the room draws, so what is picked up is visibly what goes down.
            icon.sprite = UiKit.Sprites.Furniture(def, def.Width, def.Height);
            icon.preserveAspect = true;

            var label = KitArt.Fitted(cell.transform, def.DisplayName + " ×" + count, 15, 11, KitArt.Brown,
                new Vector4(0.08f, 0.1f, 0.08f, 0.52f));
            label.gameObject.name = "Label";
            label.fontStyle = FontStyle.Normal;
            return cell.rectTransform;
        }

        /// <summary>Shows the empty plate or the populated tray, as the stock calls for.</summary>
        private void ShowTray(Dictionary<string, int> counts)
        {
            var total = 0;
            foreach (var pair in counts) total += pair.Value;

            var populated = counts.Count != 0;
            var top = populated ? 800f : 822f;
            var height = (populated ? 164f : 108f) + 16f;
            _tray.rectTransform.anchoredPosition = new Vector2(_tray.rectTransform.anchoredPosition.x,
                -(top - 8f) * K);
            _tray.rectTransform.sizeDelta = new Vector2(_tray.rectTransform.sizeDelta.x, height * K);

            _stockPlate.gameObject.SetActive(!populated);
            _trayLabel.gameObject.SetActive(populated);
            _cellView.gameObject.SetActive(populated);

            _stockPlateLabel.text = Strings.Get("hall.stock") + ": 0 — " + Strings.Get("shop.title");
            _trayLabel.text = Strings.Get("hall.stock") + ": " + total;
        }

        private void MarkHeld()
        {
            if (!_kit) return;

            foreach (var pair in _cells)
            {
                pair.Value.sprite = KitArt.Get(pair.Key == _holding
                    ? "hall_inventory_cell_selected"
                    : "hall_inventory_cell_empty");
            }
        }

        /// <summary>The line under the board. In the kit layout its strip widens to hold a long one.</summary>
        private void SetPicked(string text)
        {
            _picked.text = text;
            if (!_kit) return;

            const float min = 422f + 16f;
            var wide = Mathf.Clamp(_picked.preferredWidth / K + (69f + 63f), min, 760f);
            _strip.rectTransform.sizeDelta = new Vector2(wide * K, _strip.rectTransform.sizeDelta.y);
            _strip.rectTransform.anchoredPosition = new Vector2((798f - wide / 2f) * K,
                _strip.rectTransform.anchoredPosition.y);
        }

        public void Rotate()
        {
            _rotated = !_rotated;
            SetPicked(Strings.Get("hall.rotate") + ": " +
                      Strings.Get(_rotated ? "dialog.yes" : "dialog.no"));
        }

        /// <summary>Puts the held piece down, or says it does not fit.</summary>
        public bool PlaceAt(int x, int y)
        {
            if (_holding == null || Session == null) return false;

            var def = PlaceableCatalog.ById(_holding);
            if (!Session.Profile.Grid.Place(def, x, y, _rotated))
            {
                SetPicked(Strings.Get("hall.blocked"));
                return false;
            }

            Session.Profile.OwnedPlaceables.Remove(_holding);
            if (!Session.Profile.OwnedPlaceables.Contains(_holding)) _holding = null;

            Session.Touch();
            Refresh();
            return true;
        }

        /// <summary>Takes a piece back into stock. Nothing bought is ever destroyed.</summary>
        public bool RemoveAt(int x, int y)
        {
            if (Session == null) return false;

            var item = Session.Profile.Grid.ItemAt(x, y);
            if (item == null || item.Def.Kind == PlaceableKind.Fixture) return false;

            Session.Profile.Grid.Remove(x, y);
            Session.Profile.OwnedPlaceables.Add(item.Def.Id);

            Session.Touch();
            Refresh();
            return true;
        }
    }
}
