using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Game.Feel;
using Neko.Sim.Maids;
using Neko.Sim.Shift;
using UnityEngine;

namespace Neko.Game.View
{
    /// <summary>
    /// One maid on the floor, following her worker in the simulation.
    /// </summary>
    /// <remarks>
    /// The view never decides anything. It reads a position and a task and picks a pose; if a
    /// maid looks like she is doing the wrong thing, the bug is in the simulation and can be
    /// reproduced in a headless test. That split is why a game with this many moving parts can
    /// be debugged at all.
    /// </remarks>
    public sealed class MaidView : MonoBehaviour
    {
        private SpriteFactory _sprites;
        private SpriteRenderer _body;
        private SpriteRenderer _carried;
        private SpriteRenderer _accessory;
        private UnityEngine.Rendering.SortingGroup _bodyGroup;
        private bool _suzu;
        private bool _mina;
        private string _minaName;
        private MinaFacing _heading = new MinaFacing();

        /// <summary>V1 only: step phase in half-turns per stride, its fade, and the body lean.</summary>
        private float _stride;
        private float _motion;
        private float _tilt;
        private Vector2 _squash = Vector2.one;
        private SpriteRenderer _mood;
        private Maid _maid;
        private MaidPose _pose = MaidPose.Idle;
        private float _bob;
        private float _lift;
        private Vector3 _lastPosition;

        /// <summary>
        /// Her own small offset from wherever the simulation says she is.
        /// </summary>
        /// <remarks>
        /// Two maids legitimately end up on the same tile — the greeter walks a party to a
        /// table and the waitress brings the plate to it — and drawn at exactly the same
        /// point they read as one person. Spread in the view rather than in the shift, so
        /// the simulation stays exact and only the picture is nudged. Angles step by the
        /// golden angle, which keeps any two of them a quarter of a tile apart.
        /// </remarks>
        private Vector3 _offset;

        public Worker Worker { get; private set; }

        public static MaidView Create(Transform parent, SpriteFactory sprites, Worker worker)
        {
            var go = new GameObject("Maid");
            go.transform.SetParent(parent, false);

            var view = go.AddComponent<MaidView>();
            view._sprites = sprites;

            // The sprite lives on a child, not on the root. The walk bob writes
            // localPosition on the body every frame, and with the renderer on the root
            // that overwrote the maid's own position with (0, lift, 0) — every maid in the
            // café was drawn standing in the bottom-left corner of the room.
            var body = new GameObject("Body");
            body.transform.SetParent(go.transform, false);
            view._body = body.AddComponent<SpriteRenderer>();
            view._bodyGroup = body.AddComponent<UnityEngine.Rendering.SortingGroup>();
            view._bodyGroup.enabled = false;

            var accessory = new GameObject("Accessory");
            accessory.transform.SetParent(body.transform, false);
            accessory.transform.localPosition = new Vector3(0f, -0.20f, 0f);
            accessory.transform.localScale = Vector3.one * 0.18f;
            view._accessory = accessory.AddComponent<SpriteRenderer>();

            var carried = new GameObject("Carried");
            carried.transform.SetParent(go.transform, false);
            carried.transform.localPosition = new Vector3(0f, 0.18f, 0f);
            carried.transform.localScale = Vector3.one * 0.5f;
            view._carried = carried.AddComponent<SpriteRenderer>();
            view._carried.enabled = false;

            var mood = new GameObject("Mood");
            mood.transform.SetParent(go.transform, false);
            mood.transform.localPosition = new Vector3(0f, 0.78f, 0f);
            mood.transform.localScale = new Vector3(0.52f, 0.1f, 1f);
            view._mood = mood.AddComponent<SpriteRenderer>();
            view._mood.sprite = sprites.Solid(Color.white);

            view.Bind(worker);
            return view;
        }

