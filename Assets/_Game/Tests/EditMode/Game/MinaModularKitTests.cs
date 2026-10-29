using System.Linq;
using Neko.Game.View;
using NUnit.Framework;
using UnityEngine;

namespace Neko.Tests.EditMode.Game
{
    public sealed class MinaModularKitTests
    {
        private MinaModularKit _kit;
        [SetUp] public void SetUp() => _kit = MinaModularKit.Create();
        [TearDown] public void TearDown()
        {
            if (_kit != null) Object.DestroyImmediate(_kit.gameObject);
        }

        [Test]
        public void EveryPartHasOneRendererAndEveryJointUsesItsDeclaredParent()
        {
            Assert.That(_kit.RendererCount, Is.EqualTo(_kit.Data.parts.Length));
            foreach (var node in _kit.Data.nodes.Where(n => !string.IsNullOrEmpty(n.parent)))
                Assert.That(_kit.Joint(node.id).parent, Is.EqualTo(_kit.Joint(node.parent)), node.id);
            Assert.That(_kit.Joint("headwear_socket").IsChildOf(_kit.Joint("head")), Is.True);
            Assert.That(_kit.Joint("root").localPosition, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void PosesMoveDistalLimbsWithoutChangingRootScaleOrSegmentLengths()
        {
            var root = _kit.Joint("root").position;
            var hand = _kit.Joint("near_hand").position;
            var foot = _kit.Joint("near_foot").position;
            var offsets = _kit.Data.nodes.ToDictionary(n => n.id, n => _kit.Joint(n.id).localPosition);
            _kit.ApplyPose(1);
            Assert.That(Vector3.Distance(foot, _kit.Joint("near_foot").position), Is.GreaterThan(.02f));
            _kit.ApplyPose(3);
            Assert.That(Vector3.Distance(hand, _kit.Joint("near_hand").position), Is.GreaterThan(.02f));
            foreach (var pose in Enumerable.Range(0, _kit.Data.poses.Length))
            {
                _kit.ApplyPose(pose);
                Assert.That(_kit.Joint("root").position, Is.EqualTo(root));
                foreach (var node in _kit.Data.nodes)
                {
                    Assert.That(_kit.Joint(node.id).localPosition, Is.EqualTo(offsets[node.id]), node.id);
                    Assert.That(_kit.Joint(node.id).localScale, Is.EqualTo(Vector3.one), node.id);
                }
            }
        }

        [Test]
        public void NeutralResetsAllJointsAndDefinitionRoundTripsWithoutSceneReferences()
        {
            _kit.ApplyPose(4);
            _kit.ApplyPose(0);
            foreach (var node in _kit.Data.nodes)
                Assert.That(Quaternion.Angle(_kit.Joint(node.id).localRotation, Quaternion.identity), Is.LessThan(.001f));
            var copy = JsonUtility.FromJson<MinaModularKit.Definition>(JsonUtility.ToJson(_kit.Data));
            Assert.That(copy.parts.Select(p => p.id), Is.EqualTo(_kit.Data.parts.Select(p => p.id)));
            Assert.That(copy.poses.Length, Is.EqualTo(7));
        }

        [Test]
        public void RebuildingDoesNotDependOnPreviousPoseOrStaticInstance()
        {
            _kit.ApplyPose(6);
            Object.DestroyImmediate(_kit.gameObject);
            _kit = MinaModularKit.Create();
            Assert.That(_kit.Joint("near_forearm").localRotation, Is.EqualTo(Quaternion.identity));
            Assert.That(_kit.RendererCount, Is.EqualTo(_kit.Data.parts.Length));
        }
    }
}
