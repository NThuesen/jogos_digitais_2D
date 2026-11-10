"""Imports the UI-INT-03B kits (neko-ui-lineup-kit-delivery, neko-ui-shop-kit-delivery) into
Assets/_Game/Resources/UiKit.

Same method as import_maids_kit.py: each sprite is copied byte for byte (no resampling), the .meta is
deterministic and carries the kit's `unity_border_LBRT` as the sprite border, and the provenance lines
(sha256 + path) that DocsTests parses are written to provenance_lineup_shop_lines.txt. Lineup sprites get
the `lineup_` prefix (the background and the title plate already have it), shop sprites the `shop_`
prefix (they all have it). Resources reused from earlier kits (back, bow, paws, token counter, coin, logo)
are NOT imported again: they are already in UiKit / UiArt.

    python tools/art/ui_kit/import_lineup_shop_kits.py <lineup-kit> <shop-kit> <repo-root>
"""
import hashlib
import json
import os
import re
import shutil
import sys

lineup_kit, shop_kit, repo = sys.argv[1], sys.argv[2], sys.argv[3]
here = os.path.dirname(os.path.abspath(__file__))
out = os.path.join(repo, "Assets", "_Game", "Resources", "UiKit")

old = open(os.path.join(here, "import_ui_kit.py"), encoding="utf-8").read()
TEMPLATE = re.search(r'TEMPLATE = """(.*?)"""\n', old, re.S).group(1)

# Used by LineupScreen. The *_frame / *_surface alternates, the clipboard (already inside the cost panel),
# the ready-made vacant slot (the resizable frame + paw is used instead) and the outer chemistry frame are
# left out: each panel is used whole, never two versions on top of each other.
LINEUP = [
    "role_plate_empty", "vacant_slot_frame", "vacant_paw_plus", "chemistry_value_panel_empty",
    "daily_cost_panel_empty", "chemistry_header_empty", "chemistry_track_empty", "chemistry_fill_pink",
    "lineup_title_empty", "team_banner_empty", "bank_banner_empty", "bank_filter_empty", "bank_sort_empty",
    "bank_page_empty", "bank_left", "bank_right", "team_panel_empty", "bank_panel_empty",
    "chemistry_panel_empty", "button_auto_empty", "button_clear_empty", "lineup_background",
]

# Used by ShopScreen. The *_icon_window rows and the frame / surface alternates are left out.
SHOP = [
    "shop_tab_normal_empty", "shop_tab_selected_empty", "shop_tab_underline", "shop_buy_empty",
    "shop_money_panel_empty", "shop_title_empty", "shop_bottom_paw_ornament", "shop_panel_top_bow",
    "shop_product_row_normal", "shop_product_row_highlight", "shop_main_panel_empty", "shop_background",
]

sources_path = os.path.join(out, "SOURCES.json")
sources = json.load(open(sources_path, encoding="utf-8"))
lines = []


def run(kit_dir, kit_name, used, prefix):
    manifest = {e["id"]: e for e in json.load(open(os.path.join(kit_dir, "manifests", "assets.json"), encoding="utf-8"))}
    for entry_id in used:
        entry = manifest[entry_id]
        name = entry_id if entry_id.startswith(prefix) else prefix + entry_id
        src = os.path.join(kit_dir, entry["path"])
        dst = os.path.join(out, name + ".png")
        shutil.copyfile(src, dst)
        sha = hashlib.sha256(open(dst, "rb").read()).hexdigest()
        assert sha == entry["sha256"], entry_id
        border = entry.get("unity_border_LBRT") or [0, 0, 0, 0]
        guid = hashlib.md5(("neko-ui-kit/" + name).encode()).hexdigest()
        sid = hashlib.md5(("sid/" + name).encode()).hexdigest()[:16] + "0800000000000000"
        open(dst + ".meta", "w", newline="\n").write(
            TEMPLATE.format(guid=guid, l=border[0], b=border[1], r=border[2], t=border[3], sid=sid))
        sources[name + ".png"] = {"source": kit_name + "/" + entry["path"], "sha256": sha, "border_LBRT": border}
        lines.append("%s  Assets/_Game/Resources/UiKit/%s.png" % (sha, name))


run(lineup_kit, "neko-ui-lineup-kit-delivery", LINEUP, "lineup_")
run(shop_kit, "neko-ui-shop-kit-delivery", SHOP, "shop_")

json.dump(sources, open(sources_path, "w", newline="\n"), indent=1)
open(os.path.join(here, "provenance_lineup_shop_lines.txt"), "w", newline="\n").write("\n".join(lines) + "\n")
print(len(lines), "sprites imported")
