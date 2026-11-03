# Implementation Plan: UI-01 layout and legibility

> **ESPECIFICAÇÃO HISTÓRICA DO UI-01 (29/10–02/11/2026).** O PR #31 integrou somente a infraestrutura inicial do Layout_Check; telas ainda não foram corrigidas. O responsável aprovou em 03/11 uma execução **enxuta por marcos**: [UI-01 vigente](../../../docs/roadmap/UI-01_LAYOUT.md) e [direção visual ilustrada](../../../docs/art/CURRENT_VISUAL_DIRECTION.md). Preservar requisitos, decisões e tarefas originais para rastreabilidade; **não executar as 15 etapas/40 estados como gate obrigatório** nem aplicar câmera, baseline ou pixel art antigos automaticamente. Os estados não implementados continuam pendentes, não aprovados por inferência.


## Execution notes

- **Unity batch runs need the Unity editor closed.** `tools/run-tests.sh` refuses to start while an editor has the project open. Command form: `UNITY_EDITOR=/Applications/Unity/Hub/Editor/6000.5.9f1/Unity.app/Contents/MacOS/Unity tools/run-tests.sh PlayMode|EditMode [filter]`. Every `tools/run-tests.sh` command in this plan uses that `UNITY_EDITOR=` prefix.
  - The Layout_Check filter is `Neko.Tests.PlayMode.Layout`. A focused run can pass a quoted regular expression on the Audit_State id, for example `"Neko.Tests.PlayMode.Layout.LayoutCheckTests.*LJ-[1-4]"`; if it selects no test, run the whole Layout_Check filter.
  - Each run overwrites `Logs/tests-<Platform>.xml` and `Logs/tests-<Platform>.log`. Copy them to `Logs/ui01/<step>/` when a task keeps them as evidence. The runner rejects skipped and inconclusive tests.
  - Unity runs cannot overlap: the runner holds a project lock. The dependency graph therefore schedules one task per wave. Do not use the Unity MCP bridge for these runs.
  - A run that cannot start (editor open, lock held, wrong path) is reported as not run, with the reason. It never counts as a pass.
- **Simulation tests** run with `DOTNET_ROLL_FORWARD=Major tools/simtest.sh`.
- **New Unity files need `.meta` files**: `fileFormatVersion: 2` plus a fresh 32-hex `guid`, for example from `uuidgen | tr -d - | tr 'A-F' 'a-f'`. A new folder needs a folder `.meta` next to it: copy the layout of `Assets/_Game/Tests/EditMode/Game.meta` (`folderAsset: yes` and the `DefaultImporter` block) with a fresh guid.
- **The owner's local changes**: the uncommitted changes to `Packages/manifest.json`, `Packages/packages-lock.json` and `Assets/UniversalRenderPipelineGlobalSettings.asset` belong to the owner. Leave them as they are, and never stage or commit them.
- **Files that never change**: existing test files, Unity scenes, `ProjectSettings/`, `Packages/`, `.github/` and the `tools/` runners (Protected_Paths); every entry of `Strings.cs`; everything under `Assets/_Game/Scripts/Sim/`, `Assets/_Game/Scripts/Game/Core/` and `Assets/_Game/Scripts/Game/Input/`; colour values in `Palette.cs`, `Logo.cs` and `Scenery.cs`.
- **Run output** goes under `Logs/ui01/`, which Git ignores. Give the `NEKO_*` variables absolute paths, for example `"$PWD/Logs/ui01/antes"` from the repository root. Never write a PNG under `Assets/`: `DocsTests.NoTextureOrAudioFileWasEverImported` fails on any PNG there.
- **Local save**: every PlayMode run, including the Layout_Check, overwrites the local PlayerPrefs save, as `BootTests` and `CaptureTests` already do.
- **Layout_Check code**: files under `Assets/_Game/Tests/PlayMode/Layout/` use `namespace Neko.Tests.PlayMode.Layout`. The Layout_Check must compile against the Baseline Game_UI, so it never references a member that UI-01 adds (design, "Layout_Check"). Each rule carries the tag `// Feature: ui-01-layout, Property N: <title>`.
- **Frozen after the Baseline run**: after task 8, change an Audit_State builder or a rule only when a capture proves a checker bug, and record the change in `DEFECT_LOG.md`. If the change alters an Audit_State's session data or view, ask the owner first, because the "antes" captures of that state would no longer match.
- **Expected failures**: once the rules of task 4 exist and until task 12 is done, Layout_Check Audit_State tests fail on Screens that are not fixed yet (Requirement 20.4). Every other test passes at every checkpoint.
- **Git and the pull request** belong to the orchestrator; no task commits, pushes or opens a pull request. Two tasks need the orchestrator: task 14.3 records the hash of a commit made after the checkpoint of task 8 (the Layout_Check on the Baseline Game_UI), and task 15.1 needs the pull request URL.
- **Optional tasks**: none. The owner authorized the Layout_Check as the proof of the fix (29/10/2026) and trimmed it to Properties 1, 4, 5, 6, 7, 9, 12 and 15 (02/11/2026), so its rules are required.
- **Working rule**: write the task's code within the first few steps and save after each step; read only the files the task names and the design sections it cites; a task that ends without its files on disk has failed.
- **Baseline facts**: use the design's "Baseline hierarchy facts" instead of re-deriving them.

## Overview

The plan follows the design's delivery order and the owner's trim of 02/11/2026. Tasks 1 to 7 build the Layout_Check on the unchanged Baseline Game_UI: the batch-mode RenderTexture prototype (task 1, done), the layout model and the click helpers (task 2), the session factory and the audit loop (task 3), the layout rules (task 4) and the Audit_States with their state checks (tasks 5 and 6). They end with the Baseline run that records the "antes" captures and the failure report (task 7). Checkpoint 8 repeats that run with the full suites. Task 9 turns the Baseline run into the Defect_Log. Task 10 applies the shared C1 fix. Task 12 fixes the Screens in UI-01 order and changes only what the Layout_Check reports. Task 14 produces the "depois" captures, the full test results, the curated Evidence_Folder, the completed `DEFECT_LOG.md` and the review of the Change_Set. Task 15 moves the roadmap row to REVIEW once the pull request exists. Checkpoints 8, 11 and 13 run the Layout_Check filter and the full suites.

## Tasks

