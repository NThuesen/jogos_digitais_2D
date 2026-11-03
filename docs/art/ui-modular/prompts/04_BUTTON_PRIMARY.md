# 04 — Botão primário (4 estados)

Este arquivo é um **ÍNDICE**, não um prompt para substituir `[STATE]`. Clique em cada prompt abaixo: **ele já tem anexos reais e o PNG de saída definido**. O NORMAL ornamental v01 já foi gerado localmente, mas está em REWORK; HOVER/PRESSED/DISABLED não foram gerados. O COMPACT NORMAL será a próxima imagem.

## Decisão de produto vigente — famílias separadas

- **ORNAMENTAL GRANDE (~260×88):** `UI-004_button_primary_normal_v01.png`; QA [PR #40](https://github.com/NThuesen/jogos_digitais_2D/pull/40) resultou `REWORK` nos tamanhos atuais. O bom resultado relativo em 260×88 **não é aprovação**, e 768×256/WebGL não foram homologados.
- **COMPACTA SIMPLIFICADA (44–54):** gerar **agora somente** [`04_BUTTON_PRIMARY_COMPACT_NORMAL.md`](variants/04_BUTTON_PRIMARY_COMPACT_NORMAL.md) → `UI-004_button_primary_compact_normal_v01.png`, atualmente `PENDING`. Foco em `150×44` (compra da Loja com preço dinâmico), `120×44` (navegação) e `230×54` (teste adicional).
- **Abas:** permanecem componentes específicos da UI-006, não variantes do ornamental.

A divisão foi **aprovada como arquitetura visual**, não como aprovação de assets. Não gerar estados da compacta antes de revisar o NORMAL. [Tutorial passo a passo](../TUTORIAL.md#passo-2a--gerar-agora-o-botão-compacto-normal-nova-variante-ui-004).

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
