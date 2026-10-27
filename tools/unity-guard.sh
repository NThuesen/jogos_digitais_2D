#!/usr/bin/env bash
# Shared, fail-closed guard for commands that open Unity or alter Unity caches.
# Source this script; caller must define PROJECT (absolute project root).
# The mkdir is atomic and serializes agent-invoked operations in this clone.
# External Hub sessions are detected by their actual process argv.
UNITY_TOOLS_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
UNITY_GUARD_DIR=""

unity_guard_release() {
  if [[ -n "$UNITY_GUARD_DIR" && -d "$UNITY_GUARD_DIR" ]]; then
    rm -rf -- "$UNITY_GUARD_DIR"
  fi
}

unity_guard_prepare() {
  if ! command -v python3 >/dev/null 2>&1; then
    echo "ERROR: Python 3 is required to validate Unity runs and detect Editor processes." >&2
    return 2
  fi
  mkdir -p "$PROJECT/Logs"
  UNITY_GUARD_DIR="$PROJECT/Logs/.unity-agent-operation.lock"
  if ! mkdir "$UNITY_GUARD_DIR" 2>/dev/null; then
    echo "ERROR: Another Unity operation (or stale agent lock) exists: $UNITY_GUARD_DIR" >&2
    echo "If crashed, confirm no Unity Editor is running before removing this agent lock." >&2
    return 1
  fi
  printf 'pid=%s\n' "$$" > "$UNITY_GUARD_DIR/owner"
  trap 'unity_guard_release' EXIT
  trap 'exit 130' INT
  trap 'exit 143' TERM

  # Never remove Unity's own lock while its Editor is running.
  python3 "$UNITY_TOOLS_DIR/unity-editor-processes.py" || return $?
  if [[ -f "$PROJECT/Temp/UnityLockfile" ]]; then
    echo "Removing stale UnityLockfile after verifying no Unity Editor is running."
    rm -f -- "$PROJECT/Temp/UnityLockfile"
  fi
}
