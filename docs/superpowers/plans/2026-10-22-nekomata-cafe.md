# Nekomata Café Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** A complete, playable base version of a 2D maid-café management game — shifts, nine
rule-bearing maid archetypes, four scaling axes, a grid café editor, procedural art,
synthesised audio, a save, and a WebGL build.

**Architecture:** Two assemblies. `Neko.Sim` is pure C# with no `UnityEngine` reference and
holds the entire simulation, so a full day runs headless and deterministically in a unit
test. `Neko.Game` is the Unity layer: it builds the only scene in code at runtime, generates
every sprite into a `Texture2D`, and drives the sim. Nothing is imported — no textures, no
prefabs, no hand-authored scene YAML.

**Tech Stack:** Unity 6000.5.9f1, 2D URP (`Renderer2D`), C#, Unity Input System, Unity Test
Framework (NUnit), WebGL. Audio `.wav` produced by the workspace's `audio` MCP server.

**Spec:** `docs/superpowers/specs/2026-10-22-nekomata-cafe-design.md`

## Global Constraints

- Unity editor version is **6000.5.9f1**; `ProjectSettings/ProjectVersion.txt` must say so.
- Render pipeline is **2D URP** with a `Renderer2D` renderer asset; build target **WebGL**.
- **Zero imported art or audio assets.** Every sprite is generated at runtime; every sound
  is synthesised and committed as `.wav` under `Assets/_Game/Audio/`.
- **One scene**, `Assets/_Game/Scenes/Boot.unity`, containing one `GameRoot` object and
  nothing else. All other objects are created in code.
- `Neko.Sim.asmdef` must **not** reference `UnityEngine`. A test asserts this.
- Input via **Unity Input System** only; no `UnityEngine.Input`.
- Code, identifiers, comments and commit messages in **English**. All player-facing strings
  and `docs/GDD.md` in **Portuguese (pt-BR)**.
- Money never goes negative; no operation may leave the player unable to progress.
- Determinism: every random draw goes through `Rng`, seeded from the profile. No
  `UnityEngine.Random`, no `System.Random` outside `Rng`.
- Frame budget: 60 fps at 1280×720.
- Every sound or frame approved during development is copied to
  `../visual_assets/` (`audio/<date>_nekomata_<topic>/`, `frames/<date>_nekomata_<topic>/`)
  with a README naming what it is.

## Review Focus

Input classes the spec implies but no feature task's tests would otherwise exercise. Each
line names the task that now owns its test.

1. **A shift with zero maids rostered** — the manager alone must still be able to serve a
   customer end to end; nothing may divide by an empty roster. *(Task 6)*
2. **A save written by an older schema version, and a save whose checksum fails** — the
   first migrates, the second yields a fresh profile and a message, neither throws.
   *(Task 9)*
3. **A café grid with no table, no station, or a station with no matching recipe** — the
   shift must start, report that it cannot serve, and end as a slow day rather than
   deadlocking on an unclaimable task. *(Task 6)*
4. **Money and quota at the boundaries** — a purchase costing exactly the balance
   succeeds, one cent more is refused, and a missed quota does not advance the curve.
   *(Task 8)*
5. **A maid whose Mood reaches zero mid-task** — she finishes or releases the task she
   claimed and goes to the break room; the task must not stay claimed forever. *(Task 6)*

---

## Task 1: Project conversion and test harness

Turn the copied mobile starter into this game's project: right editor version, 2D URP,
WebGL, identity, the two assemblies and the two test assemblies.

**Files:**
- Modify: `ProjectSettings/ProjectVersion.txt`, `ProjectSettings/ProjectSettings.asset`,
  `ProjectSettings/GraphicsSettings.asset`, `ProjectSettings/EditorBuildSettings.asset`,
  `Packages/manifest.json`
- Create: `Assets/_Game/Rendering/NekoURP.asset`, `Assets/_Game/Rendering/NekoRenderer2D.asset`
- Create: `Assets/_Game/Scenes/Boot.unity`
- Create: `Assets/_Game/Scripts/Sim/Neko.Sim.asmdef`,
  `Assets/_Game/Scripts/Game/Neko.Game.asmdef`
- Create: `Assets/_Game/Tests/EditMode/Neko.Tests.EditMode.asmdef`,
  `Assets/_Game/Tests/PlayMode/Neko.Tests.PlayMode.asmdef`
- Create: `Assets/_Game/Tests/EditMode/AssemblyBoundaryTests.cs`
- Create: `tools/run-tests.sh`, `tools/build-webgl.sh`
- Delete: starter docs that describe the template rather than this game
  (`docs/TEMPLATE_CONTRACT.md`, `docs/NEW_GAME_CHECKLIST.md`, `docs/CODEX_WORKFLOW.md`)

**Interfaces:**
- Produces: assembly names `Neko.Sim`, `Neko.Game`, `Neko.Tests.EditMode`,
  `Neko.Tests.PlayMode`. `Neko.Game` references `Neko.Sim`. Both test assemblies reference
  both and are `"testAssemblies": true` with `nunit.framework.dll` in `precompiledReferences`
  and `"UNITY_INCLUDE_TESTS"` in `defineConstraints`.

- [ ] **Step 1: Write the failing test**

```csharp
// Assets/_Game/Tests/EditMode/AssemblyBoundaryTests.cs
[Test]
public void SimAssemblyDoesNotReferenceUnity()
{
    var sim = AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "Neko.Sim");
    var unityRefs = sim.GetReferencedAssemblies().Where(r => r.Name.StartsWith("UnityEngine"));
    Assert.IsEmpty(unityRefs, "Neko.Sim must stay testable without Unity: " + string.Join(", ", unityRefs.Select(r => r.Name)));
}

[Test]
public void EditorVersionIsPinned()
{
    StringAssert.Contains("6000.5.9f1", File.ReadAllText("ProjectSettings/ProjectVersion.txt"));
}
```

- [ ] **Step 2: Run it and watch it fail**

Run: `node ../agent_creater/tools/mcp/scripts/call.mjs --timeout 900 unity unity_run_tests '{"projectPath":"<abs repo path>","platform":"EditMode"}'`
Expected: the run fails to compile, because `Neko.Sim` does not exist yet.

- [ ] **Step 3: Set the project version, identity and build target**

`ProjectVersion.txt` → `6000.5.9f1` with revision `75cfa6d93e7f`. In
`ProjectSettings.asset`: `companyName: SlipperyZlope Games`, `productName: Nekomata Café`,
`applicationIdentifier` for WebGL `com.slipperyzlope.nekomatacafe`, default screen
`1280×720`, `activeInputHandler: 1` (Input System only). `EditorBuildSettings.asset` lists
only `Assets/_Game/Scenes/Boot.unity`.

- [ ] **Step 4: Create the 2D URP assets and point GraphicsSettings at them**

`NekoRenderer2D.asset` is a `Renderer2DData`; `NekoURP.asset` is a
`UniversalRenderPipelineAsset` whose renderer list holds it. Set both the default and the
single quality-level pipeline asset in `GraphicsSettings.asset` / `QualitySettings.asset`.
Write them by copying the shape of the starter's existing URP assets rather than by hand —
validate by opening the project once in batch mode and reading `unity_project_settings`.

- [ ] **Step 5: Create the four assembly definitions and the Boot scene**

`Boot.unity` holds one GameObject named `GameRoot` with no components yet.

- [ ] **Step 6: Write `tools/run-tests.sh` and `tools/build-webgl.sh`**

`run-tests.sh <EditMode|PlayMode>` shells out to the `unity` MCP `call.mjs` with this
project's absolute path. `build-webgl.sh` runs the editor in batch mode calling
`Neko.Game.Editor.WebGLBuilder.Build` (added in Task 20) and writes to `Build/WebGL/`.

- [ ] **Step 7: Run the tests and watch them pass**

