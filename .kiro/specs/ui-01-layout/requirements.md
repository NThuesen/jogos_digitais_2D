# Requirements Document

> **ESPECIFICAÇÃO HISTÓRICA DO UI-01 (29/10–02/11/2026).** O PR #31 integrou somente a infraestrutura inicial do Layout_Check; telas ainda não foram corrigidas. O responsável aprovou em 03/11 uma execução **enxuta por marcos**: [UI-01 vigente](../../../docs/roadmap/UI-01_LAYOUT.md) e [direção visual ilustrada](../../../docs/art/CURRENT_VISUAL_DIRECTION.md). Preservar requisitos, decisões e tarefas originais para rastreabilidade; **não executar as 15 etapas/40 estados como gate obrigatório** nem aplicar câmera, baseline ou pixel art antigos automaticamente. Os estados não implementados continuam pendentes, não aprovados por inferência.


## Introduction

This spec covers roadmap task **UI-01 — Estabilizar layout e legibilidade** for *Neko Maid Inc.* (Unity 6000.5.9f1, 2D URP, desktop WebGL, mouse only). The owner asked: "I would like now for you to fix the UI, currently a lot of menus and texts either collide or they clip outside their boxes, please fix them".

UI-01 fixes presentation on existing screens; it is not an art redesign. The whole interface is built in code with legacy uGUI `Text` and procedurally generated sprites under `Assets/_Game/Scripts/Game/Ui/`; nothing is imported. The task contract is `docs/roadmap/UI-01_LAYOUT.md`, the owner's observations are in `docs/roadmap/VISUAL_BASELINE.md`, and the evidence rules come from `docs/agent/UNITY.md` and `docs/agent/VALIDATION.md`. The request is broader than the UI-01 attack order (Loja, Balanço, Elenco, then a regression pass on Empresa, Escalação, Caixas and Salão), so this spec covers every Screen, the card reveal and the Dialogs while keeping that priority order.

Work happens on branch `ui/ui-01-layout`, created from `art/furniture-pixel-art` (an unmerged PR that adds pixel-art furniture and furniture icons to Loja and Salão). That starting point is the Baseline. Player-facing text stays in pt-BR.

### Background: causes verified in code

Recorded as context for the design phase. They are not design decisions; the design chooses how to address them.

- **C1 — Panels are drawn smaller than their layout boxes.** `UiKit.Panel` draws `ShapeId.RoundedRect` through `ShapeRasteriser`, whose rounded box has a half-extent of 0.84 in [-1, 1] space. The shape covers only the middle 84% of its 96×96 texture, and `Image.Type.Simple` stretches that texture to the rect. Every panel (the three nested panels of `Scenery.Plaque`, `Scenery.Plank` buttons, `MaidCard` frames, the Dialog box, Balanço rows and chips, Loja rows, Salão stock cards) is drawn about 8% inside its rect on each side, while children are positioned against the full rect. The corner radius is in normalized units, so it is stretched by the aspect ratio and wide panels get long, squashed ends. `ShapeId.RoundedRect` is also used by world art (`MaidSpriteBuilder`, `CustomerView`) and by existing EditMode tests (`ArtTests`). `ShopScreen` already compensates for C1 locally when it places the furniture icon.
- **C2 — The canvas loses width at 960×600.** The `CanvasScaler` in `UiRoot` uses Scale With Screen Size, reference 1280×720, Match Width Or Height with `matchWidthOrHeight = 1` (match height). At 960×600 (16:10) the canvas is 1152×720 reference units, 128 units narrower than the 1280 units the screens are laid out for, so full-width content is cut or overlaps at the sides. The world camera (`CameraRig`, solved against 4:3) is separate and is not affected.
- **C3 — Long text spills out of its box.** `UiKit.Label` sets `horizontalOverflow = Wrap` and `verticalOverflow = Overflow` without best fit, so a label longer than its width wraps onto extra lines that are drawn below its rect.

### Defects observed in 1280×720 captures

Source: `Logs/art-review/png-after/*.png`, written by the existing PlayMode `CaptureTests` at 1280×720 (the frame named `00_01_menu` shows Empresa). Roupas, Presentes, the card reveal, Dialogs, Falência and the Menu were never captured. Items marked † were found while reviewing the same captures for this spec. Every item must be reproduced at 960×600 and at 1280×720 before it is fixed.