- [x] 1. Prototype the batch-mode resolution rig
  - [x] 1.1 Build the primary RenderTexture path and the rig smoke test
    - Create `Assets/_Game/Tests/PlayMode/Layout/` and `Assets/_Game/Tests/PlayMode/Layout.meta`
    - Create `ResolutionRig.cs`: a `RenderTexture(w, h, 24, ARGB32)` assigned to `Camera.main.targetTexture` for the whole measurement; wait two frames, call `Canvas.ForceUpdateCanvases()`, then assert `canvas.scaleFactor` and the canvas rect size against the scaler model (the `CanvasScaler.HandleScaleWithScreenSize` formula applied to the scaler settings read at runtime: 0.8333 and 1152×720 at 960×600, 1.0 and 1280×720 at 1280×720)
    - In the same file: `Capture()` with `camera.Render()`, `ReadPixels` and `ImageConversion.EncodeToPNG`; `Restore()` puts the previous `targetTexture` back, releases the RenderTexture in a `finally` block and asserts the restore
    - Create `LayoutCheckTests.cs` with `[UnityTest] RigMatchesTheScalerModelAtBothResolutions`: load `Assets/_Game/Scenes/Boot.unity`, discard 12 frames, measure the Menu at 960×600, 1280×720 and 960×600; `[TearDown]` calls `ResolutionRig.Restore()`; with `NEKO_CAPTURE_DIR` set, write the three frames
    - Run `tools/run-tests.sh PlayMode Neko.Tests.PlayMode.Layout` with `NEKO_CAPTURE_DIR="$PWD/Logs/ui01/prototype"` and look at the three PNGs
    - _Requirements: 20.2, 20.5, 20.6, 20.7_

  - [x] 1.2 Decision point: keep the primary path or switch to the fallback path
    - Keep the primary path if the 1.1 assertions pass and each capture shows the whole window at its size
    - Otherwise implement the fallback path of the design in `ResolutionRig.cs`: disable the `CanvasScaler` during the measurement, set `canvas.scaleFactor` from the scaler model, set `canvas.updateRectTransformForStandalone` to `Enabled`, call `camera.Render()`, and re-enable the scaler in `Restore()`; run the 1.1 test again
    - If neither path passes, stop and report the measured scale factors and canvas sizes to the owner; do not start task 2
    - Record which path ran: expose `ResolutionRig.ActivePath` (`Primary` or `Fallback`) for the report header of task 2.2, and write the outcome with the measured values to `Logs/ui01/prototype.md` for the Defect_Log
    - _Requirements: 20.2, 20.7_

  - [x] 1.3 Probe meshes, glyphs and raycasts
    - Create `LayoutModel.cs` with mesh extraction: `Graphic.OnPopulateMesh(VertexHelper)` invoked through reflection, vertices transformed to canvas space; a missing member fails with its name
    - Add `[UnityTest] ProbesAgreeWithTheRenderedFrame` to `LayoutCheckTests.cs`. On Empresa (reached as `CaptureTests` does) at both resolutions, compare the mesh bounds of Simple, aspect-preserving and a test-local sliced Image, and the glyph quads of a label, with the bounding box of the pixels that change when that element is hidden. Compare `EventSystem.RaycastAll` with the topmost raycast target at control centres
    - Record in `LayoutModel.cs` and `Logs/ui01/prototype.md` whether the text generator emits quads for whitespace, and whether `RaycastAll` returns results in batch mode; if it returns nothing, the hit map alone is used
    - _Requirements: 3.3, 6.2, 20.5_

- [x] 2. Build the layout model and the helpers on the Baseline code
  - [x] 2.1 Model the elements of the Active_Layer in `LayoutModel.cs`
    - The element types of the design's Data Models, and `Measure(UiRoot ui)`, which returns the Active_Layer's elements in draw order for the current `ResolutionRig` window
    - Active_Layer: the `Dialogs` layer when `UiRoot.DialogOpen`, else the active `Reveal` child of `BoxScreen`, else `UiRoot.Current`; the floating-effects `Overlay` layer is ignored
    - Graphics in depth-first draw order with `canvasRenderer.cull`; items culled by a `RectMask2D` kept from their RectTransforms; each element's clip rect
    - Text_Bounds as the union of glyph quads without `Shadow` copies, from the existing `Mesh(Graphic)`; a TrackedLabel as one element whose own rect is the row
    - Classification from the design's "Baseline hierarchy facts": plates, composites by name, background pieces, Decor, Container and Container_Chain; Interactive_Controls (`Selectable`, `ScrollRect`)
    - Visible_Shape as the clipped mesh bounds in this task; task 2.2 refines it
    - _Requirements: 2.1, 3.1, 4.1, 4.2, 4.3, 5.1_

  - [x] 2.2 Add the Visible_Shape and the Room_Grid_Area to `LayoutModel.cs`
    - Visible_Shape from bilinear texture alpha ≥ 0.5 at the interpolated UV, with the existing `SampleAlpha` and one alpha cache per texture, clipped by every `RectMask2D` ancestor
    - Shape tests for the rules: a rectangle inside a Visible_Shape, and the intersection of two Visible_Shapes; corner tests for rectangles in convex plates, 0.5-unit sampling otherwise
    - Room_Grid_Area from the `Tile` sprite renderers, projected as the design's "Baseline hierarchy facts" state
    - _Requirements: 2.1, 3.2, 4.3, 14.5_

  - [x] 2.3 Prove the model on the Empresa
    - Add the optional debug dump to `LayoutModel.cs`: with `NEKO_LAYOUT_DUMP` set to a directory, each measurement is written as a text file with one line per element (draw index, path, kind, rect, Visible_Shape bounds, Container path)
    - Add `[UnityTest] ModelDescribesEmpresa` to `LayoutCheckTests.cs`: reach the Empresa as `ProbesAgreeWithTheRenderedFrame` does, and measure at 960×600 and at 1280×720
    - Assert at both resolutions: the Active_Layer is `HubScreen`; the 7 planks are Interactive_Controls and each plank label's Container is its face; the wordmark is one TrackedLabel element; the stats `Plaque` is one Composite_Element; the chemistry line's Container is the Viewport; the stats plaque's Visible_Shape lies more than 1 unit inside its rect (C1); with `NEKO_LAYOUT_DUMP` set, the dump is written
    - Run the Layout_Check filter; the tests in `LayoutCheckTests.cs` pass
    - _Requirements: 3.2, 20.5_

  - [x] 2.4 Write the report and capture writer in `LayoutReport.cs`
    - Violation record with Audit_State, Screen, resolution, rule, hierarchy paths, measured and expected values; one aggregated failure message per test; JSON lines appended to `NEKO_LAYOUT_REPORT`; `ResolutionRig.ActivePath` in the report header
    - PNG writer for `NEKO_CAPTURE_DIR` named `<AuditState>_<screen-slug>_<W>x<H>_playmode.png`
    - Task 1.2, which stays as written, calls this task by its number before 02/11/2026 (2.2)
    - _Requirements: 20.3, 20.6_

  - [x] 2.5 Write the click helpers in `Clicks.cs`
    - Raycast targets of the Active_Layer: `raycastTarget` on, not culled, `depth != -1`, each with its rect clipped by its `RectMask2D` ancestors
    - A control's Hit_Area: the union of the clipped raycast rects whose `ExecuteEvents.GetEventHandler<IPointerClickHandler>` is the control (`IScrollHandler` for a scroll area)
    - `TopHandler(point)`: the click handler of the topmost `EventSystem.RaycastAll` result at a canvas point, converted to screen pixels as task 1.3 does; `Click(point)` delivers a pointer click to that handler
    - Sample points of a control: its centre plus a 4-unit grid over its Visible_Shape inset by Overlap_Tolerance
    - Add `[UnityTest] ClicksFindTheBaselineTargets` to `LayoutCheckTests.cs`: on the Empresa at 960×600, the topmost raycast target at the centre of "Voltar" is `HubScreen/Plaque/Panel[1]` (D-EM-5); at 1280×720, `Click` at the centre of the Loja plank opens the Loja. If a Baseline defect blocks that point, record it in `Logs/ui01/prototype.md` and assert the measured target instead
    - Run the Layout_Check filter
    - _Requirements: 6.1, 6.2, 6.3, 14.5, 17.3_

