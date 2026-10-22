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

        public static void Save(Profile profile)
        {
            if (profile == null) return;

            PlayerPrefs.SetString(Key, SaveCodec.Encode(profile));
            PlayerPrefs.Save();
        }

        public static LoadResult Load() =>
            SaveCodec.Decode(PlayerPrefs.GetString(Key, ""));

        public static bool HasSave() => !string.IsNullOrEmpty(PlayerPrefs.GetString(Key, ""));

        public static void Wipe()
        {
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
        }
    }
}
