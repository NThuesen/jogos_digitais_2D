using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Neko.Game.Core;
using Neko.Game.Ui;
using Neko.Game.View;
using Neko.Sim.Maids;
using Neko.Sim.Progress;
using Neko.Sim.Save;
using Neko.Sim.Shift;
using Neko.Sim.Team;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Neko.Tests.PlayMode
{
    /// <summary>
    /// A real shift with Suzu on the floor. The fixture renames and re-types one maid in memory
    /// only; the roster, the draw and the save format are the game's own, and the saved profile
    /// is put back whatever the test does.
    /// </summary>
    public sealed class SuzuGameplayTests
    {
        private bool _hadSave;
        private string _save;
        private GameRoot _root;

        [UnitySetUp]
        public IEnumerator Load()
        {
            _hadSave = PlayerPrefs.HasKey(SaveBridge.Key);
            _save = PlayerPrefs.GetString(SaveBridge.Key, "");
            SaveBridge.Save(Profile.NewGame(21u));
            yield return SceneManager.LoadSceneAsync("Assets/_Game/Scenes/Boot.unity", LoadSceneMode.Single);
            yield return null;

            _root = GameRoot.Instance;
            _root.Ui.Get<MenuScreen>().OnNewGame.Invoke();

            var profile = _root.Session.Profile;
            var floor = profile.Maids.First(m => m.NaturalRole == Role.Floor);
            floor.Name = "Suzu";
            floor.Archetype = Archetype.Genki;
            profile.Lineup.AutoFill(profile.Maids);

            _root.Ui.Show<LineupScreen>();
            yield return null;
            _root.Ui.Show<HubScreen>();
            _root.Ui.Get<HubScreen>().OnOpenDay.Invoke();
            for (var i = 0; i < 14; i++) yield return null;
            Assert.AreEqual(AppState.Shift, _root.State.Current);
        }

        [UnityTearDown]
        public IEnumerator Restore()
        {
            if (_root != null) Object.Destroy(_root.gameObject);
            yield return null;
            if (_hadSave) PlayerPrefs.SetString(SaveBridge.Key, _save);
            else PlayerPrefs.DeleteKey(SaveBridge.Key);
            PlayerPrefs.Save();
            Clocks.Reset();
        }

        private MaidView Suzu => Object.FindObjectsByType<MaidView>()
            .Single(view => view.Worker.Name == "Suzu");

        private static SpriteRenderer Body(MaidView view) =>
            view.transform.Find("Body").GetComponent<SpriteRenderer>();

        private static bool IsSuzuFrame(Sprite sprite) =>
            sprite != null && sprite.name.StartsWith(SuzuSprites.Resource + "#");

        private static int FrameOf(MaidView view)
        {
            var name = Body(view).sprite.name;
            return int.Parse(name.Substring(name.IndexOf('#') + 1));
        }

        [UnityTest]
        public IEnumerator SuzuWalksAlongsideProceduralMaidsAndTheDayStillEnds()
        {
            var view = Suzu;
            Assert.IsTrue(IsSuzuFrame(Body(view).sprite), "Suzu starts on the authored sheet");
            foreach (var other in Object.FindObjectsByType<MaidView>().Where(v => v != view))
                Assert.IsFalse(IsSuzuFrame(Body(other).sprite), "the others stay procedural");

            var frames = new HashSet<int>();
            var left = false;
            var right = false;
            Clocks.Scale = 4f;
            var elapsed = 0f;
            for (var i = 0; i < 12000 && elapsed < 65f; i++)
            {
                elapsed += Clocks.Game;
                var body = Body(view);
                Assert.IsTrue(IsSuzuFrame(body.sprite));
                Assert.AreEqual(SuzuSprites.BodyOffset, body.transform.localPosition, "no second bob");
                frames.Add(FrameOf(view));
                left |= body.flipX;
                right |= !body.flipX;
                yield return null;
            }

            Assert.Greater(frames.Count, 8, "a real shift walks through much of the cycle");
            Assert.IsTrue(left && right, "the worker travels both ways and the sprite follows");

            _root.Ui.Get<DayHud>().OnSkip.Invoke();
            yield return null;
            Assert.AreEqual(AppState.Results, _root.State.Current);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator PausingFreezesTheFrame()
        {
            var view = Suzu;
            var midStride = false;
            for (var i = 0; i < 6000 && !midStride; i++)
            {
                midStride = System.Array.IndexOf(SuzuSprites.RestFrames, FrameOf(view)) < 0;
                yield return null;
            }

            Assert.IsTrue(midStride, "she must be caught mid-stride in a real shift");

            Clocks.Pause(true);
            var frozen = Body(view).sprite;
            for (var i = 0; i < 10; i++)
            {
                view.Sync();
                yield return null;
            }

            Assert.AreSame(frozen, Body(view).sprite, "no movement, no new frame");
        }

        [UnityTest]
        public IEnumerator EquipmentAndTirednessFallBackWithoutBreakingTheView()
        {
            var view = Suzu;
            var worker = view.Worker;
            Clocks.Pause(true);

            worker.OnBreak = true;
            view.Sync();
            Assert.IsTrue(IsSuzuFrame(Body(view).sprite), "tired reuses the standing frame");
            Assert.AreNotEqual(Color.white, Body(view).color, "and is dimmed");
            worker.OnBreak = false;
            view.Sync();
            Assert.AreEqual(Color.white, Body(view).color);

            worker.Carrying = true;
            view.Sync();
            Assert.IsTrue(IsSuzuFrame(Body(view).sprite));
            Assert.IsTrue(view.transform.Find("Carried").GetComponent<SpriteRenderer>().enabled,
                "a plate at her hands says she is carrying");
            worker.Carrying = false;

            worker.Maid.Gear[0] = new Equipment("fixture", "fixture", GearSlot.Outfit,
                Stats.Zero, Neko.Sim.Core.ThemeTag.None, 0, 1, "Apron", "FF0000");
            view.Bind(worker);
            Assert.IsFalse(IsSuzuFrame(Body(view).sprite), "a geared maid keeps the layered look");
            worker.Maid.Gear[0] = null;
            view.Bind(worker);
            Assert.IsTrue(IsSuzuFrame(Body(view).sprite));
            yield return null;
        }

        // ---- captures: only when NEKO_SUZU_CAPTURE_DIR is set --------------------------------

        [UnityTest]
        public IEnumerator CaptureWalkAtBothResolutions()
        {
            var directory = System.Environment.GetEnvironmentVariable("NEKO_SUZU_CAPTURE_DIR");
            if (string.IsNullOrEmpty(directory)) yield break;
            Directory.CreateDirectory(directory);

            var view = Suzu;
            var shots = 0;
            var last = view.transform.position;
            var still = 0;
            for (var i = 0; i < 30000 && shots < 200; i++)
            {
                var moved = (view.transform.position - last).sqrMagnitude > 1e-6f;
                last = view.transform.position;
                still = moved ? 0 : still + 1;

                // Fast while nothing happens, close to real time while she walks.
                Clocks.Scale = still > 3 ? 4f : 0.3f;

                // Every rendered frame while she is on the move, plus the settling that follows:
                // the stills are cut into a strip or a GIF afterwards.
                if (still < 40 && i > 20 && (moved || shots > 0))
                {
                    yield return Shot(directory, "seq_" + shots.ToString("D3"), 960, 600, view);
                    if (shots % 20 == 0) yield return Shot(directory, "seq_" + shots.ToString("D3"), 1280, 720, view);
                    shots++;
                }

                yield return null;
            }

            Assert.Greater(shots, 20, "Suzu never walked in the capture window");
        }

        [UnityTest]
        public IEnumerator CaptureBiggerRoomPointAgainstBilinear()
        {
            var directory = System.Environment.GetEnvironmentVariable("NEKO_SUZU_CAPTURE_DIR");
            if (string.IsNullOrEmpty(directory)) yield break;
            Directory.CreateDirectory(directory);

            // Ordinary progression only: the same unlock path a player takes to the 13x9 room.
            _root.Ui.Get<DayHud>().OnSkip.Invoke();
            yield return null;
            _root.Ui.Get<BooksScreen>().OnContinue.Invoke();
            var profile = _root.Session.Profile;
            profile.Wallet.AddReputation(Neko.Sim.Economy.CafeLevelTable.ReputationForLevel(9));
            profile.ApplyUnlocks();
            profile.Lineup.AutoFill(profile.Maids);
            _root.Ui.Get<HubScreen>().OnOpenDay.Invoke();
            for (var i = 0; i < 14; i++) yield return null;
            Assert.AreEqual(AppState.Shift, _root.State.Current);

            var view = Suzu;
            var texture = Resources.Load<Texture2D>(SuzuSprites.Resource);
            Clocks.Scale = 4f;
            for (var i = 0; i < 20000; i++)
            {
                if (view.transform.hasChanged && FrameOf(view) != SuzuSprites.IdleFrame) break;
                yield return null;
            }

            Clocks.Pause(true);
            foreach (var mode in new[] { FilterMode.Point, FilterMode.Bilinear })
            {
                texture.filterMode = mode;
                yield return Shot(directory, "big_" + mode, 960, 600, view);
                yield return Shot(directory, "big_" + mode, 1280, 720, view);
            }

            texture.filterMode = FilterMode.Point;
        }

        private static IEnumerator Shot(string directory, string label, int width, int height, MaidView view)
        {
            var camera = Camera.main;
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                Canvas.ForceUpdateCanvases();
                camera.Render();
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();
                File.WriteAllBytes(Path.Combine(directory, label + "_" + width + "x" + height + ".png"),
                    texture.EncodeToPNG());

                var body = Body(view);
                var feet = camera.WorldToViewportPoint(view.transform.position + SuzuSprites.BodyOffset);
                File.AppendAllText(Path.Combine(directory, "shots.tsv"),
                    label + "\t" + width + "x" + height + "\t" + body.sprite.name + "\t" + body.flipX +
                    "\t" + camera.orthographicSize + "\t" + (feet.x * width) + "\t" + (feet.y * height) + "\t" + view.Worker.Carrying + "\n");
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                Object.Destroy(texture);
                Object.Destroy(target);
            }

            yield break;
        }
    }
}