Run: `tools/run-tests.sh EditMode`
Expected: 2 passed, 0 failed.

- [ ] **Step 8: Commit**

```bash
git add -A
git commit -m "chore: convert the mobile starter into the Nekomata Café project"
```

---

## Task 2: Determinism primitives

The seeded generator and the curve helpers every later task draws from. Everything random
in this game goes through here so a shift can be replayed exactly.

**Files:**
- Create: `Assets/_Game/Scripts/Sim/Core/Rng.cs`, `Assets/_Game/Scripts/Sim/Core/Curves.cs`
- Test: `Assets/_Game/Tests/EditMode/Sim/RngTests.cs`, `.../CurvesTests.cs`

**Interfaces:**
- Produces:
  - `sealed class Rng { Rng(uint seed); uint State { get; } float NextFloat(); int NextInt(int maxExclusive); bool Chance(float probability); T Pick<T>(IReadOnlyList<T> items); T PickWeighted<T>(IReadOnlyList<T> items, Func<T, float> weight); Rng Fork(); }`
  - `static class Curves { float Power(float scale, float exponent, int step); float Lerp(float a, float b, float t); float EaseOutBack(float t); float Clamp01(float v); float Remap(float v, float inA, float inB, float outA, float outB); }`

- [ ] **Step 1: Write the failing tests**

```csharp
[Test] public void SameSeedGivesSameSequence()            // two Rng(1234) produce identical 100-draw sequences
[Test] public void NextFloatStaysInRange()                // 10_000 draws, all in [0,1)
[Test] public void NextIntNeverReturnsMax()               // NextInt(3) over 10_000 draws yields only 0,1,2 and all three appear
[Test] public void PickWeightedRespectsWeights()          // weights 1:9 over 20_000 draws lands within 2% of 0.10/0.90
[Test] public void PickWeightedWithAllZeroWeightsIsUniform() // never throws, never returns default
[Test] public void ForkDivergesButIsReproducible()        // forked streams differ from the parent and repeat for the same seed
[Test] public void PowerMatchesTheQuotaCurve()            // Curves.Power(120f, 1.35f, 1) == 120f; step 10 == 120*10^1.35 within 0.01
[Test] public void Clamp01AndRemapHandleReversedRanges()
```

- [ ] **Step 2: Run them and watch them fail**

Run: `tools/run-tests.sh EditMode`
Expected: FAIL — `Rng`/`Curves` do not exist.

- [ ] **Step 3: Implement `Rng` and `Curves`**

`Rng` is xorshift32 with the state never allowed to be 0 (seed 0 maps to `0x9E3779B9`).
`PickWeighted` falls back to uniform when the weights sum to 0. `Fork` returns a new `Rng`
seeded from the parent's next draw.

- [ ] **Step 4: Run them and watch them pass**

Run: `tools/run-tests.sh EditMode` — Expected: all green.

- [ ] **Step 5: Commit** — `feat(sim): add the seeded generator and curve helpers`

---

## Task 3: Maids — stats, archetypes and derived speeds

The nine archetypes as data plus the function that turns a maid and a moment into the
speeds the shift actually uses.

**Files:**
- Create: `Assets/_Game/Scripts/Sim/Maids/Stats.cs`, `Archetype.cs`, `ArchetypeProfile.cs`,
  `ArchetypeTable.cs`, `Maid.cs`, `MaidDerived.cs`
- Test: `Assets/_Game/Tests/EditMode/Sim/ArchetypeTests.cs`, `.../MaidDerivedTests.cs`

**Interfaces:**
- Consumes: `Rng`, `Curves` (Task 2).
- Produces:
  - `readonly struct Stats { int Serve, Cook, Charm, Stamina; Stats Plus(Stats other); static Stats Zero; }`
  - `enum Archetype { Genki, Kuudere, Tsundere, Dojikko, Ojousama, Oneesan, Idol, Gourmand, Mukuchi }`
  - `sealed class ArchetypeProfile { Archetype Kind; string DisplayName; Stats StartingStats; Stats GrowthWeights; float MoveMultiplier; float MoodDrainMultiplier; float BaseFumbleChance; bool WillBusTables; float CrowdedSpeedBonus; float IdleTipPenalty; float NeighbourSpeedAura; float VipWeightMultiplier; string AccentHex; string SilhouetteTell; }`
  - `static class ArchetypeTable { ArchetypeProfile For(Archetype kind); IReadOnlyList<ArchetypeProfile> All; }`
  - `sealed class Maid { int Id; string Name; Archetype Archetype; int Level; int Xp; Stats Base; int Bond; float Mood; List<SkillId> Skills; Equipment[] Gear; Stats Total { get; } }`
  - `readonly struct MaidDerived { float MoveSpeed, ServeSpeed, CookSpeed, TipMultiplier, FumbleChance; }`
  - `static class MaidMath { MaidDerived Derive(Maid maid, ShiftSnapshot moment); Stats GrowthFor(Maid maid, int levelsGained); int XpForLevel(int level); }`
  - `readonly struct ShiftSnapshot { float TableOccupancy; bool AtStation; bool Idle; int NeighbouringMaids; }`

- [ ] **Step 1: Write the failing tests**

```csharp
[Test] public void AllNineArchetypesAreInTheTable()
[Test] public void GenkiMovesThirtyFivePercentFaster()        // MoveMultiplier == 1.35f
[Test] public void KuudereNeverFumbles()                      // Derive(...).FumbleChance == 0f even with Cook 1
[Test] public void TsundereSpeedsUpOnlyWhenCrowded()          // occupancy 0.59 → 1.0x, 0.60 → 1.30x
[Test] public void TsundereLosesTipsWhileIdle()
[Test] public void OjousamaRefusesToBusTables()               // WillBusTables == false
[Test] public void MukuchiCharmCountsDoubleAtAStation()       // TipMultiplier at a station vs on the floor
[Test] public void OneesanAuraScalesWithNeighbours()
[Test] public void XpCurveIsMonotonicToTwenty()               // XpForLevel strictly increasing, level 20 defined
[Test] public void GrowthFollowsTheArchetypeWeights()         // a Mukuchi gains more Cook than Serve over 10 levels
[Test] public void TotalStatsIncludeEquipmentAndClampAtOneHundred()
```

- [ ] **Step 2: Run them and watch them fail** — Expected: FAIL, types missing.

- [ ] **Step 3: Implement the archetype table and `MaidMath`**

The nine profiles carry exactly the rule values in spec §6.2. `Derive` applies, in order:
base stat → archetype multiplier → situational bonus (crowded, idle, station, neighbours)
→ skill modifiers (hook left for Task 7, applied through `SkillTable.Apply`).
`XpForLevel(level)` is `Curves.Power(40f, 1.6f, level)` rounded.

- [ ] **Step 4: Run them and watch them pass** — Expected: all green.

- [ ] **Step 5: Commit** — `feat(sim): add maid stats and the nine archetype rules`

---

## Task 4: The café grid and its derived metrics

**Files:**
- Create: `Assets/_Game/Scripts/Sim/Cafe/Placeable.cs`, `PlaceableCatalog.cs`,
  `CafeGrid.cs`, `CafeMetrics.cs`
- Test: `Assets/_Game/Tests/EditMode/Sim/CafeGridTests.cs`, `.../CafeMetricsTests.cs`