- [x] 3. Build the session factory, the Audit_State registry and the audit loop
  - [x] 3.1 Write the session builder in `SessionFactory.cs`
    - Profiles from `Profile.NewGame(seed)` with explicit values (reputation for the café level, money, fichas, day, records, maids minted with a fixed `Rng` and explicit names, levels, bonds, traits, lineup, grid, owned stock), saved with `SaveBridge.Save` before `Boot.unity` loads
    - Helpers: load and discard 12 frames; navigate through the public entry points that `CaptureTests` uses; Settled (`Tween.TickAll(0.1f)` 20 times, `Canvas.ForceUpdateCanvases()`, one frame, one more tick with identical bounds); `Clocks.Pause(true)` for O dia
    - A missing reflection target fails with its name
    - _Requirements: 20.1, 20.5_

  - [x] 3.2 Add the value providers to `SessionFactory.cs`
    - Large_Values from the glossary; Longest_Values picked at test time by rendered width with `Typeface.Font` from the tables listed in the design, with maid names from the private `Lootbox` name pool by reflection plus the numbered fallback
    - Display-string replacement for unreachable Large_Values that must match exactly one label
    - A provider without candidates fails with its name
    - _Requirements: 20.1, 20.5_

  - [x] 3.3 Write the Audit_State registry in `AuditStates.cs`
    - The `AuditState` class of the design's Data Models (`Id`, `Screen`, `Build`, `Checks`, `CafeLevels`) and a registry that yields the ids of the implemented states
    - Implement EM-1 (new game, day 1, Empresa) as the first state
    - _Requirements: 20.1, 20.5_

  - [x] 3.4 Write the audit loop in `LayoutCheckTests.cs`
    - `[UnityTest] Audit([ValueSource] string id)`, so each case is named by its Audit_State id: build the state; measure at 960×600, 1280×720 and 960×600 without a new `Show`; run every registered rule and the state's checks at each of the three steps (Requirement 2.3); write the captures of the first two steps; append the report; fail after all steps with every record
    - LJ-4 iterates its tabs and SL-4 its café levels inside one load; `[TearDown]` restores the rig
    - Run EM-1: with no rule registered yet, it passes and writes its captures
    - _Requirements: 2.3, 20.1, 20.3, 20.5, 20.6, 20.7_

- [ ] 4. Implement the layout rules
  - Each rule lives in `LayoutRules.cs` unless stated otherwise, carries its property tag and is registered so that the audit loop runs it in every Audit_State it applies to
  - Check each new failure against its capture: a real Baseline defect stays for the Defect_Log; a false positive is a checker bug and is fixed in the model or the rule. No Game_UI file changes in this task
  - [~] 4.1 Implement rule R2.1
    - Non-Decor Visible_Shape bounds and Text_Bounds inside the Viewport inset by Screen_Margin
    - **Property 1: Everything fits the window**
    - **Validates: Requirements 2.1, 2.2, 13.3, 15.2, 15.3**

  - [ ] 4.2 Implement rules R3.1 to R3.3
    - Text_Bounds inside the own rect within 1 unit; Text_Bounds grown by Edge_Clearance inside the Container's Visible_Shape; one glyph quad per non-whitespace character, because whitespace emits no glyph quads (task 1.3)
    - **Property 4: Text stays inside its boxes**
    - **Validates: Requirements 3.1, 3.2, 3.3, 9.5, 10.5, 14.4, 15.3, 16.2**

  - [ ] 4.3 Implement rules R4.1 to R4.3
    - Nothing later in draw order over a Text_Bounds; nothing earlier under it unless on the Container_Chain or Decor; non-Decor Visible_Shapes intersect only within the Container_Chain or one Composite_Element; intersections of 1 unit or less are ignored
    - **Property 5: No collisions**
    - **Validates: Requirements 4.1, 4.2, 4.3, 8.1, 8.7, 9.2, 10.1, 11.1, 11.4, 12.1, 12.6, 13.1, 13.2, 13.3, 15.1, 15.4, 16.2, 16.3, 17.2**

  - [ ] 4.4 Implement rules R5.1 to R5.3
    - Content drawn inside the scroll area's Visible_Shape bounds; every item reachable inset by Edge_Clearance; the first item inside at the opening position
    - The reachability answer of R5.2 is a small pure function of the content length, the viewport length and the item extents along the scroll axis, kept inside the rule; it has no property test
    - **Property 6: Scroll areas hold and reach their items**
    - **Validates: Requirements 2.2, 5.1, 5.2, 5.3**

  - [ ] 4.5 Implement rules R6.1 to R6.3
    - Through `Clicks.cs`: Hit_Area at least Minimum_Hit_Size (rect math); `TopHandler` at every sample point of a control returns the control or a nested control; no overlap above Overlap_Tolerance between the Hit_Areas of buttons, planks, tabs, price buttons, card buttons, stock cards or Dialog buttons (rect math)
    - The report names the blocked point and the element that receives the click
    - **Property 7: Controls are hittable where they are drawn**
    - **Validates: Requirements 6.1, 6.2, 6.3, 11.5**

  - [ ] 4.6 Implement the named-containment helper in `StateChecks.cs`
    - `Inside(element, container)`: the named container is on the element's Container_Chain, and the element satisfies rules R3 and R4 against it; a locator that finds zero or several elements fails with the searched text or path
    - **Property 9: Named containment**
    - **Validates: Requirements 8.1, 9.1, 9.3, 9.6, 10.2, 10.3, 10.4, 11.2, 11.3, 12.2, 12.4, 12.5, 14.1, 14.2, 14.3, 15.1, 15.2, 17.1, 17.2**

  - [ ] 4.7 Add the Console check to `LayoutCheckTests.cs`
    - Every Audit test calls `LogAssert.NoUnexpectedReceived()`, so an error or exception logged while visiting the state at either resolution fails it
    - **Property 15: Clean Console**
    - **Validates: Requirements 21.4**

