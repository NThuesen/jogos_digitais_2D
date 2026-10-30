using System;
using Neko.Sim.Maids;
using UnityEngine;

namespace Neko.Game.View
{
    /// <summary>Partial chibi slice. Exact-name identity is temporary, never saved as a skin ID.</summary>
    public sealed class SuzuSprites : IDisposable
    {
        public const float Scale = 0.25f;
        // Match the procedural shoes' ground line; sheet padding remains inside the pivot.
        public static readonly Vector3 BodyOffset = new Vector3(0f, -0.69f, 0f);
        public const float WalkFrameSeconds = 0.11f;
        private Sprite[] _idle, _walk, _carry, _carryIdle;
        private MaidPose _pose;
        private bool _moving;
        private float _elapsed;

        public static bool Supports(Maid maid, MaidPose pose)
        {
            if (maid == null || !string.Equals(maid.Name, "Suzu", StringComparison.Ordinal)) return false;
            if (pose != MaidPose.Idle && pose != MaidPose.Walk && pose != MaidPose.Carry) return false;
            // No grounded mapping from equipment IDs to the authored outfit/accessory yet.
            foreach (var gear in maid.Gear) if (gear != null) return false;
            return true;
        }

        public bool TryFrame(Maid maid, MaidPose pose, bool moving, float seconds, out Sprite sprite)
        {
            sprite = null;
            if (!Supports(maid, pose))
            {
                _elapsed = 0f;
                return false;
            }

            if (_idle == null)
            {
                try { Load(); }
                catch { Dispose(); throw; }
            }
            if (_pose != pose || _moving != moving) _elapsed = 0f;
            _pose = pose;
            _moving = moving;
            var walking = moving && (pose == MaidPose.Walk || pose == MaidPose.Carry);
            var frames = pose == MaidPose.Carry ? (walking ? _carry : _carryIdle) :
                walking ? _walk : _idle;
            var interval = walking ? WalkFrameSeconds : 0.25f;
            _elapsed = (_elapsed + Mathf.Max(0f, seconds)) % (frames.Length * interval);
            sprite = frames[Mathf.FloorToInt(_elapsed / interval) % frames.Length];
            return true;
        }

        private void Load()
        {
            // Texture assets are included by Resources in players, including WebGL.
            // Slice once per view, never read pixels, and release only the owned sprites.
            _idle = Slice("suzu_idle_sheet", 4);
            _walk = Slice("suzu_walk_sheet", 8);
            _carry = Slice("walk_tray", 8);
            _carryIdle = Slice("tray_idle", 1);
        }

        private static Sprite[] Slice(string name, int count)
        {
            var texture = Resources.Load<Texture2D>("Suzu/" + name);
            if (texture == null || texture.width != 72 * count || texture.height != 80)
                throw new InvalidOperationException("Missing or invalid Suzu sheet: " + name);
            var frames = new Sprite[count];
            for (var i = 0; i < count; i++)
            {
                frames[i] = Sprite.Create(texture, new Rect(i * 72, 0, 72, 80),
                    new Vector2(0.5f, 0f), 16f, 0, SpriteMeshType.FullRect);
                frames[i].name = name + "_" + i;
            }
            return frames;
        }

        public void Dispose()
        {
            Release(_idle); Release(_walk); Release(_carry); Release(_carryIdle);
            _idle = _walk = _carry = _carryIdle = null;
        }

        private static void Release(Sprite[] frames)
        {
            if (frames == null) return;
            foreach (var frame in frames)
            {
                if (Application.isPlaying) UnityEngine.Object.Destroy(frame);
                else UnityEngine.Object.DestroyImmediate(frame);
            }
        }
    }
}
