"""Build the V0/V1 comparison media from MinaCapture runs.

Usage: make_media.py CAPTURE_ROOT OUTPUT_DIR
CAPTURE_ROOT holds one folder per run, named <variant>_<width>x<height> (V0_1280x720, ...),
each with the frames and CSV files written by Neko.Game.Core.MinaCapture.

Every captured frame is one fixed 1/25 s step of the game clock, so each GIF frame lasts
exactly 40 ms (4 cs, which GIF can store) and the animation plays at the game's real speed.
Frames are consecutive; nothing is skipped or interpolated. Crops are taken at native pixels.
"""
import csv
import sys
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

FRAME_MS = 40
SEGMENTS = ("natural_walk", "natural_carry", "natural_vertical")


def font(size):
    for name in ("arialbd.ttf", "DejaVuSans-Bold.ttf"):
        try:
            return ImageFont.truetype(name, size)
        except OSError:
            continue
    return ImageFont.load_default()


def rows(run, segment):
    path = run / (segment + ".csv")
    return list(csv.DictReader(path.open())) if path.exists() else []


def to_screen(x, y, width, height):
    # The camera frames the 7x5 room centred on world (3, 2); a tile is 106 px at 720 px.
    ppu = 106.0 * height / 720.0
    return width / 2 + (x - 3.0) * ppu, height / 2 - (y - 2.0) * ppu


def window(data, segment):
    """From half a second before she moves to the end of the segment.

    The carry segment can hold several errands; only its last full delivery is kept, from the
    frame she takes the Deliver task. Either way the frames stay consecutive.
    """
    first = 0
    if segment == "natural_carry":
        starts = [i for i in range(1, len(data))
                  if data[i]["task"] == "Deliver" and data[i - 1]["task"] != "Deliver"]
        first = starts[-1] if starts else 0
    moving = [i for i, r in enumerate(data) if i >= first and r["moving"] == "True"]
    start = max(0, (moving[0] if moving else first) - 12)
    return start, len(data)


def crop_box(data, start, end, width, height):
    points = [to_screen(float(r["view_x"]), float(r["view_y"]), width, height) for r in data[start:end]]
    pad_x, pad_y = 120 * height / 720, 110 * height / 720
    left = max(0, min(p[0] for p in points) - pad_x)
    right = min(width, max(p[0] for p in points) + pad_x)
    top = max(0, min(p[1] for p in points) - pad_y - 40 * height / 720)
    bottom = min(height, max(p[1] for p in points) + pad_y)
    box = [int(left), int(top), int(right), int(bottom)]
    box[2] -= (box[2] - box[0]) % 2
    box[3] -= (box[3] - box[1]) % 2
    return tuple(box)


def compare(root, size, segment, out, zoom=1):
    width, height = map(int, size.split("x"))
    runs = {v: root / f"{v}_{size}" for v in ("V0", "V1")}
    data = {v: rows(r, segment) for v, r in runs.items()}
    if not data["V0"] or not data["V1"]:
        return None
    # Same seed and fixed frame: the simulation must agree frame for frame between variants.
    for a, b in zip(data["V0"], data["V1"]):
        if (a["worker_x"], a["worker_y"], a["task"]) != (b["worker_x"], b["worker_y"], b["task"]):
            raise SystemExit(f"{segment} {size}: V0 and V1 diverge at frame {a['frame']}")
    start, end = window(data["V0"], segment)
    end = min(end, len(data["V1"]))
    box = crop_box(data["V0"], start, end, width, height)
    cw, ch = (box[2] - box[0]) * zoom, (box[3] - box[1]) * zoom
    label_h, gap = 26, 6
    title = font(16)
    frames = []
    for i in range(start, end):
        sheet = Image.new("RGB", (cw * 2 + gap, ch + label_h), (30, 24, 22))
        draw = ImageDraw.Draw(sheet)
        for k, variant in enumerate(("V0", "V1")):
            frame = Image.open(runs[variant] / segment / f"frame_{i:04d}.png").convert("RGB").crop(box)
            if zoom != 1:
                frame = frame.resize((cw, ch), Image.NEAREST)
            x = k * (cw + gap)
            sheet.paste(frame, (x, label_h))
            draw.text((x + 6, 4), f"{variant}  {size}  {segment}  t={float(data[variant][i]['time']):.2f}s",
                      font=title, fill=(255, 236, 200))
        frames.append(sheet)
    out.parent.mkdir(parents=True, exist_ok=True)
    frames[0].save(out, save_all=True, append_images=frames[1:], duration=FRAME_MS, loop=0, optimize=True)
    return out, end - start, box


def probes(run, out):
    """All controlled probe stills of one run on one labelled sheet."""
    if not (run / "probes.csv").exists():
        return None
    log = {r["probe"]: r for r in csv.DictReader((run / "probes.csv").open())}
    files = [run / "probes" / (name + ".png") for name in log]
    width, height = Image.open(files[0]).size
    ppu = 106.0 * height / 720.0
    tile = int(ppu * 2.2)
    cols = 5
    rows_n = (len(files) + cols - 1) // cols
    sheet = Image.new("RGB", (cols * tile, rows_n * (tile + 22)), (30, 24, 22))
    draw = ImageDraw.Draw(sheet)
    for n, path in enumerate(files):
        image = Image.open(path).convert("RGB")
        entry = log.get(path.stem, {})
        cx, cy = to_screen(float(entry.get("view_x", 3)), float(entry.get("view_y", 2)), width, height)
        cx, cy = int(cx), int(cy)
        half = tile // 2
        crop = image.crop((cx - half, cy - half, cx + half, cy + half))
        x, y = (n % cols) * tile, (n // cols) * (tile + 22)
        sheet.paste(crop, (x, y + 22))
        draw.text((x + 4, y + 3), f"{path.stem}: {entry.get('sprite', '?')} flip={entry.get('flip', '?')}",
                  font=font(12), fill=(255, 236, 200))
    sheet.save(out)
    return out


def main():
    root, out = Path(sys.argv[1]), Path(sys.argv[2])
    for size in ("1280x720", "960x600"):
        for segment in SEGMENTS:
            made = compare(root, size, segment, out / f"compare_{segment}_{size}.gif")
            if made:
                print(made[0].name, "frames", made[1], "crop", made[2])
        for variant in ("V0", "V1"):
            run = root / f"{variant}_{size}"
            if run.exists():
                made = probes(run, out / f"probes_{variant}_{size}.png")
                if made:
                    print(made.name)
    for segment in ("natural_walk", "natural_carry"):
        made = compare(root, "1280x720", segment, out / f"compare_{segment}_1280x720_zoom2x.gif", zoom=2)
        if made:
            print(made[0].name, "frames", made[1])


if __name__ == "__main__":
    main()
