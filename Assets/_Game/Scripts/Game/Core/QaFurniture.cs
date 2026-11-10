#if NEKO_QA_FURNITURE
using System.Collections;
using System.Collections.Generic;
using Neko.Game.Ui;
using Neko.Game.View;
using Neko.Sim.Cafe;
using Neko.Sim.Maids;
using Neko.Sim.Shift;
using UnityEngine;

namespace Neko.Game.Core
{
    /// <summary>
    /// QA-only: starts a new company in a 13x9 café that already holds the pilot illustrated
    /// pieces (stage, koi_pond, window) plus tables and stations, so a browser build shows them
    /// without buying them. Compiled ONLY when the scripting define NEKO_QA_FURNITURE is set, which
    /// only <c>WebGLBuilder.BuildQaFurniture</c> does (into Build/WebGL_QA_FURNITURE); the itch.io
    /// build never contains it. It replaces the new company's room layout in memory and changes no
    /// rule, economy value, RNG or saved format. The layout is close to the one in
    /// <c>GameplayDepthTests</c> (the window sits clear of the teabar). Six extra maids are drawn
    /// standing still in front of, on and behind the pieces: they are views only, never in the shift.
    /// </summary>
    public static class QaFurniture
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            var host = new GameObject("QaFurniture");
            Object.DontDestroyOnLoad(host);
            host.AddComponent<Runner>().StartCoroutine(Run());
        }

        private sealed class Runner : MonoBehaviour { }

        private static IEnumerator Run()
        {
            while (GameRoot.Instance == null) yield return null;
            for (var i = 0; i < 12; i++) yield return null;

            var root = GameRoot.Instance;
            root.Ui.Get<MenuScreen>().OnNewGame.Invoke();
            yield return null;

            var grid = new CafeGrid(13, 9);
            void Put(string id, int x, int y, bool rotated = false) =>
                grid.Place(PlaceableCatalog.ById(id), x, y, rotated);

            Put("stove", 3, 7);
            Put("teabar", 5, 7);
            Put("table_2", 3, 3);
            Put("table_2", 3, 1, true);
            Put("table_4", 7, 3);
            Put("stage", 9, 6);
            Put("koi_pond", 10, 1);
            Put("window", 1, 2, true);
            Put("cat_tower", 11, 4, true);
            Put("plant", 1, 6);

            root.Session.Profile.Grid = grid;
            root.Ui.Get<MenuScreen>().OnPlay.Invoke();
            yield return null;
            root.Ui.Show<HubScreen>();
            root.Ui.Get<HubScreen>().OnOpenDay.Invoke();
            yield return null;
            yield return null;

            var spots = new[]
            {
                new Vector2(9.5f, 4.9f),   // in front of the stage
                new Vector2(10.0f, 6.4f),  // on the stage
                new Vector2(10.5f, 0.1f),  // in front of the pond
                new Vector2(10.6f, 1.7f),  // on the pond
                new Vector2(1.0f, 0.7f),   // in front of the window
                new Vector2(1.0f, 2.9f),   // behind the window
            };
            var ids = new[] { "suzu", "hina", "akari", "momo", "reina", "kaede" };
            var shownGrid = root.Presenter.Shift.Grid;
            var views = new List<MaidView>();
            for (var i = 0; i < spots.Length; i++)
            {
                var maid = new Maid { Id = 800 + i, Name = "QA", Archetype = Archetype.Genki, IdentityId = ids[i] };

                // Worker id 0 gives a ring offset of (+0.5, 0), and an authored body hangs 0.58 below
                // its root: so root = feet + (−0.5, +0.58).
                var worker = new Worker { Id = 0, Maid = maid, X = spots[i].x - 0.5f, Y = spots[i].y + 0.58f };
                var view = MaidView.Create(root.Presenter.transform, root.Sprites, worker);
                view.Grid = shownGrid;
                views.Add(view);
            }

            while (true)
            {
                foreach (var view in views)
                {
                    view.transform.localScale = Vector3.one;
                    view.Sync();
                }

                yield return null;
            }
        }
    }
}
#endif
