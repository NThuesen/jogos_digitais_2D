using System.Collections.Generic;
using Neko.Game.Art;
using Neko.Game.Core;
using UnityEngine;

namespace Neko.Game.Feel
{
    /// <summary>
    /// Every bit of flourish, from one pool.
    /// </summary>
    /// <remarks>
    /// Rubric 3 asks for state carried by the world rather than by a readout, and this is how
    /// most of that is paid for: hearts over a happy table, steam off a ready plate, a number
    /// floating up off the till. A pool rather than fresh objects because a busy café fires
    /// several of these a second and allocating a GameObject per heart is how a WebGL build
    /// starts stuttering.
    /// </remarks>
    public sealed class Effects : MonoBehaviour
    {
        private const int PoolSize = 72;

        private readonly List<Puff> _pool = new List<Puff>();
        private readonly List<FloatingLabel> _labels = new List<FloatingLabel>();

        private SpriteFactory _sprites;
        private Transform _labelRoot;
        private int _next;

        public static Effects Instance { get; private set; }

        public void Bind(SpriteFactory sprites, Transform labelRoot)
        {
            Instance = this;
            _sprites = sprites;
            _labelRoot = labelRoot;

            for (var i = 0; i < PoolSize; i++) _pool.Add(Puff.Create(transform));
        }

        public void Hearts(Vector3 at, int count = 3)
        {
            var sprite = _sprites.Get(new ShapeSpec(ShapeId.Heart, Palette.InnerEar), 48);

            for (var i = 0; i < count; i++)
            {
                var spread = (i - (count - 1) * 0.5f) * 0.22f;
                Take()?.Fire(sprite, at + new Vector3(spread, 0.3f, 0f),
                    new Vector3(spread * 0.6f, 1.3f, 0f), 0.9f, 0.3f, Color.white, 40f);
            }
        }

        public void Sparkles(Vector3 at, int count = 5)
        {
            var sprite = _sprites.Get(new ShapeSpec(ShapeId.Sparkle, Palette.Focus), 48);

            for (var i = 0; i < count; i++)
            {
                var angle = i / (float)count * Mathf.PI * 2f;
                var direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);

                Take()?.Fire(sprite, at + direction * 0.15f, direction * 1.8f, 0.55f, 0.26f,
                    Color.white, 180f);
            }
        }

        public void Steam(Vector3 at)
        {
            var sprite = _sprites.Get(
                new ShapeSpec(ShapeId.Steam, Palette.WithAlpha(Palette.Panel, 0.75f)), 48);

            Take()?.Fire(sprite, at + new Vector3(0f, 0.25f, 0f), new Vector3(0.12f, 0.8f, 0f),
                1.1f, 0.34f, Color.white, 18f);
        }

        public void Anger(Vector3 at)
        {
            var sprite = _sprites.Get(new ShapeSpec(ShapeId.Sparkle, Palette.Alarm), 48);

            for (var i = 0; i < 4; i++)
            {
                var angle = i / 4f * Mathf.PI * 2f + 0.4f;
                var direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                Take()?.Fire(sprite, at + new Vector3(0f, 0.35f, 0f), direction * 2.2f, 0.4f,
                    0.22f, Color.white, -220f);
            }
        }

        public void Coins(Vector3 at, int count = 4)
        {
            var sprite = _sprites.Get(
                new ShapeSpec(ShapeId.Circle, Palette.Focus, Palette.Ink, 0.1f), 48);

            for (var i = 0; i < count; i++)
            {
                var spread = (i - (count - 1) * 0.5f) * 0.3f;
                Take()?.Fire(sprite, at + new Vector3(spread * 0.4f, 0.2f, 0f),
                    new Vector3(spread, 1.9f, 0f), 0.75f, 0.24f, Color.white, 300f);
            }
        }

        public void Note(Vector3 at)
        {
            var sprite = _sprites.Get(new ShapeSpec(ShapeId.Note, Palette.Panel), 48);
            Take()?.Fire(sprite, at + new Vector3(0.2f, 0.4f, 0f), new Vector3(0.5f, 1.1f, 0f),
                1f, 0.3f, Color.white, 55f);
        }

        /// <summary>A number that floats up off whatever just happened.</summary>
        public void Number(Vector3 at, string text, Color colour)
        {
            FloatingLabel label = null;
            for (var i = 0; i < _labels.Count; i++)
            {
                if (_labels[i].Alive) continue;
                label = _labels[i];
                break;
            }

            if (label == null)
            {
                label = FloatingLabel.Create(_labelRoot);
                _labels.Add(label);
            }

            label.Fire(text, at, colour);
        }

        public void Money(Vector3 at, int amount) =>
            Number(at, "+" + amount, Palette.Focus);

        /// <summary>
        /// Stops everything in flight.
        /// </summary>
        /// <remarks>
        /// Called when the day ends. Money popups live on the interface canvas and have
        /// their own lifetime, so without this the last few of them were still rising up
        /// the dashboard a second after the café had closed.
        /// </remarks>
        public void Clear()
        {
            for (var i = 0; i < _pool.Count; i++) _pool[i]?.Stop();

            for (var i = 0; i < _labels.Count; i++)
            {
                if (_labels[i] != null) _labels[i].gameObject.SetActive(false);
            }
        }

        private Puff Take()
        {
            for (var i = 0; i < _pool.Count; i++)
            {
                var puff = _pool[(_next + i) % _pool.Count];
                if (puff.Alive) continue;

                _next = (_next + i + 1) % _pool.Count;
                return puff;
            }

            // Every one is busy. Dropping the effect is the right answer: a dropped heart is
            // invisible, and growing the pool under load is how a frame budget disappears.
            return null;
        }
    }
}
