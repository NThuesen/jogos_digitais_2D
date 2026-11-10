using System.Collections;
using System.IO;
using System.Linq;
using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Game.Ui;
using Neko.Game.View;
using Neko.Sim.Cafe;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Neko.Tests.PlayMode
{
    /// <summary>
    /// A real day in the real scene, in a café with the illustrated pieces, a rotated table and a
    /// rotated cat tower: every frame the views must sort the way <see cref="DepthSort"/> says, a
    /// plate must stay with its maid, and a click on a waiting party must still be answered.
    /// </summary>
    /// <remarks>
    /// Writes PNG frames at 960x600 and 1280x720 only when NEKO_CAPTURE_DIR is set.
    /// </remarks>
    public sealed class GameplayDepthTests
    {
        private const string ScenePath = "Assets/_Game/Scenes/Boot.unity";

        private static CafeGrid Layout()
        {
            var grid = new CafeGrid(13, 9);
            void Put(string id, int x, int y, bool rotated = false) =>
                Assert.IsTrue(grid.Place(PlaceableCatalog.ById(id), x, y, rotated), "place " + id);

            Put("stove", 3, 7);
            Put("teabar", 5, 7);
            Put("table_2", 3, 3);
            Put("table_2", 3, 1, true);     // turned on end: falls back to procedural art
            Put("table_4", 7, 3);
            Put("stage", 9, 6);
            Put("koi_pond", 10, 1);
            Put("window", 6, 6, true);      // a quarter turn of a 1x1 keeps its footprint
            Put("cat_tower", 11, 4, true);  // 2x1 after the turn: procedural
            Put("plant", 1, 6);
            return grid;
        }

        [UnityTest]
        public IEnumerator ADayInTheIllustratedRoomSortsAndAnswersClicks()
        {
            yield return SceneManager.LoadSceneAsync(ScenePath, LoadSceneMode.Single);
            for (var i = 0; i < 12; i++) yield return null;

            var root = GameRoot.Instance;
            root.Ui.Get<MenuScreen>().OnNewGame.Invoke();
            yield return null;

            var profile = root.Session.Profile;
            profile.Grid = Layout();
            root.Ui.Get<MenuScreen>().OnPlay.Invoke();
            yield return null;
            root.Ui.Show<HubScreen>();
            root.Ui.Get<HubScreen>().OnOpenDay.Invoke();
            yield return null;

            var presenter = root.Presenter;
            Assert.IsNotNull(presenter.Shift, "the day did not start");
            var grid = presenter.Shift.Grid;
            var room = root.Room;

            var directory = System.Environment.GetEnvironmentVariable("NEKO_CAPTURE_DIR");
            var clicked = false;
            var carriedSeen = false;
            var frames = 0;
            var elapsed = 0f;
            var nextShot = 4f;
            var shot = 0;

            while (elapsed < 70f && !presenter.Shift.Finished)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
                frames++;

                AssertPieces(room, grid);

                foreach (var maid in Object.FindObjectsByType<MaidView>(FindObjectsSortMode.None))
                {
                    var body = maid.transform.Find("Body").GetComponent<SpriteRenderer>();
                    var carried = maid.transform.Find("Carried").GetComponent<SpriteRenderer>();
                    var key = DepthSort.ActorKey(grid, maid.Feet);

                    Assert.AreEqual(Layers.ActorOrder(key), body.sortingOrder, "a maid is not sorted by her feet");
                    Assert.AreEqual(body.sortingOrder + 1, carried.sortingOrder, "her plate left her");
                    carriedSeen |= carried.enabled;
                }

                foreach (var customer in Object.FindObjectsByType<CustomerView>(FindObjectsSortMode.None))
                {
                    var body = customer.GetComponent<SpriteRenderer>();
                    // Somebody seated is keyed by the seat's cell (ART-INT-02); anyone else by their feet.
                    var customerKey = customer.SeatCell.HasValue
                        ? DepthSort.SeatedKey(grid, customer.SeatCell.Value, customer.Feet, customer.Sprites)
                        : DepthSort.ActorKey(grid, customer.Feet);
                    Assert.AreEqual(Layers.ActorOrder(customerKey),
                        body.sortingOrder, "a customer is not sorted by their feet");

                    // Seated behind a table's front layer: over its whole picture, under its top.
                    if (customer.SeatCell.HasValue)
                    {
                        var seat = customer.SeatCell.Value;
                        var table = grid.ItemAt(seat.X, seat.Y);
                        var entry = table != null ? IllustratedFurniture.For(table.Def, table.Width, table.Height) : null;
                        if (customer.Sprites.DrawsFrontLayer(entry) && seat.Y - table.Y >= entry.FrontRows)
                        {
                            Assert.Less(body.sortingOrder, Layers.ActorOrder(DepthSort.SolidKey(table)),
                                "a seated customer is in front of the table top");
                            Assert.Greater(body.sortingOrder, Layers.ActorOrder(DepthSort.BackKey(table)),
                                "a seated customer is behind the chairs");
                        }
                    }
                }

                // A tap on a waiting party is still answered through the unchanged hit test.
                if (!clicked)
                {
                    var waiting = presenter.Shift.Customers.FirstOrDefault(c =>
                        c.State == Neko.Sim.Service.CustomerState.Queueing);
                    if (waiting != null)
                    {
                        var at = presenter.WorldPositionOf(waiting);
                        Assert.AreNotEqual(Neko.Sim.Shift.AssistResult.Rejected, presenter.HandleClick(at),
                            "the click on a queueing party was not answered");
                        clicked = true;
                    }
                }

                if (directory != null && elapsed >= nextShot)
                {
                    nextShot += 5f;
                    yield return Shot(directory, "play_" + (shot++).ToString("00"), 960, 600);
                    yield return Shot(directory, "play_" + (shot - 1).ToString("00"), 1280, 720);
                }
            }

            Assert.Greater(frames, 100, "the day ran too briefly to mean anything");
            Assert.IsTrue(clicked, "no party ever queued, so the click was never tried");
            Assert.IsTrue(carriedSeen, "no maid ever carried anything");
        }

        private static void AssertPieces(RoomView room, CafeGrid grid)
        {
            foreach (var item in grid.Items)
            {
                if (!DepthSort.IsSolid(item.Def) && item.Def.Id != "koi_pond") continue;

                var name = item.Def.Id == "koi_pond" ? "koi_pondRaised" : item.Def.Id;
                var found = room.transform.Cast<Transform>().Where(t => t.name == name)
                    .Select(t => t.GetComponent<SpriteRenderer>())
                    .FirstOrDefault(r => Mathf.Abs(r.transform.position.x - (item.X + (item.Width - 1) * 0.5f)) < 0.01f &&
                                         r.sortingOrder == Layers.ActorOrder(DepthSort.SolidKey(item)));
                Assert.IsNotNull(found, name + " at " + item.X + "," + item.Y + " is not sorted by its front edge");
            }
        }

        private static IEnumerator Shot(string directory, string name, int width, int height)
        {
            Directory.CreateDirectory(directory);
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            var camera = Camera.main;
            var previous = RenderTexture.active;
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply(false, false);
                File.WriteAllBytes(Path.Combine(directory, name + "_" + width + "x" + height + ".png"),
                    texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                Object.DestroyImmediate(texture);
                Object.DestroyImmediate(target);
            }

            yield return null;
        }
    }
}
