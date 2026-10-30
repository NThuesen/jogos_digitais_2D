using UnityEngine;

namespace Neko.Game.View
{
    /// <summary>
    /// Which frame of an authored walk cycle to show, driven by distance rather than by time.
    /// </summary>
    /// <remarks>
    /// The frames are a walk <em>in place</em>; the simulation owns where she goes. Advancing the
    /// cycle by the distance actually covered keeps the feet from sliding at any walking speed
    /// and makes pause, slow motion and fast-forward correct without a single special case: no
    /// movement, no new frame. When she stops, the cycle keeps going to the nearest frame with
    /// both shoes on the floor instead of freezing mid-stride.
    /// </remarks>
    public sealed class WalkCycle
    {
        /// <summary>Slower than this, in world units per second, and she counts as standing.</summary>
        public const float MinSpeed = 0.25f;

        /// <summary>How fast the cycle runs on to its resting frame once she has stopped.</summary>
        public const float SettleFramesPerSecond = 60f;

        private readonly int _frames;
        private readonly float _unitsPerCycle;
        private readonly int[] _rest;

        /// <summary>Position in the cycle, in frames. The fraction is kept so slow walks stay smooth.</summary>
        public float Phase { get; private set; }

        public bool Moving { get; private set; }

        public int Frame => Mathf.Clamp(Mathf.FloorToInt(Phase), 0, _frames - 1);

        /// <param name="frames">Frames in one full cycle (two steps).</param>
        /// <param name="unitsPerCycle">World distance covered by one full cycle.</param>
        /// <param name="rest">Frames where she stands on both feet. At least one.</param>
        /// <param name="start">Frame to begin on, normally one of <paramref name="rest"/>.</param>
        public WalkCycle(int frames, float unitsPerCycle, int[] rest, int start)
        {
            _frames = frames;
            _unitsPerCycle = unitsPerCycle;
            _rest = rest;
            Phase = start;
        }

        public bool AtRest => System.Array.IndexOf(_rest, Frame) >= 0;

        /// <summary>
        /// Advances by one rendered frame.
        /// </summary>
        /// <param name="distance">How far she moved on screen this frame.</param>
        /// <param name="seconds">Game time of the frame. Zero while paused, which freezes it.</param>
        public void Step(float distance, float seconds)
        {
            if (seconds <= 0f) return;

            Moving = distance / seconds > MinSpeed;
            if (Moving)
            {
                Phase = Mathf.Repeat(Phase + distance / _unitsPerCycle * _frames, _frames);
                return;
            }

            if (AtRest) return;

            var next = NextRestAhead();
            var gap = Mathf.Repeat(next - Phase, _frames);
            var step = SettleFramesPerSecond * seconds;
            Phase = step >= gap ? next : Mathf.Repeat(Phase + step, _frames);
        }

        private int NextRestAhead()
        {
            var best = _rest[0];
            var bestGap = float.MaxValue;
            for (var i = 0; i < _rest.Length; i++)
            {
                var gap = Mathf.Repeat(_rest[i] - Phase, _frames);
                if (gap < bestGap)
                {
                    bestGap = gap;
                    best = _rest[i];
                }
            }

            return best;
        }
    }
}
