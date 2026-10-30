using Neko.Sim.Core;
using Neko.Game.View;
using Neko.Sim.Maids;
using Neko.Sim.Progress;
using NUnit.Framework;
using UnityEngine;

namespace Neko.Tests.Game
{
    public sealed class SuzuSpriteTests
    {
        [TestCase("Suzu", true)]
        [TestCase("suzu", false)]
        [TestCase("Suzu ", false)]
        [TestCase("Aoi", false)]
        public void IdentityIsExactAndNeverInheritedFromArchetype(string name, bool expected)
        {
            Assert.AreEqual(expected, SuzuSprites.Supports(Maid.Create(1, name, Archetype.Genki), MaidPose.Idle));
            Assert.IsFalse(SuzuSprites.Supports(null, MaidPose.Idle));
        }

        [Test]
        public void UnsupportedPosesAndEveryEquipmentSlotFallBack()
        {
            var maid = Maid.Create(1, "Suzu", Archetype.Genki);
            Assert.IsFalse(SuzuSprites.Supports(maid, MaidPose.Cook));
            Assert.IsFalse(SuzuSprites.Supports(maid, MaidPose.Tired));
            for (var slot = 0; slot < maid.Gear.Length; slot++)
            {
                maid.Gear[slot] = new Equipment("test", "test", (GearSlot)slot, Stats.Zero,
                    ThemeTag.None, 0, 1, "Circle", "FFFFFF");
                Assert.IsFalse(SuzuSprites.Supports(maid, MaidPose.Idle));
                Assert.IsFalse(SuzuSprites.Supports(maid, MaidPose.Carry));
                maid.Gear[slot] = null;
            }
            Assert.IsTrue(SuzuSprites.Supports(maid, MaidPose.Idle));
        }

        [Test]
        public void SheetsSliceInOrderAndReuseFramesWithGameClockPacing()
        {
            var maid = Maid.Create(1, "Suzu", Archetype.Genki);
            using (var sprites = new SuzuSprites())
            {
                Sprite first = null;
                for (var i = 0; i < 8; i++)
                {
                    Assert.IsTrue(sprites.TryFrame(maid, MaidPose.Walk, true,
                        i == 0 ? 0f : SuzuSprites.WalkFrameSeconds + 0.00001f, out var frame));
                    if (i == 0) first = frame;
                    Assert.AreEqual(i * 72, frame.rect.x);
                    Assert.AreEqual(new Vector2(36, 0), frame.pivot);
                    Assert.AreEqual(80, frame.rect.height);
                    Assert.AreEqual(16, frame.pixelsPerUnit);
                    Assert.AreEqual(FilterMode.Point, frame.texture.filterMode);
                    Assert.IsTrue(sprites.TryFrame(maid, MaidPose.Walk, true, 0f, out var paused));
                    Assert.AreSame(frame, paused, "zero game delta must freeze the frame");
                }
                sprites.TryFrame(maid, MaidPose.Walk, true, SuzuSprites.WalkFrameSeconds, out var wrapped);
                Assert.AreSame(first, wrapped);
                sprites.TryFrame(maid, MaidPose.Carry, false, 10f, out var stationary);
                Assert.AreEqual("tray_idle_0", stationary.name);
                sprites.TryFrame(maid, MaidPose.Carry, true, 0f, out var carrying);
                Assert.AreEqual("walk_tray_0", carrying.name);
                sprites.TryFrame(maid, MaidPose.Idle, false, 0f, out var idle);
                Assert.AreEqual("suzu_idle_sheet_0", idle.name);
                sprites.TryFrame(maid, MaidPose.Idle, false, 0.25f, out idle);
                Assert.AreEqual("suzu_idle_sheet_1", idle.name);
            }
        }
    }
}
