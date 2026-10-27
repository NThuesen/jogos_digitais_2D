# UI-02 — Evoluir telas com a direção visual aprovada

**Status:** BLOCKED até aprovação explícita de ART-01; usar aprendizados de ART-02 sempre que possível.
**Agente implementador:** Codex. **Leia:** [roadmap](../ROADMAP.md), [ART-01](ART-01_STYLE.md), [ART-02](ART-02_VERTICAL_SLICE.md) somente quanto aos resultados aprovados, [Unity](../agent/UNITY.md). O GDD só se a experiência/regra mudar.

## Objetivo
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
- Preservar legibilidade dos nove arquétipos, informações de raridade e contrastes, inclusive para cartas de cores distintas. Evitar UI toda na mesma tonalidade marrom; paleta definitiva ainda depende da ART-01.
- Nova arte/animação não pode tornar performance impraticável em desktop WebGL; avaliar após integração real, não por promessa.
- Não presumir que a existência de `Screen` ou `UiKit` garante ajuste automático em 960×600; conferir em runtime.

## Evidência e aceite por PR
- Lista de telas/estados tocados, comparação antes/depois **960×600**, screenshots reais do Unity, Console relevante, interações demonstradas e resultado de testes apropriados.
- Usuário reconhece maid com rapidez no card e no restaurante; informações e botões legíveis; navegação e regra preservadas; feedback do dia explica decisões.
- Se a alteração for estética material, **aprovação visual do responsável é gate de produto**, distinto de checks GitHub. Não ativar auto-merge de trabalho visual ainda não inspecionado.
- Evitar declarações genéricas como “todas as telas finalizadas” sem validar cada tela/estado.
