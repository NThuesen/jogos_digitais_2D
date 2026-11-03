# 17_SHOP_ITEM_FRAME — Moldura repetida da Loja para uma linha/ícone/preço.

**Status:** PROMPT PREPARADO, **asset NÃO GERADO/NÃO APROVADO**. Fase: consulte [ordem e gate](../GENERATION_ORDER.md).

## Anexos

style_anchor_reference.png; UI-001 painel aprovado; screenshot Loja UI-01A 960×600.

Acesse os caminhos em [references/README.md](../references/README.md); **anexe os arquivos de imagem** no ChatGPT antes de colar o prompt.

## Campos a preencher antes de enviar

- [TARGET_ASPECT]=850:62 (~13.7:1)
- [ZONE]=icon,name,description,price-button

## PROMPT DE GERAÇÃO

Cole **antes** o bloco global em [00_GLOBAL_RULES.md](00_GLOBAL_RULES.md); em seguida, copie **somente** o texto abaixo para a geração. Um asset/estado por imagem.

```text
Design ONE very wide and shallow SHOP ROW FRAME, matching the approximate real Unity row 850x62 reference units in ratio. It is an undecorated background for a scrolling list of furniture, outfits or gifts — no item image or button drawn into it.

Provide a small quiet visual reserve on the LEFT for a separately positioned item icon; a broad cream content band in the middle for localized name/description and owned count; open clean RIGHT edge where Unity's real Buy button will overlay. Use delicate border and optional hairline divider, but no fixed-size cut-out that prevents variations.

Reusable 9-slice-friendly smooth corners/straight edges; subtle painted details, little ornament, alpha outside. Do not draw labels, numbers, currency or item art.
```

## QA e decisões

UI-01A corrigiu scroll/cortes; respeitar os limites atuais em vez de reintroduzir sobreposição de botão e estoque.

**Registro:** [ASSET_REGISTRY.md](../ASSET_REGISTRY.md). Preserve `raw_gpt`, limpe alpha no Blender quando necessário e só integre após validação Unity e aprovação do responsável.
