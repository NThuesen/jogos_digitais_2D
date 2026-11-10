using System.Collections;
using System.Collections.Generic;
using System.IO;
using Neko.Game.Art;
using Neko.Game.View;
using Neko.Sim.Cafe;
using Neko.Sim.Maids;
using Neko.Sim.Service;
using Neko.Sim.Shift;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Neko.Tests.PlayMode
{
    /// <summary>
    /// ART-INT-02: the eight functional pieces, rendered in a fixture room with real
    /// <see cref="CustomerView"/> and <see cref="MaidView"/> instances.
    /// </summary>
    /// <remarks>
    /// The fixture places pieces straight on a <see cref="CafeGrid"/> and never touches a save,
    /// the shop or the level unlocks. Frames go to NEKO_QA_DIR (default Logs/furniture_qa) so they
    /// can be looked at; the assertions cover what a frame cannot say by itself: each piece
    /// resolves to its own illustrated sprite at its own footprint, a footprint without a picture
    /// falls back, and the switch back to procedural art works.
    /// </remarks>
    public sealed class FunctionalFurnitureQaTests
    {
        private const int IsolatedLayer = 31;

        private static readonly (string id, int w, int h, bool rotated, int x, int y)[] Pieces =
        {
            ("table_2", 2, 1, false, 1, 6),
            ("table_2", 1, 2, true, 5, 5),
            ("table_4", 2, 2, false, 8, 5),
            ("stool", 1, 1, false, 11, 6),
            ("stove", 1, 1, false, 1, 2),
            ("teabar", 2, 1, false, 3, 2),
            ("dessert", 1, 1, false, 6, 2),
            ("bakery", 1, 2, false, 9, 1),
        };

        private GameObject _room;
        private GameObject _cameraObject;
        private Camera _camera;
        private SpriteFactory _sprites;
        private CafeGrid _grid;
        private readonly List<GameObject> _actors = new List<GameObject>();

        [TearDown]
        public void Clean()
        {
            IllustratedFurniture.Enabled = true;
            if (_room != null) Object.Destroy(_room);
            if (_cameraObject != null) Object.Destroy(_cameraObject);
            foreach (var actor in _actors) if (actor != null) Object.Destroy(actor);
            _actors.Clear();
        }

        private void BuildRoom(bool illustrated)
        {
            Clean();
            IllustratedFurniture.Enabled = illustrated;
            _grid = new CafeGrid(13, 9);
            foreach (var p in Pieces)
            {
                Assert.IsTrue(_grid.Place(PlaceableCatalog.ById(p.id), p.x, p.y, p.rotated),
                    p.id + " does not fit the fixture");
            }

            _sprites = new SpriteFactory();
            _room = new GameObject("QA Room");
            var room = _room.AddComponent<RoomView>();
            room.Bind(_sprites);
            room.Build(_grid);
            Isolate(_room);

            _cameraObject = new GameObject("QA Camera");
            _camera = _cameraObject.AddComponent<Camera>();
            _camera.orthographic = true;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0.1f, 0.1f, 0.12f);
            _camera.nearClipPlane = -20;
            _camera.farClipPlane = 50;
            _camera.cullingMask = 1 << IsolatedLayer;
        }

        private static void Isolate(GameObject root)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = IsolatedLayer;
        }

        private CustomerView SeatAt(Cell cell, int id)
        {
            var kinds = new[]
            {
                CustomerType.Regular, CustomerType.Student, CustomerType.Salaryman,
                CustomerType.Otaku, CustomerType.Critic,
            };
            var customer = new Customer
            {
                Id = id, Kind = kinds[id % kinds.Length], State = CustomerState.Eating, SeatIndex = id - 1,
            };
            var view = CustomerView.Create(null, _sprites, customer);
            view.Grid = _grid;

            // The same placement ShiftPresenter gives a seated customer.
            var item = _grid.ItemAt(cell.X, cell.Y);
            var entry = IllustratedFurniture.For(item.Def, item.Width, item.Height);
            var offset = entry != null ? entry.SeatOffset(cell.X - item.X, cell.Y - item.Y) : Vector2.zero;
            view.SeatCell = cell;
            view.Place(new Vector3(cell.X + offset.x, cell.Y + 0.42f + offset.y, 0f));
            view.transform.localScale = Vector3.one * CustomerView.CustomerScale;
            view.Sync();
            Isolate(view.gameObject);
            _actors.Add(view.gameObject);
            return view;
        }

        private void SeatEveryone()
        {
            var id = 1;
            foreach (var cell in _grid.SeatPositions()) SeatAt(cell, id++);
        }

        private MaidView AddMaid(int workerId, string identity, float feetX, float feetY)
        {
            var maid = new Maid
            {
                Id = workerId + 1, Name = "QA" + workerId, Archetype = Archetype.Genki, IdentityId = identity,
            };
            var worker = new Worker { Id = workerId, Maid = maid, X = feetX, Y = feetY };
            var view = MaidView.Create(null, _sprites, worker);
            view.Grid = _grid;
            view.transform.localScale = Vector3.one;
            view.Sync();

            // Put her drawn sole where asked, whatever her sprite's offset is. Moving the worker
            // (not the view) keeps it there across later Sync calls.
            var shift = new Vector3(feetX, feetY, 0f) - view.Feet;
            worker.X += shift.x;
            worker.Y += shift.y;
            view.transform.position += shift;
            view.Sync();
            Assert.AreEqual(feetX, view.Feet.x, 0.05f, "maid sole x");
            Assert.AreEqual(feetY, view.Feet.y, 0.05f, "maid sole y");
            Isolate(view.gameObject);
            _actors.Add(view.gameObject);
            return view;
        }

        private void MaidsAroundTheRoom()
        {
            // Walking in front of and behind the tables, and working each station.
            AddMaid(0, "suzu", 1.5f, 5.15f);   // in front of table_2
            AddMaid(1, "mikan", 2.4f, 7.55f);  // behind table_2
            AddMaid(2, "hibari", 4.2f, 5.6f);  // beside the rotated table
            AddMaid(3, "hina", 8.5f, 4.35f);   // in front of table_4
            AddMaid(4, "yuki", 8.5f, 7.65f);   // behind table_4
            AddMaid(5, "seira", 1.0f, 2.0f);   // at the stove
            AddMaid(6, "akari", 3.5f, 2.0f);   // at the teabar
            AddMaid(7, "kurumi", 6.0f, 2.0f);  // at the dessert counter
            AddMaid(8, "ruri", 9.0f, 1.5f);    // at the bakery
        }

        private IEnumerator Settle()
        {
            // The pop-in tweens scale actors up from nothing; wait them out, then pin the scale.
            yield return new WaitForSecondsRealtime(0.6f);
            foreach (var actor in _actors)
            {
                if (actor == null) continue;
                var customer = actor.GetComponent<CustomerView>();
                if (customer != null)
                {
                    customer.transform.localScale = Vector3.one * CustomerView.CustomerScale;
                    customer.Sync();
                }

                var maid = actor.GetComponent<MaidView>();
                if (maid != null)
                {
                    maid.transform.localScale = Vector3.one;
                    maid.Sync();
                }
            }

            yield return null;
        }

        private static string OutputDirectory()
        {
            var dir = System.Environment.GetEnvironmentVariable("NEKO_QA_DIR");
            if (string.IsNullOrEmpty(dir)) dir = Path.Combine(Application.dataPath, "../Logs/furniture_qa");
            Directory.CreateDirectory(dir);
            return dir;
        }

        private void Shot(string name, int width, int height, Vector2 centre, float halfHeight)
        {
            _camera.transform.position = new Vector3(centre.x, centre.y, -10f);
            _camera.orthographicSize = halfHeight;
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            var previous = RenderTexture.active;
            try
            {
                _camera.targetTexture = target;
                _camera.aspect = (float)width / height;
                _camera.Render();
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                File.WriteAllBytes(Path.Combine(OutputDirectory(), name + "_" + width + "x" + height + ".png"),
                    texture.EncodeToPNG());
            }
            finally
            {
                _camera.targetTexture = null;
                RenderTexture.active = previous;
                Object.Destroy(texture);
                Object.Destroy(target);
            }
        }

        private void ShootAll(string label)
        {
            foreach (var size in new[] { new Vector2Int(960, 600), new Vector2Int(1280, 720) })
            {
                Shot(label + "_room", size.x, size.y, new Vector2(6f, 4.4f), 4.9f);
                Shot(label + "_tables", size.x, size.y, new Vector2(5.8f, 6.2f), 2.6f);
                Shot(label + "_stations", size.x, size.y, new Vector2(6.0f, 2.4f), 2.6f);
            }
        }

        // ---- what a frame cannot say ------------------------------------------------------------

        [Test]
        public void EveryFunctionalPieceResolvesToItsOwnPictureAtItsOwnFootprint()
        {
            var sprites = new SpriteFactory();
            foreach (var p in Pieces)
            {
                var def = PlaceableCatalog.ById(p.id);
                var entry = IllustratedFurniture.For(def, p.w, p.h);
                Assert.IsNotNull(entry, p.id + " " + p.w + "x" + p.h + " has no illustrated entry");
                var sprite = sprites.Illustrated(entry, false);
                Assert.IsNotNull(sprite, p.id + " " + entry.Resource + " did not load");
                Assert.AreEqual(entry.PixelWidth, Mathf.RoundToInt(sprite.rect.width));
                Assert.AreEqual(0.5f, sprite.pivot.x / sprite.rect.width, 1e-3f, "pivot is bottom centre");
                Assert.AreEqual(0f, sprite.pivot.y, 1e-3f);
                Assert.AreEqual(entry.WidthUnits, sprite.bounds.size.x, 0.01f, "width in world units");
            }

            sprites.Clear();
        }

        [Test]
        public void AFootprintWithoutItsOwnPictureStaysProcedural()
        {
            Assert.IsNull(IllustratedFurniture.For(PlaceableCatalog.ById("teabar"), 1, 2));
            Assert.IsNull(IllustratedFurniture.For(PlaceableCatalog.ById("stove"), 2, 1));
            Assert.IsNull(IllustratedFurniture.For(PlaceableCatalog.ById("bakery"), 2, 1));
            Assert.IsNotNull(IllustratedFurniture.For(PlaceableCatalog.ById("table_2"), 1, 2),
                "the rotated table has its own picture");
            IllustratedFurniture.Enabled = false;
            try { Assert.IsNull(IllustratedFurniture.For(PlaceableCatalog.ById("table_4"), 2, 2)); }
            finally { IllustratedFurniture.Enabled = true; }
        }

        // ---- sorting, measured on rendered pixels --------------------------------------------------

        private const int W = 960, H = 600;

        private Color32[] Pixels()
        {
            _camera.transform.position = new Vector3(6f, 4.4f, -10f);
            _camera.orthographicSize = 4.9f;
            var target = new RenderTexture(W, H, 24, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(W, H, TextureFormat.RGB24, false);
            var previous = RenderTexture.active;
            try
            {
                _camera.targetTexture = target;
                _camera.aspect = (float)W / H;
                _camera.Render();
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, W, H), 0, 0);
                return texture.GetPixels32();
            }
            finally
            {
                _camera.targetTexture = null;
                RenderTexture.active = previous;
                Object.Destroy(texture);
                Object.Destroy(target);
            }
        }

        /// <summary>Fraction of pixels in a world rectangle that differ between two frames.</summary>
        private static float Changed(Color32[] without, Color32[] with, float x0, float x1, float y0, float y1)
        {
            var perUnit = H / (2f * 4.9f);
            int xs = Mathf.RoundToInt((x0 - 6f) * perUnit + W / 2f), xe = Mathf.RoundToInt((x1 - 6f) * perUnit + W / 2f);
            int ys = Mathf.RoundToInt((y0 - 4.4f) * perUnit + H / 2f), ye = Mathf.RoundToInt((y1 - 4.4f) * perUnit + H / 2f);
            var total = 0;
            var differing = 0;
            for (var y = ys; y < ye; y++)
            {
                for (var x = xs; x < xe; x++)
                {
                    var i = y * W + x;
                    var delta = Mathf.Abs(without[i].r - with[i].r) + Mathf.Abs(without[i].g - with[i].g) +
                                Mathf.Abs(without[i].b - with[i].b);
                    total++;
                    if (delta > 36) differing++;
                }
            }

            Assert.Greater(total, 20, "the probe window is too small to mean anything");
            return differing / (float)total;
        }

        private static Bounds BodyBounds(CustomerView view) => view.GetComponent<SpriteRenderer>().bounds;

        /// <summary>
        /// World y of the highest drawn pixel of a customer, measured by drawing her alone on bare
        /// floor: the sprite's bounds carry transparent headroom.
        /// </summary>
        private float VisibleTop(CustomerView seated)
        {
            var without = Pixels();
            seated.gameObject.SetActive(false);
            var empty = Pixels();
            seated.gameObject.SetActive(true);

            // Her own column, from her bounds' top downwards: the first row that differs once she
            // is drawn is her head, wherever the furniture is, because the probe is taken with her
            // hidden and shown over the same background.
            var b = BodyBounds(seated);
            var perUnit = H / (2f * 4.9f);
            for (var y = b.max.y; y > b.min.y; y -= 1f / perUnit)
            {
                if (Changed(empty, without, b.center.x - 0.12f, b.center.x + 0.12f, y - 2f / perUnit, y) > 0.2f) return y;
            }

            return b.max.y;
        }

        private IEnumerator Pop()
        {
            yield return new WaitForSecondsRealtime(0.5f);
            foreach (var actor in _actors)
            {
                var customer = actor != null ? actor.GetComponent<CustomerView>() : null;
                if (customer != null) { customer.transform.localScale = Vector3.one * CustomerView.CustomerScale; customer.Sync(); }
                var maid = actor != null ? actor.GetComponent<MaidView>() : null;
                if (maid != null) { maid.transform.localScale = Vector3.one; maid.Sync(); }
            }

            yield return null;
        }

        /// <summary>
        /// A customer on this seat: her head is drawn (not covered by the table), the part of her
        /// the table is in front of is covered, and (for a near row) she is over the chair.
        /// </summary>
        private IEnumerator SeatedBehindTheTop(int x, int y, string what)
        {
            BuildRoom(true);
            yield return null;
            var without = Pixels();
            var view = SeatAt(new Cell(x, y), 1);
            yield return Pop();
            var with = Pixels();
            var b = BodyBounds(view);
            var cx = b.center.x;
            var top = VisibleTop(view);
            Assert.Greater(top - b.min.y, 0.3f, what + ": most of her is drawn above the table top");

            Assert.Greater(Changed(without, with, cx - 0.1f, cx + 0.1f, top - 0.16f, top - 0.03f), 0.2f,
                what + ": her head must be drawn");
            Assert.Less(Changed(without, with, cx - 0.1f, cx + 0.1f, b.min.y + 0.05f, b.min.y + 0.22f), 0.1f,
                what + ": the table top must cover her lap");
        }

        [UnityTest]
        public IEnumerator ASeatedCustomerAtATwoSeaterIsBehindItsTop() => SeatedBehindTheTop(1, 6, "table_2 left seat");

        [UnityTest]
        public IEnumerator ASecondSeatedCustomerAtATwoSeaterIsBehindItsTop() => SeatedBehindTheTop(2, 6, "table_2 right seat");

        [UnityTest]
        public IEnumerator ARearSeatOfAFourSeaterIsBehindItsTop() => SeatedBehindTheTop(8, 6, "table_4 rear seat");

        [UnityTest]
        public IEnumerator ANearSeatOfAFourSeaterIsInFrontOfItsTopAndOnItsChair()
        {
            BuildRoom(true);
            yield return null;
            var without = Pixels();
            var view = SeatAt(new Cell(8, 5), 1);
            yield return Pop();
            var with = Pixels();
            var b = BodyBounds(view);
            var cx = b.center.x;

            Assert.Greater(Changed(without, with, cx - 0.1f, cx + 0.1f, b.min.y + 0.05f, b.max.y - 0.04f), 0.3f,
                "a near-row customer is drawn whole, over the chair");
        }

        [UnityTest]
        public IEnumerator BothCustomersAtTheTurnedTableAreDrawnWholeOnTheirChairs()
        {
            BuildRoom(true);
            yield return null;
            var without = Pixels();
            var a = SeatAt(new Cell(5, 5), 1);
            var c = SeatAt(new Cell(5, 6), 2);
            yield return Pop();
            var with = Pixels();
            foreach (var view in new[] { a, c })
            {
                var b = BodyBounds(view);
                Assert.Greater(Changed(without, with, b.center.x - 0.1f, b.center.x + 0.1f, b.min.y + 0.05f, b.max.y - 0.04f),
                    0.3f, "drawn whole");
            }
        }

        [UnityTest]
        public IEnumerator AMaidBehindATwoSeaterIsCoveredByItAndOneInFrontCoversIt()
        {
            BuildRoom(true);
            yield return null;
            var without = Pixels();

            // Behind: feet beyond the rear edge (6.5), on the left chair's column.
            AddMaid(0, "suzu", 1.0f, 6.7f);
            yield return Pop();
            var behind = Pixels();
            Assert.Less(Changed(without, behind, 0.9f, 1.1f, 6.75f, 7.0f), 0.1f,
                "she stands behind the chair, which covers her");

            Clean();
            BuildRoom(true);
            yield return null;
            without = Pixels();
            AddMaid(0, "suzu", 1.0f, 5.2f);
            yield return Pop();
            var front = Pixels();
            Assert.Greater(Changed(without, front, 0.85f, 1.15f, 5.3f, 5.7f), 0.4f,
                "she stands in front of the legs and covers them");
        }

        // ---- frames -----------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator FixtureRoomIllustratedOccupiedEmptyAndProcedural()
        {
            BuildRoom(illustrated: true);
            yield return null;
            ShootAll("ill_empty");

            SeatEveryone();
            AddMaidsAndSettle();
            yield return Settle();
            ShootAll("ill_occupied");

            BuildRoom(illustrated: false);
            SeatEveryone();
            AddMaidsAndSettle();
            yield return Settle();
            ShootAll("proc_occupied");

            BuildRoom(illustrated: false);
            yield return null;
            ShootAll("proc_empty");
        }

        private void AddMaidsAndSettle() => MaidsAroundTheRoom();
    }
}
