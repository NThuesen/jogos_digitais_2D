using System;
using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Sim.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Game.Ui
{
    /// <summary>
    /// Between days: the balance, what it costs to open, and the four places to spend.
    /// </summary>
    /// <remarks>
    /// The fixed-cost line is the most important number on the screen and it is given that
    /// weight. A player who never looks at it goes bankrupt holding cards they could not
    /// afford, which is the losing condition the whole game is built around, so it is not
    /// allowed to be a footnote.
    /// </remarks>
    public sealed class HubScreen : Screen
    {
        private Text _day;
        private Text _money;
        private Text _points;
        private Text _costs;
        private Text _danger;
        private Text _level;
        private Image _levelBar;
        private RectTransform _today;
        private Text _chemistry;

        private readonly System.Collections.Generic.List<GameObject> _cards =
            new System.Collections.Generic.List<GameObject>();
        private Button _open;
        private Text _openLabel;
        private bool _kit;
        private RectTransform _fillTrack;
        private RectTransform _warning;

        public GameSession Session;
        public Action OnOpenDay;
        public Action OnLineup;
        public Action OnRoster;
        public Action OnShop;
        public Action OnBoxes;
        public Action OnHall;
        public Action OnMenu;

        private static readonly string[] KitSprites =
        {
            "hub_background", "economy_panel_empty", "level_panel_empty", "level_track_empty",
            "level_fill", "warning_blank", "nav_pink_surface", "nav_active_blank", "cta_blank",
            "back_blank",
        };

        protected override void Build()
        {
            if (KitArt.Has(KitSprites))
            {
                _kit = true;
                BuildKit();
                return;
            }

            BuildPlain();
        }

        // ---- UI-INT-01: the illustrated Hub -------------------------------------------------
        // Reference canvas 1280x720 (match height); every size below is in those units and was
        // measured from the approved mockup (1672x941, scale 0.765). Text is Unity's, always.

        private static readonly Color Gold = Palette.Hex("#C87A12");
        private static readonly Color Purple = Palette.Hex("#8A3FC0");
        private static readonly Color Rose = Palette.Hex("#D9366A");
        private static readonly Color Sand = Palette.Hex("#E8B04A");

        private void BuildKit()
        {
            KitArt.Backdrop(transform, "hub_background");

            if (UiArt.Has("ui020_logo_wordmark"))
            {
                var logo = UiArt.Picture(transform, "ui020_logo_wordmark", new Vector2(176f, 86f));
                UiKit.Anchor(logo.rectTransform, new Vector2(0f, 1f), new Vector2(12f, -9f));
            }

            var back = KitArt.Button(transform, "back_blank", Strings.Get("dialog.back"),
                new Vector2(150f, 64f), () => OnMenu?.Invoke(), 22, 12, KitArt.Brown,
                KitArt.Margins("back_blank", 86, 27, 90, 35));
            UiKit.Anchor(back.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(-16f, -14f));

            KitLedger();
            KitToday();
            KitNav();
            KitOpen();
        }

        private void KitLedger()
        {
            var panel = KitArt.Plate(transform, "economy_panel_empty", new Vector2(806f, 144f));
            panel.gameObject.name = "EconomyPanel";
            UiKit.Anchor(panel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -62f));

            _day = KitColumn(panel.rectTransform, 0.166f, "hub.day", KitArt.Brown);
            _money = KitColumn(panel.rectTransform, 0.389f, "hub.money", Gold);
            _points = KitColumn(panel.rectTransform, 0.614f, "hub.points", Purple);
            _costs = KitColumn(panel.rectTransform, 0.839f, "hub.fixedcosts", Rose);

            var level = KitArt.Plate(transform, "level_panel_empty", new Vector2(742f, 68f));
            level.gameObject.name = "LevelPanel";
            UiKit.Anchor(level.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -194f));

            _level = KitArt.Fitted(level.transform, "", 17, 11, KitArt.Brown,
                new Vector4(0.12f, 0.46f, 0.12f, 0.12f));

            var track = KitArt.Plate(level.transform, "level_track_empty", new Vector2(600f, 15f));
            UiKit.Anchor(track.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 12f));
            _fillTrack = track.rectTransform;

            var fill = KitArt.Plate(track.transform, "level_fill", new Vector2(600f, 11f));
            fill.gameObject.name = "Fill";
            fill.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            fill.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            fill.rectTransform.pivot = new Vector2(0f, 0.5f);
            fill.rectTransform.anchoredPosition = new Vector2(2f, 0f);
            _levelBar = fill;

            var warning = KitArt.Plate(transform, "warning_blank", new Vector2(700f, 62f));
            warning.gameObject.name = "WarningPlate";
            UiKit.Anchor(warning.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -238f));
            _warning = warning.rectTransform;

            _danger = KitArt.Fitted(warning.transform, "", 17, 10, KitArt.Cream,
                KitArt.Margins("warning_blank", 124, 18, 633, 34), outline: Palette.Hex("#7A1F3A"));
        }

        private Text KitColumn(RectTransform panel, float centre, string key, Color colour)
        {
            var column = new GameObject("Figure", typeof(RectTransform)).GetComponent<RectTransform>();
            column.SetParent(panel, false);
            column.anchorMin = new Vector2(centre - 0.105f, 0f);
            column.anchorMax = new Vector2(centre + 0.105f, 1f);
            column.offsetMin = Vector2.zero;
            column.offsetMax = Vector2.zero;

            KitArt.Fitted(column, Strings.Get(key), 17, 11, KitArt.Brown,
                new Vector4(0f, 0.54f, 0f, 0.31f));

            return KitArt.Fitted(column, "", 38, 18, colour, new Vector4(0f, 0.24f, 0f, 0.47f));
        }

        private void KitToday()
        {
            _chemistry = UiKit.Label(transform, "", 21, Sand, bold: true);
            UiKit.Anchor(_chemistry.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -302f));
            _chemistry.rectTransform.sizeDelta = new Vector2(760f, 30f);
            var drop = _chemistry.gameObject.AddComponent<Shadow>();
            drop.effectColor = new Color(0f, 0f, 0f, 0.5f);
            drop.effectDistance = new Vector2(1.5f, -2f);

            _today = UiKit.Row(transform, 14f);
            UiKit.Anchor(_today, new Vector2(0.5f, 1f), new Vector2(0f, -338f));
            _today.sizeDelta = new Vector2(1100f, 184f);
        }

        private void KitNav()
        {
            var row = UiKit.Row(transform, 10f);
            UiKit.Anchor(row, new Vector2(0.5f, 0f), new Vector2(0f, 134f));
            row.sizeDelta = new Vector2(1000f, 92f);

            var active = KitArt.Button(row, "nav_active_blank", "Equipe do Dia", new Vector2(228f, 85f),
                () => OnLineup?.Invoke(), 25, 13, KitArt.Brown,
                KitArt.Margins("nav_active_blank", 107, 33, 188, 40));
            active.name = "Nav_Lineup";

            Nav(row, "Maids", () => OnRoster?.Invoke(), "Nav_Roster");
            Nav(row, "Caixinhas", () => OnBoxes?.Invoke(), "Nav_Boxes");
            Nav(row, "Lojinha", () => OnShop?.Invoke(), "Nav_Shop");
            Nav(row, "Café", () => OnHall?.Invoke(), "Nav_Hall");
        }

        private static void Nav(Transform row, string text, Action click, string name)
        {
            var button = KitArt.Button(row, "nav_pink_surface", text, new Vector2(165f, 68f), click,
                23, 12, KitArt.Brown, new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
            button.name = name;
        }

        private void KitOpen()
        {
            _open = KitArt.Button(transform, "cta_blank", "Começar Expediente", new Vector2(560f, 126f),
                () => OnOpenDay?.Invoke(), 38, 20, Color.white,
                KitArt.Margins("cta_blank", 158, 59, 472, 60), outline: Palette.Hex("#245A30"));
            _open.name = "Cta_OpenDay";
            _openLabel = KitArt.LabelOf(_open);
            UiKit.Anchor(_open.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0f, 12f));
        }

        private void BuildPlain()
        {
            Scenery.Build(transform, withFloor: false);

            var logo = Logo.Monogram(transform, 64f);
            UiKit.Anchor(logo, new Vector2(0f, 1f), new Vector2(22f, -14f));

            var title = UiKit.TrackedLabel(transform, "NEKO MAID INC.", Typeface.Body,
                Palette.Cream, 4f);
            UiKit.Anchor(title, new Vector2(0f, 1f), new Vector2(98f, -26f));

            Scenery.PlankAt(transform, Strings.Get("dialog.back"), new Vector2(120f, 44f),
                new Vector2(1f, 1f), new Vector2(-26f, -16f), () => OnMenu?.Invoke(),
                Typeface.Small);

            Ledger();
            Today();
            Buttons();
        }

        private void Ledger()
        {
            var plaque = Scenery.Plaque(transform, new Vector2(1020f, 168f));
            // Below the wordmark and the "Voltar" plank, which the plaque used to cover and block.
            UiKit.Anchor((RectTransform)plaque.parent, new Vector2(0.5f, 1f),
                new Vector2(0f, -64f));

            var row = UiKit.Row(plaque, 26f, 16f);
            UiKit.Anchor(row, new Vector2(0.5f, 1f), new Vector2(0f, -10f));
            row.sizeDelta = new Vector2(980f, 72f);

            _day = Figure(row, "hub.day", Palette.Text);
            _money = Figure(row, "hub.money", Logo.Brass);
            _points = Figure(row, "hub.points", Palette.ForArchetype(Sim.Maids.Archetype.Idol));
            _costs = Figure(row, "hub.fixedcosts", Palette.Alarm);

            _levelBar = UiKit.Bar(plaque, new Vector2(900f, 12f), Logo.Brass, Palette.Wall);
            UiKit.Anchor((RectTransform)_levelBar.rectTransform.parent, new Vector2(0.5f, 0f),
                new Vector2(0f, 42f));

            _level = UiKit.Label(plaque, "", Typeface.Tiny, Palette.TextDim);
            _level.rectTransform.sizeDelta = new Vector2(900f, 18f);
            UiKit.Anchor(_level.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 22f));

            _danger = UiKit.Label(plaque, "", Typeface.Small, Palette.Alarm, bold: true);
            _danger.rectTransform.sizeDelta = new Vector2(960f, 20f);
            UiKit.Anchor(_danger.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 2f));
        }

        /// <summary>
        /// Today's side, at a glance, in the middle of the hub.
        /// </summary>
        /// <remarks>
        /// The hub's job is to answer "what do I do next", and nine times out of ten the
        /// answer is on this strip: somebody standing in the wrong position, or a chemistry
        /// score that says the squad does not fit. Without it the screen was a balance and
        /// five buttons with three hundred pixels of empty wall in between.
        /// </remarks>
        private void Today()
        {
            _today = UiKit.Row(transform, 10f);
            UiKit.Anchor(_today, new Vector2(0.5f, 1f), new Vector2(0f, -272f));
            _today.sizeDelta = new Vector2(1120f, 200f);

            _chemistry = UiKit.Label(transform, "", Typeface.Body, Logo.Brass, bold: true);
            UiKit.Anchor(_chemistry.rectTransform, new Vector2(0.5f, 1f),
                new Vector2(0f, -240f));
            _chemistry.rectTransform.sizeDelta = new Vector2(760f, 28f);
        }

        private void SyncToday()
        {
            for (var i = 0; i < _cards.Count; i++)
            {
                if (_cards[i] != null) Destroy(_cards[i]);
            }

            _cards.Clear();

            var profile = Session.Profile;

            foreach (var slot in profile.Lineup.All)
            {
                if (slot.Empty) continue;

                var maid = profile.MaidById(slot.MaidId);
                if (maid == null) continue;

                var card = MaidCard.Build(_today, maid, _kit ? 0.8f : 0.72f, showSalary: false,
                    playingAs: slot.Role, exact: true);
                _cards.Add(card.gameObject);
            }

            var chemistry = profile.Chemistry();
            _chemistry.text = Strings.Get("lineup.chemistry") + "  " + chemistry.Score +
                              " / 100   \u00b7   " + Sim.Team.Chemistry.Verdict(chemistry.Score) +
                              "   \u00b7   x" + chemistry.Multiplier.ToString("0.00");
        }

        private static Text Figure(Transform parent, string key, Color colour)
        {
            var column = UiKit.Column(parent, 0f, 0f);
            column.sizeDelta = new Vector2(218f, 64f);

            var caption = UiKit.Label(column, Strings.Get(key), Typeface.Tiny, Palette.TextDim);
            caption.rectTransform.sizeDelta = new Vector2(218f, 18f);

            var value = UiKit.Label(column, "", Typeface.Heading, colour, bold: true);
            value.rectTransform.sizeDelta = new Vector2(218f, 40f);
            return value;
        }

        private void Buttons()
        {
            var row = UiKit.Row(transform, 14f);
            UiKit.Anchor(row, new Vector2(0.5f, 0f), new Vector2(0f, 150f));
            row.sizeDelta = new Vector2(1100f, 92f);

            Scenery.Plank(row, Strings.Get("hub.lineup"), new Vector2(240f, 84f),
                () => OnLineup?.Invoke(), Typeface.Heading, Logo.Brass, exact: true)
                .GetComponentInChildren<Text>().color = Palette.Hex("#3A2A1A");

            Scenery.Plank(row, Strings.Get("hub.roster"), new Vector2(170f, 84f),
                () => OnRoster?.Invoke());
            Scenery.Plank(row, Strings.Get("hub.boxes"), new Vector2(170f, 84f),
                () => OnBoxes?.Invoke());
            Scenery.Plank(row, Strings.Get("hub.shop"), new Vector2(170f, 84f),
                () => OnShop?.Invoke());
            Scenery.Plank(row, Strings.Get("hub.hall"), new Vector2(170f, 84f),
                () => OnHall?.Invoke());

            UiKit.Anchor(row, new Vector2(0.5f, 0f), new Vector2(0f, 180f));

            _open = Scenery.Plank(transform, Strings.Get("hub.open"), new Vector2(420f, 76f),
                () => OnOpenDay?.Invoke(), Typeface.Title, Palette.Good);
            UiKit.Anchor(_open.GetComponent<RectTransform>(), new Vector2(0.5f, 0f),
                new Vector2(0f, 68f));
        }

        /// <summary>The level bar: an end-capped 9-sliced fill, never narrower than its caps.</summary>
        private void SetKitFill(float fraction)
        {
            var track = _fillTrack.rect.width - 4f;
            var cap = _levelBar.rectTransform.rect.height * 36f / 31f;
            _levelBar.gameObject.SetActive(fraction > 0f);
            _levelBar.rectTransform.sizeDelta = new Vector2(Mathf.Max(cap, track * fraction),
                _levelBar.rectTransform.sizeDelta.y);
        }

        public override void Refresh()
        {
            if (Session == null) return;

            var profile = Session.Profile;

            _day.text = profile.Day.ToString();
            _money.text = Strings.Money(profile.Wallet.Money);
            _points.text = profile.Wallet.Points.ToString();
            _costs.text = Strings.Money(Session.FixedCosts);

            var level = profile.CafeLevel;
            var floor = CafeLevelTable.ReputationForLevel(level);
            var ceiling = level >= CafeLevelTable.MaxLevel
                ? floor
                : CafeLevelTable.ReputationForLevel(level + 1);

            var span = Mathf.Max(1, ceiling - floor);
            var fraction = level >= CafeLevelTable.MaxLevel
                ? 1f
                : Mathf.Clamp01((profile.Wallet.Reputation - floor) / (float)span);

            if (_kit)
            {
                SetKitFill(fraction);
            }
            else
            {
                UiKit.SetBar(_levelBar, fraction, 900f);
            }

            _level.text = Strings.Get("hub.level") + " " + level + " · " +
                          (level >= CafeLevelTable.MaxLevel
                              ? "no topo"
                              : CafeLevelTable.ToNextLevel(profile.Wallet.Reputation) + " " +
                                Strings.Get("hub.tonext"));

            var why = Session.WhyNotReady();
            _open.interactable = why == null;

            _danger.text = why ?? (Session.InDanger ? Strings.Get("hub.danger") : "");

            if (_kit)
            {
                _warning.gameObject.SetActive(!string.IsNullOrEmpty(_danger.text));
                if (_openLabel != null)
                {
                    _openLabel.color = why == null ? Color.white : new Color(1f, 1f, 1f, 0.92f);
                }
            }
            else
            {
                _danger.color = why != null ? Logo.Brass : Palette.Alarm;
            }

            SyncToday();
        }
    }
}
