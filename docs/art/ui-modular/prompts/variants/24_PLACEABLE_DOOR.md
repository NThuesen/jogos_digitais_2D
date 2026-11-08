# Móvel da grid — Porta (`door`, 1×1 tiles)

**Esse item EXISTE em `Assets/_Game/Scripts/Sim/Cafe/PlaceableCatalog.cs`. O sprite novo ainda não existe.**

## Anexe EXATAMENTE os PNGs/JPGs existentes

- [`style_anchor_reference.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/art-01/D-pixel-cafe/reference/style_anchor_reference.png) — `docs/art/art-01/D-pixel-cafe/reference/style_anchor_reference.png`
- [`after_SL-1_salao_960x600.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/roadmap/ui-01b/evidence/after_SL-1_salao_960x600.png) — `docs/roadmap/ui-01b/evidence/after_SL-1_salao_960x600.png`
- [`webgl_chrome_960x600.jpg`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/maid-walk/evidence_illustrated/webgl_chrome_960x600.jpg) — `docs/art/maid-walk/evidence_illustrated/webgl_chrome_960x600.jpg`

Se houver um piso piloto aprovado, poderá anexar também `art_working/ui_modular/approved/UI-022_floor_tile_warm_oak_v01.png` (FUTURO, não está na main agora).

## Prompt pronto para copiar

```text
Generate exactly ONE isolated ORIGINAL cozy anime 2D illustrated placeable object sprite for the EXISTING Neko Maid Inc. item "Porta" (code ID "door"), whose in-game grid footprint is 1 x 1 tiles. Visual subject: a single door/entry fixture, visually part of the wall edge.

Match the orthographic 3/4 perspective and scale in the attached real Unity Hall and the soft, illustrated chibi maids. Asset should have plausible visual footprint corresponding to 1 x 1 grid tiles, a consistent floor contact/pivot, restrained painterly warm material, colored outlines and legible silhouette. Do NOT change the item's mechanics, grid footprint, inventory or collision. Unity supplies the position and clicks. Original art only; old reference is about atmosphere, do NOT reproduce pixel art or copy its objects literally.

One object only, centered with generous transparent margin; no baked floor patch, UI, text, prices, names, maids, extra furniture, watermarks or display board. The PNG should allow Blender to clean alpha if needed. Do not generate a furnished restaurant.

Requested filename after I save/rename the generated image: UI-024_placeable_door_v01.png. Return only the single object image.
```

## Salve com nome EXATO

- Bruto: `art_working/ui_modular/raw_gpt/UI-024_placeable_door_v01.png`
- Blender limpo: `art_working/ui_modular/blender_clean/UI-024_placeable_door_v01.png`
- Depois de aprovado: `art_working/ui_modular/approved/UI-024_placeable_door_v01.png`

Nova tentativa: `_v02.png` no lugar de `_v01.png`. Ver [registro](../../ASSET_REGISTRY.md) e [tutorial](../../TUTORIAL.md). Não importar no Unity sem validar sorting, pivô e grid.

## Revisão técnica V02 — ART-PROD-01

Aplicar o [contrato comum](../24_FURNITURE_RENDER_CONTRACT.md): anexar também
seu bloco obrigatório ao prompt acima. Ele define escala, contato, pivô, oclusão,
sorting e orientação. A referência pixel art acima vale somente pela atmosfera.
Substituir screenshots antigos pela captura pós-PR #73 identificada no handoff;
a seleção dessa referência está pendente. Preservar aparência, ID e nome do master.
Não gerar antes de concluir G0/G1; não presumir integração ou aprovação artística.
