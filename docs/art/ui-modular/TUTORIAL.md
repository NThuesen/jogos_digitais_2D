# Tutorial de geração — anexos exatos, prompts prontos, nomes definidos

**Você não precisa inventar nomes nem preencher `[STATE]`.** Cada arquivo em `prompts/` ou `prompts/variants/` traz **(1) anexos com nomes e links reais; (2) um bloco único pronto para copiar; (3) nome e pasta exatos do PNG final**. O [manifesto](OUTPUT_MANIFEST.md) contém os **95 nomes**. **Nenhum desses novos PNGs existe ainda**: o que existe no GitHub hoje são as *referências* antigas e capturas.

## Passo zero — baixar as referências que já EXISTEM

Abra os links e **baixe o arquivo original**, depois anexe pelo ícone de imagem/arquivo na conversa de geração. Links escritos numa mensagem não são substitutos confiáveis para anexos.

| Nome EXATO do anexo real | Caminho exato no repo | Serve para |
| --- | --- | --- |
| [`style_anchor_reference.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/art-01/D-pixel-cafe/reference/style_anchor_reference.png) | `docs/art/art-01/D-pixel-cafe/reference/style_anchor_reference.png` | Direção artística de TODOS os prompts, **sem copiar pixel art** |
| [`D4_ui_kit.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/art-01/D-pixel-cafe/incoming/D4_ui_kit.png) | `docs/art/art-01/D-pixel-cafe/incoming/D4_ui_kit.png` | Estudo antigo de moldura/ornamento, não kit final |
| [`08_modularidade_ui.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/art-01/D-pixel-cafe/review/08_modularidade_ui.png) | `docs/art/art-01/D-pixel-cafe/review/08_modularidade_ui.png` | Estudo antigo de recortes e modularidade |
| [`after_LJ-1_loja-moveis_960x600.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/roadmap/ui-01a/evidence/after_LJ-1_loja-moveis_960x600.png) | `docs/roadmap/ui-01a/evidence/after_LJ-1_loja-moveis_960x600.png` | Espaço real para o painel e frames de Loja |
| [`after_credits_MN-2_menu_960x600.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/roadmap/ui-01b/evidence/after_credits_MN-2_menu_960x600.png) | `docs/roadmap/ui-01b/evidence/after_credits_MN-2_menu_960x600.png` | Composição real do menu e botões |
| [`after_ES-2_escalacao_960x600.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/roadmap/ui-01b/evidence/after_ES-2_escalacao_960x600.png) | `docs/roadmap/ui-01b/evidence/after_ES-2_escalacao_960x600.png` | Proporção real das cartas |
| [`after_EL-2_elenco_960x600.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/roadmap/ui-01a/evidence/after_EL-2_elenco_960x600.png) | `docs/roadmap/ui-01a/evidence/after_EL-2_elenco_960x600.png` | Detalhes do card |
| [`webgl_chrome_960x600.jpg`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/maid-walk/evidence_illustrated/webgl_chrome_960x600.jpg) | `docs/art/maid-walk/evidence_illustrated/webgl_chrome_960x600.jpg` | Escala/contraste das maids ilustradas em WebGL |
| [`suzu_walk.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/Assets/_Game/Resources/Maids/suzu_walk.png) | `Assets/_Game/Resources/Maids/suzu_walk.png` | Sheet **real** para identidade Suzu no piloto |
| [`after_SL-1_salao_960x600.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/roadmap/ui-01b/evidence/after_SL-1_salao_960x600.png) | `docs/roadmap/ui-01b/evidence/after_SL-1_salao_960x600.png` | Ambiente e piso do Salão existente |

**Os arquivos acima existem.** Para as outras maids, seus nomes exatos estão em [references/README.md](references/README.md) e em cada prompt 21_*. Não confundir `Assets/_Game/Resources/Maids/suzu_walk.png` (atual) com o backup histórico em `Resources/Suzu/`.

## Passo 1 — Gerar o painel grande (primeira imagem)

1. Abra [**01_PANEL_LARGE.md**](prompts/01_PANEL_LARGE.md).
2. Anexe os **três PNGs que o próprio prompt enumera**: `style_anchor_reference.png`, `D4_ui_kit.png` e `after_LJ-1_loja-moveis_960x600.png`. Esses são arquivos reais, não algo para você criar.
3. Copie **somente o bloco `text` completo** de `01_PANEL_LARGE.md`. **Não cole um segundo prompt global**, porque as regras já estão dentro dele. Peça para gerar a imagem.
4. Baixe o resultado e **renomeie** o arquivo, se necessário, para **`UI-001_panel_large_v01.png`**.
5. Coloque em `art_working/ui_modular/raw_gpt/UI-001_panel_large_v01.png`. Não substitua esse bruto ao refazer.
6. Caso precise, limpe fundo no Blender e salve **o mesmo nome** em `art_working/ui_modular/blender_clean/`. **Não apague o miolo creme do painel.**
7. Compare a imagem isolada e o resultado na Unity com conteúdo e 9-slice em 960×600 / 1280×720. Só quando o responsável aprovar, copie-a para `art_working/ui_modular/approved/UI-001_panel_large_v01.png` (local), registre [ASSET_REGISTRY.md](ASSET_REGISTRY.md) e prossiga.

## Passo 2 — Botão principal: são QUATRO prompts diferentes

**Sim: um prompt por estado, um PNG por resultado.** Não mande quatro prompts juntos nem peça uma imagem com quatro botões. A ordem certa:

| Ordem | Clique no prompt PRONTO | Anexe imagens reais e futuras como indicado | Salve com nome EXATO |
| --- | --- | --- | --- |
| 1 | [**NORMAL**](prompts/variants/04_BUTTON_PRIMARY_NORMAL.md) | Existentes: `style_anchor_reference.png`, `D4_ui_kit.png`, `after_credits_MN-2_menu_960x600.png`. Adicione `UI-001_panel_large_v01.png` após aprovação. | `UI-004_button_primary_normal_v01.png` |
| 2 | [**HOVER**](prompts/variants/04_BUTTON_PRIMARY_HOVER.md) | Mesmos existentes + **o botão NORMAL aprovado** `UI-004_button_primary_normal_v01.png`. | `UI-004_button_primary_hover_v01.png` |
| 3 | [**PRESSED**](prompts/variants/04_BUTTON_PRIMARY_PRESSED.md) | Mesmos existentes + **o botão NORMAL aprovado**. | `UI-004_button_primary_pressed_v01.png` |
| 4 | [**DISABLED**](prompts/variants/04_BUTTON_PRIMARY_DISABLED.md) | Mesmos existentes + **o botão NORMAL aprovado**. | `UI-004_button_primary_disabled_v01.png` |

**Fluxo concreto:** primeiro gere NORMAL → salve em `raw_gpt/` → limpe se necessário → aprove → coloque em `approved/` → só então HOVER, PRESSED e DISABLED. A pasta `approved/` não contém esse PNG ainda. Quando um prompt pede uma imagem "futura", ele não está dizendo que você já deveria encontrá-la no GitHub.

Ao gerar as variantes, peça para manter **o mesmo contorno e tamanho da imagem NORMAL**; se a IA alterar demais o formato entre estados, a solução técnica melhor é construir variações de iluminação/tint no Blender ou Unity sobre o PNG NORMAL, sem sacrificar alinhamento. A Unity controla cliques, hover e estado via `Button`.

## Passo 3 — Maid Card compacto (terceira peça do piloto)

1. Abra [**08_MAID_CARD_COMPACT.md**](prompts/08_MAID_CARD_COMPACT.md).
2. Anexe `style_anchor_reference.png`, `webgl_chrome_960x600.jpg`, `after_ES-2_escalacao_960x600.png` e **`suzu_walk.png` de `Assets/_Game/Resources/Maids/`**. Anexe também o painel aprovado `UI-001_panel_large_v01.png` se já existir.
3. Gere **somente a moldura vazia, com janela transparente para portrait**. Não coloque uma personagem dentro da moldura.
4. Salve como **`UI-008_maid_card_compact_v01.png`** em `raw_gpt/`, depois `blender_clean/` e `approved/` quando cabível.
5. Teste na Unity com Suzu real atrás da moldura e textos dinâmicos em 960×600 e 1280×720. A geração de portraits é outro gate.

## Passo 4 — Demais componentes sem adivinhações

- [**Botões secundários**](prompts/05_BUTTON_SECONDARY.md): quatro prompts distintos, quatro nomes fixos.
- [**Abas**](prompts/06_TABS_SET.md): três prompts, três PNGs, não uma prancha.
- [**Raridade**](prompts/10_RARITY_BADGES.md): quatro badges; [molduras completas](prompts/25_RARITY_CARD_FRAME.md) são outra família.
- [**Ícones**](prompts/11_ICON_PACK.md): dez prompts individuais com seus próprios nomes.
- [**Arquitetura**](prompts/13_RESTAURANT_BACKGROUND.md), [**props decorativos**](prompts/14_RESTAURANT_PROPS_SET.md), [**paredes**](prompts/23_WALL_SEGMENT.md) e [**placeables reais da Unity**](prompts/24_FURNITURE_SINGLE.md): cada subarquivo tem nome próprio, anexos e destino.
- [**Portraits das nove maids**](prompts/21_MAID_PORTRAIT.md): somente após aprovação específica; cada prompt já indica o PNG real `*_walk.png` e a evidência visual de sua maid.

Não é obrigatório gerar todas as 95 imagens. Elas estão **documentadas de antemão** para você poder escolher sem reescrever prompt ou nome.

## Como salvar e tratar transparência

Use sempre o **mesmo nome base** nas pastas `raw_gpt/`, `blender_clean/`, `approved/` e `export_unity/`. São pastas diferentes, portanto podem conter arquivos com o mesmo basename. O GPT pode baixar como `image.png`; **você precisa renomear após download**.

Blender pode remover chroma/fundo; `RGBA` sozinho não remove o fundo. Cheque bordas em fundo claro/escuro, halo, sombras e miolo creme. Para itens de mundo, não apague sombras de contato úteis nem orelhas/cantos finos.

PNG no `art_working/` fica **IGNORADO por Git** (regra preexistente). `.gitkeep` garante que as pastas existam em novos clones. Só depois de aprovar o asset, uma PR específica pode incluí-lo em `docs/art/ui-modular/approved/` ou em `Assets/_Game/Resources/` com `.meta` e proveniência. **Não faça `git add -f` em brutos de produção.**

## Validação por asset

Valide arquivo real, alpha, resolução, 9-slice quando aplicável, legibilidade, real botão com mouse, e screenshot Unity em 960×600 / 1280×720. Medir memória WebGL quando aplicável. Sempre distinguir **ARTE GERADA** vs. **ASSET LIMPO** vs. **UNITY REAL**.

[Voltar ao kit](README.md) · [todos os nomes de PNG](OUTPUT_MANIFEST.md) · [ficha de handoff](ASSET_HANDOFF_TEMPLATE.md).
