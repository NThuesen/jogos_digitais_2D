using System;
using System.Collections.Generic;
using Neko.Game.Art;
using Neko.Sim.Maids;
using Neko.Sim.Progress;
using UnityEngine;

namespace Neko.Game.View
{
    public enum MaidPose
    {
        Idle = 0,
        Walk,
        Carry,
        Cook,
        Tired,
    }

    /// <summary>
    /// Builds one maid as a stack of tinted shapes.
    /// </summary>
    /// <remarks>
    /// This is the design's first pillar made mechanical: equipment is visible because a piece
    /// of equipment <em>is</em> a layer in this stack, read straight off
    /// <see cref="Equipment.LayerShapeId"/> and <see cref="Equipment.TintHex"/>. There is no
    /// second table mapping gear to art that could drift out of step with the stats.
    ///
    /// Proportions are chibi — the head is about two fifths of the figure — because what
    /// carries a sixty-pixel character is its silhouette, and a big head plus whatever breaks
    /// the outline (ears, twin tails, a tiara) is a silhouette. Facial detail is two dots.
    /// </remarks>
    public static class MaidSpriteBuilder
    {
        private const float HeadY = 0.30f;
        private const float HeadRadius = 0.40f;
        private const float OutlineWidth = 0.028f;

        public static IReadOnlyList<ShapeSpec> Layers(Maid maid, MaidPose pose)
        {
            var layers = new List<ShapeSpec>(16);
            if (maid == null) return layers;

            var accent = Palette.ForArchetype(maid.Archetype);
            var hair = Palette.Darken(accent, 0.25f);
            var ink = Palette.Ink;

            var lean = pose == MaidPose.Walk ? 0.05f : pose == MaidPose.Cook ? 0.08f : 0f;
            var droop = pose == MaidPose.Tired ? -0.07f : 0f;

            // Contact shadow. Without it everybody floats.
            layers.Add(new ShapeSpec(ShapeId.Circle, Palette.WithAlpha(ink, 0.18f),
                offset: new Vector2(0f, -0.88f), scale: new Vector2(0.42f, 0.09f)));

            AddShoes(layers, maid, pose);
            AddBody(layers, maid, accent, lean);
            AddArms(layers, maid, pose, lean);
            AddTail(layers, accent, ink, pose);
            AddHairBack(layers, maid, hair, lean, droop);
            AddHead(layers, lean, droop, ink);
            AddEars(layers, accent, hair, ink, lean, droop);
            AddFringe(layers, maid, hair, lean, droop);
            AddFace(layers, maid, pose, accent, ink, lean, droop);
            AddHeadwear(layers, maid, accent, ink, lean, droop);
            AddAccessory(layers, maid, accent, ink, lean);

            if (pose == MaidPose.Carry) AddTray(layers, ink);

            return layers;
        }

        /// <summary>
        /// Everything that changes the pixels, as a short string.
        /// </summary>
        /// <remarks>
        /// Level and bond are deliberately absent: they change her numbers, not her picture,
        /// and including them would rebuild every sprite in the café on a level-up.
        /// </remarks>
        public static string CacheKey(Maid maid, MaidPose pose)
        {
            if (maid == null) return "none";

            var key = maid.Archetype + ":" + pose;
            for (var i = 0; i < maid.Gear.Length; i++) key += ":" + (maid.Gear[i]?.Id ?? "-");
            return key;
        }

        // -------------------------------------------------------------------------------

        private static void AddShoes(List<ShapeSpec> layers, Maid maid, MaidPose pose)
        {
            var gear = maid.Gear[(int)GearSlot.Shoes];
            var tint = gear == null ? Palette.Darken(Palette.Uniform, 0.1f) : Palette.Hex(gear.TintHex);
            var shape = gear == null ? ShapeId.Capsule : Shapes.Parse(gear.LayerShapeId);

            // Walking splits the feet apart, which is the whole of the walk cycle at this size.
            var spread = pose == MaidPose.Walk ? 0.11f : 0.07f;

            for (var side = -1; side <= 1; side += 2)
            {
                layers.Add(new ShapeSpec(shape, tint, Palette.Ink, OutlineWidth,
                    offset: new Vector2(side * spread, -0.78f),
                    scale: new Vector2(0.16f, 0.14f)));
            }
        }

        private static void AddBody(List<ShapeSpec> layers, Maid maid, Color accent, float lean)
        {
            var gear = maid.Gear[(int)GearSlot.Outfit];
            var tint = gear == null ? Palette.Uniform : Palette.Hex(gear.TintHex);
            var shape = gear == null ? ShapeId.Apron : Shapes.Parse(gear.LayerShapeId);

            layers.Add(new ShapeSpec(shape, tint, Palette.Ink, OutlineWidth,
                offset: new Vector2(lean * 0.4f, -0.34f), scale: new Vector2(0.40f, 0.42f)));

            // The white pinafore over the dress: the one thing that says "maid" at any size.
            layers.Add(new ShapeSpec(ShapeId.Apron, Palette.UniformTrim, Palette.Ink, OutlineWidth,
                offset: new Vector2(lean * 0.4f, -0.44f), scale: new Vector2(0.24f, 0.28f)));

            // A collar in her own colour, so the archetype reads even from behind.
            layers.Add(new ShapeSpec(ShapeId.Capsule, accent,
                offset: new Vector2(lean * 0.5f, -0.04f), scale: new Vector2(0.26f, 0.10f)));
        }