        public void Bind(Worker worker)
        {
            Worker = worker;
            _maid = worker.Maid;
            _heading = new MinaFacing();
            _minaName = null;
            _stride = _motion = _tilt = _lift = 0f;
            _squash = Vector2.one;

            const float radius = 0.5f;
            var angle = worker.Id * 137.5f * Mathf.Deg2Rad;
            _offset = new Vector3(Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius * 0.7f, 0f);

            Redraw();

            transform.position = new Vector3(worker.X, worker.Y, 0f) + _offset;
            _lastPosition = transform.position;

            // Pops in rather than appearing. Nothing in this café simply exists.
            transform.localScale = Vector3.zero;
            Tween.Scale(transform, Vector3.zero, Vector3.one, 0.35f, Ease.OutBack);
        }

        public void Sync()
        {
            if (Worker == null) return;

            var target = new Vector3(Worker.X, Worker.Y, 0f) + _offset;
            var delta = target - _lastPosition;
            var stepped = delta.sqrMagnitude > MinaFacing.StillSquared;
            var moving = stepped;
            var mina = MinaVisual.Matches(_maid);
            if (mina)
            {
                _heading.Step(delta.x, delta.y, Clocks.Game);
                moving = _heading.Moving;
            }

            // Smoothed rather than snapped: the simulation ticks in whole frames and a worker
            // crossing a tile in two of them looks like a teleport otherwise.
            transform.position = Vector3.Lerp(transform.position, target, 1f - Mathf.Exp(-18f * Clocks.Ui));
            _lastPosition = target;

            var pose = PoseFor(moving);
            // Redrawn only when the drawing itself changes, never every frame of a stable walk.
            if (pose != _pose || (mina && MinaVisual.NameFor(pose, _heading.Facing, moving) != _minaName))
            {
                _pose = pose;
                Redraw();
            }

            if (_mina && MinaVisual.Motion == MinaVisual.Variant.V1) Step(delta, stepped);
            else
            {
                // A walk bob. Four lines, and the difference between a sprite sliding and a
                // person walking.
                _bob = moving ? _bob + Clocks.Game * 11f : 0f;
                _lift = moving ? Mathf.Abs(Mathf.Sin(_bob)) * 0.07f : 0f;
                _tilt = 0f;
                _squash = Vector2.one;
            }
            _body.transform.localPosition = new Vector3(0f, _lift, 0f);

            if (_mina) _body.flipX = _heading.Left && MinaVisual.Mirrors(_minaName);
            else if (target.x - transform.position.x > 0.02f) _body.flipX = false;
            else if (target.x - transform.position.x < -0.02f) _body.flipX = true;

            SyncScale();
            Layers.Apply(_body, Layers.Characters, transform.position.y);
            if (_suzu)
            {
                _bodyGroup.sortingOrder = _body.sortingOrder;
                _body.sortingOrder = 0;
            }
            _accessory.sortingLayerID = _body.sortingLayerID;
            _accessory.sortingOrder = _body.sortingOrder + 1;
            Layers.Apply(_carried, Layers.Items, transform.position.y - 0.01f);
            Layers.Apply(_mood, Layers.Fx, transform.position.y);

            _carried.enabled = Worker.Carrying && !(_mina && _pose == MaidPose.Carry);
            SyncMood();
        }

        public void ShowCarried(Sprite sprite)
        {
            _carried.sprite = sprite;
            _carried.enabled = sprite != null && !(_mina && _pose == MaidPose.Carry);
            if (sprite != null) Tween.Punch(_carried.transform, 0.3f, 0.2f);
        }

        /// <summary>Squash on the spot. Used when she finishes something.</summary>
        public void Pop() => Tween.Punch(transform, 0.18f, 0.24f);

        private void SyncMood()
        {
            if (_maid == null)
            {
                _mood.enabled = false;
                return;
            }

            // Only shown once it matters. A full bar over every head all day is noise.
            var tired = Worker.OnBreak || _maid.Mood < 0.45f;
            _mood.enabled = tired;
            if (!tired) return;

            _mood.transform.localScale = new Vector3(0.52f * Mathf.Max(0.08f, _maid.Mood), 0.1f, 1f);
            _mood.color = Worker.OnBreak ? Palette.TextDim : Palette.ForPatience(_maid.Mood);
        }

