using System;
using System.IO;
using Neko.Game.Art;
using Neko.Game.View;
using Neko.Sim.Maids;
using NUnit.Framework;
using UnityEngine;

namespace Neko.Tests.Game
{
    /// <summary>
    /// Writes the whole shape vocabulary out as PNGs so it can be looked at.
    /// </summary>
    /// <remarks>
    /// The art is generated, so there is no file to open in an image editor and no way to
    /// catch "the bow looks like a propeller" by reading the code. This is the review step:
    /// run it, then measure the result with the studio's pixels server. It writes nothing
    /// unless NEKO_SHAPE_DIR is set, so a normal test run stays silent and fast.
    /// </remarks>
    public sealed class ShapeSheetTests
    {
        private const int Size = 128;

        [Test]
        public void WriteTheShapeSheet()
        {
            var directory = Environment.GetEnvironmentVariable("NEKO_SHAPE_DIR");
            if (string.IsNullOrEmpty(directory))
            {
                Assert.Pass("NEKO_SHAPE_DIR not set; nothing to write.");
                return;
            }

            Directory.CreateDirectory(directory);

            foreach (ShapeId shape in Enum.GetValues(typeof(ShapeId)))
            {
                var spec = new ShapeSpec(shape, Palette.Cream, detail: 0.5f)
                    .Outlined(Palette.Ink, 0.05f);

                Write(Path.Combine(directory, shape + ".png"),
                    ShapeRasteriser.Render(spec, Size));
            }

            // One extra: a maid, so the layer stack is reviewable as a whole rather than as
            // fifteen separate shapes.
            Write(Path.Combine(directory, "_maid_genki.png"),
                MaidSheet(Archetype.Genki));
            Write(Path.Combine(directory, "_maid_ojousama.png"),
                MaidSheet(Archetype.Ojousama));

            Assert.IsTrue(Directory.GetFiles(directory, "*.png").Length > 0);
        }

        private static Color32[] MaidSheet(Archetype archetype)
        {
            var maid = Maid.Create(1, "Teste", archetype);
            var pixels = new Color32[Size * Size];

            foreach (var layer in MaidSpriteBuilder.Layers(maid, MaidPose.Idle))
            {
                ShapeRasteriser.Blend(pixels, Size, layer);
            }

            return pixels;
        }

        private static void Write(string path, Color32[] pixels)
        {
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            try
            {
                texture.SetPixels32(pixels);
                texture.Apply(false, false);
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }
    }
}
