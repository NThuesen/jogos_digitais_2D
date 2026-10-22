using Neko.Game.Art;
using Neko.Game.Feel;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Game.Ui
{
    /// <summary>
    /// The wordmark: a café sign hanging off a bracket, with ears.
    /// </summary>
    /// <remarks>
    /// A logo is a lockup, not a typeface. There is no custom letterform here and there could
    /// not be — every pixel in this project is generated from distance fields and drawing an
    /// alphabet that way would cost more than the rest of the game. What makes it read as
    /// designed instead of as a label is everything around the letters: the board, the chains,
    /// the bevel, the rule, the ears, the tracking, and the small amber badge that carries the
    /// joke the whole game is built on.
    ///
    /// "NEKO MAID" is the café. "Inc." is the part that can go bankrupt.
    /// </remarks>
    public static class Logo
    {
        public static readonly Color Board = Palette.Hex("#8C5A3C");
        public static readonly Color BoardDeep = Palette.Hex("#6B4530");
        public static readonly Color BoardLight = Palette.Hex("#A87050");
        public static readonly Color Brass = Palette.Hex("#D9A24A");

        /// <summary>
        /// Builds the sign into a parent, centred on its own rect.
        /// </summary>
        /// <param name="scale">1 is the title-screen size; smaller for a header.</param>
        public static RectTransform Build(Transform parent, float scale = 1f, bool hanging = true)
        {
            var width = 760f * scale;
            var height = 260f * scale;

            var root = new GameObject("Logo", typeof(RectTransform)).GetComponent<RectTransform>();
            root.SetParent(parent, false);
            root.sizeDelta = new Vector2(width, height + (hanging ? 70f * scale : 0f));

            // Order matters and is the whole fix: the hanger goes down first, then the ears,
            // then the board over both. Built the other way round and pushed behind with
            // SetAsFirstSibling, the ears ended up under the hanger bar as well and read as
            // two loose triangles near a sign rather than as ears on one.
            if (hanging) Hanger(root, width, height, scale);

            Ears(root, width, height, scale);
            var board = BuildBoard(root, width, height, scale);
            Wordmark(board, scale);

            return root;
        }

        /// <summary>The bracket and two chains the board swings from.</summary>
        private static void Hanger(RectTransform root, float width, float height, float scale)
        {
            var bar = UiKit.Panel(root, new Vector2(width * 0.62f, 16f * scale), BoardDeep, 0.5f);
            UiKit.Anchor(bar, new Vector2(0.5f, 1f), new Vector2(0f, -4f * scale));

            for (var side = -1; side <= 1; side += 2)
            {
                var chain = UiKit.Panel(root, new Vector2(7f * scale, 62f * scale), Brass, 0.5f);
                UiKit.Anchor(chain, new Vector2(0.5f, 1f),
                    new Vector2(side * width * 0.26f, -14f * scale));

                var pin = UiKit.Panel(root, new Vector2(16f * scale, 16f * scale), Brass, 0.5f);
                UiKit.Anchor(pin, new Vector2(0.5f, 1f),
                    new Vector2(side * width * 0.26f, -6f * scale));
            }
        }

        private static RectTransform BuildBoard(RectTransform root, float width, float height,
            float scale)
        {
            var shadow = UiKit.Panel(root, new Vector2(width, height),
                Palette.WithAlpha(Palette.Ink, 0.35f), 0.26f);
            UiKit.Anchor(shadow, new Vector2(0.5f, 0f), new Vector2(6f * scale, -8f * scale));

            var board = UiKit.Panel(root, new Vector2(width, height), Board, 0.26f);
            UiKit.Anchor(board, new Vector2(0.5f, 0f), Vector2.zero);

            // A lighter plate inset into the board. One shape, and the sign stops being a
            // rectangle and starts being an object.
            var inset = UiKit.Panel(board, new Vector2(width - 34f * scale, height - 34f * scale),
                BoardLight, 0.22f);
            UiKit.Anchor(inset, new Vector2(0.5f, 0.5f), Vector2.zero);

            var face = UiKit.Panel(board, new Vector2(width - 52f * scale, height - 52f * scale),
                BoardDeep, 0.2f);
            UiKit.Anchor(face, new Vector2(0.5f, 0.5f), Vector2.zero);

            // Four brass studs. Cheap, and they do most of the work of saying "a thing that
            // was made" rather than "a rounded rectangle".
            for (var sx = -1; sx <= 1; sx += 2)
            {
                for (var sy = -1; sy <= 1; sy += 2)
                {
                    var stud = Icon(board, ShapeId.Circle, Brass, 15f * scale);
                    UiKit.Anchor(stud, new Vector2(0.5f, 0.5f),
                        new Vector2(sx * (width * 0.5f - 17f * scale),
                            sy * (height * 0.5f - 17f * scale)));
                }
            }

            return face;
        }

        /// <summary>Two ears over the top edge. The only pictorial part of the mark.</summary>
        private static void Ears(RectTransform root, float width, float height, float scale)
        {
            // Pivoted at their own base and sat on the board's top edge, so they read as
            // ears on a sign rather than as two triangles floating near one.
            for (var side = -1; side <= 1; side += 2)
            {
                var ear = Icon(root, ShapeId.CatEar, BoardDeep, 96f * scale);
                ear.pivot = new Vector2(0.5f, 0f);
                ear.anchorMin = ear.anchorMax = new Vector2(0.5f, 0f);
                ear.anchoredPosition = new Vector2(side * width * 0.23f, height - 62f * scale);
                ear.localRotation = Quaternion.Euler(0f, 0f, side * -11f);

                var inner = Icon(root, ShapeId.CatEar, Palette.InnerEar, 44f * scale);
                inner.pivot = new Vector2(0.5f, 0f);
                inner.anchorMin = inner.anchorMax = new Vector2(0.5f, 0f);
                inner.anchoredPosition = new Vector2(side * width * 0.23f, height - 40f * scale);
                inner.localRotation = Quaternion.Euler(0f, 0f, side * -11f);
            }
        }

        private static void Wordmark(RectTransform face, float scale)
        {
            var column = UiKit.Column(face, 2f * scale, 0f);
            UiKit.Stretch(column);
            column.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            var top = UiKit.TrackedLabel(column, "NEKO MAID", Mathf.RoundToInt(58f * scale),
                Palette.Cream, 7f * scale);
            top.gameObject.name = "Wordmark";

            var rule = UiKit.Panel(column, new Vector2(380f * scale, 3f * scale), Brass, 0.5f);
            rule.gameObject.name = "Rule";

            var badge = UiKit.Panel(column, new Vector2(132f * scale, 40f * scale), Brass, 0.45f);
            badge.gameObject.name = "Inc";

            var inc = UiKit.TrackedLabel(badge, "Inc.", Mathf.RoundToInt(24f * scale),
                Palette.Hex("#4A3220"), 3f * scale, shadow: false);
            UiKit.Anchor(inc, new Vector2(0.5f, 0.5f), Vector2.zero);
        }

        /// <summary>
        /// The square mark: a cat's head in a brass ring.
        /// </summary>
        /// <remarks>
        /// For the corner of a screen, where the full sign would not fit and would not be
        /// legible if it did.
        /// </remarks>
        public static RectTransform Monogram(Transform parent, float size)
        {
            var root = new GameObject("Monogram", typeof(RectTransform)).GetComponent<RectTransform>();
            root.SetParent(parent, false);
            root.sizeDelta = new Vector2(size, size);

            var ring = Icon(root, ShapeId.Circle, Brass, size);
            UiKit.Anchor(ring, new Vector2(0.5f, 0.5f), Vector2.zero);

            var inner = Icon(root, ShapeId.Circle, BoardDeep, size * 0.82f);
            UiKit.Anchor(inner, new Vector2(0.5f, 0.5f), Vector2.zero);

            for (var side = -1; side <= 1; side += 2)
            {
                var ear = Icon(root, ShapeId.CatEar, Palette.Cream, size * 0.3f);
                UiKit.Anchor(ear, new Vector2(0.5f, 0.5f),
                    new Vector2(side * size * 0.19f, size * 0.22f));
                ear.localRotation = Quaternion.Euler(0f, 0f, side * -12f);
            }

            var head = Icon(root, ShapeId.Circle, Palette.Cream, size * 0.46f);
            UiKit.Anchor(head, new Vector2(0.5f, 0.5f), new Vector2(0f, -size * 0.02f));

            for (var side = -1; side <= 1; side += 2)
            {
                var eye = Icon(root, ShapeId.Capsule, BoardDeep, size * 0.09f);
                UiKit.Anchor(eye, new Vector2(0.5f, 0.5f),
                    new Vector2(side * size * 0.1f, size * 0.01f));
            }

            return root;
        }

        /// <summary>
        /// A gentle sway, so the sign reads as hanging rather than printed.
        /// </summary>
        /// <remarks>
        /// One tween that reverses itself. Two and a half degrees, which is under the
        /// threshold where it starts to read as a thing being animated at you.
        /// </remarks>
        public static void Sway(RectTransform logo, float from = -1.4f, float to = 1.4f)
        {
            if (logo == null) return;

            Tween.Value(from, to, 2.6f, angle =>
            {
                if (logo != null) logo.localRotation = Quaternion.Euler(0f, 0f, angle);
            }, Ease.InOutQuad).Then(() => Sway(logo, to, from));
        }

        private static RectTransform Icon(Transform parent, ShapeId shape, Color colour, float size)
        {
            var go = new GameObject(shape.ToString(), typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var image = go.AddComponent<Image>();
            image.sprite = UiKit.Sprites.Get(new ShapeSpec(shape, colour), 128);
            image.raycastTarget = false;
            image.preserveAspect = true;

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(size, size);
            return rect;
        }
    }
}
