# 09_MAID_CARD_DETAIL — prompt corrigido v02 (sem dados pintados)

**Próxima geração: \`UI-009_maid_card_detail_v02.png\` — PENDING e opcional.** \`UI-009_maid_card_detail_v01.png\` é **REWORK** no QA da PR #47: barras, sinais \`+\` e moedas foram pintados na arte onde a Unity precisa renderizar dados dinâmicos. A decisão de produto já aceita o **UI-008 existente em 150×206** como apresentação detalhada; este UI-009 é uma proposta de **frame expandido** adicional, não pré-requisito para o LINEUP MINIMAL.

## 1. Anexos exatos a enviar ao GPT Web

**Obrigatórios:**

1. **\`UI-008_maid_card_compact_v01.png\`**, fonte visual limpa local \`art_working/ui_modular/blender_clean/UI-008_maid_card_compact_v01.png\` (já gerada; uso detalhado 150×206 aceito pelo responsável, registro/versionamento em PR #46). **Use para manter a família de moldura, não para copiar a ficha em escala menor.**
2. **\`UI-001_panel_large_v01.png\`**, [PNG aprovado](../approved/UI-001_panel_large_v01.png), \`docs/art/ui-modular/approved/UI-001_panel_large_v01.png\`. Material/paleta aprovados.
3. **\`after_EL-2_elenco_960x600.png\`**, [captura real](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/roadmap/ui-01a/evidence/after_EL-2_elenco_960x600.png), \`docs/roadmap/ui-01a/evidence/after_EL-2_elenco_960x600.png\`. Espaço/layout real, sem copiar a tela.

**Opcional:** \`suzu_walk.png\`, [sprite sheet real](https://github.com/NThuesen/jogos_digitais_2D/blob/main/Assets/_Game/Resources/Maids/suzu_walk.png), \`Assets/_Game/Resources/Maids/suzu_walk.png\`, apenas para confirmar estilo da personagem; **não desenhe Suzu dentro do frame**.

**Não anexe** \`UI-009_maid_card_detail_v01.png\` como referência principal; ele contém exatamente as barras, moedas e sinais que precisamos eliminar. Se o anexar para explicar o defeito, identifique-o explicitamente como **NEGATIVE REFERENCE / REWORK**, nunca como master de composição.

## 2. Prompt completo — copie somente este bloco

\`\`\`text
PROJECT: Neko Maid Inc. Create exactly ONE original expanded MAID DETAIL CARD FRAME, version v02, for a cozy illustrated 2D anime café / ornamental visual-novel management game.

REFERENCE HIERARCHY:
1. ATTACHED UI-008_maid_card_compact_v01.png: existing detailed card's frame family, border language, warm rosewood / ivory / restrained gold. Use as style/sibling reference ONLY, not a character portrait.
2. ATTACHED UI-001_panel_large_v01.png: approved material and text-safe cream surface.
3. ATTACHED after_EL-2_elenco_960x600.png: real Unity Roster/Elenco context, NEVER trace the screen or its text.
Optional suzu_walk.png is identity/scale guidance ONLY; do not embed a maid.
The previous UI-009 v01 failed QA because it had STAT BARS, PLUS SIGNS and COIN/SALARY SYMBOLS PAINTED inside the frame. These must be COMPLETELY ABSENT in v02, not simply faded or hidden.

OUTPUT: ONE empty, independent, LANDSCAPE detail frame (roughly 3:2 visible aspect ratio), front-facing and well-contained on its transparent canvas, not a completed card sheet. More spacious than the existing compact 150 x 206 card and designed for variable content supplied entirely by Unity.

STRUCTURE:
- Left side: generous vertically oriented TRUE TRANSPARENT portrait aperture (~40-45% of frame width), with continuous, readable decorative border and margin for a real maid sprite placed BEHIND the frame by Unity. Keep the entire portrait aperture truly transparent, not cream.
- Right upper side: large PLAIN, OPAQUE cream safe zone for a dynamic character name and role/rarity added separately.
- Right middle: spacious continuous PLAIN cream region suitable for variable stats, traits, salary and other information dynamically rendered by Unity. Use only subtle border framing around the region; avoid subrows, stat grooves, progress-track shapes, inset pills, dashes, and decorative dividers that imply fixed numbers or fixed fields.
- Lower band: unadorned, wide OPAQUE cream safe zone where Unity can position its own buttons/controls. Do not paint buttons, capsules, chips or icons. Keep ornaments around the periphery, not beneath labels.

EXTREMELY IMPORTANT: The artwork must be an EMPTY DECORATIVE FRAME. Absolutely NO baked progress bars, stacked horizontal rounded bars, values, numeric placeholders, small + signs, coins, currency icons, trait icons, salary icons, rarity marks, words, letters, numbers, badges or miniature UI controls, even if they appear decorative or blank. Unity must remain free to populate, omit, reorder and localize every field. Areas for dynamic text must look like calm open cream paper, not a filled-out stats panel.

ART STYLE: polished cozy illustrated anime café / visual novel. Restrained rosewood and warm cream with tasteful muted brass, minimal paw/ribbon flourishes in outer border. Match the approved family's softness and line weight; avoid ornate clutter, saturated arcade gold, pixel art, fake 3D perspective and visual noise. Smooth symmetric framing and enough transparent margin so nothing gets cut off.

TECHNICAL: one isolated PNG with real alpha exterior and genuinely transparent portrait cutout. Opaque warm-cream information safe zones. Full graphic visible; no clipping, fake checkerboard, watermark or external scene background. Image is a static modular sprite, not the final UI. Unity places portrait, labels, bars, numbers, icons, buttons, hitboxes, selection, hover, localization and data.

STRICT OUTPUT: exactly ONE EMPTY expanded frame, no maid, no labels, no buttons, no bars, no fake game screenshot, no variants or contact sheet. Intended filename: UI-009_maid_card_detail_v02.png.
\`\`\`

## 3. Nome e critérios de aceite

- RAW novo: \`art_working/ui_modular/raw_gpt/UI-009_maid_card_detail_v02.png\`.
- CLEAN: \`art_working/ui_modular/blender_clean/UI-009_maid_card_detail_v02.png\`.
- APPROVED somente com autorização: \`art_working/ui_modular/approved/UI-009_maid_card_detail_v02.png\`.
- \`UI-009_maid_card_detail_v01.png\` permanece histórico REWORK; \`UI-008_maid_card_compact_v01.png\` não é substituído.

QA: verificar **ausência de qualquer dado/elemento de UI pintado**, transparência real do portrait, áreas creme para nomes longos, fidelidade 960×600/1280×720 e interação/números da Unity (quando usados). O novo UI-009 continua opcional.

[Tutorial](../TUTORIAL.md) · [histórico PR #47](https://github.com/NThuesen/jogos_digitais_2D/pull/47).
