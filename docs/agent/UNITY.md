# Unity boundaries and visual work

**Read when:** editing gameplay integration, sprites, UI, audio, Unity assets, scenes, or Editor-facing code. For deep ownership details use [ARCHITECTURE.md](../ARCHITECTURE.md); read [GDD.md](../GDD.md) only when changing product behavior.

- Project: **Unity 6000.5.9f1**, 2D URP, desktop WebGL. Do not inherit mobile-starter assumptions.
- **Visual/UI improvement tasks only:** [active roadmap](../ROADMAP.md) → read **only the assigned task ID**. For a one-off sprite correction unrelated to the roadmap, do not load the full roadmap.
- **Art direction, menus and visual-novel presentation only:** [Cozy Pixel-Art Anime + Visual Novel (ACCEPTED)](../art/COZY_VISUAL_NOVEL_DIRECTION.md). Do not load for unrelated gameplay/simulation work.
- **Maid portraits/chibis/cards only:** [Visual direction (ACCEPTED)](../art/MAID_VISUAL_DECISION.md) → [spec/layers (PROPOSED)](../art/MAID_VISUAL_SPEC.md) → [per-identity template](../art/maids/_TEMPLATE.md). Read these only for maid appearance/pipeline tasks, not unrelated Unity work.
- `Assets/_Game/Scripts/Sim` (`Neko.Sim`) is deterministic C# without `UnityEngine`; it owns rules, economy, progression, and save format. `Assets/_Game/Scripts/Game` (`Neko.Game`) owns lifecycle, input, UI, audio, visuals and sim integration. UI must not reimplement sim rules.
- `GameSession` mediates persistent changes, `SaveCodec` owns format/checksums and `SaveBridge` integrates Unity persistence. Inspect current code before editing their contracts.
- The existing boot scene constructs most presentation at runtime. Art is currently procedural (see `Assets/_Game/Scripts/Game/Art/`) and audio is synthesized. For a small sprite task, inspect the relevant renderer/assets first; **do not preload the whole GDD** unless gameplay meaning changes.
- Preserve `.meta` files/GUIDs, serialized references, scene wiring, Unity/package versions and 2D URP settings. Check lifecycle, scene reload, and missing Inspector references when relevant.
- Code, identifiers and technical comments in English; player-facing copy in pt-BR. Never commit Unity-generated caches/builds, secrets or local credentials.
- Use the repository's guarded Unity scripts; do not launch competing Editors. Avoid speculative frameworks, unrelated asset replacement and broad refactors.
- A file change or passing simulation CI does not prove the rendered outcome. Inspect actual Unity frames/behavior when visual correctness matters, or report validation as **not run**.
