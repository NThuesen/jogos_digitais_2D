import sys, os, numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from seg import *
from PIL import Image, ImageDraw, ImageFont
from scipy import ndimage as ndi

ROOT = sys.argv[1]  # .../D-pixel-cafe
INC, PROC, REV = f"{ROOT}/incoming", f"{ROOT}/processed", f"{ROOT}/review"
os.makedirs(PROC, exist_ok=True); os.makedirs(REV, exist_ok=True)
F = lambda s: ImageFont.truetype("C:/Windows/Fonts/arial.ttf", s)
FB = lambda s: ImageFont.truetype("C:/Windows/Fonts/arialbd.ttf", s)
BOARD = (43, 38, 35)


def bbox_crop(out, pad=4):
    a = out[..., 3] > 0
    ys, xs = np.where(a)
    y0, y1, x0, x1 = max(ys.min() - pad, 0), min(ys.max() + pad + 1, a.shape[0]), max(xs.min() - pad, 0), min(xs.max() + pad + 1, a.shape[1])
    return out[y0:y1, x0:x1], (x0, y0, x1, y1)


# ---------- cutouts ----------
rgbP = load(f"{INC}/D1_portrait.png"); bgP = bg_color(rgbP)
outP, fgP, _ = cutout(rgbP, bgP, tol=32, defringe=1, seed_bottom=False)
Image.fromarray(outP, "RGBA").save(f"{PROC}/D1_portrait_cutout.png")

rgbC = load(f"{INC}/D2_chibi_poses.png"); bgC = bg_color(rgbC)
outC, fgC, _ = cutout(rgbC, bgC, tol=32, defringe=1)
cols = fgC.any(axis=0)
lab, n = ndi.label(ndi.binary_closing(cols, iterations=8))
print("chibi column groups:", n)
poses = {}
for i, name in enumerate(["idle", "walk", "carry"], 1):
    xs = np.where(lab == i)[0]
    sub = outC.copy(); m = np.zeros(fgC.shape, bool); m[:, xs.min():xs.max() + 1] = True
    sub[..., 3] = np.where(m, sub[..., 3], 0)
    c, box = bbox_crop(sub)
    poses[name] = c
    Image.fromarray(c, "RGBA").save(f"{PROC}/D2_chibi_{name}_cutout.png")
    print(name, "bbox", box, "size", c.shape[1], "x", c.shape[0], "opaque px", int((c[..., 3] > 0).sum()))

# alpha-edge sanity: any partially transparent pixels?
print("alpha values in outputs:", np.unique(outP[..., 3]))


# ---------- helpers ----------
def over(arr, color):
    im = Image.new("RGBA", (arr.shape[1], arr.shape[0]), color + (255,))
    im.alpha_composite(Image.fromarray(arr, "RGBA"))
    return im.convert("RGB")


