# Revisão artística consolidada — candidatos PENDING das #47/#49 (04/11/2026)

**Recomendações do assistente; nenhuma aprovação concedida, nada integrado, sem QA novo.** Base: capturas e auditorias já existentes ([#47](UI_BATCH_NEW_2026_11_04.md), [#49](UI_BATCH_V02_2026_11_04.md), [B4/#52](UI_BATCH_B4_2026_11_04.md)). Aprovados do B3/B4 e as seis aprovações originais seguem como registrados em `approved/`.

## Baselines que destravam estados já aceitos
| Candidato | Leitura artística | Recomendação |
| --- | --- | --- |
| **UI-005 NORMAL** | Taupe/rosewood desbotado, contraste baixo; **lê quase como desabilitado** e a diferença para DISABLED (cinza mais frio) é pequena. Silhueta e ornamentos limpos; HOVER v02 coerente. | Aprovar **restrito a secundário** e exigir distinção extra do DISABLED na Unity (cor/alpha do texto). Destrava HOVER v02, PRESSED, DISABLED. |
| **UI-006 INACTIVE** | Claramente menos saturada que ACTIVE; ornamento central de flor/gato no topo. ACTIVE≈HOVER. | Aprovar restrito: Simple/preserveAspect (sem 9-slice), indicador persistente de aba selecionada na Unity. Destrava ACTIVE e HOVER. |

## Demais PENDING
| Candidato | Leitura / risco | Recomendação |
| --- | --- | --- |
| UI-010 COMMON | Marrom/dourado com folha central; é o degrau baixo, aceitável e completa a família (RARE/EPIC/LEGENDARY aprovados). | Aprovar (completa a família de selos). |
| UI-025 COMMON | Faixa intermediária invade a janela da UI-008 detalhada (IoU 97,4%); sem SpriteSwap certificado. | Aprovar restrito com a área segura comum já registrada; completa as molduras. |
| UI-019 PANEL | Coerente com UI-001/007; 9-slice 1000×170 testado. | Aprovar; destrava o TAIL já aprovado. |
| UI-008 LINEUP MINIMAL | Moldura limpa; placa de nome pequena é o risco. | Decisão sua: manter as duas Maid Cards (compact aprovado + lineup minimal) ou só a compact. |
| UI-007 v02 | Laço no centro-topo: título obrigatoriamente abaixo; margens menores que o prompt. Limites de texto em [UI_TEXT_CONSTRAINTS](UI_TEXT_CONSTRAINTS_UI007_UI009_V02.md). | Aprovar restrito (sem 9-slice). |
| UI-009 v02 | Abertura ≈31–33% da largura; laço sobre o topo do retrato; 2 falhas de texto do QA da #49 dependem de autosize. | Aprovar restrito às regras de texto, ou manter opcional. |
| UI-022 v03 (piso) | Seamless, mas laranja saturado; existe variante tone34 mais calma. | Escolher: v03 ou tone34. |
| UI-023 parede | Lê como painel/quadro, não parede; sem perspectiva. | Não aprovar para cenário; no máximo reaproveitar como painel. |
| UI-012 fundo | Ilustração mais rica que o kit; parquet espinha contradiz o piso UI-022. | Aprovar só como fundo de menu (UI-020 já homologado sobre ele). |
| UI-011 BOOK | Detalhes ilegíveis a 24u. | Aprovar restrito 32–44u. |
| UI-007/009/022 **v01** | REWORK, superados pelos v02/v03. | Manter só como histórico. |

## Já aprovados e integráveis nas telas reais (nenhum integrado ainda)
Pré-requisito comum: **nenhum PNG está em `Assets/`** — falta importar os exports com a configuração do `B3_README`/`B4_README` e trocar o desenho por código. Requer sua autorização.
- **Loja:** UI-017 v02 (9-slice), ícones UI-011 (9), UI-018 NORMAL/TOTAL.
- **Painéis/botões:** UI-001/002/003, UI-004 HOVER/PRESSED/DISABLED (NORMAL compacto conforme registro anterior).
- **Elenco/Maid Card:** UI-008 compact; selos e molduras RARE/EPIC/LEGENDARY **só depois** de aprovar os COMMON.
- **Menu:** UI-020 logo (somente sobre o UI-012 testado).
- **Bloqueados:** UI-005 estados (NORMAL), UI-006 (INACTIVE), UI-019 TAIL (PANEL), raridades completas (COMMON).
