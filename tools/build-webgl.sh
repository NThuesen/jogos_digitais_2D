#!/usr/bin/env bash
#
# Builds the WebGL player into Build/WebGL, which is what goes on itch.io.
#
# Runs the editor in batch mode against Neko.Game.Editor.WebGLBuilder.Build, which applies
# the build settings itself — so the delivered build is reproducible from this one command
# and the choices are visible in a diff rather than buried in a YAML asset.
#
set -euo pipefail

PROJECT="$(cd "$(dirname "$0")/.." && pwd)"
EDITOR="$HOME/Unity/Hub/Editor/6000.5.9f1/Editor/Unity"
LOG="$PROJECT/.build.log"

if pgrep -x Unity > /dev/null; then
  echo "A Unity editor is already running; this machine runs one at a time." >&2
  exit 1
fi

rm -rf "$PROJECT/Build/WebGL"

"$EDITOR" \
  -batchmode -nographics \
  -projectPath "$PROJECT" \
  -executeMethod Neko.Game.Editor.WebGLBuilder.Build \
  -logFile "$LOG" || true

if [ -f "$PROJECT/Build/WebGL/index.html" ]; then
  echo "OK  Build/WebGL/index.html"
  du -sh "$PROJECT/Build/WebGL"
else
  echo "FAILED — last 40 lines of $LOG:" >&2
  tail -40 "$LOG" >&2
  exit 1
fi
