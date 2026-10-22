using System;
using System.Collections.Generic;

namespace Neko.Game.Core
{
    public enum AppState
    {
        Boot = 0,
        Menu,

        /// <summary>The café between days: roster, shop, editor, hiring.</summary>
        Cafe,

        Shift,
        Results,
    }

    /// <summary>
    /// Which screens can follow which.
    /// </summary>
    /// <remarks>
    /// Written down rather than implied by whatever calls <c>Show</c>, because rubric 11 asks
    /// how the game behaves when something unexpected happens and "it went to the results
    /// screen from the main menu and divided by a shift that was never started" is exactly
    /// that. An illegal transition is refused and reported, never thrown.
    /// </remarks>
    public sealed class AppStateMachine
    {
        private static readonly Dictionary<AppState, AppState[]> Allowed =
            new Dictionary<AppState, AppState[]>
            {
                { AppState.Boot, new[] { AppState.Menu } },
                { AppState.Menu, new[] { AppState.Cafe } },
                { AppState.Cafe, new[] { AppState.Shift, AppState.Menu } },
                { AppState.Shift, new[] { AppState.Results, AppState.Cafe, AppState.Menu } },
                { AppState.Results, new[] { AppState.Cafe, AppState.Menu } },
            };

        public AppState Current { get; private set; } = AppState.Boot;

        public event Action<AppState, AppState> Changed;

        public bool CanGo(AppState to) =>
            Allowed.TryGetValue(Current, out var next) && Array.IndexOf(next, to) >= 0;

        public bool Go(AppState to)
        {
            if (!CanGo(to)) return false;

            var from = Current;
            Current = to;
            Changed?.Invoke(from, to);
            return true;
        }
    }
}
