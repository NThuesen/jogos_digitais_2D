using Neko.Game.Core;
using Neko.Game.Ui;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Game.Feel
{
    /// <summary>
    /// A number that rises off whatever just happened and fades.
    /// </summary>
    /// <remarks>
    /// Lives on the UI canvas rather than in the world so the type stays the same size however
    /// far the camera has zoomed out, which it does by nearly half over ten café levels.
    /// </remarks>
    public sealed class FloatingLabel : MonoBehaviour
    {
        private const float Life = 1.0f;
        private const float Rise = 56f;

        private Text _text;
        private RectTransform _rect;
        private Camera _camera;
        private Vector3 _world;
        private float _age;

        public bool Alive => _age < Life;

        public static FloatingLabel Create(Transform parent)
        {
            var go = new GameObject("FloatingLabel", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var label = go.AddComponent<FloatingLabel>();
            label._rect = go.GetComponent<RectTransform>();
            label._rect.sizeDelta = new Vector2(220f, 44f);

            label._text = go.AddComponent<Text>();
            label._text.font = Typeface.Font;
            label._text.fontSize = Typeface.Body;
            label._text.fontStyle = FontStyle.Bold;
            label._text.alignment = TextAnchor.MiddleCenter;
            label._text.raycastTarget = false;
            label._text.horizontalOverflow = HorizontalWrapMode.Overflow;
            label._text.verticalOverflow = VerticalWrapMode.Overflow;

            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.55f);
            outline.effectDistance = new Vector2(1.6f, -1.6f);

            go.SetActive(false);
            return label;
        }

        public void Fire(string text, Vector3 world, Color colour)
        {
            _camera = Camera.main;
            _world = world;
            _age = 0f;
            _text.text = text;
            _text.color = colour;

            gameObject.SetActive(true);
            Place();
        }

        private void Update()
        {
            _age += Clocks.Ui;

            if (_age >= Life)
            {
                gameObject.SetActive(false);
                return;
            }

            Place();

            var t = _age / Life;
            var alpha = t < 0.65f ? 1f : 1f - (t - 0.65f) / 0.35f;
            _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, alpha);

            var scale = 0.8f + Ease.OutBack(Mathf.Clamp01(t / 0.25f)) * 0.3f;
            _rect.localScale = Vector3.one * scale;
        }

        private void Place()
        {
            if (_camera == null) _camera = Camera.main;
            if (_camera == null) return;

            var screen = _camera.WorldToScreenPoint(_world);
            var canvas = _rect.parent as RectTransform;
            if (canvas == null) return;

            var canvasComponent = _rect.GetComponentInParent<Canvas>();
            var uiCamera = canvasComponent != null &&
                           canvasComponent.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvasComponent.worldCamera
                : null;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas, screen, uiCamera, out var local);

            _rect.anchoredPosition = local + new Vector2(0f, _age / Life * Rise);
        }
    }
}
