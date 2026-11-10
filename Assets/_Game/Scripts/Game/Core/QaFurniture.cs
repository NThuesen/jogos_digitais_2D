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
    /// QA-only: starts a new company in a 13x9 café that already holds the eight functional pieces
    /// (ART-INT-02) plus the pilot stage and window, so a browser build shows them
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

            Put("stove", 2, 7);
            Put("teabar", 4, 7);
            Put("dessert", 7, 7);
            Put("bakery", 11, 7);
            Put("table_2", 3, 3);
            Put("table_2", 3, 1, true);
            Put("table_4", 7, 3);
            Put("stool", 11, 3);
            Put("stool", 11, 4);
            Put("stage", 9, 0);
            Put("window", 1, 2, true);
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
                new Vector2(3.5f, 2.55f),  // in front of the upright two-seater
                new Vector2(3.5f, 4.45f),  // behind it
                new Vector2(7.5f, 2.45f),  // in front of the four-seater
                new Vector2(7.5f, 5.45f),  // behind it
                new Vector2(9.5f, 0.1f),   // in front of the stage
                new Vector2(1.0f, 0.7f),   // in front of the window
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

            // ART-INT-02: one view-only customer on every seat (never in the shift), drawn exactly as
            // ShiftPresenter draws a seated one, so every table and stool shows its sitters.
            var seatViews = new List<CustomerView>();
            var kinds = new[] { Neko.Sim.Service.CustomerType.Regular, Neko.Sim.Service.CustomerType.Student,
                Neko.Sim.Service.CustomerType.Salaryman, Neko.Sim.Service.CustomerType.Otaku };
            var n = 0;
            foreach (var cell in shownGrid.SeatPositions())
            {
                var item = shownGrid.ItemAt(cell.X, cell.Y);
                var entry = Art.IllustratedFurniture.For(item.Def, item.Width, item.Height);
                var offset = entry != null ? entry.SeatOffset(cell.X - item.X, cell.Y - item.Y) : Vector2.zero;
                var customer = new Neko.Sim.Service.Customer
                {
                    Id = 900 + n, Kind = kinds[n % kinds.Length], State = Neko.Sim.Service.CustomerState.Eating,
                };
                n++;
                var seated = CustomerView.Create(root.Presenter.transform, root.Sprites, customer);
                seated.Grid = shownGrid;
                seated.SeatCell = cell;
                seated.Place(new Vector3(cell.X + offset.x, cell.Y + 0.42f + offset.y, 0f));
                seatViews.Add(seated);
            }

            while (true)
            {
                foreach (var seated in seatViews)
                {
                    seated.transform.localScale = Vector3.one * CustomerView.CustomerScale;
                    seated.Sync();
                }

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
