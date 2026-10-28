// Cards, Escalação screen, restaurant strip and style tile for each direction.
// These are exploratory vector mockups, NOT Unity captures.
import { f, nid, mix, adj, ell, rrect, star, heart, contrast, esc } from './lib.mjs';
import { ACCENT, ARCH_LABEL, MAIDS, RARITY, SALARY } from './styles.mjs';
import { portrait, chibi } from './art.mjs';

// ------------------------------------------------------------------ text helpers
export const txt = (s, x, y, o = {}) =>
  `<text x="${f(x)}" y="${f(y)}" font-family="${o.font}" font-size="${o.size ?? 14}" font-weight="${o.weight ?? 700}" fill="${o.fill}" text-anchor="${o.anchor ?? 'start'}"${o.ls ? ` letter-spacing="${o.ls}"` : ''}${o.op ? ` opacity="${o.op}"` : ''}>${esc(s)}</text>`;
const onColour = (bg, st) => (contrast(bg, '#FFFFFF') >= contrast(bg, '#2A2228') ? '#FFFFFF' : '#2A2228');
const shadow = (defs, dy = 4, blur = 6, op = 0.25, col = '#000') => {
  const id = nid('sh');
  defs.push(`<filter id="${id}" x="-20%" y="-20%" width="140%" height="150%"><feDropShadow dx="0" dy="${dy}" stdDeviation="${blur}" flood-color="${col}" flood-opacity="${op}"/></filter>`);
  return id;
};
const hardShadow = (d, st, dx = 4, dy = 4, col) => `<path d="${d}" transform="translate(${dx} ${dy})" fill="${col ?? st.ink}"/>`;

/** Rarity gems: 1-4 shapes — a non-colour cue so rarity is not carried by hue alone. */
function gems(st, rarity, x, y, size = 7) {
  const n = RARITY.indexOf(rarity) + 1, col = st.rar[rarity];
  let o = '';
  for (let i = 0; i < 4; i++) {
    const on = i < n, cx = x + i * (size * 2.4);
    const shape = st.key === 'A' ? heart(cx, y, size * 0.62) : star(cx, y, size, size * 0.46, 5);
    o += `<path d="${shape}" fill="${on ? col : 'none'}" stroke="${on ? (st.key === 'B' ? st.ink : st.line) : mix(st.textDim, st.panel, 0.4)}" stroke-width="${st.key === 'C' ? 1.6 : 1.2}" stroke-linejoin="round" opacity="${on ? 1 : 0.55}"/>`;
  }
  return o;
}

// ------------------------------------------------------------------ card plate (portrait window)
function plate(m, st, defs, x, y, w, h, r) {
  const acc = ACCENT[m.arch];
  const gid = nid('pl'), cid = nid('pc');
  defs.push(`<clipPath id="${cid}"><path d="${rrect(x, y, w, h, r)}"/></clipPath>`);
  if (st.key === 'A') defs.push(`<linearGradient id="${gid}" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="${mix('#FFF6EC', acc, 0.16)}"/><stop offset="1" stop-color="${mix('#FFE0E8', acc, 0.24)}"/></linearGradient>`);
  if (st.key === 'B') defs.push(`<radialGradient id="${gid}" cx="0.5" cy="0.45" r="0.75"><stop offset="0" stop-color="${mix('#3B5560', acc, 0.35)}"/><stop offset="1" stop-color="#18262A"/></radialGradient>`);
  let bg = '';
  if (st.key === 'A') {
    bg = `<rect x="${x}" y="${y}" width="${w}" height="${h}" fill="url(#${gid})"/>` +
      `<circle cx="${x + w * 0.5}" cy="${y + h * 0.46}" r="${w * 0.42}" fill="#FFFFFF" opacity=".55"/>` +
      [0, 1, 2, 3, 4, 5].map((i) => `<circle cx="${x + 20 + i * w * 0.2}" cy="${y + h - 10 - (i % 2) * 12}" r="${4 + (i % 3)}" fill="${mix(acc, '#FFFFFF', 0.6)}" opacity=".5"/>`).join('');
  } else if (st.key === 'B') {
    bg = `<rect x="${x}" y="${y}" width="${w}" height="${h}" fill="url(#${gid})"/>` +
      `<path d="${star(x + w * 0.5, y + h * 0.4, w * 0.5, w * 0.34, 12, -90)}" fill="${acc}" opacity=".12"/>`;
  } else {
    const pid = nid('hp');
    defs.push(`<pattern id="${pid}" patternUnits="userSpaceOnUse" width="9" height="9" patternTransform="rotate(30)"><rect width="9" height="9" fill="${mix(st.panelAlt, acc, 0.22)}"/><circle cx="4.5" cy="4.5" r="2" fill="${mix(st.panelAlt, acc, 0.5)}"/></pattern>`);
    bg = `<rect x="${x}" y="${y}" width="${w}" height="${h}" fill="url(#${pid})"/>` +
      `<circle cx="${x + w * 0.5}" cy="${y + h * 0.52}" r="${w * 0.4}" fill="${st.panel}"/>`;
  }
  return { cid, bg };
}

