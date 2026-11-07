using System.Collections;
using System.Collections.Generic;
using Neko.Game.Core;
using Neko.Game.Ui;
using Neko.Sim.Service;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Neko.Tests.PlayMode
{
    /// <summary>
    /// The owner's hands on the floor, through the real scene: nothing in the day's HUD may sit
    /// on top of the room and swallow the tap, and a tap on a waiting party must seat it.
    /// </summary>
    public sealed class ActivePlayTests
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

        private static bool Blocked(Vector2 screen)
        {
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = screen }, hits);
            return hits.Count > 0;
        }

        [UnityTest]
        public IEnumerator TheHudNeverSitsOnTheFloorAndSwallowsATap()
        {
            var root = GameRoot.Instance;
            var camera = root.Camera.Camera;
            var grid = root.Presenter.Shift.Grid;

            // The middle of every row of the room, and a tile in each corner.
            var points = new List<Vector2>();
            for (var y = 0; y < grid.Height; y++) points.Add(new Vector2((grid.Width - 1) * 0.5f, y));
            points.Add(new Vector2(0f, 0f));
            points.Add(new Vector2(grid.Width - 1f, grid.Height - 1f));

            foreach (var world in points)
            {
                var screen = camera.WorldToScreenPoint(world);
                Assert.IsFalse(Blocked(screen), "a UI element covers the room at tile " + world);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator ATapOnAPartyAtTheDoorSeatsIt()
        {
            var root = GameRoot.Instance;
            var presenter = root.Presenter;
            var shift = presenter.Shift;

            // Run the day until somebody is waiting to be seated, before any maid reaches them.
            Customer waiting = null;
            for (var i = 0; i < 600 && waiting == null; i++)
            {
                yield return null;
                foreach (var customer in shift.Customers)
                {
                    if (customer.State == CustomerState.Queueing) waiting = customer;
                }
            }

            Assert.IsNotNull(waiting, "precondition: a party arrived at the door");

            var world = presenter.WorldPositionOf(waiting);
            var screen = (Vector2)root.Camera.Camera.WorldToScreenPoint(world);
            Assert.IsFalse(Blocked(screen), "the party is behind a UI element");

            root.Input.SendClickForTest(screen);
            Assert.AreNotEqual(CustomerState.Queueing, waiting.State, "the tap did not seat the party");
            Assert.GreaterOrEqual(shift.Assists, 1);
        }
    }
}
