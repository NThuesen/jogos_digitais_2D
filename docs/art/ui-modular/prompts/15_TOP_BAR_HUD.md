# 15_TOP_BAR_HUD — Moldura visual do HUD superior do turno.

**Status:** PROMPT PREPARADO, **asset NÃO GERADO/NÃO APROVADO**. Fase: consulte [ordem e gate](../GENERATION_ORDER.md).

## Anexos

style_anchor_reference.png; captura do turno após PR #30; UI-001 e UI-002 aprovados.

Acesse os caminhos em [references/README.md](../references/README.md); **anexe os arquivos de imagem** no ChatGPT antes de colar o prompt.

## Campos a preencher antes de enviar

- [TARGET_ASPECT]=760:76 (~10:1)
- [ZONE]=clock,money,tallies,speed

## PROMPT DE GERAÇÃO

Cole **antes** o bloco global em [00_GLOBAL_RULES.md](00_GLOBAL_RULES.md); em seguida, copie **somente** o texto abaixo para a geração. Um asset/estado por imagem.

```text
Create ONE long, thin, reusable TOP SHIFT HUD PANEL graphic, matching the current 760x76 Unity reference-unit placement. Front-on horizontal illustrated frame with a restrained cream interior, thin rounded rosewood trim and subtle café ornaments at outer ends only. It is a visual backing; Unity renders the clock, till, numbers, counters, buttons and speed states.

Keep the central 85% almost unornamented; leave clear grouping space without drawing fixed clock or currency icons. Side ends should not clip at 960x600. Shape may use 9-slice only if corners and straight bands are consistent; avoid baked slots that lock a particular number of controls.

No numbers, names, fake UI, visible buttons or stats. True alpha around panel.
```

## QA e decisões

Não alterar a câmera nem o HUD do PR #30 só para acomodar arte; checar Yoru e faixa inferior.

**Registro:** [ASSET_REGISTRY.md](../ASSET_REGISTRY.md). Preserve `raw_gpt`, limpe alpha no Blender quando necessário e só integre após validação Unity e aprovação do responsável.
