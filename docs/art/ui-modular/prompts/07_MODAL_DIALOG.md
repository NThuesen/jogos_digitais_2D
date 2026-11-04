# 07_MODAL_DIALOG — prompt corrigido v02 (topo não cortado)

**Próxima geração: `UI-007_modal_dialog_v02.png` — PENDING.** A imagem `UI-007_modal_dialog_v01.png` permanece **REWORK** (PR #47): o ornamento superior já está cortado no RAW, com pixels opacos no limite y=0. Não faça upscale/padding artificial para fingir recuperar o que foi cortado. Preserve v01 como histórico; v02 é **novo PNG**, não renomeie o anterior.

## 1. Anexe estes arquivos reais ao GPT Web

**Obrigatórios** (baixe o PNG original ou anexe o arquivo local; links não substituem anexos):

1. **`UI-007_modal_dialog_v01.png`** — arquivo RAW já gerado, `art_working/ui_modular/raw_gpt/UI-007_modal_dialog_v01.png` (**LOCAL; REWORK; não versionado no Git**). Serve **somente** para continuidade estética e mostrar o que NÃO repetir: o topo cortado. Não copie seu enquadramento nem a faixa superior truncada.
2. **`UI-001_panel_large_v01.png`** — [PNG aprovado real](../approved/UI-001_panel_large_v01.png), `docs/art/ui-modular/approved/UI-001_panel_large_v01.png`. Referência de material, creme e moldura.
3. **`after_DG-1_dialogo-confirmar-cancelar_960x600.png`** — [captura real](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/roadmap/ui-01b/evidence/after_DG-1_dialogo-confirmar-cancelar_960x600.png), `docs/roadmap/ui-01b/evidence/after_DG-1_dialogo-confirmar-cancelar_960x600.png`. Referência de dimensão/caso de uso, nunca copiar a tela.

**Opcionais:** `style_anchor_reference.png` — [referência histórica](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/art-01/D-pixel-cafe/reference/style_anchor_reference.png), `docs/art/art-01/D-pixel-cafe/reference/style_anchor_reference.png` (**atmosfera, não pixel art**); `UI-019_dialog_panel_v01.png` — `art_working/ui_modular/blender_clean/UI-019_dialog_panel_v01.png` (**CLEAN local, candidato sem aprovação artística; apenas parentesco de acabamento**).

Se o RAW v01 local não estiver acessível, use obrigatoriamente UI-001 e a captura; **não invente arquivo ou anexo futuro**.

## 2. Prompt completo — copie somente este bloco

```text
PROJECT: Neko Maid Inc. Generate exactly ONE revised modular MODAL DIALOG FRAME sprite, v02, for an original cozy illustrated 2D anime café / ornamental visual-novel management UI.

IMAGE REFERENCE PRIORITY:
- UI-001_panel_large_v01.png (APPROVED): authoritative warm ivory, restrained brass, rosewood/terracotta surface finish and border quality.
- UI-007_modal_dialog_v01.png (REWORK): preserve its charming bow/paw/rosewood design family, BUT its top is physically CROPPED and must NOT determine the new canvas limits. Recreate a complete intact upper decoration.
- after_DG-1_dialogo-confirmar-cancelar_960x600.png: actual Unity layout reference, NOT content to copy.
Optional style_anchor_reference.png is historical pixel-art mood only: do not make pixel art. UI-019_dialog_panel_v01.png, if provided, is a non-approved sibling reference for material consistency only.

CRITICAL V01 DEFECT TO FIX: In the original raster the uppermost ornate silhouette meets the image boundary and is amputated. V02 must show the WHOLE crown/ribbon/bow/top finial and every petal/paw/curve, completely inside the PNG canvas. Provide a deliberately visible TRANSPARENT top safety margin of at least ~8-10% of canvas height above the highest ornament; keep at least ~5% clear transparent margin on left/right/bottom. No opaque or partially transparent decorative pixel may touch the top edge. Do not crop, clip, fade out or flatten the crest to satisfy the canvas; choose a wider canvas if needed. All ornaments must be completely drawn.

COMPOSITION: exactly ONE isolated, front-facing wide panel, roughly 620 x 300 Unity reference units, aspect ratio around 2.1:1 for the visible frame. Symmetric illustrated trim, elegant warm-cream OPAQUE center, thin ornate rosewood and brass borders. Three EMPTY, spacious text-safe zones: dynamic short title at top, broad multiline dialog body in center, and space for up to two Unity-controlled buttons along the bottom. No painted text or buttons. All important details must remain outside the text-safe center. Corners and edges should be consistent and front-facing, suitable for cautious use as a whole Sprite or later technical slicing without cutting ornaments.

TRANSPARENCY: real PNG alpha outside the entire silhouette, including the safety margins; no fake checkerboard, drop-shadow backdrop or matte. Cream interior stays filled and readable. No character art. Avoid fringes/halos on transparent edges. Smooth, clean illustrated linework, NOT pixel art.

UNITY OWNS: modal shade, typography, localization, clickable buttons, focus, state, sizing and interaction. Do not bake any fake UI or complete screen.

STRICT NEGATIVES: no screenshot, whole UI, words, letters, numbers, labels, prices, icons pretending to be buttons, decorative content over text zones, watermark, extra panels or variants. Do not trace or mechanically duplicate the old cut crest.

OUTPUT EXACTLY ONE self-contained EMPTY modal frame image, completely visible in its canvas, no cropping. Intended filename: UI-007_modal_dialog_v02.png.
```

## 3. Destino, status e QA

- RAW novo: `art_working/ui_modular/raw_gpt/UI-007_modal_dialog_v02.png`.
- CLEAN depois da auditoria: `art_working/ui_modular/blender_clean/UI-007_modal_dialog_v02.png`.
- Só após aprovação explícita: `art_working/ui_modular/approved/UI-007_modal_dialog_v02.png`.
- Preservar `UI-007_modal_dialog_v01.png`; nenhuma aprovação automática.

QA novo: confirmar alpha transparente na faixa superior e **zero pixels de ornamento tocando y=0**, silhueta inteira, 620×300 aproximado, título/texto/botões Unity, 960×600 / 1280×720; testar slicing apenas se fizer sentido. Não repetir QA da versão v01 desnecessariamente.

[Tutorial](../TUTORIAL.md) · [histórico técnico PR #47](https://github.com/NThuesen/jogos_digitais_2D/pull/47).
