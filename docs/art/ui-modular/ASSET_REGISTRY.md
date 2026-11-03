# Registro de produção — assets modulares

**Assets aprovados: UI-001 e UI-004 COMPACT NORMAL v01 (`APPROVED`, ambos ainda não `INTEGRATED`). O PNG do compacto permanece local; somente o UI-001 está publicado como asset aprovado.** **[OUTPUT_MANIFEST.md](OUTPUT_MANIFEST.md) lista individualmente as 99 saídas com o nome exato e o prompt específico; esta tabela agrupa as 25 famílias.** Estado padrão: `PENDING`. Um prompt existente **não** equivale a PNG disponível. Atualize esta tabela somente com arquivos e validações realmente existentes.

| ID | Peça / prompt | Estado | Raw local | PNG limpo local | Versão aprovada / PR | Unity / QA |
| --- | --- | --- | --- | --- | --- | --- |
| UI-001 | [01_PANEL_LARGE](prompts/01_PANEL_LARGE.md) | APPROVED | `raw_gpt/UI-001_panel_large_v01.png` (local, ignorado) | `blender_clean/UI-001_panel_large_v01.png` (local, ignorado) | [`approved/UI-001_panel_large_v01.png`](approved/UI-001_panel_large_v01.png) — PR #39 | UNITY_QA PASS 960×600 e 1280×720; **INTEGRATED: pendente**; WebGL/compressão NOT RUN — [revisão](reviews/UI-001_PANEL_LARGE_V01.md) |
| UI-002 | [02_PANEL_MEDIUM](prompts/02_PANEL_MEDIUM.md) | CLEAN | `raw_gpt/UI-002_panel_medium_v01.png` (local ignorado) | `blender_clean/UI-002_panel_medium_v01.png` (1356×802, local ignorado) | Não aprovado | Auditoria PNG: painel/creme preservados; pronto para QA posterior; Unity/WebGL NOT RUN — [lote](reviews/UI_BATCH_002_003_008_COMPACT_STATES_V01.md) |
| UI-003 | [03_PANEL_SMALL_LABEL](prompts/03_PANEL_SMALL_LABEL.md) | CLEAN | `raw_gpt/UI-003_panel_small_label_v01.png` (local ignorado) | `blender_clean/UI-003_panel_small_label_v01.png` (2170×526, local ignorado) | Não aprovado | Auditoria PNG: label; recorte e padding direito transparente; pronto para QA posterior; Unity/WebGL NOT RUN — [lote](reviews/UI_BATCH_002_003_008_COMPACT_STATES_V01.md) |
| UI-004 | [04_BUTTON_PRIMARY](prompts/04_BUTTON_PRIMARY.md) | **REWORK (NORMAL v01)** | `raw_gpt/UI-004_button_primary_normal_v01.png` (2172×724, local ignorado) | `blender_clean/UI-004_button_primary_normal_v01.png` (local ignorado; RGB preservado) | Não aprovado; demais estados PENDING | Unity 960×600 e 1280×720 executado: **FAIL** leitura em tamanhos pequenos/120×44; interação simulada PASS; slice viável; candidata 768×256 para 260×88; [revisão](reviews/UI-004_BUTTON_PRIMARY_NORMAL_V01.md) |
| UI-004 COMPACT NORMAL | [04_BUTTON_PRIMARY_COMPACT_NORMAL](prompts/variants/04_BUTTON_PRIMARY_COMPACT_NORMAL.md) | **APPROVED (não INTEGRATED)** | `raw_gpt/UI-004_button_primary_compact_normal_v01.png` (2172×724, local ignorado) | `blender_clean/UI-004_button_primary_compact_normal_v01.png` (2072×380, local ignorado) | Aprovação do responsável — PR #42; PNG local, não integrado | Unity Editor **PASS** 150×44/120×44/230×54 em 960×600 e 1280×720; preços reais, disponível/indisponível; [QA e capturas](reviews/UI-004_BUTTON_PRIMARY_COMPACT_NORMAL_V01.md) |
| UI-005 | [05_BUTTON_SECONDARY](prompts/05_BUTTON_SECONDARY.md) | PENDING | — | — | — | NOT RUN |
| UI-006 | [06_TABS_SET](prompts/06_TABS_SET.md) | PENDING | — | — | — | NOT RUN |
| UI-007 | [07_MODAL_DIALOG](prompts/07_MODAL_DIALOG.md) | PENDING | — | — | — | NOT RUN |
| UI-008 | [08_MAID_CARD_COMPACT](prompts/08_MAID_CARD_COMPACT.md) | CLEAN | `raw_gpt/UI-008_maid_card_compact_v01.png` (local ignorado) | `blender_clean/UI-008_maid_card_compact_v01.png` (1070×1470, local ignorado) | Não aprovado | Auditoria PNG: portrait seguro alpha0; nome/stats sólidos; pronto para QA posterior; Unity/WebGL NOT RUN — [lote](reviews/UI_BATCH_002_003_008_COMPACT_STATES_V01.md) |
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

