# Tutorial de geração — anexos exatos, prompts prontos, nomes definidos

**Você não precisa inventar nomes nem preencher `[STATE]`.** Cada arquivo em `prompts/` ou `prompts/variants/` traz **(1) anexos com nomes e links reais; (2) um bloco único pronto para copiar; (3) nome e pasta exatos do PNG final**. O [manifesto](OUTPUT_MANIFEST.md) contém os **99 nomes**. O UI-001 está aprovado e versionado; o NORMAL ornamental de UI-004 permaneceu REWORK; o COMPACT NORMAL foi gerado e validado na PR #42, com aprovação expressa do responsável, com registro APPROVED incorporado pela PR #42 (sem integração Unity). Os estados compactos HOVER/PRESSED/DISABLED são os próximos candidatos. O registro distingue prompts, PNGs disponíveis, aprovados e integrados.

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
7. Compare a imagem isolada e o resultado na Unity com conteúdo e 9-slice em 960×600 / 1280×720. Para o UI-001 isso já foi feito: o PNG limpo aprovado está versionado em `docs/art/ui-modular/approved/UI-001_panel_large_v01.png` (SHA-256 `ba300ece…ee85f8`, ver [revisão](reviews/UI-001_PANEL_LARGE_V01.md)). Para os demais, só quando o responsável aprovar, copie para `art_working/ui_modular/approved/` (local), registre [ASSET_REGISTRY.md](ASSET_REGISTRY.md) e prossiga.

## Passo 2 — Botão ornamental original: QUATRO estados planejados (histórico)

**Este é o fluxo ornamental original, mantido para consulta; não o execute em lote.** O NORMAL ornamental v01 já foi gerado e reprovou como botão universal na [revisão UI-004](reviews/UI-004_BUTTON_PRIMARY_NORMAL_V01.md). Seus três estados restantes estão PENDING. **A próxima geração vigente é o COMPACT NORMAL, no Passo 2A abaixo.** Um prompt por estado, um PNG por resultado. Não mande quatro prompts juntos nem peça uma imagem com quatro botões. A ordem certa:

| Ordem | Clique no prompt PRONTO | Anexe imagens reais e futuras como indicado | Salve com nome EXATO |
| --- | --- | --- | --- |
| 1 | [**NORMAL**](prompts/variants/04_BUTTON_PRIMARY_NORMAL.md) | Existentes: `style_anchor_reference.png`, `D4_ui_kit.png`, `after_credits_MN-2_menu_960x600.png`. Adicione `UI-001_panel_large_v01.png` após aprovação. | `UI-004_button_primary_normal_v01.png` |
| 2 | [**HOVER**](prompts/variants/04_BUTTON_PRIMARY_HOVER.md) | Mesmos existentes + **o botão NORMAL aprovado** `UI-004_button_primary_normal_v01.png`. | `UI-004_button_primary_hover_v01.png` |
| 3 | [**PRESSED**](prompts/variants/04_BUTTON_PRIMARY_PRESSED.md) | Mesmos existentes + **o botão NORMAL aprovado**. | `UI-004_button_primary_pressed_v01.png` |
| 4 | [**DISABLED**](prompts/variants/04_BUTTON_PRIMARY_DISABLED.md) | Mesmos existentes + **o botão NORMAL aprovado**. | `UI-004_button_primary_disabled_v01.png` |

**Fluxo concreto:** primeiro gere NORMAL → salve em `raw_gpt/` → limpe se necessário → aprove → coloque em `approved/` → só então HOVER, PRESSED e DISABLED. O único PNG aprovado até agora é o UI-001 (versionado em `docs/art/ui-modular/approved/`); o NORMAL ornamental v01 existe apenas como bruto/limpo local e está em REWORK, sem aprovação ou integração. Quando um prompt pede uma imagem "futura", ele não está dizendo que você já deveria encontrá-la no GitHub.

