#!/usr/bin/env bash
# Unity batch-mode test runner. Unity Test Framework exits after tests; DO NOT pass -quit.
# Usage: UNITY_EDITOR=/path/to/Unity tools/run-tests.sh EditMode [testFilter]
set -euo pipefail

PLATFORM="${1:-EditMode}"
FILTER="${2:-}"
case "$PLATFORM" in
  EditMode|PlayMode) ;;
  *) echo "ERROR: expected EditMode or PlayMode, got: $PLATFORM" >&2; exit 2 ;;
esac
if (( $# > 2 )); then
  echo "ERROR: usage: tools/run-tests.sh EditMode|PlayMode [testFilter]" >&2
  exit 2
fi

PROJECT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
source "$PROJECT/tools/unity-guard.sh"

if [[ -n "${UNITY_EDITOR:-}" ]]; then
  EDITOR_BIN="$UNITY_EDITOR"
elif command -v Unity >/dev/null 2>&1; then
  EDITOR_BIN="$(command -v Unity)"
elif command -v unity-editor >/dev/null 2>&1; then
  EDITOR_BIN="$(command -v unity-editor)"
else
  echo "ERROR: Unity Editor not found; set UNITY_EDITOR to the Unity 6000.5.9f1 executable." >&2
  exit 2
fi

# Lock and editor-process checks happen before touching the previous result file.
unity_guard_prepare

RESULTS="$PROJECT/Logs/tests-$PLATFORM.xml"
LOG="$PROJECT/Logs/tests-$PLATFORM.log"
rm -f -- "$RESULTS"

args=(-batchmode -projectPath "$PROJECT" -runTests -testPlatform "$PLATFORM" -testResults "$RESULTS" -logFile "$LOG")
if [[ -n "$FILTER" ]]; then args+=(-testFilter "$FILTER"); fi

status=0
"$EDITOR_BIN" "${args[@]}" || status=$?
if (( status != 0 )); then
  echo "ERROR: Unity exited with status $status. See $LOG" >&2
  exit "$status"
fi
if ! python3 "$PROJECT/tools/validate-unity-results.py" "$RESULTS"; then
  echo "ERROR: Unity $PLATFORM test results invalid; see $RESULTS and $LOG" >&2
  exit 1
fi
echo "Unity $PLATFORM result: $RESULTS"
