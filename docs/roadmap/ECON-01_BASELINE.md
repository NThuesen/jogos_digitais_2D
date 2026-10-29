# ECON-01A — Baseline econômico reproduzível (Codex)

**Estado:** READY FOR IMPLEMENTATION — autorizado em 29/10/2026 para **diagnóstico, instrumentação mínima e documentação**, sem alterar regras do jogo.
**Branch inicial:** `task/econ-01-baseline-20261029` (criada a partir de `main` em `ae39c2db38b056d0e224b9c2208823e1917e525e`).
**Entrega desta tarefa:** um experimento versionado e reproduzível, relatório factual e **um PR exclusivo de ECON-01A**. As etapas B–D abaixo são *backlog proposto*, não autorização para implementá-las ou mesclá-las.

## Problema e fonte de evidência

`proposal.md` (ECON-01, PR #17) relata falência mediana no dia 4, 0/60 empresas sobrevivendo ao dia 7 e inconsistência entre capacidade de elenco e vagas da escalação. Também propõe ajustes econômicos em Tier 1 e 2. A análise foi executada fora do repositório em `/tmp/neko-econ`; **o harness não foi versionado nem validado aqui**. Números são hipóteses/relatos a reproduzir, não resultados confirmados.

O objetivo é entender três perguntas separadas:
1. **Viabilidade:** o jogo permite sobreviver e progredir com decisões razoáveis?
2. **Agência:** decisões conscientes (escalação, investimentos, gestão de caixa) superam estratégias passivas, considerando a lógica real do botão Auto?
3. **Valor das escolhas:** papéis, raridades e móveis apresentam trade-offs e feedback perceptíveis, sem armadilhas obrigatórias de RNG?

Nenhuma imagem final é necessária. O trabalho de arte/UI tem trilha paralela independente.

## Fontes de verdade e leitura sob demanda

1. `AGENTS.md` — política de branches e proteção; `docs/agent/WORKFLOW.md` para PR; `docs/agent/VALIDATION.md` para evidência.
2. **Para hipóteses e números relatados:** `proposal.md`, somente seções 1, 2 e 4 na primeira leitura.
3. **Para regras do produto:** `docs/GDD.md`, somente §§1–2, 5–6 e 10 que forem relevantes. Divergências precisam ser registradas, não resolvidas silenciosamente.
4. **Para fronteiras técnicas:** `docs/ARCHITECTURE.md`, sobretudo `Neko.Sim` vs `Neko.Game`.
5. **Para UX primeiro dia:** `docs/roadmap/UX-01_FIRST_DAY.md`, observações que não são bugs.
6. Inspecionar os arquivos concretos antes de escrever, priorizando `DayPlan.cs`, `Books.cs`, `CafeLevelTable.cs`, `Profile.cs`, `GameSession.cs`, `Shift.cs`, `Lineup.cs`, `Lootbox.cs`, `Shop.cs`, `PlaceableCatalog.cs` e o harness já existente em `tools/simtest/`. Abrir somente os relacionados a uma hipótese; **não** ler cenas, arte ou todos os arquivos de uma vez.

Use `rg -n` para localizar chamadas específicas e `git diff --stat`/`git diff --check` para revisão incremental. Não alegar cache de contexto entre sessões: o handoff pequeno em `.agent/ECON-01_HANDOFF.md` é o checkpoint explícito para retomada; é a única memória operacional desta tarefa.

## Escopo fechado — ECON-01A

1. **Preflight:** verificar `git status --short`, branch e `HEAD`. Se houver alterações não relacionadas, preservá-las; não sobrescrever nem fazer reset/stash destrutivo. Trabalhar nesta branch apenas se for a branch designada e estiver segura; evitar dois agentes simultâneos no mesmo worktree.
2. **Baseline sem tuning:** criar o menor runner determinístico possível que compile/use o código real de `Neko.Sim` (sem cópia manual de fórmulas, sem UnityEngine e sem modificações em `Neko.Sim`). Preferir reaproveitar estrutura existente a criar framework genérico. Uma execução documentada deve produzir resultados comparáveis para seeds fixas. Se o harness original não estiver acessível, reimplementar o mínimo e explicar diferenças de metodologia.
3. **Cinco políticas de jogador:** reproduzir as descrições em `proposal.md`: competente, aprendiz, passivo (Auto), comprador compulsivo de caixas, escalação toda errada. Documentar escolhas exatas, inclusive quando compra/dispensa/faz Auto. **Auto é um algoritmo de escalação**, não um jogador que não toma decisão; não confundir as duas coisas.
4. **Medições por cenário:** no mínimo 60 seeds compartilhadas por cenário × até 50 dias, ou registrar justificativa técnica para desvio. Capturar sobrevivência dia 7/20/50, dia de falência, lucro, caixa, receita, salários, aluguel, ingredientes, satisfação, reputação, fichas e nível por dia/faixa; indicar n para cada estatística, inclusive sobreviventes. Incluir medições focalizadas do dia 1 e 2–5; reportar eventos imprevistos (ex.: início com elenco > cap). Se possível medir separadamente efeitos de compra de estação, mobília e raridade sem mudar produção.
5. **Paridade e rastreabilidade:** respeitar RNG, sorteios, sequenciamento, `GameSession` e regras de turno reais; explicar simplificações. Registrar lista/intervalo das seeds, configuração de cenário, comando executado, SHA do código, sem depender de arquivos temporários locais. Executar duas vezes o mesmo cenário e confirmar saída determinística; verificar que seeds distintas não são acidentalmente repetidas.
6. **Relatório pequeno:** tabelas de baseline, diferenças para `proposal.md`, limitações/possíveis vieses, inconsistências confirmadas e hipóteses não confirmadas. Distinguir bug factual de decisão deliberada (por exemplo, banco e posições vazias são válidos). Não transcrever toda a proposta: referenciar seções.
7. **Testes e entrega:** `tools/simtest.sh` deve continuar passando sem alterar expectativas, runners ou testes existentes. Se o ambiente não tiver .NET/Unity, reportar NOT RUN, não PASS. Para runner exclusivamente headless, Unity/EditMode/PlayMode/WebGL podem ser NOT RUN com justificativa. Revisar diff; preencher PR template com dados reais.

**Critério de aceite de ECON-01A:** um colega, em checkout limpo com .NET 8, consegue rodar o experimento a partir do README/comando, obter a mesma saída com as mesmas seeds, localizar o relatório, e confirmar que `Neko.Sim`, `Neko.Game`, UI, scenes, GDD, probabilidades e valores de balanceamento não foram modificados.

## Backlog após diagnóstico — NÃO implementar nesta branch

**ECON-01B — consistência de sistemas (novo branch/PR, decisão do responsável):** corrigir a capacidade do elenco de modo a comportar as vagas conforme progressão; não impor preencher todas as posições nem remover banco. O relatório mostra que o teste existente de lootbox assume o cap antigo. Testes existentes são protegidos; **obter exceção de manutenção explícita** antes de alterá-los, mantendo a intenção de testar conversão de carta em fichas quando elenco *realmente* cheio. Inspecionar compras de roupas sem equip, seleção de destinatário de presente, texto de crítico/bond 5, skills, contratação e fogão extra. Cada correção deve escolher entre conectar mecanismo, desabilitar opção enganosa ou adiar. Não implementar tudo automaticamente.

**ECON-01C — experimento econômico (novo branch/PR, somente após aprovação de regras):** testar Tier 1 como candidato, não como valor definitivo; separar o impacto de demanda, duração de receita, aluguel, ingredientes, rep/fichas e dinheiro inicial. Comparar seeds pareadas e políticas que fazem gestão ativa vs Auto; observar consequência de raridade por papel, ROI de decoração/estações e acúmulo tardio de caixa. Exigir sensibilidade e riscos de estratégia dominante. Tier 2 permanece opcional. Não mudar aleatoriedade, caixa/salários, rescisão ou progressão do GDD sem decisão explícita.

**ECON-01D — aceitação de experiência (novo PR quando pertinente):** playtest real na Unity 960×600 com cinco primeiros dias, Balanço/Empresa, escolhas e recuperação de falha; depois EditMode/PlayMode e WebGL quando escopo afetar produto. Registrar evidência de runtime e compreensão humana. Não confundir simulação com validação visual.

## Regras do loop iterativo e eficiência de contexto

- Ciclo local **inspecionar → hipótese → alteração mínima → teste → diff → registrar achado**. Trabalhar até satisfazer ECON-01A; não avançar de fase só porque uma métrica parece boa.
- Uma hipótese por ciclo; reutilizar runner e seeds, evitar regenerar relatórios enormes ou reabrir documentos já resumidos sem mudança relevante. Preferir tabelas de agregados e 1–3 traces para falhas.
- Em cada checkpoint significativo, atualizar **somente o resumo objetivo** em `.agent/ECON-01_HANDOFF.md`: SHA, fase, resultados, comandos, bloqueios, próximos dois passos. Não armazenar raciocínio privado, prompt repetido, logs longos, duplicatas da fonte ou arquivos sensíveis.
- Se travar na mesma causa após **3 tentativas de correção orientadas por evidência**, encerrar essa frente e reportar o bloqueio; não entrar em looping sem fim. Não fazer polling de CI, Unity ou PR. Não executar tarefas em segundo plano de modo autônomo sem infraestrutura explícita.
- **Stop gate:** terminada ECON-01A, abrir PR e encerrar a execução com resumo verificável. ECON-01B/C/D só começam por tarefa separada após decisão de produto/escopo. Sem auto-merge de mudança protegida ou não verificada.
- Não alterar arte, direção visual, cenas, `Packages/`, `ProjectSettings/`, tests/CI protegidos ou documentos de outros trabalhos. Não publicar build, gerar APK, instalar ferramentas pagas ou mexer em secrets.

## Entregáveis esperados

- Runner/configuração mínima e instruções reproduzíveis no próprio repositório (localização escolhida após inspecionar o estado real).
- Relatório de evidências de ECON-01A, sem alegar playtest humano.
- `.agent/ECON-01_HANDOFF.md` atualizado uma vez ao terminar a fase ou quando houver bloqueio material.
- Um PR de escopo ECON-01A com matriz PASSED / FAILED / NOT RUN e nenhuma alteração deliberada de produto.

**Importante:** nem a taxa de sobrevivência de 92% nem qualquer outra meta do `proposal.md` está aprovada como alvo obrigatório. Nosso critério é jogo com decisões significativas e falha compreensível; resultados da simulação informam decisão, não a substituem.
