"""Mockup: Escalacao 960x600 from D3 + D4 pieces + Suzu cutouts. Usage: compose_lineup.py <D-pixel-cafe dir> [out.png]"""
import sys, numpy as np
from PIL import Image, ImageDraw, ImageEnhance
import os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from artlib import *

ROOT = sys.argv[1]
OUT = sys.argv[2] if len(sys.argv) > 2 else f"{ROOT}/review/05_escalacao_960x600.png"
W, H = 960, 600
kit = Kit(f"{ROOT}/processed/ui")
chibi = Image.open(f"{ROOT}/processed/D2_chibi_idle_cutout.png").convert("RGBA")
portrait = Image.open(f"{ROOT}/processed/D1_portrait_cutout.png").convert("RGBA")

# --- background: D3 cafe, darkened so the cards read first (colour and detail untouched) ---
cafe = Image.open(f"{ROOT}/incoming/D3_cafe_interior.png").convert("RGB")
bg, _, _ = cover(cafe, (W, H))
bg = ImageEnhance.Brightness(bg).enhance(0.5)
ov = Image.new("RGB", (W, H), (45, 18, 14)); bg = Image.blend(bg, ov, 0.28)
canvas = bg.convert("RGBA")


def paste(layer, xy): canvas.alpha_composite(layer, (int(xy[0]), int(xy[1])))


def bust_crop(box=(40, 0, 1000, 1010)):
    return portrait.crop(box)


