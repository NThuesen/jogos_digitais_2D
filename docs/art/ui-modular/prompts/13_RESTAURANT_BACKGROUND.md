# 13_RESTAURANT_BACKGROUND — Camada arquitetônica do Salão — não embutir móveis interativos.

**Status:** PROMPT PREPARADO, **asset NÃO GERADO/NÃO APROVADO**. Fase: consulte [ordem e gate](../GENERATION_ORDER.md).

## Anexos

style_anchor_reference.png; captura Salão 960×600; ART-04 nove maids WebGL; imagens aprovadas de piso e parede (22/23) quando existirem.

Acesse os caminhos em [references/README.md](../references/README.md); **anexe os arquivos de imagem** no ChatGPT antes de colar o prompt.

## Campos a preencher antes de enviar

- [LAYER]=WALLS_ONLY | FLOOR_ONLY | ARCH_DETAIL
- [ROOM_GRID]=medir na Unity
- [VIEW]=ortográfica compatível

## PROMPT DE GERAÇÃO

Cole **antes** o bloco global em [00_GLOBAL_RULES.md](00_GLOBAL_RULES.md); em seguida, copie **somente** o texto abaixo para a geração. Um asset/estado por imagem.

```text
Design ONE REUSABLE ARCHITECTURAL LAYER for a 2D anime café management game's interactive room, type [LAYER]. This is NOT a complete screenshot of a furnished restaurant. The Unity game owns its variable room grid, tables, chairs, cash register, placeable props, selection and pathfinding.

Follow the existing orthographic room camera and grid orientation visible in the attached real Unity screenshot. For WALLS_ONLY: modular wall/backdrop art with believable window, wood trim, shelf recesses and painted illumination, but do not bake any tile-placeable items or UI. For FLOOR_ONLY: uninterrupted readable floor surface/pattern usable as tiles or repeated patches; no fixed 13x9 full-scene geometry and no furniture shadows. For ARCH_DETAIL: detached upper-wall/window/light decorative overlay, alpha outside with safe sorting layer.

Soft anime 2D illustration compatible with the nine imported chibi maids; clean gameplay-floor contrast; no pixelation, noisy floor props, forced perspective or character art. Output one selected layer only.
```

## QA e decisões

Para piso contínuo usar 22; parede modular usar 23. Não produzir um PNG do café inteiro que quebre rooms crescentes.

**Registro:** [ASSET_REGISTRY.md](../ASSET_REGISTRY.md). Preserve `raw_gpt`, limpe alpha no Blender quando necessário e só integre após validação Unity e aprovação do responsável.
