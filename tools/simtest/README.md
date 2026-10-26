# Fast simulation test harness

This harness compiles `Neko.Sim` and **only** `Assets/_Game/Tests/EditMode/Sim` with NUnitLite, independently of the Unity Editor. It is a fast developer check, **not** the release/test source of truth.

## Prerequisites
- .NET SDK **8 or newer** on PATH, or set `DOTNET` to the `dotnet` executable.
- Access to NuGet on the first restore (NUnit 3.14.0 and NUnitLite 3.14.0). There is no local credential or machine-specific path to another repository.

## Run
```sh
tools/simtest.sh
tools/simtest.sh --where "test =~ /Shift/"
```

`SimTest.csproj` is intentionally committed despite Unity-generated `*.csproj` files being ignored. It links the same simulation and test source files used in the Unity project. `LangVersion` is C# 9. This command exits nonzero when build or tests fail.

For actual Unity compilation/lifecycle verification, set `UNITY_EDITOR` and run `tools/run-tests.sh EditMode` and `tools/run-tests.sh PlayMode`. Both require a separately installed Unity 6000.5.9f1 Editor. Nothing here implies that those checks have been run.
