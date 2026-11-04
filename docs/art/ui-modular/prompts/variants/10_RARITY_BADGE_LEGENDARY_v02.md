# Selo de raridade LEGENDARY — v02 (REWORK do B4)

**Status:** prompt v02 pronto para copiar; imagem ainda não gerada. **UM prompt = UMA imagem.** O prompt v01 ([10_RARITY_BADGE_LEGENDARY.md](10_RARITY_BADGE_LEGENDARY.md)) e o RAW v01 ficam preservados.

**Por que v02:** o LEGENDARY v01 saiu com **2,71:1** (IoU 0,88 contra o COMMON) e coroa, laço e louros extrapolando a silhueta COMMON, margens 0,7/1,0%. A v02 mantém a distinção dourada, mas **dentro** da mesma silhueta/proporção do COMMON/EPIC. Ver [relatório B4](../../reviews/UI_BATCH_B4_2026_11_04.md).

## 1. Anexe estes arquivos (nesta ordem)

1. **Estilo:** [`style_anchor_reference.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/art-01/D-pixel-cafe/reference/style_anchor_reference.png) — `docs/art/art-01/D-pixel-cafe/reference/style_anchor_reference.png`
2. **GEOMETRIA (principal):** `UI-010_rarity_badge_common_v01.png`
   - Local (use este): `art_working/ui_modular/blender_clean/UI-010_rarity_badge_common_v01.png`
   - Repo: [branch `qa/ui-modular-new-batch` (PR #47)](https://github.com/NThuesen/jogos_digitais_2D/blob/qa/ui-modular-new-batch/docs/art/ui-modular/incoming/BATCH-2026-11-04/UI-010_rarity_badge_common_v01.png) — `docs/art/ui-modular/incoming/BATCH-2026-11-04/UI-010_rarity_badge_common_v01.png` (não está na main).
3. **GEOMETRIA (confirmação, aceito pelo responsável):** [`UI-010_rarity_badge_epic_v01.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/qa/ui-modular-batch-b4/docs/art/ui-modular/incoming/BATCH-B4/UI-010_rarity_badge_epic_v01.png) — `docs/art/ui-modular/incoming/BATCH-B4/UI-010_rarity_badge_epic_v01.png` (branch da PR #52); local: `art_working/ui_modular/blender_clean/UI-010_rarity_badge_epic_v01.png`.

### NÃO anexe como referência positiva (REFERÊNCIA NEGATIVA)
- `raw_gpt/UI-010_rarity_badge_legendary_v01.png` (2,71:1 com coroa/laço/louros fora da silhueta, rejeitado).

## 2. Prompt pronto para copiar

```text
Neko Maid Inc. Original MODULAR Unity sprite, cozy illustrated 2D anime cafe / ornamental visual novel UI. Attached image 1 is a STYLE REFERENCE ONLY. Attached images 2 and 3 are the COMMON and EPIC rarity badges of the same family: they are the GEOMETRY MASTER. No text, numbers, watermarks, character artwork, pixel art or pixel grids.

Illustrate ONE compact collectible MAID CARD rarity badge in grade LEGENDARY. It must have EXACTLY the same outer silhouette, proportion and footprint as the COMMON and EPIC badges: the same compact cloverleaf-like shape with rounded top and bottom lobes, a visible aspect of about 1.74:1 (about 1312 x 751 visible pixels centered on a 1536 x 1024 canvas, margins about 7% left/right and 13% top/bottom), the same stroke weight, the same corner roundness, the same central open area. DO NOT make it wider or taller than the COMMON/EPIC. DO NOT draw a crown, bow, ribbon, wings or laurel wreath that extends outside that silhouette; nothing may stick out of the COMMON outline.

The legendary distinction must live INSIDE the same footprint: warm honey-gold frame, a richer gold border pattern with a subtle star filigree and tiny golden sparkle details placed on the frame itself, still readable at very small on-screen sizes (about 50 px wide) and clearly different from COMMON, RARE (sapphire) and EPIC (purple). Keep the central area open like EPIC (no leaf, no emblem in the middle).

TRANSPARENCY: deliver a PNG with a REAL alpha channel (RGBA). Everything outside the badge silhouette fully transparent: no white background, no matte, no checkerboard, no shadow or glow outside the silhouette. Soft colored outlines, NOT pixel art.

ONE badge only, no other badges, no character, no full card. ONE image only. Intended filename after download: UI-010_rarity_badge_legendary_v02.png (rename it yourself if the generator does not keep the name).
```

## 3. Salve com este nome EXATO e critérios de QA

- Bruto: `art_working/ui_modular/raw_gpt/UI-010_rarity_badge_legendary_v02.png` (**não** sobrescrever `..._legendary_v01.png`).
- CLEAN (feito pelo QA): `art_working/ui_modular/blender_clean/UI-010_rarity_badge_legendary_v02.png`.
- Aprovação artística: somente pelo responsável.

**Critérios objetivos (QA incremental):**
1. Alpha real; zero pixels ≥ 8 na borda; 1 componente.
2. Proporção visível 1,74 ± 0,05 (v01: 2,71 = rejeitado); IoU ≥ 0,98 contra COMMON e EPIC; margens ≈ 7%/13% (±2 p.p.).
3. Nada fora da silhueta do COMMON; coroa/laço/louros externos = REWORK.
4. Dourado distinguível de COMMON/RARE/EPIC a 52,8–96,8 u, sem texto.
5. Mesmo contorno/peso; sem folha central.
