"""Imports the UI-INT-02A kit (neko-ui-maids-kit-delivery) into Assets/_Game/Resources/UiKit.

Same method as import_ui_kit.py: each sprite is copied byte for byte (no resampling), the .meta is
deterministic and carries the kit's `unity_border_LBRT` as the sprite border, and the provenance
lines (sha256 + path) that DocsTests parses are written to provenance_maids_lines.txt. Sprites get
the `maids_` prefix (the background and title plate already have it). The four resources reused from
UI-VISUAL-01 (back, bow, two paws) are NOT imported again: they are already in UiKit.

    python tools/art/ui_kit/import_maids_kit.py <kit-dir> <repo-root>
"""
import hashlib
import json
import os
import re
import shutil
import sys

kit, repo = sys.argv[1], sys.argv[2]
here = os.path.dirname(os.path.abspath(__file__))
out = os.path.join(repo, "Assets", "_Game", "Resources", "UiKit")

# The template is shared with the first importer (read, not imported: that script runs on import).
old = open(os.path.join(here, "import_ui_kit.py"), encoding="utf-8").read()
TEMPLATE = re.search(r'TEMPLATE = """(.*?)"""\n', old, re.S).group(1)

# Used by RosterScreen. Equip / Level-up buttons, the *_frame alternates and the brown surfaces are
# deliberately left out: no action exists for the first two, and the panels are used whole.
USED = [
    "maids_background", "maids_title_plate_empty", "token_counter_empty", "filters_container_empty",
    "filter_row_divider", "filter_selected_empty", "filter_selected_surface", "filter_rarity_normal_empty",
    "filter_position_normal_empty", "filter_paw_brown", "icon_reception", "icon_hall", "icon_kitchen",
    "sort_empty", "sort_divider", "collection_panel_empty", "selected_card_highlight", "page_left",
    "page_right", "page_plate_empty", "detail_panel_empty", "portrait_frame_empty", "rarity_badge_empty",
    "attribute_track_empty", "attribute_fill_gold", "traits_frame_empty", "dismiss_empty",
]

manifest = {e["id"]: e for e in json.load(open(os.path.join(kit, "manifests", "assets.json"), encoding="utf-8"))}
sources_path = os.path.join(out, "SOURCES.json")
sources = json.load(open(sources_path, encoding="utf-8"))

lines = []
for entry_id in USED:
    entry = manifest[entry_id]
    name = entry_id if entry_id.startswith("maids_") else "maids_" + entry_id
    src = os.path.join(kit, entry["path"])
    dst = os.path.join(out, name + ".png")
    shutil.copyfile(src, dst)
    sha = hashlib.sha256(open(dst, "rb").read()).hexdigest()
    assert sha == entry["sha256"], entry_id
    border = entry.get("unity_border_LBRT") or [0, 0, 0, 0]
    guid = hashlib.md5(("neko-ui-kit/" + name).encode()).hexdigest()
    sid = hashlib.md5(("sid/" + name).encode()).hexdigest()[:16] + "0800000000000000"
    open(dst + ".meta", "w", newline="\n").write(
        TEMPLATE.format(guid=guid, l=border[0], b=border[1], r=border[2], t=border[3], sid=sid))
    sources[name + ".png"] = {"source": "neko-ui-maids-kit-delivery/" + entry["path"], "sha256": sha, "border_LBRT": border}
    lines.append("%s  Assets/_Game/Resources/UiKit/%s.png" % (sha, name))

json.dump(sources, open(sources_path, "w", newline="\n"), indent=1)
open(os.path.join(here, "provenance_maids_lines.txt"), "w", newline="\n").write("\n".join(lines) + "\n")
print(len(lines), "sprites imported")