// ------------------------------------------------------------------ full card
export function cardFull(m, st, defs, W = 280, H = 400) {
  const rar = st.rar[m.rarity], acc = ACCENT[m.arch];
  const o = [];
  const R = st.radius + 4;
  const body = rrect(0, 0, W, H, R);
  if (st.key === 'A') {
    o.push(`<path d="${body}" fill="${st.panel}" filter="url(#${shadow(defs, 6, 7, 0.22, '#8E5F63')})"/>`);
    o.push(`<path d="${rrect(3, 3, W - 6, H - 6, R - 2)}" fill="none" stroke="${rar}" stroke-width="7"/>`);
    o.push(`<path d="${rrect(10, 10, W - 20, H - 20, R - 6)}" fill="none" stroke="${mix(rar, '#FFFFFF', 0.55)}" stroke-width="1.5" stroke-dasharray="1 5" stroke-linecap="round"/>`);
    const nr = RARITY.indexOf(m.rarity);
    if (nr >= 2) for (const [sx, sy] of [[10, 10], [W - 10, 10], ...(nr === 3 ? [[10, H - 10], [W - 10, H - 10]] : [])]) o.push(`<path d="${star(sx, sy, nr === 3 ? 15 : 11, 4.5, 4)}" fill="#FFFFFF" stroke="${rar}" stroke-width="2.2" stroke-linejoin="round"/>`);
    if (nr === 3) o.push(`<path d="${rrect(1, 1, W - 2, H - 2, R)}" fill="none" stroke="${mix(rar, '#FFFFFF', 0.6)}" stroke-width="2"/>`);
  } else if (st.key === 'B') {
    o.push(`<path d="${body}" fill="${st.panel}" filter="url(#${shadow(defs, 5, 8, 0.5)})"/>`);
    const gl = nid('gl');
    defs.push(`<filter id="${gl}" x="-20%" y="-20%" width="140%" height="140%"><feGaussianBlur stdDeviation="5"/></filter>`);
    o.push(`<path d="${rrect(3, 3, W - 6, H - 6, R)}" fill="none" stroke="${rar}" stroke-width="6" opacity=".55" filter="url(#${gl})"/>`);
    o.push(`<path d="${rrect(4, 4, W - 8, H - 8, R)}" fill="none" stroke="${rar}" stroke-width="3.4" stroke-linecap="round"/>`);
    o.push(`<path d="${rrect(11, 11, W - 22, H - 22, R - 4)}" fill="none" stroke="${st.ink}" stroke-width="1.3" stroke-dasharray="7 5" opacity=".5"/>`);
  } else {
    o.push(hardShadow(body, st, 6, 6));
    o.push(`<path d="${body}" fill="${st.panel}" stroke="${st.ink}" stroke-width="4"/>`);
    o.push(`<path d="${rrect(0, 0, W, 46, R)}" fill="${rar}"/><rect x="0" y="30" width="${W}" height="16" fill="${rar}"/>`);
    o.push(`<path d="${body}" fill="none" stroke="${st.ink}" stroke-width="4"/><line x1="0" y1="46" x2="${W}" y2="46" stroke="${st.ink}" stroke-width="4"/>`);
  }
  // portrait window
  const py = st.key === 'C' ? 58 : 16, ph = st.key === 'C' ? 190 : 236, px = 16, pw = W - 32;
  const pl = plate(m, st, defs, px, py, pw, ph, st.key === 'C' ? 4 : st.radius - 2);
  const sc = pw / 400 * 1.02;
  o.push(`<g clip-path="url(#${pl.cid})">${pl.bg}<g transform="translate(${f(px + (pw - 400 * sc) / 2)} ${f(py - 52 * sc)}) scale(${f(sc)})">${portrait(m, st, defs)}</g></g>`);
  if (st.key === 'C') o.push(`<path d="${rrect(px, py, pw, ph, 4)}" fill="none" stroke="${st.ink}" stroke-width="3.5"/>`);
  else if (st.key === 'B') o.push(`<path d="${rrect(px, py, pw, ph, st.radius - 2)}" fill="none" stroke="${st.ink}" stroke-width="1.6" opacity=".75"/>`);
  // UI-native text (never baked into the portrait)
  const ty = py + ph + 34;
  if (st.key === 'C') o.push(txt(RARITY[RARITY.indexOf(m.rarity)].toUpperCase(), 18, 31, { font: st.fonts.title, size: 17, fill: onColour(rar, st), ls: 2.4 }), gems(st, m.rarity, W - 96, 23, 8).replace(/fill="none"/g, `fill="${st.panelAlt}"`));
  o.push(txt(m.name, 20, ty, { font: st.fonts.title, size: st.key === 'C' ? 25 : 30, fill: st.text, ls: st.key === 'C' ? 1 : 0.4 }));
  const chipTxt = ARCH_LABEL[m.arch], chipW = 14 + chipTxt.length * 8.4;
  o.push(`<path d="${rrect(W - 20 - chipW, ty - 20, chipW, 26, st.key === 'C' ? 4 : 13)}" fill="${acc}" ${st.key === 'C' ? `stroke="${st.ink}" stroke-width="2.4"` : ''}/>` +
    txt(chipTxt, W - 20 - chipW / 2, ty - 2, { font: st.fonts.body, size: 13.5, fill: onColour(acc, st), anchor: 'middle' }));
  o.push(txt(`Posição natural: ${m.pos}`, 20, ty + 22, { font: st.fonts.body, size: 13.5, weight: 600, fill: st.textDim }));
  if (st.key !== 'C') {
    o.push(gems(st, m.rarity, 28, ty + 46, 8.5), txt(m.rarity, 28 + 4 * 20.5, ty + 51, { font: st.fonts.body, size: 14, fill: st.key === 'B' ? rar : mix(rar, '#3A2A2A', 0.45) }));
  } else {
    o.push(txt(`${SALARY[m.rarity]}/dia`, 20, ty + 46, { font: st.fonts.body, size: 14, fill: st.text }));
  }
  o.push(txt(`${SALARY[m.rarity]}/dia`, W - 20, ty + (st.key === 'C' ? 46 : 51), { font: st.fonts.body, size: 14, fill: st.textDim, anchor: 'end' }));
  // stats row
  const labels = ['SAL', 'COZ', 'CHA', 'FÔL'];
  const bw = (W - 32 - 18) / 4;
  m.stats.forEach((v, i) => {
    const bx = 16 + i * (bw + 6), by = H - 56;
    const d = rrect(bx, by, bw, 40, st.key === 'C' ? 3 : 10);
    o.push(`<path d="${d}" fill="${st.key === 'B' ? st.panelAlt : st.key === 'C' ? st.panelAlt : mix(st.bg, '#FFFFFF', 0.3)}" ${st.key === 'C' ? `stroke="${st.ink}" stroke-width="2.4"` : st.key === 'B' ? `stroke="${st.ink}" stroke-opacity=".35" stroke-width="1.2"` : ''}/>`);
    o.push(txt(String(v), bx + bw / 2, by + 22, { font: st.fonts.title, size: 19, fill: st.text, anchor: 'middle' }), txt(labels[i], bx + bw / 2, by + 35, { font: st.fonts.body, size: 9.5, fill: st.textDim, anchor: 'middle', ls: 0.6 }));
  });
  return o.join('');
}

