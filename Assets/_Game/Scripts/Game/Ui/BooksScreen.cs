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
    ///
    /// UI-INT-06: drawn from the results kit — a café after hours with a cream-and-gold report on it.
    /// Titles, labels, signs, currency and numbers are Unity Text on blank plates and every one of them is
    /// read from <c>Session.LastLedger</c>; nothing is computed here. The indicator band is adaptive (the
    /// five indicators plus one chip per level-up, in as many rows as needed, scrolling past two). If a kit
    /// sprite is missing the earlier procedural report is built instead, unchanged.
    /// </remarks>
    public sealed class BooksScreen : Screen
    {
        // Mockup (1672x941) pixels to the 1280x720 stage; every cut-out sprite carries 4 px of padding.
        private const float K = 1280f / 1672f;
        private const float MockupWidth = 1672f;
        private const float MockupHeight = 941f;

        // The report's own colours (the kit's suggestion) on its cream plates.
        private static readonly Color Gain = new Color32(0x25, 0x80, 0x45, 0xFF);
        private static readonly Color Loss = new Color32(0xDC, 0x31, 0x57, 0xFF);

        private static readonly string[] KitSprites =
        {
            "results_restaurant_background", "results_report_empty", "results_column_empty", "results_header_empty",
            "results_total_empty", "results_tally_empty", "results_financial_highlight_empty",
            "results_continue_empty", "results_row_empty", "results_separator",
        };

        /// <summary>For tests: build the earlier procedural report even though the kit is present.</summary>
        public static bool ForcePlain;

        private readonly List<GameObject> _rows = new List<GameObject>();

        private Text _verdict;
        private Text _subtitle;
        private RectTransform _left;
        private RectTransform _right;
        private RectTransform _events;
        private Text _bottomLine;
        private Text _balance;

        // The kit layout.
        private bool _kit;
        private ContainStage _stage;
        private RectTransform _leftView;
        private RectTransform _rightView;
        private RectTransform _tallyView;
        private Text _revenueLabel;
        private Text _revenueValue;
        private Text _costsLabel;
        private Text _costsValue;

        private const float TallyWidth = 148f;
        private const float TallyHeight = 54f;
        private const float TallyGap = 6f;

        public GameSession Session;
        public Action OnContinue;

        /// <summary>True when the report was built from the kit.</summary>
        public bool IsKit => _kit;

        protected override void Build()
        {
            if (!ForcePlain && KitArt.Has(KitSprites))
            {
                _kit = true;
                BuildKit();
                return;
            }

            BuildPlain();
        }

        // ========================================================================================
        // The kit layout
        // ========================================================================================

        private void BuildKit()
        {
            KitArt.Backdrop(transform, "results_restaurant_background");
            var stage = _stage = ContainStage.Make(transform, MockupWidth, MockupHeight, K, 4f);
            var root = stage.Rect;

            stage.At(stage.Scenery(), "results_report_empty", 222f, 0f, 1451f, 865f, "Report");

            // The verdict is the line a player reads first; whatever the day was, it is said here.
            _verdict = KitArt.Fitted(stage.Slot(root, "Verdict", 600f, 100f, 1073f, 196f), "", 58, 24, Gain,
                Vector4.zero);
            _verdict.gameObject.name = "VerdictText";

            // Chemistry and satisfaction, or why the business failed: wraps to a second line when it is long.
            _subtitle = KitArt.Fitted(stage.Slot(root, "Subtitle", 440f, 206f, 1233f, 262f), "", 22, 11,
                KitArt.Brown, Vector4.zero);
            _subtitle.gameObject.name = "SubtitleText";
            _subtitle.fontStyle = FontStyle.Normal;

            // Two columns, mirrored about the middle of the report.
            const float mirror = 2f * 836.5f;
            _leftView = BuildColumn(stage, root, "books.in", 288f, 398f, 710f, 309f, 803f,
                out _revenueLabel, out _revenueValue);
            _rightView = BuildColumn(stage, root, "books.out", mirror - 820f, mirror - 710f, mirror - 398f,
                mirror - 803f, mirror - 309f, out _costsLabel, out _costsValue);

            // Indicators and level-ups: as many as the day produced.
            _tallyView = stage.Slot(root, "Indicators", 300f, 496f, 1373f, 636f);
            _tallyView.gameObject.AddComponent<RectMask2D>();

            // The result of the day.
            stage.At(root, "results_financial_highlight_empty", 434f, 641f, 1238f, 801f, "Result");
            _bottomLine = KitArt.Fitted(stage.Slot(root, "Profit", 560f, 690f, 1112f, 742f), "", 40, 20, Gain,
                Vector4.zero);
            _bottomLine.gameObject.name = "ProfitText";
            _balance = KitArt.Fitted(stage.Slot(root, "Balance", 520f, 744f, 1152f, 792f), "", 22, 11,
                KitArt.Brown, Vector4.zero);
            _balance.gameObject.name = "BalanceText";
            _balance.fontStyle = FontStyle.Normal;

            // Back to the company.
            var plate = stage.At(root, "results_continue_empty", 571f, 819f, 1101f, 917f, "Continue");
            plate.raycastTarget = true;
            stage.TrimHitArea(plate);
            var button = plate.gameObject.AddComponent<Button>();
            button.targetGraphic = plate;
            button.transition = Selectable.Transition.ColorTint;
            var colours = button.colors;
            colours.normalColor = Color.white;
            colours.highlightedColor = new Color(1.08f, 1.06f, 1.04f, 1f);
            colours.pressedColor = new Color(0.82f, 0.78f, 0.74f, 1f);
            colours.selectedColor = Color.white;
            colours.fadeDuration = 0.07f;
            button.colors = colours;
            button.onClick.AddListener(() =>
            {
                Tween.Punch(plate.rectTransform, 0.06f, 0.14f);
                OnContinue?.Invoke();
            });

            var label = KitArt.Fitted(plate.transform, Strings.Get("books.continue"), 34, 16, KitArt.Brown,
                stage.Region(571f, 819f, 1101f, 917f, 660f, 836f, 1012f, 900f));
            label.gameObject.name = "Label";
        }

        /// <summary>One column of the report: the panel, its header, the rows area and its total.</summary>
        private RectTransform BuildColumn(ContainStage stage, RectTransform root, string key, float x0, float h0,
            float h1, float t0, float t1, out Text totalLabel, out Text totalValue)
        {
            stage.At(root, "results_column_empty", x0, 278f, x0 + 532f, 490f, "Column_" + key);

            var header = stage.At(root, "results_header_empty", h0, 262f, h1, 309f, "Header_" + key);
            var head = KitArt.Fitted(header.transform, Strings.Get(key), 28, 14, KitArt.Brown,
                stage.Region(h0, 262f, h1, 309f, h0 + 40f, 266f, h1 - 40f, 305f));
            head.gameObject.name = "HeaderText";

            // The rows scroll inside their own area if a day ever has more lines than fit; the total never moves.
            var view = stage.Slot(root, "Rows_" + key, x0 + 32f, 312f, x0 + 507f, 419f);
            view.gameObject.AddComponent<RectMask2D>();

            var total = stage.At(root, "results_total_empty", t0, 419f, t1, 468f, "Total_" + key);
            total.pixelsPerUnitMultiplier = 1f / K;
            totalLabel = KitArt.Fitted(total.transform, "", 28, 14, KitArt.Brown,
                stage.Region(t0, 419f, t1, 468f, t0 + 22f, 422f, t0 + 250f, 466f), TextAnchor.MiddleLeft);
            totalLabel.gameObject.name = "TotalLabel";
            totalValue = KitArt.Fitted(total.transform, "", 28, 12, Gain,
                stage.Region(t0, 419f, t1, 468f, t0 + 255f, 422f, t1 - 22f, 466f), TextAnchor.MiddleRight);
            totalValue.gameObject.name = "TotalValue";
            totalValue.horizontalOverflow = HorizontalWrapMode.Overflow;
            return view;
        }

        // ========================================================================================
        // The earlier procedural layout: the fallback when a kit sprite is missing
        // ========================================================================================

        private void BuildPlain()
        {
            Scenery.Build(transform, withFloor: false);

            var plaque = Scenery.Plaque(transform, new Vector2(1100f, 600f), null, exact: true);
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

            // Five tallies and up to eight level-ups: a grid of seven columns holds all thirteen
            // in two rows, where a single row ran off the plaque.
            _events = new GameObject("Tallies", typeof(RectTransform)).GetComponent<RectTransform>();
            _events.SetParent(plaque, false);
            var grid = _events.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(TallyWidth, TallyHeight);
            grid.spacing = new Vector2(TallyGap, TallyGap);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 7;
            UiKit.Anchor(_events, new Vector2(0.5f, 0f), new Vector2(0f, 128f));
            _events.sizeDelta = new Vector2(7 * TallyWidth + 6 * TallyGap, 2 * TallyHeight + TallyGap);

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

        // ========================================================================================
        // The report (shared by both layouts): every number is the ledger's
        // ========================================================================================

        public override void Refresh()
        {
            var ledger = Session?.LastLedger;
            if (ledger == null) return;

            for (var i = 0; i < _rows.Count; i++)
            {
                if (_rows[i] == null) continue;

                // Out of the layout now: the screen may be refreshed from a click.
                _rows[i].SetActive(false);
                _rows[i].transform.SetParent(null, false);
                Destroy(_rows[i]);
            }

            _rows.Clear();

            var profit = ledger.Profit;

            _verdict.text = ledger.Verdict();
            _verdict.color = _kit ? (profit > 0 ? Gain : Loss) : (profit > 0 ? Palette.Good : Palette.Alarm);

            _subtitle.text = ledger.FailureReason ??
                             (Strings.Get("books.chemistry") + " " + ledger.ChemistryScore +
                              " / 100   ·   " + Strings.Get("books.satisfaction") + " " +
                              Mathf.RoundToInt(ledger.Satisfaction * 100f) + "%");

            if (_kit) RefreshKit(ledger);
            else RefreshPlain(ledger);

            var good = _kit ? Gain : Palette.Good;
            var bad = _kit ? Loss : Palette.Alarm;

            _bottomLine.text = Strings.Get(profit >= 0 ? "books.profit" : "books.loss") + "  " +
                               Strings.Signed(profit);
            _bottomLine.color = profit >= 0 ? good : bad;

            _balance.text = Strings.Get("books.balance") + "  " +
                            Strings.Money(ledger.BalanceAfter) +
                            (ledger.Warning ? "   ·   " + Strings.Get("books.warning") : "");
            _balance.color = ledger.Warning ? bad : (_kit ? KitArt.Brown : Palette.TextDim);

            Tween.Punch(_verdict.rectTransform, 0.16f, 0.4f);
        }

        private void RefreshPlain(DayLedger ledger)
        {
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
        }

        private void RefreshKit(DayLedger ledger)
        {
            Rows(_leftView, ledger.Incoming());
            Rows(_rightView, ledger.Outgoing());

            _revenueLabel.text = Strings.Get("books.revenue");
            _revenueValue.text = Strings.Money(ledger.Revenue);
            _revenueValue.color = Gain;
            _costsLabel.text = Strings.Get("books.costs");
            _costsValue.text = "−" + Strings.Money(ledger.Costs);
            _costsValue.color = Loss;

            // Five indicators, then one per maid who levelled up, in the order the ledger has them.
            var tallies = new List<TallyChip>
            {
                new TallyChip(Strings.Get("books.wrong"), ledger.WrongOrders, ledger.WrongOrders > 0, null),
                new TallyChip(Strings.Get("books.burnt"), ledger.Burnt, ledger.Burnt > 0, null),
                new TallyChip(Strings.Get("books.angry"), ledger.LeftAngry, ledger.LeftAngry > 0, null),
                new TallyChip(Strings.Get("books.turned"), ledger.TurnedAway, ledger.TurnedAway > 0, null),
                new TallyChip(Strings.Get("books.points"), ledger.PointsEarned, false, null),
            };

            foreach (var pair in ledger.LevelUps)
            {
                var maid = Session.Profile.MaidById(pair.Key);
                if (maid == null) continue;

                var accent = Color.Lerp(Palette.ForArchetype(maid.Archetype), Color.black, 0.45f);
                tallies.Add(new TallyChip(maid.Name + " " + Strings.Get("books.levelup"), pair.Value, false, accent));
            }

            Tallies(tallies);
        }

        private readonly struct TallyChip
        {
            public readonly string Caption;
            public readonly int Value;
            public readonly bool Bad;
            public readonly Color? Tint;

            public TallyChip(string caption, int value, bool bad, Color? tint)
            {
                Caption = caption;
                Value = value;
                Bad = bad;
                Tint = tint;
            }
        }

        /// <summary>The ledger's lines in one column, tightened and then scrolled if they outgrow the area.</summary>
        private void Rows(RectTransform view, IReadOnlyList<LedgerLine> lines)
        {
            var area = view.rect.size;
            var pitch = Mathf.Min(36f * K, area.y / Mathf.Max(1, lines.Count));
            pitch = Mathf.Max(pitch, 26f);

            var content = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
            content.SetParent(view, false);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0f, Mathf.Max(area.y, lines.Count * pitch));
            _rows.Add(content.gameObject);

            if (content.sizeDelta.y > area.y + 0.5f)
            {
                var scroll = view.GetComponent<ScrollRect>() ?? view.gameObject.AddComponent<ScrollRect>();
                scroll.content = content;
                scroll.viewport = view;
                scroll.horizontal = false;
                scroll.vertical = true;
                scroll.movementType = ScrollRect.MovementType.Clamped;
                scroll.scrollSensitivity = 24f;
            }

            var rowSize = new Vector2(area.x, pitch - 1f);
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                var row = KitArt.Plate(content, "results_row_empty", rowSize);
                row.gameObject.name = "Row";
                UiKit.Anchor(row.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -i * pitch));

                var size = Mathf.Clamp(Mathf.RoundToInt(rowSize.y * 0.62f), 12, 22);
                var label = KitArt.Fitted(row.transform, line.Label, size, 11, KitArt.Brown,
                    new Vector4(0.04f, 0.08f, 0.38f, 0.08f), TextAnchor.MiddleLeft);
                label.gameObject.name = "Label";
                label.fontStyle = FontStyle.Normal;

                var value = KitArt.Fitted(row.transform, (line.Outgoing ? "−" : "") + Strings.Money(line.Amount),
                    size, 10, line.Outgoing ? Loss : Gain, new Vector4(0.64f, 0.08f, 0.04f, 0.08f),
                    TextAnchor.MiddleRight);
                value.gameObject.name = "Value";
                value.horizontalOverflow = HorizontalWrapMode.Overflow;

                if (i == lines.Count - 1) continue;

                var rule = KitArt.Plate(content, "results_separator", new Vector2(area.x, 4f));
                rule.gameObject.name = "Separator";
                rule.type = Image.Type.Simple;
                rule.preserveAspect = false;
                UiKit.Anchor(rule.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -(i + 1) * pitch + 2f));
            }
        }

        /// <summary>
        /// Indicator chips in the band under the columns: one row at the kit's size, two rows (seven across)
        /// when there are more than five, and a vertical scroll past that.
        /// </summary>
        private void Tallies(List<TallyChip> tallies)
        {
            var view = _tallyView.rect.size;
            const float gap = 8f;
            var count = tallies.Count;

            var columns = count <= 5 ? Mathf.Max(1, count) : 7;
            var rows = Mathf.CeilToInt(count / (float)columns);
            var visible = Mathf.Min(rows, 2);

            var width = Mathf.Min(197f * K, (view.x - (columns - 1) * gap) / columns);
            var height = Mathf.Min(width * 97f / 197f, (view.y - (visible - 1) * gap) / visible);
            width = Mathf.Min(width, height * 197f / 97f);

            var content = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
            content.SetParent(_tallyView, false);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            var contentHeight = rows * height + (rows - 1) * gap;
            content.sizeDelta = new Vector2(0f, contentHeight);

            // Bottom-aligned in the band, like the kit's single row; scrolled from the top when taller.
            var top = contentHeight <= view.y ? -(view.y - contentHeight) : 0f;
            content.anchoredPosition = new Vector2(0f, top);
            _rows.Add(content.gameObject);

            if (contentHeight > view.y + 0.5f)
            {
                var scroll = _tallyView.GetComponent<ScrollRect>() ?? _tallyView.gameObject.AddComponent<ScrollRect>();
                scroll.content = content;
                scroll.viewport = _tallyView;
                scroll.horizontal = false;
                scroll.vertical = true;
                scroll.movementType = ScrollRect.MovementType.Clamped;
                scroll.scrollSensitivity = 24f;
            }

            for (var i = 0; i < count; i++)
            {
                var tally = tallies[i];

                var inRow = i % columns;
                var rowCount = Mathf.Min(columns, count - (i / columns) * columns);
                var left = (view.x - (rowCount * width + (rowCount - 1) * gap)) * 0.5f;

                var chip = KitArt.Plate(content, "results_tally_empty", new Vector2(width, height));
                chip.gameObject.name = "Tally";
                UiKit.Anchor(chip.rectTransform, new Vector2(0f, 1f),
                    new Vector2(left + inRow * (width + gap), -(i / columns) * (height + gap)));

                var tight = height < 70f;
                var number = KitArt.Fitted(chip.transform, tally.Value.ToString(), tight ? 24 : 34, 12,
                    tally.Tint ?? (tally.Bad ? Loss : KitArt.Brown),
                    new Vector4(0.08f, tight ? 0.42f : 0.46f, 0.08f, 0.1f));
                number.gameObject.name = "Number";
                number.horizontalOverflow = HorizontalWrapMode.Overflow;

                var caption = KitArt.Fitted(chip.transform, tally.Caption, tight ? 12 : 17, 9, KitArt.Brown,
                    new Vector4(0.08f, 0.1f, 0.08f, tight ? 0.6f : 0.56f));
                caption.gameObject.name = "Caption";
                caption.fontStyle = FontStyle.Normal;
            }
        }

        private void Line(Transform parent, LedgerLine line, bool total = false)
        {
            var row = UiKit.Panel(parent, new Vector2(440f, total ? 34f : 28f),
                Palette.WithAlpha(total ? Logo.Brass : Palette.Wall, total ? 0.4f : 0.35f), 0.3f, exact: true);
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
            var chip = UiKit.Panel(_events, new Vector2(TallyWidth, TallyHeight),
                Palette.WithAlpha(Palette.Wall, 0.4f), 0.25f, exact: true);
            _rows.Add(chip.gameObject);

            var number = UiKit.Label(chip, value.ToString(), Typeface.Heading,
                tint ?? (bad ? Palette.Alarm : Palette.Text), bold: true);
            number.rectTransform.sizeDelta = new Vector2(TallyWidth - 6f, 30f);
            UiKit.Anchor(number.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -2f));

            var caption = UiKit.Label(chip, text ?? Strings.Get(key), Typeface.Tiny,
                Palette.TextDim);
            caption.rectTransform.sizeDelta = new Vector2(TallyWidth - 6f, 20f);
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

            Scenery.Plank(column, Strings.Get("bust.restart"), new Vector2(400f, 60f),
                () => OnRestart?.Invoke(), Typeface.Heading, Logo.Brass, exact: true);
        }
    }
}
