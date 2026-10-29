using System.Collections.Generic;
using Neko.Game.Core;
using Neko.Sim.Maids;
using UnityEngine;

namespace Neko.Game.View
{
    // Experiment only: no persistent identity mapping or player-save changes.
    public static class MinaVisual
    {
        public enum Facing { Front, Side, Back }

        /// <summary>The two movement treatments being compared.</summary>
        public enum Variant { V0, V1 }

        private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

        /// <summary>V0 keeps the time-based bob; V1 drives the step from distance walked.</summary>
        public static Variant Motion { get; set; } =
            System.Environment.GetEnvironmentVariable("NEKO_MINA_VARIANT") == "V1" ? Variant.V1 : Variant.V0;

        public static bool Matches(Maid maid) => SaveBridge.Isolated && maid != null && maid.Name == "Mina";

        /// <summary>
        /// The drawing for one state.
        /// </summary>
        /// <remarks>
        /// Standing is always the front idle, whatever she was doing, so she never stops in a
        /// stride. Cook and Tired have no art of their own: while she walks they use the walk
        /// of her heading, standing they use the idle. Carry has a single tray drawing, so it
        /// covers left, right and towards the camera; walking away she is drawn from behind,
        /// where her body hides the tray.
        /// </remarks>
        public static string NameFor(MaidPose pose, Facing facing, bool moving)
        {
            if (pose == MaidPose.Carry) return moving && facing == Facing.Back ? "back" : "carry";
            if (!moving) return "idle";
            return facing == Facing.Side ? "side" : facing == Facing.Back ? "back" : "walk";
        }

        /// <summary>Side and carry are drawn heading right; mirroring turns them left.</summary>
        public static bool Mirrors(string name) => name == "side" || name == "carry";

        public static Sprite Chibi(Maid maid, string name)
        {
            if (!Matches(maid)) return null;
            if (Sprites.TryGetValue(name, out var cached) && cached != null) return cached;
            var texture = Resources.Load<Texture2D>("Mina/" + name);
            if (texture == null) return null;
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0f), texture.height);
            sprite.name = "Mina_" + name;
            Sprites[name] = sprite;
            return sprite;
        }

        public static void Clear()
        {
            foreach (var sprite in Sprites.Values)
                if (sprite != null)
                {
                    if (Application.isPlaying) Object.Destroy(sprite);
                    else Object.DestroyImmediate(sprite);
                }
            Sprites.Clear();
        }
    }
}
