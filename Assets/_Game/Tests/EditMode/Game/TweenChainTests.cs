using Neko.Game.Core;
using Neko.Game.Feel;
using NUnit.Framework;

namespace Neko.Tests.Game
{
    /// <summary>
    /// A tween that starts another one must not run the whole chain inside one frame.
    /// </summary>
    /// <remarks>
    /// This is the test that would have caught the second crash. The menu's sign sways by
    /// reversing itself forever — <c>Tween.Value(...).Then(() => Sway(logo, to, from))</c> —
    /// and the runner advanced the list by index while a completion appended to it. One
    /// frame longer than the sway therefore completed the replacement too, which started
    /// another, which completed, for as long as the frame lasted.
    ///
    /// The editor's first frame of play mode is exactly that frame: it carries the domain
    /// reload and the scene load as delta, which measured 3.9 s against a 2.6 s sway. The
    /// loop never returned, the editor allocated 1.8 GB to 10.4 GB in nine seconds, and the
    /// kernel killed it with nothing in the log after "Loaded scene". Batch mode never saw
    /// it: a PlayMode test loads the scene with play mode already warm, so the delta stays
    /// at a frame's worth and every sway takes the 2.6 s it asked for.
    /// </remarks>
    public sealed class TweenChainTests
    {
        [SetUp]
        public void Clear() => Tween.KillAll();

        [TearDown]
        public void Tear() => Tween.KillAll();

        /// <summary>How long a frame the editor hands the game when play mode begins.</summary>
        private const float LongFrame = 4f;

        [Test]
        public void ATweenBornDuringAFrameIsNotAdvancedInIt()
        {
            var replacementProgress = -1f;

            Tween.Value(0f, 1f, 1f, _ => { })
                .Then(() => Tween.Value(0f, 1f, 1f, v => replacementProgress = v));

            Tween.TickAll(LongFrame);

            Assert.AreEqual(-1f, replacementProgress,
                "the replacement ran inside the frame that created it");
        }

        [Test]
        public void OneFrameRunsOnlyOneLinkOfASelfRenewingChain()
        {
            // Bounded so a broken runner fails this assertion instead of hanging the
            // whole suite the way it hung the editor.
            const int guard = 50;

            var links = 0;

            void Renew()
            {
                links++;
                if (links < guard) Tween.Value(0f, 1f, 1f, _ => { }).Then(Renew);
            }

            Tween.Value(0f, 1f, 1f, _ => { }).Then(Renew);

            Tween.TickAll(LongFrame);

            Assert.AreEqual(1, links, "one frame ran " + links + " links of the chain");
        }

        [Test]
        public void ClearingTheListFromInsideAFrameDoesNotReachPastItsEnd()
        {
            // The pass walks a snapshot of the list, so a list that empties under it must
            // not be indexed past its new end. GameRoot.OnDestroy is the one caller of
            // KillAll today, and it cannot land mid-frame — this pins the shape anyway,
            // because the snapshot is what makes it possible at all.
            Tween.Value(0f, 1f, 1f, _ => { });
            Tween.Value(0f, 1f, 1f, _ => Tween.KillAll());
            Tween.Value(0f, 1f, 1f, _ => { });

            Assert.DoesNotThrow(() => Tween.TickAll(LongFrame));
        }

        [Test]
        public void ALongFrameNeverReachesTheGameAtFullLength()
        {
            // The second lock. Even if some future animation renews itself the way the menu
            // sway does, the delta it is handed can no longer be long enough to finish it.
            Assert.AreEqual(Clocks.LongestFrame, Clocks.Trim(3.9f),
                "the editor's first frame reached the game at its full length");
            Assert.AreEqual(0.016f, Clocks.Trim(0.016f), "an ordinary frame was trimmed");
        }

        [Test]
        public void TheMenuSwayCompletesOncePerFrameAndNotOncePerPass()
        {
            // The menu's own shape, at its own duration: a sway that flips its ends every
            // time it finishes. Across two hundred frames each longer than the sway, it
            // should finish two hundred times — once each — and the guard is there so an
            // unbounded runner trips the assertion instead of hanging the suite the way it
            // hung the editor.
            const int frames = 200;
            const int guard = 10000;

            var from = -1.4f;
            var to = 1.4f;
            var completions = 0;

            void Sway()
            {
                Tween.Value(from, to, 2.6f, _ => { }).Then(() =>
                {
                    completions++;

                    var swap = from;
                    from = to;
                    to = swap;

                    if (completions < guard) Sway();
                });
            }

            Sway();

            for (var frame = 0; frame < frames; frame++) Tween.TickAll(LongFrame);

            Assert.AreEqual(frames, completions,
                "the sway finished " + completions + " times in " + frames + " frames");
            Assert.LessOrEqual(Tween.LiveCount, 2,
                "a self-reversing sway left " + Tween.LiveCount + " tweens alive");
        }
    }
}
