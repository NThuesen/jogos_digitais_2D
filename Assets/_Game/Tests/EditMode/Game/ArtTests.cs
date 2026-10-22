using System;
using System.Linq;
using Neko.Game.Art;
using Neko.Sim.Maids;
using NUnit.Framework;
using UnityEngine;

namespace Neko.Tests.Game
{
    public sealed class ShapeRasteriserTests
    {
        private const int Size = 64;

        private static Color32[] Render(ShapeId shape, float detail = 0.5f) =>
            ShapeRasteriser.Render(new ShapeSpec(shape, Color.white, detail: detail), Size);

        private static float Coverage(Color32[] pixels) =>
            pixels.Count(p => p.a > 8) / (float)pixels.Length;

        [Test]
        public void EveryShapeDrawsSomethingAndDoesNotFillTheWholeSprite()
        {
            foreach (ShapeId shape in Enum.GetValues(typeof(ShapeId)))
            {
                var coverage = Coverage(Render(shape));

                Assert.Greater(coverage, 0.02f, shape + " drew nothing");
                Assert.Less(coverage, 0.98f, shape + " filled the whole sprite");
            }
        }

        [Test]
        public void EveryShapeIsRoughlyCentred()
        {
            foreach (ShapeId shape in Enum.GetValues(typeof(ShapeId)))
            {
                var pixels = Render(shape);
                var sumX = 0f;
                var sumY = 0f;
                var weight = 0f;

                for (var y = 0; y < Size; y++)
                {
                    for (var x = 0; x < Size; x++)
                    {
                        var a = pixels[y * Size + x].a / 255f;
                        sumX += x * a;
                        sumY += y * a;
                        weight += a;
                    }
                }

                var cx = sumX / weight / Size;
                var cy = sumY / weight / Size;

                Assert.That(cx, Is.EqualTo(0.5f).Within(0.18f), shape + " centre of mass x");
                Assert.That(cy, Is.EqualTo(0.5f).Within(0.18f), shape + " centre of mass y");
            }
        }

        [Test]
        public void EdgesAreAntialiased()
        {
            var pixels = Render(ShapeId.Circle);
            var partial = 0;

            for (var x = 0; x < Size; x++)
            {
                var a = pixels[Size / 2 * Size + x].a;
                if (a > 8 && a < 247) partial++;
            }

            Assert.GreaterOrEqual(partial, 2, "a scanline through a circle should cross two soft edges");
        }

        [Test]
        public void AnOutlineDarkensTheRimAndNotTheMiddle()
        {
            var spec = new ShapeSpec(ShapeId.Circle, Color.white)
                .Outlined(Color.black, 0.12f);
            var pixels = ShapeRasteriser.Render(spec, Size);

            var middle = pixels[Size / 2 * Size + Size / 2];
            var rim = pixels[Size / 2 * Size + 3];

            Assert.Greater(middle.r, 200, "the fill must survive");
            Assert.Less(rim.r, 120, "the rim must be the outline colour");
        }

        [Test]
        public void ScaleSquashesTheShape()
        {
            var wide = ShapeRasteriser.Render(
                new ShapeSpec(ShapeId.Circle, Color.white, scale: new Vector2(1f, 0.3f)), Size);

            var tallColumn = 0;
            var wideRow = 0;
            for (var i = 0; i < Size; i++)
            {
                if (wide[i * Size + Size / 2].a > 8) tallColumn++;
                if (wide[Size / 2 * Size + i].a > 8) wideRow++;
            }

            Assert.Less(tallColumn, wideRow, "a squashed circle must be wider than it is tall");
        }

        [Test]
        public void OffsetMovesTheShape()
        {
            var moved = ShapeRasteriser.Render(
                new ShapeSpec(ShapeId.Circle, Color.white, scale: new Vector2(0.4f, 0.4f),
                    offset: new Vector2(0.5f, 0f)), Size);

            var left = 0;
            var right = 0;
            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    if (moved[y * Size + x].a <= 8) continue;
                    if (x < Size / 2) left++;
                    else right++;
                }
            }

