#!/usr/bin/env bash
# Runs the Neko.Sim tests outside Unity. See tools/simtest/README.md.
set -euo pipefail
DOTNET="$HOME/Unity/Hub/Editor/6000.5.9f1/Editor/Data/DotNetSdk/dotnet"
HERE="$(cd "$(dirname "$0")" && pwd)"
cd "$HERE/simtest"
"$DOTNET" build --nologo -v quiet -clp:ErrorsOnly -o "$HERE/.simtest-bin" SimTest.csproj
exec "$DOTNET" "$HERE/.simtest-bin/SimTest.dll" "$@"
