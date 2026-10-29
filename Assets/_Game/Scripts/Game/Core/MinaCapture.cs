#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Neko.Game.Ui;
using Neko.Game.View;
using Neko.Sim.Save;
using Neko.Sim.Shift;
using Neko.Sim.Team;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Neko.Game.Core
{
    /// <summary>
    /// Explicit, Editor-only evidence run for the Mina experiment. Never enabled in the player
    /// or an ordinary boot.
    /// </summary>
    /// <remarks>
    /// One run is one variant at one resolution, from a fixed seed with a fixed frame length,
    /// so V0 and V1 play the same day frame for frame. Natural segments only watch the
    /// simulation; controlled probes come after them, with the simulation suspended, and are
    /// stored apart. A natural segment that does not happen is a failure, not a skipped shot.
    /// </remarks>
    public sealed class MinaCapture : MonoBehaviour
    {
        /// <summary>25 frames a second, so an animated GIF can keep exact time (4 cs a frame).</summary>
        private const float Frame = 1f / 25f;

        private const float WaitLimit = 240f;
        private const float RecordLimit = 30f;
        private const float RequiredTravel = 2f;

        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        private float _deadline;
        private static string _dir;
        private static Vector2Int _size;
        private static RenderTexture _target;
        private static Texture2D _pixels;

        private void Awake()
        {
            _deadline = Time.realtimeSinceStartup + 900f;
            Application.logMessageReceived += OnLog;
        }

        private void OnDestroy() => Application.logMessageReceived -= OnLog;

        private void Update()
        {
            if (Time.realtimeSinceStartup > _deadline) Fail("real-time deadline exceeded");
        }

        // Exiting inside the log callback loses the message from the log file, so the reason
        // is written beside the captures first.
        private void OnLog(string message, string stack, LogType type)
        {
            if (type != LogType.Exception && type != LogType.Error) return;
            Record(type + ": " + message + "\n" + stack);
            EditorApplication.Exit(1);
        }

        private static void Record(string failure)
        {
            var dir = Environment.GetEnvironmentVariable("NEKO_MINA_CAPTURE_DIR");
            if (!string.IsNullOrEmpty(dir)) File.AppendAllText(Path.Combine(dir, "failure.txt"), failure + "\n");
        }

        private static void Fail(string why)
        {
            Record("MINA_CAPTURE_FAILED: " + why);
            Debug.LogError("MINA_CAPTURE_FAILED: " + why);
            EditorApplication.Exit(1);
        }

        public static void Begin()
        {
            if (!Application.isBatchMode || !SaveBridge.Isolated)
                throw new InvalidOperationException("Capture requires isolated Editor batch mode.");
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Boot.unity");
            EditorApplication.isPlaying = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartCapture()
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NEKO_MINA_CAPTURE_DIR"))) return;
            if (!Application.isBatchMode || !SaveBridge.Isolated)
                throw new InvalidOperationException("Refusing capture without save isolation.");
            new GameObject("Mina evidence capture").AddComponent<MinaCapture>().StartCoroutine(Run());
        }

        private static IEnumerator Run()
        {
            _dir = Environment.GetEnvironmentVariable("NEKO_MINA_CAPTURE_DIR");
            var size = (Environment.GetEnvironmentVariable("NEKO_MINA_CAPTURE_SIZE") ?? "1280x720").Split('x');
            _size = new Vector2Int(int.Parse(size[0], Invariant), int.Parse(size[1], Invariant));
            var seed = uint.Parse(Environment.GetEnvironmentVariable("NEKO_MINA_SEED") ?? "1", Invariant);
            Directory.CreateDirectory(_dir);

            for (var i = 0; i < 12; i++) yield return null;

            // A fixed frame length drives both clocks, or the run is not evidence of real time.
            // Time.captureDeltaTime is not used: it leaves Time.unscaledDeltaTime, which the
            // clocks read, at the measured value (checked in batch mode, see Clocks.FixedFrame).
            Clocks.FixedFrame = Frame;
            for (var i = 0; i < 5; i++)
            {
                yield return null;
                if (Mathf.Abs(Clocks.Game - Frame) > 1e-6f || Mathf.Abs(Clocks.Ui - Frame) > 1e-6f)
                {
                    Fail("the game clock does not follow the fixed frame: Clocks.Game=" +
                         Clocks.Game.ToString("F5", Invariant));
                    yield break;
                }
            }
            Debug.Log("MINA_CLOCK fixed=" + Frame.ToString("F4", Invariant) +
                      " Clocks.Game=" + Clocks.Game.ToString("F4", Invariant) + " Clocks.Ui=" +
                      Clocks.Ui.ToString("F4", Invariant) + " unscaledDeltaTime(real)=" +
                      Time.unscaledDeltaTime.ToString("F4", Invariant));

            var root = GameRoot.Instance;
            var profile = Profile.NewGame(seed);
            var maid = profile.Maids.First(m => m.NaturalRole == Role.Floor);
            maid.Name = "Mina";
            for (var i = 0; i < maid.Gear.Length; i++) maid.Gear[i] = null;
            var companion = profile.Maids.FirstOrDefault(m => m != maid);
            if (companion != null) companion.Name = "Suzu";
            root.StartIsolatedSession(profile);

            var camera = root.Camera.Camera;
            _target = new RenderTexture(_size.x, _size.y, 24);
            _pixels = new Texture2D(_size.x, _size.y, TextureFormat.RGB24, false);
            camera.targetTexture = _target;
            camera.aspect = (float)_size.x / _size.y;

            root.Ui.Get<HubScreen>().OnOpenDay.Invoke();
            MaidView view = null;
            for (var i = 0; i < 60 && view == null; i++)
            {
                yield return null;
                view = UnityEngine.Object.FindObjectsByType<MaidView>(FindObjectsInactive.Exclude)
                    .FirstOrDefault(v => v.Worker.Maid == maid);
            }
            if (view == null || view.Worker.Role != Role.Floor)
            {
                Fail("Mina is not on the floor in this lineup");
                yield break;
            }
            Debug.Log("MINA_RUN variant=" + MinaVisual.Motion + " size=" + _size.x + "x" + _size.y +
                      " seed=" + seed);
            for (var i = 0; i < 12; i++) yield return null;

            var walk = new Segment("natural_walk", view, w => w.Claimed != null, true);
            yield return walk.Run();
            if (walk.Failed) yield break;

            var carry = new Segment("natural_carry", view, w => w.Carrying, false);
            yield return carry.Run();
            if (carry.Failed) yield break;

            // Only if the day sends her up or down the room; reported, not required.
            var vertical = new Segment("natural_vertical", view,
                w => view.Heading.Moving && view.Heading.Facing != MinaVisual.Facing.Side, false);
            yield return vertical.Run();
            if (vertical.Failed) yield break;

            yield return Probes(root, view);
        }

        private static float TargetHeight(Camera camera) => 96f * 2f * camera.orthographicSize / 720f;

        private static SpriteRenderer BodyOf(MaidView view) =>
            view.transform.Find("Body").GetComponent<SpriteRenderer>();

        private static void Save(string path)
        {
            var camera = GameRoot.Instance.Camera.Camera;
            var previous = RenderTexture.active;
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture.active = _target;
            _pixels.ReadPixels(new Rect(0, 0, _size.x, _size.y), 0, 0);
            _pixels.Apply();
            RenderTexture.active = previous;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, _pixels.EncodeToPNG());
        }

        /// <summary>
        /// One stretch of the real day, recorded frame by frame at normal speed.
        /// </summary>
        /// <remarks>
        /// The day is played at 4× until the segment's condition first holds, then at 1× while
        /// every frame is saved. It ends once she has walked far enough and then stood still
        /// for a second. Checks run on every frame and fail the whole run.
        /// </remarks>
        private sealed class Segment
        {
            private readonly string _label;
            private readonly MaidView _view;
            private readonly Func<Worker, bool> _start;
            private readonly bool _required;
            private readonly StringBuilder _csv = new StringBuilder();
            private readonly List<string> _problems = new List<string>();

            public bool Failed { get; private set; }

            public Segment(string label, MaidView view, Func<Worker, bool> start, bool required)
            {
                _label = label;
                _view = view;
                _start = start;
                _required = required;
            }

            public IEnumerator Run()
            {
                var worker = _view.Worker;
                var shift = GameRoot.Instance.Presenter.Shift;

                Clocks.Scale = 4f;
                var waited = 0f;
                while (!_start(worker))
                {
                    waited += Clocks.Game;
                    if (waited > WaitLimit || shift.Finished)
                    {
                        Clocks.Scale = 1f;
                        if (_required)
                        {
                            Failed = true;
                            Fail(_label + ": condition never held within " + WaitLimit + " game seconds");
                        }
                        else Debug.Log("MINA_SEGMENT " + _label + " not observed within " + WaitLimit + " game seconds");
                        yield break;
                    }
                    yield return null;
                }
                Clocks.Scale = 1f;

                _csv.AppendLine("frame,time,worker_x,worker_y,view_x,view_y,task,carrying,on_break,pose," +
                                "facing,left,moving,sprite,flip,lift,tilt,scale_x,scale_y,travelled");

                var camera = GameRoot.Instance.Camera.Camera;
                var body = BodyOf(_view);
                var last = new Vector2(worker.X, worker.Y);
                var time = 0f;
                var travelled = 0f;
                var still = 0f;
                var sinceReverse = 0f;
                var lastSign = 0;
                var facings = new List<(float time, MinaVisual.Facing facing, bool left)>();

                var frames = 0;
                for (var frame = 0; ; frame++)
                {
                    yield return null;
                    frames = frame + 1;
                    time += Clocks.Game;

                    var position = new Vector2(worker.X, worker.Y);
                    var step = position - last;
                    last = position;
                    var stepped = step.sqrMagnitude > MinaFacing.StillSquared;
                    travelled += step.magnitude;
                    still = stepped ? 0f : still + Clocks.Game;

                    var sign = Mathf.Abs(step.x) > 1e-3f ? Math.Sign(step.x) : 0;
                    if (sign != 0 && sign != lastSign)
                    {
                        lastSign = sign;
                        sinceReverse = 0f;
                    }
                    else sinceReverse += Clocks.Game;

                    var heading = _view.Heading;
                    if (facings.Count == 0 || facings[facings.Count - 1].facing != heading.Facing ||
                        facings[facings.Count - 1].left != heading.Left)
                        facings.Add((time, heading.Facing, heading.Left));

                    Check(frame, time, step, still, sinceReverse, facings, body, camera);
                    Save(Path.Combine(_dir, _label, "frame_" + frame.ToString("D4", Invariant) + ".png"));

                    var scale = TargetHeight(camera) / body.sprite.bounds.size.y;
                    _csv.Append(frame).Append(',')
                        .Append(F(time)).Append(',').Append(F(worker.X)).Append(',').Append(F(worker.Y)).Append(',')
                        .Append(F(_view.transform.position.x)).Append(',').Append(F(_view.transform.position.y)).Append(',')
                        .Append(worker.Claimed != null ? worker.Claimed.Kind.ToString() : "-").Append(',')
                        .Append(worker.Carrying).Append(',').Append(worker.OnBreak).Append(',')
                        .Append(_view.Pose).Append(',').Append(heading.Facing).Append(',').Append(heading.Left).Append(',')
                        .Append(heading.Moving).Append(',').Append(body.sprite.name).Append(',').Append(body.flipX).Append(',')
                        .Append(F(body.transform.localPosition.y + TargetHeight(camera) * 0.5f)).Append(',')
                        .Append(F(Mathf.DeltaAngle(0f, body.transform.localEulerAngles.z))).Append(',')
                        .Append(F(body.transform.localScale.x / scale)).Append(',')
                        .Append(F(body.transform.localScale.y / scale)).Append(',')
                        .Append(F(travelled)).AppendLine();

                    var done = travelled >= RequiredTravel && still >= 1f &&
                               (_required || !worker.Carrying);
                    if (done || time > RecordLimit) break;
                }

                File.WriteAllText(Path.Combine(_dir, _label + ".csv"), _csv.ToString());
                Debug.Log("MINA_SEGMENT " + _label + " frames=" + frames + " travelled=" + F(travelled) +
                          " seconds=" + F(time) + " facing_changes=" + (facings.Count - 1));

                if (_required && travelled < RequiredTravel)
                    _problems.Add("travelled " + F(travelled) + " tiles, fewer than " + RequiredTravel);
                if (_problems.Count == 0) yield break;

                Failed = true;
                Fail(_label + ": " + string.Join("; ", _problems.Take(12)) +
                     (_problems.Count > 12 ? " (+" + (_problems.Count - 12) + " more)" : ""));
            }

            private void Check(int frame, float time, Vector2 step, float still, float sinceReverse,
                List<(float time, MinaVisual.Facing facing, bool left)> facings, SpriteRenderer body, Camera camera)
            {
                var name = body.sprite.name;
                var at = _label + "#" + frame;

                // Standing still past the movement hold: never frozen in a stride.
                if (still > MinaFacing.MovementHold + 2f * Frame && name != "Mina_idle" && name != "Mina_carry")
                    _problems.Add(at + " stands in " + name);

                // No moonwalk: a mirrored drawing faces the way she walks once a turn has settled.
                var horizontal = Mathf.Abs(step.x) > Mathf.Abs(step.y) * MinaFacing.EnterSide &&
                                 Mathf.Abs(step.x) > 1e-3f;
                // The carry drawing faces a side on diagonal legs too, so it is checked there.
                var crossing = Mathf.Abs(step.x) > Mathf.Abs(step.y) * MinaFacing.KeepHeading &&
                               Mathf.Abs(step.x) > 1e-3f;
                var faced = name == "Mina_side" ? horizontal : name == "Mina_carry" && crossing;
                if (faced && sinceReverse > MinaFacing.DirectionHold + 2f * Frame && body.flipX != step.x < 0f)
                    _problems.Add(at + " " + name + " faces against her step");

                // No chatter: a heading never returns to the one before inside the direction hold.
                var n = facings.Count;
                if (n >= 3 && facings[n - 1].time == time &&
                    time - facings[n - 2].time < MinaFacing.DirectionHold + Frame &&
                    facings[n - 1].facing == facings[n - 3].facing && facings[n - 1].left == facings[n - 3].left)
                    _problems.Add(at + " heading flickered back to " + facings[n - 1].facing);

                // Feet stay on her spot and the drawing keeps its size.
                var height = TargetHeight(camera);
                var lift = body.transform.localPosition.y + height * 0.5f;
                if (lift < -1e-3f || lift > 0.08f) _problems.Add(at + " feet offset " + F(lift));
                var scale = height / body.sprite.bounds.size.y;
                var sy = body.transform.localScale.y / scale;
                var sx = body.transform.localScale.x / scale;
                if (sy < 0.97f || sy > 1.001f || sx < 0.999f || sx > 1.03f)
                    _problems.Add(at + " scale " + F(sx) + "x" + F(sy));
            }
        }

        private static string F(float value) => value.ToString("F4", Invariant);

        /// <summary>
        /// Controlled presentation probes: positions are moved in memory, the day is suspended.
        /// </summary>
        private static IEnumerator Probes(GameRoot root, MaidView view)
        {
            root.enabled = false;
            var worker = view.Worker;
            var maid = worker.Maid;
            worker.Claimed = null;
            worker.Carrying = false;
            worker.OnBreak = false;
            worker.NeedsBreak = false;
            maid.Mood = 1f;
            var body = BodyOf(view);
            var problems = new List<string>();
            var log = new StringBuilder("probe,sprite,flip,view_x,view_y\n");

            IEnumerator Move(string label, Vector2 direction, int frames, string sprite, bool? flip)
            {
                for (var i = 0; i < frames; i++)
                {
                    worker.X += direction.x * Clocks.Game;
                    worker.Y += direction.y * Clocks.Game;
                    view.Sync();
                    yield return null;
                }
                var name = body.sprite.name;
                log.Append(label).Append(',').Append(name).Append(',').Append(body.flipX).Append(',')
                    .Append(F(view.transform.position.x)).Append(',').Append(F(view.transform.position.y)).Append('\n');
                Debug.Log("MINA_PROBE " + label + " sprite=" + name + " flip=" + body.flipX);
                if (name != "Mina_" + sprite || (flip.HasValue && body.flipX != flip.Value))
                    problems.Add(label + " drew " + name + " flip=" + body.flipX);
                Save(Path.Combine(_dir, "probes", label + ".png"));
            }

            var walking = 15;
            var standing = 10;
            yield return Move("right", Vector2.right, walking, "side", false);
            yield return Move("left", Vector2.left, walking, "side", true);
            yield return Move("back", Vector2.up, walking, "back", false);
            yield return Move("front", Vector2.down, walking, "walk", false);
            yield return Move("diagonal", new Vector2(1f, 1f), walking, "back", false);
            yield return Move("diagonal_near", new Vector2(1f, 0.95f), walking, "back", false);
            yield return Move("stop", Vector2.zero, standing, "idle", false);
            yield return Move("reverse_right", Vector2.right, 3, "side", false);
            yield return Move("reverse_left", Vector2.left, walking, "side", true);
            yield return Move("reverse_stop", Vector2.zero, standing, "idle", false);

            worker.Carrying = true;
            yield return Move("carry_right", Vector2.right, walking, "carry", false);
            yield return Move("carry_diag_left", new Vector2(-0.8f, -1f), walking, "carry", true);
            yield return Move("carry_left", Vector2.left, walking, "carry", true);
            yield return Move("carry_back", Vector2.up, walking, "back", false);
            yield return Move("carry_front", Vector2.down, walking, "carry", null);
            yield return Move("carry_stop", Vector2.zero, standing, "carry", null);
            worker.Carrying = false;

            worker.Claimed = new ShiftTask(-1, TaskKind.Cook, -1);
            yield return Move("cook_walk", Vector2.right, walking, "side", false);
            yield return Move("cook_stop", Vector2.zero, standing, "idle", false);
            worker.Claimed = null;

            worker.OnBreak = true;
            yield return Move("tired_walk", Vector2.left, walking, "side", true);
            yield return Move("tired_stop", Vector2.zero, standing, "idle", false);
            worker.OnBreak = false;

            File.WriteAllText(Path.Combine(_dir, "probes.csv"), log.ToString());
            if (problems.Count > 0)
            {
                Fail("probes: " + string.Join("; ", problems));
                yield break;
            }

            Debug.Log("MINA_CAPTURE_COMPLETE: isolated save; natural segments and controlled probes.");
            EditorApplication.Exit(0);
        }
    }
}
#endif
