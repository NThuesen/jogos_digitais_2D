using System;
using System.Collections.Generic;
using Neko.Game.Core;
using UnityEngine;

namespace Neko.Game.Feel
{
    /// <summary>
    /// A very small tween runner.
    /// </summary>
    /// <remarks>
    /// Hand-rolled rather than a package, for the same reason the art is: this project ships
    /// with no third-party assets, and what it needs from a tween library is two hundred
    /// lines. Everything runs on the UI clock so a pause does not leave a button half-way
    /// through its press.
    /// </remarks>
    public sealed class Tween
    {
        private static readonly List<Tween> Live = new List<Tween>();
        private static readonly List<Tween> Finished = new List<Tween>();

        private readonly Transform _transform;
        private readonly Action<float> _apply;
        private readonly Func<float, float> _ease;
        private readonly float _seconds;

        private float _elapsed;
        private Action _onDone;
        private bool _dead;

        private Tween(Transform transform, float seconds, Func<float, float> ease,
            Action<float> apply)
        {
            _transform = transform;
            _seconds = Mathf.Max(0.0001f, seconds);
            _ease = ease ?? Ease.Linear;
            _apply = apply;
            Live.Add(this);
        }

        /// <summary>Advances every live tween. Called once a frame by the game root.</summary>
        public static void TickAll()
        {
            var dt = Clocks.Ui;

            for (var i = 0; i < Live.Count; i++)
            {
                var tween = Live[i];
                if (tween._dead || (tween._transform == null && tween._apply == null))
                {
                    Finished.Add(tween);
                    continue;
                }

                tween._elapsed += dt;
                var t = Mathf.Clamp01(tween._elapsed / tween._seconds);
                tween._apply?.Invoke(tween._ease(t));

                if (t < 1f) continue;

                Finished.Add(tween);
                tween._onDone?.Invoke();
            }

            if (Finished.Count == 0) return;

            for (var i = 0; i < Finished.Count; i++) Live.Remove(Finished[i]);
            Finished.Clear();
        }

        public static int LiveCount => Live.Count;

        /// <summary>Drops everything. Used when a screen is torn down.</summary>
        public static void KillAll()
        {
            Live.Clear();
            Finished.Clear();
        }

        public static Tween Scale(Transform target, Vector3 from, Vector3 to, float seconds,
            Func<float, float> ease = null)
        {
            if (target == null) return null;

            return new Tween(target, seconds, ease ?? Ease.OutBack,
                t =>
                {
                    if (target != null) target.localScale = Vector3.LerpUnclamped(from, to, t);
                });
        }

        /// <summary>The squash a thing does when it is picked up or put down.</summary>
        public static Tween Punch(Transform target, float amount = 0.22f, float seconds = 0.26f)
        {
            if (target == null) return null;

            var baseScale = target.localScale;
            return new Tween(target, seconds, Ease.Pop,
                t =>
                {
                    if (target == null) return;
                    target.localScale = new Vector3(
                        baseScale.x * (1f + amount * t),
                        baseScale.y * (1f - amount * 0.6f * t),
                        baseScale.z);
                }).Then(() =>
                {
                    if (target != null) target.localScale = baseScale;
                });
        }

        public static Tween Move(Transform target, Vector3 from, Vector3 to, float seconds,
            Func<float, float> ease = null)
        {
            if (target == null) return null;

            return new Tween(target, seconds, ease ?? Ease.OutCubic,
                t =>
                {
                    if (target != null) target.position = Vector3.LerpUnclamped(from, to, t);
                });
        }

        public static Tween Fade(SpriteRenderer renderer, float from, float to, float seconds,
            Func<float, float> ease = null)
        {
            if (renderer == null) return null;

            return new Tween(renderer.transform, seconds, ease ?? Ease.Linear,
                t =>
                {
                    if (renderer == null) return;
                    var colour = renderer.color;
                    renderer.color = new Color(colour.r, colour.g, colour.b,
                        Mathf.LerpUnclamped(from, to, t));
                });
        }

        public static Tween Value(float from, float to, float seconds, Action<float> apply,
            Func<float, float> ease = null) =>
            new Tween(null, seconds, ease ?? Ease.OutCubic,
                t => apply?.Invoke(Mathf.LerpUnclamped(from, to, t)));

        public Tween Then(Action done)
        {
            _onDone = done;
            return this;
        }

        public void Kill() => _dead = true;
    }
}
