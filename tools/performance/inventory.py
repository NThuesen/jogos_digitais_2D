#!/usr/bin/env python3
"""Inventory PNG dimensions and an existing WebGL payload. No Unity/GPU estimates implied."""
import argparse
import hashlib
import json
from pathlib import Path
import struct

ROOT = Path(__file__).resolve().parents[2]


def inventory(build):
    catalog = json.loads((ROOT / 'Assets/_Game/Resources/Maids/maid_walk_catalog.json').read_text())
    sheets = []
    for entry in catalog['maids']:
        path = ROOT / 'Assets/_Game/Resources' / (entry['resource'] + '.png')
        data = path.read_bytes()
        width, height = struct.unpack('>II', data[16:24])
        sheets.append(dict(id=entry['id'], width=width, height=height,
                           frames=entry['frames'], cells=entry['columns'] * entry['rows'],
                           png_bytes=len(data), sha256=hashlib.sha256(data).hexdigest()))
    files = [dict(path=str(p.relative_to(build)), bytes=p.stat().st_size,
                  sha256=hashlib.sha256(p.read_bytes()).hexdigest())
             for p in sorted(build.rglob('*')) if p.is_file()]
    return dict(sheets=sheets, pixels=sum(x['width'] * x['height'] for x in sheets),
                build_files=files,
                deployment_bytes=sum(x['bytes'] for x in files if 'DoNotShip' not in x['path']))


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--build', type=Path, default=ROOT / 'Build/WebGL')
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    if not (args.build / 'index.html').is_file():
        parser.error('build must contain index.html')
    args.output.write_text(json.dumps(inventory(args.build), indent=2) + '\n')
