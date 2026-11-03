# 16_BOTTOM_STRIP_DAY_UI — Moldura inferior do turno sem posições fixas baked.

**Status:** PROMPT PREPARADO, **asset NÃO GERADO/NÃO APROVADO**. Fase: consulte [ordem e gate](../GENERATION_ORDER.md).

## Anexos

style_anchor_reference.png; captura PR #30 HUD com nove maids; compact card aprovado.

Acesse os caminhos em [references/README.md](../references/README.md); **anexe os arquivos de imagem** no ChatGPT antes de colar o prompt.

## Campos a preencher antes de enviar

- [TARGET_ASPECT]=1100:76 (~14.5:1)
- [ZONE]=repeatable border not baked slots

## PROMPT DE GERAÇÃO

Cole **antes** o bloco global em [00_GLOBAL_RULES.md](00_GLOBAL_RULES.md); em seguida, copie **somente** o texto abaixo para a geração. Um asset/estado por imagem.

```text
Generate ONE low-profile BOTTOM HUD STRIP backing for the shift, approx. 1100x76 Unity reference units in aspect ratio, designed to carry a row of independent compact maid status elements overlayed by Unity.

Soft cream/rosewood decorative lip, subtle light line and unobtrusive café ornamentation near the ends. Keep the entire horizontal middle quiet and repeatable. Do NOT bake fixed character card slots, names, faces, nine boxes, role labels, speed arrows or controls: Unity currently decides which maids and how many small cards fit.

Thin, wide, front-facing, symmetrical borders, transparent outside. No whole gameplay screenshot.
```

## QA e decisões

A UI real precisa suportar nove cartões no QA e 960x600 sem clipping; fundo gráfico não fixa quantidade.

**Registro:** [ASSET_REGISTRY.md](../ASSET_REGISTRY.md). Preserve `raw_gpt`, limpe alpha no Blender quando necessário e só integre após validação Unity e aprovação do responsável.
