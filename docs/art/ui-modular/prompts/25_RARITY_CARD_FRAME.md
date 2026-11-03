# 25_RARITY_CARD_FRAME — Variação do frame de card por raridade, portrait separado.

**Status:** PROMPT PREPARADO, **asset NÃO GERADO/NÃO APROVADO**. Fase: consulte [ordem e gate](../GENERATION_ORDER.md).

## Anexos

style_anchor_reference.png; UI-008 CARD COMPACT aprovado; badges UI-010 aprovados; screenshot Elenco.

Acesse os caminhos em [references/README.md](../references/README.md); **anexe os arquivos de imagem** no ChatGPT antes de colar o prompt.

## Campos a preencher antes de enviar

- [RARITY]=COMMON | RARE | EPIC | LEGENDARY
- [FRAME]=compact 150x206

## PROMPT DE GERAÇÃO

Cole **antes** o bloco global em [00_GLOBAL_RULES.md](00_GLOBAL_RULES.md); em seguida, copie **somente** o texto abaixo para a geração. Um asset/estado por imagem.

```text
Design ONE collectible MAID CARD RARITY FRAME overlay of grade [RARITY] for the approved compact frame, matching EXACTLY the approved 150x206 base card's silhouette, safe text areas, portrait transparent window, corner positions and border width. Only the rarity accents should change.

COMMON = neutral beige/ivory; RARE = refined sapphire blue; EPIC = violet; LEGENDARY = warm gold plus a very small premium ornament. Distinguish visually even in grayscale where possible through subtle border motif, not through a gigantic gem that obscures names.

Absolutely no maid portrait, text, numeric stats, prices, labels, UI, another frame, or full screenshot. Transparent outside and transparent portrait aperture. Don't warp existing ornament geometry or change center layout between rarities.
```

## QA e decisões

Primeiro validar frame base; quatro versões somente depois, separadas. Não confundir badge (10) com moldura integral (25).

**Registro:** [ASSET_REGISTRY.md](../ASSET_REGISTRY.md). Preserve `raw_gpt`, limpe alpha no Blender quando necessário e só integre após validação Unity e aprovação do responsável.
