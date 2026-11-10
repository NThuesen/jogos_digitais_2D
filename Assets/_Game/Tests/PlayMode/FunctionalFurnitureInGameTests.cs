using System.Collections;
using System.Collections.Generic;
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
    /// ART-INT-02: a real day in the real scene, in a café that holds all eight functional pieces.
    /// </summary>
    /// <remarks>
    /// The grid is swapped on the in-memory profile only; nothing is saved and no level or unlock
    /// is touched. Frames at 960x600 and 1280x720 are written only when NEKO_CAPTURE_DIR is set.
    /// </remarks>
    public sealed class FunctionalFurnitureInGameTests
    {
        private const string ScenePath = "Assets/_Game/Scenes/Boot.unity";

        /// <summary>Variant 0 holds every piece; 1 leaves only the four-seater to sit at and 2 only the
        /// stools, so the simulation has to seat customers there.</summary>
        private static CafeGrid Layout(int variant)
        {
            var grid = new CafeGrid(13, 9);

            void Put(string id, int x, int y, bool rotated = false) =>
                Assert.IsTrue(grid.Place(PlaceableCatalog.ById(id), x, y, rotated), "place " + id);

            Put("stove", 3, 7);
            Put("teabar", 5, 7);
            Put("dessert", 8, 7);
            Put("bakery", 11, 6);
            if (variant == 0)
            {
                Put("table_2", 3, 3);
                Put("table_2", 3, 1, true);
            }

            if (variant <= 1) Put("table_4", 7, 3);
            if (variant != 1)
            {
                Put("stool", 11, 3);
                Put("stool", 11, 2);
            }

            Put("plant", 1, 6);
            return grid;
        }

        [UnityTest]
        public IEnumerator AllEightPiecesAreDrawnSortedAndSurviveARealDay() =>
            RealDay(0, "game", new string[0]);

        [UnityTest]
        public IEnumerator TheFourSeaterSeatsRealCustomers() =>
            RealDay(1, "seats4", new[] { "table_4" });

        [UnityTest]
        public IEnumerator TheStoolsSeatRealCustomers() =>
            RealDay(2, "seatsStool", new[] { "stool" });

        private IEnumerator RealDay(int variant, string prefix, string[] mustSeat)
        {
            yield return SceneManager.LoadSceneAsync(ScenePath, LoadSceneMode.Single);
            for (var i = 0; i < 12; i++) yield return null;

            var root = GameRoot.Instance;
            root.Ui.Get<MenuScreen>().OnNewGame.Invoke();
            yield return null;

            var profile = root.Session.Profile;
            profile.Grid = Layout(variant);
            root.Ui.Get<MenuScreen>().OnPlay.Invoke();
            yield return null;
            root.Ui.Show<HubScreen>();
            root.Ui.Get<HubScreen>().OnOpenDay.Invoke();
            yield return null;

            var presenter = root.Presenter;
            Assert.IsNotNull(presenter.Shift, "the day did not start");
            var grid = presenter.Shift.Grid;
            var room = root.Room;

            // Every piece is on screen as its illustrated picture, not the procedural fallback.
            // A procedural piece is a runtime-built sprite with no resource name.
            var expected = new Dictionary<string, string>
            {
                { "stove", "Furniture/stove" }, { "teabar", "Furniture/teabar" },
                { "dessert", "Furniture/dessert" }, { "bakery", "Furniture/bakery" },
                { "stool", "Furniture/stool" }, { "table_4", "Furniture/table_4_front" },
            };
            foreach (var pair in expected)
            {
                if ((pair.Key == "table_4" && variant == 2) || (pair.Key == "stool" && variant == 1)) continue;
                var drawn = room.transform.Cast<Transform>()
                    .Where(t => t.name == pair.Key).Select(t => t.GetComponent<SpriteRenderer>().sprite);
                Assert.IsTrue(drawn.Any(s => s != null && s.name == pair.Value), pair.Key + " is not drawn from " + pair.Value);
            }

            if (variant == 0)
            {
                var tables = room.transform.Cast<Transform>().Where(t => t.name == "table_2")
                    .Select(t => t.GetComponent<SpriteRenderer>().sprite.name).OrderBy(n => n).ToList();
                CollectionAssert.AreEqual(new[] { "Furniture/table_2_front", "Furniture/table_2_rotated" }, tables,
                    "the upright table uses its own picture and the turned one uses its own");
            }

            var directory = System.Environment.GetEnvironmentVariable("NEKO_CAPTURE_DIR");
            var seated = new Dictionary<string, int>();
            var elapsed = 0f;
            var nextShot = 4f;
            var shot = 0;

            while (elapsed < 90f && !presenter.Shift.Finished)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;

                foreach (var customer in Object.FindObjectsByType<CustomerView>(FindObjectsSortMode.None))
                {
                    if (!customer.SeatCell.HasValue) continue;
                    var seat = customer.SeatCell.Value;
                    var table = grid.ItemAt(seat.X, seat.Y);
                    Assert.IsNotNull(table, "seated on empty floor");

                    var key = table.Def.Id + (table.Width < table.Height ? "_rotated" : "");
                    seated[key] = seated.TryGetValue(key, out var n) ? n + 1 : 1;

                    var body = customer.GetComponent<SpriteRenderer>();
                    var entry = IllustratedFurniture.For(table.Def, table.Width, table.Height);
                    Assert.AreEqual(Layers.ActorOrder(DepthSort.SeatedKey(grid, seat, customer.Feet)), body.sortingOrder,
                        "a seated customer is not keyed by her seat");
                    if (entry != null && entry.HasFrontLayer && seat.Y - table.Y >= entry.FrontRows)
                    {
                        Assert.Less(body.sortingOrder, Layers.ActorOrder(DepthSort.SolidKey(table)), key + ": in front of the top");
                        Assert.Greater(body.sortingOrder, Layers.ActorOrder(DepthSort.BackKey(table)), key + ": behind the chairs");
                    }
                }

                // Taps on every piece's cells never throw and never need the sprite: the hit test
                // is the logical grid.
                if (Mathf.FloorToInt(elapsed) % 7 == 3)
                {
                    foreach (var item in grid.Items)
                    {
                        presenter.HandleClick(new Vector3(item.X, item.Y, 0f));
                    }
                }

                if (directory != null && elapsed >= nextShot)
                {
                    nextShot += 6f;
                    yield return Shot(directory, prefix + "_" + (shot++).ToString("00"), 960, 600);
                    yield return Shot(directory, prefix + "_" + (shot - 1).ToString("00"), 1280, 720);
                }
            }

            Debug.Log("FURNITURE_QA seated customers observed (frames): " +
                      string.Join(", ", seated.Select(p => p.Key + "=" + p.Value)));
            Assert.IsTrue(seated.Keys.Any(), "nobody ever sat down, so no seat was checked");
            foreach (var id in mustSeat)
            {
                Assert.IsTrue(seated.Keys.Any(k => k.StartsWith(id)), "nobody was ever seated at " + id);
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
