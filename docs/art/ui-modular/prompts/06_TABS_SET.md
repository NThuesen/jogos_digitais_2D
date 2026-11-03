# 06_TABS_SET — Aba modular de Loja/menus (um estado por arquivo).

**Status:** PROMPT PREPARADO, **asset NÃO GERADO/NÃO APROVADO**. Fase: consulte [ordem e gate](../GENERATION_ORDER.md).

## Anexos

style_anchor_reference.png; painel e botões aprovados; screenshot Loja 960×600.

Acesse os caminhos em [references/README.md](../references/README.md); **anexe os arquivos de imagem** no ChatGPT antes de colar o prompt.

## Campos a preencher antes de enviar

- [STATE]=INACTIVE | ACTIVE | HOVER
- [TARGET_ASPECT]=aprox. 3:1

## PROMPT DE GERAÇÃO

Cole **antes** o bloco global em [00_GLOBAL_RULES.md](00_GLOBAL_RULES.md); em seguida, copie **somente** o texto abaixo para a geração. Um asset/estado por imagem.

```text
Create ONE reusable horizontal TAB visual for a cozy anime café management UI. State [STATE] ONLY. Must fit a tab strip with dynamic Unity labels. Approximately 3:1, moderately rounded upper corners and straight lower seating edge; symmetrical, front-facing, easy to align adjacent to sibling tabs.

Match the approved cream/rosewood illustrated style. ACTIVE: visibly raised and connected to the content panel, accent rim. INACTIVE: softer and recessed. HOVER: brighter than inactive yet clearly not selected. Geometry and alpha silhouette must remain consistent across variants.

Large blank center, no words, no icons, no numbers, no tabs beside it.
```

## QA e decisões

Três PNGs independentes; estado ACTIVE fica em Unity, não apenas desenho.

**Registro:** [ASSET_REGISTRY.md](../ASSET_REGISTRY.md). Preserve `raw_gpt`, limpe alpha no Blender quando necessário e só integre após validação Unity e aprovação do responsável.
