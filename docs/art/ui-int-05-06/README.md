# UI-INT-05/06 — Salão, Expediente e Resultado a partir dos kits aprovados

Fontes: `neko-ui-hall-kit-delivery/` (UI-VISUAL-05) e `neko-ui-shift-results-kit-delivery/` (UI-VISUAL-06). Só `HallScreen`, `DayHud` e `BooksScreen` mudam na UI (mais os helpers `RoomWindow`/`ContainStage`, `CameraRig.FrameRoomInWindow`, o filtro de cliques em `GameRoot.OnClick`/`OnRightClick` e textos novos em `Strings`). Nenhuma mudança em `Neko.Sim`, economia, RNG, saves, progressão, catálogos de móveis ou mecânicas do expediente. `BustScreen` e telas anteriores intactas. **Empilhada** na PR #81 (Equipe/Lojinha), sobre #80 (Maids) e #79 (Hub + Caixinhas).

Todo texto e número é `Text` do Unity sobre placas em branco. Se faltar sprite (`KitArt.Has`), cada tela cai no layout procedural anterior, intacto (`ForcePlain` nos testes).

## O salão real dentro da arte (decisão técnica)
O jogo tem **uma câmera** atrás do canvas (o `ResolutionRig` do Layout_Check exige isso). O salão é o mundo vivo, não uma imagem:
- **Salão (Hall):** o fundo do escritório é opaco; o mesmo PNG é cortado por coordenadas de textura (`RawImage.uvRect`) em quatro faixas ao redor da abertura, então nada é redesenhado nem editado. A prancha tem a abertura alpha real. A câmera limpa para a cor do papel da prancha.
- **Expediente (DayHud):** o cenário usa a variante `shift_restaurant_background_open` (abertura alpha real) e a moldura dourada por cima.
- `RoomWindow` marca o retângulo (viewport seguro: 690×494 no Hall, abertura do expediente) em pixels da câmera. `CameraRig.FrameRoomInWindow` enquadra o salão inteiro (e a cabeça de uma maid na fileira de cima) nesse retângulo com escala uniforme e centralizado: células quadradas, sem esticar. Reenquadra quando a janela, a resolução ou a escala do canvas mudam.
- **Cliques:** `GameRoot.OnClick`/`OnRightClick` só aceitam o clique se `RoomWindow.Contains(ponto)`. Cliques no cenário, na moldura, no HUD ou na bandeja nunca viram colocação/remoção/toque no salão. Toda arte decorativa tem `raycastTarget=false` (o `NekoInput` já descarta cliques sobre UI com raycast). O viewport e a conversão ScreenToWorld são a mesma câmera, então célula clicada = célula desenhada.
- Letterbox: cor sólida só nas bordas (quatro barras fora do palco, nunca sobre ele), para não esconder a abertura.

## Salão (Arrumar o salão)
Escritório, prancha, título, plaquinha de métricas (Lugares, Ambiente, Caminhada, Tema — mesmos textos de `Grid.Metrics()`), instrução (a linha de `_picked` com a mesma lógica: nome, "R gira", sim/não, "Não cabe aí."; a faixa alarga para frases longas), bandeja de estoque com o móvel real (`UiKit.Sprites.Furniture`), contagem agrupada, célula selecionada marcada, rolagem horizontal. Estoque vazio: "No estoque: 0 — Lojinha" sobre a placa do kit. **Não há botão "Loja"**: o Hall não tem callback para ir à loja; a arte `hall_shop_access_empty` não foi importada.

## Expediente
Cenário aberto + moldura; HUD superior com relógio, caixa, atendidos/erros/saíram (reais), velocidades 1×/2×/4× e pular como áreas de clique transparentes sobre a placa (a velocidade escolhida ganha a placa dourada), dica com tint urgente, combo/gorjeta, "Fechando!". Uma ficha por maid da escalação (variável): nome, posição, estado (Em serviço / Atendendo / A caminho / Na pausa) e barra de humor real (`MoodChip`); centradas enquanto cabem, rolagem horizontal se não couberem.

## Resultado do expediente
Fundo, relatório, colunas Entrou/Saiu (linhas de `Incoming()`/`Outgoing()`, Receita/Despesa), química e satisfação (ou `FailureReason`), indicadores adaptativos (5 + um por level-up; uma linha, duas linhas de sete, rolagem a partir da terceira), faixa do resultado financeiro (`Strings.Signed(Profit)`, "Caixa depois" e aviso), "Voltar à empresa" → `OnContinue`. Linhas além do espaço apertam e depois rolam; o total nunca se move; valores monetários encolhem (best fit) mas nunca são cortados.

## Assets importados (34 PNGs, `Resources/UiKit/`)
- **Salão (15, `hall_`):** office_background, board_frame, title_empty, metrics_plate_empty, metrics_divider, icon_seat/ambience/flow/theme, instruction_strip_empty, inventory_tray_empty, inventory_cell_empty, inventory_cell_selected, inventory_paw, stock_label_empty.
- **Expediente (9, `shift_`):** restaurant_background_open, room_frame, top_hud_empty, maid_chip_empty, mood_track, mood_fill, speed_selected, instruction_empty.
- **Resultado (10, `results_`):** restaurant_background, report_empty, column_empty, header_empty, total_empty, tally_empty, financial_highlight_empty, continue_empty, row_empty, separator.
- Reaproveitados: `back_blank`. Não importados: alternativas frame/surface, fundo neutro do expediente, cantos soltos, suporte neutro do viewport (a câmera limpa para a cor do papel), anel de retrato (a ficha não tem retrato), placa de velocidade normal (já vem na placa do HUD), `hall_shop_access_empty` (sem callback).
- `tools/art/ui_kit/import_hall_shift_results_kits.py` confere o sha256 com o manifesto, copia byte a byte e grava as bordas 9-slice; hashes em `SOURCES.json` e `docs/art/SUZU_ASSET_PROVENANCE.md`.

## Validação
Ver o corpo da PR para as contagens finais. Testes novos em `HallShiftResultsScreenTests`. Evidência em `evidence/` (frames reais de Unity em 1280×720 e 960×600 e da build WebGL no Chrome).

## Limitações
- A comparação lado a lado com os mockups é visual (frames reais em `evidence/`); o salão real tem seu próprio visual (tabuleiro, paredes), que aparece dentro da abertura.
- No Hall o viewport interativo é o retângulo seguro do kit (690×494); a faixa fina do polígono da prancha fora dele mostra só o papel.
- Fonte legada do Unity (decisão do dono). Em 960×600 os textos pequenos das fichas e das células de estoque ficam entre 9 e 11 px efetivos.
- A velocidade escolhida é marcada por um estado de apresentação do HUD (a simulação só recebe a escala, como antes).

## Limitações conhecidas (observadas na validação no Chrome)
- O texto flutuante "[shift.full]" no salão durante o dia vem de uma chave ausente em `Strings` (`ShiftPresenter`, anterior a esta tarefa); não foi alterada aqui.
- Na tela de resultado, chips de level-up muito longos ("<nome> Subiu de nível!") podem ser cortados nos chips pequenos quando há 7+ itens.
- Colocar móvel na Chrome não foi exercitado (estoque 0 nessa sessão); colocar/girar/remover estão cobertos pelos testes PlayMode com cliques reais.
