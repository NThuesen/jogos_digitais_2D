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

        public GameSession Session;
        public Action OnOpenDay;
        public Action OnLineup;
        public Action OnRoster;
        public Action OnShop;
        public Action OnBoxes;
        public Action OnHall;
        public Action OnMenu;

        protected override void Build()
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
            UiKit.Anchor((RectTransform)plaque.parent, new Vector2(0.5f, 1f),
                new Vector2(0f, -16f));

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
            UiKit.Anchor(_today, new Vector2(0.5f, 1f), new Vector2(0f, -240f));
            _today.sizeDelta = new Vector2(1120f, 200f);

            _chemistry = UiKit.Label(transform, "", Typeface.Body, Logo.Brass, bold: true);
            UiKit.Anchor(_chemistry.rectTransform, new Vector2(0.5f, 1f),
                new Vector2(0f, -198f));
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

                var card = MaidCard.Build(_today, maid, 0.72f, showSalary: false,
                    playingAs: slot.Role);
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

            Scenery.Plank(row, Strings.Get("hub.lineup"), new Vector2(200f, 84f),
                () => OnLineup?.Invoke(), Typeface.Heading, Logo.Brass)
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
            UiKit.SetBar(_levelBar, (profile.Wallet.Reputation - floor) / (float)span, 900f);

            _level.text = Strings.Get("hub.level") + " " + level + " · " +
                          (level >= CafeLevelTable.MaxLevel
                              ? "no topo"
                              : CafeLevelTable.ToNextLevel(profile.Wallet.Reputation) + " " +
                                Strings.Get("hub.tonext"));

            var why = Session.WhyNotReady();
            _open.interactable = why == null;

            _danger.text = why ?? (Session.InDanger ? Strings.Get("hub.danger") : "");
            _danger.color = why != null ? Logo.Brass : Palette.Alarm;

            SyncToday();
        }
    }
}
