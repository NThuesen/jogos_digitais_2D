"""Shared helpers for the ART-01D mockups (evaluation only; not game code)."""
import numpy as np
from PIL import Image, ImageDraw, ImageFont, ImageFilter

CREAM = (252, 236, 205, 255)
BROWN = (94, 58, 54, 255)
CREAM_TXT = (255, 244, 225, 255)
OUTLINE = (54, 17, 36, 255)
FONT_PATH = "C:/Windows/Fonts/trebucbd.ttf"  # provisional mockup font, NOT the game's Typeface


def font(sz):
    return ImageFont.truetype(FONT_PATH, sz)


def rs(im, size, flt=Image.BOX, hard=True):
    """Premultiplied resize (no background halo); optional hard alpha like pixel art."""
    size = (max(1, int(round(size[0]))), max(1, int(round(size[1]))))
    r = im.convert("RGBa").resize(size, flt).convert("RGBA")
    if hard:
        a = np.asarray(r).copy(); a[..., 3] = np.where(a[..., 3] >= 128, 255, 0); r = Image.fromarray(a, "RGBA")
    return r


def scaled(im, s, flt=Image.BOX):
    return rs(im, (im.width * s, im.height * s), flt)


def to_height(im, h, flt=Image.BOX):
    return rs(im, (im.width * h / im.height, h), flt)


def nine_slice(im, m, size, s, flt=Image.BOX):
    """m = (l,t,r,b) source margins; corners scaled by s, edges stretched to fit `size`."""
    w, h = size
    l, t, r, b = m
    dl, dt, dr, db = [max(0, int(round(v * s))) for v in (l, t, r, b)]
    dl, dr = min(dl, w // 2), min(dr, w // 2); dt, db = min(dt, h // 2), min(db, h // 2)
    sw, sh = im.size
    out = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    xs = [(0, l, 0, dl), (l, sw - r, dl, w - dr), (sw - r, sw, w - dr, w)]
    ys = [(0, t, 0, dt), (t, sh - b, dt, h - db), (sh - b, sh, h - db, h)]
    for (sx0, sx1, dx0, dx1) in xs:
        for (sy0, sy1, dy0, dy1) in ys:
            if dx1 - dx0 <= 0 or dy1 - dy0 <= 0 or sx1 <= sx0 or sy1 <= sy0: continue
            piece = im.crop((sx0, sy0, sx1, sy1))
            out.alpha_composite(rs(piece, (dx1 - dx0, dy1 - dy0), flt, hard=False), (dx0, dy0))
    a = np.asarray(out).copy(); a[..., 3] = np.where(a[..., 3] >= 128, 255, 0)
    return Image.fromarray(a, "RGBA")


def three_slice_h(im, ml, mr, size, flt=Image.BOX):
    """Fixed height (uniformly scaled), stretch only the horizontal middle."""
    w, h = size
    s = h / im.height
    dl, dr = int(round(ml * s)), int(round(mr * s))
    return nine_slice(im, (ml, 0, mr, 0), size, s, flt)


def text_size(d, txt, f):
    b = d.textbbox((0, 0), txt, font=f)
    return b[2] - b[0], b[3] - b[1], b[0], b[1]


def draw_text(layer, xy, txt, sz, fill=BROWN, outline=None, anchor="la", spacing=0):
    d = ImageDraw.Draw(layer)
    f = font(sz)
    if spacing:
        x, y = xy
        if anchor[0] == "m":
            total = sum(d.textlength(c, font=f) + spacing for c in txt) - spacing
            x -= total / 2
        for c in txt:
            if outline:
                for dx in (-1, 0, 1):
                    for dy in (-1, 0, 1):
                        d.text((x + dx, y + dy), c, font=f, fill=outline, anchor="l" + anchor[1])
            d.text((x, y), c, font=f, fill=fill, anchor="l" + anchor[1]); x += d.textlength(c, font=f) + spacing
        return
    if outline:
        for dx in (-1, 0, 1):
            for dy in (-1, 0, 1):
                d.text((xy[0] + dx, xy[1] + dy), txt, font=f, fill=outline, anchor=anchor)
    d.text(xy, txt, font=f, fill=fill, anchor=anchor)


class Kit:
    def __init__(self, ui_dir):
        import json
        self.dir = ui_dir
        self.man = json.load(open(f"{ui_dir}/ui_manifest.json"))
        self._c = {}

    def piece(self, name):
        if name not in self._c:
            self._c[name] = Image.open(f"{self.dir}/{name}.png").convert("RGBA")
        return self._c[name]

    def panel(self, kind, size, s, content=None, margins=None):
        """cream base + (optional content callback(layer, interior_box)) + frame window overlay.
        Returns (RGBA, interior_box) in output pixels."""
        w, h = size
        win = self.piece(kind + "_window")
        man = self.man[kind]
        l, t, r, b = man["margins_ltrb"]
        mm = margins or ((76, 76, 76, 76) if kind in ("frame_large", "frame_wide") else (110, 0, 110, 0))
        if kind == "frame_label":
            frame = three_slice_h(win, mm[0], mm[2], size)
            sc = h / win.height
        else:
            frame = nine_slice(win, mm, size, s); sc = s
        il, it, ir, ib = [int(round(v * sc)) for v in (l, t, r, b)]
        box = (il, it, w - ir, h - ib)
        base = Image.new("RGBA", size, (0, 0, 0, 0))
        ImageDraw.Draw(base).rectangle([box[0], box[1], box[2] - 1, box[3] - 1], fill=CREAM)
        if content: content(base, box)
        base.alpha_composite(frame)
        return base, box

    def card_frame(self, rarity, width):
        win = self.piece(f"rarity_{rarity}_window")
        man = self.man[f"rarity_{rarity}"]
        s = width / win.width
        fr = scaled(win, s)
        l, t, r, b = [int(round(v * s)) for v in man["margins_ltrb"]]
        box = (l, t, fr.width - r, fr.height - b)
        base = Image.new("RGBA", fr.size, (0, 0, 0, 0))
        ImageDraw.Draw(base).rectangle([box[0], box[1], box[2] - 1, box[3] - 1], fill=CREAM)
        return base, fr, box


def cover(im, size):
    w, h = size
    s = max(w / im.width, h / im.height)
    r = im.resize((round(im.width * s), round(im.height * s)), Image.LANCZOS)
    x, y = (r.width - w) // 2, (r.height - h) // 2
    return r.crop((x, y, x + w, y + h)), s, (x, y)
