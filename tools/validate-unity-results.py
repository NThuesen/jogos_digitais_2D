#!/usr/bin/env python3
"""Fail-closed verification of Unity Test Framework NUnit 3 XML."""
import sys
from pathlib import Path
import xml.etree.ElementTree as ET


def validate(path: Path) -> str:
    if not path.is_file() or path.stat().st_size == 0:
        raise ValueError(f"Unity did not produce a nonempty test-results XML file: {path}")
    try:
        root = ET.parse(path).getroot()
    except ET.ParseError as exc:
        raise ValueError(f"Malformed Unity test-results XML: {exc}") from exc
    if root.tag != "test-run":
        raise ValueError(f"Unexpected XML root {root.tag!r}; expected NUnit 3 <test-run>")

    counts = {}
    for name in ("total", "passed", "failed", "skipped", "inconclusive"):
        try:
            counts[name] = int(root.attrib[name])
        except (KeyError, ValueError) as exc:
            raise ValueError(f"Invalid or missing NUnit attribute {name!r}") from exc
        if counts[name] < 0:
            raise ValueError(f"Negative NUnit {name} count")

    if counts["total"] == 0:
        raise ValueError("Unity selected zero tests; an empty filter must not count as a pass")
    if root.get("result") != "Passed":
        raise ValueError(f"Unity test-run result is {root.get('result')!r}, not Passed")
    if counts["passed"] != counts["total"] or any(
        counts[name] != 0 for name in ("failed", "skipped", "inconclusive")
    ):
        raise ValueError(f"Not all Unity tests passed: {counts}")

    cases = root.findall(".//test-case")
    if len(cases) != counts["total"]:
        raise ValueError(
            f"NUnit reported {counts['total']} executed tests but contains {len(cases)} test-case results"
        )
    unsuccessful = [
        case.get("fullname", case.get("name", "<unnamed>")) + ": " + str(case.get("result"))
        for case in cases if case.get("result") != "Passed"
    ]
    if unsuccessful:
        raise ValueError("Nonpassing Unity test cases: " + "; ".join(unsuccessful[:5]))
    return f"{counts['passed']} Unity tests passed; 0 failed/skipped/inconclusive"


if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Usage: validate-unity-results.py <Unity NUnit3 XML>", file=sys.stderr)
        sys.exit(2)
    try:
        print(validate(Path(sys.argv[1])))
    except (ValueError, OSError) as exc:
        print(f"ERROR: {exc}", file=sys.stderr)
        sys.exit(1)
