using System.Collections.Generic;
using Neko.Game.Art;
using UnityEngine;

namespace Neko.Game.View
{
    /// <summary>
    /// The player's own avatar.
    /// </summary>
    /// <remarks>
    /// Deliberately the plainest figure on screen — a waistcoat, no ears, no accent colour.
    /// The whole arc of the game is the roster growing past him, and that only reads if he
    /// never joins the parade of colours they are.
    /// </remarks>
    public static class ManagerSpriteBuilder
    {
        private const float HeadY = 0.30f;
        private const float OutlineWidth = 0.028f;

        public static Sprite Sprite(SpriteFactory sprites, MaidPose pose) =>
            sprites.Layered(Layers(pose), "manager:" + pose, 128);

        public static IReadOnlyList<ShapeSpec> Layers(MaidPose pose)
        {
            var layers = new List<ShapeSpec>(10);
            var lean = pose == MaidPose.Walk ? 0.05f : pose == MaidPose.Cook ? 0.08f : 0f;
            var ink = Palette.Ink;
            var cloth = Palette.Hex("#4A5566");

            layers.Add(new ShapeSpec(ShapeId.Circle, Palette.WithAlpha(ink, 0.18f),
                offset: new Vector2(0f, -0.88f), scale: new Vector2(0.42f, 0.09f)));

            for (var side = -1; side <= 1; side += 2)
            {
                layers.Add(new ShapeSpec(ShapeId.Capsule, Palette.Darken(cloth, 0.25f), ink,
                    OutlineWidth,
                    offset: new Vector2(side * (pose == MaidPose.Walk ? 0.11f : 0.07f), -0.78f),
                    scale: new Vector2(0.16f, 0.14f)));
            }

            layers.Add(new ShapeSpec(ShapeId.Apron, cloth, ink, OutlineWidth,
                offset: new Vector2(lean * 0.4f, -0.34f), scale: new Vector2(0.38f, 0.42f)));

            layers.Add(new ShapeSpec(ShapeId.RoundedRect, Palette.Panel, ink, OutlineWidth,
                offset: new Vector2(lean * 0.4f, -0.30f), scale: new Vector2(0.11f, 0.30f),
                detail: 0.3f));

            for (var side = -1; side <= 1; side += 2)
            {
                var forward = pose == MaidPose.Carry || pose == MaidPose.Cook;
                layers.Add(new ShapeSpec(ShapeId.Capsule, Palette.Skin, ink, OutlineWidth,
                    offset: new Vector2(side * (forward ? 0.20f : 0.29f) + lean * 0.3f,
                        forward ? -0.24f : -0.32f),
                    scale: new Vector2(0.11f, 0.17f), rotationDegrees: forward ? side * 24f : 0f));
            }

            layers.Add(new ShapeSpec(ShapeId.Circle, Palette.Hex("#4A3F39"), ink, OutlineWidth,
                offset: new Vector2(lean, HeadY - 0.02f), scale: new Vector2(0.44f, 0.44f)));

            layers.Add(new ShapeSpec(ShapeId.Circle, Palette.Skin, ink, OutlineWidth,
                offset: new Vector2(lean, HeadY), scale: new Vector2(0.40f, 0.40f)));

            var eye = pose == MaidPose.Tired
                ? new Vector2(0.085f, 0.018f)
                : new Vector2(0.05f, 0.065f);

            for (var side = -1; side <= 1; side += 2)
            {
                layers.Add(new ShapeSpec(ShapeId.Capsule, ink,
                    offset: new Vector2(side * 0.16f + lean, HeadY - 0.04f), scale: eye));
            }

            if (pose == MaidPose.Carry)
            {
                layers.Add(new ShapeSpec(ShapeId.Plate, Palette.Panel, ink, OutlineWidth,
                    offset: new Vector2(0f, -0.14f), scale: new Vector2(0.26f, 0.26f)));
            }

            return layers;
        }
    }
}
