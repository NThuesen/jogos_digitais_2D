# Ordem de geração e aprovação (sem lotes)

**Status real em 03/11/2026:** UI-001 está APPROVED (não integrado); UI-004 ornamental NORMAL v01 foi gerado localmente, mas está REWORK; HOVER/PRESSED/DISABLED ornamentais e COMPACT NORMAL continuam PENDING. O objetivo é uma linguagem visual consistente, não completar 95 imagens por completude. Cada link vai para **um prompt executável**, sem `[STATE]` a preencher.

## Prioridade atual aprovada — UI-004 COMPACT NORMAL

**Próxima geração individual:** [`04_BUTTON_PRIMARY_COMPACT_NORMAL.md`](prompts/variants/04_BUTTON_PRIMARY_COMPACT_NORMAL.md) → `art_working/ui_modular/raw_gpt/UI-004_button_primary_compact_normal_v01.png` (**PENDING**, não gerado). Anexos e blocos prontos estão no [tutorial](TUTORIAL.md#passo-2a--gerar-agora-o-botão-compacto-normal-nova-variante-ui-004).

A decisão de produto estabelece duas linhas: ornamental grande (~260×88, arte antiga ainda `REWORK`) e compacta simplificada (44–54, foco `150×44` Loja e `120×44` navegação). Abas são independentes. Primeiro produzir e revisar apenas o NORMAL compacto; **não** avançar HOVER/PRESSED/DISABLED de qualquer linha sem NORMAL correspondente aprovado. Não homologar 768×256/WebGL do grande por inferência.

## P0 — primeiro piloto (sequência originalmente documentada; mantida como histórico)

| Ordem | Gerar | PNG exato que você salva | Como usar |
|---|---|---|---|
| 1 | [Painel grande](prompts/01_PANEL_LARGE.md) | `UI-001_panel_large_v01.png` | Aprovar material e testar 9-slice |
| 2 | [Botão NORMAL](prompts/variants/04_BUTTON_PRIMARY_NORMAL.md) | `UI-004_button_primary_normal_v01.png` | Anexar painel aprovado; aprovar contorno e texto |
| 3 | [Botão HOVER](prompts/variants/04_BUTTON_PRIMARY_HOVER.md) | `UI-004_button_primary_hover_v01.png` | Anexar **o PNG NORMAL aprovado** |
| 4 | [Botão PRESSED](prompts/variants/04_BUTTON_PRIMARY_PRESSED.md) | `UI-004_button_primary_pressed_v01.png` | Anexar **o PNG NORMAL aprovado** |
| 5 | [Botão DISABLED](prompts/variants/04_BUTTON_PRIMARY_DISABLED.md) | `UI-004_button_primary_disabled_v01.png` | Anexar **o PNG NORMAL aprovado** |
| 6 | [Maid Card compacto](prompts/08_MAID_CARD_COMPACT.md) | `UI-008_maid_card_compact_v01.png` | Anexar Suzu do repo, validar portrait separado |

**Aprovação:** mostrar cada asset isolado e, quando possível, aplicado numa captura real da Unity. Se um estado deformar o botão, corrigir/derivar a partir do NORMAL no Blender/Unity em vez de forçar geração problemática.

## P1 — kit estrutural após P0 aprovado

[Painel médio](prompts/02_PANEL_MEDIUM.md), [etiqueta](prompts/03_PANEL_SMALL_LABEL.md), [botões secundários](prompts/05_BUTTON_SECONDARY.md), [abas](prompts/06_TABS_SET.md), [modal](prompts/07_MODAL_DIALOG.md), [raridades](prompts/10_RARITY_BADGES.md), [ícones](prompts/11_ICON_PACK.md).

## P2 — telas de gestão

[Detalhe da maid](prompts/09_MAID_CARD_DETAIL.md), [fundo Menu](prompts/12_MENU_BACKGROUND.md), [item Loja](prompts/17_SHOP_ITEM_FRAME.md), [linha Balanço](prompts/18_BALANCE_ROW_FRAME.md), [balão](prompts/19_DIALOG_BALLOON.md), [logo](prompts/20_LOGO_WORDMARK.md), [molduras de raridade](prompts/25_RARITY_CARD_FRAME.md). [Portraits](prompts/21_MAID_PORTRAIT.md) somente após aprovação específica de identidade.

## P3 — restaurante (precisa respeitar grid da Unity)

[Piso tileable](prompts/22_FLOOR_TILE.md), [paredes](prompts/23_WALL_SEGMENT.md), [itens placeable reais](prompts/24_FURNITURE_SINGLE.md), [arquitetura em camadas](prompts/13_RESTAURANT_BACKGROUND.md) e [decorações não interativas](prompts/14_RESTAURANT_PROPS_SET.md). **Não produzir sala inteira com móveis interativos baked**; testar sorting, pivô, footprint e cliques.

## P4 — HUD do turno

[Barra superior](prompts/15_TOP_BAR_HUD.md), [faixa inferior](prompts/16_BOTTOM_STRIP_DAY_UI.md). Revalidar maids/Yoru e HUD nos tamanhos 960×600 e 1280×720. Não alterar câmera do PR #30 sem justificativa.

[Todos os 95 nomes exatos](OUTPUT_MANIFEST.md) · [Tutorial de anexos](TUTORIAL.md) · [Registro de aprovação](ASSET_REGISTRY.md).
