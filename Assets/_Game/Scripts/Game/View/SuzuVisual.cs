using System.Collections.Generic;
using Neko.Game.Art;
using Neko.Sim.Maids;
using Neko.Sim.Progress;
using UnityEngine;

namespace Neko.Game.View
{
    // Experimental mapping: names are not unique visual IDs in the save.
    public static class SuzuVisual
    {
        private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

        public static bool Matches(Maid maid)
        {
            if (maid == null || maid.Name != "Suzu") return false;
            // Preserve equipment rendering for slots not covered by this experiment.
            return maid.Gear[(int)GearSlot.Outfit] == null &&
                   maid.Gear[(int)GearSlot.Headwear] == null &&
                   maid.Gear[(int)GearSlot.Shoes] == null;
        }

        public static Sprite Portrait(Maid maid) => Matches(maid) ? Load("portrait") : null;

        public static Sprite Chibi(Maid maid, MaidPose pose)
        {
            if (!Matches(maid)) return null;
            // No cook or tired art exists; retain the existing visual for those states.
            if (pose == MaidPose.Cook || pose == MaidPose.Tired) return null;
            return Load(pose == MaidPose.Walk ? "walk" : pose == MaidPose.Carry ? "carry" : "idle");
        }

        public static Sprite Accessory(Maid maid, SpriteFactory factory)
        {
            var gear = maid.Gear[(int)GearSlot.Accessory];
            return gear == null ? null : factory.Get(new ShapeSpec(Shapes.Parse(gear.LayerShapeId),
                Palette.Hex(gear.TintHex), Palette.Ink, 0.04f), 64);
        }

        public static void Clear()
        {
            foreach (var sprite in Sprites.Values)
                if (sprite != null) Object.Destroy(sprite);
            Sprites.Clear();
        }

        private static Sprite Load(string name)
        {
            if (Sprites.TryGetValue(name, out var sprite) && sprite != null) return sprite;
            var texture = Resources.Load<Texture2D>("Suzu/" + name);
            if (texture == null) return null;
            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), texture.height / (128f / 96f));
            sprite.name = "Suzu_" + name;
            Sprites[name] = sprite;
            return sprite;
        }
    }
}
