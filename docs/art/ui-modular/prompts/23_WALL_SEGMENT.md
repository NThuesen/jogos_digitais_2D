# 23_WALL_SEGMENT — Segmento modular de parede do café.

**Status:** PROMPT PREPARADO, **asset NÃO GERADO/NÃO APROVADO**. Fase: consulte [ordem e gate](../GENERATION_ORDER.md).

## Anexos

style_anchor_reference.png; screenshot Salão; piso aprovado (se houver); visual de móveis/maids atuais.

Acesse os caminhos em [references/README.md](../references/README.md); **anexe os arquivos de imagem** no ChatGPT antes de colar o prompt.

## Campos a preencher antes de enviar

- [SEGMENT]=STRAIGHT | CORNER_LEFT | CORNER_RIGHT | WINDOW | DOOR
- [GRID_ALIGN]=conforme teste Unity

## PROMPT DE GERAÇÃO

Cole **antes** o bloco global em [00_GLOBAL_RULES.md](00_GLOBAL_RULES.md); em seguida, copie **somente** o texto abaixo para a geração. Um asset/estado por imagem.

```text
Create ONE architectural CAFÉ WALL SEGMENT, kind [SEGMENT], for a dynamically sized orthographic room viewed from the same angle as the attached Unity screenshot. Architectural illustrated-anime aesthetic: painted warm timber wainscoting, cream plaster, refined rosewood trim, cozy light, carefully muted decoration.

Segments must be composable side by side at a consistent height, vanishing direction and matching edge color; seams line up with the room's tile units. Generate a single isolated segment (not a room), alpha where empty, no giant props that obstruct interacting with floor tiles. STRAIGHT should repeat horizontally; CORNER and WINDOW/DOOR have compatible ends and don't imply a baked gameplay state.

No maid, inventory furniture, UI, text, game logo, floor grid embedded into wall image or whole café screenshot.
```

## QA e decisões

Gerar 5 variantes separadas quando aprovadas. Verificar onde Unity define parede/oclusão e sorting antes de importar.

**Registro:** [ASSET_REGISTRY.md](../ASSET_REGISTRY.md). Preserve `raw_gpt`, limpe alpha no Blender quando necessário e só integre após validação Unity e aprovação do responsável.