# --- header -----------------------------------------------------------------------------
def title_content(layer, box):
    draw_text(layer, ((box[0] + box[2]) // 2, (box[1] + box[3]) // 2 + 1), "ESCALAÇÃO", 22, BROWN, anchor="mm", spacing=2)


plate, _ = kit.panel("frame_label", (240, 56), None, title_content)
paste(plate, (14, 12))

CHEM = 62  # MOCK value


def chem_content(layer, box):
    x0, y0, x1, y1 = box
    draw_text(layer, (x0 + 10, y0 + 3), "Química", 15, BROWN)
    draw_text(layer, (x1 - 10, y0 + 3), f"{CHEM}%", 15, BROWN, anchor="ra")
    bx0, bx1, by = x0 + 10, x1 - 10, y0 + 24
    d = ImageDraw.Draw(layer)
    d.rectangle([bx0, by, bx1, by + 9], fill=(126, 78, 62, 255))
    d.rectangle([bx0, by, bx0 + int((bx1 - bx0) * CHEM / 100), by + 9], fill=(214, 150, 40, 255))
    draw_text(layer, ((x0 + x1) // 2, y1 - 5), "Time promissor", 11, (126, 78, 62, 255), anchor="mb")


chem, _ = kit.panel("frame_wide", (360, 74), 0.30, chem_content)
paste(chem, (268, 6))

pill = scaled(kit.piece("pill_normal"), 128 / 362)
back = pill.copy()
draw_text(back, (back.width // 2, back.height // 2), "Voltar", 15, CREAM_TXT, OUTLINE, anchor="mm")
paste(back, (W - 14 - back.width, 24))

# --- board: five positions (header over a row of slots), as in LineupScreen.BuildBoard ---------
SW = 80
GROUPS = [("Recepção", 2), ("Salão", 3), ("Cozinha", 3), ("Palco", 1), ("Gerência", 1)]  # level-max slot counts, GDD 2.2
GAP_IN, GAP_G = 6, 14
widths = [n * SW + (n - 1) * GAP_IN for _, n in GROUPS]
x = (W - (sum(widths) + GAP_G * (len(GROUPS) - 1))) // 2
BOARD_Y = 94
common_empty = scaled(kit.piece("rarity_common"), SW / kit.piece("rarity_common").width)
suzu_slot = None


def filled_slot(rarity, width, art, art_mode):
    base, fr, box = kit.card_frame(rarity, width)
    bw, bh = box[2] - box[0], box[3] - box[1]
    if art_mode == "chibi":
        c = to_height(art, bh - 2)
        base.alpha_composite(c, (box[0] + (bw - c.width) // 2, box[3] - c.height))
    else:
        c = to_height(art, bh)
        if c.width > bw: c = c.crop(((c.width - bw) // 2, 0, (c.width + bw) // 2, c.height))
        base.alpha_composite(c, (box[0] + (bw - c.width) // 2, box[1]))
    base.alpha_composite(fr)
    return base


for (role, n), gw in zip(GROUPS, widths):
    def hdr(layer, box, role=role):
        draw_text(layer, ((box[0] + box[2]) // 2, (box[1] + box[3]) // 2 + 1), role.upper(), 12, BROWN, anchor="mm", spacing=1)
    hp, _ = kit.panel("frame_label", (max(gw, 90), 26), None, hdr)
    paste(hp, (x + (gw - hp.width) // 2, BOARD_Y))
    for i in range(n):
        sx = x + i * (SW + GAP_IN)
        if role == "Recepção" and i == 0:
            s = filled_slot("rare", SW, chibi, "chibi"); paste(s, (sx, BOARD_Y + 32))
            draw_text(canvas, (sx + SW // 2, BOARD_Y + 32 + s.height + 3), "Suzu", 13, CREAM_TXT, OUTLINE, anchor="ma")
        else:
            paste(common_empty, (sx, BOARD_Y + 32))
    x += gw + GAP_G

# --- bench ------------------------------------------------------------------------------
def lbl(layer, box, txt="BANCO"):
    draw_text(layer, ((box[0] + box[2]) // 2, (box[1] + box[3]) // 2 + 1), txt, 12, BROWN, anchor="mm", spacing=1)


bl, _ = kit.panel("frame_label", (112, 26), None, lbl)
paste(bl, (18, 246))
bench, bbox = kit.panel("frame_wide", (700, 208), 0.34)
paste(bench, (14, 276))
BX, BY = 14 + bbox[0], 276 + bbox[1]
card_w = 116
names = [("common", "Comum"), ("rare", "Rara"), ("epic", "Épica"), ("legendary", "Lendária")]
gapb = ((bbox[2] - bbox[0]) - 5 * card_w) // 6
for i, (r, nm) in enumerate(names):
    cx = BX + gapb + i * (card_w + gapb)
    s = filled_slot(r, card_w, bust_crop(), "portrait"); paste(s, (cx, BY + 10))
    draw_text(canvas, (cx + card_w // 2, BY + 10 + s.height + 3), "Suzu · " + nm, 12, BROWN, anchor="ma")
cx = BX + gapb + 4 * (card_w + gapb)
ce = scaled(kit.piece("rarity_common"), card_w / kit.piece("rarity_common").width)
paste(ce, (cx, BY + 10))

# --- selected card preview (portrait, proposed addition) ------------------------------------
pv = filled_slot("rare", 210, bust_crop((30, 0, 1010, 1060)), "portrait")
paste(pv, (736, 276))
def pvl(layer, box):
    draw_text(layer, (box[0] + 12, (box[1] + box[3]) // 2 + 1), "Suzu", 17, BROWN, anchor="lm")
    draw_text(layer, (box[2] - 12, (box[1] + box[3]) // 2 + 1), "Rara", 14, (46, 104, 190, 255), anchor="rm")
pn, _ = kit.panel("frame_label", (210, 40), None, pvl)
paste(pn, (736, 276 + pv.height + 4))

# --- footer ------------------------------------------------------------------------------
draw_text(canvas, (20, 506), "Faltam maids no Salão e na Cozinha: o dia não abre.", 14, (255, 150, 135, 255), OUTLINE)
auto = scaled(kit.piece("pill_normal"), 176 / 362); draw_text(auto, (auto.width // 2, auto.height // 2), "Escalar sozinho", 14, CREAM_TXT, OUTLINE, anchor="mm")
clr = scaled(kit.piece("pill_pressed"), 112 / 362); draw_text(clr, (clr.width // 2, clr.height // 2), "Limpar", 14, CREAM_TXT, OUTLINE, anchor="mm")
paste(auto, (W - 14 - 112 - 8 - 176, 556)); paste(clr, (W - 14 - 112, 556))
draw_text(canvas, (20, 586), "MOCKUP ART-01D · arte gerada + componentes D4 · textos, números e fonte provisórios · não é captura Unity", 11, CREAM_TXT, OUTLINE, anchor="ls")

canvas.convert("RGB").save(OUT)
print("saved", OUT, canvas.size)
