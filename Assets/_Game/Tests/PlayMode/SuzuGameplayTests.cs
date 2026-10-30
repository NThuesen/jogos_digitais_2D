using System.Collections;
using System.IO;
using System.Linq;
using Neko.Game.Core;
using Neko.Game.Ui;
using Neko.Game.View;
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
    /// <summary>Real Boot/new-game/lineup/day smoke, restoring the original profile even on failure.</summary>
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
            // Deterministic fixture only. No starter RNG or persistent identity change.
            var profile = _root.Session.Profile;
            var fixture = Profile.NewGame(21u);
            profile.Maids.Clear();
            profile.Maids.AddRange(fixture.Maids);
            profile.Seed = fixture.Seed;
            foreach (var maid in profile.Maids) maid.Name = maid.NaturalRole == Role.Floor ? "Suzu" : "Fixture " + maid.Id;
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
        private static SpriteRenderer Body(MaidView view) => view.transform.Find("Body").GetComponent<SpriteRenderer>();

        [UnityTest]
        public IEnumerator AuthoredMaidRunsAlongsideProceduralMaidsAndFinishesDay()
        {
            var view = Suzu;
            Assert.IsTrue(Body(view).sprite.name.StartsWith("suzu_"));
            foreach (var other in Object.FindObjectsByType<MaidView>().Where(v => v != view))
                Assert.IsFalse(Body(other).sprite.name.StartsWith("suzu_"));
            var seen = new System.Collections.Generic.HashSet<string>();
            var carried = false;
            var right = false;
            var left = false;
            Clocks.Scale = 4f;
            var elapsed = 0f;
            for (var frame = 0; frame < 12000 && elapsed < 65f; frame++)
            {
                elapsed += Clocks.Game;
                var body = Body(view);
                if (body.sprite.name.StartsWith("suzu_walk")) seen.Add(body.sprite.name);
                if (body.sprite.name.StartsWith("walk_tray")) carried = true;
                if (body.sprite.name.StartsWith("suzu_walk") || body.sprite.name.StartsWith("walk_tray"))
                {
                    left |= body.flipX;
                    right |= !body.flipX;
                    Assert.AreEqual(SuzuSprites.BodyOffset, body.transform.localPosition, "no double bob");
                    Assert.IsFalse(view.transform.Find("Carried").GetComponent<SpriteRenderer>().enabled);
                }
                yield return null;
            }
            Assert.Greater(seen.Count, 1, "real shift movement must animate");
            Assert.IsTrue(carried, "the fixture must reach real delivery carrying");
            Assert.IsTrue(left && right, "the real worker must travel in both directions");
            _root.Ui.Get<DayHud>().OnSkip.Invoke();
            yield return null;
            Assert.AreEqual(AppState.Results, _root.State.Current);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator RuntimeFallbacksPreserveMoodEquipmentAndCustomItems()
        {
            var view = Suzu;
            var worker = view.Worker;
            Clocks.Pause(true);
            worker.Drop();
            worker.OnBreak = true;
            view.Sync();
            Assert.IsFalse(Body(view).sprite.name.StartsWith("suzu_"));
            Assert.IsTrue(view.transform.Find("Mood").GetComponent<SpriteRenderer>().enabled);
            yield return CaptureIfRequested("fallback_tired");
            worker.OnBreak = false;
            worker.Maid.Mood = 1f;
            worker.Claimed = new ShiftTask(999, TaskKind.Cook, 1);
            view.Sync();
            StringAssert.Contains("Cook", Body(view).sprite.name);
            yield return CaptureIfRequested("fallback_cook");
            worker.Drop();
            worker.Maid.Gear[0] = new Equipment("fixture", "fixture", GearSlot.Outfit,
                Neko.Sim.Maids.Stats.Zero, Neko.Sim.Core.ThemeTag.None, 0, 1, "Apron", "FF0000");
            view.Sync();
            StringAssert.Contains("fixture", Body(view).sprite.name);
            yield return CaptureIfRequested("fallback_gear");
            worker.Maid.Gear[0] = null;
            worker.Carrying = true;
            view.Sync();
            Assert.AreEqual("tray_idle_0", Body(view).sprite.name);
            view.ShowCarried(_root.Sprites.Solid(Color.yellow));
            view.Sync();
            StringAssert.Contains("Carry", Body(view).sprite.name);
            Assert.IsTrue(view.transform.Find("Carried").GetComponent<SpriteRenderer>().enabled);
            view.ShowCarried(null);
            view.Sync();
            Assert.AreEqual("tray_idle_0", Body(view).sprite.name);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CaptureRealShiftAtBothResolutions()
        {
            var directory = System.Environment.GetEnvironmentVariable("NEKO_SUZU_CAPTURE_DIR");
            if (string.IsNullOrEmpty(directory)) yield break;
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "fixture-profile.save"), SaveCodec.Encode(_root.Session.Profile));
            var view = Suzu;
            var seen = new System.Collections.Generic.HashSet<string>();
            var elapsed = 0f;
            for (var frame = 0; frame < 18000 && elapsed < 25f; frame++)
            {
                elapsed += Clocks.Game;
                var body = Body(view);
                var label = body.sprite.name;
                if (label.StartsWith("suzu_") || label.StartsWith("walk_tray") || label.StartsWith("tray_idle"))
                {
                    var direction = body.flipX ? "left" : "right";
                    if (seen.Add(label)) yield return CaptureIfRequested(label + "_" + direction);
                }
                yield return null;
            }
            Assert.IsTrue(seen.Any(name => name.StartsWith("suzu_walk_sheet")),
                "the starting room must show real walking");

            // Real later-room dimensions/unlocks through the existing hub -> Day path.
            _root.Ui.Get<DayHud>().OnSkip.Invoke();
            yield return null;
            _root.Ui.Get<BooksScreen>().OnContinue.Invoke();
            var profile = _root.Session.Profile;
            profile.Wallet.Earn(5000);
            profile.Wallet.AddReputation(Neko.Sim.Economy.CafeLevelTable.ReputationForLevel(9));
            profile.ApplyUnlocks();
            // Longer delivery route exposes the whole carry cycle using ordinary sim movement.
            Assert.IsTrue(profile.Grid.Remove(5, 2));
            Assert.IsTrue(profile.Grid.Place(Neko.Sim.Cafe.PlaceableCatalog.ById("stove"), 11, 6));
            profile.Lineup.AutoFill(profile.Maids);
            foreach (var maid in profile.Maids) maid.Mood = 1f;
            _root.Ui.Get<HubScreen>().OnOpenDay.Invoke();
            for (var i = 0; i < 14; i++) yield return null;
            Assert.AreEqual(AppState.Shift, _root.State.Current);
            Assert.AreEqual(13, _root.Presenter.Shift.Grid.Width);
            Assert.AreEqual(9, _root.Presenter.Shift.Grid.Height);
            yield return CaptureIfRequested("later_room_13x9");
            var laterSeen = new System.Collections.Generic.HashSet<string>();
            elapsed = 0f;
            view = Suzu;
            for (var frame = 0; frame < 18000 && elapsed < 40f; frame++)
            {
                elapsed += Clocks.Game;
                var body = Body(view);
                var label = body.sprite.name;
                if ((label.StartsWith("walk_tray") || label.StartsWith("suzu_walk_sheet")) && laterSeen.Add(label))
                    yield return CaptureIfRequested("later_" + label + (body.flipX ? "_left" : "_right"));
                yield return null;
            }
            Assert.AreEqual(8, laterSeen.Count(name => name.StartsWith("suzu_walk_sheet")));
            Assert.AreEqual(8, laterSeen.Count(name => name.StartsWith("walk_tray")),
                "the longer delivery route must expose the complete carry cycle");
            LogAssert.NoUnexpectedReceived();
        }

        private static IEnumerator CaptureIfRequested(string label)
        {
            var directory = System.Environment.GetEnvironmentVariable("NEKO_SUZU_CAPTURE_DIR");
            if (string.IsNullOrEmpty(directory)) yield break;
            foreach (var size in new[] { new Vector2Int(960, 600), new Vector2Int(1280, 720) })
                yield return Shot(directory, label, size.x, size.y);
        }

        private static IEnumerator Shot(string directory, string label, int width, int height)
        {
            var camera = Camera.main;
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                // Update the existing scaler for the target dimensions without advancing the
                // shift between paired captures of the same real animation frame.
                Canvas.ForceUpdateCanvases();
                camera.Render();
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();
                File.WriteAllBytes(Path.Combine(directory, label + "_" + width + "x" + height + ".png"), texture.EncodeToPNG());
                var suzu = Object.FindObjectsByType<MaidView>().Single(v => v.Worker.Name == "Suzu");
                var body = Body(suzu);
                File.AppendAllText(Path.Combine(directory, "frames.tsv"),
                    label + "\t" + width + "\t" + height + "\t" + body.sprite.name + "\t" +
                    body.flipX + "\t" + camera.orthographicSize + "\t" +
                    body.bounds.size.y * height / (2f * camera.orthographicSize) + "\n");
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
