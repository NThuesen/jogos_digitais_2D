using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Neko.Game.Core;
using Neko.Game.Feel;
using Neko.Game.Ui;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using BindingFlags = System.Reflection.BindingFlags;
using CafeLevelTable = Neko.Sim.Economy.CafeLevelTable;
using FieldInfo = System.Reflection.FieldInfo;
using Lootbox = Neko.Sim.Economy.Lootbox;
using Object = UnityEngine.Object;
using Rarity = Neko.Sim.Team.Rarity;
using Role = Neko.Sim.Team.Role;
using ShopItemKind = Neko.Sim.Economy.ShopItemKind;
using TraitId = Neko.Sim.Team.TraitId;

namespace Neko.Tests.PlayMode.Layout
{
    /// <summary>
    /// The Layout_Check: the game measured at both Reference_Resolutions.
    /// </summary>
    /// <remarks>
    /// It holds the audit loop, <see cref="Audit"/>, one case per implemented Audit_State, and the
    /// tests the loop is built on: the smoke test of the batch-mode resolution rig, the probes
    /// that compare the element model with rendered frames, the test that the element model
    /// describes Empresa, the test that the click helpers find the Baseline's click targets
    /// there, and the smoke tests of the session factory and its value providers.
    /// Captures are written only when NEKO_CAPTURE_DIR is set, the report only when
    /// NEKO_LAYOUT_REPORT is set, and model dumps only when NEKO_LAYOUT_DUMP is set; give each a
    /// path under Logs/, never under Assets/, where DocsTests rejects every image file and Unity
    /// imports every file.
    /// </remarks>
    public sealed class LayoutCheckTests
    {
        private const string ScenePath = "Assets/_Game/Scenes/Boot.unity";

        /// <summary>Batch mode renders the first frames before material albedo is ready.</summary>
        private const int DiscardFrames = 12;

        /// <summary>
        /// The anti-aliasing tolerance of the probes: how far, in window pixels, an edge of the
        /// box of changed pixels may lie from the edge the model predicts.
        /// </summary>
        /// <remarks>
        /// The model and the GPU both decide coverage at pixel centres, but the GPU snaps
        /// vertices to a sub-pixel grid, applies its own fill rule to a centre that lies on an
        /// edge, and filters textures with fixed-point weights. A pixel whose centre lies that
        /// close to an edge, or to an alpha threshold, can fall either way, which moves an edge
        /// of a box by one pixel.
        /// </remarks>
        private const int EdgeTolerancePx = 1;

        /// <summary>How far inside its glyph quads the pixels a label changes may stop.</summary>
        /// <remarks>
        /// A glyph quad is the glyph's bitmap rectangle, which the font rasteriser rounds
        /// outwards to whole pixels, so its outermost row or column can hold too little ink to
        /// change a pixel: one pixel for that, plus <see cref="EdgeTolerancePx"/>.
        /// </remarks>
        private const int GlyphInkTolerancePx = 2;

        /// <summary>
        /// The Interactive_Controls of the illustrated Empresa (UI-INT-01), by object name: back, the
        /// five places to spend, and open the day. The label of each is its child <c>Label</c>.
        /// </summary>
        private static readonly string[] EmpresaControlNames =
            { "Back", "Nav_Lineup", "Nav_Roster", "Nav_Boxes", "Nav_Shop", "Nav_Hall", "Cta_OpenDay" };

        /// <summary>What each of <see cref="EmpresaControlNames"/> reads.</summary>
        private static string[] EmpresaControlLabels() => new[]
        {
            Strings.Get("dialog.back"), "Equipe do Dia", "Maids", "Caixinhas", "Lojinha", "Café",
            "Começar Expediente",
        };

        /// <summary>The economy panel, the Image directly on HubScreen that holds the four figures.</summary>
        private const string EconomyPanelName = "EconomyPanel";

        /// <summary>The name of the wordmark, the aspect-preserving picture directly on HubScreen.</summary>
        private const string WordmarkName = "ui020_logo_wordmark";

        /// <summary>The captions and values of the economy panel: four columns of two labels.</summary>
        private const int EconomyLabels = 8;

        /// <summary>
        /// The measurement steps of a view whose frames the audit loop captures: 960×600 and
        /// 1280×720, one capture per Reference_Resolution (Requirement 20.6).
        /// </summary>
        private const int CaptureSteps = 2;

        /// <summary>
        /// The windows of a measurement: 960×600, 1280×720, then 960×600 again with no new Show
        /// in between, which is the round trip of Requirement 2.3.
        /// </summary>
        private static readonly Vector2Int[] Windows =
        {
            new Vector2Int(960, 600),
            new Vector2Int(1280, 720),
            new Vector2Int(960, 600),
        };

        /// <summary>The windows the probes measure: the two Reference_Resolutions.</summary>
        private static readonly Vector2Int[] ProbeWindows =
        {
            new Vector2Int(960, 600),
            new Vector2Int(1280, 720),
        };

        /// <summary>
        /// The ids of the implemented Audit_States, in table order: the cases of <see cref="Audit"/>,
        /// each named by its id, such as <c>Audit("EM-1")</c>.
        /// </summary>
        public static IEnumerable<string> StateIds => AuditStates.Ids;

        [TearDown]
        public void RestoreTheRigAndTheSession()
        {
            // The audit loop's context. Clearing it is one assignment that cannot fail, so it goes
            // first and every restore below still runs.
            StateContext.End();

            try
            {
                ResolutionRig.Restore();
            }
            finally
            {
                SessionFactory.RestoreClock();
                SessionFactory.RestoreSave();
            }
        }

        /// <summary>
        /// The audit loop: the Audit_State <paramref name="id"/>, each of its views measured at
        /// 960×600, 1280×720 and 960×600 again without a new Show, with every registered rule that
        /// applies and the state's checks at each step (Requirements 2.3 and 20.1).
        /// </summary>
        /// <remarks>
        /// One case per implemented Audit_State, named by its id, so a filter such as
        /// <c>Neko.Tests.PlayMode.Layout.LayoutCheckTests.*EM-1</c> runs one state. The state loads
        /// once: its Build crafts the session, boots the game and walks to the Screen. Each view is
        /// entered inside that load, as LJ-4 selects a tab and SL-4 raises the café level, and settled
        /// once. Each step then applies its window, settles, measures the Active_Layer, and runs the
        /// rules and the checks on that measurement.
        ///
        /// Steps 1 and 2 write the view's captures when NEKO_CAPTURE_DIR is set (Requirement 20.6),
        /// every step writes its model dump when NEKO_LAYOUT_DUMP is set, and each view appends its
        /// records to the NEKO_LAYOUT_REPORT file once its three steps are measured. The test fails
        /// once, after every view and step, with every record in the message (Requirement 20.3). A
        /// canvas that differed from the scaler model fails it first, because its records would not
        /// describe a real window (Requirement 20.2). [TearDown] restores the window (Requirement 20.7).
        /// </remarks>
        [UnityTest]
        public IEnumerator Audit([ValueSource(nameof(StateIds))] string id)
        {
            var state = AuditStates.Get(id);
            var context = StateContext.Begin(state);

            var build = state.Build(context);
            Assert.IsTrue(build != null, "the Build of " + state.Id + " returned no routine");
            yield return build;

            var all = new List<Violation>();
            var canvases = new List<CanvasMeasurement>();

            foreach (var view in state.Views)
            {
                yield return context.Enter(view);
                yield return Settle();

                var rules = new List<LayoutRule>();
                foreach (var rule in LayoutRules.All)
                {
                    if (rule.AppliesTo(state, view)) rules.Add(rule);
                }

                var records = new List<Violation>();
                var windows = new List<Vector2Int>();
                Transform layer = null;
                string layerPath = null;

                for (var step = 1; step <= Windows.Length; step++)
                {
                    var window = Windows[step - 1];
                    var size = ResolutionRig.Size(window.x, window.y);
                    yield return ResolutionRig.Apply(window.x, window.y);
                    yield return Settle();

                    canvases.Add(ResolutionRig.Last);
                    var measurement = LayoutModel.Measure(context.Ui);
                    windows.Add(measurement.Window);

                    // Requirement 2.3: the three steps measure the one Screen the view showed.
                    if (step == 1)
                    {
                        layer = measurement.Layer;
                        layerPath = measurement.LayerPath;
                    }

                    Assert.IsTrue(measurement.Layer == layer,
                        state.Id + " (" + view.Screen + ") shows " + measurement.LayerPath + " at step " + step + ", " +
                        size + ", but showed " + layerPath + " at step 1, so the Screen changed between the steps");

                    // <id>_<screen-slug>_<W>x<H>_playmode.png, only when NEKO_CAPTURE_DIR is set. Step 3
                    // is 960×600 again, whose capture would replace the one of step 1.
                    var capture = step <= CaptureSteps ? LayoutReport.WriteCapture(state.Id, view.Screen) : null;

                    // <id>_<screen-slug>_step<N>_<W>x<H>.txt, only when NEKO_LAYOUT_DUMP is set.
                    var dump = LayoutModel.Dump(measurement,
                        state.Id + "_" + LayoutReport.Slug(view.Screen) + "_step" + step);

                    var found = new List<Violation>();
                    var input = new RuleInput(context, measurement, step);
                    foreach (var rule in rules) rule.Check(input, found);
                    state.Checks(measurement, found);

                    foreach (var violation in found)
                    {
                        Assert.IsTrue(violation != null,
                            "a rule or check of " + state.Id + " (" + view.Screen + ") added a null violation at " + size);
                        records.Add(violation.In(state.Id, view.Screen));
                    }

                    Debug.Log("Layout_Check audit, " + state.Id + " (" + view.Screen + "), step " + step + ", " + size +
                              ": " + measurement.LayerPath + " at scale factor " + Number(measurement.ScaleFactor) + ", " +
                              measurement.Elements.Count + " elements, " + measurement.Controls.Count +
                              " Interactive_Controls; " + rules.Count + " rule(s) and the state's checks found " +
                              found.Count + " violation(s)" + (capture != null ? "; capture " + capture : "") +
                              (dump != null ? "; dump " + dump : ""));
                }

                LayoutReport.Append(state.Id, view.Screen, windows, records);
                all.AddRange(records);
            }

            ResolutionRig.AssertMatchesModel(canvases);
            LayoutReport.AssertNone(all);

            // The Console check of task 4.7 goes here, once per test, after every record is
            // reported: LogAssert.NoUnexpectedReceived().
        }

