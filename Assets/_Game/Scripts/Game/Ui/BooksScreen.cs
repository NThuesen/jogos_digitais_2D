using System;
using System.Collections.Generic;
using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Game.Feel;
using Neko.Sim.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Game.Ui
{
    /// <summary>
    /// The dashboard: what came in, what went out, and whether the business is still here.
    /// </summary>
    /// <remarks>
    /// The screen the whole day is judged on, so nothing here is a score. Every line is money
    /// in the same currency, the costs are shown next to the takings rather than netted
    /// against them, and the only verdict is the one at the top — which is a sentence about
    /// the day, not a grade.
    /// </remarks>
    public sealed class BooksScreen : Screen
    {
        private readonly List<GameObject> _rows = new List<GameObject>();

        private Text _verdict;
        private Text _subtitle;
        private RectTransform _left;
        private RectTransform _right;
        private RectTransform _events;
        private Text _bottomLine;
        private Text _balance;

        public GameSession Session;
        public Action OnContinue;

        protected override void Build()
        {
            Scenery.Build(transform, withFloor: false);

            var plaque = Scenery.Plaque(transform, new Vector2(1100f, 600f));
            UiKit.Anchor((RectTransform)plaque.parent, new Vector2(0.5f, 1f),
                new Vector2(0f, -16f));

            _verdict = UiKit.Label(plaque, "", Typeface.Title, Palette.Text, bold: true);
            _verdict.rectTransform.sizeDelta = new Vector2(1000f, 74f);
            UiKit.Anchor(_verdict.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -20f));

            _subtitle = UiKit.Label(plaque, "", Typeface.Small, Palette.TextDim);
            _subtitle.rectTransform.sizeDelta = new Vector2(1000f, 24f);
            UiKit.Anchor(_subtitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -96f));

            _left = Column(plaque, "books.in", new Vector2(-268f, -132f));
            _right = Column(plaque, "books.out", new Vector2(268f, -132f));

            _events = UiKit.Row(plaque, 10f);
            UiKit.Anchor(_events, new Vector2(0.5f, 0f), new Vector2(0f, 128f));
            _events.sizeDelta = new Vector2(1060f, 72f);

            _bottomLine = UiKit.Label(plaque, "", Typeface.Heading, Palette.Text, bold: true);
            _bottomLine.rectTransform.sizeDelta = new Vector2(1000f, 40f);
            UiKit.Anchor(_bottomLine.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 72f));

            _balance = UiKit.Label(plaque, "", Typeface.Body, Palette.TextDim);
            _balance.rectTransform.sizeDelta = new Vector2(1000f, 28f);
            UiKit.Anchor(_balance.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 40f));

            Scenery.PlankAt(transform, Strings.Get("books.continue"), new Vector2(360f, 62f),
                    new Vector2(0.5f, 0f), new Vector2(0f, 34f), () => OnContinue?.Invoke(),
                    Typeface.Heading, Logo.Brass)
                .GetComponentInChildren<Text>().color = Palette.Hex("#3A2A1A");
        }

        private RectTransform Column(Transform parent, string key, Vector2 at)
        {
            var column = UiKit.Column(parent, 6f, 0f);
            UiKit.Anchor(column, new Vector2(0.5f, 1f), at);
            column.sizeDelta = new Vector2(460f, 190f);

            var header = UiKit.Label(column, Strings.Get(key), Typeface.Body, Logo.Brass,
                bold: true);
            header.rectTransform.sizeDelta = new Vector2(440f, 26f);

            return column;
        }

        public override void Refresh()
        {
            var ledger = Session?.LastLedger;
            if (ledger == null) return;

            for (var i = 0; i < _rows.Count; i++)
            {
                if (_rows[i] != null) Destroy(_rows[i]);
            }

            _rows.Clear();

            _verdict.text = ledger.Verdict();
            _verdict.color = ledger.Profit > 0 ? Palette.Good : Palette.Alarm;

            _subtitle.text = ledger.FailureReason ??
                             (Strings.Get("books.chemistry") + " " + ledger.ChemistryScore +
                              " / 100   ·   " + Strings.Get("books.satisfaction") + " " +
                              Mathf.RoundToInt(ledger.Satisfaction * 100f) + "%");

            foreach (var line in ledger.Incoming()) Line(_left, line);
            foreach (var line in ledger.Outgoing()) Line(_right, line);

            Line(_left, new LedgerLine(Strings.Get("books.revenue"), ledger.Revenue), true);
            Line(_right, new LedgerLine(Strings.Get("books.costs"), ledger.Costs, true), true);

            Tally("books.wrong", ledger.WrongOrders, ledger.WrongOrders > 0);
            Tally("books.burnt", ledger.Burnt, ledger.Burnt > 0);
            Tally("books.angry", ledger.LeftAngry, ledger.LeftAngry > 0);
            Tally("books.turned", ledger.TurnedAway, ledger.TurnedAway > 0);
            Tally("books.points", ledger.PointsEarned, false, Logo.Brass);

            foreach (var pair in ledger.LevelUps)
            {
                var maid = Session.Profile.MaidById(pair.Key);
                if (maid == null) continue;

                Tally(null, pair.Value, false, Palette.ForArchetype(maid.Archetype),
                    maid.Name + " " + Strings.Get("books.levelup"));
            }

            var profit = ledger.Profit;
            _bottomLine.text = Strings.Get(profit >= 0 ? "books.profit" : "books.loss") + "  " +
                               Strings.Signed(profit);
            _bottomLine.color = profit >= 0 ? Palette.Good : Palette.Alarm;

            _balance.text = Strings.Get("books.balance") + "  " +
                            Strings.Money(ledger.BalanceAfter) +
                            (ledger.Warning ? "   ·   " + Strings.Get("books.warning") : "");
            _balance.color = ledger.Warning ? Palette.Alarm : Palette.TextDim;

            Tween.Punch(_verdict.rectTransform, 0.16f, 0.4f);
        }

        private void Line(Transform parent, LedgerLine line, bool total = false)
        {
            var row = UiKit.Panel(parent, new Vector2(440f, total ? 34f : 28f),
                Palette.WithAlpha(total ? Logo.Brass : Palette.Wall, total ? 0.4f : 0.35f), 0.3f);
            _rows.Add(row.gameObject);

            var label = UiKit.Label(row, line.Label, total ? Typeface.Body : Typeface.Small,
                Palette.Text, TextAnchor.MiddleLeft, total);
            label.rectTransform.sizeDelta = new Vector2(260f, 24f);
            label.rectTransform.anchoredPosition = new Vector2(-80f, 0f);

            var value = UiKit.Label(row,
                (line.Outgoing ? "−" : "") + Strings.Money(line.Amount),
                total ? Typeface.Body : Typeface.Small,
                line.Outgoing ? Palette.Alarm : Palette.Good, TextAnchor.MiddleRight, true);
            value.rectTransform.sizeDelta = new Vector2(180f, 24f);
            value.rectTransform.anchoredPosition = new Vector2(120f, 0f);
        }

        private void Tally(string key, int value, bool bad, Color? tint = null, string text = null)
        {
            var chip = UiKit.Panel(_events, new Vector2(158f, 58f),
                Palette.WithAlpha(Palette.Wall, 0.4f), 0.25f);
            _rows.Add(chip.gameObject);

            var number = UiKit.Label(chip, value.ToString(), Typeface.Heading,
                tint ?? (bad ? Palette.Alarm : Palette.Text), bold: true);
            number.rectTransform.sizeDelta = new Vector2(152f, 32f);
            UiKit.Anchor(number.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -2f));

            var caption = UiKit.Label(chip, text ?? Strings.Get(key), Typeface.Tiny,
                Palette.TextDim);
            caption.rectTransform.sizeDelta = new Vector2(152f, 20f);
            UiKit.Anchor(caption.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 2f));
        }
    }

    /// <summary>The end. Said plainly, with the records kept and a way straight back in.</summary>
    public sealed class BustScreen : Screen
    {
        public Action OnRestart;

        protected override void Build()
        {
            Backdrop(Palette.WithAlpha(Palette.Ink, 0.86f));

            var plaque = Scenery.Plaque(transform, new Vector2(680f, 360f));
            UiKit.Anchor((RectTransform)plaque.parent, new Vector2(0.5f, 0.5f), Vector2.zero);

            var column = UiKit.Column(plaque, 16f, 26f);
            UiKit.Stretch(column);

            var title = UiKit.TrackedLabel(column, "FALÊNCIA", Typeface.Title, Palette.Alarm, 8f);
            title.gameObject.name = "Bust";

            var body = UiKit.Label(column, Strings.Get("bust.body"), Typeface.Body, Palette.Text);
            body.rectTransform.sizeDelta = new Vector2(600f, 100f);

            Scenery.Plank(column, Strings.Get("bust.restart"), new Vector2(340f, 60f),
                () => OnRestart?.Invoke(), Typeface.Heading, Logo.Brass);
        }
    }
}
