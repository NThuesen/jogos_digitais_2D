# UI-INT-03B — Equipe do Dia e Lojinha a partir dos kits aprovados

Fontes: `neko-ui-lineup-kit-delivery/` (UI-VISUAL-03) e `neko-ui-shop-kit-delivery/` (UI-VISUAL-04). Só `LineupScreen` e `ShopScreen` mudam na UI (mais o helper `KitStage` e textos novos em `Strings`). Nenhuma mudança em gameplay, economia, RNG, preços, saves, progressão, regras de compra ou de escalação, nem em `Neko.Sim`. **Empilhada** na PR #80 (Maids), que está sobre a #79 (Hub + Caixinhas).

Todo texto e número é `Text` do Unity sobre placas em branco. Se faltar sprite (`KitArt.Has`), cada tela cai no layout procedural anterior, intacto (`BuildPlain`).

## Equipe do Dia
- Fundo, título (sem indicador de dia), contador de fichas, Voltar.
- **Equipe escalada (N / C):** grupos por posição lidos de `Lineup.Slots.For(role)` (só posições com vagas), placa da posição, cartas reais (`MaidCard.Build`, escala 0,62 como sempre) e vagas vazias (moldura + pata). Clique numa carta tira a maid; clique numa vaga espera a próxima maid do banco, como antes. Um café com mais vagas do que cabem numa tela rola na horizontal.
- **Química da equipe:** pontuação, barra (preenchimento recortado pelo valor real, vai a zero), veredito e multiplicador de `Profile.Chemistry()`; **Custo fixo por dia** de `Books.FixedCostsFor`.
- **Banco de maids (N):** quem não está escalado, com filtro de raridade (Todas → Comuns → Raras → Épicas → Lendárias), ordem (Raridade → Nome → Nível; `RosterQuery`, só apresentação), paginação e "p / n". Vazio e filtro sem resultado têm texto.
- **Escalar sozinho** e **Limpar** chamam `Lineup.AutoFill` / `Lineup.Clear`; o aviso de `Session.WhyNotReady()` aparece ao lado.

## Lojinha
- Um layout para Móveis, Roupas e Presentes: só a lista muda (nome, descrição, preço, ícone, "No estoque: N"). Lê `Shop.StockFor` e filtra por categoria; dinheiro (¥) de `Wallet.Money`.
- Linha normal; linha dourada ao passar o mouse (decoração, sem mover nada); botão de compra próprio ligado direto a `Buy(item)` (sem seleção prévia), desabilitado e esmaecido quando não há saldo.
- Presentes: pergunta "Para quem?" e só então paga (fluxo existente). Lista com rolagem dentro do viewport, mantendo a posição após comprar.
- Ícones: o móvel real (`UiKit.Sprites.Furniture`) ou os selos já existentes; nenhum recorte do mockup. Sem preview de roupa/presente (não existe no jogo).

## Assets importados (34 PNGs, `Resources/UiKit/`)
- **Equipe (22, prefixo `lineup_`):** background, title_empty, team_panel_empty, team_banner_empty, role_plate_empty, vacant_slot_frame, vacant_paw_plus, chemistry_panel_empty, chemistry_header_empty, chemistry_value_panel_empty, chemistry_track_empty, chemistry_fill_pink, daily_cost_panel_empty, bank_panel_empty, bank_banner_empty, bank_filter_empty, bank_sort_empty, bank_page_empty, bank_left, bank_right, button_auto_empty, button_clear_empty.
- **Lojinha (12, prefixo `shop_`):** background, title_empty, money_panel_empty, tab_normal_empty, tab_selected_empty, tab_underline, main_panel_empty, panel_top_bow, bottom_paw_ornament, product_row_normal, product_row_highlight, buy_empty.
- Reaproveitados (já no projeto): back_blank, bow_gold, paw_gold, paw_cream, maids_token_counter_empty, icon_coin, logo.
- Não importados: alternativas `*_frame`/`*_surface`, linhas `*_icon_window`, clipboard avulso, vaga pronta, moldura externa da química.
- `tools/art/ui_kit/import_lineup_shop_kits.py` confere o sha256 com o manifesto, copia byte a byte e grava as bordas 9-slice do manifesto; hashes em `SOURCES.json` e `docs/art/SUZU_ASSET_PROVENANCE.md`.

## Validação (medida)
Ver o corpo da PR para a contagem final. Testes novos em `LineupShopScreenTests` (20): vagas reais por nível, números reais, vagas vazias, banco → posição, vaga espera a maid, Escalar sozinho/Limpar, filtro/ordem/paginação do banco, vazio, barra de química, Voltar, fallback; Lojinha com o mesmo layout nas 3 categorias, estoque real, compra por clique real, saldo insuficiente, presente com confirmação, rolagem, hover, Voltar, fallback. Clique real pelo EventSystem e legibilidade medida (`KitLegibility`) em 960×600 e 1280×720.

## Limitações
- A rolagem horizontal da equipe aparece nos cafés com mais de ~6 vagas (nível 6 em diante): as cartas do tabuleiro continuam em 0,62 (contrato dos testes de retrato e legibilidade do texto da carta).
- Filtro e ordem do banco são botões de ciclo, não dropdowns (como na tela Maids).
- Texto das cartas dentro do tabuleiro é o da `MaidCard` (compartilhada); `KitLegibility` não o cobra nos testes novos (o `Layout_Check` ES-1..ES-4 o cobra e passa).
- Fonte legada do Unity (decisão do dono).
- Comparação lado a lado com os mockups não foi gerada; os frames reais estão em `evidence/`.
