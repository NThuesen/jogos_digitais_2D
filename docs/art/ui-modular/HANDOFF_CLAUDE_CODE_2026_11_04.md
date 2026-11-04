# Handoff final para Claude Code — UI modular (04/11/2026)

## Comece aqui

1. Leia `AGENTS.md` e siga a recuperação mínima de contexto do Brain; repositório/filesystem são autoridade para o estado atual. Este handoff é documentação do projeto, não uma substituição da memória canônica.
2. Abra o [índice visual dos 12 PNGs individuais](incoming/BATCH-2026-11-04/README.md), a [proveniência verificável](incoming/BATCH-2026-11-04/PROVENANCE.json), o [registro](ASSET_REGISTRY.md) e o [manifesto](OUTPUT_MANIFEST.md).
3. A #47 **não pode receber merge/auto-merge sem revisão artística do responsável**. Nove PASS técnicos não são APPROVED. Nenhum asset foi integrado às telas finais.

## PRs e commits confirmados no encerramento

| PR | Estado confirmado | Commit / instrução |
| --- | --- | --- |
| [#46](https://github.com/NThuesen/jogos_digitais_2D/pull/46) | MERGED | `697bd2a1ad941efc29af5fbf84218ec9d19b09b1` |
| [#47](https://github.com/NThuesen/jogos_digitais_2D/pull/47) | OPEN — revisão artística pendente, sem auto-merge | `HEAD vivo da PR; este handoff será um novo commit` |
| [#48](https://github.com/NThuesen/jogos_digitais_2D/pull/48) | MERGED | `e431c096374561976afbbdff88e0a81a5e038836` |

[Snapshot com hashes consultados](incoming/BATCH-2026-11-04/PR_STATUS.json). #46: base main, draft retirado, auto-merge nativo solicitado uma vez; conflito documental com #48 resolvido preservando os prompts e aprovações; merge protegido concluído. #48: prompts corrigidos integrados, sem reescrita neste handoff. #47: permanece aberta, não draft e sem auto-merge. Não criar PR duplicada nem instalar watchers/polling de CI.

Commits de QA já publicados na #47: `46b88b8702aabce79eeedb12879e7998a1b0e673` (lote) e `b995746593731934595732e1831efb354fe49b4e` (limpeza do plugin Burst gerado). O commit de publicação/handoff é o HEAD subsequente da mesma PR; consulte o link da PR para o hash mais recente. #44 já estava integrada em `53b7f965141871041b21e96a053eff4d0e7e4596`.

## O que está versionado

- **Aprovados (#46, agora main):** seis CLEAN masters em `docs/art/ui-modular/approved/`, seis exports testados em `approved/exports/`, [handoff/configuração](https://github.com/NThuesen/jogos_digitais_2D/blob/697bd2a1ad941efc29af5fbf84218ec9d19b09b1/docs/art/ui-modular/approved/README.md) e `approved/UI_MODULAR_SIX_APPROVAL_MANIFEST.json`. UI-001 também permanece publicado; NORMAL compacto continua aprovado e local, sem integração.
- **Em revisão (#47):** 12 CLEAN masters individuais em `docs/art/ui-modular/incoming/BATCH-2026-11-04/`, 10 exports distintos em `exports/`, 2 RAWs distintos em `raw/`. UI-012 export reutiliza o CLEAN; UI-022 RAW=CLEAN=export, compartilhando o master. **24 PNGs únicos**, sem cópias binárias redundantes.
- [PROVENANCE.json](incoming/BATCH-2026-11-04/PROVENANCE.json) registra caminhos locais/repo, bytes, dimensões, SHA-256, operações de limpeza/export e configuração Unity usada. O RAW dos três REWORK está recuperável: UI-007/UI-009 em `raw/`, UI-022 no master com os mesmos bytes do RAW.
- [Relatório consolidado](reviews/UI_BATCH_NEW_2026_11_04.md) e [evidências existentes](reviews/evidence/BATCH-NEW-2026-11-04/README.md): inventário dos 23 RAWs, auditoria, 14 capturas Editor, 6 WebGL, contact sheets, resultados, fontes do harness/runner como texto e hashes. As imagens históricas não foram alteradas ou substituídas por este handoff.

### Seis masters já aprovados — hashes

Aprovação artística expressa do responsável, não deduzida de QA. Todos não INTEGRATED. Valores de export e configuração completos no manifesto da #46.

| Nome | Master SHA-256 | Limite artístico |
| --- | --- | --- |
| `UI-004_button_primary_compact_hover_v02.png` | `c47a56f4f1a83c12eb9a64644a38a9dba66a55cefbb2d15e7913f3f88000d45e` | Casos e limites já validados |
| `UI-004_button_primary_compact_pressed_v01.png` | `291a26b50675ecce4670b77de3705a5f71a4eeffdf4d0b50734a58da910d038d` | Casos e limites já validados |
| `UI-004_button_primary_compact_disabled_v02.png` | `3709ee3a696aa5b929a315fcacec31c2457736a068e34352bea11cd2dc51074d` | Casos e limites já validados |
| `UI-002_panel_medium_v01.png` | `00c44a34dca15be2cb61d994f05e32ec52775451bf80227bde5faa072683b2c9` | Casos e limites já validados |
| `UI-003_panel_small_label_v01.png` | `0a79225c60c61541f34c1d34f3d45f7e0685917d5d9c82cd19b9fec534b31a27` | Tamanho/texto restritos conforme QA |
| `UI-008_maid_card_compact_v01.png` | `74ea40f0b528580b4f809bd20a359bd1dbdacb02e4014f0d8ce0576676fb6342` | Somente detalhado 150×206; REWORK/não aprovado em 0,62× |

### Doze masters em revisão — hashes e decisões pendentes

**APPROVAL=PENDING para todos os 12**, inclusive os nove PASS técnicos. Usar os exports/configurações do manifesto ao preparar testes futuros; não aplicar borders de export ao master de outra resolução.

| CLEAN individual | SHA-256 | Veredito técnico / restrição |
| --- | --- | --- |
| [UI-005_button_secondary_normal_v01.png](incoming/BATCH-2026-11-04/UI-005_button_secondary_normal_v01.png) | `b1e18aa35f9edea77147be5e6f66836b6a8d89f5acdc5155b8e1849fb9c9b8ba` | **PASS restrito** — NORMAL; 120×44 Voltar, 150×44 Cancelar, 180×50 Detalhes, 230×54 Continuar; demais estados PENDING. |
| [UI-006_tab_inactive_v01.png](incoming/BATCH-2026-11-04/UI-006_tab_inactive_v01.png) | `8b4b093168b30a9da3db57066b39fa0f7101d57d6df9ff79b716229eccf17b5c` | **PASS restrito** — Somente INACTIVE, Simple/preserveAspect em 210×44/54; em 210×44 arte ocupa ≈167u de largura dentro do hitbox 210u. Flor central não homologada para 9-slice; ACTIVE/HOVER PENDING. |
| [UI-007_modal_dialog_v01.png](incoming/BATCH-2026-11-04/UI-007_modal_dialog_v01.png) | `21b332c2b60dc01ff32f0e8862e5b9e3b02344b8a76defbd02b9b1010d219cc0` | **REWORK** — Silhueta superior cortada pelo canvas: 42 pixels opacos no limite y=0. Reenviar fonte não cortada ou regenerar com margem; não reconstruída. |
| [UI-008_maid_card_lineup_minimal_v01.png](incoming/BATCH-2026-11-04/UI-008_maid_card_lineup_minimal_v01.png) | `0f80760c6d37a5016924c3f74a7500c482a9dd618365fcfe556cbdb19fd1a43a` | **PASS restrito** — LINEUP MINIMAL: PASS técnico em 150×206 e 0,62× (93×127,72), Suzu real/nome curto dinâmico. Nome pequeno/mais pesado no WebGL exige avaliação artística; nomes longos/outros portraits/tela inteira NOT RUN. |
| [UI-009_maid_card_detail_v01.png](incoming/BATCH-2026-11-04/UI-009_maid_card_detail_v01.png) | `5970311d3d765da4ff19c63e42c5059be81103c58dca09416d8e61ea0fcfe9de` | **REWORK** — Barras, sinais + e moedas pintados em áreas de dados; contradizem símbolos/dados separados Unity. Render diagnóstico não aprova composição. |
| [UI-010_rarity_badge_common_v01.png](incoming/BATCH-2026-11-04/UI-010_rarity_badge_common_v01.png) | `4a7cb48a3faac90a50f995d2c5448c749e83a3ba137eaa192613d7693158516f` | **PASS restrito** — Somente COMMON, Simple; 52,8×30,72 / 70,4×40,96 / 96,8×56,32. Outras raridades PENDING. |
| [UI-011_icon_book_v01.png](incoming/BATCH-2026-11-04/UI-011_icon_book_v01.png) | `7055446d2982a937365475f88d9e1f0f4a2790c9a1fe13a24ef5772233a094c6` | **PASS restrito** — BOOK: PASS restrito de silhueta em 32–44u; 24u foi renderizado, mas detalhes internos perdem leitura. Preferir 32–44u; simplificação depende do responsável se 24u for obrigatório. Demais ícones PENDING. |
| [UI-012_menu_background_v01.png](incoming/BATCH-2026-11-04/UI-012_menu_background_v01.png) | `d079d353925245c9e8c7e2891e69d40e19b47678808241fce9e618b67c18c22d` | **PASS restrito** — Background uniforme cover em 16:9 e 16:10; crop central sem esticar. Overlays de teste não são layout aprovado; contraste/tela final dependem de composição. |
| [UI-019_dialog_panel_v01.png](incoming/BATCH-2026-11-04/UI-019_dialog_panel_v01.png) | `631e6c8724b064be52f122fa19324f796a18d30fa0d7ed272dd73b6b2358db9b` | **PASS restrito** — PANEL, Sliced em 1000×170, texto multiline dinâmico; TAIL e outros contextos PENDING/NOT RUN. |
| [UI-022_floor_tile_warm_oak_v01.png](incoming/BATCH-2026-11-04/UI-022_floor_tile_warm_oak_v01.png) | `1258bda075f2b6f641b972c2805d990ba7d64162e6b0f2f4d5767b43ef3636d8` | **REWORK** — Repetição não seamless: MAE bordas X=3,437/Y=7,097 versus vizinhos 1,439/1,485; seam Y≈4,78× baseline. Não mesclado/recolorido. |
| [UI-023_wall_straight_v01.png](incoming/BATCH-2026-11-04/UI-023_wall_straight_v01.png) | `a270baf32d74c3e5fedaec45fa71d93f0478a4029849a9fcff98fbef34b230de` | **PASS render isolado** — STRAIGHT, dois segmentos Simple; encaixe no grid, perspectiva/câmera/sorting de produção NOT RUN. Outras paredes PENDING. |
| [UI-025_rarity_card_frame_common_v01.png](incoming/BATCH-2026-11-04/UI-025_rarity_card_frame_common_v01.png) | `1d60c0b894120372a7afcb58bb6121f644e61a1157bf5f969603458caa703327` | **PASS restrito** — COMMON, Simple em 150×206, janela/máscara própria; IoU 97,44% contra janela da UI-008 detalhada, sem certificação de SpriteSwap direto. Outras raridades PENDING. |

## Decisões de Maid Card que devem permanecer

- **UI-008 existente `maid_card_compact_v01`:** aceite artístico somente para apresentação detalhada em **150×206**; uso em **0,62× na Escalação não aprovado / REWORK**. Não descartar, renomear ou usar sua aprovação para homologar uma mini-card.
- **UI-008 LINEUP MINIMAL v01:** outra apresentação do mesmo personagem/dados; já tem PNG/CLEAN e QA técnico em **150×206 e 0,62×** com Suzu real/nome dinâmico curto. Agora está publicada em incoming; **aprovação artística continua PENDING**, nome WebGL pequeno/mais pesado exige revisão. Não permanece “não gerada”.
- **UI-009 v02:** possível detalhe expandido opcional, não pré-requisito para as duas apresentações já decididas. Não criar nova entidade de maid, retrato, economia, save ou controle por causa das variantes.

## Próximas tentativas v02 — sem reescrever os prompts da #48

| v01 preservada | Prompt vigente | Nova saída |
| --- | --- | --- |
| UI-007 REWORK: topo já cortado no RAW | [07_MODAL_DIALOG.md](prompts/07_MODAL_DIALOG.md) | `UI-007_modal_dialog_v02.png` — PENDING |
| UI-009 REWORK: barras, + e moedas pintados | [09_MAID_CARD_DETAIL.md](prompts/09_MAID_CARD_DETAIL.md) | `UI-009_maid_card_detail_v02.png` — PENDING; opcional |
| UI-022 REWORK: piso não seamless | [22_FLOOR_TILE.md](prompts/22_FLOOR_TILE.md) | `UI-022_floor_tile_warm_oak_v02.png` — PENDING |

Os três v02 **ainda não existem no RAW local verificado**, sem CLEAN/export/QA/aprovação. Os três prompts são byte a byte os integrados pela #48. Seus anexos v01 antes locais podem agora ser baixados pelo índice individual/proveniência: não é necessário alterar o bloco de geração para recuperar a referência. Não sobrescrever v01 nem renomear CLEAN v01 como v02. Somente o responsável gera/refaz as artes e dá aceite.

## Caminhos locais importantes — não apagar

- Projeto/worktree original: `/mnt/c/dev/PocketSlop-Games/jogos_digitais_2D` (Windows `C:\dev\PocketSlop-Games\jogos_digitais_2D`). A branch de aprovação da #46 permanece neste worktree; foi sincronizada/pushed, não é uma branch para iniciar trabalho diferente.
- Worktree da #47: `/mnt/c/dev/PocketSlop-Games/ui-modular-new-batch` (Windows `C:\dev\PocketSlop-Games\ui-modular-new-batch`), branch **`qa/ui-modular-new-batch`**. Trabalhar aqui para continuar a revisão/handoff desta PR; não tentar selecionar essa branch em outro worktree enquanto estiver anexada.
- Fontes RAW: `<projeto original>/art_working/ui_modular/raw_gpt/` — **23 PNGs** no inventário existente. Todos preservados localmente; somente os três RAWs REWORK foram solicitados para arquivamento adicional nesta entrega. RAWs não publicados continuam importantes: **não executar git clean -fdx, apagar worktree original ou limpar art_working**.
- CLEANs: `<projeto original>/art_working/ui_modular/blender_clean/` — **8 anteriores + 12 novos**, todos preservados. PRESSED v01 já era RAW tecnicamente limpo, sem CLEAN duplicado fictício.
- Exports novos: `<projeto original>/art_working/ui_modular/export_unity/BATCH_NEW_2026_11_04/` — 12 arquivos locais, mapeados para os PNGs únicos de incoming.
- Exports históricos: `.../export_unity/UI_MODULAR_CONSOLIDATED_QA/`, `UI-004_STATES_QA/`, `UI-004_COMPACT_QA/` e `UI-004_QA/`; mantidos. Não trocar DISABLED v02 por v01 nem o COMPACT por ornamental REWORK.
- Os hashes dos arquivos locais estão em [audit.json](reviews/evidence/BATCH-NEW-2026-11-04/audit.json), [inventory.json](reviews/evidence/BATCH-NEW-2026-11-04/inventory.json) e nos manifestos de approved/incoming. Conferir hashes antes de copiar ou reutilizar CLEAN existente.
- `/tmp/ui-modular-batch-env` foi apenas ambiente temporário; pode desaparecer. Não contém a única cópia de nenhuma fonte: procedimentos, harness e versões de ferramentas estão versionados como texto nas evidências. Caches Library separados são ignorados; não são evidência necessária para recuperar a arte.

## Validação e limite deste encerramento

Esta etapa fez **somente integridade de arquivos/PNG, hashes, links e diff**; nenhum Editor/WebGL, teste extenso, build, publicação de jogo ou geração de imagem. Os PASS de QA são históricos e restritos aos casos documentados. Cópias em incoming são byte a byte os CLEANs/exports já testados; RAWs preservados por hash, sem processamento novo. QA técnico, aprovação artística e integração continuam separados.

O QA anterior foi 1 PlayMode direcionado em 960×600/1280×720 (cliques/raycast simulados), mais 1 build WebGL/Chromium automatizado/SwiftShader nas mesmas resoluções. Zero erros de página/console; warning FSR e texto mais pesado documentados. Não homologou mouse/GPU físicos, outros browsers, performance/memória/compressão, teclado/gamepad, todos os nomes/portraits, telas completas ou integração. Build, cena, harness/template temporários e alteração do plugin Burst gerado foram removidos/restaurados naquela etapa; não reutilizar build como produto.

## Próximos trabalhos, em ordem

1. Responsável revisar os **12 PNGs individuais**, aceitando/rejeitando separadamente os nove candidatos técnicos e as restrições (nome MINIMAL, BOOK 24u, máscara UI-025, contexto UI-023). **Não marcar APPROVED por conta própria.**
2. Quando autorizado, produzir UI-007 v02 e UI-022 v02; UI-009 v02 é opcional. Usar prompts corrigidos e RAWs de referência, sem substituir v01. Depois inventariar/auditar somente arquivos novos, preservando hashes de fontes.
3. Registrar decisões artísticas explícitas e, só após revisão do responsável, concluir o fluxo protegido da #47. **Não habilitar auto-merge agora.**
4. Integração nas telas definitivas e QA de produção são tarefa separada: preservar controles/dados Unity, GUIDs, saves, gameplay, formatos e decisões das duas Maid Cards. Não usar este handoff como autorização de mudança de escopo ou publicação.

## Manifesto reconciliado no encerramento

O manifesto contém **105 nomes** (100 saídas-base + versões posteriores), 23 RAWs presentes e **82 nomes ainda não gerados**. O inventário histórico do QA listava 79 ausentes; as três v02 da #48 acrescentam três novas tentativas PENDING. [Lista atual por presença de arquivo](incoming/BATCH-2026-11-04/MANIFEST_STATUS.json). Não confundir os três v01 REWORK já existentes com suas novas v02 PENDING e não gerar o catálogo inteiro.
