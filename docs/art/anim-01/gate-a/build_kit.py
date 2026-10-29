"""Rebuild Gate A from owner-supplied originals; requires existing Pillow/NumPy.

Run from any directory: python build_kit.py [--check]. No AI generation or downloads.
Source coordinates are top-left pixels; Unity uses the same bind frame with Y inverted.
"""
import hashlib
import json
import sys
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[3]
OUT = ROOT / 'Assets/_Game/Resources/Anim01GateA'
REVIEW = ROOT / 'Logs/anim01/offline'
SIZE = (1254, 1254)


def polygon(points):
    im = Image.new('L', SIZE)
    ImageDraw.Draw(im).polygon(points, fill=255)
    return np.array(im) > 0


def key_original(path):
    rgb = np.array(Image.open(path).convert('RGB')).astype(float)
    excess = np.minimum(rgb[:, :, 0], rgb[:, :, 2]) - rgb[:, :, 1]
    alpha = 1 - np.clip((excess - 55) / 170, 0, 1)
    clean = np.clip((rgb - (1 - alpha[:, :, None]) * [255, 0, 255]) /
                    np.maximum(alpha[:, :, None], .001), 0, 255)
    clean[alpha < 1 / 255] = 0
    return Image.fromarray(np.dstack([clean, alpha * 255]).astype('uint8'))


# These polygons are source-specific segmentation, not a reusable character generator.
# Last owner wins at overlaps. Reconstruction remains behind retained source pixels.
REGIONS = [
    ('head', [(0, 0), (1253, 0), (1253, 466), (655, 466), (605, 455), (0, 455)]),
    ('ponytail', [(0, 85), (612, 85), (619, 182), (616, 252), (625, 329),
                  (636, 387), (654, 425), (644, 467), (598, 510), (549, 559), (0, 559)]),
    ('tail_base', [(190, 409), (366, 410), (407, 460), (410, 560), (397, 621),
                   (433, 657), (534, 677), (539, 729), (437, 729), (351, 694),
                   (320, 626), (318, 531), (188, 567)]),
    ('tail_tip', [(176, 404), (364, 404), (408, 457), (403, 547),
                  (371, 585), (323, 573), (320, 532), (178, 569)]),
    ('far_thigh', [(704, 765), (805, 787), (814, 902), (781, 950),
                   (708, 937), (678, 852)]),
    ('far_shin', [(712, 891), (810, 891), (797, 943), (708, 1068),
                  (648, 1046), (679, 953)]),
    ('near_thigh', [(608, 770), (704, 786), (720, 870), (693, 926),
                    (610, 942), (597, 893)]),
    ('near_shin', [(608, 893), (700, 898), (676, 962), (647, 1080),
                   (585, 1072), (591, 959)]),
    ('far_shoe', [(652, 1025), (711, 1032), (716, 1071), (774, 1094),
                  (779, 1175), (659, 1190), (626, 1103)]),
    ('near_shoe', [(526, 1011), (615, 1011), (653, 1048), (650, 1085),
                   (700, 1130), (710, 1220), (524, 1220)]),
    ('skirt', [(440, 637), (636, 598), (805, 620), (850, 716),
               (889, 801), (889, 864), (792, 877), (696, 842),
               (640, 874), (539, 854), (421, 795), (422, 714)]),
    ('back_bow', [(454, 568), (561, 568), (622, 596), (612, 642),
                  (544, 681), (451, 666)]),
    ('bodice', [(631, 436), (756, 428), (854, 567), (865, 616),
                (810, 658), (703, 650), (649, 613), (608, 536)]),
    ('apron', [(692, 628), (788, 650), (824, 694), (853, 768),
               (849, 794), (812, 796), (777, 775), (745, 745),
               (711, 715), (690, 677)]),
    ('far_upper_arm', [(809, 609), (845, 616), (880, 644), (885, 689),
                        (850, 692), (816, 673), (797, 642)]),
    ('far_forearm', [(858, 637), (911, 649), (924, 670), (914, 703),
                      (880, 704), (852, 687)]),
    ('far_hand', [(922, 669), (953, 670), (979, 678), (1004, 691),
                  (1017, 719), (990, 753), (947, 752), (912, 715)]),
    ('far_cuff', [(878, 629), (923, 630), (947, 663), (934, 697),
                  (930, 733), (905, 736), (882, 713), (870, 678)]),
    ('near_upper_arm', [(626, 558), (679, 573), (682, 601), (638, 642),
                         (610, 661), (573, 643), (592, 602)]),
    ('near_forearm', [(593, 616), (637, 630), (613, 670), (602, 707),
                       (564, 713), (555, 675), (574, 642)]),
    ('near_hand', [(546, 698), (593, 712), (598, 742), (578, 775),
                   (550, 801), (527, 798), (526, 785), (510, 804),
                   (490, 806), (475, 792), (455, 787), (452, 764), (487, 738)]),
    ('near_cuff', [(554, 664), (591, 666), (620, 692), (621, 716),
                   (603, 727), (596, 750), (574, 741), (553, 721), (544, 695)]),
    ('near_sleeve', [(623, 454), (680, 453), (721, 474), (744, 519),
                     (739, 551), (702, 578), (691, 613), (669, 611),
                     (657, 587), (620, 582), (597, 561), (600, 541), (615, 535)]),
    ('far_sleeve', [(817, 592), (849, 592), (860, 612), (846, 633), (815, 625)]),
    ('near_garter', [(617, 839), (654, 843), (682, 852), (705, 874),
                     (695, 902), (653, 897), (615, 881), (609, 859)]),
    ('far_garter', [(692, 849), (747, 823), (785, 838), (805, 863),
                    (786, 893), (764, 889), (741, 915), (702, 916), (691, 891)]),
    ('near_foot_skin', []), ('far_foot_skin', []),
]

