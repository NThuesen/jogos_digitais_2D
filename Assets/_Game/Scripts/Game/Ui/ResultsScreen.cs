using System.Collections.Generic;
using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Game.Feel;
using Neko.Sim.Shift;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Game.Ui
{
    /// <summary>
    /// What the day came to.
    /// </summary>
    /// <remarks>
    /// A missed quota is reported as a slow day with the sentence that explains why it is not
    /// a loss — the curve does not advance, so tomorrow is the same and the player is simply
    /// better at it. That one line is most of what keeps a bad run from feeling like a wall.
    /// </remarks>
    public sealed class ResultsScreen : Screen
    {
        private readonly List<GameObject> _rows = new List<GameObject>();

        private Text _title;
        private Text _subtitle;
        private RectTransform _list;

        public GameSession Session;
        public System.Action OnContinue;

        protected override void Build()
        {
            Backdrop(Palette.WithAlpha(Palette.Ink, 0.72f));

            var box = UiKit.Panel(transform, new Vector2(680f, 560f), Palette.Panel);
            UiKit.Anchor(box, new Vector2(0.5f, 0.5f), Vector2.zero);

            var column = UiKit.Column(box, 10f, 26f);
            UiKit.Stretch(column);

            _title = UiKit.Label(column, "", Typeface.Title, Palette.Text, bold: true);
            _title.rectTransform.sizeDelta = new Vector2(620f, 66f);

            _subtitle = UiKit.Label(column, "", Typeface.Small, Palette.TextDim);
            _subtitle.rectTransform.sizeDelta = new Vector2(620f, 48f);

            _list = UiKit.Column(column, 6f, 0f);
            _list.sizeDelta = new Vector2(620f, 300f);

            UiKit.Button(column, Strings.Get("results.continue"), new Vector2(300f, 58f),
                Palette.Focus, () => OnContinue?.Invoke(), Typeface.Heading);
        }

        public override void Refresh()
        {
            var result = Session?.LastResult;
            if (result == null) return;

            for (var i = 0; i < _rows.Count; i++)
            {
                if (_rows[i] != null) Destroy(_rows[i]);
            }

            _rows.Clear();

            var met = result.QuotaMet;
            _title.text = Strings.Get(met ? "results.met" : "results.missed");
            _title.color = met ? Palette.Good : Palette.Text;

            _subtitle.text = result.FailureReason ??
                             (met ? "" : Strings.Get("results.missedhint"));

            Row(Strings.Get("results.earned"), result.Earned + " / " + result.Quota,
                met ? Palette.Good : Palette.Text);
            Row(Strings.Get("results.served"), result.Served.ToString(), Palette.Text);
            Row(Strings.Get("results.angry"), result.Angry.ToString(),
                result.Angry > 0 ? Palette.Alarm : Palette.TextDim);
            Row(Strings.Get("results.turned"), result.TurnedAway.ToString(),
                result.TurnedAway > 0 ? Palette.Alarm : Palette.TextDim);
            Row(Strings.Get("results.reputation"), "+" + Mathf.Max(0, result.ReputationDelta),
                Palette.Focus);

            foreach (var pair in Session.LastLevelUps)
            {
                var maid = Session.Profile.MaidById(pair.Key);
                if (maid == null) continue;

                Row(maid.Name, Strings.Get("results.levelup") + " " + maid.Level,
                    Palette.ForArchetype(maid.Archetype));
            }

            if (Session.CafeLevelledUp)
            {
                Row(Strings.Get("cafe.level"), Session.Profile.CafeLevel.ToString(), Palette.Focus);
            }

            Tween.Punch(_title.rectTransform, 0.18f, 0.4f);
        }

        private void Row(string left, string right, Color colour)
        {
            var row = UiKit.Panel(_list, new Vector2(600f, 38f),
                Palette.WithAlpha(Palette.Wall, 0.5f), 0.3f);

            var label = UiKit.Label(row, left, Typeface.Body, Palette.Text, TextAnchor.MiddleLeft);
            label.rectTransform.sizeDelta = new Vector2(380f, 32f);
            label.rectTransform.anchoredPosition = new Vector2(-100f, 0f);

            var value = UiKit.Label(row, right, Typeface.Body, colour, TextAnchor.MiddleRight,
                bold: true);
            value.rectTransform.sizeDelta = new Vector2(190f, 32f);
            value.rectTransform.anchoredPosition = new Vector2(150f, 0f);

            _rows.Add(row.gameObject);
        }
    }
}
