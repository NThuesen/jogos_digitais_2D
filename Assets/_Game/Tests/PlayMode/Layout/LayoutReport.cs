using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;

namespace Neko.Tests.PlayMode.Layout
{
    /// <summary>
    /// What the Layout_Check leaves behind: the failure message of a test, the JSON-lines report
    /// and the captures.
    /// </summary>
    /// <remarks>
    /// The design's "Failure report and captures" and "Evidence pipeline". Rules and state checks
    /// add a <see cref="Violation"/> for each failure. The audit loop attributes each one to its
    /// Audit_State and Screen with <see cref="Violation.In"/>, measures all three steps, then calls
    /// <see cref="Append"/> and <see cref="AssertNone"/>, so one run shows every defect
    /// (Requirement 20.3). <see cref="WriteCapture"/> writes the frame of the applied window
    /// (Requirement 20.6).
    ///
    /// Nothing is written while the variable is unset. Both variables take absolute paths, for
    /// example "$PWD/Logs/ui01/antes" from the repository root; a relative path is read from the
    /// project root, which is Unity's working directory. A path under Assets/ is refused, because
    /// Unity would import the file and DocsTests.NoTextureOrAudioFileWasEverImported fails on any
    /// PNG there.
    /// </remarks>
    public static class LayoutReport
    {
        /// <summary>The environment variable that names the report file, which gets one JSON object per line.</summary>
        public const string ReportVariable = "NEKO_LAYOUT_REPORT";

        /// <summary>The environment variable that names the directory the captures go to.</summary>
        public const string CaptureVariable = "NEKO_CAPTURE_DIR";

        /// <summary>The value of the <c>header</c> key, which only the header lines of the report have.</summary>
        public const string HeaderTag = "Layout_Check";

        /// <summary>UTF-8 without a byte order mark, so the first line of a new report is plain JSON too.</summary>
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

        /// <summary>
        /// The letters <see cref="Slug"/> folds to ASCII: the first character of each group is what
        /// the others become. They cover the accented letters of Portuguese.
        /// </summary>
        private static readonly string[] Folds =
        {
            "aàáâãäåªÀÁÂÃÄÅ", "cçÇ", "eèéêëÈÉÊË", "iìíîïÌÍÎÏ", "nñÑ", "oòóôõöºÒÓÔÕÖ", "uùúûüÙÚÛÜ", "yýÿÝ",
        };

        /// <summary>
        /// Appends one test's records to the report: a header line, then one line per violation.
        /// </summary>
        /// <remarks>
        /// The header names the Audit_State, its Screen, the windows measured in order, the number of
        /// violations, <see cref="ResolutionRig.ActivePath"/> and the Unity version, so a state that
        /// passed shows in the report too. The file and its directory are created when missing; an
        /// existing file is only appended to. Every violation must be attributed to
        /// <paramref name="state"/> with <see cref="Violation.In"/>.
        /// </remarks>
        /// <returns>The absolute path of the report, or null when <see cref="ReportVariable"/> is unset.</returns>
        public static string Append(string state, string screen, IEnumerable<Vector2Int> windows,
            IEnumerable<Violation> violations)
        {
            RequireId(state, "LayoutReport.Append");
            Assert.IsFalse(string.IsNullOrEmpty(screen), "LayoutReport.Append: the report of " + state + " needs its Screen");
            Assert.IsNotNull(windows, "LayoutReport.Append: the report of " + state + " needs the windows it measured");

            var records = Records(violations, "LayoutReport.Append");
            foreach (var violation in records)
            {
                Assert.IsTrue(violation.State == state && !string.IsNullOrEmpty(violation.Screen),
                    "LayoutReport.Append: the report of " + state + " holds a violation that is not attributed to it: " +
                    violation + ". Attribute each violation with Violation.In(state, screen) first");
            }

            var file = ReportFile();
            if (file == null) return null;

            var lines = new StringBuilder();
            lines.Append(HeaderJson(state, screen, windows, records.Count)).Append('\n');
            foreach (var violation in records) lines.Append(violation.ToJson()).Append('\n');

            File.AppendAllText(file, lines.ToString(), Utf8);
            return file;
        }

