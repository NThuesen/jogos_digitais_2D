#!/usr/bin/env python3
"""Classification of Unity-named processes: MCP server vs real Editor vs ambiguous."""
import importlib.util
from pathlib import Path
import unittest

PATH = Path(__file__).resolve().parents[1] / "unity-editor-processes.py"
spec = importlib.util.spec_from_file_location("unity_editor_processes", PATH)
mod = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mod)

EDITOR_EXE = r'"C:\Program Files\Unity\Hub\Editor\6000.5.9f1\Editor\Unity.exe"'


class Classification(unittest.TestCase):
    def test_mcp_server_is_ignored(self):
        self.assertEqual(mod.classify("unity mcp"), mod.MCP)
        self.assertEqual(mod.blocking([(1, "unity mcp")]), [])

    def test_real_editor_blocks(self):
        for line in (EDITOR_EXE,
                     EDITOR_EXE + r' -projectPath C:\dev\game',
                     EDITOR_EXE + " -batchmode -quit",
                     "unity -projectPath x"):
            self.assertEqual(mod.classify(line), mod.EDITOR, line)
        self.assertEqual(mod.blocking([(7, EDITOR_EXE)]), [7])

    def test_mcp_word_with_editor_flags_or_editor_binary_still_blocks(self):
        self.assertEqual(mod.classify("unity mcp -projectPath x"), mod.EDITOR)
        self.assertEqual(mod.classify(EDITOR_EXE + " mcp"), mod.EDITOR)

    def test_ambiguous_fails_closed(self):
        for line in (None, "", "   "):
            self.assertEqual(mod.classify(line), mod.AMBIGUOUS)
            self.assertEqual(mod.blocking([(9, line)]), [9])

    def test_mixed_list_blocks_only_on_non_mcp(self):
        self.assertEqual(mod.blocking([(1, "unity mcp"), (2, EDITOR_EXE), (3, None)]), [2, 3])


if __name__ == "__main__":
    unittest.main()
