# UI modular — aprovações da PR #47 (04/11/2026)

Decisão expressa do responsável (Decisao expressa do responsavel: Encerrar revisao artistica e iniciar integracao visual (04/11/2026)). **APPROVED ≠ INTEGRATED.** Masters = CLEANs byte a byte (hashes e RAW no [manifesto](approved_manifest)); exports reduzidos quando existiam. QA técnico é o já registrado nas PRs de origem, **não repetido**; WebGL só nas telas integradas (PR de integração). RAWs/REWORK/v01 preservados em `incoming/`, nunca como assets aprovados.

| Master aprovado | Restrição |
| --- | --- |
| [UI-005_button_secondary_normal_v01.png](UI-005_button_secondary_normal_v01.png) | Secundario apenas; a Unity deve distinguir claramente o DISABLED (cor/alpha do texto, nao so a arte). Libera HOVER v02, PRESSED, DISABLED. |
| [UI-006_tab_inactive_v01.png](UI-006_tab_inactive_v01.png) | Somente Simple/preserveAspect (sem 9-slice). Libera ACTIVE e HOVER, com indicador persistente de aba selecionada na Unity. |
| [UI-010_rarity_badge_common_v01.png](UI-010_rarity_badge_common_v01.png) | Somente Simple; completa a familia de 4 selos. |
| [UI-025_rarity_card_frame_common_v01.png](UI-025_rarity_card_frame_common_v01.png) | Area segura comum do retrato [120,172,950,789] (1070x1470); sem presumir SpriteSwap direto; mascara propria. |
| [UI-019_dialog_panel_v01.png](UI-019_dialog_panel_v01.png) | 9-slice 1000x170 testado; libera o TAIL sobreposto. |
| [UI-011_icon_book_v01.png](UI-011_icon_book_v01.png) | Somente 32-44u; 24u nao homologado. |
| [UI-012_menu_background_v01.png](UI-012_menu_background_v01.png) | Somente como fundo de menu (cover 16:9/16:10). |
| [UI-008_maid_card_lineup_minimal_v01.png](UI-008_maid_card_lineup_minimal_v01.png) | Duas Maid Cards preservadas (compact + lineup minimal); nomes curtos; validar legibilidade na tela real. |

**REWORK:** UI-023 WALL STRAIGHT — para uso como parede; não integrar como cenário (histórico em `incoming/`). **Superados:** UI-007 v01, UI-009 v01, UI-022 v01.

**Liberados por estas aprovações (dependem de masters de outras PRs):** UI-005 HOVER v02/PRESSED/DISABLED e UI-006 ACTIVE/HOVER (#52); TAIL UI-019 (#52); selos/molduras RARE/EPIC/LEGENDARY (#52).
