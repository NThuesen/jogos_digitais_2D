# Selo de raridade RARE — v02 (REWORK do B4)

**Status:** prompt v02 pronto para copiar; imagem ainda não gerada. **UM prompt = UMA imagem.** O prompt v01 ([10_RARITY_BADGE_RARE.md](10_RARITY_BADGE_RARE.md)) e o RAW v01 ficam preservados.

**Por que v02:** o RARE v01 saiu com proporção **2,97:1** contra **≈1,74:1** do COMMON/EPIC (IoU 0,93, margem inferior 0,3%), ficando ~40% menor e mais baixo em caixas preserveAspect. Ver [relatório B4](../../reviews/UI_BATCH_B4_2026_11_04.md).

## 1. Anexe estes arquivos (nesta ordem)

1. **Estilo:** [`style_anchor_reference.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/art-01/D-pixel-cafe/reference/style_anchor_reference.png) — `docs/art/art-01/D-pixel-cafe/reference/style_anchor_reference.png`
2. **GEOMETRIA (principal):** `UI-010_rarity_badge_common_v01.png`
   - Local (use este): `art_working/ui_modular/blender_clean/UI-010_rarity_badge_common_v01.png`
   - Repo: [branch `qa/ui-modular-new-batch` (PR #47)](https://github.com/NThuesen/jogos_digitais_2D/blob/qa/ui-modular-new-batch/docs/art/ui-modular/incoming/BATCH-2026-11-04/UI-010_rarity_badge_common_v01.png) — `docs/art/ui-modular/incoming/BATCH-2026-11-04/UI-010_rarity_badge_common_v01.png` (não está na main).
3. **GEOMETRIA (confirmação, aceito pelo responsável):** [`UI-010_rarity_badge_epic_v01.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/qa/ui-modular-batch-b4/docs/art/ui-modular/incoming/BATCH-B4/UI-010_rarity_badge_epic_v01.png) — `docs/art/ui-modular/incoming/BATCH-B4/UI-010_rarity_badge_epic_v01.png` (branch da PR #52); local: `art_working/ui_modular/blender_clean/UI-010_rarity_badge_epic_v01.png`.

### NÃO anexe como referência positiva (REFERÊNCIA NEGATIVA)
- `raw_gpt/UI-010_rarity_badge_rare_v01.png` (largo 2,97:1, rejeitado).

## 2. Prompt pronto para copiar

```text
Neko Maid Inc. Original MODULAR Unity sprite, cozy illustrated 2D anime cafe / ornamental visual novel UI. Attached image 1 is a STYLE REFERENCE ONLY. Attached images 2 and 3 are the COMMON and EPIC rarity badges of the same family: they are the GEOMETRY MASTER. No text, numbers, watermarks, character artwork, pixel art or pixel grids.

Illustrate ONE compact collectible MAID CARD rarity badge in grade RARE. It must have EXACTLY the same outer silhouette, proportion and footprint as the COMMON and EPIC badges: the same compact cloverleaf-like shape with rounded top and bottom lobes, a visible aspect of about 1.74:1 (about 1312 x 751 visible pixels centered on a 1536 x 1024 canvas, margins about 7% left/right and 13% top/bottom), the same stroke weight, the same corner roundness, the same central open area. DO NOT make it wider, flatter, taller or smaller than the COMMON/EPIC; DO NOT add wings, crowns, ribbons or any element that extends outside that silhouette.

Rarity distinction only through color and inner detail INSIDE the same footprint: muted sapphire blue frame accents and two small inner facets (small cut-gem facets), with a border pattern slightly different from COMMON and EPIC, still readable at very small on-screen sizes (about 50 px wide). Same central empty area as EPIC (no leaf, no emblem in the middle).

TRANSPARENCY: deliver a PNG with a REAL alpha channel (RGBA). Everything outside the badge silhouette fully transparent: no white background, no matte, no checkerboard, no shadow or glow outside the silhouette. Soft colored outlines, NOT pixel art.

ONE badge only, no other badges, no character, no full card. ONE image only. Intended filename after download: UI-010_rarity_badge_rare_v02.png (rename it yourself if the generator does not keep the name).
```

## 3. Salve com este nome EXATO e critérios de QA

- Bruto: `art_working/ui_modular/raw_gpt/UI-010_rarity_badge_rare_v02.png` (**não** sobrescrever `..._rare_v01.png`).
- CLEAN (feito pelo QA): `art_working/ui_modular/blender_clean/UI-010_rarity_badge_rare_v02.png`.
- Aprovação artística: somente pelo responsável.

**Critérios objetivos (QA incremental):**
1. Alpha real; zero pixels ≥ 8 na borda; 1 componente.
2. Proporção visível 1,74 ± 0,05 (v01: 2,97 = rejeitado); IoU ≥ 0,98 contra COMMON e EPIC; margens ≈ 7%/13% (±2 p.p.).
3. Nada fora da silhueta do COMMON (coroa, laço, asas = REWORK).
4. Distinguível de COMMON e EPIC a 52,8–96,8 u (azul safira + facetas) sem texto.
5. Mesmo contorno/peso; sem folha central.
