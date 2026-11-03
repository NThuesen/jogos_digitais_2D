# 24_FURNITURE_SINGLE — Móvel específico placeable na grid; um objeto por render.

**Status:** PROMPT PREPARADO, **asset NÃO GERADO/NÃO APROVADO**. Fase: consulte [ordem e gate](../GENERATION_ORDER.md).

## Anexos

style_anchor_reference.png; screenshot Salão; piso e parede se aprovados; captura maids ART-04.

Acesse os caminhos em [references/README.md](../references/README.md); **anexe os arquivos de imagem** no ChatGPT antes de colar o prompt.

## Campos a preencher antes de enviar

- [FURNITURE]=nome exato do item existente
- [FOOTPRINT]=medir WxH tiles
- [PIVOT]=floor contact
- [VIEW]=room camera

## PROMPT DE GERAÇÃO

Cole **antes** o bloco global em [00_GLOBAL_RULES.md](00_GLOBAL_RULES.md); em seguida, copie **somente** o texto abaixo para a geração. Um asset/estado por imagem.

```text
Create ONE SINGLE PLACEABLE FURNITURE SPRITE representing the EXISTING game item [FURNITURE] in Neko Maid Inc., preserving the footprint [FOOTPRINT] relative to the room grid and the orthographic view of the attached real Unity screenshot. Avoid inventing mechanics, collision size or a new interactive object.

Painted illustrated anime café style: consistent 3/4 or top-visible orthographic perspective from the room's own camera, warm color harmony, tidy outlines, moderate readable detail, materials appropriate to the chosen item. Full object and contact base visible with alpha around it. Do not include a floor patch, shadow that spills across many tiles, UI label, other furniture, character or room.

Object should be recognizable at the same on-screen scale as the imported chibi maids. If rotation matters, produce separate direction-specific images later based on actual Unity needs, not a single generic tilted asset.
```

## QA e decisões

Inserção e footprint/cliques ficam em Unity; uma imagem não constitui item placeable jogável sem integração.

**Registro:** [ASSET_REGISTRY.md](../ASSET_REGISTRY.md). Preserve `raw_gpt`, limpe alpha no Blender quando necessário e só integre após validação Unity e aprovação do responsável.
