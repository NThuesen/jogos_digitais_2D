// Shared helpers for the ART-01 exploratory mockups (colour maths, SVG kit, path mirroring).
// Everything here is deterministic: same input -> same bytes.

export const f = (n) => Math.round(n * 100) / 100;

let uid = 0;
export const nid = (p) => `${p}${++uid}`;
export const resetIds = () => { uid = 0; };

// ---------------------------------------------------------------- colour
export const hex2rgb = (h) => {
  h = h.replace('#', '');
  return [0, 2, 4].map((i) => parseInt(h.slice(i, i + 2), 16));
};
export const rgb2hex = (c) =>
  '#' + c.map((v) => Math.max(0, Math.min(255, Math.round(v))).toString(16).padStart(2, '0')).join('');
export const mix = (a, b, t) => {
  const A = hex2rgb(a), B = hex2rgb(b);
  return rgb2hex(A.map((v, i) => v + (B[i] - v) * t));
};
export function rgb2hsl([r, g, b]) {
  r /= 255; g /= 255; b /= 255;
  const mx = Math.max(r, g, b), mn = Math.min(r, g, b);
  let h = 0, s = 0; const l = (mx + mn) / 2;
  if (mx !== mn) {
    const d = mx - mn;
    s = l > 0.5 ? d / (2 - mx - mn) : d / (mx + mn);
    if (mx === r) h = (g - b) / d + (g < b ? 6 : 0);
    else if (mx === g) h = (b - r) / d + 2;
    else h = (r - g) / d + 4;
    h /= 6;
  }
  return [h, s, l];
}
export function hsl2rgb([h, s, l]) {
  const q = l < 0.5 ? l * (1 + s) : l + s - l * s, p = 2 * l - q;
  const t = (x) => {
    if (x < 0) x += 1; if (x > 1) x -= 1;
    if (x < 1 / 6) return p + (q - p) * 6 * x;
    if (x < 1 / 2) return q;
    if (x < 2 / 3) return p + (q - p) * (2 / 3 - x) * 6;
    return p;
  };
  if (s === 0) return [l * 255, l * 255, l * 255];
  return [t(h + 1 / 3) * 255, t(h) * 255, t(h - 1 / 3) * 255];
}
/** Multiplies saturation and shifts lightness; keeps hue so the identity survives a style change. */
export function adj(hex, { s = 1, l = 0 } = {}) {
  const [h, sat, lig] = rgb2hsl(hex2rgb(hex));
  return rgb2hex(hsl2rgb([h, Math.max(0, Math.min(1, sat * s)), Math.max(0, Math.min(1, lig + l))]));
}
const lin = (v) => { v /= 255; return v <= 0.03928 ? v / 12.92 : ((v + 0.055) / 1.055) ** 2.4; };
export const luminance = (h) => { const [r, g, b] = hex2rgb(h); return 0.2126 * lin(r) + 0.7152 * lin(g) + 0.0722 * lin(b); };
/** WCAG 2.x contrast ratio. */
export const contrast = (a, b) => {
  const x = luminance(a), y = luminance(b);
  return (Math.max(x, y) + 0.05) / (Math.min(x, y) + 0.05);
};

// ---------------------------------------------------------------- path maths
/** Mirrors an absolute SVG path (M L C Q A Z only) around the vertical axis x = w/2. */
export function flipX(d, w = 400) {
  const tok = d.match(/[MLCQAZ]|-?\d*\.?\d+(?:e-?\d+)?/g);
  let out = [], i = 0;
  while (i < tok.length) {
    const c = tok[i++];
    if (c === 'Z') { out.push('Z'); continue; }
    out.push(c);
    if (c === 'A') {
      while (i < tok.length && !/[A-Z]/.test(tok[i])) {
        const [rx, ry, rot, la, sw, x, y] = tok.slice(i, i + 7).map(Number);
        out.push(rx, ry, -rot, la, sw ? 0 : 1, f(w - x), y);
        i += 7;
      }
    } else {
      while (i < tok.length && !/[A-Z]/.test(tok[i])) {
        out.push(f(w - Number(tok[i])), tok[i + 1]);
        i += 2;
      }
    }
  }
  return out.join(' ');
}
export const ell = (cx, cy, rx, ry) =>
  `M ${f(cx - rx)} ${f(cy)} A ${f(rx)} ${f(ry)} 0 1 0 ${f(cx + rx)} ${f(cy)} A ${f(rx)} ${f(ry)} 0 1 0 ${f(cx - rx)} ${f(cy)} Z`;