# The source skirt overlaps both upper legs. Explicitly return exposed skin to anatomy;
# this prevents a future outfit swap carrying pieces of Mina's thighs with the dress.
SKIN_OVERRIDES = [
    ('far_thigh', [(656, 790), (692, 796), (743, 812), (788, 840),
                    (774, 861), (711, 877), (685, 852), (662, 830)]),
    ('near_thigh', [(617, 820), (650, 824), (677, 850), (693, 872),
                     (677, 890), (622, 882), (603, 859)]),
]

# Two localized fabric reconstructions, sampled from unoccluded original fabric.
# Their masks only fill pixels originally owned by the occluding near arm/hand.
FABRIC_PATCHES = [
    {'part': 'skirt', 'sample': [641, 672, 682, 764],
     'target': [529, 615, 687, 807],
     'polygon': [(601, 616), (679, 621), (686, 725), (620, 801),
                 (534, 781), (546, 710), (573, 668)]},
    {'part': 'skirt', 'sample': [806, 814, 866, 857],
     'target': [462, 762, 612, 845],
     'polygon': [(487, 763), (573, 788), (610, 808), (601, 839),
                 (548, 837), (495, 818), (463, 794)]},
]

# Bind positions, source pixels. Root is a stable ground reference, not sprite bounds.
NODES = [
    ('root', '', (650, 1215)),
    ('pelvis', 'root', (714, 792)),
    ('torso', 'pelvis', (729, 607)),
    ('head', 'torso', (749, 450)),
    ('ponytail', 'head', (611, 230)),
    ('headwear_socket', 'head', (738, 143)),
    ('tail_base', 'pelvis', (495, 691)),
    ('tail_tip', 'tail_base', (365, 560)),
    ('near_upper_arm', 'torso', (672, 520)),
    ('near_forearm', 'near_upper_arm', (611, 630)),
    ('near_hand', 'near_forearm', (573, 715)),
    ('far_upper_arm', 'torso', (815, 603)),
    ('far_forearm', 'far_upper_arm', (859, 659)),
    ('far_hand', 'far_forearm', (924, 698)),
    ('near_thigh', 'pelvis', (668, 804)),
    ('near_shin', 'near_thigh', (652, 917)),
    ('near_foot', 'near_shin', (612, 1058)),
    ('far_thigh', 'pelvis', (744, 817)),
    ('far_shin', 'far_thigh', (757, 915)),
    ('far_foot', 'far_shin', (685, 1048)),
    ('accessory_socket', 'torso', (780, 583)),
]

