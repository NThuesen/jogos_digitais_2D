# Direção visual vigente — Neko Maid Inc.

**Estado: ACEITO pelo responsável em 03/11/2026.** Esta é a referência operacional para arte e apresentação, prevalecendo sobre os briefs exploratórios de outubro. **Não** altera economia, RNG, recrutamento, saves ou o GDD das mecânicas.

## Decisões definitivas

1. **Personagens:** as nove identidades Suzu, Hina, Akari, Momo, Reina, Kaede, Nene, Ichika e Yoru usam **anime 2D ilustrado/chibi**, preservando o máximo possível dos desenhos, cores, transparência e sombreamento dos frames originais. Não converter para pixel art nem aplicar quantização/hard alpha para combinar com o cenário atual. Não regenerar imagens/vídeos sem uma necessidade e aprovação específicas.
2. **Escala na gameplay:** a restrição é caber e permanecer visível em tela, sem tapar o HUD. Resolução da textura e tamanho no mundo são independentes; preferir ajustar escala, PPU, câmera e layout, sem destruir detalhes dos originais. Validar em **960×600 e 1280×720**, inclusive no desktop WebGL.
3. **Restaurante:** o cenário e os móveis atuais (inclusive os procedurais/pixelados) são provisórios. A próxima direção é um **café cozy anime 2D ilustrado**, desenhado para harmonizar com as maids, e não o contrário. O redesign do ambiente **não foi implementado nem possui layout final aprovado**; a pessoa responsável aprovará decisões artísticas materiais.
4. **Menus/UI:** podem ter linguagem própria, inspirada em visual novel/management sim, diferente da gameplay. Portraits expressivos e Maid Cards ilustrados são **trabalho futuro**, não uma funcionalidade já entregue. Nome, raridade, stats, preço e botões continuam sendo UI dinâmica e legível, não texto impresso em imagens.
5. **Identidade e limites atuais:** os nove walk sheets ilustrados estão integrados por `MaidAnimCatalog`, mediante correspondência de **nome e arquétipo**, e foram validados em turno real/QA WebGL no [PR #30](https://github.com/NThuesen/jogos_digitais_2D/pull/30) (merge `f71b4db`). Isso **não** significa que uma empresa nova comece com as nove maids: o recrutamento normal ainda pode gerar personagens procedurais. Não existe `visual_id` persistido; equipamentos usam fallback procedural; Idle/Carry/Cook/Tired ainda não são animações ilustradas independentes. Suzu e Kaede têm limitações de loop e há deslizamento dos pés.
6. **Desempenho:** a build WebGL do ART-04 reportou cerca de **50,8 MB de texturas RGBA32 com mipmaps** para os nove walk sheets; a memória residente de GPU no navegador não foi medida. O responsável decidiu **não forçar compressão de bloco por enquanto**. Revisitar com medições, não com estimativas apresentadas como fato.

## Execução aprovada — estratégia B

| Marco | Escopo | Estado |
| --- | --- | --- |
| 1. Contratos | Atualizar referências de estilo, roadmap, status e escopo; preservar specs anteriores como histórico. | Documentação desta revisão |
| 2. UI-01 prioritário | Reusar a infraestrutura `Layout_Check` do [PR #31](https://github.com/NThuesen/jogos_digitais_2D/pull/31); verificar clipping, colisões, scroll e cliques; corrigir **Loja, Balanço e Elenco** em ambas as resoluções, sem redesign. Painel 9-sliced somente se houver defeito reproduzido. | Pendente |
| 3. Regressão funcional | Revisar Empresa, Escalação, Caixas, Menu, Salão, turno, Dialogs e Falência; corrigir apenas falhas relevantes e registrar cobertura incompleta. Preservar a câmera/HUD integrados no PR #30. | Pendente |
| 4. Redesign artístico | Em entregas separadas, criar restaurante ilustrado compatível com as maids; depois trabalhar menus, portraits e cards. Usar os testes de UI-01 para prevenir regressões. | Pendente |

**UI-01 é estabilização da interface existente, não arte final.** Os **40 estados** da spec histórica são uma meta de cobertura incremental, **não um bloqueio** para iniciar correções prioritárias nem um critério para declarar que todos foram testados. Sempre registrar precisamente os estados verificados e `PASS / FAIL / NOT RUN`. A spec histórica aprovada anteriormente está em [`.kiro/specs/ui-01-layout/`](../../.kiro/specs/ui-01-layout/requirements.md); preservar os fatos, testes e decisões históricas, mas aplicar o escopo vigente deste documento.

## Responsabilidades e proteção

- **Codex:** correções de layout, testes e regressões UI-01; tarefas técnicas de UI-02 quando autorizadas.
- **Claude Code:** direção visual, produção/integração artística e revisão crítica, trabalhando em arquivos distintos quando houver paralelismo.
- **Responsável:** aprova aparência final e decisões estruturais. Nenhuma escolha estética nova deve ser presumida a partir de arte histórica.
- Preservar `Neko.Sim`, o save, `.meta`/GUIDs, a proteção de testes/CI e o fluxo branch → PR → integração. Os testes já adicionados pelo PR #31 agora são arquivos protegidos; alterações neles exigem evidência independente e exceção administrativa específica, **sem desativar guard**.

## Fontes de verdade e histórico

- [Walk sheets ilustrados e limitações](maid-walk/README.md) · [proveniência dos assets](SUZU_ASSET_PROVENANCE.md).
- [Contrato de identidade / portrait + chibi](MAID_VISUAL_DECISION.md) · [UI-01 atual](../roadmap/UI-01_LAYOUT.md) · [UI-02 futuro](../roadmap/UI-02_SCREEN_POLISH.md).
- [Direção cozy pixel-art + visual novel de 28/10](COZY_VISUAL_NOVEL_DIRECTION.md), [ART-01/ART-01D](../roadmap/ART-01_STYLE.md) e [prompts Hina v2](prompt-pipeline/README.md) são **registros históricos**; referências à obrigação de pixel art ou a gates de uma única maid **não substituem a direção vigente**.
