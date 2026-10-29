"""Deterministic Mina v01 extraction. Usage: process.py SOURCE_DIR OUTPUT_DIR."""
import sys, json, hashlib
from pathlib import Path
import numpy as np
from PIL import Image
src, dst = map(Path, sys.argv[1:])
dst.mkdir(parents=True, exist_ok=True)
report = {}
for name, filename in dict(idle='v01_chibi_idle', walk='v01_chibi_walk', carry='v01_chibi_carry', side='lado', back='costas').items():
    path = src / (filename + '.png')
    im = Image.open(path).convert('RGB')
    rgb = np.array(im).astype(float)
    # Magenta has high blue/red, low green; brown/cream character colors do not.
    excess = np.minimum(rgb[:,:,0], rgb[:,:,2]) - rgb[:,:,1]
    alpha = 1 - np.clip((excess - 55) / 170, 0, 1)
    # Unmatte partially covered pixels against the original magenta key.
    key = np.array([255, 0, 255])
    clean = np.clip((rgb - (1-alpha[:,:,None])*key) / np.maximum(alpha[:,:,None], .001), 0, 255)
    rgba = np.dstack([clean, alpha*255]).astype('uint8')
    ys, xs = np.where(alpha > .5)
    box = (int(xs.min()), int(ys.min()), int(xs.max()+1), int(ys.max()+1))
    cut = Image.fromarray(rgba).crop(box)
    # Equal nominal ear-to-ground height; bottom aligned on one fixed canvas.
    h = 384
    cut = cut.resize((round(cut.width*h/cut.height), h), Image.Resampling.LANCZOS)
    canvas = Image.new('RGBA', (448, 400))
    canvas.alpha_composite(cut, ((448-cut.width)//2, 400-h))
    canvas.save(dst/(name+'.png'))
    report[name] = dict(source=path.name, sha256=hashlib.sha256(path.read_bytes()).hexdigest(), crop=box)
(dst/'processing.json').write_text(json.dumps(report, indent=2)+'\n')
