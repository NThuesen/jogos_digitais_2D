#!/usr/bin/env bash
# Portable fast harness for pure Neko.Sim (does not validate Unity rendering/lifecycle).
# Requires .NET SDK 8+ and NuGet access for first restore.
set -euo pipefail

HERE="$(cd "$(dirname "$0")" && pwd)"
PROJECT="$HERE/simtest/SimTest.csproj"
OUT="$HERE/.simtest-bin"

if [[ -n "${DOTNET:-}" ]]; then
  DOTNET_BIN="$DOTNET"
elif command -v dotnet >/dev/null 2>&1; then
  DOTNET_BIN="$(command -v dotnet)"
else
  echo "ERROR: .NET SDK 8+ not found. Install dotnet or set DOTNET=/path/to/dotnet." >&2
  exit 2
fi

"$DOTNET_BIN" build "$PROJECT" --nologo -v quiet -clp:ErrorsOnly -o "$OUT"
exec "$DOTNET_BIN" "$OUT/SimTest.dll" "$@"