**Interfaces:**
- Produces:
  - `enum PlaceableKind { Table, Station, Decor, Fixture }`
  - `enum StationKind { Stove, Dessert, TeaBar, Bakery }`
  - `enum ThemeTag { None, Sakura, Gothic, Starry, Showa }`
  - `sealed class PlaceableDef { string Id; string DisplayName; PlaceableKind Kind; int Width, Height; int Seats; StationKind? Station; float Ambience; ThemeTag Theme; int Price; int UnlockCafeLevel; }`
  - `static class PlaceableCatalog { PlaceableDef ById(string id); IReadOnlyList<PlaceableDef> All; IReadOnlyList<PlaceableDef> UnlockedAt(int cafeLevel); }`
  - `sealed class PlacedItem { string DefId; int X, Y; int Rotation; }`
  - `sealed class CafeGrid { CafeGrid(int width, int height); int Width, Height; IReadOnlyList<PlacedItem> Items; bool CanPlace(PlaceableDef def, int x, int y); bool Place(PlaceableDef def, int x, int y); bool Remove(int x, int y); void Resize(int width, int height); CafeMetrics Metrics(); IReadOnlyList<(int x, int y)> SeatPositions(); }`
  - `readonly struct CafeMetrics { float Ambience; float Flow; int Seats; int StationCount; ThemeTag DominantTheme; bool ThemeBonusActive; float PatienceMultiplier; float TipMultiplier; }`

- [ ] **Step 1: Write the failing tests**

```csharp
[Test] public void PlacingOutsideTheGridIsRefused()
[Test] public void OverlappingPlacementIsRefused()
[Test] public void RemovingFreesTheFootprint()
[Test] public void ResizeKeepsItemsThatStillFitAndDropsTheRest()
[Test] public void SeatsCountEverySeatOfEveryTable()
[Test] public void AmbienceIsScaledByRoomSizeSoASmallRoomIsNotTrivallyCosy()
[Test] public void FlowFallsWhenAStationMovesNearTheTables()   // same items, two layouts, assert ordering
[Test] public void FlowIsDefinedWhenThereAreNoTables()         // returns 0, does not divide by zero
[Test] public void ThemeBonusNeedsSeventyPercentOfDecor()      // 2/3 off, 7/10 on
[Test] public void PatienceAndTipMultipliersRiseWithAmbienceAndAreClamped() // never below 1.0, never above 2.0
```

- [ ] **Step 2: Run them and watch them fail.**

- [ ] **Step 3: Implement `CafeGrid`, `CafeMetrics` and the catalog**

`Flow` is the mean Chebyshev distance over (door→each seat) and (each seat→nearest
station supporting any unlocked recipe), 0 when there are no seats.
`Ambience` is `sum(decor.Ambience) / sqrt(Width * Height)`.
`PatienceMultiplier = Clamp(1 + Ambience * 0.05, 1, 2)`; `TipMultiplier` the same with
`0.07`, and `×1.25` while the theme bonus is active.
The catalog holds the placeables of spec §7 with prices and unlock levels.

- [ ] **Step 4: Run them and watch them pass.**

- [ ] **Step 5: Commit** — `feat(sim): add the café grid and its derived metrics`

---

## Task 5: Recipes, customers and orders

**Files:**
- Create: `Assets/_Game/Scripts/Sim/Service/Recipe.cs`, `RecipeBook.cs`,
  `CustomerType.cs`, `CustomerProfile.cs`, `CustomerTable.cs`, `Customer.cs`, `Order.cs`
- Test: `Assets/_Game/Tests/EditMode/Sim/CustomerTests.cs`, `.../RecipeBookTests.cs`

**Interfaces:**
- Consumes: `Rng`, `StationKind`.
- Produces:
  - `sealed class Recipe { string Id; string DisplayName; float Seconds; int Price; StationKind Station; int UnlockCafeLevel; }`
  - `static class RecipeBook { Recipe ById(string id); IReadOnlyList<Recipe> UnlockedAt(int cafeLevel); IReadOnlyList<Recipe> ForStation(StationKind station, int cafeLevel); }`
  - `enum CustomerType { Regular, Student, Salaryman, Critic, Otaku, Cat, Vip }`
  - `sealed class CustomerProfile { CustomerType Kind; float PatienceSeconds; float SpendMultiplier; int PartySizeMin, PartySizeMax; float ArrivalWeight; int UnlockCafeLevel; Archetype? WantedArchetype; float AmbienceWhileSeated; }`
  - `static class CustomerTable { CustomerProfile For(CustomerType kind); CustomerType Roll(Rng rng, int cafeLevel, float vipWeightMultiplier); }`
  - `enum CustomerState { Queueing, Seating, Thinking, Ordering, Waiting, Eating, Paying, Leaving, Angry }`
  - `sealed class Customer { int Id; CustomerType Kind; int PartySize; CustomerState State; float Patience; float PatienceMax; int TableIndex; Order Order; float StateTimer; bool DrainsPatience { get; } }`
  - `sealed class Order { IReadOnlyList<Recipe> Dishes; int ServedCount; int BasePrice { get; } bool Complete { get; } }`

- [ ] **Step 1: Write the failing tests**

```csharp
[Test] public void RecipesUnlockWithCafeLevel()
[Test] public void EveryStationHasAtLeastOneRecipeAtCafeLevelOne()
[Test] public void CatOnlyEverOrdersMilk()
[Test] public void PatienceDrainsWhileWaitingAndNotWhileEating()   // DrainsPatience per state
[Test] public void VipHasTheLeastPatienceAndTheMostSpend()
[Test] public void OjousamaDoublesTheVipArrivalWeight()            // Roll with multiplier 2 over 20_000 draws
[Test] public void LockedCustomerTypesNeverRoll()
[Test] public void OrderIsCompleteOnlyWhenEveryDishIsServed()
```

- [ ] **Step 2: Run them and watch them fail.**

- [ ] **Step 3: Implement the service data and `Customer`**

The seven profiles carry exactly the modifiers of spec §9. `Cat` has
`PatienceSeconds = float.PositiveInfinity` and a one-recipe pool.

- [ ] **Step 4: Run them and watch them pass.**

- [ ] **Step 5: Commit** — `feat(sim): add recipes, customer types and orders`

---

## Task 6: The shift

The heart. A deterministic tick that moves customers through the loop, generates tasks,
lets workers claim and perform them, and reports a result.

**Files:**
- Create: `Assets/_Game/Scripts/Sim/Shift/ShiftTask.cs`, `Worker.cs`, `TaskBoard.cs`,
  `ShiftSetup.cs`, `Shift.cs`, `ShiftResult.cs`, `ShiftEvent.cs`
- Test: `Assets/_Game/Tests/EditMode/Sim/TaskBoardTests.cs`, `.../ShiftTests.cs`,
  `.../ShiftEdgeCaseTests.cs`

**Interfaces:**
- Consumes: everything from Tasks 2–5.
- Produces:
  - `enum TaskKind { Greet, TakeOrder, Cook, Deliver, Bus }`
  - `sealed class ShiftTask { int Id; TaskKind Kind; int TargetId; float Priority; int? ClaimedByWorker; bool Done; }`
  - `enum WorkerKind { Maid, Manager }`
  - `sealed class Worker { int Id; WorkerKind Kind; Maid Maid; float X, Y; ShiftTask Claimed; float Progress; bool OnBreak; float RushSeconds; }`
  - `sealed class TaskBoard { void Add(ShiftTask task); ShiftTask ClaimBest(Worker worker, CafeGrid grid); void Release(ShiftTask task); void Complete(ShiftTask task); IReadOnlyList<ShiftTask> Open { get; } }`
  - `sealed class ShiftSetup { int Day; int CafeLevel; CafeGrid Grid; IReadOnlyList<Maid> Roster; float DurationSeconds; float ArrivalsPerSecond; int Quota; uint Seed; }`
  - `sealed class Shift { Shift(ShiftSetup setup); void Tick(float dt); bool Finished { get; } float TimeRemaining { get; } int Earned { get; } IReadOnlyList<Customer> Customers { get; } IReadOnlyList<Worker> Workers { get; } Queue<ShiftEvent> Events { get; } void CommandManager(TaskKind kind, int targetId); void Rush(int workerId); ShiftResult Finish(); }`
  - `readonly struct ShiftEvent { ShiftEventKind Kind; int SubjectId; float X, Y; string Payload; }` with
    `enum ShiftEventKind { CustomerArrived, Seated, Ordered, TicketQueued, CookStarted, Fumbled, PlateReady, Served, Paid, LeftAngry, Bussed, MoodEmpty, Rushed, EncoreUsed, TurnedAway }`
  - `sealed class ShiftResult { int Earned; int Quota; bool QuotaMet; int Served; int Angry; int TurnedAway; int ReputationDelta; IReadOnlyDictionary<int, int> XpByMaid; bool SlowDay; string FailureReason; }`

