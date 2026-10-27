#!/usr/bin/env python3
"""Detect real Unity Editor processes (Linux argv / macOS ps / Windows tasklist).

Unlike `pgrep -x Unity`, reading argv is not affected by Linux's truncated
"Unity Main Thre" thread name. Exit 0 if no Editor, 1 if any, 2 if unknown.
"""
import csv
import io
import os
import pathlib
import subprocess
import sys


def is_editor(executable: str) -> bool:
    name = executable.replace("\\", "/").rstrip("/").split("/")[-1].lower()
    return name in {"unity", "unity.exe"}


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
                found.append(int(pid_dir.name))
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
                found.append(int(fields[0]))
    return found


def editors_windows():
    output = subprocess.check_output(
        ["tasklist.exe", "/FI", "IMAGENAME eq Unity.exe", "/FO", "CSV", "/NH"],
        text=True, errors="replace"
    )
    found = []
    for cells in csv.reader(io.StringIO(output)):
        if len(cells) >= 2 and cells[0].lower() == "unity.exe" and cells[1].isdigit():
            found.append(int(cells[1]))
    return found


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
    if found:
        print(f"ERROR: Unity Editor is already running (PID(s) {', '.join(map(str, found))}). "
              "Close it before running another Editor operation.", file=sys.stderr)
        sys.exit(1)
