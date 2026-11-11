"""Imports the UI-INT-05/06 kits (neko-ui-hall-kit-delivery, neko-ui-shift-results-kit-delivery) into
Assets/_Game/Resources/UiKit.

Same method as import_lineup_shop_kits.py: each sprite is copied byte for byte (no resampling), the .meta is
deterministic and carries the kit's 9-slice border (`unity_border_LBRT` in the hall manifest, `nine_slice` in
the shift/results manifest, both left/bottom/right/top) as the sprite border, and the provenance lines
(sha256 + path) that DocsTests parses are written to provenance_hall_shift_results_lines.txt. Every sprite
already carries its prefix (hall_, shift_, results_). Sprites reused from earlier kits (back_blank, bow_gold,
paw_cream) are NOT imported again. Alternatives (tray frame/surface, chip frame/surface, report frame/surface,
the neutral shift background, the neutral viewport support (the camera clears to the board's paper colour instead), the loose corners, the shop-access art that has no callback) are left out.

    python tools/art/ui_kit/import_hall_shift_results_kits.py <hall-kit> <shift-results-kit> <repo-root>
"""
import hashlib
import json
import os
import re
import shutil
import sys

hall_kit, shift_kit, repo = sys.argv[1], sys.argv[2], sys.argv[3]
here = os.path.dirname(os.path.abspath(__file__))
out = os.path.join(repo, "Assets", "_Game", "Resources", "UiKit")

old = open(os.path.join(here, "import_ui_kit.py"), encoding="utf-8").read()
TEMPLATE = re.search(r'TEMPLATE = """(.*?)"""\n', old, re.S).group(1)

HALL = [
    "hall_office_background", "hall_board_frame", "hall_title_empty",
    "hall_metrics_plate_empty", "hall_metrics_divider", "hall_icon_seat", "hall_icon_ambience",
    "hall_icon_flow", "hall_icon_theme", "hall_instruction_strip_empty", "hall_inventory_tray_empty",
    "hall_inventory_cell_empty", "hall_inventory_cell_selected", "hall_inventory_paw",
    "hall_stock_label_empty",
]

SHIFT = [
    "shift_restaurant_background_open", "shift_room_frame", "shift_top_hud_empty", "shift_maid_chip_empty",
    "shift_mood_track", "shift_mood_fill", "shift_speed_selected", "shift_instruction_empty",
    "results_restaurant_background", "results_report_empty", "results_column_empty", "results_header_empty",
    "results_total_empty", "results_tally_empty", "results_financial_highlight_empty",
    "results_continue_empty", "results_row_empty", "results_separator",
]

sources_path = os.path.join(out, "SOURCES.json")
sources = json.load(open(sources_path, encoding="utf-8"))
lines = []


def run(kit_dir, kit_name, used):
    raw = json.load(open(os.path.join(kit_dir, "manifests", "assets.json"), encoding="utf-8"))
    manifest = {(e.get("id") or e.get("name")): e for e in raw}
    for name in used:
        entry = manifest[name]
        src = os.path.join(kit_dir, entry["path"])
        dst = os.path.join(out, name + ".png")
        shutil.copyfile(src, dst)
        sha = hashlib.sha256(open(dst, "rb").read()).hexdigest()
        assert sha == entry["sha256"], name
        border = entry.get("unity_border_LBRT") or entry.get("nine_slice") or [0, 0, 0, 0]
        guid = hashlib.md5(("neko-ui-kit/" + name).encode()).hexdigest()
        sid = hashlib.md5(("sid/" + name).encode()).hexdigest()[:16] + "0800000000000000"
        open(dst + ".meta", "w", newline="\n").write(
            TEMPLATE.format(guid=guid, l=border[0], b=border[1], r=border[2], t=border[3], sid=sid))
        sources[name + ".png"] = {"source": kit_name + "/" + entry["path"], "sha256": sha, "border_LBRT": border}
        lines.append("%s  Assets/_Game/Resources/UiKit/%s.png" % (sha, name))


run(hall_kit, "neko-ui-hall-kit-delivery", HALL)
run(shift_kit, "neko-ui-shift-results-kit-delivery", SHIFT)

json.dump(sources, open(sources_path, "w", newline="\n"), indent=1)
open(os.path.join(here, "provenance_hall_shift_results_lines.txt"), "w", newline="\n").write("\n".join(lines) + "\n")
print(len(lines), "sprites imported")
