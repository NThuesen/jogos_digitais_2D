# Fast loop for `Neko.Sim`

`Neko.Sim` deliberately has no `UnityEngine` reference, which means the whole simulation —
shift, maids, economy, save — compiles and runs outside the editor. This project points the
.NET SDK bundled with Unity at the same `.cs` files the editor compiles and runs the same
NUnit tests, in under a second instead of in a two-minute batch-mode editor launch.

```sh
tools/simtest.sh              # everything
tools/simtest.sh --where "test =~ /Shift/"
```

`LangVersion` is pinned to C# 9 so nothing that compiles here can fail in the editor.
This is a development convenience, not the source of truth: `tools/run-tests.sh EditMode`
runs the same tests inside Unity and is what a claim of "passing" has to mean.