        /// <summary>
        /// Each window the rig applies gives the canvas the scale and size that the game's own
        /// scaler computes for a real window of that size.
        /// </summary>
        /// <remarks>
        /// Measures the Menu as it comes up after boot. Every window is measured, logged and
        /// captured before the assertion, so a failing run still reports all three.
        /// </remarks>
        [UnityTest]
        public IEnumerator RigMatchesTheScalerModelAtBothResolutions()
        {
            yield return SceneManager.LoadSceneAsync(ScenePath, LoadSceneMode.Single);
            for (var i = 0; i < DiscardFrames; i++) yield return null;

            var root = GameRoot.Instance;
            Assert.IsTrue(root != null, "GameRoot never woke up");
            Assert.IsInstanceOf<MenuScreen>(root.Ui.Current, "the game did not come up on the Menu");

            var measurements = new List<CanvasMeasurement>();
            for (var step = 1; step <= Windows.Length; step++)
            {
                var window = Windows[step - 1];
                yield return ResolutionRig.Apply(window.x, window.y);

                var measurement = ResolutionRig.Last;
                measurements.Add(measurement);
                Debug.Log("Layout_Check rig, Menu, step " + step + ": " + measurement);

                // rig-<step>_menu_<W>x<H>_playmode.png, only when NEKO_CAPTURE_DIR is set.
                LayoutReport.WriteCapture("rig-" + step, "Menu");
            }

            ResolutionRig.AssertMatchesModel(measurements);
        }

        /// <summary>
        /// The meshes and glyph quads the model reads agree with what the camera draws on
        /// Empresa, at both Reference_Resolutions.
        /// </summary>
        /// <remarks>
        /// The third step of the design's prototype. Each probe hides one element through its
        /// renderer's alpha, which leaves layout alone, renders again and bounds the pixels that
        /// changed.
        /// <list type="bullet">
        /// <item>Images: the Hub backdrop (Simple), the face of the Maids button (the kit's
        /// sliced nav plate), the economy panel and the start-of-day button (the kit's
        /// aspect-preserving plates), and two test-local images that cover the letterbox and
        /// sliced-with-clear-border cases. A sprite leaves a transparent margin inside its rect
        /// (design cause C1), so the raw quad is larger than what is drawn. The model therefore samples
        /// the sprite's alpha at each pixel centre's interpolated UV: the changed pixels must lie
        /// inside the pixels where the mesh samples any alpha, and cover the pixels where it
        /// samples at least half.</item>
        /// <item>Text: the chemistry line, whose changed pixels must match its glyph quads.</item>
        /// </list>
        /// The test also counts glyph quads against the characters of every Empresa label, which
        /// settles whether whitespace gets a quad, and raycasts every control centre through
        /// <c>EventSystem.RaycastAll</c> next to the topmost raycast target in draw order. The
        /// raycast outcome is recorded, never asserted. Everything is logged before the test fails.
        /// </remarks>
        [UnityTest]
        public IEnumerator ProbesAgreeWithTheRenderedFrame()
        {
            yield return SceneManager.LoadSceneAsync(ScenePath, LoadSceneMode.Single);
            for (var i = 0; i < DiscardFrames; i++) yield return null;

            var root = GameRoot.Instance;
            Assert.IsTrue(root != null, "GameRoot never woke up");

            // Empresa, reached as CaptureTests reaches it: a clean business, then the play button.
            root.Ui.Get<MenuScreen>().OnNewGame.Invoke();
            yield return null;
            root.Ui.Get<MenuScreen>().OnPlay.Invoke();
            yield return null;

            var hub = root.Ui.Get<HubScreen>();
            Assert.IsTrue(hub != null && root.Ui.Current == hub, "the game did not reach Empresa");
            Assert.IsFalse(root.Ui.DialogOpen, "a Dialog is open over Empresa, so the probes would measure it");

            var canvas = root.Ui.GetComponent<Canvas>();
            var local = new LocalImages(hub.transform, canvas.referencePixelsPerUnit);

            try
            {
                var probes = new[]
                {
                    new Probe("simple", "Simple image, the Hub backdrop", BackdropImage(hub)),
                    new Probe("sliced-kit", "sliced image, the face of the Maids button (nav_pink_surface)",
                        KitFace(hub, "Nav_Roster", Image.Type.Sliced)),
                    new Probe("aspect", "aspect-preserving image, the economy panel", EconomyPanelImage(hub)),
                    new Probe("aspect-cta", "aspect-preserving image, the start-of-day button",
                        KitFace(hub, "Cta_OpenDay", Image.Type.Simple)),
                    new Probe("letterbox", "test-local aspect-preserving image, a 2:1 sprite in a square rect",
                        local.Letterbox),
                    new Probe("sliced", "test-local sliced image, 8-texel borders whose outer 2 texels are clear",
                        local.Sliced),
                    new Probe("text", "label, the chemistry line", ChemistryLine(hub)),
                };

                var failures = new List<string>();
                var measurements = new List<CanvasMeasurement>();
                var glyphs = new GlyphCount();
                var raycasts = new RaycastProbe();

                Debug.Log("Layout_Check probe, Empresa, labels with mesh effects: " + MeshEffects(hub.transform));

                foreach (var window in ProbeWindows)
                {
                    yield return ResolutionRig.Apply(window.x, window.y);
                    yield return Settle();

                    var size = ResolutionRig.Size(window.x, window.y);
                    var scale = canvas.scaleFactor;
                    measurements.Add(ResolutionRig.Last);
                    Debug.Log("Layout_Check probe, Empresa, " + ResolutionRig.Last + "; the rig regenerated " +
                              ResolutionRig.RegeneratedTexts + " Text components");

                    glyphs.Count(hub.transform, size);
                    raycasts.Cast(canvas, hub.transform, size);

                    var shown = ResolutionRig.Frame();

                    // probe_empresa_<W>x<H>_playmode.png, only when NEKO_CAPTURE_DIR is set.
                    LayoutReport.WriteCapture("probe", "Empresa");

                    // A hidden element is found by the pixels that change, so nothing else may move.
                    yield return null;
                    var motion = Changed(shown, ResolutionRig.Frame(), window);
                    if (!motion.IsEmpty)
                    {
                        failures.Add(size + ": two renders of Empresa one frame apart differ in " + motion.Count +
                                     " pixels at " + motion + ", so a hidden element cannot be told from motion");
                        continue;
                    }

                    foreach (var probe in probes)
                    {
                        // Read before hiding: the layout is the one the shown frame was drawn from.
                        var mesh = LayoutModel.Mesh(probe.Graphic);
                        var renderer = probe.Graphic.canvasRenderer;
                        var alpha = renderer.GetAlpha();

                        renderer.SetAlpha(0f);
                        yield return null;
                        var changed = Changed(shown, ResolutionRig.Frame(), window);
                        renderer.SetAlpha(alpha);

                        string detail;
                        var agrees = probe.Graphic is Text
                            ? GlyphsAgree(mesh, changed, scale, out detail)
                            : ImageAgrees(mesh, changed, alpha, scale, window, out detail);

                        var line = size + " " + probe.Slug + " (" + probe.Description + ", " +
                                   LayoutModel.PathOf(probe.Graphic.transform) + "): " + detail + ": " +
                                   (agrees ? "agrees" : "DISAGREES");
                        Debug.Log("Layout_Check probe, Empresa, " + line);
                        if (!agrees) failures.Add(line);
                    }
                }

                foreach (var line in raycasts.Lines) Debug.Log("Layout_Check probe, Empresa, raycast " + line);
                Debug.Log("Layout_Check probe finding, whitespace: " + glyphs.Summary);
                Debug.Log("Layout_Check probe finding, RaycastAll: " + raycasts.Summary);

                if (raycasts.Answers != LayoutModel.RaycastAllAnswersInBatchMode)
                {
                    Debug.LogWarning("Layout_Check probe: LayoutModel.RaycastAllAnswersInBatchMode records " +
                                     LayoutModel.RaycastAllAnswersInBatchMode + ", but this run measured " +
                                     raycasts.Answers + ": " + raycasts.Summary);
                }

                glyphs.Check(failures);
                ResolutionRig.AssertMatchesModel(measurements);

                if (failures.Count > 0)
                {
                    Assert.Fail(failures.Count + " probe result(s) disagree with the rendered frame:\n" +
                                string.Join("\n", failures));
                }
            }
            finally
            {
                local.Destroy();
            }
        }

