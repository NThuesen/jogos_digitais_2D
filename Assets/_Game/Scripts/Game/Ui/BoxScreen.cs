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

        protected override void Build()
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
                           Strings.Get("hub.points").ToLowerInvariant();

            foreach (var box in Lootbox.All) BuildBox(box);
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

                var card = MaidCard.Build(column, entry.Maid);
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
