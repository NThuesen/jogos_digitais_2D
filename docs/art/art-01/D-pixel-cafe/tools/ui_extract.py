"""Extract D4 UI-kit pieces as separate RGBA PNGs (ART-01D). Usage: ui_extract.py <D-pixel-cafe dir>"""
import sys, os, numpy as np
from PIL import Image, ImageDraw
from scipy import ndimage as ndi

ROOT = sys.argv[1]
SRC = Image.open(f"{ROOT}/incoming/D4_ui_kit.png").convert("RGB")
OUT = f"{ROOT}/processed/ui"
os.makedirs(OUT, exist_ok=True)
rgb = np.asarray(SRC).astype(int)

# bounding boxes measured on the 1536x1024 sheet: name -> (x0, y0, x1, y1)
PIECES = {
    "frame_large": (30, 26, 446, 464),
    "frame_wide": (454, 26, 1506, 304),
    "frame_label": (30, 468, 446, 606),
    "balloon": (454, 324, 1058, 574),
    "icon_book": (1068, 314, 1212, 452), "icon_paw": (1214, 314, 1358, 452), "icon_cup": (1358, 314, 1502, 452),
    "icon_clock": (1068, 460, 1212, 598), "icon_gear": (1214, 460, 1358, 598), "icon_star": (1358, 460, 1502, 598),
    "bar_toggles": (30, 610, 1506, 720),
    "rarity_common": (44, 742, 256, 958), "rarity_rare": (272, 742, 502, 958),
    "rarity_epic": (512, 742, 752, 958), "rarity_legendary": (752, 716, 1040, 970),
    "pill_normal": (1094, 744, 1466, 846), "pill_pressed": (1094, 852, 1466, 956),
}


def is_bg_tone(p):
    r, g, b = p[..., 0], p[..., 1], p[..., 2]
    return (r >= 150) & (r <= 222) & (g >= 70) & (g <= 135) & (b >= 70) & (b <= 128) & (r - g >= 50) & (np.abs(g - b) <= 22)


def extract(box, min_island=200):
    x0, y0, x1, y1 = box
    c = rgb[y0:y1, x0:x1]
    cand = is_bg_tone(c)
    lab, n = ndi.label(cand)
    edge = np.concatenate([lab[0], lab[-1], lab[:, 0], lab[:, -1]])
    ext = np.isin(lab, np.unique(edge[edge > 0]))
    fg = ~ext
    # defringe: outermost ring pixels that still look like background tone
    er = ndi.binary_erosion(fg, iterations=1)
    ring = fg & ~er
    fg &= ~(ring & cand)
    lab2, n2 = ndi.label(fg, structure=np.ones((3, 3)))
    sizes = ndi.sum(fg, lab2, range(1, n2 + 1))
    big = sizes.max()
    for i, s in enumerate(sizes, 1):
        if s < (min_island if s != big else 0) and s < big * 0.02:
            fg[lab2 == i] = False
    a = np.dstack([c.astype(np.uint8), (fg * 255).astype(np.uint8)])
    ys, xs = np.where(fg)
    a = a[ys.min():ys.max() + 1, xs.min():xs.max() + 1]
    return a


def checker(w, h, s=12):
    im = Image.new("RGB", (w, h), (205, 205, 205)); d = ImageDraw.Draw(im)
    for y in range(0, h, s):
        for x in range(0, w, s):
            if (x // s + y // s) % 2: d.rectangle([x, y, x + s - 1, y + s - 1], fill=(150, 150, 150))
    return im


if __name__ == "__main__":
    report = {}
    for name, box in PIECES.items():
        a = extract(box, min_island=(15 if name == "rarity_legendary" else 200))
        Image.fromarray(a, "RGBA").save(f"{OUT}/{name}.png")
        report[name] = a.shape[1::-1]
        print(name, a.shape[1], "x", a.shape[0], "opaque %.0f%%" % ((a[..., 3] > 0).mean() * 100))
    if len(sys.argv) < 3: sys.exit(0)  # optional contact sheet only when an output path is given
    names = list(PIECES)
    ims = [Image.open(f"{OUT}/{n}.png") for n in names]
    W = 1500; x = y = 8; rowh = 0; placed = []
    for n, im in zip(names, ims):
        if x + im.width + 8 > W: x = 8; y += rowh + 8; rowh = 0
        placed.append((n, im, x, y)); x += im.width + 8; rowh = max(rowh, im.height)
    sheet = checker(W, y + rowh + 8)
    s = sheet.convert("RGBA")
    for n, im, px, py in placed: s.alpha_composite(im, (px, py))
    s.convert("RGB").save(sys.argv[2])
