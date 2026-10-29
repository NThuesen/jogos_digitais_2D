using System;
using System.Collections.Generic;
using UnityEngine;

namespace Neko.Game.View
{
    /// <summary>Gate A only. Source-pixel bind frame, explicit joints, no gameplay binding.</summary>
    public sealed class MinaModularKit : MonoBehaviour
    {
        [Serializable] public sealed class Node
        {
            public string id, parent;
            public float x, y;
        }

        [Serializable] public sealed class Part
        {
            public string id, node, sha256;
            public int order, x, y, width, height, reconstructedCaps;
        }

        [Serializable] public sealed class Angle { public string node; public float degrees; }
        [Serializable] public sealed class Pose { public string id, label; public Angle[] angles; }
        [Serializable] public sealed class Definition
        {
            public int version, width, height;
            public string source;
            public float pixelsPerUnit, floorX, floorY;
            public Node[] nodes;
            public Part[] parts;
            public Pose[] poses;
        }

        private readonly Dictionary<string, Transform> _joints = new Dictionary<string, Transform>();
        private readonly List<Sprite> _ownedSprites = new List<Sprite>();
        public Definition Data { get; private set; }
        public int RendererCount => GetComponentsInChildren<SpriteRenderer>().Length;
        public Transform Joint(string id) => _joints[id];

        public static MinaModularKit Create(Transform parent = null)
        {
            var root = new GameObject("Mina Gate A — modular side kit");
            root.transform.SetParent(parent, false);
            var kit = root.AddComponent<MinaModularKit>();
            try { kit.Build(); }
            catch
            {
                if (Application.isPlaying) Destroy(root); else DestroyImmediate(root);
                throw;
            }
            return kit;
        }

        private void Build()
        {
            var json = Resources.Load<TextAsset>("Anim01GateA/rig");
            if (json == null) throw new InvalidOperationException("Missing Gate A rig.json.");
            Data = JsonUtility.FromJson<Definition>(json.text);
            if (Data == null || Data.version != 1 || Data.pixelsPerUnit <= 0f ||
                Data.nodes == null || Data.parts == null || Data.poses == null)
                throw new InvalidOperationException("Invalid Gate A definition.");

            foreach (var node in Data.nodes)
            {
                if (_joints.ContainsKey(node.id)) throw new InvalidOperationException("Duplicate joint " + node.id);
                var joint = new GameObject(node.id).transform;
                var parent = string.IsNullOrEmpty(node.parent) ? transform : _joints[node.parent];
                joint.SetParent(parent, false);
                var originX = Data.floorX;
                var originY = Data.floorY;
                if (!string.IsNullOrEmpty(node.parent))
                {
                    var bindParent = Array.Find(Data.nodes, n => n.id == node.parent);
                    originX = bindParent.x;
                    originY = bindParent.y;
                }
                joint.localPosition = new Vector3((node.x - originX) / Data.pixelsPerUnit,
                    (originY - node.y) / Data.pixelsPerUnit, 0f);
                _joints.Add(node.id, joint);
            }

            foreach (var part in Data.parts)
            {
                var texture = Resources.Load<Texture2D>("Anim01GateA/" + part.id);
                if (texture == null || texture.width != part.width || texture.height != part.height)
                    throw new InvalidOperationException("Missing or resized Gate A part " + part.id);
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(.5f, .5f), Data.pixelsPerUnit, 0, SpriteMeshType.FullRect);
                sprite.name = "GateA_" + part.id;
                _ownedSprites.Add(sprite);
                var renderer = new GameObject("sprite_" + part.id).AddComponent<SpriteRenderer>();
                renderer.transform.SetParent(_joints[part.node], false);
                var bind = Array.Find(Data.nodes, n => n.id == part.node);
                renderer.transform.localPosition = new Vector3(
                    (part.x + part.width * .5f - bind.x) / Data.pixelsPerUnit,
                    (bind.y - part.y - part.height * .5f) / Data.pixelsPerUnit, 0f);
                renderer.sprite = sprite;
                renderer.sortingOrder = part.order;
            }
        }

        public void ApplyPose(int index)
        {
            if (index < 0 || index >= Data.poses.Length) throw new ArgumentOutOfRangeException(nameof(index));
            foreach (var joint in _joints.Values) joint.localRotation = Quaternion.identity;
            foreach (var angle in Data.poses[index].angles)
                _joints[angle.node].localRotation = Quaternion.Euler(0f, 0f, angle.degrees);
        }

        private void OnDestroy()
        {
            foreach (var sprite in _ownedSprites)
                if (sprite != null)
                {
                    if (Application.isPlaying) Destroy(sprite); else DestroyImmediate(sprite);
                }
            _ownedSprites.Clear();
        }
    }
}
