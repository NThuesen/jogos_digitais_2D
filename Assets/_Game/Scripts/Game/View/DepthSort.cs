using Neko.Sim.Cafe;
using UnityEngine;

namespace Neko.Game.View
{
    /// <summary>
    /// The floor-contact y that decides who draws in front: characters and solid furniture are
    /// ordered by it through <see cref="Core.Layers.ActorOrder"/>.
    /// </summary>
    /// <remarks>
    /// A piece of furniture is keyed by the front edge of its footprint. Whatever stands behind
    /// that edge is behind the piece and whatever stands in front of it is in front, for any
    /// footprint and any y. A character <em>inside</em> a footprint is using the piece — seated at
    /// a table, working a station, crossing a rug-sized stage — so it is keyed just in front of
    /// that piece instead of behind it, ordered among its neighbours by its own y. The
    /// simulation walks workers in straight lines through furniture cells, so this case is
    /// routine rather than an edge.
    /// </remarks>
    public static class DepthSort
    {
        /// <summary>Which pieces occlude and are occluded. Flat ones and the door lie underneath.</summary>
        public static bool IsSolid(PlaceableDef def) =>
            def != null && def.Id != "door" && !Art.FurnitureArt.LiesFlat(def.Id);

        /// <summary>
        /// Whether somebody inside the footprint is "on" the piece. The pond is flat as a whole but
        /// has standing parts, so it counts; a rug and the door do not.
        /// </summary>
        public static bool StandsOn(PlaceableDef def) =>
            def != null && def.Id != "door" && def.Id != "rug";

        /// <summary>Where a piece meets the floor in front: the bottom edge of its lowest row.</summary>
        public static float FrontEdge(PlacedItem item) => item.Y - 0.5f;

        /// <summary>The key of a solid piece: its front edge.</summary>
        public static float SolidKey(PlacedItem item) => FrontEdge(item);

        /// <summary>
        /// The key for a character whose feet are at <paramref name="feet"/>.
        /// </summary>
        public static float ActorKey(CafeGrid grid, Vector3 feet)
        {
            if (grid == null) return feet.y;

            var item = grid.ItemAt(Mathf.RoundToInt(feet.x), Mathf.RoundToInt(feet.y));
            if (item == null || !StandsOn(item.Def)) return feet.y;

            // Just in front of the piece it stands on. The slope keeps two occupants of a
            // two-row table in their own order; across a footprint of up to two rows it spans
            // [-0.2, -0.04] below the front edge, so it never reaches the edge itself.
            var front = FrontEdge(item);
            var depth = Mathf.Clamp(feet.y - front, 0f, 2f);
            return front - 0.2f + depth * 0.08f;
        }
    }
}
