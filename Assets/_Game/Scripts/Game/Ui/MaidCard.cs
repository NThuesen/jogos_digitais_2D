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
            bool showSalary = true, Role? playingAs = null)
        {
            var size = new Vector2(Width * scale, Height * scale);
            var rarity = maid != null ? maid.Card : null;
            var frame = rarity != null ? Palette.Hex(rarity.TintHex) : Palette.Wall;

            var root = UiKit.Panel(parent, size, frame, 0.14f);
            root.gameObject.name = maid != null ? "Card " + maid.Name : "Card";

            var face = UiKit.Panel(root, new Vector2(size.x - 8f * scale, size.y - 8f * scale),
                Palette.Panel, 0.13f);
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
                Palette.WithAlpha(Palette.ForArchetype(maid.Archetype), 0.22f), 0.12f);
            UiKit.Anchor(wash, new Vector2(0.5f, 1f), new Vector2(0f, -4f * scale));

            Portrait(face, maid, size, scale);
            Name(face, maid, size, scale);
            Position(root, maid, size, scale, playingAs);
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
            Role? playingAs)
        {
            var wrong = playingAs.HasValue && playingAs.Value != maid.NaturalRole;
            var tint = wrong ? Palette.Alarm : Logo.Brass;

            var tab = UiKit.Panel(root, new Vector2(size.x * 0.56f, 22f * scale), tint, 0.45f);
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

            var number = UiKit.Label(column, value.ToString(), Mathf.RoundToInt(15f * scale),
                Palette.Text, bold: true);
            number.rectTransform.sizeDelta = new Vector2(30f * scale, 16f * scale);

            var caption = UiKit.Label(column, label, Mathf.RoundToInt(9f * scale), Palette.TextDim);
            caption.rectTransform.sizeDelta = new Vector2(30f * scale, 11f * scale);
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
    }
}