# Rendering order is anatomical, not polygon ownership order.
PARTS = [
    ('tail_base', 'tail_base', 0), ('tail_tip', 'tail_tip', 1),
    ('far_thigh', 'far_thigh', 2), ('far_shin', 'far_shin', 3),
    ('far_foot_skin', 'far_foot', 4), ('far_shoe', 'far_foot', 5),
    ('far_sleeve', 'far_upper_arm', 6),
    ('far_upper_arm', 'far_upper_arm', 7), ('far_forearm', 'far_forearm', 8),
    ('far_hand', 'far_hand', 8), ('far_cuff', 'far_forearm', 9),
    ('near_thigh', 'near_thigh', 10), ('near_shin', 'near_shin', 11),
    ('near_foot_skin', 'near_foot', 12), ('near_shoe', 'near_foot', 13),
    ('torso_skin', 'torso', 14), ('back_bow', 'pelvis', 15),
    ('skirt', 'pelvis', 16), ('bodice', 'torso', 17), ('apron', 'pelvis', 18),
    ('near_upper_arm', 'near_upper_arm', 19), ('near_forearm', 'near_forearm', 20),
    ('near_hand', 'near_hand', 21), ('near_cuff', 'near_forearm', 22),
    ('near_sleeve', 'near_upper_arm', 23), ('head', 'head', 24),
    ('ponytail', 'ponytail', 25),
    ('near_garter', 'near_thigh', 14), ('far_garter', 'far_thigh', 6),
]

# Hidden caps: same local palette, smooth shaded, behind original pixels. The recipe
# records every invented region. Caps are not per-pose corrections or full-body art.
CAPS = {
    'near_upper_arm': [(672, 550, 36, 53), (611, 630, 31, 29)],
    'near_forearm': [(611, 630, 31, 30), (578, 694, 25, 27)],
    'near_hand': [(573, 715, 23, 23)],
    'far_upper_arm': [(826, 631, 22, 27), (859, 659, 25, 23)],
    'far_forearm': [(859, 659, 26, 24), (907, 682, 24, 23)],
    'far_hand': [(924, 698, 21, 24)],
    'near_thigh': [(665, 820, 42, 70), (652, 917, 34, 32)],
    'near_shin': [(652, 917, 33, 32), (612, 1058, 27, 30)],
    'far_thigh': [(744, 835, 40, 68), (757, 915, 34, 34)],
    'far_shin': [(757, 915, 33, 34), (685, 1048, 26, 28)],
    'near_foot_skin': [(618, 1093, 32, 43)],
    'far_foot_skin': [(687, 1088, 29, 40)],
    'torso_skin': [(745, 544, 60, 89)],
    'tail_base': [(365, 557, 34, 40)],
    'tail_tip': [(365, 557, 33, 39)],
    'ponytail': [(611, 234, 30, 46)],
    'head': [(746, 450, 22, 21)],
}

POSES = [
    {'id': 'neutral', 'label': 'Neutra', 'angles': {}},
    {'id': 'near_leg_forward', 'label': 'Perna próxima à frente',
     'angles': {'near_thigh': 24, 'near_shin': -12, 'near_foot': -6,
                'far_thigh': -12, 'far_shin': 14}},
    {'id': 'far_leg_forward', 'label': 'Perna distante à frente',
     'angles': {'far_thigh': 24, 'far_shin': -12, 'far_foot': -6,
                'near_thigh': -12, 'near_shin': 14}},
    {'id': 'opposite_arms', 'label': 'Braços opostos',
     'angles': {'near_upper_arm': -23, 'near_forearm': 12,
                'far_upper_arm': 23, 'far_forearm': -12}},
    {'id': 'joint_flexion', 'label': 'Joelhos e cotovelos',
     'angles': {'near_thigh': 10, 'near_shin': -35, 'far_thigh': -8,
                'far_shin': -28, 'near_forearm': -38, 'far_forearm': 35}},
    {'id': 'head_rotation', 'label': 'Cabeça +7 graus', 'angles': {'head': 7}},
    {'id': 'occlusion', 'label': 'Oclusão de roupa e cabelo',
     'angles': {'near_thigh': 20, 'far_thigh': -15, 'near_upper_arm': 18,
                'near_forearm': -24, 'head': -5, 'ponytail': 7,
                'tail_base': -6, 'tail_tip': 9}},
]


