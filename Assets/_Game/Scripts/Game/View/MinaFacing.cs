using System;

namespace Neko.Game.View
{
    /// <summary>
    /// Which way the Mina experiment is drawn, from how far she moved this frame.
    /// </summary>
    /// <remarks>
    /// Kept free of Unity so the rules can be tested directly. Two holds stop the picture
    /// from chattering: a direction hold, so a near-diagonal path does not flip between axes
    /// every frame, and a movement hold, so a frame without a step does not drop her into
    /// the idle pose mid-walk. Once the movement hold runs out she is stopped, and the view
    /// draws her standing rather than frozen in a stride.
    /// </remarks>
    public sealed class MinaFacing
    {
        public const float DirectionHold = 0.15f;
        public const float MovementHold = 0.12f;

        /// <summary>Squared distance per frame below which she has not moved.</summary>
        public const float StillSquared = 1e-5f;

        /// <summary>How much wider than tall a step must be to turn sideways, and to stay so.</summary>
        public const float EnterSide = 1.25f;
        public const float StaySide = 0.8f;

        /// <summary>
        /// How wide a step must be, outside a sideways leg, to update the left/right heading.
        /// </summary>
        /// <remarks>
        /// Workers walk straight at their target, so most legs are diagonal. A carry drawn from
        /// the front still faces a side, and keeping the heading of the last sideways leg had
        /// her carry a plate down and to the left while facing right.
        /// </remarks>
        public const float KeepHeading = 0.35f;

        private float _directionHold;
        private float _movementHold;

        public MinaVisual.Facing Facing { get; private set; } = MinaVisual.Facing.Front;

        /// <summary>Last horizontal heading. Kept through vertical legs so a flip never jumps.</summary>
        public bool Left { get; private set; }

        public bool Moving { get; private set; }

        /// <summary>Advances by one frame. Returns whether the facing or heading changed.</summary>
        public bool Step(float dx, float dy, float dt)
        {
            _directionHold = Math.Max(0f, _directionHold - dt);

            if (dx * dx + dy * dy <= StillSquared)
            {
                _movementHold = Math.Max(0f, _movementHold - dt);
                Moving = _movementHold > 0f;
                return false;
            }

            _movementHold = MovementHold;
            Moving = true;

            var horizontal = Math.Abs(dx);
            var vertical = Math.Abs(dy);
            var side = horizontal > vertical *
                (Facing == MinaVisual.Facing.Side ? StaySide : EnterSide);
            var facing = side ? MinaVisual.Facing.Side :
                dy > 0f ? MinaVisual.Facing.Back : MinaVisual.Facing.Front;
            var left = side || horizontal > vertical * KeepHeading ? dx < 0f : Left;

            if (_directionHold > 0f || (facing == Facing && left == Left)) return false;

            Facing = facing;
            Left = left;
            _directionHold = DirectionHold;
            return true;
        }
    }
}
