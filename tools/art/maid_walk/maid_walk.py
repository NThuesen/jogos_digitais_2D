#!/usr/bin/env python3
"""Reproducible offline pipeline for the maids' authored walk cycles (ART-03).

  python tools/art/maid_walk/maid_walk.py audit   [--only id,id]   # QA report + contact sheets, writes nothing in Assets/
  python tools/art/maid_walk/maid_walk.py process [--only id,id]   # sprite sheets + per-maid manifest + catalog

Input : art_working/<folder>/*.png (git-ignored Blender frames, any count, never modified).
Output: Assets/_Game/Resources/Maids/<id>_walk.png, Resources/Maids/maid_walk_catalog.json,
        docs/art/maid-walk/<id>/manifest.json (+ preview GIF), docs/art/maid-walk/INVENTORY.md.

Every number is measured on the sequence being processed; nothing is copied from another maid.
  clean   : zero the low-alpha matte, key green chroma where the export still has it, drop specks
  anchor  : feet (lowest opaque row) and body axis (median x of the torso band) per frame
  resize  : ONE Lanczos resample from the raw crop, premultiplied alpha, one scale per maid
  style   : "illustrated" (default, ART-04): colours, shading and soft alpha untouched.
            "pixel" (ART-03, kept only to reproduce / roll back): per-maid 48-colour palette + hard alpha.
  measure : rest frames (both shoes down), idle frame, foot speed -> no-slide units per cycle

Requires Python 3.9+, Pillow, numpy, scipy.
"""
import argparse
import glob
import hashlib
import json
import os
import re
import sys

import numpy as np
from PIL import Image
from scipy import ndimage as nd

BASE_WALK_SPEED = 2.2   # Neko.Sim MaidMath.BaseWalkSpeed, world units per second at 1x
CADENCE_FPS = 30        # drawings per second at that speed: the rate the source videos were authored at
ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
CFG = os.path.join(os.path.dirname(__file__), "maids.json")
OUT_RES = os.path.join(ROOT, "Assets", "_Game", "Resources", "Maids")
OUT_DOCS = os.path.join(ROOT, "docs", "art", "maid-walk")
REFERENCE_HEIGHT_PX = 120  # height the foot analysis was tuned at
SOLID = 128  # alpha at or above this is "the character"


def sha256(path):
    with open(path, "rb") as f:
        return hashlib.sha256(f.read()).hexdigest()


def load_config(only, style=None):
    cfg = json.load(open(CFG, encoding="utf8"))
    style = style or cfg["style"]
    maids = []
    for m in cfg["maids"]:
        full = dict(cfg["defaults"])
        full.update(cfg["styles"][style])
        full.update(m)
        full["style"] = style
        # texture pixels per world unit: the sprite is always `world_height` units tall, however sharp the texture
        full["pixels_per_unit"] = round(full["height_px"] / full["world_height"], 3)
        if not only or m["id"] in only:
            maids.append(full)
    return cfg, maids


# ------------------------------------------------------------------------------ cleaning
def key_green(rgba):
    """Remove a green screen that the export left in, including pockets the figure encloses."""
    rgb = rgba[..., :3].astype(int)
    dom = rgb[..., 1] - np.maximum(rgb[..., 0], rgb[..., 2])
    # Every strongly green pixel, not only the ones touching the border: the screen also shows through
    # enclosed gaps (between a hair bun and a curl). No maid wears this green; teal has g close to b, so dom stays small.
    bg = dom > 60
    ring = nd.binary_dilation(bg, iterations=3) & ~bg
    soft = ring & (dom > 12)
    out = rgba.copy()
    out[bg, 3] = 0
    # edge pixels: despill (pull green down to the other channels) and fade by how green they are
    g = np.minimum(rgb[..., 1], np.maximum(rgb[..., 0], rgb[..., 2]))
    out[..., 1] = np.where(soft, g, out[..., 1])
    fade = np.clip(1.0 - (dom - 12) / 80.0, 0, 1)
    out[..., 3] = np.where(soft, (out[..., 3] * fade).astype(np.uint8), out[..., 3])
    return out, int(bg.sum())


