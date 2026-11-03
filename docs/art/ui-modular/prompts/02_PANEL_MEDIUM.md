# 02_PANEL_MEDIUM — Painel médio e lateral da mesma família do grande.

**Status:** PROMPT PREPARADO, **asset NÃO GERADO/NÃO APROVADO**. Fase: consulte [ordem e gate](../GENERATION_ORDER.md).

## Anexos

style_anchor_reference.png; UI-001 panel_large APROVADO; captura de Empresa/Elenco (opcional).

Acesse os caminhos em [references/README.md](../references/README.md); **anexe os arquivos de imagem** no ChatGPT antes de colar o prompt.

## Campos a preencher antes de enviar

- [TARGET_ASPECT]=4:3 ou 3:2
- [VARIANT]=base

## PROMPT DE GERAÇÃO

Cole **antes** o bloco global em [00_GLOBAL_RULES.md](00_GLOBAL_RULES.md); em seguida, copie **somente** o texto abaixo para a geração. Um asset/estado por imagem.

```text
Generate ONE medium rectangular reusable information panel to be nested inside or alongside the previously approved large panel. Keep exactly the same finish, border thickness ratio, line weight and lighting as the attached approved sibling; slightly fewer ornaments and a quieter interior.

Medium-scale, front-facing, symmetrical 9-slice-compatible geometry. Rounded corners, simple continuous side bands, visually clean painted cream content field, subtle terracotta/rosewood outline. A short vertical panel must look natural as well as a wider one after 9-slicing.

Do not draw labels, bars, charts, decorative badges or icons. The object must stand alone with alpha outside it and a filled cream center. Do not create a screen composition.
```

## QA e decisões

Conferir se novo painel parece pertencente ao UI-001; paleta exata somente após aprovação.

**Registro:** [ASSET_REGISTRY.md](../ASSET_REGISTRY.md). Preserve `raw_gpt`, limpe alpha no Blender quando necessário e só integre após validação Unity e aprovação do responsável.
