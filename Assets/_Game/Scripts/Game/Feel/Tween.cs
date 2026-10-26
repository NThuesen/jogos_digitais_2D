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
        public static void TickAll() => TickAll(Clocks.Ui);

        /// <summary>
        /// Advances every live tween by one frame.
        /// </summary>
        /// <remarks>
        /// The delta is a parameter so a test can pin what happens on a long frame, which is
        /// the case that killed the editor and the one a normal frame never reaches.
        /// </remarks>
        public static void TickAll(float dt)
        {
            // A completion is allowed to start another tween — the menu's sign sways by
            // reversing itself forever — and a new tween appends to this very list. Walking
            // it to its live end therefore advanced the replacement inside the frame that
            // created it, and on a frame longer than the sway the replacement finished too,
            // and started another. The editor's first frame of play mode carries the domain
            // reload, which this log times at 3.9 s, against a 2.6 s sway — so every link
            // completed at once: ten thousand of them in a single pass, counted by
            // TweenChainTests. The loop never returned, and the editor grew from 1.8 GB to
            // 10.4 GB before the kernel killed it, with nothing in the log after "Loaded
            // scene". A tween born this frame starts moving on the next one.
            var countAtFrameStart = Live.Count;

            // The second condition is for a list that shrinks under the loop: KillAll clears
            // it, and a snapshot taken before that would index past the end. Nothing calls
            // KillAll from inside a tween today — only GameRoot.OnDestroy does — so this
            // costs a comparison to close a trap rather than to fix a crash.
            for (var i = 0; i < countAtFrameStart && i < Live.Count; i++)
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