- [ ] 5. Implement the Audit_States and state checks of the priority Screens
  - Builders go in `AuditStates.cs`, value providers in `SessionFactory.cs`, criteria in `StateChecks.cs` with the helper of task 4.6; clicks go through `Clicks.cs`
  - Failures on the Baseline are expected; check each against its capture as in task 4. No Game_UI file changes
  - [ ] 5.1 Implement the Loja states LJ-1 to LJ-4 and their checks
    - Café level 10 stock, balance 9.999.999 ¥, affordable and unaffordable rows, two-digit "No estoque" counts on Móveis and Roupas, Presentes without stock counts; LJ-4 scrolls each tab to the end
    - Checks: icon (Móveis only), name, description, "No estoque" count and price button inside the row and apart (8.1); title, balance, tabs and "Voltar" apart from each other and from the list (8.7); a click on an enabled price button performs `Shop.Buy` for furniture and clothes and opens "Para quem?" for gifts (8.5)
    - _Requirements: 8.1, 8.5, 8.7, 20.1_

  - [ ] 5.2 Implement the Balanço and Falência states BL-1 to BL-4 and FL-1 and their checks
    - `Session.FinishDay(new ShiftResult { … })`, then the returned `DayLedger` fields set to Large_Values; BL-1 profitable day, BL-2 loss day with the cash warning, BL-3 every tally non-zero plus a level-up chip for each of 8 maids (13 chips), BL-4 "O café não abriu" with the longest shift failure reason; FL-1 after a bankruptcy
    - Checks: verdict, subtitle, both ledger columns, every chip, the profit or loss line and "Caixa depois" inside the plaque (9.1); 13 chips inside and apart (9.2); row label and amount inside the row (9.3); the complete failure reason (9.5); the Falência title, body and button inside its plaque (9.6)
    - _Requirements: 9.1, 9.2, 9.3, 9.5, 9.6, 20.1_

  - [ ] 5.3 Implement the Elenco states EL-1 to EL-4 and their checks
    - EL-1 roster at cap (8 / 8) with a fixed cost of 99.999 ¥ by display replacement and the first maid selected; EL-2 a maid with 3 traits, a bond-5 signature line, Longest_Values chemistry tags, level 20 / 20, rarity Lendária and salary 9.999 ¥; EL-3 a maid without traits or signature; EL-4 the grid scrolled to the end
    - Checks: grid and detail panel apart (10.1); card name, archetype, stats and "Salário" inside the card face (10.2); every detail item inside the detail panel (10.3, 10.4); the complete header inside its rect (10.5); a click on a grid card shows that maid in the detail panel (10.6)
    - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5, 10.6, 20.1_

- [ ] 6. Implement the Audit_States and state checks of the other Screens, the Overlay and the Dialogs
  - Same files and rules as task 5
  - [ ] 6.1 Implement the Empresa states EM-1 to EM-4 and their checks
    - EM-1 exists from task 3.3; EM-2 Large_Values, café level 10 (level line ending in "no topo"), 8 maids in the lineup and the cash warning; EM-3 a required position empty, with the not-ready line and "Abrir o dia" disabled; EM-4 an empty lineup
    - Checks: nothing drawn over the monogram or the wordmark (11.1); figures, level bar and lines inside the stats plaque (11.2); every plank label inside its face (11.3); 8 today cards apart from the chemistry line, each other and the planks (11.4); a click on every cell of the "Voltar" face returns to the Menu (11.5)
    - _Requirements: 11.1, 11.2, 11.3, 11.4, 11.5, 20.1_

  - [ ] 6.2 Implement the Escalação states ES-1 to ES-3 and their checks
    - ES-1 the starting squad; ES-2 café level 8 or higher with 10 slots, 8 maids placed, one out of position, one empty slot and an empty bench ("vazio"); ES-3 the lineup cleared, with 8 maids on the bench and the not-ready warning
    - Checks: position headers and slot cards apart and clear of the chemistry panel (12.1); card texts inside the card face (12.2); "Banco" above the bench and within its horizontal extent (12.4); score, bar and verdict inside the chemistry panel (12.5); bench, warning, "Escalar sozinho" and "Limpar" apart (12.6)
    - _Requirements: 12.1, 12.2, 12.4, 12.5, 12.6, 20.1_

  - [ ] 6.3 Implement the Caixas and reveal states CX-1 to CX-4 and their checks
    - CX-1 every box affordable with 99.999 fichas; CX-2 no box affordable; CX-3 a Caixa Simples reveal with 1 card; CX-4 a Caixa Prata or Dourada reveal with the roster full (3 cards with duplicate notes), measured on the `Reveal` layer
    - Checks: crate, name, odds text and "Abrir N" apart (13.1); the fichas balance apart from the title and "Voltar" (13.2); heading, cards, notes and "Guardar" inside the Viewport and apart (13.3)
    - _Requirements: 13.1, 13.2, 13.3, 20.1_

  - [ ] 6.4 Implement the Salão states SL-1 to SL-4 and their checks
    - SL-1 the level-1 room with an empty stock; SL-2 the 13×9 room with the theme bonus and the longest theme name, two-digit metrics and every placeable in stock with two-digit counts; SL-3 the Longest_Values placeable picked, then a blocked placement message; SL-4 each room size of café levels 1 to 10 with stock, inside one load
    - Locate the stock area as the `ScrollRect` that holds the stock card labels, whatever its axis, so the same checks run on the Baseline bottom bar and on the later column
    - Checks: title and metrics inside the top bar (14.1); icon and complete label inside each stock card (14.2); the empty-stock text inside the stock area (14.3); the complete hint inside its rect (14.4)
    - _Requirements: 14.1, 14.2, 14.3, 14.4, 20.1_

  - [ ] 6.5 Implement rule R14.5 in `StateChecks.cs`
    - At each SL-4 café level and both resolutions, the clipped rect of every Salão raycast target from `Clicks.cs` lies outside the Room_Grid_Area beyond Overlap_Tolerance (rect math); the report names the level, the element and the measured clearance
    - **Property 12: Salão tiles stay clickable**
    - **Validates: Requirements 14.5**

  - [ ] 6.6 Implement the O dia states OD-1 to OD-3 and their checks
    - OD-1 the first day of a new game; OD-2 8 workers, one out of position, Large_Values till and counts set through `Shift`'s private setters by reflection and formatted by `DayHud.Sync()`, with the game clock paused; OD-3 the real shift ticked with `Shift.Tick(0.05f)` until closing time, with "Fechando!" shown
    - Checks: clock, "Caixa do dia", tally line and speed buttons inside the top plaque and apart (15.1); 8 chips inside the Viewport, each with name, position and mood bar inside the chip (15.2); the complete banner (15.3); the hint apart from every chip (15.4)
    - _Requirements: 15.1, 15.2, 15.3, 15.4, 20.1_

  - [ ] 6.7 Implement the Menu states MN-1 and MN-2 and their checks
    - MN-1 a first launch without records; MN-2 Large_Values records reached through Menu → Empresa → "Voltar"; both measured with the sign at ±1.4° and the parade maids at ±3.5°
    - Checks: a line break at each of the two record-group separators (16.1); the complete credits inside their rect and apart from the sign (16.2); sign, tagline, parade maids and plates, play button, "Nova empresa" and records apart (16.3)
    - _Requirements: 16.1, 16.2, 16.3, 20.1_

  - [ ] 6.8 Implement the Dialog states MN-3, ES-4, EL-5, LJ-5, LJ-6, DG-1 and DG-2 and their checks
    - Real paths as in the design: "Nova empresa" (MN-3); a click on an empty slot with the longest position description (ES-4); "Dispensar" for the Longest_Values maid name (EL-5); a gift price button with the longest-name maid first (LJ-5); money set to 0 without `Touch()`, then the "Faltam N moedas." refusal (LJ-6); a corrupt save (DG-1); "Abrir o dia" with an empty required position (DG-2)
    - Checks: title, body and buttons inside the box (17.1); two buttons apart, each label inside its face (17.2); a click outside the box reaches only the shade (17.3)
    - Add a plain `[Test]` to `LayoutCheckTests.cs` that asserts that the registered ids equal the 40 ids of the Audit States table
    - _Requirements: 17.1, 17.2, 17.3, 20.1_