// ------------------------------------------------------------------ compact card (Escalação slots, bench)
export function cardMini(m, st, defs, W = 124, H = 188) {
  const rar = st.rar[m.rarity], acc = ACCENT[m.arch];
  const o = [];
  const R = st.radius;
  const body = rrect(0, 0, W, H, R);
  if (st.key === 'A') {
    o.push(`<path d="${body}" fill="${st.panel}" filter="url(#${shadow(defs, 4, 5, 0.2, '#8E5F63')})"/><path d="${rrect(2, 2, W - 4, H - 4, R - 1)}" fill="none" stroke="${rar}" stroke-width="5"/>`);
  } else if (st.key === 'B') {
    o.push(`<path d="${body}" fill="${st.panel}"/><path d="${rrect(2, 2, W - 4, H - 4, R)}" fill="none" stroke="${rar}" stroke-width="3" stroke-linecap="round"/>`);
  } else {
    o.push(hardShadow(body, st, 4, 4), `<path d="${body}" fill="${st.panel}" stroke="${st.ink}" stroke-width="3"/><path d="${rrect(0, 0, W, 16, R)}" fill="${rar}"/><rect x="0" y="8" width="${W}" height="8" fill="${rar}"/><path d="${body}" fill="none" stroke="${st.ink}" stroke-width="3"/><line x1="0" y1="16" x2="${W}" y2="16" stroke="${st.ink}" stroke-width="2.4"/>`);
  }
  const px = 8, py = st.key === 'C' ? 22 : 8, pw = W - 16, ph = st.key === 'C' ? 96 : 108;
  const pl = plate(m, st, defs, px, py, pw, ph, 8);
  const sc = pw / 290;
  o.push(`<g clip-path="url(#${pl.cid})">${pl.bg}<g transform="translate(${f(px - 55 * sc)} ${f(py - 52 * sc)}) scale(${f(sc)})">${portrait(m, st, defs)}</g></g>`);
  const ty = py + ph + 19;
  o.push(txt(m.name, 10, ty, { font: st.fonts.title, size: 16.5, fill: st.text }));
  const chipTxt = ARCH_LABEL[m.arch], cw = 10 + chipTxt.length * 6.6;
  o.push(`<path d="${rrect(10, ty + 6, cw, 18, st.key === 'C' ? 3 : 9)}" fill="${acc}" ${st.key === 'C' ? `stroke="${st.ink}" stroke-width="1.8"` : ''}/>` + txt(chipTxt, 10 + cw / 2, ty + 19, { font: st.fonts.body, size: 11, fill: onColour(acc, st), anchor: 'middle' }));
  o.push(gems(st, m.rarity, 15, H - 14, 5.2), txt(`${SALARY[m.rarity]}/dia`, W - 10, H - 10, { font: st.fonts.body, size: 11, weight: 600, fill: st.textDim, anchor: 'end' }));
  return o.join('');
}

export function emptySlot(st, W = 124, H = 188) {
  const R = st.radius;
  const d = rrect(2, 2, W - 4, H - 4, R);
  const c = st.key === 'B' ? st.textDim : st.textDim;
  return `<path d="${d}" fill="${st.key === 'B' ? 'none' : mix(st.bg, '#FFFFFF', 0.35)}" stroke="${c}" stroke-width="2.4" stroke-dasharray="9 7" stroke-linecap="round" opacity=".85"/>` +
    txt('Vazio', W / 2, H / 2 - 2, { font: st.fonts.title, size: 19, fill: st.textDim, anchor: 'middle' }) +
    txt('toque para escalar', W / 2, H / 2 + 18, { font: st.fonts.body, size: 11, weight: 600, fill: st.textDim, anchor: 'middle' });
}