        private static void AddArms(List<ShapeSpec> layers, Maid maid, MaidPose pose, float lean)
        {
            var skin = Palette.Skin;
            var forward = pose == MaidPose.Carry || pose == MaidPose.Cook;

            for (var side = -1; side <= 1; side += 2)
            {
                var x = side * (forward ? 0.20f : 0.29f) + lean * 0.3f;
                var y = forward ? -0.24f : -0.32f;

                layers.Add(new ShapeSpec(ShapeId.Capsule, skin, Palette.Ink, OutlineWidth,
                    offset: new Vector2(x, y), scale: new Vector2(0.11f, 0.17f),
                    rotationDegrees: forward ? side * 24f : side * 8f));
            }
        }

        private static void AddTail(List<ShapeSpec> layers, Color accent, Color ink, MaidPose pose)
        {
            var curl = pose == MaidPose.Tired ? -18f : 26f;

            layers.Add(new ShapeSpec(ShapeId.Capsule, Palette.Darken(accent, 0.18f), ink,
                OutlineWidth, offset: new Vector2(0.37f, -0.46f),
                scale: new Vector2(0.09f, 0.22f), rotationDegrees: curl));
        }

        private static void AddHairBack(List<ShapeSpec> layers, Maid maid, Color hair, float lean,
            float droop)
        {
            layers.Add(new ShapeSpec(ShapeId.Circle, hair, Palette.Ink, OutlineWidth,
                offset: new Vector2(lean, HeadY + droop - 0.03f),
                scale: new Vector2(HeadRadius + 0.07f, HeadRadius + 0.07f)));

            AddTell(layers, maid, hair, lean, droop);
        }

        /// <summary>
        /// The one thing that breaks her outline, which is all a small figure has.
        /// </summary>
        private static void AddTell(List<ShapeSpec> layers, Maid maid, Color hair, float lean,
            float droop)
        {
            var y = HeadY + droop;
            var ink = Palette.Ink;

            switch (maid.Archetype)
            {
                case Archetype.Genki: // twin tails, high
                    for (var side = -1; side <= 1; side += 2)
                    {
                        layers.Add(new ShapeSpec(ShapeId.Capsule, hair, ink, OutlineWidth,
                            offset: new Vector2(side * 0.44f + lean, y + 0.12f),
                            scale: new Vector2(0.11f, 0.26f), rotationDegrees: side * 22f));
                    }

                    break;

                case Archetype.Kuudere: // straight bob
                    layers.Add(new ShapeSpec(ShapeId.RoundedRect, hair, ink, OutlineWidth,
                        offset: new Vector2(lean, y - 0.16f), scale: new Vector2(0.46f, 0.22f),
                        detail: 0.25f));
                    break;

                case Archetype.Tsundere: // side ponytail
                    layers.Add(new ShapeSpec(ShapeId.Capsule, hair, ink, OutlineWidth,
                        offset: new Vector2(0.46f + lean, y + 0.02f),
                        scale: new Vector2(0.13f, 0.27f), rotationDegrees: 34f));
                    break;

                case Archetype.Dojikko: // ahoge
                    layers.Add(new ShapeSpec(ShapeId.Capsule, hair, ink, OutlineWidth,
                        offset: new Vector2(lean - 0.06f, y + 0.46f),
                        scale: new Vector2(0.05f, 0.14f), rotationDegrees: 28f));
                    break;

                case Archetype.Ojousama: // drill curls
                    for (var side = -1; side <= 1; side += 2)
                    {
                        layers.Add(new ShapeSpec(ShapeId.Circle, hair, ink, OutlineWidth,
                            offset: new Vector2(side * 0.44f + lean, y - 0.14f),
                            scale: new Vector2(0.19f, 0.19f)));
                    }

                    break;

                case Archetype.Oneesan: // long hair past the shoulders
                    layers.Add(new ShapeSpec(ShapeId.Apron, hair, ink, OutlineWidth,
                        offset: new Vector2(lean, y - 0.42f), scale: new Vector2(0.40f, 0.36f)));
                    break;

                case Archetype.Idol: // cape
                    layers.Add(new ShapeSpec(ShapeId.Apron, Palette.Lighten(hair, 0.35f), ink,
                        OutlineWidth, offset: new Vector2(lean, -0.30f),
                        scale: new Vector2(0.48f, 0.34f)));
                    break;

                case Archetype.Gourmand: // chef scarf
                    layers.Add(new ShapeSpec(ShapeId.Capsule, Palette.UniformTrim, ink,
                        OutlineWidth, offset: new Vector2(lean, -0.03f),
                        scale: new Vector2(0.30f, 0.09f)));
                    break;

                case Archetype.Mukuchi: // hair over one eye
                    layers.Add(new ShapeSpec(ShapeId.Crescent, hair, ink, OutlineWidth,
                        offset: new Vector2(lean - 0.16f, y + 0.08f),
                        scale: new Vector2(0.30f, 0.30f), rotationDegrees: 160f));
                    break;
            }
        }