        /// <summary>
        /// Fails the test once with every violation in the message, or does nothing when there are none.
        /// </summary>
        /// <remarks>
        /// Call it after the last measurement step and after <see cref="Append"/>, so the report and
        /// the message both hold every violation of the test.
        /// </remarks>
        public static void AssertNone(IEnumerable<Violation> violations)
        {
            var records = Records(violations, "LayoutReport.AssertNone");
            if (records.Count == 0) return;

            Assert.Fail(Message(records));
        }

        /// <summary>
        /// The failure message of <see cref="AssertNone"/>: a line with the count and the Audit_States,
        /// then one numbered line per violation.
        /// </summary>
        public static string Message(IEnumerable<Violation> violations)
        {
            var records = Records(violations, "LayoutReport.Message");
            if (records.Count == 0) return "Layout_Check: no violations";

            var states = new List<string>();
            foreach (var violation in records)
            {
                var state = violation.State ?? Violation.Unattributed;
                if (!states.Contains(state)) states.Add(state);
            }

            var message = new StringBuilder();
            message.Append("Layout_Check: ").Append(records.Count.ToString(CultureInfo.InvariantCulture))
                .Append(records.Count == 1 ? " violation in " : " violations in ")
                .Append(string.Join(", ", states)).Append(':');
            for (var i = 0; i < records.Count; i++)
            {
                message.Append('\n').Append((i + 1).ToString(CultureInfo.InvariantCulture)).Append(". ")
                    .Append(records[i]);
            }

            return message.ToString();
        }

        /// <summary>
        /// Writes the frame of the applied window to the capture directory as
        /// <c>&lt;AuditState&gt;_&lt;screen-slug&gt;_&lt;W&gt;x&lt;H&gt;_playmode.png</c>.
        /// </summary>
        /// <remarks>
        /// Renders through <see cref="ResolutionRig.Capture"/> only when <see cref="CaptureVariable"/>
        /// is set, so a run without captures renders nothing extra. The directory is created when
        /// missing, and a capture of the same name is replaced. A state that shows several views in
        /// one load, such as LJ-4's tabs or SL-4's café levels, passes each view's own Screen name,
        /// so each view gets its own file.
        /// </remarks>
        /// <returns>The absolute path of the capture, or null when <see cref="CaptureVariable"/> is unset.</returns>
        public static string WriteCapture(string state, string screen)
        {
            var name = CaptureFileName(state, screen, ResolutionRig.Window);
            var directory = CaptureDirectory();
            if (directory == null) return null;

            var file = Path.Combine(directory, name);
            File.WriteAllBytes(file, ResolutionRig.Capture());
            return file;
        }

        /// <summary>
        /// The file name of a capture, for example <c>LJ-1_loja-moveis_960x600_playmode.png</c> for
        /// LJ-1, "Loja (Móveis)" and 960×600.
        /// </summary>
        public static string CaptureFileName(string state, string screen, Vector2Int window)
        {
            RequireId(state, "LayoutReport.CaptureFileName");
            Assert.IsTrue(window.x > 0 && window.y > 0,
                "LayoutReport.CaptureFileName: there is no window to name the capture of " + state + " by (" +
                ResolutionRig.Size(window.x, window.y) + "); apply one with ResolutionRig.Apply first");

            return state + "_" + Slug(screen) + "_" + ResolutionRig.Size(window.x, window.y) + "_playmode.png";
        }

        /// <summary>
        /// A Screen name as it appears in a capture's name: lower-case ASCII letters and digits with
        /// accents folded, and one hyphen for each run of other characters. "Loja (Móveis)" gives
        /// "loja-moveis", "Escalação" gives "escalacao".
        /// </summary>
        public static string Slug(string screen)
        {
            Assert.IsFalse(string.IsNullOrEmpty(screen), "LayoutReport.Slug: a capture needs the name of its Screen");

            var slug = new StringBuilder(screen.Length);
            var gap = false;
            foreach (var c in screen)
            {
                var plain = Fold(c);
                if (plain == '\0')
                {
                    gap = slug.Length > 0;
                    continue;
                }

                if (gap) slug.Append('-');
                gap = false;
                slug.Append(plain);
            }

            Assert.IsTrue(slug.Length > 0,
                "LayoutReport.Slug: the Screen name \"" + screen + "\" has no letter or digit to name a capture by");
            return slug.ToString();
        }

