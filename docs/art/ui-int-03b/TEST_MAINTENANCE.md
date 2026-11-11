# UI-INT-03B — arquivos protegidos tocados

Nenhum workflow, guard (`protected-change-guard`), runner ou proteção de branch foi alterado; nenhum teste foi removido ou enfraquecido; `LayoutCheckTests`, `LayoutRules`, `AuditStates` e `KitLegibility` estão intactos. Os estados do Layout_Check `ES-1`..`ES-4` (Escalação) passam como estão.

| Arquivo | Natureza |
|---|---|
| `Assets/_Game/Tests/EditMode/Game/DocsTests.cs` | +34 entradas na allowlist fechada de proveniência (só os PNGs `lineup_*` e `shop_*` importados; hashes em `docs/art/SUZU_ASSET_PROVENANCE.md`) |
| `Assets/_Game/Tests/PlayMode/UiArtIntegrationTests.cs` | `ShopKeepsBuyingTabsAndTheDisabledState` atualizado para os controles da nova Lojinha (ver abaixo) |
| `Assets/_Game/Tests/PlayMode/LineupShopScreenTests.cs` | arquivo novo (20 testes) |

## O que mudou em `ShopKeepsBuyingTabsAndTheDisabledState` (e por quê)
O teste procurava os botões da Lojinha antiga pelos nomes `ArtTab`/`ArtButton` e pelos sprites `tab_active`, `tab_inactive`, `btn1_hover`, `btn1_pressed` (SpriteSwap). A Lojinha nova usa o kit: abas `Tab_Furniture|Tab_Clothes|Tab_Gifts`, compra `Buy`, voltar `Back`, e o kit traz uma imagem por botão (estados = *tint*, como no Hub e nas Caixinhas).
- Preservado: três abas, exatamente uma selecionada e que acompanha o clique; compra viva × desabilitada (`interactable`, texto escurecido); comprar gasta dinheiro e entrega o item; o botão desabilitado não gasta; Voltar chama `OnBack`.
- Trocado: sprites de aba (agora `shop_tab_selected_empty` × `shop_tab_normal_empty`); SpriteSwap → `ColorTint` com cores de hover/pressed/disabled diferentes da normal.
- A Lojinha antiga continua coberta pelo fallback (`WithoutTheKitTheEarlierShopStillSells`).

## Contrato preservado nos testes de retrato
Os testes de retrato (`PremiumPortraitsTests`, `MaidPortraits*QaTests`, `NewGameIllustratedTests`) localizam as cartas da escalação por escala 0,62: as cartas do tabuleiro continuam em 0,62 (um café com mais vagas do que cabem rola na horizontal).
