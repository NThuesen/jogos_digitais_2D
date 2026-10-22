using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Sim.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Game.Ui
{
    /// <summary>
    /// The hub between days: where the money goes before it goes back to work.
    /// </summary>
    public sealed class CafeScreen : Screen
    {
        private Text _header;
        private Text _quota;
        private Image _levelBar;
        private Text _levelText;

        public GameSession Session;
        public System.Action OnOpenDay;
        public System.Action OnRoster;
        public System.Action OnShop;
        public System.Action OnEditor;
        public System.Action OnMenu;

        protected override void Build()
        {
            var bar = UiKit.Panel(transform, new Vector2(1240f, 86f),
                Palette.WithAlpha(Palette.Panel, 0.95f));
            UiKit.Anchor(bar, new Vector2(0.5f, 1f), new Vector2(0f, -10f));

            var column = UiKit.Column(bar, 2f, 12f);
            UiKit.Stretch(column);

            _header = UiKit.Label(column, "", Typeface.Body, Palette.Text, bold: true);
            _header.rectTransform.sizeDelta = new Vector2(1180f, 30f);

            _levelBar = UiKit.Bar(column, new Vector2(1180f, 12f), Palette.Focus, Palette.Wall);
            _levelText = UiKit.Label(column, "", Typeface.Tiny, Palette.TextDim);
            _levelText.rectTransform.sizeDelta = new Vector2(1180f, 20f);

            var buttons = UiKit.Row(transform, 12f);
            UiKit.Anchor(buttons, new Vector2(0.5f, 0f), new Vector2(0f, 18f));
            buttons.sizeDelta = new Vector2(1200f, 72f);

            UiKit.Button(buttons, Strings.Get("cafe.roster"), new Vector2(190f, 60f),
                Palette.Wall, () => OnRoster?.Invoke());
            UiKit.Button(buttons, Strings.Get("cafe.shop"), new Vector2(190f, 60f),
                Palette.Wall, () => OnShop?.Invoke());
            UiKit.Button(buttons, Strings.Get("cafe.editor"), new Vector2(190f, 60f),
                Palette.Wall, () => OnEditor?.Invoke());
            UiKit.Button(buttons, Strings.Get("cafe.openday"), new Vector2(280f, 60f),
                Palette.Focus, () => OnOpenDay?.Invoke(), Typeface.Heading);

            var back = UiKit.Button(transform, "<", new Vector2(56f, 48f), Palette.Wall,
                () => OnMenu?.Invoke());
            UiKit.Anchor(back.GetComponent<RectTransform>(), new Vector2(0f, 1f),
                new Vector2(14f, -108f));

            _quota = UiKit.Label(transform, "", Typeface.Body, Palette.Text, bold: true);
            UiKit.Anchor(_quota.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 100f));
            _quota.rectTransform.sizeDelta = new Vector2(900f, 34f);
        }

        public override void Refresh()
        {
            if (Session == null) return;

            var profile = Session.Profile;

            _header.text =
                Strings.Get("cafe.day") + " " + profile.Day + "    " +
                Strings.Get("cafe.money") + " " + profile.Wallet.Money + "    " +
                Strings.Get("cafe.reputation") + " " + profile.Wallet.Reputation + "    " +
                Strings.Get("cafe.level") + " " + profile.CafeLevel;

            var level = profile.CafeLevel;
            var floor = CafeLevelTable.ReputationForLevel(level);
            var ceiling = level >= CafeLevelTable.MaxLevel
                ? floor
                : CafeLevelTable.ReputationForLevel(level + 1);

            var span = Mathf.Max(1, ceiling - floor);
            UiKit.SetBar(_levelBar, (profile.Wallet.Reputation - floor) / (float)span, 1180f);

            _levelText.text = level >= CafeLevelTable.MaxLevel
                ? Strings.Get("cafe.level") + " " + level
                : CafeLevelTable.ToNextLevel(profile.Wallet.Reputation) + " " +
                  Strings.Get("cafe.tonext");

            _quota.text = Strings.Get("cafe.quota") + ": " + DayPlan.QuotaFor(profile.QuotaStep);
        }
    }
}