        /// <summary>
        /// A length or coordinate in Canvas_Units as records write it: invariant culture with one or two
        /// decimals, such as "18.0" or "57.25".
        /// </summary>
        public static string Number(float value)
        {
            // Negative zero would print as "-0.0".
            if (value == 0f) value = 0f;
            return value.ToString("0.0#", CultureInfo.InvariantCulture);
        }

        /// <summary>A size as records write it, such as "18.0x58.0".</summary>
        public static string Size(Vector2 size) => Number(size.x) + "x" + Number(size.y);

        /// <summary>A point as records write it, such as "(57.0,203.0)".</summary>
        public static string Point(Vector2 point) => "(" + Number(point.x) + "," + Number(point.y) + ")";

        /// <summary>A rect as its size at its bottom-left corner, such as "18.0x58.0 at (57.0,203.0)".</summary>
        public static string Box(Rect rect) => Size(rect.size) + " at " + Point(rect.position);

        /// <summary>
        /// Appends <paramref name="value"/> to <paramref name="json"/> as a JSON string, or as null.
        /// </summary>
        /// <remarks>
        /// Escapes the quote, the backslash, control characters, U+2028, U+2029 and unpaired
        /// surrogates, which UTF-8 cannot encode. Every other character, accents included, is
        /// written as it is.
        /// </remarks>
        internal static void AppendJson(StringBuilder json, string value)
        {
            if (value == null)
            {
                json.Append("null");
                return;
            }

            json.Append('"');
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                switch (c)
                {
                    case '"': json.Append("\\\""); break;
                    case '\\': json.Append("\\\\"); break;
                    case '\b': json.Append("\\b"); break;
                    case '\f': json.Append("\\f"); break;
                    case '\n': json.Append("\\n"); break;
                    case '\r': json.Append("\\r"); break;
                    case '\t': json.Append("\\t"); break;
                    default:
                        if (char.IsHighSurrogate(c) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]))
                        {
                            json.Append(c).Append(value[++i]);
                        }
                        else if (c < ' ' || char.IsSurrogate(c) || c == '\u2028' || c == '\u2029')
                        {
                            json.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            json.Append(c);
                        }