- [ ] **Step 1: Write the failing tests for the task board**

```csharp
[Test] public void ClaimBestPrefersTheMostUrgentCustomerOverADirtyTable()
[Test] public void ATaskIsClaimedByExactlyOneWorker()
[Test] public void OjousamaIsNeverOfferedABusTask()
[Test] public void ReleasedTasksBecomeClaimableAgain()
[Test] public void ClaimBestBreaksTiesByDistance()
```

- [ ] **Step 2: Write the failing tests for a whole shift**

```csharp
[Test] public void ASingleCustomerIsServedEndToEnd()     // fixed seed, 1 arrival, tick to Finished → Served == 1, Earned > 0
[Test] public void TheSameSeedProducesTheSameResult()    // two shifts, identical ShiftResult and event sequence
[Test] public void AnImpatientCustomerLeavesAngryAndTheTableStillNeedsBussing()
[Test] public void BetterMaidsServeMoreInTheSameTime()   // level 1 roster vs level 15 roster, same seed, assert ordering
[Test] public void PaymentUsesAmbienceCharmAndRemainingPatience()
[Test] public void AFullQueueTurnsArrivalsAway()
[Test] public void RushSpeedsAMaidUpAndCostsMood()
```

- [ ] **Step 3: Write the failing edge-case tests (Review Focus 1, 3, 5)**

```csharp
[Test] public void AShiftWithNoMaidsIsPlayableByTheManagerAlone()
[Test] public void AShiftWithNoTablesEndsAsASlowDayWithAReason()
[Test] public void AShiftWithNoStationEndsAsASlowDayWithAReason()
[Test] public void AMaidWhoseMoodEmptiesMidTaskReleasesItAndGoesOnBreak()  // the task is re-claimable within one tick
[Test] public void NoTaskStaysClaimedAfterTheShiftEnds()
```

- [ ] **Step 4: Run them all and watch them fail.**

- [ ] **Step 5: Implement `TaskBoard`, `Worker` and `Shift`**

`Shift.Tick` runs, in order: arrivals → customer state timers and patience → task
generation from customer and table state → worker claim/walk/perform → completion effects
→ event drain. Workers walk at `derived.MoveSpeed` tiles/second toward the task's tile and
only make progress once within one tile. `Finish()` is idempotent and always releases every
claimed task. A setup that cannot possibly serve (no seats, or no station for any unlocked
recipe) finishes immediately with `SlowDay = true` and a `FailureReason`.

- [ ] **Step 6: Run them and watch them pass.**

- [ ] **Step 7: Commit** — `feat(sim): add the shift simulation, task board and workers`

---

## Task 7: Progression — levels, skills, equipment and bond

**Files:**
- Create: `Assets/_Game/Scripts/Sim/Progress/SkillId.cs`, `SkillDef.cs`, `SkillTable.cs`,
  `Equipment.cs`, `EquipmentCatalog.cs`, `BondTable.cs`, `MaidProgression.cs`
- Modify: `Assets/_Game/Scripts/Sim/Maids/MaidDerived.cs` (apply skill and bond modifiers)
- Test: `Assets/_Game/Tests/EditMode/Sim/SkillTests.cs`, `.../EquipmentTests.cs`,
  `.../BondTests.cs`, `.../MaidProgressionTests.cs`

**Interfaces:**
- Produces:
  - `enum SkillBranch { Floor, Kitchen, Charm }`
  - `sealed class SkillDef { SkillId Id; string DisplayName; SkillBranch Branch; int RequiredLevel; string Description; }`
  - `static class SkillTable { SkillDef ById(SkillId id); IReadOnlyList<SkillDef> Branch(SkillBranch branch); IReadOnlyList<SkillDef> AvailableTo(Maid maid); MaidDerived Apply(Maid maid, MaidDerived derived, ShiftSnapshot moment); int SlotsAt(int level); }`
  - `enum GearSlot { Outfit, Headwear, Accessory, Shoes }`
  - `sealed class Equipment { string Id; string DisplayName; GearSlot Slot; Stats Bonus; ThemeTag Theme; int Price; int UnlockCafeLevel; string LayerShapeId; string TintHex; }`
  - `static class EquipmentCatalog { Equipment ById(string id); IReadOnlyList<Equipment> ForSlot(GearSlot slot, int cafeLevel); Stats SetBonus(Maid maid); }`
  - `static class BondTable { int PointsForLevel(int bondLevel); string PerkFor(Archetype archetype, int bondLevel); string LineFor(Archetype archetype, int bondLevel); }`
  - `static class MaidProgression { bool GrantXp(Maid maid, int xp, out int levelsGained); bool EquipSkill(Maid maid, SkillId id, out string refusal); bool Equip(Maid maid, Equipment item); bool GiveGift(Maid maid, int points, out int bondLevelsGained); void RecoverBetweenDays(Maid maid, bool wasOnShift); }`

- [ ] **Step 1: Write the failing tests**

```csharp
[Test] public void SlotsOpenEveryFourthLevelToThreeAtTwenty()
[Test] public void EquippingAFourthSkillIsRefusedWithAReason()
[Test] public void ASkillAboveTheMaidsLevelIsRefused()
[Test] public void DoubleTrayHalvesDeliveryTrips()
[Test] public void MiseEnPlaceMakesTheFirstTicketInstantOncePerShift()
[Test] public void EquipmentChangesTotalStatsAndTheSpriteLayerId()
[Test] public void ThreePiecesOfATnemeGrantTheSetBonus()
[Test] public void BondFiveUnlocksASignaturePerkUniquePerArchetype()
[Test] public void GrantXpCanCrossSeveralLevelsAtOnce()
[Test] public void LevelIsCappedAtTwentyAndExcessXpIsNotLost()
[Test] public void MoodRecoversFasterOffShift()
```

- [ ] **Step 2: Run them and watch them fail.**

- [ ] **Step 3: Implement the four axes**

Eighteen skills (6 per branch) with the four named in spec §6.3 among them. Equipment
carries `LayerShapeId` and `TintHex` so Task 12 can draw it without a second table.
`SkillTable.Apply` is called from `MaidMath.Derive`, after situational bonuses.

- [ ] **Step 4: Run them and watch them pass.**

- [ ] **Step 5: Commit** — `feat(sim): add levels, skills, equipment and bond`

---

## Task 8: Economy — money, reputation, café level, shop and applicants

**Files:**
- Create: `Assets/_Game/Scripts/Sim/Economy/Wallet.cs`, `CafeLevelTable.cs`,
  `DayPlan.cs`, `Shop.cs`, `ApplicantBoard.cs`
- Test: `Assets/_Game/Tests/EditMode/Sim/WalletTests.cs`, `.../CafeLevelTests.cs`,
  `.../ShopTests.cs`, `.../ApplicantBoardTests.cs`

