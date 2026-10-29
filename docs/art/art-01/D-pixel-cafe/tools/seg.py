import numpy as np
from PIL import Image
from scipy import ndimage as ndi


def load(path):
    return np.asarray(Image.open(path).convert("RGB")).astype(int)


def bg_color(rgb):
    h, w, _ = rgb.shape
    # use top corners/edges only: portrait/bottom edges touch the figure
    top = np.concatenate([rgb[:3, :].reshape(-1, 3), rgb[:, :3].reshape(-1, 3)])
    return np.median(top, axis=0)


def exterior_mask(rgb, bg, tol, seed_bottom=True):
    d = np.abs(rgb - bg).sum(axis=2)
    cand = d <= tol
    lab, n = ndi.label(cand)  # 4-connectivity
    h, w = cand.shape
    parts = [lab[0], lab[:, 0], lab[:, -1]] + ([lab[-1]] if seed_bottom else [])
    edge = np.concatenate(parts)
    keep = np.unique(edge[edge > 0])
    ext = np.isin(lab, keep)
    return ext, d


def enclosed_holes(rgb, bg, ext, tight=10, min_area=120):
    d = np.abs(rgb - bg).sum(axis=2)
    cand = (d <= tight) & ~ext
    lab, n = ndi.label(cand)
    holes = np.zeros_like(cand)
    info = []
    for i in range(1, n + 1):
        m = lab == i
        a = int(m.sum())
        if a >= min_area:
            holes |= m
            ys, xs = np.where(m)
            info.append((a, xs.min(), ys.min(), xs.max(), ys.max()))
    return holes, info


def cutout(rgb, bg, tol=20, defringe=1, tight_hole=10, seed_bottom=True):
    ext, d = exterior_mask(rgb, bg, tol, seed_bottom)
    holes, info = enclosed_holes(rgb, bg, ext, tight_hole)
    bgmask = ext | holes
    fg = ~bgmask
    # drop specks
    lab, n = ndi.label(fg, structure=np.ones((3, 3)))
    if n > 1:
        sizes = ndi.sum(fg, lab, range(1, n + 1))
        for i, s in enumerate(sizes, 1):
            if s < 40:
                fg[lab == i] = False
    fg = ndi.binary_fill_holes(fg) | fg  # keep interior intact
    if defringe:
        # remove boundary pixels that are still bg-tinted (blend of bg and outline)
        er = ndi.binary_erosion(fg, iterations=defringe, border_value=1)  # canvas edge is not a contour
        ring = fg & ~er
        tinted = ring & (np.abs(rgb - bg).sum(axis=2) <= tol * 2.5)
        fg = fg & ~tinted
    alpha = (fg * 255).astype(np.uint8)
    out = np.dstack([rgb.astype(np.uint8), alpha])
    return out, fg, info


def fringe_metric(rgb, bg, fg, ring=2):
    """Share of the outermost `ring` px of the cutout whose colour is still close to bg."""
    er = ndi.binary_erosion(fg, iterations=ring)
    edge = fg & ~er
    inner = ndi.binary_erosion(fg, iterations=ring + 4) & ~ndi.binary_erosion(fg, iterations=ring + 2)
    de = np.abs(rgb - bg).sum(axis=2)
    return (de[edge] <= 60).mean() * 100, (de[inner] <= 60).mean() * 100, int(edge.sum())
