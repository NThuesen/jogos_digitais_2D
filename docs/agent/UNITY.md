# Unity boundaries and visual work

**Read when:** editing gameplay integration, sprites, UI, audio, Unity assets, scenes, or Editor-facing code. For deep ownership details use [ARCHITECTURE.md](../ARCHITECTURE.md); read [GDD.md](../GDD.md) only when changing product behavior.

- Project: **Unity 6000.5.9f1**, 2D URP, desktop WebGL. Do not inherit mobile-starter assumptions.
- **Visual/UI improvement tasks only:** [active roadmap](../ROADMAP.md) → read **only the assigned task ID**. For a one-off sprite correction unrelated to the roadmap, do not load the full roadmap.
- **Current art direction, menus and visual-novel presentation only:** [Illustrated Anime 2D / current decisions (ACCEPTED)](../art/CURRENT_VISUAL_DIRECTION.md). The earlier cozy pixel-art note is historical; do not treat it as a mandatory style.
- **Maid portraits/chibis/cards only:** [Visual direction (ACCEPTED)](../art/MAID_VISUAL_DECISION.md) → [spec/layers (PROPOSED)](../art/MAID_VISUAL_SPEC.md) → [per-identity template](../art/maids/_TEMPLATE.md). Read these only for maid appearance/pipeline tasks, not unrelated Unity work.
- `Assets/_Game/Scripts/Sim` (`Neko.Sim`) is deterministic C# without `UnityEngine`; it owns rules, economy, progression, and save format. `Assets/_Game/Scripts/Game` (`Neko.Game`) owns lifecycle, input, UI, audio, visuals and sim integration. UI must not reimplement sim rules.
- `GameSession` mediates persistent changes, `SaveCodec` owns format/checksums and `SaveBridge` integrates Unity persistence. Inspect current code before editing their contracts.
- The boot scene constructs presentation at runtime. **Nine illustrated maid Walk sheets are imported** under `Assets/_Game/Resources/Maids/` and resolved by `MaidAnimCatalog`; other visuals still use procedural art/furniture. The source walking PNGs are local and Git-ignored. Portraits, gear-compatible illustrated variants and the environment redesign are not implemented. Inspect the relevant renderer/assets before changing visuals; **do not preload the GDD** for a small art fix.
- Preserve `.meta` files/GUIDs, serialized references, scene wiring, Unity/package versions and 2D URP settings. Check lifecycle, scene reload, and missing Inspector references when relevant.
- Code, identifiers and technical comments in English; player-facing copy in pt-BR. Never commit Unity-generated caches/builds, secrets or local credentials.
- Use the repository's guarded Unity scripts; do not launch competing Editors. Avoid speculative frameworks, unrelated asset replacement and broad refactors.
- A file change or passing simulation CI does not prove the rendered outcome. Inspect actual Unity frames/behavior when visual correctness matters, or report validation as **not run**.
