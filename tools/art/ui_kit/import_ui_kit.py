"""Imports the UI-INT-01 kit (neko-ui-kit-delivery) into Assets/_Game/Resources/UiKit.

Copies each sprite byte for byte (no resampling), writes a deterministic .meta whose sprite border
comes from the kit manifest (`unity_border_LBRT`, Left/Bottom/Right/Top = Unity's x/y/z/w), and
prints the provenance lines (sha256 + path) that DocsTests parses.

    python tools/art/ui_kit/import_ui_kit.py <kit-dir> <repo-root>
"""
import hashlib
import json
import os
import shutil
import sys

kit, repo = sys.argv[1], sys.argv[2]
out = os.path.join(repo, "Assets", "_Game", "Resources", "UiKit")
os.makedirs(out, exist_ok=True)

SKIP = {"cream_surface_tile"}
manifest = json.load(open(os.path.join(kit, "manifests", "assets.json"), encoding="utf-8"))

TEMPLATE = """fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 1
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 0
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 100
  spriteBorder: {{x: {l}, y: {b}, z: {r}, w: {t}}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    customData:
    physicsShape: []
    bones: []
    spriteID: {sid}
    internalID: 0
    vertices: []
    indices:
    edges: []
    weights: []
    secondaryTextures: []
    spriteCustomMetadata:
      entries: []
    nameFileIdTable: {{}}
  mipmapLimitGroupName:
  pSDRemoveMatte: 0
  userData:
  assetBundleName:
  assetBundleVariant:
"""

lines, sources = [], {}
for entry in manifest:
    name = {"hub": "hub_background", "caixinhas": "caixinhas_background"}.get(entry["id"], entry["id"])
    if entry["id"] in SKIP:
        continue
    src = os.path.join(kit, entry["path"])
    dst = os.path.join(out, name + ".png")
    shutil.copyfile(src, dst)
    data = open(dst, "rb").read()
    sha = hashlib.sha256(data).hexdigest()
    border = entry.get("unity_border_LBRT") or [0, 0, 0, 0]
    guid = hashlib.md5(("neko-ui-kit/" + name).encode()).hexdigest()
    sid = hashlib.md5(("sid/" + name).encode()).hexdigest()[:16] + "0800000000000000"
    open(dst + ".meta", "w", newline="\n").write(
        TEMPLATE.format(guid=guid, l=border[0], b=border[1], r=border[2], t=border[3], sid=sid))
    sources[name + ".png"] = {"source": entry["path"], "sha256": sha, "border_LBRT": border}
    lines.append("%s  Assets/_Game/Resources/UiKit/%s.png" % (sha, name))

json.dump(sources, open(os.path.join(out, "SOURCES.json"), "w", newline="\n"), indent=1)
open(os.path.join(os.path.dirname(os.path.abspath(__file__)), "provenance_lines.txt"), "w",
     newline="\n").write("\n".join(lines) + "\n")
print(len(lines), "sprites imported")
