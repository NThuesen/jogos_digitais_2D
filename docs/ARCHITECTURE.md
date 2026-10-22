# Architecture

## Purpose

The starter defines architectural defaults, not a prebuilt gameplay framework. Each generated game should begin with as little runtime structure as possible and grow only in response to actual requirements.

## Baseline philosophy

The default architecture is intentionally shallow:

```text
scene objects
  -> small MonoBehaviours
  -> plain data/config where useful
  -> Unity/platform APIs
```

Additional layers must earn their existence.

## Folder ownership

### `Assets/_Game/Art`

Game-owned visual assets and materials. Third-party packages should not be mixed into `_Game` when they can remain under their vendor/package location.

### `Assets/_Game/Audio`

Game-owned music and sound effects.

### `Assets/_Game/Data`

Serializable configuration that benefits from being separated from behavior. ScriptableObjects are appropriate when shared editable data is genuinely useful; they are not mandatory for every value.

### `Assets/_Game/Prefabs`

Game-owned reusable scene objects.

### `Assets/_Game/Scenes`

Scenes belonging to the game. The starter does not prescribe a boot/menu/gameplay scene hierarchy before a title needs one.

### `Assets/_Game/Scripts/Core`

Game flow and cross-feature runtime code that truly forms the core of that title. Avoid turning `Core` into a dumping ground.

### `Assets/_Game/Scripts/Input`

Touch/input behavior owned by the game. Prefer the simplest input approach that satisfies the title.

### `Assets/_Game/Scripts/Mobile`

Platform-facing behavior such as haptics, safe-area handling, lifecycle concerns, or integrations when required.

### `Assets/_Game/Scripts/Save`

Persistence code. For very small games, a few `PlayerPrefs` values can be acceptable. Introduce a larger save model only when the data warrants it.

### `Assets/_Game/Scripts/UI`

Presentation and UI interaction code. Gameplay domain rules should not live here merely because a button triggers them.

## Dependency rules

- UI may call small public game-facing APIs; it should not own gameplay rules.
- Platform/mobile integrations should be isolated from gameplay when practical.
- Data containers should not become service locators.
- Avoid circular feature dependencies.
- Prefer direct references for small local relationships over global lookup.

## Managers and singletons

A class named `GameManager` is not forbidden, but it is not a default requirement. Create a coordinator only when there is actual coordination to perform.

Persistent singletons should be rare. If a system only exists in one gameplay scene, let the scene own it.

## Events

Use direct calls for simple relationships. Use C# events or UnityEvents when decoupling has a concrete benefit. Do not introduce a project-wide event bus by default.

## Object pooling

Pooling is not part of the starter. Add it when a title repeatedly creates/destroys enough objects for pooling to materially simplify behavior or improve runtime performance.

## Rendering

The starter does not force URP, URP 2D, or a specific renderer in v0.2. A generated game chooses the lightest rendering setup that matches its art direction. Once chosen, keep that choice stable unless a migration has a concrete benefit.

## Input

The package baseline is **Unity Input System 1.20.0**, with the new Input System selected as the active input backend. This standardizes the supported API without forcing a universal gameplay-input abstraction.

A one-finger tap, drag, swipe, or hold can still be implemented locally and directly. Create shared input wrappers only when a real title benefits from them; do not add an input framework merely because the package exists.

## Performance baseline

For these small mobile games:

- avoid per-frame managed allocations in hot paths;
- avoid expensive scene-wide searches inside `Update`;
- cache references used repeatedly;
- keep physics use proportional to the mechanic;
- profile before performing non-obvious optimization;
- test on a real mobile device before calling performance finished.

Dynamic Batching is disabled in the template because Unity has deprecated that path. Do not replace it with speculative optimization; use the rendering approach appropriate to the selected pipeline and profile the actual game.

## Architecture change rule

When a game needs more structure, add the smallest structure that resolves the current pressure. Do not redesign the project around hypothetical future titles.
