using Neko.Game.Art;
using Neko.Sim.Cafe;
using UnityEngine;

namespace Neko.Game.Core
{
    /// <summary>
    /// Frames the room and shakes when something goes wrong.
    /// </summary>
    /// <remarks>
    /// Orthographic size is solved from the room rather than set by hand, because the room
    /// grows four times over the course of a game and a camera tuned for the first one clips
    /// the last. It is solved against the <em>narrowest</em> aspect the game supports, so a
    /// wide window shows more rather than a narrow one showing less.
    /// </remarks>
    public sealed class CameraRig : MonoBehaviour
    {
        /// <summary>The narrowest window the layout is guaranteed against: 4:3.</summary>
        public const float NarrowestAspect = 4f / 3f;

        /// <summary>Tiles of air left around the room so nothing touches the edge.</summary>
        /// <summary>
        /// Tiles of air around the room.
        /// </summary>
        /// <remarks>
        /// Solved against 4:3 and then seen at 16:9, which is most of a tile of extra air
        /// on each side already. At 1.6 on top of that the café sat in the middle of a
        /// very large empty floor.
        /// </remarks>
        public const float Margin = 0.9f;

        private Vector3 _home;
        private float _shakeSeconds;
        private float _shakeAmplitude;

        public Camera Camera { get; private set; }

        private void Awake()
        {
            Camera = GetComponent<Camera>();
            if (Camera == null) Camera = gameObject.AddComponent<Camera>();

            // Without this the whole game is silent and Unity says so once, as a log line
            // nobody reads. A PlayMode test now fails instead.
            if (GetComponent<AudioListener>() == null) gameObject.AddComponent<AudioListener>();

            Camera.orthographic = true;
            Camera.clearFlags = CameraClearFlags.SolidColor;
            // Darker than the floor, so the room reads as a room inside a space
            // rather than as furniture floating on a sheet of the same colour.
            Camera.backgroundColor = Palette.Darken(Palette.Wall, 0.45f);
            Camera.transform.position = new Vector3(0f, 0f, -10f);
            Camera.nearClipPlane = -20f;
            Camera.farClipPlane = 50f;
        }

        public void FrameRoom(int width, int height)
        {
            var centre = new Vector3((width - 1) * 0.5f, (height - 1) * 0.5f, -10f);
            transform.position = centre;
            _home = centre;

            var wantedHeight = (height + Margin * 2f) * 0.5f;
            var wantedWidth = (width + Margin * 2f) * 0.5f / NarrowestAspect;

            Camera.orthographicSize = Mathf.Max(wantedHeight, wantedWidth);
        }

        public void Shake(float amplitude, float seconds)
        {
            _shakeAmplitude = Mathf.Max(_shakeAmplitude, amplitude);
            _shakeSeconds = Mathf.Max(_shakeSeconds, seconds);
        }

        public Vector2 ScreenToWorld(Vector2 screen) => Camera.ScreenToWorldPoint(screen);

        private void LateUpdate()
        {
            if (_shakeSeconds <= 0f)
            {
                if (transform.position != _home) transform.position = _home;
                return;
            }

            _shakeSeconds -= Clocks.Ui;

            // The one place UnityEngine.Random is used. A camera shake is not simulation:
            // nothing reads it back, and a shift replays identically whatever it does.
            var strength = _shakeAmplitude * Mathf.Clamp01(_shakeSeconds * 4f);

            transform.position = _home + new Vector3(
                Random.Range(-strength, strength), Random.Range(-strength, strength), 0f);

            if (_shakeSeconds > 0f) return;

            _shakeAmplitude = 0f;
            transform.position = _home;
        }
    }
}
