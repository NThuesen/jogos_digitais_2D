#!/usr/bin/env bash
# Runs every play style, with and without investment, for both bot hiring policies:
# ECON_BOT_CAP=old reproduces the pre-ECON-03 hiring limit (isolates the wage rule), none hires without a limit.
cd "$(dirname "$0")"
dotnet build -nologo -v quiet -clp:ErrorsOnly -o ../.econ-bin EconBalance.csproj || exit 1
for bots in old none; do
  for style in idle lazy casual pro; do
    for inv in yes no; do
      echo "[bots=$bots]"; ECON_BOT_CAP=$bots dotnet ../.econ-bin/EconBalance.dll ${1:-40} ${2:-40} $style $inv
    done
  done
done
