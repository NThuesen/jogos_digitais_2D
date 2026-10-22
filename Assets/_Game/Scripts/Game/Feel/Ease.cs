using UnityEngine;

namespace Neko.Game.Feel
{
    /// <summary>
    /// The curves everything in the game moves on.
    /// </summary>
    /// <remarks>
    /// Every one of them starts at 0 and ends at 1, which is what lets them be swapped freely;
    /// a test pins that. The overshooting ones are the whole of the game's feel — nothing in a
    /// café this cute should arrive at its destination and simply stop.
    /// </remarks>
    public static class Ease
    {
        public static float Linear(float t) => Mathf.Clamp01(t);

        public static float OutCubic(float t)
        {
            var x = 1f - Mathf.Clamp01(t);
            return 1f - x * x * x;
        }

        public static float InOutQuad(float t)
        {
            var x = Mathf.Clamp01(t);
            return x < 0.5f ? 2f * x * x : 1f - Mathf.Pow(-2f * x + 2f, 2f) / 2f;
        }

        /// <summary>Overshoots and settles. The one on every button and every pickup.</summary>
        public static float OutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            var x = Mathf.Clamp01(t) - 1f;
            return 1f + c3 * x * x * x + c1 * x * x;
        }

        public static float OutElastic(float t)
        {
            var x = Mathf.Clamp01(t);
            if (x <= 0f) return 0f;
            if (x >= 1f) return 1f;

            const float c4 = 2f * Mathf.PI / 3f;
            return Mathf.Pow(2f, -10f * x) * Mathf.Sin((x * 10f - 0.75f) * c4) + 1f;
        }

        /// <summary>Up and back down. For a one-shot pop that returns to where it started.</summary>
        public static float Pop(float t)
        {
            var x = Mathf.Clamp01(t);
            return Mathf.Sin(x * Mathf.PI);
        }
    }
}
