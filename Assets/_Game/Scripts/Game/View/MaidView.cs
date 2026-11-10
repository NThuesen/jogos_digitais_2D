using Neko.Game.Art;
using Neko.Game.Core;
using Neko.Game.Feel;
using Neko.Sim.Cafe;
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
        private SpriteRenderer _mood;
        private Maid _maid;
        private MaidPose _pose = MaidPose.Idle;
        private float _bob;
        private Vector3 _lastPosition;

        /// <summary>
        /// The authored walk frames when this maid has them, otherwise null and the procedural
        /// figure is drawn as it always was.
        /// </summary>
        private Sprite[] _authored;

        private MaidAnimCatalog.Entry _entry;
        private WalkCycle _cycle;
        private bool _carriedIsPlate;

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

        /// <summary>The room she walks in, for depth against its furniture. Optional.</summary>
        public CafeGrid Grid { get; set; }

        /// <summary>
        /// Where her sole is drawn, which is not where her root is: an authored body hangs about
        /// 0.58 of a tile below it. Depth is decided here so a maid crossing the back edge of a
        /// piece is hidden or shown by what the player sees, not by the logical position. The
        /// procedural walk bob is left out, or the key would flicker with every step.
        /// </summary>
        public Vector3 Feet
        {
            get
            {
                var body = _body.transform;
                var sprite = _body.sprite;
                var sole = sprite != null ? sprite.bounds.min.y * body.lossyScale.y : 0f;
                var bob = _authored == null ? body.localPosition.y : 0f;
                return new Vector3(body.position.x, body.position.y - bob + sole, 0f);
            }
        }

        public void Bind(Worker worker)
        {
            Worker = worker;
            _maid = worker.Maid;

            const float radius = 0.5f;
            var angle = worker.Id * 137.5f * Mathf.Deg2Rad;
            _offset = new Vector3(Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius * 0.7f, 0f);

            BindAuthored();
            if (_authored == null) Redraw();

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
            var moving = (target - _lastPosition).sqrMagnitude > 1e-5f;

            // Smoothed rather than snapped: the simulation ticks in whole frames and a worker
            // crossing a tile in two of them looks like a teleport otherwise.
            var before = transform.position;
            transform.position = Vector3.Lerp(transform.position, target, 1f - Mathf.Exp(-18f * Clocks.Ui));
            _lastPosition = target;

            var pose = PoseFor(moving);
            if (_authored != null)
            {
                _pose = pose;
                SyncAuthored(pose, Vector3.Distance(before, transform.position));
            }
            else
            {
                if (pose != _pose)
                {
                    _pose = pose;
                    Redraw();
                }

                // A walk bob. Four lines, and the difference between a sprite sliding and a
                // person walking.
                _bob = moving ? _bob + Clocks.Game * 11f : 0f;
                var lift = moving ? Mathf.Abs(Mathf.Sin(_bob)) * 0.07f : 0f;
                _body.transform.localPosition = new Vector3(0f, lift, 0f);
            }

            if (target.x - transform.position.x > 0.02f) _body.flipX = false;
            else if (target.x - transform.position.x < -0.02f) _body.flipX = true;

            // Sorted with the furniture, by where the feet are. The plate is one slot above her
            // body rather than in the items layer, which would draw it over a table she is behind.
            var depth = DepthSort.ActorKey(Grid, Feet);
            Layers.ApplyActor(_body, depth);
            Layers.ApplyActor(_carried, depth, 1);
            Layers.Apply(_mood, Layers.Fx, transform.position.y);

            _carried.enabled = Worker.Carrying;
            SyncMood();
        }

        public void ShowCarried(Sprite sprite)
        {
            _carried.sprite = sprite;
            _carried.enabled = sprite != null;
            if (sprite != null) Tween.Punch(_carried.transform, 0.3f, 0.2f);
        }

        /// <summary>Squash on the spot. Used when she finishes something.</summary>
        public void Pop() => Tween.Punch(transform, 0.18f, 0.24f);

        private void BindAuthored()
        {
            _entry = MaidAnimCatalog.Resolve(_maid);
            _authored = _entry != null ? MaidAnimCatalog.Load(_sprites, _entry) : null;
            _cycle = null;
            _body.color = Color.white;

            if (_authored == null)
            {
                _carried.transform.localPosition = new Vector3(0f, 0.18f, 0f);
                _body.transform.localScale = Vector3.one;
                if (_carriedIsPlate) _carried.sprite = null;
                _carriedIsPlate = false;
                return;
            }

            // The sheet has no carrying drawing, so a plate at her hands is what says she is
            // carrying. Nothing else in the game assigns a carried sprite today.
            if (_carried.sprite == null)
            {
                _carried.sprite = _sprites.Get(new ShapeSpec(ShapeId.Plate, Palette.Panel, Palette.Ink,
                    0.05f, scale: new Vector2(0.9f, 0.9f)), 64);
                _carriedIsPlate = true;
            }

            _cycle = new WalkCycle(_entry.frames, _entry.units_per_cycle,
                _entry.rest_frames, _entry.idle_frame);
            _body.sprite = _authored[_cycle.Frame];
            _body.transform.localPosition = _entry.BodyOffset;
            _carried.transform.localPosition = _entry.CarriedOffset;
        }

        /// <summary>
        /// Picks the frame for an authored maid (any entry of the catalog). The pose only decides what is laid on top of the
        /// walk: the sheet has no carry, cook or tired drawings, so those reuse the walk and the
        /// standing frame, with the existing plate and mood bar saying the rest.
        /// </summary>
        private void SyncAuthored(MaidPose pose, float travelled)
        {
            _cycle.Step(travelled, Clocks.Game, Clocks.Ui);
            _body.sprite = _authored[_cycle.Frame];
            _body.transform.localPosition = _entry.BodyOffset;
            _body.color = pose == MaidPose.Tired ? new Color(0.8f, 0.8f, 0.88f) : Color.white;
        }

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

        private void Redraw()
        {
            _body.sprite = _sprites.Layered(MaidSpriteBuilder.Layers(_maid, _pose),
                MaidSpriteBuilder.CacheKey(_maid, _pose), 128);
        }
    }
}
