using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Neko.Game.Editor
{
    /// <summary>
    /// The batch-mode entry point for the itch.io build.
    /// </summary>
    /// <remarks>
    /// Settings are applied here rather than left in ProjectSettings so that the build the
    /// graders download is reproducible from one command, and so that what was chosen is
    /// visible in a diff instead of buried in a YAML asset.
    /// </remarks>
    public static class WebGLBuilder
    {
        public const string Output = "Build/WebGL";

        public static void Build()
        {
            var report = Run();
            var summary = report.summary;

            Debug.Log("Build " + summary.result + " · " + summary.totalSize + " bytes · " +
                      summary.totalTime);

            if (summary.result == BuildResult.Succeeded) EditorApplication.Exit(0);
            else EditorApplication.Exit(1);
        }

        /// <summary>
        /// QA build with the NEKO_QA_NINE fixture (nine authored maids in one shift), written to
        /// Build/WebGL_QA. The define is added only for this build and removed afterwards.
        /// </summary>
        public static void BuildQa()
        {
            const string define = "NEKO_QA_NINE";
            var target = NamedBuildTarget.WebGL;
            var before = PlayerSettings.GetScriptingDefineSymbols(target);
            BuildReport report;
            try
            {
                PlayerSettings.SetScriptingDefineSymbols(target, string.IsNullOrEmpty(before) ? define : before + ";" + define);
                report = Run("Build/WebGL_QA");
            }
            finally
            {
                PlayerSettings.SetScriptingDefineSymbols(target, before);
            }

            Debug.Log("QA build " + report.summary.result + " · " + report.summary.totalSize + " bytes");
            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        public static BuildReport Run() => Run(Output);

        public static BuildReport Run(string output)
        {
            Directory.CreateDirectory(output);

            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;

            // itch.io serves gzip correctly for Unity builds, but the fallback costs almost
            // nothing and removes the one way a delivered build can come up blank.
            PlayerSettings.WebGL.decompressionFallback = true;

            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.template = "APPLICATION:Default";
            PlayerSettings.runInBackground = true;

            PlayerSettings.SetScriptingBackend(NamedBuildTarget.WebGL, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.WebGL,
                Il2CppCompilerConfiguration.Release);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/_Game/Scenes/Boot.unity" },
                locationPathName = output,
                target = BuildTarget.WebGL,
                targetGroup = BuildTargetGroup.WebGL,
                options = BuildOptions.None,
            };

            return BuildPipeline.BuildPlayer(options);
        }
    }
}