        private static void AddHead(List<ShapeSpec> layers, float lean, float droop, Color ink)
        {
            layers.Add(new ShapeSpec(ShapeId.Circle, Palette.Skin, ink, OutlineWidth,
                offset: new Vector2(lean, HeadY + droop),
                scale: new Vector2(HeadRadius, HeadRadius)));
        }

        private static void AddEars(List<ShapeSpec> layers, Color accent, Color hair, Color ink,
            float lean, float droop)
        {
            for (var side = -1; side <= 1; side += 2)
            {
                layers.Add(new ShapeSpec(ShapeId.CatEar, hair, ink, OutlineWidth,
                    offset: new Vector2(side * 0.24f + lean, HeadY + droop + 0.36f),
                    scale: new Vector2(0.17f, 0.20f), rotationDegrees: side * 14f));

                // The inner ear is the one warm dot on the whole head. Pink rather than a
                // light tint of her accent: a pale orange inside an orange ear is invisible,
                // which is what the first render showed.
                layers.Add(new ShapeSpec(ShapeId.CatEar, Palette.InnerEar,
                    offset: new Vector2(side * 0.24f + lean, HeadY + droop + 0.33f),
                    scale: new Vector2(0.085f, 0.105f), rotationDegrees: side * 14f));
            }
        }

        private static void AddFringe(List<ShapeSpec> layers, Maid maid, Color hair, float lean,
            float droop)
        {
            layers.Add(new ShapeSpec(ShapeId.Crescent, hair, Palette.Ink, OutlineWidth,
                offset: new Vector2(lean, HeadY + droop + 0.12f),
                scale: new Vector2(HeadRadius + 0.02f, HeadRadius * 0.62f),
                rotationDegrees: 90f));
        }

        private static void AddFace(List<ShapeSpec> layers, Maid maid, MaidPose pose, Color accent,
            Color ink, float lean, float droop)
        {
            var y = HeadY + droop - 0.04f;

            // Tired eyes are dashes. One shape, and the whole expression changes.
            var tired = pose == MaidPose.Tired;
            var eye = tired ? new Vector2(0.085f, 0.018f) : new Vector2(0.055f, 0.075f);

            for (var side = -1; side <= 1; side += 2)
            {
                // Mukuchi keeps one eye behind her hair, so only draw the visible one.
                if (maid.Archetype == Archetype.Mukuchi && side < 0) continue;

                layers.Add(new ShapeSpec(ShapeId.Capsule, ink,
                    offset: new Vector2(side * 0.16f + lean, y), scale: eye));

                // A highlight. Two pixels of white, and she stops looking like a doll.
                if (tired) continue;
                layers.Add(new ShapeSpec(ShapeId.Circle, Palette.Panel,
                    offset: new Vector2(side * 0.16f + lean + 0.018f, y + 0.032f),
                    scale: new Vector2(0.021f, 0.024f)));
            }

            layers.Add(new ShapeSpec(ShapeId.Circle, Palette.WithAlpha(accent, 0.5f),
                offset: new Vector2(0.25f + lean, y - 0.07f), scale: new Vector2(0.07f, 0.05f)));

            layers.Add(new ShapeSpec(ShapeId.Circle, Palette.WithAlpha(accent, 0.5f),
                offset: new Vector2(-0.25f + lean, y - 0.07f), scale: new Vector2(0.07f, 0.05f)));
        }

        private static void AddHeadwear(List<ShapeSpec> layers, Maid maid, Color accent, Color ink,
            float lean, float droop)
        {
            var gear = maid.Gear[(int)GearSlot.Headwear];
            if (gear == null) return;

            layers.Add(new ShapeSpec(Shapes.Parse(gear.LayerShapeId), Palette.Hex(gear.TintHex),
                ink, OutlineWidth, offset: new Vector2(lean + 0.18f, HeadY + droop + 0.30f),
                scale: new Vector2(0.19f, 0.15f)));
        }

        private static void AddAccessory(List<ShapeSpec> layers, Maid maid, Color accent, Color ink,
            float lean)
        {
            var gear = maid.Gear[(int)GearSlot.Accessory];
            if (gear == null) return;

            layers.Add(new ShapeSpec(Shapes.Parse(gear.LayerShapeId), Palette.Hex(gear.TintHex),
                ink, OutlineWidth, offset: new Vector2(lean, -0.14f),
                scale: new Vector2(0.11f, 0.11f)));
        }

        private static void AddTray(List<ShapeSpec> layers, Color ink)
        {
            layers.Add(new ShapeSpec(ShapeId.Plate, Palette.Panel, ink, OutlineWidth,
                offset: new Vector2(0f, -0.14f), scale: new Vector2(0.26f, 0.26f)));
        }
    }

    /// <summary>Turns the shape names the data tables carry into the enum.</summary>
    public static class Shapes
    {
        public static ShapeId Parse(string name) =>
            Enum.TryParse(name, true, out ShapeId shape) ? shape : ShapeId.Circle;
    }
}
