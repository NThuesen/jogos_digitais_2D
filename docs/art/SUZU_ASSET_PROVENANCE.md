# Suzu — imported runtime asset

The textures imported into the player are the Suzu walk sheet and the seven maid walk sheets of ART-03 (below).

**Origin.** Suzu was designed in this project (ChatGPT image, D-pixel-cafe direction), animated
from a still with Tencent AI Studio, exported to 30 PNG frames with Blender
(`art_working/01_suzu/step4_blender/walk_x/frames/`, git-ignored, never modified) and reduced to
the sheet below by `tools/art/suzu_walk/process.py`. No third-party character or image is
imported. Every parameter and the SHA-256 of each source frame are in
[`docs/art/suzu-walk/manifest.json`](suzu-walk/manifest.json).

**What the tool does:** removes matte streaks, replaces the closed eyes of the video's long
blink with the eyes of the nearest open frame, anchors the planted foot and the skirt axis,
scales every frame by one factor to 120 px of character height, quantises to one shared 48 colour
palette with hard alpha, and packs the 30 cells into one grid. Import settings are Point, no
mipmaps, uncompressed, clamp, non-readable (see `suzu_walk_sheet.png.meta`).

SHA-256 allowlist (also checked by `DocsTests`):

```text
4b7b617482c6ee210a604cc27d65234748892418fc826a991b179336cbf059da  Assets/_Game/Resources/Suzu/suzu_walk_sheet.png
76f5ae16df6f9d17db00cd965e9a1fbe1a66658c63001965780a2b1561bd6500  Assets/_Game/Resources/Maids/akari_walk.png
3fb2711015e3a66ce6eac3cf304669113a941a7be042dd88b973426d8ece7f51  Assets/_Game/Resources/Maids/hina_walk.png
414cd27b68f9ca0e7b6164d479ebdcbaa4b21bae66f1dde255095d0bc823ed4f  Assets/_Game/Resources/Maids/ichika_walk.png
fa83f45386f2fe18a3d21706e4dbf516faa80f415a86dd5df01c3136ee510398  Assets/_Game/Resources/Maids/kaede_walk.png
53cf87055f9343b87967bf813c15e0a374434089d659fd52fbc6139093395093  Assets/_Game/Resources/Maids/momo_walk.png
9f8ab9019ec6d2fd9cd439dd7ef4a96335da21f966facc7c598d0267c005c9e8  Assets/_Game/Resources/Maids/nene_walk.png
ea474732cd88803e75b065226ddcb020bd56e1267f65a6f3265633cc3eb80400  Assets/_Game/Resources/Maids/reina_walk.png
```

## Maid walk sheets (ART-03)

Hina, Akari, Momo, Reina, Kaede, Nene and Ichika were designed in this project (same chain as Suzu:
ChatGPT image, Tencent AI Studio animation, Blender frame export to the git-ignored
`art_working/<NN_maid>/step4_blender/walk_x/frames/`, never modified). Each sheet is produced by
`tools/art/maid_walk/maid_walk.py process` (parameters, per-frame source SHA-256 and the sheet hash are in
`docs/art/maid-walk/<id>/manifest.json`). Import settings are those of the Suzu sheet (Point, no mipmaps,
uncompressed, non-readable). The hashes are in the allowlist block above, which `DocsTests` checks.

Suzu's and Yoru's new sequences are **not** imported: see `docs/art/maid-walk/INVENTORY.md`.