        /// <summary>
        /// The element model describes Empresa as the design's Baseline hierarchy facts do, at both
        /// Reference_Resolutions.
        /// </summary>
        /// <remarks>
        /// The first test of <see cref="LayoutModel.Measure"/>. It reaches Empresa as
        /// <see cref="ProbesAgreeWithTheRenderedFrame"/> does, measures it at 960×600 and at
        /// 1280×720, and checks that the Active_Layer is HubScreen; that the Interactive_Controls
        /// are the 7 buttons, each reading its label; that the
        /// wordmark is one aspect-preserving Image element; that the economy panel is one Image
        /// element with its eight labels; that every label of the screen lies inside the Viewport and
        /// inside the drawn bounds of the plate it sits on, with no Image drawn over it
        /// (<see cref="KitLegibility"/>); and that the chemistry line's Container is the Viewport.
        /// It also compares the alpha cache with <see cref="LayoutModel.SampleAlpha"/> on the
        /// economy panel's texture. With NEKO_LAYOUT_DUMP set,
        /// <see cref="LayoutModel.Dump"/> writes each measurement and the file must exist. Every
        /// finding is logged before the test fails.
        /// </remarks>
        [UnityTest]
        public IEnumerator ModelDescribesEmpresa()
        {
            yield return SceneManager.LoadSceneAsync(ScenePath, LoadSceneMode.Single);
            for (var i = 0; i < DiscardFrames; i++) yield return null;

            var root = GameRoot.Instance;
            Assert.IsTrue(root != null, "GameRoot never woke up");

            // Empresa, reached as the probes reach it: a clean business, then the play button.
            root.Ui.Get<MenuScreen>().OnNewGame.Invoke();
            yield return null;
            root.Ui.Get<MenuScreen>().OnPlay.Invoke();
            yield return null;

            var hub = root.Ui.Get<HubScreen>();
            Assert.IsTrue(hub != null && root.Ui.Current == hub, "the game did not reach Empresa");
            Assert.IsFalse(root.Ui.DialogOpen, "a Dialog is open over Empresa, so the model would measure it");

            var dumping = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(LayoutModel.DumpVariable));
            var failures = new List<string>();

            var economyRoot = hub.transform.Find(EconomyPanelName);
            Assert.IsTrue(economyRoot != null, "Empresa has no economy panel at HubScreen/" + EconomyPanelName);
            AlphaCacheAgrees(economyRoot.GetComponent<Image>(), failures);

            foreach (var window in ProbeWindows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return Settle();

                var size = ResolutionRig.Size(window.x, window.y);
                var measurement = LayoutModel.Measure(root.Ui);
                var dump = LayoutModel.Dump(measurement, "empresa");

                var found = new List<string>();
                var summary = DescribeEmpresa(measurement, hub, found);
                Debug.Log("Layout_Check model, Empresa, " + size + ": " + summary + "; dump " +
                          (dump ?? "not written, " + LayoutModel.DumpVariable + " is unset"));

                if (dumping)
                {
                    var expected = "empresa_" + window.x + "x" + window.y + ".txt";
                    if (dump == null || !Path.IsPathRooted(dump) || Path.GetFileName(dump) != expected ||
                        !File.Exists(dump) || new FileInfo(dump).Length == 0)
                    {
                        found.Add("LayoutModel.Dump did not write " + expected + " as an absolute path under " +
                                  LayoutModel.DumpVariable + ": it returned " + (dump ?? "null"));
                    }
                }
                else if (dump != null)
                {
                    found.Add("LayoutModel.Dump wrote " + dump + " although " + LayoutModel.DumpVariable + " is unset");
                }

                foreach (var line in found) failures.Add(size + ": " + line);
            }

            if (failures.Count > 0)
            {
                Assert.Fail(failures.Count + " finding(s) where the model does not describe Empresa:\n" +
                            string.Join("\n", failures));
            }
        }

        /// <summary>
        /// The click helpers inspect Empresa's seven controls at both resolutions: a real
        /// EventSystem click at the centre of each reaches that control and fires its own callback
        /// only; at 1280×720 a click on the Lojinha button must open the Loja.
        /// </summary>
        /// <remarks>
        /// Reaches Empresa as <see cref="ProbesAgreeWithTheRenderedFrame"/> does. A plank's centre is
        /// its first <see cref="Clicks.SamplePoints"/> point. At 960×600 the test logs the topmost
        /// target and handler and checks that "Voltar" still has a raycast Hit_Area. Whether the
        /// plaque blocks this point is a diagnostic, not a required outcome; the eventual R6.2
        /// rule will enforce clickability. The Loja click comes last, because it changes the Screen.
        /// Every finding is logged before the test fails.
        /// </remarks>
        [UnityTest]
        public IEnumerator ClicksFindTheBaselineTargets()
        {
            yield return SceneManager.LoadSceneAsync(ScenePath, LoadSceneMode.Single);
            for (var i = 0; i < DiscardFrames; i++) yield return null;

            var root = GameRoot.Instance;
            Assert.IsTrue(root != null, "GameRoot never woke up");

            // Empresa, reached as the probes reach it: a clean business, then the play button.
            root.Ui.Get<MenuScreen>().OnNewGame.Invoke();
            yield return null;
            root.Ui.Get<MenuScreen>().OnPlay.Invoke();
            yield return null;

            var hub = root.Ui.Get<HubScreen>();
            Assert.IsTrue(hub != null && root.Ui.Current == hub, "the game did not reach Empresa");
            Assert.IsFalse(root.Ui.DialogOpen, "a Dialog is open over Empresa, so it would take the clicks");

            var failures = new List<string>();

            // The seven callbacks are replaced by recorders for the real clicks below, and put back.
            var fired = new List<string>();
            var original = new Action[]
                { hub.OnMenu, hub.OnLineup, hub.OnRoster, hub.OnBoxes, hub.OnShop, hub.OnHall, hub.OnOpenDay };
            hub.OnMenu = () => fired.Add("Back");
            hub.OnLineup = () => fired.Add("Nav_Lineup");
            hub.OnRoster = () => fired.Add("Nav_Roster");
            hub.OnBoxes = () => fired.Add("Nav_Boxes");
            hub.OnShop = () => fired.Add("Nav_Shop");
            hub.OnHall = () => fired.Add("Nav_Hall");
            hub.OnOpenDay = () => fired.Add("Cta_OpenDay");

            // At both windows, a click at the centre of each of the seven controls must travel the
            // EventSystem (RaycastAll, then the pointer-click handler) to that control, fire its own
            // callback once and no other, and the control must have a raycast Hit_Area.
            foreach (var window in ProbeWindows)
            {
                yield return ResolutionRig.Apply(window.x, window.y);
                yield return Settle();

                var windowSize = ResolutionRig.Size(window.x, window.y);
                var clicks = new Clicks(LayoutModel.Measure(root.Ui));

                foreach (var name in EmpresaControlNames)
                {
                    var control = KitControl(clicks.Measurement, name);
                    var points = clicks.SamplePoints(control);
                    Assert.IsTrue(points.Count > 0, windowSize + ": \"" + name + "\" draws nothing, so it has no centre");

                    var centre = points[0];
                    var area = clicks.HitAreaOf(control);
                    var top = TargetPath(clicks.Measurement, clicks.TopTarget(centre));
                    var handlerAt = clicks.TopHandler(centre);
                    Debug.Log("Layout_Check clicks, Empresa, " + windowSize + ": " + name + " " + control.Path + ", rect " +
                              LayoutModel.Describe(control.OwnRect) + ", Hit_Area " + area + ", centre " +
                              LayoutReport.Point(centre) + ", topmost raycast target " + top + ", " +
                              (handlerAt == control.Component.gameObject ? "reachable" : "BLOCKED"));

                    if (area.Targets.Count == 0 || !area.Bounds.HasValue)
                    {
                        failures.Add(windowSize + ": \"" + name + "\" has no raycast Hit_Area: " + area);
                    }

                    fired.Clear();
                    var received = clicks.Click(centre);
                    if (received != control.Component.gameObject)
                    {
                        failures.Add(windowSize + ": a click at the centre of \"" + name + "\" " + LayoutReport.Point(centre) +
                                     " reached " + TargetPath(clicks.Measurement, received) + " through " + top +
                                     ", not the control " + control.Path);
                    }

                    if (fired.Count != 1 || fired[0] != name)
                    {
                        failures.Add(windowSize + ": a click on \"" + name + "\" fired [" + string.Join(", ", fired) + "]");
                    }
                }
            }

            hub.OnMenu = original[0];
            hub.OnLineup = original[1];
            hub.OnRoster = original[2];
            hub.OnBoxes = original[3];
            hub.OnShop = original[4];
            hub.OnHall = original[5];
            hub.OnOpenDay = original[6];

            // 1280×720: a click at the centre of the Lojinha button, last because it changes the Screen.
            yield return ResolutionRig.Apply(1280, 720);
            yield return Settle();

            var size = ResolutionRig.Size(1280, 720);
            var shopClicks = new Clicks(LayoutModel.Measure(root.Ui));
            var shop = KitControl(shopClicks.Measurement, "Nav_Shop");
            var shopPoints = shopClicks.SamplePoints(shop);
            Assert.IsTrue(shopPoints.Count > 0, size + ": the Lojinha button draws nothing, so it has no centre");

            var shopCentre = shopPoints[0];
            var shopArea = shopClicks.HitAreaOf(shop);
            var shopTarget = TargetPath(shopClicks.Measurement, shopClicks.TopTarget(shopCentre));
            var navigated = shopClicks.Click(shopCentre);
            yield return Settle();

            var receivedPath = TargetPath(shopClicks.Measurement, navigated);
            var current = root.Ui.Current;
            var currentName = current != null ? current.GetType().Name : "none";
            Debug.Log("Layout_Check clicks, Empresa, " + size + ": " + shopClicks.Targets.Count + " raycast targets; " +
                      "Lojinha " + shop.Path + ", rect " + LayoutModel.Describe(shop.OwnRect) + ", Hit_Area " + shopArea +
                      ", " + shopPoints.Count + " sample points; at its centre " + LayoutReport.Point(shopCentre) +
                      " the topmost raycast target is " + shopTarget + ", the click reached " + receivedPath +
                      ", and the current Screen is now " + currentName);

            if (navigated != shop.Component.gameObject)
            {
                failures.Add(size + ": a click at the centre of the Lojinha button " + LayoutReport.Point(shopCentre) +
                             " reached " + receivedPath + " through " + shopTarget + ", not the button " + shop.Path);
            }

            if (!(current is ShopScreen))
            {
                failures.Add(size + ": after the click on the Lojinha button the current Screen is " + currentName +
                             ", not ShopScreen");
            }

            if (failures.Count > 0)
            {
                Assert.Fail(failures.Count + " click finding(s) on Empresa:\n" + string.Join("\n", failures));
            }
        }

