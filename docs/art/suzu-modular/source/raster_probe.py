"""Slice generated raster groups and assemble review evidence; never draws art.

Run from any directory with Pillow. Source sheets/prompts are kept untouched.
OpenRaster layers preserve the cropped RGBA pixels and editable placement.
Comparison sheets are offline composites, NOT Unity camera captures.
"""
from io import BytesIO
from pathlib import Path
from xml.etree import ElementTree as ET
from zipfile import ZIP_STORED, ZIP_DEFLATED, ZipFile

from PIL import Image, ImageDraw, ImageFont

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[3]
SRC = HERE / 'raster'
EVIDENCE = HERE.parent / 'evidence'
CANVAS = (920, 1420)
BG = '#382e33'


def png_bytes(image):
    buffer = BytesIO()
    image.save(buffer, format='PNG')
    return buffer.getvalue()


def groups(iteration):
    sheet = Image.open(SRC / f'probe-0{iteration}-generated.png').convert('RGBA')
    if sheet.size != (1774, 887):
        raise ValueError('Source dimensions changed; inspect before slicing.')
    # Crop boundaries lie outside the visible subject; no resampling or repaint.
    head = sheet.crop((0, 0, 880, 860 if iteration == 1 else 775))
    body = sheet.crop((887, 173, 1744, 870))
    return [('Head / hair', head, (0, 0)),
            ('Torso / skirt', body, (28, 716))]


def assemble(layers):
    image = Image.new('RGBA', CANVAS)
    # Head sits in front of collar; neck overlap is static, not a rig test.
    for _, layer, offset in reversed(layers):
        image.alpha_composite(layer, offset)
    return image


def openraster(layers, merged):
    root = ET.Element('image', w=str(CANVAS[0]), h=str(CANVAS[1]),
                      name='Suzu raster probe 02', version='0.0.3')
    stack = ET.SubElement(root, 'stack')
    with ZipFile(SRC / 'suzu-probe-02.ora', 'w') as archive:
        archive.writestr('mimetype', 'image/openraster', compress_type=ZIP_STORED)
        for i, (name, layer, (x, y)) in enumerate(layers):
            path = f'data/layer{i}.png'
            ET.SubElement(stack, 'layer', name=name, src=path, x=str(x), y=str(y),
                          opacity='1.0', visibility='visible',
                          **{'composite-op': 'svg:src-over'})
            archive.writestr(path, png_bytes(layer), compress_type=ZIP_DEFLATED)
        archive.writestr('stack.xml', ET.tostring(root, encoding='utf-8'))
        archive.writestr('mergedimage.png', png_bytes(merged))
        thumb = merged.copy()
        thumb.thumbnail((256, 256))
        archive.writestr('Thumbnails/thumbnail.png', png_bytes(thumb))


def font(size):
    # DejaVu on the original authoring environment; fallback keeps regeneration usable.
    for path in ['/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf',
                 'DejaVuSans.ttf']:
        try:
            return ImageFont.truetype(path, size)
        except OSError:
            pass
    return ImageFont.load_default(size=size)


def paste_height(canvas, image, center, top, height):
    width = round(image.width * height / image.height)
    sample = image.resize((width, height), Image.Resampling.NEAREST)
    canvas.alpha_composite(sample, (center - width // 2, top))


def legacy_regions():
    # Actual version 05 Unity capture: head-to-hem crop of its enlarged sample.
    # Keep original arms/tail/context instead of modifying the baseline design.
    return Image.open(EVIDENCE / 'static-05-960x600.png').convert('RGBA').crop(
        (130, 193, 291, 420))


def comparison(images, labels, filename):
    canvas = Image.new('RGBA', (960, 600), BG)
    draw = ImageDraw.Draw(canvas)
    draw.text((20, 12), 'SUZU | montagem offline: NAO e captura Unity',
              fill='#fff0d8', font=font(20))
    draw.text((20, 40), 'Mesma altura cabeca-barra; Point. Referencias apenas para comparacao.',
              fill='#dfc1b0', font=font(15))
    centers = [160 + 320 * i for i in range(len(images))] if len(images) == 3 else [260, 700]
    for center, image, label in zip(centers, images, labels):
        draw.text((center - 125, 67), label, fill='#fff0d8', font=font(19))
        for top, height in [(102, 240), (380, 80), (500, 54)]:
            paste_height(canvas, image, center, top, height)
    for y, text in [(351, '240 px: ampliacao'), (468, '80 px: hipotese de jogo'),
                    (560, '54 px: estresse de leitura')]:
        draw.text((20, y), text, fill='#dfc1b0', font=font(15))
    canvas.convert('RGB').save(EVIDENCE / filename)


if __name__ == '__main__':
    EVIDENCE.mkdir(exist_ok=True)
    layers = groups(2)
    merged = assemble(layers)
    for filename, (_, layer, _) in zip(['head-hair.png', 'torso-skirt.png'], layers):
        layer.save(SRC / filename)
    merged.save(SRC / 'assembly.png')
    openraster(layers, merged)
    # Show only the first reference figure's head-to-hem region, including its
    # original arms/tail as context. Never reused as generated sprite layers.
    reference = Image.open(ROOT / 'docs/art/art-01/D-pixel-cafe/incoming/D2_chibi_poses.png')
    reference = reference.convert('RGBA').crop((40, 60, 593, 725))
    comparison([reference, legacy_regions(), merged],
               ['D2 | referencia (recorte)', '05 | captura Unity (recorte)', 'Raster 02 | proposta'],
               'raster-comparison-960x600.png')
    comparison([assemble(groups(1)), merged],
               ['01 | volume e rosto', '02 | remover laco duplicado'],
               'raster-iterations-960x600.png')
    seam = merged.crop((280, 600, 680, 850))
    seam_bg = Image.new('RGBA', seam.size, BG)
    seam_bg.alpha_composite(seam)
    seam_bg.convert('RGB').save(EVIDENCE / 'raster-neck-seam.png')
    print('Wrote 2 raster layers, editable OpenRaster, assembly and 3 offline evidence images.')
