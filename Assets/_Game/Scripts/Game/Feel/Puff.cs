using Neko.Game.Art;
using Neko.Game.Core;
using UnityEngine;

namespace Neko.Game.Feel
{
    /// <summary>
    /// One short-lived sprite that drifts, spins and fades. Hearts, sparkles, steam, coins.
    /// </summary>
    /// <remarks>
    /// Pooled quads using the sprite factory rather than Unity's particle system, which keeps
    /// the "no imported assets" rule intact — a ParticleSystem needs a material, and a
    /// material is an asset.
    /// </remarks>
    public sealed class Puff : MonoBehaviour
    {
        private SpriteRenderer _renderer;
        private Vector3 _velocity;
        private float _life;
        private float _age;
        private float _spin;
        private float _startScale;

        public bool Alive => _age < _life;

        public static Puff Create(Transform parent)
        {
            var go = new GameObject("Puff");
            go.transform.SetParent(parent, false);

            var puff = go.AddComponent<Puff>();
            puff._renderer = go.AddComponent<SpriteRenderer>();
            go.SetActive(false);
            return puff;
        }

        public void Fire(Sprite sprite, Vector3 at, Vector3 velocity, float life, float scale,
            Color tint, float spin = 0f)
        {
            _renderer.sprite = sprite;
            _renderer.color = tint;
            Layers.Apply(_renderer, Layers.Fx, at.y);

            transform.position = at;
            transform.localScale = Vector3.one * scale;
            transform.rotation = Quaternion.identity;

            _velocity = velocity;
            _life = Mathf.Max(0.05f, life);
            _age = 0f;
            _spin = spin;
            _startScale = scale;

            gameObject.SetActive(true);
        }

        public void Stop() => gameObject.SetActive(false);

        private void Update()
        {
            var dt = Clocks.Ui;
            _age += dt;

            if (_age >= _life)
            {
                gameObject.SetActive(false);
                return;
            }

            var t = _age / _life;

            transform.position += _velocity * dt;
            if (!Mathf.Approximately(_spin, 0f))
            {
                transform.Rotate(0f, 0f, _spin * dt);
            }

            // Pops out, then shrinks away. The pop is what makes it read at a glance.
            var scale = _startScale * (t < 0.2f ? Ease.OutBack(t / 0.2f) : 1f - (t - 0.2f) * 0.5f);
            transform.localScale = Vector3.one * scale;

            var colour = _renderer.color;
            _renderer.color = new Color(colour.r, colour.g, colour.b,
                t < 0.6f ? 1f : 1f - (t - 0.6f) / 0.4f);
        }
    }
}
