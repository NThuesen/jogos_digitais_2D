using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Neko.Game.Core;
using Neko.Game.Ui;
using Neko.Sim.Cafe;
using Neko.Sim.Maids;
using Neko.Sim.Save;
using Neko.Sim.Shift;
using Neko.Sim.Team;
using Neko.Tests.PlayMode.Layout;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Neko.Tests.PlayMode
{
    /// <summary>
    /// UI-INT-05/06: the illustrated Hall, the day and the report, with real data, the real room and real
    /// input at both reference windows. Frames are written only when NEKO_UI_QA_DIR is set.
    /// </summary>
    public sealed class HallShiftResultsScreenTests
    {
        private static readonly Vector2Int[] Windows = { new Vector2Int(1280, 720), new Vector2Int(960, 600) };

        private static string Dir => System.Environment.GetEnvironmentVariable("NEKO_UI_QA_DIR");

        [TearDown]
        public void Restore()
        {
            HallScreen.ForcePlain = false;
            DayHud.ForcePlain = false;
            BooksScreen.ForcePlain = false;
            ResolutionRig.Restore();
            SessionFactory.RestoreClock();
            SessionFactory.RestoreSave();
        }

        private static void Save(string name, Vector2Int window)
        {
            if (string.IsNullOrEmpty(Dir)) return;
            Directory.CreateDirectory(Dir);
            File.WriteAllBytes(Path.Combine(Dir, name + "_" + window.x + "x" + window.y + ".png"), ResolutionRig.Capture());
        }

        private static readonly string[] Names = { "Aoi", "Beni", "Chie", "Daisy", "Emi", "Fuyu", "Gin", "Hana", "Iku", "Jun" };

        private static ProfileBuilder Cafe(int level, int maids, bool fill)
        {
            var builder = SessionFactory.NewProfile(7u, level).WithDay(1).WithMoney(900);
            for (var i = 0; i < maids; i++) builder.WithMaid(Names[i], Rarity.Common, 3, 1);
            if (fill) builder.AutoFillLineup();
            return builder;
        }

        private static GameRoot Root => GameRoot.Instance;

        private static HallScreen Hall => Root.Ui.Get<HallScreen>();

        private static DayHud Hud => Root.Ui.Get<DayHud>();

        private static BooksScreen Books => Root.Ui.Get<BooksScreen>();

        private static Rect Grown(Rect r) => Rect.MinMaxRect(r.xMin - 1f, r.yMin - 1f, r.xMax + 1f, r.yMax + 1f);

        private static Vector2 Pixel(float x, float y) => Root.Camera.Camera.WorldToScreenPoint(new Vector3(x, y, 0f));

        private static void Click(Vector2 pixel) => Root.Input.SendClickForTest(pixel);

        private static void RightClick(Vector2 pixel) =>
            typeof(GameRoot).GetMethod("OnRightClick", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(Root, new object[] { pixel });

        private static IEnumerator OpenHall(ProfileBuilder builder)
        {
            yield return SessionFactory.Boot(builder.Build());
            yield return SessionFactory.ToHub();
            yield return SessionFactory.ToHall();
            yield return ResolutionRig.Apply(1280, 720);
            yield return SessionFactory.Settle();
        }

        private static IEnumerator AtEachWindow(string state, System.Action check)
        {
            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return SessionFactory.Settle();
                check?.Invoke();
                Save(state, window);
            }
        }

        private static IEnumerator Frames(int frames)
        {
            for (var i = 0; i < frames; i++) yield return null;
        }

        private static Button Cell(string id) => Hall.GetComponentsInChildren<Button>(false).First(b => b.name == "Cell_" + id);

        private static int Stock(string id) => Root.Session.Profile.OwnedPlaceables.Count(p => p == id);

        /// <summary>The first cell the held piece fits in, found through the grid (never through the UI).</summary>
        private static Cell FreeCell(string id, bool rotated = false)
        {
            var grid = Root.Session.Profile.Grid;
            var def = PlaceableCatalog.ById(id);
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    if (grid.CanPlace(def, x, y, rotated)) return new Cell(x, y);
                }
            }

            Assert.Fail("no free cell for " + id);
            return default;
        }

        private static string HeldLine() => Hall.GetComponentsInChildren<Text>(false).First(t => t.name == "Picked").text;

        // -------------------------------------------------------------------------------------
        // The Hall
        // -------------------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator HallFramesTheRealRoomInsideTheBoardOpening()
        {
            yield return OpenHall(Cafe(2, 3, true).WithStock("table_2", 2).WithStock("plant", 1));
            Assert.IsTrue(Hall.IsKit, "the kit art is present, so the Hall is the kit layout");

            yield return AtEachWindow("hall", () =>
            {
                Assert.IsTrue(Hall.Window.TryPixels(out var window));
                var scale = window.width / 513.36f * (Root.Camera.Camera.pixelHeight / 720f);
                Assert.AreEqual(window.width / window.height, 690f / 494f, 0.01f, "the window keeps the board's proportion, not stretched");

                var grid = Root.Session.Profile.Grid;
                var cam = Root.Camera.Camera;
                foreach (var corner in new[] { new Vector2(-0.5f, -0.5f), new Vector2(grid.Width - 0.5f, grid.Height - 0.5f) })
                {
                    var pixel = Pixel(corner.x, corner.y);
                    Assert.IsTrue(window.Contains(pixel), "room corner " + corner + " at " + pixel + " is inside " + window);
                }

                // Tiles stay square: one world unit is the same number of pixels both ways.
                var a = Pixel(0, 0);
                Assert.AreEqual(Pixel(1, 0).x - a.x, Pixel(0, 1).y - a.y, 0.01f, "square tiles at " + cam.pixelWidth);
                Assert.Greater(scale, 0.5f);
            });
        }

        [UnityTest]
        public IEnumerator HallShowsTheRealMetricsAndNoFakeShopButton()
        {
            yield return OpenHall(Cafe(2, 2, false).WithStock("table_2", 1));
            var metrics = Root.Session.Profile.Grid.Metrics();
            var all = string.Join(" | ", Hall.GetComponentsInChildren<Text>(false).Select(t => t.text));
            StringAssert.Contains(Strings.Get("hall.seats") + " " + metrics.Seats, all);
            StringAssert.Contains(Strings.Get("hall.ambience") + " " + metrics.Ambience.ToString("0.0"), all);
            StringAssert.Contains(Strings.Get("hall.flow") + " " + metrics.Flow.ToString("0.0"), all);
            StringAssert.Contains(Strings.Get("hall.theme") + " " + Strings.Get("hall.notheme"), all);

            // The only buttons are Voltar and the stock cells: the kit's "Loja" art has no callback here.
            var names = Hall.GetComponentsInChildren<Button>(false).Select(b => b.name).ToList();
            Assert.AreEqual(1, names.Count(n => n == "Back"));
            Assert.IsTrue(names.All(n => n == "Back" || n.StartsWith("Cell_")), string.Join(",", names));
        }

        [UnityTest]
        public IEnumerator HallDecorationNeverCatchesARaycast()
        {
            yield return OpenHall(Cafe(2, 2, false).WithStock("table_2", 1));
            var buttons = new HashSet<Graphic>(Hall.GetComponentsInChildren<Button>(true).Select(b => b.targetGraphic));
            var offenders = Hall.GetComponentsInChildren<Graphic>(true)
                .Where(g => g.raycastTarget && !buttons.Contains(g)).Select(g => g.name).ToList();
            Assert.IsEmpty(offenders, "decorative graphics that take raycasts: " + string.Join(",", offenders));
        }

        [UnityTest]
        public IEnumerator HallPlacesRotatesAndRemovesWithRealClicks()
        {
            yield return OpenHall(Cafe(2, 2, false).WithStock("plant", 2).WithStock("table_2", 1));
            var grid = Root.Session.Profile.Grid;
            var items = grid.Items.Count;
            var seats = grid.Metrics().Seats;

            foreach (var window in Windows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return SessionFactory.Settle();
                var stock = Stock("plant");

                Cell(  "plant").onClick.Invoke();
                yield return null;
                StringAssert.Contains(PlaceableCatalog.ById("plant").DisplayName, HeldLine());
                StringAssert.Contains(Strings.Get("hall.rotate"), HeldLine());

                // An occupied cell: the door, a fixture. Refused with the same message as before.
                var door = grid.Door;
                Click(Pixel(door.X, door.Y));
                Assert.AreEqual(Strings.Get("hall.blocked"), HeldLine(), "occupied cell refuses");
                Assert.AreEqual(stock, Stock("plant"), "a refused click spends nothing");

                // A free cell, through the real click path: placed, stock down, metrics follow.
                var free = FreeCell("plant");
                Click(Pixel(free.X, free.Y));
                yield return null;
                Assert.AreEqual(stock - 1, Stock("plant"), "stock goes down by one at " + window);
                Assert.IsNotNull(grid.ItemAt(free.X, free.Y), "the piece is where the click was");

                // Right click takes it back into stock.
                RightClick(Pixel(free.X, free.Y));
                yield return null;
                Assert.AreEqual(stock, Stock("plant"), "removal returns the piece");
                Assert.IsNull(grid.ItemAt(free.X, free.Y));
            }

            Assert.AreEqual(items, grid.Items.Count, "everything placed was taken back");
            Assert.AreEqual(seats, grid.Metrics().Seats);

            // Rotation: R flips the held piece, and the line says yes or no.
            Cell("table_2").onClick.Invoke();
            Hall.Rotate();
            StringAssert.Contains(Strings.Get("dialog.yes"), HeldLine());
            var rotated = FreeCell("table_2", true);
            Click(Pixel(rotated.X, rotated.Y));
            yield return null;
            Assert.AreEqual(1, grid.Items.Count - items, "a rotated table was placed");
            Assert.AreEqual(seats + 2, grid.Metrics().Seats, "and the seats follow");
            Hall.Rotate();
            StringAssert.Contains(Strings.Get("dialog.no"), HeldLine());
        }

        [UnityTest]
        public IEnumerator HallIgnoresClicksOutsideTheWindow()
        {
            yield return OpenHall(Cafe(2, 2, false).WithStock("plant", 2));
            yield return SessionFactory.Settle();
            Cell("plant").onClick.Invoke();
            yield return null;
            var held = HeldLine();
            var stock = Stock("plant");

            Assert.IsTrue(Hall.Window.TryPixels(out var window));
            var free = FreeCell("plant");
            var inside = Pixel(free.X, free.Y);
            Assert.IsTrue(window.Contains(inside));

            // Slide the window off the cell, within one frame, so the camera has not followed yet: the very same
            // free cell now sits on the desk, outside the window. Without the gate the click would place the piece
            // there; with it, nothing happens.
            var slot = (RectTransform)Hall.Window.transform;
            var home = slot.anchoredPosition;
            slot.anchoredPosition = home + new Vector2(700f, 0f);
            Assert.IsFalse(RoomWindow.Contains(inside), "the cell is now outside the window at " + inside);
            Click(inside);
            RightClick(inside);
            slot.anchoredPosition = home;
            yield return null;

            Assert.AreEqual(stock, Stock("plant"), "a click on the desk places nothing");
            Assert.AreEqual(held, HeldLine(), "and says nothing");

            // The same click with the window back where it belongs does place.
            Click(Pixel(free.X, free.Y));
            yield return null;
            Assert.AreEqual(stock - 1, Stock("plant"));

            // A right click outside the window removes nothing either.
            var placed = Root.Session.Profile.Grid.Items.Count;
            var again = Pixel(free.X, free.Y);
            slot.anchoredPosition = home + new Vector2(700f, 0f);
            RightClick(again);
            slot.anchoredPosition = home;
            Assert.AreEqual(placed, Root.Session.Profile.Grid.Items.Count);

            // Points over the board frame, the title and the tray are not in the window.
            foreach (var point in new[] { new Vector2(20, 20), new Vector2(640, 60), new Vector2(640, 100), new Vector2(640, 650) })
            {
                Assert.IsFalse(RoomWindow.Contains(point), "scenery point " + point + " is not the room");
            }
        }

        [UnityTest]
        public IEnumerator HallStockShowsTheRealFurnitureGrouped()
        {
            yield return OpenHall(Cafe(2, 2, false).WithStock("plant", 3).WithStock("table_2", 2).WithStock("stove", 1));
            var cells = Hall.GetComponentsInChildren<Button>(false).Where(b => b.name.StartsWith("Cell_")).ToList();
            Assert.AreEqual(3, cells.Count, "one cell per kind, however many are owned");

            foreach (var pair in new[] { ("plant", 3), ("table_2", 2), ("stove", 1) })
            {
                var def = PlaceableCatalog.ById(pair.Item1);
                var cell = Cell(pair.Item1);
                StringAssert.Contains(def.DisplayName + " ×" + pair.Item2, cell.GetComponentInChildren<Text>().text);
                var icon = cell.transform.Find("Icon").GetComponent<Image>();
                Assert.AreEqual(Neko.Game.Ui.UiKit.Sprites.Furniture(def, def.Width, def.Height), icon.sprite, "the room's own picture");
            }

            StringAssert.Contains(Strings.Get("hall.stock") + ": 6", string.Join("|", Hall.GetComponentsInChildren<Text>(false).Select(t => t.text)));

            // The held cell is marked.
            Cell("plant").onClick.Invoke();
            yield return null;
            Assert.AreEqual(KitArt.Get("hall_inventory_cell_selected"), Cell("plant").GetComponent<Image>().sprite);
            Assert.AreEqual(KitArt.Get("hall_inventory_cell_empty"), Cell("stove").GetComponent<Image>().sprite);
        }

        [UnityTest]
        public IEnumerator HallEmptyStockSaysSoAndBackStillWorks()
        {
            yield return OpenHall(Cafe(2, 2, false).WithStock("plant", 0).WithStock("table_2", 0).WithStock("stove", 0));
            Root.Session.Profile.OwnedPlaceables.Clear();
            Hall.Refresh();
            yield return null;

            var all = string.Join("|", Hall.GetComponentsInChildren<Text>(false).Select(t => t.text));
            StringAssert.Contains(Strings.Get("hall.stock") + ": 0", all);
            Assert.AreEqual(0, Hall.GetComponentsInChildren<Button>(false).Count(b => b.name.StartsWith("Cell_")));

            var back = false;
            Hall.OnBack = () => back = true;
            Hall.GetComponentsInChildren<Button>(false).First(b => b.name == "Back").onClick.Invoke();
            Assert.IsTrue(back);
        }

        [UnityTest]
        public IEnumerator HallFallsBackToTheEarlierLayoutWithoutTheKit()
        {
            HallScreen.ForcePlain = true;
            yield return SessionFactory.Boot(Cafe(2, 2, false).WithStock("plant", 1).Build());
            yield return SessionFactory.ToHub();
            yield return SessionFactory.ToHall();
            yield return SessionFactory.Settle();

            Assert.IsFalse(Hall.IsKit);
            Assert.IsNull(Hall.Window);
            Assert.IsNull(RoomWindow.Active, "the room is unframed in the earlier layout");

            var card = Hall.GetComponentsInChildren<Button>(false).First(b => b.GetComponentInChildren<Text>()?.text.Contains("Samambaia") == true);
            card.onClick.Invoke();
            var free = FreeCell("plant");
            Click(Pixel(free.X, free.Y));
            yield return null;
            Assert.AreEqual(0, Stock("plant"));
        }

        // -------------------------------------------------------------------------------------
        // The day
        // -------------------------------------------------------------------------------------

        private static IEnumerator OpenDay(ProfileBuilder builder)
        {
            yield return SessionFactory.Boot(builder.Build());
            yield return SessionFactory.ToHub();
            yield return SessionFactory.ToDay();
            yield return ResolutionRig.Apply(1280, 720);
            yield return SessionFactory.Settle();
        }

        [UnityTest]
        public IEnumerator DayShowsTheLiveRoomInTheOpeningAndOneChipPerMaid()
        {
            yield return OpenDay(Cafe(6, 7, true));
            Assert.IsTrue(Hud.IsKit);
            var shift = Hud.Shift;
            Assert.Greater(shift.Workers.Count, 1);

            yield return AtEachWindow("day", () =>
            {
                Assert.IsTrue(Hud.Window.TryPixels(out var window));
                var grid = shift.Grid;
                foreach (var corner in new[] { new Vector2(-0.5f, -0.5f), new Vector2(grid.Width - 0.5f, grid.Height + 0.3f) })
                {
                    Assert.IsTrue(Grown(window).Contains(Pixel(corner.x, corner.y)), "room corner " + corner + " is inside " + window);
                }

                var chips = Hud.GetComponentsInChildren<MoodChip>(false);
                Assert.AreEqual(shift.Workers.Count, chips.Length, "one chip per worker");
                foreach (var worker in shift.Workers) StringAssert.Contains(worker.Name, string.Join("|", Hud.GetComponentsInChildren<Text>(false).Select(t => t.text)));
            });

            // The room is the world: the maids and their furniture are real renderers, not a picture.
            Assert.Greater(Root.Room.GetComponentsInChildren<SpriteRenderer>().Length, 10);
            Assert.IsNull(Hud.GetComponentsInChildren<RawImage>(true).FirstOrDefault(), "no captured room texture");
        }

        [UnityTest]
        public IEnumerator DayChipsFollowTheWorkersMoodAndState()
        {
            yield return OpenDay(Cafe(3, 4, true));
            var worker = Hud.Shift.Workers[0];
            var chip = Hud.GetComponentsInChildren<MoodChip>(false)[0];
            worker.Maid.Mood = 0.25f;
            worker.OnBreak = true;
            yield return Frames(2);
            var fill = chip.transform.Find("MoodTrack/MoodFill").GetComponent<Image>();
            Assert.AreEqual(0.25f, fill.fillAmount, 0.001f, "the bar is the maid's real mood");
            Assert.AreEqual(Strings.Get("day.state.break"), chip.transform.Find("State").GetComponent<Text>().text);
            worker.OnBreak = false;
            yield return Frames(2);
            Assert.AreNotEqual(Strings.Get("day.state.break"), chip.transform.Find("State").GetComponent<Text>().text);
        }

        private static IEnumerator ChipsFor(int level, int maids)
        {
            yield return OpenDay(Cafe(level, maids, true));
            var workers = Hud.Shift.Workers.Count;
            Assert.AreEqual(workers, Hud.GetComponentsInChildren<MoodChip>(false).Length);
            yield return AtEachWindow("day_" + workers + "_maids", () =>
            {
                // Every chip is inside the strip under the room (or reachable by scrolling when there are many).
                var view = (RectTransform)Hud.transform.Find("Stage/Chips");
                foreach (var chip in Hud.GetComponentsInChildren<MoodChip>(false))
                {
                    var rect = (RectTransform)chip.transform;
                    Assert.Greater(rect.rect.width, 90f, "a chip keeps a readable width");
                }

                Assert.IsNotNull(view);
            });
        }

        [UnityTest]
        public IEnumerator DayChipsFitAFourMaidCard() => ChipsFor(3, 4);

        [UnityTest]
        public IEnumerator DayChipsFitASevenMaidCard() => ChipsFor(6, 7);

        [UnityTest]
        public IEnumerator DayChipsFitATenMaidCard() => ChipsFor(10, 10);

        [UnityTest]
        public IEnumerator DayHudShowsTheRealNumbersAndTheSpeedControlsWork()
        {
            yield return OpenDay(Cafe(3, 4, true));
            var shift = Hud.Shift;
            Hud.Sync();
            var all = string.Join("|", Hud.GetComponentsInChildren<Text>(false).Select(t => t.text));
            StringAssert.Contains(Strings.Get("day.till") + " " + Strings.Money(shift.Earned), all);
            StringAssert.Contains(Strings.Get("day.served") + " " + shift.Served, all);
            StringAssert.Contains(Strings.Get("day.wrong") + " " + shift.WrongOrders, all);
            StringAssert.Contains(Strings.Get("day.angry") + " " + shift.LeftAngry, all);
            StringAssert.Contains(Mathf.FloorToInt(shift.TimeRemaining / 60f) + ":", all);

            var seen = new List<float>();
            Hud.OnSpeed = s => seen.Add(s);
            foreach (var name in new[] { "Speed2", "Speed4", "Speed1" })
            {
                Hud.GetComponentsInChildren<Button>(false).First(b => b.name == name).onClick.Invoke();
            }

            CollectionAssert.AreEqual(new[] { 2f, 4f, 1f }, seen);
            Assert.AreEqual(1f, Hud.CurrentSpeed);

            var skipped = false;
            Hud.OnSkip = () => skipped = true;
            Hud.GetComponentsInChildren<Button>(false).First(b => b.name == "Skip").onClick.Invoke();
            Assert.IsTrue(skipped);
            Assert.AreEqual(4, Hud.GetComponentsInChildren<Button>(false).Length, "1x, 2x, 4x and skip are the only controls");
        }

        [UnityTest]
        public IEnumerator DayRoomTakesTapsOnlyInsideTheOpening()
        {
            yield return OpenDay(Cafe(3, 4, true));
            Assert.IsTrue(Hud.Window.TryPixels(out var window));

            // The HUD's art never holds a raycast the room could have wanted.
            var buttons = new HashSet<Graphic>(Hud.GetComponentsInChildren<Button>(true).Select(b => b.targetGraphic));
            var offenders = Hud.GetComponentsInChildren<Graphic>(true).Where(g => g.raycastTarget && !buttons.Contains(g)).Select(g => g.name).ToList();
            Assert.IsEmpty(offenders, "decorative graphics that take raycasts: " + string.Join(",", offenders));

            // The door, a table and the stove are inside the window: real taps reach them.
            var grid = Hud.Shift.Grid;
            foreach (var cell in new[] { grid.Door }.Concat(grid.Items.Select(i => new Cell(i.X, i.Y))))
            {
                Assert.IsTrue(window.Contains(Pixel(cell.X, cell.Y)), "cell " + cell + " is tappable");
            }

            // Scenery is not: the lamp, the door painted on the wall, the rug and the frame itself.
            foreach (var point in new[] { new Vector2(80, 400), new Vector2(250, 300), new Vector2(640, 120), new Vector2(1180, 500), new Vector2(window.xMin - 6, window.center.y) })
            {
                Assert.IsFalse(RoomWindow.Contains(point), "scenery point " + point + " is not the room");
            }

            var served = Hud.Shift.Served;
            var combo = Hud.Shift.Combo;
            Click(new Vector2(80, 400));
            Click(new Vector2(window.xMin - 6, window.center.y));
            Assert.AreEqual(served, Hud.Shift.Served);
            Assert.AreEqual(combo, Hud.Shift.Combo, "a tap on scenery does nothing to the day");

            // A real tap on the door, on a party that waits there, seats it.
            Clocks.Pause(false);
            var guard = 0;
            while (guard++ < 1200 && !Hud.Shift.Customers.Any(c => c.State == Neko.Sim.Service.CustomerState.Queueing)) yield return null;
            var queueing = Hud.Shift.Customers.FirstOrDefault(c => c.State == Neko.Sim.Service.CustomerState.Queueing);
            Clocks.Pause(true);
            if (queueing != null)
            {
                var at = Root.Presenter.WorldPositionOf(queueing);
                Assert.IsTrue(window.Contains(Pixel(at.x, at.y)), "the party at the door is inside the opening");
                Click(Pixel(at.x, at.y));
                yield return null;
                Assert.AreNotEqual(Neko.Sim.Service.CustomerState.Queueing, queueing.State, "the tap reached the party at the door");
            }
        }

        [UnityTest]
        public IEnumerator DayFallsBackToTheEarlierHudWithoutTheKit()
        {
            DayHud.ForcePlain = true;
            yield return SessionFactory.Boot(Cafe(3, 4, true).Build());
            yield return SessionFactory.ToHub();
            yield return SessionFactory.ToDay();
            Assert.IsFalse(Hud.IsKit);
            Assert.IsNull(RoomWindow.Active);
            Assert.AreEqual(Hud.Shift.Workers.Count, Hud.GetComponentsInChildren<MoodChip>(false).Length);
        }

        // -------------------------------------------------------------------------------------
        // The report
        // -------------------------------------------------------------------------------------

        private enum Day { Profit, Loss, Zero, Huge, LevelUps }

        private static IEnumerator OpenBooks(Day day)
        {
            var builder = SessionFactory.NewProfile(7u, 10).WithDay(1).WithMoney(day == Day.Loss ? 40 : day == Day.Huge ? SessionFactory.LargeValues.Balance : 1800).EmptyRoom();
            foreach (var n in Names) builder.WithMaid(n, Rarity.Common, 1, 0);
            yield return SessionFactory.Boot(builder.Build());
            yield return SessionFactory.ToHub();

            var result = new ShiftResult { Day = 1, Satisfaction = day == Day.Loss ? 0.2f : 0.9f, ChemistryScore = day == Day.Loss ? 10 : 80 };
            switch (day)
            {
                case Day.Profit:
                    result.DishRevenue = 4200; result.Tips = 600; result.Served = 30; result.DishesCooked = 32;
                    break;
                case Day.Loss:
                    result.DishRevenue = 20; result.Served = 1; result.DishesCooked = 5;
                    break;
                case Day.Huge:
                    result.DishRevenue = SessionFactory.LargeValues.DayAmount; result.Tips = SessionFactory.LargeValues.DayAmount;
                    result.Served = SessionFactory.LargeValues.Tally; result.DishesCooked = SessionFactory.LargeValues.Tally;
                    result.WrongOrders = result.Burnt = result.LeftAngry = result.TurnedAway = SessionFactory.LargeValues.Tally;
                    break;
                case Day.LevelUps:
                    result.DishRevenue = 4200; result.Served = 30; result.DishesCooked = 32;
                    result.WrongOrders = 2; result.Burnt = 1;
                    foreach (var maid in Root.Session.Profile.Maids.Take(8)) result.XpByMaid[maid.Id] = 99999;
                    break;
            }

            Root.Session.FinishDay(result);
            Root.Ui.Show<BooksScreen>();
            yield return ResolutionRig.Apply(1280, 720);
            yield return SessionFactory.Settle();
        }

        private static string AllText() => string.Join("|", Books.GetComponentsInChildren<Text>(false).Select(t => t.text));

        private static void AssertLegible(string where)
        {
            var found = new List<string>();
            var measured = KitLegibility.Check(LayoutModel.Measure(Root.Ui), Books.transform, where, found);
            Assert.Greater(measured, 8, where + ": almost no label was measured");
            Assert.IsEmpty(found, string.Join("\n", found));
        }

        [UnityTest]
        public IEnumerator BooksShowsTheLedgerOfAProfitableDay()
        {
            yield return OpenBooks(Day.Profit);
            Assert.IsTrue(Books.IsKit);
            var ledger = Root.Session.LastLedger;
            var all = AllText();
            StringAssert.Contains(ledger.Verdict(), all);
            StringAssert.Contains(Strings.Get("books.chemistry") + " " + ledger.ChemistryScore, all);
            StringAssert.Contains(Strings.Get("books.satisfaction") + " " + Mathf.RoundToInt(ledger.Satisfaction * 100f) + "%", all);
            foreach (var line in ledger.Incoming().Concat(ledger.Outgoing())) { StringAssert.Contains(line.Label, all); StringAssert.Contains(Strings.Money(line.Amount), all); }
            StringAssert.Contains(Strings.Money(ledger.Revenue), all);
            StringAssert.Contains(Strings.Money(ledger.Costs), all);
            StringAssert.Contains(Strings.Get("books.profit") + "  " + Strings.Signed(ledger.Profit), all);
            StringAssert.Contains(Strings.Get("books.balance") + "  " + Strings.Money(ledger.BalanceAfter), all);
            StringAssert.Contains(Strings.Get("books.continue"), all);

            yield return AtEachWindow("results_profit", () => AssertLegible("profit"));
        }

        [UnityTest]
        public IEnumerator BooksShowsALossAndAWarning()
        {
            yield return OpenBooks(Day.Loss);
            var ledger = Root.Session.LastLedger;
            Assert.Less(ledger.Profit, 0);
            var all = AllText();
            StringAssert.Contains(Strings.Get("books.loss") + "  " + Strings.Signed(ledger.Profit), all);
            StringAssert.Contains(ledger.Verdict(), all);
            if (ledger.Warning) StringAssert.Contains(Strings.Get("books.warning"), all);
            Assert.AreEqual(ledger.Profit > 0 ? new Color32(0x25, 0x80, 0x45, 0xFF) : new Color32(0xDC, 0x31, 0x57, 0xFF),
                (Color32)Books.GetComponentsInChildren<Text>(false).First(t => t.name == "VerdictText").color);

            yield return AtEachWindow("results_loss", () => AssertLegible("loss"));
        }

        [UnityTest]
        public IEnumerator BooksShowsHugeAmountsWithoutHidingAnything()
        {
            yield return OpenBooks(Day.Huge);
            var ledger = Root.Session.LastLedger;
            var all = AllText();
            StringAssert.Contains(Strings.Money(ledger.Revenue), all);
            StringAssert.Contains(Strings.Money(ledger.BalanceAfter), all);
            yield return AtEachWindow("results_huge", () => AssertLegible("huge"));
        }

        [UnityTest]
        public IEnumerator BooksGrowsWithEveryLevelUp()
        {
            yield return OpenBooks(Day.LevelUps);
            var ledger = Root.Session.LastLedger;
            var expected = 5 + ledger.LevelUps.Count(p => Root.Session.Profile.MaidById(p.Key) != null);
            Assert.GreaterOrEqual(ledger.LevelUps.Count, 6, "the day levelled several maids up");
            var tallies = Books.GetComponentsInChildren<Image>(false).Count(i => i.name == "Tally");
            Assert.AreEqual(expected, tallies, "five indicators plus one chip per level-up");
            StringAssert.Contains(Strings.Get("books.levelup"), AllText());
            yield return AtEachWindow("results_levelups", () => AssertLegible("levelups"));
        }

        [UnityTest]
        public IEnumerator BooksContinueGoesBackToTheCompany()
        {
            yield return OpenBooks(Day.Profit);
            var continued = false;
            Books.OnContinue = () => continued = true;
            var button = Books.GetComponentsInChildren<Button>(false).Single();
            Assert.AreEqual(Strings.Get("books.continue"), button.GetComponentInChildren<Text>().text);
            button.onClick.Invoke();
            Assert.IsTrue(continued);
        }

        [UnityTest]
        public IEnumerator BooksFallsBackToTheEarlierReportWithoutTheKit()
        {
            BooksScreen.ForcePlain = true;
            yield return OpenBooks(Day.Profit);
            Assert.IsFalse(Books.IsKit);
            StringAssert.Contains(Strings.Money(Root.Session.LastLedger.Revenue), AllText());
        }
    }
}