        /// <summary>
        /// <see cref="SessionFactory"/> boots a crafted profile: the session holds every value the
        /// profile was given, and Empresa comes up through the Menu's play plank.
        /// </summary>
        /// <remarks>
        /// The profile is café level 3 on day 7, with 123456 ¥, 345 fichas, records, four named
        /// maids at explicit rarities, levels, bonds and traits, an explicit lineup, a table placed
        /// in the room, and furniture and clothes in stock. <see cref="SessionFactory.Boot"/> compares
        /// the whole profile; the test then reads the values back from the session on Empresa. It
        /// measures no layout, so no Baseline layout defect can fail it. It also checks that a
        /// missing reflection target fails with its type and member name.
        /// </remarks>
        [UnityTest]
        public IEnumerator SessionFactoryBootsACraftedProfile()
        {
            var missing = Assert.Throws<AssertionException>(() =>
                SessionFactory.RequireMember<FieldInfo>(typeof(Lootbox), "NoSuchNamePool",
                    BindingFlags.NonPublic | BindingFlags.Static));
            StringAssert.Contains(typeof(Lootbox).FullName + ".NoSuchNamePool", missing.Message);

            // Each maid: name, rarity, level, bond, then her traits.
            var crafted = SessionFactory.NewProfile(4242u, 3)
                .WithMoney(123456)
                .WithFichas(345)
                .WithDay(7)
                .WithRecords(bestDay: 6, bestRevenue: 2345, customersServed: 321, daysPlayed: 6,
                    businessesLost: 1, legendariesFound: 1)
                .WithMaid("Aoi", Rarity.Legendary, 12, 5, TraitId.Lucky, TraitId.Hostess, TraitId.Charisma)
                .WithMaid("Mina", Rarity.Epic, 7, 3, TraitId.QuickStep)
                .WithMaid("Yuki", Rarity.Rare, 3, 1)
                .WithMaid("Suzu", Rarity.Common, 1, 0)
                .Assign(Role.Reception, 0, "Aoi")
                .Assign(Role.Floor, 0, "Mina")
                .Assign(Role.Floor, 1, "Yuki")
                .Assign(Role.Kitchen, 0, "Suzu")
                .WithItem("table_4", 7, 4)
                .WithStock("table_4", 12)
                .WithStock("head_sakura", 2)
                .Build();

            yield return SessionFactory.Boot(crafted);

            var root = GameRoot.Instance;
            var menu = root.Ui.Get<MenuScreen>();
            Debug.Log("Layout_Check session, booted: " + Tween.LiveCount + " live tweens, the Menu at scale " +
                      Number(menu.transform.localScale.x) + "; the session holds\n" +
                      SessionFactory.Describe(root.Session.Profile));

            yield return SessionFactory.ToHub();

            var profile = root.Session.Profile;
            Assert.AreEqual(123456, profile.Wallet.Money, "the money");
            Assert.AreEqual(345, profile.Wallet.Points, "the fichas");
            Assert.AreEqual(7, profile.Day, "the day");
            Assert.AreEqual(CafeLevelTable.ReputationForLevel(3), profile.Wallet.Reputation, "the reputation");
            Assert.AreEqual(3, profile.CafeLevel, "the café level");
            Assert.AreEqual(6, profile.Records.BestDay, "the best day");

            var unlocks = CafeLevelTable.UnlocksAt(3);
            Assert.AreEqual(unlocks.GridWidth + "x" + unlocks.GridHeight, profile.Grid.Width + "x" + profile.Grid.Height,
                "the room does not have the size of café level 3");

            var names = new List<string>();
            foreach (var maid in profile.Maids) names.Add(maid.Name);
            CollectionAssert.AreEqual(new[] { "Aoi", "Mina", "Yuki", "Suzu" }, names, "the roster");

            var aoi = profile.Maids[0];
            Assert.AreEqual(Rarity.Legendary, aoi.Rarity, "Aoi's rarity");
            Assert.AreEqual(12, aoi.Level, "Aoi's level");
            Assert.AreEqual(5, aoi.Bond, "Aoi's bond");
            CollectionAssert.AreEqual(new[] { TraitId.Lucky, TraitId.Hostess, TraitId.Charisma }, aoi.Traits,
                "Aoi's traits");
            Assert.AreEqual(profile.Maids[2].Id, profile.Lineup.Slot(Role.Floor, 1).MaidId,
                "Yuki is not in the second Floor slot");

            var tables = 0;
            foreach (var id in profile.OwnedPlaceables)
            {
                if (id == "table_4") tables++;
            }

            Assert.AreEqual(12, tables, "the tables for four in stock");

            var hub = root.Ui.Get<HubScreen>();
            Assert.IsTrue(hub != null && root.Ui.Current == hub && hub.Visible,
                "Empresa is not the current Screen; " + (root.Ui.Current != null ? root.Ui.Current.GetType().Name : "none") +
                " is");
        }

        /// <summary>
        /// The value providers of <see cref="SessionFactory"/> pick from the game's tables: every
        /// Longest provider ranks its candidates widest first, and the longest maid name is the
        /// widest name of the closed catalog.
        /// </summary>
        /// <remarks>
        /// Calls every provider once and logs its pick, width and runner-up, then the Large_Values
        /// as the Baseline formats them; everything is logged before the test fails. It measures
        /// text in the game font and no layout, so no Baseline layout defect can fail it. It also
        /// checks that a provider without candidates fails with its name.
        /// </remarks>
        [Test]
        public void SessionFactoryPicksTheLongestValues()
        {
            var empty = Assert.Throws<AssertionException>(() =>
                SessionFactory.Longest.Widest("NoSuchField", new string[0], text => text, Typeface.Tiny, false));
            StringAssert.Contains("SessionFactory.Longest.NoSuchField", empty.Message);

            var failures = new List<string>();
            var maidName = SessionFactory.Longest.MaidName();
            var lines = new List<string>
            {
                Ranked(maidName, failures),
                Ranked(SessionFactory.Longest.PoolName(), failures),
                Ranked(SessionFactory.Longest.ArchetypeName(), failures),
                Ranked(SessionFactory.Longest.PositionName(), failures),
                Ranked(SessionFactory.Longest.PositionDescription(), failures),
                Ranked(SessionFactory.Longest.ChemistryTagLine(), failures),
                Ranked(SessionFactory.Longest.TraitLine(), failures),
                Ranked(SessionFactory.Longest.SignatureLine(), failures),
                Ranked(SessionFactory.Longest.FlavourLine(), failures),
                Ranked(SessionFactory.Longest.BoxDescription(), failures),
                Ranked(SessionFactory.Longest.PlaceableName(), failures),
                Ranked(SessionFactory.Longest.ThemeName(), failures),
            };

            foreach (ShopItemKind tab in Enum.GetValues(typeof(ShopItemKind)))
            {
                lines.Add(Ranked(SessionFactory.Longest.ShopItemName(tab), failures));
                lines.Add(Ranked(SessionFactory.Longest.ShopItemDescription(tab), failures));
            }

            foreach (var line in lines) Debug.Log("Layout_Check values, " + line);
            Debug.Log("Layout_Check values, Large_Values as the Baseline formats them: balance " +
                      SessionFactory.LargeValues.BalanceText + ", day amount " + SessionFactory.LargeValues.DayAmountText +
                      ", profit " + SessionFactory.LargeValues.ProfitText + ", loss " + SessionFactory.LargeValues.LossText +
                      ", salary " + SessionFactory.LargeValues.SalaryText + ", Elenco header " +
                      SessionFactory.LargeValues.RosterHeader(8, 8, SessionFactory.LargeValues.DayAmount) + ", dismiss plank " +
                      SessionFactory.LargeValues.DismissPlank(SessionFactory.LargeValues.Severance));

            if (!System.Linq.Enumerable.Any(Neko.Sim.Maids.MaidCatalog.All, identity => identity.Name == maidName.Text))
            {
                failures.Add(maidName.Field + " picked \"" + maidName.Text + "\", which is not a name of the closed " +
                             "maid catalog (boxes no longer number names)");
            }

            if (failures.Count > 0)
            {
                Assert.Fail(failures.Count + " value provider finding(s):\n" + string.Join("\n", failures));
            }
        }