def clean(rgba, chroma):
    info = {}
    if chroma == "green":
        rgba, n = key_green(rgba)
        info["chroma_px"] = n
    a = rgba[..., 3]
    info["matte_px"] = int(((a > 0) & (a < 40)).sum())
    a[a < 40] = 0  # faint matte box some exports keep around the figure
    lab, n = nd.label(a >= SOLID, structure=np.ones((3, 3)))
    sizes = nd.sum(a >= SOLID, lab, range(1, n + 1))
    specks = 0
    for i, sl in enumerate(nd.find_objects(lab), start=1):
        h, w = sl[0].stop - sl[0].start, sl[1].stop - sl[1].start
        if sizes[i - 1] < 60 or (h <= 8 and w >= 30):
            a[sl][lab[sl] == i] = 0
            specks += 1
    info["specks"] = specks
    return rgba, info


def metrics(rgba):
    a = rgba[..., 3] >= SOLID
    ys, xs = np.where(a)
    top, bot = ys.min(), ys.max()
    band = (ys > top + 0.42 * (bot - top)) & (ys < top + 0.60 * (bot - top))
    return {"top": int(top), "bot": int(bot), "cx": float(np.median(xs[band])), "x0": int(xs.min()), "x1": int(xs.max())}


# ------------------------------------------------------------------------------ feet
def foot_blobs(cell, floor_rows=2):
    """Blobs of the silhouette touching the lowest `floor_rows` rows: the shoes on the floor."""
    a = cell[..., 3] >= SOLID
    ys = np.where(a.any(axis=1))[0]
    ground = ys.max()
    m = np.zeros_like(a)
    m[ground - floor_rows + 1:ground + 1] = a[ground - floor_rows + 1:ground + 1]
    lab, n = nd.label(m, structure=np.ones((3, 3)))
    out = []
    for i in range(1, n + 1):
        yy, xx = np.where(lab == i)
        if len(yy) >= 3:
            out.append((float(xx.mean()), int(yy.max())))
    return sorted(out), int(ground)


def analyse_feet(cells, ppu):
    rest, seps, singles = [], [], []
    centres = []
    for i, c in enumerate(cells):
        blobs, ground = foot_blobs(c)
        centres.append((blobs, ground))
        if len(blobs) >= 2 and abs(blobs[-1][0] - blobs[0][0]) >= 4:
            rest.append(i)
            seps.append(abs(blobs[-1][0] - blobs[0][0]))
        else:
            seps.append(0.0)
    # planted foot speed: the foot that stays on the ground slides back by the body's speed
    speeds = []
    for i in range(len(cells) - 1):
        (b0, g0), (b1, g1) = centres[i], centres[i + 1]
        for x0, _ in b0:
            near = [x1 for x1, _ in b1 if abs(x1 - x0) < 14]
            if near:
                d = min(near, key=lambda x1: abs(x1 - x0)) - x0
                if abs(d) > 0.3:
                    speeds.append(d)
    speed = float(np.median(np.abs(speeds))) if speeds else 0.0
    return rest, seps, speed


# ------------------------------------------------------------------------------ audit helpers
def sil_diff(a, b):
    sa, sb = a[..., 3] >= SOLID, b[..., 3] >= SOLID
    return float((sa ^ sb).sum() / max(1, (sa | sb).sum()))


def loop_diff(a, b):
    """Silhouette XOR plus colour difference over the shared body, both 0..1."""
    sa, sb = a[..., 3] >= SOLID, b[..., 3] >= SOLID
    both = sa & sb
    colour = float(np.abs(a[..., :3].astype(int) - b[..., :3].astype(int))[both].mean() / 255.0) if both.any() else 1.0
    return sil_diff(a, b) + colour


def best_loop(small, min_len):
    """(start, end) whose last frame leads most naturally back into the first one."""
    n = len(small)
    steps = [loop_diff(small[k], small[k + 1]) for k in range(n - 1)]
    mean_step = float(np.mean(steps))
    if loop_diff(small[-1], small[0]) <= 1.5 * mean_step:
        return 0, n - 1, mean_step  # already closes: keep every frame
    best = (loop_diff(small[-1], small[0]), 0, n - 1)
    for st in range(0, n - min_len + 1):
        for en in range(st + min_len - 1, n):
            score = loop_diff(small[en], small[st]) + 0.01 * (n - (en - st + 1))
            if score < best[0] - 1e-9:
                best = (score, st, en)
    return best[1], best[2], mean_step