- [ ] 7. Run the Layout_Check on the Baseline and record its outputs
  - [ ] 7.1 Record the "antes" captures and the failure report
    - Run `NEKO_CAPTURE_DIR="$PWD/Logs/ui01/antes" NEKO_LAYOUT_REPORT="$PWD/Logs/ui01/antes.jsonl" tools/run-tests.sh PlayMode Neko.Tests.PlayMode.Layout`
    - Expected: the Audit_State tests fail (20.4); every other test of the filter passes
    - Check that `Logs/ui01/antes/` holds at least one capture per Audit_State and resolution; copy the XML and the log to `Logs/ui01/baseline/`
    - _Requirements: 20.4, 20.5, 20.6_

- [ ] 8. Checkpoint: Layout_Check on the Baseline
  - Run the Layout_Check filter with `NEKO_LAYOUT_REPORT="$PWD/Logs/ui01/antes-repeat.jsonl"`, `tools/run-tests.sh EditMode`, `tools/run-tests.sh PlayMode` and `DOTNET_ROLL_FORWARD=Major tools/simtest.sh`. No Game_UI file has changed yet. The Layout_Check Audit_State tests fail with the same violations as `Logs/ui01/antes.jsonl`; a difference is non-determinism in the Layout_Check, which is fixed before task 9 (then repeat task 7.1 and this checkpoint). Every other test passes, including every existing EditMode and PlayMode test (read the PlayMode XML, because the Audit_State failures make the whole run fail). Ensure all tests pass apart from the recorded Baseline failures, ask the user if questions arise.

- [ ] 9. Write the Defect_Log from the Baseline run
  - [ ] 9.1 Create `docs/roadmap/ui-01-evidence/DEFECT_LOG.md` with every entry
    - One entry per observed and per suspected item of the requirements Introduction, with the design's provisional IDs and the columns of the design's Data Models; statuses per resolution from `Logs/ui01/antes.jsonl` and the "antes" captures; the cause (C1, C2, C3 or a described cause); the failing rule ids, or `captura` where only the capture shows the defect
    - New entries for Baseline failures that no listed item explains; the empty Escalação position with maids on the bench recorded as a valid state; the Findings outside UI-01 recorded as out of scope; the Salão scroll-direction change noted under D-SL-4 and D-SL-5 as the owner's change of 29/10/2026; the rig path and probe results from `Logs/ui01/prototype.md`, and the unverified stale-glyph risk of a live window resize from the design's "Baseline hierarchy facts"
    - D-LJ-4 judged by eye on the "antes" captures, with the WCAG 2.2 ratio of the flat `Palette.cs` colours computed by hand as support; no probe measures contrast
    - If a capture shows a listed defect that a rule evaluates but the Layout_Check did not report, fix the checker and repeat 7.1 before continuing
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.7, 20.4_

  - [ ] 9.2 Plan each fix in `DEFECT_LOG.md`
    - Fill "Correção" for each "reproduzido" entry from the design's per-screen and Shared_Component tables; "não reproduzido" entries get no fix; D-OD-4 and D-OD-6 read `adiado para UI-02`
    - Write the Shared_Component justification table for `SpriteFactory.cs`, `UiKit.cs` and `UiRoot.cs`, and for `MaidCard.cs` only if card entries reproduce
    - If a reproduced entry needs a Shared_Component change that the design does not list, stop and ask the owner
    - _Requirements: 1.6, 19.8_

- [ ] 10. Apply the shared C1 fix
  - [ ] 10.1 Add `SpriteFactory.Panel()` to `Assets/_Game/Scripts/Game/Art/SpriteFactory.cs`
    - One cached white 66×66 sprite: a rounded rect that touches the texture edges, radius 32 px, the 1.2 px softness of `ShapeRasteriser`, `Sprite.Create(..., pixelsPerUnit: 100, extrude: 0, SpriteMeshType.FullRect, border: (32, 32, 32, 32))`; kept in the cache so `Clear()` releases it
    - `ShapeRasteriser`, `ShapeSpec`, `ShapeId` and every existing `SpriteFactory` method stay unchanged
    - _Requirements: 3.2, 4.3, 19.8_

  - [ ] 10.2 Switch `UiKit.Panel` in `Assets/_Game/Scripts/Game/Ui/UiKit.cs` to the sliced sprite
    - `image.sprite = Sprites.Panel()`, `image.type = Image.Type.Sliced`, `image.pixelsPerUnitMultiplier = 32 / R`, with R = min(w, h) / 2 when `corner >= 0.5`, else R = lerp(0.04, 0.42, corner) / 0.84 × min(w, h) / 2; the signature and the rest of `UiKit` stay unchanged
    - Run `tools/run-tests.sh EditMode`: `ShapeRasteriserTests.EveryShapeDrawsSomethingAndDoesNotFillTheWholeSprite`, `SpriteFactoryTests.ClearingReleasesEverything`, `ArtTests` and `FurnitureArtTests` pass
    - _Requirements: 3.2, 4.3, 19.8_

  - [ ] 10.3 Record the Layout_Check after the C1 fix
    - Run the Layout_Check filter with `NEKO_LAYOUT_REPORT="$PWD/Logs/ui01/after-c1.jsonl"` and `NEKO_CAPTURE_DIR="$PWD/Logs/ui01/after-c1"`, and look at the captures for the C1 look (panels at full size, full-window backdrops and shades)
    - In `DEFECT_LOG.md`, mark the entries the C1 fix resolved, and add each failure that the C1 fix introduced to the entry list of its Screen under D-ALL-1
    - _Requirements: 1.6, 19.8, 19.9_

