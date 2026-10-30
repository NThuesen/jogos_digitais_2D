# ART-02 — Suzu chibi in real gameplay (implementation handoff)

**Status:** plan only; NOT implemented, tested or approved for merge. Branch starts at `main` SHA `6d5767b8`. Owner has provisionally approved the **small chibi** for gameplay distance, NOT close-ups or a final portrait.

## Objective and scope
Integrate Suzu's approved native pixel-art chibi into the EXISTING Neko Maid Inc. shift gameplay (the real Boot/GameRoot/ShiftPresenter/MaidView path), retaining the original simulation, camera framing, furniture, UI, other maids and save contract. This is a **chibi-only partial ART-02**, not the complete portrait + chibi gate. Obtain owner visual approval before merge.

Before editing: inspect `git status`, branch, source assets, current `main` and `AGENTS.md`; work in this task branch, in a clean independent worktree if needed. Read `docs/agent/UNITY.md`, `docs/agent/WORKFLOW.md`, `docs/agent/VALIDATION.md`, `docs/roadmap/ART-02_VERTICAL_SLICE.md`, `docs/art/MAID_VISUAL_DECISION.md` selectively.

**Source** (do not merge/cherry-pick the experimental history wholesale):
`experiment/suzu-opus-review-20261030` @ `670a594a31ff`. Relevant source: `Assets/Experiments/SuzuPixel/` (sprites/sheets/variants) and `docs/art/suzu-modular/pixel/` (editable generators + validations). Isolate and import ONLY actually needed game assets with original .meta/provenance, no old SVG/raster studies, offline previews or Editor capture scenes in the player. Do not discard or mutate the original experiment.

## Integration contract
1. Inspect `MaidView.cs`, `MaidSpriteBuilder.cs`, `ShiftPresenter.cs`, `SpriteFactory.cs`, `CameraRig.cs`, `Layers.cs`, `MaidCard.cs`, `Maid.cs`, and Unity PlayMode tests before deciding implementation details. Prefer a small Unity-view-side adapter/provider; avoid altering `Neko.Sim`, save schemas, gameplay rules, `GameRoot` or `Boot.unity` when unnecessary.
2. **Identity:** currently `Maid` has Id, Name, Archetype, Gear but no visual_id; the starting 3 maids are randomized. For this slice bind the authored Suzu appearance only to the exact maid name `Suzu` (document this TEMPORARY identity mapping) with procedural fallback for all other names. Create a reproducible, non-destructive in-game smoke setup where Suzu is actually fielded; do not alter player saves or guarantee Suzu by changing starter RNG/product mechanics. If a permanent identity mapping becomes necessary, flag as a separate product/architecture decision.
3. **Rendering:** expose idle, genuine 8-frame walk, and carry (where supported) with correct world-foot pivot, scale, clipping, sorting order, mirroring, frame pacing and lifecycle. Base art was 64×72/72-wide frames with experimental PPU 16 and a large preview camera; DO NOT copy preview-world scale blindly. Tune to a **small** maid in real café, aiming roughly 70–100 pixels tall at 960×600 as a starting hypothesis, also inspect 1280×720. The real `CameraRig.FrameRoom` dynamically frames café size. Don't apply the old procedural walk bob twice, or reconstruct sprites / allocate textures each Update. Test point-filtered readability and jitter at both resolutions.
4. **Preserve behavior:** keep existing pose resolution, fatigue/mood, `Worker.Carrying`/carried-item presentation, equipment visual differences, other maid graphics, sorting, save/load, movement, day end and state transitions. For `Cook`/`Tired` and equipment combinations not faithfully supported by Suzu art, use an explicit procedural fallback rather than silently hiding gameplay information. Do not imply an accessory/outfit is tied to an equipped gear ID without a grounded mapping. Avoid double-rendering trays/items.
5. **Asset inclusion:** experimental `Assets/Experiments/SuzuPixel/` scripts reference textures through `AssetDatabase`; those references are Editor-only and do NOT constitute a WebGL runtime loading path. Choose the smallest reliable player-build-compatible import/loading method (e.g. narrow `Resources/Suzu` bundle with correct Point/PPU/import settings); no Addressables framework or sweeping sprite system rewrite.

## IMPORTANT blocking verification-contract conflict — must not bypass
`Assets/_Game/Tests/EditMode/Game/DocsTests.cs` contains `NoTextureOrAudioFileWasEverImported` and `TheReadmeStatesThatNoAssetWasImported`; `README.md` currently says no imported assets. Adding intentional original PNG sprites invalidates those claims.
- Do NOT hide/re-encode PNGs into code merely to game the test, weaken assertions, silence failures or bypass `protected-change-guard`.
- A narrow maintenance exception from the repository owner is REQUIRED before editing the **existing guarded test**. With that approval, replace obsolete blanket absence checks with truth-based provenance/credit checks, update `README.md` (and directly affected art documentation), preserving unrelated rubric assertions. Expect CI guard review/manual integration as documented in `docs/agent/WORKFLOW.md`, **not auto-merge**.
- If approval is not yet available: implement only safe isolated work and report this gate as BLOCKING; do not claim a green Unity suite or an integration ready to merge.

## Acceptance / evidence
- In **real Boot -> new game/lineup -> Day** (not only experiment scene), a Suzu maid uses the pixel art while other maids use existing procedural sprites; no incorrect skin inheritance. Use a controlled visual test fixture without mutating personal saved profiles.
- Idle -> walk -> carry -> cook/tired fallback and opposite travel direction work as expected; stepping animation matches actual movement and clock; carried items/mood/equipment remain truthful.
- Screenshots and short frame sequence **from runtime Game View** at **960×600 and 1280×720** show small, readable Suzu relative to chairs, tables, other maids. Test room framing/scale with later café dimensions if feasible. User evaluates appearance.
- New focused tests for frame slicing and fallback/selection when practical; run Unity EditMode, PlayMode, Console and real WebGL browser checks when available. Record `passed / failed / not run` with evidence; green simulation-only CI does not validate visuals.
- Preserve collaboration: avoid changes to PR #20 economy, PR #22 furniture, PR #16 catalog, protected scenes/config/Packages, and unrelated local changes. No merge or auto-merge; submit a **draft PR** only after implementation and evidence. Update ART-02 status as **partial chibi integration**, never complete portrait+chibi without owner approval.

## Handback
Deliver: focused diff, demo instructions, runtime images, any required protected-test exception, honest gate status and PR link. Stop before destructive changes or any product decision beyond this scope.
