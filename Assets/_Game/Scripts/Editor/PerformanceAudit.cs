using System;
using System.IO;
using System.Collections.Generic;
using Neko.Game.View;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace Neko.Game.Editor
{
    /// <summary>Repeatable imported walk-texture inventory; not a GPU residency measurement.</summary>
    public static class PerformanceAudit
    {
        [Serializable] private sealed class TextureRow
        {
            public string id, format, importerFormat;
            public bool overridden;
            public int width, height, mipmaps;
            public long nativeBytes;
            public bool readable;
        }
        [Serializable] private sealed class Inventory
        {
            public string unity, target, graphics;
            public long walkNativeBytes;
            public List<TextureRow> textures = new List<TextureRow>();
        }

        public static void BenchmarkCustomers()
        {
            var factory = new Neko.Game.Art.SpriteFactory();
            var customer = new Neko.Sim.Service.Customer { State = Neko.Sim.Service.CustomerState.Ordering };
            var view = CustomerView.Create(null, factory, customer);
            for (var i = 0; i < 100; i++) view.Sync();
            var before = GC.GetAllocatedBytesForCurrentThread();
            var timer = System.Diagnostics.Stopwatch.StartNew();
            for (var i = 0; i < 10000; i++) view.Sync();
            timer.Stop();
            var bytes = GC.GetAllocatedBytesForCurrentThread() - before;
            File.WriteAllText("Logs/performance-customers.json", "{\"iterations\":10000,\"allocatedBytes\":" + bytes +
                ",\"milliseconds\":" + timer.Elapsed.TotalMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture) + "}");
            UnityEngine.Object.DestroyImmediate(view.gameObject);
            factory.Clear();
            EditorApplication.Exit(0);
        }

        public static void ExportBuildReport()
        {
            var report = UnityEditor.Build.Reporting.BuildReport.GetLatestReport();
            if (report == null) throw new InvalidOperationException("No saved BuildReport");
            var rows = new List<string> { "source_asset\tpacked_bytes" };
            ulong walks = 0;
            foreach (var packed in report.packedAssets)
                foreach (var asset in packed.contents)
                {
                    if (!asset.sourceAssetPath.StartsWith("Assets/_Game/Resources/", StringComparison.Ordinal)) continue;
                    rows.Add(asset.sourceAssetPath + "\t" + asset.packedSize);
                    if (asset.sourceAssetPath.StartsWith("Assets/_Game/Resources/Maids/", StringComparison.Ordinal) &&
                        asset.sourceAssetPath.EndsWith("_walk.png", StringComparison.Ordinal)) walks += asset.packedSize;
                }
            File.WriteAllLines("Logs/performance-packed-assets.tsv", rows);
            File.WriteAllText("Logs/performance-build-summary.txt", report.summary.result + "\n" +
                report.summary.outputPath + "\nBuild bytes: " + report.summary.totalSize +
                "\nPacked walk bytes (not GPU residency): " + walks + "\n");
            EditorApplication.Exit(0);
        }

        // Run on the baseline revision/settings, with no build-time padding.
        public static void BuildBaseline()
        {
            var report = WebGLBuilder.Run("Build/WebGL_baseline", false);
            Debug.Log("Baseline build " + report.summary.result + " " + report.summary.totalSize);
            EditorApplication.Exit(report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? 0 : 1);
        }

        public static void Capture()
        {
            EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.DXT;
            var inventory = new Inventory { unity = Application.unityVersion,
                target = EditorUserBuildSettings.activeBuildTarget.ToString(), graphics = SystemInfo.graphicsDeviceType.ToString() };
            foreach (var entry in MaidAnimCatalog.All)
            {
                var assetPath = "Assets/_Game/Resources/" + entry.resource + ".png";
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
                var platform = importer.GetPlatformTextureSettings("WebGL");
                var texture = Resources.Load<Texture2D>(entry.resource);
                if (texture == null) throw new InvalidOperationException(entry.resource);
                var row = new TextureRow { id = entry.id, format = texture.format.ToString(),
                    width = texture.width, height = texture.height, mipmaps = texture.mipmapCount,
                    readable = texture.isReadable, importerFormat = platform.format.ToString(),
                    overridden = platform.overridden, nativeBytes = Profiler.GetRuntimeMemorySizeLong(texture) };
                inventory.textures.Add(row);
                inventory.walkNativeBytes += row.nativeBytes;
            }
            var path = Environment.GetEnvironmentVariable("NEKO_PERF_INVENTORY") ?? "Logs/performance-textures.json";
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(inventory, true));
            Debug.Log("Texture inventory: " + path + " (native objects, not GPU residency)");
            EditorApplication.Exit(0);
        }
    }
}
