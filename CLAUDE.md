# Claude Code — Neko Maid Inc.

**Canonical rules:** [AGENTS.md](AGENTS.md). Read and follow it **once at task start**, including automatic task-branch creation **before the first edit**, one task → one branch → one PR, truthful validation, safety and GitHub gates. **This file does not replace or duplicate those rules.**

## Load only the task's context
- UI/art workflow: [active roadmap index](docs/ROADMAP.md) → **only the assigned task note**; Unity implementation: [Unity](docs/agent/UNITY.md).
- Maid visuals: [current illustrated art direction](docs/art/CURRENT_VISUAL_DIRECTION.md) and [current maid implementation/limits](docs/art/MAID_VISUAL_DECISION.md); conceptual spec only when needed.
- Game rules: [GDD](docs/GDD.md); technical boundaries: [Architecture](docs/ARCHITECTURE.md); test evidence: [Validation](docs/agent/VALIDATION.md).
- For a one-file edit, inspect that file and only immediately relevant references. **Do not preload all docs or files.**

## Task-specific handoffs
- **Reviewing Codex implementation:** read [Review & correction contract](docs/agent/REVIEW.md) and only the changed sources/evidence. No invented bugs, cosmetic refactors or continuous CI monitoring.
- **Historical ART-01 work:** [ART-01](docs/roadmap/ART-01_STYLE.md) is archived exploration; do not restart pixel-art selection. Current art is illustrated anime/chibi, with future scene/UI changes requiring owner approval.
