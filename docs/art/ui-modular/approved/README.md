# UI modular — aprovações artísticas de 04/11/2026

O responsável aprovou explicitamente HOVER v02, PRESSED v01, DISABLED v02, UI-002 e UI-003 (com as restrições do QA). Aceitou UI-008 **somente para apresentação detalhada em 150×206**; o uso em 0,62× na Escalação continua **REWORK / não aprovado**. LINEUP MINIMAL permanece **PENDING**, não gerada, QA NOT RUN.

**APPROVED não significa INTEGRATED.** Estes arquivos ficam fora de `Assets/`; nenhuma tela, cena, GUID, importer ou regra de jogo foi alterado. A aprovação visual é a decisão expressa do responsável; o PASS técnico é o QA existente da [PR #44](https://github.com/NThuesen/jogos_digitais_2D/pull/44), não repetido. Histórico e limitações no [relatório](../reviews/UI_BATCH_002_003_008_COMPACT_STATES_V01.md#13-aprovação-artística-expressa-do-responsável).

## Arquivos publicados e proveniência

Os masters abaixo são cópias byte a byte dos CLEANs locais; PRESSED v01 usa o RAW já tecnicamente limpo, sem inventar outro CLEAN. `exports/` contém cópias byte a byte dos exports finais do QA; **nenhum resize, recoloração ou processamento novo**. RAWs, CLEANs, exports locais e versões antigas foram preservados. Não foram publicados os v01 REWORK de HOVER/DISABLED nem o ornamental REWORK.

O [manifesto verificável](UI_MODULAR_SIX_APPROVAL_MANIFEST.json) registra SHA-256 integral, bytes, dimensões e origem RAW/master/export de cada arquivo, além de limites e configurações. O [manifesto final do QA](../reviews/evidence/BATCH-UI-002-003-008-004-STATES/FOLLOWUP/final_export_manifest.json) é a referência dos hashes de export; não confundir DISABLED v02 com `disabled_v01.png`.

| Master aprovado | Dimensões master | Export testado | Dimensões export |
| --- | --- | --- | --- |
| [UI-004_button_primary_compact_hover_v02.png](UI-004_button_primary_compact_hover_v02.png) | 2176×408 | [Export](exports/UI-004_button_primary_compact_hover_v02.png) | 768×141 |
| [UI-004_button_primary_compact_pressed_v01.png](UI-004_button_primary_compact_pressed_v01.png) | 2048×375 | [Export](exports/UI-004_button_primary_compact_pressed_v01.png) | 768×141 |
| [UI-004_button_primary_compact_disabled_v02.png](UI-004_button_primary_compact_disabled_v02.png) | 2176×410 | [Export](exports/UI-004_button_primary_compact_disabled_v02.png) | 768×141 |
| [UI-002_panel_medium_v01.png](UI-002_panel_medium_v01.png) | 1356×802 | [Export](exports/UI-002_panel_medium_v01.png) | 678×401 |
| [UI-003_panel_small_label_v01.png](UI-003_panel_small_label_v01.png) | 2170×526 | [Export](exports/UI-003_panel_small_label_v01.png) | 1085×263 |
| [UI-008_maid_card_compact_v01.png](UI-008_maid_card_compact_v01.png) | 1070×1470 | [Export](exports/UI-008_maid_card_compact_v01.png) | 535×735 |

## Configurações Unity já validadas — somente os exports

Todos: Sprite Single, Full Rect, PPU 100, Bilinear, Clamp, sem mipmaps, Uncompressed, alphaIsTransparency, NPOT None, maxTextureSize 4096 e Read/Write desativado. Esses valores vêm do [setup arquivado](../reviews/evidence/BATCH-UI-002-003-008-004-STATES/FOLLOWUP/harness/Editor/ModularQaSetup.cs.txt); não foram aplicados a importers de produção nesta tarefa. Não aplicar os borders de export ao master de dimensões diferentes.

| Export | Borders L/B/R/T (px) | Image / multiplier | Uso aprovado e restrições |
| --- | --- | --- | --- |
| HOVER v02 / PRESSED v01 / DISABLED v02 | 59,305019305 / 40,815789474 / 59,305019305 / 40,815789474 | Sliced; 141/altura | 120×44, 150×44, 230×54; texto 18/24 escuro; SpriteSwap sem ColorTint adicional; hitbox fixa e arte filha. NORMAL já aprovado permanece local; seu export não é republicado por esta tarefa. |
| UI-002 | 80 / 75 / 80 / 75 | Sliced; 1,5 | 330×170, 380×220, 330×270; margem de conteúdo ≥45u nos casos testados; fonte 18/20. |
| UI-003 | 160 / 50 / 160 / 50 | Sliced; 263/altura | Fonte 18: 180×44 com “Voltar”; 260×54 com “Escalação de hoje”; 360×64 com “Química”. Texto longo exige largura maior; preferir 54–64u quando o detalhe ornamental importar. Margem horizontal 160×altura/263+6u. |
| UI-008 | 0 / 0 / 0 / 0 | Simple, preserveAspect; escala uniforme | **Apenas 150×206 detalhado.** Portrait separado sob moldura, RectMask2D; janela segura no master [145,185,925,795). **Não usar em 0,62×.** Outras escalas/telas não homologadas. |

QA arquivado em 960×600 e 1280×720; Chromium/SwiftShader, não GPU/mouse físicos. Fit de texto não garante leitura fora dos casos testados. Compressão de bloco, memória GPU residente, outros browsers, nomes/localizações adicionais, telas completas e integração permanecem não validados, conforme relatório. UI-001 e NORMAL compacto mantêm suas aprovações anteriores; ornamental e estados antigos REWORK mantêm seu histórico.

## Validação deste fechamento

**PASS:** 12 PNGs versionados com assinatura/chunks/CRC/IDAT/filtros RGBA8 válidos; SHA-256, bytes, dimensões e igualdade byte a byte com masters/exports locais; hashes RAW/master conferidos contra o QA histórico; 31 evidências históricas conferidas por hash/bytes; links locais da documentação envolvida verificados; diff sem erros de whitespace. Nenhuma fonte local foi alterada.

**NOT RUN:** Editor, WebGL, geração e builds, conforme escopo. Os resultados técnicos usados são os existentes da PR #44. Integração em telas definitivas não executada.
