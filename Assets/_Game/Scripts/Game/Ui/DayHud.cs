using System;
using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Sim.Shift;
using Neko.Sim.Team;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Game.Ui
{
    /// <summary>
    /// What the player sees while the day runs.
    /// </summary>
    /// <remarks>
    /// The lineup still plays the day, but the owner has hands: seat a party, take an order,
    /// clear a table, stop a dish on the golden mark, calm a table. Those are taps on the room
    /// itself, not buttons here, so this screen only keeps the clock, the till, the speed and
    /// skip controls, the combo the taps build, and a line saying what can be tapped.
    /// </remarks>
    public sealed class DayHud : Screen
    {
        private Text _clock;
        private Text _till;
        private Text _tally;
        private Text _closing;
        private Text _combo;
        private Text _comboTip;
        private Text _hint;
        private RectTransform _strip;
        private readonly System.Collections.Generic.List<GameObject> _chips =
            new System.Collections.Generic.List<GameObject>();

        public Sim.Shift.Shift Shift;
        public Action<float> OnSpeed;
        public Action OnSkip;

        protected override void Build()
        {
            var bar = Scenery.Plaque(transform, new Vector2(760f, 76f), null, exact: true);
            UiKit.Anchor((RectTransform)bar.parent, new Vector2(0.5f, 1f), new Vector2(0f, -10f));

            var row = UiKit.Row(bar, 24f, 12f);
            UiKit.Stretch(row);

            _clock = UiKit.Label(row, "0:00", Typeface.Heading, Palette.Text, bold: true);
            _clock.rectTransform.sizeDelta = new Vector2(120f, 42f);

            var till = UiKit.Column(row, 0f, 0f);
            till.sizeDelta = new Vector2(300f, 56f);

            _till = UiKit.Label(till, "", Typeface.Body, Logo.Brass, bold: true);
            _till.rectTransform.sizeDelta = new Vector2(300f, 28f);

            _tally = UiKit.Label(till, "", Typeface.Tiny, Palette.TextDim);
            _tally.rectTransform.sizeDelta = new Vector2(300f, 20f);

            var speeds = UiKit.Row(row, 6f);
            speeds.sizeDelta = new Vector2(250f, 48f);

            Speed(speeds, "1×", 1f);
            Speed(speeds, "2×", 2f);
            Speed(speeds, "4×", 4f);

            Scenery.Plank(speeds, "»", new Vector2(56f, 44f), () => OnSkip?.Invoke(),
                Typeface.Body, Logo.Brass);

            // The owner's combo: a run of helpful taps that lifts tips. Only drawn while it lasts.
            _combo = UiKit.Label(transform, "", Typeface.Heading, Logo.Brass, bold: true);
            UiKit.Anchor(_combo.rectTransform, new Vector2(0f, 1f), new Vector2(20f, -14f));
            _combo.rectTransform.sizeDelta = new Vector2(170f, 38f);
            _combo.alignment = TextAnchor.MiddleLeft;
            _combo.gameObject.SetActive(false);

            _comboTip = UiKit.Label(transform, "", Typeface.Tiny, Palette.Cream);
            UiKit.Anchor(_comboTip.rectTransform, new Vector2(0f, 1f), new Vector2(20f, -50f));
            _comboTip.rectTransform.sizeDelta = new Vector2(170f, 20f);
            _comboTip.alignment = TextAnchor.MiddleLeft;
            _comboTip.gameObject.SetActive(false);

            _closing = UiKit.Label(transform, Strings.Get("day.closing"), Typeface.Title,
                Palette.Alarm, bold: true);
            UiKit.Anchor(_closing.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 140f));
            _closing.rectTransform.sizeDelta = new Vector2(700f, 76f);
            _closing.gameObject.SetActive(false);

            _strip = UiKit.Row(transform, 8f);
            UiKit.Anchor(_strip, new Vector2(0f, 0f), new Vector2(16f, 46f));
            _strip.sizeDelta = new Vector2(1100f, 76f);

            _hint = UiKit.Label(transform, Strings.Get("day.watching"), Typeface.Small,
                Palette.WithAlpha(Palette.Cream, 0.7f));
            UiKit.Anchor(_hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 14f));
            _hint.rectTransform.sizeDelta = new Vector2(820f, 26f);
        }

        private void Speed(Transform parent, string label, float scale)
        {
            Scenery.Plank(parent, label, new Vector2(56f, 44f), () => OnSpeed?.Invoke(scale),
                Typeface.Small);
        }

        private bool _hintUrgent;

        /// <summary>
        /// The one thing worth tapping right now, in order of how fast it goes stale: a dish on
        /// its mark, a table with a hand up, a party at the door, a dirty table, an upset party.
        /// </summary>
        private string HintKey()
        {
            _hintUrgent = false;
            if (Shift == null || Shift.Finished) return "day.watching";

            foreach (var ticket in Shift.Tickets)
            {
                if (Shift.CookProgressOf(ticket) >= Sim.Shift.Shift.PerfectMin)
                {
                    _hintUrgent = true;
                    return "hint.stove";
                }
            }

            var greet = false;
            var upset = false;
            foreach (var customer in Shift.Customers)
            {
                if (customer.State == Sim.Service.CustomerState.Ordering)
                {
                    _hintUrgent = true;
                    return "hint.order";
                }

                if (customer.State == Sim.Service.CustomerState.Queueing) greet = true;
                else if (customer.DrainsPatience && !customer.Profile.NeverImpatient && customer.Patience < 0.5f) upset = true;
            }

            if (greet) return "hint.greet";

            foreach (var seat in Shift.Seats)
            {
                if (seat.Dirty) return "hint.clean";
            }

            return upset ? "hint.pamper" : "day.watching";
        }

        /// <summary>One chip per maid on the card, with her position and her mood.</summary>
        public void BuildChips(Sim.Shift.Shift shift)
        {
            Shift = shift;

            for (var i = 0; i < _chips.Count; i++)
            {
                if (_chips[i] != null) Destroy(_chips[i]);
            }

            _chips.Clear();
            if (shift == null) return;

            foreach (var worker in shift.Workers)
            {
                var chip = UiKit.Panel(_strip, new Vector2(116f, 68f),
                    Palette.WithAlpha(Palette.Panel, 0.92f), 0.2f);
                _chips.Add(chip.gameObject);

                var name = UiKit.Label(chip, worker.Name, Typeface.Tiny, Palette.Text,
                    bold: true);
                name.rectTransform.sizeDelta = new Vector2(112f, 18f);
                UiKit.Anchor(name.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -4f));

                var role = UiKit.Label(chip, RoleTable.For(worker.Role).DisplayName,
                    Typeface.Tiny, worker.InPosition ? Palette.TextDim : Palette.Alarm);
                role.rectTransform.sizeDelta = new Vector2(112f, 16f);
                UiKit.Anchor(role.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -22f));

                var mood = UiKit.Bar(chip, new Vector2(98f, 9f), Palette.Good, Palette.Wall);
                UiKit.Anchor((RectTransform)mood.rectTransform.parent, new Vector2(0.5f, 0f),
                    new Vector2(0f, 8f));

                chip.gameObject.AddComponent<MoodChip>().Bind(worker, mood);
            }
        }

        public void Sync()
        {
            if (Shift == null) return;

            var seconds = Mathf.Max(0f, Shift.TimeRemaining);
            _clock.text = Mathf.FloorToInt(seconds / 60f) + ":" +
                          Mathf.FloorToInt(seconds % 60f).ToString("00");
            _clock.color = seconds <= 20f ? Palette.Alarm : Palette.Text;

            _till.text = Strings.Get("day.till") + " " + Strings.Money(Shift.Earned);
            _tally.text = Strings.Get("day.served") + " " + Shift.Served + " · " +
                          Strings.Get("day.wrong") + " " + Shift.WrongOrders + " · " +
                          Strings.Get("day.angry") + " " + Shift.LeftAngry;

            _closing.gameObject.SetActive(Shift.ClosingTime && !Shift.Finished);

            _hint.text = Strings.Get(HintKey());
            _hint.color = Palette.WithAlpha(Palette.Cream, _hintUrgent ? 1f : 0.7f);

            var combo = Shift.Combo;
            _combo.gameObject.SetActive(combo > 0 && !Shift.Finished);
            _comboTip.gameObject.SetActive(combo > 0 && !Shift.Finished);
            if (combo > 0)
            {
                _combo.text = Strings.Get("day.combo") + " x" + combo;
                _comboTip.text = Strings.Get("day.tips") + " +" + Mathf.RoundToInt(Shift.ComboTipBonus * 100f) + "%";
            }
        }
    }

    /// <summary>Keeps one chip's mood bar in step with its worker.</summary>
    public sealed class MoodChip : MonoBehaviour
    {
        private Worker _worker;
        private Image _bar;

        public void Bind(Worker worker, Image bar)
        {
            _worker = worker;
            _bar = bar;
        }

        private void Update()
        {
            if (_worker?.Maid == null || _bar == null) return;

            UiKit.SetBar(_bar, _worker.Maid.Mood, 110f);
            _bar.color = _worker.OnBreak ? Palette.TextDim : Palette.ForPatience(_worker.Maid.Mood);
        }
    }
}
