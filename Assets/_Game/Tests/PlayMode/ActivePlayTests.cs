using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Neko.Game.Core;
using Neko.Game.Ui;
using Neko.Sim.Service;
using Neko.Sim.Shift;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Neko.Tests.PlayMode
{
    /// <summary>
    /// The owner's hands on the floor, through the real scene and the real click path: nothing
    /// in the day's HUD may sit on top of the room and swallow a tap, and a tap on each kind of
    /// thing that can be helped does what it says.
    /// </summary>
    /// <remarks>
    /// Every action is a screen-space click sent through the input layer after checking that the
    /// UI's own raycaster does not own that pixel, which is exactly what a mouse would meet.
    /// </remarks>
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

        private static Vector2 ScreenOf(Vector2 world) =>
            GameRoot.Instance.Camera.Camera.WorldToScreenPoint(world);

        private static void Tap(Vector2 world)
        {
            var screen = ScreenOf(world);
            Assert.IsFalse(Blocked(screen), "a UI element covers the room at " + world);
            GameRoot.Instance.Input.SendClickForTest(screen);
        }

        /// <summary>Runs frames until <paramref name="ready"/> holds; fails after <paramref name="seconds"/>.</summary>
        private static IEnumerator Until(Func<bool> ready, string what, float seconds = 90f)
        {
            var end = Time.realtimeSinceStartup + seconds;
            while (!ready())
            {
                Assert.Less(Time.realtimeSinceStartup, end, "timed out waiting for " + what);
                yield return null;
            }
        }

        private static void Faster() => GameRoot.Instance.Ui.Get<DayHud>().OnSpeed.Invoke(4f);

        [UnityTest]
        public IEnumerator TheHudNeverSitsOnTheFloorAndSwallowsATap()
        {
            var camera = GameRoot.Instance.Camera.Camera;
            var grid = GameRoot.Instance.Presenter.Shift.Grid;

            var points = new List<Vector2>();
            for (var y = 0; y < grid.Height; y++) points.Add(new Vector2((grid.Width - 1) * 0.5f, y));
            points.Add(new Vector2(0f, 0f));
            points.Add(new Vector2(grid.Width - 1f, grid.Height - 1f));

            foreach (var world in points)
            {
                Assert.IsFalse(Blocked(camera.WorldToScreenPoint(world)), "a UI element covers the room at tile " + world);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator ATapOnAPartyAtTheDoorSeatsIt()
        {
            var presenter = GameRoot.Instance.Presenter;
            var shift = presenter.Shift;

            yield return Until(() => shift.Customers.Any(c => c.State == CustomerState.Queueing), "a party at the door", 30f);
            var waiting = shift.Customers.First(c => c.State == CustomerState.Queueing);

            Tap(presenter.WorldPositionOf(waiting));

            Assert.AreNotEqual(CustomerState.Queueing, waiting.State, "the tap did not seat the party");
            Assert.GreaterOrEqual(shift.Assists, 1);
        }

        [UnityTest]
        public IEnumerator ATapOnATableWithAHandUpTakesTheOrder()
        {
            var presenter = GameRoot.Instance.Presenter;
            var shift = presenter.Shift;
            Faster();

            yield return Until(() => shift.Customers.Any(c => c.State == CustomerState.Ordering), "a table with a hand up");
            var customer = shift.Customers.First(c => c.State == CustomerState.Ordering);
            var tickets = shift.Tickets.Count;

            Tap(presenter.WorldPositionOf(customer));

            Assert.AreNotEqual(CustomerState.Ordering, customer.State, "the order was not taken");
            Assert.Greater(shift.Tickets.Count, tickets, "taking the order raised a ticket");
        }

        [UnityTest]
        public IEnumerator ATapOnADirtyTableClearsIt()
        {
            var shift = GameRoot.Instance.Presenter.Shift;
            Faster();

            yield return Until(() => shift.Seats.Any(s => s.Dirty), "a dirty table");
            var seat = shift.Seats.First(s => s.Dirty);

            // Dirty tables are cleared by tapping the mess, drawn just above the seat.
            Tap(new Vector2(seat.Cell.X, seat.Cell.Y + 0.55f));

            Assert.IsFalse(seat.Dirty, "the table is still dirty");
        }

        [UnityTest]
        public IEnumerator ATapOnAnImpatientCustomerCalmsThem()
        {
            var presenter = GameRoot.Instance.Presenter;
            var shift = presenter.Shift;
            Faster();

            Func<Customer> upset = () => shift.Customers.FirstOrDefault(c =>
                c.DrainsPatience && !c.Profile.NeverImpatient && c.Patience < 0.8f &&
                c.State != CustomerState.Queueing && c.State != CustomerState.Ordering);

            yield return Until(() => upset() != null, "an impatient customer");
            var customer = upset();
            var before = customer.Patience;

            Tap(presenter.WorldPositionOf(customer));

            Assert.Greater(customer.Patience, before, "the customer was not calmed");
            Assert.AreEqual(1, shift.Pampers);
        }

        [UnityTest]
        public IEnumerator ATapOnTheStoveInsideTheGoldenMarkServesThePerfectDish()
        {
            var presenter = GameRoot.Instance.Presenter;
            var shift = presenter.Shift;
            Faster();

            Func<Ticket> inMark = () => shift.Tickets.FirstOrDefault(t =>
            {
                var p = shift.CookProgressOf(t);
                return p >= Shift.PerfectMin + 0.02f && p <= Shift.PerfectMax - 0.02f;
            });

            yield return Until(() => inMark() != null, "a dish on the golden mark");
            var ticket = inMark();
            var cell = shift.Stations[ticket.StationIndex].Cell;

            Tap(new Vector2(cell.X, cell.Y + 0.4f));

            Assert.AreEqual(1, shift.PerfectDishes, "the dish was not stopped on the mark");
            Assert.AreEqual(TicketState.Ready, ticket.State);
        }
    }
}
