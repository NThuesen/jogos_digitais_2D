// Portrait (400x500 bust) and chibi (120x150) drawn from one maid spec, in any of the styles.
import { f, nid, mix, adj, ell, flipX, smooth, star, kit } from './lib.mjs';

const PIVOT = 400;

function palette(m, st) {
  const hair = adj(m.hair, st.hairAdj);
  return {
    hair,
    hairLit: mix(hair, '#FFFFFF', 0.32),
    hairDark: mix(hair, st.shadeTint, 0.45),
    eye: m.eye, skin: st.skin,
    ear: hair,
  };
}

// ------------------------------------------------------------------ portrait
export function portrait(m, st, defs) {
  const { P, T, line } = kit(st, defs, 1);
  const c = palette(m, st);
  const L = (d, fill, o) => P(d, fill, o);
  const both = (d, fill, o) => P(d, fill, o) + P(flipX(d, PIVOT), fill, o);
  const out = [];

  // ---- back hair + tail (rear layers)
  const tailD = 'M 292 470 C 352 470 372 420 356 372 C 346 340 360 312 384 306';
  if (m.hairStyle === 'twin') {
    const pig = 'M 120 112 C 60 70 -2 130 8 214 C 14 282 30 340 56 394 C 84 352 100 318 98 272 C 96 230 118 192 130 150 Z';
    out.push(L(pig, c.hair, {}), L(flipX(pig, PIVOT), c.hair, {}));
    out.push(line('M 38 160 C 26 220 40 300 58 360', st.lineW * 1.2, c.hairLit, { op: 0.55 }));
    out.push(line(flipX('M 38 160 C 26 220 40 300 58 360', PIVOT), st.lineW * 1.2, c.hairLit, { op: 0.55 }));
  } else if (m.hairStyle === 'long' || m.hairStyle === 'tiara') {
    const back = 'M 200 98 C 312 98 350 190 344 270 C 340 360 352 420 330 480 L 70 480 C 48 420 60 360 56 270 C 50 190 88 98 200 98 Z';
    out.push(L(back, c.hair, {}));
    if (m.hairStyle === 'tiara') {
      // drill curls
      const dr = 'M 64 300 C 28 310 22 372 54 392 C 84 408 112 384 100 356 C 92 338 70 346 74 364';
      out.push(T(dr, 34, c.hair), T(flipX(dr, PIVOT), 34, c.hair));
    }
  } else { // bob
    const back = 'M 200 100 C 304 100 342 178 336 258 C 334 312 318 336 292 346 L 108 346 C 82 336 66 312 64 258 C 58 178 96 100 200 100 Z';
    out.push(L(back, c.hair, {}));
  }
  out.push(T(tailD, 26, c.hair), `<circle cx="384" cy="306" r="${f(13 + st.lineW * 0.2)}" fill="${mix(st.skin, '#FFFFFF', 0.5)}" stroke="${st.line}" stroke-width="${f(st.lineW)}"/>`);

  // ---- neck + body + uniform
  out.push(L('M 178 318 L 178 392 Q 200 408 222 392 L 222 318 Z', mix(st.skin, st.shadeTint, 0.16), { noShade: true }));
  out.push(L('M 108 500 C 110 430 140 392 176 384 L 200 412 L 224 384 C 260 392 290 430 292 500 Z', st.uniform));
  // sleeves
  const sl = 'M 100 470 C 96 424 112 396 144 388 C 164 394 170 424 166 454 Z';
  out.push(L(sl, st.collar === '#FFFFFF' ? mix(st.uniform, '#FFFFFF', 0.45) : mix(st.uniform, '#FFFFFF', 0.18)), L(flipX(sl, PIVOT), mix(st.uniform, '#FFFFFF', 0.3)));
  // apron bib + frill
  out.push(L('M 154 500 L 164 432 Q 200 418 236 432 L 246 500 Z', st.apron));
  out.push(L('M 164 432 Q 200 418 236 432 Q 232 444 218 444 Q 200 436 182 444 Q 168 444 164 432 Z', mix(st.apron, '#FFFFFF', 0.5), { noShade: true }));
  // collar
  out.push(L('M 178 384 Q 148 390 140 412 Q 172 424 200 412 Z', st.collar), L(flipX('M 178 384 Q 148 390 140 412 Q 172 424 200 412 Z', PIVOT), st.collar));
  // bow
  out.push(L('M 200 418 C 176 396 160 404 164 424 C 168 442 188 432 200 424 Z', st.bow), L(flipX('M 200 418 C 176 396 160 404 164 424 C 168 442 188 432 200 424 Z', PIVOT), st.bow));
  out.push(L(ell(200, 421, 8, 8), mix(st.bow, st.shadeTint, 0.2)));
  // choker + bell (signature accessory)
  out.push(L('M 176 358 Q 200 372 224 358 L 224 372 Q 200 386 176 372 Z', st.bow));
  out.push(L(ell(200, 386, 10, 10), st.bell), line('M 191 387 L 209 387', st.lineW * 0.7, st.line), `<circle cx="200" cy="392" r="${f(st.lineW * 0.7)}" fill="${st.line}"/>`);

  // ---- ears (behind bangs, in front of back hair)
  const earO = 'M 94 190 C 86 142 90 96 108 60 C 146 76 178 104 194 134 Z';
  const earI = 'M 110 152 C 106 120 108 94 116 80 C 138 92 156 108 166 128 Z';
  out.push(L(earO, c.ear), L(flipX(earO, PIVOT), c.ear));
  out.push(L(earI, st.inner, { noStroke: true, shade: st.shade === 'none' ? 'none' : st.shade }), L(flipX(earI, PIVOT), st.inner, { noStroke: true }));

  // ---- face
  const jaw = st.chin ? 352 : 344;
  const face = st.chin
    ? `M 200 126 C 280 126 322 176 320 236 C 318 292 276 322 240 342 Q 200 360 160 342 C 124 322 82 292 80 236 C 78 176 120 126 200 126 Z`
    : `M 200 126 C 282 126 324 176 322 238 C 320 300 272 344 200 ${jaw} C 128 344 80 300 78 238 C 76 176 118 126 200 126 Z`;
  out.push(L(face, st.skin, { shade: st.shade }));

  // bang shadow on forehead (cast by the fringe)
  const bangs = bangPath(m);
  const fid = nid('fc');
  defs.push(`<clipPath id="${fid}"><path d="${face}"/></clipPath>`);
  const shadowFill = mix(st.skin, st.shadeTint, 0.22);
  out.push(`<g clip-path="url(#${fid})"><path d="${bangs}" transform="translate(0 20)" fill="${shadowFill}"${st.shade === 'soft' ? ` filter="url(#${blur(defs, 6)})"` : ''}/></g>`);

  // ---- features
  out.push(...features(m, st, c, defs, { P, line }));

  // ---- front hair
  out.push(L(bangs, c.hair));
  out.push(line('M 150 140 C 176 126 224 126 250 140', st.lineW * 1.1, c.hairLit, { op: 0.6 }));
  // parting wisp
  out.push(line('M 200 110 C 196 140 190 160 180 176', st.lineW * 0.8, c.hairDark, { op: 0.55 }));

  // ---- hair accessories
  if (m.hairStyle === 'twin') {
    for (const flip of [false, true]) {
      const g = ribbon(92, 150, c, st, { P, line });
      out.push(flip ? `<g transform="translate(${PIVOT} 0) scale(-1 1)">${g}</g>` : g);
    }
  }
  if (m.hairStyle === 'tiara') {
    out.push(L('M 130 128 L 150 90 L 174 118 L 200 78 L 226 118 L 250 90 L 270 128 Q 200 112 130 128 Z', '#F5C84A'));
    for (const [x, y, col] of [[200, 100, '#E85D8C'], [160, 112, '#7FC6E8'], [240, 112, '#7FC6E8']]) out.push(L(ell(x, y, 8, 8), col, { noShade: true, lw: 0.7 }));
  }
  if (m.clip === 'star') out.push(L(star(268, 156, 22, 10, 5), '#FFE27A'));
  if (m.hairStyle === 'bob') out.push(L('M 270 150 L 300 150 L 300 160 L 270 160 Z', '#C9D4E8', { lw: 0.7 }));

  // ---- style flourishes
  if (st.key === 'A') {
    out.push(L(star(352, 120, 14, 5, 4, -90), '#FFFFFF', { noShade: true, lw: 0.6, stroke: '#F4B6C6' }));
    out.push(L(star(46, 80, 10, 4, 4, -90), '#FFFFFF', { noShade: true, lw: 0.6, stroke: '#F4B6C6' }));
  }
  if (st.key === 'B') {
    out.push(L(star(350, 100, 16, 5, 4, -90), '#FFF6D2', { noShade: true, lw: 0.5 }));
    out.push(L(star(48, 90, 11, 4, 4, -90), '#FFF6D2', { noShade: true, lw: 0.5 }));
  }
  return out.join('');
}

