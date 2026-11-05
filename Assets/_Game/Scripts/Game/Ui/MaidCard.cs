using Neko.Game.Art;
using Neko.Game.View;
using Neko.Sim.Maids;
using Neko.Sim.Team;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Game.Ui
{
    /// <summary>
    /// A maid as a collectible card.
    /// </summary>
    /// <remarks>
    /// Used on the lineup board, in the roster and when a box opens, so a card looks like the
    /// same object everywhere it appears — which is most of what makes a collection feel like
    /// a collection rather than a list.
    ///
    /// The rarity is the frame, not a word: a Legendary is gold-edged from across the room,
    /// and nobody has to read anything to know they pulled something good.
    /// </remarks>
    public static class MaidCard
    {
        public const float Width = 150f;
        public const float Height = 206f;

        public static RectTransform Build(Transform parent, Maid maid, float scale = 1f,
            bool showSalary = true, Role? playingAs = null, bool exact = false)
        {
            if (ArtReady())
            {
                return BuildArt(parent, maid, scale, showSalary, playingAs);
            }

            var size = new Vector2(Width * scale, Height * scale);
            var rarity = maid != null ? maid.Card : null;
            var frame = rarity != null ? Palette.Hex(rarity.TintHex) : Palette.Wall;

            var root = UiKit.Panel(parent, size, frame, 0.14f, exact);
            root.gameObject.name = maid != null ? "Card " + maid.Name : "Card";

            var face = UiKit.Panel(root, new Vector2(size.x - 8f * scale, size.y - 8f * scale),
                Palette.Panel, 0.13f, exact);
            UiKit.Anchor(face, new Vector2(0.5f, 0.5f), Vector2.zero);
            face.GetComponent<Image>().raycastTarget = false;

            if (maid == null)
            {
                var empty = UiKit.Label(face, Strings.Get("lineup.empty"), Typeface.Small,
                    Palette.TextDim);
                UiKit.Stretch(empty.rectTransform, 8f);
                return root;
            }

            // A wash of her own accent behind the portrait, so two cards of the same rarity
            // still read apart at a glance.
            var wash = UiKit.Panel(face, new Vector2(size.x - 16f * scale, size.y * 0.52f),
                Palette.WithAlpha(Palette.ForArchetype(maid.Archetype), 0.22f), 0.12f, exact);
            UiKit.Anchor(wash, new Vector2(0.5f, 1f), new Vector2(0f, -4f * scale));

            Portrait(face, maid, size, scale);
            Name(face, maid, size, scale);
            Position(root, maid, size, scale, playingAs, exact);
            Numbers(face, maid, size, scale, showSalary);
            RarityPip(root, maid, size, scale);

            return root;
        }

        private static void Portrait(RectTransform face, Maid maid, Vector2 size, float scale)
        {
            var image = new GameObject("Portrait", typeof(RectTransform)).AddComponent<Image>();
            image.transform.SetParent(face, false);
            image.rectTransform.sizeDelta = new Vector2(size.y * 0.46f, size.y * 0.46f);
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.sprite = UiKit.Sprites.Layered(MaidSpriteBuilder.Layers(maid, MaidPose.Idle),
                MaidSpriteBuilder.CacheKey(maid, MaidPose.Idle), 128);

            UiKit.Anchor(image.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -6f * scale));
        }

        private static void Name(RectTransform face, Maid maid, Vector2 size, float scale)
        {
            var name = UiKit.Label(face, maid.Name, Mathf.RoundToInt(20f * scale), Palette.Text,
                bold: true);
            name.rectTransform.sizeDelta = new Vector2(size.x - 16f * scale, 24f * scale);
            UiKit.Anchor(name.rectTransform, new Vector2(0.5f, 1f),
                new Vector2(0f, -size.y * 0.52f));

            var kind = UiKit.Label(face, maid.Profile.DisplayName, Mathf.RoundToInt(13f * scale),
                Palette.ForArchetype(maid.Archetype));
            kind.rectTransform.sizeDelta = new Vector2(size.x - 16f * scale, 18f * scale);
            UiKit.Anchor(kind.rectTransform, new Vector2(0.5f, 1f),
                new Vector2(0f, -size.y * 0.52f - 22f * scale));
        }

        /// <summary>
        /// Her natural position, as a tab on the card.
        /// </summary>
        /// <remarks>
        /// Turns red when she is being played somewhere else, which is the single most useful
        /// thing the lineup screen can tell somebody at a glance.
        /// </remarks>
        private static void Position(RectTransform root, Maid maid, Vector2 size, float scale,
            Role? playingAs, bool exact)
        {
            var wrong = playingAs.HasValue && playingAs.Value != maid.NaturalRole;
            var tint = wrong ? Palette.Alarm : Logo.Brass;

            var tab = UiKit.Panel(root, new Vector2(size.x * 0.56f, 22f * scale), tint, 0.45f, exact);
            UiKit.Anchor(tab, new Vector2(0.5f, 1f), new Vector2(0f, -2f * scale));
            tab.GetComponent<Image>().raycastTarget = false;

            var label = UiKit.Label(tab, RoleTable.For(maid.NaturalRole).DisplayName,
                Mathf.RoundToInt(12f * scale), Palette.Hex("#3A2A1A"), bold: true);
            UiKit.Stretch(label.rectTransform, 2f);
        }

        private static void Numbers(RectTransform face, Maid maid, Vector2 size, float scale,
            bool showSalary)
        {
            var stats = maid.Total;
            var row = UiKit.Row(face, 4f * scale);
            row.sizeDelta = new Vector2(size.x - 14f * scale, 26f * scale);
            UiKit.Anchor(row, new Vector2(0.5f, 0f), new Vector2(0f, showSalary ? 26f * scale : 8f * scale));

            Pip(row, "SAL", stats.Serve, scale);
            Pip(row, "COZ", stats.Cook, scale);
            Pip(row, "CHA", stats.Charm, scale);
            Pip(row, "FÔL", stats.Stamina, scale);

            if (!showSalary) return;

            var salary = UiKit.Label(face,
                Strings.Get("roster.salary") + " " + Strings.Money(maid.Salary),
                Mathf.RoundToInt(12f * scale), Palette.TextDim);
            salary.rectTransform.sizeDelta = new Vector2(size.x - 14f * scale, 18f * scale);
            UiKit.Anchor(salary.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 6f * scale));
        }

        private static void Pip(Transform parent, string label, int value, float scale)
        {
            var column = UiKit.Column(parent, 0f, 0f);
            column.sizeDelta = new Vector2(30f * scale, 26f * scale);

            // The boxes are at least a line tall at the rounded font size, so at small scales the
            // number's line and the caption's line do not overlap.
            var numberSize = Mathf.RoundToInt(15f * scale);
            var captionSize = Mathf.RoundToInt(9f * scale);
            var number = UiKit.Label(column, value.ToString(), numberSize, Palette.Text, bold: true);
            number.rectTransform.sizeDelta = new Vector2(30f * scale, Mathf.Max(16f * scale, numberSize * 1.3f));

            var caption = UiKit.Label(column, label, captionSize, Palette.TextDim);
            caption.rectTransform.sizeDelta = new Vector2(30f * scale, Mathf.Max(11f * scale, captionSize * 1.3f));
        }

        /// <summary>Traits as dots along the bottom edge. Hovering is not a thing here.</summary>
        private static void RarityPip(RectTransform root, Maid maid, Vector2 size, float scale)
        {
            if (maid.Traits.Count == 0) return;

            var row = UiKit.Row(root, 3f * scale);
            row.sizeDelta = new Vector2(size.x, 10f * scale);
            UiKit.Anchor(row, new Vector2(0.5f, 0f), new Vector2(0f, 2f * scale));

            for (var i = 0; i < maid.Traits.Count; i++)
            {
                var dot = new GameObject("Trait", typeof(RectTransform)).AddComponent<Image>();
                dot.transform.SetParent(row, false);
                dot.rectTransform.sizeDelta = new Vector2(8f * scale, 8f * scale);
                dot.raycastTarget = false;
                dot.sprite = UiKit.Sprites.Get(new ShapeSpec(ShapeId.Star, Logo.Brass), 32);
            }
        }

        // ---- Approved art (UI-008 compact / lineup minimal, UI-025 frames, UI-010 badges) -------
        // The frames are plain sprites drawn over the portrait: no 9-slice, text stays Unity's.
        // Below MinimalBelow the card is the LINEUP MINIMAL (portrait and name only); the compact
        // frame was approved at 150x206 and is used from 0.72x up, never below.

        private const float MinimalBelow = 0.7f;

        /// <summary>Share of the portrait window kept free at the bottom for tab and badge.</summary>
        private const float StripFraction = 0.2f;

        // Fractions of the card (x, y from the top, width, height) measured on the 535x735 exports.
        private static readonly Rect CompactWindow = new Rect(0.1355f, 0.126f, 0.729f, 0.415f);
        private static readonly Rect MinimalWindow = new Rect(0.1495f, 0.156f, 0.701f, 0.524f);
        private static readonly Rect CompactName = new Rect(0.14f, 0.566f, 0.72f, 0.098f);
        private static readonly Rect MinimalName = new Rect(0.12f, 0.768f, 0.76f, 0.168f);
        private static readonly Rect CompactKind = new Rect(0.12f, 0.684f, 0.76f, 0.05f);
        private static readonly Rect CompactKindLeft = new Rect(0.12f, 0.684f, 0.38f, 0.05f);
        private static readonly Rect CompactSalaryRight = new Rect(0.5f, 0.684f, 0.38f, 0.05f);
        private static readonly Rect CompactStats = new Rect(0.12f, 0.738f, 0.76f, 0.17f);

        private static readonly string[] ArtNames =
        {
            "card_common", "card_rare", "card_epic", "card_legendary", "card_minimal",
            "badge_common", "badge_rare", "badge_epic", "badge_legendary",
        };

        private static bool ArtReady() => UiArt.Has(ArtNames);

        private static string Key(Rarity rarity) => rarity.ToString().ToLowerInvariant();

        /// <summary>Anchors a rect to a fraction of its parent: x, y from the top, width, height.</summary>
        private static void Place(RectTransform rect, Rect fraction)
        {
            rect.anchorMin = new Vector2(fraction.x, 1f - fraction.y - fraction.height);
            rect.anchorMax = new Vector2(fraction.x + fraction.width, 1f - fraction.y);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static RectTransform BuildArt(Transform parent, Maid maid, float scale, bool showSalary,
            Role? playingAs)
        {
            var minimal = maid == null || scale < MinimalBelow;
            var size = new Vector2(Width * scale, Height * scale);

            // The root is only the hit area (clear, still a raycast target): the frame sits above
            // the portrait, so it cannot be the root's own sprite.
            var root = UiKit.Panel(parent, size, new Color(1f, 1f, 1f, 0f), 0.14f, true);
            root.gameObject.name = maid != null ? "Card " + maid.Name : "Card";

            var window = minimal ? MinimalWindow : CompactWindow;
            var clip = new GameObject("Window", typeof(RectTransform)).AddComponent<RectMask2D>();
            var clipRect = (RectTransform)clip.transform;
            clipRect.SetParent(root, false);
            Place(clipRect, window);

            if (maid != null)
            {
                var wash = new GameObject("Wash", typeof(RectTransform)).AddComponent<Image>();
                wash.transform.SetParent(clipRect, false);
                wash.color = Palette.WithAlpha(Palette.ForArchetype(maid.Archetype), 0.3f);
                wash.raycastTarget = false;
                UiKit.Stretch(wash.rectTransform);

                var portrait = new GameObject("Portrait", typeof(RectTransform)).AddComponent<Image>();
                portrait.transform.SetParent(clipRect, false);
                portrait.preserveAspect = true;
                portrait.raycastTarget = false;
                portrait.sprite = AuthoredPortrait(maid) ?? UiKit.Sprites.Layered(
                    MaidSpriteBuilder.Layers(maid, MaidPose.Idle),
                    MaidSpriteBuilder.CacheKey(maid, MaidPose.Idle), 128);
                UiKit.Stretch(portrait.rectTransform);
                // The bottom strip of the window belongs to the position tab and the rarity badge,
                // so neither covers the character.
                portrait.rectTransform.anchorMin = new Vector2(0f, StripFraction);
            }

            var frame = new GameObject("Frame", typeof(RectTransform)).AddComponent<Image>();
            frame.transform.SetParent(root, false);
            frame.sprite = UiArt.Get(minimal ? "card_minimal" : "card_" + Key(maid.Rarity));
            frame.raycastTarget = false;
            frame.color = maid == null ? new Color(1f, 1f, 1f, 0.55f) : Color.white;
            UiKit.Stretch(frame.rectTransform);

            if (maid == null)
            {
                var empty = UiKit.Label(root, Strings.Get("lineup.empty"), Typeface.Small, Palette.TextDim);
                Place(empty.rectTransform, MinimalWindow);
                return root;
            }

            // Rarity is the frame on the compact card; the minimal frame has one look, so the
            // gem badge (also on the compact card) is what tells the rarities apart there.
            var stripHeight = window.height * size.y * StripFraction;
            var badge = new GameObject("Rarity", typeof(RectTransform)).AddComponent<Image>();
            badge.transform.SetParent(root, false);
            badge.sprite = UiArt.Get("badge_" + Key(maid.Rarity));
            badge.preserveAspect = true;
            badge.raycastTarget = false;
            badge.rectTransform.sizeDelta = new Vector2(stripHeight * 1.75f, stripHeight);
            UiKit.Anchor(badge.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 0f));
            badge.rectTransform.pivot = new Vector2(1f, 0f);
            badge.rectTransform.anchoredPosition = new Vector2(window.xMax * size.x - 2f * scale,
                -window.yMax * size.y + 1f * scale);

            // Her natural position, bottom-left of the window; red when she plays elsewhere.
            var wrong = playingAs.HasValue && playingAs.Value != maid.NaturalRole;
            var tab = UiKit.Panel(root, new Vector2(window.width * size.x * 0.48f, stripHeight),
                wrong ? Palette.Alarm : Logo.Brass, 0.45f, true);
            tab.GetComponent<Image>().raycastTarget = false;
            tab.anchorMin = tab.anchorMax = new Vector2(0f, 1f);
            tab.pivot = new Vector2(0f, 0f);
            tab.anchoredPosition = new Vector2(window.x * size.x + 2f * scale,
                -window.yMax * size.y + 1f * scale);
            var role = UiKit.Label(tab, RoleTable.For(maid.NaturalRole).DisplayName,
                Mathf.Max(8, Mathf.RoundToInt(11f * scale)), Palette.Hex("#3A2A1A"), bold: true);
            UiKit.Stretch(role.rectTransform, 1f);
            role.resizeTextForBestFit = true;
            role.resizeTextMinSize = 6;
            role.resizeTextMaxSize = role.fontSize;

            var name = UiKit.Label(root, maid.Name, Mathf.RoundToInt((minimal ? 17f : 18f) * scale),
                Palette.Text, bold: true);
            Place(name.rectTransform, minimal ? MinimalName : CompactName);
            name.resizeTextForBestFit = true;
            name.resizeTextMinSize = 6;
            name.resizeTextMaxSize = name.fontSize;

            if (minimal) return root;

            // Archetype and wage share one line when the wage is shown; each keeps its own box.
            var small = Mathf.Max(8, Mathf.RoundToInt(10f * scale));
            // Darkened: the archetype colours are tuned for dark panels, and this one is cream.
            var kind = UiKit.Label(root, maid.Profile.DisplayName, small,
                Palette.Darken(Palette.ForArchetype(maid.Archetype), 0.35f),
                showSalary ? TextAnchor.MiddleLeft : TextAnchor.MiddleCenter, bold: true);
            Place(kind.rectTransform, showSalary ? CompactKindLeft : CompactKind);

            var stats = maid.Total;
            var row = new GameObject("Stats", typeof(RectTransform)).GetComponent<RectTransform>();
            row.SetParent(root, false);
            Place(row, CompactStats);
            ArtPip(row, 0, "SAL", stats.Serve, scale);
            ArtPip(row, 1, "COZ", stats.Cook, scale);
            ArtPip(row, 2, "CHA", stats.Charm, scale);
            ArtPip(row, 3, "FÔL", stats.Stamina, scale);

            if (showSalary)
            {
                var salary = UiKit.Label(root,
                    Strings.Get("roster.salary") + " " + Strings.Money(maid.Salary), small,
                    Palette.Text, TextAnchor.MiddleRight, bold: true);
                Place(salary.rectTransform, CompactSalaryRight);
            }

            return root;
        }

        /// <summary>One stat: the number over its caption, each in its own band so they never overlap.</summary>
        private static void ArtPip(RectTransform row, int index, string caption, int value, float scale)
        {
            var number = UiKit.Label(row, value.ToString(), Mathf.Max(9, Mathf.RoundToInt(15f * scale)),
                Palette.Text, bold: true);
            Place(number.rectTransform, new Rect(index * 0.25f, 0f, 0.25f, 0.56f));

            var label = UiKit.Label(row, caption, Mathf.Max(7, Mathf.RoundToInt(8f * scale)),
                Palette.TextDim);
            Place(label.rectTransform, new Rect(index * 0.25f, 0.62f, 0.25f, 0.38f));
        }

        private static readonly System.Collections.Generic.Dictionary<string, Sprite> Portraits =
            new System.Collections.Generic.Dictionary<string, Sprite>();

        /// <summary>
        /// The premium portrait (UI-021) of the authored character, else her resting frame, cropped to head and torso, or null when this
        /// maid is not one of the nine (the procedural figure is used then). It is a crop of the
        /// existing walk sheet: no new image, and never another maid's face under this name.
        /// </summary>
        private static Sprite AuthoredPortrait(Maid maid)
        {
            var premium = MaidPortraits.Head(maid);
            if (premium != null) return premium;

            var entry = MaidAnimCatalog.Identify(maid);
            if (entry == null) return null;
            if (Portraits.TryGetValue(entry.id, out var cached) && cached != null) return cached;

            var frames = MaidAnimCatalog.Load(UiKit.Sprites, entry);
            if (frames == null || entry.idle_frame >= frames.Length) return null;

            var cell = frames[entry.idle_frame];
            var rect = cell.rect;
            var crop = new Rect(rect.x, rect.y + rect.height * 0.36f, rect.width, rect.height * 0.64f);
            var sprite = Sprite.Create(cell.texture, crop, new Vector2(0.5f, 0.5f), cell.pixelsPerUnit, 0,
                SpriteMeshType.FullRect);
            sprite.name = entry.id + "_portrait";
            sprite.hideFlags = HideFlags.HideAndDontSave;
            Portraits[entry.id] = sprite;
            return sprite;
        }
    }
}
