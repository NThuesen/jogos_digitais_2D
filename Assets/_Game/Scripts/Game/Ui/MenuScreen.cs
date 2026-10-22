using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Game.Feel;
using Neko.Game.View;
using Neko.Sim.Maids;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Game.Ui
{
    /// <summary>The title screen, with a maid standing on it.</summary>
    public sealed class MenuScreen : Screen
    {
        private Text _records;
        private Button _continueButton;

        public System.Action OnPlay;
        public System.Action OnNewGame;

        protected override void Build()
        {
            Backdrop(Palette.Cream);

            var column = UiKit.Column(transform, 14f, 40f);
            UiKit.Stretch(column);

            var title = UiKit.Label(column, Strings.Get("game.title"), Typeface.Title,
                Palette.Text, bold: true);
            title.rectTransform.sizeDelta = new Vector2(760f, 76f);

            var subtitle = UiKit.Label(column, Strings.Get("game.subtitle"), Typeface.Body,
                Palette.TextDim);
            subtitle.rectTransform.sizeDelta = new Vector2(760f, 34f);

            // Three of the nine, so the title screen says what the game is about before any
            // text does.
            var parade = UiKit.Row(column, 8f);
            parade.sizeDelta = new Vector2(760f, 150f);

            foreach (var archetype in new[] { Archetype.Genki, Archetype.Ojousama, Archetype.Mukuchi })
            {
                var maid = Maid.Create(0, "", archetype);
                var image = new GameObject(archetype.ToString(), typeof(RectTransform))
                    .AddComponent<Image>();
                image.transform.SetParent(parade, false);
                image.rectTransform.sizeDelta = new Vector2(150f, 150f);
                image.preserveAspect = true;
                image.sprite = UiKit.Sprites.Layered(MaidSpriteBuilder.Layers(maid, MaidPose.Idle),
                    MaidSpriteBuilder.CacheKey(maid, MaidPose.Idle), 128);

                Tween.Punch(image.rectTransform, 0.06f, 0.9f);
            }

            _continueButton = UiKit.Button(column, Strings.Get("menu.play"),
                new Vector2(330f, 62f), Palette.Focus, () => OnPlay?.Invoke(),
                Typeface.Heading);

            UiKit.Button(column, Strings.Get("menu.newgame"), new Vector2(260f, 52f),
                Palette.Wall, () => Root.Dialog(
                    Strings.Get("menu.newgame"),
                    Strings.Get("settings.wipeconfirm"),
                    Strings.Get("dialog.yes"), () => OnNewGame?.Invoke(),
                    Strings.Get("dialog.no")));

            _records = UiKit.Label(column, "", Typeface.Small, Palette.TextDim);
            _records.rectTransform.sizeDelta = new Vector2(760f, 80f);

            var credits = UiKit.Label(column, Strings.Get("credits.body"), Typeface.Tiny,
                Palette.TextDim);
            credits.rectTransform.sizeDelta = new Vector2(760f, 70f);
        }

        public void SetSession(GameSession session)
        {
            if (session == null || _records == null) return;

            var records = session.Profile.Records;
            _records.text =
                Strings.Get("menu.records") + " · " +
                Strings.Get("cafe.day") + " " + records.BestDay + " · " +
                records.TotalCustomersServed + " " + Strings.Get("results.served").ToLowerInvariant();

            _continueButton.GetComponentInChildren<Text>().text =
                records.TotalDaysPlayed > 0
                    ? Strings.Get("menu.continue")
                    : Strings.Get("menu.play");
        }
    }
}