// ------------------------------------------------------------------ buttons / chips
export function button(st, defs, x, y, w, h, label, kind = 'primary') {
  const d = rrect(x, y, w, h, st.key === 'C' ? 6 : h / 2.6);
  const fill = kind === 'primary' ? st.focus : kind === 'disabled' ? mix(st.panel, st.textDim, 0.18) : st.panel;
  const col = kind === 'primary' ? st.focusText : kind === 'disabled' ? st.textDim : st.text;
  let o = '';
  if (st.key === 'C') o += hardShadow(d, st, 4, 4);
  else if (st.key === 'A') o += `<path d="${d}" fill="${kind === 'primary' ? mix(st.focus, '#B8741C', 0.35) : mix(st.panel, st.wood, 0.5)}" transform="translate(0 4)"/>`;
  else o += `<path d="${d}" fill="#000" opacity=".35" transform="translate(0 4)"/>`;
  o += `<path d="${d}" fill="${fill}"${st.key === 'C' ? ` stroke="${st.ink}" stroke-width="3"` : st.key === 'B' ? ` stroke="${kind === 'primary' ? '#FFF1B8' : st.ink}" stroke-width="2.2"` : kind === 'secondary' ? ` stroke="${mix(st.wood, '#FFFFFF', 0.3)}" stroke-width="2"` : ''}/>`;
  o += txt(label, x + w / 2, y + h / 2 + h * 0.18, { font: st.fonts.title, size: Math.round(h * 0.42), fill: col, anchor: 'middle', ls: st.key === 'C' ? 1.2 : 0.4 });
  return o;
}
const chip = (st, x, y, label, value, w = 120) => {
  const d = rrect(x, y, w, 30, st.key === 'C' ? 4 : 15);
  return (st.key === 'C' ? hardShadow(d, st, 3, 3) : '') + `<path d="${d}" fill="${st.panel}" ${st.key === 'C' ? `stroke="${st.ink}" stroke-width="2.6"` : st.key === 'B' ? `stroke="${st.ink}" stroke-opacity=".5" stroke-width="1.4"` : ''}/>` +
    txt(label, x + 14, y + 20, { font: st.fonts.body, size: 12, weight: 600, fill: st.textDim }) + txt(value, x + w - 14, y + 20.5, { font: st.fonts.title, size: 16, fill: st.text, anchor: 'end' });
};

// ------------------------------------------------------------------ screens
function screenBg(st, defs, W, H) {
  const o = [];
  if (st.key === 'A') {
    const g = nid('gi');
    defs.push(`<pattern id="${g}" patternUnits="userSpaceOnUse" width="28" height="28"><rect width="28" height="28" fill="${st.bg}"/><rect width="14" height="28" fill="#FFE3EA" opacity=".55"/><rect width="28" height="14" fill="#FFE3EA" opacity=".55"/></pattern>`);
    o.push(`<rect width="${W}" height="${H}" fill="url(#${g})"/>`);
    o.push(`<rect y="${H - 34}" width="${W}" height="34" fill="${st.wood}"/><rect y="${H - 34}" width="${W}" height="5" fill="${mix(st.wood, '#FFFFFF', 0.4)}"/>`);
    for (let i = 0; i < 14; i++) o.push(`<line x1="${i * 74 + 20}" y1="${H - 29}" x2="${i * 74 + 20}" y2="${H}" stroke="${st.woodDark}" stroke-opacity=".35" stroke-width="2"/>`);
  } else if (st.key === 'B') {
    o.push(`<rect width="${W}" height="${H}" fill="${st.bg}"/>`);
    const b = nid('bl');
    defs.push(`<filter id="${b}" x="-50%" y="-50%" width="200%" height="200%"><feGaussianBlur stdDeviation="28"/></filter>`);
    o.push(`<ellipse cx="260" cy="260" rx="260" ry="120" fill="#FFFFFF" opacity=".05" filter="url(#${b})"/><ellipse cx="760" cy="440" rx="220" ry="110" fill="#FFFFFF" opacity=".05" filter="url(#${b})"/>`);
    o.push(`<rect x="3" y="3" width="${W - 6}" height="${H - 6}" fill="none" stroke="${st.wood}" stroke-width="6"/><rect x="0" y="0" width="${W}" height="${H}" fill="none" stroke="${st.woodDark}" stroke-width="3"/>`);
  } else {
    const p = nid('st');
    defs.push(`<pattern id="${p}" patternUnits="userSpaceOnUse" width="26" height="26" patternTransform="rotate(45)"><rect width="26" height="26" fill="${st.bg}"/><rect width="9" height="26" fill="${mix(st.bg, st.mint, 0.35)}"/></pattern>`);
    o.push(`<rect width="${W}" height="${H}" fill="url(#${p})"/>`);
    const ck = nid('ck');
    defs.push(`<pattern id="${ck}" patternUnits="userSpaceOnUse" width="32" height="32"><rect width="32" height="32" fill="${st.panel}"/><rect width="16" height="16" fill="${st.uniform}"/><rect x="16" y="16" width="16" height="16" fill="${st.uniform}"/></pattern>`);
    o.push(`<rect y="${H - 32}" width="${W}" height="32" fill="url(#${ck})"/><line x1="0" y1="${H - 32}" x2="${W}" y2="${H - 32}" stroke="${st.ink}" stroke-width="3.5"/>`);
  }
  return o.join('');
}

