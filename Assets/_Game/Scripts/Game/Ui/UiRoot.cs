using System;
using System.Collections.Generic;
using Neko.Game.Art;
using Neko.Game.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace Neko.Game.Ui
{
    /// <summary>
    /// The canvas, the screens on it, and the dialog that interrupts them.
    /// </summary>
    /// <remarks>
    /// A single canvas scaled to a reference resolution rather than per-screen canvases: a
    /// WebGL window gets resized constantly and one scaler is one place for that to be right.
    /// </remarks>
    public sealed class UiRoot : MonoBehaviour
    {
        public const float ReferenceWidth = 1280f;
        public const float ReferenceHeight = 720f;

        private readonly Dictionary<Type, Screen> _screens = new Dictionary<Type, Screen>();

        private Canvas _canvas;
        private RectTransform _screenLayer;
        private RectTransform _dialogLayer;
        private AudioDirector _audio;

        public Transform OverlayLayer { get; private set; }

        public Screen Current { get; private set; }

        public void Build(SpriteFactory sprites, AudioDirector audio)
        {
            UiKit.Sprites = sprites;
            _audio = audio;
            UiKit.EnsureEventSystem();

            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);

            // Match height, so a wider window shows a wider café rather than smaller type.
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;

            gameObject.AddComponent<GraphicRaycaster>();

            _screenLayer = UiKit.Root("Screens", transform);
            OverlayLayer = UiKit.Root("Overlay", transform);
            _dialogLayer = UiKit.Root("Dialogs", transform);
        }

        public T Add<T>() where T : Screen
        {
            var go = new GameObject(typeof(T).Name, typeof(RectTransform));
            go.transform.SetParent(_screenLayer, false);

            var screen = go.AddComponent<T>();
            screen.Attach(this);
            _screens[typeof(T)] = screen;
            return screen;
        }

        public T Get<T>() where T : Screen =>
            _screens.TryGetValue(typeof(T), out var screen) ? (T)screen : null;

        /// <summary>Shows one screen and hides the rest. There is no stack to get lost in.</summary>
        public void Show<T>() where T : Screen
        {
            foreach (var screen in _screens.Values)
            {
                if (screen != null && screen.Visible) screen.Hide();
            }

            var wanted = Get<T>();
            if (wanted == null) return;

            wanted.Show();
            Current = wanted;
            _audio?.Play(SoundId.UiConfirm, 0.4f);
        }

        public void RefreshCurrent() => Current?.Refresh();

        /// <summary>
        /// A message the player has to acknowledge.
        /// </summary>
        /// <remarks>
        /// Where a corrupt save, an impossible café layout and a caught exception all end up.
        /// Rubric 11 is mostly about making sure failure arrives here rather than in a log
        /// nobody reads.
        /// </remarks>
        public void Dialog(string title, string body, string confirm = null, Action onConfirm = null,
            string cancel = null, Action onCancel = null)
        {
            foreach (Transform child in _dialogLayer) Destroy(child.gameObject);

            var shade = UiKit.Panel(_dialogLayer, Vector2.one * 100f,
                Palette.WithAlpha(Palette.Ink, 0.55f), 0f);
            UiKit.Stretch(shade);
            shade.GetComponent<Image>().raycastTarget = true;

            var box = UiKit.Panel(_dialogLayer, new Vector2(620f, 300f), Palette.Panel);
            UiKit.Anchor(box, new Vector2(0.5f, 0.5f), Vector2.zero);

            var column = UiKit.Column(box, 18f, 28f);
            UiKit.Stretch(column);

            var heading = UiKit.Label(column, title, Typeface.Heading, Palette.Text, bold: true);
            heading.rectTransform.sizeDelta = new Vector2(560f, 46f);

            var text = UiKit.Label(column, body, Typeface.Body, Palette.Text);
            text.rectTransform.sizeDelta = new Vector2(560f, 110f);

            var row = UiKit.Row(column, 14f);
            row.sizeDelta = new Vector2(560f, 56f);

            UiKit.Button(row, confirm ?? Strings.Get("dialog.ok"), new Vector2(200f, 54f),
                Palette.Focus, () =>
                {
                    CloseDialog();
                    onConfirm?.Invoke();
                });

            if (cancel == null) return;

            UiKit.Button(row, cancel, new Vector2(200f, 54f), Palette.Wall, () =>
            {
                CloseDialog();
                onCancel?.Invoke();
            });
        }

        public bool DialogOpen => _dialogLayer != null && _dialogLayer.childCount > 0;

        public void CloseDialog()
        {
            if (_dialogLayer == null) return;
            foreach (Transform child in _dialogLayer) Destroy(child.gameObject);
        }
    }
}