- [ ] 11. Checkpoint: after the C1 fix
  - Run the Layout_Check filter, `tools/run-tests.sh EditMode`, `tools/run-tests.sh PlayMode` and `DOTNET_ROLL_FORWARD=Major tools/simtest.sh`. Every test outside the Layout_Check Audit_State tests passes. The remaining Audit_State failures are those in `Logs/ui01/after-c1.jsonl`, on Screens whose tasks are still open. Ensure all tests pass apart from those failures, ask the user if questions arise.

- [ ] 12. Fix the Screens in UI-01 order
  - Each sub-task changes only what the Layout_Check reports for its Screen in `Logs/ui01/after-c1.jsonl` or a later run, including the re-tune that the C1 fix requires; entries recorded as "não reproduzido" get no change
  - Follow the design's per-screen table and the C2 layout rules: content within 1136 units at 960×600, corner elements anchored to the canvas edges, no read of `Screen.width` or the canvas width at Build time
  - Keep `Strings.cs`, every colour value, the `CanvasScaler`, the camera and the existing test files, scenes, `ProjectSettings/`, `Packages/`, CI and `tools/` runners unchanged. `Typeface.cs`, `Screen.cs`, `Scenery.cs` and `Logo.cs` change only where the Layout_Check proves a defect in them, with a Shared_Component justification in `DEFECT_LOG.md`
  - Lower no `fontSize`, no `Typeface.cs` size and no `localScale` of a text-holding object below its Baseline value, add no best fit, and change no text colour or opacity outside the Loja rows (Requirement 7, checked by the review of task 14.4)
  - Run the Screen's Audit_States until they pass at both resolutions, write their captures to `Logs/ui01/work/` and look at them, and add each change with its entries to the change map in `DEFECT_LOG.md`
  - [ ] 12.1 Fix the Loja in `Assets/_Game/Scripts/Game/Ui/ShopScreen.cs`
    - States LJ-1 to LJ-4; entries D-LJ-1 to D-LJ-4 and any new Loja entry
    - Row (850×62, now drawn full) laid out left to right: icon slot (Móveis only), name and description column, "No estoque: N" in its own right-aligned slot, price plank at the right padding; remove the 84% compensation (icon at x = −322, text indent 118)
    - Only if task 9.1 records D-LJ-4 as "reproduzido": description `Palette.Text` instead of `TextDim`, "No estoque" `Logo.BoardDeep` instead of Brass, price label `Typeface.Body` bold instead of `Small`; face tints and the other label colours stay as in the Baseline
    - Title, balance, tabs and "Voltar" placed by the C2 rules, with the list inside 1136 units
    - _Requirements: 2.1, 2.2, 2.3, 5.1, 5.2, 5.3, 7.2, 8.1, 8.5, 8.7_

  - [ ] 12.2 Fix the Balanço and the Falência in `Assets/_Game/Scripts/Game/Ui/BooksScreen.cs`
    - States BL-1 to BL-4 and FL-1; entries D-BL-1 to D-BL-5 and D-FL-1
    - Tally chips in a wrapping block of 7 columns × 148 units under the ledger columns, chips 72 units tall (two rows for 13 chips, two-line captions); profit and balance lines anchored to the bottom; budget inside the 588-unit inner panel as in the design (verdict 20–94, subtitle 96–120, columns 132–300, chips 312–464, profit 476–516, balance 520–548)
    - Falência: the C1 fix only, unless the Layout_Check still reports FL-1
    - _Requirements: 9.1, 9.2, 9.3, 9.5, 9.6_

  - [ ] 12.3 Fix the Elenco in `Assets/_Game/Scripts/Game/Ui/RosterScreen.cs`, and in `MaidCard.cs` for card entries
    - States EL-1 to EL-4; entries D-EL-1 to D-EL-5
    - Detail plaque fixed at 540 units, 16 units from the right edge; grid stretched from 16 units on the left to 16 units before the detail panel (564 units at 960×600, 692 at 1280×720) with rows of 4 cards; header 640 units wide, right-aligned, on one line at Large_Values; detail column as in the design (two-line "Etiquetas" value, four stat rows, three trait lines, two-line flavour and signature, "Dispensar" plank)
    - Card entries still reported after the C1 fix (for example the position tab height at card scale 0.62) are fixed in `Assets/_Game/Scripts/Game/Ui/MaidCard.cs` without changing the card composition; add the Shared_Component justification, then run EM-1 to EM-4, ES-1 to ES-3, CX-3 and CX-4 and confirm that no card-internal failure remains or appears there
    - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5, 10.6, 19.8, 19.9_

  - [ ] 12.4 Fix the Empresa in `Assets/_Game/Scripts/Game/Ui/HubScreen.cs`
    - States EM-1 to EM-4; entries D-EM-1 to D-EM-5
    - Header band (y 14–78) with the monogram and wordmark on the left and "Voltar" on the right, unchanged; stats plaque below the band (top 86, width up to 1020) so it covers neither; figure columns sized for Large_Values ("Caixa" with "9.999.999 ¥" is the widest); chemistry line, today strip (8 cards, 934 units), plank row (936 units) and "Abrir o dia" moved down to fit 720 units; plank labels inside the full-size faces
    - _Requirements: 11.1, 11.2, 11.3, 11.4, 11.5_

  - [ ] 12.5 Fix the Escalação in `Assets/_Game/Scripts/Game/Ui/LineupScreen.cs`
    - States ES-1 to ES-3; entries D-ES-1 to D-ES-5
    - Bench stretched between 26-unit side offsets (1100 units at 960×600), still scrolling horizontally; "Banco" aligned with the bench's left edge; the 10-slot board (1062 units) starts below the chemistry plaque; "Escalar sozinho" and "Limpar" 44 units tall; slot and bench assignment unchanged
    - _Requirements: 6.1, 12.1, 12.2, 12.4, 12.5, 12.6_

  - [ ] 12.6 Fix the Caixas and the reveal in `Assets/_Game/Scripts/Game/Ui/BoxScreen.cs`
    - States CX-1 to CX-4; only the entries the Layout_Check reports, at 960×600 or in the reveal; no change when none is reported
    - _Requirements: 13.1, 13.2, 13.3_

  - [ ] 12.7 Fix the Salão in `Assets/_Game/Scripts/Game/Ui/HallScreen.cs`
    - States SL-1 to SL-4; entries D-SL-1 to D-SL-6
    - Decision point: the stock column is the fix for D-SL-4 and D-SL-5. If neither is "reproduzido" at either resolution, stop and ask the owner before moving the stock
    - Top bar: plaque 48 units tall at y 8–56 from the top, stretched to the safe width, with the title on the left and the metrics on the right on one line
    - Stock column: `UiKit.ScrollList(transform, new Vector2(128f, 648f), 8f)` in place of `UiKit.ScrollRow`, its viewport anchored to the top-left corner at x 8 and 64 units below the top; the content's `VerticalLayoutGroup` padding set to 8 on every side inside `HallScreen`, with `UiKit.cs` unchanged; cards 112×100 with the 64×58 icon slot at the top inset and a two-line "<name> ×N" label below it; the empty-stock text in a 112×48 label
    - Hint: a non-raycast label of 864×28 units, anchored bottom-centre with its bottom edge at y 8, on one line; "Voltar" unchanged (120×44 at (−26, −78) from the top-right corner)
    - Run SL-4 at all ten café levels at both resolutions; the design's per-level table gives the expected clearances, the smallest being 18.4 units at levels 9 and 10 at 960×600
    - _Requirements: 5.1, 5.2, 5.3, 14.1, 14.2, 14.3, 14.4, 14.5, 18.3_

  - [ ] 12.8 Fix the O dia in `Assets/_Game/Scripts/Game/Ui/DayHud.cs`
    - States OD-1 to OD-3; entries D-OD-1, D-OD-2, D-OD-3 and D-OD-5; D-OD-4 and D-OD-6 stay deferred to UI-02 with no change
    - Top plaque about 860 units wide with a 380-unit till column, so the tally line stays on one line at Large_Values counts; chip strip anchored bottom-centre with the chips' own width (1112 units); hint and chips on separate rows
    - _Requirements: 15.1, 15.2, 15.3, 15.4_

  - [ ] 12.9 Fix the Menu in `Assets/_Game/Scripts/Game/Ui/MenuScreen.cs`
    - States MN-1 and MN-2; entries D-MN-1 and D-MN-2
    - `SetSession` joins the record groups with `"\n"` instead of `"\\n"`; `Strings.cs` stays untouched
    - Credits moved to the bottom right, under the parade, in a box of about 420×64 units (four lines); parade, records and tagline spacing adjusted only where the Layout_Check reports an intersection, at both sway extremes; the boot-time `SetSession` behaviour stays (Findings outside UI-01)
    - _Requirements: 16.1, 16.2, 16.3_

  - [ ] 12.10 Fix the Dialogs in `Assets/_Game/Scripts/Game/Ui/UiRoot.cs`
    - States MN-3, ES-4, EL-5, LJ-5, LJ-6, DG-1 and DG-2; entry D-DG-1 and any new Dialog entry
    - The `Dialog` box grows from 620×300 to 620×320, because the column needs 304 units; the `CanvasScaler` and the rest of `UiRoot` stay unchanged; add the Shared_Component justification to `DEFECT_LOG.md`
    - _Requirements: 17.1, 17.2, 17.3, 19.8, 19.9_

