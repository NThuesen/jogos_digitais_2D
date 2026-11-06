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
            var tagY = Backdrop() ? -292f : -330f;
            var logo = WordmarkOrSign();
            Logo.Sway(logo, -0.6f, 0.6f);

            var tagline = UiKit.Label(transform, Strings.Get("game.subtitle"), Typeface.Body,
                Palette.WithAlpha(Palette.Cream, 0.9f));
            UiKit.Anchor(tagline.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, tagY));
            var tagShadow = tagline.gameObject.AddComponent<Shadow>();
            tagShadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
            tagShadow.effectDistance = new Vector2(1.5f, -2f);
            tagline.rectTransform.sizeDelta = new Vector2(760f, 30f);

            // The subtitle sits on the busiest part of the café: a soft dark plate keeps it readable.
            Plate(tagline.rectTransform, new Vector2(520f, 36f), 0.6f, tagY + 3f);

            Parade();
            Buttons();

            _records = UiKit.Label(transform, "", Typeface.Tiny,
                Palette.WithAlpha(Palette.Cream, 0.92f), TextAnchor.LowerLeft);
            UiKit.Anchor(_records.rectTransform, new Vector2(0f, 0f), new Vector2(26f, 14f));
            _records.rectTransform.sizeDelta = new Vector2(480f, 92f);

            var credits = UiKit.Label(transform, Strings.Get("credits.body"), Typeface.Tiny,
                Palette.WithAlpha(Palette.Cream, 0.9f), TextAnchor.LowerRight);
            UiKit.Anchor(credits.rectTransform, new Vector2(1f, 1f), new Vector2(-18f, -14f));
            credits.rectTransform.sizeDelta = new Vector2(330f, 92f);
            credits.alignment = TextAnchor.UpperRight;

            Plate(_records.rectTransform, new Vector2(500f, 96f), 0.55f, null);
            Plate(credits.rectTransform, new Vector2(350f, 78f), 0.55f, null);

            // Both corners sit on the illustrated café, so the small print gets a drop shadow.
            foreach (var small in new[] { _records, credits })
            {
                var drop = small.gameObject.AddComponent<Shadow>();
                drop.effectColor = new Color(0f, 0f, 0f, 0.75f);
                drop.effectDistance = new Vector2(1f, -1.5f);
            }
        }

        /// <summary>A soft dark plate directly behind a label, matching its anchor, pivot and position.</summary>
        private static void Plate(RectTransform label, Vector2 size, float alpha, float? y)
        {
            var plate = UiKit.Panel(label.parent, size, Palette.WithAlpha(Palette.Ink, alpha), 0.3f, exact: true);
            plate.GetComponent<Image>().raycastTarget = false;
            plate.anchorMin = label.anchorMin;
            plate.anchorMax = label.anchorMax;
            plate.pivot = label.pivot;
            plate.anchoredPosition = y.HasValue
                ? new Vector2(label.anchoredPosition.x, y.Value)
                : label.anchoredPosition + new Vector2(label.pivot.x < 0.5f ? -10f : 10f, label.pivot.y < 0.5f ? -6f : 6f);
            plate.SetSiblingIndex(label.GetSiblingIndex());
        }

        /// <summary>
        /// The approved illustrated café (UI-012), cover-fitted behind everything; false when it
        /// is missing and the procedural room is built instead. It is a menu backdrop only.
        /// </summary>
        private bool Backdrop()
        {
            var art = UiArt.Get("ui012_menu_background");
            if (art == null)
            {
                Scenery.Build(transform);
                return false;
            }

            var root = UiKit.Root("Scenery", transform);
            root.SetAsFirstSibling();

            var picture = new GameObject("Cafe", typeof(RectTransform)).AddComponent<Image>();
            picture.transform.SetParent(root, false);
            picture.sprite = art;
            picture.raycastTarget = false;
            var fit = picture.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fit.aspectRatio = art.rect.width / art.rect.height;

            // A warm scrim: the café is bright, and the cream text and name plates above it
            // are drawn by Unity and have to stay readable on every part of the picture.
            var scrim = UiKit.Panel(root, Vector2.one * 100f, Palette.WithAlpha(Palette.Ink, 0.2f), 0f);
            UiKit.Stretch(scrim);
            scrim.GetComponent<Image>().raycastTarget = false;
            UiArt.Shade(root, top: true, height: 230f, alpha: 0.88f);
            UiArt.Shade(root, top: false, height: 270f, alpha: 0.85f);
            return true;
        }

        /// <summary>The approved wordmark (UI-020) when present, the procedural sign otherwise.</summary>
        private RectTransform WordmarkOrSign()
        {
            if (!UiArt.Has("ui020_logo_wordmark"))
            {
                var sign = Logo.Build(transform, 0.92f);
                UiKit.Anchor(sign, new Vector2(0.5f, 1f), new Vector2(0f, -22f));
                return sign;
            }

            // 520u is the size the wordmark was approved at over the menu background.
            var mark = UiArt.Picture(transform, "ui020_logo_wordmark", new Vector2(520f, 253f));
            UiKit.Anchor(mark.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -20f));
            return mark.rectTransform;
        }

        /// <summary>The standing frame of the first released maid of this archetype.</summary>
        private static Sprite Authored(Archetype archetype)
        {
            foreach (var identity in MaidCatalog.Released)
            {
                if (identity.Archetype != archetype) continue;

                var entry = MaidAnimCatalog.ById(identity.Id);
                var frames = entry == null ? null : MaidAnimCatalog.Load(UiKit.Sprites, entry);
                if (frames != null && entry.idle_frame < frames.Length) return frames[entry.idle_frame];
            }

            return null;
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

                var column = UiKit.Column(row, 2f, 0f);
                column.sizeDelta = new Vector2(190f, 210f);

                var image = new GameObject(archetype.ToString(), typeof(RectTransform))
                    .AddComponent<Image>();
                image.transform.SetParent(column, false);
                image.rectTransform.sizeDelta = new Vector2(170f, 170f);
                image.preserveAspect = true;
                image.raycastTarget = false;
                image.sprite = Authored(archetype);

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
            column.sizeDelta = new Vector2(420f, 110f);

            // UI-004 compact was approved at heights 44 and 54 (the slice absorbs the width), so the
            // primary action uses 54 and the secondary 44 instead of the old 68/48 planks.
            var play = UiArt.StateButton(column, Strings.Get("menu.play"), new Vector2(300f, 54f),
                () => OnPlay?.Invoke(), "btn1", Typeface.Body);
            _continueLabel = play.GetComponentInChildren<Text>();

            UiArt.StateButton(column, Strings.Get("menu.newgame"), new Vector2(220f, 44f), () =>
                Root.Dialog(Strings.Get("menu.newgame"), Strings.Get("menu.newgameask"),
                    Strings.Get("dialog.yes"), () => OnNewGame?.Invoke(),
                    Strings.Get("dialog.no")), "btn2", Typeface.Small);
        }

        private GameSession _session;

        /// <summary>The Menu is built on first show, after the game binds its session: keep it and apply on Refresh.</summary>
        public void SetSession(GameSession session)
        {
            _session = session;
            Apply();
        }

        public override void Refresh() => Apply();

        private void Apply()
        {
            var session = _session;
            if (session == null || _records == null) return;

            var records = session.Profile.Records;

            _records.text =
                Strings.Get("menu.records") + "\n" +
                Strings.Get("menu.bestday") + ": " + records.BestDay + "   " +
                Strings.Get("menu.bestrevenue") + ": " + Strings.Money(records.BestRevenue) + "\n" +
                Strings.Get("menu.servedtotal") + ": " + records.TotalCustomersServed + "   " +
                Strings.Get("menu.legendaries") + ": " + records.LegendariesFound + "   " +
                Strings.Get("menu.lost") + ": " + records.BusinessesLost;

            _continueLabel.text = records.TotalDaysPlayed > 0 && !session.Bankrupt
                ? Strings.Get("menu.continue")
                : Strings.Get("menu.play");
        }
    }
}
