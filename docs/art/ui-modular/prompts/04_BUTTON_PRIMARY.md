# 04_BUTTON_PRIMARY — Botão primário (um PNG para cada estado).

**Status:** PROMPT PREPARADO, **asset NÃO GERADO/NÃO APROVADO**. Fase: consulte [ordem e gate](../GENERATION_ORDER.md).

## Anexos

style_anchor_reference.png; UI-001 aprovado; a versão NORMAL aprovada obrigatória para os outros estados.

Acesse os caminhos em [references/README.md](../references/README.md); **anexe os arquivos de imagem** no ChatGPT antes de colar o prompt.

## Campos a preencher antes de enviar

- [STATE]=NORMAL | HOVER | PRESSED | DISABLED
- [TARGET_ASPECT]=aprox. 3.5:1

## PROMPT DE GERAÇÃO

Cole **antes** o bloco global em [00_GLOBAL_RULES.md](00_GLOBAL_RULES.md); em seguida, copie **somente** o texto abaixo para a geração. Um asset/estado por imagem.

```text
Create EXACTLY ONE graphic for a primary interactive BUTTON in state [STATE], matching the approved panel family. Horizontal front-facing rounded-rectangle wooden/ivory café button, approximately 3.5:1 ratio, with straight middle bands for potential 9-slice. The central 65% must remain plain and bright enough for dynamic Unity text.

Consistent with an original refined anime visual-novel café: warm rosewood outline, illustrated enamel/wood texture, restrained soft glow, playful but professional. [NORMAL]: clear solid baseline. [HOVER]: gentle brighter top rim/contrast, same silhouette. [PRESSED]: slightly inset/down-lit shading, no size or alignment change. [DISABLED]: desaturated quieter finish, still readable, same silhouette. Only the selected [STATE] should appear in THIS image.

No baked label, no arrows, no symbol, no sprite sheet, no row of states. Real transparent outside, natural shadow within padding.
```

## QA e decisões

Gerar 4 renders individuais (normal, hover, pressed, disabled); manter geometria e crop idênticos. Unity faz Button/state.

**Registro:** [ASSET_REGISTRY.md](../ASSET_REGISTRY.md). Preserve `raw_gpt`, limpe alpha no Blender quando necessário e só integre após validação Unity e aprovação do responsável.
