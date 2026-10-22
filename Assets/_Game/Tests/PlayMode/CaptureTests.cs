using System.Collections;
using System.IO;
using Neko.Game.Core;
using Neko.Game.Ui;
using Neko.Sim.Economy;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Neko.Tests.PlayMode
{
    /// <summary>
    /// Walks the game and writes a frame at each moment worth looking at.
    /// </summary>
    /// <remarks>
    /// The art is generated, so there is no scene to open and judge by eye — this is the only
    /// way to see what the game actually looks like. Driven by the studio's unity MCP server,
    /// which decodes the dumps and reports each one's palette; writes nothing unless
    /// NEKO_CAPTURE_DIR is set.
    ///
    /// The first frames are discarded on purpose: batch mode renders them before material
    /// albedo is ready, and a flat, desaturated capture nearly always means a test that did
    /// not throw them away.
    /// </remarks>
    public sealed class CaptureTests
    {
        private const int DiscardFrames = 12;
        private const string ScenePath = "Assets/_Game/Scenes/Boot.unity";

        private string _directory;
        private int _index;

        [UnityTest]
        public IEnumerator CaptureTheGame()
        {
            _directory = System.Environment.GetEnvironmentVariable("NEKO_CAPTURE_DIR");
            if (string.IsNullOrEmpty(_directory))
            {
                Assert.Pass("NEKO_CAPTURE_DIR not set; nothing to capture.");
                yield break;
            }

            Directory.CreateDirectory(_directory);

            yield return SceneManager.LoadSceneAsync(ScenePath, LoadSceneMode.Single);
            for (var i = 0; i < DiscardFrames; i++) yield return null;

            var root = GameRoot.Instance;
            Assert.IsNotNull(root);

            // Give the player enough to look at: a few levels of café, some money, and
            // furniture in stock, so the screens are not all empty states.
            root.Session.Profile.Wallet.Earn(4000);
            root.Session.Profile.Wallet.AddReputation(CafeLevelTable.ReputationForLevel(4));
            root.Session.Profile.OwnedPlaceables.Add("table_4");
            root.Session.Profile.OwnedPlaceables.Add("lantern_sakura");
            root.Session.Profile.OwnedEquipment.Add("head_sakura");
            root.Session.Profile.ApplyUnlocks();

            yield return Shot("01_menu");

            root.Ui.Get<MenuScreen>().OnPlay.Invoke();
            yield return Shot("02_cafe");

            root.Ui.Show<RosterScreen>();
            yield return Shot("03_roster");

            root.Ui.Show<ShopScreen>();
            yield return Shot("04_shop");

            root.Ui.Show<EditorScreen>();
            yield return Shot("05_editor");

            root.Ui.Show<CafeScreen>();
            root.Ui.Get<CafeScreen>().OnOpenDay.Invoke();
            yield return Wait(6f);
            yield return Shot("06_shift_early");

            yield return Wait(26f);
            yield return Shot("07_shift_busy");

            yield return Wait(40f);
            yield return Shot("08_shift_late");

            // Run the day out so the results screen has something real on it. Counted in
            // seconds rather than frames: a frame budget that looked generous ran out part way
            // through the closing grace period and captured the shift again.
            var waited = 0f;
            while (!root.Presenter.Shift.Finished && waited < 90f)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }

            yield return null;
            yield return null;
            Assert.AreEqual(AppState.Results, root.State.Current,
                "the day never finished, so there is no results screen to capture");

            yield return Shot("09_results");

            Assert.Greater(Directory.GetFiles(_directory).Length, 0);
        }

        private IEnumerator Wait(float seconds)
        {
            var spent = 0f;
            while (spent < seconds)
            {
                spent += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        /// <summary>
        /// Writes one raw RGBA dump, which the MCP server turns into a PNG.
        /// </summary>
        /// <remarks>
        /// Renders the camera into a texture rather than reading back the screen. Batch mode
        /// never evokes WaitForEndOfFrame — a yield on it throws outright — so reading the
        /// back buffer is not available, and asking the camera to draw once is both available
        /// and deterministic.
        /// </remarks>
        private IEnumerator Shot(string name)
        {
            yield return null;
            yield return null;

            const int width = 1280;
            const int height = 720;

            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);

            // RGB24, which is the layout the studio's decoder expects: an eight-byte header
            // of width and height, then three bytes a pixel, bottom row first.
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

                var path = Path.Combine(_directory,
                    (_index++).ToString("00") + "_" + name + "_" + width + "x" + height + ".raw");

                var body = texture.GetRawTextureData();
                var file = new byte[8 + body.Length];
                System.BitConverter.GetBytes(width).CopyTo(file, 0);
                System.BitConverter.GetBytes(height).CopyTo(file, 4);
                body.CopyTo(file, 8);

                File.WriteAllBytes(path, file);
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                Object.DestroyImmediate(texture);
                Object.DestroyImmediate(target);
            }
        }
    }
}
