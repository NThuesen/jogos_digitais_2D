using System;
using System.Collections.Generic;
using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Game.Feel;
using Neko.Sim.Economy;
using Neko.Sim.Team;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Game.Ui
{
    /// <summary>
    /// The boxes, and the moment one opens.
    /// </summary>
    /// <remarks>
    /// The odds are printed on every box, in the same size as the price. A pack-opening screen
    /// that hides its numbers is doing something to the player rather than for them, and this
    /// one is bought with fichas earned by playing well — so there is no reason to be coy.
    /// </remarks>
    public sealed class BoxScreen : Screen
    {
        private readonly List<GameObject> _built = new List<GameObject>();

        private RectTransform _shelf;
        private RectTransform _reveal;
        private Text _points;
        private BoxResult _opened;

        public GameSession Session;
        public Action OnBack;

        private static readonly string[] KitSprites =
        {
            "caixinhas_background", "chest_simple", "chest_silver", "chest_gold",
            "box_simple_panel_empty", "box_silver_panel_empty", "box_gold_panel_empty",
            "box_name_empty", "odds_panel_empty", "title_blank", "box_button_active_blank",
            "box_button_brown_blank", "back_blank", "paw_gold", "paw_cream",
        };

        private bool _kit;

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

        // ---- UI-INT-01: the illustrated Caixinhas ------------------------------------------
        // Sizes in the 1280x720 reference canvas, measured from the approved mockup
        // (scale 0.765). Names, chances, prices and the token counter all come from the real
        // BoxDef / Wallet and are Unity Text over blank plates; none is in a texture.

        private static readonly Color Purple = Palette.Hex("#7A3AA8");

        private void BuildKit()
        {
            KitArt.Backdrop(transform, "caixinhas_background");

            if (UiArt.Has("ui020_logo_wordmark"))
            {
                var logo = UiArt.Picture(transform, "ui020_logo_wordmark", new Vector2(176f, 86f));
                UiKit.Anchor(logo.rectTransform, new Vector2(0f, 1f), new Vector2(12f, -9f));
            }

            var title = KitArt.Plate(transform, "title_blank", new Vector2(360f, 103f));
            UiKit.Anchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -36f));
            title.gameObject.name = "TitlePlate";
            KitArt.Fitted(title.transform, "Caixinhas", 38, 18, KitArt.Brown,
                KitArt.Margins("title_blank", 148, 73, 292, 60));

            var back = KitArt.Button(transform, "back_blank", Strings.Get("dialog.back"),
                new Vector2(150f, 64f), () => OnBack?.Invoke(), 22, 12, KitArt.Brown,
                KitArt.Margins("back_blank", 86, 27, 90, 35));
            UiKit.Anchor(back.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(-16f, -14f));

            // The token counter: a plate, a coin made of a purple disc and a cream paw, and the number.
            var pill = KitArt.Plate(transform, "box_name_empty", new Vector2(226f, 52f));
            pill.gameObject.name = "TokenPlate";
            UiKit.Anchor(pill.rectTransform, new Vector2(1f, 1f), new Vector2(-176f, -66f));

            var coin = UiKit.Panel(pill.transform, new Vector2(34f, 34f), Palette.Hex("#A24BD0"), 0.5f);
            UiKit.Anchor(coin, new Vector2(0f, 0.5f), new Vector2(12f, 0f));
            coin.GetComponent<Image>().raycastTarget = false;
            var paw = KitArt.Picture(coin, "paw_cream", 22f);
            UiKit.Anchor(paw.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero);

            _points = KitArt.Fitted(pill.transform, "", 24, 12, Purple, new Vector4(0.24f, 0.08f, 0.12f, 0.08f),
                TextAnchor.MiddleCenter);
            _points.gameObject.name = "Tokens";

            _shelf = UiKit.Row(transform, 24f);
            UiKit.Anchor(_shelf, new Vector2(0.5f, 1f), new Vector2(0f, -150f));
            _shelf.sizeDelta = new Vector2(960f, 500f);

            _reveal = UiKit.Root("Reveal", transform);
            _reveal.gameObject.SetActive(false);
        }

        private void BuildKitBox(BoxDef box)
        {
            var kind = box.Floor == Rarity.Epic ? "gold" : box.Floor == Rarity.Rare ? "silver" : "simple";
            var affordable = Session.Profile.Wallet.CanAffordPoints(box.PricePoints);

            var card = KitArt.Plate(_shelf, "box_" + kind + "_panel_empty", new Vector2(285f, 490f));
            card.gameObject.name = "BoxCard_" + box.Id;
            _built.Add(card.gameObject);

            var chestSprite = KitArt.Get("chest_" + kind);
            var chest = KitArt.Plate(card.transform, "chest_" + kind,
                new Vector2(166f, 166f * chestSprite.rect.height / chestSprite.rect.width));
            UiKit.Anchor(chest.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -62f));
            chest.gameObject.name = "Chest";

            var plate = KitArt.Plate(card.transform, "box_name_empty", new Vector2(270f, 64f));
            UiKit.Anchor(plate.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -190f));
            plate.gameObject.name = "NamePlate";
            KitArt.Fitted(plate.transform, box.DisplayName, 22, 13, KitArt.Brown,
                KitArt.Margins("box_name_empty", 67, 21, 235, 43));

            var lines = KitArt.Fitted(card.transform, Describe(box), 16, 10, KitArt.Brown * 0.9f,
                new Vector4(0.06f, 0.435f, 0.06f, 0.525f));
            lines.fontStyle = FontStyle.Normal;
            lines.gameObject.name = "Tagline";

            var odds = KitArt.Plate(card.transform, "odds_panel_empty", new Vector2(240f, 112f));
            UiKit.Anchor(odds.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -283f));
            odds.gameObject.name = "OddsPanel";
            BuildOdds(odds.rectTransform, box);

            var button = KitArt.Button(card.transform,
                affordable ? "box_button_active_blank" : "box_button_brown_blank",
                Strings.Get("box.open") + " " + box.PricePoints, new Vector2(250f, 77f),
                () => Open(box), 26, 12, affordable ? KitArt.Brown : KitArt.Cream,
                KitArt.Margins("box_button_active_blank", 124, 32, 169, 43));
            UiKit.Anchor(button.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0f, 18f));
            button.name = "Open_" + box.Id;
            button.interactable = affordable;
        }

        /// <summary>One line per rarity the box can give, in the real percentages.</summary>
        private static void BuildOdds(RectTransform panel, BoxDef box)
        {
            var odds = Lootbox.Odds(box);
            var shown = new List<int>();
            for (var i = 0; i < odds.Count; i++)
            {
                if (odds[i] > 0f) shown.Add(i);
            }

            const float row = 24f;

            for (var n = 0; n < shown.Count; n++)
            {
                var rarity = RarityTable.All[shown[n]];
                var y = shown.Count * row * 0.5f - row * 0.5f - n * row;

                var icon = KitArt.Picture(panel, "paw_gold", 15f);
                UiKit.Anchor(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(46f, y));
                icon.rectTransform.pivot = new Vector2(0f, 0.5f);

                var text = UiKit.Label(panel, Percent(odds[shown[n]]) + " " + rarity.DisplayName, 19,
                    KitArt.Brown, TextAnchor.MiddleLeft, bold: true);
                text.rectTransform.sizeDelta = new Vector2(150f, row);
                text.horizontalOverflow = HorizontalWrapMode.Overflow;
                UiKit.Anchor(text.rectTransform, new Vector2(0f, 0.5f), new Vector2(68f, y));
                text.rectTransform.pivot = new Vector2(0f, 0.5f);
            }
        }

        private static string Percent(float value)
        {
            var rounded = Mathf.Round(value * 10f) / 10f;
            return rounded.ToString("0.#", new System.Globalization.CultureInfo("pt-BR")) + "%";
        }

        /// <summary>What the box gives, from the data: how many cards and the worst one.</summary>
        private static string Describe(BoxDef box)
        {
            var cards = box.Cards == 1 ? "1 carta" : box.Cards + " cartas";
            if (box.Floor == Rarity.Common) return cards;

            return cards + " · mínimo " + RarityTable.All[(int)box.Floor].DisplayName;
        }

        private void BuildPlain()
        {
            Scenery.Build(transform, withFloor: false);

            var title = UiKit.TrackedLabel(transform, "CAIXAS", Typeface.Heading, Palette.Cream, 6f);
            UiKit.Anchor(title, new Vector2(0f, 1f), new Vector2(26f, -18f));

            _points = UiKit.Label(transform, "", Typeface.Heading, Logo.Brass,
                TextAnchor.MiddleRight, bold: true);
            UiKit.Anchor(_points.rectTransform, new Vector2(1f, 1f), new Vector2(-170f, -26f));
            _points.rectTransform.sizeDelta = new Vector2(260f, 40f);

            Scenery.PlankAt(transform, Strings.Get("dialog.back"), new Vector2(120f, 44f),
                new Vector2(1f, 1f), new Vector2(-26f, -18f), () => OnBack?.Invoke(),
                Typeface.Small);

            _shelf = UiKit.Row(transform, 20f);
            UiKit.Anchor(_shelf, new Vector2(0.5f, 0.5f), new Vector2(0f, -10f));
            _shelf.sizeDelta = new Vector2(1100f, 380f);

            _reveal = UiKit.Root("Reveal", transform);
            _reveal.gameObject.SetActive(false);
        }

        public override void Refresh()
        {
            if (Session == null) return;

            Clear();
            _points.text = Session.Profile.Wallet.Points + " " +
                           (_kit ? Strings.Get("hub.points") : Strings.Get("hub.points").ToLowerInvariant());

            foreach (var box in Lootbox.All)
            {
                if (_kit) BuildKitBox(box);
                else BuildBox(box);
            }
        }

        private void BuildBox(BoxDef box)
        {
            var column = UiKit.Column(_shelf, 8f, 0f);
            column.sizeDelta = new Vector2(330f, 370f);
            _built.Add(column.gameObject);

            var lid = Palette.Hex(box.Floor == Rarity.Epic
                ? "#C9B04A"
                : box.Floor == Rarity.Rare
                    ? "#9FB4C9"
                    : "#A89078");

            var crate = UiKit.Panel(column, new Vector2(300f, 170f), lid, 0.2f);

            var band = UiKit.Panel(crate, new Vector2(300f, 34f),
                Palette.WithAlpha(Logo.BoardDeep, 0.85f), 0.2f);
            UiKit.Anchor(band, new Vector2(0.5f, 0.5f), Vector2.zero);

            var mark = Logo.Monogram(crate, 72f);
            UiKit.Anchor(mark, new Vector2(0.5f, 0.5f), new Vector2(0f, 6f));

            var name = UiKit.Label(column, box.DisplayName, Typeface.Body, Palette.Cream,
                bold: true);
            name.rectTransform.sizeDelta = new Vector2(300f, 26f);

            var odds = UiKit.Label(column, box.Description, Typeface.Tiny,
                Palette.WithAlpha(Palette.Cream, 0.8f));
            odds.rectTransform.sizeDelta = new Vector2(300f, 56f);

            var affordable = Session.Profile.Wallet.CanAffordPoints(box.PricePoints);

            var button = Scenery.Plank(column,
                Strings.Get("box.open") + "  " + box.PricePoints,
                new Vector2(260f, 56f), () => Open(box), Typeface.Body,
                affordable ? Logo.Brass : Logo.Board);

            button.interactable = affordable;
            if (affordable) button.GetComponentInChildren<Text>().color = Palette.Hex("#3A2A1A");
        }

        private void Open(BoxDef box)
        {
            var result = Lootbox.Open(Session.Profile, box.Id, Session.Rng);

            if (!result.Ok)
            {
                Root.Dialog(Strings.Get("box.title"), result.Refusal);
                return;
            }

            Session.Touch();
            _opened = result;
            ShowReveal();
        }

        /// <summary>
        /// The cards, dealt one at a time.
        /// </summary>
        /// <remarks>
        /// Staggered rather than appearing together, because the whole point of a box is the
        /// second between the lid and the card — and because a Legendary landing last is worth
        /// far more than three cards arriving at once.
        /// </remarks>
        private void ShowReveal()
        {
            foreach (Transform child in _reveal) Destroy(child.gameObject);

            _reveal.gameObject.SetActive(true);

            var shade = UiKit.Panel(_reveal, Vector2.one * 100f,
                Palette.WithAlpha(Palette.Ink, 0.8f), 0f);
            UiKit.Stretch(shade);
            shade.GetComponent<Image>().raycastTarget = true;

            var heading = UiKit.TrackedLabel(_reveal, "VEIO NA CAIXA", Typeface.Heading,
                Palette.Cream, 6f);
            UiKit.Anchor(heading, new Vector2(0.5f, 1f), new Vector2(0f, -70f));

            var row = UiKit.Row(_reveal, 22f);
            UiKit.Anchor(row, new Vector2(0.5f, 0.5f), new Vector2(0f, 10f));
            row.sizeDelta = new Vector2(1000f, 300f);

            for (var i = 0; i < _opened.Cards.Count; i++)
            {
                var entry = _opened.Cards[i];
                var column = UiKit.Column(row, 4f, 0f);
                column.sizeDelta = new Vector2(MaidCard.Width + 12f, 280f);

                var card = MaidCard.Build(column, entry.Maid, exact: true);
                card.localScale = Vector3.zero;

                if (entry.Duplicate)
                {
                    var note = UiKit.Label(column,
                        Strings.Get("box.duplicate") + " +" + entry.PointsBack,
                        Typeface.Tiny, Logo.Brass);
                    note.rectTransform.sizeDelta = new Vector2(MaidCard.Width, 34f);
                }

                var delay = 0.18f * i;
                Tween.Value(0f, 1f, 0.2f + delay, _ => { })
                    .Then(() => Tween.Scale(card, Vector3.zero, Vector3.one, 0.42f, Ease.OutBack));
            }

            var buttons = UiKit.Row(_reveal, 12f);
            UiKit.Anchor(buttons, new Vector2(0.5f, 0f), new Vector2(0f, 60f));
            buttons.sizeDelta = new Vector2(600f, 60f);

            Scenery.Plank(buttons, Strings.Get("box.done"), new Vector2(240f, 56f), () =>
            {
                _reveal.gameObject.SetActive(false);
                Refresh();
            }, Typeface.Body, Logo.Brass);
        }

        private void Clear()
        {
            for (var i = 0; i < _built.Count; i++)
            {
                if (_built[i] != null) Destroy(_built[i]);
            }

            _built.Clear();
        }
    }
}
