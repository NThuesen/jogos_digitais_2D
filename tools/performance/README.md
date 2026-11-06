# WebGL performance evidence

Run from the repository root. Do not run competing Editors; use the existing Unity guard.

- `python3 tools/performance/inventory.py --build Build/WebGL --output Logs/performance-files.json`: actual PNG dimensions, SHA-256 and per-file deployment bytes (excludes DoNotShip debug data).
- Unity batch execute method `Neko.Game.Editor.PerformanceAudit.Capture`: imported walk texture dimensions, format, mip levels, readability and native object bytes. Output `Logs/performance-textures.json`. Use the same graphics mode on both sides; null-device bytes are not GPU residency.
- `Neko.Game.Editor.PerformanceAudit.BenchmarkCustomers`: warm one Ordering customer, time 10,000 calls to its actual Sync method. Output `Logs/performance-customers.json`. This microbenchmark does not measure whole-game FPS. A zero allocation counter on this Editor is not evidence of zero allocations.
- `Neko.Game.Editor.PerformanceAudit.ExportBuildReport`: read `Library/LastBuild.buildreport`, write resource sizes as TSV and a summary in Logs. Packed asset bytes are not browser residency.
- `Neko.Game.Editor.PerformanceAudit.BuildBaseline`: same builder settings without walk padding/compression subtarget. **Use original baseline runtime sources and importer settings.** This helper does not check out or restore Git files.

Normal `tools/build-webgl.sh` uses `WebGLBuilder.Build`. Its walk preparation saves source PNG bytes under `Library/WebGLWalkTextureBackup`, pads only the top/right transparently, imports, builds, and restores the originals. Do not edit processed walk PNGs during a build. If Unity is killed, first verify no Editor is running, restore each backup PNG to `Assets/_Game/Resources/Maids/`, then remove the empty backup directory and reimport. Never discard the backup without restoring its contents. The next build refuses to proceed with an outstanding backup.

The full audit, limitations, browser samples and decisions are in `docs/performance/OPTIMIZATION_REPORT.md`. Unity tests still run against the original source grids; final WebGL smoke checks must verify the padded compressed payload too.

For controlled comparisons, clear both HTTP cache and Unity IndexedDB in an isolated browser profile before each load. Keep the machine idle while sampling. Record console errors and texture upload formats separately from frame timing. Never compare a null-device inventory with a real-driver native-memory inventory as if it measured the same quantity.

Browser capture (Node 22+, no npm dependencies): launch an isolated Chrome with `--remote-debugging-port=9223 --user-data-dir=<temporary-directory>` and serve the builds locally. Then run `node tools/performance/browser.cjs <label> <full-build-url>`. The tool clears storage **only in that Chrome profile for the supplied origin**, opens F8, captures two successive animation frames at 1280×720 and 960×600 and writes JSON/screenshots to `docs/performance/`. Do not run two capture processes against the same tab. It instruments `texStorage2D`/`compressedTexImage2D` to confirm GPU format requests; this is not a VRAM profiler. Run both sides with the same instrumentation and no Editor/build running. JS heap and rAF timing are samples, not C# heap or CPU/GPU timing.

For JS heap comparisons, restart the isolated Chrome process between builds. In this audit, navigation alone retained prior Unity contexts and inflated `performance.memory` despite explicit DevTools garbage collection. The capture also forces GC before heap samples; it does not characterize natural GC pauses.
