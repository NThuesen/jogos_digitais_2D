using System;
using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Game.Feel;
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
    ///
    /// UI-INT-06: drawn from the shift kit — the café's decorated walls with a real opening (alpha) that
    /// shows the live room, a gold frame, a top HUD with the clock and the speed controls, one chip per
    /// maid on the card and the hint plate. The opening is a <see cref="RoomWindow"/>: the camera is
    /// framed into it and a click counts as a click on the room only inside it, so scenery never takes a
    /// tap meant for a customer, a table or the door. If a kit sprite is missing the earlier layout is
    /// built instead, unchanged.
    /// </remarks>
    public sealed class DayHud : Screen
    {
        // Mockup (1672x941) pixels to the 1280x720 stage; every cut-out sprite carries 4 px of padding.
        private const float K = 1280f / 1672f;
        private const float MockupWidth = 1672f;
        private const float MockupHeight = 941f;

        // The opening of the café's wall and the part of it where the room is framed and clicks count.
        private static readonly UnityEngine.Rect Safe = UnityEngine.Rect.MinMaxRect(466f, 201f, 1204f, 638f);

        private static readonly string[] KitSprites =
        {
            "shift_restaurant_background_open", "shift_room_frame", "shift_top_hud_empty",
            "shift_maid_chip_empty", "shift_mood_track", "shift_mood_fill", "shift_instruction_empty",
            "shift_speed_selected",
        };

        /// <summary>For tests: build the earlier procedural HUD even though the kit is present.</summary>
        public static bool ForcePlain;

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

        // The kit layout.
        private bool _kit;
        private ContainStage _stage;
        private RoomWindow _window;
        private Image _hintPlate;
        private RectTransform _chipView;
        private readonly Image[] _speedMarks = new Image[3];
        private float _speed = 1f;

        public Sim.Shift.Shift Shift;
        public CameraRig Camera;
        public Action<float> OnSpeed;
        public Action OnSkip;

        /// <summary>True when this HUD was built from the kit (the room sits in the café's opening).</summary>
        public bool IsKit => _kit;

        /// <summary>The window of the screen that shows the room, in camera pixels (kit layout only).</summary>
        public RoomWindow Window => _window;

        /// <summary>The speed the last speed control asked for; 1 until one is pressed.</summary>
        public float CurrentSpeed => _speed;

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
            var stage = _stage = ContainStage.Make(transform, MockupWidth, MockupHeight, K, 4f);
            stage.Letterbox(Palette.Hex("#3B2A25"));
            var root = stage.Rect;

            // The café, with its real opening: the live room shows through it, everything else is wall.
            var scenery = stage.Scenery();
            stage.Exact(scenery, "shift_restaurant_background_open", 0f, 0f, MockupWidth, MockupHeight, "Cafe");
            stage.Exact(root, "shift_room_frame", 440f, 175f, 1230f, 665f, "RoomFrame");

            // Where the room is framed and where a click counts as being on it.
            var window = stage.Slot(root, "RoomWindow", Safe.xMin, Safe.yMin, Safe.xMax, Safe.yMax);
            _window = window.gameObject.AddComponent<RoomWindow>();
            _window.Changed += OnWindowChanged;

            // The top HUD: one plate with the clock, the till and the speed controls drawn on it.
            stage.At(scenery, "shift_top_hud_empty", 342f, 5f, 1332f, 178f, "TopHud");

            _clock = KitArt.Fitted(stage.Slot(root, "Clock", 440f, 78f, 556f, 132f), "0:00", 42, 20,
                KitArt.Brown, Vector4.zero);
            _clock.gameObject.name = "ClockText";

            _till = KitArt.Fitted(stage.Slot(root, "Till", 625f, 56f, 985f, 102f), "", 30, 16, KitArt.Brown,
                Vector4.zero);
            _till.gameObject.name = "TillText";

            _tally = KitArt.Fitted(stage.Slot(root, "Tally", 615f, 102f, 995f, 136f), "", 19, 11, KitArt.Brown,
                Vector4.zero);
            _tally.gameObject.name = "TallyText";
            _tally.fontStyle = FontStyle.Normal;

            var speeds = new[] { 1f, 2f, 4f };
            for (var i = 0; i < speeds.Length; i++)
            {
                var x = 1023f + i * 65.2f;
                var scale = speeds[i];
                var index = i;

                // The gold plate over the chosen speed, under its label.
                var mark = stage.At(root, "shift_speed_selected", x, 83f, x + 58f, 131f, "Speed" + scale + "Mark");
                mark.gameObject.SetActive(i == 0);
                _speedMarks[i] = mark;

                Control(stage, root, "Speed" + scale, x - 3f, 75f, x + 61f, 139f, scale + "×", KitArt.Cream,
                    () =>
                    {
                        _speed = scale;
                        MarkSpeed(index);
                        OnSpeed?.Invoke(scale);
                    });
            }

            // The skip plate is the gold one baked into the HUD.
            Control(stage, root, "Skip", 1216f, 75f, 1280f, 139f, "»", KitArt.Brown, () => OnSkip?.Invoke());

            // The owner's combo, and the closing warning.
            _combo = KitArt.Fitted(stage.Slot(root, "Combo", 40f, 760f, 410f, 810f), "", 26, 14, Logo.Brass,
                Vector4.zero, TextAnchor.MiddleLeft, Palette.WithAlpha(KitArt.Brown, 0.95f));
            _combo.gameObject.name = "ComboText";
            _combo.transform.parent.gameObject.SetActive(false);

            _comboTip = KitArt.Fitted(stage.Slot(root, "ComboTip", 40f, 808f, 410f, 840f), "", 18, 11,
                KitArt.Cream, Vector4.zero, TextAnchor.MiddleLeft, Palette.WithAlpha(KitArt.Brown, 0.95f));
            _comboTip.gameObject.name = "ComboTipText";
            _comboTip.fontStyle = FontStyle.Normal;
            _comboTip.transform.parent.gameObject.SetActive(false);

            // The chips, one per maid on the card, in a row under the room that scrolls only when it must.
            _chipView = stage.Slot(root, "Chips", 438f, 668f, 1236f, 812f);
            _chipView.gameObject.AddComponent<RectMask2D>();
            _strip = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
            _strip.SetParent(_chipView, false);
            _strip.anchorMin = new Vector2(0f, 0.5f);
            _strip.anchorMax = new Vector2(0f, 0.5f);
            _strip.pivot = new Vector2(0f, 0.5f);
            _strip.anchoredPosition = Vector2.zero;
            _strip.sizeDelta = _chipView.sizeDelta;
            var scroll = _chipView.gameObject.AddComponent<ScrollRect>();
            scroll.content = _strip;
            scroll.viewport = _chipView;
            scroll.horizontal = true;
            scroll.vertical = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 28f;

            // What can be tapped right now.
            _hintPlate = stage.At(root, "shift_instruction_empty", 437f, 828f, 1236f, 899f, "HintPlate");
            _hint = KitArt.Fitted(stage.Slot(_hintPlate.transform, "Hint", 0f, 0f, 1f, 1f), Strings.Get("day.watching"),
                16, 10, KitArt.Cream, Vector4.zero);
            Region(_hint.rectTransform.parent as RectTransform, new Vector4(0.17f, 0.08f, 0.17f, 0.08f));
            _hint.gameObject.name = "HintText";
            _hint.fontStyle = FontStyle.Normal;

            _closing = UiKit.Label(root, Strings.Get("day.closing"), Typeface.Title, Palette.Alarm, bold: true);
            UiKit.Anchor(_closing.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -170f));
            _closing.rectTransform.sizeDelta = new Vector2(700f, 76f);
            var edge = _closing.gameObject.AddComponent<Outline>();
            edge.effectColor = Palette.WithAlpha(Color.white, 0.9f);
            edge.effectDistance = new Vector2(2f, -2f);
            _closing.gameObject.SetActive(false);
        }

        private static void Region(RectTransform rect, Vector4 margin)
        {
            rect.anchorMin = new Vector2(margin.x, margin.y);
            rect.anchorMax = new Vector2(1f - margin.z, 1f - margin.w);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>A control drawn on the HUD plate: an invisible hit area over the art, with a Unity label.</summary>
        private static Button Control(ContainStage stage, RectTransform root, string name, float x0, float y0,
            float x1, float y1, string label, Color colour, Action onClick)
        {
            var slot = stage.Slot(root, name, x0, y0, x1, y1);
            var catcher = slot.gameObject.AddComponent<Image>();
            catcher.color = new Color(1f, 1f, 1f, 0f);
            catcher.canvasRenderer.cullTransparentMesh = false;
            catcher.raycastTarget = true;

            var button = slot.gameObject.AddComponent<Button>();
            button.targetGraphic = catcher;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() =>
            {
                Tween.Punch(slot, 0.08f, 0.16f);
                onClick?.Invoke();
            });

            var text = KitArt.Fitted(slot, label, 26, 14, colour, new Vector4(0.04f, 0.04f, 0.04f, 0.04f));
            text.gameObject.name = "Label";
            return button;
        }

        private void MarkSpeed(int index)
        {
            for (var i = 0; i < _speedMarks.Length; i++)
            {
                if (_speedMarks[i] != null) _speedMarks[i].gameObject.SetActive(i == index);
            }
        }

        private void OnWindowChanged(UnityEngine.Rect pixels)
        {
            if (Shift == null || Camera == null) return;

            Camera.FrameRoomInWindow(Shift.Grid.Width, Shift.Grid.Height, pixels);
        }

        // ========================================================================================
        // The earlier procedural layout: the fallback when a kit sprite is missing
        // ========================================================================================

        private void BuildPlain()
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
            UiKit.Anchor(_combo.rectTransform, new Vector2(0f, 0.5f), new Vector2(20f, 30f));
            _combo.rectTransform.sizeDelta = new Vector2(260f, 38f);
            _combo.alignment = TextAnchor.MiddleLeft;
            _combo.gameObject.SetActive(false);

            _comboTip = UiKit.Label(transform, "", Typeface.Tiny, Palette.Cream);
            UiKit.Anchor(_comboTip.rectTransform, new Vector2(0f, 0.5f), new Vector2(20f, -4f));
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
            Scenery.Plank(parent, label, new Vector2(56f, 44f), () => { _speed = scale; OnSpeed?.Invoke(scale); },
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
                var progress = Shift.CookProgressOf(ticket);
                if (progress >= Sim.Shift.Shift.PerfectMin && progress <= Sim.Shift.Shift.PerfectMax)
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

        /// <summary>One chip per maid on the card, with her position, her state and her mood.</summary>
        public void BuildChips(Sim.Shift.Shift shift)
        {
            Shift = shift;

            for (var i = 0; i < _chips.Count; i++)
            {
                if (_chips[i] == null) continue;

                _chips[i].SetActive(false);
                _chips[i].transform.SetParent(null, false);
                Destroy(_chips[i]);
            }

            _chips.Clear();
            if (shift == null) return;

            if (_kit)
            {
                BuildKitChips(shift);
                _window?.Reframe();
                return;
            }

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

        private void BuildKitChips(Sim.Shift.Shift shift)
        {
            var count = shift.Workers.Count;
            if (count == 0) return;

            const float gap = 8f;
            var view = _chipView.rect.size;
            var width = Mathf.Clamp((view.x - gap * (count - 1)) / count, 104f, 175f);
            var height = width * 148f / 236f;
            var total = count * width + (count - 1) * gap;

            // Centred while it fits; flush left and scrollable when there are more maids than room.
            _strip.sizeDelta = new Vector2(Mathf.Max(total, view.x), view.y);
            var left = Mathf.Max(0f, (view.x - total) * 0.5f);

            var big = Mathf.Clamp(Mathf.RoundToInt(width * 0.115f), 12, 18);
            var small = Mathf.Clamp(Mathf.RoundToInt(width * 0.085f), 11, 14);

            var at = 0;
            foreach (var worker in shift.Workers)
            {
                var chip = KitArt.Plate(_strip, "shift_maid_chip_empty", new Vector2(width, height));
                chip.gameObject.name = "Chip_" + worker.Name;
                chip.type = Image.Type.Simple;
                chip.preserveAspect = false;
                UiKit.Anchor(chip.rectTransform, new Vector2(0f, 0.5f), new Vector2(left + at * (width + gap), 0f));
                _chips.Add(chip.gameObject);
                at++;

                var name = KitArt.Fitted(chip.transform, worker.Name, big, 10, KitArt.Brown,
                    new Vector4(0.1f, 0.52f, 0.1f, 0.18f));
                name.gameObject.name = "Name";

                var role = KitArt.Fitted(chip.transform, RoleTable.For(worker.Role).DisplayName, small, 9,
                    KitArt.Brown, new Vector4(0.08f, 0.36f, 0.08f, 0.46f));
                role.gameObject.name = "Role";
                role.fontStyle = FontStyle.Normal;

                var state = KitArt.Fitted(chip.transform, "", small, 9, KitArt.Brown,
                    new Vector4(0.08f, 0.2f, 0.08f, 0.64f));
                state.gameObject.name = "State";
                state.fontStyle = FontStyle.Normal;

                var track = KitArt.Plate(chip.transform, "shift_mood_track", new Vector2(width * 0.62f, 9f));
                track.gameObject.name = "MoodTrack";
                UiKit.Anchor(track.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, height * 0.12f));

                var fill = KitArt.Plate(track.transform, "shift_mood_fill", new Vector2(width * 0.62f, 9f));
                fill.gameObject.name = "MoodFill";
                fill.type = Image.Type.Filled;
                fill.fillMethod = Image.FillMethod.Horizontal;
                fill.fillOrigin = 0;
                fill.preserveAspect = false;
                UiKit.Stretch(fill.rectTransform);

                chip.gameObject.AddComponent<MoodChip>().BindKit(worker, fill, role, state);
            }
        }

        public void Sync()
        {
            if (Shift == null) return;

            var seconds = Mathf.Max(0f, Shift.TimeRemaining);
            _clock.text = Mathf.FloorToInt(seconds / 60f) + ":" +
                          Mathf.FloorToInt(seconds % 60f).ToString("00");
            _clock.color = seconds <= 20f ? Palette.Alarm : (_kit ? KitArt.Brown : Palette.Text);

            _till.text = Strings.Get("day.till") + " " + Strings.Money(Shift.Earned);
            _tally.text = Strings.Get("day.served") + " " + Shift.Served + " · " +
                          Strings.Get("day.wrong") + " " + Shift.WrongOrders + " · " +
                          Strings.Get("day.angry") + " " + Shift.LeftAngry;

            _closing.gameObject.SetActive(Shift.ClosingTime && !Shift.Finished);

            _hint.text = Strings.Get(HintKey());
            if (_kit)
            {
                // An urgent tap tints the plate the hint sits on; the text keeps its colour.
                _hintPlate.color = _hintUrgent ? new Color(1f, 0.8f, 0.72f, 1f) : Color.white;
            }
            else
            {
                _hint.color = Palette.WithAlpha(Palette.Cream, _hintUrgent ? 1f : 0.7f);
            }

            var combo = Shift.Combo;
            var comboOn = combo > 0 && !Shift.Finished;
            ShowCombo(_combo, comboOn);
            ShowCombo(_comboTip, comboOn);
            if (combo > 0)
            {
                _combo.text = Strings.Get("day.combo") + " x" + combo +
                              (combo >= Sim.Shift.Shift.ComboCap ? " " + Strings.Get("day.combo.max") : "");
                _comboTip.text = Strings.Get("day.tips") + " +" + Mathf.RoundToInt(Shift.ComboTipBonus * 100f) + "%";
            }
        }

        /// <summary>The combo labels sit in a slot of their own in the kit layout; the slot is what is shown.</summary>
        private void ShowCombo(Text label, bool on)
        {
            var target = _kit ? label.transform.parent.gameObject : label.gameObject;
            if (target.activeSelf != on) target.SetActive(on);
        }

        /// <summary>Plain-text state of a worker: what she is doing right now.</summary>
        public static string StateOf(Worker worker)
        {
            if (worker.OnBreak) return Strings.Get("day.state.break");
            if (worker.Claimed != null || worker.Carrying) return Strings.Get("day.state.busy");
            return Strings.Get(worker.InPosition ? "day.state.ready" : "day.state.walking");
        }
    }

    /// <summary>Keeps one chip's mood bar (and, in the kit layout, her state line) in step with its worker.</summary>
    public sealed class MoodChip : MonoBehaviour
    {
        private Worker _worker;
        private Image _bar;
        private Text _role;
        private Text _state;
        private bool _kit;

        public void Bind(Worker worker, Image bar)
        {
            _worker = worker;
            _bar = bar;
        }

        public void BindKit(Worker worker, Image fill, Text role, Text state)
        {
            _worker = worker;
            _bar = fill;
            _role = role;
            _state = state;
            _kit = true;
        }

        private void Update()
        {
            if (_worker?.Maid == null || _bar == null) return;

            if (!_kit)
            {
                UiKit.SetBar(_bar, _worker.Maid.Mood, 110f);
                _bar.color = _worker.OnBreak ? Palette.TextDim : Palette.ForPatience(_worker.Maid.Mood);
                return;
            }

            var mood = Mathf.Clamp01(_worker.Maid.Mood);
            _bar.fillAmount = mood;
            _bar.color = _worker.OnBreak ? new Color(0.7f, 0.7f, 0.7f, 1f)
                : mood < 0.3f ? new Color(1f, 0.45f, 0.45f, 1f) : Color.white;

            var state = DayHud.StateOf(_worker);
            if (_state.text != state) _state.text = state;
            _role.color = _worker.InPosition ? KitArt.Brown : Palette.Alarm;
        }
    }
}
