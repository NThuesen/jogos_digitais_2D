using System;
using System.Collections.Generic;
using Neko.Game.Art;
using Neko.Game.Feel;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Game.Ui
{
    /// <summary>
    /// The illustrated, owner-approved modular sprites, and the controls built from them.
    /// </summary>
    /// <remarks>
    /// The art is only a skin. Text, layout, raycasts and click handlers stay in Unity, exactly
    /// as they are on <see cref="Scenery.Plank"/>; every helper here falls back to the procedural
    /// widget when a sprite is missing, so a bad import can never leave a screen without buttons.
    /// Sources and hashes of every file under <c>Resources/UiArt</c> are in its SOURCES.json.
    /// </remarks>
    public static class UiArt
    {
        private const string Folder = "UiArt/";

        /// <summary>Dark brown used for text over the cream of the approved sprites.</summary>
        public static readonly Color Ink = new Color(0.29f, 0.196f, 0.125f);

        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        /// <summary>Loads an approved sprite by file name (no extension); null when missing.</summary>
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

        /// <summary>A decorative picture, kept in proportion and never a raycast target.</summary>
        public static Image Picture(Transform parent, string name, Vector2 size)
        {
            var image = new GameObject(name, typeof(RectTransform)).AddComponent<Image>();
            image.transform.SetParent(parent, false);
            image.rectTransform.sizeDelta = size;
            image.sprite = Get(name);
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        /// <summary>
        /// A button skinned by four approved states: <c>{prefix}_normal/_hover/_pressed/_disabled</c>.
        /// </summary>
        /// <param name="prefix"><c>btn1</c> primary compact, <c>btn2</c> secondary.</param>
        public static Button StateButton(Transform parent, string text, Vector2 size, Action onClick,
            string prefix, int fontSize, Color? textColour = null)
        {
            if (!Has(prefix + "_normal", prefix + "_hover", prefix + "_pressed", prefix + "_disabled"))
            {
                return Scenery.Plank(parent, text, size, onClick, fontSize);
            }

            var normal = Get(prefix + "_normal");
            var root = new GameObject("ArtButton", typeof(RectTransform)).GetComponent<RectTransform>();
            root.SetParent(parent, false);
            root.sizeDelta = size;

            var image = root.gameObject.AddComponent<Image>();
            image.sprite = normal;
            image.type = Image.Type.Sliced;
            // Corners keep the proportions they were approved with: scale follows the rect height.
            image.pixelsPerUnitMultiplier = normal.rect.height / size.y;
            image.raycastTarget = true;

            var button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState
            {
                highlightedSprite = Get(prefix + "_hover"),
                pressedSprite = Get(prefix + "_pressed"),
                selectedSprite = normal,
                disabledSprite = Get(prefix + "_disabled"),
            };

            AddLabel(root, text, fontSize, textColour ?? Ink);
            button.onClick.AddListener(() =>
            {
                Tween.Punch(root, 0.1f, 0.18f);
                onClick?.Invoke();
            });

            return button;
        }

        /// <summary>
        /// A tab skinned by the approved INACTIVE / HOVER / ACTIVE art. Call <see cref="SetSelected"/>
        /// to show the selected one; the selected tab also carries a persistent bar underneath,
        /// because ACTIVE and HOVER are close in colour.
        /// </summary>
        public static Button Tab(Transform parent, string text, Vector2 size, Action onClick, int fontSize)
        {
            if (!Has("tab_inactive", "tab_hover", "tab_active"))
            {
                return Scenery.Plank(parent, text, size, onClick, fontSize);
            }

            var root = new GameObject("ArtTab", typeof(RectTransform)).GetComponent<RectTransform>();
            root.SetParent(parent, false);
            root.sizeDelta = size;

            var image = root.gameObject.AddComponent<Image>();
            image.sprite = Get("tab_inactive");
            image.preserveAspect = true;
            image.raycastTarget = true;

            var button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.SpriteSwap;

            var mark = UiKit.Panel(root, new Vector2(size.x * 0.46f, 4f), Logo.Brass, 0.5f);
            mark.gameObject.name = "SelectedMark";
            mark.GetComponent<Image>().raycastTarget = false;
            mark.anchorMin = mark.anchorMax = new Vector2(0.5f, 0f);
            mark.pivot = new Vector2(0.5f, 1f);
            mark.anchoredPosition = new Vector2(0f, -3f);

            AddLabel(root, text, fontSize, Ink);
            button.onClick.AddListener(() =>
            {
                Tween.Punch(root, 0.1f, 0.18f);
                onClick?.Invoke();
            });

            SetSelected(button, false);
            return button;
        }

        /// <summary>Shows a tab as selected (ACTIVE art plus the persistent bar) or not.</summary>
        public static void SetSelected(Button tab, bool selected)
        {
            var image = tab.targetGraphic as Image;
            if (image == null || !Has("tab_inactive", "tab_hover", "tab_active")) return;

            var active = Get("tab_active");
            image.sprite = selected ? active : Get("tab_inactive");
            tab.spriteState = new SpriteState
            {
                highlightedSprite = selected ? active : Get("tab_hover"),
                pressedSprite = active,
                selectedSprite = selected ? active : Get("tab_inactive"),
                disabledSprite = Get("tab_inactive"),
            };

            var mark = tab.transform.Find("SelectedMark");
            if (mark != null) mark.gameObject.SetActive(selected);
        }

        private static Sprite _fade;

        /// <summary>A white-to-clear vertical ramp (opaque at the bottom), tinted by the Image colour.</summary>
        public static Sprite Fade()
        {
            if (_fade != null) return _fade;

            const int height = 64;
            var texture = new Texture2D(2, height, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = "UiArtFade",
            };

            for (var y = 0; y < height; y++)
            {
                var t = 1f - y / (height - 1f);
                var alpha = t * t * (3f - 2f * t);
                for (var x = 0; x < 2; x++) texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }

            texture.Apply(false, true);
            _fade = Sprite.Create(texture, new Rect(0, 0, 2, height), new Vector2(0.5f, 0.5f), 100f);
            _fade.name = "UiArtFade";
            return _fade;
        }

        /// <summary>A soft shade along the top or bottom edge, stretched across the parent.</summary>
        public static Image Shade(Transform parent, bool top, float height, float alpha)
        {
            var image = new GameObject(top ? "ShadeTop" : "ShadeBottom", typeof(RectTransform))
                .AddComponent<Image>();
            image.transform.SetParent(parent, false);
            image.sprite = Fade();
            image.color = Palette.WithAlpha(Palette.Ink, alpha);
            image.raycastTarget = false;

            var rect = image.rectTransform;
            rect.anchorMin = new Vector2(0f, top ? 1f : 0f);
            rect.anchorMax = new Vector2(1f, top ? 1f : 0f);
            rect.pivot = new Vector2(0.5f, top ? 1f : 0f);
            rect.sizeDelta = new Vector2(0f, height);
            rect.anchoredPosition = Vector2.zero;
            if (top) rect.localScale = new Vector3(1f, -1f, 1f);
            return image;
        }

        private static void AddLabel(RectTransform root, string text, int fontSize, Color colour)
        {
            var label = UiKit.Label(root, text, fontSize, colour, bold: true);
            UiKit.Stretch(label.rectTransform, 8f);
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
        }
    }
}