function blur(defs, s) {
  const id = nid('bl');
  defs.push(`<filter id="${id}" x="-30%" y="-30%" width="160%" height="160%"><feGaussianBlur stdDeviation="${s}"/></filter>`);
  return id;
}

function bangPath(m) {
  const side = m.hairStyle === 'bob' ? 262 : 280;
  const pts = [[324, side], [310, 214], [298, 188], [274, 232], [250, 188], [226, 226], [200, 178], [176, 226], [150, 190], [126, 232], [102, 190], [90, 214], [76, side]];
  return `M 76 ${side} C 60 150 118 104 200 104 C 282 104 340 150 324 ${side} ` + smooth(pts, false).replace(/^M [\d. -]+ /, '') + ' Z';
}

function ribbon(cx, cy, c, st, { P, line }) {
  return P(`M ${cx} ${cy} C ${cx - 30} ${cy - 22} ${cx - 34} ${cy + 8} ${cx - 8} ${cy + 6} Z`, st.bow) +
    P(`M ${cx} ${cy} C ${cx + 30} ${cy - 22} ${cx + 34} ${cy + 8} ${cx + 8} ${cy + 6} Z`, st.bow) +
    P(ell(cx, cy + 3, 7, 7), mix(st.bow, st.shadeTint, 0.2)) +
    P(ell(cx - 2, cy + 30, 11, 11), st.bell) + line(`M ${cx - 10} ${cy + 31} L ${cx + 6} ${cy + 31}`, st.lineW * 0.6, st.line);
}

