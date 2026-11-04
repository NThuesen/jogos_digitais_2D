# Botão secundário — HOVER v02 (REWORK do B4)

**Status:** prompt v02 pronto para copiar; imagem ainda não gerada. **UM prompt = UMA imagem.** O prompt v01 ([05_BUTTON_SECONDARY_HOVER.md](05_BUTTON_SECONDARY_HOVER.md)) e o RAW v01 ficam preservados; o v01 **não** deve ser sobrescrito.

**Por que v02:** o HOVER v01 veio como RGB opaco 2172×724 com matte branco e xadrez falso (253/235) assado, sem alpha, e com cor mais viva/rosada que o NORMAL (parecia outro botão). Ver [relatório B4](../../reviews/UI_BATCH_B4_2026_11_04.md).

## 1. Anexe estes arquivos (nesta ordem)

1. **Estilo:** [`style_anchor_reference.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/art-01/D-pixel-cafe/reference/style_anchor_reference.png) — `docs/art/art-01/D-pixel-cafe/reference/style_anchor_reference.png`
2. **Estilo:** [`D4_ui_kit.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/art-01/D-pixel-cafe/incoming/D4_ui_kit.png) — `docs/art/art-01/D-pixel-cafe/incoming/D4_ui_kit.png`
3. **GEOMETRIA E COR (principal):** `UI-005_button_secondary_normal_v01.png`
   - Local (use este): `art_working/ui_modular/blender_clean/UI-005_button_secondary_normal_v01.png`
   - Repo: [branch `qa/ui-modular-new-batch` (PR #47)](https://github.com/NThuesen/jogos_digitais_2D/blob/qa/ui-modular-new-batch/docs/art/ui-modular/incoming/BATCH-2026-11-04/UI-005_button_secondary_normal_v01.png) — `docs/art/ui-modular/incoming/BATCH-2026-11-04/UI-005_button_secondary_normal_v01.png` (ainda PENDING, não está na main).
4. **Família de estados (opcional, mesma silhueta):** [`UI-005_button_secondary_pressed_v01.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/qa/ui-modular-batch-b4/docs/art/ui-modular/incoming/BATCH-B4/UI-005_button_secondary_pressed_v01.png) — `docs/art/ui-modular/incoming/BATCH-B4/UI-005_button_secondary_pressed_v01.png` (branch da PR #52).

### NÃO anexe como referência positiva (REFERÊNCIA NEGATIVA)
- `raw_gpt/UI-005_button_secondary_hover_v01.png` (RAW v01 rejeitado: fundo branco/xadrez assado, cor rosada). Se quiser mostrá-lo, diga no chat que é o que **NÃO** repetir.

## 2. Prompt pronto para copiar

```text
Neko Maid Inc. Original MODULAR Unity sprite, cozy illustrated 2D anime cafe / ornamental visual novel UI. Attached images 1-2 are STYLE REFERENCES ONLY, NOT content to copy. Attached image 3 is the approved-candidate NORMAL state of this exact button: it is the GEOMETRY AND COLOR MASTER. Avoid pixel art/pixel grids, traced ornaments, and complete screenshots. No text, prices, labels, watermarks or character artwork.

Redraw the SAME secondary button from attached image 3 in its HOVER state. Keep EXACTLY: the same outline, the same rounded-rectangle silhouette, the same ornament pattern, the same horizontal proportion (about 4.1:1 visible, ~2100 x 508 visible pixels inside a 2172 x 724 canvas), the same position and the same chromatic family (taupe / rosewood frame with quiet cream middle). Change ONLY: the frame edge becomes slightly brighter (a few percent in lightness) and one small, soft warm highlight appears on the top edge. Do NOT make it pink, saturated, glowing or a different color; HOVER must read as the same button, only gently lit. Central area stays empty and calm for dynamic Unity text.

TRANSPARENCY IS MANDATORY: deliver a PNG with a REAL alpha channel (RGBA). Everything outside the button silhouette must be fully transparent. NO white background, NO matte, NO checkerboard (real or painted), NO drop shadow outside the silhouette, NO glow outside the silhouette, NO halo. The soft edge of the button must be anti-aliased alpha only. Keep the cream inside the button opaque.

Render ONE HOVER button only, not four states together, no text, no icon. Output precisely the single button asset.

ONE image only. Intended filename after download: UI-005_button_secondary_hover_v02.png (rename it yourself if the generator does not keep the name).
```

## 3. Salve com este nome EXATO e critérios de QA

- Bruto: `art_working/ui_modular/raw_gpt/UI-005_button_secondary_hover_v02.png` (**não** sobrescrever `..._hover_v01.png`).
- CLEAN (feito pelo QA, não pelo gerador): `art_working/ui_modular/blender_clean/UI-005_button_secondary_hover_v02.png`.
- Aprovação artística: somente pelo responsável.

**Critérios objetivos (QA incremental):**
1. PNG **RGBA com alpha real**: canto (0,0) com alpha 0; fração alpha=0 ≈ a do NORMAL; **sem** matte branco nem xadrez 253/235 assado; zero pixels ≥ 8 de alpha na borda do canvas.
2. Silhueta: IoU ≥ 0,99 contra o NORMAL; bbox visível ≈ 2099×507 (±2%); proporção ≈ 4,14.
3. Família cromática: média RGB visível próxima do NORMAL (referência NORMAL ≈ 174/144/134; HOVER v01 foi 228/198/184 = **rejeitado**); admitir só ligeiro clareamento (≲ +15 por canal).
4. Sem halo claro nas bordas contra fundo escuro e claro; sem texto/ícone.
5. Mantém diferença perceptível vs NORMAL (borda mais clara + realce quente discreto).
