# Suzu — imported runtime asset

The textures imported into the player are the Suzu walk sheet the old Suzu sheet kept as a backup, and the nine maid walk sheets of ART-03 (below).

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
b45fc50515464814243fcac49e110a8411152dd049282fcd80cd975d20e81294  Assets/_Game/Resources/Maids/akari_walk.png
a70b1d5934a7d4a9cee9b324e9c7817b9331b364971ea64dfa09b927e61fc5b8  Assets/_Game/Resources/Maids/hina_walk.png
0e2fea32076ebf949598be2a7edc0426ba4f8a6a27e4d4a69592398f734ffbc1  Assets/_Game/Resources/Maids/ichika_walk.png
d5f7e47ed17d4a996714489858bc087ddcfa34bc3d683ed7caa81fc0b49eb331  Assets/_Game/Resources/Maids/kaede_walk.png
0f40961c552574e5bb32fdd2bb87b02a12e4c611f4e8d7aa1ecfcb7b4ae1466b  Assets/_Game/Resources/Maids/momo_walk.png
fa90374d2ca21b49487fb58f0e9834a7160d28757eb33f5e3e1cebfc0db1087f  Assets/_Game/Resources/Maids/nene_walk.png
1febda572a21fb5de7c32f1e6e56ccf5a9a8db693d872ea2a87e89d7f56c6c64  Assets/_Game/Resources/Maids/reina_walk.png
001cb5b0b2e538fb7979ee9c78185913b0a96bc8573fea4deb4bf32e2820aefb  Assets/_Game/Resources/Maids/yoru_walk.png
8a7f4f09c865c77e878dca5460585bafe02d5f7f3fe78ae75b7458be12caffc7  Assets/_Game/Resources/Maids/suzu_walk.png
```

## Maid walk sheets (ART-03, restyled in ART-04)

Suzu (new frames), Hina, Akari, Momo, Reina, Kaede, Nene, Ichika and Yoru were designed in this project (same chain as Suzu:
ChatGPT image, Tencent AI Studio animation, Blender frame export to the git-ignored
`art_working/<NN_maid>/step4_blender/walk_x/frames/`, never modified). Each sheet is produced by
`tools/art/maid_walk/maid_walk.py process` (parameters, per-frame source SHA-256 and the sheet hash are in
`docs/art/maid-walk/<id>/manifest.json`). Since ART-04 the sheets are the illustrated
style (colours, shading and soft alpha kept; ~240 px tall characters; Trilinear, mipmaps, high-quality block compression,
non-readable). The ART-03 pixel-art sheets are commit `37b0f3f` and `maid_walk.py process --style pixel` reproduces them. The hashes are in the allowlist block above, which `DocsTests` checks.

`Resources/Suzu/suzu_walk_sheet.png` (PR #27) is no longer used by the catalog; it stays only as a rollback backup until the owner approves deleting it. See `docs/art/maid-walk/INVENTORY.md`.
