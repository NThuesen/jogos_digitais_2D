"""Knock out the baked-in interior of D4 frames so they work as overlays. Usage: ui_window.py <D-pixel-cafe dir>"""
import sys, os, json, numpy as np
from PIL import Image
from scipy import ndimage as ndi

ROOT = sys.argv[1]
UI = f"{ROOT}/processed/ui"
FRAMES = ["frame_large", "frame_wide", "frame_label", "rarity_common", "rarity_rare", "rarity_epic", "rarity_legendary"]


def interior_mask(a):
    rgb = a[..., :3].astype(int)
    h, w = a.shape[:2]
    cream = (rgb[..., 0] > 222) & (rgb[..., 1] > 190) & (rgb[..., 2] > 140) & (a[..., 3] > 0)
    lab, n = ndi.label(cream)
    c = lab[h // 2, w // 2]
    if c == 0:  # centre hit a paw/cat watermark: take the largest cream component
        sizes = ndi.sum(cream, lab, range(1, n + 1)); c = int(np.argmax(sizes)) + 1
    m = lab == c
    m = ndi.binary_fill_holes(m)       # swallow watermark islands (paws, cat silhouette)
    m = ndi.binary_closing(m, iterations=2)
    m = ndi.binary_fill_holes(m)
    return m


manifest = {}
for name in FRAMES:
    a = np.asarray(Image.open(f"{UI}/{name}.png").convert("RGBA")).copy()
    m = interior_mask(a)
    ys, xs = np.where(m)
    h, w = a.shape[:2]
    box = [int(xs.min()), int(ys.min()), int(xs.max()) + 1, int(ys.max()) + 1]
    # shrink the hole by 1px so the inner line is never eaten
    hole = ndi.binary_erosion(m, iterations=1)
    a[hole, 3] = 0
    Image.fromarray(a, "RGBA").save(f"{UI}/{name}_window.png")
    manifest[name] = {"size": [w, h], "interior": box, "margins_ltrb": [box[0], box[1], w - box[2], h - box[3]], "interior_share": round(float(m.mean()), 2)}
    print(name, manifest[name])
json.dump(manifest, open(f"{UI}/ui_manifest.json", "w"), indent=1)
