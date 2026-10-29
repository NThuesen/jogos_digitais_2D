using System;
using System.IO;
using System.Linq;
using Neko.Game.Art;
using Neko.Sim.Cafe;
using NUnit.Framework;
using UnityEngine;

namespace Neko.Tests.Game
{
    public sealed class PixelCanvasTests
    {
        [Test]
        public void ExportFlipsToUnitysRowOrderAndScalesByWholePixels()
        {
            var canvas = new PixelCanvas(2, 2);
            canvas.Set(0, 0, Color.red);

            var pixels = canvas.Export(3);
            const int width = 6;

            // Logical (0, 0) is the top-left, which in a bottom-up buffer is the last rows.
            for (var y = 3; y < 6; y++)
            {
                for (var x = 0; x < 3; x++) Assert.AreEqual(255, pixels[y * width + x].r, x + "," + y);
            }

            Assert.AreEqual(0, pixels[0].a, "the bottom-left must stay empty");
        }

        [Test]
        public void TheOutlineHugsSolidPixelsAndIgnoresTranslucentOnes()
        {
            var canvas = new PixelCanvas(9, 9);
            canvas.Set(2, 2, Color.white);
            canvas.Set(6, 6, new Color(0f, 0f, 0f, 0.25f));

            canvas.Outline(Palette.Ink);

            Assert.IsTrue(canvas.IsSolid(2, 1), "above the solid pixel");
            Assert.IsTrue(canvas.IsSolid(1, 2), "left of the solid pixel");
            Assert.IsFalse(canvas.IsSolid(1, 1), "the outline is four-connected, not a box");
            Assert.IsFalse(canvas.IsSolid(6, 5), "a shadow is drawn over, never outlined");
        }

        [Test]
        public void ShadesGoDarkerAndLightsGoLighter()
        {
            var wood = Palette.Wood;
            Color.RGBToHSV(wood, out _, out _, out var value);
            Color.RGBToHSV(Tones.Shift(wood, -2), out _, out _, out var darker);
            Color.RGBToHSV(Tones.Shift(wood, 2), out _, out _, out var lighter);

            Assert.Less(darker, value);
            Assert.Greater(lighter, value);
        }

        [Test]
        public void AnEllipseCentredBetweenPixelsIsSymmetric()
        {
            var canvas = new PixelCanvas(12, 12);
            canvas.Ellipse(6f, 6f, 4.5f, 3f, Color.white);

            for (var y = 0; y < 12; y++)
            {
                for (var x = 0; x < 6; x++)
                {
                    Assert.AreEqual(canvas.IsSolid(x, y), canvas.IsSolid(11 - x, y), x + "," + y);
                }
            }
        }
    }

    public sealed class FurnitureArtTests
    {
        private static float Coverage(PixelCanvas canvas)
        {
            var solid = 0;
            for (var y = 0; y < canvas.Height; y++)
            {
                for (var x = 0; x < canvas.Width; x++)
                {
                    if (canvas.IsSolid(x, y)) solid++;
                }
            }

            return solid / (float)(canvas.Width * canvas.Height);
        }

        [Test]
        public void EveryPieceInTheCatalogIsPaintedForBothOrientations()
        {
            foreach (var def in PlaceableCatalog.All)
            {
                foreach (var (w, h) in new[] { (def.Width, def.Height), (def.Height, def.Width) })
                {
                    var canvas = FurnitureArt.PaintCanvas(def.Id, Palette.Hex(def.TintHex), w, h);
                    var label = def.Id + " " + w + "x" + h;

                    Assert.AreEqual(w * FurnitureArt.TilePixels, canvas.Width, label);
                    Assert.AreEqual(h * FurnitureArt.TilePixels + FurnitureArt.HeadroomFor(def.Id),
                        canvas.Height, label);

                    // The stage, curtain and platform both, legitimately covers 94% of its
                    // canvas; a painter that broke and filled a solid block covers all of it.
                    var coverage = Coverage(canvas);
                    Assert.Greater(coverage, 0.08f, label + " drew almost nothing");
                    Assert.Less(coverage, 0.97f, label + " came out as a solid block");
                }
            }
        }

