using Neko.Game.Art;
using Neko.Game.Feel;
using NUnit.Framework;
using UnityEngine;

namespace Neko.Tests.Game
{
    /// <summary>
    /// The flourish has to be bounded, because the day that fires it is not.
    /// </summary>
    /// <remarks>
    /// Skipping a day ticks the whole simulation through inside one frame and then plays
    /// every event it queued. A label is only reusable once it has aged past its lifetime,
    /// and nothing ages inside a single frame — so every popup in a skipped day asked for a
    /// brand new one. Each is a GameObject carrying a RectTransform, a uGUI Text and an
    /// Outline, which is a mesh effect that doubles the vertex count.
    ///
    /// Measured before the cap: the editor went from 1.0 GB to 8.1 GB in fourteen seconds
    /// and was killed by the kernel. These tests are the cap, stated as a number.
    /// </remarks>
    public sealed class EffectsPoolTests
    {
        private GameObject _host;
        private Effects _effects;

        [SetUp]
        public void Build()
        {
            _host = new GameObject("Effects");
            var labelRoot = new GameObject("Labels").transform;
            labelRoot.SetParent(_host.transform, false);

            _effects = _host.AddComponent<Effects>();
            _effects.Bind(new SpriteFactory(), labelRoot);
        }

        [TearDown]
        public void Tear() => Object.DestroyImmediate(_host);

        [Test]
        public void ANumberPopupNeverAllocatesPastTheCap()
        {
            // A real day pays out once per customer served, and a skipped one lands them all
            // in the same frame. Two thousand is a generous day.
            for (var i = 0; i < 2000; i++)
            {
                _effects.Number(new Vector3(i % 8, i % 5, 0f), "+" + i, Color.white);
            }

            Assert.LessOrEqual(_effects.LabelCount, Effects.LabelPoolSize,
                "allocated " + _effects.LabelCount + " labels for 2000 popups in one frame");
        }

        [Test]
        public void TheOldestLabelIsTheOneReused()
        {
            // Reuse has to take the oldest, not the first in the list, or a long burst keeps
            // overwriting one object and every popup but the last is lost.
            for (var i = 0; i < Effects.LabelPoolSize; i++)
            {
                _effects.Number(Vector3.zero, "first" + i, Color.white);
            }

            var filled = _effects.LabelCount;

            _effects.Number(Vector3.zero, "overflow", Color.white);
            _effects.Number(Vector3.zero, "second", Color.white);

            Assert.AreEqual(filled, _effects.LabelCount, "the cap was breached");

            Assert.Contains("overflow", (string[])_effects.LabelTexts);
            Assert.Contains("second", (string[])_effects.LabelTexts);

            // The two oldest were recycled, in that order, and nothing younger was touched.
            CollectionAssert.DoesNotContain(_effects.LabelTexts, "first0");
            CollectionAssert.DoesNotContain(_effects.LabelTexts, "first1");
            CollectionAssert.Contains(_effects.LabelTexts, "first2");
        }

        [Test]
        public void PuffsWereAlreadyBounded()
        {
            // The particle pool always had a cap; this pins it so the two cannot drift apart.
            for (var i = 0; i < 500; i++) _effects.Hearts(new Vector3(i % 8, i % 5, 0f), 4);

            Assert.LessOrEqual(_effects.PuffCount, Effects.PuffPoolSize);
        }
    }
}