function eyeSvg(m, st, c, x, y, side, { P, line }, defs) {
  // side: -1 left eye, +1 right eye
  const s = st.eyeStyle;
  if (s === 'glossy') { // A — big gradient iris, many highlights
    const gid = nid('ig');
    defs.push(`<linearGradient id="${gid}" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="${mix(c.eye, '#3A2A55', 0.45)}"/><stop offset="0.6" stop-color="${c.eye}"/><stop offset="1" stop-color="${mix(c.eye, '#FFFFFF', 0.55)}"/></linearGradient>`);
    return `<path d="${ell(x, y, 26, 33)}" fill="#FFFFFF" stroke="${st.line}" stroke-width="${f(st.lineW * 0.7)}"/>` +
      `<path d="${ell(x, y + 2, 22, 29)}" fill="url(#${gid})"/>` +
      `<path d="${ell(x, y + 2, 11, 16)}" fill="#4A2F4A"/>` +
      `<circle cx="${x - 7}" cy="${y - 10}" r="9" fill="#FFF"/><circle cx="${x + 9}" cy="${y + 12}" r="4.5" fill="#FFF"/><circle cx="${x + 11}" cy="${y - 4}" r="2.2" fill="#FFF" opacity=".9"/>` +
      `<path d="M ${x - 28} ${y - 6} Q ${x} ${y - 44} ${x + 28} ${y - 6}" fill="none" stroke="${st.line}" stroke-width="${f(st.lineW * 1.9)}" stroke-linecap="round"/>` +
      `<path d="M ${x + side * 26} ${y - 10} l ${side * 9} -6" stroke="${st.line}" stroke-width="${f(st.lineW * 1.3)}" stroke-linecap="round"/>`;
  }
  if (s === 'spark') { // B — narrower, taller, star highlight, coloured lash line
    return `<path d="${ell(x, y, 22, 34)}" fill="${mix(c.eye, '#0E0B22', 0.35)}" stroke="${st.line}" stroke-width="${f(st.lineW * 0.8)}"/>` +
      `<path d="${ell(x, y + 6, 17, 26)}" fill="${c.eye}"/>` +
      `<path d="${ell(x, y + 14, 13, 14)}" fill="${mix(c.eye, '#FFFFFF', 0.5)}" opacity=".85"/>` +
      `<path d="${star(x - 5, y - 9, 11, 4, 4, -90)}" fill="#FFF"/>` +
      `<circle cx="${x + 8}" cy="${y + 14}" r="3.4" fill="#FFF"/>` +
      `<path d="M ${x - 26} ${y - 8} Q ${x} ${y - 46} ${x + 26} ${y - 8}" fill="none" stroke="${st.line}" stroke-width="${f(st.lineW * 2.2)}" stroke-linecap="round"/>` +
      `<path d="M ${x + side * 24} ${y - 14} l ${side * 12} -10" stroke="${st.line}" stroke-width="${f(st.lineW * 1.4)}" stroke-linecap="round"/>`;
  }
  // 'dot' — C: simple retro oval, single highlight, bold lash
  return `<path d="${ell(x, y + 2, 17, 25)}" fill="${st.line}"/>` +
    `<path d="${ell(x, y + 10, 11, 12)}" fill="${c.eye}"/>` +
    `<circle cx="${x - 5}" cy="${y - 6}" r="7" fill="#FFF"/>` +
    `<path d="M ${x - 22} ${y - 14} Q ${x} ${y - 40} ${x + 22} ${y - 14}" fill="none" stroke="${st.line}" stroke-width="${f(st.lineW * 1.5)}" stroke-linecap="round"/>`;
}

