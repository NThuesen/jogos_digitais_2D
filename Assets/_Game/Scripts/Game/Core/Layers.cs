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
    }
}
