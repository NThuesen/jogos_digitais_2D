"""One authorized A1/A2 repair in original source pixels. No per-pose art.

Unedited regions come from the pinned 228f534 sprite, not a regenerated character.
Curved cloth/hem geometry is painted at 4x and sampled back once for antialiasing.
The cloth donor is translated 1:1; the leather donor uses a documented perspective map.
"""
import hashlib
import json
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFilter

SIZE = (1254, 1254)
SCALE = 4
OLD_PATCH_MASKS = [
    [(601,616),(679,621),(686,725),(620,801),(534,781),(546,710),(573,668)],
    [(487,763),(573,788),(610,808),(601,839),(548,837),(495,818),(463,794)],
]
CLOTH_AUTH = [(469,641),(618,613),(685,613),(694,704),(641,815),
              (632,877),(589,877),(466,808)]
SKIN_SLIVERS = [
    [(608,839),(625,830),(635,856),(623,877),(605,868)],
    [(650,802),(662,798),(703,842),(694,849)],
]
NEAR_RESIDUES = [
    [(648,1053),(707,1097),(707,1128),(692,1124),(647,1076)],
    [(702,1105),(708,1105),(708,1183),(702,1183)],
]
FAR_AUTH = [(605,1024),(783,1024),(783,1183),(605,1183)]
FAR_SEAM = [(645,1058),(648,1075),(682,1107),(703,1117),
            (702,1176),(694,1176),(693,1124),(675,1111),(643,1076)]
DONOR_CLOTH = [(588,680),(651,635),(689,640),(681,735),
               (618,778),(574,762),(556,749),(563,732),(584,716)]

RECIPE = {
    'revision': 'A1-A2-exception-1', 'baseline': '228f534',
    'canvas': list(SIZE), 'supersampling': SCALE,
    'authorized': {'skirt': [CLOTH_AUTH] + OLD_PATCH_MASKS + SKIN_SLIVERS,
                   'near_shoe': NEAR_RESIDUES, 'far_shoe': [FAR_AUTH]},
    'removed_skin_slivers': SKIN_SLIVERS, 'removed_near_shoe_residues': NEAR_RESIDUES,
    'cloth': {'method': 'curved panel + directional folds + curved gold hem + individually painted ruffles',
              'native_donor': 'lado.png', 'donor_mask': DONOR_CLOTH,
              'donor_translation': [6,-6], 'donor_scale': [1,1],
              'gold_cubic': [[491,738],[532,758],[577,780],[618,784]],
              'ruffle_top_cubic': [[486,755],[524,783],[575,789],[619,802]]},
    'far_shoe': {'method': 'near-shoe leather underlay, painted rear counter and seam removal',
                 'donor': 'cleaned near_shoe', 'scale': [.86,.80],
                 'near_ankle': [612,1058], 'far_ankle': [685,1048],
                 'exclude_donor_above_y': 1090, 'seam_mask': FAR_SEAM},
}


def mask(polygons):
    result = Image.new('L', SIZE)
    draw = ImageDraw.Draw(result)
    for poly in polygons:
        draw.polygon(poly, fill=255)
    return result


def cubic(points, samples=100):
    p = np.asarray(points, dtype=float)
    t = np.linspace(0, 1, samples)[:, None]
    return (1-t)**3*p[0] + 3*(1-t)**2*t*p[1] + 3*(1-t)*t*t*p[2] + t**3*p[3]


def aa_polygon(points, fill, outline=None, width=2):
    # The small construction window avoids allocating several 5000x5000 canvases.
    origin = (400, 590)
    surface = Image.new('RGBA', (340*SCALE, 300*SCALE))
    draw = ImageDraw.Draw(surface)
    coords = [((x-origin[0])*SCALE, (y-origin[1])*SCALE) for x,y in points]
    draw.polygon(coords, fill=fill)
    if outline:
        draw.line(coords+[coords[0]], fill=outline, width=round(width*SCALE), joint='curve')
    image = Image.new('RGBA', SIZE)
    image.alpha_composite(surface.resize((340,300),Image.Resampling.LANCZOS),origin)
    return image


def aa_stroke(points, color, width):
    surface = Image.new('RGBA', (340*SCALE,300*SCALE))
    draw = ImageDraw.Draw(surface)
    draw.line([((x-400)*SCALE,(y-590)*SCALE) for x,y in points],
              fill=color,width=max(1,round(width*SCALE)),joint='curve')
    image = Image.new('RGBA',SIZE)
    image.alpha_composite(surface.resize((340,300),Image.Resampling.LANCZOS),(400,590))
    return image