function features(m, st, c, defs, k) {
  const { line } = k;
  const o = [];
  const ey = 252;
  // blush
  if (st.key === 'C') {
    const pid = nid('hb');
    defs.push(`<pattern id="${pid}" patternUnits="userSpaceOnUse" width="6" height="6" patternTransform="rotate(30)"><circle cx="3" cy="3" r="1.9" fill="${st.blush}"/></pattern>`);
    o.push(`<path d="${ell(136, 290, 26, 15)}" fill="url(#${pid})"/><path d="${ell(264, 290, 26, 15)}" fill="url(#${pid})"/>`);
  } else {
    const gid = nid('bg');
    defs.push(`<radialGradient id="${gid}"><stop offset="0" stop-color="${st.blush}" stop-opacity="${st.key === 'A' ? 0.75 : 0.9}"/><stop offset="1" stop-color="${st.blush}" stop-opacity="0"/></radialGradient>`);
    o.push(`<path d="${ell(136, 290, 32, 20)}" fill="url(#${gid})"/><path d="${ell(264, 290, 32, 20)}" fill="url(#${gid})"/>`);
  }
  // brows
  o.push(line('M 100 206 Q 120 194 144 202', st.lineW * 1.2, mix(c.hair, st.line, 0.45)));
  o.push(line('M 300 206 Q 280 194 256 202', st.lineW * 1.2, mix(c.hair, st.line, 0.45)));
  o.push(eyeSvg(m, st, c, 138, ey, -1, k, defs), eyeSvg(m, st, c, 262, ey, 1, k, defs));
  // nose + mouth (open, genki grin for Suzu; calmer for the rest via a flag)
  o.push(`<circle cx="200" cy="284" r="2.6" fill="${mix(st.skin, st.shadeTint, 0.5)}"/>`);
  const calm = ['Kuudere', 'Mukuchi', 'Ojousama'].includes(m.arch);
  if (calm) {
    o.push(line('M 186 304 Q 200 312 214 304', st.lineW * 1.1, st.mouth));
  } else {
    o.push(`<path d="M 182 296 Q 200 330 218 296 Q 200 302 182 296 Z" fill="${st.mouth}" stroke="${st.line}" stroke-width="${f(st.lineW * 0.8)}" stroke-linejoin="round"/>`);
    o.push(`<path d="M 190 313 Q 200 304 210 313 Q 200 322 190 313 Z" fill="#FF9AA8"/>`);
    o.push(`<path d="M 184 297 L 189 297 L 186.5 304 Z" fill="#FFF" stroke="${st.line}" stroke-width="${f(st.lineW * 0.4)}"/>`);
  }
  // cheek whiskers
  for (const sx of [-1, 1]) for (const dy of [0, 7]) o.push(line(`M ${200 + sx * 108} ${272 + dy} l ${sx * 16} ${dy * 0.4 - 2}`, st.lineW * 0.6, mix(st.skin, st.line, 0.45)));
  return o;
}

