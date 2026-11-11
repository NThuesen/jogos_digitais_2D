using UnityEngine;
using UnityEngine.UI;

namespace Neko.Game.Ui
{
    /// <summary>
    /// A fixed-size stage that a whole illustrated composition is laid out on, scaled uniformly to fit
    /// the screen and centred: the Hall (UI-INT-05) and the day and its report (UI-INT-06). The art is never
    /// stretched or warped; what the screen has beyond the stage is a solid letterbox.
    /// </summary>
    /// <remarks>
    /// Coordinates are in pixels of the approved mockup (top-left origin); <see cref="K"/> maps them to the
    /// stage's own pixels (the 1280x720 reference). Unlike <see cref="KitStage"/>, which hangs from the
    /// top, this centres, because the openings of these two compositions are part of the picture and a
    /// letterbox above and below keeps them where the art puts them.
    /// </remarks>
    public sealed class ContainStage : MonoBehaviour
    {
        private RectTransform _parent;

        public Vector2 Size;

        /// <summary>Mockup pixels to stage pixels.</summary>
        public float K { get; private set; }

        /// <summary>Transparent padding every cut-out sprite carries on each side, in mockup pixels.</summary>
        public float Pad { get; private set; }

        public RectTransform Rect => (RectTransform)transform;

        public static ContainStage Make(Transform parent, float mockupWidth, float mockupHeight, float k, float pad)
        {
            var go = new GameObject("Stage", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var stage = go.AddComponent<ContainStage>();
            stage.K = k;
            stage.Pad = pad;
            stage.Size = new Vector2(mockupWidth * k, mockupHeight * k);

            var rect = stage.Rect;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = stage.Size;
            return stage;
        }

        private void OnEnable() => Fit();

        private void LateUpdate() => Fit();

        private void Fit()
        {
            if (_parent == null) _parent = transform.parent as RectTransform;
            if (_parent == null) return;

            var room = _parent.rect;
            if (room.width <= 0f || room.height <= 0f) return;

            var scale = Mathf.Min(1f, Mathf.Min(room.width / Size.x, room.height / Size.y));
            transform.localScale = new Vector3(scale, scale, 1f);
            FitBars(_parent, scale);
        }

        /// <summary>A sprite placed where the mockup crop (x0,y0)-(x1,y1) was: the crop plus the sprite's padding.</summary>
        public Image At(Transform parent, string sprite, float x0, float y0, float x1, float y1, string name = null)
        {
            var image = KitArt.Plate(parent, sprite, new Vector2((x1 - x0 + 2f * Pad) * K, (y1 - y0 + 2f * Pad) * K));
            image.gameObject.name = name ?? sprite;
            UiKit.Anchor(image.rectTransform, new Vector2(0f, 1f), new Vector2((x0 - Pad) * K, -(y0 - Pad) * K));
            return image;
        }

        /// <summary>A sprite with no padding (backgrounds, frames) placed exactly over a mockup rectangle.</summary>
        public Image Exact(Transform parent, string sprite, float x0, float y0, float x1, float y1, string name = null)
        {
            var image = KitArt.Plate(parent, sprite, new Vector2((x1 - x0) * K, (y1 - y0) * K));
            image.gameObject.name = name ?? sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            UiKit.Anchor(image.rectTransform, new Vector2(0f, 1f), new Vector2(x0 * K, -y0 * K));
            return image;
        }

        /// <summary>An empty rect over mockup pixels (x0,y0)-(x1,y1), for text or content.</summary>
        public RectTransform Slot(Transform parent, string name, float x0, float y0, float x1, float y1)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            UiKit.Anchor(rect, new Vector2(0f, 1f), new Vector2(x0 * K, -y0 * K));
            rect.sizeDelta = new Vector2((x1 - x0) * K, (y1 - y0) * K);
            return rect;
        }

        /// <summary>
        /// Margins (left, bottom, right, top, as fractions) of a label region inside a plate placed with
        /// <see cref="At"/> over the crop (x0,y0)-(x1,y1); the region is in mockup pixels.
        /// </summary>
        public Vector4 Region(float x0, float y0, float x1, float y1, float rx0, float ry0, float rx1, float ry1)
        {
            var w = x1 - x0 + 2f * Pad;
            var h = y1 - y0 + 2f * Pad;
            return new Vector4((rx0 - x0 + Pad) / w, (y1 + Pad - ry1) / h, (x1 + Pad - rx1) / w, (ry0 - y0 + Pad) / h);
        }

        /// <summary>The click area of a padded plate is the drawn plate, not its transparent margin.</summary>
        public void TrimHitArea(Image face)
        {
            var pad = Pad * K;
            face.raycastPadding = new Vector4(pad, pad, pad, pad);
        }

        private RectTransform[] _bars;

        /// <summary>A stretched container named Scenery inside the stage, for the pictures that are only decoration.</summary>
        public RectTransform Scenery()
        {
            var rect = new GameObject("Scenery", typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(transform, false);
            UiKit.Stretch(rect);
            return rect;
        }

        /// <summary>
        /// A solid colour on the part of the screen the stage does not cover: four bars around it, never
        /// over it, so an opening in the stage still shows what is behind the interface (the live room).
        /// </summary>
        public void Letterbox(Color colour)
        {
            _bars = new RectTransform[4];

            // Named Scenery: decoration, which no layout rule measures as content.
            var holder = new GameObject("Scenery", typeof(RectTransform)).GetComponent<RectTransform>();
            holder.SetParent(transform.parent, false);
            holder.SetSiblingIndex(transform.GetSiblingIndex());
            UiKit.Stretch(holder);

            for (var i = 0; i < 4; i++)
            {
                var go = new GameObject("Letterbox" + i, typeof(RectTransform));
                go.transform.SetParent(holder, false);
                var image = go.AddComponent<Image>();
                image.color = colour;
                image.raycastTarget = false;
                _bars[i] = image.rectTransform;
            }

            Fit();
        }

        private void FitBars(RectTransform room, float scale)
        {
            if (_bars == null) return;

            var w = Size.x * scale;
            var h = Size.y * scale;
            var side = Mathf.Max(0f, (room.rect.width - w) * 0.5f) + 1f;
            var edge = Mathf.Max(0f, (room.rect.height - h) * 0.5f) + 1f;

            // left, right, top, bottom; the sides only as tall as the stage, so no bar overlaps another.
            Bar(_bars[0], new Vector2(0f, 0.5f), new Vector2(side, h));
            Bar(_bars[1], new Vector2(1f, 0.5f), new Vector2(side, h));
            Bar(_bars[2], new Vector2(0.5f, 1f), new Vector2(room.rect.width, edge));
            Bar(_bars[3], new Vector2(0.5f, 0f), new Vector2(room.rect.width, edge));
            for (var i = 0; i < 4; i++) _bars[i].gameObject.SetActive(i < 2 ? side > 1.5f : edge > 1.5f);
        }

        private static void Bar(RectTransform bar, Vector2 anchor, Vector2 size)
        {
            bar.anchorMin = bar.anchorMax = anchor;
            bar.pivot = anchor;
            bar.anchoredPosition = Vector2.zero;
            bar.sizeDelta = size;
        }
    }
}
