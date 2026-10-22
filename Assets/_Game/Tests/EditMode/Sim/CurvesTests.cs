using Neko.Sim.Core;
using NUnit.Framework;

namespace Neko.Tests.Sim
{
    public sealed class CurvesTests
    {
        [Test]
        public void PowerMatchesTheQuotaCurve()
        {
            // The day quota in the design is quota(n) = 120 * n^1.35.
            Assert.That(Curves.Power(120f, 1.35f, 1), Is.EqualTo(120f).Within(0.01f));
            Assert.That(Curves.Power(120f, 1.35f, 10), Is.EqualTo(120f * 22.3872f).Within(1f));
        }

        [Test]
        public void PowerIsStrictlyIncreasing()
        {
            var previous = float.MinValue;

            for (var n = 1; n <= 60; n++)
            {
                var value = Curves.Power(120f, 1.35f, n);
                Assert.Greater(value, previous);
                previous = value;
            }
        }

        [Test]
        public void PowerAtStepZeroIsZeroRatherThanUndefined()
        {
            Assert.AreEqual(0f, Curves.Power(120f, 1.35f, 0));
        }

        [Test]
        public void Clamp01Clamps()
        {
            Assert.AreEqual(0f, Curves.Clamp01(-3f));
            Assert.AreEqual(1f, Curves.Clamp01(4f));
            Assert.AreEqual(0.25f, Curves.Clamp01(0.25f));
        }

        [Test]
        public void ClampHandlesReversedBounds()
        {
            Assert.AreEqual(5f, Curves.Clamp(1f, 5f, 9f));
            Assert.AreEqual(5f, Curves.Clamp(1f, 9f, 5f), "a reversed pair must not invert the clamp");
        }

        [Test]
        public void RemapMovesBetweenRanges()
        {
            Assert.That(Curves.Remap(5f, 0f, 10f, 0f, 100f), Is.EqualTo(50f).Within(1e-4f));
            Assert.That(Curves.Remap(0f, 0f, 10f, 20f, 30f), Is.EqualTo(20f).Within(1e-4f));
        }

        [Test]
        public void RemapOfADegenerateRangeReturnsTheLowOutputRatherThanInfinity()
        {
            Assert.AreEqual(20f, Curves.Remap(5f, 3f, 3f, 20f, 30f));
        }

        [Test]
        public void RemapIsClampedToTheOutputRange()
        {
            Assert.AreEqual(100f, Curves.Remap(50f, 0f, 10f, 0f, 100f));
            Assert.AreEqual(0f, Curves.Remap(-50f, 0f, 10f, 0f, 100f));
        }

        [Test]
        public void LerpAndInverseLerpAreInverses()
        {
            var value = Curves.Lerp(4f, 12f, 0.25f);

            Assert.That(value, Is.EqualTo(6f).Within(1e-4f));
            Assert.That(Curves.InverseLerp(4f, 12f, value), Is.EqualTo(0.25f).Within(1e-4f));
        }

        [Test]
        public void SmoothStepIsFlatAtBothEnds()
        {
            Assert.AreEqual(0f, Curves.SmoothStep(0f));
            Assert.AreEqual(1f, Curves.SmoothStep(1f));
            Assert.That(Curves.SmoothStep(0.5f), Is.EqualTo(0.5f).Within(1e-4f));
            Assert.Less(Curves.SmoothStep(0.1f), 0.1f, "smoothstep must ease in");
        }
    }
}
