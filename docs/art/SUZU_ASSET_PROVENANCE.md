# Original Suzu pixel-art runtime assets

Source: `experiment/suzu-opus-review-20261030` at `670a594a31ff`.
Original native pixel art authored in this project with Claude Code (Opus), using the editable
`docs/art/suzu-modular/pixel/suzu_pixel.py`, `suzu_anim.py` and `suzu_variants.py` generators
at that commit. No third-party character, image or licence is imported.

Only these four original PNGs enter the player. PNG bytes and original .meta GUIDs are preserved.
The stationary tray import is corrected to Point / no mipmaps / Clamp / no compression / no NPOT scaling;
the other imports already use those settings. Runtime slicing uses 72x80, PPU 16, bottom-centre pivot.
Source generators and experimental scenes stay in their original branch, outside the player.

SHA-256 allowlist (also checked by DocsTests):

```text
4fe518b8de42d87eccf19a222ab29642ef7f8eddf6055f2ba3a5c4141f4a512e  Assets/_Game/Resources/Suzu/suzu_idle_sheet.png
e2c3de8d65704b4dd8c6ff31dc7362617c5d79956dfd7823d5613dcb4c70b05f  Assets/_Game/Resources/Suzu/suzu_walk_sheet.png
5f8e25277675020f84f7d27a647f14d749bc3f6964629b7925029419fdbe3f9e  Assets/_Game/Resources/Suzu/tray_idle.png
7abc47a7dd2af1af915621638f0f9e75786fd7f2aaf61e6078b0ddc310bee068  Assets/_Game/Resources/Suzu/walk_tray.png
```