export const rrect = (x, y, w, h, r) => {
  r = Math.min(r, w / 2, h / 2);
  return `M ${f(x + r)} ${f(y)} L ${f(x + w - r)} ${f(y)} Q ${f(x + w)} ${f(y)} ${f(x + w)} ${f(y + r)} L ${f(x + w)} ${f(y + h - r)} Q ${f(x + w)} ${f(y + h)} ${f(x + w - r)} ${f(y + h)} L ${f(x + r)} ${f(y + h)} Q ${f(x)} ${f(y + h)} ${f(x)} ${f(y + h - r)} L ${f(x)} ${f(y + r)} Q ${f(x)} ${f(y)} ${f(x + r)} ${f(y)} Z`;
};
/** Smooth open/closed curve through points (Catmull-Rom -> cubic Bezier). */
export function smooth(pts, closed = false, k = 1 / 6) {
  const n = pts.length;
  const P = (i) => pts[closed ? (i + n) % n : Math.max(0, Math.min(n - 1, i))];
  let d = `M ${f(pts[0][0])} ${f(pts[0][1])}`;
  const last = closed ? n : n - 1;
  for (let i = 0; i < last; i++) {
    const p0 = P(i - 1), p1 = P(i), p2 = P(i + 1), p3 = P(i + 2);
    d += ` C ${f(p1[0] + (p2[0] - p0[0]) * k)} ${f(p1[1] + (p2[1] - p0[1]) * k)} ${f(p2[0] - (p3[0] - p1[0]) * k)} ${f(p2[1] - (p3[1] - p1[1]) * k)} ${f(p2[0])} ${f(p2[1])}`;
  }
  return d + (closed ? ' Z' : '');
}
export const star = (cx, cy, R, r, n = 5, rot = -90) => {
  const p = [];
  for (let i = 0; i < n * 2; i++) {
    const a = ((rot + (i * 180) / n) * Math.PI) / 180, rr = i % 2 ? r : R;
    p.push(`${f(cx + Math.cos(a) * rr)} ${f(cy + Math.sin(a) * rr)}`);
  }
  return 'M ' + p.join(' L ') + ' Z';
};
export const heart = (cx, cy, s) =>
  `M ${f(cx)} ${f(cy + s * 0.9)} C ${f(cx - s * 1.6)} ${f(cy - s * 0.2)} ${f(cx - s * 0.7)} ${f(cy - s * 1.2)} ${f(cx)} ${f(cy - s * 0.4)} C ${f(cx + s * 0.7)} ${f(cy - s * 1.2)} ${f(cx + s * 1.6)} ${f(cy - s * 0.2)} ${f(cx)} ${f(cy + s * 0.9)} Z`;

export const esc = (s) => String(s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');

// ---------------------------------------------------------------- drawing kit
/**
 * Binds the shape primitive to one visual direction. `unit` scales line weights and shade
 * offsets so the same drawing code serves a 400-unit portrait and a 120-unit chibi.
 */
export function kit(st, defs, unit = 1) {
  const lw = st.lineW * unit;
  const shadeId = (d, fill, shade, o) => {
    const mode = o.shade ?? st.shade;
    if (mode === 'none' || !shade) return '';
    const m = nid('m');
    const off = (o.off ?? st.shadeOff) * unit;
    const dx = -off * 0.7, dy = -off;
    let blur = '', blk = `<path d="${d}" fill="#000" transform="translate(${f(dx)} ${f(dy)})"/>`;
    if (mode === 'soft') {
      const fl = nid('fl');
      defs.push(`<filter id="${fl}" x="-30%" y="-30%" width="160%" height="160%"><feGaussianBlur stdDeviation="${f(off * 0.55)}"/></filter>`);
      blk = `<path d="${d}" fill="#000" transform="translate(${f(dx)} ${f(dy)})" filter="url(#${fl})"/>`;
    }
    defs.push(`<mask id="${m}" maskUnits="userSpaceOnUse" x="-200" y="-200" width="1400" height="1400"><path d="${d}" fill="#fff"/>${blk}</mask>`);
    let sf = shade;
    if (mode === 'halftone') {
      const pt = nid('pt');
      const cell = 5.2 * unit;
      defs.push(`<pattern id="${pt}" patternUnits="userSpaceOnUse" width="${f(cell)}" height="${f(cell)}" patternTransform="rotate(28)"><rect width="${f(cell)}" height="${f(cell)}" fill="${fill}"/><circle cx="${f(cell / 2)}" cy="${f(cell / 2)}" r="${f(cell * 0.3)}" fill="${shade}"/></pattern>`);
      sf = `url(#${pt})`;
    }
    return `<path d="${d}" fill="${sf}" mask="url(#${m})" opacity="${mode === 'soft' ? 0.85 : 1}"/>`;
  };
  /** Filled + shaded + outlined shape. o: shade colour, noStroke, lw multiplier, stroke colour. */
  const P = (d, fill, o = {}) => {
    const shade = o.shadeColor ?? (o.noShade ? null : mix(fill, st.shadeTint, st.shadeAmt));
    const body = `<path d="${d}" fill="${fill}"/>` + shadeId(d, fill, shade, o);
    const mis = st.misreg ? `<g transform="translate(${f(st.misreg * unit)} ${f(st.misreg * unit)})">${body}</g>` : body;
    const w = (o.lw ?? 1) * lw;
    const stroke = o.noStroke ? '' : `<path d="${d}" fill="none" stroke="${o.stroke ?? st.line}" stroke-width="${f(w)}" stroke-linejoin="round" stroke-linecap="round"/>`;
    return mis + stroke;
  };
  /** A thick rounded stroke with an outline (tails, limbs). */
  const T = (d, w, fill, o = {}) =>
    `<path d="${d}" fill="none" stroke="${st.line}" stroke-width="${f(w + 2 * lw * (o.lw ?? 1))}" stroke-linecap="round" stroke-linejoin="round"/>` +
    `<path d="${d}" fill="none" stroke="${fill}" stroke-width="${f(w)}" stroke-linecap="round" stroke-linejoin="round"/>`;
  const line = (d, w, color, o = {}) =>
    `<path d="${d}" fill="none" stroke="${color}" stroke-width="${f(w)}" stroke-linecap="round" stroke-linejoin="round" ${o.op ? `opacity="${o.op}"` : ''}/>`;
  return { P, T, line, lw };
}

export const svgDoc = (w, h, defs, body, bg = null) =>
  `<svg xmlns="http://www.w3.org/2000/svg" width="${w}" height="${h}" viewBox="0 0 ${w} ${h}">` +
  (defs.length ? `<defs>${defs.join('')}</defs>` : '') +
  (bg ? `<rect width="${w}" height="${h}" fill="${bg}"/>` : '') +
  body + '</svg>';
