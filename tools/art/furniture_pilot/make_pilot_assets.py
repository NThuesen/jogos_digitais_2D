#!/usr/bin/env python3
"""Derive the ART-INT-01 pilot furniture sprites from the Codex raw PNGs (read-only source).

No new art is drawn: every output pixel comes from the source image. The steps are
  1. drop distant translucent haze (keep RGBA within HALO px of alpha >= 128, same rule as
     art_working/ui_modular/qa/remove_halo.py),
  2. crop to the illustrated area (not the generation canvas),
  3. resample to PIXELS_PER_UNIT * width_units,
  4. for koi_pond only: write the full image as the flat base and a copy with the water
     removed as the raised layer, so the two align pixel for pixel.

The width_units here must match Neko.Game.Art.IllustratedFurniture.

usage: make_pilot_assets.py <raw_gpt dir> <output dir>
"""
import sys
from pathlib import Path

import numpy as np
from PIL import Image
from scipy import ndimage as ndi

HALO = 6
PIXELS_PER_UNIT = 256

# id -> (source file, width in world units of the cropped art)
ASSETS = {
    "stage": ("UI-024_placeable_stage_v01.png", 2.08),
    "koi_pond": ("UI-024_placeable_koi_pond_v01.png", 2.12),
    "window": ("UI-024_placeable_window_v01.png", 0.92),
}


def clean_and_crop(path):
    rgba = np.array(Image.open(path).convert("RGBA"))
    solid = rgba[..., 3] >= 128
    near = ndi.binary_dilation(solid, structure=np.ones((3, 3), bool), iterations=HALO)
    rgba[~near, 3] = 0
    ys, xs = np.where(rgba[..., 3] > 0)
    return rgba[ys.min():ys.max() + 1, xs.min():xs.max() + 1]


def water_mask(rgba):
    """Pool surface including what floats on it (fish, lilies, lotus); not rocks, not the spout."""
    r, g, b, a = [rgba[..., i].astype(float) for i in range(4)]
    teal = (a > 200) & (g > r + 8) & (b > r + 8) & (np.abs(g - b) < 60)
    teal = ndi.binary_closing(teal, iterations=6)
    teal = ndi.binary_fill_holes(teal)
    # Opening detaches the thin waterfall ribbon, which is raised, from the pool.
    pool = ndi.binary_opening(teal, structure=np.ones((3, 3), bool), iterations=22)
    labels, count = ndi.label(pool)
    if count == 0:
        raise SystemExit("no water found")
    sizes = ndi.sum(pool, labels, range(1, count + 1))
    pool = labels == (1 + int(np.argmax(sizes)))
    pool = ndi.binary_fill_holes(pool)
    return ndi.binary_erosion(pool, iterations=3)


def save(rgba, width_units, out):
    width = round(width_units * PIXELS_PER_UNIT)
    height = round(rgba.shape[0] * width / rgba.shape[1])
    image = Image.fromarray(rgba, "RGBA").resize((width, height), Image.LANCZOS)
    image.save(out, optimize=True)
    print(f"{out.name}: {width}x{height}px, {width_units} units wide")


def main(source, target):
    source, target = Path(source), Path(target)
    target.mkdir(parents=True, exist_ok=True)

    for name, (file, width_units) in ASSETS.items():
        art = clean_and_crop(source / file)
        save(art, width_units, target / f"{name}.png")

        if name != "koi_pond":
            continue

        mask = ndi.gaussian_filter(water_mask(art).astype(float), 1.2)
        raised = art.copy()
        raised[..., 3] = np.round(raised[..., 3] * (1.0 - np.clip(mask, 0, 1))).astype(np.uint8)
        save(raised, width_units, target / "koi_pond_raised.png")


if __name__ == "__main__":
    main(*sys.argv[1:3])
