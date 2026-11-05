using System.Collections.Generic;

namespace Neko.Sim.Maids
{
    /// <summary>Whether an identity can be drawn, hired and shown.</summary>
    public enum IdentityStatus
    {
        /// <summary>Reserved in the catalog; has no portrait or walk cycle yet, so it is never drawn.</summary>
        Planned = 0,

        /// <summary>Has every asset it needs (portrait, walk sheet) and enters the draws.</summary>
        Released,
    }

    /// <summary>
    /// One of the closed set of maids: who she is, independent of the card she comes on.
    /// </summary>
    /// <remarks>
    /// The <see cref="Id"/> is a lower-case slug that never changes: saves, portraits and walk
    /// cycles are keyed by it, never by the name or the archetype. Rarity, stats, traits, house
    /// and origin belong to the <see cref="Maid"/> (the card), not to the identity.
    /// </remarks>
    public sealed class MaidIdentity
    {
        public readonly string Id;
        public readonly string Name;
        public readonly Archetype Archetype;
        public readonly IdentityStatus Status;

        public MaidIdentity(string id, string name, Archetype archetype, IdentityStatus status)
        {
            Id = id;
            Name = name;
            Archetype = archetype;
            Status = status;
        }

        public bool Released => Status == IdentityStatus.Released;
    }

    /// <summary>
    /// The closed catalog of 27 maids, three for each of the nine archetypes.
    /// </summary>
    /// <remarks>
    /// Releasing a planned maid is a one-word change here, and only once her portrait and walk
    /// cycle exist (a test in the game assembly fails for a released identity without them).
    /// Nothing outside this class invents a name or pairs one with an archetype.
    /// </remarks>
    public static class MaidCatalog
    {
        private static MaidIdentity Out(string id, string name, Archetype a) =>
            new MaidIdentity(id, name, a, IdentityStatus.Released);

        private static MaidIdentity Planned(string id, string name, Archetype a) =>
            new MaidIdentity(id, name, a, IdentityStatus.Planned);

        private static readonly MaidIdentity[] Entries =
        {
            Out("suzu", "Suzu", Archetype.Genki),
            Planned("mikan", "Mikan", Archetype.Genki),
            Planned("hibari", "Hibari", Archetype.Genki),

            Out("hina", "Hina", Archetype.Kuudere),
            Planned("yuki", "Yuki", Archetype.Kuudere),
            Planned("seira", "Seira", Archetype.Kuudere),

            Out("akari", "Akari", Archetype.Tsundere),
            Planned("kurumi", "Kurumi", Archetype.Tsundere),
            Planned("ruri", "Ruri", Archetype.Tsundere),

            Out("momo", "Momo", Archetype.Dojikko),
            Planned("nonoka", "Nonoka", Archetype.Dojikko),
            Planned("chiyo", "Chiyo", Archetype.Dojikko),

            Out("reina", "Reina", Archetype.Ojousama),
            Planned("kaguya", "Kaguya", Archetype.Ojousama),
            Planned("shiori", "Shiori", Archetype.Ojousama),

            Out("kaede", "Kaede", Archetype.Oneesan),
            Planned("mizuki", "Mizuki", Archetype.Oneesan),
            Planned("tsubaki", "Tsubaki", Archetype.Oneesan),

            Out("nene", "Nene", Archetype.Idol),
            Planned("himari", "Himari", Archetype.Idol),
            Planned("mirai", "Mirai", Archetype.Idol),

            Out("ichika", "Ichika", Archetype.Gourmand),
            Planned("anzu", "Anzu", Archetype.Gourmand),
            Planned("kinako", "Kinako", Archetype.Gourmand),

            Out("yoru", "Yoru", Archetype.Mukuchi),
            Planned("kasumi", "Kasumi", Archetype.Mukuchi),
            Planned("mayu", "Mayu", Archetype.Mukuchi),
        };

        private static readonly MaidIdentity[] ReleasedEntries = Filter();

        /// <summary>Every identity, released or not, in catalog order.</summary>
        public static IReadOnlyList<MaidIdentity> All => Entries;

        /// <summary>The identities that can be drawn, in catalog order.</summary>
        public static IReadOnlyList<MaidIdentity> Released => ReleasedEntries;

        public static MaidIdentity ById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            for (var i = 0; i < Entries.Length; i++)
            {
                if (Entries[i].Id == id) return Entries[i];
            }

            return null;
        }

        /// <summary>
        /// The released identity with this name and archetype, or null. Used only to give a
        /// save written before identities existed the identity its maids already had.
        /// </summary>
        public static MaidIdentity FindReleased(string name, Archetype archetype)
        {
            for (var i = 0; i < ReleasedEntries.Length; i++)
            {
                var entry = ReleasedEntries[i];
                if (entry.Name == name && entry.Archetype == archetype) return entry;
            }

            return null;
        }

        /// <summary>Released identities that none of <paramref name="roster"/> already is.</summary>
        public static List<MaidIdentity> FreeReleased(IEnumerable<Maid> roster)
        {
            var taken = new HashSet<string>();
            if (roster != null)
            {
                foreach (var maid in roster)
                {
                    if (maid != null && !string.IsNullOrEmpty(maid.IdentityId)) taken.Add(maid.IdentityId);
                }
            }

            var free = new List<MaidIdentity>();
            for (var i = 0; i < ReleasedEntries.Length; i++)
            {
                if (!taken.Contains(ReleasedEntries[i].Id)) free.Add(ReleasedEntries[i]);
            }

            return free;
        }

        private static MaidIdentity[] Filter()
        {
            var list = new List<MaidIdentity>();
            for (var i = 0; i < Entries.Length; i++)
            {
                if (Entries[i].Released) list.Add(Entries[i]);
            }

            return list.ToArray();
        }
    }
}
