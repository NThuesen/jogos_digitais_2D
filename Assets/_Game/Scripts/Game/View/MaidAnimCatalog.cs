using System;
using Neko.Game.Art;
using Neko.Sim.Maids;
using UnityEngine;

namespace Neko.Game.View
{
    /// <summary>
    /// Which maids have an authored walk sheet and everything the view needs to draw it.
    /// </summary>
    /// <remarks>
    /// The data is <c>Resources/Maids/maid_walk_catalog.json</c>, written by
    /// <c>tools/art/maid_walk/maid_walk.py</c> from measurements of each sequence, so the
    /// numbers cannot drift from the sheets. Who gets which art is decided by the maid's
    /// <see cref="Maid.IdentityId"/> alone (the entry <c>id</c> is the identity slug), never by name
    /// or archetype. Only a legacy maid (an old save's maid with no identity) keeps the procedural
    /// figure; gear no longer changes the figure of an authored maid.
    /// </remarks>
    public static class MaidAnimCatalog
    {
        public const string CatalogResource = "Maids/maid_walk_catalog";

        [Serializable]
        public sealed class Entry
        {
            public string id, name, archetype, resource, source;
            public int frames, columns, rows, idle_frame;
            public int[] cell, rest_frames;
            public float pixels_per_unit, units_per_cycle, body_offset_y, carried_offset_y;

            public int CellWidth => cell[0];
            public int CellHeight => cell[1];
            public Vector3 BodyOffset => new Vector3(0f, body_offset_y, 0f);
            public Vector3 CarriedOffset => new Vector3(0f, carried_offset_y, 0f);
            public Archetype ArchetypeValue => (Archetype)Enum.Parse(typeof(Archetype), archetype);
        }

        [Serializable]
        private sealed class Root
        {
            public Entry[] maids;
        }

        private static Entry[] _entries;

        public static Entry[] All
        {
            get
            {
                if (_entries != null) return _entries;
                var asset = Resources.Load<TextAsset>(CatalogResource);
                _entries = asset == null ? new Entry[0] : JsonUtility.FromJson<Root>(asset.text).maids ?? new Entry[0];
                return _entries;
            }
        }

        public static Entry ById(string id)
        {
            foreach (var entry in All)
            {
                if (entry.id == id) return entry;
            }

            return null;
        }

        /// <summary>
        /// The walk entry for this maid, or null. Equipment is shown in the interface instead of
        /// as layers, so it does not send an authored maid back to the procedural figure.
        /// </summary>
        public static Entry Resolve(Maid maid) => Identify(maid);

        /// <summary>Which authored character she is: the entry whose id is her identity, else null.</summary>
        public static Entry Identify(Maid maid) => maid == null ? null : ById(maid.IdentityId);

        /// <summary>The frames, or null when the sheet is missing or does not match the catalog.</summary>
        public static Sprite[] Load(SpriteFactory sprites, Entry entry) =>
            sprites.Sheet(entry.resource, entry.columns, entry.rows, entry.CellWidth, entry.CellHeight,
                entry.frames, entry.pixels_per_unit);
    }
}
