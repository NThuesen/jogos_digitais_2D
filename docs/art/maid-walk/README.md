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
- **Style**: one Lanczos downsample to 120 px height + 48-colour palette + hard alpha; faces lose some detail.
- **Loops**: Suzu (24→15, a single-step loop), Momo (27→20 frames), Reina (36→33) and Kaede (28→17) were trimmed to the stretch that loops; Kaede still jumps slightly at the wrap (LIMITED).
- **Equipment**: gear removes the authored look (layers cannot be drawn over a flat sheet).
- **Only walk exists**: Idle/Carry/Cook/Tired reuse walk frames, plate and dimming.
- **Memory**: nine sheets, ~10.2 MB uncompressed in total, shared per sheet through `SpriteFactory`; WebGL build not measured.