                        break;
                }
            }

            json.Append('"');
        }

        /// <summary>The header line of one test's records.</summary>
        internal static string HeaderJson(string state, string screen, IEnumerable<Vector2Int> windows, int violations)
        {
            var json = new StringBuilder(192);
            json.Append("{\"header\":");
            AppendJson(json, HeaderTag);
            json.Append(",\"state\":");
            AppendJson(json, state);
            json.Append(",\"screen\":");
            AppendJson(json, screen);
            json.Append(",\"windows\":[");
            var first = true;
            foreach (var window in windows)
            {
                if (!first) json.Append(',');
                first = false;
                AppendJson(json, ResolutionRig.Size(window.x, window.y));
            }

            json.Append("],\"violations\":").Append(violations.ToString(CultureInfo.InvariantCulture));
            json.Append(",\"rigPath\":");
            AppendJson(json, ResolutionRig.ActivePath.ToString());
            json.Append(",\"unity\":");
            AppendJson(json, Application.unityVersion);
            return json.Append('}').ToString();
        }

        /// <summary>The absolute report path, with its directory created; null when the variable is unset.</summary>
        private static string ReportFile()
        {
            var value = System.Environment.GetEnvironmentVariable(ReportVariable);
            if (string.IsNullOrWhiteSpace(value)) return null;

            var file = FullPath(value);
            Assert.IsFalse(string.IsNullOrEmpty(Path.GetFileName(file)) || Directory.Exists(file),
                "LayoutReport: " + ReportVariable + " names the directory " + file +
                "; give it a file, for example \"$PWD/Logs/ui01/antes.jsonl\"");
            RefuseAssets(file, ReportVariable, "Unity would import the report");

            Directory.CreateDirectory(Path.GetDirectoryName(file));
            return file;
        }

        /// <summary>The absolute capture directory, created when missing; null when the variable is unset.</summary>
        private static string CaptureDirectory()
        {
            var value = System.Environment.GetEnvironmentVariable(CaptureVariable);
            if (string.IsNullOrWhiteSpace(value)) return null;

            var directory = FullPath(value);
            RefuseAssets(directory, CaptureVariable,
                "DocsTests.NoTextureOrAudioFileWasEverImported fails on any PNG there");
            Assert.IsFalse(File.Exists(directory),
                "LayoutReport: " + CaptureVariable + " names the file " + directory + "; give it a directory");

            Directory.CreateDirectory(directory);
            return directory;
        }

        /// <summary>
        /// An absolute path for the value of a variable: itself when it is absolute, else read from the
        /// project root.
        /// </summary>
        private static string FullPath(string value)
        {
            var projectRoot = Path.GetDirectoryName(Path.GetFullPath(Application.dataPath));
            return Path.GetFullPath(Path.Combine(projectRoot, value));
        }

        private static void RefuseAssets(string path, string variable, string reason)
        {
            var assets = Path.GetFullPath(Application.dataPath);
            Assert.IsFalse(IsUnder(path, assets),
                "LayoutReport: " + variable + " names " + path + ", which is under " + assets + ", and " + reason +
                ". Give it a path under Logs/, for example \"$PWD/Logs/ui01/antes\"");
        }

        /// <summary>Whether <paramref name="path"/> is <paramref name="directory"/> or lies inside it.</summary>
        /// <remarks>Ignores case, because the default macOS and Windows file systems do.</remarks>
        private static bool IsUnder(string path, string directory)
        {
            var root = directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var full = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return string.Equals(full, root, System.StringComparison.OrdinalIgnoreCase) ||
                   full.StartsWith(root + Path.DirectorySeparatorChar, System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>A copy of the violations, failing on a missing list or a missing record.</summary>
        private static List<Violation> Records(IEnumerable<Violation> violations, string caller)
        {
            Assert.IsNotNull(violations, caller + ": there is no list of violations");

            var records = new List<Violation>(violations);
            for (var i = 0; i < records.Count; i++)
            {
                Assert.IsNotNull(records[i], caller + ": violation " + i + " of the list is null");
            }

            return records;
        }

        /// <summary>Fails unless <paramref name="state"/> can name a file: ASCII letters, digits and hyphens.</summary>
        private static void RequireId(string state, string caller)
        {
            Assert.IsFalse(string.IsNullOrEmpty(state), caller + ": the Audit_State id is missing");
            foreach (var c in state)
            {
                var plain = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '-';
                Assert.IsTrue(plain, caller + ": the Audit_State id \"" + state +
                                     "\" names files, so it may hold only ASCII letters, digits and hyphens");
            }
        }

        /// <summary>The lower-case ASCII letter or digit for <paramref name="c"/>, or '\0' for any other character.</summary>
        private static char Fold(char c)
        {
            if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')) return c;
            if (c >= 'A' && c <= 'Z') return (char)(c - 'A' + 'a');

            foreach (var group in Folds)
            {
                if (group.IndexOf(c, 1) >= 0) return group[0];
            }

            return '\0';
        }
    }

    /// <summary>
    /// One failure of a rule or state check: the design's Violation record (Data Models), written as
    /// one JSON line of the report and one line of the failure message.
    /// </summary>
    /// <remarks>
    /// Rules and state checks see a <see cref="Measurement"/> but not the Audit_State, so they create
    /// records through the measurement constructor, and the audit loop attributes them with
    /// <see cref="In"/>. Measured and expected values are text; <see cref="LayoutReport.Number"/>,
    /// <see cref="LayoutReport.Size"/>, <see cref="LayoutReport.Point"/> and
    /// <see cref="LayoutReport.Box"/> write numbers the same way in every record, so two runs with the
    /// same defects write the same lines.
    /// </remarks>
    public sealed class Violation
    {
        /// <summary>What the failure message shows for a record without an Audit_State.</summary>
        internal const string Unattributed = "(no Audit_State)";

        /// <summary>A record with every field of the report line.</summary>
        /// <param name="state">The Audit_State id, such as "BL-3"; null until attributed.</param>
        /// <param name="screen">The Screen, such as "Balanço"; null until attributed.</param>
        /// <param name="window">The window measured in, in pixels.</param>
        /// <param name="rule">The id of the failing rule or check, such as "R4.3".</param>
        /// <param name="paths">The hierarchy paths of the elements involved, from the Active_Layer root.</param>
        /// <param name="measured">What was measured, such as "intersection 18.0x58.0 at (57.0,203.0)".</param>
        /// <param name="expected">What the rule expects, such as "&lt;= 1.0".</param>
        public Violation(string state, string screen, Vector2Int window, string rule, IEnumerable<string> paths,
            string measured, string expected)
        {
            Assert.IsFalse(string.IsNullOrEmpty(rule), "Violation: a violation needs the id of its rule, such as R4.3");
            Assert.IsTrue(window.x > 0 && window.y > 0,
                "Violation: the " + rule + " violation has no window (" + ResolutionRig.Size(window.x, window.y) + ")");

            var list = paths == null ? new List<string>() : new List<string>(paths);
            Assert.IsFalse(list.Contains(null), "Violation: the " + rule + " violation has a null hierarchy path");

            State = state;
            Screen = screen;
            Window = window;
            Rule = rule;
            Paths = new ReadOnlyCollection<string>(list);
            Measured = measured ?? string.Empty;
            Expected = expected ?? string.Empty;
        }

        /// <summary>
        /// A record of a rule or state check, in the window of <paramref name="measurement"/>; the audit
        /// loop attributes it with <see cref="In"/>.
        /// </summary>
        public Violation(Measurement measurement, string rule, string measured, string expected, params string[] paths)
            : this(null, null, WindowOf(measurement), rule, paths, measured, expected)
        {
        }

        /// <summary>The Audit_State id, such as "BL-3"; null until <see cref="In"/> attributes the record.</summary>
        public string State { get; }

        /// <summary>The Screen, such as "Balanço"; null until <see cref="In"/> attributes the record.</summary>
        public string Screen { get; }

        /// <summary>The window measured in, in pixels.</summary>
        public Vector2Int Window { get; }

        /// <summary>The window as the report writes it, such as "960x600".</summary>
        public string Resolution => ResolutionRig.Size(Window.x, Window.y);

        /// <summary>The id of the failing rule or check, such as "R4.3".</summary>
        public string Rule { get; }

        /// <summary>The hierarchy paths of the elements involved, from the Active_Layer root.</summary>
        public IReadOnlyList<string> Paths { get; }

        /// <summary>What was measured, such as "intersection 18.0x58.0 at (57.0,203.0)".</summary>
        public string Measured { get; }

        /// <summary>What the rule expects, such as "&lt;= 1.0".</summary>
        public string Expected { get; }

        /// <summary>A copy of this record attributed to an Audit_State and the Screen it shows.</summary>
        public Violation In(string state, string screen)
        {
            Assert.IsFalse(string.IsNullOrEmpty(state), "Violation.In: the " + Rule + " violation needs its Audit_State id");
            Assert.IsFalse(string.IsNullOrEmpty(screen), "Violation.In: the " + Rule + " violation of " + state + " needs its Screen");
            return new Violation(state, screen, Window, Rule, Paths, Measured, Expected);
        }

        /// <summary>
        /// The record as one line of the report, with the design's keys in the design's order:
        /// state, screen, resolution, rule, paths, measured, expected.
        /// </summary>
        public string ToJson()
        {
            var json = new StringBuilder(192);
            json.Append("{\"state\":");
            LayoutReport.AppendJson(json, State);
            json.Append(",\"screen\":");
            LayoutReport.AppendJson(json, Screen);
            json.Append(",\"resolution\":");
            LayoutReport.AppendJson(json, Resolution);
            json.Append(",\"rule\":");
            LayoutReport.AppendJson(json, Rule);
            json.Append(",\"paths\":[");
            for (var i = 0; i < Paths.Count; i++)
            {
                if (i > 0) json.Append(',');
                LayoutReport.AppendJson(json, Paths[i]);
            }

            json.Append("],\"measured\":");
            LayoutReport.AppendJson(json, Measured);
            json.Append(",\"expected\":");
            LayoutReport.AppendJson(json, Expected);
            return json.Append('}').ToString();
        }

        /// <summary>The record as one line of the failure message.</summary>
        /// <remarks>A line break inside a value, such as a quoted label, is written as \n.</remarks>
        public override string ToString() =>
            ((State ?? Unattributed) + " (" + (Screen ?? "no Screen") + ") at " + Resolution + ", " + Rule +
             ": measured " + Measured + "; expected " + Expected + "; paths: " +
             (Paths.Count == 0 ? "none" : string.Join(", ", Paths)))
            .Replace("\r", "\\r").Replace("\n", "\\n");

        private static Vector2Int WindowOf(Measurement measurement)
        {
            Assert.IsNotNull(measurement, "Violation: a violation needs the measurement it was found in");
            return measurement.Window;
        }
    }
}
