#!/usr/bin/env python3
"""Detect real Unity Editor processes (Linux argv / macOS ps / Windows tasklist).

Unlike `pgrep -x Unity`, reading argv is not affected by Linux's truncated
"Unity Main Thre" thread name. Exit 0 if no Editor, 1 if any, 2 if unknown.
"""
import csv
import io
import json
import os
import pathlib
import re
import subprocess
import sys


def is_editor(executable: str) -> bool:
    name = executable.replace("\\", "/").rstrip("/").split("/")[-1].lower()
    return name in {"unity", "unity.exe", "unity-editor"}


EDITOR_FLAGS = {"-projectpath", "-batchmode", "-executemethod", "-createproject",
                "-runtests", "-quit", "-logfile", "-nographics"}
EDITOR = "editor"
MCP = "mcp"
AMBIGUOUS = "ambiguous"


def tokenize(command_line: str):
    """Split a command line into tokens, honouring double quotes (Windows style)."""
    return [a or b for a, b in re.findall(r'"([^"]*)"|(\S+)', command_line)]


def classify(command_line):
    """Classify one Unity-named process by its FULL command line.

    MCP: the Unity CLI's MCP server, i.e. `unity mcp [...]` with no Editor flags. It never
         opens the project, so it must not block Editor operations.
    AMBIGUOUS: the command line is missing/unreadable. Treated as blocking (fail-closed).
    EDITOR: anything else, including every real Editor launch.
    """
    if command_line is None or not command_line.strip():
        return AMBIGUOUS
    tokens = tokenize(command_line)
    if not tokens:
        return AMBIGUOUS
    # The Editor binary lives in an ".../Editor/Unity.exe" install folder; the CLI does not.
    if re.search(r"[\\/]editor[\\/]unity(\.exe)?$", tokens[0].lower()):
        return EDITOR
    args = [t.lower() for t in tokens[1:]]
    if args and args[0] == "mcp" and not any(a in EDITOR_FLAGS for a in args):
        return MCP
    return EDITOR


def blocking(processes):
    """processes: iterable of (pid, command_line). Returns the pids that block an Editor run."""
    return [pid for pid, line in processes if classify(line) != MCP]


def editors_linux():
    proc = pathlib.Path("/proc")
    if not proc.is_dir():
        raise RuntimeError("/proc unavailable; cannot safely detect Unity Editor")
    found = []
    for pid_dir in proc.iterdir():
        if not pid_dir.name.isdigit() or int(pid_dir.name) == os.getpid():
            continue
        try:
            args = (pid_dir / "cmdline").read_bytes().split(b"\0")
            if args[0] and is_editor(args[0].decode("utf-8", errors="replace")):
                line = " ".join('"%s"' % a.decode("utf-8", errors="replace")
                                for a in args if a)
                found.append((int(pid_dir.name), line))
        except (FileNotFoundError, ProcessLookupError, PermissionError):
            # Short-lived process gone or not owned by this user.
            continue
    return found


def editors_macos():
    output = subprocess.check_output(["ps", "-axo", "pid=,command="], text=True)
    found = []
    for line in output.splitlines():
        fields = line.strip().split(None, 1)
        if len(fields) == 2 and fields[0].isdigit():
            executable = fields[1].split(None, 1)[0]
            if is_editor(executable):
                found.append((int(fields[0]), fields[1]))
    return found


def editors_windows():
    """(pid, full command line) of every Unity.exe; the line is None when unreadable."""
    script = ("Get-CimInstance Win32_Process -Filter \"Name='Unity.exe'\" | "
              "Select-Object ProcessId,CommandLine | ConvertTo-Json -Compress")
    output = subprocess.check_output(
        ["powershell.exe", "-NoProfile", "-NonInteractive", "-Command", script],
        text=True, errors="replace")
    output = output.strip()
    if not output:
        return []
    data = json.loads(output)
    if isinstance(data, dict):
        data = [data]
    return [(int(row["ProcessId"]), row.get("CommandLine")) for row in data]


if __name__ == "__main__":
    try:
        if sys.platform.startswith("linux"):
            found = editors_linux()
        elif sys.platform == "darwin":
            found = editors_macos()
        elif sys.platform in ("win32", "cygwin"):
            found = editors_windows()
        else:
            raise RuntimeError(f"unsupported platform {sys.platform}")
    except (RuntimeError, OSError, subprocess.SubprocessError) as exc:
        print(f"ERROR: Unity Editor process detection unavailable: {exc}", file=sys.stderr)
        sys.exit(2)
    found = blocking(found)
    if found:
        print(f"ERROR: Unity Editor is already running or cannot be told apart from one "
              f"(PID(s) {', '.join(map(str, found))}). "
              "Close it before running another Editor operation.", file=sys.stderr)
        sys.exit(1)
