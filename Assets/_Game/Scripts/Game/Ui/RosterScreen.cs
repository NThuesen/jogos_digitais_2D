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
    ///
    /// UI-INT-02A draws it from the Maids kit (<c>Resources/UiKit/maids_*</c>) and adds two
    /// independent, combinable filters (rarity; natural position), an order and pages. If a kit
    /// sprite is missing the screen falls back to the earlier procedural layout, unchanged.
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

        // ---- The kit layout ----------------------------------------------------------------
        // Everything below is placed on a 1280x720 stage in the coordinates of the approved
        // mockup (1672x941, scale 0.7655). Names, numbers, counts and every piece of a maid's
        // data are Unity Text over blank plates; none is in a texture.

        private const float S = 1280f / 1672f;

        private static readonly string[] KitSprites =
        {
            "maids_background", "maids_title_plate_empty", "maids_token_counter_empty",
            "maids_filters_container_empty", "maids_filter_row_divider", "maids_filter_selected_empty",
            "maids_filter_selected_surface", "maids_filter_rarity_normal_empty",
            "maids_filter_position_normal_empty", "maids_filter_paw_brown", "maids_icon_reception",
            "maids_icon_hall", "maids_icon_kitchen", "maids_sort_empty", "maids_sort_divider",
            "maids_collection_panel_empty", "maids_selected_card_highlight", "maids_page_left",
            "maids_page_right", "maids_page_plate_empty", "maids_detail_panel_empty",
            "maids_portrait_frame_empty", "maids_rarity_badge_empty", "maids_attribute_track_empty",
            "maids_attribute_fill_gold", "maids_traits_frame_empty", "maids_dismiss_empty",
            "back_blank", "bow_gold", "paw_gold",
        };

        private static readonly Color Purple = Palette.Hex("#7A3AA8");
        private static readonly Color Orange = Palette.Hex("#C87A12");
        private static readonly Color Rose = Palette.Hex("#D9366A");
        private static readonly Color SoftBrown = new Color(0.43f, 0.3f, 0.2f);

        private bool _kit;

        /// <summary>Asks for the earlier procedural layout even when the kit is present (tests, tools).</summary>
        public bool ForcePlain;

        // View state: presentation only, never saved.
        private Rarity? _rarity;
        private Role? _role;
        private RosterSort _sort = RosterSort.Rarity;
        private int _page;

        private RectTransform _filters;
        private RectTransform _cells;
        private RectTransform _detailLayer;
        private Text _info;
        private Text _tokens;
        private Text _pageLabel;
        private Text _shown;
        private Text _sortLabel;
        private Text _empty;
        private Button _pagePrev;
        private Button _pageNext;
        private readonly Dictionary<int, GameObject> _highlights = new Dictionary<int, GameObject>();

        /// <summary>The current filters, for tests: null means "all".</summary>
        public Rarity? RarityFilter => _rarity;

        public Role? RoleFilter => _role;

        public RosterSort Sort => _sort;

        public int Page => _page;

        public int SelectedId => _selected;

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

        public override void Refresh()
        {
            if (Session == null) return;

            if (_kit) RefreshKit();
            else RefreshPlain();
        }

        // ========================================================================================
        // The illustrated screen
        // ========================================================================================

        private void BuildKit()
        {
            KitArt.Backdrop(transform, "maids_background");

            var stage = new GameObject("Stage", typeof(RectTransform), typeof(FitStage)).GetComponent<RectTransform>();
            stage.SetParent(transform, false);
            stage.anchorMin = new Vector2(0.5f, 1f);
            stage.anchorMax = new Vector2(0.5f, 1f);
            stage.pivot = new Vector2(0.5f, 1f);
            stage.anchoredPosition = Vector2.zero;
            stage.sizeDelta = new Vector2(1280f, 720f);

            if (UiArt.Has("ui020_logo_wordmark"))
            {
                var logo = UiArt.Picture(stage, "ui020_logo_wordmark", new Vector2(176f, 86f));
                UiKit.Anchor(logo.rectTransform, new Vector2(0f, 1f), new Vector2(12f, -9f));
            }

            // Title.
            var title = At(stage, "maids_title_plate_empty", 587, 58, 1044, 140, "TitlePlate");
            KitArt.Fitted(title.transform, Strings.Get("roster.maids"), 38, 18, KitArt.Brown,
                Inset(170, 31, 303, 79, 473, 98, 473, 98));
            var bow = KitArt.Picture(stage, "bow_gold", 118f);
            bow.gameObject.name = "Bow";
            UiKit.Anchor(bow.rectTransform, new Vector2(0.5f, 1f), new Vector2((815.5f - 836f) * S, -22f));

            // Fichas.
            var pill = At(stage, "maids_token_counter_empty", 1124, 71, 1399, 120, "TokenPlate");
            _tokens = KitArt.Fitted(pill.transform, "", 24, 12, Purple, Inset(88, 17, 233, 46, 291, 65, 291, 65));
            _tokens.gameObject.name = "Tokens";

            // The roster count and the daily fixed cost: the two facts the earlier screen carried.
            _info = KitArt.Fitted(Slot(stage, "RosterInfo", 262, 46, 580, 124), "", 16, 10, KitArt.Cream,
                Vector4.zero, TextAnchor.MiddleLeft, outline: Palette.WithAlpha(KitArt.Brown, 0.9f));

            var back = KitArt.Button(stage, "back_blank", Strings.Get("dialog.back"), new Vector2(150f, 64f),
                () => OnBack?.Invoke(), 22, 12, KitArt.Brown, KitArt.Margins("back_blank", 86, 27, 90, 35));
            back.name = "Back";
            UiKit.Anchor(back.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(-16f, -14f));

            // The filter board: two rows and the order.
            At(stage, "maids_filters_container_empty", 149, 139, 1516, 274, "FiltersPlate");
            At(stage, "maids_filter_row_divider", 177, 202, 1140, 209, "RowDivider");
            At(stage, "maids_sort_divider", 1183, 156, 1190, 254, "SortDivider");

            var sort = At(stage, "maids_sort_empty", 1208, 175, 1490, 234, "Sort");
            sort.raycastTarget = true;
            var sortButton = sort.gameObject.AddComponent<Button>();
            StyleButton(sortButton, sort);
            sortButton.onClick.AddListener(() =>
            {
                _sort = RosterQuery.Next(_sort);
                _page = 0;
                RefreshKit();
            });
            _sortLabel = KitArt.Fitted(sort.transform, "", 19, 10, KitArt.Brown,
                Inset(37, 20, 226, 52, 298, 75, 298, 75));
            _sortLabel.gameObject.name = "Label";

            _filters = Layer(stage, "Filters");

            // The collection.
            At(stage, "maids_collection_panel_empty", 158, 263, 926, 879, "CollectionPlate");
            _cells = Layer(stage, "Cells");

            _empty = KitArt.Fitted(Slot(stage, "CollectionEmpty", 230, 480, 860, 600), "", 22, 12, KitArt.Cream,
                Vector4.zero);
            _empty.gameObject.SetActive(false);

            _shown = KitArt.Fitted(Slot(stage, "Shown", 190, 812, 410, 852), "", 15, 9, KitArt.Cream, Vector4.zero,
                TextAnchor.MiddleLeft);

            _pagePrev = PageButton(stage, "PagePrev", "maids_page_left", 415, 801, 478, 861, () => GoToPage(_page - 1));
            _pageNext = PageButton(stage, "PageNext", "maids_page_right", 609, 801, 674, 861, () => GoToPage(_page + 1));
            var plate = At(stage, "maids_page_plate_empty", 480, 811, 604, 853, "PagePlate");
            _pageLabel = KitArt.Fitted(plate.transform, "", 22, 11, KitArt.Brown, Inset(29, 14, 111, 41, 140, 58, 140, 58));
            _pageLabel.gameObject.name = "Label";

            // The detail.
            At(stage, "maids_detail_panel_empty", 916, 265, 1519, 875, "DetailPlate");
            _detailLayer = Layer(stage, "Detail");
        }

        /// <summary>
        /// How many she has collected (of the identities actually released: planned ones are not obtainable
        /// yet) and the daily fixed cost, which is the fielded team's wages plus rent. The collection has no
        /// cap, and a maid who is not fielded costs nothing to keep.
        /// </summary>
        public static string CollectionText(Sim.Save.Profile profile) =>
            Strings.Get("roster.maids") + " " + profile.Maids.Count + "   ·   " + Strings.Get("roster.collection") + " " +
            CollectedCount(profile) + "/" + MaidCatalog.Released.Count + "\n" +
            Strings.Get("hub.fixedcosts") + " " + Strings.Money(Books.FixedCostsFor(profile));

        /// <summary>Released identities she owns.</summary>
        public static int CollectedCount(Sim.Save.Profile profile)
        {
            var owned = 0;
            foreach (var identity in MaidCatalog.Released)
            {
                if (profile.HasIdentity(identity.Id)) owned++;
            }

            return owned;
        }

        /// <summary>Her wage is only paid on the days she is fielded; the label says which it is today.</summary>
        private string SalaryKey(Maid maid) =>
            Session.Profile.Lineup.Contains(maid.Id) ? "roster.salary.paid" : "roster.salary.idle";

        private void RefreshKit()
        {
            var profile = Session.Profile;
            var all = profile.Maids;

            _tokens.text = profile.Wallet.Points + " " + Strings.Get("hub.points");
            _info.text = CollectionText(profile);
            _sortLabel.text = Strings.Get("roster.sort") + ": " + SortName(_sort);

            var visible = RosterQuery.Visible(all, _rarity, _role, _sort);
            _page = RosterQuery.ClampPage(_page, visible.Count);

            // With nothing selected yet (a fresh visit, or she was just dismissed) the screen reads
            // the first maid of the roster, as it always has, when the filters show her.
            if (_selected < 0 && all.Count > 0 && RosterQuery.Page(visible, _page).Exists(m => m.Id == all[0].Id))
            {
                _selected = all[0].Id;
            }

            _selected = RosterQuery.Resolve(_selected, visible);

            BuildFilters(all);
            BuildCells(visible);
            BuildDetail();
        }

        private void BuildFilters(IList<Maid> all)
        {
            Discard(_filters);

            // Row 1: rarity. Row 2: natural position, all five.
            var x = 205f;
            AddRarity(null, Strings.Get("roster.f.all"), RosterQuery.Count(all, (Rarity?)null), ref x);
            for (var i = 0; i < RosterQuery.Rarities.Length; i++)
            {
                var rarity = RosterQuery.Rarities[i];
                AddRarity(rarity, Strings.Get("roster.f." + rarity.ToString().ToLowerInvariant()),
                    RosterQuery.Count(all, (Rarity?)rarity), ref x);
            }

            x = 239f;
            AddPosition(null, Strings.Get("roster.f.allroles"), 150f, ref x);
            for (var i = 0; i < RosterQuery.Roles.Length; i++)
            {
                var role = RosterQuery.Roles[i];
                AddPosition(role, RoleTable.For(role).DisplayName, 134f, ref x);
            }
        }

        private void AddRarity(Rarity? rarity, string name, int count, ref float x)
        {
            var active = _rarity == rarity;
            var sprite = active ? "maids_filter_selected_empty" : "maids_filter_rarity_normal_empty";
            var width = active ? 178f : 175f;
            var slot = At(_filters, sprite, x, 150, x + width, 205, "Filter_Rarity_" + (rarity.HasValue ? rarity.ToString() : "All"));
            x += 175f + 8f;

            slot.raycastTarget = true;
            var button = slot.gameObject.AddComponent<Button>();
            StyleButton(button, slot);
            var chosen = rarity;
            button.onClick.AddListener(() => ChooseRarity(chosen));

            // The sprites already carry the paw: the label sits in the content rect of the plate.
            var label = KitArt.Fitted(slot.transform, name + " (" + count + ")", 17, 8, KitArt.Brown,
                Inset(63, 21, 163, 52, 191, 71, slot.rectTransform.sizeDelta.x / S, 71));
            label.gameObject.name = "Label";
        }

        private void AddPosition(Role? role, string name, float width, ref float x)
        {
            var active = _role == role;
            var sprite = active ? "maids_filter_selected_surface" : "maids_filter_position_normal_empty";
            var slot = At(_filters, sprite, x, 209, x + width, 259, "Filter_Role_" + (role.HasValue ? role.ToString() : "All"));
            x += width + 8f;

            slot.raycastTarget = true;
            var button = slot.gameObject.AddComponent<Button>();
            StyleButton(button, slot);
            var chosen = role;
            button.onClick.AddListener(() => ChooseRole(chosen));

            var icon = KitArt.Picture(slot.transform, IconFor(role), 26f);
            icon.gameObject.name = "Icon";
            UiKit.Anchor(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(11f, 1f));
            icon.rectTransform.pivot = new Vector2(0f, 0.5f);

            var wide = slot.rectTransform.sizeDelta.x / S;
            var label = KitArt.Fitted(slot.transform, name, 17, 8, KitArt.Brown,
                new Vector4(46f / wide, 0.24f, 14f / wide, 0.24f));
            label.gameObject.name = "Label";
        }

        private static string IconFor(Role? role)
        {
            if (!role.HasValue) return "maids_filter_paw_brown";

            switch (role.Value)
            {
                case Role.Reception: return "maids_icon_reception";
                case Role.Floor: return "maids_icon_hall";
                case Role.Kitchen: return "maids_icon_kitchen";
                default: return "maids_filter_paw_brown";
            }
        }

        private void ChooseRarity(Rarity? rarity)
        {
            _rarity = rarity;
            _page = 0;
            RefreshKit();
        }

        private void ChooseRole(Role? role)
        {
            _role = role;
            _page = 0;
            RefreshKit();
        }

        private void GoToPage(int page)
        {
            var visible = RosterQuery.Visible(Session.Profile.Maids, _rarity, _role, _sort);
            _page = RosterQuery.ClampPage(page, visible.Count);
            BuildCells(visible);
        }

        /// <summary>The cards of the current page: four columns by two rows.</summary>
        private void BuildCells(List<Maid> visible)
        {
            Discard(_cells);
            _highlights.Clear();

            var profile = Session.Profile;
            var pages = RosterQuery.Pages(visible.Count);
            var page = RosterQuery.Page(visible, _page);

            _pageLabel.text = (_page + 1) + " / " + pages;
            _pagePrev.interactable = _page > 0;
            _pageNext.interactable = _page < pages - 1;
            _shown.text = Strings.Get("roster.shown") + " " + visible.Count + " / " + profile.Maids.Count;

            _empty.gameObject.SetActive(visible.Count == 0);
            if (visible.Count == 0)
            {
                _empty.text = Strings.Get(profile.Maids.Count == 0 ? "roster.noroster" : "roster.nomatch");
            }

            const float cellW = 136f;
            const float cellH = 186f;
            var left = 178f * S;
            var top = 282f * S + 4f;

            for (var i = 0; i < page.Count; i++)
            {
                var maid = page[i];
                var cell = new GameObject("Cell " + maid.Name, typeof(RectTransform)).GetComponent<RectTransform>();
                cell.SetParent(_cells, false);
                UiKit.Anchor(cell, new Vector2(0f, 1f),
                    new Vector2(left + (i % 4) * (cellW + 4f), -(top + (i / 4) * (cellH + 12f))));
                cell.sizeDelta = new Vector2(cellW, cellH);

                var highlight = KitArt.Plate(cell, "maids_selected_card_highlight", new Vector2(cellW + 6f, cellH + 6f));
                highlight.gameObject.name = "Highlight";
                UiKit.Anchor(highlight.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero);
                highlight.gameObject.SetActive(maid.Id == _selected);
                _highlights[maid.Id] = highlight.gameObject;

                var card = MaidCard.Build(cell, maid, 0.82f, showSalary: true,
                    playingAs: profile.Lineup.RoleOf(maid.Id), exact: true);
                UiKit.Anchor(card, new Vector2(0.5f, 0.5f), Vector2.zero);
                card.GetComponent<Image>().raycastTarget = true;

                var button = card.gameObject.AddComponent<Button>();
                button.targetGraphic = card.GetComponent<Image>();

                var captured = maid.Id;
                button.onClick.AddListener(() => Select(captured));
            }
        }

        private void Select(int id)
        {
            _selected = id;
            foreach (var pair in _highlights)
            {
                if (pair.Value != null) pair.Value.SetActive(pair.Key == id);
            }

            BuildDetail();
        }

        private void BuildDetail()
        {
            Discard(_detailLayer);

            var maid = Session.Profile.MaidById(_selected);
            if (maid == null)
            {
                var none = KitArt.Fitted(Slot(_detailLayer, "DetailEmpty", 990, 460, 1450, 600), Strings.Get(
                    Session.Profile.Maids.Count == 0 ? "roster.noroster" : "roster.pick"), 22, 12, KitArt.Brown, Vector4.zero);
                none.gameObject.name = "Label";
                return;
            }

            var layer = _detailLayer;

            // Portrait: the 3:4 premium picture, top-aligned inside the frame's window; a maid
            // without one keeps the compact card.
            var frame = At(layer, "maids_portrait_frame_empty", 954, 294, 1176, 527, "PortraitFrame");
            var portrait = MaidPortraits.Detail(maid);
            var window = Slot(frame.transform, portrait != null ? "Detail Portrait " + maid.Name : "PortraitWindow", 0, 0, 1, 1);
            window.anchorMin = new Vector2(15f / 238f, 1f - 234f / 249f);
            window.anchorMax = new Vector2(223f / 238f, 1f - 15f / 249f);
            window.offsetMin = Vector2.zero;
            window.offsetMax = Vector2.zero;
            window.gameObject.AddComponent<RectMask2D>();

            var wash = new GameObject("Wash", typeof(RectTransform)).AddComponent<Image>();
            wash.transform.SetParent(window, false);
            wash.color = Palette.WithAlpha(Palette.ForArchetype(maid.Archetype), 0.55f);
            wash.raycastTarget = false;
            UiKit.Stretch(wash.rectTransform);

            if (portrait != null)
            {
                var picture = new GameObject("Portrait", typeof(RectTransform)).AddComponent<Image>();
                picture.transform.SetParent(window, false);
                picture.sprite = portrait;
                picture.preserveAspect = true;
                picture.raycastTarget = false;

                var rect = picture.rectTransform;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = Vector2.zero;
                var aspect = portrait.rect.height / portrait.rect.width;
                rect.sizeDelta = new Vector2(0f, 0f);
                var fitter = picture.gameObject.AddComponent<AspectRatioFitter>();
                fitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
                fitter.aspectRatio = 1f / aspect;
            }
            else
            {
                var card = MaidCard.Build(window, maid, 0.72f, showSalary: false,
                    playingAs: Session.Profile.Lineup.RoleOf(maid.Id), exact: true);
                UiKit.Anchor(card, new Vector2(0.5f, 0.5f), Vector2.zero);
            }

            // Name and rarity.
            var name = KitArt.Fitted(Slot(layer, "Name", 1195, 297, 1336, 340), maid.Name, 26, 12, KitArt.Brown,
                Vector4.zero, TextAnchor.MiddleLeft);

            var badge = At(layer, "maids_rarity_badge_empty", 1336, 299, 1456, 338, "RarityBadge");
            var rarityLabel = KitArt.Fitted(badge.transform, maid.Card.DisplayName, 16, 8, KitArt.Brown,
                Inset(27, 12, 111, 39, 136, 55, 136, 55));
            rarityLabel.gameObject.name = "Label";

            // The facts.
            Fact(layer, "lineup.natural", RoleTable.For(maid.NaturalRole).DisplayName, 342, 369, 16, Orange);
            Fact(layer, SalaryKey(maid), Strings.Money(maid.Salary), 370, 395, 17, Rose);
            Fact(layer, "roster.level", maid.Level + " / " + Maid.MaxLevel, 397, 422, 17, KitArt.Brown);
            Fact(layer, "roster.tags", string.Join(" · ", ChemistryTags.Describe(maid)), 424, 478, 15, KitArt.Brown);
            Fact(layer, "roster.gear", WornText(maid), 481, 528, 15, KitArt.Brown);

            // The four attributes.
            Stat(layer, "roster.serve", maid.Total.Serve, 0);
            Stat(layer, "roster.cook", maid.Total.Cook, 1);
            Stat(layer, "roster.charm", maid.Total.Charm, 2);
            Stat(layer, "roster.stamina", maid.Total.Stamina, 3);

            // Traits, her description and her signature: the three that can run long, so they
            // scroll inside their frame rather than being cut.
            At(layer, "maids_traits_frame_empty", 939, 643, 1498, 750, "TraitsFrame");

            var paw = KitArt.Picture(layer, "paw_gold", 28f * S);
            paw.gameObject.name = "TraitsPaw";
            UiKit.Anchor(paw.rectTransform, new Vector2(0f, 1f), new Vector2(964f * S, -648f * S));
            var heading = KitArt.Fitted(Slot(layer, "TraitsTitle", 1000, 644, 1130, 667), Strings.Get("roster.traits"), 15, 9,
                Orange, Vector4.zero, TextAnchor.MiddleLeft);
            heading.gameObject.name = "Label";

            TraitsBody(layer, maid);

            // Dismissal. Equip and level-up have no action in the game, so they have no button.
            var dismiss = KitArt.Button(layer, "maids_dismiss_empty",
                Strings.Get("roster.dismiss") + " (+" + maid.Severance + ")",
                new Vector2((190f + 16f) * S, (79f + 16f) * S), () => AskDismiss(maid), 20, 10, Palette.Hex("#7A1F3D"),
                Inset(47, 20, 164, 76, 206, 95, 206, 95));
            dismiss.name = "Dismiss";
            TrimHitArea((Image)dismiss.targetGraphic);
            UiKit.Anchor(dismiss.GetComponent<RectTransform>(), new Vector2(0f, 1f),
                new Vector2((1217.5f - 103f) * S, -(761f - 8f) * S));
        }

        private void AskDismiss(Maid maid)
        {
            Root.Dialog(maid.Name, Strings.Get("roster.dismissask"), Strings.Get("dialog.yes"),
                () =>
                {
                    Session.Profile.Dismiss(maid.Id);
                    _selected = -1;
                    Session.Touch();
                    Refresh();
                }, Strings.Get("dialog.no"));
        }

        private void Fact(RectTransform layer, string key, string value, float y0, float y1, int max, Color? colour)
        {
            var label = KitArt.Fitted(Slot(layer, "FactLabel_" + key, 1200, y0, 1346, y1), Strings.Get(key), 15, 8,
                SoftBrown, Vector4.zero, TextAnchor.UpperLeft);
            label.gameObject.name = "Label";
            label.fontStyle = FontStyle.Normal;

            var text = KitArt.Fitted(Slot(layer, "FactValue_" + key, 1348, y0, 1506, y1), value, max, 8, colour ?? KitArt.Brown,
                Vector4.zero, TextAnchor.UpperLeft);
            text.gameObject.name = "Value";
        }

        private static void Stat(RectTransform layer, string key, int value, int row)
        {
            var cy = 542.5f + 28f * row;

            var label = KitArt.Fitted(Slot(layer, "StatLabel_" + key, 967, cy - 12, 1093, cy + 12), Strings.Get(key), 17, 9,
                KitArt.Brown, Vector4.zero, TextAnchor.MiddleLeft);
            label.gameObject.name = "Label";
            label.fontStyle = FontStyle.Normal;

            At(layer, "maids_attribute_track_empty", 1097, cy - 10.5f, 1425, cy + 10.5f, "Track_" + key);

            var fraction = Mathf.Clamp01(value / 100f);
            var fillWidth = Mathf.Max(18f, 320f * fraction);
            At(layer, "maids_attribute_fill_gold", 1101, cy - 5.5f, 1101 + fillWidth, cy + 9.5f, "Fill_" + key);

            var number = KitArt.Fitted(Slot(layer, "StatValue_" + key, 1436, cy - 12, 1494, cy + 12), value.ToString(), 19, 9,
                KitArt.Brown, Vector4.zero, TextAnchor.MiddleRight);
            number.gameObject.name = "Value";
        }

        private void TraitsBody(RectTransform layer, Maid maid)
        {
            var view = Slot(layer, "TraitsScroll", 953, 672, 1482, 732);
            view.gameObject.AddComponent<RectMask2D>();

            // A clear sibling under the text takes the drag, so the labels themselves sit on no image.
            var catcher = new GameObject("Catcher", typeof(RectTransform)).AddComponent<Image>();
            catcher.transform.SetParent(view, false);
            catcher.color = new Color(1f, 1f, 1f, 0f);
            catcher.raycastTarget = true;
            UiKit.Stretch(catcher.rectTransform);

            var scroll = view.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 24f;

            var content = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
            content.SetParent(view, false);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = Vector2.zero;

            var group = content.gameObject.AddComponent<VerticalLayoutGroup>();
            group.spacing = 2f;
            group.childAlignment = TextAnchor.UpperLeft;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = false;

            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = view;
            scroll.content = content;

            if (maid.Traits.Count == 0)
            {
                TraitLine(content, Strings.Get("roster.notraits"), KitArt.Brown, false);
            }
            else
            {
                for (var i = 0; i < maid.Traits.Count; i++)
                {
                    var def = TraitTable.ById(maid.Traits[i]);
                    TraitLine(content, def.DisplayName + " — " + def.Description, KitArt.Brown, false);
                }
            }

            TraitLine(content, maid.Profile.Flavour, SoftBrown, false);

            var signature = BondTable.SignatureFor(maid.Archetype, maid.Bond);
            if (!string.IsNullOrEmpty(signature))
            {
                TraitLine(content, Strings.Get("roster.signature") + ": " + signature, Orange, true);
            }
        }

        private static void TraitLine(Transform parent, string text, Color colour, bool bold)
        {
            var line = UiKit.Label(parent, text, 14, colour, TextAnchor.UpperLeft, bold);
            line.gameObject.name = "TraitLine";
        }

        // ---- kit helpers -------------------------------------------------------------------

        /// <summary>
        /// A kit sprite placed where the mockup crop (x0,y0)-(x1,y1) was, in mockup pixels: the
        /// sprite is its crop plus the kit's 8 px of transparent padding on every side.
        /// </summary>
        private static Image At(Transform parent, string sprite, float x0, float y0, float x1, float y1, string name = null)
        {
            var image = KitArt.Plate(parent, sprite, new Vector2((x1 - x0 + 16f) * S, (y1 - y0 + 16f) * S));
            image.gameObject.name = name ?? sprite;
            UiKit.Anchor(image.rectTransform, new Vector2(0f, 1f), new Vector2((x0 - 8f) * S, -(y0 - 8f) * S));
            return image;
        }

        /// <summary>An empty rect over mockup pixels (x0,y0)-(x1,y1), for text.</summary>
        private static RectTransform Slot(Transform parent, string name, float x0, float y0, float x1, float y1)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            UiKit.Anchor(rect, new Vector2(0f, 1f), new Vector2(x0 * S, -y0 * S));
            rect.sizeDelta = new Vector2((x1 - x0) * S, (y1 - y0) * S);
            return rect;
        }

        private static RectTransform Layer(Transform parent, string name)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            UiKit.Stretch(rect);
            return rect;
        }

        /// <summary>
        /// The margins (left, bottom, right, top, as fractions) of a content rect given in native
        /// sprite pixels, for a plate drawn <paramref name="actualW"/> x <paramref name="actualH"/>
        /// of the sprite's own pixels (a plate stretched wider keeps its fixed borders, so the
        /// content rect moves with the edges).
        /// </summary>
        private static Vector4 Inset(float cx0, float cy0, float cx1, float cy1, float nativeW, float nativeH,
            float actualW, float actualH) =>
            new Vector4(cx0 / actualW, (nativeH - cy1) / actualH, (nativeW - cx1) / actualW, cy0 / actualH);

        /// <summary>
        /// The kit's plates carry 8 px of transparent padding, which overlaps the neighbours (the two
        /// filter rows overlap by a few pixels): the click area is the drawn plate, not its padding.
        /// </summary>
        private static void TrimHitArea(Image face)
        {
            var pad = 8f * S;
            face.raycastPadding = new Vector4(pad, pad, pad, pad);
        }

        private static void StyleButton(Button button, Image face)
        {
            button.targetGraphic = face;
            button.transition = Selectable.Transition.ColorTint;

            TrimHitArea(face);

            var colours = button.colors;
            colours.normalColor = Color.white;
            colours.highlightedColor = new Color(1.08f, 1.06f, 1.04f, 1f);
            colours.pressedColor = new Color(0.82f, 0.78f, 0.74f, 1f);
            colours.selectedColor = Color.white;
            colours.disabledColor = new Color(0.62f, 0.6f, 0.6f, 0.9f);
            colours.fadeDuration = 0.07f;
            button.colors = colours;
        }

        private static Button PageButton(Transform parent, string name, string sprite, float x0, float y0, float x1, float y1,
            Action onClick)
        {
            var face = At(parent, sprite, x0, y0, x1, y1, name);
            face.raycastTarget = true;
            var button = face.gameObject.AddComponent<Button>();
            StyleButton(button, face);
            button.onClick.AddListener(() => onClick());
            return button;
        }

        private static string SortName(RosterSort sort)
        {
            switch (sort)
            {
                case RosterSort.Name: return Strings.Get("roster.sort.name");
                case RosterSort.Level: return Strings.Get("roster.sort.level");
                default: return Strings.Get("roster.sort.rarity");
            }
        }

        /// <summary>Removes the children now (a click may be what asked), and destroys them at frame end.</summary>
        private static void Discard(Transform layer)
        {
            for (var i = layer.childCount - 1; i >= 0; i--)
            {
                var child = layer.GetChild(i);
                child.gameObject.SetActive(false);
                child.SetParent(null, false);
                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// Keeps the 1280x720 stage inside a canvas that is narrower than 16:9: scaled down about
        /// its top edge, so the art of the backdrop (which only crops at the sides) stays under it.
        /// </summary>
        public sealed class FitStage : MonoBehaviour
        {
            private void OnEnable() => Apply();

            private void LateUpdate() => Apply();

            private void Apply()
            {
                var parent = transform.parent as RectTransform;
                if (parent == null) return;

                var rect = parent.rect;
                if (rect.width <= 0f || rect.height <= 0f) return;

                var scale = Mathf.Min(1f, Mathf.Min(rect.width / 1280f, rect.height / 720f));
                transform.localScale = new Vector3(scale, scale, 1f);
            }
        }

        // ========================================================================================
        // The earlier procedural screen: the fallback when a kit sprite is missing
        // ========================================================================================

        private void BuildPlain()
        {
            Scenery.Build(transform, withFloor: false);

            var title = UiKit.TrackedLabel(transform, "ELENCO", Typeface.Heading, Palette.Cream, 6f);
            UiKit.Anchor(title, new Vector2(0f, 1f), new Vector2(26f, -18f));

            _header = UiKit.Label(transform, "", Typeface.Body, Logo.Brass,
                TextAnchor.MiddleRight, bold: true);
            UiKit.Anchor(_header.rectTransform, new Vector2(1f, 1f), new Vector2(-170f, -26f));
            _header.rectTransform.sizeDelta = new Vector2(460f, 32f);

            // The back control is the approved secondary button (UI-005), as on the shop.
            var back = UiArt.StateButton(transform, Strings.Get("dialog.back"), new Vector2(120f, 44f),
                () => OnBack?.Invoke(), "btn2", Typeface.Small);
            UiKit.Anchor(back.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(-26f, -18f));

            // 600 wide: the detail plaque starts 630 from the left at the 960x600 canvas (1152 wide),
            // and the grid ends at 622. At 700 it ran 92 units under the plaque.
            _grid = UiKit.ScrollList(transform, new Vector2(600f, 560f), 10f, exact: true);
            UiKit.Anchor((RectTransform)_grid.parent, new Vector2(0f, 0f), new Vector2(22f, 20f));

            var plaque = Scenery.Plaque(transform, new Vector2(500f, 560f), null, exact: true);
            UiKit.Anchor((RectTransform)plaque.parent, new Vector2(1f, 0f), new Vector2(-22f, 20f));
            _detail = plaque;
        }

        private void RefreshPlain()
        {
            Clear();

            var profile = Session.Profile;
            _header.text = CollectionText(profile);

            if (_selected < 0 && profile.Maids.Count > 0) _selected = profile.Maids[0].Id;

            for (var i = 0; i < profile.Maids.Count; i += 4) BuildRow(profile, i);

            BuildDetailPlain();
        }

        private void BuildRow(Sim.Save.Profile profile, int start)
        {
            var row = UiKit.Row(_grid, 8f);
            row.sizeDelta = new Vector2(580f, 180f);
            _built.Add(row.gameObject);

            for (var i = start; i < start + 4 && i < profile.Maids.Count; i++)
            {
                var maid = profile.Maids[i];
                var playing = profile.Lineup.RoleOf(maid.Id);

                var card = MaidCard.Build(row, maid, 0.82f, showSalary: true, playingAs: playing, exact: true);
                card.GetComponent<Image>().raycastTarget = true;

                var button = card.gameObject.AddComponent<Button>();
                button.targetGraphic = card.GetComponent<Image>();

                var captured = maid.Id;
                button.onClick.AddListener(() =>
                {
                    _selected = captured;
                    BuildDetailPlain();
                });
            }
        }

        private void BuildDetailPlain()
        {
            foreach (Transform child in _detail) Destroy(child.gameObject);

            var maid = Session.Profile.MaidById(_selected);
            if (maid == null) return;

            var column = UiKit.Column(_detail, 7f, 16f);
            UiKit.Stretch(column);

            // With a premium portrait the head row shows her large (3:4) beside the facts; a maid
            // without one keeps the compact card.
            var portrait = MaidPortraits.Detail(maid);
            var head = UiKit.Row(column, portrait != null ? 10f : 12f);
            head.sizeDelta = new Vector2(460f, DetailPortraitHeight);

            if (portrait != null)
            {
                DetailPortrait(head, maid, portrait);
            }
            else
            {
                MaidCard.Build(head, maid, 0.72f, showSalary: false,
                    playingAs: Session.Profile.Lineup.RoleOf(maid.Id), exact: true);
            }

            var facts = UiKit.Column(head, 3f, 0f);
            facts.sizeDelta = new Vector2(portrait != null ? 300f : 310f, DetailPortraitHeight - 4f);

            PlainFact(facts, "roster.rarity", maid.Card.DisplayName, Palette.Hex(maid.Card.TintHex));
            PlainFact(facts, "lineup.natural", RoleTable.For(maid.NaturalRole).DisplayName, Logo.Brass);
            PlainFact(facts, SalaryKey(maid), Strings.Money(maid.Salary), Palette.Alarm);
            PlainFact(facts, "roster.level", maid.Level + " / " + Maid.MaxLevel, Palette.Text);
            PlainFact(facts, "roster.tags", string.Join(" · ", ChemistryTags.Describe(maid)),
                Palette.TextDim, 2);
            PlainFact(facts, "roster.gear", WornText(maid), Palette.TextDim, 2);

            PlainStat(column, "roster.serve", maid.Total.Serve);
            PlainStat(column, "roster.cook", maid.Total.Cook);
            PlainStat(column, "roster.charm", maid.Total.Charm);
            PlainStat(column, "roster.stamina", maid.Total.Stamina);

            PlainTraits(column, maid);

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

        private const float DetailPortraitHeight = 200f;
        private const float DetailPortraitWidth = 150f;

        /// <summary>
        /// The large premium portrait in a frame of her rarity colour over a wash of her accent.
        /// The picture keeps its 3:4 proportion and never takes clicks.
        /// </summary>
        private static void DetailPortrait(Transform parent, Maid maid, Sprite sprite)
        {
            var frame = UiKit.Panel(parent, new Vector2(DetailPortraitWidth, DetailPortraitHeight),
                Palette.Hex(maid.Card.TintHex), 0.08f, true);
            frame.gameObject.name = "Detail Portrait " + maid.Name;
            frame.GetComponent<Image>().raycastTarget = false;

            var inner = UiKit.Panel(frame, new Vector2(DetailPortraitWidth - 6f, DetailPortraitHeight - 6f),
                Palette.WithAlpha(Palette.ForArchetype(maid.Archetype), 0.55f), 0.06f, true);
            UiKit.Anchor(inner, new Vector2(0.5f, 0.5f), Vector2.zero);
            inner.GetComponent<Image>().raycastTarget = false;

            var picture = new GameObject("Portrait", typeof(RectTransform)).AddComponent<Image>();
            picture.transform.SetParent(inner, false);
            picture.sprite = sprite;
            picture.preserveAspect = true;
            picture.raycastTarget = false;
            UiKit.Stretch(picture.rectTransform);
        }

        /// <summary>
        /// What she wears. Equipment still counts for stats and set bonuses; it is shown here, in the
        /// interface, rather than as layers over her illustrated figure.
        /// </summary>
        private static string WornText(Maid maid)
        {
            var worn = new List<string>();
            for (var i = 0; i < maid.Gear.Length; i++)
            {
                if (maid.Gear[i] != null) worn.Add(maid.Gear[i].DisplayName);
            }

            return worn.Count > 0 ? string.Join(" · ", worn) : Strings.Get("roster.empty");
        }

        private static void PlainFact(Transform parent, string key, string value, Color colour,
            int lines = 1)
        {
            var row = UiKit.Row(parent, 6f);
            row.sizeDelta = new Vector2(300f, 22f * lines);

            var label = UiKit.Label(row, Strings.Get(key), Typeface.Tiny, Palette.TextDim,
                TextAnchor.MiddleLeft);
            label.rectTransform.sizeDelta = new Vector2(120f, 20f);

            var text = UiKit.Label(row, value, Typeface.Small, colour, TextAnchor.MiddleLeft,
                bold: true);
            text.rectTransform.sizeDelta = new Vector2(176f, 22f * lines - 2f);
            if (lines > 1) text.alignment = TextAnchor.MiddleLeft;
        }

        private static void PlainStat(Transform parent, string key, int value)
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

        private void PlainTraits(Transform parent, Maid maid)
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
