using System.Collections.Generic;
using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Game.View;
using Neko.Sim.Economy;
using Neko.Sim.Maids;
using Neko.Sim.Progress;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Game.Ui
{
    /// <summary>
    /// The maids: who is on shift, what they can do, and what they are wearing.
    /// </summary>
    /// <remarks>
    /// The screen the game is about. Everything a player spends money on ends up visible
    /// here — and, because gear is a layer in her sprite, visible on her as well.
    /// </remarks>
    public sealed class RosterScreen : Screen
    {
        private readonly List<GameObject> _cards = new List<GameObject>();
        private readonly List<GameObject> _detail = new List<GameObject>();

        private RectTransform _list;
        private RectTransform _pane;
        private Text _hireHeader;
        private RectTransform _hireList;
        private int _selected = -1;

        public GameSession Session;
        public System.Action OnBack;

        protected override void Build()
        {
            Backdrop(Palette.Cream);

            var title = UiKit.Label(transform, Strings.Get("roster.title"), Typeface.Heading,
                Palette.Text, bold: true);
            UiKit.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(24f, -16f));
            title.rectTransform.sizeDelta = new Vector2(400f, 44f);
            title.alignment = TextAnchor.MiddleLeft;

            var back = UiKit.Button(transform, "<", new Vector2(56f, 48f), Palette.Wall,
                () => OnBack?.Invoke());
            UiKit.Anchor(back.GetComponent<RectTransform>(), new Vector2(1f, 1f),
                new Vector2(-16f, -16f));

            _list = UiKit.ScrollList(transform, new Vector2(380f, 540f));
            UiKit.Anchor((RectTransform)_list.parent, new Vector2(0f, 0f), new Vector2(20f, 20f));

            _pane = UiKit.Panel(transform, new Vector2(500f, 540f), Palette.Panel);
            UiKit.Anchor(_pane, new Vector2(0.5f, 0f), new Vector2(20f, 20f));

            _hireHeader = UiKit.Label(transform, Strings.Get("hire.title"), Typeface.Body,
                Palette.Text, bold: true);
            UiKit.Anchor(_hireHeader.rectTransform, new Vector2(1f, 1f), new Vector2(-16f, -76f));
            _hireHeader.rectTransform.sizeDelta = new Vector2(300f, 30f);

            _hireList = UiKit.ScrollList(transform, new Vector2(300f, 450f));
            UiKit.Anchor((RectTransform)_hireList.parent, new Vector2(1f, 0f),
                new Vector2(-16f, 20f));
        }

        public override void Refresh()
        {
            if (Session == null) return;

            Clear(_cards);
            var profile = Session.Profile;

            for (var i = 0; i < profile.Maids.Count; i++) BuildCard(profile.Maids[i]);

            if (_selected < 0 && profile.Maids.Count > 0) _selected = profile.Maids[0].Id;

            BuildDetail();
            BuildHiring();
        }

        private void BuildCard(Maid maid)
        {
            var profile = Session.Profile;
            var onShift = profile.ShiftRoster.Contains(maid.Id);

            var card = UiKit.Panel(_list, new Vector2(350f, 96f),
                onShift ? Palette.Panel : Palette.WithAlpha(Palette.Wall, 0.6f));
            card.GetComponent<Image>().raycastTarget = true;

            var button = card.gameObject.AddComponent<Button>();
            button.targetGraphic = card.GetComponent<Image>();
            button.onClick.AddListener(() =>
            {
                _selected = maid.Id;
                BuildDetail();
            });

            var portrait = Portrait(card, maid, 78f);
            UiKit.Anchor(portrait.rectTransform, new Vector2(0f, 0.5f), new Vector2(8f, 0f));

            var name = UiKit.Label(card, maid.Name + "  ·  " + maid.Profile.DisplayName,
                Typeface.Small, Palette.Text, TextAnchor.MiddleLeft, bold: true);
            name.rectTransform.sizeDelta = new Vector2(230f, 24f);
            name.rectTransform.anchoredPosition = new Vector2(55f, 24f);

            var level = UiKit.Label(card,
                Strings.Get("roster.level") + " " + maid.Level + "   " +
                Strings.Get("roster.bond") + " " + maid.Bond + "/" + BondTable.MaxLevel,
                Typeface.Tiny, Palette.TextDim, TextAnchor.MiddleLeft);
            level.rectTransform.sizeDelta = new Vector2(230f, 20f);
            level.rectTransform.anchoredPosition = new Vector2(55f, 2f);

            var toggle = UiKit.Button(card,
                Strings.Get(onShift ? "roster.remove" : "roster.put"), new Vector2(210f, 26f),
                onShift ? Palette.Wall : Palette.Focus, () =>
                {
                    if (!profile.SetRostered(maid.Id, !onShift))
                    {
                        Root.Dialog(Strings.Get("roster.title"),
                            Strings.Get("roster.rosterfull"));
                        return;
                    }

                    Session.Touch();
                    Refresh();
                }, Typeface.Tiny);

            UiKit.Anchor(toggle.GetComponent<RectTransform>(), new Vector2(1f, 0f),
                new Vector2(-8f, 6f));

            _cards.Add(card.gameObject);
        }

        private Image Portrait(Transform parent, Maid maid, float size)
        {
            var image = new GameObject("Portrait", typeof(RectTransform)).AddComponent<Image>();
            image.transform.SetParent(parent, false);
            image.rectTransform.sizeDelta = new Vector2(size, size);
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.sprite = UiKit.Sprites.Layered(MaidSpriteBuilder.Layers(maid, MaidPose.Idle),
                MaidSpriteBuilder.CacheKey(maid, MaidPose.Idle), 128);
            return image;
        }

        private void BuildDetail()
        {
            Clear(_detail);

            var maid = Session.Profile.MaidById(_selected);
            if (maid == null) return;

            var column = UiKit.Column(_pane, 8f, 18f);
            UiKit.Stretch(column);
            _detail.Add(column.gameObject);

            var head = UiKit.Row(column, 12f);
            head.sizeDelta = new Vector2(460f, 128f);
            Portrait(head, maid, 120f);

            var names = UiKit.Column(head, 2f, 0f);
            names.sizeDelta = new Vector2(320f, 120f);

            var name = UiKit.Label(names, maid.Name, Typeface.Heading, Palette.Text,
                TextAnchor.MiddleLeft, bold: true);
            name.rectTransform.sizeDelta = new Vector2(320f, 40f);

            var kind = UiKit.Label(names, maid.Profile.DisplayName, Typeface.Body,
                Palette.ForArchetype(maid.Archetype), TextAnchor.MiddleLeft, bold: true);
            kind.rectTransform.sizeDelta = new Vector2(320f, 28f);

            var flavour = UiKit.Label(names, maid.Profile.Flavour, Typeface.Tiny,
                Palette.TextDim, TextAnchor.UpperLeft);
            flavour.rectTransform.sizeDelta = new Vector2(320f, 46f);

            Stat(column, "roster.serve", maid.Total.Serve);
            Stat(column, "roster.cook", maid.Total.Cook);
            Stat(column, "roster.charm", maid.Total.Charm);
            Stat(column, "roster.stamina", maid.Total.Stamina);

            var bond = UiKit.Label(column,
                Strings.Get("roster.bond") + " " + maid.Bond + "/" + BondTable.MaxLevel +
                "   \"" + BondTable.LineFor(maid.Archetype, Mathf.Max(1, maid.Bond)) + "\"",
                Typeface.Tiny, Palette.TextDim, TextAnchor.UpperLeft);
            bond.rectTransform.sizeDelta = new Vector2(460f, 40f);

            var signature = BondTable.SignatureFor(maid.Archetype, maid.Bond);
            if (!string.IsNullOrEmpty(signature))
            {
                var mark = UiKit.Label(column,
                    Strings.Get("roster.signature") + ": " + signature, Typeface.Tiny,
                    Palette.Focus, TextAnchor.UpperLeft, bold: true);
                mark.rectTransform.sizeDelta = new Vector2(460f, 36f);
            }

            BuildSkills(column, maid);
            BuildGear(column, maid);
        }

        private void Stat(Transform parent, string key, int value)
        {
            var row = UiKit.Row(parent, 8f);
            row.sizeDelta = new Vector2(460f, 22f);

            var label = UiKit.Label(row, Strings.Get(key), Typeface.Small, Palette.Text,
                TextAnchor.MiddleLeft);
            label.rectTransform.sizeDelta = new Vector2(110f, 20f);

            var bar = UiKit.Bar(row, new Vector2(260f, 12f), Palette.Focus, Palette.Wall);
            UiKit.SetBar(bar, value / 100f, 260f);

            var number = UiKit.Label(row, value.ToString(), Typeface.Small, Palette.Text,
                TextAnchor.MiddleRight, bold: true);
            number.rectTransform.sizeDelta = new Vector2(54f, 20f);
        }

        private void BuildSkills(Transform parent, Maid maid)
        {
            var header = UiKit.Label(parent,
                Strings.Get("roster.skills") + "  " + maid.Skills.Count + "/" +
                SkillTable.SlotsAt(maid.Level), Typeface.Small, Palette.Text,
                TextAnchor.MiddleLeft, bold: true);
            header.rectTransform.sizeDelta = new Vector2(460f, 24f);

            var row = UiKit.Row(parent, 6f);
            row.sizeDelta = new Vector2(460f, 30f);

            foreach (var id in maid.Skills)
            {
                var def = SkillTable.ById(id);
                UiKit.Button(row, def.DisplayName, new Vector2(140f, 28f), Palette.Wall,
                    () => Root.Dialog(def.DisplayName, def.Description), Typeface.Tiny);
            }

            var available = SkillTable.AvailableTo(maid);
            if (maid.Skills.Count >= SkillTable.SlotsAt(maid.Level) || available.Count == 0) return;

            var pick = UiKit.Row(parent, 6f);
            pick.sizeDelta = new Vector2(460f, 30f);

            for (var i = 0; i < available.Count && i < 3; i++)
            {
                var def = available[i];
                UiKit.Button(pick, "+ " + def.DisplayName, new Vector2(146f, 28f), Palette.Focus,
                    () =>
                    {
                        if (!MaidProgression.EquipSkill(maid, def.Id, out var refusal))
                        {
                            Root.Dialog(def.DisplayName, refusal);
                            return;
                        }

                        Session.Touch();
                        BuildDetail();
                    }, Typeface.Tiny);
            }
        }

        private void BuildGear(Transform parent, Maid maid)
        {
            var header = UiKit.Label(parent, Strings.Get("roster.gear"), Typeface.Small,
                Palette.Text, TextAnchor.MiddleLeft, bold: true);
            header.rectTransform.sizeDelta = new Vector2(460f, 24f);

            var row = UiKit.Row(parent, 6f);
            row.sizeDelta = new Vector2(460f, 32f);

            foreach (GearSlot slot in System.Enum.GetValues(typeof(GearSlot)))
            {
                var worn = maid.Gear[(int)slot];
                var text = worn?.DisplayName ?? Strings.Get("roster.empty");

                var captured = slot;
                UiKit.Button(row, text, new Vector2(110f, 30f),
                    worn == null ? Palette.Wall : Palette.ForTheme(worn.Theme),
                    () => OfferGear(maid, captured), Typeface.Tiny);
            }

            if (maid.SetBonus.Serve <= 0) return;

            var bonus = UiKit.Label(parent,
                Strings.Get("roster.setbonus") + " +" + maid.SetBonus.Serve, Typeface.Tiny,
                Palette.Focus, TextAnchor.MiddleLeft, bold: true);
            bonus.rectTransform.sizeDelta = new Vector2(460f, 22f);
        }

        /// <summary>Offers everything in the wardrobe that fits this slot.</summary>
        private void OfferGear(Maid maid, GearSlot slot)
        {
            var owned = Session.Profile.OwnedEquipment;
            string firstFit = null;

            for (var i = 0; i < owned.Count; i++)
            {
                var item = EquipmentCatalog.ById(owned[i]);
                if (item.Slot != slot) continue;

                firstFit = owned[i];
                break;
            }

            if (firstFit == null)
            {
                Root.Dialog(Strings.Get("roster.gear"), Strings.Get("roster.empty") + " — " +
                                                        Strings.Get("shop.title"));
                return;
            }

            var chosen = EquipmentCatalog.ById(firstFit);
            MaidProgression.Equip(maid, chosen, out var removed);
            owned.Remove(firstFit);
            if (removed != null) owned.Add(removed.Id);

            Session.Touch();
            Refresh();
        }

        private void BuildHiring()
        {
            Clear(new List<GameObject>());
            foreach (Transform child in _hireList) Destroy(child.gameObject);

            _hireHeader.text = Strings.Get("hire.title") + "  " +
                               Session.Profile.Maids.Count + "/" +
                               Session.Profile.Unlocks.RosterCap;

            for (var i = 0; i < Session.Applicants.Candidates.Count; i++)
            {
                var applicant = Session.Applicants.Candidates[i];
                if (applicant == null) continue;

                var slot = i;
                var card = UiKit.Panel(_hireList, new Vector2(270f, 108f), Palette.Panel);

                Portrait(card, applicant.Maid, 64f).rectTransform.anchoredPosition =
                    new Vector2(-92f, 14f);

                var name = UiKit.Label(card,
                    applicant.Maid.Name + " · " + applicant.Maid.Profile.DisplayName,
                    Typeface.Tiny, Palette.Text, TextAnchor.MiddleLeft, bold: true);
                name.rectTransform.sizeDelta = new Vector2(170f, 20f);
                name.rectTransform.anchoredPosition = new Vector2(38f, 28f);

                var stats = UiKit.Label(card,
                    Strings.Get("roster.level") + " " + applicant.Maid.Level + " · " +
                    applicant.Maid.Total.Serve + "/" + applicant.Maid.Total.Cook + "/" +
                    applicant.Maid.Total.Charm, Typeface.Tiny, Palette.TextDim,
                    TextAnchor.MiddleLeft);
                stats.rectTransform.sizeDelta = new Vector2(170f, 18f);
                stats.rectTransform.anchoredPosition = new Vector2(38f, 8f);

                UiKit.Button(card, Strings.Get("hire.cost") + " " + applicant.Price,
                    new Vector2(250f, 30f), Palette.Focus, () =>
                    {
                        var result = Session.Applicants.Hire(Session.Profile, slot);
                        if (!result.Ok)
                        {
                            Root.Dialog(Strings.Get("hire.title"), result.Refusal);
                            return;
                        }

                        Session.Touch();
                        Refresh();
                    }, Typeface.Tiny).GetComponent<RectTransform>().anchoredPosition =
                    new Vector2(0f, -36f);
            }
        }

        private void Clear(List<GameObject> list)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i] != null) Destroy(list[i]);
            }

            list.Clear();
        }
    }
}
