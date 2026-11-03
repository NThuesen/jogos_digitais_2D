# Maid walk cycles in the game (ART-03)

Nine maids walk with authored sheets in the real shift: Suzu (new frames), Hina, Akari, Momo, Reina, Kaede, Nene, Ichika and Yoru. Details, per-maid QA and limits: [INVENTORY.md](INVENTORY.md).

## Architecture
- `tools/art/maid_walk/maids.json` — roster and per-maid parameters; `maid_walk.py audit|process` — reproducible pipeline (clean, chroma key, loop trim, foot/axis anchor, one resample, palette, pack, rest-frame and idle detection, manifest with source SHA-256).
- Output: `Assets/_Game/Resources/Maids/<id>_walk.png` (+ `maid_walk_catalog.json`, the single source of the numbers). Raw frames never enter `Assets/`.
- Runtime: `MaidAnimCatalog` (data + resolution), `MaidView` (generic), `WalkCycle` (distance-driven, per-maid `units_per_cycle`). `SuzuSprites` is gone; the PR #27 sheet stays in `Resources/Suzu/` only as a rollback backup (`legacy` in `maids.json`).
- Identity: name **and** archetype must match a catalog entry; any gear falls back to the procedural figure. Nothing persisted, no save change. A persistent `visual_id` or one design per archetype is an owner decision, not made here.

## See each maid in a real automatic shift
Window > General > Test Runner > PlayMode, run `MaidGameplayTests`:
- `EachAuthoredMaidWalksInARealShiftAndTheDayStillEnds` — one case per maid (renames a floor maid in memory only; your save is restored afterwards).
- To watch rather than assert: set environment variable `NEKO_MAID_CAPTURE_DIR=<folder>` before opening Unity; `CaptureEachMaidWalking` writes 960x600 and 1280x720 stills per maid, `CaptureAllTogetherInTheBiggestRoom` fields all nine at once in the 13x9 room (extra maids added in memory only).
- Regenerate sheets: `python tools/art/maid_walk/maid_walk.py process` (needs the local raw frames), then update the hashes in `docs/art/SUZU_ASSET_PROVENANCE.md`.

## Trade-offs
- **Sliding feet**: sources are slow walks in place; cadence is ~30 drawings/s at 1x, capped at 60/s. Feet slide 2.4-5.2x (see inventory). Needs faster-stride footage to fix.
- **Style (ART-04, current)**: illustrated anime 2D. One Lanczos downsample (premultiplied alpha) of the original frame to 240 px character height; original colours, shading, outlines and soft alpha untouched; no palette, no hard alpha. The sprite is still 1.3 world units tall (`pixels_per_unit = 240/1.3`), so sharpness and on-screen size are independent. Import: Trilinear, mipmaps (bias -0.5), high-quality block compression, non-readable. Cells are multiples of 4 px.
- **Style (ART-03, rollback)**: 120 px + 48-colour palette + hard alpha, commit `37b0f3f`; `maid_walk.py process --style pixel` reproduces it and rewrites the `.meta` import settings to match (Point filter, no mipmaps). Existing GUIDs are always kept, and `process --only a,b` touches only those maids' PNG, `.meta`, manifest and catalog entry. Rolling back is therefore `process --style pixel` plus restoring the old protected test thresholds, or `git revert` of the ART-04 commits.
- **Evidence**: `evidence_illustrated/` (9 maids together in a real shift at 1280x720 and 960x600, per-maid sequence shots, `fidelity_vs_original.png`).
- **Loops**: Suzu (24→15, a single-step loop), Momo (27→20 frames), Reina (36→33) and Kaede (28→17) were trimmed to the stretch that loops; Kaede still jumps slightly at the wrap (LIMITED).
- **Equipment**: gear removes the authored look (layers cannot be drawn over a flat sheet).
- **Only walk exists**: Idle/Carry/Cook/Tired reuse walk frames, plate and dimming.
- **Memory**: nine sheets, ~10.2 MB uncompressed in total, shared per sheet through `SpriteFactory`; WebGL build not measured.

## ART-04 closing validation (HUD framing, WebGL, memory)
- **HUD framing**: during a shift `CameraRig.FrameRoomBetweenHud` frames the room between the top bar (92 px) and the lineup strip (128 px) in 720-px reference units, with 0.8 tile of head room above the top row. Yoru on the top row is fully visible at 1280x720 and 960x600; lineup chips were narrowed (132 -> 116 px) so all nine fit at 16:10. On-screen scale in the 13x9 room drops about 25%; sprites and resolution are unchanged.
- **WebGL build**: PASS (`tools/build-webgl.sh`, 35.0 MB build, 25.3 MB data gzip). Browser (Chrome, localhost): boots, menu, hub and a live shift run; console shows only two benign "sound not loaded yet" logs. **The nine illustrated sheets were NOT seen in the browser**: a new company opens with three procedural maids, so the illustrated ones only appear after recruiting. Their rendering is covered by PlayMode captures only. Verdict: BUILD PASS, BROWSER PARTIAL, ILLUSTRATED SPRITES NOT VERIFIED IN BROWSER.
- **Texture memory (measured from the build report)**: the nine walk sheets are 3.6-8.7 MB each, 50.8 MB together, which is RGBA32 *with mipmaps* (9.97 Mpx x 4 B x 4/3 = 53 MB). **They are not block-compressed in the WebGL build** (the importer's WebGL entry is not overridden and resolves to uncompressed). The "~13 MB compressed" figure in earlier notes was a theoretical estimate and does not hold for WebGL. Not measured: browser JS heap / GPU residency.
- **Budget test**: `TheSharedTextureBudgetStaysSmall` caps total *pixels* at 12 Mpx (current 9.97). It guards against sheet bloat; it is not a GPU memory guarantee (real WebGL cost is about 5.3 bytes/pixel today).
