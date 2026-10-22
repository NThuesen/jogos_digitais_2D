using System;
using System.Collections.Generic;
using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Sim.Economy;
using Neko.Sim.Maids;
using Neko.Sim.Progress;
using Neko.Sim.Team;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Game.Ui
{
    /// <summary>
    /// The whole collection, and one card read in full.
    /// </summary>
    /// <remarks>
    /// Where a player goes to find out why a maid is or is not worth her wage. Everything that
    /// matters to the lineup is on this screen in words: her position, her tags, her traits
    /// and what she costs every day whether she plays or not.
    /// </remarks>
    public sealed class RosterScreen : Screen
    {
        private readonly List<GameObject> _built = new List<GameObject>();

        private RectTransform _grid;
        private RectTransform _detail;
        private Text _header;
        private int _selected = -1;

        public GameSession Session;
        public Action OnBack;

        protected override void Build()
        {
            Scenery.Build(transform, withFloor: false);

            var title = UiKit.TrackedLabel(transform, "ELENCO", Typeface.Heading, Palette.Cream, 6f);
            UiKit.Anchor(title, new Vector2(0f, 1f), new Vector2(26f, -18f));

            _header = UiKit.Label(transform, "", Typeface.Body, Logo.Brass,
                TextAnchor.MiddleRight, bold: true);
            UiKit.Anchor(_header.rectTransform, new Vector2(1f, 1f), new Vector2(-170f, -26f));
            _header.rectTransform.sizeDelta = new Vector2(300f, 32f);

            Scenery.PlankAt(transform, Strings.Get("dialog.back"), new Vector2(120f, 44f),
                new Vector2(1f, 1f), new Vector2(-26f, -18f), () => OnBack?.Invoke(),
                Typeface.Small);

            _grid = UiKit.ScrollList(transform, new Vector2(700f, 560f), 10f);
            UiKit.Anchor((RectTransform)_grid.parent, new Vector2(0f, 0f), new Vector2(22f, 20f));

            var plaque = Scenery.Plaque(transform, new Vector2(500f, 560f));
            UiKit.Anchor((RectTransform)plaque.parent, new Vector2(1f, 0f), new Vector2(-22f, 20f));
            _detail = plaque;
        }

        public override void Refresh()
        {
            if (Session == null) return;

            Clear();

            var profile = Session.Profile;
            _header.text = profile.Maids.Count + " / " + profile.Unlocks.RosterCap + "   ·   " +
                           Strings.Get("hub.fixedcosts") + " " +
                           Strings.Money(Books.FixedCostsFor(profile));

            if (_selected < 0 && profile.Maids.Count > 0) _selected = profile.Maids[0].Id;

            for (var i = 0; i < profile.Maids.Count; i += 4) BuildRow(profile, i);

            BuildDetail();
        }

        private void BuildRow(Sim.Save.Profile profile, int start)
        {
            var row = UiKit.Row(_grid, 8f);
            row.sizeDelta = new Vector2(680f, 180f);
            _built.Add(row.gameObject);

            for (var i = start; i < start + 4 && i < profile.Maids.Count; i++)
            {
                var maid = profile.Maids[i];
                var playing = profile.Lineup.RoleOf(maid.Id);

                var card = MaidCard.Build(row, maid, 0.82f, showSalary: true, playingAs: playing);
                card.GetComponent<Image>().raycastTarget = true;

                var button = card.gameObject.AddComponent<Button>();
                button.targetGraphic = card.GetComponent<Image>();

                var captured = maid.Id;
                button.onClick.AddListener(() =>
                {
                    _selected = captured;
                    BuildDetail();
                });
            }
        }

        private void BuildDetail()
        {
            foreach (Transform child in _detail) Destroy(child.gameObject);

            var maid = Session.Profile.MaidById(_selected);
            if (maid == null) return;

            var column = UiKit.Column(_detail, 7f, 16f);
            UiKit.Stretch(column);

            var head = UiKit.Row(column, 12f);
            head.sizeDelta = new Vector2(460f, 160f);

            MaidCard.Build(head, maid, 0.72f, showSalary: false,
                playingAs: Session.Profile.Lineup.RoleOf(maid.Id));

            var facts = UiKit.Column(head, 3f, 0f);
            facts.sizeDelta = new Vector2(310f, 155f);

            Fact(facts, "roster.rarity", maid.Card.DisplayName, Palette.Hex(maid.Card.TintHex));
            Fact(facts, "lineup.natural", RoleTable.For(maid.NaturalRole).DisplayName, Logo.Brass);
            Fact(facts, "roster.salary", Strings.Money(maid.Salary), Palette.Alarm);
            Fact(facts, "roster.level", maid.Level + " / " + Maid.MaxLevel, Palette.Text);
            Fact(facts, "roster.tags", string.Join(" · ", ChemistryTags.Describe(maid)),
                Palette.TextDim);

            Stat(column, "roster.serve", maid.Total.Serve);
            Stat(column, "roster.cook", maid.Total.Cook);
            Stat(column, "roster.charm", maid.Total.Charm);
            Stat(column, "roster.stamina", maid.Total.Stamina);

            Traits(column, maid);

            var flavour = UiKit.Label(column, maid.Profile.Flavour, Typeface.Tiny,
                Palette.TextDim, TextAnchor.UpperLeft);
            flavour.rectTransform.sizeDelta = new Vector2(460f, 34f);

            var signature = BondTable.SignatureFor(maid.Archetype, maid.Bond);
            if (!string.IsNullOrEmpty(signature))
            {
                var mark = UiKit.Label(column,
                    Strings.Get("roster.signature") + ": " + signature, Typeface.Tiny,
                    Logo.Brass, TextAnchor.UpperLeft, bold: true);
                mark.rectTransform.sizeDelta = new Vector2(460f, 32f);
            }

            Scenery.Plank(column, Strings.Get("roster.dismiss") + " (+" + maid.Severance + ")",
                new Vector2(260f, 44f), () => Root.Dialog(
                    maid.Name, Strings.Get("roster.dismissask"), Strings.Get("dialog.yes"),
                    () =>
                    {
                        Session.Profile.Dismiss(maid.Id);
                        _selected = -1;
                        Session.Touch();
                        Refresh();
                    }, Strings.Get("dialog.no")), Typeface.Small, Palette.Alarm);
        }

        private static void Fact(Transform parent, string key, string value, Color colour)
        {
            var row = UiKit.Row(parent, 6f);
            row.sizeDelta = new Vector2(300f, 22f);

            var label = UiKit.Label(row, Strings.Get(key), Typeface.Tiny, Palette.TextDim,
                TextAnchor.MiddleLeft);
            label.rectTransform.sizeDelta = new Vector2(120f, 20f);

            var text = UiKit.Label(row, value, Typeface.Small, colour, TextAnchor.MiddleLeft,
                bold: true);
            text.rectTransform.sizeDelta = new Vector2(176f, 20f);
        }

        private static void Stat(Transform parent, string key, int value)
        {
            var row = UiKit.Row(parent, 8f);
            row.sizeDelta = new Vector2(460f, 20f);

            var label = UiKit.Label(row, Strings.Get(key), Typeface.Tiny, Palette.Text,
                TextAnchor.MiddleLeft);
            label.rectTransform.sizeDelta = new Vector2(96f, 18f);

            var bar = UiKit.Bar(row, new Vector2(286f, 11f), Logo.Brass, Palette.Wall);
            UiKit.SetBar(bar, value / 100f, 286f);

            var number = UiKit.Label(row, value.ToString(), Typeface.Tiny, Palette.Text,
                TextAnchor.MiddleRight, bold: true);
            number.rectTransform.sizeDelta = new Vector2(40f, 18f);
        }

        private void Traits(Transform parent, Maid maid)
        {
            var header = UiKit.Label(parent, Strings.Get("roster.traits"), Typeface.Tiny,
                Logo.Brass, TextAnchor.MiddleLeft, bold: true);
            header.rectTransform.sizeDelta = new Vector2(460f, 20f);

            if (maid.Traits.Count == 0)
            {
                var none = UiKit.Label(parent, Strings.Get("roster.notraits"), Typeface.Tiny,
                    Palette.TextDim, TextAnchor.MiddleLeft);
                none.rectTransform.sizeDelta = new Vector2(460f, 20f);
                return;
            }

            for (var i = 0; i < maid.Traits.Count; i++)
            {
                var def = TraitTable.ById(maid.Traits[i]);
                var line = UiKit.Label(parent, def.DisplayName + " — " + def.Description,
                    Typeface.Tiny, Palette.Text, TextAnchor.MiddleLeft);
                line.rectTransform.sizeDelta = new Vector2(460f, 20f);
            }
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