def checker(w, h, s=16):
    im = Image.new("RGB", (w, h), (200, 200, 200)); d = ImageDraw.Draw(im)
    for y in range(0, h, s):
        for x in range(0, w, s):
            if (x // s + y // s) % 2: d.rectangle([x, y, x + s - 1, y + s - 1], fill=(150, 150, 150))
    return im


def over_checker(arr):
    im = checker(arr.shape[1], arr.shape[0]).convert("RGBA")
    im.alpha_composite(Image.fromarray(arr, "RGBA"))
    return im.convert("RGB")


def fit(im, h):
    return im.resize((round(im.width * h / im.height), h), Image.LANCZOS)


def label(d, xy, text, size=18, fill=(240, 235, 225), bold=False):
    d.text(xy, text, font=(FB if bold else F)(size), fill=fill)


def paste_row(board, tiles, x, y, gap=14):
    for t in tiles:
        board.paste(t, (x, y)); x += t.width + gap
    return x


# ---------- Board 1: original vs processed ----------
H = 520
cafe = (233, 205, 170)
dark = (30, 28, 40)
specs = [("Retrato (D1)", Image.open(f"{INC}/D1_portrait.png").convert("RGB"), outP)] + \
        [(f"Chibi {n} (D2)", None, poses[n]) for n in poses]
tiles_rows = []
for title, orig, arr in specs:
    if orig is None:
        c = Image.fromarray(arr, "RGBA")
        bg = Image.fromarray(np.dstack([np.full(arr.shape[:2] + (3,), bgC, np.uint8), np.full(arr.shape[:2], 255, np.uint8)]), "RGBA")
        bg.alpha_composite(c)
        # original context: the exact pixels as generated (flat bg + figure) cropped to the same bbox
        orig_t = None
    tiles_rows.append((title, orig, arr))

# Build chibi originals crop using the same boxes
chibi_orig_crops = {}
rgbC_img = Image.open(f"{INC}/D2_chibi_poses.png").convert("RGB")
for name in poses:
    arr = poses[name]
    # locate by matching alpha bbox in cutout of sheet (recompute)
    pass

# recompute boxes to crop originals
boxes = {}
for i, name in enumerate(["idle", "walk", "carry"], 1):
    xs = np.where(lab == i)[0]
    m = np.zeros(fgC.shape, bool); m[:, xs.min():xs.max() + 1] = True
    a = (fgC & m)
    ys2, xs2 = np.where(a)
    boxes[name] = (max(xs2.min() - 4, 0), max(ys2.min() - 4, 0), min(xs2.max() + 5, fgC.shape[1]), min(ys2.max() + 5, fgC.shape[0]))

rows = []
for title, orig, arr in tiles_rows:
    if orig is None:
        name = title.split()[1]
        orig_im = rgbC_img.crop(boxes[name])
    else:
        orig_im = orig
    tiles = [fit(orig_im, H), fit(over(arr, (255, 0, 255)), H), fit(over(arr, dark), H), fit(over_checker(arr), H), fit(over(arr, cafe), H)]
    rows.append((title, tiles))

captions = ["ORIGINAL (arte gerada)", "PROCESSADO s/ magenta", "s/ fundo escuro", "s/ xadrez", "s/ cor de café"]
# row layout: portrait row separate, then 3 chibis in a combined view (original|magenta|dark)
W = max(sum(t.width for t in tiles) + 14 * 4 for _, tiles in rows) + 40
board1 = Image.new("RGB", (W, 40 + sum(H + 70 for _ in rows) + 20), BOARD)
d = ImageDraw.Draw(board1)
label(d, (20, 8), "ART-01D — Recorte: ORIGINAL (arte gerada) vs PROCESSADO — nenhuma imagem é Unity", 22, bold=True)
y = 50
for title, tiles in rows:
    label(d, (20, y), title, 20, bold=True)
    x = 20
    for t, cap in zip(tiles, captions):
        board1.paste(t, (x, y + 30))
        label(d, (x, y + 30 + H + 4), cap, 14)
        x += t.width + 14
    y += H + 70
board1.save(f"{REV}/01_recorte_original_vs_processado.png")
print("board1", board1.size)


# ---------- Board 2: size ladder ----------
def small(arr, h, method):
    im = Image.fromarray(arr, "RGBA")
    w = max(1, round(im.width * h / im.height))
    if method == "nearest":
        r = im.resize((w, h), Image.NEAREST)
    else:
        flt = {"area": Image.BOX, "lanczos": Image.LANCZOS}[method]
        r = im.convert("RGBa").resize((w, h), flt).convert("RGBA")  # premultiplied: no bg halo
    a = np.asarray(r).copy()
    a[..., 3] = np.where(a[..., 3] >= 128, 255, 0)  # hard alpha edge, like pixel art
    return a


def quant(a, colors):
    rgb = Image.fromarray(a[..., :3], "RGB").quantize(colors=colors, method=Image.MEDIANCUT, dither=Image.NONE).convert("RGB")
    q = np.asarray(rgb).copy()
    return np.dstack([q, a[..., 3]])


def zoomed(a, z, bgc=cafe):
    im = over(a, bgc)
    return im.resize((im.width * z, im.height * z), Image.NEAREST)


variants = [("40 px · área", 40, "area", None, 6), ("48 px · área", 48, "area", None, 6), ("48 px · nearest", 48, "nearest", None, 6),
            ("48 px · lanczos", 48, "lanczos", None, 6), ("48 px · área + 24 cores", 48, "area", 24, 6),
            ("64 px · área", 64, "area", None, 4), ("96 px · área", 96, "area", None, 3), ("128 px · área", 128, "area", None, 2)]
cells = {}
for pn, arr in poses.items():
    for vt, h, m, q, z in variants:
        a = small(arr, h, m)
        if q: a = quant(a, q)
        cells[(pn, vt)] = (zoomed(a, z), a)
cw = max(c[0].width for c in cells.values()) + 12
ch = max(c[0].height for c in cells.values()) + 34
cols_n = len(variants)
board2 = Image.new("RGB", (cols_n * cw + 40, 70 + 3 * ch + 220), BOARD)
d = ImageDraw.Draw(board2)
label(d, (20, 8), "ART-01D — Chibi em escala (altura visual testada; NÃO aprovada) — ampliado com nearest para inspeção", 22, bold=True)
label(d, (20, 38), "Ampliações: 40–48 px = 6×, 64 px = 4×, 96 px = 3×, 128 px = 2×. Alfa binarizado (borda dura).", 15)
for ri, pn in enumerate(poses):
    for ci, (vt, *_r) in enumerate(variants):
        im, _a = cells[(pn, vt)]
        x, y = 20 + ci * cw, 70 + ri * ch
        board2.paste(im, (x, y + 22))
        if ri == 0: label(d, (x, y), vt, 14, bold=True)
    label(d, (20, 70 + ri * ch - 18) if ri else (880, 8), "", 1)
    label(d, (20 + 0, 70 + ri * ch + 0), "", 1)
    d.text((20, 70 + ri * ch + ch - 26), "pose: " + pn, font=FB(14), fill=(255, 200, 120))
# 1x strip, actual pixel size, on cafe-floor colour
sy = 70 + 3 * ch + 14
label(d, (20, sy), "Tamanho real (1×, sem ampliar) — 40 / 48 / 64 / 96 px, 3 poses", 15, bold=True)
strip = Image.new("RGB", (cols_n * cw, 180), cafe)
x = 10
for h in (40, 48, 64, 96):
    for pn, arr in poses.items():
        a = small(arr, h, "area")
        im = over(a, cafe)
        strip.paste(im, (x, 10 + (96 - h))); x += im.width + 10
    x += 30
board2.paste(strip.crop((0, 0, 820, 125)), (20, sy + 26))
board2 = board2.crop((0, 0, board2.width, sy + 26 + 125 + 14))
board2.save(f"{REV}/02_chibi_escala.png")
print("board2", board2.size)

# ---------- Board 3: identity ----------
sheet = Image.open(f"{INC}/D0_suzu_sheet.png").convert("RGB")
sw = [(x, 961) for x in (80, 138, 195, 253, 311, 368, 425, 483, 541)]
declared = [tuple(int(v) for v in np.asarray(sheet.crop((x - 4, y - 4, x + 4, y + 4))).reshape(-1, 3).mean(axis=0)) for x, y in sw]


def palette(arr, k=16):
    m = arr[..., 3] > 0
    px = arr[..., :3][m]
    im = Image.fromarray(px.reshape(-1, 1, 3).astype(np.uint8), "RGB").quantize(colors=k, method=Image.MEDIANCUT, dither=Image.NONE)
    pal = np.asarray(im.getpalette()[:k * 3]).reshape(-1, 3)
    idx = np.asarray(im).reshape(-1)
    share = np.bincount(idx, minlength=k) / len(idx)
    order = np.argsort(-share)
    return [(tuple(int(v) for v in pal[i]), float(share[i])) for i in order if share[i] > 0]


palP, palC = palette(outP), palette(poses["idle"])
headP = Image.fromarray(outP, "RGBA").crop((60, 0, 970, 700))
ci = poses["idle"]; headC = Image.fromarray(ci, "RGBA").crop((0, 0, ci.shape[1], int(ci.shape[0] * 0.60)))
hh = 430
tp, tc = fit(over(np.asarray(headP), (60, 56, 52)), hh), fit(over(np.asarray(headC), (60, 56, 52)), hh)
board3 = Image.new("RGB", (max(1500, tp.width + tc.width + 60), 40 + hh + 420), BOARD)
d = ImageDraw.Draw(board3)
label(d, (20, 8), "ART-01D — Identidade: retrato (D1) vs chibi idle (D2), mesma altura de cabeça", 22, bold=True)
board3.paste(tp, (20, 44)); board3.paste(tc, (40 + tp.width, 44))
label(d, (20, 44 + hh + 2), "Retrato — cabeça/busto", 14); label(d, (40 + tp.width, 44 + hh + 2), "Chibi idle — cabeça", 14)
y = 44 + hh + 34


def swatches(row, y, name):
    label(d, (20, y), name, 14, bold=True)
    x = 220
    for c, s in row:
        d.rectangle([x, y, x + 56, y + 26], fill=c, outline=(255, 255, 255))
        label(d, (x, y + 28), ("#%02X%02X%02X" % c) + (" %d%%" % round(s * 100) if s else ""), 10); x += 76


swatches([(c, 0.0) for c in declared], y, "Swatches declarados (D0)")
swatches(palP, y + 60, "Paleta medida retrato")
swatches(palC, y + 120, "Paleta medida chibi idle")


def nearest(c, pal):
    best = min(pal, key=lambda p: sum((a - b) ** 2 for a, b in zip(p[0], c)))
    return best[0], sum((a - b) ** 2 for a, b in zip(best[0], c)) ** 0.5


label(d, (20, y + 190), "Distância RGB da cor declarada (D0) até a cor mais próxima medida (0 = igual; >40 ≈ visível)", 14, bold=True)
txt = []
for i, c in enumerate(declared):
    _, dp = nearest(c, palP); _, dc = nearest(c, palC)
    txt.append("sw%d #%02X%02X%02X -> retrato %.0f · chibi %.0f" % (i + 1, *c, dp, dc))
    print(txt[-1])
for i, t in enumerate(txt):
    label(d, (20 + (i % 3) * 480, y + 215 + (i // 3) * 22), t, 14)
board3 = board3.crop((0, 0, board3.width, y + 215 + 3 * 22 + 12))
board3.save(f"{REV}/03_identidade_retrato_chibi.png")
print("board3", board3.size)
