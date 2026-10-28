import fs from 'node:fs';
import { toPng } from './render.mjs';
import { STYLES, MAIDS, RARITY } from './styles.mjs';
import { portrait, chibi } from './art.mjs';
import { cardFull, lineupScreen, restaurantStrip, styleTile, txt } from './ui.mjs';
import { svgDoc, resetIds, rrect, mix } from './lib.mjs';

const OUT = process.argv[2];
const sheets = {};

for (const k of ['A', 'B', 'C']) {
  const st = STYLES[k], dir = `${OUT}/${st.dir}`;
  const suzu = MAIDS.suzu;

  // --- identity sheet: portrait + chibi poses at several scales
  {
    resetIds(); const defs = [], o = [];
    const W = 1280, H = 640;
    o.push(`<rect width="${W}" height="${H}" fill="${st.bg}"/>`);
    o.push(txt(`${st.key} · ${st.name} — Suzu (Genki)`, 28, 46, { font: st.fonts.title, size: 30, fill: st.text }));
    o.push(txt('Mesmo brief: gêmeas altas, orelhas e cauda de gato ruivas, olhos verde-água, sino dourado no pescoço e nos laços', 28, 72, { font: st.fonts.body, size: 14, weight: 600, fill: st.textDim }));
    o.push(`<g transform="translate(28 96)"><rect width="400" height="500" rx="${st.radius}" fill="${st.key === 'B' ? '#2A383D' : st.key === 'C' ? st.panelAlt : st.panel}" ${st.key === 'C' ? `stroke="${st.ink}" stroke-width="4"` : ''}/>${portrait(suzu, st, defs)}</g>`);
    o.push(txt('PORTRAIT (camada de arte do card, sem moldura/texto)', 28, 618, { font: st.fonts.body, size: 11.5, weight: 800, fill: st.textDim, ls: 0.8 }));
    o.push(txt('CHIBI · poses idle / andar / andar / carregar / cansada (zoom 1,5×)', 460, 112, { font: st.fonts.body, size: 11.5, weight: 800, fill: st.textDim, ls: 0.8 }));
    ['idle', 'walk', 'walk2', 'carry', 'tired'].forEach((p, i) => o.push(`<g transform="translate(${450 + i * 166} 126) scale(1.38)">${chibi(suzu, st, defs, p)}</g>`));
    o.push(txt('CHIBI no tamanho de jogo (≈0,62×, ≈ 90 px de altura) sobre o piso da direção', 460, 366, { font: st.fonts.body, size: 11.5, weight: 800, fill: st.textDim, ls: 0.8 }));
    o.push(`<rect x="460" y="378" width="780" height="130" rx="${st.radius}" fill="${st.key === 'A' ? '#F0D9BC' : st.key === 'B' ? '#3A2A21' : st.panelAlt}" ${st.key === 'C' ? `stroke="${st.ink}" stroke-width="3.5"` : ''}/>`);
    ['idle', 'walk', 'walk2', 'carry', 'tired'].forEach((p, i) => o.push(`<g transform="translate(${500 + i * 148} 398) scale(0.72)">${chibi(suzu, st, defs, p)}</g>`));
    o.push(txt('Peças de identidade compartilhadas (retrato ↔ chibi)', 460, 540, { font: st.fonts.body, size: 11.5, weight: 800, fill: st.textDim, ls: 0.8 }));
    ['cabelo + gêmeas altas', 'orelhas + cauda ruivas', 'olhos verde-água', 'sino dourado'].forEach((t, i) => {
      const x = 460 + i * 196;
      o.push(`<path d="${rrect(x, 552, 184, 30, st.key === 'C' ? 4 : 15)}" fill="${st.panel}" ${st.key === 'C' ? `stroke="${st.ink}" stroke-width="2.6"` : ''}/>` + txt(t, x + 92, 572, { font: st.fonts.body, size: 12.5, fill: st.text, anchor: 'middle' }));
    });
    toPng(svgDoc(W, H, defs, o.join('')), `${dir}/identidade-retrato-chibi.png`);
  }

  // --- card + rarity ladder
  {
    resetIds(); const defs = [], o = [];
    const W = 1280, H = 560;
    o.push(`<rect width="${W}" height="${H}" fill="${st.key === 'B' ? st.bg2 : st.bg}"/>`);
    o.push(txt(`${st.key} · ${st.name} — card composto`, 28, 44, { font: st.fonts.title, size: 28, fill: st.text }));
    o.push(txt('Retrato + moldura/nome/raridade/atributos feitos pela UI (texto nativo, fora da imagem)', 28, 68, { font: st.fonts.body, size: 13.5, weight: 600, fill: st.textDim }));
    o.push(`<g transform="translate(28 90) scale(1.15)">${cardFull(suzu, st, defs)}</g>`);
    o.push(txt('Escada de raridade (mesma maid, só a moldura muda; cor + nº de gemas)', 400, 112, { font: st.fonts.body, size: 12, weight: 800, fill: st.textDim, ls: 0.6 }));
    RARITY.forEach((r, i) => o.push(`<g transform="translate(${400 + i * 218} 126) scale(0.7)">${cardFull({ ...suzu, rarity: r }, st, defs)}</g>`));
    o.push(txt('Cards em 0,7× (≈196×280 px) para checar leitura de nome, arquétipo e raridade em tamanho menor', 400, 440, { font: st.fonts.body, size: 12.5, weight: 600, fill: st.textDim }));
    toPng(svgDoc(W, H, defs, o.join('')), `${dir}/card-e-raridade.png`);
  }

  // --- lineup, restaurant, tile
  for (const [name, fn] of [['escalacao-960x600', lineupScreen], ['restaurante-960x420', restaurantStrip], ['style-tile', styleTile]]) {
    resetIds(); const defs = [];
    const r = fn(st, defs);
    const svg = svgDoc(r.W, r.H, defs, r.body);
    toPng(svg, `${dir}/${name}.png`);
    sheets[`${k}:${name}`] = svg;
  }
}
console.log('done');