        /// <summary>
        /// <see cref="SessionFactory.ReplaceDisplay(Transform, Func{int, string}, int, int)"/> composes
        /// the one label that reads the display string again from the envelope value, and fails,
        /// listing the labels, when two read it or none does.
        /// </summary>
        /// <remarks>
        /// Builds the Elenco's two salary labels with <c>UiKit.Label</c> under a bare layer: the
        /// detail's "Salário" value, which reads only the amount, and a card's salary line, which
        /// contains it. It measures no layout.
        /// </remarks>
        [Test]
        public void SessionFactoryReplacesExactlyOneLabel()
        {
            const int salary = 1234;
            var layer = new GameObject("Layer", typeof(RectTransform)).transform;
            try
            {
                var card = UiKit.Label(layer, SessionFactory.LargeValues.CardSalary(salary), Typeface.Tiny, Color.white);
                var fact = UiKit.Label(layer, SessionFactory.LargeValues.SalaryFact(salary), Typeface.Small, Color.white);

                var replaced = SessionFactory.ReplaceDisplay(layer, SessionFactory.LargeValues.SalaryFact, salary,
                    SessionFactory.LargeValues.Salary);
                Assert.AreSame(fact, replaced, "the replacement did not land on the detail's value");
                Assert.AreEqual(SessionFactory.LargeValues.SalaryText, fact.text, "the detail's value");
                Assert.AreEqual(SessionFactory.LargeValues.CardSalary(salary), card.text,
                    "the card's salary line, which only contains the amount, changed");

                UiKit.Label(layer, SessionFactory.LargeValues.CardSalary(salary), Typeface.Tiny, Color.white);
                var twice = Assert.Throws<AssertionException>(() => SessionFactory.ReplaceDisplay(layer,
                    SessionFactory.LargeValues.CardSalary, salary, SessionFactory.LargeValues.Salary));
                StringAssert.Contains("2 labels", twice.Message);

                // The detail now reads the envelope, and the two cards only contain the amount.
                var none = Assert.Throws<AssertionException>(() => SessionFactory.ReplaceDisplay(layer,
                    SessionFactory.LargeValues.SalaryFact, salary, SessionFactory.LargeValues.Salary));
                StringAssert.Contains("no active label", none.Message);
                StringAssert.Contains("these contain it", none.Message);

                Debug.Log("Layout_Check values, ReplaceDisplay refusals:\n" + twice.Message + "\n" + none.Message);
            }
            finally
            {
                Object.Destroy(layer.gameObject);
            }
        }

        /// <summary>
        /// Adds a finding unless <paramref name="pick"/> has a positive width and ranks its
        /// candidates widest first; returns the pick for the log.
        /// </summary>
        private static string Ranked<T>(LongestPick<T> pick, List<string> failures)
        {
            if (!(pick.Width > 0f))
            {
                failures.Add(pick.Field + " measured \"" + pick.Text + "\" as " + Number(pick.Width) + " units wide");
            }

            for (var i = 1; i < pick.Ranking.Count; i++)
            {
                var before = pick.Ranking[i - 1];
                var after = pick.Ranking[i];
                if (after.Width <= before.Width) continue;

                failures.Add(pick.Field + " ranks \"" + after.Text + "\" (" + Number(after.Width) + ") after the narrower \"" +
                             before.Text + "\" (" + Number(before.Width) + ")");
            }

            return pick.ToString();
        }

        /// <summary>
        /// Checks one measurement of Empresa against the Baseline hierarchy facts, adds what
        /// disagrees to <paramref name="found"/>, and returns a summary for the log.
        /// </summary>
        private static string DescribeEmpresa(Measurement measurement, HubScreen hub, List<string> found)
        {
            if (measurement.Layer != hub.transform)
            {
                found.Add("the Active_Layer is " + measurement.LayerPath + ", not " + LayoutModel.PathOf(hub.transform));
            }

            // The seven Interactive_Controls, each reading its label.
            var labels = EmpresaControlLabels();
            if (measurement.Controls.Count != EmpresaControlNames.Length)
            {
                found.Add("Empresa has " + measurement.Controls.Count + " Interactive_Controls, not " +
                          EmpresaControlNames.Length + ": " + string.Join(", ", measurement.Controls));
            }

            for (var i = 0; i < EmpresaControlNames.Length; i++)
            {
                InteractiveControl control = null;
                foreach (var candidate in measurement.Controls)
                {
                    if (candidate.Component.name == EmpresaControlNames[i]) control = candidate;
                }

                if (control == null)
                {
                    found.Add("Empresa has no Interactive_Control named " + EmpresaControlNames[i]);
                    continue;
                }

                if (control.Kind != ControlKind.Selectable) found.Add(control.Path + " is not a Selectable");

                var label = control.RectTransform.Find("Label");
                var text = label != null ? label.GetComponent<Text>() : null;
                if (text == null || text.text != labels[i])
                {
                    found.Add(control.Path + " reads \"" + (text != null ? text.text : "(no label)") +
                              "\", not \"" + labels[i] + "\"");
                }
                else if (measurement.ElementOf(label) == null)
                {
                    found.Add("the label \"" + text.text + "\" of " + control.Path + " is not an element");
                }
            }

            // Every label of the screen can be read: in the Viewport, inside its plate's drawn bounds,
            // under no later Image.
            var legible = KitLegibility.Check(measurement, hub.transform, "Empresa", found);

            // The wordmark: one aspect-preserving picture.
            var logo = hub.transform.Find(WordmarkName);
            var mark = measurement.ElementOf(logo);
            if (logo == null || mark == null || mark.Kind != ElementKind.Image ||
                !(mark.Graphic is Image) || !((Image)mark.Graphic).preserveAspect)
            {
                found.Add("Empresa has no aspect-preserving wordmark Image at HubScreen/" + WordmarkName +
                          (mark != null ? " (it is " + mark + ")" : ""));
            }

            // The economy panel: one Image element, drawn inside the Viewport, with its eight labels.
            var panelRoot = hub.transform.Find(EconomyPanelName);
            var panel = measurement.ElementOf(panelRoot);
            var panelSummary = "no economy panel";
            if (panelRoot == null || panel == null || panel.Kind != ElementKind.Image)
            {
                found.Add("the economy panel HubScreen/" + EconomyPanelName + " is not one Image element");
            }
            else
            {
                var drawn = panel.Shape.DrawnBounds;
                if (!drawn.HasValue)
                {
                    found.Add("the economy panel draws no point");
                }
                else if (!measurement.Viewport.Contains(drawn.Value.min) || !measurement.Viewport.Contains(drawn.Value.max))
                {
                    found.Add("the economy panel's drawn bounds " + LayoutModel.Describe(drawn.Value) +
                              " are not inside the Viewport " + LayoutModel.Describe(measurement.Viewport));
                }

                var inside = 0;
                foreach (var element in measurement.Elements)
                {
                    if (element.IsText && element.Transform.IsChildOf(panelRoot)) inside++;
                }

                if (inside != EconomyLabels)
                {
                    found.Add("the economy panel holds " + inside + " label elements, not " + EconomyLabels);
                }

                panelSummary = "economy panel rect " + LayoutModel.Describe(panel.OwnRect) + ", drawn bounds " +
                               (drawn.HasValue ? LayoutModel.Describe(drawn.Value) : "none") + ", " + inside + " labels";
            }

            // The chemistry line: on the Screen root, framed by nothing but the Viewport.
            var chemistry = measurement.ElementOf(ChemistryLine(hub).transform);
            if (chemistry == null) found.Add("the chemistry line is not an element");
            else if (chemistry.Container != null)
            {
                found.Add("the chemistry line's Container is " + chemistry.Container.Path + ", not the Viewport");
            }

            return "layer " + measurement.LayerPath + ", scale factor " + Number(measurement.ScaleFactor) + "; " +
                   measurement.Elements.Count + " elements, " + measurement.Controls.Count +
                   " Interactive_Controls, " + legible + " legible labels; wordmark " +
                   (mark != null ? mark.ToString() : "none") + "; " + panelSummary +
                   "; chemistry line container " +
                   (chemistry != null ? ContainerName(chemistry.Container) : "(no element)");
        }

