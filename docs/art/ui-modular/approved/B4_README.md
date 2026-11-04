# UI modular — aprovações do lote B4 (04/11/2026, PR #52)

Decisões expressas do responsável ("Fechamento artístico do lote B4" e "Encerramento definitivo do B4"). **APPROVED não é INTEGRATED**: nada foi integrado a telas/cenas/importers, nenhuma build, WebGL **NOT RUN**. QA técnico = QA Editor da #52 (960×600 e 1280×720, pointer simulado), **não repetido**: [B4](../reviews/evidence/BATCH-B4-QA-2026-11-04/) e [v02](../reviews/evidence/BATCH-B4-V02-QA-2026-11-04/); relatório [UI_BATCH_B4](../reviews/UI_BATCH_B4_2026_11_04.md). Hashes/proveniência: [manifesto](UI_MODULAR_B4_APPROVAL_MANIFEST.json). Masters = CLEANs byte a byte; exports = exports testados; RAWs e v01 preservados em `incoming/BATCH-B4/`.

| Asset | Estado | Master / export | Restrição |
| --- | --- | --- | --- |
| UI-010 EPIC v01 | APPROVED restrito | [m](UI-010_rarity_badge_epic_v01.png) / [e](exports/UI-010_rarity_badge_epic_v01.png) | Simple; sem folha central |
| UI-010 RARE **v02** | **APPROVED** | [m](UI-010_rarity_badge_rare_v02.png) / [e](exports/UI-010_rarity_badge_rare_v02.png) | ≈5% menor que COMMON/EPIC aceito; sem v03 |
| UI-010 LEGENDARY **v02** | **APPROVED** | [m](UI-010_rarity_badge_legendary_v02.png) / [e](exports/UI-010_rarity_badge_legendary_v02.png) | sem v03 |
| UI-017 v02 | APPROVED restrito | [m](UI-017_shop_item_frame_v02.png) / [e](exports/UI-017_shop_item_frame_v02.png) | Só 9-slice (85/45 px); ~7:1 aceito |
| UI-018 TOTAL | APPROVED restrito | [m](UI-018_balance_row_total_v01.png) / [e](exports/UI-018_balance_row_total_v01.png) | Cápsula fixa |
| UI-019 TAIL | APPROVED restrito | [m](UI-019_dialog_tail_v01.png) / [e](exports/UI-019_dialog_tail_v01.png) | Sobreposta ao painel |
| UI-025 RARE / EPIC / LEGENDARY | APPROVED restrito | [r](UI-025_rarity_card_frame_rare_v01.png) [e](UI-025_rarity_card_frame_epic_v01.png) [l](UI-025_rarity_card_frame_legendary_v01.png) | Área segura comum do retrato [120,172,950,789] (1070×1470); **sem SpriteSwap direto presumido**; EPIC: fenda transparente 2 px×~1200 px em x≈55–57 (já no UI-008 aprovado) |

**Estados liberados em 04/11/2026** (baselines NORMAL/INACTIVE aprovados na #47):

| Asset | Estado | Master / export | Restrição |
| --- | --- | --- | --- |
| UI-005_button_secondary_hover_v02.png | APPROVED restrito | [m](UI-005_button_secondary_hover_v02.png) / [e](exports/UI-005_button_secondary_hover_v02.png) | Aprovado apos o NORMAL (#47). Realce sutil vs NORMAL; distinguir DISABLED na Unity. |
| UI-005_button_secondary_pressed_v01.png | APPROVED restrito | [m](UI-005_button_secondary_pressed_v01.png) / [e](exports/UI-005_button_secondary_pressed_v01.png) | Aprovado apos o NORMAL (#47). |
| UI-005_button_secondary_disabled_v01.png | APPROVED restrito | [m](UI-005_button_secondary_disabled_v01.png) / [e](exports/UI-005_button_secondary_disabled_v01.png) | Aprovado apos o NORMAL (#47); a Unity deve distinguir claramente o DISABLED (texto/alpha). |
| UI-006_tab_active_v01.png | APPROVED restrito | [m](UI-006_tab_active_v01.png) / [e](exports/UI-006_tab_active_v01.png) | Aprovado apos o INACTIVE (#47); Simple/preserveAspect; indicador persistente de aba selecionada na Unity. |
| UI-006_tab_hover_v01.png | APPROVED restrito | [m](UI-006_tab_hover_v01.png) / [e](exports/UI-006_tab_hover_v01.png) | Aprovado apos o INACTIVE (#47); Simple/preserveAspect; quase igual ao ACTIVE: nao usar como unico indicador de selecao. |

**Superados (preservados, não usar):** UI-010 RARE/LEGENDARY v01 e UI-005 HOVER v01 (REWORK).

Config Unity (só exports): Sprite Single, Full Rect, PPU 100, Bilinear, Clamp, sem mipmaps, Uncompressed, alphaIsTransparency, NPOT None, maxTextureSize 4096, Read/Write off; UI-017/UI-018: Sliced, borders 85/45/85/45 px, `pixelsPerUnitMultiplier` = altura do export ÷ altura renderizada. Não aplicar aos masters.
