# ART-01D — Prompts para o ChatGPT Plus

Contexto: [README.md](README.md). Os prompts estão em inglês porque o modelo de imagem responde melhor; as instruções para você estão em português.

## Como usar (sem nenhum trabalho de edição)

1. Abra **um chat novo** no ChatGPT. Anexe `reference/style_anchor_reference.png` (a imagem de referência) e escreva: *"Use the attached image only as a style reference. Do not copy its characters or layout."*
2. Rode os prompts **na ordem**, **uma imagem por mensagem**, **no mesmo chat**, para o modelo manter a identidade. Do Prompt 1 em diante, anexe também o resultado do Prompt 0 (character sheet), se o chat não o tiver mais no contexto.
3. Se o resultado não ficar bom, peça só o ajuste ("same image, but eyes bigger…"). Não precisa me avisar a cada tentativa.
4. Salve **o arquivo original, sem recortar nem editar**, em `docs/art/art-01/D-pixel-cafe/incoming/` com o nome indicado (`D0_...png`). Pode salvar mais de uma tentativa: `D1_portrait_v2.png`, etc.
5. Quando terminar (ou cansar), me diga "imagens em incoming". Eu processo, avalio e mostro o resultado.

Se algum prompt falhar repetidamente, pare e me avise: isso já é dado de avaliação, não é culpa sua.

Ordem recomendada e prioridade: **0 → 1 → 2 → 3 → 4** são essenciais; **5** é opcional (só mede a "sensação" final).

---

## Bloco de fundo — transparência é um EXPERIMENTO

Não há solução definitiva. Cada prompt abaixo termina com `[FUNDO]`; substitua por uma das variantes:

- **Variante T (tentar primeiro):** `Background: truly transparent (real alpha channel PNG), no checkerboard pattern drawn in the image, no shadows or glow on the background.`
- **Variante K (alternativa/chroma-key):** `Background: flat solid pure magenta #FF00FF, nothing else, no shadows on the background. Do not use magenta or strong pink on the subject.`

Teste inicial sugerido, só no **Prompt 1**: gere uma imagem com T e outra com K e salve ambas (`D1_portrait_T.png`, `D1_portrait_K.png`). Se o ChatGPT desenhar um xadrez falso ou entregar o PNG sem alfa, é resultado válido: registre salvando assim mesmo. Eu comparo halos, bordas e recorte e só depois fixamos o método para os demais. Nos prompts seguintes use a variante que funcionou melhor — ou as duas, se estiver em dúvida.

---

## Bloco de estilo (já incluído em cada prompt; não precisa colar à parte)

> Cozy pixel-art anime café game art. True pixel-art look: chunky visible pixels, hard edges, NO anti-aliasing, NO blur, NO smooth gradients (use dithering for gradients), limited palette of about 24–32 colors. Warm lighting, hue-shifted shading (shadows lean cool purple-brown, never pure black), colored outlines (dark brown or darker hue of the fill, not black). Rich, charming, collectible visual-novel quality. Original design; do not copy any existing game or character.

---

> **Identidade da Suzu = hipótese experimental.** Cabelo menta, olhos âmbar, sininho etc. são ponto de partida para **o responsável avaliar**; não são características aprovadas. Se o resultado não agradar, mudamos o desenho e regeneramos. Os números de cor (`#hex`), "~28 cores" e tamanhos são parâmetros de teste, não especificação final.

## Prompt 0 — Character sheet da Suzu (âncora de identidade)
Arquivo: `D0_suzu_sheet.png` · formato 3:2 horizontal

```
Using the attached image only as a style reference (do not copy its characters), create an ORIGINAL character reference sheet for a cat-eared maid named Suzu, in the same cozy pixel-art anime style: chunky visible pixels, hard edges, no anti-aliasing, no blur, limited ~28-color palette, hue-shifted shading, colored (non-black) outlines.

Design (keep exactly):
- Soft mint-green bob hair (main #8FD8C0, shadow #4FA38F, highlight #D6F5E8) with straight bangs and two long side locks; two short tied tufts with a small ribbon holding a tiny golden bell.
- Large slightly droopy amber-gold eyes (#E8A93A) with two highlights each and thick upper lashes.
- Upright cat ears, cream-pink inside; the LEFT ear has a small notch at the tip (identity mark).
- Slim cream tail with mint tip and a small golden bell.
- Warm light skin tone.
- Chocolate-brown maid dress, cream frilled apron, white collar, burgundy bow, golden bell on a thin choker.
- Calm proud gentle smile.

Layout: with no text, the following elements spaced apart on the same plain background: (1) large bust portrait front, (2) full body front, (3) full body three-quarter view, (4) three small expression busts (happy, flustered, sleepy) on the bottom row. A strip of 8 color swatches of the palette at the bottom.
[FUNDO]
```

## Prompt 1 — Portrait da Suzu (para o card)
Arquivo: `D1_portrait.png` · 1024×1024 ou 2:3 vertical

```
Same character as the attached Suzu sheet, same pixel-art style as the style reference. Bust portrait (head to waist), slightly angled, looking at the viewer, calm proud smile, holding nothing. Expressive anime face, large amber eyes with two highlights, mint bob hair with rim light on the hair edge, bangs in individual strands, cat ears with the notched left ear, golden bell choker, chocolate maid dress with cream frilled apron and burgundy bow.
Chunky visible pixels, hard edges, no anti-aliasing, no blur, ~28 colors, dithered gradients, hue-shifted shading, colored outlines, collectible-card charm.
No frame, no text. Character fills most of the frame, with a small margin.
[FUNDO]
```

*Opcional, mesma tela:* "Same portrait with a flustered expression (spiral eyes, open mouth)" → `D1_portrait_flustered.png` (testa a consistência entre expressões).

