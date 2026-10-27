#!/usr/bin/env python3
"""Focused regression tests for the Unity command wrapper; no Unity installation needed."""
import os
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile
import unittest

TOOLS = Path(__file__).resolve().parents[1]
NAMES = ("run-tests.sh", "unity-guard.sh", "unity-editor-processes.py",
         "validate-unity-results.py")


def xml(total=1, passed=1, failed=0, skipped=0, inconclusive=0, result="Passed",
        case_result="Passed"):
    cases = "".join(f'<test-case name="case-{i}" result="{case_result}" />'
                    for i in range(total))
    return (f'<test-run total="{total}" passed="{passed}" failed="{failed}" '
            f'skipped="{skipped}" inconclusive="{inconclusive}" result="{result}">'
            f'{cases}</test-run>')


class UnityValidationContract(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        dest = self.root / "tools"
        dest.mkdir()
        for name in NAMES:
            shutil.copy2(TOOLS / name, dest / name)
        self.editor = self.root / "fake-editor"
        self.editor.write_text(
            '#!/usr/bin/env bash\n'
            'set -euo pipefail\n'
            'printf "%s\\n" "$@" > "$MOCK_ARGS"\n'
            'result=""\n'
            'while (( $# )); do\n'
            '  if [[ "$1" == "-testResults" ]]; then shift; result="$1"; fi\n'
            '  shift\n'
            'done\n'
            'if [[ "${MOCK_WRITE_XML:-1}" == "1" ]]; then\n'
            '  printf "%s" "$MOCK_XML" > "$result"\n'
            'fi\n'
            'exit "${MOCK_EXIT:-0}"\n'
        )
        self.editor.chmod(0o755)
        self.args_path = self.root / "arguments.txt"

    def run_mock(self, document=None, write=True, code=0, extra_args=()):
        env = dict(os.environ, UNITY_EDITOR=str(self.editor),
                   MOCK_ARGS=str(self.args_path), MOCK_XML=document or xml(),
                   MOCK_WRITE_XML="1" if write else "0", MOCK_EXIT=str(code))
        return subprocess.run(
            ["bash", str(self.root / "tools/run-tests.sh"), "EditMode", *extra_args],
            env=env, text=True, capture_output=True, timeout=20
        )

    def test_success_runs_with_correct_args_and_without_quit(self):
        result = self.run_mock()
        self.assertEqual(result.returncode, 0, result.stderr)
        args = self.args_path.read_text().splitlines()
        self.assertIn("-runTests", args)
        self.assertIn("-testPlatform", args)
        self.assertIn("EditMode", args)
        self.assertNotIn("-quit", args)
        self.assertFalse((self.root / "Logs/.unity-agent-operation.lock").exists())

    def test_empty_filtered_run_fails(self):
        result = self.run_mock(xml(total=0, passed=0), extra_args=("NoSuchTest",))
        self.assertNotEqual(result.returncode, 0)
        self.assertIn("zero tests", result.stderr)

    def test_failed_skipped_and_inconclusive_results_fail(self):
        bad = [
            xml(total=1, passed=0, failed=1, result="Failed", case_result="Failed"),
            xml(total=1, passed=0, skipped=1, result="Skipped", case_result="Skipped"),
            xml(total=1, passed=0, inconclusive=1, result="Inconclusive",
                case_result="Inconclusive"),
        ]
        for document in bad:
            with self.subTest(document=document):
                self.assertNotEqual(self.run_mock(document).returncode, 0)

    def test_missing_or_malformed_xml_fails(self):
        self.assertNotEqual(self.run_mock(write=False).returncode, 0)
        self.assertNotEqual(self.run_mock("<truncated").returncode, 0)

    def test_nonzero_editor_status_is_not_masked(self):
        self.assertEqual(self.run_mock(code=7).returncode, 7)

    def test_busy_agent_lock_refuses_before_touching_results(self):
        lock = self.root / "Logs/.unity-agent-operation.lock"
        lock.mkdir(parents=True)
        output = self.root / "Logs/tests-EditMode.xml"
        output.write_text("previous-result-must-survive")
        result = self.run_mock()
        self.assertNotEqual(result.returncode, 0)
        self.assertEqual(output.read_text(), "previous-result-must-survive")

    @unittest.skipUnless(sys.platform.startswith("linux"), "Linux /proc test")
    def test_detects_real_editor_argv_even_with_different_process_name(self):
        fake_unity = self.root / "Unity"
        fake_unity.symlink_to(sys.executable)
        proc = subprocess.Popen([str(fake_unity), "-c", "import time; time.sleep(10)"])
        try:
            result = self.run_mock()
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("Unity Editor is already running", result.stderr)
        finally:
            proc.terminate()
            proc.wait(timeout=5)


if __name__ == "__main__":
    unittest.main()