Ao gerar as variantes, peça para manter **o mesmo contorno e tamanho da imagem NORMAL**; se a IA alterar demais o formato entre estados, a solução técnica melhor é construir variações de iluminação/tint no Blender ou Unity sobre o PNG NORMAL, sem sacrificar alinhamento. A Unity controla cliques, hover e estado via `Button`.

## Passo 2A — Gerar AGORA o botão compacto NORMAL (nova variante UI-004)

**Decisão do responsável em 03/11/2026:** duas famílias de botões — ornamental grande e compacto simplificado. Esta é a **próxima imagem a produzir no GPT Web**; a família compacta é distinta da ornamental e **não** altera o estado REWORK do botão antigo.

1. Abra [**04_BUTTON_PRIMARY_COMPACT_NORMAL.md**](prompts/variants/04_BUTTON_PRIMARY_COMPACT_NORMAL.md), que já contém anexos exatos, prompt inglês completo para copiar e caminho final.
2. Baixe e anexe no GPT Web **quatro imagens reais** conforme o novo arquivo: `UI-001_panel_large_v01.png` (arte aprovada), `after_LJ-1_loja-moveis_960x600.png` (dimensões/contexto real), `style_anchor_reference.png` (atmosfera, não pixel art) e `unity_sliced_1280x720.png` (exemplo negativo do QA ornamental antigo). Não é necessário ter o PNG ornamental bruto local.
3. Copie **somente o bloco `text`** do prompt compacto. Peça **UM botão NORMAL**, isolado, sem texto/ícone e com centro creme sólido, área para preços dinâmicos, moldura rosewood/terracota e dourado discreto. Não gere prancha nem outros estados.
4. Baixe e renomeie para **`UI-004_button_primary_compact_normal_v01.png`**, salvando em `art_working/ui_modular/raw_gpt/`. Limpe alpha apenas se necessário em `blender_clean/`, preservando o interior creme e o original bruto.
5. Mostre o PNG para revisão humana antes de aprovar. Claude Code avalia coesão/legibilidade visual; depois Codex testa os tamanhos reais **150×44 da Loja (preços, não o rótulo "Comprar")**, **120×44** e **230×54** nas resoluções `960×600` e `1280×720`. Sem conclusões de WebGL antes de uma execução real.
6. Registre o resultado no [ASSET_REGISTRY.md](ASSET_REGISTRY.md) (`RAW` / `CLEAN` / `UNITY_QA` / `REWORK` / `APPROVED`, conforme evidência); a existência deste tutorial **não aprova nem integra** o asset. HOVER, PRESSED e DISABLED compactos só após aprovação do NORMAL correspondente.