// --- comparison board (all three directions, same brief)
{
  resetIds(); const defs = [], o = [];
  const W = 1920, H = 1150;
  o.push(`<rect width="${W}" height="${H}" fill="#F3EEE8"/>`);
  o.push(txt('ART-01 — quadro comparativo (mesma maid, Suzu · Genki · Rara)', 30, 52, { font: 'Nunito', size: 34, fill: '#2A2228' }));
  o.push(txt('Mockups exploratórios vetoriais — não são capturas do Unity e não provam funcionamento em runtime. Nenhuma direção está aprovada.', 30, 82, { font: 'Nunito', size: 16, weight: 600, fill: '#6B5A5A' }));
  ['A', 'B', 'C'].forEach((k, i) => {
    const st = STYLES[k], x0 = 30 + i * 630;
    o.push(`<rect x="${x0 - 8}" y="104" width="616" height="1030" rx="14" fill="${st.bg}" stroke="#CFC6BC" stroke-width="1.5"/>`);
    o.push(txt(`${st.key} · ${st.name}`, x0 + 8, 142, { font: st.fonts.title, size: 30, fill: st.key === 'C' ? st.alarm : st.text, ls: st.key === 'C' ? 1 : 0.3 }));
    o.push(txt(st.tagline, x0 + 8, 166, { font: st.fonts.body, size: 13.5, weight: 600, fill: st.textDim }));
    o.push(`<g transform="translate(${x0 + 8} 184) scale(0.56)"><rect width="400" height="500" rx="${st.radius * 1.5}" fill="${st.key === 'B' ? '#2A383D' : st.key === 'C' ? st.panelAlt : st.panel}" ${st.key === 'C' ? `stroke="${st.ink}" stroke-width="6"` : ''}/>${portrait(MAIDS.suzu, st, defs)}</g>`);
    o.push(`<g transform="translate(${x0 + 246} 184) scale(0.7)">${cardFull(MAIDS.suzu, st, defs)}</g>`);
    ['idle', 'walk', 'carry'].forEach((p, j) => o.push(`<g transform="translate(${x0 + 450 + (j % 2) * 20} ${184 + j * 94}) scale(0.62)">${chibi(MAIDS.suzu, st, defs, p)}</g>`));
    const ln = lineupScreen(st, defs), rs = restaurantStrip(st, defs);
    o.push(`<g transform="translate(${x0 + 4} 480) scale(0.62)"><svg width="960" height="600" viewBox="0 0 960 600" overflow="hidden"></svg>${ln.body}</g>`);
    o.push(`<g transform="translate(${x0 + 4} 872) scale(0.62)">${rs.body}</g>`);
    o.push(txt('Escalação 960×600 (escala 0,62×)', x0 + 8, 474, { font: st.fonts.body, size: 12, weight: 800, fill: st.textDim, ls: 0.6 }));
    o.push(txt('Restaurante com chibis (escala 0,62×)', x0 + 8, 866, { font: st.fonts.body, size: 12, weight: 800, fill: st.textDim, ls: 0.6 }));
  });
  // clip each column's embedded screens so nothing bleeds
  toPng(svgDoc(W, H, defs, o.join('')), `${OUT}/quadro-comparativo.png`);
}
console.log('board');
