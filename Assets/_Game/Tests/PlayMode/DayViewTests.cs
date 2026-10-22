using System.Collections;
using System.Linq;
using Neko.Game.Core;
using Neko.Game.Ui;
using Neko.Game.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Neko.Tests.PlayMode
{
    /// <summary>
    /// What is actually on screen while the day runs.
    /// </summary>
    /// <remarks>
    /// The simulation can be perfect and the café can still look broken. These check the one
    /// thing a capture kept suggesting and no other test covered: that every maid the player
    /// fielded is visibly standing in the room.
    /// </remarks>
    public sealed class DayViewTests
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
        public IEnumerator EveryFieldedMaidIsDrawnInTheRoom()
        {
            var root = GameRoot.Instance;
            var fielded = root.Session.Profile.Lineup.MaidIds().Count;

            Assert.Greater(fielded, 0, "precondition: somebody was fielded");

            var views = Object.FindObjectsByType<MaidView>(FindObjectsSortMode.None);

            Assert.AreEqual(fielded, views.Length,
                "fielded " + fielded + " maids and drew " + views.Length);

            yield return null;
        }

        [UnityTest]
        public IEnumerator NobodyIsParkedOnTopOfSomebodyElse()
        {
            // Two maids working the same table at the same moment is correct — the greeter
            // walks a party over and the waitress brings the plate. What looked broken was
            // the idle case: a whole position standing on one tile, drawing a six-strong
            // squad as a café with two people in it.
            yield return null;
            yield return null;

            var shift = GameRoot.Instance.Presenter.Shift;
            var views = Object.FindObjectsByType<MaidView>(FindObjectsSortMode.None)
                .Where(v => v.Worker != null && v.Worker.Claimed == null)
                .ToList();

            for (var i = 0; i < views.Count; i++)
            {
                for (var j = i + 1; j < views.Count; j++)
                {
                    var apart = Vector2.Distance(views[i].transform.position,
                        views[j].transform.position);

                    Assert.Greater(apart, 0.25f,
                        views[i].Worker.Name + " (" + views[i].Worker.Role + ") at " +
                        views[i].transform.position + " and " + views[j].Worker.Name + " (" +
                        views[j].Worker.Role + ") at " + views[j].transform.position);
                }
            }

            Assert.IsNotNull(shift);
        }

        [UnityTest]
        public IEnumerator EverybodyIsInsideTheRoom()
        {
            var root = GameRoot.Instance;
            var grid = root.Presenter.Shift.Grid;

            for (var frame = 0; frame < 120; frame++)
            {
                foreach (var view in Object.FindObjectsByType<MaidView>(FindObjectsSortMode.None))
                {
                    var at = view.transform.position;

                    Assert.That(at.x, Is.InRange(-1f, grid.Width + 1f), view.Worker.Name);
                    Assert.That(at.y, Is.InRange(-1f, grid.Height + 1f), view.Worker.Name);
                }

                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator EveryMaidHasASpriteToDraw()
        {
            foreach (var view in Object.FindObjectsByType<MaidView>(FindObjectsSortMode.None))
            {
                var body = view.transform.Find("Body");

                Assert.IsNotNull(body, view.Worker.Name + " has no body object");
                Assert.IsNotNull(body.GetComponent<SpriteRenderer>().sprite,
                    view.Worker.Name + " has no sprite");
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator TheHudShowsOneChipPerFieldedMaid()
        {
            var root = GameRoot.Instance;
            var fielded = root.Session.Profile.Lineup.MaidIds().Count;

            var chips = Object.FindObjectsByType<MoodChip>(FindObjectsSortMode.None);

            Assert.AreEqual(fielded, chips.Length);
            yield return null;
        }
    }
}