        private MaidPose PoseFor(bool moving)
        {
            if (Worker.OnBreak) return MaidPose.Tired;
            if (_maid != null && _maid.Mood < 0.2f) return MaidPose.Tired;
            if (Worker.Carrying) return MaidPose.Carry;

            var task = Worker.Claimed;
            if (task != null && task.Kind == TaskKind.Cook) return MaidPose.Cook;

            return moving ? MaidPose.Walk : MaidPose.Idle;
        }

        /// <summary>
        /// V1: a step driven by the distance actually walked rather than by the clock.
        /// </summary>
        /// <remarks>
        /// A stride of a little under half a tile is one half-turn of the phase, so the lift
        /// peaks once per footfall and the body rocks to alternate sides. Faster maids step
        /// faster instead of hopping on the spot. The amplitude fades in and out over the same
        /// time as the movement hold, so the swap to the idle drawing lands with her at rest.
        /// The pivot is at her feet: lean and squash never move where she stands.
        /// </remarks>
        private void Step(Vector3 delta, bool stepped)
        {
            const float strideLength = 0.45f;
            _motion = Mathf.MoveTowards(_motion, stepped ? 1f : 0f, Clocks.Game / MinaFacing.MovementHold);
            _stride += new Vector2(delta.x, delta.y).magnitude / strideLength * Mathf.PI;

            var swing = Mathf.Sin(_stride);
            var planted = 1f - Mathf.Abs(swing);
            _lift = Mathf.Abs(swing) * 0.045f * _motion;

            var lean = _heading.Facing == MinaVisual.Facing.Side ? (_heading.Left ? 2f : -2f) : 0f;
            _tilt = (lean + swing * 1.5f) * _motion;

            var squash = 0.025f * planted * _motion;
            _squash = new Vector2(1f + squash, 1f - squash);
        }

        /// <summary>For the Editor evidence run only: what the view decided this frame.</summary>
        public MaidPose Pose => _pose;

        public MinaFacing Heading => _heading;

        private void Redraw()
        {
            _minaName = MinaVisual.Matches(_maid)
                ? MinaVisual.NameFor(_pose, _heading.Facing, _heading.Moving) : null;
            var mina = _minaName != null ? MinaVisual.Chibi(_maid, _minaName) : null;
            _mina = mina != null;
            var suzu = _mina ? null : SuzuVisual.Chibi(_maid, _pose);
            _suzu = suzu != null;
            _bodyGroup.enabled = _suzu;
            _body.sprite = _mina ? mina : suzu != null ? suzu : _sprites.Layered(
                MaidSpriteBuilder.Layers(_maid, _pose), MaidSpriteBuilder.CacheKey(_maid, _pose), 128);
            _accessory.sprite = suzu != null ? SuzuVisual.Accessory(_maid, _sprites) : null;
            SyncScale();
        }

        private void SyncScale()
        {
            // 96 pixels at the 720-pixel reference height, before visual review.
            var camera = Camera.main;
            var targetHeight = camera != null && camera.orthographic
                ? 96f * 2f * camera.orthographicSize / 720f : 128f / 96f;
            if (_mina)
            {
                var scale = targetHeight / _body.sprite.bounds.size.y;
                _body.transform.localPosition = new Vector3(0f, _lift - targetHeight * 0.5f, 0f);
                _body.transform.localRotation = Quaternion.Euler(0f, 0f, _tilt);
                _body.transform.localScale = new Vector3(scale * _squash.x, scale * _squash.y, 1f);
                return;
            }
            _body.transform.localRotation = Quaternion.identity;
            _body.transform.localScale = _suzu
                ? Vector3.one * (targetHeight / _body.sprite.bounds.size.y) : Vector3.one;
        }
    }
}
