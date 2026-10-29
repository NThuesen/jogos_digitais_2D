using Neko.Game.View;
using NUnit.Framework;

namespace Neko.Tests.Game
{
    /// <summary>
    /// The Mina experiment's heading and stopping rules, without Unity in the loop.
    /// </summary>
    /// <remarks>
    /// The first capture of this experiment left her standing at a table, mid-stride, with her
    /// back to the room: the heading never returned to standing. These tests pin the rules the
    /// review asked for: the right drawing per heading, no chatter on a diagonal, and the idle
    /// once the movement hold runs out.
    /// </remarks>
    public sealed class MinaFacingTests
    {
        private const float Frame = 1f / 25f;
        private const float Step = 0.08f;

        private static MinaFacing Walk(MinaFacing facing, float dx, float dy, int frames)
        {
            for (var i = 0; i < frames; i++) facing.Step(dx, dy, Frame);
            return facing;
        }

        [Test]
        public void RightAndLeftAreSideTurnedByHeading()
        {
            var facing = Walk(new MinaFacing(), Step, 0f, 2);
            Assert.That(facing.Facing, Is.EqualTo(MinaVisual.Facing.Side));
            Assert.That(facing.Left, Is.False);

            Walk(facing, -Step, 0f, 10);
            Assert.That(facing.Facing, Is.EqualTo(MinaVisual.Facing.Side));
            Assert.That(facing.Left, Is.True);
        }

        [Test]
        public void UpIsBackAndDownIsFront()
        {
            var facing = Walk(new MinaFacing(), 0f, Step, 2);
            Assert.That(facing.Facing, Is.EqualTo(MinaVisual.Facing.Back));

            Walk(facing, 0f, -Step, 10);
            Assert.That(facing.Facing, Is.EqualTo(MinaVisual.Facing.Front));
        }

        [Test]
        public void ANearDiagonalKeepsWhicheverAxisItAlreadyHas()
        {
            // Wider than tall, but not by enough to leave a vertical heading...
            var vertical = Walk(new MinaFacing(), 0f, Step, 2);
            Walk(vertical, Step, Step * 0.9f, 20);
            Assert.That(vertical.Facing, Is.EqualTo(MinaVisual.Facing.Back));

            // ...and taller than wide, but not by enough to leave a side heading.
            var side = Walk(new MinaFacing(), Step, 0f, 2);
            Walk(side, Step, Step * 1.1f, 20);
            Assert.That(side.Facing, Is.EqualTo(MinaVisual.Facing.Side));
        }

        [Test]
        public void AClearlyWiderStepTurnsSideways()
        {
            var facing = Walk(new MinaFacing(), 0f, Step, 2);
            Walk(facing, Step, Step * 0.7f, 10);
            Assert.That(facing.Facing, Is.EqualTo(MinaVisual.Facing.Side));
        }

        [Test]
        public void TheDirectionHoldAbsorbsAOneFrameWobble()
        {
            var facing = Walk(new MinaFacing(), Step, 0f, 1);
            Assert.That(facing.Step(0f, Step, Frame), Is.False, "a turn inside the hold");
            Assert.That(facing.Facing, Is.EqualTo(MinaVisual.Facing.Side));
        }

        [Test]
        public void AQuickReversalTurnsOnceTheHoldRunsOut()
        {
            var facing = Walk(new MinaFacing(), Step, 0f, 1);
            var turns = 0;
            for (var i = 0; i < 10; i++)
                if (facing.Step(-Step, 0f, Frame)) turns++;

            Assert.That(turns, Is.EqualTo(1));
            Assert.That(facing.Left, Is.True);
        }

        [Test]
        public void StoppingEndsTheWalkAfterTheMovementHold()
        {
            var facing = Walk(new MinaFacing(), Step, 0f, 5);
            facing.Step(0f, 0f, Frame);
            Assert.That(facing.Moving, Is.True, "one frame without a step is not a stop");

            Walk(facing, 0f, 0f, 3);
            Assert.That(facing.Moving, Is.False);
            Assert.That(MinaVisual.NameFor(MaidPose.Walk, facing.Facing, facing.Moving), Is.EqualTo("idle"));
        }

        [Test]
        public void StandingStillNeverTurns()
        {
            var facing = Walk(new MinaFacing(), Step, 0f, 5);
            var turns = 0;
            for (var i = 0; i < 50; i++)
                if (facing.Step(0f, 0f, Frame)) turns++;
            Assert.That(turns, Is.Zero);
        }

        [Test]
        public void EveryStandingStateIsTheIdleExceptCarry()
        {
            foreach (MinaVisual.Facing heading in System.Enum.GetValues(typeof(MinaVisual.Facing)))
            {
                Assert.That(MinaVisual.NameFor(MaidPose.Idle, heading, false), Is.EqualTo("idle"));
                Assert.That(MinaVisual.NameFor(MaidPose.Cook, heading, false), Is.EqualTo("idle"));
                Assert.That(MinaVisual.NameFor(MaidPose.Tired, heading, false), Is.EqualTo("idle"));
                Assert.That(MinaVisual.NameFor(MaidPose.Carry, heading, false), Is.EqualTo("carry"));
            }
        }

        [Test]
        public void CarryNeverWalksBackwards()
        {
            Assert.That(MinaVisual.NameFor(MaidPose.Carry, MinaVisual.Facing.Side, true), Is.EqualTo("carry"));
            Assert.That(MinaVisual.Mirrors("carry"), Is.True, "carry turns left with her heading");
            Assert.That(MinaVisual.NameFor(MaidPose.Carry, MinaVisual.Facing.Back, true), Is.EqualTo("back"));
            Assert.That(MinaVisual.NameFor(MaidPose.Carry, MinaVisual.Facing.Front, true), Is.EqualTo("carry"));
        }

        [Test]
        public void ADiagonalLegTurnsTheHeadingItCarriesWith()
        {
            // Last sideways leg went right; the plate goes down and to the left.
            var facing = Walk(new MinaFacing(), Step, 0f, 5);
            Walk(facing, -Step * 0.5f, -Step, 10);

            Assert.That(facing.Facing, Is.EqualTo(MinaVisual.Facing.Front));
            Assert.That(facing.Left, Is.True, "the 3/4 carry must face the way she crosses the screen");
        }

        [Test]
        public void AStraightVerticalLegKeepsTheLastHeading()
        {
            var facing = Walk(new MinaFacing(), -Step, 0f, 5);
            Walk(facing, Step * 0.1f, -Step, 10);
            Assert.That(facing.Left, Is.True);
        }

        [Test]
        public void CookAndTiredWalkWithTheirHeading()
        {
            Assert.That(MinaVisual.NameFor(MaidPose.Cook, MinaVisual.Facing.Side, true), Is.EqualTo("side"));
            Assert.That(MinaVisual.NameFor(MaidPose.Tired, MinaVisual.Facing.Back, true), Is.EqualTo("back"));
            Assert.That(MinaVisual.NameFor(MaidPose.Tired, MinaVisual.Facing.Front, true), Is.EqualTo("walk"));
        }
    }
}
