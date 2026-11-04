# Direção visual vigente — Neko Maid Inc.

**Estado: ACEITO pelo responsável em 03/11/2026.** Esta é a referência operacional para arte e apresentação, prevalecendo sobre os briefs exploratórios de outubro. **Não** altera economia, RNG, recrutamento, saves ou o GDD das mecânicas.

## Decisões definitivas

1. **Personagens:** as nove identidades Suzu, Hina, Akari, Momo, Reina, Kaede, Nene, Ichika e Yoru usam **anime 2D ilustrado/chibi**, preservando o máximo possível dos desenhos, cores, transparência e sombreamento dos frames originais. Não converter para pixel art nem aplicar quantização/hard alpha para combinar com o cenário atual. Não regenerar imagens/vídeos sem uma necessidade e aprovação específicas.
2. **Escala na gameplay:** a restrição é caber e permanecer visível em tela, sem tapar o HUD. Resolução da textura e tamanho no mundo são independentes; preferir ajustar escala, PPU, câmera e layout, sem destruir detalhes dos originais. Validar em **960×600 e 1280×720**, inclusive no desktop WebGL.
3. **Restaurante:** o cenário e os móveis atuais (inclusive os procedurais/pixelados) são provisórios. A próxima direção é um **café cozy anime 2D ilustrado**, desenhado para harmonizar com as maids, e não o contrário. O redesign do ambiente **não foi implementado nem possui layout final aprovado**; a pessoa responsável aprovará decisões artísticas materiais.
4. **Menus/UI:** podem ter linguagem própria, inspirada em visual novel/management sim, diferente da gameplay. Portraits expressivos e Maid Cards ilustrados são **trabalho futuro**, não uma funcionalidade já entregue. Nome, raridade, stats, preço e botões continuam sendo UI dinâmica e legível, não texto impresso em imagens.
5. **Identidade e limites atuais:** os nove walk sheets ilustrados estão integrados por `MaidAnimCatalog`, mediante correspondência de **nome e arquétipo**, e foram validados em turno real/QA WebGL no [PR #30](https://github.com/NThuesen/jogos_digitais_2D/pull/30) (merge `f71b4db`). Isso **não** significa que uma empresa nova comece com as nove maids: o recrutamento normal ainda pode gerar personagens procedurais. Não existe `visual_id` persistido; equipamentos usam fallback procedural; Idle/Carry/Cook/Tired ainda não são animações ilustradas independentes. Suzu e Kaede têm limitações de loop e há deslizamento dos pés.
6. **Desempenho:** a build WebGL do ART-04 reportou cerca de **50,8 MB de texturas RGBA32 com mipmaps** para os nove walk sheets; a memória residente de GPU no navegador não foi medida. O responsável decidiu **não forçar compressão de bloco por enquanto**. Revisitar com medições, não com estimativas apresentadas como fato.

## Decisão de produção aprovada — imagens modulares + interatividade Unity (4A)

**ACEITO pelo responsável em 03/11/2026:** produzir os elementos visuais do restaurante e da UI preferencialmente como **assets de imagem originais gerados com GPT, sob prompts específicos e referências aprovadas**, separando peças reutilizáveis; **Unity constrói a interface e define a interatividade**. Esta decisão é de **arquitetura de produção**, **não** a aprovação final de paleta, forma de moldura, fonte, material, composição ou assets individuais.

- **Gerar separadamente, quando fizer sentido:** fundo e camadas de cenário; mobiliário/props; molduras, painéis, abas, ornamentos, ícones, botões gráficos, estados visuais, faixas de raridade e portraits. Não obrigar a gerar uma imagem para cada detalhe que um componente nativo resolve melhor. **Uma tela inteira desenhada como PNG não é a interface funcional**; somente um mockup ou fundo cênico, nunca substituto de componentes.
- **Unity controla:** `Button` e eventos, hitboxes, hover/pressed/disabled, layouts/anchors, máscaras/scroll, navegação, estados, nomes, textos/localização, stats, preços, raridade, moedas e dados dinâmicos. Esses conteúdos **não são escritos dentro das imagens**. A imagem fornece superfície visual, nunca lógica de jogo.
- **Transparência:** preferir PNG com alpha verdadeiro para imagens sobrepostas. Se o GPT fornecer fundo opaco/chroma, o responsável pode remover o fundo no **Blender** (técnica já usada nos frames das maids), ou agentes podem usar processamento equivalente. O critério de aceite é a **qualidade do PNG final**: transparência real, bordas sem halo/contaminação, dimensões e escala coerentes; não impor qual software foi usado.
- **Reutilização e dimensionamento:** partes simples e bordas apropriadas podem usar `Image.Type.Sliced` / 9-slice na Unity; ornamentos de canto, gemas e detalhes assimétricos não devem ser esticados ou cortados. Gerar assets em separado ou combiná-los em camadas, com padding e áreas seguras para conteúdo variável. Preservar original, versões aprovadas, nomenclatura e GUIDs; evitar uma biblioteca inteira antes de validar um kit mínimo em Unity.
- **Restaurante interativo:** elementos visuais modulares podem representar móveis e ambiente, mas grid, cliques, posicionamento, colisão, oclusão/sorting, UI de estoque, movimentação e economia permanecem controlados pelo jogo. Arte não pode ocultar tiles clicáveis ou prejudicar as maids.
- **WebGL/QA:** avaliar nitidez e legibilidade em **960×600 e 1280×720**, dispositivos desktop, foco/hover/cliques, alpha e performance/memória reais. O ART-04 já consome ~50,8 MB de texturas na build de referência; não assumir que mais imagens grandes serão gratuitas nem impor compressão sem teste e aprovação.
- **Aprovação:** primeiro um **kit piloto original** e uma tela/cena representativa; o responsável aprova o visual real antes da produção em lote. Se um mockup gerado parecer bom mas falhar em runtime, prevalecem os resultados Unity. Evitar retrabalhar as nove maids aprovadas.

### Referência estética para discussão, não decisão automática

A [style reference do estudo ART-01D](art-01/D-pixel-cafe/reference/style_anchor_reference.png), seu [UI kit D4](art-01/D-pixel-cafe/incoming/D4_ui_kit.png) e o [parecer técnico](art-01/D-pixel-cafe/EVALUATION_FINAL.md) continuam **referências de atmosfera e composição para a fase 4A**, não assets finais aprovados nem ordem para usar pixel art. É permitido aproveitar a ideia de café acolhedor, luz quente, molduras ornamentais e linguagem visual novel; **pixelização, paleta exata, materialidade, densidade decorativa e composição final seguem em aberto** para decisão com o responsável.

**Biblioteca de prompts aprovada como infraestrutura (sem novos assets finais):** [UI modular GPT → Blender → Unity](ui-modular/README.md), [tutorial e anexos](ui-modular/TUTORIAL.md), [prompts individuais](ui-modular/prompts/), [registro de aprovação](ui-modular/ASSET_REGISTRY.md). Respeitar o gate de piloto; a paleta específica ainda não foi aprovada.

## Execução aprovada — estratégia B

| Marco | Escopo | Estado |
| --- | --- | --- |
| 1. Contratos | Atualizar referências de estilo, roadmap, status e escopo; preservar specs anteriores como histórico. | Documentação desta revisão |
| 2. UI-01 prioritário | Reusar a infraestrutura `Layout_Check` do [PR #31](https://github.com/NThuesen/jogos_digitais_2D/pull/31); verificar clipping, colisões, scroll e cliques; corrigir **Loja, Balanço e Elenco** em ambas as resoluções, sem redesign. | **Concluído** ([PR #33](https://github.com/NThuesen/jogos_digitais_2D/pull/33)) |
| 3. Regressão funcional | Revisar Empresa, Escalação, Caixas, Menu, Salão, turno, Dialogs e Falência; corrigir apenas falhas relevantes e registrar cobertura incompleta. | **Concluído** ([PR #34](https://github.com/NThuesen/jogos_digitais_2D/pull/34)); 25 estados de Layout_Check no total, não os 40 históricos |
| 4. Redesign artístico | **4A:** decidir linguagem estética juntos; **4B:** validar kit modular piloto com GPT/Blender/Unity; depois restaurante, menus, portraits e cards em PRs específicos. | **4A em discussão**; produção e layouts finais não aprovados |

**UI-01 é estabilização da interface existente, não arte final.** Os **40 estados** da spec histórica são uma meta de cobertura incremental, **não um bloqueio** para iniciar correções prioritárias nem um critério para declarar que todos foram testados. Sempre registrar precisamente os estados verificados e `PASS / FAIL / NOT RUN`. A spec histórica aprovada anteriormente está em [`.kiro/specs/ui-01-layout/`](../../.kiro/specs/ui-01-layout/requirements.md); preservar os fatos, testes e decisões históricas, mas aplicar o escopo vigente deste documento.

## Responsabilidades e proteção## Decisão adicional de produto — duas apresentações de Maid Card (04/11/2026)

**APROVADO pelo responsável:** a **Escalação** deve usar uma apresentação **simplificada**, enquanto a apresentação **detalhada** preserva portrait e informações dinâmicas em contextos maiores. A mesma maid/dados alimentam ambas; **não** criar duas entidades, novos portraits ou alterar economia/saves.

- **UI-008 existente:** `UI-008_maid_card_compact_v01.png` (RAW/CLEAN local; QA na [PR #44](https://github.com/NThuesen/jogos_digitais_2D/pull/44)) teve **aceite artístico expresso apenas em 150×206 detalhado** e **REWORK em 0,62×**. Não descartar nem renomear o arquivo. Está APPROVED somente nesse limite, não INTEGRATED; [PNGs finais e proveniência](ui-modular/approved/README.md).
- **Novo frame simplificado de Escalação:** `UI-008_maid_card_lineup_minimal_v01.png` (**PENDING, não gerado**), [prompt específico](ui-modular/prompts/variants/08_MAID_CARD_LINEUP_MINIMAL.md). Na referência atual 150×206, a Escalação usa 0,62× (~93×128). Moldura com janela transparente de portrait, faixa simples de nome e no máximo espaço para indicador Unity; sem salary/stats/traits dentro da mini-card. Outras informações ficam em componentes dinâmicos fora dela.
- **UI-009:** `UI-009_maid_card_detail_v01.png` segue **PENDING**, como possível frame expandido de detalhes. Não é pré-requisito para criar o frame simplificado; não confundir com o UI-008 existente, que tem aceite detalhado restrito a 150×206.
- **Integração:** somente após aprovação visual/QA específicos; definir layout/posição do indicador e conteúdo externo em tarefa posterior. Reutilizar dados e sprites, preservar seleção, interação, localização e máscaras. Não integrar automaticamente em outras telas de `MaidCard.Build`.

Esta decisão aprova **o conceito de duas apresentações**, não aprova PNG novo, nem autoriza mudanças de tela/código nesta atualização documental.



- **Codex:** correções de layout, testes e regressões UI-01; tarefas técnicas de UI-02 quando autorizadas.
- **Claude Code:** direção visual, produção/integração artística e revisão crítica, trabalhando em arquivos distintos quando houver paralelismo.
- **Responsável:** aprova aparência final e decisões estruturais. Nenhuma escolha estética nova deve ser presumida a partir de arte histórica.
- Preservar `Neko.Sim`, o save, `.meta`/GUIDs, a proteção de testes/CI e o fluxo branch → PR → integração. Os testes já adicionados pelo PR #31 agora são arquivos protegidos; alterações neles exigem evidência independente e exceção administrativa específica, **sem desativar guard**.

## Fontes de verdade e histórico

- [Walk sheets ilustrados e limitações](maid-walk/README.md) · [proveniência dos assets](SUZU_ASSET_PROVENANCE.md).
- [Contrato de identidade / portrait + chibi](MAID_VISUAL_DECISION.md) · [UI-01 atual](../roadmap/UI-01_LAYOUT.md) · [UI-02 futuro](../roadmap/UI-02_SCREEN_POLISH.md).
- [Direção cozy pixel-art + visual novel de 28/10](COZY_VISUAL_NOVEL_DIRECTION.md), [ART-01/ART-01D](../roadmap/ART-01_STYLE.md) e [prompts Hina v2](prompt-pipeline/README.md) são **registros históricos**; referências à obrigação de pixel art ou a gates de uma única maid **não substituem a direção vigente**.

## Decisão adicional de produto — UI-004 (03/11/2026)

**APROVADO pelo responsável: duas famílias de botão primário**, sem aprovação automática de PNG, export, layout ou integração.

1. **Ornamental grande (~260×88):** uso potencial como CTA de destaque. O `UI-004_button_primary_normal_v01.png` existente foi avaliado na [revisão UI-004](ui-modular/reviews/UI-004_BUTTON_PRIMARY_NORMAL_V01.md) e segue `REWORK`; 768×256 é apenas candidata técnica e WebGL permanece `NOT RUN`.
2. **Compacta simplificada (altura 44–54):** família própria para ações pequenas/frequentes. Próximo passo autorizado: documentar e gerar, manualmente no GPT Web, **apenas** `UI-004_button_primary_compact_normal_v01.png` usando [prompt próprio](ui-modular/prompts/variants/04_BUTTON_PRIMARY_COMPACT_NORMAL.md), priorizando compra real da Loja `150×44` (preços dinâmicos), navegação `120×44` e avaliação adicional em `230×54`.
3. **Abas são peças independentes** (UI-006); não reutilizar o ornamental de forma universal. Não produzir HOVER/PRESSED/DISABLED compactos antes de aprovar NORMAL. Unity continua responsável por textos, preços, controle, interação e estados.

Esta decisão substitui **somente a pendência sobre dividir as famílias** registrada na revisão histórica de UI-004. Não reclassifica aquele asset como `APPROVED`, não altera a arte atual de nenhuma tela e não modifica economia, gameplay, saves ou identidades das maids.

**Aprovação artística posterior — 04/11/2026:** HOVER v02, PRESSED v01, DISABLED v02, UI-002 e UI-003 (respeitando tamanho/texto do QA) aprovados expressamente pelo responsável. UI-008 aceita somente em 150×206 detalhado, REWORK em 0,62×; LINEUP MINIMAL PENDING. [Registro e arquivos aprovados](ui-modular/approved/README.md). Nenhuma integração ou autorização de mudança em telas nesta etapa.
