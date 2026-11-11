using System;
using System.Collections.Generic;
using Neko.Sim.Maids;
using Neko.Sim.Team;

namespace Neko.Game.Ui
{
    /// <summary>How the Maids screen orders the collection it shows.</summary>
    public enum RosterSort
    {
        Rarity,
        Name,
        Level,
    }

    /// <summary>
    /// What the Maids screen shows of the collection: a rarity filter and a natural-position
    /// filter that combine by intersection, one of three orders, and pages of eight.
    /// </summary>
    /// <remarks>
    /// Presentation only. Nothing here changes the collection, the profile or a save: every call
    /// returns a new list, and a filter looks at <see cref="Maid.NaturalRole"/>, never at where a
    /// maid happens to stand in today's lineup.
    /// </remarks>
    public static class RosterQuery
    {
        /// <summary>Four columns by two rows.</summary>
        public const int PageSize = 8;

        /// <summary>The five real positions, in the order the buttons show them.</summary>
        public static readonly Role[] Roles =
        {
            Role.Reception, Role.Floor, Role.Kitchen, Role.Stage, Role.Management,
        };

        public static readonly Rarity[] Rarities =
        {
            Rarity.Common, Rarity.Rare, Rarity.Epic, Rarity.Legendary,
        };

        /// <summary>Null means "all" for either filter.</summary>
        public static bool Matches(Maid maid, Rarity? rarity, Role? role) =>
            maid != null &&
            (!rarity.HasValue || maid.Rarity == rarity.Value) &&
            (!role.HasValue || maid.NaturalRole == role.Value);

        /// <summary>The maids that pass both filters, in the given order. Ties fall to name, then id.</summary>
        public static List<Maid> Visible(IEnumerable<Maid> maids, Rarity? rarity, Role? role, RosterSort sort)
        {
            var list = new List<Maid>();
            foreach (var maid in maids)
            {
                if (Matches(maid, rarity, role)) list.Add(maid);
            }

            list.Sort((a, b) => Compare(a, b, sort));
            return list;
        }

        /// <summary>How many maids the player owns of a rarity (null: all), whatever the other filter says.</summary>
        public static int Count(IEnumerable<Maid> maids, Rarity? rarity)
        {
            var n = 0;
            foreach (var maid in maids)
            {
                if (Matches(maid, rarity, null)) n++;
            }

            return n;
        }

        /// <summary>How many maids the player owns with a natural position (null: all), whatever the rarity filter says.</summary>
        public static int Count(IEnumerable<Maid> maids, Role? role)
        {
            var n = 0;
            foreach (var maid in maids)
            {
                if (Matches(maid, null, role)) n++;
            }

            return n;
        }

        public static int Pages(int visible) => Math.Max(1, (visible + PageSize - 1) / PageSize);

        /// <summary>A page number inside 0..Pages-1.</summary>
        public static int ClampPage(int page, int visible) => Math.Max(0, Math.Min(page, Pages(visible) - 1));

        public static List<Maid> Page(IList<Maid> visible, int page)
        {
            var from = ClampPage(page, visible.Count) * PageSize;
            var result = new List<Maid>();
            for (var i = from; i < visible.Count && i < from + PageSize; i++) result.Add(visible[i]);
            return result;
        }

        /// <summary>
        /// The maid to keep selected: the same one while she is still visible, otherwise the first
        /// visible, otherwise none (-1).
        /// </summary>
        public static int Resolve(int selectedId, IList<Maid> visible)
        {
            for (var i = 0; i < visible.Count; i++)
            {
                if (visible[i].Id == selectedId) return selectedId;
            }

            return visible.Count > 0 ? visible[0].Id : -1;
        }

        public static RosterSort Next(RosterSort sort) => (RosterSort)(((int)sort + 1) % 3);

        /// <summary>A total order: rarity (rarest first) or level (highest first) or name, then name, then id.</summary>
        public static int Compare(Maid a, Maid b, RosterSort sort)
        {
            var c = 0;
            if (sort == RosterSort.Rarity) c = b.Rarity.CompareTo(a.Rarity);
            else if (sort == RosterSort.Level) c = b.Level.CompareTo(a.Level);

            if (c != 0) return c;

            c = string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            if (c != 0) return c;

            c = string.CompareOrdinal(a.Name, b.Name);
            return c != 0 ? c : a.Id.CompareTo(b.Id);
        }
    }
}