function panel(st, defs, x, y, w, h, title) {
  const d = rrect(x, y, w, h, st.radius + 4);
  let o = '';
  if (st.key === 'A') o += `<path d="${d}" fill="${st.panel}" opacity=".92" filter="url(#${shadow(defs, 4, 6, 0.14, '#8E5F63')})"/>`;
  else if (st.key === 'B') o += `<path d="${d}" fill="${st.panel}" opacity=".96"/><path d="${rrect(x + 5, y + 5, w - 10, h - 10, st.radius)}" fill="none" stroke="${st.ink}" stroke-opacity=".45" stroke-width="1.5" stroke-dasharray="8 6"/>`;
  else o += hardShadow(d, st, 5, 5) + `<path d="${d}" fill="${st.panel}" stroke="${st.ink}" stroke-width="3.5"/>`;
  if (title) o += txt(title, x + 18, y + 28, { font: st.fonts.title, size: st.key === 'C' ? 15 : 19, fill: st.text, ls: st.key === 'C' ? 2 : 0.4 });
  return o;
}

export function lineupScreen(st, defs) {
  const W = 960, H = 600, o = [];
  o.push(screenBg(st, defs, W, H));
  // header
  if (st.key === 'A') o.push(`<path d="M 0 0 H 960 V 52 Q 940 66 920 52 Q 900 66 880 52 ${Array.from({ length: 40 }, (_, i) => `Q ${860 - i * 40} 66 ${840 - i * 40} 52`).join(' ')} V 0 Z" fill="${st.pink}"/>`);
  else if (st.key === 'B') o.push(`<rect x="6" y="6" width="948" height="58" fill="${st.bg2}"/>`);
  else o.push(`<rect width="960" height="62" fill="${st.alarm}"/><line x1="0" y1="62" x2="960" y2="62" stroke="${st.ink}" stroke-width="4"/>`);
  const tcol = st.key === 'C' ? '#FFF8E6' : st.key === 'A' ? '#8F3F5A' : st.text;
  o.push(txt('Escalação', 28, 44, { font: st.fonts.title, size: st.key === 'C' ? 28 : 36, fill: tcol, ls: st.key === 'C' ? 3 : 0.8 }));
  o.push(txt('Quem trabalha hoje?', st.key === 'C' ? 330 : 210, 43, { font: st.fonts.body, size: 15, weight: 600, fill: st.key === 'C' ? '#FFE7C2' : st.textDim }));
  o.push(chip(st, 620, 17, 'Caixa', '1.240', 150), chip(st, 784, 17, 'Fichas', '35', 150));
  // slots panel
  o.push(panel(st, defs, 20, 76, 668, 292, 'POSIÇÕES'));
  const slots = [['Recepção', 'hana'], ['Salão', 'suzu'], ['Cozinha', 'kuro'], ['Palco', 'lumi'], ['Gerência', null]];
  slots.forEach(([pos, id], i) => {
    const x = 34 + i * 128, y = 142;
    o.push(txt(pos.toUpperCase(), x + 62, 134, { font: st.fonts.body, size: 11.5, weight: 800, fill: st.textDim, anchor: 'middle', ls: 1.4 }));
    o.push(`<g transform="translate(${x} ${y})">${id ? cardMini(MAIDS[id], st, defs) : emptySlot(st)}</g>`);
    if (id) {
      const ok = MAIDS[id].pos === pos;
      const col = ok ? (st.key === 'A' ? '#2F8A62' : st.good) : st.alarm;
      if (ok) o.push(`<path d="M ${x + 33} ${346} l 3.6 4 l 7 -8.4" fill="none" stroke="${col}" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"/>`, txt('natural', x + 51, 351, { font: st.fonts.body, size: 11.5, weight: 700, fill: col }));
      else o.push(txt('−35% fora de posto', x + 62, 351, { font: st.fonts.body, size: 11.5, weight: 700, fill: col, anchor: 'middle' }));
    }
  });
  // bench
  o.push(panel(st, defs, 20, 378, 668, 198, 'BANCO'));
  ['pip', 'mochi', 'nori'].forEach((id, i) => {
    const x = 36 + i * 134, s = 0.8;
    o.push(`<g transform="translate(${x} 414) scale(${s})">${cardMini(MAIDS[id], st, defs)}</g>`);
  });
  o.push(txt('Toque numa maid do banco', 462, 466, { font: st.fonts.title, size: st.key === 'C' ? 13 : 17, fill: st.text }), txt('e depois no posto desejado.', 462, 490, { font: st.fonts.body, size: 13.5, weight: 600, fill: st.textDim }));
  o.push(txt('Posto vazio é uma escolha válida.', 462, 516, { font: st.fonts.body, size: 12.5, weight: 600, fill: st.textDim }));
  // chemistry + summary
  o.push(panel(st, defs, 704, 76, 236, 500, 'RESUMO DO DIA'));
  o.push(txt('Química', 722, 130, { font: st.fonts.body, size: 13, weight: 700, fill: st.textDim }), txt('72%', 922, 132, { font: st.fonts.title, size: 24, fill: st.text, anchor: 'end' }));
  const bar = rrect(722, 142, 200, 16, st.key === 'C' ? 3 : 8);
  o.push(`<path d="${bar}" fill="${st.key === 'B' ? '#16201F' : mix(st.bg, st.textDim, 0.22)}" ${st.key === 'C' ? `stroke="${st.ink}" stroke-width="2.6"` : ''}/><path d="${rrect(722, 142, 144, 16, st.key === 'C' ? 3 : 8)}" fill="${st.key === 'A' ? '#58B88C' : st.good}" ${st.key === 'C' ? `stroke="${st.ink}" stroke-width="2.6"` : ''}/>`);
  const rows = [['Posição natural', '4 de 5'], ['Salários do dia', '−252'], ['Fã esperada', 'Genki']];
  rows.forEach(([a, b], i) => {
    const y = 192 + i * 38;
    o.push(txt(a, 722, y, { font: st.fonts.body, size: 14, weight: 600, fill: st.text }), txt(b, 922, y, { font: st.fonts.title, size: 16, fill: st.text, anchor: 'end' }));
    o.push(`<line x1="722" y1="${y + 12}" x2="922" y2="${y + 12}" stroke="${st.key === 'C' ? st.ink : st.textDim}" stroke-opacity="${st.key === 'C' ? 0.8 : 0.28}" stroke-width="${st.key === 'C' ? 1.8 : 1.2}" ${st.key === 'C' ? 'stroke-dasharray="3 4"' : ''}/>`);
  });
  o.push(txt('Prévia no salão', 722, 316, { font: st.fonts.body, size: 12, weight: 800, fill: st.textDim, ls: 1 }));
  [['hana', 'idle', 746], ['suzu', 'walk', 822], ['kuro', 'idle', 898]].forEach(([id, pose, cx]) => o.push(`<g transform="translate(${cx - 38} 322) scale(0.63)">${chibi(MAIDS[id], st, defs, pose)}</g>`));
  o.push(txt('Fã de Genki chega às 14h:', 722, 440, { font: st.fonts.body, size: 12.5, weight: 600, fill: st.textDim }), txt('Suzu escalada = gorjeta 1,8×', 722, 458, { font: st.fonts.body, size: 12.5, weight: 700, fill: st.text }));
  o.push(button(st, defs, 722, 472, 200, 52, 'Abrir o café', 'primary'));
  o.push(button(st, defs, 722, 530, 200, 38, 'Voltar', 'secondary'));
  if (st.key === 'C') o.push(`<path d="${rrect(250, 574, 460, 20, 4)}" fill="${st.panel}" stroke="${st.ink}" stroke-width="2"/>`);
  o.push(txt('Valores ilustrativos — mockup exploratório, não é captura do Unity', 480, 588, { font: st.fonts.body, size: 10.5, weight: 600, fill: st.key === 'B' ? st.textDim : st.text, anchor: 'middle', op: 0.9 }));
  return { W, H, body: o.join('') };
}

