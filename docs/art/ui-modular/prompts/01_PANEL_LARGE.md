# 01_PANEL_LARGE — Painel estrutural amplo; piloto de materiais e 9-slice.

**Status:** PROMPT PREPARADO, **asset NÃO GERADO/NÃO APROVADO**. Fase: consulte [ordem e gate](../GENERATION_ORDER.md).

## Anexos

OBRIGATÓRIO: style_anchor_reference.png. ÚTIL: captura da Loja ou Elenco (ver references/README.md). Depois do piloto: painel aprovado.

Acesse os caminhos em [references/README.md](../references/README.md); **anexe os arquivos de imagem** no ChatGPT antes de colar o prompt.

## Campos a preencher antes de enviar

- [TARGET_ASPECT]=aprox. 3:2
- [CONTENT_CLEAR]=70% da área interna
- [VARIANT]=base

## PROMPT DE GERAÇÃO

Cole **antes** o bloco global em [00_GLOBAL_RULES.md](00_GLOBAL_RULES.md); em seguida, copie **somente** o texto abaixo para a geração. Um asset/estado por imagem.

```text
Create exactly ONE standalone large ornamental, scalable UI panel background — no screenshot or extra pieces.

Geometry: broad horizontal rectangle, target aspect ratio about 3:2. Four matching, gently rounded corners; matching left/right and top/bottom straight bands, suitable for a nine-slice frame. Outer contour and frame are continuous; keep enough border width for a convincing material finish but reserve at least 70% of the panel as quiet, uninterrupted content area.

Finish hypothesis for visual approval (not fixed palette): painted ivory inset panel; muted rosewood/terracotta outer rim; controlled soft peach highlights, subtle warm wooden café identity and small paw/leaf relief ONLY at the corners outside the resizable bands. A touch of layered relief and contact shadow, not faux 3D plastic. No dramatic perspective or trapezoid.

The cream center is an intentional SOLID surface (do not make it transparent). Outside the shape must be real alpha (or a removable matte). No header, no baked typography, no portrait, no icons, no buttons.

Design the shape so Unity dynamic text and buttons can sit in the center at a range of sizes. No single corner may be unique unless supplied as a separate overlay. Output only this panel.
```

## QA e decisões

Piloto P0. Antes de aprovar, conferir corners e side bands com um mock 9-slice no Unity; não esticar losangos/coroas.

**Registro:** [ASSET_REGISTRY.md](../ASSET_REGISTRY.md). Preserve `raw_gpt`, limpe alpha no Blender quando necessário e só integre após validação Unity e aprovação do responsável.
