#!/usr/bin/env bash
# Runs a Unity test platform in batch mode through the workspace's unity MCP server.
#
#   tools/run-tests.sh EditMode
#   tools/run-tests.sh PlayMode 'Neko.Tests.PlayMode.BootTests'
#
# The sim tests also run outside Unity in a tenth of a second (tools/simtest.sh); this is
# the one that counts, because it is the compiler and the runtime the game actually ships on.
set -euo pipefail

PLATFORM="${1:-EditMode}"
FILTER="${2:-}"
PROJECT="$(cd "$(dirname "$0")/.." && pwd)"
CALL="$PROJECT/../agent_creater/tools/mcp/scripts/call.mjs"

if pgrep -fa Unity | grep -v pgrep | grep -q .; then
  echo "A Unity process is already running; this machine runs one at a time." >&2
  pgrep -fa Unity | grep -v pgrep >&2
  exit 1
fi

if [ -f "$PROJECT/Temp/UnityLockfile" ]; then
  echo "Removing a stale Temp/UnityLockfile (no editor is alive)." >&2
  rm -f "$PROJECT/Temp/UnityLockfile"
fi

ARGS="{\"projectPath\":\"$PROJECT\",\"platform\":\"$PLATFORM\""
[ -n "$FILTER" ] && ARGS="$ARGS,\"testFilter\":\"$FILTER\""
ARGS="$ARGS}"

exec node "$CALL" --timeout 1800 unity unity_run_tests "$ARGS"
