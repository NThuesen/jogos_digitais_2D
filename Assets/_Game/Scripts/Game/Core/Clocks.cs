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

        /// <summary>
        /// The longest frame either clock will admit to.
        /// </summary>
        /// <remarks>
        /// The editor hands the game its domain reload as the first frame's delta, and this
        /// project's log times that reload at 3.9 s. Nothing here wants to advance by that:
        /// the café would serve several seconds of customers between two drawn frames, and
        /// every animation would snap to its end on the frame it started. Unity clamps its
        /// own <c>Time.deltaTime</c> for the same reason, and this is the second lock on the
        /// crash that <see cref="Feel.Tween"/> explains — a long frame can no longer reach
        /// anything.
        /// </remarks>
        public const float LongestFrame = 0.1f;

        public static bool Paused { get; private set; }

        /// <summary>Seconds since the last frame for anything in the café.</summary>
        public static float Game => Paused ? 0f : Trim(Measured) * _scale;

        /// <summary>Seconds since the last frame for menus, tweens and popups.</summary>
        public static float Ui => Trim(Measured);

#if UNITY_EDITOR
        /// <summary>
        /// Editor evidence runs only: a fixed frame length in place of the measured one.
        /// </summary>
        /// <remarks>
        /// <c>Time.captureDeltaTime</c> does not reach <c>Time.unscaledDeltaTime</c> — measured
        /// in a 6000.5.9f1 batch run, it stayed at the real 1/60 s — and both clocks read the
        /// unscaled one. Saving a frame takes longer than a frame, so a recording without this
        /// shows the café jumping forward between the frames it kept. Zero means measured.
        /// </remarks>
        internal static float FixedFrame;

        private static float Measured => FixedFrame > 0f ? FixedFrame : Time.unscaledDeltaTime;
#else
        private static float Measured => Time.unscaledDeltaTime;
#endif

        /// <summary>A frame length, with an outlier cut down to something playable.</summary>
        public static float Trim(float seconds) => Mathf.Min(seconds, LongestFrame);

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
