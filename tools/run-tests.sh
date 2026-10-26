#!/usr/bin/env bash
# Portable Unity batch-mode tests; no sibling agent_creater repository required.
# Usage: UNITY_EDITOR=/path/to/Unity tools/run-tests.sh EditMode [testFilter]
set -euo pipefail

PLATFORM="${1:-EditMode}"
FILTER="${2:-}"
case "$PLATFORM" in
  EditMode|PlayMode) ;;
  *) echo "Expected EditMode or PlayMode, got: $PLATFORM" >&2; exit 2 ;;
esac

PROJECT="$(cd "$(dirname "$0")/.." && pwd)"
mkdir -p "$PROJECT/Logs"
RESULTS="$PROJECT/Logs/tests-$PLATFORM.xml"
LOG="$PROJECT/Logs/tests-$PLATFORM.log"
rm -f "$RESULTS"

if [[ -n "${UNITY_EDITOR:-}" ]]; then
  EDITOR_BIN="$UNITY_EDITOR"
elif command -v Unity >/dev/null 2>&1; then
  EDITOR_BIN="$(command -v Unity)"
elif command -v unity-editor >/dev/null 2>&1; then
  EDITOR_BIN="$(command -v unity-editor)"
else
  echo "ERROR: Unity Editor not found. Set UNITY_EDITOR to Unity 6000.5.9f1 executable." >&2
  exit 2
fi

args=(-batchmode -quit -projectPath "$PROJECT" -runTests -testPlatform "$PLATFORM" -testResults "$RESULTS" -logFile "$LOG")
if [[ -n "$FILTER" ]]; then args+=(-testFilter "$FILTER"); fi

"$EDITOR_BIN" "${args[@]}"

if [[ ! -s "$RESULTS" ]]; then
  echo "ERROR: Unity returned without a test-results XML file. See $LOG" >&2
  exit 1
fi
if grep -Eq 'result="(Failed|Error)"|failed="[1-9][0-9]*"' "$RESULTS"; then
  echo "ERROR: Unity reported failing tests. See $RESULTS and $LOG" >&2
  exit 1
fi
echo "Unity $PLATFORM results: $RESULTS"
