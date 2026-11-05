"""Conservative cleanup and derived crops for the Suzu v03 portrait (QA only).

Reads the RAW (never modified), writes derived PNGs. No regeneration, no resampling of the
character other than the final downscale. Usage: python tools/art/suzu_v03_prepare.py
"""
import sys
from pathlib import Path
import numpy as np
from PIL import Image, ImageFilter
from scipy import ndimage as ndi

ROOT = Path(__file__).resolve().parents[2]
RAW = ROOT / "art_working/ui_modular/raw_gpt/021_maid_portrait_suzu_neutral_v03.png"
OUT = ROOT / "Assets/_Game/Tests/PlayMode/QaArt"
MARGIN = 12          # transparent margin kept around the full portrait
MIN_ALPHA = 10       # below this the pixel is noise, not character
MIN_AREA = 120       # connected specks smaller than this are not character

im = Image.open(RAW).convert("RGBA")
a = np.array(im)
alpha = a[..., 3].copy()

# 1. alpha noise, then isolated specks (everything not part of a real shape)
alpha[alpha < MIN_ALPHA] = 0
lab, n = ndi.label(alpha > 0)
areas = ndi.sum(alpha > 0, lab, range(1, n + 1))
for i, area in enumerate(areas, 1):
    if area < MIN_AREA:
        alpha[lab == i] = 0
removed = int((a[..., 3] > 0).sum() - (alpha > 0).sum())

# 2. bleed edge colour into the transparent area, so filtering/mips never pull in black
rgb = a[..., :3].copy()
solid = alpha > 0
idx = ndi.distance_transform_edt(~solid, return_distances=False, return_indices=True)
bled = rgb[idx[0], idx[1]]
rgb = np.where(solid[..., None], rgb, bled)
clean = np.dstack([rgb, alpha]).astype(np.uint8)

# 3. trim to the character and give it a small transparent margin (the top edge touched it)
ys, xs = np.where(alpha > 0)
x0, x1, y0, y1 = xs.min(), xs.max() + 1, ys.min(), ys.max() + 1
padded = np.zeros((y1 - y0 + MARGIN, x1 - x0 + 2 * MARGIN, 4), np.uint8)
body = clean[y0:y1, x0:x1]
padded[MARGIN:MARGIN + body.shape[0], MARGIN:MARGIN + body.shape[1]] = body
# bleed colour into the new margin as well
fa = padded[..., 3] > 0
ix = ndi.distance_transform_edt(~fa, return_distances=False, return_indices=True)
padded[..., :3] = np.where(fa[..., None], padded[..., :3], padded[ix[0], ix[1]][..., :3])
full = Image.fromarray(padded, "RGBA")
W, H = full.size
print("raw", im.size, "-> clean", full.size, "speck px removed", removed)

DOCS = ROOT / "docs/art/ui-modular/qa-suzu-v03"
DOCS.mkdir(parents=True, exist_ok=True)

def save(img, name, folder=OUT):
    img.save(folder / name, optimize=True)
    print(name, img.size)

# Work on a canvas with extra transparent room (colour bled in), so a crop may start above the
# image: the ear tips touched the top edge of the RAW and would sit flush with the card window.
PAD = 100
canvas = Image.new("RGBA", (W + 2 * PAD, H + 2 * PAD))
arr = np.zeros((H + 2 * PAD, W + 2 * PAD, 4), np.uint8)
arr[PAD:PAD + H, PAD:PAD + W] = np.array(full)
solid2 = arr[..., 3] > 0
ix2 = ndi.distance_transform_edt(~solid2, return_distances=False, return_indices=True)
arr[..., :3] = np.where(solid2[..., None], arr[..., :3], arr[ix2[0], ix2[1]][..., :3])
canvas = Image.fromarray(arr, "RGBA")

def crop(box, size, sharpen=False):
    x0, y0, x1, y1 = box
    c = canvas.crop((x0 + PAD, y0 + PAD, x1 + PAD, y1 + PAD)).resize(size, Image.LANCZOS)
    if sharpen:
        a = c.getchannel("A")
        c = c.convert("RGB").filter(ImageFilter.UnsharpMask(radius=0.8, percent=35, threshold=2))
        c.putalpha(a)
    return c

# Derived crops, all from the same cleaned image, never stretched (box aspect == output aspect).
# Head crop: tiara, both ears, bells and bows down to the choker bell, with air above the ear
# tips and beside the hair; aspect 1.22 = the card's portrait area.
hw = 1060
hh = int(round(hw / 1.22))   # 869
head = (20, -50, 20 + hw, -50 + hh)
save(crop(head, (192, 157), sharpen=True), "suzu_v03_head_192.png")
# Detail: the whole portrait (uniform, apron, bow, tail), 3:4
save(full.resize((480, int(round(480 * H / W))), Image.LANCZOS), "suzu_v03_detail_480.png")
save(full, "UI-021_suzu_v03_clean.png", DOCS)
print("head box", head, "aspect", hw / hh)
