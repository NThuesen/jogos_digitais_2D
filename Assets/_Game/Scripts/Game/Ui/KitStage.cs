using System;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Game.Ui
{
    /// <summary>
    /// The 1280x720 stage the illustrated screens (UI-INT-02A, UI-INT-03B) are laid out on. Every
    /// coordinate is in pixels of the approved mockup (1672x941); <see cref="S"/> maps them to the stage.
    /// </summary>
    public static class KitStage
    {
        /// <summary>Mockup pixels to stage pixels.</summary>
        public const float S = 1280f / 1672f;

        /// <summary>The stage: 1280x720 anchored top-centre, scaled down to fit a narrower canvas.</summary>
        public static RectTransform Make(Transform parent)
        {
            var stage = new GameObject("Stage", typeof(RectTransform), typeof(RosterScreen.FitStage))
                .GetComponent<RectTransform>();
            stage.SetParent(parent, false);
            stage.anchorMin = new Vector2(0.5f, 1f);
            stage.anchorMax = new Vector2(0.5f, 1f);
            stage.pivot = new Vector2(0.5f, 1f);
            stage.anchoredPosition = Vector2.zero;
            stage.sizeDelta = new Vector2(1280f, 720f);
            return stage;
        }

        /// <summary>
        /// A kit sprite placed where the mockup crop (x0,y0)-(x1,y1) was: the sprite is its crop plus
        /// the kit's 8 px of transparent padding on every side.
        /// </summary>
        public static Image At(Transform parent, string sprite, float x0, float y0, float x1, float y1,
            string name = null)
        {
            var image = KitArt.Plate(parent, sprite, new Vector2((x1 - x0 + 16f) * S, (y1 - y0 + 16f) * S));
            image.gameObject.name = name ?? sprite;
            UiKit.Anchor(image.rectTransform, new Vector2(0f, 1f), new Vector2((x0 - 8f) * S, -(y0 - 8f) * S));
            return image;
        }

        /// <summary>An empty rect over mockup pixels (x0,y0)-(x1,y1), for text or content.</summary>
        public static RectTransform Slot(Transform parent, string name, float x0, float y0, float x1, float y1)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            UiKit.Anchor(rect, new Vector2(0f, 1f), new Vector2(x0 * S, -y0 * S));
            rect.sizeDelta = new Vector2((x1 - x0) * S, (y1 - y0) * S);
            return rect;
        }

        public static RectTransform Layer(Transform parent, string name)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            UiKit.Stretch(rect);
            return rect;
        }

        /// <summary>
        /// Margins (left, bottom, right, top, as fractions) of a label region, in mockup pixels, inside a
        /// plate placed with <see cref="At"/> over the crop (x0,y0)-(x1,y1).
        /// </summary>
        public static Vector4 Region(float x0, float y0, float x1, float y1, float rx0, float ry0, float rx1, float ry1)
        {
            var w = x1 - x0 + 16f;
            var h = y1 - y0 + 16f;
            return new Vector4((rx0 - x0 + 8f) / w, (y1 + 8f - ry1) / h, (x1 + 8f - rx1) / w, (ry0 - y0 + 8f) / h);
        }

        /// <summary>The kit's plates carry 8 px of transparent padding: the click area is the drawn plate.</summary>
        public static void TrimHitArea(Image face)
        {
            var pad = 8f * S;
            face.raycastPadding = new Vector4(pad, pad, pad, pad);
        }

        public static void StyleButton(Button button, Image face)
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

        /// <summary>A plate that is a button, at a mockup crop.</summary>
        public static Button PlateButton(Transform parent, string sprite, string name, float x0, float y0, float x1,
            float y1, Action onClick)
        {
            var face = At(parent, sprite, x0, y0, x1, y1, name);
            face.raycastTarget = true;
            var button = face.gameObject.AddComponent<Button>();
            StyleButton(button, face);
            button.onClick.AddListener(() => onClick());
            return button;
        }

        /// <summary>Removes the children now (a click may be what asked), and destroys them at frame end.</summary>
        public static void Discard(Transform layer)
        {
            for (var i = layer.childCount - 1; i >= 0; i--)
            {
                var child = layer.GetChild(i);
                child.gameObject.SetActive(false);
                child.SetParent(null, false);
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }
    }
}