- [ ] 13. Checkpoint: after the screens
  - Run the Layout_Check filter, `tools/run-tests.sh EditMode`, `tools/run-tests.sh PlayMode` and `DOTNET_ROLL_FORWARD=Major tools/simtest.sh`. The Layout_Check passes in every Audit_State at both resolutions, and every other test passes. Ensure all tests pass, ask the user if questions arise.

- [ ] 14. Produce the "depois" captures, the Evidence_Folder and the complete Defect_Log
  - [ ] 14.1 Run the "depois" captures and the full suites
    - Run `NEKO_CAPTURE_DIR="$PWD/Logs/ui01/depois" NEKO_LAYOUT_REPORT="$PWD/Logs/ui01/depois.jsonl" tools/run-tests.sh PlayMode Neko.Tests.PlayMode.Layout`; it passes with an empty report
    - Run `tools/run-tests.sh EditMode`, `tools/run-tests.sh PlayMode` and `DOTNET_ROLL_FORWARD=Major tools/simtest.sh`; the task 13 results may be reused only if no file has changed since
    - Record each command with passed, failed or not run and the reason; the Console check is the Property 15 result; the WebGL build check stays not run unless `tools/build-webgl.sh` runs and the build is inspected at both window sizes
    - _Requirements: 19.9, 21.4, 21.5, 21.6_

  - [ ] 14.2 Copy the committed captures to the Evidence_Folder
    - Into `docs/roadmap/ui-01-evidence/antes/` and `docs/roadmap/ui-01-evidence/depois/`, copy the Requirement 21.1 set: LJ-1, LJ-2, LJ-3, EL-1, EL-2, BL-1 and BL-3, plus one Audit_State each of Menu, Empresa, Escalação, Caixas, Salão, O dia and Falência, one Overlay state and one Dialog state, each chosen for the most reproduced entries, at both resolutions (64 PNGs)
    - Add the Requirement 21.2 set: for each "reproduzido" entry that the first set does not already show at a resolution where it reproduces, one "antes" and one "depois" capture of a matching Audit_State at one such resolution, each file copied once; copy no other file
    - _Requirements: 21.1, 21.2, 21.3, 21.10_

  - [ ] 14.3 Complete `docs/roadmap/ui-01-evidence/DEFECT_LOG.md`
    - Per entry: the "depois" status and links; for each other resolution at which the entry reproduces, the file name that the regeneration command writes (1.3)
    - The change map of every Game_UI hunk to "reproduzido" entries, checked against the actual changes (1.6); the Shared_Component justifications (19.8); the capture index with Screen, Audit_State, resolution, folder and source (21.3); the 21.1 captures without an entry, under their Audit_State (21.11)
    - "Regenerar capturas": the exact "depois" command on the pull request head; the exact "antes" command on the commit hash that the orchestrator supplies for the state after task 8, or, without one, on the Baseline revision with `Assets/_Game/Tests/PlayMode/Layout/` and `Layout.meta` copied from the pull request head; which rig path ran (21.13)
    - "Validação": the results of 14.1, and the statement that visual approval covers layout and legibility only, not final art (21.5, 21.7)
    - "Validação" also states, per Requirement 21.14, how each criterion that the Layout_Check does not evaluate was verified and the result: Requirement 7 by the review of task 14.4 and the captures; 18.1 and 18.2 by the existing PlayMode tests, the click checks of 8.5, 10.6, 11.5 and 17.3, and the review; 8.2–8.4, 8.6, 9.4, 12.3, 18.3, 18.4, 19.2 and 19.3 by the review; 19.4 by the review and `ArtTests` and `FurnitureArtTests`. Fill in the test results from 14.1 here; task 14.4 adds the review results
    - _Requirements: 1.3, 1.6, 19.8, 21.3, 21.5, 21.7, 21.11, 21.13, 21.14_

  - [ ] 14.4 Check the scope of the Change_Set
    - List every changed and added file, from the orchestrator or with read-only Git queries, and confirm: `Strings.cs`, `Assets/_Game/Scripts/Sim/`, `Assets/_Game/Scripts/Game/Core/`, `Assets/_Game/Scripts/Game/Input/` and every Protected_Path unchanged; colour values unchanged; no new Screen, Overlay, animation, tutorial, sound, imported asset or gameplay feature; a `.meta` for every new file and folder under `Assets/`; no PNG under `Assets/`; nothing under `.kiro/specs/`; the owner's local package and URP changes left out
    - Review the diff of the Game_UI files: no `fontSize`, `Typeface` size or text-holding `localScale` lowered, no best fit added, `CanvasScaler` unchanged (7.1); Loja row colours only as 7.2 allows, and no other text colour or opacity changed (7.2, 7.3); no click handler, navigation target or action changed (18.1, 18.2); every `ScrollRect` keeps its Baseline axis, movement type and sensitivity, except the Salão stock, which scrolls vertically (18.3); no displayed value format or binding to GameSession, Profile, Shift or DayLedger changed (8.2–8.4, 8.6, 9.4, 12.3, 18.4); `Typeface.Font` on every new Text and the MaidCard composition unchanged (19.2, 19.3); no World_Art builder changed, and the existing `SpriteFactory` methods, `ShapeRasteriser`, `PixelCanvas`, `FurnitureArt` and the room drawing code unchanged (19.4)
    - Record each review result in the "Validação" section of `DEFECT_LOG.md` (21.14), and report the file list and the results to the orchestrator
    - _Requirements: 7.1, 7.2, 7.3, 8.2, 8.3, 8.4, 8.6, 9.4, 12.3, 18.1, 18.2, 18.3, 18.4, 18.5, 18.6, 18.7, 19.1, 19.2, 19.3, 19.4, 19.5, 19.6, 19.7, 21.8, 21.10, 21.14_

