# UI-INT-02A — Tela Maids (Elenco) a partir do kit `neko-ui-maids-kit-delivery`

Só `RosterScreen` mudou na UI. Nenhuma mudança em gameplay, economia, RNG, saves, `Neko.Sim`, nem em Hub/Caixinhas/Equipe/Loja/Salão. Pilha sobre a PR #79 (base `task/ui-int-01-hub-boxes`).
Todo texto/número é `Text` do Unity sobre placas em branco. Se faltar sprite (`KitArt.Has`) cai no layout procedural anterior (intacto, `BuildPlain`).

## Funcionalidade
- **Filtro 1 (raridade):** Todas, Comuns, Raras, Épicas, Lendárias, com o total possuído (independe do outro filtro).
- **Filtro 2 (posição natural):** Todas as posições, Recepção, Salão, Cozinha, Palco, Gerência (os cinco `Role`, via `NaturalRole`). Sem contadores (para não sobrepor o Ordenar).
- Os dois combinam (interseção); ativo = dourado; nada altera coleção/perfil/saves; vazio mostra "Nenhuma maid com esses filtros." e detalhe vazio.
- **Ordenar:** botão que alterna Raridade → Nome → Nível; desempate por nome e depois id (`RosterQuery`).
- **Coleção:** `MaidCard.Build` real + retratos reais, 4×2 por página, paginação, realce da selecionada. Trocar filtro volta à 1ª página; seleção mantida por id se ainda visível, senão a primeira visível, senão vazio.
- **Detalhe:** nome, raridade, posição, salário, nível, etiquetas, vestuário, 4 atributos (trilho + preenchimento), traços/flavour/assinatura com rolagem interna, `MaidPortraits.Detail` com fallback.
- **Dispensar** com confirmação e devolução de fichas, como antes. **Não** há Equipar/Subir Nível clicáveis (não existem no jogo).
- Cabeçalho: `maids / RosterCap`, Custo fixo por dia (`Books.FixedCostsFor`), Fichas reais.

## Assets importados (27 PNGs, `Resources/UiKit/`, prefixo `maids_`)
maids_background, maids_title_plate_empty, maids_token_counter_empty, maids_filters_container_empty, maids_filter_row_divider, maids_filter_selected_empty, maids_filter_selected_surface, maids_filter_rarity_normal_empty, maids_filter_position_normal_empty, maids_filter_paw_brown, maids_icon_reception/hall/kitchen, maids_sort_empty, maids_sort_divider, maids_collection_panel_empty, maids_selected_card_highlight, maids_page_left/right, maids_page_plate_empty, maids_detail_panel_empty, maids_portrait_frame_empty, maids_rarity_badge_empty, maids_attribute_track_empty, maids_attribute_fill_gold, maids_traits_frame_empty, maids_dismiss_empty.
Reaproveitados (já existiam): back_blank, bow_gold, paw_gold, paw_cream. Não importados: placas Equipar/Subir Nível, alternativas `*_frame`, superfícies marrons, detail_frame.
Importação: `tools/art/ui_kit/import_maids_kit.py` (confere sha256 com o manifesto, bordas 9-slice do manifesto). Hashes em `SOURCES.json` e `docs/art/SUZU_ASSET_PROVENANCE.md`.

## Validação (medida)
- EditMode 390/390 (11 novos em `RosterQueryTests`). PlayMode 155/155 (22 novos em `RosterScreenTests`: filtros, interseção, vazio, ordem, paginação >8, seleção/detalhe, dispensar confirmar/cancelar, compra de caixa → elenco, fichas/custos reais, fallback de retrato, cliques e legibilidade em 960×600 e 1280×720; `Layout_Check` preservado).
- WebGL: `tools/build-webgl.sh` OK (76 MB). Chrome (janela ~16:9): Menu → Hub → Maids com dados reais, Cozinha, Raras (vazio), Palco, Gerência, Ordenar (Raridade→Nome), Todas; sem erros no console. 960×600 foi verificado nos testes PlayMode (frames em `evidence/`), não em janela de navegador.
- Evidências: `evidence/roster_*_{1280x720,960x600}.png` (frames do Unity) e `evidence/webgl_chrome_maids.jpg`.

## Limitações
- Ordenar é um botão de ciclo, não um dropdown.
- Palco/Gerência usam a pata marrom (o kit só traz ícones de Recepção/Salão/Cozinha).
- Sem contadores no filtro de posição.
- Traços longos rolam dentro do painel.
- Fonte legada do Unity (decisão do dono). Fixtures de teste excedem o RosterCap.
- Detalhe padrão = primeira maid do perfil (contrato existente dos testes de retrato).
- Comparação lado a lado com o mockup não foi gerada neste PR; os frames acima são a base para ela.
