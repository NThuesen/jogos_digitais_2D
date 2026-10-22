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

        public static BuildReport Run()
        {
            Directory.CreateDirectory(Output);

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
                locationPathName = Output,
                target = BuildTarget.WebGL,
                targetGroup = BuildTargetGroup.WebGL,
                options = BuildOptions.None,
            };

            return BuildPipeline.BuildPlayer(options);
        }
    }
}