**Um ID pode ter várias variantes.** Os sufixos canônicos estão no [manifesto de saídas](OUTPUT_MANIFEST.md), por exemplo `UI-004_button_primary_normal_v01.png` e `UI-004_button_primary_hover_v01.png`. Registrar separadamente os arquivos/estados, sem inventar nomes. Para cada imagem aprovada, adicionar linha de detalhes ou ficha:

- Origem: prompt e versão, data, anexos exatos (caminhos/versões).
- Original: caminho local / versão do bruto (não é necessário commitar bruto).
- Arte limpa: caminho, dimensões, alpha real e técnica (Blender etc.), tamanho em bytes e SHA-256.
- Unity: caminho em `Assets/_Game/` se integrado, tipo `Sliced/Simple`, `Pixels Per Unit`, bordas ou camadas, escala.
- QA: screenshot antes/depois 960×600 e 1280×720, interação/scroll/contraste, WebGL e memória quando relevante.
- Decisão: APPROVED/REWORK/REJECTED, motivo e PR.

## UI-001 `panel_large` v01 — ficha

- Origem: [01_PANEL_LARGE](prompts/01_PANEL_LARGE.md), GPT; bruto local `raw_gpt/UI-001_panel_large_v01.png` (SHA-256 `d5631b7f…7f22a`, intacto, ignorado).
- Arte limpa/aprovada: `docs/art/ui-modular/approved/UI-001_panel_large_v01.png`, 1536×1024 RGBA, 1 833 033 bytes, SHA-256 `ba300ece650f18e8ef4e938aa84e9c9df3146396dc82a91ba4551306a9ee85f8` (idêntico ao `blender_clean/`). Técnica: processamento programático de alpha; sem trim, resize, quantização ou compressão.
- Unity (testado, não integrado): Sprite Single, Full Rect, **border L280 B280 R280 T250**, PPU 100 + `Image.pixelsPerUnitMultiplier` 2 (escala 0,5), Uncompressed, sem mipmaps, Clamp, Bilinear.
- QA e limites de uso: [revisão](reviews/UI-001_PANEL_LARGE_V01.md#6-aprovação-final).
- Decisão: **APPROVED** pelo responsável; **INTEGRATED** pendente (nenhuma tela alterada).

## UI-004 `button_primary_normal` v01 — QA pendente de aceite

- **REWORK**, não APPROVED nem INTEGRATED. Direção visual geral aceita; tamanho real ainda não homologado. HOVER/PRESSED/DISABLED permanecem PENDING, não produzidos.
- Bruto intacto: **2172×724 RGBA**, SHA-256 `91466ecc77f6bb1d6b30540883961c5b1d0babbce7e3d9b83c9aef059e98f2bf`. A informação preliminar 2048×682 não se confirmou para este arquivo.
- Limpo local: **2172×724**, SHA-256 `03b58c3e2b500b8a48311cd3b3c75c8551c79d599693176ad5b5da664aeb0446`; removeu 1.094 pixels de resíduos separados e solidificou alpha≥250; RGB integralmente preservado.
- Slice testado: borders de origem L400/B360/R400/T360, escala vertical ajustada à altura; ornamentos inteiros, porém pouco legíveis em 44–54 u. Texto Comprar 16 falha em 120×44 a 1280×720.
- Proposta condicional: candidata **768×256**, 0,75 MiB RGBA32, caso maior **260×88**; não autoriza mudar telas. Texturas do botão locais/ignoradas; duas capturas Unity versionadas, hashes e restrições na [revisão](reviews/UI-004_BUTTON_PRIMARY_NORMAL_V01.md).
- **Decisão de produto posterior APROVADA em 03/11/2026:** duas famílias — ornamental grande (~260×88) e compacto simplificado (altura 44–54); abas seguem como componentes próprios. Isto não aprova o NORMAL ornamental nem sua candidata 768×256/WebGL. O compacto tem candidato local e QA próprio abaixo, sem integração definitiva.
- Pendências: responsável decidir rework/uso maior restrito, limpeza, resolução e publicação das texturas; compressão, WebGL e integração do ornamental NOT RUN.

## UI-004 `button_primary_compact_normal` v01 — aprovado pelo responsável

- **Estado: APPROVED pelo responsável em 03/11/2026; não INTEGRATED.** Aprovação explícita do NORMAL compacto, recorte 2072×380 e configuração 9-slice 768×141 testada na PR #42 — [decisão e limites](reviews/UI-004_BUTTON_PRIMARY_COMPACT_NORMAL_V01.md#9-aprovação-do-responsável). QA e evidências preservados, sem repetição.
- Bruto existente, preservado e ignorado: `art_working/ui_modular/raw_gpt/UI-004_button_primary_compact_normal_v01.png`, **2172×724 RGBA**, 1.093.825 bytes, SHA-256 `72deadbfffbda54e1a6fc0a91a192916a42c7db91a9bc3cb769d9a80676e64b0`. A preliminar 2048×682 não se confirmou.
- Limpo local/ignorado: `blender_clean/UI-004_button_primary_compact_normal_v01.png`, **2072×380**, 908.246 bytes, SHA-256 `c82d06142b3953a97b9654d8b7bf53e611679a6ffb4822a3b268dff239c17eb6`; crop [52,171,2124,551), removeu 1.950 px desconectados alpha≤3 e solidificou alpha≥250; RGB retido integralmente, sombra/AA conectados preservados.
- **Configuração aprovada:** 9-slice Full Rect, borders master L160/B110/R160/T110; export 768×141 (0,413 MiB RGBA32 estimado) após recorte, borders≈59,305/40,816/59,305/40,816; PPU 100, multiplier141/altura (3,204545 em 44; 2,611111 em 54), Bilinear/Clamp, sem mipmaps/compressão, Image branca. Texto 18 Loja/navegação, 24 diálogo; cor escura disponível/creme indisponível. Aprovada sob os casos e limites medidos; integração definitiva pendente.
- QA Editor: componentes reais isolados, preços de catálogo **0/60/900 ¥** via Strings.Money; 49 rótulos varridos; 150×44,120×44,230×54 nas duas resoluções; 288 sequências down/up/click simuladas (48 bloqueadas), 144 pontos externos rejeitados. Compra/refusa via Shop.Buy real em perfil descartável. **Não mouse físico ou QA da Loja completa.** Capturas e reprodução na revisão.
- **WebGL isolado PASS** 960×600/1280×720, Chromium 153 headless/SwiftShader, 40 callbacks e 8 tentativas desabilitadas bloqueadas via mouse automatizado do navegador, não mouse físico. Warning FSR software documentado. Build/harness removidos; compressão de bloco, memória GPU residente e integração final NOT RUN. Nenhum PNG bruto/limpo/export publicado como asset aprovado.
- **Pendências:** integração própria, testes de produção e itens NOT RUN documentados. RAW, CLEAN e evidências preservados; nenhum PNG integrado nesta atualização. HOVER/PRESSED/DISABLED compactos têm auditoria própria abaixo e permanecem sem aprovação. O [prompt](prompts/variants/04_BUTTON_PRIMARY_COMPACT_NORMAL.md) documenta a origem; geração manual informada pelo responsável, anexos efetivamente usados não inferidos do PNG.
- **Famílias separadas:** ornamental antigo permanece **REWORK**; UI-001 mantém seu histórico. Abas são componentes próprios.

## UI-004 COMPACT — estados HOVER, PRESSED e DISABLED (auditoria de PNG)

- **RAWs v01 locais existentes**, todos 2048×375 RGBA8 e ignorados. O HOVER v02 foi auditado em adendo próprio abaixo. Auditoria estática concluída; nenhum estado aprovado ou integrado. [Revisão do lote](reviews/UI_BATCH_002_003_008_COMPACT_STATES_V01.md) contém os hashes integrais, métricas e composições.
- **HOVER v01: REWORK**, por realce pouco perceptível em 120/150×44 no parecer raster estático; alpha já limpo, sem CLEAN duplicado. Rever intensidade com o responsável, sem recoloração/regeneração nesta tarefa.
- **HOVER v02: CLEAN**, pronto para QA Unity direcionado; realce mais perceptível em 120×44/150×44 na comparação estática. RAW 2172×724 preservado, SHA-256 `4beaadaf15870747b92f1bd8ea323d442e0b7e083d9ca8cba4e1730369ac6e6a`; CLEAN local 2176×408, SHA-256 `c47a56f4f1a83c12eb9a64644a38a9dba66a55cefbb2d15e7913f3f88000d45e`. Removidos 985 pixels residuais, alpha250–254 solidificado; crop [0,157,2172,565) +padding L2/R2, sem resize/RGB alterado. Corpo difere≈−1,49% em proporção; simulação mostra 1px extra na largura visível e centro +0,37px. Exige export/import comum e conferência de alinhamento real; **não APPROVED/INTEGRATED**, Unity/WebGL/builds NOT RUN — [adendo](reviews/UI_BATCH_002_003_008_COMPACT_STATES_V01.md#7-adendo--ui-004-compact-hover-v02).
- **PRESSED v01: CLEAN**, RAW já tecnicamente limpo; escurecimento coerente sem deslocar a peça. Pronto para preparar QA Unity posterior, com export/import comum ao NORMAL.
- **DISABLED v01: CLEAN**, RAW já tecnicamente limpo; contorno moderadamente dessaturado e creme opaco. Pronto para QA posterior de percepção, texto e tint nativo combinados.
- Arquivos: `raw_gpt/UI-004_button_primary_compact_hover_v01.png`, `raw_gpt/UI-004_button_primary_compact_pressed_v01.png`, `raw_gpt/UI-004_button_primary_compact_disabled_v01.png`. **Nenhum CLEAN novo necessário nos três estados v01; somente o v02 recebeu limpeza.**
- **Referência geométrica exclusiva:** COMPACT NORMAL limpo/aprovado 2072×380, SHA-256 `c82d06142b3953a97b9654d8b7bf53e611679a6ffb4822a3b268dff239c17eb6`, preservado. Máscaras dos três estados v01 idênticas entre si; IoU 99,9712% contra o NORMAL normalizado apenas em memória, contorno difere no máximo 1px. Não há igualdade literal de canvas; não usar como troca direta de master sem preparar metadados/exports.
- 9-slice proporcional 2048×375: L158,1467/B108,5526/R158,1467/T108,5526. Export/import comum com o NORMAL deve ser preparado/verificado na etapa Unity; **não executado aqui**. Não usar o ornamental como referência.
- Prompts: [HOVER](prompts/variants/04_BUTTON_PRIMARY_COMPACT_HOVER.md), [PRESSED](prompts/variants/04_BUTTON_PRIMARY_COMPACT_PRESSED.md), [DISABLED](prompts/variants/04_BUTTON_PRIMARY_COMPACT_DISABLED.md). Origem/anexos efetivamente usados não inferidos dos pixels.
- Unity/WebGL/builds/QA anteriores **NOT RUN por escopo deste lote**. NORMAL aprovado, ornamental REWORK e UI-001 preservados; controles/estados funcionais continuam responsabilidade Unity.

## Lote UI-002/UI-003/UI-008 — limpeza concluída, sem aprovação

- **UI-002 CLEAN:** RAW 1448×1086 →1356×802, crop [43,142,1399,944), 2.934 px residuais removidos e alpha250–254 solidificado; RGB retido preservado. CLEAN SHA-256 `00c44a34dca15be2cb61d994f05e32ec52775451bf80227bde5faa072683b2c9`.
- **UI-003 CLEAN:** RAW 2172×724 →2170×526, crop [4,92,2172,618) e 2px transparentes extras à direita; 735 px residuais removidos e alpha250–254 solidificado; RGB retido preservado. CLEAN SHA-256 `0a79225c60c61541f34c1d34f3d45f7e0685917d5d9c82cd19b9fec534b31a27`.
- **UI-008 CLEAN:** 1070×1470 preservado, sem recorte/resize; 99.986 px residuais removidos e alpha250–254 solidificado. Portrait seguro totalmente transparente e superfícies de nome/stats sólidas. CLEAN SHA-256 `74ea40f0b528580b4f809bd20a359bd1dbdacb02e4014f0d8ce0576676fb6342`.
- RAW/CLEAN locais/ignorados. Campos creme, RGB, desenho e AA/sombra próximos foram preservados. Arquivos/transformações completos na [revisão](reviews/UI_BATCH_002_003_008_COMPACT_STATES_V01.md); aprovações artísticas, slice e legibilidade em Unity continuam pendentes.
