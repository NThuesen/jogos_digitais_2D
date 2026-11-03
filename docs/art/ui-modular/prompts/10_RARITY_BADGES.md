# 10_RARITY_BADGES — Badge de raridade (uma raridade por arquivo).

**Status:** PROMPT PREPARADO, **asset NÃO GERADO/NÃO APROVADO**. Fase: consulte [ordem e gate](../GENERATION_ORDER.md).

## Anexos

style_anchor_reference.png; UI-008 compact aprovado; screenshot com quatro raridades (quando disponível).

Acesse os caminhos em [references/README.md](../references/README.md); **anexe os arquivos de imagem** no ChatGPT antes de colar o prompt.

## Campos a preencher antes de enviar

- [RARITY]=COMMON | RARE | EPIC | LEGENDARY

## PROMPT DE GERAÇÃO

Cole **antes** o bloco global em [00_GLOBAL_RULES.md](00_GLOBAL_RULES.md); em seguida, copie **somente** o texto abaixo para a geração. Um asset/estado por imagem.

```text
Create ONE isolated tiny ornamental RARITY BADGE in grade [RARITY], intended as a sprite overlay in the collectible maid card UI. This is ONE grade, not an entire four-badge board.

Visual distinctions proposed: COMMON neutral parchment/beige; RARE luminous muted sapphire-blue; EPIC soft violet; LEGENDARY warm honey/gold. Preserve consistent outline weight, size, silhouette and shadow for all grades while making each recognizable beyond color alone (simple trim pattern variation). No text, no crown/wing requiring microscopic details at 30px, no rarity name, no number.

A small clean icon-like ornament, strong silhouette and true alpha outside.
```

## QA e decisões

Uma versão por raridade; validar reconhecimento em escala pequena e contrastes distintos.

**Registro:** [ASSET_REGISTRY.md](../ASSET_REGISTRY.md). Preserve `raw_gpt`, limpe alpha no Blender quando necessário e só integre após validação Unity e aprovação do responsável.
