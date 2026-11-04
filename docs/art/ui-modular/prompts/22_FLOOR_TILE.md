# 22_FLOOR_TILE — prompt corrigido v02 (seamless real)

**Próxima geração: \`UI-022_floor_tile_warm_oak_v02.png\` — PENDING.** \`UI-022_floor_tile_warm_oak_v01.png\` foi REWORK na PR #47 por falhar no mosaico 3×3: descontinuidade de bordas, principalmente vertical (Y). **Não use o v01 na Unity como material Repeat aprovado.**

## 1. Anexos exatos para o GPT Web

**Obrigatórios:**

1. **\`UI-022_floor_tile_warm_oak_v01.png\`**, \`art_working/ui_modular/raw_gpt/UI-022_floor_tile_warm_oak_v01.png\` (**RAW local existente; REWORK**). **Somente** para a tonalidade da madeira; a distribuição de tábuas e as bordas NÃO são referência válida.
2. **\`after_SL-1_salao_960x600.png\`**, [captura real](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/roadmap/ui-01b/evidence/after_SL-1_salao_960x600.png), \`docs/roadmap/ui-01b/evidence/after_SL-1_salao_960x600.png\`. Proporção de chão/câmera/cenário reais.
3. **\`style_anchor_reference.png\`**, [referência histórica](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/art-01/D-pixel-cafe/reference/style_anchor_reference.png), \`docs/art/art-01/D-pixel-cafe/reference/style_anchor_reference.png\`. **Apenas** aconchego/cores; sem pixel art.

**Opcional:** \`UI-023_wall_straight_v01.png\`, \`art_working/ui_modular/blender_clean/UI-023_wall_straight_v01.png\` (**CLEAN local, candidato ainda não aprovado; contexto de material, NÃO padrão de tile**). Não confundir o screenshot do salão com a textura real a ser repetida.

## 2. Prompt completo — copie somente este bloco

\`\`\`text
PROJECT: Neko Maid Inc. Create exactly ONE new original seamless WARM OAK FLOOR TILE texture, v02, for a 2D URP orthographic cozy anime café game.

CRITICAL REWORK: A previous square floor texture v01 appeared attractive in isolation but FAILED a 3x3 seamless-repeat test. Its plank directions and color patterns visibly break at opposite edges, especially TOP-to-BOTTOM. V02 must be a true 2D PERIODIC / TILEABLE texture, NOT just a square illustration that happens to contain floorboards.

ATTACHMENT HIERARCHY:
- UI-022_floor_tile_warm_oak_v01.png is a NEGATIVE REFERENCE for the tile seams and plank layout. Use it ONLY for the desired warm oak material / gentle amber hue. Do NOT reuse its four edge patterns, large plank intersections or non-periodic shading.
- after_SL-1_salao_960x600.png illustrates the game's actual room/grid/camera scale; do NOT bake its furniture, characters, rays, perspective or UI into this image.
- style_anchor_reference.png supplies cozy mood, NOT historical pixel art. Optional UI-023_wall_straight_v01.png may inform material harmony only, not floor layout.

SEAMLESS IS A HARD REQUIREMENT:
The LEFT edge must match the RIGHT edge and the TOP edge must match the BOTTOM edge in line placement, texture grain, lightness and color, continuously, so identical copies can be placed in a 3x3 or larger grid without visible lines or jumps. Treat both axes as periodically wrapping into each other. Do not place a hard horizontal grout/plank break exactly on the canvas boundary unless that break continues identically at the opposite boundary. A grain streak leaving the right side must continue from the left at exactly the same vertical position; likewise any grain at the bottom must continue at the top. Do not add a special border or darker ring around the tile. Keep all edges as ordinary interior material rather than frame edges.

DESIGN: perfectly square front-facing diffuse oak floor surface with a very low-contrast subtle wood grain and restrained hand-painted variation. Prefer calm, broadly uniform, warm wood color with fine continuous grain over a decorative pattern of large chessboard parquet blocks; avoid strong alternating vertical/horizontal plank regions, hard repeating squares, deep seams, large knots or high-contrast geometric joins. The source v01's obvious change of grain direction at internal squares should not be replicated. If planks are visible, keep them small, subtly aligned and PERIODIC across both edges. No cast lighting gradient; Unity will handle room lighting/placement.

TECHNICAL: one FULLY OPAQUE, perfectly square, flat 2D texture. No perspective, borders, transparent corners, alpha cutouts, floor grid marks, arrows or vignette. No fake checkerboard (not even for transparency), photographic stock texture, artificial pixel quantization, furniture, placeable objects, baked shadows, characters, labels, logos or screenshots. Preserve soft illustrated anime café style.

VALIDATION TARGET: the SAME PNG tiled three times horizontally and vertically should form one calm uninterrupted oak field, with NO detectable seams. Prioritize exact periodic border continuity over decorative complexity. Note: a visual generation is a candidate, not a mathematical guarantee; the PNG must still be checked with a pixel/3x3 repeat audit before Unity integration.

STRICT OUTPUT: EXACTLY ONE isolated seamless square tile, no example grid, no mockup, no contact sheet, no watermark. Intended filename UI-022_floor_tile_warm_oak_v02.png.
\`\`\`

## 3. Saída e gate

- RAW novo: \`art_working/ui_modular/raw_gpt/UI-022_floor_tile_warm_oak_v02.png\`.
- CLEAN se necessário: \`art_working/ui_modular/blender_clean/UI-022_floor_tile_warm_oak_v02.png\`.
- APPROVED somente após revisão: \`art_working/ui_modular/approved/UI-022_floor_tile_warm_oak_v02.png\`.
- Guardar \`UI-022_floor_tile_warm_oak_v01.png\` como REWORK histórico.

**QA mínimo inegociável:** validar bordas opostas e mosaico repetido 3×3 em ambos os eixos ANTES de gastar uma build WebGL. Se ainda falhar, manter REWORK; considerar síntese periodicamente corrigida por ferramenta gráfica apenas após preservar o original e registrar a transformação.

[Tutorial](../TUTORIAL.md) · [QA original PR #47](https://github.com/NThuesen/jogos_digitais_2D/pull/47).
