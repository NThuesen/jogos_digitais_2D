import { STYLES, MAIDS } from './styles.mjs';
import { contrast, adj } from './lib.mjs';
const r = (a, b) => contrast(a, b).toFixed(2);
for (const k of 'ABC') {
  const st = STYLES[k];
  const hair = adj(MAIDS.suzu.hair, st.hairAdj);
  const bgs = k === 'A' ? { 'parede': '#E8F8F1', 'piso': '#F0D9BC' } : k === 'B' ? { 'parede': st.bg, 'piso': '#3A2A21' } : { 'parede': st.alarm, 'piso xadrez creme': st.panel, 'piso xadrez verde': st.uniform };
  console.log(`\n${k} ${st.name}`);
  console.log(' texto/painel', r(st.text, st.panel), '| texto fraco/painel', r(st.textDim, st.panel), '| texto/fundo', r(st.text, st.bg), '| botão destaque', r(st.focusText, st.focus), '| alarme/painel', r(st.alarm, st.panel));
  console.log(' cabelo Suzu', hair, Object.entries(bgs).map(([n, c]) => `${n} ${r(hair, c)}`).join(' | '));
  console.log(' raridades vs painel:', Object.entries(st.rar).map(([n, c]) => `${n} ${r(c, st.panel)}`).join(' | '));
  console.log(' destaque/painel', r(st.focus, st.panel), '| destaque/fundo', r(st.focus, st.bg));
}
