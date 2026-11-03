using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Neko.Tests.PlayMode.Layout
{
    /// <summary>
    /// The layout rules the audit loop runs: the design's rule table, R2.1 to R14.5.
    /// </summary>
    /// <remarks>
    /// The audit loop runs every rule of <see cref="All"/> whose <see cref="LayoutRule.AppliesTo"/>
    /// accepts the view, at each of the three measurement steps of every view, before the state's
    /// own <see cref="AuditState.Checks"/>. A rule is registered in <see cref="Create"/>, in the
    /// order of the design's rule table; registering fails when an id comes twice.
    /// </remarks>
    public static class LayoutRules
    {
        private static IReadOnlyList<LayoutRule> _all;

        /// <summary>The registered rules, in the order of the design's rule table.</summary>
        public static IReadOnlyList<LayoutRule> All => _all ?? (_all = Register(Create()));

        /// <summary>
        /// Screen_Margin: the smallest gap, in Canvas_Units, between a non-Decor element and the
        /// Viewport edge.
        /// </summary>
        public const float ScreenMargin = 8f;

        /// <summary>
        /// How far, in Canvas_Units, a measured edge may pass a limit by float rounding alone: the
        /// canvas matrices and the sample ticks of a drawn edge carry an error of about 1e-4.
        /// </summary>
        private const float Rounding = 0.01f;

        /// <summary>The rules to register, in the order of the design's rule table.</summary>
        private static LayoutRule[] Create() => new LayoutRule[]
        {
            new LayoutRule("R2.1", "Property 1: Everything fits the window", EverythingFitsTheWindow),
            // Task 4 registers the other rules here: R3.1–R3.3, R4.1–R4.3, R5.1–R5.3, R6.1–R6.3,
            // then R14.5, which applies to SL-4 alone.
        };

        // Feature: ui-01-layout, Property 1: Everything fits the window
        /// <summary>
        /// R2.1, for Requirement 2.1 and, at 960×600, 2.2: the Visible_Shape of every non-Decor
        /// element and the Text_Bounds of every Text_Element of the Active_Layer lie inside the
        /// Viewport inset by <see cref="ScreenMargin"/>.
        /// </summary>
        /// <remarks>
        /// Adds one violation for each element that passes the inset rect, with the bounds it was
        /// tested by and each side it passes, by how much. The rule reads elements, so a composite
        /// is tested through its pieces and never reported twice. An element that draws nothing is
        /// skipped: a culled one, one whose shape is empty, such as a graphic that its
        /// <c>RectMask2D</c> clips away, and an image whose sprite reaches alpha 0.5 nowhere.
        /// <list type="bullet">
        /// <item>A Text_Element is tested by its Text_Bounds cut to its clip rect, Decor or not, as
        /// Requirement 2.1 reads. The glyphs that a scroll area clips are not drawn; R5.1–R5.3 test
        /// what the scroll area holds and whether its items scroll into view.</item>
        /// <item>A non-Decor image is tested by its drawn bounds (<see cref="VisibleShape.DrawnBounds"/>),
        /// which leave out the clear margin of a Baseline panel's sprite (cause C1). Its mesh bounds
        /// hold that margin, so they would report a panel whose drawn edge lies inside. The drawn
        /// edges are sampled every <see cref="VisibleShape.SampleStep"/>, finer than a window pixel
        /// at both Reference_Resolutions.</item>
        /// </list>
        /// An edge passes the inset rect when it lies more than <see cref="Rounding"/> beyond it.
        /// </remarks>
        private static void EverythingFitsTheWindow(RuleInput input, List<Violation> violations)
        {
            var measurement = input.Measurement;
            var inside = Inset(measurement.Viewport, ScreenMargin);
            var expected = "inside the Viewport inset by " + LayoutReport.Number(ScreenMargin) + ": " +
                           LayoutReport.Box(inside);

            foreach (var element in measurement.Elements)
            {
                if (element.Culled || element.Shape.IsEmpty) continue;

                Rect bounds;
                string tested;
                if (element.IsText)
                {
                    bounds = element.ClipRect.HasValue
                        ? LayoutModel.Intersect(element.TextBounds, element.ClipRect.Value)
                        : element.TextBounds;
                    tested = "Text_Bounds ";
                }
                else
                {
                    if (element.IsDecor) continue;

                    var drawn = element.Shape.DrawnBounds;
                    if (!drawn.HasValue) continue;

                    bounds = drawn.Value;
                    tested = "Visible_Shape ";
                }

                var outside = Outside(bounds, inside);
                if (outside == null) continue;

                violations.Add(new Violation(measurement, "R2.1",
                    tested + LayoutReport.Box(bounds) + ", outside by " + outside, expected, element.Path));
            }
        }

        /// <summary><paramref name="rect"/> moved in by <paramref name="margin"/> on each side.</summary>
        private static Rect Inset(Rect rect, float margin) =>
            Rect.MinMaxRect(rect.xMin + margin, rect.yMin + margin, rect.xMax - margin, rect.yMax - margin);

        /// <summary>
        /// The sides of <paramref name="bounds"/> that pass <paramref name="inside"/> by more than
        /// <see cref="Rounding"/>, each with how far, such as "left 3.0, top 12.5"; null when none does.
        /// </summary>
        private static string Outside(Rect bounds, Rect inside)
        {
            var sides = new List<string>(4);
            AddSide(sides, "left", inside.xMin - bounds.xMin);
            AddSide(sides, "right", bounds.xMax - inside.xMax);
            AddSide(sides, "bottom", inside.yMin - bounds.yMin);
            AddSide(sides, "top", bounds.yMax - inside.yMax);
            return sides.Count == 0 ? null : string.Join(", ", sides);
        }

        private static void AddSide(List<string> sides, string side, float amount)
        {
            if (amount > Rounding) sides.Add(side + " " + LayoutReport.Number(amount));
        }

        private static IReadOnlyList<LayoutRule> Register(LayoutRule[] rules)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < rules.Length; i++)
            {
                Assert.IsTrue(rules[i] != null, "LayoutRules: rule " + i + " of the registry is null");
                Assert.IsTrue(ids.Add(rules[i].Id), "LayoutRules: " + rules[i].Id + " is registered twice");
            }

            return Array.AsReadOnly(rules);
        }
    }

    /// <summary>
    /// One rule of the design's rule table, such as R2.1: its id, the property it checks, the
    /// Audit_States it applies to, and its check.
    /// </summary>
    /// <remarks>
    /// The check reads one measurement step through its <see cref="RuleInput"/> and adds one
    /// <see cref="Violation"/> per failure, built with
    /// <c>new Violation(input.Measurement, rule, measured, expected, paths…)</c>; the audit loop
    /// attributes it to the Audit_State and the view. A check that cannot measure what it needs, which
    /// is a checker bug and not a layout defect, fails with an assertion instead.
    /// </remarks>
    public sealed class LayoutRule
    {
        /// <param name="id">The rule's id in the design's rule table, such as "R2.1" or "R6.1–R6.3".</param>
        /// <param name="property">
        /// The tag of the correctness property the rule checks, as the design writes it, such as
        /// "Property 1: Everything fits the window".
        /// </param>
        /// <param name="check">Adds a violation for each failure of the rule at one measurement step.</param>
        /// <param name="appliesTo">
        /// Whether the rule runs in a view of an Audit_State; null for <see cref="EveryState"/>.
        /// </param>
        public LayoutRule(string id, string property, Action<RuleInput, List<Violation>> check,
            Func<AuditState, AuditView, bool> appliesTo = null)
        {
            Assert.IsFalse(string.IsNullOrEmpty(id), "LayoutRule: a rule needs its id, such as R2.1");
            Assert.IsFalse(string.IsNullOrEmpty(property),
                "LayoutRule: " + id + " needs the tag of its property, such as \"Property 1: Everything fits the window\"");
            Assert.IsTrue(check != null, "LayoutRule: " + id + " needs its check");

            Id = id;
            Property = property;
            Check = check;
            AppliesTo = appliesTo ?? EveryState;
        }

        /// <summary>The rule's id in the design's rule table, such as "R2.1".</summary>
        /// <remarks>
        /// A rule over several criteria, such as R6.1–R6.3, names each of its violations by the
        /// criterion that failed, such as "R6.2".
        /// </remarks>
        public string Id { get; }

        /// <summary>The tag of the property the rule checks, such as "Property 1: Everything fits the window".</summary>
        public string Property { get; }

        /// <summary>
        /// Whether the rule runs in a view of an Audit_State: <see cref="EveryState"/> unless the rule
        /// was given its own predicate, as R14.5 is for SL-4.
        /// </summary>
        public Func<AuditState, AuditView, bool> AppliesTo { get; }

        /// <summary>Adds a violation to the list for each failure of the rule at one measurement step.</summary>
        public Action<RuleInput, List<Violation>> Check { get; }

        /// <summary>The <see cref="AppliesTo"/> of a rule that runs in every view of every Audit_State.</summary>
        public static bool EveryState(AuditState state, AuditView view) => true;

        public override string ToString() => Id + " (" + Property + ")";
    }

    /// <summary>
    /// What a rule reads at one measurement step: the measurement and its click helper, the
    /// Audit_State with its view and café level, the step and the window.
    /// </summary>
    /// <remarks>
    /// The audit loop makes one input per step and passes it to every rule that applies, right after
    /// the measurement, so <see cref="Clicks"/> reads the hierarchy that was measured.
    /// </remarks>
    public sealed class RuleInput
    {
        /// <summary>The number of measurement steps of a view: 960×600, 1280×720, then 960×600 again.</summary>
        public const int Steps = 3;

        private Clicks _clicks;

        /// <param name="context">The audited state, with the view being measured entered.</param>
        /// <param name="measurement">The measurement of the step.</param>
        /// <param name="step">The measurement step, 1 to <see cref="Steps"/>.</param>
        public RuleInput(StateContext context, Measurement measurement, int step)
        {
            Assert.IsTrue(context != null, "RuleInput: there is no Audit_State to measure");
            Assert.IsTrue(context.View != null,
                "RuleInput: no view of " + context.State.Id + " is entered; the audit loop enters one with StateContext.Enter");
            Assert.IsTrue(measurement != null, "RuleInput: " + context.State.Id + " has no measurement for its rules");
            Assert.IsTrue(step >= 1 && step <= Steps,
                "RuleInput: " + context.State.Id + " is measured at step " + step + "; the steps run from 1 to " + Steps);

            Context = context;
            View = context.View;
            CafeLevel = context.CafeLevel;
            Measurement = measurement;
            Step = step;
        }

        /// <summary>The audited state's context: the game and the values its Build kept.</summary>
        public StateContext Context { get; }

        /// <summary>The audited Audit_State.</summary>
        public AuditState State => Context.State;

        /// <summary>The view measured, whose Screen name its records take.</summary>
        public AuditView View { get; }

        /// <summary>The café level the view shows.</summary>
        public int CafeLevel { get; }

        /// <summary>The measurement of the Active_Layer at this step.</summary>
        public Measurement Measurement { get; }

        /// <summary>The measurement step: 1 at 960×600, 2 at 1280×720, 3 at 960×600 again without a new Show.</summary>
        public int Step { get; }

        /// <summary>The window measured in, in pixels.</summary>
        public Vector2Int Window => Measurement.Window;

        /// <summary>The click helper of <see cref="Measurement"/>, made the first time a rule asks for it.</summary>
        public Clicks Clicks => _clicks ?? (_clicks = new Clicks(Measurement));
    }
}
