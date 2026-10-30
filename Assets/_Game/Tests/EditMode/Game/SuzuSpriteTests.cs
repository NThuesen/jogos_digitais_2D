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
    public sealed class SuzuSpriteTests
    {
        private SpriteFactory _factory;

        [SetUp]
        public void Setup() => _factory = new SpriteFactory();

        [TearDown]
        public void Teardown() => _factory.Clear();

        private static Maid Maid(string name, Archetype archetype) =>
            new Maid { Id = 1, Name = name, Archetype = archetype };

        [Test]
        public void OnlySuzuTheGenkiGetsTheAuthoredFrames()
        {
            Assert.IsTrue(SuzuSprites.Applies(Maid("Suzu", Archetype.Genki)));
            Assert.IsFalse(SuzuSprites.Applies(Maid("Suzu", Archetype.Kuudere)), "another Suzu from the draw");
            Assert.IsFalse(SuzuSprites.Applies(Maid("Mina", Archetype.Genki)));
            Assert.IsFalse(SuzuSprites.Applies(null));
        }

        [Test]
        public void AnyEquipmentKeepsTheLayeredLookSoGearStaysVisible()
        {
            var maid = Maid("Suzu", Archetype.Genki);
            maid.Gear[0] = new Equipment("x", "x", GearSlot.Outfit, Stats.Zero,
                Neko.Sim.Core.ThemeTag.None, 0, 1, "Apron", "FF0000");
            Assert.IsFalse(SuzuSprites.Applies(maid));
        }

        [Test]
        public void TheSheetCutsIntoTheDeclaredFramesWithFeetAtTheBottomCentre()
        {
            var frames = SuzuSprites.Load(_factory);
            Assert.IsNotNull(frames, "Resources/Suzu/suzu_walk_sheet.png is missing or the wrong size");
            Assert.AreEqual(SuzuSprites.Frames, frames.Length);
            Assert.AreEqual(new Vector2(SuzuSprites.CellWidth, SuzuSprites.CellHeight), frames[0].rect.size);
            Assert.AreEqual(new Vector2(0.5f, 0f), frames[0].pivot / frames[0].rect.size);
            foreach (var index in SuzuSprites.RestFrames) Assert.Less(index, SuzuSprites.Frames);
            Assert.Contains(SuzuSprites.IdleFrame, SuzuSprites.RestFrames);
        }

        [Test]
        public void TheSheetIsImportedPointFilteredAndNonReadable()
        {
            var texture = Resources.Load<Texture2D>(SuzuSprites.Resource);
            Assert.IsNotNull(texture);
            Assert.IsFalse(texture.isReadable);
            Assert.AreEqual(FilterMode.Point, texture.filterMode);
        }

        [Test]
        public void TheConstantsMatchWhatTheProcessingToolWrote()
        {
            var path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "docs", "art", "suzu-walk",
                "manifest.json"));
            var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(path));
            Assert.AreEqual(SuzuSprites.Frames, manifest.frames);
            Assert.AreEqual(SuzuSprites.Columns, manifest.cols);
            Assert.AreEqual(SuzuSprites.Rows, manifest.rows);
            Assert.AreEqual(SuzuSprites.CellWidth, manifest.cell[0]);
            Assert.AreEqual(SuzuSprites.CellHeight, manifest.cell[1]);
            Assert.AreEqual(SuzuSprites.IdleFrame, manifest.idle_frame);
            CollectionAssert.AreEqual(SuzuSprites.RestFrames.OrderBy(i => i).ToArray(),
                manifest.settle_frames.OrderBy(i => i).ToArray());
        }

        [System.Serializable]
        private sealed class Manifest
        {
            public int frames, cols, rows, idle_frame;
            public int[] cell, settle_frames;
        }

        [Test]
        public void AMissingSheetIsNotAnError()
        {
            Assert.IsNull(_factory.Sheet("Suzu/does_not_exist", 1, 1, 1, 1, 1, 96f));
        }
    }

    public sealed class WalkCycleTests
    {
        private static WalkCycle Cycle() => new WalkCycle(30, 1.5f, new[] { 10, 11, 26 }, 26);

        [Test]
        public void FramesFollowDistanceNotTime()
        {
            var cycle = Cycle();
            cycle.Step(0.05f, 0.016f); // 0.05 / 1.5 * 30 = exactly one frame
            Assert.AreEqual(27, cycle.Frame);
            Assert.IsTrue(cycle.Moving);
        }

        [Test]
        public void ACompleteCycleOfDistanceReturnsToTheSameFrame()
        {
            var cycle = Cycle();
            for (var i = 0; i < 30; i++) cycle.Step(0.05f, 0.016f);
            Assert.AreEqual(26, cycle.Frame);
        }

        [Test]
        public void ZeroGameTimeFreezesTheCycle()
        {
            var cycle = Cycle();
            cycle.Step(0.05f, 0.016f);
            var frame = cycle.Frame;
            for (var i = 0; i < 20; i++) cycle.Step(0.5f, 0f);
            Assert.AreEqual(frame, cycle.Frame);
        }

        [Test]
        public void StoppingRunsOnToAFrameWithBothFeetDownAndHolds()
        {
            var cycle = Cycle();
            for (var i = 0; i < 17; i++) cycle.Step(0.05f, 0.016f); // frame 43 % 30 = 13
            Assert.IsFalse(cycle.AtRest);
            for (var i = 0; i < 60; i++) cycle.Step(0f, 0.016f);
            Assert.IsTrue(cycle.AtRest);
            var held = cycle.Frame;
            for (var i = 0; i < 20; i++) cycle.Step(0f, 0.016f);
            Assert.AreEqual(held, cycle.Frame);
        }

        [Test]
        public void SettlingIsQuickEnoughNotToLookLikeSlidingToAStop()
        {
            var cycle = Cycle();
            for (var i = 0; i < 17; i++) cycle.Step(0.05f, 0.016f);
            var time = 0f;
            while (!cycle.AtRest && time < 5f)
            {
                cycle.Step(0f, 0.016f);
                time += 0.016f;
            }

            Assert.Less(time, 0.35f);
        }
    }
}
