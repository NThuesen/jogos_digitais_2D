# Ordem de geração e gates de aprovação

**Não gere tudo automaticamente.** Os prompts são catálogo; avançar entre gates exige comparação visual real e aprovação explícita. Reaproveitar referência e assets aprovados; evitar 20 variações sem critério.

## Gate P0 — piloto mínimo (prioridade imediata)

| Ordem | Prompt | Objetivo | Gate |
| --- | --- | --- | --- |
| 1 | [01_PANEL_LARGE](prompts/01_PANEL_LARGE.md) | Fixar moldura/creme/matéria, testar 9-slice no Unity | Aprovação de 1 painel |
| 2 | [04_BUTTON_PRIMARY](prompts/04_BUTTON_PRIMARY.md) | Fixar ação, 4 estados gerados **individualmente** | Aprovação normal + contraste hover/pressed/disabled |
| 3 | [08_MAID_CARD_COMPACT](prompts/08_MAID_CARD_COMPACT.md) | Fixar moldura, área de retrato e legibilidade em tamanho real | Aprovação com uma maid existente atrás da moldura |

**Não gerar novas maids** nesta etapa. Use imagens de Walk existentes apenas como referências de proporção; portrait específico é trabalho futuro.

## Gate P1 — kit estrutural após P0

[02 painel médio](prompts/02_PANEL_MEDIUM.md), [03 label](prompts/03_PANEL_SMALL_LABEL.md), [05 botão secundário](prompts/05_BUTTON_SECONDARY.md), [06 abas](prompts/06_TABS_SET.md), [07 modal](prompts/07_MODAL_DIALOG.md), [10 raridades](prompts/10_RARITY_BADGES.md), [11 ícones](prompts/11_ICON_PACK.md).

## Gate P2 — tela representativa de UI

[09 detalhe da maid](prompts/09_MAID_CARD_DETAIL.md), [12 fundo Menu](prompts/12_MENU_BACKGROUND.md), [17 Loja](prompts/17_SHOP_ITEM_FRAME.md), [18 Balanço](prompts/18_BALANCE_ROW_FRAME.md), [19 balão](prompts/19_DIALOG_BALLOON.md), [20 logo](prompts/20_LOGO_WORDMARK.md), [21 portrait de maid](prompts/21_MAID_PORTRAIT.md), [25 moldura por raridade](prompts/25_RARITY_CARD_FRAME.md). Gerar `21` somente com identidade de personagem e gate específicos.

## Gate P3 — restaurante interativo (módulos, não screenshot)

Antes de gerar imagem grande, conferir geometria atual, câmera, sorting e área clicável. [22 piso](prompts/22_FLOOR_TILE.md), [23 parede](prompts/23_WALL_SEGMENT.md), [24 móvel individual](prompts/24_FURNITURE_SINGLE.md), [13 fundo arquitetônico](prompts/13_RESTAURANT_BACKGROUND.md), [14 catálogo de props](prompts/14_RESTAURANT_PROPS_SET.md). O fundo é **layer arquitetônica**, não o café completo com mesas baked.

## Gate P4 — HUD e acabamento em gameplay

[15 barra superior](prompts/15_TOP_BAR_HUD.md), [16 faixa inferior](prompts/16_BOTTOM_STRIP_DAY_UI.md). Revalidar HUD/Yoru em duas resoluções após integração.

## Ao concluir cada gate

Registrar aprovado/reprovado, arquivo e hash, screenshot do Unity, tamanho/técnica de slicing, legibilidade, alpha e impacto aproximado de memória. **Um mockup bonito não substitui um teste de uso real.**
