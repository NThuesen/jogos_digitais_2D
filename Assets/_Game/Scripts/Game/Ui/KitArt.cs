using System;
using System.Collections.Generic;
using Neko.Game.Art;
using Neko.Game.Feel;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Game.Ui
{
    /// <summary>
    /// The UI-INT-01 kit: blank illustrated plates, buttons, chests and backdrops from
    /// <c>Resources/UiKit</c>. The art is only a skin: every number and every word on a screen is
    /// a Unity <see cref="Text"/> laid over a blank plate, and every helper answers null/false
    /// when a sprite is missing so a screen can fall back to its procedural layout.
    /// </summary>
    public static class KitArt
    {
        private const string Folder = "UiKit/";

        /// <summary>Dark brown for text on cream plates.</summary>
        public static readonly Color Brown = new Color(0.29f, 0.196f, 0.125f);

        public static readonly Color Cream = new Color(1f, 0.96f, 0.88f);

        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite Get(string name)
        {
            if (Cache.TryGetValue(name, out var cached) && cached != null) return cached;

            var sprite = Resources.Load<Sprite>(Folder + name);
            if (sprite != null) Cache[name] = sprite;
            return sprite;
        }

        public static bool Has(params string[] names)
        {
            for (var i = 0; i < names.Length; i++)
            {
                if (Get(names[i]) == null) return false;
            }

            return true;
        }

        /// <summary>
        /// A plate drawn from a kit sprite. A sprite with a 9-slice border is sliced so its corners
        /// keep the proportions of the art at this height; one without is drawn whole, in
        /// proportion to its width, never stretched.
        /// </summary>
        public static Image Plate(Transform parent, string name, Vector2 size)
        {
            var sprite = Get(name);
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            image.rectTransform.sizeDelta = size;

            if (sprite != null && sprite.border != Vector4.zero)
            {
                image.type = Image.Type.Sliced;
                image.pixelsPerUnitMultiplier = sprite.rect.height / Mathf.Max(1f, size.y);
            }
            else
            {
                image.preserveAspect = true;
            }

            return image;
        }

        /// <summary>A picture at the given width, its height following the art.</summary>
        public static Image Picture(Transform parent, string name, float width)
        {
            var sprite = Get(name);
            var height = sprite != null ? width * sprite.rect.height / sprite.rect.width : width;
            var image = Plate(parent, name, new Vector2(width, height));
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            return image;
        }

        /// <summary>The full-screen backdrop: covers the screen, keeps its proportion, centred.</summary>
        public static bool Backdrop(Transform parent, string name)
        {
            var sprite = Get(name);
            if (sprite == null) return false;

            var go = new GameObject("Scenery", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.transform.SetAsFirstSibling();

            var image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;

            var fitter = go.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = sprite.rect.width / sprite.rect.height;
            return true;
        }

        /// <summary>
        /// A label on a plate, fitted into the plate's content area (fractions of the plate:
        /// left, bottom, right, top margins) and shrunk to fit rather than overflowing it.
        /// </summary>
        public static Text Fitted(Transform parent, string text, int maxSize, int minSize, Color colour,
            Vector4 margin, TextAnchor anchor = TextAnchor.MiddleCenter, Color? outline = null)
        {
            var label = UiKit.Label(parent, text, maxSize, colour, anchor, bold: true);
            label.resizeTextForBestFit = true;
            label.resizeTextMaxSize = maxSize;
            label.resizeTextMinSize = minSize;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;

            var rect = label.rectTransform;
            rect.anchorMin = new Vector2(margin.x, margin.y);
            rect.anchorMax = new Vector2(1f - margin.z, 1f - margin.w);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            if (outline.HasValue)
            {
                var edge = label.gameObject.AddComponent<Outline>();
                edge.effectColor = outline.Value;
                edge.effectDistance = new Vector2(1.5f, -1.5f);
            }

            return label;
        }

        /// <summary>Margins (as fractions) from a kit content rect, top-left origin, in sprite pixels.</summary>
        public static Vector4 Margins(string name, int x, int y, int w, int h)
        {
            var sprite = Get(name);
            if (sprite == null) return new Vector4(0.1f, 0.1f, 0.1f, 0.1f);

            var sw = sprite.rect.width;
            var sh = sprite.rect.height;
            return new Vector4(x / sw, 1f - (y + h) / sh, 1f - (x + w) / sw, y / sh);
        }

        /// <summary>
        /// A button on a kit plate. Hover, pressed and disabled are tints of the one blank plate
        /// (the kit ships one image per button), and the label is a Unity Text.
        /// </summary>
        public static Button Button(Transform parent, string spriteName, string text, Vector2 size,
            Action onClick, int maxFont, int minFont, Color textColour, Vector4 textMargin,
            Color? outline = null)
        {
            var image = Plate(parent, spriteName, size);
            image.gameObject.name = "KitButton";
            image.raycastTarget = true;

            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;

            var colours = button.colors;
            colours.normalColor = Color.white;
            colours.highlightedColor = new Color(1.08f, 1.06f, 1.04f, 1f);
            colours.pressedColor = new Color(0.82f, 0.78f, 0.74f, 1f);
            colours.selectedColor = Color.white;
            colours.disabledColor = new Color(0.62f, 0.6f, 0.6f, 0.9f);
            colours.fadeDuration = 0.07f;
            button.colors = colours;

            var label = Fitted(image.transform, text, maxFont, minFont, textColour, textMargin,
                outline: outline);
            label.gameObject.name = "Label";

            var rect = image.rectTransform;
            button.onClick.AddListener(() =>
            {
                Tween.Punch(rect, 0.08f, 0.16f);
                onClick?.Invoke();
            });

            return button;
        }

        /// <summary>The label of a <see cref="Button"/>.</summary>
        public static Text LabelOf(Button button) => button.transform.Find("Label")?.GetComponent<Text>();
    }
}
