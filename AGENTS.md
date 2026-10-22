# SlipperyZlope Unity Agent Rules

This repository is a minimal reusable Unity mobile starter. Keep it small.

## Mission

Build simple mobile games quickly without turning the starter into a framework. Codex should optimize for understandable code, short iteration loops, and safe Unity changes.

## Authority

For a game repository created from this starter, use this order when instructions conflict:

1. the user's current explicit request;
2. the current repository/runtime state;
3. that game's `docs/GAME_SPEC.md` and accepted game-specific decisions;
4. repository-local architecture or execution-plan docs;
5. this file and inherited starter conventions.

Report conflicts instead of silently guessing.

## Engineering defaults

- Unity 6000.5.8f1 unless the repository explicitly migrates.
- C# only for gameplay/runtime code.
- Android-first, touch-first.
- Unity Input System is the baseline input package; do not re-enable the legacy Input Manager or introduce legacy `UnityEngine.Input` usage unless an explicit migration requirement justifies it.
- Prefer the smallest implementation that satisfies the current game.
- Prefer composition and plain MonoBehaviours over framework layers.
- Prefer serialized fields and small data objects over global registries.
- A tiny game does not need enterprise architecture.
- Do not add a dependency-injection framework, event bus, service locator, ECS/DOTS stack, or generic state-machine framework unless a concrete requirement justifies it.
- Do not create generic managers merely because a future game might need them.

## Generated-game initialization

When this template has been copied into a game repository, treat `DefaultCompany`, `unity-mobile-starter`, orientation, rendering choice, and application identifiers as values that must be explicitly reviewed for that game. Follow `docs/NEW_GAME_CHECKLIST.md`; do not let template identity reach a release build.

## Starter boundary

Do not add game-specific mechanics, art, progression, economy, characters, level rules, or theme-specific code to the starter.

A reusable system should be promoted back to the starter only after repeated use demonstrates that it is genuinely generic, or when it is a cross-game compliance/build requirement.

## Unity safety

- Preserve `.meta` files and GUIDs.
- Avoid mass asset reimports or unrelated serialized scene/prefab churn.
- Do not hand-edit Unity YAML unless there is a clear reason and the result can be validated.
- Never commit `Library`, `Temp`, `Obj`, `Logs`, `UserSettings`, builds, signing keys, or local credentials.
- Do not change packages, render pipeline, scripting backend, target API, or build settings as collateral work.

## Validation

For code changes:

1. compile when a Unity-capable environment is available;
2. run relevant EditMode/PlayMode tests when they exist;
3. perform the smallest relevant runtime check when practical;
4. state clearly what was and was not validated.

Do not claim visual, runtime, device, store, or performance validation that did not happen.

## Scope discipline

Before implementing, inspect the files that own the behavior. Change the fewest files required. If a task reveals unrelated cleanup, report it separately unless it blocks the requested work.

Do not commit or push unless explicitly asked.