// ------------------------------------------------------------------ restaurant strip
export function restaurantStrip(st, defs) {
  const W = 960, H = 420, o = [];
  const floorY = 250;
  if (st.key === 'A') {
    o.push(`<rect width="${W}" height="${floorY}" fill="#D6F1E5"/>`);
    const g = nid('gi');
    defs.push(`<pattern id="${g}" patternUnits="userSpaceOnUse" width="24" height="24"><rect width="24" height="24" fill="#E8F8F1"/><rect width="12" height="24" fill="#CDEFE0" opacity=".7"/></pattern>`);
    o.push(`<rect y="0" width="${W}" height="${floorY - 80}" fill="url(#${g})"/>`);
    o.push(`<rect y="${floorY - 14}" width="${W}" height="14" fill="${st.wood}"/>`);
    o.push(`<rect y="${floorY}" width="${W}" height="${H - floorY}" fill="#F0D9BC"/>`);
    for (let i = 0; i < 18; i++) o.push(`<line x1="${i * 56}" y1="${floorY}" x2="${i * 56 - 40}" y2="${H}" stroke="${st.wood}" stroke-opacity=".55" stroke-width="2"/>`);
    // awning
    o.push(`<path d="M 40 20 H 300 L 320 70 H 20 Z" fill="${st.pink}" stroke="${st.line}" stroke-width="2.4"/>` + Array.from({ length: 6 }, (_, i) => `<path d="M ${30 + i * 48} 70 q 24 20 48 0" fill="#FFFFFF" stroke="${st.line}" stroke-width="2.4"/>`).join(''));
    o.push(txt('NEKO MAID Inc.', 170, 52, { font: st.fonts.title, size: 22, fill: '#8F3F5A', anchor: 'middle', ls: 2 }));
    o.push(`<path d="${rrect(640, 40, 220, 110, 14)}" fill="#FFFFFF" stroke="${st.line}" stroke-width="2.4"/><path d="${rrect(652, 52, 196, 86, 8)}" fill="#BFE6F5"/>`);
  } else if (st.key === 'B') {
    o.push(`<rect width="${W}" height="${floorY}" fill="${st.bg}"/><rect y="${floorY}" width="${W}" height="${H - floorY}" fill="#3A2A21"/>`);
    for (let i = 0; i < 18; i++) o.push(`<line x1="${i * 56}" y1="${floorY}" x2="${i * 56 - 40}" y2="${H}" stroke="#5E3E27" stroke-width="2.4"/>`);
    o.push(`<rect y="${floorY - 12}" width="${W}" height="12" fill="${st.wood}"/>`);
    o.push(`<path d="${rrect(30, 24, 280, 150, 6)}" fill="#18262A" stroke="${st.wood}" stroke-width="8"/>`);
    o.push(txt('Menu do dia', 170, 62, { font: st.fonts.title, size: 28, fill: st.text, anchor: 'middle' }), txt('café ··· 8', 100, 100, { font: st.fonts.title, size: 20, fill: '#7CE0C3' }), txt('bolo ··· 12', 100, 130, { font: st.fonts.title, size: 20, fill: '#FF9DB0' }), txt('leite (gato) ··· 3', 100, 160, { font: st.fonts.title, size: 18, fill: '#FFD66B' }));
    for (const [x, c] of [[470, '#FFD66B'], [740, '#FF9DB0'], [880, '#7CE0C3']]) {
      const b = nid('lb'); defs.push(`<radialGradient id="${b}"><stop offset="0" stop-color="${c}" stop-opacity=".55"/><stop offset="1" stop-color="${c}" stop-opacity="0"/></radialGradient>`);
      o.push(`<circle cx="${x}" cy="70" r="90" fill="url(#${b})"/><line x1="${x}" y1="0" x2="${x}" y2="40" stroke="${st.textDim}" stroke-width="2"/><path d="${ell(x, 48, 16, 12)}" fill="${c}"/>`);
    }
  } else {
    o.push(`<rect width="${W}" height="${floorY}" fill="${st.alarm}"/>`);
    for (let i = 0; i < 14; i++) o.push(`<rect x="${i * 72}" y="0" width="36" height="${floorY - 14}" fill="#F1664F" opacity=".7"/>`);
    const ck = nid('ck');
    defs.push(`<pattern id="${ck}" patternUnits="userSpaceOnUse" width="56" height="56"><rect width="56" height="56" fill="${st.panel}"/><rect width="28" height="28" fill="${st.uniform}"/><rect x="28" y="28" width="28" height="28" fill="${st.uniform}"/></pattern>`);
    o.push(`<rect y="${floorY}" width="${W}" height="${H - floorY}" fill="url(#${ck})"/><line x1="0" y1="${floorY}" x2="${W}" y2="${floorY}" stroke="${st.ink}" stroke-width="4"/>`);
    o.push(`<path d="${rrect(40, 24, 300, 70, 8)}" fill="${st.focus}" stroke="${st.ink}" stroke-width="4"/>` + txt('KISSATEN', 190, 74, { font: st.fonts.title, size: 38, fill: st.ink, anchor: 'middle', ls: 5 }));
    o.push(`<path d="${rrect(640, 40, 220, 110, 6)}" fill="#BFE3E0" stroke="${st.ink}" stroke-width="4"/><line x1="750" y1="40" x2="750" y2="150" stroke="${st.ink}" stroke-width="4"/><line x1="640" y1="95" x2="860" y2="95" stroke="${st.ink}" stroke-width="4"/>`);
  }
  // furniture + chibis
  const table = (cx, cy) => {
    const top = ell(cx, cy, 50, 15);
    const topFill = st.key === 'A' ? '#FFFFFF' : st.key === 'B' ? st.wood : st.focus;
    return `<rect x="${cx - 5}" y="${cy}" width="10" height="46" fill="${st.key === 'B' ? st.woodDark : st.wood}" stroke="${st.key === 'B' ? '#000' : st.line}" stroke-opacity=".6" stroke-width="2"/>` +
      `<path d="${top}" fill="${topFill}" stroke="${st.key === 'C' ? st.ink : st.key === 'B' ? '#000' : st.line}" stroke-opacity="${st.key === 'B' ? 0.5 : 1}" stroke-width="${st.key === 'C' ? 3.5 : 2.4}"/>` +
      `<path d="${ell(cx - 14, cy - 2, 9, 4)}" fill="${st.key === 'B' ? '#F5EEDD' : '#FFF'}" opacity=".9"/><path d="${ell(cx + 16, cy - 3, 6, 3)}" fill="${st.bow}"/>`;
  };
  o.push(table(560, 310), table(800, 330), table(120, 330));
  const place = (m, x, y, pose, s = 0.62) => `<g transform="translate(${x - 60 * s} ${y - 143 * s}) scale(${s})">${chibi(m, st, defs, pose)}</g>`;
  o.push(place(MAIDS.hana, 330, 270, 'idle'), place(MAIDS.kuro, 250, 258, 'idle'), place(MAIDS.suzu, 450, 360, 'carry'), place(MAIDS.nori, 660, 392, 'walk'), place(MAIDS.pip, 700, 290, 'walk2'), place(MAIDS.lumi, 900, 275, 'idle'), place(MAIDS.mochi, 40, 270, 'tired'));
  if (st.key !== 'A') o.push(`<path d="${rrect(170, H - 26, 620, 20, 4)}" fill="${st.key === 'B' ? st.bg2 : st.panel}" opacity=".92"/>`);
  o.push(txt('Mockup exploratório — chibis a ≈0,62× (≈ 90 px de altura); escala real do jogo não verificada', 480, H - 10, { font: st.fonts.body, size: 11, weight: 600, fill: st.key === 'B' ? st.textDim : st.text, anchor: 'middle', op: 0.9 }));
  return { W, H, body: o.join('') };
}

