#!/usr/bin/env python3
"""Small independent CI guard for Neko.Sim, using only the Python standard library.

This is not a game-design or code-review substitute. It prevents accidentally disabling
the simulation boundary or running an empty NUnit harness. No test-count quotas.
"""
from __future__ import annotations

import json
from pathlib import Path
import re
import sys
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
SIM = ROOT / "Assets/_Game/Scripts/Sim"
TESTS = ROOT / "Assets/_Game/Tests/EditMode/Sim"
ASMDEF = SIM / "Neko.Sim.asmdef"
PROJECT = ROOT / "tools/simtest/SimTest.csproj"


def fail(message: str) -> None:
    print(f"ERROR: {message}", file=sys.stderr)
    raise SystemExit(1)


def boundaries() -> None:
    config = json.loads(ASMDEF.read_text(encoding="utf-8"))
    if config.get("noEngineReferences") is not True:
        fail("Neko.Sim.asmdef must keep noEngineReferences=true")
    if config.get("references"):
        fail("Neko.Sim.asmdef gained assembly references; inspect the boundary")

    sources = list(SIM.rglob("*.cs"))
    tests = list(TESTS.rglob("*.cs"))
    if not sources or not tests:
        fail("Simulation sources or NUnit test sources are missing")
    # This is a cheap guard against accidentally importing Unity APIs into pure sim.
    # The Unity compilation and existing boundary tests are still authoritative.
    forbidden = re.compile(r"\b(?:using\s+(?:static\s+)?(?:UnityEngine|UnityEditor)\b|(?:UnityEngine|UnityEditor)\s*\.)")
    comments = re.compile(r"//[^\n]*|/\*.*?\*/", re.DOTALL)
    contaminated = [
        str(p.relative_to(ROOT)) for p in sources
        if forbidden.search(comments.sub("", p.read_text(encoding="utf-8")))
    ]
    if contaminated:
        fail("Unity-specific code entered pure simulation: " + ", ".join(contaminated))

    xml = PROJECT.read_text(encoding="utf-8")
    required = [
        "../../Assets/_Game/Scripts/Sim/**/*.cs",
        "../../Assets/_Game/Tests/EditMode/Sim/**/*.cs",
        'PackageReference Include="NUnitLite"',
    ]
    for marker in required:
        if marker not in xml:
            fail("Standalone test project must compile all sim and NUnit tests: " + marker)
    print(f"Simulation boundary OK ({len(sources)} sources, {len(tests)} test files; no quotas).")


def results(path: Path) -> None:
    if not path.is_file():
        fail(f"NUnit results file not produced: {path}")
    root = ET.parse(path).getroot()
    # NUnitLite writes NUnit 3 XML with individual test-case nodes.
    cases = root.findall(".//test-case")
    if not cases:
        fail("NUnit completed without executing any test cases")
    passed = sum(1 for case in cases if case.get("result") == "Passed")
    failed = sum(1 for case in cases if case.get("result") in ("Failed", "Error"))
    if failed or passed == 0:
        fail(f"Tests did not pass: passed={passed}, failed={failed}, total={len(cases)}")
    print(f"NUnit result verified: {passed} passed, {len(cases) - passed} skipped/inconclusive.")
    print("Passing tests do not validate Unity PlayMode, WebGL, visual feedback or product quality.")


if __name__ == "__main__":
    if len(sys.argv) == 2 and sys.argv[1] == "boundaries":
        boundaries()
    elif len(sys.argv) == 3 and sys.argv[1] == "results":
        results(Path(sys.argv[2]))
    else:
        fail("Usage: verify.py boundaries | verify.py results <NUnit XML>")
