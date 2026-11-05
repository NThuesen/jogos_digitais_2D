"""Comparison sheets from the QA captures (NEKO_CAPTURE_DIR of MaidPortraitsQaTests). Usage: python portraits_sheets.py <captures dir>"""
import sys, shutil
from PIL import Image, ImageDraw
S = sys.argv[1].rstrip('/') + '/'
D = 'docs/art/ui-modular/portraits/'
Q = 'Assets/_Game/Tests/PlayMode/QaArt/'
ids = ['suzu', 'hina', 'akari', 'momo', 'reina', 'kaede', 'nene', 'ichika', 'yoru']
def img(n, r): return Image.open(f'{S}{n}_{r[0]}x{r[1]}.png').convert('RGB')
def region(im, res, kind):
    w, h = res; s = h / 720; cx = w / 2
    b = {'lineup': (cx + (110 - 640) * s, 120 * s, cx + (1170 - 640) * s, 262 * s),
         'roster': (22 * s, 150 * s, 622 * s, 520 * s),
         'detail': (w - (1280 - 770) * s, 150 * s, w - (1280 - 1255) * s, 372 * s)}[kind]
    return im.crop(tuple(int(v) for v in b))
o = Image.new('RGB', (9 * 250, 500), (60, 60, 60))
for i, m in enumerate(ids):
    d = Image.open(Q + m + '_detail_480.png').convert('RGBA').resize((246, 328), Image.LANCZOS)
    bg = Image.new('RGBA', d.size, (150, 150, 150, 255)); bg.alpha_composite(d); o.paste(bg.convert('RGB'), (i * 250 + 2, 0))
    h = Image.open(Q + m + '_head_192.png').convert('RGBA').resize((200, 164), Image.LANCZOS)
    bg = Image.new('RGBA', h.size, (140, 105, 75, 255)); bg.alpha_composite(h); o.paste(bg.convert('RGB'), (i * 250 + 25, 334))
    ImageDraw.Draw(o).text((i * 250 + 8, 8), m.capitalize(), fill=(0, 0, 0))
o.save(D + 'nove_retratos.png')
for res in [(960, 600), (1280, 720)]:
    tag = f'{res[0]}x{res[1]}'
    rows = []
    for g in (0, 1):
        rows += [region(img(f'g{g}_lineup_B_v03', res), res, 'lineup'), region(img(f'g{g}_roster_B_v03', res), res, 'roster')]
    c = Image.new('RGB', (max(r.width for r in rows), sum(r.height + 10 for r in rows)), (30, 30, 30)); y = 0
    for r in rows: c.paste(r, (0, y)); y += r.height + 10
    c.save(D + f'cards_{tag}.png')
    dets = [region(img('detail_' + m, res), res, 'detail') for m in ids]
    dw, dh = dets[0].size
    c = Image.new('RGB', (3 * dw + 20, 3 * dh + 20), (30, 30, 30))
    for i, d in enumerate(dets): c.paste(d, ((i % 3) * (dw + 10), (i // 3) * (dh + 10)))
    c.save(D + f'detalhe_{tag}.png')