## Prompt 2 — Chibi da Suzu (restaurante)
Arquivos: `D2_chibi_idle.png`, `D2_chibi_walk.png`, `D2_chibi_carry.png` · 1024×1024 cada

```
Same character as the attached Suzu sheet (identical colors, hair, ears with the notched left ear, bell tail, maid outfit), now as a CHIBI game sprite, same pixel-art style as the style reference. Full body, 2–2.5 heads tall, simple readable silhouette, big head with the same mint bob and ribbon-bell, tiny body, cream tail with the bell, slightly bigger-than-life eyes with highlights.
Pose: [IDLE: standing, hands together in front of the apron, tiny smile] / [WALK: mid-step walking to the right, arms swinging] / [CARRY: walking to the right, carrying a small round tray with a teacup and a slice of cake with both hands].
Side/three-quarter facing right. Chunky visible pixels, hard edges, no anti-aliasing, no blur, ~24 colors, colored outlines, no soft shadows.
No text, no ground shadow. Sprite fills the frame with margin.
[FUNDO]
```
(Envie três mensagens, trocando só o trecho de pose entre colchetes.)

## Prompt 3 — Interior do café
Arquivo: `D3_cafe_interior.png` · 3:2 horizontal

```
Using the attached image only as a style reference (do not copy its layout, characters or props), create an ORIGINAL cozy cafe interior in pixel-art anime style, empty of characters, wide view in three-quarter perspective with clear depth in 3–4 planes: a wooden counter with shelves of tea jars and a small coffee machine at the back, 3–4 round wooden tables with chairs in the middle, a bay window with a warm diagonal sunbeam falling on the floor, tall ceiling with wooden beams, pendant yellow lamps lit, hanging and potted plants, framed pictures, a bookshelf with plush toys, a cat-shaped chalk menu board, a checkerboard wooden floor, a basket of fruit on one table, a teapot and cups. Warm honey/cream/chocolate palette with sage-green plants and small teal and rose accents.
Chunky visible pixels, hard edges, no anti-aliasing, no blur, ~32 colors, dithered gradients, hue-shifted shading (shadows cool purple-brown, never black), colored outlines. Rich cozy detail, props clearly separated and readable. No text, no UI, no frame, no people, no animals. Leave the center-floor between the counter and the tables fairly clear (a walking area for staff).
```

*Opcional (modularidade):* depois do café, peça "the same cafe's props as a sheet on a plain background ([FUNDO] variant T or K): 6 tables/chairs variants, 4 plants, 3 hanging lamps, counter, cake display, each separated by clear space" → `D3_cafe_props_sheet.png`. Isso mede se props separados saem consistentes.

## Prompt 4 — Kit de UI ornamental
Arquivo: `D4_ui_kit.png` · 3:2 horizontal

```
Using the attached image only as a style reference (do not copy its exact frames, icons or text), create an ORIGINAL ornamental visual-novel UI kit sheet for a cat-maid café game, in the same pixel-art style. Palette: terracotta/rosewood frames, cream inner panels, gold highlights, warm brown shadows; a subtle paw-print pattern and an argyle diamond pattern as background textures.
Include, laid out with clear spacing and NO readable text/letters/numbers:
1) three large rectangular ornamental frames with notched corners and a nested double border (empty cream interior), one tall (for a character portrait), one wide (for a scene), one small;
2) a cream parchment speech/dialogue balloon with an angular outline and a small down-arrow;
3) a row of 6 small icon buttons in beveled boxes: a recipe book, a cat paw, a cup, a clock, a gear, a star (pixel-art icons);
4) a long horizontal panel with two ON/OFF toggle switches (no text);
5) a rarity-frame set of four small card-corner ornaments in escalating richness: plain brown, mint, gold, and a sparkly rainbow-tinted gold;
6) two pill-shaped buttons: normal and pressed state.
Chunky visible pixels, hard edges, no anti-aliasing, ~28 colors.
[FUNDO]
```

## Prompt 5 (opcional) — Conceito da tela de Escalação
Arquivo: `D5_lineup_concept.png` · 16:10 horizontal · **só conceito emocional, será descartado como asset**

```
Using the attached Suzu sheet, the attached cafe interior and the attached UI kit as references, compose a full-screen concept of a "Lineup" management screen for a pixel-art cat-maid café game, 16:10: left, a tall ornate frame with the Suzu bust portrait; right, a grid of 4 empty ornate card slots labelled with small pixel icons (no readable text) and one slot showing her chibi; background, a softly darkened version of the cafe interior. Keep all pixel-art rules (chunky pixels, hard edges, no blur, ~32 colors). This is concept art only.
```

---

## O que eu vou fazer com os arquivos (para você saber o que esperar)

| Arquivo | Processamento previsto | Risco já esperado |
| --- | --- | --- |
| `D0` | extrair paleta; checar identidade | cores diferentes das hex pedidas |
| `D1`, `D2_*` | checar alfa real; comparar alfa vs. chroma-key (halo, bordas); **testar** métodos de redução e paletas, sem presumir que reduzir funciona | xadrez falso, franja de cor, pixels de tamanhos mistos, olhos/orelhas mudando entre poses |
| `D3` | recorte 16:10, testes de escala/paleta, medir pixel grid; separar camadas **só se** viável | perspectiva inconsistente, texto inventado em quadros |
| `D4` | recorte de cada peça, teste de 9-slice | molduras assimétricas (9-slice pode não funcionar) |
| `D5` | só comparação "sensação" com a referência | — |

**Saída final para sua aprovação:** quadro com ARTE GERADA, PROCESSADO e MOCKUP identificados + lista de defeitos reais. Nada disso é captura Unity.
