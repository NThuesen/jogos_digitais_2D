# 21_MAID_PORTRAIT — Retrato individual de uma maid já definida; sob gate de identidade separado.

**Status:** PROMPT PREPARADO, **asset NÃO GERADO/NÃO APROVADO**. Fase: consulte [ordem e gate](../GENERATION_ORDER.md).

## Anexos

APENAS com autorização para portraits: imagens oficiais daquela maid (nunca só frame de referência de ART-01D), identidade aprovada, card compacto/detalhe aprovado; style_anchor_reference.png.

Acesse os caminhos em [references/README.md](../references/README.md); **anexe os arquivos de imagem** no ChatGPT antes de colar o prompt.

## Campos a preencher antes de enviar

- [MAID_NAME]=uma das nove
- [EXPRESSION]=NEUTRAL | HAPPY | FLUSTERED etc.
- [POSE]=BUST_3Q

## PROMPT DE GERAÇÃO

Cole **antes** o bloco global em [00_GLOBAL_RULES.md](00_GLOBAL_RULES.md); em seguida, copie **somente** o texto abaixo para a geração. Um asset/estado por imagem.

```text
Create ONE ORIGINAL HIGH-FIDELITY 2D ILLUSTRATED ANIME PORTRAIT of the existing Neko Maid Inc. maid [MAID_NAME] with expression [EXPRESSION] and pose [POSE], using the ATTACHED OFFICIAL CHARACTER IDENTITY ART as a non-negotiable subject reference. Do not invent a new maid, hair shape, eyes, cat ears, tail, uniform accents, accessories, face identity or color palette. Preserve identity across expression variations.

Composition: isolated bust/half-body portrait facing slightly toward the viewer, head/ears fully visible with breathing room; clean readable expression, soft colored linework, shading and proportions harmonious with the existing anime 2D Walk artwork (not pixel art). Transparent outside and no painted card frame; Unity will place portrait behind the card border and add names/stats dynamically.

No caption, speech, icon, watermark or UI. Never base the character only on the old reference image's pink maid; this must be [MAID_NAME] from the real existing game.

If no clear official reference to [MAID_NAME] is attached, DO NOT generate a substitute or generic catmaid; request the proper identity reference.
```

## QA e decisões

NÃO executar sem autorização de portrait e anexos oficiais por personagem; não reescrever nine walking sprites.

**Registro:** [ASSET_REGISTRY.md](../ASSET_REGISTRY.md). Preserve `raw_gpt`, limpe alpha no Blender quando necessário e só integre após validação Unity e aprovação do responsável.
