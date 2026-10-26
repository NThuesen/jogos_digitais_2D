# Neko Maid Inc. — AI agent instructions

## Mission and current product
Build and maintain **Neko Maid Inc.**, a Unity 6000.5.9f1 2D URP game delivered as desktop WebGL (itch.io). Four GitHub collaborators use Codex, Claude Code and Kiro to implement and maintain it. This repository is **not** a reusable Android/mobile starter.

The player composes a team of autonomous café maids **between days**, watches the day run without gameplay intervention, then adjusts lineup, economy and décor. Preserve the current design; do not revive the discarded real-time manager game.

## Read before changing anything
1. Read this file and inspect the files that own the requested behavior.
2. For game rules, UX, balance and progression, read `docs/GDD.md`.
3. For assembly boundaries and runtime design, read `docs/ARCHITECTURE.md`.
4. Read the linked GitHub Issue / task contract (scope and acceptance criteria).
5. Treat runtime and tests as evidence of implementation, not permission to silently redefine design. If documentation contradicts code, report the mismatch and resolve it in scope.

Avoid inventing missing requirements. A design change needs a clearly stated decision in the task, not a speculative "improvement". Keep changes small and reversible.

## Architecture invariants
- `Assets/_Game/Scripts/Sim` / `Neko.Sim`: pure deterministic C#, **no `UnityEngine` reference**. Owns maids, teamwork, chemistry, customer/shift simulation, economy, progression and save format.
- `Assets/_Game/Scripts/Game` / `Neko.Game`: Unity lifecycle, input, screen presentation, sprites, audio and sim integration. Screens must not duplicate sim rules.
- `GameSession` mediates persistent changes; `SaveCodec` owns version/checksum, `SaveBridge` owns Unity persistence.
- Unity scene `Assets/_Game/Scenes/Boot.unity` contains the boot root. The project creates scene objects procedurally. Do not add prefab/scene frameworks or edit Unity YAML unless required and verified.
- Preserve `.meta` files/GUIDs, Unity version, package versions and rendering pipeline unless the task explicitly authorizes changes.
- Do not replace the existing procedural-art and synthesised-audio direction just to make implementation easier.
- Do not add new dependency injection/event bus/singleton infrastructure, third-party packages or large refactors without a concrete need.
- Never commit `Library/`, `Temp/`, `Logs/`, `UserSettings/`, builds, secrets or local credentials.

## Working in parallel: four accounts, three agent tools
- **One task → one branch → one PR**. Keep unrelated cleanup out. Coordinate edits to shared entry points (`GameRoot`, `AppState`, saves, packages, scenes, project settings).
- The Issue defines the contract; the PR summarizes implementation and evidence. Do not copy the whole Issue into new markdown planning files.
- Use **this single `AGENTS.md`** across Codex, Claude Code and Kiro. Avoid parallel rulebooks unless tool compatibility requires a thin pointer to this file.
- Update the authoritative docs **in the same PR** when product or architecture behavior changes. Do not maintain historical alternative game specs.
- Code, identifiers and technical comments in English; player-facing text and GDD in pt-BR.

## Verification and evidence
- For pure sim changes, run `tools/simtest.sh` (needs .NET SDK 8+) and relevant deterministic tests. Do not present the fast harness as Unity validation.
- For Unity-affecting changes, run `tools/run-tests.sh EditMode` and `tools/run-tests.sh PlayMode` with `UNITY_EDITOR` pointing to the installed editor if available. Validate scene wiring, runtime and logs when applicable.
- For WebGL changes, build and **actually launch** the build before claiming browser validation.
- A missing editor, test harness or credentials is **not a pass**. In the PR state exactly: executed/passed, executed/failed, or not run (with reason).
- Test boundary conditions: empty roles/roster, no valid stations, corrupt save, insufficient funds, bankrupt state, repeated opening/skipping of days. No claims of success based only on compilation.
- Do not claim visual quality from source review; inspect rendered frames and animation where available.

## Independent agent review, no human code-approval gate
1. The implementation agent runs available checks and opens a PR.
2. **A separate agent/session** that did not implement the change reviews the **actual PR diff**, relevant tests, Unity wiring, correctness and scope. Prefer a different tool/model (Codex ↔ Claude Code ↔ Kiro). Record findings with severity: blocking / important / optional, plus reproducible evidence.
3. The implementing agent corrects blocking and important findings, reruns checks, and requests a fresh independent review of the new head commit.
4. Do **not** mark an agent review complete by copying a checklist or reviewing one's own patch. The PR must link to the second review or state explicitly that it has not happened.
5. No human line-by-line review or GitHub approval is required by this workflow. Human intervention is reserved for unresolved **product choices**, credentials or external publishing, not routine code approval.
6. **Do not enable unattended merging until CI requirements and a reliable independent review gate have actually been implemented and verified.** The review protocol in this file is policy, not an automatic enforcement mechanism. The CI and automatic merge implementation is a separate phase.

## Code Review Rules
Prioritize real regressions over style:
- Wrong sim/UI authority, nondeterministic calculations and economy/save invariants.
- Missing runtime/scene references, Unity lifecycle and cleanup problems, build/import issues.
- Invalid state transitions and ways to become permanently stuck.
- Documentation claiming features or tests that do not match implementation.
- Security of CI/workflow modifications, credential leaks and overly broad token permissions.
- Scope creep or changed product behavior without a task decision.
Avoid cosmetic refactors and "LGTM" without inspecting evidence.