def status_for(report, m):
    if m.get("pack") is False:
        return "REJECTED", m.get("reject_reason", "")
    limits, fixes = [], []
    if report["loop_ratio"] > 2.0:
        limits.append("loop does not close even after trimming (x%.1f a normal step)" % report["loop_ratio"])
    if len(report["rest_frames"]) < 2:
        limits.append("fewer than two both-feet-down frames")
    if report["cx_std"] > 40:
        limits.append("body axis wanders (std %.0f px)" % report["cx_std"])
    if report["duplicate_pairs"]:
        limits.append("%d duplicated neighbours" % report["duplicate_pairs"])
    if report["chroma_px"]:
        fixes.append("green chroma keyed offline")
    if report["matte_px"] > 100000:
        fixes.append("faint matte removed")
    if report["specks"] > 5:
        fixes.append("%d specks removed" % report["specks"])
    if report["loop_trim"] != [0, report["frames_found"] - 1]:
        fixes.append("trimmed to source frames %d-%d to close the loop" % tuple(report["source_frame_numbers"]))
    if limits:
        return "LIMITED", "; ".join(limits + fixes)
    if fixes:
        return "NEEDS_PROCESSING", "; ".join(fixes)
    return "READY", ""


# ------------------------------------------------------------------------------ main work
def prepare(m):
    # Not glob("*.png"): glob skips dot-files, and one export arrived as ".gitignore0057.png".
    # Any "<prefix><digits>.png" counts; the files themselves are never renamed.
    path = os.path.join(ROOT, "art_working", m["folder"])
    found = []
    for name in os.listdir(path) if os.path.isdir(path) else []:
        hit = re.search(r"(\d+)\.png$", name, re.I)
        if hit:
            found.append((int(hit.group(1)), os.path.join(path, name)))
    return [p for _, p in sorted(found)]


