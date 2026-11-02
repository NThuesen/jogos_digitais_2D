using System.IO;
using System.Linq;
using Neko.Game.Art;
using Neko.Game.View;
using Neko.Sim.Maids;
using Neko.Sim.Progress;
using NUnit.Framework;
using UnityEngine;

namespace Neko.Tests.EditMode.Game
{
    /// <summary>The shared catalog of authored walk sheets (Suzu legacy sheet plus the ART-03 maids, nine in all).</summary>
    public sealed class MaidAnimCatalogTests
    {
        private static readonly string[] Expected =
            { "suzu", "hina", "akari", "momo", "reina", "kaede", "nene", "ichika", "yoru" };

        private SpriteFactory _factory;

        [SetUp]
        public void Setup() => _factory = new SpriteFactory();

        [TearDown]
        public void Teardown() => _factory.Clear();

        private static Maid Maid(string name, Archetype archetype) =>
            new Maid { Id = 1, Name = name, Archetype = archetype };

        [Test]
        public void TheCatalogListsEveryIntegratedMaidAndNoOtherOne()
        {
            CollectionAssert.AreEquivalent(Expected, MaidAnimCatalog.All.Select(e => e.id).ToArray());
        }

        [Test]
        public void ANameAndArchetypePairIsNeededAndIsNeverConfusedWithAnotherMaid()
        {
            foreach (var entry in MaidAnimCatalog.All)
            {
                Assert.AreSame(entry, MaidAnimCatalog.Resolve(Maid(entry.name, entry.ArchetypeValue)));
                var other = entry.ArchetypeValue == Archetype.Genki ? Archetype.Kuudere : Archetype.Genki;
                Assert.IsNull(MaidAnimCatalog.Resolve(Maid(entry.name, other)), entry.name + " from another draw");
            }

            Assert.IsNull(MaidAnimCatalog.Resolve(Maid("Mina", Archetype.Genki)));
            Assert.IsNull(MaidAnimCatalog.Resolve(null));
        }

        [Test]
        public void AnyEquipmentKeepsTheLayeredLookSoGearStaysVisible()
        {
            foreach (var entry in MaidAnimCatalog.All)
            {
                var maid = Maid(entry.name, entry.ArchetypeValue);
                maid.Gear[0] = new Equipment("x", "x", GearSlot.Outfit, Stats.Zero,
                    Neko.Sim.Core.ThemeTag.None, 0, 1, "Apron", "FF0000");
                Assert.IsNull(MaidAnimCatalog.Resolve(maid), entry.name);
            }
        }

        [Test]
        public void EverySheetCutsIntoTheDeclaredFramesWithFeetAtTheBottomCentre()
        {
            foreach (var entry in MaidAnimCatalog.All)
            {
                var frames = MaidAnimCatalog.Load(_factory, entry);
                Assert.IsNotNull(frames, entry.resource + " is missing or the wrong size");
                Assert.AreEqual(entry.frames, frames.Length, entry.id);
                Assert.AreEqual(new Vector2(entry.CellWidth, entry.CellHeight), frames[0].rect.size, entry.id);
                Assert.AreEqual(new Vector2(0.5f, 0f), frames[0].pivot / frames[0].rect.size, entry.id);
                foreach (var index in entry.rest_frames) Assert.Less(index, entry.frames, entry.id);
                Assert.Contains(entry.idle_frame, entry.rest_frames, entry.id);
                Assert.Greater(entry.units_per_cycle, 0.5f, entry.id);
            }
        }

        [Test]
        public void EverySheetIsImportedTrilinearMippedAndNonReadable()
        {
            foreach (var entry in MaidAnimCatalog.All)
            {
                var texture = Resources.Load<Texture2D>(entry.resource);
                Assert.IsNotNull(texture, entry.resource);
                Assert.IsFalse(texture.isReadable, entry.id);
                Assert.AreEqual(FilterMode.Trilinear, texture.filterMode, entry.id); // illustrated art (ART-04): smooth, not point-sampled
                Assert.Greater(texture.mipmapCount, 1, entry.id + " needs mipmaps: the sprite is drawn smaller than its texture");
                Assert.LessOrEqual(texture.width * texture.height, 2048 * 2048, entry.id + " is bigger than it needs to be");
            }
        }

        [Test]
        public void TheSharedTextureBudgetStaysSmall()
        {
            long pixels = 0;
            foreach (var entry in MaidAnimCatalog.All)
                pixels += Resources.Load<Texture2D>(entry.resource).width * Resources.Load<Texture2D>(entry.resource).height;
            Assert.Less(pixels, 12L * 1000 * 1000, "the walk sheets must stay under 12 Mpx for nine maids (about 10 Mpx measured: ~13 MB block-compressed on the GPU, ~40 MB if a platform left them uncompressed)");
        }

