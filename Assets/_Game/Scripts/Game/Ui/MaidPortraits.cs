using System.Collections.Generic;
using Neko.Game.View;
using Neko.Sim.Maids;
using UnityEngine;

namespace Neko.Game.Ui
{
    /// <summary>
    /// The premium illustrated portraits (UI-021) of the nine authored maids.
    /// </summary>
    /// <remarks>
    /// A portrait is keyed by the maid's <see cref="Maid.IdentityId"/> (<see cref="MaidAnimCatalog.Identify"/>),
    /// so nobody ever wears somebody else's face. Everything returns null when there is no portrait
    /// (a legacy maid), and the callers keep their existing fallback. Pure skin: nothing here reads
    /// or writes the profile.
    /// </remarks>
    public static class MaidPortraits
    {
        private const string Folder = "MaidPortraits/";

        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        /// <summary>Head and shoulders, 192x157, for Maid Cards; null without a portrait.</summary>
        public static Sprite Head(Maid maid) => Load(maid, "head");

        /// <summary>Waist-up, 480x641 (3:4), for the Elenco detail panel; null without a portrait.</summary>
        public static Sprite Detail(Maid maid) => Load(maid, "detail");

        private static Sprite Load(Maid maid, string kind)
        {
            var entry = MaidAnimCatalog.Identify(maid);
            if (entry == null) return null;

            var key = entry.id + "_" + kind;
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;

            var sprite = Resources.Load<Sprite>(Folder + key);
            if (sprite != null) Cache[key] = sprite;
            return sprite;
        }
    }
}