def cloth_paint(donor):
    panel = np.concatenate([
        cubic([(680,615),(638,632),(531,669),(487,710)]),
        cubic([(487,710),(470,732),(475,747),(487,758)]),
        cubic(RECIPE['cloth']['ruffle_top_cubic']),
        cubic([(619,802),(646,766),(670,698),(687,646)]),
    ])
    silhouette = aa_polygon(panel,(255,255,255,255))
    yy,xx=np.mgrid[:1254,:1254]
    # Two broad folds converge on the waist; gentle source-scale variation preserves
    # shaded volume instead of a flat fill. All colors come from adjacent source cloth.
    base = np.clip((yy-635)/180,0,1)
    fold1 = np.exp(-((xx-(671-.62*(yy-635)))/10)**2)
    fold2 = np.exp(-((xx-(650-1.03*(yy-640)))/16)**2)
    grain = .55*np.sin(xx*.73+yy*.31)*np.sin(yy*.48)
    rgb = np.stack([111+28*base-27*fold1-14*fold2+grain,
                    45+15*base-12*fold1-6*fold2+grain,
                    18+8*base-5*fold1-3*fold2+grain],axis=-1)
    rgba=np.dstack([np.clip(rgb,0,255),np.array(silhouette)[:,:,3]]).astype('uint8')
    painted=Image.fromarray(rgba)
    # Registered native-resolution cloth, excluding the other pose's arm and trim.
    translated=Image.new('RGBA',SIZE);translated.alpha_composite(donor,(6,-6))
    donor_mask=Image.new('L',SIZE);donor_mask.paste(mask([DONOR_CLOTH]),(6,-6))
    donor_mask=donor_mask.filter(ImageFilter.MinFilter(5)).filter(ImageFilter.GaussianBlur(3))
    arr=np.array(translated);arr[:,:,3]=np.minimum(arr[:,:,3],np.array(donor_mask))
    painted.alpha_composite(Image.fromarray(arr))
    # Thin fold edges rather than a rectangular cloned texture boundary.
    for points in [([(675,631),(639,662),(609,713),(580,757)],(65,25,10,135),1.8),
                   ([(655,642),(618,669),(563,724),(516,762)],(74,29,12,100),1.4)]:
        painted.alpha_composite(aa_stroke(cubic(points[0]),points[1],points[2]))
    top=cubic(RECIPE['cloth']['ruffle_top_cubic'],7)
    # Six alternating folds. Native retained ruffles are composited over this underlay.
    for i in range(6):
        a,b=top[i],top[i+1]; dx,dy=b-a
        lower=38+(3 if i%2 else 0)
        pts=np.concatenate([
            cubic([a,a+[dx*.35,dy*.3],b-[dx*.2,dy*.2],b],20),
            cubic([b,b+[-1,lower*.65],b+[-4,lower+2],b+[-12,lower]],20),
            cubic([b+[-12,lower],a+[6,lower+4],a+[-4,lower*.7],a+[0,5]],20),
        ])
        cream=(255,241,188,255) if i%2==0 else (251,231,176,255)
        painted.alpha_composite(aa_polygon(pts,cream,(85,40,20,255),1.8))
        crease=cubic([b+[-2,5],b+[-5,17],b+[-3,28],b+[-12,lower-2]],30)
        painted.alpha_composite(aa_stroke(crease,(222,168,111,220),4.5))
        seam=cubic([a+[0,lower-11],a+[dx*.4,lower-6],b+[-14,lower-9],b+[-11,lower-8]],25)
        painted.alpha_composite(aa_stroke(seam,(211,110,57,235),1.3))
    hem=cubic(RECIPE['cloth']['ruffle_top_cubic'])
    painted.alpha_composite(aa_stroke(hem,(66,27,12,255),2.8))
    gold=cubic(RECIPE['cloth']['gold_cubic'])
    painted.alpha_composite(aa_stroke(gold,(111,56,16,255),4))
    painted.alpha_composite(aa_stroke(gold,(233,171,48,255),2))
    painted.alpha_composite(aa_stroke(gold+np.array([0,-1]),(247,190,70,160),.8))
    return painted


