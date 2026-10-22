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

            // From a clean business. PlayerPrefs survives between editor runs, so without
            // this the frames show whatever the last capture left behind — which is how one
            // run came out with fifteen maids and four of them called Aoi.
            root.Ui.Get<MenuScreen>().OnNewGame.Invoke();
            yield return null;

            // Give the player enough to look at: a few levels of café, some money, and
            // furniture in stock, so the screens are not all empty states.
            var profile = root.Session.Profile;
            profile.Wallet.Earn(4000);
            profile.Wallet.AddPoints(900);
            profile.Wallet.AddReputation(CafeLevelTable.ReputationForLevel(5));
            profile.OwnedPlaceables.Add("table_4");
            profile.OwnedPlaceables.Add("lantern_sakura");
            profile.OwnedEquipment.Add("head_sakura");
            profile.ApplyUnlocks();

            // A few cards out of a box, so the roster and the lineup are not three commons.
            var rng = new Neko.Sim.Core.Rng(21u);
            profile.Add(Neko.Sim.Economy.Lootbox.Mint(profile, Neko.Sim.Team.Rarity.Legendary, rng));
            profile.Add(Neko.Sim.Economy.Lootbox.Mint(profile, Neko.Sim.Team.Rarity.Epic, rng));
            profile.Add(Neko.Sim.Economy.Lootbox.Mint(profile, Neko.Sim.Team.Rarity.Rare, rng));
            profile.Lineup.AutoFill(profile.Maids);

            yield return Shot("01_menu");

            root.Ui.Get<MenuScreen>().OnPlay.Invoke();
            yield return Shot("02_hub");

            root.Ui.Show<LineupScreen>();
            yield return Shot("03_lineup");

            root.Ui.Show<RosterScreen>();
            yield return Shot("04_roster");

            root.Ui.Show<BoxScreen>();
            yield return Shot("05_boxes");

            root.Ui.Show<ShopScreen>();
            yield return Shot("06_shop");

            root.Ui.Show<HallScreen>();
            yield return Shot("07_hall");

            root.Ui.Show<HubScreen>();
            root.Ui.Get<HubScreen>().OnOpenDay.Invoke();
            yield return Wait(6f);
            yield return Shot("08_day_early");

            yield return Wait(26f);
            yield return Shot("09_day_busy");

            yield return Wait(40f);
            yield return Shot("10_day_late");

            // Run the day out with the skip button, which is what a player in a hurry
            // presses and which therefore has to work.
            root.Ui.Get<DayHud>().OnSkip.Invoke();
            yield return null;
            yield return null;

            Assert.AreEqual(AppState.Results, root.State.Current,
                "the day never finished, so there is no books screen to capture");

            yield return Shot("11_books");

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
