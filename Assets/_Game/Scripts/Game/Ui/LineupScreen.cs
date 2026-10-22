using System;
using System.Collections.Generic;
using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Game.Feel;
using Neko.Sim.Maids;
using Neko.Sim.Team;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Game.Ui
{
    /// <summary>
    /// The board. This screen is the game.
    /// </summary>
    /// <remarks>
    /// Everything else — the boxes, the shop, the room, the day itself — exists so that there
    /// is something to decide here. A player who understands this screen understands Neko Maid
    /// Inc., and one who does not has nothing else to learn that would help.
    ///
    /// Laid out like a team sheet rather than a list: positions across, bench underneath,
    /// chemistry in the corner, and every card showing where it belongs so a bad fit is
    /// visible before the doors open rather than after.
    /// </remarks>
    public sealed class LineupScreen : Screen
    {
        private readonly List<GameObject> _built = new List<GameObject>();

        private RectTransform _board;
        private RectTransform _bench;
        private Text _chemistry;
        private Image _chemistryBar;
        private Text _verdict;
        private Text _warning;

        /// <summary>The slot waiting for somebody, or null.</summary>
        private LineupSlot _picking;

        public GameSession Session;
        public Action OnBack;

        protected override void Build()
        {
            Scenery.Build(transform, withFloor: false);

            var title = UiKit.TrackedLabel(transform, "ESCALAÇÃO", Typeface.Heading,
                Palette.Cream, 5f);
            UiKit.Anchor(title, new Vector2(0f, 1f), new Vector2(26f, -18f));

            Scenery.PlankAt(transform, Strings.Get("dialog.back"), new Vector2(120f, 46f),
                new Vector2(1f, 1f), new Vector2(-26f, -18f), () => OnBack?.Invoke(),
                Typeface.Small);

            ChemistryPanel();

            _board = UiKit.Row(transform, 18f);
            UiKit.Anchor(_board, new Vector2(0.5f, 1f), new Vector2(0f, -86f));
            _board.sizeDelta = new Vector2(1200f, 190f);

            var benchLabel = UiKit.Label(transform, Strings.Get("lineup.bench"), Typeface.Body,
                Logo.Brass, TextAnchor.MiddleLeft, bold: true);
            UiKit.Anchor(benchLabel.rectTransform, new Vector2(0f, 1f), new Vector2(26f, -292f));
            benchLabel.rectTransform.sizeDelta = new Vector2(300f, 26f);

            _bench = UiKit.ScrollRow(transform, new Vector2(1200f, 252f), 12f);
            UiKit.Anchor((RectTransform)_bench.parent, new Vector2(0.5f, 1f),
                new Vector2(0f, -318f));

            _warning = UiKit.Label(transform, "", Typeface.Body, Palette.Alarm, bold: true);
            UiKit.Anchor(_warning.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 76f));
            _warning.rectTransform.sizeDelta = new Vector2(900f, 28f);

            var tools = UiKit.Row(transform, 10f);
            UiKit.Anchor(tools, new Vector2(1f, 0f), new Vector2(-26f, 20f));
            tools.sizeDelta = new Vector2(400f, 44f);

            Scenery.Plank(tools, Strings.Get("lineup.auto"), new Vector2(190f, 42f), () =>
            {
                Session.Profile.Lineup.AutoFill(Session.Profile.Maids);
                Session.Touch();
                Refresh();
            }, Typeface.Small);

            Scenery.Plank(tools, Strings.Get("lineup.clear"), new Vector2(130f, 42f), () =>
            {
                Session.Profile.Lineup.Clear();
                Session.Touch();
                Refresh();
            }, Typeface.Small);
        }

        private void ChemistryPanel()
        {
            var plaque = Scenery.Plaque(transform, new Vector2(330f, 74f));
            UiKit.Anchor((RectTransform)plaque.parent, new Vector2(1f, 1f),
                new Vector2(-160f, -18f));

            _chemistry = UiKit.Label(plaque, "", Typeface.Body, Palette.Text, bold: true);
            _chemistry.rectTransform.sizeDelta = new Vector2(300f, 24f);
            UiKit.Anchor(_chemistry.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -6f));

            _chemistryBar = UiKit.Bar(plaque, new Vector2(290f, 12f), Logo.Brass, Palette.Wall);
            UiKit.Anchor((RectTransform)_chemistryBar.rectTransform.parent,
                new Vector2(0.5f, 0.5f), new Vector2(0f, -2f));

            _verdict = UiKit.Label(plaque, "", Typeface.Tiny, Palette.TextDim);
            _verdict.rectTransform.sizeDelta = new Vector2(300f, 18f);
            UiKit.Anchor(_verdict.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 6f));
        }

        public override void Refresh()
        {
            if (Session == null) return;

            for (var i = 0; i < _built.Count; i++)
            {
                if (_built[i] != null) Destroy(_built[i]);
            }

            _built.Clear();

            BuildBoard();
            BuildBench();
            SyncChemistry();

            var why = Session.WhyNotReady();
            _warning.text = why ?? "";
            _warning.gameObject.SetActive(why != null);
        }

        private void BuildBoard()
        {
            var profile = Session.Profile;

            foreach (Role role in Enum.GetValues(typeof(Role)))
            {
                var count = profile.Lineup.Slots.For(role);
                if (count == 0) continue;

                // A header over a row of that position's slots, rather than a stack. Two
                // kitchen slots stacked ran four hundred pixels tall and spilled through the
                // bench underneath it.
                var group = UiKit.Column(_board, 5f, 0f);
                var width = (MaidCard.Width * 0.62f + 6f) * count;
                group.sizeDelta = new Vector2(width, 180f);
                _built.Add(group.gameObject);

                var header = UiKit.Panel(group, new Vector2(width, 24f), Logo.BoardDeep, 0.4f);
                var label = UiKit.Label(header, RoleTable.For(role).DisplayName, Typeface.Tiny,
                    Logo.Brass, bold: true);
                UiKit.Stretch(label.rectTransform, 2f);

                var slots = UiKit.Row(group, 6f);
                slots.sizeDelta = new Vector2(width, 142f);

                for (var i = 0; i < count; i++)
                {
                    BuildSlot(slots, profile.Lineup.Slot(role, i));
                }
            }
        }

        private void BuildSlot(Transform parent, LineupSlot slot)
        {
            if (slot == null) return;

            var maid = Session.Profile.MaidById(slot.MaidId);
            var card = MaidCard.Build(parent, maid, 0.62f, showSalary: false, playingAs: slot.Role);
            card.GetComponent<Image>().raycastTarget = true;

            var button = card.gameObject.AddComponent<Button>();
            button.targetGraphic = card.GetComponent<Image>();
            button.onClick.AddListener(() =>
            {
                if (maid != null)
                {
                    Session.Profile.Lineup.Remove(maid.Id);
                    Session.Touch();
                    Refresh();
                    return;
                }

                _picking = slot;
                Root.Dialog(Strings.Get("lineup.pick"),
                    RoleTable.For(slot.Role).Description, Strings.Get("dialog.ok"));
                Refresh();
            });

            if (_picking == slot) Tween.Punch(card, 0.08f, 0.5f);
        }

        /// <summary>
        /// Everybody not on the board, with what it would cost to play them where they stand.
        /// </summary>
        /// <remarks>
        /// Clicking a bench card drops her into whichever slot is waiting, or into her own
        /// natural position if none is. Picking the position first and the maid second is one
        /// click fewer than the other way round, and the natural-position fallback means a
        /// player who never reads the instructions still builds a sensible side.
        /// </remarks>
        private void BuildBench()
        {
            var profile = Session.Profile;
            var any = false;

            for (var i = 0; i < profile.Maids.Count; i++)
            {
                var maid = profile.Maids[i];
                if (profile.Lineup.Contains(maid.Id)) continue;

                any = true;
                var card = MaidCard.Build(_bench, maid, 1.02f);
                card.GetComponent<Image>().raycastTarget = true;
                _built.Add(card.gameObject);

                var button = card.gameObject.AddComponent<Button>();
                button.targetGraphic = card.GetComponent<Image>();

                var captured = maid;
                button.onClick.AddListener(() => Field(captured));
            }

            if (any) return;

            var empty = UiKit.Label(_bench, Strings.Get("lineup.empty"), Typeface.Small,
                Palette.TextDim);
            empty.rectTransform.sizeDelta = new Vector2(400f, 40f);
            _built.Add(empty.gameObject);
        }

        private void Field(Maid maid)
        {
            var lineup = Session.Profile.Lineup;
            var target = _picking;

            if (target == null)
            {
                // Her own position first, then anywhere free.
                target = FirstFree(maid.NaturalRole) ?? FirstFreeAnywhere();
            }

            if (target == null)
            {
                Root.Dialog(Strings.Get("lineup.title"), Strings.Get("roster.full"));
                return;
            }

            lineup.Assign(target.Role, target.Index, maid.Id);
            _picking = null;

            Session.Touch();
            Refresh();
        }

        private LineupSlot FirstFree(Role role)
        {
            var lineup = Session.Profile.Lineup;

            for (var i = 0; i < lineup.Slots.For(role); i++)
            {
                var slot = lineup.Slot(role, i);
                if (slot != null && slot.Empty) return slot;
            }

            return null;
        }

        private LineupSlot FirstFreeAnywhere()
        {
            foreach (var slot in Session.Profile.Lineup.All)
            {
                if (slot.Empty) return slot;
            }

            return null;
        }

        private void SyncChemistry()
        {
            var chemistry = Session.Profile.Chemistry();

            _chemistry.text = Strings.Get("lineup.chemistry") + "  " + chemistry.Score + " / 100";
            _verdict.text = Chemistry.Verdict(chemistry.Score) + "   ×" +
                            chemistry.Multiplier.ToString("0.00");

            UiKit.SetBar(_chemistryBar, chemistry.Score / 100f, 290f);
            _chemistryBar.color = chemistry.Score >= 65
                ? Palette.Good
                : chemistry.Score >= 35
                    ? Logo.Brass
                    : Palette.Alarm;
        }
    }
}