**Nome e status atualizados após geração:** `UI-004_button_primary_compact_normal_v01.png` — gerado e limpo localmente; QA da [PR #42](https://github.com/NThuesen/jogos_digitais_2D/pull/42) recomendou aprovação, confirmada pelo responsável. A aprovação formal consta da PR #42 integrada; o asset ainda não está INTEGRATED na Unity. [Manifesto de saídas](OUTPUT_MANIFEST.md).

## Passo 2B — Gerar HOVER, PRESSED e DISABLED do botão **compacto** aprovado

**Não utilize os três prompts ornamentais originais** (`04_BUTTON_PRIMARY_HOVER/PRESSED/DISABLED.md`): eles pedem o botão antigo `UI-004_button_primary_normal_v01.png`, que continua em REWORK.

1. Na pasta local `art_working/ui_modular/blender_clean/`, localize **`UI-004_button_primary_compact_normal_v01.png`**, versão limpa **2072×380** e SHA-256 **`c82d06142b3953a97b9654d8b7bf53e611679a6ffb4822a3b268dff239c17eb6`**. Esse é o único anexo obrigatório de geometria. Uma cópia byte a byte idêntica em `approved/` é equivalente. **Não anexe o RAW 2172×724 nem a redução 768×141 como master**.
2. Opcional: anexe o painel [`UI-001_panel_large_v01.png`](approved/UI-001_panel_large_v01.png) para estilo. **O NORMAL compacto prevalece sobre ele** em cor, contorno e geometria. Não é necessário reenviar capturas históricas/pixel art.
3. Gere **uma única imagem por prompt**, preferencialmente pedindo **edição da imagem NORMAL anexada** no GPT Web:
   - [COMPACT HOVER](prompts/variants/04_BUTTON_PRIMARY_COMPACT_HOVER.md) → `UI-004_button_primary_compact_hover_v01.png`
   - [COMPACT PRESSED](prompts/variants/04_BUTTON_PRIMARY_COMPACT_PRESSED.md) → `UI-004_button_primary_compact_pressed_v01.png`
   - [COMPACT DISABLED](prompts/variants/04_BUTTON_PRIMARY_COMPACT_DISABLED.md) → `UI-004_button_primary_compact_disabled_v01.png`
4. Baixe/renomeie os PNGs em `art_working/ui_modular/raw_gpt/`, sem sobrescrever o NORMAL ou o ornamental. Se a IA mudar silhueta, margens, espessura da moldura ou banda 9-slice, **rejeite/regere o estado**; alternativa: derivar iluminação/tint a partir do NORMAL no Blender/Unity.
5. Mostre cada variante para revisão visual. Somente depois peça ao Codex **limpeza e QA incremental** dos estados em 120×44, 150×44 e 230×54 (960×600 e 1280×720). Cada estado permanece `PENDING`/RAW até evidências e aprovação próprias; Unity controla interação real.

**Não existe homologação automática dos três PNGs só porque NORMAL passou no QA.** A família ornamental grande continua independente e em REWORK.

## Passo 3 — Maid Card compacto (terceira peça do piloto)

1. Abra [**08_MAID_CARD_COMPACT.md**](prompts/08_MAID_CARD_COMPACT.md).
2. Anexe `style_anchor_reference.png`, `webgl_chrome_960x600.jpg`, `after_ES-2_escalacao_960x600.png` e **`suzu_walk.png` de `Assets/_Game/Resources/Maids/`**. Anexe também o painel aprovado `docs/art/ui-modular/approved/UI-001_panel_large_v01.png` (já existe no Git).
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

Não é obrigatório gerar todas as 99 imagens. Elas estão **documentadas de antemão** para você poder escolher sem reescrever prompt ou nome.

## Como salvar e tratar transparência

Use sempre o **mesmo nome base** nas pastas `raw_gpt/`, `blender_clean/`, `approved/` e `export_unity/`. São pastas diferentes, portanto podem conter arquivos com o mesmo basename. O GPT pode baixar como `image.png`; **você precisa renomear após download**.

Blender pode remover chroma/fundo; `RGBA` sozinho não remove o fundo. Cheque bordas em fundo claro/escuro, halo, sombras e miolo creme. Para itens de mundo, não apague sombras de contato úteis nem orelhas/cantos finos.

PNG no `art_working/` fica **IGNORADO por Git** (regra preexistente). `.gitkeep` garante que as pastas existam em novos clones. Só depois de aprovar o asset, uma PR específica pode incluí-lo em `docs/art/ui-modular/approved/` ou em `Assets/_Game/Resources/` com `.meta` e proveniência. **Não faça `git add -f` em brutos de produção.**

## Validação por asset

Valide arquivo real, alpha, resolução, 9-slice quando aplicável, legibilidade, real botão com mouse, e screenshot Unity em 960×600 / 1280×720. Medir memória WebGL quando aplicável. Sempre distinguir **ARTE GERADA** vs. **ASSET LIMPO** vs. **UNITY REAL**.

[Voltar ao kit](README.md) · [todos os nomes de PNG](OUTPUT_MANIFEST.md) · [ficha de handoff](ASSET_HANDOFF_TEMPLATE.md).