**Interfaces:**
- Produces:
  - `sealed class Wallet { int Money; int Reputation; bool CanAfford(int price); bool Spend(int price); void Earn(int amount); void AddReputation(int delta); }`
  - `static class CafeLevelTable { int LevelFor(int reputation); int ReputationForLevel(int level); CafeUnlocks UnlocksAt(int level); }`
  - `readonly struct CafeUnlocks { int GridWidth, GridHeight; int RosterCap; int ShiftSlots; IReadOnlyList<string> RecipeIds; IReadOnlyList<string> PlaceableIds; IReadOnlyList<CustomerType> CustomerTypes; }`
  - `static class DayPlan { int QuotaFor(int day); float ArrivalsPerSecondFor(int day, int cafeLevel); float PatienceScaleFor(int day); float DurationFor(int cafeLevel); ShiftSetup Build(Profile profile, uint seed); }`
  - `enum ShopItemKind { Placeable, Equipment, Gift, Expansion }`
  - `sealed class ShopItem { string Id; ShopItemKind Kind; string DisplayName; int Price; int UnlockCafeLevel; }`
  - `static class Shop { IReadOnlyList<ShopItem> StockFor(int cafeLevel); PurchaseResult Buy(Profile profile, string itemId, int? maidId); }` with `readonly struct PurchaseResult { bool Ok; string Refusal; }`
  - `sealed class ApplicantBoard { IReadOnlyList<Applicant> Slots; void Refresh(Rng rng, int cafeLevel); HireResult Hire(Profile profile, int slotIndex); }` with `sealed class Applicant { Maid Maid; int Price; }`

- [ ] **Step 1: Write the failing tests (Review Focus 4 lives here)**

```csharp
[Test] public void SpendingExactlyTheBalanceSucceedsAndLeavesZero()
[Test] public void SpendingOneMoreThanTheBalanceIsRefusedAndChangesNothing()
[Test] public void MoneyNeverGoesNegative()
[Test] public void QuotaFollowsTheCurve()                        // QuotaFor(1)==120, QuotaFor(10)==120*10^1.35 rounded
[Test] public void AMissedQuotaDoesNotAdvanceTheCurve()          // slow day → next day's quota is unchanged
[Test] public void CafeLevelRisesWithReputationAndNeverFalls()
[Test] public void EachCafeLevelUnlocksSomethingNew()            // no level unlocks an empty set
[Test] public void BuyingALockedItemIsRefused()
[Test] public void BuyingAGiftRaisesBondAndChargesOnce()
[Test] public void HiringOverTheRosterCapIsRefusedWithAReason()
[Test] public void RefreshAlwaysOffersThreeHireableApplicants()
[Test] public void ArrivalsAndPatienceGetHarderEachDay()
```

- [ ] **Step 2: Run them and watch them fail.**

- [ ] **Step 3: Implement the economy**

Ten café levels, each unlocking at least one thing, reaching grid 13×9, roster 8, shift
slots 6. `DayPlan.Build` composes `ShiftSetup` from the profile. Check the quota and
arrival curves with the `balance` server before fixing the constants:
`node ../agent_creater/tools/mcp/scripts/call.mjs balance progression_check '{...}'`.

- [ ] **Step 4: Run them and watch them pass.**

- [ ] **Step 5: Commit** — `feat(sim): add the wallet, café levels, day plan, shop and hiring`

---

## Task 9: The save — model, checksum, migration

Rubric 11's evidence. Nothing here may throw on bad input.

**Files:**
- Create: `Assets/_Game/Scripts/Sim/Save/Profile.cs`, `SaveSchema.cs`, `SaveCodec.cs`
- Test: `Assets/_Game/Tests/EditMode/Sim/SaveCodecTests.cs`

**Interfaces:**
- Produces:
  - `sealed class Profile { int SchemaVersion; uint Seed; int Day; int Money; int Reputation; int CafeLevel; List<Maid> Maids; List<int> ShiftRoster; CafeGrid Grid; Records Records; static Profile NewGame(uint seed); }`
  - `sealed class Records { int BestDay; int BestEarnings; int BestEndlessDay; int TotalCustomersServed; }`
  - `enum LoadStatus { Ok, Migrated, Corrupt, Empty }`
  - `readonly struct LoadResult { LoadStatus Status; Profile Profile; string Message; }`
  - `static class SaveCodec { const int CurrentVersion = 1; string Encode(Profile profile); LoadResult Decode(string payload); }`

- [ ] **Step 1: Write the failing tests (Review Focus 2 lives here)**

```csharp
[Test] public void RoundTripPreservesEveryField()
[Test] public void AnEmptyOrNullPayloadYieldsEmptyAndAFreshProfile()
[Test] public void ATruncatedPayloadYieldsCorruptAndAFreshProfileAndNeverThrows()
[Test] public void AFlippedByteFailsTheChecksum()
[Test] public void AnOlderSchemaVersionIsMigratedAndReportsMigrated()
[Test] public void ANewerSchemaVersionIsTreatedAsCorruptRatherThanMisread()
[Test] public void GarbageThatHappensToBeValidJsonIsStillCorrupt()
[Test] public void TheMessageOnCorruptIsPortugueseAndNamesWhatHappened()
```

- [ ] **Step 2: Run them and watch them fail.**

- [ ] **Step 3: Implement `SaveCodec`**

Payload is `v<version>.<base64 json>.<fnv1a32 hex of the json>`. `Decode` validates shape,
version and checksum before parsing, and every failure path returns a `LoadResult` — no
exception escapes. Migration from version 0 is a no-op placeholder that proves the path.

- [ ] **Step 4: Run them and watch them pass.**

- [ ] **Step 5: Commit** — `feat(sim): add the versioned, checksummed save`

---

## Task 10: Palette and the procedural sprite factory

First Unity-side task. No texture is ever imported; this is where pixels come from.

**Files:**
- Create: `Assets/_Game/Scripts/Game/Art/Palette.cs`, `ShapeSpec.cs`, `ShapeRasteriser.cs`,
  `SpriteFactory.cs`
- Test: `Assets/_Game/Tests/EditMode/Game/SpriteFactoryTests.cs`

**Interfaces:**
- Produces:
  - `static class Palette { Color Cream, Wood, Shadow, Ink, Focus, Accent(ThemeTag), ForArchetype(Archetype); Color[] Ramp(Color baseColour, int steps); }`
  - `enum ShapeId { Circle, Capsule, RoundedRect, Triangle, Star, Ring, Heart, Teardrop, Crescent, CatEar, Bow, Apron, Plate, Steam }`
  - `readonly struct ShapeSpec { ShapeId Shape; int Size; Color Fill; Color? GradientTo; Color? Outline; float OutlineWidth; float Detail; float Rotation; Vector2 Offset; Vector2 Scale; }`
  - `static class ShapeRasteriser { float SignedDistance(ShapeId shape, Vector2 p, float detail); void Blend(Color32[] pixels, int size, ShapeSpec spec); }`
  - `sealed class SpriteFactory { Sprite Get(ShapeSpec spec); Sprite Layered(IReadOnlyList<ShapeSpec> layers, int size, string cacheKey); void Clear(); int CachedCount { get; } }`

- [ ] **Step 1: Write the failing tests**

```csharp
[Test] public void EveryShapeIdRasterisesToSomethingNonEmpty()        // every enum value, >2% non-transparent pixels
[Test] public void TheSameSpecReturnsTheCachedSprite()                // reference equality, CachedCount stays 1
[Test] public void DifferentTintsAreDifferentSprites()
[Test] public void EdgesAreAntialiased()                              // a scanline through a circle has >=2 partial-alpha pixels
[Test] public void LayeredDrawsInOrderWithTheLastLayerOnTop()
[Test] public void PaletteFocusIsTheMostSaturatedColourInTheSet()
[Test] public void NoTwoPaletteNeighboursCollideInLightness()         // CIE L* at least 4 apart
```

- [ ] **Step 2: Run them and watch them fail.**

- [ ] **Step 3: Implement the rasteriser, palette and factory**

Shapes are signed distance fields sampled with a 1-pixel smoothstep so an outline costs
nothing. The palette's values are fixed with
`node ../agent_creater/tools/mcp/scripts/call.mjs pixels palette_check '{...}'` and
`palette_ramp` before being written into `Palette.cs`; keep the tool output in the commit
message. `SpriteFactory` caches on a struct key and must never allocate a texture twice
for the same spec.

