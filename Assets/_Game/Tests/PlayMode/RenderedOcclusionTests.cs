using System.Collections;
using Neko.Game.Art;
using Neko.Game.View;
using Neko.Sim.Cafe;
using Neko.Sim.Maids;
using Neko.Sim.Shift;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Neko.Tests.PlayMode
{
    /// <summary>
    /// Occlusion measured on rendered pixels: a real <see cref="MaidView"/> crosses the back edge
    /// of a tall piece and the frame is compared with and without her.
    /// </summary>
    /// <remarks>
    /// A maid's view root is her logical position; her drawn sole is the body's offset below it
    /// (about 0.58 of a tile for an authored maid). These cases put the root on one side of the
    /// piece's rear edge and the sole on the other, which is exactly where a key taken from the
    /// root disagrees with what is on screen. Nothing here calls <see cref="DepthSort"/>.
    /// </remarks>
    public sealed class RenderedOcclusionTests
    {
        private const int Width = 960;
        private const int Height = 600;

        // The window piece placed at cell (5,3): footprint rows 2.5..3.5, art rising to about 4.17.
        private const int CellX = 5;
        private const int CellY = 3;

        private GameObject _room;
        private GameObject _cameraObject;
        private Camera _camera;
        private SpriteFactory _sprites;
        private CafeGrid _grid;

        [SetUp]
        public void Build()
        {
            IllustratedFurniture.Enabled = true;
            _grid = new CafeGrid(13, 9);
            Assert.IsTrue(_grid.Place(PlaceableCatalog.ById("window"), CellX, CellY));

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
            _camera.backgroundColor = Color.black;
            _camera.nearClipPlane = -20;
            _camera.farClipPlane = 50;
            _camera.transform.position = new Vector3(6, 4, -10);
            _camera.orthographicSize = 5.9f;

            // Whatever an earlier test left in the scene is not on this layer, so it is not drawn.
            _camera.cullingMask = 1 << IsolatedLayer;
        }

        [TearDown]
        public void Clean()
        {
            Object.Destroy(_room);
            Object.Destroy(_cameraObject);
            foreach (var view in Object.FindObjectsByType<MaidView>(FindObjectsSortMode.None)) Object.Destroy(view.gameObject);
        }

        private static int _shots;
        private const int IsolatedLayer = 31;

        private static void Isolate(GameObject root)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = IsolatedLayer;
        }

        private MaidView SpawnMaid(float rootY)
        {
            var maid = new Maid { Id = 1, Name = "QA", Archetype = Archetype.Genki, IdentityId = "suzu" };

            // Worker id 0 puts her ring offset at (+0.5, 0): the root lands on the window's column.
            var worker = new Worker { Id = 0, Maid = maid, X = CellX - 0.5f, Y = rootY };
            var view = MaidView.Create(null, _sprites, worker);
            view.Grid = _grid;
            view.transform.localScale = Vector3.one;
            view.Sync();
            Isolate(view.gameObject);
            return view;
        }

        /// <summary>Fraction of pixels in a world rectangle that differ from the frame without her.</summary>
        private float Changed(Color32[] without, Color32[] with, float x0, float x1, float y0, float y1)
        {
            // The camera is orthographic and centred on (6, 4); pixel row 0 is the bottom.
            var perUnit = Height / (2f * _camera.orthographicSize);
            int xs = Mathf.RoundToInt((x0 - 6f) * perUnit + Width / 2f), xe = Mathf.RoundToInt((x1 - 6f) * perUnit + Width / 2f);
            int ys = Mathf.RoundToInt((y0 - 4f) * perUnit + Height / 2f), ye = Mathf.RoundToInt((y1 - 4f) * perUnit + Height / 2f);

            var total = 0;
            var differing = 0;
            for (var y = ys; y < ye; y++)
            {
                for (var x = xs; x < xe; x++)
                {
                    var i = y * Width + x;
                    var delta = Mathf.Abs(without[i].r - with[i].r) + Mathf.Abs(without[i].g - with[i].g) +
                                Mathf.Abs(without[i].b - with[i].b);
                    total++;
                    if (delta > 36) differing++;
                }
            }

            Assert.Greater(total, 200, "the probe window is too small to mean anything");
            return differing / (float)total;
        }

        private Color32[] Render()
        {
            var target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            var previous = RenderTexture.active;
            try
            {
                _camera.targetTexture = target;
                _camera.aspect = (float)Width / Height;
                _camera.Render();
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                var dump = System.IO.Path.Combine(Application.dataPath, "../Logs/occlusion_" + (_shots++) + ".png");
                System.IO.File.WriteAllBytes(dump, texture.EncodeToPNG());
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

        private IEnumerator Frame(MaidView view)
        {
            // The pop-in tween (0.35 s) scales her up from nothing whenever a tween driver is alive;
            // measure only after it has finished, or a slow frame leaves her half-size and the
            // comparison reads an empty rectangle.
            yield return new WaitForSecondsRealtime(0.6f);
            view.transform.localScale = Vector3.one;
            view.Sync();
            yield return null;
        }

        // The body of the maid over the window art, clear of her edges and of the window's frame.
        private const float X0 = CellX - 0.18f, X1 = CellX + 0.18f;

        [UnityTest]
        public IEnumerator SoleInsideTheFootprintWhileTheRootIsBeyondItIsDrawnInFront()
        {
            yield return null;
            var without = Render();

            // Root 3.98: beyond the rear edge (3.5) and in row 4. Drawn sole 3.40: still inside.
            var view = SpawnMaid(3.98f);
            yield return Frame(view);
            var with = Render();

            Assert.Greater(Changed(without, with, X0, X1, 3.65f, 4.05f), 0.4f,
                "her sole is on the window's footprint, so she must be painted over it");
        }

        [UnityTest]
        public IEnumerator SoleJustBeyondTheRearEdgeIsHiddenByTheTallPiece()
        {
            yield return null;
            var without = Render();

            // Root 4.18, sole 3.60: just behind the rear edge, under the window's upper half.
            var view = SpawnMaid(4.18f);
            yield return Frame(view);
            var with = Render();

            Assert.Less(Changed(without, with, X0, X1, 3.7f, 4.05f), 0.05f,
                "she is standing behind the window; it must cover her");
        }

        [UnityTest]
        public IEnumerator SoleInFrontOfTheFrontEdgeIsDrawnOverThePiece()
        {
            yield return null;
            var without = Render();

            var view = SpawnMaid(2.78f);
            yield return Frame(view);
            var with = Render();

            Assert.Greater(Changed(without, with, X0, X1, 2.75f, 3.2f), 0.4f,
                "she is in front of the window and must be painted over it");
        }
    }
}
