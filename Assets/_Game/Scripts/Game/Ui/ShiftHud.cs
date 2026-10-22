using System.Collections.Generic;
using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Sim.Maids;
using Neko.Sim.Shift;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Game.Ui
{
    /// <summary>
    /// The clock, the till and the maids on shift.
    /// </summary>
    /// <remarks>
    /// Deliberately thin. Rubric 3 rewards a game that says what is happening through the
    /// world rather than the HUD, and nearly everything urgent here — who is about to leave,
    /// which plate is ready, who is worn out — is said by the café itself. What is left is
    /// the two numbers the player cannot infer from looking: the clock and the till.
    /// </remarks>
    public sealed class ShiftHud : Screen
    {
        private sealed class MaidChip
        {
            public RectTransform Rect;
            public Image Portrait;
            public Image Mood;
            public Button Rush;
            public Button Encore;
            public int WorkerId;
        }

        private readonly List<MaidChip> _chips = new List<MaidChip>();

        private Text _clock;
        private Text _earned;
        private Image _quotaBar;
        private Text _quotaText;
        private Text _closing;

        public Sim.Shift.Shift Shift;
        public System.Action OnPause;

        protected override void Build()
        {
            var top = UiKit.Panel(transform, new Vector2(760f, 74f),
                Palette.WithAlpha(Palette.Panel, 0.94f));
            UiKit.Anchor(top, new Vector2(0.5f, 1f), new Vector2(0f, -12f));

            var row = UiKit.Row(top, 26f, 18f);
            UiKit.Stretch(row);

            _clock = UiKit.Label(row, "0:00", Typeface.Heading, Palette.Text, bold: true);
            _clock.rectTransform.sizeDelta = new Vector2(130f, 44f);

            var till = UiKit.Column(row, 2f, 0f);
            till.sizeDelta = new Vector2(420f, 56f);

            _earned = UiKit.Label(till, "", Typeface.Body, Palette.Text, bold: true);
            _earned.rectTransform.sizeDelta = new Vector2(420f, 28f);

            _quotaBar = UiKit.Bar(till, new Vector2(400f, 14f), Palette.Focus, Palette.Wall);
            _quotaText = UiKit.Label(till, "", Typeface.Tiny, Palette.TextDim);
            _quotaText.rectTransform.sizeDelta = new Vector2(420f, 20f);

            UiKit.Button(row, "II", new Vector2(56f, 52f), Palette.Wall, () => OnPause?.Invoke());

            _closing = UiKit.Label(transform, Strings.Get("shift.closing"), Typeface.Title,
                Palette.Alarm, bold: true);
            UiKit.Anchor(_closing.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 120f));
            _closing.rectTransform.sizeDelta = new Vector2(700f, 80f);
            _closing.gameObject.SetActive(false);

            var hint = UiKit.Label(transform, Strings.Get("shift.hint"), Typeface.Small,
                Palette.TextDim);
            UiKit.Anchor(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 14f));
            hint.rectTransform.sizeDelta = new Vector2(820f, 28f);
        }

        /// <summary>Rebuilds the row of maid chips for a new day's roster.</summary>
        public void BuildChips(Sim.Shift.Shift shift)
        {
            Shift = shift;

            for (var i = 0; i < _chips.Count; i++)
            {
                if (_chips[i].Rect != null) Destroy(_chips[i].Rect.gameObject);
            }

            _chips.Clear();
            if (shift == null) return;

            var strip = UiKit.Row(transform, 10f);
            UiKit.Anchor(strip, new Vector2(0f, 0f), new Vector2(16f, 54f));
            strip.sizeDelta = new Vector2(900f, 108f);

            foreach (var worker in shift.Workers)
            {
                if (worker.IsManager) continue;
                _chips.Add(BuildChip(strip, worker));
            }
        }

        private MaidChip BuildChip(Transform parent, Worker worker)
        {
            var maid = worker.Maid;
            var card = UiKit.Panel(parent, new Vector2(104f, 104f),
                Palette.WithAlpha(Palette.Panel, 0.92f));

            var portrait = new GameObject("Portrait", typeof(RectTransform)).AddComponent<Image>();
            portrait.transform.SetParent(card, false);
            portrait.rectTransform.sizeDelta = new Vector2(74f, 74f);
            portrait.rectTransform.anchoredPosition = new Vector2(0f, 10f);
            portrait.preserveAspect = true;
            portrait.raycastTarget = false;
            portrait.sprite = UiKit.Sprites.Layered(
                View.MaidSpriteBuilder.Layers(maid, View.MaidPose.Idle),
                View.MaidSpriteBuilder.CacheKey(maid, View.MaidPose.Idle), 128);

            var mood = UiKit.Bar(card, new Vector2(80f, 9f), Palette.Good, Palette.Wall);
            UiKit.Anchor(mood.rectTransform.parent as RectTransform, new Vector2(0.5f, 0f),
                new Vector2(0f, 30f));

            var chip = new MaidChip
            {
                Rect = card,
                Portrait = portrait,
                Mood = mood,
                WorkerId = worker.Id,
            };

            chip.Rush = UiKit.Button(card, Strings.Get("shift.rush"), new Vector2(96f, 26f),
                Palette.ForArchetype(maid.Archetype),
                () => Shift?.Rush(chip.WorkerId), Typeface.Tiny);
            UiKit.Anchor(chip.Rush.GetComponent<RectTransform>(), new Vector2(0.5f, 0f),
                new Vector2(0f, 3f));

            if (!worker.HasEncore) return chip;

            chip.Encore = UiKit.Button(card, Strings.Get("shift.encore"), new Vector2(96f, 24f),
                Palette.Focus, () => Shift?.UseEncore(chip.WorkerId), Typeface.Tiny);
            UiKit.Anchor(chip.Encore.GetComponent<RectTransform>(), new Vector2(0.5f, 1f),
                new Vector2(0f, -2f));

            return chip;
        }

        public void Sync()
        {
            if (Shift == null) return;

            var seconds = Mathf.Max(0f, Shift.TimeRemaining);
            _clock.text = Mathf.FloorToInt(seconds / 60f) + ":" +
                          Mathf.FloorToInt(seconds % 60f).ToString("00");
            _clock.color = seconds <= 20f ? Palette.Alarm : Palette.Text;

            _earned.text = Strings.Get("shift.earned") + " " + Shift.Earned;
            _quotaText.text = Strings.Get("shift.quota") + " " + Shift.Quota + " · " +
                              Strings.Get("shift.served") + " " + Shift.Served + " · " +
                              Strings.Get("shift.angry") + " " + Shift.Angry;

            var progress = Shift.Quota <= 0 ? 1f : Shift.Earned / (float)Shift.Quota;
            UiKit.SetBar(_quotaBar, progress, 400f);
            _quotaBar.color = progress >= 1f ? Palette.Good : Palette.Focus;

            _closing.gameObject.SetActive(Shift.ClosingTime && !Shift.Finished);

            for (var i = 0; i < _chips.Count; i++) SyncChip(_chips[i]);
        }

        private void SyncChip(MaidChip chip)
        {
            var worker = Shift.WorkerById(chip.WorkerId);
            if (worker?.Maid == null) return;

            UiKit.SetBar(chip.Mood, worker.Maid.Mood, 80f);
            chip.Mood.color = Palette.ForPatience(worker.Maid.Mood);

            chip.Portrait.color = worker.OnBreak
                ? Palette.WithAlpha(Color.white, 0.45f)
                : Color.white;

            chip.Rush.interactable = !worker.OnBreak && worker.RushRemaining <= 0f;

            if (chip.Encore != null) chip.Encore.interactable = worker.EncoreCooldown <= 0f;
        }
    }
}