def cap_layer(name, original_alpha):
    im = Image.new('RGBA', SIZE)
    hair = name in ('tail_base', 'tail_tip', 'ponytail')
    color = (222, 139, 58) if hair else (255, 236, 179)
    if 'arm' in name or 'hand' in name or name in ('head', 'torso_skin'):
        color = (251, 215, 146)
    yy, xx = np.mgrid[:SIZE[1], :SIZE[0]]
    for x, y, rx, ry in CAPS.get(name, []):
        radius = ((xx - x) / rx) ** 2 + ((yy - y) / ry) ** 2
        alpha = np.clip((1 - radius) * min(rx, ry) / 1.5, 0, 1) * original_alpha
        shade = .94 + .06 * np.clip(1 - (xx - x + rx) / (2 * rx), 0, 1)
        rgb = np.stack([np.clip(c * shade, 0, 255) for c in color], axis=-1)
        im.alpha_composite(Image.fromarray(np.dstack([rgb, alpha * 255]).astype('uint8')))
    return im


def build():
    manifest = json.loads((HERE / 'sources.json').read_text())
    for entry in manifest['sources']:
        data = (HERE / 'sources' / entry['file']).read_bytes()
        if hashlib.sha256(data).hexdigest() != entry['sha256']:
            raise ValueError('Source hash mismatch: ' + entry['file'])
    original = key_original(HERE / 'sources' / manifest['preferred'])
    rgba = np.array(original)
    owner = np.full(SIZE[::-1], -1, dtype=np.int16)
    ids = {name: index for index, (name, _) in enumerate(REGIONS)}
    for index, (_, poly) in enumerate(REGIONS):
        if poly:
            owner[polygon(poly)] = index
    for name, poly in SKIN_OVERRIDES:
        owner[polygon(poly)] = ids[name]
    # Garters are clothing, not skin. Reapply after the upper-leg skin overrides.
    for name in ('near_garter', 'far_garter'):
        owner[polygon(dict(REGIONS)[name])] = ids[name]
    # Pale skin between the leather straps stays on the foot, independent of the shoe.
    pale_skin = (rgba[:, :, 0] > 210) & (rgba[:, :, 1] > 170) & (rgba[:, :, 2] > 100)
    for side in ('near', 'far'):
        owner[(owner == ids[side + '_shoe']) & pale_skin] = ids[side + '_foot_skin']
    visible = rgba[:, :, 3] > 128
    missing = visible & (owner < 0)
    boundary_pixels = int(missing.sum())
    # Preserve antialiased edge pixels just outside hand-authored polygons. Extend the
    # nearest region label, never its alpha or colour, at most 32 source pixels.
    for _ in range(32):
        if not np.any(visible & (owner < 0)):
            break
        expanded = owner.copy()
        for dy, dx in [(-1, 0), (1, 0), (0, -1), (0, 1)]:
            neighbor = np.roll(owner, (dy, dx), axis=(0, 1))
            take = (expanded < 0) & (neighbor >= 0)
            expanded[take] = neighbor[take]
        owner = expanded
    missing = visible & (owner < 0)
    if missing.sum():
        yy, xx = np.where(missing)
        raise ValueError(f'{missing.sum()} unassigned foreground pixels, bounds '
                         f'{xx.min()},{yy.min()}..{xx.max()},{yy.max()}')
    images = {}
    original_alpha = rgba[:, :, 3].astype(float) / 255
    for index, (name, _) in enumerate(REGIONS):
        pixels = rgba.copy()
        pixels[:, :, 3] = np.where(owner == index, pixels[:, :, 3], 0)
        layer = cap_layer(name, original_alpha)
        layer.alpha_composite(Image.fromarray(pixels))
        images[name] = layer
    for name in CAPS:
        if name not in images:
            images[name] = cap_layer(name, original_alpha)
    occluders = np.isin(owner, [ids[n] for n in ('near_upper_arm', 'near_forearm',
                                               'near_hand', 'near_cuff')])
    for patch in FABRIC_PATCHES:
        x0, y0, x1, y1 = patch['target']
        canvas = Image.new('RGBA', SIZE)
        sample = original.crop(patch['sample']).resize((x1 - x0, y1 - y0), Image.Resampling.BICUBIC)
        canvas.alpha_composite(sample, (x0, y0))
        pixels = np.array(canvas)
        allowed = polygon(patch['polygon']) & occluders
        pixels[:, :, 3] = np.where(allowed, pixels[:, :, 3], 0)
        canvas = Image.fromarray(pixels)
        canvas.alpha_composite(images[patch['part']])
        images[patch['part']] = canvas
    OUT.mkdir(parents=True, exist_ok=True)
    REVIEW.mkdir(parents=True, exist_ok=True)
    original.save(OUT / 'reference.png')
    nodes = [{'id': n, 'parent': p, 'x': xy[0], 'y': xy[1]} for n, p, xy in NODES]
    parts = []
    neutral = Image.new('RGBA', SIZE)
    for name, node, order in sorted(PARTS, key=lambda p: p[2]):
        layer = images[name]
        bounds = layer.getbbox()
        if not bounds:
            raise ValueError('Empty part: ' + name)
        layer.crop(bounds).save(OUT / (name + '.png'))
        neutral.alpha_composite(layer)
        parts.append({'id': name, 'node': node, 'order': order, 'x': bounds[0],
                      'y': bounds[1], 'width': bounds[2] - bounds[0],
                      'height': bounds[3] - bounds[1],
                      'reconstructedCaps': len(CAPS.get(name, [])),
                      'sha256': hashlib.sha256((OUT / (name + '.png')).read_bytes()).hexdigest()})
    data = {'version': 1, 'source': manifest['preferred'], 'width': SIZE[0],
            'height': SIZE[1], 'pixelsPerUnit': 1175, 'floorX': 650, 'floorY': 1215,
            'nodes': nodes, 'parts': parts,
            'poses': [{'id': p['id'], 'label': p['label'],
                       'angles': [{'node': n, 'degrees': v} for n, v in p['angles'].items()]}
                      for p in POSES]}
    (OUT / 'rig.json').write_text(json.dumps(data, indent=2) + '\n', encoding='utf-8')
    recipe = {'coordinates': '1254x1254 top-left original pixels; no resizing',
              'regions': [{'part': n, 'polygon': p} for n, p in REGIONS],
              'caps': CAPS, 'skinOverrides': SKIN_OVERRIDES,
              'fabricPatches': FABRIC_PATCHES,
              'description': 'Source-palette hidden caps and two local fabric patches; retained source pixels overlaid unchanged.'}
    (HERE / 'recipe.json').write_text(json.dumps(recipe, indent=2) + '\n')
    neutral.save(REVIEW / 'neutral_offline.png')
    bg = Image.new('RGBA', (SIZE[0] * 2, SIZE[1]), '#b8a08c')
    bg.alpha_composite(original, (0, 0)); bg.alpha_composite(neutral, (SIZE[0], 0))
    bg.resize((1254, 627), Image.Resampling.LANCZOS).save(REVIEW / 'comparison_offline.png')
    def matte(image):
        bg = Image.new('RGBA', SIZE, '#b8a08c'); bg.alpha_composite(image)
        return np.array(bg.convert('RGB')).astype(int)
    diff = np.abs(matte(neutral) - matte(original))
    Image.fromarray(matte(original).astype('uint8')).save(REVIEW / 'reference_matte.png')
    Image.fromarray(matte(neutral).astype('uint8')).save(REVIEW / 'neutral_matte.png')
    metrics = {'foreground_pixels': int(visible.sum()),
               'boundary_pixels_assigned_to_nearest_region': boundary_pixels,
               'unassigned_foreground': int(missing.sum()), 'parts': len(parts),
               'different_matted_pixels_over_10': int((diff.max(axis=2) > 10).sum()),
               'note': 'Difference includes hidden overlap reconstruction; not aesthetic acceptance.'}
    (HERE / 'asset_checks.json').write_text(json.dumps(metrics, indent=2) + '\n')
    print(json.dumps(metrics))


def check():
    manifest = json.loads((HERE / 'sources.json').read_text())
    data = json.loads((OUT / 'rig.json').read_text())
    for item in manifest['sources']:
        assert hashlib.sha256((HERE / 'sources' / item['file']).read_bytes()).hexdigest() == item['sha256']
    known = set()
    for node in data['nodes']:
        assert not node['parent'] or node['parent'] in known, node['id']
        assert node['id'] not in known
        known.add(node['id'])
    for part in data['parts']:
        assert part['node'] in known
        file = OUT / (part['id'] + '.png')
        assert hashlib.sha256(file.read_bytes()).hexdigest() == part['sha256']
        assert Image.open(file).size == (part['width'], part['height'])
    print(f'PASS: {len(manifest["sources"])} sources, {len(known)} nodes, {len(data["parts"])} parts; hashes, parent order and dimensions.')


if __name__ == '__main__':
    check() if '--check' in sys.argv else build()
