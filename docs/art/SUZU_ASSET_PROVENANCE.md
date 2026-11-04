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
be482f46bdb9479c476cdcf11cb6ac8a627ff0acbbacbdfde091614ded9d6c5a  Assets/_Game/Resources/UiArt/btn1_disabled.png
5ae7316ad06a835ee427474f3bd89fe474b48d508334ffa3dbef313d8078dcb2  Assets/_Game/Resources/UiArt/btn1_hover.png
547ea47509b00cfb2f752b5a31d12578bb20231ce54b5adf9bce9319fa86c6fa  Assets/_Game/Resources/UiArt/btn1_normal.png
70ea100d92814aebdbfb0bc48e7facf5f6ac8311c9a19bfcadde0b75d7bf0c07  Assets/_Game/Resources/UiArt/btn1_pressed.png
a53894a6ef00b0d52f9d511b388dcd8bff82f052867b81981ba13c9d216e72fd  Assets/_Game/Resources/UiArt/btn2_disabled.png
c70296433bfcb17c5ae948c7968f8b4cc2b1d260953264ab14a2b779caf278e7  Assets/_Game/Resources/UiArt/btn2_hover.png
49fe2d82a4dbebaed2ca16ab457b6bec74848aa8e53d81fe2450bf00d5b07b98  Assets/_Game/Resources/UiArt/btn2_normal.png
f7fb0934255ddc17346bca012ef36a77023a7f8b1b7b6cb8d4101eca316240f1  Assets/_Game/Resources/UiArt/btn2_pressed.png
1faac4771f2d2777b97ea1f91ada6ffd5cb5a64971ddbb9f11d18c9e5a98809c  Assets/_Game/Resources/UiArt/icon_cat_paw.png
264d56051ff5a5621c40010871af213b8900a9eadb7b3c18922c3bdf32cfe42a  Assets/_Game/Resources/UiArt/icon_coin.png
212828d066a6889cbf27ec0db30cca012173c21de36415a3fb040983f0db3002  Assets/_Game/Resources/UiArt/icon_gift.png
43caf48781ce65991f7502a50dac4c093289be618c8de444d99c47607c560d6d  Assets/_Game/Resources/UiArt/tab_active.png
3d6a545a47da324ad1cdd3edd52f65597e6b004be4d31f4624d777a6d3219d63  Assets/_Game/Resources/UiArt/tab_hover.png
d79f4f76f02df177ae38679e8bae11b6dbc7c6e1bd7ec31f01f3d14e72f81ebb  Assets/_Game/Resources/UiArt/tab_inactive.png
d079d353925245c9e8c7e2891e69d40e19b47678808241fce9e618b67c18c22d  Assets/_Game/Resources/UiArt/ui012_menu_background.png
f1b69db6ed2a8cff93234d16344a05b4e75390ea6974956fe54dd54f58de2035  Assets/_Game/Resources/UiArt/ui017_shop_row.png
39e09b3959fd5a9759160a44d909672a7f26d0cc37877ed907954d29afb20dd0  Assets/_Game/Resources/UiArt/ui020_logo_wordmark.png
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

## Modular UI art (UI integration)

The `Assets/_Game/Resources/UiArt/*.png` files are owner-approved UI sprites copied byte-for-byte from `docs/art/ui-modular/approved/`; each source path and hash is in `Assets/_Game/Resources/UiArt/SOURCES.json`. Their hashes are in the allowlist block above, which `DocsTests` checks.