        /// <summary>
        /// Compares <see cref="AlphaCache"/> with <see cref="LayoutModel.SampleAlpha"/> on the texture of
        /// <paramref name="image"/>, along three lines across its sprite: the middle row, the middle
        /// column and the diagonal. They must agree within 1/255.
        /// </summary>
        /// <remarks>
        /// The lines cross the clear margin, the edge where the alpha ramps (at a rounded corner on
        /// the diagonal) and the opaque middle, at a step that is not a whole number of texels, so
        /// the samples fall between texel centres. Some samples must be partly transparent, or the
        /// comparison would not have tested the filter where the 0.5 contour lies.
        /// </remarks>
        private static void AlphaCacheAgrees(Image image, List<string> failures)
        {
            var sprite = image != null ? image.overrideSprite : null;
            var texture = sprite != null ? sprite.texture : null;
            Assert.IsTrue(texture != null && texture.isReadable,
                "the economy panel's texture cannot be read on the CPU, so the alpha cache cannot be compared");
            Assert.IsTrue(texture.wrapModeU == TextureWrapMode.Clamp && texture.wrapModeV == TextureWrapMode.Clamp,
                "the economy panel's texture does not clamp, so the alpha cache would sample through SampleAlpha " +
                "and the comparison would compare it with itself");

            const float stepTexels = 0.37f;
            var area = sprite.textureRect;
            var steps = Mathf.CeilToInt(Mathf.Max(area.width, area.height) / stepTexels);
            var middle = new Vector2((area.center.x + 0.27f) / texture.width, (area.center.y + 0.27f) / texture.height);
            var uvs = new List<Vector2>();
            for (var i = 0; i <= steps; i++)
            {
                var u = Mathf.Lerp(area.xMin, area.xMax, i / (float)steps) / texture.width;
                var v = Mathf.Lerp(area.yMin, area.yMax, i / (float)steps) / texture.height;
                uvs.Add(new Vector2(u, middle.y));
                uvs.Add(new Vector2(middle.x, v));
                uvs.Add(new Vector2(u, v));
            }

            var cache = new AlphaCache(texture);
            var worst = 0f;
            var worstAt = Vector2.zero;
            var partial = 0;
            foreach (var uv in uvs)
            {
                var cached = cache.Sample(uv);
                var sampled = LayoutModel.SampleAlpha(texture, uv);
                if (sampled > 0f && sampled < 1f) partial++;

                var difference = Mathf.Abs(cached - sampled);
                if (difference <= worst) continue;
                worst = difference;
                worstAt = uv;
            }

            var line = "alpha cache against SampleAlpha on " + texture.name + " (" + texture.width + "x" +
                       texture.height + ", " + texture.format + ", " + texture.filterMode + "): " + uvs.Count +
                       " UVs, " + partial + " of them partly transparent; worst difference " +
                       worst.ToString("0.######", CultureInfo.InvariantCulture) + " at UV " +
                       worstAt.x.ToString("0.####", CultureInfo.InvariantCulture) + ", " +
                       worstAt.y.ToString("0.####", CultureInfo.InvariantCulture);
            Debug.Log("Layout_Check model, Empresa, " + line);
            if (worst > 1f / 255f) failures.Add(line + ", more than 1/255");
            if (partial == 0) failures.Add(line + ": no sample was partly transparent, so the filter was not tested");
        }

        private static string ContainerName(MeasuredElement container) =>
            container != null ? container.Path : "the Viewport";

        /// <summary>
        /// The design's Settled, through <see cref="SessionFactory.Settle"/>: one-shot tweens run out,
        /// the layout is rebuilt and one frame drawn, and one more tick moves nothing.
        /// </summary>
        private static IEnumerator Settle() => SessionFactory.Settle();

        /// <summary>The Hub backdrop: the Simple (not aspect-preserving, not sliced) image named Scenery.</summary>
        private static Image BackdropImage(HubScreen hub)
        {
            var scenery = hub.transform.Find("Scenery");
            var image = scenery != null ? scenery.GetComponent<Image>() : null;
            Assert.IsTrue(image != null && image.type == Image.Type.Simple && !image.preserveAspect,
                "Empresa has no Simple backdrop Image at HubScreen/Scenery");
            Assert.IsTrue(image.sprite != null && image.sprite.texture.isReadable,
                "the backdrop's texture cannot be read on the CPU");
            return image;
        }

        /// <summary>The face (the control's own Image) of the button named <paramref name="name"/>.</summary>
        private static Image KitFace(HubScreen hub, string name, Image.Type type)
        {
            var node = hub.transform.Find("Row[1]/" + name) ?? hub.transform.Find(name);
            if (node == null)
            {
                foreach (var button in hub.GetComponentsInChildren<Button>(true))
                {
                    if (button.name == name) node = button.transform;
                }
            }

            var face = node != null ? node.GetComponent<Image>() : null;
            Assert.IsTrue(face != null, "Empresa has no button named \"" + name + "\"");
            Assert.AreEqual(type, face.type, "the face of \"" + name + "\" is not a " + type + " image");
            Assert.IsTrue(face.sprite != null && face.sprite.texture.isReadable,
                "the texture of \"" + name + "\" cannot be read on the CPU, so its drawn pixels cannot be predicted");
            return face;
        }

        private static Image EconomyPanelImage(HubScreen hub)
        {
            var panel = hub.transform.Find(EconomyPanelName);
            var image = panel != null ? panel.GetComponent<Image>() : null;
            Assert.IsTrue(image != null && image.preserveAspect,
                "Empresa has no aspect-preserving image at HubScreen/" + EconomyPanelName);
            return image;
        }

        private static Text ChemistryLine(HubScreen hub)
        {
            var prefix = Strings.Get("lineup.chemistry");
            Text found = null;
            foreach (var text in hub.GetComponentsInChildren<Text>())
            {
                if (!text.text.StartsWith(prefix, StringComparison.Ordinal)) continue;

                Assert.IsTrue(found == null, "Empresa has more than one label starting with \"" + prefix + "\"");
                found = text;
            }

            Assert.IsTrue(found != null, "Empresa has no label starting with \"" + prefix + "\"");
            Assert.IsTrue(found.GetComponent<BaseMeshEffect>() == null,
                "the chemistry line carries a mesh effect, so the pixels it changes are not its glyphs alone");
            return found;
        }

        /// <summary>The Interactive_Control named <paramref name="name"/>.</summary>
        private static InteractiveControl KitControl(Measurement measurement, string name)
        {
            InteractiveControl found = null;
            foreach (var control in measurement.Controls)
            {
                if (control.Kind != ControlKind.Selectable || control.Component.name != name) continue;

                Assert.IsTrue(found == null, "Empresa has more than one control named \"" + name + "\"");
                found = control;
            }

            Assert.IsTrue(found != null, "Empresa has no control named \"" + name + "\" among " +
                                         string.Join(", ", measurement.Controls));
            return found;
        }

        /// <summary>The path of <paramref name="target"/> from the measured Active_Layer root, or "(none)".</summary>
        private static string TargetPath(Measurement measurement, GameObject target) =>
            target != null ? LayoutModel.PathFrom(measurement.Layer, target.transform) : "(none)";

        /// <summary>Every label of a screen that a mesh effect copies, grouped by effect and parent.</summary>
        private static string MeshEffects(Transform screen)
        {
            var groups = new SortedDictionary<string, List<string>>(StringComparer.Ordinal);
            foreach (var text in screen.GetComponentsInChildren<Text>())
            {
                foreach (var effect in text.GetComponents<BaseMeshEffect>())
                {
                    var parent = text.transform.parent != null ? text.transform.parent.name : "(none)";
                    var key = effect.GetType().Name + " under " + parent;
                    if (!groups.TryGetValue(key, out var texts)) groups[key] = texts = new List<string>();
                    texts.Add("\"" + text.text + "\"");
                }
            }

            if (groups.Count == 0) return "none";

            var parts = new List<string>();
            foreach (var pair in groups)
            {
                parts.Add(pair.Key + " (" + pair.Value.Count + "): " + string.Join(" ", pair.Value));
            }

            return string.Join("; ", parts);
        }

        /// <summary>The bounds of the pixels that differ between two frames of one window.</summary>
        private static PixelBox Changed(Color32[] before, Color32[] after, Vector2Int window)
        {
            Assert.AreEqual(before.Length, after.Length, "two frames of the same window differ in size");

            var box = new PixelBox();
            for (var i = 0; i < before.Length; i++)
            {
                var a = before[i];
                var b = after[i];
                if (a.r != b.r || a.g != b.g || a.b != b.b) box.Include(i % window.x, i / window.x);
            }

            return box;
        }

        /// <summary>
        /// Whether the pixels an image changed lie inside the pixels where its mesh samples any
        /// alpha and cover the pixels where it samples at least half, within the tolerance.
        /// </summary>
        private static bool ImageAgrees(GraphicMesh mesh, PixelBox changed, float rendererAlpha, float scale,
            Vector2Int window, out string detail)
        {
            var texture = mesh.Graphic.mainTexture as Texture2D;
            Assert.IsTrue(texture != null && texture.isReadable,
                "the texture of " + LayoutModel.PathOf(mesh.Graphic.transform) +
                " cannot be read on the CPU, so its drawn pixels cannot be predicted");

            var any = new PixelBox();
            var half = new PixelBox();
            Predict(mesh, texture, rendererAlpha, scale, window, any, half);

            detail = "mesh of " + mesh.Vertices.Count + " vertices at " + Pixels(mesh.Bounds, scale) +
                     "; it samples alpha >= 0.5 at " + half + " and alpha > 0 at " + any +
                     "; changed pixels " + changed + " (" + changed.Count + "); tolerance " + EdgeTolerancePx + " px";
            return changed.Within(any, EdgeTolerancePx) && half.Within(changed, EdgeTolerancePx);
        }

