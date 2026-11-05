"""Conservative CLEAN and derived crops for the nine UI-021 maid portraits (QA/integration prep).

RAWs are read, never modified. For each maid: alpha noise and isolated specks removed, edge colour
bled outward (no dark halo under filtering), transparent margin added where the drawing is not cut
by the canvas, then two derivatives from the same CLEAN image, never stretched:
  head   192x157 (1.22:1)  card portrait (Escalacao / Elenco)
  detail 480x641 (3:4)     detail panel
Usage: python tools/art/maid_portraits_prepare.py   -> writes MANIFEST.json with SHA-256 of everything
"""
import hashlib, json
from pathlib import Path
import numpy as np
from PIL import Image, ImageFilter
from scipy import ndimage as ndi

ROOT = Path(__file__).resolve().parents[2]
RAW_DIR = ROOT / "art_working/ui_modular/raw_gpt"
QA = ROOT / "Assets/_Game/Tests/PlayMode/QaArt"
DOCS = ROOT / "docs/art/ui-modular/portraits"
MARGIN, MIN_ALPHA, MIN_AREA, PAD = 12, 10, 120, 100
GHOST_MIN_AREA = 150
HEAD_ASPECT = 1.22
MAIDS = {  # id: raw version
    "suzu": 3, "hina": 1, "akari": 1, "momo": 1, "reina": 1, "kaede": 1, "nene": 1,
    "ichika": 1, "yoru": 1,
}
# Per-maid tuning of the head box if the automatic one is not right: (width factor, centre x shift, top shift)
TUNE = {}

def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()

def bleed(arr):
    solid = arr[..., 3] > 0
    idx = ndi.distance_transform_edt(~solid, return_distances=False, return_indices=True)
    arr[..., :3] = np.where(solid[..., None], arr[..., :3], arr[idx[0], idx[1]][..., :3])
    return arr

def clean(raw):
    a = np.array(raw.convert("RGBA"))
    alpha = a[..., 3].copy()
    alpha[alpha < MIN_ALPHA] = 0
    lab, n = ndi.label(alpha > 0)
    areas = ndi.sum(alpha > 0, lab, range(1, n + 1))
    specks = 0
    for i, area in enumerate(areas, 1):
        if area < MIN_AREA:
            alpha[lab == i] = 0
            specks += 1
    # semi-transparent neutral-grey patches (generation ghosts, e.g. behind Ichika's hat): never
    # legitimate drawing, which is saturated or opaque at its edges
    rgb = a[..., :3].astype(int)
    grey = ((rgb.max(-1) - rgb.min(-1)) < 22) & (alpha > 0) & (alpha < 160) & (rgb.mean(-1) > 110) & (rgb.mean(-1) < 215)
    glab, gn = ndi.label(grey)
    ghosts = 0
    for i in range(1, gn + 1):
        part = glab == i
        if part.sum() >= GHOST_MIN_AREA:
            alpha[part] = 0
            ghosts += 1
    a[..., 3] = alpha
    ys, xs = np.where(alpha > 0)
    x0, x1, y0, y1 = xs.min(), xs.max() + 1, ys.min(), ys.max() + 1
    # the bust is cut by the canvas at the bottom: no margin there (a gap would float)
    out = np.zeros((y1 - y0 + MARGIN, x1 - x0 + 2 * MARGIN, 4), np.uint8)
    out[MARGIN:, MARGIN:MARGIN + (x1 - x0)] = a[y0:y1, x0:x1]
    return Image.fromarray(bleed(out), "RGBA"), specks, ghosts

def head_box(full):
    """Box (x0, y0, x1, y1) in full-image coordinates; y0 negative gives air above the ears."""
    al = np.array(full)[..., 3] > 128
    H, W = al.shape
    rows = al[: int(H * 0.45)]
    ys, xs = np.where(rows)
    # centre on the face: weighted by the mass of the top 45%, ignoring thin wisps
    cols = rows.sum(0)
    cx = (np.arange(W) * cols).sum() / cols.sum()
    span = xs.max() - xs.min()
    # height from the chin line (about 0.56 of the bust, down to the neck), width from the aspect;
    # widened only if the ears and hair need more room than that gives
    h = max(int(H * 0.56), int(span * 1.06 / HEAD_ASPECT))
    w = int(round(h * HEAD_ASPECT))
    top = -int(H * 0.045)
    return int(cx - w / 2), top, int(cx - w / 2) + w, top + h

def main():
    QA.mkdir(parents=True, exist_ok=True)
    DOCS.mkdir(parents=True, exist_ok=True)
    manifest = {}
    for name, ver in MAIDS.items():
        src = RAW_DIR / f"UI-021_maid_portrait_{name}_neutral_v{ver:02d}.png"
        raw = Image.open(src)
        full, specks, ghosts = clean(raw)
        W, H = full.size
        arr = np.zeros((H + 2 * PAD, W + 2 * PAD, 4), np.uint8)
        arr[PAD:PAD + H, PAD:PAD + W] = np.array(full)
        canvas = Image.fromarray(bleed(arr), "RGBA")

        x0, y0, x1, y1 = head_box(full)
        head = canvas.crop((x0 + PAD, y0 + PAD, x1 + PAD, y1 + PAD)).resize((192, 157), Image.LANCZOS)
        alpha = head.getchannel("A")
        head = head.convert("RGB").filter(ImageFilter.UnsharpMask(radius=0.8, percent=35, threshold=2))
        head.putalpha(alpha)

        # detail: whole CLEAN fitted into 3:4 by adding transparent room above / beside, bottom flush
        dw = max(W, int(round(H * 0.75)))
        dh = int(round(dw / 0.75))
        dh = max(dh, H)
        dw = int(round(dh * 0.75))
        det = Image.new("RGBA", (dw, dh))
        det.paste(full, ((dw - W) // 2, dh - H))
        det = det.resize((480, 641), Image.LANCZOS)

        files = {
            "head": QA / f"{name}_head_192.png",
            "detail": QA / f"{name}_detail_480.png",
            "clean": DOCS / f"{name}_clean.png",
        }
        head.save(files["head"], optimize=True)
        det.save(files["detail"], optimize=True)
        full.save(files["clean"], optimize=True)
        manifest[name] = {
            "raw": {"file": src.name, "sha256": sha(src), "size": list(raw.size)},
            "clean": {"size": [W, H], "sha256": sha(files["clean"]), "specks_removed": specks, "grey_ghosts_removed": ghosts},
            "head_192x157": {"sha256": sha(files["head"]), "box": [x0, y0, x1, y1]},
            "detail_480x641": {"sha256": sha(files["detail"])},
        }
        print(name, raw.size, "->", full.size, "specks", specks, "ghosts", ghosts, "head box", (x0, y0, x1, y1))
    (DOCS / "MANIFEST.json").write_text(json.dumps(manifest, indent=1), encoding="utf-8")

if __name__ == "__main__":
    main()
