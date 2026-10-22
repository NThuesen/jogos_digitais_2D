using System;
using UnityEngine;

namespace Neko.Game.Core
{
    /// <summary>
    /// Catches what would otherwise end the session.
    /// </summary>
    /// <remarks>
    /// Wrapped around the shift tick and every button handler. An exception inside a frame of
    /// Unity does not stop the game; it stops <em>that</em> call, every frame, forever, and
    /// the player is left with a café that no longer responds and no idea why. Catching it
    /// here means the day ends as a slow day, the player is told, and the café is still there.
    ///
    /// Deliberately catches everything. There is no exception a café management game should
    /// let reach the player as a frozen screen.
    /// </remarks>
    public static class ErrorBoundary
    {
        public static event Action<string, Exception> Failed;

        public static int FailureCount { get; private set; }

        public static string LastFailure { get; private set; }

        public static bool Guard(string what, Action body)
        {
            try
            {
                body?.Invoke();
                return true;
            }
            catch (Exception error)
            {
                Report(what, error);
                return false;
            }
        }

        public static T Guard<T>(string what, Func<T> body, T fallback = default)
        {
            try
            {
                return body == null ? fallback : body();
            }
            catch (Exception error)
            {
                Report(what, error);
                return fallback;
            }
        }

        public static void Reset()
        {
            FailureCount = 0;
            LastFailure = null;
        }

        private static void Report(string what, Exception error)
        {
            FailureCount++;
            LastFailure = what + ": " + error.Message;

            Debug.LogError("[" + what + "] " + error);
            Failed?.Invoke(what, error);
        }
    }
}