        /// <summary>
        /// Whether the pixels a label changed lie inside its glyph quads and reach their edges,
        /// within the tolerances.
        /// </summary>
        private static bool GlyphsAgree(GraphicMesh mesh, PixelBox changed, float scale, out string detail)
        {
            var quads = mesh.QuadCount;
            var glyphs = new PixelBox();
            for (var q = 0; q < quads; q++)
            {
                var quad = mesh.QuadBounds(q);
                var x0 = FirstPixel(quad.xMin * scale);
                var x1 = LastPixel(quad.xMax * scale);
                var y0 = FirstPixel(quad.yMin * scale);
                var y1 = LastPixel(quad.yMax * scale);
                if (x1 < x0 || y1 < y0) continue;

                glyphs.Include(x0, y0);
                glyphs.Include(x1, y1);
            }

            detail = "\"" + ((Text)mesh.Graphic).text + "\", " + quads + " glyph quads at " +
                     Pixels(mesh.Bounds, scale) + " cover the pixels " + glyphs + "; changed pixels " + changed +
                     " (" + changed.Count + "); tolerance " + EdgeTolerancePx + " px outside, " +
                     GlyphInkTolerancePx + " px inside";
            return changed.Within(glyphs, EdgeTolerancePx) && glyphs.Within(changed, GlyphInkTolerancePx);
        }

