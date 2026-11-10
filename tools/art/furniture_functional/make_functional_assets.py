#!/usr/bin/env python3
"""Derive the ART-INT-02 functional furniture sprites from the ART-PROD-02C CLEAN exports.

Read-only source: <delivery>/export_unity/UI-024_placeable_<id>_clean_c01.png. No pixel is drawn:
  1. the SHA-256 of every source is checked against <delivery>/INVENTORY.json (abort on mismatch),
  2. crop to the illustrated area (alpha >= 8), not the generation canvas,
  3. resample with premultiplied alpha (no dark fringe) to PIXELS_PER_UNIT * width_units.

width_units must match Neko.Game.Art.IllustratedFurniture. Prints the pixel sizes the entries need.

usage: make_functional_assets.py <delivery dir> <output dir>
"""
import hashlib, json, sys
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw

PIXELS_PER_UNIT = 256

# output name -> (source id, width in world units of the cropped art)
ASSETS = json.loads(Path(__file__).with_name("calibration.json").read_text())


def crop(path):
    rgba = np.array(Image.open(path).convert("RGBA"))
    ys, xs = np.where(rgba[..., 3] >= 8)
    return rgba[ys.min():ys.max() + 1, xs.min():xs.max() + 1]


def resample(rgba, width, height):
    f = rgba.astype(np.float64)
    a = f[..., 3:4] / 255.0
    pre = np.concatenate([f[..., :3] * a, f[..., 3:4]], axis=2)
    chans = [Image.fromarray(pre[..., i].astype(np.float32), "F").resize((width, height), Image.LANCZOS)
             for i in range(4)]
    out = np.stack([np.asarray(c) for c in chans], axis=2)
    alpha = np.clip(out[..., 3], 0, 255)
    safe = np.where(alpha > 0.5, alpha / 255.0, 1.0)[..., None]
    rgb = np.clip(out[..., :3] / safe, 0, 255)
    rgb[alpha <= 0.5] = 0
    return np.dstack([rgb, alpha]).round().astype(np.uint8)


def front_layer(full, rule):
    """Same size and pivot as the whole picture; only what stands in front of seated customers.

    above_y: fraction of the height; everything above it (the chairs behind the top) is removed.
    remove_polygons: polygons in width/height fractions removed from the picture (the chairs
    behind a four-seater's top). Nothing is drawn: pixels are only made transparent.
    """
    out = full.copy()
    h, w = out.shape[:2]
    if "above_y" in rule:
        out[: round(rule["above_y"] * h), :, 3] = 0
    for poly in rule.get("remove_polygons", []):
        mask = Image.new("L", (w, h), 0)
        ImageDraw.Draw(mask).polygon([(x * w, y * h) for x, y in poly], fill=255)
        out[np.asarray(mask) > 0, 3] = 0
    return out


def main(delivery, target):
    delivery, target = Path(delivery), Path(target)
    inventory = {Path(e["path"]).name: e["sha256"] for e in json.loads((delivery / "INVENTORY.json").read_text())}
    target.mkdir(parents=True, exist_ok=True)
    for name, spec in ASSETS.items():
        source_id, width_units = spec["source"], spec["width"]
        file = f"UI-024_placeable_{source_id}_clean_c01.png"
        src = delivery / "export_unity" / file
        digest = hashlib.sha256(src.read_bytes()).hexdigest()
        if digest != inventory.get(file):
            raise SystemExit(f"HASH MISMATCH {file}")
        art = crop(src)
        width = round(width_units * PIXELS_PER_UNIT)
        height = round(art.shape[0] * width / art.shape[1])
        full = resample(art, width, height)
        Image.fromarray(full, "RGBA").save(target / f"{name}.png", optimize=True)
        if "front" in spec:
            Image.fromarray(front_layer(full, spec["front"]), "RGBA").save(target / f"{name}_front.png", optimize=True)
        print(f'{name}: {width}x{height}px, {width_units} units wide, source sha256 {digest}')


if __name__ == "__main__":
    main(*sys.argv[1:3])
