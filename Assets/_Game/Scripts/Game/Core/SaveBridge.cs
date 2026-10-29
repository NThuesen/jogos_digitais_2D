using Neko.Sim.Save;
using UnityEngine;

namespace Neko.Game.Core
{
    /// <summary>
    /// Where the save actually lives.
    /// </summary>
    /// <remarks>
    /// PlayerPrefs, which on a WebGL build is IndexedDB behind the browser's own storage —
    /// the only durable place a page is allowed. Everything about the format, the version and
    /// the checksum is in <see cref="SaveCodec"/>, which has no UnityEngine reference and is
    /// therefore the part that can be tested properly.
    /// </remarks>
    public static class SaveBridge
    {
        public const string Key = "nekomaidinc.profile";

#if UNITY_EDITOR
        // Batch tests and explicitly isolated captures never touch PlayerPrefs.
        // In-memory state is disposable even if Unity crashes or is interrupted.
        private static string _testSave = "";
        public static bool Isolated => Application.isBatchMode ||
            System.Environment.GetEnvironmentVariable("NEKO_ISOLATED_SAVE") == "1";
#else
        public static bool Isolated => false;
#endif

        private static string Read()
        {
#if UNITY_EDITOR
            if (Isolated) return _testSave;
#endif
            return PlayerPrefs.GetString(Key, "");
        }

        public static void Save(Profile profile)
        {
            if (profile == null) return;

            #if UNITY_EDITOR
            if (Isolated)
            {
                _testSave = SaveCodec.Encode(profile);
                return;
            }
#endif
            PlayerPrefs.SetString(Key, SaveCodec.Encode(profile));
            PlayerPrefs.Save();
        }

        public static LoadResult Load() =>
            SaveCodec.Decode(Read());

        public static bool HasSave() => !string.IsNullOrEmpty(Read());

        public static void Wipe()
        {
            #if UNITY_EDITOR
            if (Isolated)
            {
                _testSave = "";
                return;
            }
#endif
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
        }
    }
}
