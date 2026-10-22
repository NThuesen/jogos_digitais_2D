using System.Collections;
using System.Linq;
using Neko.Game.Audio;
using Neko.Game.Core;
using Neko.Game.Ui;
using Neko.Sim.Maids;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Neko.Tests.PlayMode
{
    /// <summary>
    /// Proof that the thing actually runs.
    /// </summary>
    /// <remarks>
    /// Everything else in this project is tested headless and in isolation, which is fast and
    /// catches most things — but a game that passes four hundred unit tests and shows a black
    /// screen has passed nothing. These load the real scene and look at what comes up.
    /// </remarks>
    public sealed class BootTests
    {
        private const string ScenePath = "Assets/_Game/Scenes/Boot.unity";

        /// <summary>Every screen in the game, as something that can be shown.</summary>
        private static System.Action[] Screens(GameRoot root) => new System.Action[]
        {
            () => root.Ui.Show<LineupScreen>(),
            () => root.Ui.Show<RosterScreen>(),
            () => root.Ui.Show<ShopScreen>(),
            () => root.Ui.Show<BoxScreen>(),
            () => root.Ui.Show<HallScreen>(),
            () => root.Ui.Show<HubScreen>(),
            () => root.Ui.Show<BooksScreen>(),
            () => root.Ui.Show<BustScreen>(),
        };

        [UnitySetUp]
        public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync(ScenePath, LoadSceneMode.Single);
            yield return null;
            yield return null;
        }

        [UnityTest]
        public IEnumerator BootBuildsAPlayableSceneWithoutErrors()
        {
            LogAssert.NoUnexpectedReceived();

            Assert.IsNotNull(GameRoot.Instance, "GameRoot never woke up");
            Assert.IsNotNull(Camera.main, "there is no camera");
            Assert.IsNotNull(GameRoot.Instance.Ui, "there is no interface");
            Assert.AreEqual(AppState.Menu, GameRoot.Instance.State.Current);

            yield return null;
        }

        [UnityTest]
        public IEnumerator TheFontResolvesSoTheGameHasText()
        {
            // The one thing that would silently hollow out every screen: TextMeshPro has no
            // font asset in this project, which is why the interface uses legacy Text.
            Assert.IsNotNull(Typeface.Font, "no font: every label would render as nothing");

            var labels = Object.FindObjectsByType<Text>(FindObjectsSortMode.None);
            Assert.IsNotEmpty(labels, "the menu drew no text at all");
            Assert.IsTrue(labels.All(l => l.font != null));

            yield return null;
        }

        [UnityTest]
        public IEnumerator EverySoundAndEveryBedIsOnDisk()
        {
            var audio = GameRoot.Instance.Audio;
            foreach (SoundId id in System.Enum.GetValues(typeof(SoundId)))
            {
                Assert.IsTrue(audio.Has(id), "som faltando: " + AudioDirector.FileNameOf(id));
            }

            var music = GameRoot.Instance.Music;
            foreach (MusicBed bed in System.Enum.GetValues(typeof(MusicBed)))
            {
                Assert.IsTrue(music.Has(bed), "trilha faltando: " + bed);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator EveryArchetypeHasItsOwnVoiceFile()
        {
            var audio = GameRoot.Instance.Audio;
            var voices = ArchetypeTable.All.Select(p => AudioDirector.NyaFor(p.Kind)).ToList();

            CollectionAssert.AllItemsAreUnique(voices);
            Assert.IsTrue(voices.All(audio.Has));

            yield return null;
        }

        [UnityTest]
        public IEnumerator TheMenuLeadsIntoTheCafeAndIntoADay()
        {
            var root = GameRoot.Instance;

            root.Ui.Get<MenuScreen>().OnPlay.Invoke();
            yield return null;
            Assert.AreEqual(AppState.Cafe, root.State.Current);
            Assert.IsTrue(root.Ui.Get<HubScreen>().Visible);

            root.Ui.Get<HubScreen>().OnOpenDay.Invoke();
            yield return null;
            Assert.AreEqual(AppState.Shift, root.State.Current);
            Assert.IsNotNull(root.Presenter.Shift);
            Assert.IsTrue(root.Ui.Get<DayHud>().Visible);

            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator ARoomIsDrawnWithOneObjectPerTile()
        {
            var root = GameRoot.Instance;
            root.Ui.Get<MenuScreen>().OnPlay.Invoke();
            yield return null;

            var grid = root.Session.Profile.Grid;
            var tiles = root.Room.GetComponentsInChildren<SpriteRenderer>()
                .Count(r => r.gameObject.name == "Tile");

            Assert.AreEqual(grid.Width * grid.Height, tiles);
        }

        [UnityTest]
        public IEnumerator SortingPutsLowerCharactersInFront()
        {
            Assert.Greater(
                Layers.Order(Layers.Characters, 1f),
                Layers.Order(Layers.Characters, 4f),
                "somebody standing at the front of the room has to draw over somebody at the back");

            Assert.Greater(
                Layers.Order(Layers.Characters, 9f),
                Layers.Order(Layers.Furniture, 0f),
                "a character always draws over furniture");

            yield return null;
        }

        [UnityTest]
        public IEnumerator EveryScreenBuildsAndShowsWithoutErrors()
        {
            var root = GameRoot.Instance;
            root.Ui.Get<MenuScreen>().OnPlay.Invoke();
            yield return null;

            foreach (var show in Screens(root))
            {
                show();
                yield return null;
            }

            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator ThereIsNoWayToTouchTheCafeWhileADayIsRunning()
        {
            // The pivot, as a test. If a pointer ever reaches the simulation again, the
            // lineup stops being the whole of the player's turn.
            var root = GameRoot.Instance;
            root.Ui.Get<MenuScreen>().OnPlay.Invoke();
            yield return null;

            root.Ui.Get<HubScreen>().OnOpenDay.Invoke();
            yield return null;

            var before = root.Presenter.Shift.Earned;
            for (var i = 0; i < 20; i++)
            {
                root.Input.SendClickForTest(new Vector2(640f, 360f));
                yield return null;
            }

            Assert.AreEqual(AppState.Shift, root.State.Current);
            Assert.GreaterOrEqual(root.Presenter.Shift.Earned, before);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator ANewBusinessOpensWithAPlayableLineup()
        {
            var root = GameRoot.Instance;
            root.Ui.Get<MenuScreen>().OnNewGame.Invoke();
            yield return null;

            Assert.IsNull(root.Session.WhyNotReady(),
                "a brand new business has to be able to open its doors");
            Assert.AreEqual(3, root.Session.Profile.Maids.Count);
        }

        [UnityTest]
        public IEnumerator EveryStringKeyTheInterfaceAsksForExists()
        {
            // Walks every label that came up and checks none of them rendered as [key].
            var root = GameRoot.Instance;
            root.Ui.Get<MenuScreen>().OnPlay.Invoke();
            yield return null;

            foreach (var screen in Screens(root))
            {
                screen();
                yield return null;

                foreach (var label in Object.FindObjectsByType<Text>(FindObjectsSortMode.None))
                {
                    Assert.IsFalse(label.text.StartsWith("["), "missing string: " + label.text);
                }
            }
        }
    }
}