| Screen | Observation |
|---|---|
| Empresa | The "NEKO M…" wordmark is partly hidden behind the stats plaque; "Custo fixo por dia" runs onto the plaque's right edge; the text of the highlighted "Escalação" plank and of "Abrir o dia" is larger than the plank face; card stat numbers overflow the card face. |
| Escalação | Card stat numbers overflow the card face; the "Banco" heading sits outside the bench panel; † the chemistry bar runs to the plaque's visible side edges. |
| Elenco | The header "6 / 6 · Custo fixo por dia 648 ¥" wraps to two lines. In the detail panel, "Raridade" sits above the visible top edge, the small card overlaps the panel corner, attribute labels start left of the visible edge and the numbers sit past the right edge, "Traços", "Nenhum traço" and the flavour line start outside the visible edge, and the "Etiquetas" value wraps. Grid cards' stats row and "Salário" line overflow the card; the first card row overlaps the grid panel's top edge. |
| Caixas | Acceptable at 1280×720. |
| Loja (Móveis only) | Item names started left of the visible row (partly addressed by the furniture branch's icon indent); "No estoque: N" overlaps the price button; row ends look stretched. |
| Salão | "Arrumar o salão" starts left of the plaque's visible edge; "Tema sem tema" runs past the plaque's right edge; stock card labels overflow ("Mesa para 4 ×1") or wrap and spill below the card ("Lanterna de sakura ×1"); † the stock bar covers part of the room's bottom row of tiles. |
| O dia | The clock "1:26" is half outside the top plaque on the left; the stats line "Atendidos 1 · Pedidos errados 0 · Foram embora 0" wraps and its second line spills below the plaque; † worker chip names touch the chip's visible top edge; † the worker chips cover part of the room's bottom row of tiles (recorded; fix deferred to UI-02, see Owner decisions). |
| Balanço | The verdict title overlaps the plaque's top edge; the row of 7 tally chips is wider than the plaque, so the first and last chips sit outside it; "Caixa depois …" sits on the plaque's bottom edge; ledger row labels start left of the visible row pills. |

### Suspected defects (code inspection only)

These are hypotheses, not observations. Each is reproduced first; a hypothesis that does not reproduce is recorded as "não reproduzido" and gets no fix.

- At 960×600 (canvas 1152 units wide): the rects of the Elenco grid (700 units) and detail panel (500 units) overlap; the Escalação bench (1200), the Salão top bar (1240) and the Salão stock bar (1180) are wider than the canvas; the Empresa stats plaque covers most of the "Voltar" plank's click area (16 of its 120 units are already covered at 1280×720).
- Menu: the records text joins its lines with the two characters `\n` (the C# literal `"\\n"` in `MenuScreen.SetSession`) where line breaks are intended; the credits text may spill out of its 330×92 box and overlap the hanging sign.
- Dialog, Falência and the Elenco detail panel: content laid out against the full rect is wider or taller than the visible panel, so text and buttons can reach or cross the visible edges, for example for a maid with 3 traits and a bond-5 signature line.
- Balanço with many level-ups: 5 tally chips plus one chip per maid who levelled up (up to 13 chips) exceed the plaque width.
- Escalação: "Escalar sozinho" and "Limpar" are 42 units tall, below `UiKit.MinimumHitSize` (44).
- Salão: `NekoInput` ignores world clicks over the interface, so the stock bar that covers part of the bottom row probably blocks placement and removal clicks on those tiles.
- Salão and O dia: the room's margins change with the café level; the 12×9 room of level 8 leaves about 60 units above and below the tiles, so the Salão top bar and the O dia top plaque may also cover the top row of tiles. For the Salão this matters for clicks (Requirement 14.5); for O dia the covering is visual only and its fix is deferred to UI-02 (see Owner decisions).

### Not defects

- An Escalação position left empty while a maid sits on the bench is a valid player choice, confirmed by the owner on 27/10/2026.
- A loss on the first day and the brown temporary palette are not layout defects.

### Out of scope

Maid portrait and chibi art (ART-02); UI-02 polish; the earlier economy proposal; palette, theme, font and card-system changes; new assets, screens, animations, onboarding, balance changes and extra systems; publishing (itch.io); window sizes other than 960×600 and 1280×720, which remain exploration only.

Deferred by the owner on 29/10/2026:

- **Visual covering of room tiles** by the Salão and O dia interface, deferred to UI-02. These were the criteria that kept the Salão Visible_Shapes and Text_Bounds, and the O dia top plaque and worker chips, outside the Room_Grid_Area (numbered 14.6 and 15.4 before the renumbering of 29/10/2026). Meeting them for every room size needs camera changes, which Requirement 18.6 forbids, or a HUD relayout that the roadmap reserves for later. Clicks on Salão tiles stay in scope through Requirement 14.5.
- **Larger cards and larger card text**, deferred to ART-02 and UI-02. UI-01 neither enlarges cards nor raises card text to the 11 px floor.

### Owner decisions (29/10/2026)

The owner approved these requirements on 29/10/2026 with the following answers to the review questions.

1. **Room overlap.** Requirement 14.5 stays: no Salão Game_UI Hit_Area inside the Room_Grid_Area, because `NekoInput` ignores world clicks over the interface and a covered tile cannot be clicked, which is a usability bug. The purely visual criteria on the Salão and O dia interface covering tiles are removed and listed under Out of scope as deferred to UI-02; Requirement 15 was renumbered after the removal. Audit_State SL-4 stays; Audit_State OD-4 is dropped because no remaining criterion uses it.
2. **Card text size.** UI-01 does not enlarge cards or card text. Requirement 7.1 stands as written: each Text_Element renders at least at the smaller of 11 px and its Baseline size, so text that is below 11 px today must not get smaller and is not raised to 11 px. Tiny card text is deferred to ART-02 and UI-02. (Since 02/11/2026, Requirement 7.1 states this as a review of the Change_Set: no text size is lowered below its Baseline value.)
3. **Thresholds and envelopes.** Edge_Clearance 4 Canvas_Units, Screen_Margin 8 Canvas_Units, Overlap_Tolerance 1 Canvas_Unit, Minimum_Text_Size 11 px at 960×600, Large_Values and Longest_Values are accepted as written. (Since 02/11/2026, Minimum_Text_Size is no longer measured and has left the Glossary.)
4. **Contrast.** WCAG AA applies to the Loja rows only (Requirement 7.2). Elsewhere contrast must not get worse than in the Baseline (Requirement 7.3). (Since 02/11/2026, no contrast is measured: Requirement 7.2 fixes the Loja row colours and Requirement 7.3 keeps the colour and opacity of every other text as in the Baseline.)
5. **Scaling.** Whichever fix the design chooses for C2, every Text_Element must still satisfy Requirement 7.1 at both Reference_Resolutions. (Since 02/11/2026, Requirement 7.1 also keeps the `CanvasScaler` settings unchanged.)
6. **Evidence storage.** The before and after captures and the Defect_Log are committed with the pull request under `docs/roadmap/ui-01-evidence/`: PNG captures plus a `DEFECT_LOG.md` index that maps each entry to its captures. The folder is outside `Assets/`, so `DocsTests.NoTextureOrAudioFileWasEverImported` still holds; ART-01D already committed review PNGs under `docs/art/`. The committed set is curated: the Audit_States that Requirement 21.1 lists plus every reproduced entry that Requirement 21.2 covers.
7. **Automated layout check.** Requirement 20 stays. `docs/ROADMAP.md` restriction 7 asks for authorization before new infrastructure in this cycle; the owner explicitly authorized the Layout_Check as new test infrastructure on 29/10/2026. The owner trimmed the Layout_Check to its core rules on 02/11/2026 (see "Owner decisions (02/11/2026): trimmed Layout_Check").

### Design review changes (29/10/2026)

The owner approved the design on 29/10/2026 and changed these criteria as part of that review (see "Owner decisions (29/10/2026)" in `design.md`):

- **Requirement 18.3**: the Salão stock scrolls vertically instead of horizontally, because the design moves the stock into a column beside the room (design decision 2).
- **Requirement 21.2**: the committed captures beyond the Requirement 21.1 set are narrowed to one before and one after capture per reproduced entry (design decision 4). Requirement 21.13 was added so that every other capture can be regenerated, and Requirement 1.3 now accepts the file name that the regeneration command writes.
- **Requirement 21.12**: the UI-01 row of `docs/ROADMAP.md` moves to REVIEW (design decision 5).

### Owner decisions (02/11/2026): trimmed Layout_Check

On 02/11/2026 the owner chose "Trim the checker, then fix the screens": "Keep the core rules: text inside its box, no overlaps, fits the window, buttons clickable. Drop contrast measuring, the stored text-size snapshot, the separate navigation and art tests, and the extra property tests. I update the spec first; screen fixes start much sooner."

- The Layout_Check evaluates Requirements 2, 3, 4, 5, 6 and 14.5, plus the containment, separation and click criteria of Requirements 8 to 17 (Requirement 20.1).
- Requirement 7 is verified by review of the Change_Set: no text gets smaller, and no text colour changes outside the Loja rows. The Loja row colours change only as decided (Requirement 7.2). D-LJ-4, the design's provisional Defect_Log entry for hard-to-read Loja row text, is judged by eye on the captures.
- Requirements 18.1 and 18.2 are verified by the existing EditMode and PlayMode tests, the click checks of Requirements 8.5, 10.6, 11.5 and 17.3, and review of the Change_Set.
- Requirements 8.2 to 8.4, 8.6, 9.4, 12.3, 18.3, 18.4, 19.2 and 19.3 are verified by review of the Change_Set.
- Requirement 19.4 is verified by review of the Change_Set and by the existing `ArtTests` and `FurnitureArtTests` in `Assets/_Game/Tests/EditMode/Game/`.
- Requirement 21.14 asks the Evidence_Package to state how each of these criteria was verified and the result.
- Owner decision 3 of 29/10/2026 keeps its thresholds except Minimum_Text_Size, which is no longer measured and has left the Glossary. Owner decision 4 of 29/10/2026 no longer sets a measured contrast threshold.

## Glossary

- **Game_UI**: The code-built uGUI interface under `Assets/_Game/Scripts/Game/Ui/`, drawn on the single canvas that `UiRoot` creates: every Screen, the Overlay, every Dialog and the Shared_Components behind these layers.
- **Screen**: One full-screen page of the Game_UI: Menu (`MenuScreen`), Empresa (`HubScreen`), Escalação (`LineupScreen`), Elenco (`RosterScreen`), Caixas (`BoxScreen`), Loja (`ShopScreen`, tabs Móveis, Roupas and Presentes), Salão (`HallScreen`), O dia (`DayHud`), Balanço (`BooksScreen`) and Falência (`BustScreen`, defined in `BooksScreen.cs`).
- **Overlay**: The Caixas card reveal ("VEIO NA CAIXA"), shown over the Caixas Screen until the player clicks "Guardar".
- **Dialog**: The modal message box built by `UiRoot.Dialog`, with a title, a body and one or two buttons over a dark shade.
- **Active_Layer**: The topmost visible layer: the open Dialog if one is open, otherwise the open Overlay if one is open, otherwise the current Screen. Content under a Dialog or Overlay shade is dimmed by design and is not measured.
- **Shared_Component**: UI code used by more than one Screen, Overlay or Dialog: `UiKit.cs`, `Scenery.cs`, `UiRoot.cs`, `Screen.cs`, `MaidCard.cs`, `Logo.cs`, `Typeface.cs`, and the sprite generation that `UiKit.Panel` relies on (`SpriteFactory`, `ShapeRasteriser`).
- **World_Art**: Sprites drawn in the room by the world camera (maids, customers, furniture, room tiles, effects), plus the furniture sprites reused as Loja and Salão icons.
- **Baseline**: The project as it stands on branch `ui/ui-01-layout` before any UI-01 change, which is the head of `art/furniture-pixel-art` from which the branch was created.
- **Reference_Resolution**: A window size at which the Game_UI is evaluated: 960×600, the mandatory reference of UI-01 and of the roadmap, and 1280×720, the canvas reference and the resolution stated on the itch.io page. The two sizes have different aspect ratios (16:10 and 16:9) and are evaluated separately.
- **Viewport**: The whole game window at a Reference_Resolution. When the Viewport acts as a Container, the Viewport's Visible_Shape is the whole window.
- **Canvas_Unit**: One reference unit of the UI canvas after CanvasScaler scaling. With the Baseline scaler, 1 Canvas_Unit is 1 screen pixel at 1280×720 and 0.833 screen pixels at 960×600, where the canvas measures 1152×720 Canvas_Units.
- **Text_Element**: A visible uGUI `Text` with non-empty text in the Active_Layer, including button labels. A `UiKit.TrackedLabel` (one `Text` per glyph) counts as one Text_Element. Transient `FloatingLabel` effects that float over the room are not Text_Elements.
- **Text_Bounds**: The screen-space rectangle that encloses every glyph a Text_Element draws, on all lines including lines added by wrapping, excluding drop-shadow copies.
- **Visible_Shape**: The screen-space region in which an element's own sprite is drawn with an alpha of at least 0.5, after clipping by masks. For a rounded panel this is the drawn rounded shape, which can be smaller than the element's rect (C1). For a Composite_Element it is the union of the Visible_Shapes of the element's pieces.
- **Composite_Element**: An element built from layered pieces by design and measured as a whole: Plaque (three nested panels), Plank and Button (root, face, shine), Bar (track and fill), MaidCard (frame, face, archetype wash, portrait, position tab, trait stars), the Logo sign and monogram, and the Caixas crate (lid, band, monogram). The background pieces of a Composite_Element are the plates drawn behind its content: plaque layers; plank root, face and shine; bar track; card frame, face and wash; logo board layers; crate lid and band.
- **Container**: The plate that visually frames an element: the plaque inner panel, panel, row, chip, card face, tab, stock card or dialog box on which the element is drawn. For a label on a plank or button, the Container is the plank or button face. For an item in a list, the Container is the scroll area. For an element drawn directly on the Scenery, on a shade or over the room, the Container is the Viewport.
- **Container_Chain**: An element's Container, that Container's Container, and so on up to the Viewport.
- **Decor_Element**: A backdrop element meant to fill or bleed past the Viewport: the `Scenery` backdrop (wall, lamplight, stripes, floor, skirting, rail), Screen backdrops, Dialog and Overlay shades, and the hanger of the Menu sign.
- **Interactive_Control**: A Game_UI element that the player clicks: buttons, planks, tabs, price buttons, card buttons (Escalação slots and bench, Elenco grid), Salão stock cards, Dialog buttons and scroll areas.
- **Hit_Area**: The screen-space area in which a Game_UI element intercepts pointer clicks (the element's raycast target area). For an Interactive_Control, the Hit_Area is the area in which a click reaches the control.
- **Minimum_Hit_Size**: 44 × 44 Canvas_Units, the value of `UiKit.MinimumHitSize`.
- **Edge_Clearance**: 4 Canvas_Units, the minimum gap between Text_Bounds and the edge of the Container's Visible_Shape.
- **Screen_Margin**: 8 Canvas_Units, the minimum gap between a non-Decor element and the Viewport edge.
- **Overlap_Tolerance**: 1 Canvas_Unit. An intersection whose width or height is at most this value does not count as an intersection.
- **Room_Grid_Area**: The on-screen rectangle covered by the room's floor tiles, as framed by `CameraRig`, on the Salão and O dia Screens.
- **Settled**: The condition in which no one-shot UI tween is running (screen-show scale, punch, card reveal) and the layout has been rebuilt after the last change. The continuous idle sways on the Menu (the sign and the parade maids) are measured at both extremes of the sway.
- **Audit_State**: A named combination of Screen, Overlay or Dialog, session data and scroll position listed in the Audit States section.
- **Audit_Scope**: Every Audit_State at both Reference_Resolutions, each measured once Settled.
- **Large_Values**: Upper-envelope numbers used to build Audit_States. Balances (Empresa "Caixa", Loja balance, "Caixa depois"): 9.999.999 ¥. Per-day amounts (ledger rows, "Receita", "Despesa", "Lucro" and "Prejuízo", "Caixa do dia", "Custo fixo por dia", "Melhor faturamento"): 99.999 ¥. Fichas: 99.999. "Dia" and "Melhor dia": 999. Shift counts and Balanço tallies: 999. "Clientes atendidos": 999.999. "Lendárias encontradas" and "Empresas falidas": 99. Owned stock: 99 per item. Maid salary: 9.999 ¥; severance: 999. Salão metrics: two integer digits ("Lugares 99", "Ambiente 99.9", "Caminhada 99.9").
- **Longest_Values**: For each text field, the longest value that the game's tables can produce at test time: maid names (including the numbered fallback form, for example "Kaede 31"), archetype names, position names and descriptions, chemistry tags, trait lines, bond signature lines, flavour lines, shop item names and descriptions per tab, box descriptions, placeable names, theme names, shift failure reasons, save messages and purchase refusals.
- **Defect_Log**: The UI-01 record of every observed, suspected and newly found defect, with Screen, Audit_State, Reference_Resolution, status per Reference_Resolution ("reproduzido" or "não reproduzido"), cause, fix and capture references, kept in `docs/roadmap/ui-01-evidence/DEFECT_LOG.md`.
- **Evidence_Folder**: `docs/roadmap/ui-01-evidence/`, the repository folder that holds the Defect_Log and the committed before and after PNG captures.
- **Layout_Check**: The automated PlayMode verification added by UI-01 that puts the game in each Audit_State at each Reference_Resolution and measures the layout rules.
- **Evidence_Package**: What the UI-01 pull request presents for review: the Defect_Log and the before and after captures committed in the Evidence_Folder, plus the Console result and the test results reported in the pull request description.
- **Change_Set**: All repository file changes delivered by the UI-01 pull request.
- **Protected_Path**: A path guarded by the repository's `protected-change-guard` check: existing test files, `.github/` workflows, the `tools/` build and test runners, `Packages/`, `ProjectSettings/` and Unity scenes.

## Audit States

Each Audit_State is measured at both Reference_Resolutions once Settled (the Audit_Scope). Elements inside a scroll area are measured at a scroll position where they lie fully inside the scroll area; Requirement 5 covers reachability. Inactive Text_Elements and Text_Elements with empty text are not measured. Café levels 1 to 10 produce rooms from 7×5 to 13×9 tiles.

| ID | Screen or layer | Session data and view |
|---|---|---|
| MN-1 | Menu | First launch without records; the play button reads "Abrir a empresa". |
| MN-2 | Menu | Returning player with Large_Values records; the play button reads "Continuar". |
| MN-3 | Menu + Dialog | "Nova empresa" confirmation open. |
| EM-1 | Empresa | New game, day 1. |
| EM-2 | Empresa | Large_Values, café level 10 (level line ends in "no topo"), 8 maids in the lineup, cash warning line shown. |
| EM-3 | Empresa | A required position empty: not-ready line shown, "Abrir o dia" disabled. |
| EM-4 | Empresa | Empty lineup: no cards in the today strip. |
| ES-1 | Escalação | New game with the starting squad. |
| ES-2 | Escalação | Café level 8 or higher with 10 slots (Recepção 2, Salão 3, Cozinha 3, Palco 1, Gerência 1), 8 maids placed, at least one maid out of position, at least one empty slot, empty bench ("vazio"). |
| ES-3 | Escalação | Lineup cleared: 8 maids on the bench (wider than the bench area), not-ready warning shown. |
| ES-4 | Escalação + Dialog | Slot-pick Dialog "Escolha quem fica aqui" open with the longest position description. |
| EL-1 | Elenco | Roster at cap (8 / 8), Large_Values fixed cost, first maid selected. |
| EL-2 | Elenco | Detail of a maid with 3 traits, a bond-5 signature line, Longest_Values chemistry tags, level 20 / 20, rarity Lendária and salary 9.999 ¥. |
| EL-3 | Elenco | Detail of a maid without traits and without a signature line. |
| EL-4 | Elenco | Grid scrolled to the end. |
| EL-5 | Elenco + Dialog | Dismissal Dialog open for the Longest_Values maid name. |
| CX-1 | Caixas | All boxes affordable, 99.999 fichas. |
| CX-2 | Caixas | No box affordable. |
| CX-3 | Caixas + Overlay | Reveal of a Caixa Simples: 1 card. |
| CX-4 | Caixas + Overlay | Reveal of a Caixa Prata or Dourada with the roster full: 3 cards with duplicate notes. |
| LJ-1 | Loja, Móveis | Café level 10 stock, Large_Values balance, affordable and unaffordable rows, two-digit "No estoque" counts. |
| LJ-2 | Loja, Roupas | As LJ-1, for clothes. |
| LJ-3 | Loja, Presentes | As LJ-1, for gifts (gifts have no stock count). |
| LJ-4 | Loja, each tab | List scrolled to the end. |
| LJ-5 | Loja + Dialog | "Para quem?" gift Dialog with the Longest_Values maid name and gift name. |
| LJ-6 | Loja + Dialog | Purchase refusal Dialog with the longest refusal text. |
| SL-1 | Salão | Level-1 room (7×5), empty stock. |
| SL-2 | Salão | Largest room (13×9), theme bonus active with the longest theme name, two-digit metrics, every placeable in stock with two-digit counts (the stock scrolls). |
| SL-3 | Salão | An item with the Longest_Values placeable name picked, then a blocked placement message. |
| SL-4 | Salão | Each room size of café levels 1 to 10, with stock. |
| OD-1 | O dia | First day of a new game. |
| OD-2 | O dia | 8 workers, at least one out of position, Large_Values till and counts. |
| OD-3 | O dia | Closing time with "Fechando!" shown. |
| BL-1 | Balanço | Profitable day ("Dia excelente" or "Dia bom"). |
| BL-2 | Balanço | Loss day ("Prejuízo") with the cash warning. |
| BL-3 | Balanço | Every tally non-zero plus a level-up chip for each of 8 maids (13 chips), Large_Values amounts. |
| BL-4 | Balanço | "O café não abriu" with the longest shift failure reason as subtitle. |
| FL-1 | Falência | After a bankruptcy. |
| DG-1 | Dialog | Save error Dialog "Algo deu errado" with the longest save message. |
| DG-2 | Dialog | "Escalação de hoje" Dialog with the longest shift failure reason. |

## Requirements

### Requirement 1: Reproduce and record defects before fixing

**User Story:** As the owner, I want every defect reproduced in the running game before it is fixed, so that UI-01 fixes real problems and invents none from code reading.

#### Acceptance Criteria

1. THE Defect_Log SHALL contain one entry for each observed defect and each suspected defect listed in the Introduction.
2. THE Defect_Log SHALL record the status of each entry separately for 960×600 and for 1280×720, as "reproduzido" or "não reproduzido".
3. WHEN the Defect_Log records an entry as "reproduzido" at a Reference_Resolution, THE Defect_Log SHALL reference a before capture of the matching Audit_State at that Reference_Resolution, rendered from the Baseline, either as a file in the Evidence_Folder or as the file name that the regeneration command of Requirement 21.13 writes.
4. WHEN a defect that the Introduction does not list is found in an Audit_State, THE Defect_Log SHALL add an entry with the Screen, the Audit_State, the Reference_Resolution and a before capture.
5. THE Defect_Log SHALL name the cause of each "reproduzido" entry as C1, C2, C3 or another described cause.
6. THE Defect_Log SHALL map every Game_UI source change in the Change_Set to at least one entry recorded as "reproduzido" at one or both Reference_Resolutions.
7. THE Defect_Log SHALL classify an Escalação position left empty while a maid is on the bench as a valid state.

### Requirement 2: Fit the window at both Reference_Resolutions

**User Story:** As a player with a 960×600 or a 1280×720 window, I want every screen to fit the window, so that nothing is cut off at the edges.

#### Acceptance Criteria

1. THE Game_UI SHALL place the Visible_Shape of every non-Decor element and the Text_Bounds of every Text_Element of the Active_Layer inside the Viewport inset by Screen_Margin, across the Audit_Scope.
2. THE Game_UI SHALL present at 960×600 every Text_Element and Interactive_Control that the same Audit_State presents at 1280×720, either inside the Viewport or reachable by scrolling the scroll area that holds the element.
3. WHILE a Screen, the Overlay or a Dialog is shown, WHEN the Viewport changes from one Reference_Resolution to the other, THE Game_UI SHALL satisfy Requirements 2 to 6 on the Active_Layer once Settled, without a change of Screen.

### Requirement 3: Keep text inside its box

**User Story:** As a player, I want every text to sit completely inside the box that frames it, so that no words hang over panel edges or spill below them.

#### Acceptance Criteria

1. THE Game_UI SHALL keep the Text_Bounds of every Text_Element inside the Text_Element's own layout rect, within Overlap_Tolerance, across the Audit_Scope.
2. THE Game_UI SHALL keep the Text_Bounds of every Text_Element, grown by Edge_Clearance on each side, inside the Visible_Shape of the Text_Element's Container, across the Audit_Scope.
3. THE Game_UI SHALL render every non-whitespace character of the string of each Text_Element, across the Audit_Scope.

### Requirement 4: Keep texts, panels and images from colliding

**User Story:** As a player, I want texts, panels, cards and images not to overlap each other, so that each piece of information can be read on its own.

#### Acceptance Criteria

1. THE Game_UI SHALL draw no other element, including another Text_Element, over any part of a Text_Element's Text_Bounds, across the Audit_Scope.
2. THE Game_UI SHALL draw, directly beneath every part of a Text_Element's Text_Bounds, only elements of the Text_Element's Container_Chain, background pieces of Composite_Elements in that Container_Chain, and Decor_Elements, across the Audit_Scope.
3. THE Game_UI SHALL keep the Visible_Shapes of any two non-Decor elements from intersecting by more than Overlap_Tolerance, except where one element belongs to the other element's Container_Chain or both elements are pieces of the same Composite_Element, across the Audit_Scope.

### Requirement 5: Scroll areas

**User Story:** As a player, I want long lists to scroll inside their own area, so that every item can be read and nothing shows outside the list.

#### Acceptance Criteria

1. THE Game_UI SHALL draw the content of each scroll area (Loja list, Elenco grid, Escalação bench, Salão stock) only inside the bounding rectangle of that scroll area's Visible_Shape, across the Audit_Scope.
2. THE Game_UI SHALL provide, for every item in a scroll area, a scroll position at which the item's Visible_Shape lies inside the scroll area's Visible_Shape inset by Edge_Clearance.
3. WHILE a scroll area is at the scroll position with which the Screen opens, THE Game_UI SHALL show the first item of that scroll area inside the scroll area's Visible_Shape inset by Edge_Clearance.

### Requirement 6: Controls are visible and clickable

**User Story:** As a mouse player, I want every button fully visible and clickable over its whole face, so that a click always does what the button says.

#### Acceptance Criteria

1. THE Game_UI SHALL give every Interactive_Control of the Active_Layer a Hit_Area of at least Minimum_Hit_Size, across the Audit_Scope.
2. WHEN the player clicks a point inside the Visible_Shape of an Interactive_Control of the Active_Layer, THE Game_UI SHALL deliver the click to that Interactive_Control or to an Interactive_Control nested inside that Interactive_Control.
3. THE Game_UI SHALL keep the Hit_Areas of any two buttons, planks, tabs, price buttons, card buttons, stock cards or Dialog buttons of the Active_Layer from intersecting by more than Overlap_Tolerance, across the Audit_Scope.

### Requirement 7: Legible text

**User Story:** As a player at 960×600, I want every text no smaller and no harder to read than today, so that the layout fixes cost no legibility.

Since 02/11/2026 these criteria are verified by review of the Change_Set and by the captures, not by the Layout_Check (see "Owner decisions (02/11/2026): trimmed Layout_Check").

#### Acceptance Criteria

1. THE Change_Set SHALL lower no `fontSize` value, no size constant of `Typeface.cs` and no `localScale` of an object that holds a Text_Element below its Baseline value, SHALL enable best fit on no Text_Element that lacks it in the Baseline, and SHALL keep the settings of the `CanvasScaler` of `UiRoot` unchanged.
2. WHERE the Defect_Log records D-LJ-4 as "reproduzido", THE Game_UI SHALL draw Loja row prices in `Typeface.Body` bold, descriptions in `Palette.Text` and the "No estoque" count in `Logo.BoardDeep`, and SHALL keep every other Loja row colour as in the Baseline.
3. THE Change_Set SHALL keep the colour and the opacity of every Text_Element outside the Loja rows as in the Baseline.

### Requirement 8: Loja (UI-01 priority 1)

**User Story:** As a player shopping, I want each Loja row to show the item, its description, my stock and a working price button without overlap on all three tabs, so that I can compare and buy items.

#### Acceptance Criteria

1. WHILE the Loja shows the Móveis, Roupas or Presentes tab, THE Game_UI SHALL show the icon (Móveis only), name, description, "No estoque" count and price button of every row inside that row's Visible_Shape, with no two of these elements intersecting.
2. THE Game_UI SHALL label each row's price button with `Strings.Money` of the price of that row's item.
3. WHILE the player can afford a row's item, THE Game_UI SHALL show that row's price button enabled, with the Baseline affordable tint.
4. WHILE the player cannot afford a row's item, THE Game_UI SHALL show that row's price button disabled, with the Baseline unaffordable tint.
5. WHEN the player clicks an enabled price button, THE Game_UI SHALL perform the Baseline purchase action: `Shop.Buy` for furniture and clothes, and the "Para quem?" Dialog for gifts.
6. WHEN the player selects a tab, THE Game_UI SHALL list exactly the items of that tab for the current café level, in the Baseline order.
7. THE Game_UI SHALL show the "LOJA" title, the balance at Large_Values, the three tabs and "Voltar" with no two of these elements intersecting each other or the list, in Audit_States LJ-1 to LJ-4.

### Requirement 9: Balanço and Falência (UI-01 priority 2)

**User Story:** As a player at the end of a day, I want the Balanço to show the verdict, both ledger columns, every event counter and the final cash inside the board, so that I can understand how the day went.

#### Acceptance Criteria

1. THE Game_UI SHALL show the verdict, the subtitle, the "Entrou" and "Saiu" headings with the rows and totals of each column, every tally chip, the profit or loss line and the "Caixa depois" line inside the Visible_Shape of the Balanço plaque, in Audit_States BL-1 to BL-4.
2. WHILE the Balanço shows 13 tally chips (Audit_State BL-3), THE Game_UI SHALL show every chip, with the chip's number and caption, inside the Balanço plaque and with no two chips intersecting.
3. THE Game_UI SHALL show the label and the amount of every ledger row inside that row's Visible_Shape, in Audit_States BL-1 to BL-4.
4. THE Game_UI SHALL display each ledger row amount as `Strings.Money` of the matching DayLedger field with the Baseline "−" prefix on outgoing rows, the profit or loss as `Strings.Signed` of DayLedger.Profit, "Caixa depois" as `Strings.Money` of DayLedger.BalanceAfter, and each tally as the matching DayLedger count.
5. WHILE the ledger carries a shift failure reason (Audit_State BL-4), THE Game_UI SHALL show the complete failure reason as the subtitle inside the Balanço plaque.
6. WHILE the Falência Screen is shown (Audit_State FL-1), THE Game_UI SHALL show the "FALÊNCIA" title, the body text and the "Abrir outra empresa" button inside the Visible_Shape of the Falência plaque.

### Requirement 10: Elenco (UI-01 priority 3)

**User Story:** As a player managing the roster, I want the card grid and the detail panel side by side without overlap, and every fact, attribute, trait and action inside the detail panel, so that I can judge each maid.

#### Acceptance Criteria

1. THE Game_UI SHALL keep the Visible_Shapes of the Elenco grid and of the detail panel from intersecting, in Audit_States EL-1 to EL-4.
2. WHILE the roster holds 8 maids (Audit_State EL-1), THE Game_UI SHALL show each grid card's name, archetype, four stat numbers with captions and "Salário" line inside that card's face.
3. WHILE the detail panel shows the maid of Audit_State EL-2, THE Game_UI SHALL show the small card, the five facts (Raridade, Posição natural, Salário, Nível, Etiquetas), the four attribute rows with bar and number, the "Traços" heading with the three trait lines, the flavour line, the signature line and the "Dispensar (+N)" button inside the detail panel's Visible_Shape.
4. WHILE the detail panel shows a maid without traits (Audit_State EL-3), THE Game_UI SHALL show "Nenhum traço" and the flavour line inside the detail panel's Visible_Shape.
5. THE Game_UI SHALL show the complete Elenco header (roster count, roster cap and fixed cost per day at Large_Values) inside the header's own layout rect.
6. WHEN the player clicks a grid card, THE Game_UI SHALL show that card's maid in the detail panel.

### Requirement 11: Empresa

**User Story:** As a player between days, I want the hub's sign, figures, today strip and buttons complete and separate, so that I can read the state of the business and pick the next action.

#### Acceptance Criteria

1. THE Game_UI SHALL show the monogram and the complete "NEKO MAID INC." wordmark with no element drawn over the monogram or the wordmark, in Audit_States EM-1 to EM-4.
2. THE Game_UI SHALL show the four figures (Dia, Caixa, Fichas, Custo fixo por dia) with captions and Large_Values values, the level bar, the level line and the warning or not-ready line inside the Visible_Shape of the stats plaque.
3. THE Game_UI SHALL show the label of every Empresa plank (Escalação, Elenco, Caixas, Loja, Salão, Abrir o dia, Voltar) inside that plank's face.
4. WHILE the lineup holds 8 maids (Audit_State EM-2), THE Game_UI SHALL show all 8 cards of the today strip with no card intersecting the chemistry line, another card or an Empresa plank.
5. WHEN the player clicks any point of the "Voltar" plank's face, THE Game_UI SHALL return to the Menu.

### Requirement 12: Escalação

**User Story:** As a player building the lineup, I want every position, slot, card, the bench and the tools readable and separate even with every position unlocked, so that I can see the whole side at a glance.

#### Acceptance Criteria

1. WHILE the lineup has 10 slots (Audit_State ES-2), THE Game_UI SHALL show every position header and every slot card with no two of these elements intersecting and none of these elements intersecting the chemistry panel.
2. THE Game_UI SHALL show the name, archetype, stat numbers and stat captions of every slot card and bench card inside that card's face, in Audit_States ES-1 to ES-3.
3. WHILE a position is empty and maids are on the bench, THE Game_UI SHALL show the empty slot card reading "vazio" and the same slot and bench assignment that the Baseline lineup rules produce for the same session data.
4. THE Game_UI SHALL place the "Banco" heading above the bench area and within the horizontal extent of the bench area's Visible_Shape.
5. THE Game_UI SHALL show the chemistry score, bar and verdict inside the Visible_Shape of the chemistry panel.
6. THE Game_UI SHALL show the bench area, the not-ready warning line, "Escalar sozinho" and "Limpar" with no two of these elements intersecting, in Audit_States ES-1 to ES-3.

### Requirement 13: Caixas and the card reveal

**User Story:** As a player opening boxes, I want the shelf and the reveal to show every box, price, card and note without overlap, so that I can see what I paid and what I got.

#### Acceptance Criteria

1. THE Game_UI SHALL show each box's crate, name, odds text and "Abrir N" button with no two of these elements intersecting, in Audit_States CX-1 and CX-2.
2. THE Game_UI SHALL show the fichas balance at Large_Values with no intersection with the "CAIXAS" title or the "Voltar" plank.
3. WHILE the Overlay shows 1 or 3 cards (Audit_States CX-3 and CX-4), THE Game_UI SHALL show the "VEIO NA CAIXA" heading, every card, every duplicate note and the "Guardar" button inside the Viewport with no two of these elements intersecting.

### Requirement 14: Salão

**User Story:** As a player arranging the room, I want the title bar, the metrics, the stock and the hint readable, and no interface control blocking a tile, so that I can read the room's numbers and place furniture on any tile.

#### Acceptance Criteria

1. THE Game_UI SHALL show the "Arrumar o salão" title and the complete metrics line (Lugares, Ambiente, Caminhada, Tema) inside the Visible_Shape of the top bar, with two-digit metrics and the longest theme name.
2. THE Game_UI SHALL show each stock card's icon and complete "<name> ×N" label inside that stock card's Visible_Shape, for the Longest_Values placeable name with a two-digit count.
3. WHILE the stock is empty (Audit_State SL-1), THE Game_UI SHALL show the empty-stock text inside the Visible_Shape of the stock area.
4. THE Game_UI SHALL show the placement hint, including the picked item's name, the rotate hint and the blocked message, complete inside the hint's own layout rect.
5. WHILE the Salão is shown, THE Game_UI SHALL keep the Hit_Area of every Game_UI element outside the Room_Grid_Area, for each room size of café levels 1 to 10 (Audit_State SL-4).

### Requirement 15: O dia

**User Story:** As a player watching the day, I want the clock, the till, the counts, the speed controls and the worker chips readable, so that I can follow the day.

#### Acceptance Criteria

1. THE Game_UI SHALL show the clock, "Caixa do dia" at Large_Values, the tally line at Large_Values counts and the 1×, 2×, 4× and » buttons inside the Visible_Shape of the top plaque, with no two of these elements intersecting.
2. WHILE 8 maids work the day (Audit_State OD-2), THE Game_UI SHALL show all 8 worker chips inside the Viewport, each chip with the maid's name, position and mood bar inside that chip's Visible_Shape.
3. WHILE "Fechando!" is shown (Audit_State OD-3), THE Game_UI SHALL show the complete banner inside the banner's own layout rect and inside the Viewport.
4. THE Game_UI SHALL show the hint "Você não pode interferir. O dia é da escalação." with no intersection with any worker chip.

### Requirement 16: Menu

**User Story:** As a player opening the game, I want the sign, the buttons, my records and the credits laid out without overlap, so that the first screen reads clearly.

#### Acceptance Criteria

1. THE Game_UI SHALL start a new line of the records text at each of the two points where `MenuScreen.SetSession` separates record groups, in Audit_State MN-2.
2. THE Game_UI SHALL show the complete credits text inside the credits label's own layout rect, with no intersection with the Menu sign.
3. THE Game_UI SHALL show the sign, the tagline, the three parade maids and the position plate under each maid, the play button, "Nova empresa" and the records block with no two of these elements intersecting, in Audit_States MN-1 and MN-2.

### Requirement 17: Dialogs

**User Story:** As a player, I want every message box to hold its title, text and buttons inside the box, so that I can read the message and answer it.

#### Acceptance Criteria

1. THE Game_UI SHALL show each Dialog's title, body and buttons inside the Visible_Shape of the dialog box, in Audit_States MN-3, ES-4, EL-5, LJ-5, LJ-6, DG-1 and DG-2.
2. WHILE a Dialog shows two buttons, THE Game_UI SHALL show the two buttons with no intersection between the two buttons and each button's label inside that button's face.
3. WHILE a Dialog is open, THE Game_UI SHALL block clicks on the Screen beneath the Dialog's shade, as in the Baseline.

### Requirement 18: Preserve behaviour and data

**User Story:** As the owner, I want UI-01 to change presentation only, so that navigation, clicks, scrolling, session state, saves and economy behave exactly as before.

On 29/10/2026, as part of the design review, the owner changed the scroll direction of the Salão stock from horizontal to vertical (design decision 2). Criterion 3 reflects that change; every other scroll area keeps its Baseline direction.

#### Acceptance Criteria

1. WHEN the player clicks a navigation control, THE Game_UI SHALL open the same destination as the Baseline: Menu play button to Empresa (Falência when bankrupt); "Nova empresa" to the "Nova empresa" confirmation Dialog; Empresa planks to Escalação, Elenco, Caixas, Loja and Salão; "Abrir o dia" to O dia (the not-ready Dialog when the lineup is not ready); Empresa "Voltar" to the Menu; "Voltar" on Escalação, Elenco, Caixas, Loja and Salão to Empresa; » to Balanço (Falência when bankrupt); "Voltar à empresa" to Empresa (Falência when bankrupt); "Abrir outra empresa" to a new game.
2. WHEN the player clicks any other Interactive_Control, THE Game_UI SHALL perform the Baseline action: Loja tabs and price buttons; Elenco card selection and dismissal; Escalação slot cards, bench cards, "Escalar sozinho" and "Limpar"; Caixas "Abrir N" and "Guardar"; Salão stock selection, placement, removal and rotation; O dia speed buttons; Dialog buttons.
3. THE Game_UI SHALL scroll the Loja list, the Elenco grid and the Salão stock vertically and the Escalação bench horizontally, each with elastic movement and a scroll sensitivity of 28.
4. THE Game_UI SHALL display every value read from GameSession, Profile, Shift and DayLedger with the Baseline formatting: `Strings.Money`, `Strings.Signed`, integer counts, and the Baseline "0.0" and "0.00" decimal formats.
5. THE Change_Set SHALL keep every entry of `Strings.cs` unchanged.
6. THE Change_Set SHALL keep every file under `Assets/_Game/Scripts/Sim/`, `Assets/_Game/Scripts/Game/Core/` and `Assets/_Game/Scripts/Game/Input/` unchanged.
7. THE Change_Set SHALL keep every game rule (economy, lineup, chemistry, progression, shop and box logic) in `Neko.Sim`, with Game_UI code reading results from `Neko.Sim` and GameSession.

### Requirement 19: Stay within UI-01 scope

**User Story:** As the owner, I want the change limited to layout and legibility, so that palette, art, cards, world rendering and protected files stay as they are.

#### Acceptance Criteria

1. THE Change_Set SHALL keep every colour value defined in `Palette.cs`, `Logo.cs` and `Scenery.cs` unchanged.
2. THE Game_UI SHALL render all text with `Typeface.Font`.
3. THE Game_UI SHALL keep the MaidCard composition: rarity-tinted frame, face, archetype wash, portrait, name, archetype, position tab, four stat pips with captions, the optional salary line and the trait stars.
4. THE Change_Set SHALL keep the pixels of every World_Art sprite identical to the Baseline.
5. THE Change_Set SHALL add no Screen, Overlay, animation, tutorial, sound, imported asset or gameplay feature.
6. THE Change_Set SHALL keep every Protected_Path unchanged.
7. WHERE the Change_Set adds an automated test, THE Change_Set SHALL add the test as a new file together with the Unity `.meta` files generated for the new file and for any new folder.
8. WHERE the Change_Set modifies a Shared_Component, THE Defect_Log SHALL name the "reproduzido" entries that the modification resolves and state why a change confined to the affected Screens does not resolve those entries.
9. WHERE the Change_Set modifies a Shared_Component, THE Layout_Check SHALL pass in every Audit_State of every Screen, Overlay and Dialog that uses that Shared_Component.

### Requirement 20: Automated layout check

**User Story:** As the owner, I want an automated check that measures these rules in every Audit_State at both resolutions, so that a layout regression fails a test instead of reaching players.

The owner authorized this new test infrastructure on 29/10/2026, as `docs/ROADMAP.md` restriction 7 requires (Owner decision 7), and trimmed it to its core rules on 02/11/2026 (see "Owner decisions (02/11/2026): trimmed Layout_Check").

#### Acceptance Criteria

1. THE Layout_Check SHALL put the game in every Audit_State and evaluate Requirements 2 to 6 and 14.5, together with the containment, separation and click criteria of Requirements 8 to 17, at 960×600, at 1280×720 and again at 960×600 without showing the Screen again.
2. THE Layout_Check SHALL assert, for each measurement, that the canvas size and scale factor match the values the game produces in a real window of the same Reference_Resolution (with the Baseline scaler, 1152×720 Canvas_Units at 960×600 and 1280×720 Canvas_Units at 1280×720).
3. WHEN a rule fails, THE Layout_Check SHALL report the Screen, the Audit_State, the Reference_Resolution, the hierarchy path of each element involved, the rule and the measured values.
4. WHEN the Layout_Check runs against the Baseline Game_UI code, THE Layout_Check SHALL fail for each Defect_Log entry recorded as "reproduzido" whose rule the Layout_Check evaluates.
5. THE Layout_Check SHALL run in batch mode through `tools/run-tests.sh PlayMode` and build the session data of every Audit_State from fixed seeds and explicit profile values.
6. WHERE the environment variable `NEKO_CAPTURE_DIR` is set, THE Layout_Check SHALL write one capture per Audit_State and Reference_Resolution to that directory.
7. WHEN the Layout_Check finishes, THE Layout_Check SHALL restore the Viewport size that was active when the Layout_Check started.

### Requirement 21: Evidence and delivery

**User Story:** As the owner, I want a small PR with comparable before/after captures and truthful test results, so that I can approve the legibility fix without opening the editor.

#### Acceptance Criteria

1. THE Evidence_Package SHALL include before and after captures at 960×600 and at 1280×720 of Audit_States LJ-1, LJ-2, LJ-3, EL-1, EL-2, BL-1 and BL-3, and of at least one Audit_State of every other Screen, of the Overlay and of a Dialog.
2. THE Evidence_Package SHALL include, for each Defect_Log entry recorded as "reproduzido", one before capture and one after capture of an Audit_State that shows the entry, at one Reference_Resolution at which the Defect_Log records the entry as "reproduzido", with each capture included once even when the capture serves several entries or Requirement 21.1 also requires the capture.
3. THE Evidence_Package SHALL label each capture with the Screen, the Audit_State, the Reference_Resolution, "antes" or "depois", and the source (PlayMode render, Editor Game view or WebGL build).
4. WHEN the Audit_States are visited at both Reference_Resolutions, THE Game_UI SHALL log zero errors and zero exceptions to the Console.
5. THE Evidence_Package SHALL report, for `tools/run-tests.sh EditMode`, `tools/run-tests.sh PlayMode`, the Layout_Check, the Console check and any WebGL build check, the command run and the result as passed, failed or not run, with the reason for each result reported as not run.
6. THE Change_Set SHALL keep every EditMode and PlayMode test that passes on the Baseline passing.
7. THE Evidence_Package SHALL state that visual approval covers layout and legibility only, not final art.
8. THE Change_Set SHALL exclude the planning documents under `.kiro/specs/ui-01-layout/`.
9. THE Change_Set SHALL be delivered in one pull request that requests auto-merge only after the owner has reviewed the after captures.
10. THE Change_Set SHALL add to the Evidence_Folder the Defect_Log as `DEFECT_LOG.md` and, as PNG files, exactly the captures that Requirements 21.1 and 21.2 require.
11. THE Defect_Log SHALL link each capture in the Evidence_Folder to the Defect_Log entries that the capture documents, and SHALL list each capture that Requirement 21.1 requires without a matching entry under the Audit_State that the capture shows.
12. THE Change_Set SHALL set the status of the UI-01 row of `docs/ROADMAP.md` to REVIEW, as the roadmap's status legend defines it, and SHALL add to that row links to the UI-01 pull request and to the Defect_Log.
13. THE Defect_Log SHALL state the exact commands, and the code revision on which each command runs, that regenerate with the Layout_Check and `NEKO_CAPTURE_DIR` the before and after captures of every Audit_State at both Reference_Resolutions.
14. THE Evidence_Package SHALL state, for each acceptance criterion that the Layout_Check does not evaluate (Requirements 7, 8.2 to 8.4, 8.6, 9.4, 12.3, 18.1 to 18.4 and 19.2 to 19.4), how the criterion was verified (review of the Change_Set, a named existing test, or the captures) and the result.