- [ ] **Step 4: Run them and watch them pass.**

- [ ] **Step 5: Commit** — `feat(art): add the palette and the procedural sprite factory`

---

## Task 11: The scene, built in code

**Files:**
- Create: `Assets/_Game/Scripts/Game/Core/GameRoot.cs`, `SceneBuilder.cs`,
  `CameraRig.cs`, `Layers.cs`
- Test: `Assets/_Game/Tests/PlayMode/BootTests.cs`

**Interfaces:**
- Produces:
  - `static class Layers { const int Floor, Furniture, Characters, Items, Fx, Ui; int Order(int layer, float y); }`
  - `sealed class SceneBuilder { GameObject BuildRoom(CafeGrid grid, SpriteFactory sprites); GameObject BuildCharacter(string name, int sortingLayer); }`
  - `sealed class CameraRig { void FrameRoom(int width, int height); void Shake(float amplitude, float seconds); Vector2 ScreenToWorld(Vector2 screen); }`
  - `sealed class GameRoot : MonoBehaviour { static GameRoot Instance; SpriteFactory Sprites; CameraRig Camera; AppState State; }`

- [ ] **Step 1: Write the failing PlayMode test**

```csharp
[UnityTest] public IEnumerator BootBuildsAPlayableSceneWithoutErrors()
// load Boot, wait 2 frames, assert: a Camera exists, GameRoot.Instance != null,
// the room has one floor tile object per grid cell, and LogAssert catches no error
[UnityTest] public IEnumerator SortingOrderPutsLowerCharactersInFront()
```

- [ ] **Step 2: Run them and watch them fail**

Run: `tools/run-tests.sh PlayMode`

- [ ] **Step 3: Implement `GameRoot`, `SceneBuilder`, `CameraRig` and `Layers`**

`GameRoot.Awake` creates the camera, the sprite factory, the audio director and the UI
root; nothing is authored in the scene file. `Layers.Order` returns
`layer * 10000 - Mathf.RoundToInt(y * 100)` so a character lower on screen draws in front.
`CameraRig.FrameRoom` sets an orthographic size that fits the grid with a one-tile margin
at 16:9 and never clips at 4:3.

- [ ] **Step 4: Run them and watch them pass.**

- [ ] **Step 5: Commit** — `feat(game): build the scene in code at boot`

---

## Task 12: Views — maids, customers, room, bound to the sim

**Files:**
- Create: `Assets/_Game/Scripts/Game/View/MaidSpriteBuilder.cs`, `MaidView.cs`,
  `CustomerView.cs`, `RoomView.cs`, `ShiftPresenter.cs`
- Test: `Assets/_Game/Tests/EditMode/Game/MaidSpriteBuilderTests.cs`,
  `Assets/_Game/Tests/PlayMode/ShiftPresenterTests.cs`

**Interfaces:**
- Consumes: `SpriteFactory`, `Palette`, `Shift`, `Maid`, `Customer`, `CafeGrid`.
- Produces:
  - `static class MaidSpriteBuilder { IReadOnlyList<ShapeSpec> Layers(Maid maid, MaidPose pose); string CacheKey(Maid maid, MaidPose pose); }` with `enum MaidPose { Idle, Walk, Carry, Cook, Tired }`
  - `sealed class MaidView : MonoBehaviour { void Bind(Maid maid); void Sync(Worker worker); void PlayPose(MaidPose pose); }`
  - `sealed class CustomerView : MonoBehaviour { void Bind(Customer customer); void Sync(Customer customer); }`
  - `sealed class RoomView : MonoBehaviour { void Build(CafeGrid grid); void SetFlowOverlay(bool visible); }`
  - `sealed class ShiftPresenter : MonoBehaviour { void Begin(Shift shift); void Tick(float dt); event Action<ShiftResult> Finished; }`

- [ ] **Step 1: Write the failing tests**

```csharp
[Test] public void EquippingAHatChangesTheLayerStackAndTheCacheKey()
[Test] public void EachArchetypeProducesADistinctSilhouette()  // pairwise: layer stacks differ in shape, not only in tint
[Test] public void ATiredMaidUsesTheTiredPose()
[UnityTest] public IEnumerator EveryCustomerInTheSimHasExactlyOneViewAndNoneLeak()
[UnityTest] public IEnumerator ACustomerAboutToLeaveIsTintedWithTheFocusColour()
```

- [ ] **Step 2: Run them and watch them fail.**

- [ ] **Step 3: Implement the views**

`MaidSpriteBuilder.Layers` stacks body, tail, ears, hair, outfit, headwear, accessory,
shoes, face — reading `Equipment.LayerShapeId` and `TintHex` for the four gear slots and
the archetype's tell for hair and ears. `ShiftPresenter` owns the view pools, keyed by sim
id, and creates/destroys views strictly from the sim's collections so a leak is a test
failure.

- [ ] **Step 4: Run them and watch them pass.**

- [ ] **Step 5: Commit** — `feat(view): draw maids, customers and the room from the sim`

---

## Task 13: Juice

The rubric-3 and rubric-8 task. Feedback carried by the world, not the HUD.

**Files:**
- Create: `Assets/_Game/Scripts/Game/Feel/Tween.cs`, `Squash.cs`, `Popup.cs`,
  `ParticleBurst.cs`, `JuiceDirector.cs`, `Clocks.cs`
- Test: `Assets/_Game/Tests/EditMode/Game/TweenTests.cs`,
  `Assets/_Game/Tests/PlayMode/JuiceDirectorTests.cs`

**Interfaces:**
- Produces:
  - `static class Clocks { float Game { get; } float Ui { get; } float Scale { set; } void Pause(bool paused); }`
  - `static class Ease { float OutBack(float t), OutElastic(float t), InOutQuad(float t), OutCubic(float t); }`
  - `sealed class Tween { static Tween Scale(Transform t, Vector3 to, float seconds, Func<float,float> ease); static Tween Move(...); static Tween Fade(SpriteRenderer r, float to, float seconds); Tween Then(Action done); void Kill(); }`
  - `static class Popup { void Money(Vector3 world, int amount); void Text(Vector3 world, string text, Color colour); }`
  - `static class ParticleBurst { void Hearts(Vector3 world, int count); void Sparkles(...); void Steam(...); void Anger(...); }`
  - `sealed class JuiceDirector : MonoBehaviour { void Handle(ShiftEvent e); }`

- [ ] **Step 1: Write the failing tests**

```csharp
[Test] public void EaseFunctionsStartAtZeroAndEndAtOne()
[Test] public void OutBackOvershootsPastOne()
[Test] public void PausingStopsTheGameClockButNotTheUiClock()
[UnityTest] public IEnumerator EveryShiftEventKindProducesSomeVisibleResponse()  // the test that stops a new event being silent
[UnityTest] public IEnumerator PopupsAreReleasedAfterTheirLifetime()             // no leak over 200 popups
```

- [ ] **Step 2: Run them and watch them fail.**

- [ ] **Step 3: Implement the feel layer**

`JuiceDirector.Handle` switches on every `ShiftEventKind`; the test above is what makes a
missing case fail rather than pass silently. Particles are pooled quads using the sprite
factory — no Unity particle system assets, so nothing is imported.

- [ ] **Step 4: Run them and watch them pass.**

- [ ] **Step 5: Commit** — `feat(feel): add tweens, popups, particles and the juice director`

---

## Task 14: Audio

**Files:**
- Create: `Assets/_Game/Audio/*.wav` (synthesised, committed)
- Create: `Assets/_Game/Scripts/Game/Audio/SoundId.cs`, `AudioDirector.cs`, `MusicDirector.cs`
- Create: `tools/synthesise-audio.sh`
- Test: `Assets/_Game/Tests/EditMode/Game/AudioTests.cs`,
  `Assets/_Game/Tests/PlayMode/MusicDirectorTests.cs`