        /// <summary>
        /// Fills <paramref name="any"/> with every pixel whose centre the mesh covers with a
        /// non-zero alpha, and <paramref name="half"/> with every pixel it covers with an alpha
        /// of at least 0.5: texture alpha, sampled as the GPU filters it at the interpolated UV,
        /// times the vertex and renderer alpha.
        /// </summary>
        private static void Predict(GraphicMesh mesh, Texture2D texture, float rendererAlpha, float scale,
            Vector2Int window, PixelBox any, PixelBox half)
        {
            const float slack = 1e-5f;
            var corners = mesh.TriangleCorners;

            for (var t = 0; t + 2 < corners.Count; t += 3)
            {
                var a = corners[t];
                var b = corners[t + 1];
                var c = corners[t + 2];

                var pa = a.Position * scale;
                var pb = b.Position * scale;
                var pc = c.Position * scale;

                var area = Cross(pb - pa, pc - pa);
                if (Mathf.Abs(area) < 1e-6f) continue;

                var x0 = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(pa.x, Mathf.Min(pb.x, pc.x))));
                var x1 = Mathf.Min(window.x - 1, Mathf.CeilToInt(Mathf.Max(pa.x, Mathf.Max(pb.x, pc.x))));
                var y0 = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(pa.y, Mathf.Min(pb.y, pc.y))));
                var y1 = Mathf.Min(window.y - 1, Mathf.CeilToInt(Mathf.Max(pa.y, Mathf.Max(pb.y, pc.y))));

                for (var y = y0; y <= y1; y++)
                {
                    for (var x = x0; x <= x1; x++)
                    {
                        var p = new Vector2(x + 0.5f, y + 0.5f);
                        var wa = Cross(pc - pb, p - pb) / area;
                        var wb = Cross(pa - pc, p - pc) / area;
                        var wc = 1f - wa - wb;
                        if (wa < -slack || wb < -slack || wc < -slack) continue;

                        var uv = a.Uv * wa + b.Uv * wb + c.Uv * wc;
                        var vertexAlpha = (a.Colour.a * wa + b.Colour.a * wb + c.Colour.a * wc) / 255f;
                        var drawn = LayoutModel.SampleAlpha(texture, uv) * vertexAlpha * rendererAlpha;

                        if (drawn > 0f) any.Include(x, y);
                        if (drawn >= 0.5f) half.Include(x, y);
                    }
                }
            }
        }

        /// <summary>The first pixel whose centre lies at or after <paramref name="edge"/>.</summary>
        private static int FirstPixel(float edge) => Mathf.CeilToInt(edge - 0.5f);

        /// <summary>The last pixel whose centre lies before <paramref name="edge"/>.</summary>
        private static int LastPixel(float edge) => Mathf.CeilToInt(edge - 0.5f) - 1;

        private static float Cross(Vector2 u, Vector2 v) => u.x * v.y - u.y * v.x;

        /// <summary>A rect of Canvas_Units in window pixels.</summary>
        private static string Pixels(Rect rect, float scale) =>
            "x " + Number(rect.xMin * scale) + ".." + Number(rect.xMax * scale) + ", y " +
            Number(rect.yMin * scale) + ".." + Number(rect.yMax * scale) + " px";

        private static string Number(float value) => value.ToString("0.##", CultureInfo.InvariantCulture);

        /// <summary>One element a probe hides.</summary>
        private sealed class Probe
        {
            public Probe(string slug, string description, Graphic graphic)
            {
                Slug = slug;
                Description = description;
                Graphic = graphic;
            }

            public string Slug { get; }

            public string Description { get; }

            public Graphic Graphic { get; }
        }

        /// <summary>A box of window pixels, edges included, with the origin at the bottom-left.</summary>
        private sealed class PixelBox
        {
            public int XMin { get; private set; } = int.MaxValue;

            public int YMin { get; private set; } = int.MaxValue;

            public int XMax { get; private set; } = int.MinValue;

            public int YMax { get; private set; } = int.MinValue;

            /// <summary>How many pixels were included.</summary>
            public int Count { get; private set; }

            public bool IsEmpty => Count == 0;

            public void Include(int x, int y)
            {
                if (x < XMin) XMin = x;
                if (x > XMax) XMax = x;
                if (y < YMin) YMin = y;
                if (y > YMax) YMax = y;
                Count++;
            }

            /// <summary>
            /// Whether this box lies inside <paramref name="other"/> grown by
            /// <paramref name="margin"/> pixels on each side. Never true for an empty box.
            /// </summary>
            public bool Within(PixelBox other, int margin) =>
                !IsEmpty && !other.IsEmpty &&
                XMin >= other.XMin - margin && XMax <= other.XMax + margin &&
                YMin >= other.YMin - margin && YMax <= other.YMax + margin;

            public override string ToString() =>
                IsEmpty ? "nowhere" : "x " + XMin + ".." + XMax + ", y " + YMin + ".." + YMax;
        }

        /// <summary>
        /// The two images Empresa lacks, built on its screen for the probes and destroyed after.
        /// </summary>
        /// <remarks>
        /// One texel is one Canvas_Unit, so the sliced borders are 8 units. Both images sit left
        /// of the today strip, where they cover no other probe, and neither is a raycast target.
        /// </remarks>
        private sealed class LocalImages
        {
            private readonly List<Object> _owned = new List<Object>();

            public LocalImages(Transform screen, float referencePixelsPerUnit)
            {
                // 32×32 magenta whose outer two texels are clear.
                var bordered = new Color32[32 * 32];
                for (var i = 0; i < bordered.Length; i++)
                {
                    var x = i % 32;
                    var y = i / 32;
                    var inside = x >= 2 && x < 30 && y >= 2 && y < 30;
                    bordered[i] = new Color32(255, 0, 255, (byte)(inside ? 255 : 0));
                }

                Sliced = Build(screen, "Layout_Check probe: sliced", 32, 32, bordered, new Vector4(8f, 8f, 8f, 8f),
                    referencePixelsPerUnit, new Vector2(200f, 100f), new Vector2(-400f, 60f));
                Sliced.type = Image.Type.Sliced;

                // 32×16 opaque cyan: a sprite twice as wide as it is tall.
                var wide = new Color32[32 * 16];
                for (var i = 0; i < wide.Length; i++) wide[i] = new Color32(0, 255, 255, 255);

                Letterbox = Build(screen, "Layout_Check probe: letterbox", 32, 16, wide, Vector4.zero,
                    referencePixelsPerUnit, new Vector2(100f, 100f), new Vector2(-400f, -40f));
                Letterbox.preserveAspect = true;
            }

            public Image Sliced { get; }

            public Image Letterbox { get; }

            public void Destroy()
            {
                for (var i = _owned.Count - 1; i >= 0; i--)
                {
                    if (_owned[i] != null) Object.Destroy(_owned[i]);
                }

                _owned.Clear();
            }

            private Image Build(Transform parent, string name, int width, int height, Color32[] pixels,
                Vector4 border, float pixelsPerUnit, Vector2 size, Vector2 offset)
            {
                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
                {
                    name = name,
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                };
                texture.SetPixels32(pixels);
                texture.Apply(false, false);
                _owned.Add(texture);

                var sprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f),
                    pixelsPerUnit, 0, SpriteMeshType.FullRect, border);
                sprite.name = name;
                _owned.Add(sprite);

                var go = new GameObject(name, typeof(RectTransform));
                go.transform.SetParent(parent, false);
                _owned.Add(go);

                var image = go.AddComponent<Image>();
                image.sprite = sprite;
                image.raycastTarget = false;

                var rect = image.rectTransform;
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = size;
                rect.anchoredPosition = offset;
                return image;
            }
        }

        /// <summary>
        /// Glyph quads counted against characters on every label of a screen, which settles
        /// whether the text generator emits a quad for whitespace.
        /// </summary>
        private sealed class GlyphCount
        {
            private readonly List<string> _mismatches = new List<string>();

            private int _labels;
            private int _withWhitespace;
            private int _quadPerVisibleCharacter;
            private int _quadPerCharacter;
            private int _richText;
            private string _example;

            /// <summary>
            /// False when every label with whitespace had one quad per non-whitespace character,
            /// true when every one had one quad per character, null when the labels did not settle it.
            /// </summary>
            public bool? EmitsWhitespaceQuads
            {
                get
                {
                    if (_mismatches.Count > 0) return null;
                    if (_quadPerVisibleCharacter > 0 && _quadPerCharacter == 0) return false;
                    if (_quadPerCharacter > 0 && _quadPerVisibleCharacter == 0) return true;
                    return null;
                }
            }

            public string Summary =>
                _labels + " label meshes over both windows; of the " + _withWhitespace + " meshes with whitespace, " +
                _quadPerVisibleCharacter + " had one quad per non-whitespace character and " + _quadPerCharacter +
                " one quad per character; " + _mismatches.Count + " labels matched neither count; " + _richText +
                " skipped for rich-text tags" + (_example != null ? ". Example: " + _example : "");

            public void Count(Transform screen, string size)
            {
                foreach (var text in screen.GetComponentsInChildren<Text>())
                {
                    var value = text.text;
                    if (!text.isActiveAndEnabled || string.IsNullOrEmpty(value)) continue;

                    // A tag draws no glyph, so a tagged string has no fixed quad count to compare.
                    if (text.supportRichText && value.IndexOf('<') >= 0)
                    {
                        _richText++;
                        continue;
                    }

                    var whitespace = 0;
                    foreach (var character in value)
                    {
                        if (char.IsWhiteSpace(character)) whitespace++;
                    }

                    var quads = LayoutModel.Mesh(text).QuadCount;
                    var line = size + " \"" + value + "\": " + value.Length + " characters, " + whitespace +
                               " of them whitespace, " + quads + " quads";
                    _labels++;

                    if (whitespace == 0)
                    {
                        if (quads != value.Length) _mismatches.Add(LayoutModel.PathOf(text.transform) + ", " + line);
                        continue;
                    }

                    _withWhitespace++;
                    if (quads == value.Length - whitespace) _quadPerVisibleCharacter++;
                    else if (quads == value.Length) _quadPerCharacter++;
                    else
                    {
                        _mismatches.Add(LayoutModel.PathOf(text.transform) + ", " + line);
                        continue;
                    }

                    if (_example == null) _example = line;
                }
            }

            /// <summary>Fails on a label that matches neither count, or on a finding the model does not record.</summary>
            public void Check(List<string> failures)
            {
                foreach (var mismatch in _mismatches) failures.Add("glyph count: " + mismatch);

                var measured = EmitsWhitespaceQuads;
                if (measured == null)
                {
                    if (_mismatches.Count == 0)
                    {
                        failures.Add("glyph count: the Empresa labels did not settle whether whitespace gets a quad: " +
                                     Summary);
                    }

                    return;
                }

                if (measured.Value == LayoutModel.WhitespaceEmitsGlyphQuads) return;

                failures.Add("glyph count: LayoutModel.WhitespaceEmitsGlyphQuads records " +
                             LayoutModel.WhitespaceEmitsGlyphQuads + ", but the text generator " +
                             (measured.Value ? "emits" : "does not emit") + " quads for whitespace: " + Summary);
            }
        }

        /// <summary>
        /// <c>EventSystem.RaycastAll</c> at the centre of every control of a screen, next to the
        /// topmost raycast target in draw order.
        /// </summary>
        /// <remarks>
        /// The model walks the canvas depth-first, which is uGUI's draw order on one canvas, and
        /// takes the last active, enabled, unculled raycast target whose rect holds the point.
        /// That is all GraphicRaycaster tests where there is no RectMask2D, Mask, CanvasGroup,
        /// raycast padding or alpha hit threshold; the summary counts those on the screen.
        /// </remarks>
        private sealed class RaycastProbe
        {
            private int _points;
            private int _answered;
            private int _agreed;
            private string _inputs;
            private string _unmodelled;

            public List<string> Lines { get; } = new List<string>();

            /// <summary>Whether RaycastAll returned results at every control centre.</summary>
            public bool Answers => _points > 0 && _answered == _points;

            public string Summary =>
                "RaycastAll returned results at " + _answered + " of " + _points + " control centres, and its top " +
                "result was the model's topmost raycast target at " + _agreed + " of them. Display inputs: " +
                _inputs + ". On the screen, outside the model: " + _unmodelled;

            public void Cast(Canvas canvas, Transform screen, string size)
            {
                var system = EventSystem.current;
                var targets = new List<Graphic>();
                CollectTargets(canvas.transform, targets);
                if (_unmodelled == null) _unmodelled = Unmodelled(canvas, screen, targets);

                foreach (var control in screen.GetComponentsInChildren<Selectable>())
                {
                    if (!control.isActiveAndEnabled) continue;

                    var rect = (RectTransform)control.transform;
                    var centre = LayoutModel.ToCanvas(rect, rect.rect.center);
                    var pixel = centre * canvas.scaleFactor;

                    GameObject top = null;
                    for (var i = targets.Count - 1; i >= 0 && top == null; i--)
                    {
                        if (Contains(LayoutModel.RectCorners(targets[i].rectTransform), centre))
                        {
                            top = targets[i].gameObject;
                        }
                    }

                    var results = new List<RaycastResult>();
                    if (system != null) system.RaycastAll(new PointerEventData(system) { position = pixel }, results);

                    _points++;
                    if (_inputs == null) _inputs = Diagnose(system, canvas, pixel);

                    var line = size + " " + Name(control) + " at " + Number(pixel.x) + "," + Number(pixel.y) +
                               " px: topmost raycast target " + Path(top) + "; RaycastAll ";

                    if (results.Count == 0)
                    {
                        line += "returned nothing";
                    }
                    else
                    {
                        _answered++;
                        var hit = results[0].gameObject;
                        if (hit == top) _agreed++;
                        line += "returned " + results.Count + " results, the top one " + Path(hit) +
                                (hit == top ? ": same top" : ": DIFFERENT top");
                    }

                    Lines.Add(line);
                }
            }

            private static void CollectTargets(Transform node, List<Graphic> targets)
            {
                if (!node.gameObject.activeInHierarchy) return;

                var graphic = node.GetComponent<Graphic>();
                if (graphic != null && graphic.isActiveAndEnabled && graphic.raycastTarget &&
                    !graphic.canvasRenderer.cull)
                {
                    targets.Add(graphic);
                }

                for (var i = 0; i < node.childCount; i++) CollectTargets(node.GetChild(i), targets);
            }

            /// <summary>Whether a point lies inside a convex quad, edges included.</summary>
            private static bool Contains(Vector2[] corners, Vector2 point)
            {
                var sign = 0f;
                for (var i = 0; i < corners.Length; i++)
                {
                    var from = corners[i];
                    var to = corners[(i + 1) % corners.Length];
                    var cross = Cross(to - from, point - from);
                    if (cross == 0f) continue;

                    if (sign == 0f) sign = Mathf.Sign(cross);
                    else if (Mathf.Sign(cross) != sign) return false;
                }

                return true;
            }

            private static string Path(GameObject target) =>
                target != null ? LayoutModel.PathOf(target.transform) : "(none)";

            private static string Name(Selectable control)
            {
                var label = control.GetComponentInChildren<Text>();
                return (label != null ? "\"" + label.text + "\" " : "") + control.GetType().Name;
            }

            /// <summary>The inputs with which GraphicRaycaster decides that a pointer is on its display.</summary>
            /// <remarks>
            /// In the Editor, when <c>Display.RelativeMouseAt</c> returns zero, uGUI's
            /// MultipleDisplayUtilities puts the pointer on <c>Display.activeEditorGameViewTarget</c>,
            /// and GraphicRaycaster drops a pointer whose display is not its event camera's
            /// <c>targetDisplay</c> before it tests any graphic.
            /// </remarks>
            private static string Diagnose(EventSystem system, Canvas canvas, Vector2 pixel)
            {
                var raycaster = canvas.GetComponent<GraphicRaycaster>();
                var camera = raycaster != null ? raycaster.eventCamera : null;

                return "EventSystem " + (system != null ? "present" : "missing") +
                       ", GraphicRaycaster " + (raycaster != null && raycaster.IsActive() ? "active" : "missing or inactive") +
                       ", event camera " + (camera != null ? camera.name + " on display " + camera.targetDisplay : "none") +
                       ", Display.RelativeMouseAt(pointer) " + Display.RelativeMouseAt(new Vector3(pixel.x, pixel.y, 0f)) +
                       ", Display.activeEditorGameViewTarget " + Display.activeEditorGameViewTarget;
            }

            private static string Unmodelled(Canvas canvas, Transform screen, List<Graphic> targets)
            {
                var padded = 0;
                var alphaTested = 0;
                foreach (var target in targets)
                {
                    if (!target.transform.IsChildOf(screen)) continue;
                    if (target.raycastPadding != Vector4.zero) padded++;
                    if (target is Image image && image.alphaHitTestMinimumThreshold > 0f) alphaTested++;
                }

                return screen.GetComponentsInChildren<RectMask2D>().Length + " RectMask2D, " +
                       screen.GetComponentsInChildren<Mask>().Length + " Mask, " +
                       padded + " padded and " + alphaTested + " alpha-tested raycast targets; " +
                       canvas.GetComponentsInChildren<CanvasGroup>(true).Length + " CanvasGroup on the whole canvas";
            }
        }
    }
}
