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
        private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
        public static bool Matches(Maid maid) => SaveBridge.Isolated && maid != null && maid.Name == "Mina";

        public static Sprite Chibi(Maid maid, MaidPose pose, Facing facing)
        {
            if (!Matches(maid)) return null;
            // Missing state art keeps the same identity. Mood/task rules remain in MaidView.
            var name = pose == MaidPose.Carry ? "carry" :
                pose == MaidPose.Cook || pose == MaidPose.Tired ? "idle" :
                facing == Facing.Side ? "side" : facing == Facing.Back ? "back" :
                pose == MaidPose.Walk ? "walk" : "idle";
            if (Sprites.TryGetValue(name, out var cached)) return cached;
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
