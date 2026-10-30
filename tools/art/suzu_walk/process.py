#!/usr/bin/env python3
"""Reproducible offline processing of the Suzu walk frames (ART-02).

Input : art_working/01_suzu/step4_blender/walk_x/frames/*.png   (never modified)
Output: one packed sprite sheet + manifest + preview GIFs.

Steps, in order, each one answering a defect measured in the raw frames:
  1. clean   - drop thin horizontal matte streaks that survived the video's background removal
  2. blink   - the video closes her eyes for ~11 of the 30 frames; paste the eyes of the nearest
               open-eyed frame (aligned on the surrounding bangs/cheeks) so the face is stable
  3. align   - walk-in-place drifts +-17 px; anchor the planted foot (y) and the skirt (x)
  4. resize  - one scale for every frame (premultiplied alpha), common cell, feet on the bottom
  5. style   - "smooth" keeps the downsampled illustration; "pixel" quantises to one shared
               palette and binarises alpha so it sits next to the pixel-art furniture
  6. pack    - grid sheet + manifest.json (hashes of the sources, every parameter)

Requires: Python 3.9+, Pillow, numpy, scipy.   Run:  python tools/art/suzu_walk/process.py
"""
import argparse
import glob
import hashlib
import json
import os
import sys

import numpy as np
from PIL import Image
from scipy import ndimage as nd

ALPHA_MIN = 8
SRC_DEFAULT = "art_working/01_suzu/step4_blender/walk_x/frames"


def sha256(path):
    h = hashlib.sha256()
    with open(path, "rb") as f:
        h.update(f.read())
    return h.hexdigest()


def clean(rgba):
    """Remove thin, wide components (matte streaks). Returns the cleaned array and #removed."""
    a = rgba[..., 3]
    lab, n = nd.label(a > ALPHA_MIN, structure=np.ones((3, 3)))
    removed = 0
    for i, sl in enumerate(nd.find_objects(lab), start=1):
        h = sl[0].stop - sl[0].start
        w = sl[1].stop - sl[1].start
        if h <= 8 and w >= 30:
            m = lab[sl] == i
            a[sl][m] = 0
            removed += 1
    return rgba, removed


def head_cx(rgba):
    ys, xs = np.where(rgba[200:320, :, 3] > 128)
    return int(np.median(xs))


EYE_Y0, EYE_Y1, EYE_HALF = 258, 318, 85


def amber_count(rgba, cx):
    box = rgba[EYE_Y0:EYE_Y1, cx - EYE_HALF:cx + EYE_HALF].astype(int)
    r, g, b = box[..., 0], box[..., 1], box[..., 2]
    return int(((r > 200) & (g > 110) & (g < 200) & (b < 90)).sum())


def align_donor(target, tcx, donor, dcx):
    """Best (dy, dx) so that donor shifted lands on target, measured on bangs + cheeks."""
    t = target[..., :3].astype(np.float32)
    d = donor[..., :3].astype(np.float32)
    best = (1e18, 0, 0)
    for dy in range(-30, 31, 2):
        for dx in range(-40, 41, 2):
            err = 0.0
            for (y0, y1) in ((212, 246), (324, 346)):
                tb = t[y0:y1, tcx - 100:tcx + 100]
                db = d[y0 - dy:y1 - dy, dcx + dx - 100:dcx + dx + 100]
                if db.shape != tb.shape:
                    err = 1e18
                    break
                err += float(((tb - db) ** 2).mean())
            if err < best[0]:
                best = (err, dy, dx)
    return best[1], best[2]


