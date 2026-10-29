// The three visual directions and the shared maid roster used by every mockup.
// Archetype accents are copied from Palette.cs (GDD 2.6); rarity hues follow GDD 2.8
// (bege / azul / violeta / dourado). Everything else is a proposal, not a decision.

export const ACCENT = {
  Ojousama: '#A42F79', Idol: '#9940B5', Oneesan: '#636AC5', Tsundere: '#D34F4F', Mukuchi: '#32957D',
  Dojikko: '#4DA33F', Genki: '#D68946', Kuudere: '#6BAED0', Gourmand: '#C9B04A',
};
export const ARCH_LABEL = {
  Ojousama: 'Ojou-sama', Idol: 'Idol', Oneesan: 'Onee-san', Tsundere: 'Tsundere', Mukuchi: 'Mukuchi',
  Dojikko: 'Dojikko', Genki: 'Genki', Kuudere: 'Kuudere', Gourmand: 'Gourmand',
};
export const RARITY = ['Comum', 'Rara', 'Épica', 'Lendária'];
export const SALARY = { Comum: 18, Rara: 44, Épica: 95, Lendária: 210 };

/** Original characters. `Suzu` is the single shared brief used for every proposal. */
export const MAIDS = {
  suzu: { id: 'suzu', name: 'Suzu', arch: 'Genki', hairStyle: 'twin', hair: '#D9792B', eye: '#2FA39A', rarity: 'Rara', pos: 'Salão', stats: [32, 14, 24, 18] },
  hana: { id: 'hana', name: 'Hana', arch: 'Ojousama', hairStyle: 'tiara', hair: '#E6C26A', eye: '#8E63C8', rarity: 'Lendária', pos: 'Recepção', stats: [16, 12, 38, 18] },
  kuro: { id: 'kuro', name: 'Kuro', arch: 'Kuudere', hairStyle: 'bob', hair: '#8FB0D4', eye: '#3D5A99', rarity: 'Épica', pos: 'Cozinha', stats: [18, 32, 14, 26] },
  lumi: { id: 'lumi', name: 'Lumi', arch: 'Idol', hairStyle: 'long', hair: '#B069D6', eye: '#F08CBE', rarity: 'Épica', pos: 'Palco', stats: [20, 12, 36, 20], clip: 'star' },
  pip: { id: 'pip', name: 'Pip', arch: 'Dojikko', hairStyle: 'long', hair: '#8A5A3C', eye: '#4DA33F', rarity: 'Comum', pos: 'Recepção', stats: [24, 18, 30, 16], clip: 'none' },
  mochi: { id: 'mochi', name: 'Mochi', arch: 'Gourmand', hairStyle: 'bob', hair: '#C98F4A', eye: '#B8602F', rarity: 'Comum', pos: 'Cozinha', stats: [14, 36, 18, 22] },
  nori: { id: 'nori', name: 'Nori', arch: 'Mukuchi', hairStyle: 'bob', hair: '#2E7D74', eye: '#E8C84A', rarity: 'Rara', pos: 'Cozinha', stats: [12, 34, 20, 24] },
};

export const STYLES = {
  A: {
    key: 'A', dir: 'A-cafe-pastel', name: 'Café Pastel',
    tagline: 'Creme, rosa, menta e mel; madeira só como apoio',
    // character rendering
    line: '#8E5F63', lineW: 2.6, shade: 'soft', shadeTint: '#E08AA6', shadeAmt: 0.36, shadeOff: 9, misreg: 0,
    eyeStyle: 'glossy', chin: 0, skin: '#FFE6D6', blush: '#FF9DB0', mouth: '#C8505F',
    uniform: '#F7B6C8', collar: '#FFFFFF', apron: '#FFF8EE', bow: '#8FD8C2', bell: '#FFC857',
    hairAdj: { s: 1.0, l: 0.09 }, chibiHead: 1.0, inner: '#FFB3C1',
    // UI
    bg: '#FFF3E6', bg2: '#FFE3EA', panel: '#FFFDF8', panelAlt: '#FFEFD9', mint: '#CDEFE0', pink: '#FFD3DF',
    text: '#5A3E43', textDim: '#7A5F63', focus: '#FFC857', focusText: '#5A3E43', good: '#58B88C', alarm: '#EE5C73',
    wood: '#D9B38C', woodDark: '#B98E64', ink: '#8E5F63',
    rar: { Comum: '#D8C3A5', Rara: '#7BB6F2', Épica: '#B48AE8', Lendária: '#F5B93A' },
    fonts: { title: 'Fredoka', body: 'Nunito' },
    radius: 18,
  },
  B: {
    key: 'B', dir: 'B-quadro-negro', name: 'Quadro-Negro Doce',
    tagline: 'Café noturno: lousa escura, giz creme e cores de doce que saltam',
    line: '#2B1F3D', lineW: 3.4, shade: 'cel', shadeTint: '#3B2A78', shadeAmt: 0.42, shadeOff: 11, misreg: 0,
    eyeStyle: 'spark', chin: 1, skin: '#FFD9C2', blush: '#FF7A9A', mouth: '#B8334F',
    uniform: '#3A4FA3', collar: '#F5EEDD', apron: '#F5EEDD', bow: '#FF7A8A', bell: '#FFD66B',
    hairAdj: { s: 1.15, l: 0.0 }, chibiHead: 0.9, inner: '#FF8FA8',
    bg: '#1E2A2F', bg2: '#16201F', panel: '#2A383D', panelAlt: '#32454B', mint: '#7CE0C3', pink: '#FF7A8A',
    text: '#F5EEDD', textDim: '#A9B8B6', focus: '#FFD66B', focusText: '#2A2118', good: '#7CE0C3', alarm: '#FF6B81',
    wood: '#8B5E3C', woodDark: '#5E3E27', ink: '#F5EEDD',
    rar: { Comum: '#C9BFA8', Rara: '#6FB1FF', Épica: '#B98CFF', Lendária: '#FFD04A' },
    fonts: { title: 'Patrick Hand', body: 'Nunito' },
    radius: 12,
  },
  C: {
    key: 'C', dir: 'C-kissaten-showa', name: 'Kissaten Showa',
    tagline: 'Letreiro e impresso retrô: tinta chapada, contorno grosso, halftone',
    line: '#2B2330', lineW: 4.6, shade: 'halftone', shadeTint: '#8A2E2A', shadeAmt: 0.5, shadeOff: 11, misreg: 3.2,
    eyeStyle: 'dot', chin: 0, skin: '#FBD9BC', blush: '#EE6A5E', mouth: '#9C2B2B',
    uniform: '#1F7A74', collar: '#F6EBD3', apron: '#F6EBD3', bow: '#E5483B', bell: '#E8B03A',
    hairAdj: { s: 1.05, l: 0.0 }, chibiHead: 1.1, inner: '#F2A5A0',
    bg: '#F6EBD3', bg2: '#EAD9B5', panel: '#FFF8E6', panelAlt: '#F9E3B8', mint: '#7FC6B8', pink: '#F2A5A0',
    text: '#2B2330', textDim: '#6C5B5A', focus: '#E8B03A', focusText: '#2B2330', good: '#1F7A74', alarm: '#E5483B',
    wood: '#B97B45', woodDark: '#7E4F2A', ink: '#2B2330',
    rar: { Comum: '#CDB98E', Rara: '#2F7DB8', Épica: '#7A4FA8', Lendária: '#E8B03A' },
    fonts: { title: 'Dela Gothic One', body: 'Rubik' },
    radius: 6,
  },
};