**Interfaces:**
- Produces:
  - `enum SoundId { DoorBell, OrderPop, TicketSlap, Sizzle, PlateChime, Register, Coin, Heart, NyaGenki, NyaKuudere, NyaTsundere, NyaDojikko, NyaOjousama, NyaOneesan, NyaIdol, NyaGourmand, NyaMukuchi, AngryPuff, LevelUp, UiTick, UiConfirm, UiBack, Drop }`
  - `sealed class AudioDirector : MonoBehaviour { void Play(SoundId id, float pitchJitter = 0.04f); void PlayAt(SoundId id, Vector3 world); void Nya(Archetype archetype); void Handle(ShiftEvent e); }`
  - `sealed class MusicDirector : MonoBehaviour { void Play(MusicBed bed, float crossfadeSeconds = 1.5f); void Duck(bool ducked); }` with `enum MusicBed { Menu, ShiftCalm, ShiftRush }`

- [ ] **Step 1: Write `tools/synthesise-audio.sh` and produce the set**

Every sound comes from the `audio` MCP server — `sfx_palette` to settle the family, then
`sfx_synthesise`/`sfx_from_preset` per sound, `sfx_mix` for the three music beds. Then
`sfx_measure` each file and keep the report: nothing may peak above −1 dBFS or sit more
than 6 LU from the set's mean loudness, which is the rule that stops a sound set going
shrill. Copy the approved set to `../visual_assets/audio/2026-10-22_nekomata_sfx/` with a
README.

- [ ] **Step 2: Write the failing tests**

```csharp
[Test] public void EverySoundIdHasAClipOnDisk()          // the test that stops a silent enum value
[Test] public void EveryArchetypeHasItsOwnNya()
[Test] public void NoClipPeaksAboveMinusOneDbfs()        // read the committed WAV headers and samples
[UnityTest] public IEnumerator MusicCrossfadesToRushAtSeventyPercentOccupancy()
[UnityTest] public IEnumerator PausingDucksTheMusicAndRestoresIt()
```

- [ ] **Step 3: Run them and watch them fail.**

- [ ] **Step 4: Implement the two directors**

Clips are loaded from `Resources` (the one place a file is read, because synthesised audio
has to live somewhere). One pooled `AudioSource` set with pitch jitter so a repeated sound
never machine-guns. `MusicDirector` keeps two sources and crossfades between them.

- [ ] **Step 5: Run them and watch them pass.**

- [ ] **Step 6: Commit** — `feat(audio): synthesise the sound set and add the directors`

---

## Task 15: UI

**Files:**
- Create: `Assets/_Game/Scripts/Game/Ui/UiKit.cs`, `Screen.cs`, `UiRoot.cs`,
  `MenuScreen.cs`, `ShiftHud.cs`, `ResultsScreen.cs`, `RosterScreen.cs`,
  `MaidDetailScreen.cs`, `ShopScreen.cs`, `EditorScreen.cs`, `PauseScreen.cs`,
  `SettingsScreen.cs`, `DialogScreen.cs`, `Strings.cs`
- Test: `Assets/_Game/Tests/EditMode/Game/StringsTests.cs`,
  `Assets/_Game/Tests/PlayMode/UiTests.cs`

**Interfaces:**
- Produces:
  - `static class UiKit { RectTransform Panel(...); Button Button(...); TMP_Text Label(...); Slider Bar(...); RectTransform Row(...); }` — every widget built in code from generated sprites and the project's TMP default font
  - `abstract class Screen : MonoBehaviour { void Show(); void Hide(); bool Visible { get; } abstract void Build(); }`
  - `sealed class UiRoot : MonoBehaviour { T Get<T>() where T : Screen; void Push(Screen screen); void Pop(); Screen Top { get; } void Dialog(string title, string body, string confirm, Action onConfirm); }`
  - `static class Strings { string Get(string key); IReadOnlyDictionary<string, string> All; }` — pt-BR

- [ ] **Step 1: Write the failing tests**

```csharp
[Test] public void EveryStringKeyUsedInTheUiExists()      // reflect over the screens' key constants
[Test] public void NoStringIsEmptyOrStillInEnglish()
[UnityTest] public IEnumerator EveryScreenBuildsAndShowsWithoutErrors()
[UnityTest] public IEnumerator EscapeOpensPauseFromTheShiftAndClosesItAgain()
[UnityTest] public IEnumerator TheEditorScreenShowsAmbienceFlowSeatsAndThemeAndUpdatesOnPlacement()
[UnityTest] public IEnumerator ButtonsAreAtLeastFortyFourPixelsAndInsideTheSafeArea()
```

- [ ] **Step 2: Run them and watch them fail.**

- [ ] **Step 3: Implement the screens**

Ten screens, built in code, all text through `Strings`. Audit the result with
`node ../agent_creater/tools/mcp/scripts/call.mjs layout ui_audit '{...}'` on a capture
from Task 18 and fix what it reports.

- [ ] **Step 4: Run them and watch them pass.**

- [ ] **Step 5: Commit** — `feat(ui): add the ten screens and the pt-BR string table`

---

## Task 16: Input and the manager

**Files:**
- Create: `Assets/_Game/Scripts/Game/Input/NekoInput.cs`, `ManagerController.cs`,
  `ClickTargeting.cs`, `EditorPlacementInput.cs`
- Test: `Assets/_Game/Tests/PlayMode/InputTests.cs`

**Interfaces:**
- Produces:
  - `sealed class NekoInput : MonoBehaviour { event Action<Vector2> Click, RightClick; event Action Pause, ToggleFlow; Vector2 Pointer { get; } }`
  - `static class ClickTargeting { bool Resolve(Shift shift, CafeGrid grid, Vector2 world, out TaskKind kind, out int targetId); }`
  - `sealed class ManagerController : MonoBehaviour { void Bind(Shift shift); }`
  - `sealed class EditorPlacementInput : MonoBehaviour { void Begin(string defId); void Cancel(); }`

- [ ] **Step 1: Write the failing tests**

```csharp
[UnityTest] public IEnumerator ClickingACustomerSendsTheManagerToSeatThem()
[UnityTest] public IEnumerator ClickingEmptyFloorDoesNothingAndIsNotAnError()
[UnityTest] public IEnumerator RightClickingAMaidRushesHer()
[UnityTest] public IEnumerator PlacementRefusesAnOverlapAndSaysWhy()
[UnityTest] public IEnumerator NoLegacyInputApiIsReferenced()   // reflect over Neko.Game for UnityEngine.Input
```

- [ ] **Step 2: Run them and watch them fail.**

- [ ] **Step 3: Implement input**

`ClickTargeting.Resolve` picks the topmost sim entity under the cursor and maps it to the
task the manager would perform there; ambiguous or empty targets return `false`.

- [ ] **Step 4: Run them and watch them pass.**

- [ ] **Step 5: Commit** — `feat(input): add pointer control, the manager and placement`

---

## Task 17: Application state machine, save bridge and the error boundary

Rubric 11's other half.

**Files:**
- Create: `Assets/_Game/Scripts/Game/Core/AppState.cs`, `AppStateMachine.cs`,
  `SaveBridge.cs`, `ErrorBoundary.cs`
- Modify: `Assets/_Game/Scripts/Game/Core/GameRoot.cs`
- Test: `Assets/_Game/Tests/PlayMode/AppStateTests.cs`,
  `Assets/_Game/Tests/EditMode/Game/SaveBridgeTests.cs`

