# ECON-01B — Consistência da capacidade do elenco

**Status:** APPROVED / READY FOR CODEX (29/10/2026).
**Base Git:** `main` em `6d5767b8de3031423fabb22a506e49086dedad9a` (merge ECON-01A / PR #19).
**Branch preparada:** `task/econ-01b-roster-cap-20261029`.
**Produto aprovado pelo responsável:** capacidade máxima do elenco em cada nível = **quantidade de vagas desbloqueadas na escalação + 1 reserva**. Não mudar outros sistemas de economia nem implementar Tier 1/2.

## Contexto mínimo e leitura

- `AGENTS.md`, esta tarefa e `docs/agent/WORKFLOW.md` / `VALIDATION.md` conforme a etapa.
- Evidência: `docs/economy/econ-01a/README.md` (cap inicial menor que as três maids), `proposal.md` §2.3 e §6, `docs/GDD.md` §2.2 (produto) e §11 (estado/saves).
- Código provável: `Assets/_Game/Scripts/Sim/Economy/CafeLevelTable.cs`, `Assets/_Game/Scripts/Sim/Team/Lineup.cs`, `Assets/_Game/Scripts/Sim/Economy/Lootbox.cs`, `Assets/_Game/Scripts/Sim/Economy/ApplicantBoard.cs`, `Assets/_Game/Scripts/Sim/Save/Profile.cs`, `Assets/_Game/Tests/EditMode/Sim/DayTests.cs`. Inspecionar os usos reais de `RosterCap` antes de editar; não carregar arte, cenas e documentação completa sem necessidade.

## Contrato de comportamento

| Nível | Vagas da escalação | Cap aprovado |
|:---:|---:|---:|
| 1 | 3 | 4 |
| 2 | 4 | 5 |
| 3 | 5 | 6 |
| 4 | 6 | 7 |
| 5 | 7 | 8 |
| 6 | 8 | 9 |
| 7 | 9 | 10 |
| 8–10 | 10 | 11 |

1. A regra vale para **todos** os níveis 1–10, inclusive desbloqueios após progressão. Evitar divergência futura entre `Lineup.SlotsFor(level).Total` e `RosterCap`; escolher a menor implementação que assegure/valide essa relação.
2. Nova empresa no nível 1: 3 maids iniciais, capacidade 4; a primeira caixa Simples pode acrescentar a quarta maid, em vez de convertê-la imediatamente por lotação. A quinta carta, quando o elenco estiver cheio, continua convertida em fichas, preservando as regras de `Lootbox.Open`.
3. O limite controla **elenco total**, não tamanho do banco. `+1` **não obriga** a manter exatamente uma no banco, nem proíbe mais de uma maid não escalada caso existam vagas da escalação vazias. Posições opcionais vazias e banco são estados válidos; nenhuma atribuição deve ser automática por consequência da mudança de cap.
4. `ApplicantBoard.Hire`, compra de caixas, dispensas e informações `RosterScreen` devem ler o mesmo cap; preservar recusas com motivo, custo e devolução de fichas, salários pagos por todas as maids (inclusive reservas), RNG e chances de raridade.
5. Saves anteriores e novas empresas devem carregar sem perda de maids, falhas, duplicações ou alteração forçada da escalação. Sem mudança de schema/versionamento de save por mero ajuste de cap; se for detectado problema real de migração, registrar como bloqueio antes de ampliar escopo.
6. Não modificar números de demanda, preços, ingredientes, aluguel, salários, receita, reputação, fichas, dinheiro inicial, paciência, raridades, odds, velocidade ou progressão. O efeito econômico indireto de poder contratar mais maids **é esperado**; não compensá-lo com tuning nesta tarefa.
7. `CafeUnlocks.ShiftSlots` eventualmente ocioso é achado separado, não autorização para limpá-lo/refatorá-lo.

## Exceção de manutenção protegida — autorização expressa do responsável

Autorizada **somente** a alteração mínima do teste existente
`Assets/_Game/Tests/EditMode/Sim/DayTests.cs` →
`LootboxTests.AFullRosterTurnsACardIntoFichasRatherThanLosingIt`.

O teste hoje assume cap 2 no L1 e já começa com três maids. Reorganizar **apenas o setup necessário** para preencher o elenco até o novo cap **antes** de abrir a caixa, mantendo os asserts sobre conversão em fichas, resultado válido e não perda do card; pode fortalecer assertions relacionadas ao estado full sem mudar a intenção. Adicionar testes novos em arquivo novo quando justificável (p.ex. cap/vagas nos dez níveis, primeira caixa adiciona quando há vaga, caixa converte somente quando cheio), respeitando o custo mínimo. Se outra alteração em arquivo de testes existente se mostrar necessária, **parar e solicitar autorização adicional**, não ampliar esta exceção.

**Importante:** esta autorização **não** é permissão para modificar/burlar `.github/workflows/pr-safety.yml`, `tools/ci/`, scripts, branch protection ou checks. `protected-change-guard` explicitamente rejeita qualquer modificação de teste existente mesmo com exceção textual. Portanto: abrir PR, registrar falha esperada do guard + evidência independente do teste e **não solicitar auto-merge** nem forçar integração. O mantenedor deve realizar o procedimento de exceção no GitHub conforme as proteções reais; se não houver rota autorizada disponível, **entregar PR bloqueado e relatar**. Não separar mudanças com o objetivo de enganar o guard.

## Execução e critérios de aceite

1. Preflight: `git status --short`, `git branch --show-current`, `git rev-parse HEAD`. Evitar editar um worktree ocupado por ART/UI. Se estiver no checkout principal com alterações locais, criar worktree isolado da branch preparada, sem stash/reset.
2. Examinar regras e testes atuais antes de alterar código. Fazer a mudança mínima em `Neko.Sim` e a manutenção de teste autorizada; atualizar `docs/GDD.md` §2.2 para deixar a regra explícita e §6 **somente se** precisar corrigir a descrição de desbloqueios.
3. Testar as 10 combinações de vagas/cap; L1 3/4; entrada de carta quando há espaço; conversão quando cheio; contratação no limiar; aumento de cap ao subir de nível; `Dismiss`; saves anteriores; uso válido do banco/vagas vazias. Preferir testes determinísticos, sem forçar RNG ou mudar harness para esconder regressões.
4. Rodar `tools/simtest.sh`; `python3 tools/ci/verify.py boundaries`; `git diff --check`. Se Unity disponível, `tools/run-tests.sh EditMode` e `tools/run-tests.sh PlayMode` e conferir a tela Elenco 3/4 → 4/4 e uma nova maid no banco em 960×600, Console e scene wiring. Caso contrário, marcar NOT RUN com motivo. WebGL apenas se alterado comportamento Unity ou se exigido por evidência — jamais declarar verificação não realizada.
5. Baseline ECON-01A: compare pelo menos o cenário passivo (sem novas maids) e o cenário de abertura de caixa com seeds fixas, **sem regravar** os artefatos anteriores em `docs/economy/econ-01a/`. Diferenças no cenário com caixas são esperadas e devem ser explicadas.
6. Revisar diff quanto a scope creep, exceção protegida e GDD. Preencher `.github/PULL_REQUEST_TEMPLATE.md` com PASSED / FAILED / NOT RUN. Atualizar checkpoint em `.agent/ECON-01B_HANDOFF.md` com SHA, fatos, testes, limitações e PR.
7. **Stop gate:** abrir **um único PR de ECON-01B** e parar. Não iniciar ECON-01C, não alterar economia, não fazer auto-merge de PR com teste existente modificado, não fazer polling de CI. Se o guard falhar, relatar a necessidade de intervenção do mantenedor em vez de tratá-la como falha de implementação.

### Pendências fora do escopo

Equipamento comprado sem equipar, escolha de destinatário dos presentes, bond-5, crítico, habilidades, `ApplicantBoard` UI, informação sobre segundo fogão, preços de móveis e balanceamento Tier 1/2. Apenas apontar descobertas relevantes, sem implementar nessa branch.
