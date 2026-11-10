using System;
using UnityEngine;

namespace Neko.Game.Ui
{
    /// <summary>
    /// The rectangle of the screen that shows the real room (the Hall's board, the gameplay frame).
    /// </summary>
    /// <remarks>
    /// The room is not an image: it is the live world, drawn by the one camera behind the canvas. A screen
    /// that wants it inside a picture frame puts this on the rectangle of the frame's opening. It does two
    /// things and nothing else: it tells whoever frames the camera where the room has to fit (the
    /// <see cref="Changed"/> event, again whenever the window or the layout changes), and it answers whether a
    /// pointer position is inside it, so a click on scenery or on the frame never reaches the room.
    /// Pixel rectangles are in the camera's own pixels, the space <c>Camera.ScreenToWorldPoint</c> takes.
    /// </remarks>
    public sealed class RoomWindow : MonoBehaviour
    {
        private Rect _last;
        private Vector2 _lastCamera;
        private bool _hasLast;

        /// <summary>The window being shown, or null when the room fills the screen.</summary>
        public static RoomWindow Active { get; private set; }

        /// <summary>Called with the window in camera pixels when it appears and whenever it moves or resizes.</summary>
        public event Action<Rect> Changed;

        private Canvas RootCanvas => GetComponentInParent<Canvas>()?.rootCanvas;

        /// <summary>The window in camera pixels, or false while the canvas has no camera or no size yet.</summary>
        public bool TryPixels(out Rect pixels)
        {
            pixels = default;

            var canvas = RootCanvas;
            var camera = canvas != null ? canvas.worldCamera : null;
            if (camera == null || camera.pixelWidth <= 0) return false;

            var corners = new Vector3[4];
            ((RectTransform)transform).GetWorldCorners(corners);

            var low = (Vector2)camera.WorldToScreenPoint(corners[0]);
            var high = (Vector2)camera.WorldToScreenPoint(corners[2]);
            pixels = Rect.MinMaxRect(Mathf.Min(low.x, high.x), Mathf.Min(low.y, high.y),
                Mathf.Max(low.x, high.x), Mathf.Max(low.y, high.y));
            return pixels.width > 1f && pixels.height > 1f;
        }

        /// <summary>True when the room fills the screen, or the point is inside the window.</summary>
        public static bool Contains(Vector2 screen)
        {
            if (Active == null || !Active.isActiveAndEnabled) return true;
            return Active.TryPixels(out var pixels) && pixels.Contains(screen);
        }

        /// <summary>Asks for the window again, as after the room was rebuilt.</summary>
        public void Reframe()
        {
            _hasLast = false;
            Canvas.ForceUpdateCanvases();
            Publish();
        }

        private void OnEnable()
        {
            Active = this;
            _hasLast = false;
        }

        private void OnDisable()
        {
            if (Active == this) Active = null;
        }

        private void LateUpdate() => Publish();

        private void Publish()
        {
            if (!TryPixels(out var pixels)) return;

            var camera = RootCanvas.worldCamera;
            var size = new Vector2(camera.pixelWidth, camera.pixelHeight);
            if (_hasLast && Approximately(pixels, _last) && size == _lastCamera) return;

            _last = pixels;
            _lastCamera = size;
            _hasLast = true;
            Changed?.Invoke(pixels);
        }

        private static bool Approximately(Rect a, Rect b) =>
            Mathf.Abs(a.xMin - b.xMin) < 0.5f && Mathf.Abs(a.yMin - b.yMin) < 0.5f &&
            Mathf.Abs(a.width - b.width) < 0.5f && Mathf.Abs(a.height - b.height) < 0.5f;
    }
}
