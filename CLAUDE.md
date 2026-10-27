# Claude Code — Neko Maid Inc.

**Canonical rules:** [AGENTS.md](AGENTS.md). Read and follow it **once at task start**, including automatic task-branch creation **before the first edit**, one task → one branch → one PR, truthful validation, safety and GitHub gates. **This file does not replace or duplicate those rules.**

## Load only the task's context
- UI/art workflow: [active roadmap index](docs/ROADMAP.md) → **only the assigned task note**; Unity implementation: [Unity](docs/agent/UNITY.md).
- Maid portraits/chibis: [accepted visual decision](docs/art/MAID_VISUAL_DECISION.md) and proposed spec only when applicable.
- Game rules: [GDD](docs/GDD.md); technical boundaries: [Architecture](docs/ARCHITECTURE.md); test evidence: [Validation](docs/agent/VALIDATION.md).
- For a one-file edit, inspect that file and only immediately relevant references. **Do not preload all docs or files.**

## When asked to review Codex work
Review the **actual HEAD diff**, relevant sources and runtime/visual evidence; classify issues `blocking / important / optional` with reproducible reasons. Distinguish confirmed defects from design preferences, and do not invent bugs (a partially empty lineup is valid). Send findings; Codex fixes valid ones and revalidates. Avoid cosmetic refactors and do not claim test or visual success without execution.

For ART-01, present alternative appearances and trade-offs for **owner approval**; do not implement the game, choose a final style, or start mass asset generation without authorization. Do not watch CI by polling.
