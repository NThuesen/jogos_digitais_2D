"""CLEAN, derivatives, audit and contact sheet for the second UI-021 batch (nine new maids, QA only).

Reuses clean() / head_box() of maid_portraits_prepare.py unchanged, so the nine new portraits go through
exactly the pipeline the approved nine did. RAWs are only read (they are git-ignored, so the folder is
taken from NEKO_RAW_DIR, default art_working/ui_modular/raw_gpt next to the repository or the main checkout).
Writes under docs/art/ui-modular/portraits/batch2/ (nothing under Assets/, nothing ships).
Usage: python tools/art/maid_portraits_batch2.py   -> MANIFEST_batch2.json, audit.json, contact sheets
"""
import hashlib, json, os, sys
from pathlib import Path
import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont
from scipy import ndimage as ndi

sys.path.insert(0, str(Path(__file__).resolve().parent))
import maid_portraits_prepare as prep  # noqa: E402

ROOT = Path(__file__).resolve().parents[2]
RAW_DIR = Path(os.environ.get("NEKO_RAW_DIR", ROOT / "art_working/ui_modular/raw_gpt"))
OUT = ROOT / "docs/art/ui-modular/portraits/batch2"
OLD = ROOT / "docs/art/ui-modular/portraits"
NEW = {  # id: (archetype, raw version)
    "mikan": ("Genki", 1), "yuki": ("Kuudere", 1), "kurumi": ("Tsundere", 1),
    "nonoka": ("Dojikko", 1), "kaguya": ("Ojousama", 1), "mizuki": ("Oneesan", 1),
    "himari": ("Idol", 1), "anzu": ("Gourmand", 1), "kasumi": ("Mukuchi", 1),
}
OLD_IDS = {"suzu": "Genki", "hina": "Kuudere", "akari": "Tsundere", "momo": "Dojikko", "reina": "Ojousama",
           "kaede": "Oneesan", "nene": "Idol", "ichika": "Gourmand", "yoru": "Mukuchi"}


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def audit(raw, full):
    """Alpha / margin / cut audit of the RAW (what the artist delivered) and the CLEAN."""
    a = np.array(raw.convert("RGBA"))[..., 3]
    H, W = a.shape
    solid = a > 128
    ys, xs = np.where(solid)
    touch = {  # share of canvas edge pixels that are opaque drawing
        "top": float(solid[0].mean()), "bottom": float(solid[-1].mean()),
        "left": float(solid[:, 0].mean()), "right": float(solid[:, -1].mean()),
    }
    lab, n = ndi.label(a > 10)
    areas = sorted((int(x) for x in ndi.sum(a > 10, lab, range(1, n + 1))), reverse=True)
    semi = float(((a > 10) & (a < 245)).sum() / max(1, (a > 10).sum()))
    # colour of the fully transparent pixels would show as a halo when filtered
    rgba = np.array(raw.convert("RGBA"))
    ghost_rgb = int(((rgba[..., 3] == 0) & (rgba[..., :3].sum(-1) > 0)).sum())
    return {
        "raw_size": [W, H], "mode": raw.mode,
        "bbox": [int(xs.min()), int(ys.min()), int(xs.max()) + 1, int(ys.max()) + 1],
        "margins_px": {"left": int(xs.min()), "top": int(ys.min()), "right": int(W - xs.max() - 1),
                       "bottom": int(H - ys.max() - 1)},
        "edge_opaque_share": {k: round(v, 4) for k, v in touch.items()},
        "components": n, "largest_components": areas[:4], "small_components_lt120": int(sum(1 for x in areas if x < 120)),
        "semi_transparent_share": round(semi, 4), "invisible_pixels_with_colour": ghost_rgb,
    }


def features(clean_img):
    """Rough, comparable numbers: dominant hair/skin/eye-ish colours and silhouette proportions."""
    a = np.array(clean_img)
    al = a[..., 3] > 128
    H, W = al.shape
    ys, xs = np.where(al)
    rgb = a[..., :3].astype(float)
    top = al.copy(); top[int(H * 0.5):] = False
    hair = rgb[top].mean(0) if top.any() else np.zeros(3)
    mid = al.copy(); mid[: int(H * 0.30)] = False; mid[int(H * 0.5):] = False
    # skin guess: warm, bright, low saturation pixels in the face band
    face = rgb[mid]
    mx, mn = face.max(1), face.min(1)
    skin = face[(mx > 190) & ((mx - mn) < 90) & (face[:, 0] >= face[:, 2])]
    row_w = al.sum(1)
    return {"hair_rgb": [int(v) for v in hair], "skin_rgb": [int(v) for v in skin.mean(0)] if len(skin) else None,
            "width_over_height": round(float((xs.max() - xs.min()) / (ys.max() - ys.min())), 3),
            "shoulder_over_head_width": round(float(row_w[int(H * 0.8)] / max(1, row_w[int(H * 0.2)])), 3)}


