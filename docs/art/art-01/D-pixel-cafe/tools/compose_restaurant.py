"""Mockup: restaurant (D3) with Suzu chibis at 64/96/128 px. Usage: compose_restaurant.py <D-pixel-cafe dir> [outdir]"""
import sys, os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import numpy as np
from PIL import Image, ImageDraw, ImageFilter
from artlib import *

ROOT = sys.argv[1]
OUT = sys.argv[2] if len(sys.argv) > 2 else f"{ROOT}/review"
W, H = 960, 600
cafe = Image.open(f"{ROOT}/incoming/D3_cafe_interior.png").convert("RGB")
bg, _, _ = cover(cafe, (W, H))
P = {n: Image.open(f"{ROOT}/processed/D2_chibi_{n}_cutout.png").convert("RGBA") for n in ("idle", "walk", "carry")}

# feet positions (canvas px) on the open floor; x = centre of the sprite
SPOTS = {"idle": (400, 500), "walk": (612, 512), "carry": (506, 580)}


def shadow(canvas, cx, fy, w, h):
    sh = Image.new("RGBA", canvas.size, (0, 0, 0, 0))
    d = ImageDraw.Draw(sh)
    d.ellipse([cx - w * 0.38, fy - h * 0.045, cx + w * 0.38, fy + h * 0.06], fill=(40, 18, 10, 120))
    canvas.alpha_composite(sh.filter(ImageFilter.GaussianBlur(1.2)))


def place(canvas, name, h, spot, mirror=False):
    spr = to_height(P[name], h)
    if mirror: spr = spr.transpose(Image.FLIP_LEFT_RIGHT)
    cx, fy = spot
    shadow(canvas, cx, fy, spr.width, h)
    canvas.alpha_composite(spr, (int(cx - spr.width / 2), int(fy - h)))


def frame(h, label=True, sizes=None):
    c = bg.convert("RGBA")
    sz = sizes or {"idle": h, "walk": h, "carry": h}
    # draw back-to-front by feet y
    items = [("idle", SPOTS["idle"], False), ("walk", SPOTS["walk"], False), ("carry", SPOTS["carry"], True)]
    for n, sp, mir in sorted(items, key=lambda t: t[1][1]):
        place(c, n, sz[n], sp, mir)
    if label:
        draw_text(c, (14, 14), "MOCKUP ART-01D · D3 + chibis processados · altura de exibição dos chibis: " + (f"{h} px" if not sizes else "64 / 96 / 128 px por profundidade"), 13, CREAM_TXT, OUTLINE)
        draw_text(c, (14, 584), "arte gerada + composição estática · sombra e escala escolhidas por mim · carry espelhado · não é captura Unity", 11, CREAM_TXT, OUTLINE, anchor="ls")
    return c


for h in (64, 96, 128):
    f = frame(h)
    f.convert("RGB").save(f"{OUT}/06_restaurante_{h}px.png")
d = frame(0, sizes={"idle": 64, "walk": 96, "carry": 128})
d.convert("RGB").save(f"{OUT}/06_restaurante_profundidade.png")

# comparison board: 1:1 crops of the walkway + 2x zoom of the idle chibi
crop = (300, 340, 760, 596)
rows = []
for h in (64, 96, 128):
    f = frame(h, label=False).convert("RGB")  # unlabelled so crops carry no caption text
    c1 = f.crop(crop)
    ix = SPOTS["idle"][0]
    z = f.crop((ix - 70, SPOTS["idle"][1] - 150, ix + 70, SPOTS["idle"][1] + 12)).resize((280, 324), Image.NEAREST)
    rows.append((h, c1, z))
BW = crop[2] - crop[0] + 12 + 280 + 40
board = Image.new("RGBA", (BW, 70 + sum(max(r[1].height, r[2].height) + 34 for r in rows)), (43, 38, 35, 255))
draw_text(board, (14, 10), "ART-01D · restaurante: chibis a 64 / 96 / 128 px (recorte 1:1 do 960×600 + zoom 2× do chibi idle)", 16, CREAM_TXT)
draw_text(board, (14, 36), "Escala real do canvas 960×600; não é Unity. Sombra de contato é adereço do mockup.", 12, (200, 190, 175, 255))
y = 66
for h, c1, z in rows:
    draw_text(board, (14, y), f"{h} px", 15, (255, 200, 120, 255))
    board.alpha_composite(c1.convert("RGBA"), (14, y + 24)); board.alpha_composite(z.convert("RGBA"), (14 + c1.width + 12, y + 24))
    y += max(c1.height, z.height) + 34
board.convert("RGB").save(f"{OUT}/07_restaurante_comparacao.png")
print("done", board.size)