        [Test]
        public void PaintingIsDeterministic()
        {
            var def = PlaceableCatalog.ById("sakura_tree");

            CollectionAssert.AreEqual(FurnitureArt.Paint(def, 1, 1).Pixels,
                FurnitureArt.Paint(def, 1, 1).Pixels);
        }

        [Test]
        public void ThePivotIsTheCentreOfTheFootprintWhateverTheHeight()
        {
            foreach (var id in new[] { "table_2", "plant", "chandelier", "stage" })
            {
                var def = PlaceableCatalog.ById(id);
                var picture = FurnitureArt.Paint(def, def.Width, def.Height);
                var footprint = def.Height * FurnitureArt.TilePixels * FurnitureArt.Scale;

                Assert.AreEqual(def.Width * FurnitureArt.TilePixels * FurnitureArt.Scale,
                    picture.Width, id);
                Assert.That(picture.Pivot.x, Is.EqualTo(0.5f).Within(1e-5f), id);
                Assert.That(picture.Pivot.y * picture.Height, Is.EqualTo(footprint * 0.5f).Within(0.01f),
                    id);
            }
        }

        [Test]
        public void SomethingNotInTheCatalogStillGetsAPicture()
        {
            Assert.DoesNotThrow(() => FurnitureArt.Paint("nao_existe", Palette.Wood, 1, 1));
            Assert.DoesNotThrow(() => FurnitureArt.Paint(null, 1, 1));
            Assert.Greater(Coverage(FurnitureArt.PaintCanvas("nao_existe", Palette.Wood, 1, 1)), 0.08f);
        }

        [Test]
        public void OnlyFlatPiecesLieUnderTheFurniture()
        {
            Assert.IsTrue(FurnitureArt.LiesFlat("rug"));
            Assert.IsTrue(FurnitureArt.LiesFlat("koi_pond"));
            Assert.IsFalse(FurnitureArt.LiesFlat("plant"), "a fern has to cover what is behind it");
            Assert.IsFalse(FurnitureArt.LiesFlat("table_2"));
        }
    }

    public sealed class FurnitureSpriteTests
    {
        private SpriteFactory _factory;

        [SetUp]
        public void Setup() => _factory = new SpriteFactory();

        [TearDown]
        public void Teardown() => _factory.Clear();

        [Test]
        public void APieceIsOneWorldUnitPerTileAndCachedPerFootprint()
        {
            var table = PlaceableCatalog.ById("table_2");

            var lying = _factory.Furniture(table, 2, 1);
            var again = _factory.Furniture(table, 2, 1);
            var standing = _factory.Furniture(table, 1, 2);

            Assert.AreSame(lying, again);
            Assert.AreNotSame(lying, standing, "a turned table is drawn again, not stretched");
            Assert.That(lying.rect.width / lying.pixelsPerUnit, Is.EqualTo(2f).Within(1e-4f));
            Assert.That(standing.rect.width / standing.pixelsPerUnit, Is.EqualTo(1f).Within(1e-4f));
        }

        /// <summary>
        /// Writes every piece out as PNGs, for review. Silent unless NEKO_FURNITURE_DIR is set,
        /// like the shape sheet.
        /// </summary>
        [Test]
        public void WriteTheFurnitureSheet()
        {
            var directory = Environment.GetEnvironmentVariable("NEKO_FURNITURE_DIR");
            if (string.IsNullOrEmpty(directory))
            {
                Assert.Pass("NEKO_FURNITURE_DIR not set; nothing to write.");
                return;
            }

            Directory.CreateDirectory(directory);

            foreach (var def in PlaceableCatalog.All)
            {
                var picture = FurnitureArt.Paint(def, def.Width, def.Height);
                var texture = new Texture2D(picture.Width, picture.Height, TextureFormat.RGBA32, false);
                try
                {
                    texture.SetPixels32(picture.Pixels);
                    texture.Apply(false, false);
                    File.WriteAllBytes(Path.Combine(directory, def.Id + ".png"), texture.EncodeToPNG());
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                }
            }

            Assert.GreaterOrEqual(Directory.GetFiles(directory, "*.png").Length,
                PlaceableCatalog.All.Count);
        }
    }
}
