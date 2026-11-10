using UnityEngine;

namespace Neko.Game.Core
{
    /// <summary>
    /// What draws in front of what.
    /// </summary>
    /// <remarks>
    /// One sorting layer with a computed order rather than several Unity sorting layers,
    /// because sorting layers live in ProjectSettings and this project builds everything in
    /// code: a number here is reviewable in a diff, a name in a YAML asset is not.
    /// </remarks>
    public static class Layers
    {
        public const int Floor = 0;
        public const int Rug = 1;
        public const int Furniture = 2;
        public const int Characters = 3;
        public const int Items = 4;
        public const int Fx = 5;
        public const int Overlay = 6;

        /// <summary>
        /// Within a layer, whatever is lower on screen draws in front.
        /// </summary>
        /// <remarks>
        /// Kept well under the interface's own sorting order. The first version multiplied the
        /// layer by ten thousand, which put the café somewhere above fifty thousand and the
        /// canvas at a hundred — so the room rendered straight through every menu in the game.
        /// </remarks>
        public const int InterfaceOrder = 10000;

        public static int Order(int layer, float y) =>
            layer * 1000 - Mathf.RoundToInt(Mathf.Clamp(y, -40f, 40f) * 10f);

        public static void Apply(SpriteRenderer renderer, int layer, float y)
        {
            if (renderer != null) renderer.sortingOrder = Order(layer, y);
        }

        /// <summary>Order slots per tile in the actor band: finer than <see cref="Order"/>'s ten.</summary>
        public const int ActorSlotsPerUnit = 40;

        /// <summary>
        /// Beyond this the actor order saturates, which keeps the whole band (±480) clear of the
        /// furniture band below it and the carried-item band above it.
        /// </summary>
        public const float ActorReach = 12f;

        /// <summary>
        /// The order of anything that stands on the floor: characters and every solid piece.
        /// </summary>
        /// <remarks>
        /// Characters and furniture share this one band, keyed by where each touches the floor,
        /// so a maid behind a table is covered by it and a maid in front covers it. Before this
        /// they lived in separate layers and a character was on top whatever its y.
        /// <paramref name="slot"/> puts a part of the same body (a carried plate) one step above
        /// it without leaving the band.
        /// </remarks>
        public static int ActorOrder(float floorY, int slot = 0) =>
            Characters * 1000 - Mathf.RoundToInt(Mathf.Clamp(floorY, -ActorReach, ActorReach) *
                                                 ActorSlotsPerUnit) + slot;

        public static void ApplyActor(SpriteRenderer renderer, float floorY, int slot = 0)
        {
            if (renderer != null) renderer.sortingOrder = ActorOrder(floorY, slot);
        }
    }
}
