# Registro de produção — assets modulares

**Nenhum asset novo foi gerado ou aprovado nesta infra.** Estado padrão: `PENDING`. Um prompt existente **não** equivale a PNG disponível. Atualize esta tabela somente com arquivos e validações realmente existentes.

| ID | Peça / prompt | Estado | Raw local | PNG limpo local | Versão aprovada / PR | Unity / QA |
| --- | --- | --- | --- | --- | --- | --- |
| UI-001 | [01_PANEL_LARGE](prompts/01_PANEL_LARGE.md) | PENDING | — | — | — | NOT RUN |
| UI-002 | [02_PANEL_MEDIUM](prompts/02_PANEL_MEDIUM.md) | PENDING | — | — | — | NOT RUN |
| UI-003 | [03_PANEL_SMALL_LABEL](prompts/03_PANEL_SMALL_LABEL.md) | PENDING | — | — | — | NOT RUN |
| UI-004 | [04_BUTTON_PRIMARY](prompts/04_BUTTON_PRIMARY.md) | PENDING | — | — | — | NOT RUN |
| UI-005 | [05_BUTTON_SECONDARY](prompts/05_BUTTON_SECONDARY.md) | PENDING | — | — | — | NOT RUN |
| UI-006 | [06_TABS_SET](prompts/06_TABS_SET.md) | PENDING | — | — | — | NOT RUN |
| UI-007 | [07_MODAL_DIALOG](prompts/07_MODAL_DIALOG.md) | PENDING | — | — | — | NOT RUN |
| UI-008 | [08_MAID_CARD_COMPACT](prompts/08_MAID_CARD_COMPACT.md) | PENDING | — | — | — | NOT RUN |
| UI-009 | [09_MAID_CARD_DETAIL](prompts/09_MAID_CARD_DETAIL.md) | PENDING | — | — | — | NOT RUN |
| UI-010 | [10_RARITY_BADGES](prompts/10_RARITY_BADGES.md) | PENDING | — | — | — | NOT RUN |
| UI-011 | [11_ICON_PACK](prompts/11_ICON_PACK.md) | PENDING | — | — | — | NOT RUN |
| UI-012 | [12_MENU_BACKGROUND](prompts/12_MENU_BACKGROUND.md) | PENDING | — | — | — | NOT RUN |
| UI-013 | [13_RESTAURANT_BACKGROUND](prompts/13_RESTAURANT_BACKGROUND.md) | PENDING | — | — | — | NOT RUN |
| UI-014 | [14_RESTAURANT_PROPS_SET](prompts/14_RESTAURANT_PROPS_SET.md) | PENDING | — | — | — | NOT RUN |
| UI-015 | [15_TOP_BAR_HUD](prompts/15_TOP_BAR_HUD.md) | PENDING | — | — | — | NOT RUN |
| UI-016 | [16_BOTTOM_STRIP_DAY_UI](prompts/16_BOTTOM_STRIP_DAY_UI.md) | PENDING | — | — | — | NOT RUN |
| UI-017 | [17_SHOP_ITEM_FRAME](prompts/17_SHOP_ITEM_FRAME.md) | PENDING | — | — | — | NOT RUN |
| UI-018 | [18_BALANCE_ROW_FRAME](prompts/18_BALANCE_ROW_FRAME.md) | PENDING | — | — | — | NOT RUN |
| UI-019 | [19_DIALOG_BALLOON](prompts/19_DIALOG_BALLOON.md) | PENDING | — | — | — | NOT RUN |
| UI-020 | [20_LOGO_WORDMARK](prompts/20_LOGO_WORDMARK.md) | PENDING | — | — | — | NOT RUN |
| UI-021 | [21_MAID_PORTRAIT](prompts/21_MAID_PORTRAIT.md) | PENDING | — | — | — | NOT RUN |
| UI-022 | [22_FLOOR_TILE](prompts/22_FLOOR_TILE.md) | PENDING | — | — | — | NOT RUN |
| UI-023 | [23_WALL_SEGMENT](prompts/23_WALL_SEGMENT.md) | PENDING | — | — | — | NOT RUN |
| UI-024 | [24_FURNITURE_SINGLE](prompts/24_FURNITURE_SINGLE.md) | PENDING | — | — | — | NOT RUN |
| UI-025 | [25_RARITY_CARD_FRAME](prompts/25_RARITY_CARD_FRAME.md) | PENDING | — | — | — | NOT RUN |

## Estados válidos

`PENDING` → `RAW` (imagem salva) → `CLEAN` (alfa/bordas verificados) → `UNITY_QA` (composição testada) → `APPROVED` (decisão do responsável) → `INTEGRATED` (PR/commit com GUID/asset). `REJECTED` ou `REWORK` exigem motivo.

**Um ID pode ter várias variantes.** Use sufixos, por exemplo `UI-004_normal_v01`, `UI-004_hover_v01`; registrar separadamente os arquivos/estados. Para cada imagem aprovada, adicionar linha de detalhes ou ficha:

- Origem: prompt e versão, data, anexos exatos (caminhos/versões).
- Original: caminho local / versão do bruto (não é necessário commitar bruto).
- Arte limpa: caminho, dimensões, alpha real e técnica (Blender etc.), tamanho em bytes e SHA-256.
- Unity: caminho em `Assets/_Game/` se integrado, tipo `Sliced/Simple`, `Pixels Per Unit`, bordas ou camadas, escala.
- QA: screenshot antes/depois 960×600 e 1280×720, interação/scroll/contraste, WebGL e memória quando relevante.
- Decisão: APPROVED/REWORK/REJECTED, motivo e PR.
