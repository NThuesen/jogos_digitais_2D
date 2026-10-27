# Evidence and focused testing

**Read when:** selecting tests, checking a change, investigating a failing check, or preparing the PR evidence.

## Choose checks by impact
- Pure `Neko.Sim`: `tools/simtest.sh` (requires .NET SDK 8+); use deterministic scenarios, meaningful assertions and relevant edge cases.
- Unity integration, scene or gameplay: `tools/run-tests.sh EditMode` and `tools/run-tests.sh PlayMode` with `UNITY_EDITOR` pointing at 6000.5.9f1, plus runtime/Console/scene wiring checks as appropriate.
- WebGL/browser behavior: `tools/build-webgl.sh`, actually launch the build and inspect browser/visual behavior. Static source inspection is not a visual test.
- Pure docs/operational changes: inspect diff and references; CI is still run by GitHub, but Unity tests are not required unless the change affects Unity behavior.

## Trust the result, not the test count
- Keep tests proportional to regression risks; avoid count targets, duplicated assertions and incidental snapshots. New tests should fail for a real broken behavior.
- Never change production behavior, fixtures, expected results, skip flags or runners **only** to get a green check. Diagnose and report failures; maintainers handle legitimate protected validation changes separately.
- `simulation-and-boundaries` validates the .NET simulation and small runner contracts. `protected-change-guard` limits changes to verification/configuration. **Neither validates the actual Unity Editor, PlayMode graphics, scene wiring or WebGL.**
- Record executed command, pass/fail/not-run and reason; an unavailable tool is **not** a pass. Inspect the final process result/logs once, rather than spending tokens watching CI every few seconds.
- In self-review prioritize broken saves/economy, invalid state, Unity lifecycle, missing references, game-rule changes and security regressions. Distinguish implementation, compilation, runtime testing and verified behavior.
