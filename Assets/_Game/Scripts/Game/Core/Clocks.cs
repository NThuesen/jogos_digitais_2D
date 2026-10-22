using UnityEngine;

namespace Neko.Game.Core
{
    /// <summary>
    /// Two clocks: one the game runs on, one the interface runs on.
    /// </summary>
    /// <remarks>
    /// Separate because pausing, and the brief slow-motion on a level-up, must stop the café
    /// without stopping the menu that is asking whether to resume. Tying both to
    /// <c>Time.deltaTime</c> is how a pause screen ends up frozen behind its own fade.
    /// </remarks>
    public static class Clocks
    {
        private static float _scale = 1f;

        public static bool Paused { get; private set; }

        /// <summary>Seconds since the last frame for anything in the café.</summary>
        public static float Game => Paused ? 0f : Time.unscaledDeltaTime * _scale;

        /// <summary>Seconds since the last frame for menus, tweens and popups.</summary>
        public static float Ui => Time.unscaledDeltaTime;

        /// <summary>Slow motion. 1 is normal; the level-up flourish dips to about 0.35.</summary>
        public static float Scale
        {
            get => _scale;
            set => _scale = Mathf.Clamp(value, 0.05f, 4f);
        }

        public static void Pause(bool paused) => Paused = paused;

        /// <summary>Back to a known state. Called whenever a screen is entered.</summary>
        public static void Reset()
        {
            Paused = false;
            _scale = 1f;
        }
    }
}