// ------------------------------------------------------------------ chibi
/** pose: 'idle' | 'walk' | 'walk2' | 'carry' | 'tired' */
export function chibi(m, st, defs, pose = 'idle') {
  const u = 0.3;
  const { P, T, line, lw } = kit(st, defs, u);
  const c = palette(m, st);
  const o = [];
  const hb = [], hf = [];
  const hs = st.chibiHead ?? 1;
  const wrap = (a) => (hs === 1 ? a.join('') : `<g transform="translate(60 98) scale(${hs}) translate(-60 -98)">${a.join('')}</g>`);
  const bob = pose === 'walk' ? -2 : pose === 'walk2' ? 0 : pose === 'tired' ? 3 : 0;
  const sw = pose === 'walk' ? 11 : pose === 'walk2' ? -11 : 0;
  o.push(`<ellipse cx="60" cy="143" rx="${pose === 'tired' ? 30 : 26}" ry="5.5" fill="#000" opacity="0.18"/>`);
  // tail
  const tail = pose === 'tired' ? 'M 80 112 C 104 120 112 132 100 140' : `M 80 112 C 104 112 116 92 106 74${pose === 'walk' ? ' C 104 68 100 64 96 64' : ''}`;
  o.push(T(tail, 9, c.hair), `<circle cx="${pose === 'tired' ? 100 : pose === 'walk' ? 96 : 106}" cy="${pose === 'tired' ? 140 : pose === 'walk' ? 64 : 74}" r="5" fill="#FFF3E6" stroke="${st.line}" stroke-width="${f(lw)}"/>`);
  // back pigtails
  if (m.hairStyle === 'twin') {
    const pig = `M 34 40 C 8 40 4 76 14 100 C 24 92 34 80 38 62 Z`;
    hb.push(P(pig, c.hair), P(flipX(pig, 120), c.hair));
  } else if (m.hairStyle === 'long' || m.hairStyle === 'tiara') {
    hb.push(P('M 60 22 C 96 22 104 56 100 84 C 98 106 104 116 96 124 L 24 124 C 16 116 22 106 20 84 C 16 56 24 22 60 22 Z', c.hair));
  } else {
    hb.push(P('M 60 22 C 94 22 104 48 100 72 C 98 84 94 90 88 92 L 32 92 C 26 90 22 84 20 72 C 16 48 26 22 60 22 Z', c.hair));
  }
  o.push(wrap(hb));
  // legs + shoes
  o.push(P(`M ${44 + sw * 0.8} 112 L ${44 + sw * 0.8} ${130 + bob} L ${54 + sw * 0.8} ${130 + bob} L ${54 + sw * 0.8} 112 Z`, st.skin, { noShade: true }));
  o.push(P(`M ${66 - sw * 0.8} 112 L ${66 - sw * 0.8} ${130 + bob} L ${76 - sw * 0.8} ${130 + bob} L ${76 - sw * 0.8} 112 Z`, st.skin, { noShade: true }));
  o.push(P(ell(49 + sw * 0.8, 134 + bob, 8, 5), '#5A3E4A', { noShade: true }), P(ell(71 - sw * 0.8, 134 + bob, 8, 5), '#5A3E4A', { noShade: true }));
  // dress + apron
  o.push(P('M 34 118 Q 60 128 86 118 L 78 92 Q 60 86 42 92 Z', st.uniform));
  o.push(P('M 48 94 Q 60 90 72 94 L 74 118 Q 60 122 46 118 Z', st.apron));
  o.push(P('M 44 92 Q 60 100 76 92 L 70 88 Q 60 92 50 88 Z', st.collar, { noShade: true }));
  o.push(P(ell(60, 95, 5, 4), st.bow, { noShade: true }));
  o.push(P(ell(60, 101, 3.4, 3.4), st.bell, { noShade: true, lw: 0.7 }));
  // arms
  const armL = pose === 'carry' ? [46, 100] : pose === 'idle' ? [32, 104] : [34, 102 + (pose === 'walk' ? 4 : -2)];
  const armR = pose === 'carry' ? [74, 100] : pose === 'idle' ? [90, 84] : [86, 102 + (pose === 'walk' ? -2 : 4)];
  o.push(P(ell(armL[0], armL[1], 5.5, 6.5), st.skin, { noShade: true }), P(ell(armR[0], armR[1], 5.5, 6.5), st.skin, { noShade: true }));
  if (pose === 'carry') { // tray with a cup
    o.push(P('M 26 96 L 94 96 L 90 102 L 30 102 Z', '#C9A77E', { noShade: true }));
    o.push(P('M 52 78 L 68 78 L 66 96 L 54 96 Z', '#FFFFFF', { noShade: true, lw: 0.8 }), P(ell(60, 78, 8, 3), '#7B4A2E', { noShade: true, lw: 0.7 }));
  }
  // ears
  const ear = 'M 22 40 C 16 24 18 12 26 4 C 40 8 50 16 54 26 Z';
  const earIn = 'M 28 32 C 25 22 27 15 30 10 C 38 13 44 18 46 24 Z';
  hf.push(P(ear, c.ear), P(flipX(ear, 120), c.ear));
  hf.push(P(earIn, st.inner, { noShade: true, noStroke: true }), P(flipX(earIn, 120), st.inner, { noShade: true, noStroke: true }));
  // head
  const head = ell(60, 58 + bob * 0.5, 40, 35);
  hf.push(P(head, st.skin));
  // eyes
  const ey = 62 + bob * 0.5;
  const eyes = (x) => st.eyeStyle === 'glossy'
    ? `<path d="${ell(x, ey, 7, 9.5)}" fill="#FFF" stroke="${st.line}" stroke-width="${f(lw * 0.7)}"/><path d="${ell(x, ey + 1, 5.6, 8)}" fill="${c.eye}"/><path d="${ell(x, ey + 1, 3, 5)}" fill="#4A2F4A"/><circle cx="${x - 2}" cy="${ey - 3}" r="2.6" fill="#FFF"/><circle cx="${x + 2.5}" cy="${ey + 4}" r="1.3" fill="#FFF"/>`
    : st.eyeStyle === 'spark'
      ? `<path d="${ell(x, ey, 6, 9.5)}" fill="${mix(c.eye, '#0E0B22', 0.35)}"/><path d="${ell(x, ey + 2, 4.6, 7)}" fill="${c.eye}"/><path d="${star(x - 1.6, ey - 3, 3.4, 1.3, 4)}" fill="#FFF"/>`
      : `<path d="${ell(x, ey + 1, 5, 7.4)}" fill="${st.line}"/><circle cx="${x - 1.6}" cy="${ey - 2}" r="2.2" fill="#FFF"/>`;
  if (pose === 'tired') {
    hf.push(line(`M ${34} ${ey} Q ${42} ${ey + 6} ${50} ${ey}`, lw * 1.2, st.line), line(`M ${70} ${ey} Q ${78} ${ey + 6} ${86} ${ey}`, lw * 1.2, st.line));
    hf.push(line('M 52 78 Q 60 74 68 78', lw, st.mouth));
    hf.push(`<text x="96" y="30" font-family="${st.fonts.body}" font-weight="700" font-size="14" fill="${st.textDim}">z</text>`);
  } else {
    hf.push(eyes(44), eyes(76));
    hf.push(pose === 'idle' || pose === 'carry'
      ? `<path d="M 54 76 Q 60 84 66 76 Q 60 78 54 76 Z" fill="${st.mouth}" stroke="${st.line}" stroke-width="${f(lw * 0.6)}" stroke-linejoin="round"/>`
      : line('M 55 77 Q 60 82 65 77', lw * 1.1, st.mouth));
  }
  hf.push(`<path d="${ell(34, 72 + bob * 0.5, 7, 4)}" fill="${st.blush}" opacity="${st.key === 'C' ? 0.8 : 0.6}"/><path d="${ell(86, 72 + bob * 0.5, 7, 4)}" fill="${st.blush}" opacity="${st.key === 'C' ? 0.8 : 0.6}"/>`);
  // bangs
  const bang = 'M 21 54 C 18 28 40 18 60 18 C 80 18 102 28 99 54 C 94 44 90 40 86 36 C 84 46 78 52 72 54 C 70 44 66 38 60 34 C 54 38 50 44 48 54 C 42 52 36 46 34 36 C 30 40 24 46 21 54 Z';
  hf.push(P(bang, c.hair));
  // accessories
  if (m.hairStyle === 'twin') {
    for (const fl of [false, true]) {
      const g = P('M 24 42 C 14 36 12 48 20 50 Z', st.bow, { noShade: true }) + P('M 24 42 C 34 34 36 48 28 50 Z', st.bow, { noShade: true }) + P(ell(24, 56, 4, 4), st.bell, { noShade: true, lw: 0.7 });
      hf.push(fl ? `<g transform="translate(120 0) scale(-1 1)">${g}</g>` : g);
    }
  }
  if (m.hairStyle === 'tiara') hf.push(P('M 38 24 L 44 12 L 52 22 L 60 8 L 68 22 L 76 12 L 82 24 Q 60 20 38 24 Z', '#F5C84A', { noShade: true, lw: 0.8 }));
  if (m.clip === 'star') hf.push(P(star(84, 34, 8, 3.6, 5), '#FFE27A', { noShade: true, lw: 0.8 }));
  o.push(wrap(hf));
  return o.join('');
}