// ------------------------------------------------------------------ style tile
export function styleTile(st, defs, extra = {}) {
  const W = 960, H = 600, o = [];
  o.push(`<rect width="${W}" height="${H}" fill="${st.bg}"/>`);
  o.push(txt(`${st.key} · ${st.name}`, 32, 54, { font: st.fonts.title, size: 36, fill: st.key === 'C' ? st.alarm : st.text, ls: st.key === 'C' ? 2 : 0.5 }), txt(st.tagline, 32, 82, { font: st.fonts.body, size: 15, weight: 600, fill: st.textDim }));
  // swatches
  const sw = [['fundo', st.bg], ['painel', st.panel], ['painel alt.', st.panelAlt], ['texto', st.text], ['texto fraco', st.textDim], ['destaque', st.focus], ['bom', st.good], ['alarme', st.alarm], ['madeira', st.wood], ['uniforme', st.uniform]];
  sw.forEach(([n, c], i) => {
    const x = 32 + (i % 5) * 92, y = 110 + Math.floor(i / 5) * 84;
    o.push(`<path d="${rrect(x, y, 80, 48, st.key === 'C' ? 3 : 10)}" fill="${c}" stroke="${st.key === 'C' ? st.ink : mix(c, '#888', 0.4)}" stroke-width="${st.key === 'C' ? 3 : 1.4}"/>`, txt(n, x, y + 62, { font: st.fonts.body, size: 11.5, fill: st.text }), txt(c.toUpperCase(), x, y + 76, { font: st.fonts.body, size: 10.5, weight: 600, fill: st.textDim }));
  });
  // rarity frames
  o.push(txt('RARIDADE (GDD §2.8: bege · azul · violeta · dourado)', 32, 296, { font: st.fonts.body, size: 12, weight: 800, fill: st.textDim, ls: 1 }));
  RARITY.forEach((r, i) => {
    const x = 32 + i * 118;
    o.push(`<path d="${rrect(x, 308, 104, 40, st.key === 'C' ? 3 : 12)}" fill="${st.panel}" stroke="${st.rar[r]}" stroke-width="${st.key === 'C' ? 5 : 4}"/>` + gems(st, r, x + 14, 328, 6) + txt(r, x + 52, 343, { font: st.fonts.body, size: 11, fill: st.text, anchor: 'middle' }));
  });
  // buttons
  o.push(txt('BOTÕES', 32, 378, { font: st.fonts.body, size: 12, weight: 800, fill: st.textDim, ls: 1 }));
  o.push(button(st, defs, 32, 392, 168, 46, 'Abrir o café', 'primary'), button(st, defs, 216, 392, 126, 46, 'Voltar', 'secondary'), button(st, defs, 358, 392, 126, 46, 'Indisponível', 'disabled'));
  // type
  o.push(txt('TIPOGRAFIA', 32, 470, { font: st.fonts.body, size: 12, weight: 800, fill: st.textDim, ls: 1 }));
  o.push(txt('Título da tela', 32, 506, { font: st.fonts.title, size: st.key === 'C' ? 26 : 32, fill: st.text, ls: st.key === 'C' ? 2 : 0.5 }), txt('Corpo: Kuudere nunca erra um pedido, mas anda 20% mais devagar.', 32, 534, { font: st.fonts.body, size: 14, weight: 600, fill: st.text }), txt('Rótulo pequeno 11 px · 1.240 · 72%', 32, 556, { font: st.fonts.body, size: 11, weight: 600, fill: st.textDim }));
  // measured contrast
  const pairs = [['texto / painel', st.text, st.panel], ['texto fraco / painel', st.textDim, st.panel], ['texto / fundo', st.text, st.bg], ['botão destaque', st.focusText, st.focus], ['alarme / painel', st.alarm, st.panel]];
  o.push(panel(st, defs, 520, 108, 408, 232, 'CONTRASTE MEDIDO (WCAG)'));
  pairs.forEach(([n, a, b], i) => {
    const r = contrast(a, b), y = 160 + i * 34, pass = r >= 4.5;
    o.push(`<path d="${rrect(540, y - 18, 40, 24, 4)}" fill="${b}" stroke="${mix(b, '#888', 0.5)}"/>` + txt('Aa', 560, y, { font: st.fonts.title, size: 16, fill: a, anchor: 'middle' }));
    o.push(txt(n, 596, y, { font: st.fonts.body, size: 13.5, weight: 600, fill: st.text }), txt(`${r.toFixed(1)}:1`, 820, y, { font: st.fonts.title, size: 17, fill: st.text, anchor: 'end' }), txt(pass ? 'AA ok' : r >= 3 ? 'só texto grande' : 'abaixo de 3', 912, y, { font: st.fonts.body, size: 12, weight: 800, fill: pass ? (st.key === 'B' ? st.good : '#2F8A62') : r >= 3 ? '#B8741C' : st.alarm, anchor: 'end' }));
  });
  // chips
  o.push(txt('ARQUÉTIPOS (cores de Palette.cs mantidas)', 520, 372, { font: st.fonts.body, size: 12, weight: 800, fill: st.textDim, ls: 1 }));
  Object.keys(ACCENT).forEach((k, i) => {
    const x = 520 + (i % 3) * 138, y = 386 + Math.floor(i / 3) * 34, a = ACCENT[k], w = 126;
    o.push(`<path d="${rrect(x, y, w, 26, st.key === 'C' ? 3 : 13)}" fill="${a}" ${st.key === 'C' ? `stroke="${st.ink}" stroke-width="2.4"` : ''}/>` + txt(ARCH_LABEL[k], x + w / 2, y + 18, { font: st.fonts.body, size: 13, fill: onColour(a, st), anchor: 'middle' }));
  });
  o.push(txt('Mockup exploratório — não é captura do Unity', 32, 588, { font: st.fonts.body, size: 10.5, weight: 600, fill: st.textDim }));
  return { W, H, body: o.join('') };
}

export { adj };