def label(draw, xy, text, fill=(255, 255, 255)):
    draw.text(xy, text, fill=fill, font=ImageFont.load_default())


def sheet(entries, path, cols, title):
    """entries: list of (id, archetype, CLEAN image, tag). Each cell = the detail crop on neutral grey."""
    cw, ch = 300, 420
    rows = (len(entries) + cols - 1) // cols
    img = Image.new("RGB", (cols * cw, rows * ch + 24), (92, 86, 98))
    d = ImageDraw.Draw(img)
    label(d, (6, 6), title)
    for i, (name, arch, clean_img, tag) in enumerate(entries):
        x, y = (i % cols) * cw, (i // cols) * ch + 24
        w, h = clean_img.size
        s = min((cw - 8) / w, (ch - 28) / h)
        thumb = clean_img.resize((max(1, int(w * s)), max(1, int(h * s))), Image.LANCZOS)
        img.paste(thumb, (x + (cw - thumb.width) // 2, y + 4), thumb)
        label(d, (x + 6, y + ch - 20), f"{name.upper()}  {arch}  {tag}", (255, 240, 200) if tag == "NEW" else (200, 220, 255))
    img.save(path, optimize=True)


def process_one(name, ver):
    """Incremental: one maid at one RAW version, written as <name>_v0N_* so earlier versions stay untouched."""
    arch = NEW[name][0]
    src = RAW_DIR / f"UI-021_maid_portrait_{name}_neutral_v{ver:02d}.png"
    raw = Image.open(src)
    full, specks, ghosts = prep.clean(raw)
    W, H = full.size
    arr = np.zeros((H + 2 * prep.PAD, W + 2 * prep.PAD, 4), np.uint8)
    arr[prep.PAD:prep.PAD + H, prep.PAD:prep.PAD + W] = np.array(full)
    canvas = Image.fromarray(prep.bleed(arr), "RGBA")
    x0, y0, x1, y1 = prep.head_box(full)
    head = canvas.crop((x0 + prep.PAD, y0 + prep.PAD, x1 + prep.PAD, y1 + prep.PAD)).resize((192, 157), Image.LANCZOS)
    al = head.getchannel("A")
    head = head.convert("RGB").filter(ImageFilter.UnsharpMask(radius=0.8, percent=35, threshold=2))
    head.putalpha(al)
    dw = max(W, int(round(H * 0.75))); dh = max(int(round(dw / 0.75)), H); dw = int(round(dh * 0.75))
    det = Image.new("RGBA", (dw, dh)); det.paste(full, ((dw - W) // 2, dh - H)); det = det.resize((480, 641), Image.LANCZOS)
    tag = f"{name}_v{ver:02d}"
    files = {"clean": OUT / f"{tag}_clean.png", "head": OUT / "derived" / f"{tag}_head_192.png",
             "detail": OUT / "derived" / f"{tag}_detail_480.png"}
    full.save(files["clean"], optimize=True); head.save(files["head"], optimize=True); det.save(files["detail"], optimize=True)
    entry = {
        "archetype": arch,
        "raw": {"file": src.name, "sha256": sha(src), "size": list(raw.size)},
        "clean": {"size": [W, H], "sha256": sha(files["clean"]), "specks_removed": specks, "grey_ghosts_removed": ghosts},
        "head_192x157": {"sha256": sha(files["head"]), "box": [x0, y0, x1, y1]},
        "detail_480x641": {"sha256": sha(files["detail"])},
        "status": "NOT_REVIEWED",
    }
    mpath = OUT / "MANIFEST_batch2.json"
    manifest = json.loads(mpath.read_text(encoding="utf-8"))
    manifest[tag] = entry
    mpath.write_text(json.dumps(manifest, indent=1), encoding="utf-8")
    apath = OUT / "audit_batch2.json"
    report = json.loads(apath.read_text(encoding="utf-8"))
    report[tag] = {"raw_audit": audit(raw, full), "features": features(full)}
    apath.write_text(json.dumps(report, indent=1), encoding="utf-8")
    print(tag, raw.size, "->", full.size, "specks", specks, "ghosts", ghosts, "head box", (x0, y0, x1, y1))


def main():
    if len(sys.argv) > 1:  # e.g.  maid_portraits_batch2.py nonoka:2
        for arg in sys.argv[1:]:
            n, v = arg.split(":")
            process_one(n, int(v))
        return
    OUT.mkdir(parents=True, exist_ok=True)
    (OUT / "derived").mkdir(exist_ok=True)
    manifest, report, cleans = {}, {}, {}
    for name, (arch, ver) in NEW.items():
        src = RAW_DIR / f"UI-021_maid_portrait_{name}_neutral_v{ver:02d}.png"
        raw = Image.open(src)
        full, specks, ghosts = prep.clean(raw)
        W, H = full.size
        arr = np.zeros((H + 2 * prep.PAD, W + 2 * prep.PAD, 4), np.uint8)
        arr[prep.PAD:prep.PAD + H, prep.PAD:prep.PAD + W] = np.array(full)
        canvas = Image.fromarray(prep.bleed(arr), "RGBA")
        x0, y0, x1, y1 = prep.head_box(full)
        head = canvas.crop((x0 + prep.PAD, y0 + prep.PAD, x1 + prep.PAD, y1 + prep.PAD)).resize((192, 157), Image.LANCZOS)
        al = head.getchannel("A")
        head = head.convert("RGB").filter(ImageFilter.UnsharpMask(radius=0.8, percent=35, threshold=2))
        head.putalpha(al)
        dw = max(W, int(round(H * 0.75))); dh = max(int(round(dw / 0.75)), H); dw = int(round(dh * 0.75))
        det = Image.new("RGBA", (dw, dh)); det.paste(full, ((dw - W) // 2, dh - H)); det = det.resize((480, 641), Image.LANCZOS)

        files = {"clean": OUT / f"{name}_clean.png", "head": OUT / "derived" / f"{name}_head_192.png",
                 "detail": OUT / "derived" / f"{name}_detail_480.png"}
        full.save(files["clean"], optimize=True); head.save(files["head"], optimize=True); det.save(files["detail"], optimize=True)
        manifest[name] = {
            "archetype": arch,
            "raw": {"file": src.name, "sha256": sha(src), "size": list(raw.size)},
            "clean": {"size": [W, H], "sha256": sha(files["clean"]), "specks_removed": specks, "grey_ghosts_removed": ghosts},
            "head_192x157": {"sha256": sha(files["head"]), "box": [x0, y0, x1, y1]},
            "detail_480x641": {"sha256": sha(files["detail"])},
            "status": "NOT_REVIEWED",
        }
        report[name] = {"raw_audit": audit(raw, full), "features": features(full)}
        cleans[name] = full
        print(name, raw.size, "->", full.size, "specks", specks, "ghosts", ghosts)
    (OUT / "MANIFEST_batch2.json").write_text(json.dumps(manifest, indent=1), encoding="utf-8")

    old_cleans = {n: Image.open(OLD / f"{n}_clean.png").convert("RGBA") for n in OLD_IDS}
    for n, img in old_cleans.items():
        report.setdefault(n, {})["features"] = features(img)
    (OUT / "audit_batch2.json").write_text(json.dumps(report, indent=1), encoding="utf-8")

    order = ["Genki", "Kuudere", "Tsundere", "Dojikko", "Ojousama", "Oneesan", "Idol", "Gourmand", "Mukuchi"]
    old_by = {v: k for k, v in OLD_IDS.items()}
    new_by = {v[0]: k for k, v in NEW.items()}
    entries = []
    for arch in order:  # each archetype: original then new, side by side
        o, nw = old_by[arch], new_by[arch]
        entries.append((o, arch, old_cleans[o], "ORIG"))
        entries.append((nw, arch, cleans[nw], "NEW"))
    sheet(entries, OUT / "contact_sheet_18.png", 6, "UI-021  originals (blue) vs new (gold), grouped by archetype")
    sheet([(n, NEW[n][0], cleans[n], "NEW") for n in NEW], OUT / "contact_sheet_new9.png", 5, "UI-021 new nine")


if __name__ == "__main__":
    main()
