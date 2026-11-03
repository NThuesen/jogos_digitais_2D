# 04 — Botão primário (duas famílias, quatro estados por família)

Este arquivo é um **ÍNDICE**, não um prompt para substituir `[STATE]`. Clique em cada prompt abaixo: **ele já tem anexos reais e o PNG de saída definido**. O NORMAL ornamental v01 já foi gerado localmente, mas está em REWORK; HOVER/PRESSED/DISABLED não foram gerados. O NORMAL compacto foi gerado, passou pelo QA da PR #42 e recebeu aprovação de produto do responsável; a formalização no registry/merge da PR #42 é independente da documentação destes novos prompts.

## Decisão de produto vigente — famílias separadas

- **ORNAMENTAL GRANDE (~260×88):** `UI-004_button_primary_normal_v01.png`; QA [PR #40](https://github.com/NThuesen/jogos_digitais_2D/pull/40) resultou `REWORK` nos tamanhos atuais. O bom resultado relativo em 260×88 **não é aprovação**, e 768×256/WebGL não foram homologados.
- **COMPACTA SIMPLIFICADA (44–54):** NORMAL gerado, limpo e testado na [PR #42](https://github.com/NThuesen/jogos_digitais_2D/pull/42), com aprovação de produto do responsável (a formalização/integração têm gates próprios). Os novos prompts de estados compactos usam o **NORMAL limpo local 2072×380**, nunca o ornamental ou o RAW 2172×724. Foco em `150×44`, `120×44` e `230×54`.
- **Abas:** permanecem componentes específicos da UI-006, não variantes do ornamental.

A divisão foi **aprovada como arquitetura visual**; o NORMAL compacto foi aprovado separadamente pelo responsável. Outros estados permanecem `PENDING` até geração, revisão e QA. [Tutorial passo a passo](../TUTORIAL.md#passo-2a--gerar-agora-o-botão-compacto-normal-nova-variante-ui-004).

## Botão compacto — novos prompts HOVER/PRESSED/DISABLED

**Referência obrigatória para todos:** `art_working/ui_modular/blender_clean/UI-004_button_primary_compact_normal_v01.png` (2072×380, SHA-256 `c82d06142b3953a97b9654d8b7bf53e611679a6ffb4822a3b268dff239c17eb6`). O UI-001 é referência secundária opcional. Salve cada saída em `raw_gpt/` com o nome exato.

| Estado | Prompt pronto | Nome exato |
|---|---|---|
| NORMAL | [COMPACT NORMAL](variants/04_BUTTON_PRIMARY_COMPACT_NORMAL.md) — histórico de geração/QA | `UI-004_button_primary_compact_normal_v01.png` |
| HOVER | [COMPACT HOVER](variants/04_BUTTON_PRIMARY_COMPACT_HOVER.md) | `UI-004_button_primary_compact_hover_v01.png` |
| PRESSED | [COMPACT PRESSED](variants/04_BUTTON_PRIMARY_COMPACT_PRESSED.md) | `UI-004_button_primary_compact_pressed_v01.png` |
| DISABLED | [COMPACT DISABLED](variants/04_BUTTON_PRIMARY_COMPACT_DISABLED.md) | `UI-004_button_primary_compact_disabled_v01.png` |

Gere **uma imagem por vez** e preserve a geometria do NORMAL (mesmo canvas/silhueta/cantos/bandas para 9-slice). Se a geração mudar a forma, **não aprove**: uma derivação de iluminação/tint no Blender/Unity pode ser tecnicamente mais segura. Esses três estados novos ainda **não foram gerados nem integrados**.

## Botão ornamental original — instruções históricas (não repetir agora)

1. Abra **NORMAL** e anexe os PNGs existentes indicados nele. Anexe também o painel aprovado `docs/art/ui-modular/approved/UI-001_panel_large_v01.png` (existe no Git). Gere só **uma imagem** e renomeie `UI-004_button_primary_normal_v01.png`.
2. Abra **HOVER**. Anexe os PNGs indicados **e o normal já aprovado**. Gere e salve `UI-004_button_primary_hover_v01.png`.
3. Abra **PRESSED**; anexe o NORMAL e salve `UI-004_button_primary_pressed_v01.png`.
4. Abra **DISABLED**; anexe o NORMAL e salve `UI-004_button_primary_disabled_v01.png`.

**São quatro prompts diferentes, quatro imagens diferentes. Não faça uma imagem com quatro botões.** Se o contorno mudar entre estados, ajuste/derive os efeitos a partir do normal no Blender/Unity. O clique é Unity `Button`.


| Ordem | Prompt completo | Nome EXATO ao salvar |
|---|---|---|
| 1 | [`04_BUTTON_PRIMARY_DISABLED.md`](variants/04_BUTTON_PRIMARY_DISABLED.md) | `UI-004_button_primary_disabled_v01.png` |
| 2 | [`04_BUTTON_PRIMARY_HOVER.md`](variants/04_BUTTON_PRIMARY_HOVER.md) | `UI-004_button_primary_hover_v01.png` |
| 3 | [`04_BUTTON_PRIMARY_NORMAL.md`](variants/04_BUTTON_PRIMARY_NORMAL.md) | `UI-004_button_primary_normal_v01.png` |
| 4 | [`04_BUTTON_PRIMARY_PRESSED.md`](variants/04_BUTTON_PRIMARY_PRESSED.md) | `UI-004_button_primary_pressed_v01.png` |

Após baixar, **renomeie o PNG** conforme a terceira coluna e salve em `art_working/ui_modular/raw_gpt/`. O nome do download pode ser definido automaticamente pelo ChatGPT. Os arquivos aprovados só existirão depois do gate do responsável. [Tutorial](../TUTORIAL.md) · [Todos os nomes](../OUTPUT_MANIFEST.md).
