#!/usr/bin/env bash
# Runs every play style, with and without investment, (the roster has no level cap since ROSTER-UNCAP-01).
cd "$(dirname "$0")"
for cap in base; do
  dotnet build -nologo -v quiet -clp:ErrorsOnly -p:CapMode=$cap -o ../.econ-bin-$cap EconBalance.csproj || exit 1
  for style in idle lazy casual pro; do
    for inv in yes no; do
      echo "[cap=$cap]"; dotnet ../.econ-bin-$cap/EconBalance.dll ${1:-40} ${2:-40} $style $inv
    done
  done
done
