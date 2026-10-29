import sys, numpy as np
from PIL import Image, ImageDraw, ImageFont
proc, inc, outpath = sys.argv[1:4]
F = ImageFont.truetype("C:/Windows/Fonts/arialbd.ttf", 16)
bgc = (60, 56, 52)


def over(im):
    b = Image.new("RGBA", im.size, bgc + (255,)); b.alpha_composite(im); return b.convert("RGB")


tiles = []
P = Image.open(f"{proc}/D1_portrait_cutout.png")
tiles.append(("retrato", over(P.crop((560, 0, 800, 200)))))
S = Image.open(f"{inc}/D0_suzu_sheet.png").convert("RGB")
tiles.append(("sheet D0 (busto)", S.crop((470, 20, 570, 120)).resize((240, 240), Image.NEAREST)))
for n in ("idle", "walk", "carry"):
    C = Image.open(f"{proc}/D2_chibi_{n}_cutout.png")
    # find top-right ear: topmost opaque pixel in the right half of the head region
    a = np.asarray(C)[..., 3] > 0
    w = a.shape[1]
    right = a[:200, w // 2:]
    ys, xs = np.where(right)
    # ear tip = min y in right half
    i = ys.argmin(); tx, ty = xs[i] + w // 2, ys[i]
    box = (max(tx - 120, 0), max(ty - 10, 0), min(tx + 70, w), ty + 190)
    tiles.append((f"chibi {n}", over(C.crop(box)).resize((240, 240), Image.NEAREST)))
out = Image.new("RGB", (len(tiles) * 250, 270), (30, 28, 26))
d = ImageDraw.Draw(out)
for i, (t, im) in enumerate(tiles):
    im = im.resize((240, 240), Image.NEAREST) if im.size != (240, 240) else im
    out.paste(im, (i * 250 + 5, 25)); d.text((i * 250 + 5, 3), t, font=F, fill=(255, 220, 160))
out.save(outpath)
