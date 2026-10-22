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
    /// The design calls decoration without consequence an anti-goal. What makes this a layout
    /// puzzle rather than a dress-up screen is that Ambience, Flow and Seats all move while
    /// the player drags, and the floor turns into a heat map of how far anybody has to walk —
    /// so a bad layout is visible before the day that proves it.
    /// </remarks>
    public sealed class EditorScreen : Screen
    {
        private readonly List<GameObject> _stock = new List<GameObject>();

        private RectTransform _stockList;
        private Text _metrics;
        private Text _picked;
        private string _holding;
        private bool _rotated;

        public GameSession Session;
        public RoomView Room;
        public CameraRig Camera;
        public System.Action OnBack;

        public string Holding => _holding;

        protected override void Build()
        {
            var header = UiKit.Panel(transform, new Vector2(1240f, 64f),
                Palette.WithAlpha(Palette.Panel, 0.95f));
            UiKit.Anchor(header, new Vector2(0.5f, 1f), new Vector2(0f, -10f));

            var title = UiKit.Label(header, Strings.Get("editor.title"), Typeface.Body,
                Palette.Text, TextAnchor.MiddleLeft, bold: true);
            title.rectTransform.sizeDelta = new Vector2(300f, 30f);
            title.rectTransform.anchoredPosition = new Vector2(-440f, 0f);

            _metrics = UiKit.Label(header, "", Typeface.Small, Palette.Text,
                TextAnchor.MiddleRight);
            _metrics.rectTransform.sizeDelta = new Vector2(820f, 30f);
            _metrics.rectTransform.anchoredPosition = new Vector2(150f, 0f);

            var back = UiKit.Button(transform, "<", new Vector2(56f, 48f), Palette.Wall, () =>
            {
                _holding = null;
                OnBack?.Invoke();
            });
            UiKit.Anchor(back.GetComponent<RectTransform>(), new Vector2(1f, 1f),
                new Vector2(-16f, -84f));

            _picked = UiKit.Label(transform, Strings.Get("editor.place"), Typeface.Small,
                Palette.TextDim);
            UiKit.Anchor(_picked.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 150f));
            _picked.rectTransform.sizeDelta = new Vector2(900f, 26f);

            _stockList = UiKit.ScrollRow(transform, new Vector2(1180f, 120f), 8f);
            UiKit.Anchor((RectTransform)_stockList.parent, new Vector2(0.5f, 0f),
                new Vector2(0f, 16f));
        }

        public override void Refresh()
        {
            if (Session == null) return;

            Room?.Build(Session.Profile.Grid);
            Room?.SetFlowOverlay(true);
            Camera?.FrameRoom(Session.Profile.Grid.Width, Session.Profile.Grid.Height);

            SyncMetrics();
            BuildStock();
        }

        public void SyncMetrics()
        {
            var metrics = Session.Profile.Grid.Metrics();
            var theme = metrics.ThemeBonusActive
                ? metrics.DominantTheme.ToString()
                : Strings.Get("editor.notheme");

            _metrics.text =
                Strings.Get("editor.seats") + " " + metrics.Seats + "    " +
                Strings.Get("editor.ambience") + " " + metrics.Ambience.ToString("0.0") + "    " +
                Strings.Get("editor.flow") + " " + metrics.Flow.ToString("0.0") + "    " +
                Strings.Get("editor.theme") + " " + theme;
        }

        private void BuildStock()
        {
            for (var i = 0; i < _stock.Count; i++)
            {
                if (_stock[i] != null) Destroy(_stock[i]);
            }

            _stock.Clear();

            var counts = new Dictionary<string, int>();
            foreach (var id in Session.Profile.OwnedPlaceables)
            {
                counts.TryGetValue(id, out var count);
                counts[id] = count + 1;
            }

            foreach (var pair in counts)
            {
                var def = PlaceableCatalog.ById(pair.Key);
                var card = UiKit.Panel(_stockList, new Vector2(112f, 100f), Palette.Panel);
                card.GetComponent<Image>().raycastTarget = true;

                var icon = new GameObject("Icon", typeof(RectTransform)).AddComponent<Image>();
                icon.transform.SetParent(card, false);
                icon.rectTransform.sizeDelta = new Vector2(52f, 52f);
                icon.rectTransform.anchoredPosition = new Vector2(0f, 16f);
                icon.raycastTarget = false;
                icon.sprite = UiKit.Sprites.Get(new ShapeSpec(Shapes.Parse(def.ShapeId),
                    Palette.Hex(def.TintHex), Palette.Ink, 0.05f));

                var label = UiKit.Label(card, def.DisplayName + " ×" + pair.Value, Typeface.Tiny,
                    Palette.Text);
                label.rectTransform.sizeDelta = new Vector2(104f, 30f);
                label.rectTransform.anchoredPosition = new Vector2(0f, -32f);

                var button = card.gameObject.AddComponent<Button>();
                button.targetGraphic = card.GetComponent<Image>();

                var id = pair.Key;
                button.onClick.AddListener(() =>
                {
                    _holding = id;
                    _rotated = false;
                    _picked.text = def.DisplayName + " — " + Strings.Get("editor.place") +
                                   " · " + Strings.Get("editor.rotate");
                });

                _stock.Add(card.gameObject);
            }

            if (counts.Count != 0) return;

            var empty = UiKit.Label(_stockList, Strings.Get("editor.stock") + ": 0 — " +
                                                Strings.Get("shop.title"), Typeface.Small,
                Palette.TextDim);
            empty.rectTransform.sizeDelta = new Vector2(600f, 40f);
            _stock.Add(empty.gameObject);
        }

        public void Rotate()
        {
            _rotated = !_rotated;
            _picked.text = Strings.Get("editor.rotate") + ": " +
                           Strings.Get(_rotated ? "dialog.yes" : "dialog.no");
        }

        /// <summary>Puts the held piece down, or says it does not fit.</summary>
        public bool PlaceAt(int x, int y)
        {
            if (_holding == null || Session == null) return false;

            var def = PlaceableCatalog.ById(_holding);
            if (!Session.Profile.Grid.Place(def, x, y, _rotated))
            {
                _picked.text = Strings.Get("editor.blocked");
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
