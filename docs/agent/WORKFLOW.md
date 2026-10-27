# Branch → PR → merge

**Read when:** starting a Git task, preparing a PR, or resolving a GitHub gate. Baseline branch safety is in [AGENTS.md](../../AGENTS.md).

## Start and isolate
- Inspect `git status --short`, current branch, and the relevant task before touching files.
- Work in one task-specific branch. If on `main`, detached HEAD, or an unrelated branch, create/switch automatically first. Do not erase, stash, or silently incorporate unrelated local edits. If cleanly switching is impossible, report the blocker instead of modifying `main`.
- One scoped task → one branch → one PR. Coordinate shared entry points; no unrelated refactors or generated planning documents.
- The task/Issue defines scope and acceptance criteria. Changed design rules belong in `docs/GDD.md`; technical boundaries in `docs/ARCHITECTURE.md`.

## PR delivery
- Review the actual diff, risks, and acceptance criteria. Use [the PR template](../../.github/PULL_REQUEST_TEMPLATE.md), documenting checks as **passed / failed / not run**, with reasons.
- GitHub Actions runs on events. **Never poll** status periodically or ask another teammate to activate a reviewing AI.
- Normal PRs may request GitHub-native auto-merge **once**, after confirming the ruleset requires `simulation-and-boundaries` and `protected-change-guard` and no human approvals. The GitHub server waits for the checks; the agent does not.
- Existing test files, CI/workflows, build/test runners, `Packages/`, `ProjectSettings/`, and Unity scenes are guarded. A failed `protected-change-guard` is **not** a reason to weaken it. Legitimate changes need an explicit repository-owner maintenance exception and independent evidence; do not auto-merge them.
- Owner authorization is mandatory for credentials/secrets, paid services, material product changes, public releases (including itch.io), and destructive operations. No routine human line-by-line code review is required.
