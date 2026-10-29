"""Board: minimum modularity test of the D4 UI kit. Usage: compose_modularity.py <D-pixel-cafe dir> [out.png]"""
import sys, os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from PIL import Image, ImageDraw
from artlib import *

ROOT = sys.argv[1]
OUT = sys.argv[2] if len(sys.argv) > 2 else f"{ROOT}/review/08_modularidade_ui.png"
kit = Kit(f"{ROOT}/processed/ui")
BOARD = (43, 38, 35, 255)
W = 1500
board = Image.new("RGBA", (W, 3200), BOARD)
Z = 2


def zoom(im, z=Z):
    flat = Image.new("RGBA", im.size, (88, 80, 74, 255)); flat.alpha_composite(im)
    return flat.resize((im.width * z, im.height * z), Image.NEAREST)


def put(im, xy, cap=None):
    z = zoom(im)
    board.alpha_composite(z, xy)
    if cap: draw_text(board, (xy[0], xy[1] + z.height + 4), cap, 12, (210, 200, 185, 255))
    return z.width, z.height


def title(y, t, sub=None):
    draw_text(board, (14, y), t, 18, (255, 220, 160, 255))
    if sub: draw_text(board, (14, y + 24), sub, 12, (200, 190, 175, 255))


def ctr(txt, sz=14, color=BROWN):
    def f(layer, box): draw_text(layer, ((box[0] + box[2]) // 2, (box[1] + box[3]) // 2 + 1), txt, sz, color, anchor="mm")
    return f


draw_text(board, (14, 8), "ART-01D · teste mínimo de modularidade do kit D4 (ampliado 2×, fundo cinza = transparência)", 18, CREAM_TXT)

cur = {"x": 14, "y": 52, "rowh": 0}


def flow(im, cap=None, gap=16):
    z = zoom(im)
    if cur["x"] + z.width > W - 14:
        newrow()
    board.alpha_composite(z, (cur["x"], cur["y"]))
    extra = 0
    if cap:
        draw_text(board, (cur["x"], cur["y"] + z.height + 3), cap, 12, (210, 200, 185, 255)); extra = 18
    cur["x"] += z.width + gap
    cur["rowh"] = max(cur["rowh"], z.height + extra)


def newrow(pad=14):
    cur["y"] += cur["rowh"] + pad; cur["x"] = 14; cur["rowh"] = 0


def section(t, sub):
    if cur["rowh"]: newrow(10)
    title(cur["y"], t, sub); cur["y"] += 54; cur["x"] = 14; cur["rowh"] = 0


section("1) Molduras fatiáveis (9-slice): mesma peça em tamanhos diferentes; cantos escalam, bordas esticam", "última da linha 2: o mesmo tamanho (200×60) com redimensionamento ingênuo, sem fatiar, para comparar")
for (w, h) in [(300, 74), (200, 60), (120, 120)]:
    p, _ = kit.panel("frame_wide", (w, h), 0.30, ctr(f"{w}×{h}")); flow(p)
newrow()
for (w, h) in [(110, 130), (220, 90), (160, 160)]:
    p, _ = kit.panel("frame_large", (w, h), 0.30, ctr(f"{w}×{h}")); flow(p)
flow(rs(kit.piece("frame_wide"), (200, 60), Image.BOX), "ingênuo: borda e patinhas deformam")

section("2) Faixa de título (3-slice horizontal): largura acompanha o texto, altura fixa", "texto e número são independentes da arte; a moldura não traz nada baked in")
for t in ["PALCO", "RECEPÇÃO", "GERÊNCIA", "Escalar sozinho", "Química do time: 62%"]:
    f = font(13); tw = int(ImageDraw.Draw(Image.new("RGBA", (1, 1))).textlength(t, font=f)) + 70
    p, _ = kit.panel("frame_label", (tw, 30), None, ctr(t, 13)); flow(p)

section("3) Molduras de raridade (escala uniforme; NÃO esticar): 64 / 80 / 96 px de largura", "bege = comum · azul = rara · violeta = épica · dourada = lendária (cores do GDD §2.8)")
for wd in (64, 80, 96):
    for r in ("common", "rare", "epic", "legendary"):
        base, fr, box = kit.card_frame(r, wd); base.alpha_composite(fr); flow(base, gap=10)
    newrow()
flow(rs(kit.piece("rarity_epic_window"), (150, 96), Image.BOX), "épica esticada 150×96: gemas e cantos deformam")
flow(kit.panel("frame_label", (150, 96), None)[0], "frame_label forçado a 150×96 (3-slice não serve para caixa)")

section("4) Ícones do kit em 32 / 48 / 64 px de altura", "legibilidade em tamanho pequeno (ampliado 2× para inspeção)")
for hh in (32, 48, 64):
    for n in ("book", "paw", "cup", "clock", "gear", "star"):
        flow(to_height(kit.piece(f"icon_{n}"), hh), gap=6)
    newrow()

section("5) Botões (pílula) e barra com toggles", "texto independente; normal/pressionado são peças separadas; toggles vêm baked na barra")
for st, txt in (("normal", "Voltar"), ("pressed", "Limpar")):
    pl = scaled(kit.piece(f"pill_{st}"), 0.5); draw_text(pl, (pl.width // 2, pl.height // 2), txt, 15, CREAM_TXT, OUTLINE, anchor="mm"); flow(pl)
newrow()
flow(scaled(kit.piece("bar_toggles"), 0.35), "barra com 2 toggles baked in (não ligam/desligam sozinhos)")
newrow(0)
board.crop((0, 0, W, cur["y"] + 10)).convert("RGB").save(OUT)
print("saved", OUT, (W, cur["y"] + 10))
