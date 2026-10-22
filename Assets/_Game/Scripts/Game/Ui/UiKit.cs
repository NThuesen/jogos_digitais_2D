using System;
using Neko.Game.Art;
using Neko.Game.Feel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Neko.Game.Ui
{
    /// <summary>
    /// Every widget in the game, built in code.
    /// </summary>
    /// <remarks>
    /// No prefabs. A prefab is Unity YAML, and Unity YAML in a team of four is a merge
    /// conflict with a GUID in it — on a project whose whole delivery is one branch, that is
    /// a real cost. Building the interface in code also means the layout is reviewable in a
    /// diff and testable without opening the editor.
    ///
    /// Sprites come from the same generator as the rest of the art, so a panel and a plate are
    /// the same rounded rectangle at different tints.
    /// </remarks>
    public static class UiKit
    {
        /// <summary>Smallest a thing anybody has to hit is allowed to be.</summary>
        public const float MinimumHitSize = 44f;

        public static SpriteFactory Sprites;

        public static RectTransform Root(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        public static RectTransform Panel(Transform parent, Vector2 size, Color colour,
            float corner = 0.35f)
        {
            var go = new GameObject("Panel", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = size;

            var image = go.AddComponent<Image>();
            image.sprite = Sprites.Get(
                new ShapeSpec(ShapeId.RoundedRect, Color.white, detail: corner), 96);
            image.color = colour;
            image.type = Image.Type.Simple;
            return rect;
        }

        public static Text Label(Transform parent, string text, int size, Color colour,
            TextAnchor anchor = TextAnchor.MiddleCenter, bool bold = false)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var label = go.AddComponent<Text>();
            label.font = Typeface.Font;
            label.fontSize = size;
            label.color = colour;
            label.alignment = anchor;
            label.text = text;
            label.raycastTarget = false;
            label.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(360f, size * 1.6f);
            return label;
        }

        /// <summary>
        /// A button that presses.
        /// </summary>
        /// <remarks>
        /// The bounce is not decoration. Every click in this game is a command to a person on
        /// the floor, and a control that does not answer feels broken long before the café
        /// does.
        /// </remarks>
        public static Button Button(Transform parent, string text, Vector2 size, Color colour,
            Action onClick, int fontSize = Typeface.Body)
        {
            var rect = Panel(parent, new Vector2(
                Mathf.Max(size.x, MinimumHitSize), Mathf.Max(size.y, MinimumHitSize)), colour);
            rect.gameObject.name = "Button";

            var image = rect.GetComponent<Image>();
            image.raycastTarget = true;

            var label = Label(rect, text, fontSize, Palette.Text, bold: true);
            Stretch(label.rectTransform, 10f);

            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            var colours = button.colors;
            colours.normalColor = Color.white;
            colours.highlightedColor = new Color(1.06f, 1.06f, 1.06f, 1f);
            colours.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colours.disabledColor = new Color(1f, 1f, 1f, 0.4f);
            colours.fadeDuration = 0.08f;
            button.colors = colours;

            button.onClick.AddListener(() =>
            {
                Tween.Punch(rect, 0.12f, 0.2f);
                onClick?.Invoke();
            });

            return button;
        }

        /// <summary>A bar that fills. Patience, mood, experience, the day clock.</summary>
        public static Image Bar(Transform parent, Vector2 size, Color fill, Color track)
        {
            var back = Panel(parent, size, track, 0.5f);
            back.gameObject.name = "Bar";

            var front = Panel(back, size, fill, 0.5f);
            front.gameObject.name = "Fill";
            front.anchorMin = new Vector2(0f, 0f);
            front.anchorMax = new Vector2(0f, 1f);
            front.pivot = new Vector2(0f, 0.5f);
            front.offsetMin = new Vector2(0f, 0f);
            front.offsetMax = new Vector2(0f, 0f);
            front.sizeDelta = new Vector2(size.x, 0f);

            var image = front.GetComponent<Image>();
            image.raycastTarget = false;
            return image;
        }

        public static void SetBar(Image fill, float fraction, float width)
        {
            if (fill == null) return;

            var rect = fill.rectTransform;
            rect.sizeDelta = new Vector2(Mathf.Clamp01(fraction) * width, rect.sizeDelta.y);
        }

        public static RectTransform Column(Transform parent, float spacing, float padding = 16f)
        {
            var go = new GameObject("Column", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var group = go.AddComponent<VerticalLayoutGroup>();
            group.spacing = spacing;
            group.padding = new RectOffset((int)padding, (int)padding, (int)padding, (int)padding);
            group.childAlignment = TextAnchor.UpperCenter;
            group.childForceExpandHeight = false;
            group.childForceExpandWidth = false;
            group.childControlHeight = false;
            group.childControlWidth = false;

            return go.GetComponent<RectTransform>();
        }

        public static RectTransform Row(Transform parent, float spacing, float padding = 0f)
        {
            var go = new GameObject("Row", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var group = go.AddComponent<HorizontalLayoutGroup>();
            group.spacing = spacing;
            group.padding = new RectOffset((int)padding, (int)padding, (int)padding, (int)padding);
            group.childAlignment = TextAnchor.MiddleCenter;
            group.childForceExpandHeight = false;
            group.childForceExpandWidth = false;
            group.childControlHeight = false;
            group.childControlWidth = false;

            return go.GetComponent<RectTransform>();
        }

        /// <summary>A scrollable list. Returns the content transform to add rows to.</summary>
        public static RectTransform ScrollList(Transform parent, Vector2 size, float spacing = 8f)
        {
            var viewport = Panel(parent, size, Palette.WithAlpha(Palette.Wall, 0.45f), 0.12f);
            viewport.gameObject.name = "Scroll";

            viewport.gameObject.AddComponent<RectMask2D>();
            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 28f;

            var content = Column(viewport, spacing, 10f);
            content.gameObject.name = "Content";
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = new Vector2(0f, 0f);

            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewport;
            scroll.content = content;
            return content;
        }

        /// <summary>
        /// A list that scrolls sideways.
        /// </summary>
        /// <remarks>
        /// Built as its own thing rather than by swapping the layout group on a vertical one.
        /// Destroy is deferred to the end of the frame, so the old group is still attached
        /// when the new one is added, Unity refuses the second component, AddComponent
        /// returns null and the next line throws — which is exactly what happened.
        /// </remarks>
        public static RectTransform ScrollRow(Transform parent, Vector2 size, float spacing = 8f)
        {
            var viewport = Panel(parent, size, WithAlphaWall(), 0.12f);
            viewport.gameObject.name = "ScrollRow";
            viewport.gameObject.AddComponent<RectMask2D>();

            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = true;
            scroll.vertical = false;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 28f;

            var content = Row(viewport, spacing, 10f);
            content.gameObject.name = "Content";
            content.anchorMin = new Vector2(0f, 0f);
            content.anchorMax = new Vector2(0f, 1f);
            content.pivot = new Vector2(0f, 0.5f);
            content.sizeDelta = new Vector2(0f, 0f);

            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewport;
            scroll.content = content;
            return content;
        }

        private static Color WithAlphaWall() =>
            new Color(Palette.Wall.r, Palette.Wall.g, Palette.Wall.b, 0.45f);

        public static void Stretch(RectTransform rect, float padding = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(padding, padding);
            rect.offsetMax = new Vector2(-padding, -padding);
        }

        public static void Anchor(RectTransform rect, Vector2 anchor, Vector2 position)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
        }

        /// <summary>
        /// The event system the whole interface needs exactly one of.
        /// </summary>
        /// <remarks>
        /// InputSystemUIInputModule, not StandaloneInputModule: this project runs with the
        /// Input System package as the only active handler, and the legacy module silently
        /// delivers no clicks at all under it.
        /// </remarks>
        public static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;

            var go = new GameObject("EventSystem", typeof(EventSystem));
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
    }
}
