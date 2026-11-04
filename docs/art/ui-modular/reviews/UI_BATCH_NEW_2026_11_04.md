# Novo lote modular — inventário, limpeza e QA incremental (04/11/2026)

## Escopo e isolamento

Base main `53b7f965141871041b21e96a053eff4d0e7e4596` (merge protegido da PR #44). A PR #46 permanece independente; sua branch/arquivos não foram alterados. Este lote usa worktree/branch `qa/ui-modular-new-batch` a partir da main, sem duplicar o diff #44/#46. Os hashes históricos/aprovações expressas foram consultados para excluir os seis candidatos já validados e preservar suas decisões; não se usa ausência na main para reprocessá-los.

**23 RAWs encontrados; 11 históricos preservados, 12 novos processados.** Nenhum nome incorreto, duplicado byte a byte/pixels ou candidato BLOCKED no inventário. HOVER/DISABLED v01/v02 são versões históricas distintas, não duplicatas a apagar. Nenhum RAW alterado, CLEAN anterior sobrescrito, nova aprovação artística ou integração. Os 12 CLEANs/exports novos continuam locais/ignorados; somente relatório, métricas, provas e fontes reproduzíveis como texto são versionados. Esta PR exige revisão do responsável e **não tem merge/auto-merge solicitado**.

## Inventário RAW → CLEAN → export

Inventário completo e SHA-256 integrais: [inventory.json](evidence/BATCH-NEW-2026-11-04/inventory.json). CLEAN/export, transformações e auditoria por pixel: [audit.json](evidence/BATCH-NEW-2026-11-04/audit.json). Cada caminho RAW/CLEAN refere-se a `art_working/ui_modular/` da sessão de origem; exports novos em `export_unity/BATCH_NEW_2026_11_04/`. O basename é preservado. Arquivos de produção não foram importados/integrados.

| PNG novo | RAW | CLEAN | Export de QA | Veredito / pendência |
| --- | --- | --- | --- | --- |
| `UI-005_button_secondary_normal_v01.png` | 2172×724 | 2172×724 | 1059×262 | **PASS restrito** — NORMAL; 120×44 Voltar, 150×44 Cancelar, 180×50 Detalhes, 230×54 Continuar; demais estados PENDING. |
| `UI-006_tab_inactive_v01.png` | 2172×724 | 2172×724 | 1002×264 | **PASS restrito** — Somente INACTIVE, Simple/preserveAspect em 210×44/54; em 210×44 arte ocupa ≈167u de largura dentro do hitbox 210u. Flor central não homologada para 9-slice; ACTIVE/HOVER PENDING. |
| `UI-007_modal_dialog_v01.png` | 1816×866 | 1816×866 | 908×433 | **REWORK** — Silhueta superior cortada pelo canvas: 42 pixels opacos no limite y=0. Reenviar fonte não cortada ou regenerar com margem; não reconstruída. |
| `UI-008_maid_card_lineup_minimal_v01.png` | 1070×1470 | 1070×1470 | 535×735 | **PASS restrito** — LINEUP MINIMAL: PASS técnico em 150×206 e 0,62× (93×127,72), Suzu real/nome curto dinâmico. Nome pequeno/mais pesado no WebGL exige avaliação artística; nomes longos/outros portraits/tela inteira NOT RUN. |
| `UI-009_maid_card_detail_v01.png` | 1536×1024 | 1536×1024 | 768×512 | **REWORK** — Barras, sinais + e moedas pintados em áreas de dados; contradizem símbolos/dados separados Unity. Render diagnóstico não aprova composição. |
| `UI-010_rarity_badge_common_v01.png` | 1536×1024 | 1536×1024 | 664×384 | **PASS restrito** — Somente COMMON, Simple; 52,8×30,72 / 70,4×40,96 / 96,8×56,32. Outras raridades PENDING. |
| `UI-011_icon_book_v01.png` | 1286×1223 | 1286×1223 | 512×512 | **PASS restrito** — BOOK: PASS restrito de silhueta em 32–44u; 24u foi renderizado, mas detalhes internos perdem leitura. Preferir 32–44u; simplificação depende do responsável se 24u for obrigatório. Demais ícones PENDING. |
| `UI-012_menu_background_v01.png` | 1672×941 | 1672×941 | 1672×941 | **PASS restrito** — Background uniforme cover em 16:9 e 16:10; crop central sem esticar. Overlays de teste não são layout aprovado; contraste/tela final dependem de composição. |
| `UI-019_dialog_panel_v01.png` | 2172×724 | 2172×724 | 1086×362 | **PASS restrito** — PANEL, Sliced em 1000×170, texto multiline dinâmico; TAIL e outros contextos PENDING/NOT RUN. |
| `UI-022_floor_tile_warm_oak_v01.png` | 1254×1254 | 1254×1254 | 1254×1254 | **REWORK** — Repetição não seamless: MAE bordas X=3,437/Y=7,097 versus vizinhos 1,439/1,485; seam Y≈4,78× baseline. Não mesclado/recolorido. |
| `UI-023_wall_straight_v01.png` | 1536×1024 | 1536×1024 | 768×512 | **PASS render isolado** — STRAIGHT, dois segmentos Simple; encaixe no grid, perspectiva/câmera/sorting de produção NOT RUN. Outras paredes PENDING. |
| `UI-025_rarity_card_frame_common_v01.png` | 1070×1470 | 1070×1470 | 535×735 | **PASS restrito** — COMMON, Simple em 150×206, janela/máscara própria; IoU 97,44% contra janela da UI-008 detalhada, sem certificação de SpriteSwap direto. Outras raridades PENDING. |

### Históricos não reprocessados

- `UI-001_panel_large_v01.png` — RAW SHA-256 `d5631b7fd8901d49239dc43871801bcfff391878c6ce7552efb1aeb61fb7f22a`; CLEAN/export, quando existentes, conforme inventário. QA anterior/aprovação ou REWORK preservados; **não reexecutado**.
- `UI-002_panel_medium_v01.png` — RAW SHA-256 `a939d2bc990e56787fea85f0b543303ade489bddfbadb412a4c20a60a2288b16`; CLEAN/export, quando existentes, conforme inventário. QA anterior/aprovação ou REWORK preservados; **não reexecutado**.
- `UI-003_panel_small_label_v01.png` — RAW SHA-256 `0775e22cfdfa766b25eafa68ec13cb819a961c5f63e6e02c367829bd80c1e3df`; CLEAN/export, quando existentes, conforme inventário. QA anterior/aprovação ou REWORK preservados; **não reexecutado**.
- `UI-004_button_primary_compact_disabled_v01.png` — RAW SHA-256 `5ddb38f5275dfda5b95eb4ae73d06e9df468c5400310d20900e4c73df4e0cb60`; CLEAN/export, quando existentes, conforme inventário. QA anterior/aprovação ou REWORK preservados; **não reexecutado**.
- `UI-004_button_primary_compact_disabled_v02.png` — RAW SHA-256 `7d937f5757e7044f8184cf58dbf0d910405d5fce894c3eb45bf2b5f1e548d1fb`; CLEAN/export, quando existentes, conforme inventário. QA anterior/aprovação ou REWORK preservados; **não reexecutado**.
- `UI-004_button_primary_compact_hover_v01.png` — RAW SHA-256 `6c8dc92d2274419387301b7b1161920011032d9d0341256c935a694de718121c`; CLEAN/export, quando existentes, conforme inventário. QA anterior/aprovação ou REWORK preservados; **não reexecutado**.
- `UI-004_button_primary_compact_hover_v02.png` — RAW SHA-256 `4beaadaf15870747b92f1bd8ea323d442e0b7e083d9ca8cba4e1730369ac6e6a`; CLEAN/export, quando existentes, conforme inventário. QA anterior/aprovação ou REWORK preservados; **não reexecutado**.
- `UI-004_button_primary_compact_normal_v01.png` — RAW SHA-256 `72deadbfffbda54e1a6fc0a91a192916a42c7db91a9bc3cb769d9a80676e64b0`; CLEAN/export, quando existentes, conforme inventário. QA anterior/aprovação ou REWORK preservados; **não reexecutado**.
- `UI-004_button_primary_compact_pressed_v01.png` — RAW SHA-256 `291a26b50675ecce4670b77de3705a5f71a4eeffdf4d0b50734a58da910d038d`; CLEAN/export, quando existentes, conforme inventário. QA anterior/aprovação ou REWORK preservados; **não reexecutado**.
- `UI-004_button_primary_normal_v01.png` — RAW SHA-256 `91466ecc77f6bb1d6b30540883961c5b1d0babbce7e3d9b83c9aef059e98f2bf`; CLEAN/export, quando existentes, conforme inventário. QA anterior/aprovação ou REWORK preservados; **não reexecutado**.
- `UI-008_maid_card_compact_v01.png` — RAW SHA-256 `c849093964090779adc92a83f9880fcc0e72de22622479f12b8917b733f118a2`; CLEAN/export, quando existentes, conforme inventário. QA anterior/aprovação ou REWORK preservados; **não reexecutado**.

## P1 — limpeza conservadora e inspeção

Todos os 12 PNGs foram verificados e decodificados integralmente. Nenhum contém ICC/gamma/metadado de cor a preservar. A limpeza mantém canvas, coordenadas, RGB e alpha 4–249 pixel a pixel. Remove somente alpha 1–3 a mais de 6px de qualquer alpha≥8; solidifica alpha 250–254, incluindo o creme intencional. AA/sombra restante e interiores transparentes foram preservados. Remoção de resíduos não é prova automática de ausência de halo; inspeção visual sobre fundo contrastante complementou as métricas. Nenhum recorte, redesenho ou preenchimento de áreas faltantes no master.

UI-012 e UI-022 são cópias CLEAN byte a byte dos RAWs opacos, sem remoção de fundo. Exports: recorte somente externo para UI-005/006/010/011, padding quadrado no BOOK e redução Lanczos premultiplicada, registrados exatamente no audit. Os demais preservam proporção/canvas; background e piso mantêm resolução. Nenhum recolor ou correção artística destrutiva. RAW/CLEAN/export nunca são intercambiáveis sem seus metadados.

| Prova útil | Evidência |
| --- | --- |
| Todos os CLEANs | [Lote 1](evidence/BATCH-NEW-2026-11-04/clean_contact_1.png), [lote 2](evidence/BATCH-NEW-2026-11-04/clean_contact_2.png) |
| Janelas transparentes e RAW/CLEAN | [Comparação](evidence/BATCH-NEW-2026-11-04/portrait_alpha_raw_clean.png), [retângulos seguros](evidence/BATCH-NEW-2026-11-04/portrait_safe_regions.json) |
| Coesão com referências aprovadas, sem repetir seu QA | [UI-001/NORMAL compacto × candidatos](evidence/BATCH-NEW-2026-11-04/family_references.png) |
| UI-007: corte já presente no RAW | [Topo em resolução de origem](evidence/BATCH-NEW-2026-11-04/modal_top_edge.png) |
| UI-022: repetição do arquivo original | [3×3](evidence/BATCH-NEW-2026-11-04/floor_repeat_3x3.png) |

A inspeção confirma linguagem creme/rosewood/terracota com ornamentos, mas coesão e legibilidade artística final são decisão do responsável. UI-025 tem janela próxima, porém não idêntica à UI-008 detalhada (IoU 0,974369 na ROI documentada); usar máscara própria, sem declarar intercâmbio exato. UI-009 foi testada como diagnóstico dos símbolos/barras pintados, sem apagar desenho para obter PASS.

## P2 — Editor direcionado

**PASS: 1 teste PlayMode direcionado, 0 falhas/skips/inconclusivos; não uma suíte extensa.** 14 páginas reais renderizadas (7 famílias × 2 resoluções), 32 verificações de fit, 56 verificações de clique e 56 de raycast. Asserções de ponteiro usam ExecuteEvents: 26 cliques disponíveis aceitos e 30 tentativas indisponíveis bloqueadas; interação **simulada**, não mouse físico. Controles/cliques são do harness, não fluxos reais de compra, seleção ou save. O XML lista testes descobertos, mas somente `ModularQaTests.NewCandidatesOnly` foi executado.

[XML do teste](evidence/BATCH-NEW-2026-11-04/editor_results.xml), [resultados/fatos por página](evidence/BATCH-NEW-2026-11-04/editor_results.json), [fontes/configuração](evidence/BATCH-NEW-2026-11-04/unity_candidate_config.json). A primeira compilação do harness falhou por ambiguidade `Screen` (CS0104); corrigido somente para `UnityEngine.Screen` no código temporário, antes da execução válida. Nenhum teste de produção foi alterado ou enfraquecido. Unity 6000.5.9f1; projeto importado em cache separado, sem tocar no Library da sessão original.

| Família / casos | 960×600 | 1280×720 |
| --- | --- | --- |
| UI-005/006 — proporção, texto, hitbox e disponibilidade | [Captura](evidence/BATCH-NEW-2026-11-04/editor_controls_960x600.png) | [Captura](evidence/BATCH-NEW-2026-11-04/editor_controls_1280x720.png) |
| UI-007/019 — modal, texto e botões nativos | [Captura](evidence/BATCH-NEW-2026-11-04/editor_panels_960x600.png) | [Captura](evidence/BATCH-NEW-2026-11-04/editor_panels_1280x720.png) |
| UI-008 MINIMAL e UI-025 — Suzu, alpha, nome, seleção simulada | [Captura](evidence/BATCH-NEW-2026-11-04/editor_lineup_960x600.png) | [Captura](evidence/BATCH-NEW-2026-11-04/editor_lineup_1280x720.png) |
| UI-009 — composição diagnóstica; REWORK preservado | [Captura](evidence/BATCH-NEW-2026-11-04/editor_detail_960x600.png) | [Captura](evidence/BATCH-NEW-2026-11-04/editor_detail_1280x720.png) |
| UI-010/011 — tamanhos pequenos | [Captura](evidence/BATCH-NEW-2026-11-04/editor_icons_960x600.png) | [Captura](evidence/BATCH-NEW-2026-11-04/editor_icons_1280x720.png) |
| UI-012 — cover e crop, overlay de teste | [Captura](evidence/BATCH-NEW-2026-11-04/editor_background_960x600.png) | [Captura](evidence/BATCH-NEW-2026-11-04/editor_background_1280x720.png) |
| UI-022/023 — repetição/composição isolada | [Captura](evidence/BATCH-NEW-2026-11-04/editor_environment_960x600.png) | [Captura](evidence/BATCH-NEW-2026-11-04/editor_environment_1280x720.png) |

**LINEUP MINIMAL:** referência 150×206 e escala real 0,62× → 93×127,72. Suzu real de `MaidAnimCatalog`, frame idle ilustrado existente, recorte separado sob moldura; nome vem de `Maid.Create`, não está pintado na textura. Janela segura master [160,230,910,1000), o retângulo de clipping fica sob a moldura (99,954% transparente, com a pequena interseção de borda ocultada pelo próprio frame), nome na faixa creme inferior, fonte nominal 16 no tamanho base e 10 em 0,62×. A 960×600 a fonte pequena corresponde a ≈8,33px; PASS restrito ao nome curto “Suzu”, sem inferir leitura de todos os nomes/idiomas. Sem stats/salário na mini-card. Não é QA da tela inteira da Escalação nem homologação de seleção de produção.

**UI-023:** PASS de render e composição de dois segmentos, sem certificar grid, câmera, sorting ou encaixe nas salas reais. **UI-012:** escala uniforme cover; adaptação 16:9→16:10 recorta laterais, sem esticar. Texto que cabe não garante contraste universal sobre cenário; overlay de teste não é layout artístico aprovado.

## WebGL consolidado

**PASS** — uma única build temporária, sem publicação. [Resultados do navegador](evidence/BATCH-NEW-2026-11-04/webgl_results.json). 153.0.8010.12 headless/SwiftShader; duas sessões em 960×600/1280×720, 7 famílias por sessão, cliques automatizados do navegador (não mouse físico). Capturas somente de controles, lineup e background para complementar Editor sem duplicar todas as provas.

- controls: [960×600](evidence/BATCH-NEW-2026-11-04/webgl_controls_960x600.png), [1280×720](evidence/BATCH-NEW-2026-11-04/webgl_controls_1280x720.png).
- lineup: [960×600](evidence/BATCH-NEW-2026-11-04/webgl_lineup_960x600.png), [1280×720](evidence/BATCH-NEW-2026-11-04/webgl_lineup_1280x720.png).
- background: [960×600](evidence/BATCH-NEW-2026-11-04/webgl_background_960x600.png), [1280×720](evidence/BATCH-NEW-2026-11-04/webgl_background_1280x720.png).

Uma renderização WebGL PASS não elimina REWORK de corte, dados pintados ou costura do piso. Zero erros de página/console; 26 cliques disponíveis aceitos e 2 tentativas indisponíveis bloqueadas. Warning FSR: shader Hidden/Universal Render Pipeline/Edge Adaptive Spatial Upsampling não suportado pelo SwiftShader; postprocess correspondente não executado. Texto WebGL é visualmente mais pesado que Editor; “Suzu” em 0,62×/960×600 permanece candidato técnico, com leitura pequena para avaliação do responsável, sem aprovação universal. [Resumo da única build](evidence/BATCH-NEW-2026-11-04/webgl_build_summary.txt): Succeeded, 153.981.515 bytes, 20min06,865s no cache isolado, 0 erros. O tamanho inclui Resources existentes e não mede custo incremental dos assets. SwiftShader não certifica GPU física, desempenho ou memória residente.

## Configurações e limites recomendados

[Configuração exata usada](evidence/BATCH-NEW-2026-11-04/unity_candidate_config.json) e [setup reproduzível](evidence/BATCH-NEW-2026-11-04/harness/Editor/ModularQaSetup.cs.txt): Sprite Single, Full Rect, PPU 100, Bilinear, Clamp, sem mipmaps, Uncompressed, alphaIsTransparency, NPOT None, maxTextureSize 4096. Valores abaixo referem-se **aos exports**, não aos masters.

- UI-005: Sliced, border L/R=190,62 e B/T=94,32 px, multiplier262/altura. Somente tamanhos/rótulos medidos; estados restantes não foram inventados.
- UI-019: Sliced, L/B/R/T=120/80/120/80, multiplier362/170. Não copiar borders de painéis antigos.
- UI-006 e UI-007: Simple/preserveAspect para evitar esticar flor/laço centrais. Outros tamanhos/slicing exigem teste próprio; UI-007 ainda REWORK por corte de origem.
- UI-008 MINIMAL, UI-009 e UI-025: Simple/preserveAspect, portrait/dados separados, máscaras próprias; nunca aplicar 9-slice à card inteira.
- UI-010/011: Simple/preserveAspect. BOOK deve preferir 32–44u; 24u é prova diagnóstica, sem homologar leitura dos detalhes. Padding e bounds do export estão documentados.
- UI-012: imagem opaca, cover uniforme com crop central. UI-022: prova 3×3, não Repeat aprovado; corrigir seamless antes de produção. UI-023: Simple, grid/perspectiva/sorting pendentes.

A inspeção atual não mede compressão de bloco, GPU/memória residente/performance, outros browsers, input físico, teclado/gamepad/foco/drag-cancel, localização/nomes longos, múltiplos portraits, telas completas, saves ou integração. Os seis assets anteriores não foram re-testados. Não há APK/AAB, publicação ou mudança de gameplay.

## Pendências artísticas e imagens a refazer

1. **UI-007:** reenviar fonte original sem corte superior ou regenerar com margem transparente que preserve o ornamento inteiro. Não preencher pixels ausentes automaticamente.
2. **UI-009:** corrigir/regenerar como moldura vazia, sem barras, + ou moedas fixos em zonas que a Unity deve preencher; qualquer exceção de design exige decisão expressa do responsável.
3. **UI-022:** refazer/corrigir a continuidade das quatro bordas; validar repetição antes de aceite. Nenhum blending destrutivo foi aplicado.
4. Demais nove candidatos aguardam avaliação artística expressa, sob os limites de QA. LINEUP MINIMAL tem PNG e QA agora, **não APPROVED**. UI-025 exige aceite de geometria própria; UI-023 exige decisão de contexto/perspectiva e validação posterior no grid, sem pressupor regeneração obrigatória.

## PNGs do manifesto ainda não gerados

**79 nomes ausentes**, listados abaixo e em `inventory.json`. Ausência significa **PENDING**, não REWORK. As versões v02 históricas não justificam apagar os v01 nem gerar o catálogo inteiro. Famílias parcialmente produzidas continuam com os demais estados/variantes pendentes.

- `UI-004_button_primary_disabled_v01.png`
- `UI-004_button_primary_hover_v01.png`
- `UI-004_button_primary_pressed_v01.png`
- `UI-005_button_secondary_disabled_v01.png`
- `UI-005_button_secondary_hover_v01.png`
- `UI-005_button_secondary_pressed_v01.png`
- `UI-006_tab_active_v01.png`
- `UI-006_tab_hover_v01.png`
- `UI-010_rarity_badge_epic_v01.png`
- `UI-010_rarity_badge_legendary_v01.png`
- `UI-010_rarity_badge_rare_v01.png`
- `UI-011_icon_cat_paw_v01.png`
- `UI-011_icon_clock_v01.png`
- `UI-011_icon_coin_v01.png`
- `UI-011_icon_cup_v01.png`
- `UI-011_icon_gear_v01.png`
- `UI-011_icon_gift_v01.png`
- `UI-011_icon_heart_v01.png`
- `UI-011_icon_shop_bag_v01.png`
- `UI-011_icon_star_v01.png`
- `UI-013_restaurant_arch_detail_v01.png`
- `UI-013_restaurant_arch_floor_v01.png`
- `UI-013_restaurant_arch_walls_v01.png`
- `UI-014_prop_decor_book_stack_v01.png`
- `UI-014_prop_decor_cake_slice_v01.png`
- `UI-014_prop_decor_coffee_grinder_v01.png`
- `UI-014_prop_decor_hanging_plant_v01.png`
- `UI-014_prop_decor_napkin_holder_v01.png`
- `UI-014_prop_decor_pastry_plate_v01.png`
- `UI-014_prop_decor_tea_cup_v01.png`
- `UI-014_prop_decor_tea_jars_v01.png`
- `UI-014_prop_decor_tea_kettle_v01.png`
- `UI-014_prop_decor_wall_picture_v01.png`
- `UI-015_top_bar_hud_v01.png`
- `UI-016_bottom_strip_day_ui_v01.png`
- `UI-017_shop_item_frame_v01.png`
- `UI-018_balance_row_normal_v01.png`
- `UI-018_balance_row_total_v01.png`
- `UI-019_dialog_tail_v01.png`
- `UI-020_logo_wordmark_v01.png`
- `UI-021_maid_portrait_akari_neutral_v01.png`
- `UI-021_maid_portrait_hina_neutral_v01.png`
- `UI-021_maid_portrait_ichika_neutral_v01.png`
- `UI-021_maid_portrait_kaede_neutral_v01.png`
- `UI-021_maid_portrait_momo_neutral_v01.png`
- `UI-021_maid_portrait_nene_neutral_v01.png`
- `UI-021_maid_portrait_reina_neutral_v01.png`
- `UI-021_maid_portrait_suzu_neutral_v01.png`
- `UI-021_maid_portrait_yoru_neutral_v01.png`
- `UI-023_wall_corner_left_v01.png`
- `UI-023_wall_corner_right_v01.png`
- `UI-023_wall_door_v01.png`
- `UI-023_wall_window_v01.png`
- `UI-024_placeable_bakery_v01.png`
- `UI-024_placeable_cat_tower_v01.png`
- `UI-024_placeable_chandelier_v01.png`
- `UI-024_placeable_dessert_v01.png`
- `UI-024_placeable_door_v01.png`
- `UI-024_placeable_gothic_candelabra_v01.png`
- `UI-024_placeable_gothic_mirror_v01.png`
- `UI-024_placeable_koi_pond_v01.png`
- `UI-024_placeable_lantern_sakura_v01.png`
- `UI-024_placeable_planetarium_v01.png`
- `UI-024_placeable_plant_v01.png`
- `UI-024_placeable_rug_v01.png`
- `UI-024_placeable_sakura_tree_v01.png`
- `UI-024_placeable_showa_jukebox_v01.png`
- `UI-024_placeable_showa_poster_v01.png`
- `UI-024_placeable_stage_v01.png`
- `UI-024_placeable_star_lamp_v01.png`
- `UI-024_placeable_stool_v01.png`
- `UI-024_placeable_stove_v01.png`
- `UI-024_placeable_table_2_v01.png`
- `UI-024_placeable_table_4_v01.png`
- `UI-024_placeable_teabar_v01.png`
- `UI-024_placeable_window_v01.png`
- `UI-025_rarity_card_frame_epic_v01.png`
- `UI-025_rarity_card_frame_legendary_v01.png`
- `UI-025_rarity_card_frame_rare_v01.png`

## Reprodução e integridade

Os scripts de processamento, preparação, runner e navegador estão em `evidence/BATCH-NEW-2026-11-04/` como texto, e o harness em `harness/`. Usam guard de operação Unity e verificam processo Editor do host Windows antes de iniciar. Reproduzir em projeto/cache isolados, com RAWs cujo hash coincide, configurando caminhos SOURCE/WORK/EVID/EXPORT para a sessão; não executar sobre evidências já registradas. O processador recusa substituir uma auditoria existente; CLEAN existente só pode ser reutilizado após igualdade de pixels, sem overwrite. Fontes de produção/Suzu têm [hashes de proveniência](evidence/BATCH-NEW-2026-11-04/qa_source_manifest.json).

[Classificação por asset](evidence/BATCH-NEW-2026-11-04/classification.json) distingue estágio, veredito técnico, aprovação PENDING e `integrated=false`. [Manifesto das evidências](evidence/BATCH-NEW-2026-11-04/evidence_manifest.json) permite conferir bytes/SHA-256 dos arquivos registrados. Resultado técnico não autoriza aprovação artística, integração ou merge.

## Limpeza e validação final

**PASS:** 23 RAWs e 8 CLEANs anteriores intactos por hash; 12 CLEANs/exports novos verificados por SHA-256/bytes/dimensões, RGB/canvas e política exata de alpha; JSON/XML/Python e PNGs válidos; links e diff conferidos. Build, cena, harness e template temporários removidos; [registro](evidence/BATCH-NEW-2026-11-04/cleanup.json). Nenhuma diferença em Assets/_Game, ProjectSettings ou Packages após o finally do builder. RAW/CLEAN/export locais preservados.

Textos de evidência usam LF para que hashes coincidam no Git e no checkout; somente finais de linha do XML/fontes eventualmente CRLF foram normalizados, sem mudar valores, resultados ou código. [Registro de normalização](evidence/BATCH-NEW-2026-11-04/evidence_text_normalization.json) mantém hashes/bytes de origem e destino dos textos afetados. Não confundir a formatação portátil com reexecução ou alteração de resultados.
