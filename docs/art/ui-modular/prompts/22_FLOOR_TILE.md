# 22_FLOOR_TILE — Tile base repetível do restaurante.

**Status:** PROMPT PREPARADO, **asset NÃO GERADO/NÃO APROVADO**. Fase: consulte [ordem e gate](../GENERATION_ORDER.md).

## Anexos

style_anchor_reference.png; screenshot Salão UI-01B; captura maids ilustradas ART-04.

Acesse os caminhos em [references/README.md](../references/README.md); **anexe os arquivos de imagem** no ChatGPT antes de colar o prompt.

## Campos a preencher antes de enviar

- [MATERIAL]=WARM_OAK | CHECKERED_CREAM | ... (a decidir)
- [VIEW]=ortográfico
- [TILE]=square

## PROMPT DE GERAÇÃO

Cole **antes** o bloco global em [00_GLOBAL_RULES.md](00_GLOBAL_RULES.md); em seguida, copie **somente** o texto abaixo para a geração. Um asset/estado por imagem.

```text
Generate ONE SQUARE SEAMLESS TOP-DOWN FLOOR TILE texture for an interactive orthographic 2D anime café game, material [MATERIAL], not a scene illustration.

The image must repeat on ALL four sides seamlessly when Unity places dozens of identical tiles in a room that can change size. Soft warm painted wood/cream finish with subtle grain and enough local variation to avoid dullness, but VERY low texture contrast and no baked tables, objects, stains, arrows, UI or character shadows. No large directional sunlight gradient across a single tile. Each edge must join the opposite edge naturally and the floor perspective must be compatible with the attached actual game camera.

Export a single perfectly square tile, no perspective frame or border, no fake transparency.
```

## QA e decisões

Testar mosaico 3x3 antes de aprovar; piso claro para maids destacarem; feedback futuro de cliques/tile permanece Unity.

**Registro:** [ASSET_REGISTRY.md](../ASSET_REGISTRY.md). Preserve `raw_gpt`, limpe alpha no Blender quando necessário e só integre após validação Unity e aprovação do responsável.
