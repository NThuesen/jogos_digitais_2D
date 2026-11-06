using System;
using System.Collections.Generic;
using System.IO;
using Neko.Game.View;
using UnityEditor;
using UnityEngine;

namespace Neko.Game.Editor
{
    /// <summary>
    /// DXT mip chains require power-of-two sheets. Pad at build time without resampling
    /// any frame, then restore the source PNGs even when BuildPipeline throws.
    /// </summary>
    internal sealed class WebGLWalkTextures : IDisposable
    {
        private const string Backup = "Library/WebGLWalkTextureBackup";
        private readonly List<string> _paths = new List<string>();

        public void Prepare()
        {
            if (Directory.Exists(Backup))
                throw new InvalidOperationException("Unrestored walk textures: recover PNGs from " + Backup + " before building.");
            Directory.CreateDirectory(Backup);
            foreach (var entry in MaidAnimCatalog.All)
            {
                var path = "Assets/_Game/Resources/" + entry.resource + ".png";
                var original = File.ReadAllBytes(path);
                var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                Texture2D padded = null;
                try
                {
                    if (!ImageConversion.LoadImage(source, original)) throw new InvalidOperationException(path);
                    var width = Mathf.NextPowerOfTwo(source.width);
                    var height = Mathf.NextPowerOfTwo(source.height);
                    if (width == source.width && height == source.height) continue;
                    padded = new Texture2D(width, height, TextureFormat.RGBA32, false);
                    padded.SetPixels32(new Color32[width * height]);
                    // Unity rects start at the bottom left. Frame origins must not move.
                    padded.SetPixels32(0, 0, source.width, source.height, source.GetPixels32());
                    padded.Apply();
                    File.WriteAllBytes(Path.Combine(Backup, Path.GetFileName(path)), original);
                    _paths.Add(path);
                    File.WriteAllBytes(path, ImageConversion.EncodeToPNG(padded));
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(source);
                    if (padded != null) UnityEngine.Object.DestroyImmediate(padded);
                }
            }
        }

        public void Dispose()
        {
            foreach (var path in _paths)
            {
                var backup = Path.Combine(Backup, Path.GetFileName(path));
                File.Copy(backup, path, true);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                File.Delete(backup);
            }
            if (Directory.Exists(Backup) && Directory.GetFiles(Backup).Length == 0) Directory.Delete(Backup);
        }
    }
}
