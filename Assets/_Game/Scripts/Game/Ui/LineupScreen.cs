using System;
using System.Collections.Generic;
using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Game.Feel;
using Neko.Sim.Economy;
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
            if (!ForcePlain && KitArt.Has(KitSprites))
            {
                _kit = true;
                BuildKit();
                return;
            }

            BuildPlain();
        }

        private void BuildPlain()
        {
            Scenery.Build(transform, withFloor: false);

            var title = UiKit.TrackedLabel(transform, "ESCALAÇÃO", Typeface.Heading,
                Palette.Cream, 5f);
            UiKit.Anchor(title, new Vector2(0f, 1f), new Vector2(26f, -18f));

            // The back control is the approved secondary button (UI-005), as on the shop.
            var back = UiArt.StateButton(transform, Strings.Get("dialog.back"), new Vector2(120f, 44f),
                () => OnBack?.Invoke(), "btn2", Typeface.Small);
            UiKit.Anchor(back.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(-26f, -18f));

            ChemistryPanel();

            _board = UiKit.Row(transform, 18f);
            UiKit.Anchor(_board, new Vector2(0.5f, 1f), new Vector2(0f, -86f));
            _board.sizeDelta = new Vector2(1100f, 190f);

            var benchLabel = UiKit.Label(transform, Strings.Get("lineup.bench"), Typeface.Body,
                Logo.Brass, TextAnchor.MiddleLeft, bold: true);
            UiKit.Anchor(benchLabel.rectTransform, new Vector2(0f, 1f), new Vector2(26f, -292f));
            benchLabel.rectTransform.sizeDelta = new Vector2(300f, 26f);

            _bench = UiKit.ScrollRow(transform, new Vector2(1100f, 252f), 12f);
            UiKit.Anchor((RectTransform)_bench.parent, new Vector2(0.5f, 1f),
                new Vector2(0f, -318f));

            _warning = UiKit.Label(transform, "", Typeface.Body, Palette.Alarm, bold: true);
            UiKit.Anchor(_warning.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 76f));
            _warning.rectTransform.sizeDelta = new Vector2(900f, 28f);

            var tools = UiKit.Row(transform, 10f);
            UiKit.Anchor(tools, new Vector2(1f, 0f), new Vector2(-26f, 20f));
            tools.sizeDelta = new Vector2(400f, 44f);

            Scenery.Plank(tools, Strings.Get("lineup.auto"), new Vector2(190f, 46f), AutoFill, Typeface.Small);

            Scenery.Plank(tools, Strings.Get("lineup.clear"), new Vector2(130f, 46f), ClearLineup, Typeface.Small);
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

            if (_kit)
            {
                RefreshKit();
                return;
            }

            RefreshPlain();
        }

        private void RefreshPlain()
        {
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
            var card = MaidCard.Build(parent, maid, 0.62f, showSalary: false, playingAs: slot.Role, exact: true);
            card.GetComponent<Image>().raycastTarget = true;

            var button = card.gameObject.AddComponent<Button>();
            button.targetGraphic = card.GetComponent<Image>();
            button.onClick.AddListener(() => ClickSlot(slot, maid));

            if (_picking == slot) Tween.Punch(card, 0.08f, 0.5f);
        }

        /// <summary>A click on a slot: a maid standing there leaves it, an empty one waits for somebody.</summary>
        private void ClickSlot(LineupSlot slot, Maid maid)
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
        }

        private void AutoFill()
        {
            Session.Profile.Lineup.AutoFill(Session.Profile.Maids);
            Session.Touch();
            Refresh();
        }

        private void ClearLineup()
        {
            Session.Profile.Lineup.Clear();
            Session.Touch();
            Refresh();
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
                var card = MaidCard.Build(_bench, maid, 1.02f, exact: true);
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

        // ========================================================================================
        // The illustrated screen (UI-INT-03B)
        // ========================================================================================
        // Placed on a 1280x720 stage in pixels of the approved mockup (see KitStage). Every name,
        // number and verdict is Unity Text over a blank plate; the cards are the real MaidCard.

        private const float S = KitStage.S;

        private static readonly string[] KitSprites =
        {
            "lineup_background", "lineup_title_empty", "lineup_team_panel_empty", "lineup_team_banner_empty",
            "lineup_role_plate_empty", "lineup_vacant_slot_frame", "lineup_vacant_paw_plus",
            "lineup_chemistry_panel_empty", "lineup_chemistry_header_empty", "lineup_chemistry_value_panel_empty",
            "lineup_chemistry_track_empty", "lineup_chemistry_fill_pink", "lineup_daily_cost_panel_empty",
            "lineup_bank_panel_empty", "lineup_bank_banner_empty", "lineup_bank_filter_empty",
            "lineup_bank_sort_empty", "lineup_bank_page_empty", "lineup_bank_left", "lineup_bank_right",
            "lineup_button_auto_empty", "lineup_button_clear_empty",
            "maids_token_counter_empty", "back_blank", "bow_gold",
        };

        private static readonly Color Purple = Palette.Hex("#7A3AA8");
        private static readonly Color Rose = Palette.Hex("#D9366A");
        private static readonly Color RoleGold = Palette.Hex("#F5D27A");
        private static readonly Color SoftBrown = new Color(0.43f, 0.3f, 0.2f);

        /// <summary>The track's drawn length, in mockup pixels: the fill is clipped to the real score.</summary>
        private const float TrackWidth = 282f;

        private bool _kit;

        /// <summary>Asks for the earlier procedural layout even when the kit is present (tests, tools).</summary>
        public bool ForcePlain;

        // View state of the bench: presentation only, never saved.
        private Rarity? _benchRarity;
        private RosterSort _benchSort = RosterSort.Rarity;
        private int _benchPage;

        private RectTransform _teamLayer;
        private RectTransform _teamView;
        private RectTransform _benchLayer;
        private RectTransform _fillMask;
        private Text _tokens;
        private Text _teamCount;
        private Text _benchTitle;
        private Text _costValue;
        private Text _filterLabel;
        private Text _sortLabel;
        private Text _pageLabel;
        private Text _benchEmpty;
        private Button _benchPrev;
        private Button _benchNext;

        /// <summary>The bench's view state, for tests: a null rarity means "all".</summary>
        public Rarity? BenchRarity => _benchRarity;

        public RosterSort BenchSort => _benchSort;

        public int BenchPage => _benchPage;

        private void BuildKit()
        {
            KitArt.Backdrop(transform, "lineup_background");
            var stage = KitStage.Make(transform);

            // Title (no day indicator: only the title and the fixed cost are kept, by decision).
            var title = KitStage.At(stage, "lineup_title_empty", 586, 60, 1079, 146, "TitlePlate");
            KitArt.Fitted(title.transform, Strings.Get("lineup.screen"), 38, 18, KitArt.Brown,
                KitStage.Region(586, 60, 1079, 146, 706, 81, 967, 131));
            var bow = KitArt.Picture(stage, "bow_gold", 118f);
            bow.gameObject.name = "Bow";
            UiKit.Anchor(bow.rectTransform, new Vector2(0.5f, 1f), new Vector2((832.5f - 836f) * S, -22f));

            var pill = KitStage.At(stage, "maids_token_counter_empty", 1124, 71, 1399, 120, "TokenPlate");
            _tokens = KitArt.Fitted(pill.transform, "", 24, 12, Purple,
                KitStage.Region(1124, 71, 1399, 120, 1204, 80, 1349, 109));
            _tokens.gameObject.name = "Tokens";

            var back = KitArt.Button(stage, "back_blank", Strings.Get("dialog.back"), new Vector2(150f, 64f),
                () => OnBack?.Invoke(), 22, 12, KitArt.Brown, KitArt.Margins("back_blank", 86, 27, 90, 35));
            back.name = "Back";
            UiKit.Anchor(back.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(-16f, -14f));

            // The team.
            KitStage.At(stage, "lineup_team_panel_empty", 183, 160, 1138, 504, "TeamPlate");
            var banner = KitStage.At(stage, "lineup_team_banner_empty", 465, 151, 928, 211, "TeamBanner");
            _teamCount = KitArt.Fitted(banner.transform, "", 22, 10, KitArt.Cream,
                KitStage.Region(465, 151, 928, 211, 553, 166, 855, 197), outline: Palette.WithAlpha(KitArt.Brown, 0.9f));
            _teamCount.gameObject.name = "TeamCount";
            _teamView = KitStage.Slot(stage, "Team", 244, 212, 1095, 472);
            _teamView.gameObject.AddComponent<RectMask2D>();
            var teamCatcher = _teamView.gameObject.AddComponent<Image>();
            teamCatcher.color = new Color(1f, 1f, 1f, 0f);
            teamCatcher.raycastTarget = true;
            _teamLayer = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
            _teamLayer.SetParent(_teamView, false);
            _teamLayer.anchorMin = new Vector2(0f, 0f);
            _teamLayer.anchorMax = new Vector2(0f, 1f);
            _teamLayer.pivot = new Vector2(0f, 0.5f);
            _teamLayer.anchoredPosition = Vector2.zero;
            var teamScroll = _teamView.gameObject.AddComponent<ScrollRect>();
            teamScroll.content = _teamLayer;
            teamScroll.viewport = _teamView;
            teamScroll.horizontal = true;
            teamScroll.vertical = false;
            teamScroll.movementType = ScrollRect.MovementType.Clamped;
            teamScroll.scrollSensitivity = 30f;

            // Chemistry, the verdict and what the day costs.
            KitStage.At(stage, "lineup_chemistry_panel_empty", 1146, 160, 1500, 504, "ChemistryPlate");
            var header = KitStage.At(stage, "lineup_chemistry_header_empty", 1161, 179, 1484, 234, "ChemistryHeader");
            KitArt.Fitted(header.transform, Strings.Get("lineup.chemteam"), 19, 9, KitArt.Brown,
                KitStage.Region(1161, 179, 1484, 234, 1226, 190, 1421, 220));
            KitStage.At(stage, "lineup_chemistry_value_panel_empty", 1160, 227, 1484, 365, "ChemistryValuePanel");
            _chemistry = KitArt.Fitted(KitStage.Slot(stage, "ChemistryScore", 1197, 249, 1457, 280), "", 26, 12,
                KitArt.Brown, Vector4.zero);
            KitStage.At(stage, "lineup_chemistry_track_empty", 1176, 284, 1466, 310, "ChemistryTrack");
            _fillMask = KitStage.Slot(stage, "ChemistryFillMask", 1180, 288, 1180 + TrackWidth, 307);
            _fillMask.gameObject.AddComponent<RectMask2D>();
            var fill = KitArt.Plate(_fillMask, "lineup_chemistry_fill_pink",
                new Vector2((TrackWidth + 16f) * S, (19f + 16f) * S));
            fill.gameObject.name = "ChemistryFill";
            UiKit.Anchor(fill.rectTransform, new Vector2(0f, 1f), new Vector2(-8f * S, 8f * S));
            _verdict = KitArt.Fitted(KitStage.Slot(stage, "ChemistryVerdict", 1207, 320, 1451, 349), "", 17, 9,
                SoftBrown, Vector4.zero);

            KitStage.At(stage, "lineup_daily_cost_panel_empty", 1160, 370, 1484, 476, "CostPlate");
            KitArt.Fitted(KitStage.Slot(stage, "CostLabel", 1251, 387, 1462, 416), Strings.Get("hub.fixedcosts"), 18, 9,
                KitArt.Brown, Vector4.zero);
            _costValue = KitArt.Fitted(KitStage.Slot(stage, "CostValue", 1226, 425, 1458, 466), "", 32, 14, Rose,
                Vector4.zero);

            // The bench.
            KitStage.At(stage, "lineup_bank_panel_empty", 214, 536, 1447, 852, "BankPlate");
            var bankBanner = KitStage.At(stage, "lineup_bank_banner_empty", 265, 511, 621, 570, "BankBanner");
            _benchTitle = KitArt.Fitted(bankBanner.transform, "", 22, 10, KitArt.Cream,
                KitStage.Region(265, 511, 621, 570, 345, 522, 595, 560), outline: Palette.WithAlpha(KitArt.Brown, 0.9f));
            _benchTitle.gameObject.name = "BankTitle";

            var filter = KitStage.PlateButton(stage, "lineup_bank_filter_empty", "BankFilter", 780, 512, 1048, 570, () =>
            {
                _benchRarity = NextRarity(_benchRarity);
                _benchPage = 0;
                RefreshKit();
            });
            _filterLabel = KitArt.Fitted(filter.transform, "", 20, 10, KitArt.Brown,
                KitStage.Region(780, 512, 1048, 570, 849, 526, 974, 554));
            _filterLabel.gameObject.name = "Label";

            var sort = KitStage.PlateButton(stage, "lineup_bank_sort_empty", "BankSort", 1058, 512, 1392, 570, () =>
            {
                _benchSort = RosterQuery.Next(_benchSort);
                _benchPage = 0;
                RefreshKit();
            });
            _sortLabel = KitArt.Fitted(sort.transform, "", 20, 10, KitArt.Brown,
                KitStage.Region(1058, 512, 1392, 570, 1099, 527, 1335, 554));
            _sortLabel.gameObject.name = "Label";

            _benchLayer = KitStage.Layer(stage, "Bench");
            _benchEmpty = KitArt.Fitted(KitStage.Slot(stage, "BenchEmpty", 400, 660, 1260, 720), "", 22, 12,
                KitArt.Cream, Vector4.zero);
            _benchEmpty.gameObject.SetActive(false);

            _benchPrev = KitStage.PlateButton(stage, "lineup_bank_left", "BankPrev", 276, 665, 333, 721,
                () => GoToBenchPage(_benchPage - 1));
            _benchNext = KitStage.PlateButton(stage, "lineup_bank_right", "BankNext", 1327, 665, 1383, 721,
                () => GoToBenchPage(_benchPage + 1));
            var plate = KitStage.At(stage, "lineup_bank_page_empty", 518, 801, 661, 856, "BankPagePlate");
            _pageLabel = KitArt.Fitted(plate.transform, "", 22, 11, KitArt.Brown,
                KitStage.Region(518, 801, 661, 856, 538, 816, 641, 846));
            _pageLabel.gameObject.name = "Label";

            // The notice about why the doors cannot open, and the two actions.
            _warning = KitArt.Fitted(KitStage.Slot(stage, "Warning", 230, 856, 880, 906), "", 20, 10, Palette.Alarm,
                Vector4.zero, TextAnchor.MiddleLeft, outline: Palette.WithAlpha(KitArt.Cream, 0.95f));

            var auto = KitStage.PlateButton(stage, "lineup_button_auto_empty", "Auto", 890, 820, 1173, 900, AutoFill);
            KitArt.Fitted(auto.transform, Strings.Get("lineup.auto"), 22, 11, KitArt.Brown,
                KitStage.Region(890, 820, 1173, 900, 975, 843, 1154, 874)).gameObject.name = "Label";
            var clear = KitStage.PlateButton(stage, "lineup_button_clear_empty", "Clear", 1177, 820, 1429, 900,
                ClearLineup);
            KitArt.Fitted(clear.transform, Strings.Get("lineup.clear"), 22, 11, KitArt.Brown,
                KitStage.Region(1177, 820, 1429, 900, 1282, 842, 1399, 874)).gameObject.name = "Label";
        }

        private void RefreshKit()
        {
            var profile = Session.Profile;
            var lineup = profile.Lineup;

            _tokens.text = profile.Wallet.Points + " " + Strings.Get("hub.points");

            var placed = 0;
            foreach (var slot in lineup.All)
            {
                if (!slot.Empty) placed++;
            }

            _teamCount.text = Strings.Get("lineup.team") + " (" + placed + " / " + lineup.Slots.Total + ")";

            BuildTeamKit();
            BuildBenchKit();

            var chemistry = profile.Chemistry();
            _chemistry.text = Strings.Get("lineup.chemistry") + " " + chemistry.Score + " / 100";
            _verdict.text = Chemistry.Verdict(chemistry.Score) + "   ×" + chemistry.Multiplier.ToString("0.00");
            _fillMask.sizeDelta = new Vector2(TrackWidth * S * Mathf.Clamp01(chemistry.Score / 100f), 19f * S);
            _costValue.text = Strings.Money(Books.FixedCostsFor(profile));

            var why = Session.WhyNotReady();
            _warning.text = why ?? "";
            _warning.gameObject.SetActive(why != null);
        }

        /// <summary>
        /// One group per position that has slots (read from the lineup, never assumed), each a role plate
        /// over its cards. The cards shrink a little when the café has many slots, so all of them fit.
        /// </summary>
        private void BuildTeamKit()
        {
            KitStage.Discard(_teamLayer);

            var lineup = Session.Profile.Lineup;
            var roles = new List<Role>();
            var total = 0;
            foreach (Role role in Enum.GetValues(typeof(Role)))
            {
                var count = lineup.Slots.For(role);
                if (count == 0) continue;
                roles.Add(role);
                total += count;
            }

            if (total == 0) return;

            const float gapIn = 4f;
            const float gapGroup = 12f;
            const float plateH = 40f;
            const float below = 4f;

            var width = (1095f - 244f) * S;
            var height = (472f - 212f) * S;

            // The board's cards keep the size they always had on this screen (0.62): the card's own text is
            // laid out for it. A café with more slots than fit one screenful scrolls sideways instead.
            const float scale = 0.62f;
            var cw = MaidCard.Width * scale;
            var ch = MaidCard.Height * scale;

            var used = total * cw + (total - roles.Count) * gapIn + (roles.Count - 1) * gapGroup;
            _teamLayer.sizeDelta = new Vector2(Mathf.Max(width, used), 0f);
            var x = Mathf.Max(0f, (width - used) / 2f);
            var top = Mathf.Max(0f, (height - (plateH + below + ch)) / 2f);

            foreach (var role in roles)
            {
                var count = lineup.Slots.For(role);
                var groupW = count * cw + (count - 1) * gapIn;

                var plate = KitArt.Plate(_teamLayer, "lineup_role_plate_empty", new Vector2(groupW, plateH));
                plate.gameObject.name = "Role_" + role;
                UiKit.Anchor(plate.rectTransform, new Vector2(0f, 1f), new Vector2(x, -top));
                var label = KitArt.Fitted(plate.transform, RoleTable.For(role).DisplayName, 16, 7, RoleGold,
                    new Vector4(0.12f, 0.2f, 0.12f, 0.2f));
                label.gameObject.name = "Label";

                for (var i = 0; i < count; i++)
                {
                    var slot = lineup.Slot(role, i);
                    if (slot == null) continue;
                    BuildSlotKit(slot, new Vector2(x + i * (cw + gapIn), -(top + plateH + below)), scale, cw, ch);
                }

                x += groupW + gapGroup;
            }
        }

        private void BuildSlotKit(LineupSlot slot, Vector2 position, float scale, float cw, float ch)
        {
            var maid = Session.Profile.MaidById(slot.MaidId);

            var cell = new GameObject("Slot_" + slot.Role + "_" + slot.Index, typeof(RectTransform))
                .GetComponent<RectTransform>();
            cell.SetParent(_teamLayer, false);
            UiKit.Anchor(cell, new Vector2(0f, 1f), position);
            cell.sizeDelta = new Vector2(cw, ch);

            if (maid != null)
            {
                var card = MaidCard.Build(cell, maid, scale, showSalary: false, playingAs: slot.Role, exact: true);
                UiKit.Anchor(card, new Vector2(0.5f, 0.5f), Vector2.zero);
                var face = card.GetComponent<Image>();
                face.raycastTarget = true;
                var button = card.gameObject.AddComponent<Button>();
                button.targetGraphic = face;
                button.onClick.AddListener(() => ClickSlot(slot, maid));
            }
            else
            {
                // An empty slot: a resizable frame and a paw that keeps its own proportion.
                var face = KitArt.Plate(cell, "lineup_vacant_slot_frame", new Vector2(cw, ch));
                face.gameObject.name = "Vacant";
                UiKit.Anchor(face.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero);
                face.raycastTarget = true;
                var button = face.gameObject.AddComponent<Button>();
                KitStage.StyleButton(button, face);

                // The frame's own 8 px of padding, as drawn at this size: the click area is the drawn frame.
                var padding = 8f * ch / Mathf.Max(1f, face.sprite.rect.height);
                face.raycastPadding = new Vector4(padding, padding, padding, padding);
                button.onClick.AddListener(() => ClickSlot(slot, null));

                var paw = KitArt.Picture(cell, "lineup_vacant_paw_plus", Mathf.Min(cw, ch) * 0.62f);
                paw.gameObject.name = "Paw";
                UiKit.Anchor(paw.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero);
            }

            if (_picking == slot) Tween.Punch(cell, 0.08f, 0.5f);
        }

        /// <summary>
        /// Everybody not on the board, filtered by rarity and ordered, a page at a time. A card still
        /// drops her into the waiting slot, or her own position, exactly as before.
        /// </summary>
        private void BuildBenchKit()
        {
            KitStage.Discard(_benchLayer);

            var profile = Session.Profile;
            var pool = new List<Maid>();
            for (var i = 0; i < profile.Maids.Count; i++)
            {
                if (!profile.Lineup.Contains(profile.Maids[i].Id)) pool.Add(profile.Maids[i]);
            }

            var visible = RosterQuery.Visible(pool, _benchRarity, null, _benchSort);

            // The room between the two arrows.
            var rx0 = 340f * S;
            var ry0 = 575f * S;
            var width = (1320f - 340f) * S;
            var height = (814f - 575f) * S;
            const float gap = 8f;

            var scale = Mathf.Min(0.8f, (height - 8f) / MaidCard.Height);
            var cw = MaidCard.Width * scale;
            var ch = MaidCard.Height * scale;
            var perPage = Mathf.Max(1, Mathf.FloorToInt((width + gap) / (cw + gap)));
            var pages = Mathf.Max(1, Mathf.CeilToInt(visible.Count / (float)perPage));
            _benchPage = Mathf.Clamp(_benchPage, 0, pages - 1);

            var first = _benchPage * perPage;
            var count = Mathf.Max(0, Mathf.Min(perPage, visible.Count - first));
            var x = rx0 + (width - (count * cw + Mathf.Max(0, count - 1) * gap)) / 2f;
            var y = ry0 + (height - ch) / 2f;

            for (var i = 0; i < count; i++)
            {
                var maid = visible[first + i];
                var cell = new GameObject("Bench " + maid.Name, typeof(RectTransform)).GetComponent<RectTransform>();
                cell.SetParent(_benchLayer, false);
                UiKit.Anchor(cell, new Vector2(0f, 1f), new Vector2(x + i * (cw + gap), -y));
                cell.sizeDelta = new Vector2(cw, ch);

                var card = MaidCard.Build(cell, maid, scale, exact: true);
                UiKit.Anchor(card, new Vector2(0.5f, 0.5f), Vector2.zero);
                card.GetComponent<Image>().raycastTarget = true;
                var button = card.gameObject.AddComponent<Button>();
                button.targetGraphic = card.GetComponent<Image>();
                var captured = maid;
                button.onClick.AddListener(() => Field(captured));
            }

            _benchTitle.text = Strings.Get("lineup.benchtitle") + " (" + pool.Count + ")";
            _filterLabel.text = RarityName(_benchRarity);
            _sortLabel.text = Strings.Get("roster.sort") + ": " + SortName(_benchSort);
            _pageLabel.text = (_benchPage + 1) + " / " + pages;
            _benchPrev.interactable = _benchPage > 0;
            _benchNext.interactable = _benchPage < pages - 1;

            _benchEmpty.gameObject.SetActive(visible.Count == 0);
            if (visible.Count == 0)
            {
                _benchEmpty.text = Strings.Get(pool.Count == 0 ? "lineup.empty" : "roster.nomatch");
            }
        }

        private void GoToBenchPage(int page)
        {
            _benchPage = Mathf.Max(0, page);
            RefreshKit();
        }

        private static Rarity? NextRarity(Rarity? current)
        {
            var all = RosterQuery.Rarities;
            var index = current.HasValue ? Array.IndexOf(all, current.Value) + 1 : 0;
            return index >= all.Length ? (Rarity?)null : all[index];
        }

        private static string RarityName(Rarity? rarity)
        {
            switch (rarity)
            {
                case Rarity.Common: return Strings.Get("roster.f.common");
                case Rarity.Rare: return Strings.Get("roster.f.rare");
                case Rarity.Epic: return Strings.Get("roster.f.epic");
                case Rarity.Legendary: return Strings.Get("roster.f.legendary");
                default: return Strings.Get("roster.f.all");
            }
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
    }
}
