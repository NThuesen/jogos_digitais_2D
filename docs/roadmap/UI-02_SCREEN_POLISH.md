# UI-02 — Evoluir telas com a direção visual aprovada

**Status:** PENDENTE no marco 4 da [estratégia B](../ROADMAP.md); **não** bloqueado pela antiga exploração ART-01. A direção ilustrada foi aprovada e o Walk das nove maids já está integrado (PRs #29/#30); portraits e layout de cada tela continuam sujeitos a aprovação estética.
**Implementação:** Claude Code (arte/crítica) + Codex (UI/integração/QA), em entregas separadas. **Leia:** [direção vigente](../art/CURRENT_VISUAL_DIRECTION.md), [roadmap](../ROADMAP.md) e [Unity](../agent/UNITY.md). ART-01/ART-02 registram decisões históricas, não gates ativos.

## Objetivo

**Método de produção aprovado para UI-02/Marco 4:** GPT gera **peças de imagem separadas**, com prompts específicos e referência estética; Unity faz `Canvas`, `Button`, estados, textos, stats, preços, scroll, hitboxes e layout responsivo. **Jamais tratar uma tela inteira ilustrada como UI funcional.** Exportar PNG com alpha real; remover fundo com Blender é permitido se necessário, mantendo bordas limpas. Componentes com borda ajustável podem usar 9-slice; ornamentos assimétricos não devem ser deformados. Validar um kit piloto antes da produção em lote. Ver [contrato vigente](../art/CURRENT_VISUAL_DIRECTION.md#decisão-de-produção-aprovada--imagens-modulares--interatividade-unity-4a). A estética da referência ART-01D é **inspiração**, não retorno ao pixel art. **Estilo detalhado, paleta, materialidade e assets finais ainda dependem de decisão conjunta.**
Fazer a estética, hierarquia e feedback trabalharem em favor da fantasia **“escalar maids adoráveis e acompanhar o desempenho do café”**. Preservar fluxo e funcionalidade existentes; o trabalho de UI-01 já deve ter resolvido bugs de layout antes de redesenhar.

## Sequência — dividir em PRs independentes, não um PR gigantesco
1. **Elenco + Escalação** (primeiro foco): card expressivo e legível, sensação de coleção, arquitetura de texto separada da ilustração; leitura imediata da posição, raridade, química e papel de cada maid. Preservar espaço para informações de salário/stats.
2. **Empresa (Hub):** hierarquia clara entre economia, time escalado e CTA “abrir o dia”; dar centralidade às personagens sem esconder custo fixo e riscos.
3. **Dia + Salão:** cena mais viva (expressões, animação e microfeedback do trabalho das maids); manter indicadores de execução e controle de velocidade/pular, sem permitir intervenção na simulação durante o dia.
4. **Caixas + Loja:** reforçar sensação de aquisição e valor de uma carta; preservar chances visíveis nas caixas, compra/custos na loja e distinção das duas moedas.
5. **Balanço + Menu:** reforçar identidade e qualidade do feedback; o jogador precisa entender por que o turno foi lucrativo ou não, e continuar com uma ação clara.

O responsável pode escolher uma **fatias menor** para o PR inicial. Uma tela por PR quando houver sobreposição de código/cena ou dificuldade de validação.

## Constraints
- Estilo **aprovado** não é licença para refazer código de base indiscriminadamente. Preferir melhorias localizadas antes de alterar `UiKit.cs`, `Scenery.cs` e `Palette.cs` globalmente.
- **Raridade, texto, stats, nomes, posição e moeda são UI**, não pixels gerados na imagem de portrait. A identidade de maid entre portrait e chibi segue [decisão visual](../art/MAID_VISUAL_DECISION.md).
- Não mudar geração de economia, save, lootbox, chances, seleção e resultados do turno. Mudanças de comportamento exigem decisão separada no GDD.
- Preservar legibilidade dos nove arquétipos, informações de raridade e contrastes, inclusive para cartas de cores distintas. Evitar UI toda na mesma tonalidade marrom; a paleta final dos menus ainda será definida com a aprovação visual das telas; não reativar a escolha pixel art.
- Nova arte/animação não pode tornar performance impraticável em desktop WebGL; avaliar após integração real, não por promessa.
- Não presumir que a existência de `Screen` ou `UiKit` garante ajuste automático em 960×600; conferir em runtime.

## Evidência e aceite por PR
- Lista de telas/estados tocados, comparação antes/depois **960×600**, screenshots reais do Unity, Console relevante, interações demonstradas e resultado de testes apropriados.
- Usuário reconhece maid com rapidez no card e no restaurante; informações e botões legíveis; navegação e regra preservadas; feedback do dia explica decisões.
- Se a alteração for estética material, **aprovação visual do responsável é gate de produto**, distinto de checks GitHub. Não ativar auto-merge de trabalho visual ainda não inspecionado.
- Evitar declarações genéricas como “todas as telas finalizadas” sem validar cada tela/estado.