            Assert.Greater(right, left * 4, "an offset of +0.5 should land the shape on the right");
        }

        [Test]
        public void RotationTurnsAnAsymmetricShape()
        {
            var upright = ShapeRasteriser.Render(new ShapeSpec(ShapeId.Triangle, Color.white), Size);
            var turned = ShapeRasteriser.Render(
                new ShapeSpec(ShapeId.Triangle, Color.white, rotationDegrees: 180f), Size);

            var sameCount = 0;
            for (var i = 0; i < upright.Length; i++)
            {
                if (upright[i].a == turned[i].a) sameCount++;
            }

            Assert.Less(sameCount / (float)upright.Length, 0.95f,
                "turning a triangle upside down has to change the pixels");
        }

        [Test]
        public void BlendingASecondShapeDrawsOverTheFirst()
        {
            var pixels = ShapeRasteriser.Render(new ShapeSpec(ShapeId.RoundedRect, Color.red), Size);
            ShapeRasteriser.Blend(pixels, Size,
                new ShapeSpec(ShapeId.Circle, Color.blue, scale: new Vector2(0.4f, 0.4f)));

            var middle = pixels[Size / 2 * Size + Size / 2];

            Assert.Greater(middle.b, 200, "the last layer has to be on top");
            Assert.Less(middle.r, 60);
        }

        [Test]
        public void ATransparentLayerLeavesWhatIsUnderIt()
        {
            var pixels = ShapeRasteriser.Render(new ShapeSpec(ShapeId.RoundedRect, Color.red), Size);
            ShapeRasteriser.Blend(pixels, Size,
                new ShapeSpec(ShapeId.Circle, new Color(0f, 0f, 1f, 0f),
                    scale: new Vector2(0.4f, 0.4f)));

            Assert.Greater(pixels[Size / 2 * Size + Size / 2].r, 200);
        }

        [Test]
        public void RenderingIsDeterministic()
        {
            var once = Render(ShapeId.Star);
            var twice = Render(ShapeId.Star);

            CollectionAssert.AreEqual(once, twice);
        }

        [Test]
        public void ADegenerateSizeIsRefusedRatherThanThrown()
        {
            Assert.DoesNotThrow(() => ShapeRasteriser.Blend(null, 8,
                new ShapeSpec(ShapeId.Circle, Color.white)));
            Assert.DoesNotThrow(() => ShapeRasteriser.Blend(new Color32[4], 0,
                new ShapeSpec(ShapeId.Circle, Color.white)));
        }
    }

    public sealed class PaletteTests
    {
        [Test]
        public void EveryArchetypeHasItsOwnColour()
        {
            var colours = ArchetypeTable.All
                .Select(p => Palette.ForArchetype(p.Kind))
                .Select(ColorUtility.ToHtmlStringRGB)
                .ToList();

            CollectionAssert.AllItemsAreUnique(colours);
        }

        [Test]
        public void TheArchetypeColourMatchesWhatTheSimulationSays()
        {
            foreach (var profile in ArchetypeTable.All)
            {
                Assert.AreEqual(
                    profile.AccentHex.ToUpperInvariant().TrimStart('#'),
                    ColorUtility.ToHtmlStringRGB(Palette.ForArchetype(profile.Kind)),
                    profile.Kind + ": the roster card and the sprite must not disagree");
            }
        }

        [Test]
        public void TheFocusColourIsTheMostSaturatedThingInThePalette()
        {
            Color.RGBToHSV(Palette.Focus, out _, out var focusSaturation, out _);

            foreach (var profile in ArchetypeTable.All)
            {
                Color.RGBToHSV(Palette.ForArchetype(profile.Kind), out _, out var saturation, out _);
                Assert.Less(saturation, focusSaturation, profile.Kind + " is louder than the focus");
            }
        }

        [Test]
        public void NoArchetypeAccentSitsInTheFocusColoursValueBand()
        {
            var focus = Lightness(Palette.Focus);

            foreach (var profile in ArchetypeTable.All)
            {
                var difference = Mathf.Abs(Lightness(Palette.ForArchetype(profile.Kind)) - focus);
                Assert.Greater(difference, 4f,
                    profile.Kind + " is the same value as 'look here'");
            }
        }

        [Test]
        public void TheAccentsAreSpreadAcrossTheValueRange()
        {
            var values = ArchetypeTable.All
                .Select(p => Lightness(Palette.ForArchetype(p.Kind)))
                .OrderBy(v => v)
                .ToList();

            for (var i = 1; i < values.Count; i++)
            {
                Assert.GreaterOrEqual(values[i] - values[i - 1], 3f,
                    "two maids at the same value read as the same person at thumbnail size");
            }
        }

        [Test]
        public void EveryAccentSeparatesFromTheBackgroundItIsSeenAgainst()
        {
            var cream = Lightness(Palette.Cream);

            foreach (var profile in ArchetypeTable.All)
            {
                Assert.Greater(Mathf.Abs(Lightness(Palette.ForArchetype(profile.Kind)) - cream), 8f,
                    profile.Kind + " vanishes into the floor");
            }
        }

        [Test]
        public void PatienceGoesFromGoodThroughFocusToAlarm()
        {
            Assert.AreEqual(Palette.Good, Palette.ForPatience(1f));
            Assert.AreEqual(Palette.Focus, Palette.ForPatience(0.4f));
            Assert.AreEqual(Palette.Alarm, Palette.ForPatience(0.1f));
        }

        [Test]
        public void ABadHexIsLoudRatherThanSilentlyBlack()
        {
            foreach (var bad in new[] { null, "", "nope", "#12345", "#gggggg" })
            {
                Assert.AreEqual(Color.magenta, Palette.Hex(bad), bad ?? "null");
            }
        }

        [Test]
        public void HexReadsBothLengths()
        {
            Assert.AreEqual(new Color(1f, 0f, 0f, 1f), Palette.Hex("#FF0000"));
            Assert.That(Palette.Hex("#FF000080").a, Is.EqualTo(128f / 255f).Within(0.01f));
        }

        private static float Lightness(Color colour)
        {
            float Linear(float c) => c <= 0.04045f ? c / 12.92f : Mathf.Pow((c + 0.055f) / 1.055f, 2.4f);

            var y = 0.2126f * Linear(colour.r) + 0.7152f * Linear(colour.g) +
                    0.0722f * Linear(colour.b);
            return y > 0.008856f ? 116f * Mathf.Pow(y, 1f / 3f) - 16f : 903.3f * y;
        }
    }

    public sealed class SpriteFactoryTests
    {
        private SpriteFactory _factory;

        [SetUp]
        public void Setup() => _factory = new SpriteFactory();

        [TearDown]
        public void Teardown() => _factory.Clear();

        [Test]
        public void TheSameSpecComesBackFromTheCache()
        {
            var spec = new ShapeSpec(ShapeId.Circle, Palette.Focus);

            var first = _factory.Get(spec);
            var second = _factory.Get(spec);

            Assert.AreSame(first, second);
            Assert.AreEqual(1, _factory.CachedCount);
        }

        [Test]
        public void DifferentTintsAreDifferentSprites()
        {
            _factory.Get(new ShapeSpec(ShapeId.Circle, Palette.Focus));
            _factory.Get(new ShapeSpec(ShapeId.Circle, Palette.Alarm));

            Assert.AreEqual(2, _factory.CachedCount);
        }

        [Test]
        public void DifferentSizesAreDifferentSprites()
        {
            var spec = new ShapeSpec(ShapeId.Circle, Palette.Focus);
            _factory.Get(spec, 32);
            _factory.Get(spec, 64);

            Assert.AreEqual(2, _factory.CachedCount);
        }

        [Test]
        public void ALayeredSpriteIsCachedOnItsKey()
        {
            var layers = new[]
            {
                new ShapeSpec(ShapeId.RoundedRect, Palette.Wood),
                new ShapeSpec(ShapeId.Circle, Palette.Focus, scale: new Vector2(0.4f, 0.4f)),
            };

            var first = _factory.Layered(layers, "test");
            var second = _factory.Layered(layers, "test");

            Assert.AreSame(first, second);
            Assert.AreEqual(1, _factory.CachedCount);
        }

        [Test]
        public void ASpriteHasTheRightSizeAndPivot()
        {
            var sprite = _factory.Get(new ShapeSpec(ShapeId.Circle, Palette.Focus), 48);

            Assert.AreEqual(48, sprite.texture.width);
            Assert.AreEqual(48, sprite.texture.height);
            Assert.AreEqual(new Vector2(0.5f, 0.5f),
                new Vector2(sprite.pivot.x / sprite.rect.width, sprite.pivot.y / sprite.rect.height));
        }

        [Test]
        public void ClearingReleasesEverything()
        {
            _factory.Get(new ShapeSpec(ShapeId.Circle, Palette.Focus));
            _factory.Solid(Palette.Wood);

            _factory.Clear();

            Assert.AreEqual(0, _factory.CachedCount);
        }

        [Test]
        public void TheCacheKeyIsCultureIndependent()
        {
            // A machine with a comma decimal separator would otherwise produce a key in which
            // scale 1,5 and offset 1,5 are indistinguishable, and hand back the wrong sprite.
            var previous = System.Threading.Thread.CurrentThread.CurrentCulture;
            try
            {
                var spec = new ShapeSpec(ShapeId.Circle, Color.white,
                    scale: new Vector2(1.5f, 0.5f));

                System.Threading.Thread.CurrentThread.CurrentCulture =
                    new System.Globalization.CultureInfo("pt-BR");
                var brazilian = spec.CacheKey();

                System.Threading.Thread.CurrentThread.CurrentCulture =
                    System.Globalization.CultureInfo.InvariantCulture;

                Assert.AreEqual(spec.CacheKey(), brazilian);
            }
            finally
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = previous;
            }
        }
    }
}
