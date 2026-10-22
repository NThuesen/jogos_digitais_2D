using System;
using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Game.Feel;
using Neko.Game.View;
using Neko.Sim.Maids;
using Neko.Sim.Team;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Game.Ui
{
    /// <summary>
    /// The front door: a lit room, a sign hanging in it, and three maids waiting under it.
    /// </summary>
    /// <remarks>
    /// Deliberately not a list of buttons on a colour. This is the first thing anybody sees
    /// and the only chance the game has to say what it is before a single word is read — a
    /// café, run as a business, staffed by cats.
    /// </remarks>
    public sealed class MenuScreen : Screen
    {
        private Text _records;
        private Text _continueLabel;

        public Action OnPlay;
        public Action OnNewGame;

        protected override void Build()
        {
            Scenery.Build(transform);

            var logo = Logo.Build(transform, 0.92f);
            UiKit.Anchor(logo, new Vector2(0.5f, 1f), new Vector2(0f, -22f));
            Logo.Sway(logo);

            var tagline = UiKit.Label(transform, Strings.Get("game.subtitle"), Typeface.Body,
                Palette.WithAlpha(Palette.Cream, 0.78f));
            UiKit.Anchor(tagline.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -330f));
            tagline.rectTransform.sizeDelta = new Vector2(760f, 30f);

            Parade();
            Buttons();

            _records = UiKit.Label(transform, "", Typeface.Tiny,
                Palette.WithAlpha(Palette.Cream, 0.6f), TextAnchor.LowerLeft);
            UiKit.Anchor(_records.rectTransform, new Vector2(0f, 0f), new Vector2(26f, 14f));
            _records.rectTransform.sizeDelta = new Vector2(480f, 92f);

            var credits = UiKit.Label(transform, Strings.Get("credits.body"), Typeface.Tiny,
                Palette.WithAlpha(Palette.Cream, 0.45f), TextAnchor.LowerRight);
            UiKit.Anchor(credits.rectTransform, new Vector2(1f, 1f), new Vector2(-18f, -14f));
            credits.rectTransform.sizeDelta = new Vector2(330f, 92f);
            credits.alignment = TextAnchor.UpperRight;
        }

        /// <summary>
        /// Three archetypes standing on the floorboards under the sign.
        /// </summary>
        /// <remarks>
        /// One from each working position, so the title screen states the shape of the game
        /// before any text does: somebody greets, somebody serves, somebody cooks.
        /// </remarks>
        private void Parade()
        {
            var cast = new[]
            {
                (Archetype.Ojousama, Role.Reception),
                (Archetype.Genki, Role.Floor),
                (Archetype.Mukuchi, Role.Kitchen),
            };

            var row = UiKit.Row(transform, 16f);
            UiKit.Anchor(row, new Vector2(1f, 0f), new Vector2(-40f, 104f));
            row.sizeDelta = new Vector2(620f, 220f);

            for (var i = 0; i < cast.Length; i++)
            {
                var (archetype, role) = cast[i];
                var maid = Maid.Create(0, "", archetype);

                var column = UiKit.Column(row, 2f, 0f);
                column.sizeDelta = new Vector2(190f, 210f);

                var image = new GameObject(archetype.ToString(), typeof(RectTransform))
                    .AddComponent<Image>();
                image.transform.SetParent(column, false);
                image.rectTransform.sizeDelta = new Vector2(170f, 170f);
                image.preserveAspect = true;
                image.raycastTarget = false;
                image.sprite = UiKit.Sprites.Layered(
                    MaidSpriteBuilder.Layers(maid, MaidPose.Idle),
                    MaidSpriteBuilder.CacheKey(maid, MaidPose.Idle), 128);

                var plate = UiKit.Panel(column, new Vector2(132f, 26f),
                    Palette.WithAlpha(Logo.BoardDeep, 0.85f), 0.5f);

                var label = UiKit.Label(plate, RoleTable.For(role).DisplayName, Typeface.Tiny,
                    Logo.Brass, bold: true);
                UiKit.Stretch(label.rectTransform, 4f);

                // A slow tilt rather than a bob.
                //
                // The first version animated anchoredPosition, which the layout group also
                // owns — they fought, and the three maids ended up at three different heights
                // with their name plates left behind. Rotation is nobody else's business.
                Sway(image.rectTransform, i);
            }
        }

        private static void Sway(RectTransform rect, int index)
        {
            var from = index % 2 == 0 ? -3.5f : 3.5f;
            Swing(rect, from, -from, 1.6f + 0.25f * index);
        }

        private static void Swing(RectTransform rect, float from, float to, float seconds)
        {
            if (rect == null) return;

            Tween.Value(from, to, seconds,
                    angle =>
                    {
                        if (rect != null) rect.localRotation = Quaternion.Euler(0f, 0f, angle);
                    },
                    Ease.InOutQuad)
                .Then(() => Swing(rect, to, from, seconds));
        }

        private void Buttons()
        {
            var column = UiKit.Column(transform, 12f, 0f);
            UiKit.Anchor(column, new Vector2(0f, 0f), new Vector2(60f, 118f));
            column.sizeDelta = new Vector2(420f, 136f);

            var play = Scenery.Plank(column, Strings.Get("menu.play"), new Vector2(380f, 68f),
                () => OnPlay?.Invoke(), Typeface.Heading, Logo.Brass);
            _continueLabel = play.GetComponentInChildren<Text>();
            _continueLabel.color = Palette.Hex("#4A3220");

            Scenery.Plank(column, Strings.Get("menu.newgame"), new Vector2(300f, 48f), () =>
                Root.Dialog(Strings.Get("menu.newgame"), Strings.Get("menu.newgameask"),
                    Strings.Get("dialog.yes"), () => OnNewGame?.Invoke(),
                    Strings.Get("dialog.no")), Typeface.Small);
        }

        public void SetSession(GameSession session)
        {
            if (session == null || _records == null) return;

            var records = session.Profile.Records;

            _records.text =
                Strings.Get("menu.records") + "\\n" +
                Strings.Get("menu.bestday") + ": " + records.BestDay + "   " +
                Strings.Get("menu.bestrevenue") + ": " + Strings.Money(records.BestRevenue) + "\\n" +
                Strings.Get("menu.servedtotal") + ": " + records.TotalCustomersServed + "   " +
                Strings.Get("menu.legendaries") + ": " + records.LegendariesFound + "   " +
                Strings.Get("menu.lost") + ": " + records.BusinessesLost;

            _continueLabel.text = records.TotalDaysPlayed > 0 && !session.Bankrupt
                ? Strings.Get("menu.continue")
                : Strings.Get("menu.play");
        }
    }
}
