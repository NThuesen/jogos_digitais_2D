using System.Collections;
using Neko.Game.Core;
using Neko.Game.Feel;
using Neko.Game.Ui;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Neko.Tests.PlayMode
{
    /// <summary>
    /// What skipping a day costs.
    /// </summary>
    /// <remarks>
    /// This is the test that would have caught the crash. Skipping ticks the whole
    /// simulation through inside one frame and then dealt with every event it queued, and
    /// each payout asked for a fresh floating label, because a label only becomes reusable
    /// once it has aged — and nothing ages inside a single frame. The editor went from
    /// 1.0 GB to 8.1 GB in fourteen seconds and the kernel killed it. Every unit test
    /// passed throughout: the simulation was right, and the view was unbounded.
    ///
    /// Written against the button's own callback rather than against SkipDay, so that moving
    /// the work somewhere else cannot quietly uncover the bug again.
    /// </remarks>
    public sealed class SkipDayTests
    {
        private const string ScenePath = "Assets/_Game/Scenes/Boot.unity";

        [UnitySetUp]
        public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync(ScenePath, LoadSceneMode.Single);
            yield return null;

            var root = GameRoot.Instance;
            root.Ui.Get<MenuScreen>().OnNewGame.Invoke();
            yield return null;

            root.Ui.Get<HubScreen>().OnOpenDay.Invoke();
            yield return null;
            yield return null;
        }

        [UnityTest]
        public IEnumerator SkippingADayAllocatesNoUnboundedFlourish()
        {
            var root = GameRoot.Instance;
            var effects = Object.FindAnyObjectByType<Effects>();

            Assert.IsNotNull(effects, "precondition: the day has its effects");

            root.Ui.Get<DayHud>().OnSkip.Invoke();
            yield return null;

            // Zero, not "within the cap". A skipped day plays nothing, so it has no reason
            // to own a single label — and stating it as zero gives this test teeth at any
            // café size. A day-1 café serves four customers, which would sit under any cap
            // and prove nothing; the cap itself is pinned by EffectsPoolTests, where two
            // thousand popups in one frame is the case that actually killed the editor.
            Assert.AreEqual(0, effects.LabelCount,
                "a skipped day built " + effects.LabelCount + " floating labels");

            Assert.LessOrEqual(effects.PuffCount, Effects.PuffPoolSize);
        }

        [UnityTest]
        public IEnumerator SkippingADayFinishesIt()
        {
            // The cheap way to bound the flourish would be to stop skipping from working.
            var root = GameRoot.Instance;

            root.Ui.Get<DayHud>().OnSkip.Invoke();
            yield return null;

            Assert.IsTrue(root.Presenter.Shift.Finished, "the day did not actually end");
        }

        [UnityTest]
        public IEnumerator ASkippedDayLeavesNothingForTheNextOne()
        {
            // The original reason skipping played the queue at all: events left behind were
            // played by the following day. Discarding has to leave the queue empty too.
            var root = GameRoot.Instance;

            root.Ui.Get<DayHud>().OnSkip.Invoke();
            yield return null;

            Assert.AreEqual(0, root.Presenter.Shift.Events.Count);
        }
    }
}
