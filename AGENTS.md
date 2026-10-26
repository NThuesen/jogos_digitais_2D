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

## Independent agent review and GitHub-native merge gate
1. The implementation agent opens one PR and records actual verification, failures and missing Unity/visual evidence. No agent may claim CI success from a local test alone.
2. A **different GitHub collaborator account**, operating an independent Codex, Claude Code or Kiro session, reads the **current PR diff**, task contract and relevant sources/tests. Prioritize correctness, save/economy invariants, scene wiring and test tampering, not style.
3. The reviewing agent writes an actual GitHub PR review with reproducible **blocking / important / optional** findings. Only if no blocking or important problems remain may that agent submit GitHub **APPROVE** under the reviewing collaborator account. Do not rubber-stamp, impersonate reviewers or approve your own PR.
4. The implementation agent fixes blocking/important findings and pushes changes; the reviewer must examine the **new HEAD** before approving. GitHub's **Dismiss stale approvals** protection should invalidate old approvals automatically.
5. Humans choose tasks, resolve product decisions and own accounts; **no human line-by-line code review is required**. An approval is a real GitHub review event under another account, performed by that account's AI agent, **not a manually entered checklist field**. GitHub cannot attest which model wrote the review; authenticity of AI work remains a team practice.
6. Required safeguards on `main`: PR-only, **one GitHub approval by a different account**, stale approvals dismissed, required `simulation-and-boundaries` check, force-push and deletion blocked. The previous zero-approval ruleset does **not** enforce independent review.
7. After those safeguards are **confirmed active** and repository **Allow auto-merge** is enabled, the PR author's agent can request `gh pr merge --auto --squash` or GitHub's Auto-merge. GitHub then waits for checks and the independent approval; **no status polling or recurring AI calls**. Never request automatic merging when the ruleset lacks the independent approval gate.
8. This is a GitHub-native approval gate, **not a guarantee of semantic correctness**; Unity PlayMode, rendered UI and WebGL still require the evidence explicitly relevant to the task.

## CI and test-quality discipline
- GitHub Actions runs only on PR events and pushes to main. **Never create scheduled jobs, loops, agents or timers that poll CI results.** GitHub checks carry the result; look at it once when the event finishes or when next acting on that PR.
- CI uses one fast deterministic NUnit simulation suite plus an independent boundary/nonempty-results check. Keep the number of tests proportional to risks: no test-count targets, duplicated assertions, snapshots of incidental implementation details or tests that simply mirror production logic.
- Write behavioral tests that would fail for a real regression (wrong lineup, bankrupt balance, invalid state, malformed save), include counterexamples/edge cases and stable seeds. Avoid brittle tests that only check method names, documentation keywords or internal class shapes unless the contract explicitly concerns those.
- **Never change production logic, expected test results, skip flags, test runner, CI workflow or fixtures simply to make a red CI check turn green.** Diagnose the defect; legitimate test changes must explain why the expected behavior changed and demonstrate an assertion that fails on the broken case.
- Changing tests or CI is higher risk than changing a regular component: independent reviewer must compare these files with the actual gameplay contract, not accept 'all green' as proof.
- No AI model tokens for watching test logs. Run the command, wait for its process to exit, then read the final exit code and concise failure output once. Do not run a continuous GitHub status polling agent.
- GitHub Actions does **not** run Unity PlayMode or browser/UI verification in this phase. The CI success badge means only the explicitly named simulation/boundary checks passed.
- A green CI alone is not a product decision or an independent AI review. The one-shot PR review remains a separate task. Do not enable unattended merge without a verified review gate and working required checks.
## Code Review Rules
Prioritize real regressions over style:
- Wrong sim/UI authority, nondeterministic calculations and economy/save invariants.
- Missing runtime/scene references, Unity lifecycle and cleanup problems, build/import issues.
- Invalid state transitions and ways to become permanently stuck.
- Documentation claiming features or tests that do not match implementation.
- Security of CI/workflow modifications, credential leaks and overly broad token permissions.
- Scope creep or changed product behavior without a task decision.
Avoid cosmetic refactors and "LGTM" without inspecting evidence.