def run(m, write):
    paths = prepare(m)
    result = {"id": m["id"], "name": m["name"], "frames_found": len(paths), "folder": m["folder"]}
    if not paths:
        result.update(status="REJECTED", note=m.get("reject_reason", "no frames"))
        return result, None
    nums = [int(re.search(r"(\d+)\.png$", os.path.basename(p), re.I).group(1)) for p in paths]
    result["numbering"] = [nums[0], nums[-1]]
    result["gaps"] = [b for a, b in zip(nums, nums[1:]) if b != a + 1]
    raw = [np.array(Image.open(p).convert("RGBA")) for p in paths]
    result["size"] = list(raw[0].shape[1::-1])
    result["real_alpha_levels"] = int(len(np.unique(raw[0][..., 3])))
    chroma = m.get("chroma")
    if chroma is None and (raw[0][..., 3] > 0).mean() > 0.2:
        chroma = "green"  # almost fully opaque frame: a screen the export did not remove
    cleaned, specks, mattes, chromas = [], 0, 0, 0
    for f in raw:
        f = f.copy()
        f, info = clean(f, chroma)
        cleaned.append(f)
        specks += info["specks"]
        mattes = max(mattes, info["matte_px"])
        chromas = max(chromas, info.get("chroma_px", 0))
    result.update(specks=specks, matte_px=mattes, chroma_px=chromas)

    info = [metrics(f) for f in cleaned]
    heights = [i["bot"] - i["top"] for i in info]
    cxs = np.array([i["cx"] for i in info])
    result.update(height_mean=round(float(np.mean(heights)), 1), height_range=[min(heights), max(heights)],
                  cx_std=round(float(cxs.std()), 1),
                  drift_x=round(float(cxs.max() - cxs.min()), 1),
                  drift_y=int(max(i["bot"] for i in info) - min(i["bot"] for i in info)))

    # continuity: silhouette difference between neighbours vs from the last frame back to the first
    small = [np.array(Image.fromarray(f).resize((480, 270), Image.BILINEAR)) for f in cleaned]
    shifted = []
    for f, i in zip(small, info):
        dx = int(round((np.mean(cxs) - i["cx"]) / 4))
        dy = int(round((np.mean([k["bot"] for k in info]) - i["bot"]) / 4))
        shifted.append(np.roll(np.roll(f, dy, axis=0), dx, axis=1))
    steps = [sil_diff(shifted[k], shifted[k + 1]) for k in range(len(shifted) - 1)]
    wrap = sil_diff(shifted[-1], shifted[0])
    result.update(step_diff_mean=round(float(np.mean(steps)), 4), wrap_diff=round(wrap, 4),
                  loop_ratio=round(wrap / max(1e-6, float(np.mean(steps))), 2),
                  duplicate_pairs=int(sum(1 for s in steps if s < 0.002)),
                  max_step_diff=round(float(max(steps)), 4))

    # ---- the longest stretch that loops: trim lead-in/out the export kept ----------------
    st, en, mean_step = best_loop(shifted, max(12, int(0.6 * len(shifted))))
    wrap_before = result["loop_ratio"]
    if (st, en) != (0, len(shifted) - 1):
        cleaned, info = cleaned[st:en + 1], info[st:en + 1]
        shifted = shifted[st:en + 1]
        cxs = np.array([i["cx"] for i in info])
    wrap_after = loop_diff(shifted[-1], shifted[0]) / max(1e-6, mean_step)
    result.update(loop_trim=[st, en], loop_ratio_before_trim=wrap_before, loop_ratio=round(wrap_after, 2),
                  source_frame_numbers=[nums[st], nums[en]])
    n = len(cleaned)
    ground = int(round(np.mean([i["bot"] for i in info])))
    ax = float(np.mean(cxs))
    scale = m["height_px"] / float(np.mean(heights))
    shifted_full = []
    for f, i in zip(cleaned, info):
        dy, dx = ground - i["bot"], int(round(ax - i["cx"]))
        canvas = np.zeros_like(f)
        h, w = f.shape[:2]
        ys0, ys1, xs0, xs1 = max(0, dy), min(h, h + dy), max(0, dx), min(w, w + dx)
        canvas[ys0:ys1, xs0:xs1] = f[ys0 - dy:ys1 - dy, xs0 - dx:xs1 - dx]
        shifted_full.append(canvas)
    boxes = []
    for f in shifted_full:
        ys, xs = np.where(f[..., 3] >= SOLID)
        boxes.append((xs.min(), ys.min(), xs.max(), ys.max()))
    pad = int(round(m["pad_px"] / scale))
    half = max(int(round(ax)) - min(b[0] for b in boxes), max(b[2] for b in boxes) - int(round(ax))) + pad
    x0, x1 = int(round(ax)) - half, int(round(ax)) + half
    y0, y1 = min(b[1] for b in boxes) - pad, ground + 1
    cw, ch = int(round((x1 - x0) * scale)), int(round((y1 - y0) * scale))
    cw += cw % 2
    cells = []
    for f in shifted_full:
        crop = np.zeros((y1 - y0, x1 - x0, 4), np.uint8)
        sy0, sx0 = max(0, y0), max(0, x0)
        crop[sy0 - y0:sy0 - y0 + (min(f.shape[0], y1) - sy0), sx0 - x0:sx0 - x0 + (min(f.shape[1], x1) - sx0)] = \
            f[sy0:min(f.shape[0], y1), sx0:min(f.shape[1], x1)]
        im = Image.fromarray(crop, "RGBA").convert("RGBa").resize((cw, ch), Image.LANCZOS).convert("RGBA")
        arr = np.array(im)
        # whole-pixel multiples of 4 (block compression), feet stay on the bottom row, body axis stays centred
        h4, w4 = -(-arr.shape[0] // 4) * 4, -(-arr.shape[1] // 4) * 4
        canvas = np.zeros((h4, w4, 4), np.uint8)
        ox = (w4 - arr.shape[1]) // 2
        canvas[h4 - arr.shape[0]:, ox:ox + arr.shape[1]] = arr
        cells.append(canvas)
    cw, ch = cells[0].shape[1], cells[0].shape[0]

    if m["style"] == "pixel":
        opaque = np.concatenate([c[c[..., 3] >= SOLID][:, :3] for c in cells])
        side = int(np.ceil(np.sqrt(len(opaque))))
        pal_src = Image.fromarray(np.resize(opaque, (side * side, 3)).reshape(side, side, 3).astype(np.uint8))
        pal = pal_src.quantize(colors=m["colors"], method=Image.MEDIANCUT, dither=Image.NONE)
        out = []
        for c in cells:
            q = np.array(Image.fromarray(c[..., :3]).quantize(palette=pal, dither=Image.NONE).convert("RGB"))
            out.append(np.dstack([q, np.where(c[..., 3] >= SOLID, 255, 0).astype(np.uint8)]))
        cells = out
    for c in cells:  # bleed colour under transparent texels: no dark halo under filtering
        if m["style"] != "pixel":
            c[c[..., 3] < 4, 3] = 0  # resampling dust, invisible but it would keep mip levels from being clean
        idx = nd.distance_transform_edt(c[..., 3] == 0, return_distances=False, return_indices=True)
        filled = c[idx[0], idx[1]]
        c[..., :3] = np.where((c[..., 3] == 0)[..., None], filled[..., :3], c[..., :3])

    # Foot analysis thresholds (two floor rows, 4 px apart, 14 px per step) were tuned at the 120 px
    # reference height; measure on a copy at that height so the result does not depend on texture sharpness.
    ref_scale = REFERENCE_HEIGHT_PX / float(m["height_px"])
    ref_ppu = REFERENCE_HEIGHT_PX / float(m["world_height"])
    if abs(ref_scale - 1.0) > 1e-6:
        ref = [np.array(Image.fromarray(c, "RGBA").convert("RGBa").resize(
            (max(1, int(round(c.shape[1] * ref_scale))), max(1, int(round(c.shape[0] * ref_scale)))), Image.LANCZOS).convert("RGBA"))
            for c in cells]
    else:
        ref = cells
    rest, seps, speed = analyse_feet(ref, ref_ppu)
    if len(rest) < 2:  # never leave the runtime without a resting frame
        rest = sorted(np.argsort(seps)[:2].tolist()) if any(seps) else [0, n // 2]
    idle = min(rest, key=lambda i: seps[i] if seps[i] >= 4 else 999)
    stride = max(seps)
    noslide = round(speed * n / ref_ppu, 3) if speed else 0.0
    cols = 6
    rows = (n + cols - 1) // cols
    cadence = m.get("cadence_fps", CADENCE_FPS)
    upc = round(n * BASE_WALK_SPEED / cadence, 2)
    result.update(units_per_cycle=upc, slide_factor=round(upc / noslide, 1) if noslide else None,
                  cell=[cw, ch], scale=round(scale, 5), frames=n, columns=cols, rows=rows, idle_frame=int(idle),
                  rest_frames=[int(i) for i in rest], foot_speed_px_per_frame=round(speed, 2),
                  max_foot_separation_px=round(stride, 1), noslide_units_per_cycle=noslide)
    return result, cells


def pack(cells, cols, rows, cw, ch):
    sheet = np.zeros((rows * ch, cols * cw, 4), np.uint8)
    for i, c in enumerate(cells):
        r, col = divmod(i, cols)
        sheet[(rows - 1 - r) * ch:(rows - r) * ch, col * cw:(col + 1) * cw] = c  # row 0 at the bottom (Unity rects)
    return Image.fromarray(sheet, "RGBA")


def contact(cells, path, rest, idle, bg=(214, 178, 140), z=2, resample=Image.NEAREST):
    cw, ch = cells[0].shape[1], cells[0].shape[0]
    cols = min(12, len(cells))
    rows = (len(cells) + cols - 1) // cols
    sheet = Image.new("RGB", (cols * cw * z, rows * ch * z), bg)
    for i, c in enumerate(cells):
        im = Image.fromarray(c, "RGBA").resize((cw * z, ch * z), resample)
        sheet.paste(im, ((i % cols) * cw * z, (i // cols) * ch * z), im)
        if i in rest:
            x, y = (i % cols) * cw * z, (i // cols) * ch * z
            sheet.paste((255, 255, 255) if i != idle else (255, 60, 60), (x + 2, y + 2, x + 8, y + 8))
    sheet.save(path)


def gif(cells, path, fps=30, z=3, resample=Image.NEAREST):
    cw, ch = cells[0].shape[1], cells[0].shape[0]
    frames = []
    for c in cells:
        bg = Image.new("RGBA", (cw, ch), (214, 178, 140, 255))
        bg.alpha_composite(Image.fromarray(c, "RGBA"))
        frames.append(bg.resize((cw * z, ch * z), resample).convert("P", palette=Image.ADAPTIVE))
    frames[0].save(path, save_all=True, append_images=frames[1:], duration=int(1000 / fps), loop=0, disposal=2)


META_TEXTURE = open(os.path.join(os.path.dirname(__file__), "texture.meta.template"), encoding="utf8").read()
META_FOLDER = "fileFormatVersion: 2\nguid: GUID\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n"
META_TEXT = "fileFormatVersion: 2\nguid: GUID\nTextScriptImporter:\n  externalObjects: {}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n"


def ensure_meta(path, template):
    """Unity .meta with a GUID derived from the asset's path, so reruns never change it."""
    meta = path + ".meta"
    if True:  # always rewritten: import settings (filter, mipmaps, compression) come from the template
        guid = hashlib.md5(os.path.relpath(path, ROOT).replace("\\", "/").encode()).hexdigest()
        with open(meta, "w", encoding="utf8", newline="\n") as f:
            f.write(template.replace("GUID", guid))


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("cmd", choices=["audit", "process"])
    ap.add_argument("--only", default="")
    ap.add_argument("--style", choices=["illustrated", "pixel"], help="default: the style named in maids.json")
    args = ap.parse_args()
    only = [s for s in args.only.split(",") if s]
    cfg, maids = load_config(only, args.style)
    pixel = maids and maids[0]["style"] == "pixel"
    pv = dict(z=2, resample=Image.NEAREST) if pixel else dict(z=1, resample=Image.BICUBIC)
    write = args.cmd == "process"
    work = os.path.join(ROOT, "art_working", "_maid_walk_out")
    os.makedirs(work, exist_ok=True)
    results, catalog = [], []
    for m in maids:
        print("==", m["id"], flush=True)
        res, cells = run(m, write)
        if cells is None:
            results.append(res)
            continue
        res["status"], res["note"] = status_for(res, m)
        contact(cells, os.path.join(work, m["id"] + "_contact.png"), res["rest_frames"], res["idle_frame"], **pv)
        if res["status"] != "REJECTED" and m.get("pack", True):
            sheet = pack(cells, res["columns"], res["rows"], res["cell"][0], res["cell"][1])
            if write:
                os.makedirs(OUT_RES, exist_ok=True)
                sheet_path = os.path.join(OUT_RES, m["id"] + "_walk.png")
                sheet.save(sheet_path, optimize=True)
                res["resource"] = "Maids/" + m["id"] + "_walk"
                res["sheet_sha256"] = sha256(sheet_path)
                res["source_sha256"] = {os.path.basename(p): sha256(p) for p in prepare(m)}
                res["parameters"] = {k: m[k] for k in ("height_px", "colors", "style", "pad_px", "pixels_per_unit", "world_height")}
                d = os.path.join(OUT_DOCS, m["id"])
                os.makedirs(d, exist_ok=True)
                gif(cells, os.path.join(d, "walk_preview.gif"), z=3 if pixel else 1, resample=pv["resample"])
                contact(cells, os.path.join(d, "contact_sheet.png"), res["rest_frames"], res["idle_frame"], **pv)
                with open(os.path.join(d, "manifest.json"), "w", encoding="utf8") as f:
                    json.dump(res, f, indent=1)
        results.append(res)
        print(json.dumps({k: res[k] for k in res if k not in ("source_sha256",)}), flush=True)
    if write:
        # Merge into the existing catalog: entries of maids not processed this run stay untouched.
        cat_path = os.path.join(OUT_RES, "maid_walk_catalog.json")
        existing = {}
        if os.path.exists(cat_path):
            existing = {e["id"]: e for e in json.load(open(cat_path, encoding="utf8"))["maids"]}
        for leg in cfg["legacy"]:
            existing.setdefault(leg["id"], leg)
        for r in results:
            if r.get("resource"):
                m = next(x for x in maids if x["id"] == r["id"])
                existing[r["id"]] = {
                    "id": r["id"], "name": r["name"], "archetype": m["archetype"], "resource": r["resource"],
                    "frames": r["frames"], "columns": r["columns"], "rows": r["rows"], "cell": r["cell"],
                    "pixels_per_unit": m["pixels_per_unit"], "idle_frame": r["idle_frame"], "rest_frames": r["rest_frames"],
                    "units_per_cycle": r["units_per_cycle"],
                    "body_offset_y": m["body_offset_y"], "carried_offset_y": m["carried_offset_y"],
                    "source": "docs/art/maid-walk/%s/manifest.json" % r["id"]}
        order = [m["id"] for m in cfg["maids"]]
        entries = sorted(existing.values(), key=lambda e: order.index(e["id"]) if e["id"] in order else 99)
        with open(cat_path, "w", encoding="utf8", newline="\n") as f:
            json.dump({"maids": entries}, f, indent=1)
            f.write("\n")
    if write:
        for f in glob.glob(os.path.join(OUT_RES, "*_walk.png")):  # rewrites every .meta from the template
            ensure_meta(f, META_TEXTURE)
        ensure_meta(os.path.join(OUT_RES, "maid_walk_catalog.json"), META_TEXT)
        ensure_meta(OUT_RES, META_FOLDER)
    with open(os.path.join(work, "audit.json"), "w", encoding="utf8") as f:
        json.dump(results, f, indent=1)


if __name__ == "__main__":
    main()
