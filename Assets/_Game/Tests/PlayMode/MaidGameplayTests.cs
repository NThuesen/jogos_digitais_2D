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
    /// A real shift for each authored maid. The fixture renames and re-types maids in memory
    /// only; the roster, the draw and the save format are the game's own, and the saved profile
    /// is put back whatever the test does. Set NEKO_MAID_CAPTURE_DIR to also write screenshots.
    /// </summary>
    public sealed class MaidGameplayTests
    {
        private static readonly string[] Ids =
            { "suzu", "hina", "akari", "momo", "reina", "kaede", "nene", "ichika" };

        private bool _hadSave;
        private string _save;
        private GameRoot _root;

        private IEnumerator Begin(params string[] ids)
        {
            _hadSave = PlayerPrefs.HasKey(SaveBridge.Key);
            _save = PlayerPrefs.GetString(SaveBridge.Key, "");
            SaveBridge.Save(Profile.NewGame(21u));
            yield return SceneManager.LoadSceneAsync("Assets/_Game/Scenes/Boot.unity", LoadSceneMode.Single);
            yield return null;

            _root = GameRoot.Instance;
            _root.Ui.Get<MenuScreen>().OnNewGame.Invoke();

            var profile = _root.Session.Profile;
            Rename(profile, ids);
            profile.Lineup.AutoFill(profile.Maids);
            _root.Ui.Show<LineupScreen>();
            yield return null;
            _root.Ui.Show<HubScreen>();
            _root.Ui.Get<HubScreen>().OnOpenDay.Invoke();
            for (var n = 0; n < 14; n++) yield return null;
            Assert.AreEqual(AppState.Shift, _root.State.Current);
        }

        /// <summary>In memory only: the first maids take the designed names and archetypes.</summary>
        private static void Rename(Profile profile, string[] ids)
        {
            // A single identity goes to a floor maid, who walks the most.
            var floor = profile.Maids.FirstOrDefault(m => m.NaturalRole == Role.Floor);
            var pool = profile.Maids.OrderBy(m => m == floor ? 0 : 1).ToList();
            for (var i = 0; i < ids.Length && i < pool.Count; i++)
            {
                var entry = MaidAnimCatalog.ById(ids[i]);
                pool[i].Name = entry.name;
                pool[i].Archetype = entry.ArchetypeValue;
            }

            // The draw may already have produced a "Kaede" of her own; she would be a second one.
            for (var i = ids.Length; i < pool.Count; i++)
            {
                if (MaidAnimCatalog.All.Any(e => e.name == pool[i].Name)) pool[i].Name = "Extra" + pool[i].Id;
            }
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

        private static MaidView ViewOf(string id)
        {
            var name = MaidAnimCatalog.ById(id).name;
            return Object.FindObjectsByType<MaidView>().Single(view => view.Worker.Name == name);
        }

        private static SpriteRenderer Body(MaidView view) =>
            view.transform.Find("Body").GetComponent<SpriteRenderer>();

        private static bool IsSheetFrame(Sprite sprite, string id) =>
            sprite != null && sprite.name.StartsWith(MaidAnimCatalog.ById(id).resource + "#");

        private static int FrameOf(MaidView view)
        {
            var name = Body(view).sprite.name;
            return int.Parse(name.Substring(name.IndexOf('#') + 1));
        }

        [UnityTest]
        public IEnumerator EachAuthoredMaidWalksInARealShiftAndTheDayStillEnds([ValueSource(nameof(Ids))] string id)
        {
            yield return Begin(id);
            var entry = MaidAnimCatalog.ById(id);
            var view = ViewOf(id);
            Assert.IsTrue(IsSheetFrame(Body(view).sprite, id), id + " starts on her own sheet");
            foreach (var other in Object.FindObjectsByType<MaidView>().Where(v => v != view))
            {
                foreach (var otherId in Ids)
                    Assert.IsFalse(IsSheetFrame(Body(other).sprite, otherId), "the others never borrow " + id);
            }

            var frames = new HashSet<int>();
            var left = false;
            var right = false;
            Clocks.Scale = 4f;
            var elapsed = 0f;
            for (var i = 0; i < 12000 && elapsed < 65f; i++)
            {
                elapsed += Clocks.Game;
                var body = Body(view);
                Assert.IsTrue(IsSheetFrame(body.sprite, id));
                Assert.AreEqual(entry.BodyOffset, body.transform.localPosition, "no second bob");
                frames.Add(FrameOf(view));
                left |= body.flipX;
                right |= !body.flipX;
                yield return null;
            }

            Assert.Greater(frames.Count, 6, "a real shift walks through much of the cycle");
            Assert.IsTrue(left && right, "the worker travels both ways and the sprite follows");

            _root.Ui.Get<DayHud>().OnSkip.Invoke();
            yield return null;
            Assert.AreEqual(AppState.Results, _root.State.Current);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator PausingFreezesTheFrame([Values("hina", "ichika")] string id)
        {
            yield return Begin(id);
            var entry = MaidAnimCatalog.ById(id);
            var view = ViewOf(id);
            var midStride = false;
            for (var i = 0; i < 6000 && !midStride; i++)
            {
                midStride = System.Array.IndexOf(entry.rest_frames, FrameOf(view)) < 0;
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
        public IEnumerator EquipmentAndTirednessFallBackWithoutBreakingTheView([Values("hina", "nene")] string id)
        {
            yield return Begin(id);
            var view = ViewOf(id);
            var worker = view.Worker;
            Clocks.Pause(true);

            worker.OnBreak = true;
            view.Sync();
            Assert.IsTrue(IsSheetFrame(Body(view).sprite, id), "tired reuses the standing frame");
            Assert.AreNotEqual(Color.white, Body(view).color, "and is dimmed");
            worker.OnBreak = false;
            view.Sync();
            Assert.AreEqual(Color.white, Body(view).color);

            worker.Carrying = true;
            view.Sync();
            Assert.IsTrue(IsSheetFrame(Body(view).sprite, id));
            Assert.IsTrue(view.transform.Find("Carried").GetComponent<SpriteRenderer>().enabled,
                "a plate at her hands says she is carrying");
            worker.Carrying = false;

            worker.Maid.Gear[0] = new Equipment("fixture", "fixture", GearSlot.Outfit,
                Stats.Zero, Neko.Sim.Core.ThemeTag.None, 0, 1, "Apron", "FF0000");
            view.Bind(worker);
            Assert.IsFalse(IsSheetFrame(Body(view).sprite, id), "a geared maid keeps the layered look");
            worker.Maid.Gear[0] = null;
            view.Bind(worker);
            Assert.IsTrue(IsSheetFrame(Body(view).sprite, id));
            yield return null;
        }

        [UnityTest]
        public IEnumerator AMaidOutsideTheCatalogStaysProcedural()
        {
            yield return Begin("hina");
            var view = ViewOf("hina");
            view.Worker.Maid.Archetype = Archetype.Genki; // a Hina of another draw, not the designed one
            view.Bind(view.Worker);
            foreach (var id in Ids) Assert.IsFalse(IsSheetFrame(Body(view).sprite, id));
            yield return null;
        }

        // ---- captures: only when NEKO_MAID_CAPTURE_DIR is set --------------------------------

        private static string CaptureDir()
        {
            var directory = System.Environment.GetEnvironmentVariable("NEKO_MAID_CAPTURE_DIR");
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            return directory;
        }

        [UnityTest]
        public IEnumerator CaptureEachMaidWalking([ValueSource(nameof(Ids))] string id)
        {
            var directory = CaptureDir();
            if (string.IsNullOrEmpty(directory)) yield break;
            yield return Begin(id);

            var view = ViewOf(id);
            var shots = 0;
            var last = view.transform.position;
            var still = 0;
            for (var i = 0; i < 30000 && shots < 40; i++)
            {
                var moved = (view.transform.position - last).sqrMagnitude > 1e-6f;
                last = view.transform.position;
                still = moved ? 0 : still + 1;
                Clocks.Scale = still > 3 ? 4f : 0.3f;
                if (still < 40 && i > 20 && (moved || shots > 0))
                {
                    yield return Shot(directory, id + "_seq_" + shots.ToString("D3"), 960, 600, view);
                    if (shots % 10 == 0) yield return Shot(directory, id + "_seq_" + shots.ToString("D3"), 1280, 720, view);
                    shots++;
                }

                yield return null;
            }

            Assert.Greater(shots, 10, id + " never walked in the capture window");
        }

        [UnityTest]
        public IEnumerator CaptureAllTogetherInTheBiggestRoom()
        {
            var directory = CaptureDir();
            if (string.IsNullOrEmpty(directory)) yield break;
            yield return Begin("suzu");

            // Ordinary progression only: the same unlock path a player takes to the biggest room.
            _root.Ui.Get<DayHud>().OnSkip.Invoke();
            yield return null;
            _root.Ui.Get<BooksScreen>().OnContinue.Invoke();
            var profile = _root.Session.Profile;
            profile.Wallet.AddReputation(Neko.Sim.Economy.CafeLevelTable.ReputationForLevel(9));
            profile.ApplyUnlocks();

            // In memory only (the save is restored afterwards): enough extra maids to field all
            // eight identities at once, because the opening roster is three.
            var roles = new[] { Role.Reception, Role.Floor, Role.Kitchen, Role.Floor, Role.Kitchen, Role.Reception, Role.Floor, Role.Kitchen };
            for (var n = profile.Maids.Count; n < Ids.Length; n++)
            {
                profile.Maids.Add(new Maid { Id = 900 + n, Name = "Fixture" + n, Archetype = Archetype.Genki,
                    NaturalRole = roles[n], Base = profile.Maids[0].Base, Rarity = profile.Maids[0].Rarity });
            }

            Rename(profile, Ids);
            profile.Lineup.AutoFill(profile.Maids);
            _root.Ui.Get<HubScreen>().OnOpenDay.Invoke();
            for (var n = 0; n < 14; n++) yield return null;
            Assert.AreEqual(AppState.Shift, _root.State.Current);

            var views = Object.FindObjectsByType<MaidView>();
            File.AppendAllText(Path.Combine(directory, "together.txt"),
                "views: " + string.Join(",", views.Select(v => v.Worker.Name)) + " of roster " + profile.Maids.Count + "\n");
            var any = views.First();
            Clocks.Scale = 0.5f;
            for (var shot = 0; shot < 12; shot++)
            {
                for (var n = 0; n < 40; n++) yield return null;
                yield return Shot(directory, "together_" + shot.ToString("D2"), 1280, 720, any);
                yield return Shot(directory, "together_" + shot.ToString("D2"), 960, 600, any);
            }

            LogAssert.NoUnexpectedReceived();
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
                File.AppendAllText(Path.Combine(directory, "shots.tsv"),
                    label + "\t" + width + "x" + height + "\t" + body.sprite.name + "\t" + body.flipX +
                    "\t" + camera.orthographicSize + "\t" + view.Worker.Carrying + "\n");
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