        [Test]
        public void TheCatalogMatchesWhatTheProcessingToolWroteForEachMaid()
        {
            foreach (var entry in MaidAnimCatalog.All)
            {
                var path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "docs", "art", "maid-walk", entry.id,
                    "manifest.json"));
                var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(path));
                Assert.AreEqual(entry.frames, manifest.frames, entry.id);
                Assert.AreEqual(entry.columns, manifest.columns, entry.id);
                Assert.AreEqual(entry.rows, manifest.rows, entry.id);
                Assert.AreEqual(entry.CellWidth, manifest.cell[0], entry.id);
                Assert.AreEqual(entry.CellHeight, manifest.cell[1], entry.id);
                Assert.AreEqual(entry.idle_frame, manifest.idle_frame, entry.id);
                Assert.AreEqual(entry.units_per_cycle, manifest.units_per_cycle, 1e-4f, entry.id);
                CollectionAssert.AreEqual(entry.rest_frames, manifest.rest_frames, entry.id);
            }
        }

        [Test]
        public void TheBackupSuzuSheetStillLoadsForRollback()
        {
            var frames = _factory.Sheet("Suzu/suzu_walk_sheet", 6, 5, 92, 126, 30, 96f);
            Assert.IsNotNull(frames, "the PR #27 sheet is kept as a backup");
            Assert.AreNotEqual("Suzu/suzu_walk_sheet", MaidAnimCatalog.ById("suzu").resource, "the catalog uses the new sheet");
        }

        [System.Serializable]
        private sealed class Manifest
        {
            public int frames, columns, rows, idle_frame;
            public int[] cell, rest_frames;
            public float units_per_cycle;
        }

        [Test]
        public void AMissingSheetIsNotAnError()
        {
            Assert.IsNull(_factory.Sheet("Maids/does_not_exist", 1, 1, 1, 1, 1, 96f));
        }
    }

    public sealed class WalkCycleTests
    {
        private static WalkCycle Cycle() => new WalkCycle(30, 1.5f, new[] { 10, 11, 26 }, 26);

        [Test]
        public void FramesFollowDistanceNotTime()
        {
            var cycle = Cycle();
            cycle.Step(0.05f, 0.02f, 0.02f); // 0.05 / 1.5 * 30 = exactly one frame
            Assert.AreEqual(27, cycle.Frame);
            Assert.IsTrue(cycle.Moving);
        }

        [Test]
        public void FastForwardNeverSkipsMoreThanADrawingPerScreenFrame()
        {
            var cycle = Cycle();
            var before = cycle.Phase;
            cycle.Step(0.8f, 0.0667f, 0.0167f); // 4x speed: 16 frames of distance in one 60 Hz frame
            var moved = Mathf.Repeat(cycle.Phase - before, 30f);
            Assert.LessOrEqual(moved, WalkCycle.MaxFramesPerSecond * 0.0167f + 1e-4f);
            Assert.IsTrue(cycle.Moving);
        }

        [Test]
        public void ACompleteCycleOfDistanceReturnsToTheSameFrame()
        {
            var cycle = Cycle();
            for (var i = 0; i < 30; i++) cycle.Step(0.05f, 0.02f, 0.02f);
            Assert.AreEqual(26, cycle.Frame);
        }

        [Test]
        public void ZeroGameTimeFreezesTheCycle()
        {
            var cycle = Cycle();
            cycle.Step(0.05f, 0.02f, 0.02f);
            var frame = cycle.Frame;
            for (var i = 0; i < 20; i++) cycle.Step(0.5f, 0f, 0f);
            Assert.AreEqual(frame, cycle.Frame);
        }

        [Test]
        public void StoppingRunsOnToAFrameWithBothFeetDownAndHolds()
        {
            var cycle = Cycle();
            for (var i = 0; i < 17; i++) cycle.Step(0.05f, 0.02f, 0.02f); // frame 43 % 30 = 13
            Assert.IsFalse(cycle.AtRest);
            for (var i = 0; i < 60; i++) cycle.Step(0f, 0.016f, 0.016f);
            Assert.IsTrue(cycle.AtRest);
            var held = cycle.Frame;
            for (var i = 0; i < 20; i++) cycle.Step(0f, 0.016f, 0.016f);
            Assert.AreEqual(held, cycle.Frame);
        }

        [Test]
        public void SettlingIsQuickEnoughNotToLookLikeSlidingToAStop()
        {
            var cycle = Cycle();
            for (var i = 0; i < 17; i++) cycle.Step(0.05f, 0.02f, 0.02f);
            var time = 0f;
            while (!cycle.AtRest && time < 5f)
            {
                cycle.Step(0f, 0.016f, 0.016f);
                time += 0.016f;
            }

            Assert.Less(time, 0.35f);
        }
    }
}