def leather_underlay(near):
    arr=np.array(near)
    arr[:1090,:,3]=0  # bow, garter and ankle decoration are never cloned
    clean=Image.fromarray(arr)
    sx,sy=.86,.80; tx=685-sx*612;ty=1048-sy*1058
    warped=clean.transform(SIZE,Image.Transform.AFFINE,
                           (1/sx,0,-tx/sx,0,1/sy,-ty/sy),Image.Resampling.BICUBIC)
    # Rear counter closes the missing volume above the original visible vamp.
    outline=np.concatenate([cubic([(648,1064),(630,1066),(621,1082),(619,1106)],40),
                            cubic([(619,1106),(618,1125),(614,1143),(622,1155)],30),
                            np.array([(646,1165),(660,1147),(714,1164),(724,1140),
                                      (694,1116),(668,1085),(653,1070)])])
    surface=Image.new('RGBA',(240*SCALE,200*SCALE));draw=ImageDraw.Draw(surface)
    pts=[((x-590)*SCALE,(y-1000)*SCALE) for x,y in outline]
    draw.polygon(pts,fill=(119,51,22,255))
    draw.line(pts+[pts[0]],fill=(58,24,13,255),width=2*SCALE,joint='curve')
    base=Image.new('RGBA',SIZE)
    base.alpha_composite(surface.resize((240,200),Image.Resampling.LANCZOS),(590,1000))
    base.alpha_composite(warped)
    # A short highlight curves along the rear counter; it does not create a second bow.
    detail=Image.new('RGBA',(240*SCALE,200*SCALE));draw=ImageDraw.Draw(detail)
    pts=cubic([(634,1078),(629,1087),(628,1097),(632,1106)],30)
    draw.line([((x-590)*SCALE,(y-1000)*SCALE) for x,y in pts],fill=(166,82,33,200),width=2*SCALE)
    base.alpha_composite(detail.resize((240,200),Image.Resampling.LANCZOS),(590,1000))
    return base


def repair(images, donor, here, review):
    folder=here/'exception';baseline=json.loads((folder/'baseline.json').read_text())
    parts={p['id']:p for p in baseline['definition']['parts']}
    old={}
    for name in ('skirt','near_shoe','far_shoe'):
        file=folder/'baseline'/(name+'.png')
        assert hashlib.sha256(file.read_bytes()).hexdigest()==baseline['png_hashes'][file.name]
        canvas=Image.new('RGBA',SIZE);p=parts[name]
        canvas.paste(Image.open(file).convert('RGBA'),(p['x'],p['y']))
        old[name]=canvas
    masks={n:mask(polys) for n,polys in RECIPE['authorized'].items()}
    # A1: complete garment independent of the arm's silhouette.
    skirt=cloth_paint(donor)
    retained=images['skirt'].copy()
    array=np.array(retained);array[np.array(mask(SKIN_SLIVERS))>0]=0
    retained=Image.fromarray(array)
    skirt.alpha_composite(retained)
    # A2 near: explicit removal only; retained leather pixels are not repainted.
    near=np.array(old['near_shoe']);near[np.array(masks['near_shoe'])>0]=0
    near=Image.fromarray(near)
    far=leather_underlay(near)
    front=np.array(old['far_shoe']);front[np.array(mask([FAR_SEAM]))>0]=0
    far.alpha_composite(Image.fromarray(front))
    candidates={'skirt':skirt,'near_shoe':near,'far_shoe':far}
    integrity={}
    for name,candidate in candidates.items():
        before=np.array(old[name]);after=before.copy();allowed=np.array(masks[name])>0
        after[allowed]=np.array(candidate)[allowed]
        changed=np.any(before!=after,axis=2)
        assert not np.any(changed & ~allowed)
        images[name]=Image.fromarray(after)
        integrity[name]={'changed_pixels':int(changed.sum()),
                         'authorized_pixels':int(allowed.sum()),'changed_outside_mask':0}
    masks_dir=folder/'masks';masks_dir.mkdir(exist_ok=True)
    for name,image in masks.items():image.save(masks_dir/('authorized_'+name+'.png'))
    (folder/'reconstruction.json').write_text(json.dumps(RECIPE,indent=2)+'\n')
    (folder/'pixel_integrity.json').write_text(json.dumps(integrity,indent=2)+'\n')
    return RECIPE