**Interfaces:**
- Produces:
  - `enum AppState { Boot, Menu, Cafe, Shift, Results, Paused }`
  - `sealed class AppStateMachine { AppState Current { get; } bool CanGo(AppState to); void Go(AppState to); event Action<AppState, AppState> Changed; }`
  - `static class SaveBridge { const string Key = "nekomata.profile"; void Save(Profile profile); LoadResult Load(); void Wipe(); }`
  - `static class ErrorBoundary { void Guard(string what, Action body); event Action<string> Failed; }`

- [ ] **Step 1: Write the failing tests**

```csharp
[Test] public void IllegalTransitionsAreRefusedNotThrown()      // Shift → Results ok, Menu → Results refused
[Test] public void SaveBridgeRoundTripsThroughPlayerPrefs()
[Test] public void ACorruptStoredSaveYieldsAFreshProfileAndAMessage()
[UnityTest] public IEnumerator TheProfileIsSavedAfterEveryShiftAndEveryPurchase()
[UnityTest] public IEnumerator AnExceptionInsideAShiftEndsItAsASlowDayAndReturnsToTheCafe()
[UnityTest] public IEnumerator PauseStopsTheShiftClockAndNoTaskResolvesWhilePaused()
```

- [ ] **Step 2: Run them and watch them fail.**

- [ ] **Step 3: Implement the state machine, bridge and boundary**

`ErrorBoundary.Guard` wraps the shift tick and every screen action: it logs, raises
`Failed`, shows the dialog from Task 15, and leaves the app in a legal state.

- [ ] **Step 4: Run them and watch them pass.**

- [ ] **Step 5: Commit** — `feat(core): add the state machine, the save bridge and the error boundary`

---

## Task 18: PlayMode capture and a scripted long run

Proof it is actually on screen, and the frames the owner's `visual_assets` repertoire wants.

**Files:**
- Create: `Assets/_Game/Tests/PlayMode/CaptureTests.cs`, `LongRunTests.cs`
- Create: `tools/capture.sh`

**Interfaces:**
- Consumes: `NEKO_CAPTURE_DIR` environment variable, read by the capture test.

- [ ] **Step 1: Write the capture test**

```csharp
[UnityTest] public IEnumerator CaptureTheGame()
// discards the first 10 frames (batch mode renders them without albedo),
// then writes one raw dump per named moment: menu, café editor, shift early,
// shift rush, a level-up, roster, maid detail, shop, results
```

- [ ] **Step 2: Write the long-run test**

```csharp
[UnityTest] public IEnumerator TwentyDaysRunWithoutExceptionOrStall()
// drives the sim through 20 days headlessly at a fixed seed; asserts no LogAssert error,
// every day terminates, and the profile stays loadable after each one
```

- [ ] **Step 3: Run the capture through the MCP server**

Run: `tools/capture.sh` →
`node ../agent_creater/tools/mcp/scripts/call.mjs --timeout 1200 unity unity_capture '{"projectPath":"<abs>","testFilter":"Neko.Tests.PlayMode.CaptureTests.CaptureTheGame","outputDirectory":"../visual_assets/frames/2026-10-22_nekomata_base","captureEnvVar":"NEKO_CAPTURE_DIR"}'`
Expected: nine PNGs with a reported palette that is not flat or desaturated.

- [ ] **Step 4: Check the frames by measurement**

`pixels image_describe` and `layout ui_audit` on the captures; `palette_check` on the
census. Fix what they report, re-capture, and write the set's README.

- [ ] **Step 5: Commit** — `test: add the capture and long-run PlayMode tests`

---

## Task 19: Balance pass

**Files:**
- Modify: `Assets/_Game/Scripts/Sim/Economy/DayPlan.cs`,
  `Assets/_Game/Scripts/Sim/Economy/CafeLevelTable.cs`,
  `Assets/_Game/Scripts/Sim/Service/CustomerTable.cs`
- Create: `Assets/_Game/Tests/EditMode/Sim/BalanceTests.cs`, `docs/BALANCE.md`

- [ ] **Step 1: Write the failing balance tests**

```csharp
[Test] public void DayOneIsWinnableWithTheStartingRosterAndLayout()  // 200 seeded shifts, >=90% meet quota
[Test] public void DayTenIsNotWinnableWithTheStartingRoster()        // the upgrade must matter
[Test] public void DayTenIsWinnableWithAReasonablyUpgradedRoster()   // >=70% of 200 seeds
[Test] public void NoDayIsWinnableByIdling()                         // manager does nothing → quota never met past day 2
[Test] public void EveryArchetypeIsTheBestPickForSomeDay()           // no dead archetype
[Test] public void TheShopNeverPricesAnItemAtMoreThanTwentyMinutesOfPlay()
```

- [ ] **Step 2: Run them and watch them fail; read the numbers they print.**

- [ ] **Step 3: Tune with the balance server**

`progression_check` on the quota and arrival curves, `economy_check` on shop prices in
minutes of play, `sequence_check` on the applicant and customer rollers. Record every
tool call and its verdict in `docs/BALANCE.md`.

- [ ] **Step 4: Run them and watch them pass.**

- [ ] **Step 5: Commit** — `balance: tune the day curve, prices and arrival weights`

---

## Task 20: GDD, README, build and delivery

**Files:**
- Create: `docs/GDD.md` (pt-BR), `docs/ITCH_PAGE.md` (pt-BR), `README.md` (rewrite)
- Create: `Assets/_Game/Scripts/Editor/WebGLBuilder.cs`,
  `Assets/_Game/Scripts/Editor/Neko.Game.Editor.asmdef`
- Test: `Assets/_Game/Tests/EditMode/Game/DocsTests.cs`

**Interfaces:**
- Produces: `static class Neko.Game.Editor.WebGLBuilder { void Build(); }` — batch-mode entry
  point, output `Build/WebGL/`, compression Gzip, exceptions `ExplicitlyThrownExceptionsOnly`,
  `WebGLTemplate` the default, colour space Gamma (WebGL1-safe and cheaper).

- [ ] **Step 1: Write the failing tests**

```csharp
[Test] public void TheGddCoversEveryRubricHeading()   // all 12 headings present in docs/GDD.md
[Test] public void TheReadmeStatesThatNoAssetWasImported()
[Test] public void EveryArchetypeAndEveryCustomerTypeIsDocumentedInTheGdd()
```

- [ ] **Step 2: Write `docs/GDD.md` in Portuguese**

One section per rubric, so the graders can read it the way they grade it: conceito e
narrativa, mecânicas, feedback diegético, UI/HUD, engajamento, level design e progressão,
som e música, efeitos visuais, rejogabilidade, balanceamento, estados e erros, e o próprio
GDD. Mirror `docs/superpowers/specs/2026-10-22-nekomata-cafe-design.md`, in Portuguese,
and keep it current as the game changes — it is a living document and the brief grades it
as one.

- [ ] **Step 3: Rewrite `README.md`**

What the game is, how to run it, how to build it, the team, and a **Credits** section
stating that every sprite is generated at runtime and every sound synthesised, with the
tools named — which is the brief's asset-credit requirement answered honestly.

- [ ] **Step 4: Build for WebGL and play it**

Run: `tools/build-webgl.sh`
Expected: `Build/WebGL/index.html` exists and the build log reports no error. Serve it and
confirm the menu appears and a shift can be finished.

- [ ] **Step 5: Commit and push** — `docs: add the GDD, the README and the WebGL build entry point`

---

## Notes for whoever executes this

- **One Unity process at a time.** Check `pgrep -fa Unity | grep -v pgrep` before every run;
  a `Temp/UnityLockfile` with no editor alive is stale and can be deleted.
- Run **EditMode before PlayMode** — it is fast and catches the compile errors.
- Tasks 2–9 are pure C# and need no editor to reason about, but still need one to *run*,
  because the tests live in a Unity test assembly.
- Tasks 10–17 depend on 2–9 only through the interfaces above; 14 (audio) is the one task
  that can genuinely run in parallel with another, because it is shell work against the
  `audio` server and touches no shared file.