def repair_blinks(frames):
    cxs = [head_cx(f) for f in frames]
    counts = [amber_count(f, c) for f, c in zip(frames, cxs)]
    open_ref = float(np.median(sorted(counts)[len(counts) // 2:]))
    is_open = [c >= 0.8 * open_ref for c in counts]
    fixed = []
    n = len(frames)
    for i in range(n):
        if is_open[i]:
            continue
        # nearest open frame, cyclic
        donor = min((j for j in range(n) if is_open[j]), key=lambda j: min((j - i) % n, (i - j) % n))
        dy, dx = align_donor(frames[i], cxs[i], frames[donor], cxs[donor])
        # donor patch whose eye box sits where the target's does
        ty0, ty1 = EYE_Y0, EYE_Y1
        tx0, tx1 = cxs[i] - EYE_HALF, cxs[i] + EYE_HALF
        sx0 = cxs[donor] + dx - EYE_HALF
        patch = frames[donor][ty0 - dy:ty1 - dy, sx0:sx0 + (tx1 - tx0)].astype(np.float32)
        h, w = patch.shape[:2]
        yy, xx = np.mgrid[0:h, 0:w]
        # soft elliptical mask, feathered
        m = 1.0 - np.clip((((yy - h / 2) / (h / 2)) ** 2 + ((xx - w / 2) / (w / 2)) ** 2) ** 0.5 - 0.62, 0, 0.38) / 0.38
        m = nd.gaussian_filter(m, 3)[..., None]
        tgt = frames[i][ty0:ty1, tx0:tx1].astype(np.float32)
        frames[i][ty0:ty1, tx0:tx1] = (tgt * (1 - m) + patch * m).round().astype(np.uint8)
        fixed.append((i, donor, dy, dx))
    return frames, fixed, counts, open_ref


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--src", default=SRC_DEFAULT)
    ap.add_argument("--out", default="Assets/_Game/Resources/Suzu")
    ap.add_argument("--docs", default="docs/art/suzu-walk")
    ap.add_argument("--work", default="art_working/01_suzu/step5_processed")
    ap.add_argument("--height", type=int, default=112, help="character height in output pixels")
    ap.add_argument("--style", choices=["smooth", "pixel"], default="pixel")
    ap.add_argument("--colors", type=int, default=48)
    args = ap.parse_args()

    paths = sorted(glob.glob(os.path.join(args.src, "*.png")))
    if not paths:
        sys.exit("no frames in " + args.src)
    frames = [np.array(Image.open(p).convert("RGBA")) for p in paths]
    n = len(frames)

    removed = 0
    for f in frames:
        _, r = clean(f)
        removed += r

    frames, fixed, counts, open_ref = repair_blinks(frames)

    # ---- anchors ------------------------------------------------------------------------
    info = []
    for f in frames:
        a = f[..., 3]
        ys, xs = np.where(a > 128)
        top, bot = ys.min(), ys.max()
        band = (ys > top + 0.42 * (bot - top)) & (ys < top + 0.60 * (bot - top))
        info.append((top, bot, float(np.median(xs[band]))))
    heights = [b - t for t, b, _ in info]
    scale = args.height / float(np.mean(heights))
    ax = float(np.mean([c for _, _, c in info]))
    ground = int(round(np.mean([b for _, b, _ in info])))

    # ---- shift to the common anchor, then crop a common box --------------------------------
    pad = 40
    shifted = []
    for f, (t, b, cx) in zip(frames, info):
        dy = ground - b
        dx = int(round(ax - cx))
        canvas = np.zeros_like(f)
        h, w = f.shape[:2]
        ys0, ys1 = max(0, dy), min(h, h + dy)
        xs0, xs1 = max(0, dx), min(w, w + dx)
        canvas[ys0:ys1, xs0:xs1] = f[ys0 - dy:ys1 - dy, xs0 - dx:xs1 - dx]
        shifted.append(canvas)
    boxes = []
    for f in shifted:
        ys, xs = np.where(f[..., 3] > ALPHA_MIN)
        boxes.append((xs.min(), ys.min(), xs.max(), ys.max()))
    x0 = int(min(b[0] for b in boxes)) - pad
    x1 = int(max(b[2] for b in boxes)) + pad
    y0 = int(min(b[1] for b in boxes)) - pad
    # symmetric about the anchor so a mirror flip keeps her on the same spot
    half = max(int(round(ax)) - x0, x1 - int(round(ax)))
    x0, x1 = int(round(ax)) - half, int(round(ax)) + half
    y1 = ground + 6

    # ---- resize (premultiplied) ------------------------------------------------------------
    cw = int(round((x1 - x0) * scale))
    ch = int(round((y1 - y0) * scale))
    cw += cw % 2
    cells = []
    for f in shifted:
        im = Image.fromarray(f[y0:y1, x0:x1], "RGBA").convert("RGBa")
        im = im.resize((cw, ch), Image.LANCZOS).convert("RGBA")
        cells.append(np.array(im))

    # ---- style --------------------------------------------------------------------------
    if args.style == "pixel":
        opaque = np.concatenate([c[c[..., 3] >= 128][:, :3] for c in cells])
        sw = int(np.ceil(np.sqrt(len(opaque))))
        pal_src = Image.fromarray(np.resize(opaque, (sw * sw, 3)).reshape(sw, sw, 3).astype(np.uint8))
        pal = pal_src.quantize(colors=args.colors, method=Image.MEDIANCUT, dither=Image.NONE)
        out_cells = []
        for c in cells:
            rgb = Image.fromarray(c[..., :3]).quantize(palette=pal, dither=Image.NONE).convert("RGB")
            q = np.array(rgb)
            alpha = np.where(c[..., 3] >= 128, 255, 0).astype(np.uint8)
            out_cells.append(np.dstack([q, alpha]))
        cells = out_cells
    # bleed colour into transparent texels so bilinear filtering never shows a dark halo
    for i, c in enumerate(cells):
        idx = nd.distance_transform_edt(c[..., 3] == 0, return_distances=False, return_indices=True)
        filled = c[idx[0], idx[1]]
        c[..., :3] = np.where((c[..., 3] == 0)[..., None], filled[..., :3], c[..., :3])

    # ---- idle candidates: both shoes on the ground (the two lowest shoe blobs level) ----------
    settle, spread = [], []
    for i, c in enumerate(cells):
        r, g, bl, al = (c[..., k].astype(int) for k in range(4))
        m = (al > 0) & (r < 130) & (g < 80) & (bl < 80)
        m[:int(c.shape[0] * 0.7)] = False
        lab, k = nd.label(m, structure=np.ones((3, 3)))
        sizes = nd.sum(m, lab, range(1, k + 1))
        top = [nd.find_objects(lab)[j] for j in np.argsort(sizes)[::-1][:2]]
        ydiff = abs(top[0][0].stop - top[1][0].stop) if len(top) == 2 else 99
        xsep = abs((top[0][1].start + top[0][1].stop) - (top[1][1].start + top[1][1].stop)) / 2 if len(top) == 2 else 99
        spread.append([int(ydiff), float(xsep)])
        if ydiff <= 2:
            settle.append(i)
    idle = min(settle, key=lambda i: (spread[i][0], spread[i][1]))
    sep = spread

    # ---- pack ---------------------------------------------------------------------------
    cols = 6
    rows = (n + cols - 1) // cols
    sheet = np.zeros((rows * ch, cols * cw, 4), np.uint8)
    for i, c in enumerate(cells):
        r, col = divmod(i, cols)
        sheet[(rows - 1 - r) * ch:(rows - r) * ch, col * cw:(col + 1) * cw] = c  # row 0 at the bottom (Unity rects)
    os.makedirs(args.out, exist_ok=True)
    sheet_path = os.path.join(args.out, "suzu_walk_sheet.png")
    Image.fromarray(sheet, "RGBA").save(sheet_path, optimize=True)

    manifest = {
        "tool": "tools/art/suzu_walk/process.py",
        "source_dir": args.src.replace("\\", "/"),
        "source_sha256": {os.path.basename(p): sha256(p) for p in paths},
        "frames": n, "cols": cols, "rows": rows, "cell": [cw, ch],
        "pivot": "bottom-centre (planted foot on the last row, skirt axis on the centre column)",
        "pixels_per_unit_hint": 96,
        "character_height_px": args.height, "scale": round(scale, 5),
        "style": args.style, "palette_colors": args.colors if args.style == "pixel" else None,
        "streaks_removed": removed,
        "blink_patched": [{"frame": i, "donor": d, "dy": dy, "dx": dx} for i, d, dy, dx in fixed],
        "amber_counts": counts, "amber_open_reference": open_ref,
        "idle_frame": idle, "settle_frames": settle, "foot_ydiff_xsep": sep,
        "sheet_sha256": sha256(sheet_path),
    }
    os.makedirs(args.docs, exist_ok=True)
    prov = os.path.join(os.path.dirname(args.docs.rstrip("/" + chr(92))), "SUZU_ASSET_PROVENANCE.md")
    if os.path.exists(prov):
        tail = "  Assets/_Game/Resources/Suzu/suzu_walk_sheet.png"
        with open(prov, encoding="utf8", newline="") as f:
            text = f.read()
        out = []
        for line in text.split(chr(10)):
            out.append(manifest["sheet_sha256"] + tail if line.rstrip(chr(13)).endswith(tail) else line)
        with open(prov, "w", encoding="utf8", newline="") as f:
            f.write(chr(10).join(out))
    with open(os.path.join(args.docs, "manifest.json"), "w") as f:
        json.dump(manifest, f, indent=1)

    # local per-frame output (git-ignored) + preview
    os.makedirs(args.work, exist_ok=True)
    for i, c in enumerate(cells):
        Image.fromarray(c, "RGBA").save(os.path.join(args.work, "walk_%02d.png" % i))
    previews = []
    for c in cells:
        bg = Image.new("RGBA", (cw, ch), (214, 178, 140, 255))
        bg.alpha_composite(Image.fromarray(c, "RGBA"))
        previews.append(bg.resize((cw * 3, ch * 3), Image.NEAREST).convert("P", palette=Image.ADAPTIVE))
    previews[0].save(os.path.join(args.docs, "walk_preview.gif"), save_all=True,
                     append_images=previews[1:], duration=33, loop=0, disposal=2)
    print(json.dumps({k: manifest[k] for k in ("frames", "cell", "scale", "streaks_removed", "idle_frame")}))
    print("blink patched:", [(i, d) for i, d, _, _ in fixed])


if __name__ == "__main__":
    main()
