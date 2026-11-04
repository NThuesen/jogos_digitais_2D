# Ordem de geração e aprovação (sem lotes)

**Status vigente em 04/11/2026:** UI-001 e COMPACT NORMAL mantêm APPROVED. HOVER v02, PRESSED v01, DISABLED v02, UI-002 e UI-003 (com restrições) foram aprovados expressamente pelo responsável; UI-008 aceita somente em 150×206 detalhado, REWORK em 0,62×. [PNGs finais e handoff](approved/README.md). Nenhum integrado. LINEUP MINIMAL PENDING, não gerada. O objetivo é uma linguagem visual consistente, não completar 100 imagens por completude.


## Histórico de geração — estados da UI-004 compacta

**Checkpoint anterior às aprovações de 04/11/2026; sequência abaixo mantida como histórico, não regenerar os estados aprovados.**

**NORMAL compacto já foi gerado e validado na [PR #42](https://github.com/NThuesen/jogos_digitais_2D/pull/42), com aprovação do responsável.** Próximas imagens candidatas são **COMPACT HOVER**, **COMPACT PRESSED** e **COMPACT DISABLED**, nessa ordem, cada qual com prompt próprio e um único PNG. [Tutorial e anexos](TUTORIAL.md#passo-2b--gerar-hover-pressed-e-disabled-do-botão-compacto-aprovado).

| Ordem | Prompt pronto | Saída local RAW |
|---|---|---|
| 1 | [COMPACT HOVER](prompts/variants/04_BUTTON_PRIMARY_COMPACT_HOVER.md) | `UI-004_button_primary_compact_hover_v01.png` |
| 2 | [COMPACT PRESSED](prompts/variants/04_BUTTON_PRIMARY_COMPACT_PRESSED.md) | `UI-004_button_primary_compact_pressed_v01.png` |
| 3 | [COMPACT DISABLED](prompts/variants/04_BUTTON_PRIMARY_COMPACT_DISABLED.md) | `UI-004_button_primary_compact_disabled_v01.png` |

**Anexo principal:** NORMAL COMPACTO **limpo 2072×380** em `art_working/ui_modular/blender_clean/`, não o ornamental e não o bruto. Outros estados permanecem PENDING até geração, QA e aprovação próprios.

A decisão de produto estabelece duas linhas: ornamental grande (~260×88, arte antiga ainda `REWORK`) e compacta simplificada (44–54, foco `150×44` Loja e `120×44` navegação). Abas são independentes. O NORMAL compacto já foi aprovado pelo responsável; gerar seus outros estados em separado, **sem** iniciar os estados ornamentais (NORMAL ornamental ainda REWORK). Não homologar 768×256/WebGL do grande por inferência.

## P0 — primeiro piloto (sequência originalmente documentada; mantida como histórico)## Nova prioridade — Maid Card minimalista da Escalação (04/11/2026)

**Decisão aprovada:** duas apresentações para a mesma maid, sem duplicar dados. O UI-008 já gerado é aprovado expressamente para uso **detalhado somente em 150×206**, mas falhou visualmente na Escalação a 0,62×. Gerar agora **somente** [UI-008 LINEUP MINIMAL](prompts/variants/08_MAID_CARD_LINEUP_MINIMAL.md) → `art_working/ui_modular/raw_gpt/UI-008_maid_card_lineup_minimal_v01.png` (**PENDING**).

**Anexos obrigatórios:** `art_working/ui_modular/blender_clean/UI-008_maid_card_compact_v01.png` (CLEAN local aprovado apenas em 150×206; [master publicado](approved/UI-008_maid_card_compact_v01.png)); `docs/roadmap/ui-01b/evidence/after_ES-2_escalacao_960x600.png`; `docs/art/ui-modular/approved/UI-001_panel_large_v01.png`. Opcionais: `Assets/_Game/Resources/Maids/suzu_walk.png` e `docs/art/maid-walk/evidence_illustrated/webgl_chrome_960x600.jpg`. [Tutorial detalhado](TUTORIAL.md#passo-3b--novo-maid-card-minimalista-para-escalação-duas-apresentações-aprovadas).

**UI-009** (frame expandido) continua PENDING e não bloqueia esta etapa. UI-008 atual não é descartado nem renomeado; seu aceite artístico explícito é restrito a 150×206. Nenhuma integração antes do QA/decisão.



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

[Todos os 100 nomes exatos](OUTPUT_MANIFEST.md) · [Tutorial de anexos](TUTORIAL.md) · [Registro de aprovação](ASSET_REGISTRY.md).
