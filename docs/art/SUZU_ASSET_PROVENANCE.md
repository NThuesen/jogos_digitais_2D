# Suzu — imported runtime asset

The only texture imported into the player is the Suzu walk sheet.

**Origin.** Suzu was designed in this project (ChatGPT image, D-pixel-cafe direction), animated
from a still with Tencent AI Studio, exported to 30 PNG frames with Blender
(`art_working/01_suzu/step4_blender/walk_x/frames/`, git-ignored, never modified) and reduced to
the sheet below by `tools/art/suzu_walk/process.py`. No third-party character or image is
imported. Every parameter and the SHA-256 of each source frame are in
[`docs/art/suzu-walk/manifest.json`](suzu-walk/manifest.json).

**What the tool does:** removes matte streaks, replaces the closed eyes of the video's long
blink with the eyes of the nearest open frame, anchors the planted foot and the skirt axis,
scales every frame by one factor to 112 px of character height, quantises to one shared 48 colour
palette with hard alpha, and packs the 30 cells into one grid. Import settings are Point, no
mipmaps, uncompressed, clamp, non-readable (see `suzu_walk_sheet.png.meta`).

SHA-256 allowlist (also checked by `DocsTests`):

```text
4b7b617482c6ee210a604cc27d65234748892418fc826a991b179336cbf059da  Assets/_Game/Resources/Suzu/suzu_walk_sheet.png
```