- [ ] 15. Move the UI-01 roadmap row to REVIEW
  - [ ] 15.1 Edit the UI-01 row of `docs/ROADMAP.md`
    - After the orchestrator has opened the pull request and supplied its URL: set the status to REVIEW and add a link to the pull request and a link to `roadmap/ui-01-evidence/DEFECT_LOG.md`, in the style of the ART-01 row; nothing else in the file changes
    - _Requirements: 21.12_

## Notes

- No task is optional. The owner authorized the Layout_Check as the proof of the fix on 29/10/2026 and trimmed it to its core rules on 02/11/2026.
- Properties 1, 4, 5, 6, 7, 9, 12 and 15 run exhaustively over the Audit_Scope; none uses random inputs. Each property is implemented once and tagged `// Feature: ui-01-layout, Property N: <title>`. The removed property numbers (2, 3, 8, 10, 11, 13, 14, 16, 17 and 18) are not reused.
- Requirement 21.9 (one pull request, auto-merge requested only after the owner has reviewed the "depois" captures) and every commit are the orchestrator's steps.
- Checkpoints 8, 11 and 13 each run the Layout_Check filter and the full EditMode, PlayMode and simulation suites.

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1"] },
    { "id": 1, "tasks": ["1.2"] },
    { "id": 2, "tasks": ["1.3"] },
    { "id": 3, "tasks": ["2.1"] },
    { "id": 4, "tasks": ["2.2"] },
    { "id": 5, "tasks": ["2.3"] },
    { "id": 6, "tasks": ["2.4"] },
    { "id": 7, "tasks": ["2.5"] },
    { "id": 8, "tasks": ["3.1"] },
    { "id": 9, "tasks": ["3.2"] },
    { "id": 10, "tasks": ["3.3"] },
    { "id": 11, "tasks": ["3.4"] },
    { "id": 12, "tasks": ["4.1"] },
    { "id": 13, "tasks": ["4.2"] },
    { "id": 14, "tasks": ["4.3"] },
    { "id": 15, "tasks": ["4.4"] },
    { "id": 16, "tasks": ["4.5"] },
    { "id": 17, "tasks": ["4.6"] },
    { "id": 18, "tasks": ["4.7"] },
    { "id": 19, "tasks": ["5.1"] },
    { "id": 20, "tasks": ["5.2"] },
    { "id": 21, "tasks": ["5.3"] },
    { "id": 22, "tasks": ["6.1"] },
    { "id": 23, "tasks": ["6.2"] },
    { "id": 24, "tasks": ["6.3"] },
    { "id": 25, "tasks": ["6.4"] },
    { "id": 26, "tasks": ["6.5"] },
    { "id": 27, "tasks": ["6.6"] },
    { "id": 28, "tasks": ["6.7"] },
    { "id": 29, "tasks": ["6.8"] },
    { "id": 30, "tasks": ["7.1"] },
    { "id": 31, "tasks": ["9.1"] },
    { "id": 32, "tasks": ["9.2"] },
    { "id": 33, "tasks": ["10.1"] },
    { "id": 34, "tasks": ["10.2"] },
    { "id": 35, "tasks": ["10.3"] },
    { "id": 36, "tasks": ["12.1"] },
    { "id": 37, "tasks": ["12.2"] },
    { "id": 38, "tasks": ["12.3"] },
    { "id": 39, "tasks": ["12.4"] },
    { "id": 40, "tasks": ["12.5"] },
    { "id": 41, "tasks": ["12.6"] },
    { "id": 42, "tasks": ["12.7"] },
    { "id": 43, "tasks": ["12.8"] },
    { "id": 44, "tasks": ["12.9"] },
    { "id": 45, "tasks": ["12.10"] },
    { "id": 46, "tasks": ["14.1"] },
    { "id": 47, "tasks": ["14.2"] },
    { "id": 48, "tasks": ["14.3"] },
    { "id": 49, "tasks": ["14.4"] },
    { "id": 50, "tasks": ["15.1"] }
  ]
}
```
