using System;

namespace Neko.Sim.Core
{
    /// <summary>
    /// The arithmetic the design's curves are written in.
    /// </summary>
    /// <remarks>
    /// These live in <c>Neko.Sim</c> rather than being taken from <c>UnityEngine.Mathf</c>
    /// because the simulation must compile and run without Unity — which is what lets a
    /// twenty-day balance run finish in a unit test instead of in the editor.
    /// </remarks>
    public static class Curves
    {
        /// <summary>
        /// <c>scale * step^exponent</c>, the shape every progression curve in the game uses.
        /// Step 0 is zero rather than undefined, so "before day one" is a usable input.
        /// </summary>
        public static float Power(float scale, float exponent, int step) =>
            step <= 0 ? 0f : scale * (float)Math.Pow(step, exponent);

        public static float Clamp01(float value) => Clamp(value, 0f, 1f);

        /// <summary>
        /// Clamps, tolerating a reversed pair. A swapped min and max is a typo, and the
        /// honest response is the value the author meant rather than a silent inversion.
        /// </summary>
        public static float Clamp(float value, float a, float b)
        {
            var min = Math.Min(a, b);
            var max = Math.Max(a, b);
            if (value < min) return min;
            return value > max ? max : value;
        }

        public static int ClampInt(int value, int a, int b)
        {
            var min = Math.Min(a, b);
            var max = Math.Max(a, b);
            if (value < min) return min;
            return value > max ? max : value;
        }

        public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp01(t);

        public static float InverseLerp(float a, float b, float value) =>
            Math.Abs(b - a) < 1e-6f ? 0f : Clamp01((value - a) / (b - a));

        /// <summary>
        /// Remaps and clamps. A zero-width input range returns the low output instead of
        /// infinity, because a curve whose ends happen to meet should flatten, not explode.
        /// </summary>
        public static float Remap(float value, float inA, float inB, float outA, float outB) =>
            Lerp(outA, outB, InverseLerp(inA, inB, value));

        public static float SmoothStep(float t)
        {
            var x = Clamp01(t);
            return x * x * (3f - 2f * x);
        }
    }
}
