# ECON-03 — coleção ilimitada e salários só da equipe ativa

Estado: **testado pelo mantenedor no itch.io (build experimental), sem integração à `main`.**
Branch: `task/econ-03-uncapped-collection-active-wages` (worktree `econ-03`), empilhada sobre o commit validado da PR #83 (`1a0a13c`).
Este documento existe para guiar a atualização do GDD e do texto do itch.io.

## 1. A regra nova, em uma frase

> Você pode **possuir** quantas maids quiser (uma cópia por identidade); só **paga salário**
> quem está **escalada** (`Profile.FieldedMaids()`). O limite da equipe passa a ser apenas as
> **vagas da escalação** (`Lineup.SlotsFor(nível)`), que continuam crescendo com o nível do café.

## 2. O que mudou no jogo

| Tema | Antes | Agora |
| --- | --- | --- |
| Limite de maids possuídas (`RosterCap`) | 2 → 8 conforme o nível | **não existe** |
| Quem recebe salário | todo o elenco | **só as escaladas** (sem contar duas vezes a mesma identidade) |
| Custo fixo diário | salários do elenco inteiro + aluguel | salários das escaladas + aluguel |
| Caixa (Lootbox) com elenco cheio | carta nova virava ficha | **só carta de identidade que você já tem** vira ficha ("Repetida — virou ficha") |
| Contratar (quadro de candidatas) | bloqueado pelo teto | sem verificação de teto |
| Cabeçalho da tela Maids | `Maids N / teto` | `Maids N · Coleção c/R` e `Custo fixo por dia ¥X` (R = identidades lançadas, hoje 23) |
| Salário na ficha da maid | "Salário" | "Salário (escalada)" ou "Salário se escalada" |
| Dispensar maid | cortava a folha sempre | dispensar vira fichas; maid guardada nunca custou salário |
| Vagas da escalação | `Lineup.SlotsFor(nível)` | **inalterado** |
| Saves | — | **compatíveis**: nenhum campo mudou; um save com mais de 8 maids carrega e salva igual |

Código: `Books.WagesFor` / `Books.FixedCostsFor`, `CafeLevelTable` (sem o campo `RosterCap`),
`Lootbox.Open`, `ApplicantBoard.Hire`, `RosterScreen` (`CollectionText`, `CollectedCount`) e `Strings`.
Nada mudou em RNG, progressão, catálogo de móveis ou mecânica do expediente.

## 3. Impacto no balanceamento (medido, **sem compensação**)

Simulação headless, 60 sementes × 60 dias, `tools/econ-balance`. Valores = **empresas falidas em 60**.
A coluna *yes/no* é o robô investir (caixas, contratar, móveis).
"Depois (bots antigos)" mantém a política de contratação com teto legado; "Depois (bots sem teto)" deixa
os robôs contratarem sem limite — é o cenário mais fiel ao jogo novo.

| Perfil | Investe | Antes | Depois (bots antigos) | Depois (bots sem teto) |
| --- | --- | ---: | ---: | ---: |
| idle | sim | 60 | 60 | 60 |
| idle | não | 60 | 60 | 60 |
| lazy | sim | 56 | 46 | 46 |
| lazy | não | 52 | 52 | 52 |
| casual | sim | 49 | 22 | 24 |
| casual | não | 4 | 4 | 4 |
| pro | sim | 20 | 1 | 2 |
| pro | não | 0 | 0 | 0 |

Leitura:

- A redução do custo fixo ajuda quem **investe** (jogador "casual" e "pro" quebravam por pagar
  salário de quem nem trabalhava). Quem não faz nada continua falindo, como esperado.
- Caixa no dia 30 do perfil *pro* que investe: **3605 → 5997** (bots antigos) / **4270** (bots sem teto).
  Com a coleção livre os robôs chegam a ~21–22 maids no dia 30.
- **Não houve ajuste automático** de preço, aluguel ou custo para compensar. O ganho de folga é
  uma consequência direta da regra pedida; se o time quiser reequilibrar, deve ser outra tarefa,
  com medição própria (candidatos: aluguel por nível, preço das caixas, salário das raras).
- Reprodução: `tools/econ-balance/run-all.sh` (usa `ECON_BOT_CAP=old|none`); linha de base em
  `tools/econ-baseline`.

## 4. Testes

Novos (arquivos novos — permitidos pelo `protected-change-guard`):

- `Assets/_Game/Tests/EditMode/Sim/ActiveWagesTests.cs` — salário só das escaladas, sem dupla
  contagem, ledger, aviso de falência/falência, contratar acima do antigo teto, caixa acima do teto,
  repetida vira ficha, coleção completa, save/load com mais de 8.
- `Assets/_Game/Tests/PlayMode/CollectionEconomyUiTests.cs` — cabeçalho da tela Maids, custo fixo,
  paginação/filtros com 19 maids, dispensar maid guardada, custo na tela de escalação (limpar/auto),
  escalar uma maid do banco.

**Testes existentes alterados (exceções protegidas — precisam de aprovação do dono):**
cada um codificava uma regra antiga (teto de elenco / folha total) que a ECON-03 revoga de propósito;
nenhum foi enfraquecido, apenas atualizado para a regra nova.

| Arquivo | Mudança |
| --- | --- |
| `Tests/EditMode/Sim/DayTests.cs` | `EverybodyOnTheBooksIsPaid…` → `OnlyTheFieldedAreOnTheWageBill…`; `AFullRosterTurnsACard…` → `ACardOfAnIdentityAlreadyOwnedTurnsIntoFichas…` |
| `Tests/EditMode/Sim/MaidCatalogTests.cs` | `AFullRoster…` → `ACardOfAFreeIdentityJoinsEvenAFullSizedCollection` |
| `Tests/PlayMode/NewGameIllustratedTests.cs` | laço sem teto; `Math.Min(8, Maids.Count)`; checagem do retrato por qualquer `Detail Portrait *` |
| `Tests/PlayMode/RosterScreenTests.cs` | cabeçalho novo; salário por estado escalada/guardada |
| `Tests/PlayMode/Layout/SessionValues.cs` | `RosterHeader(maids, collected, fixedCost)` no novo formato |

`LettingSomebodyGoPaysSomethingBackAndCutsTheWageBill` ficou **inalterado** (continua válido).

Resultado da suíte completa no commit final: ver o relatório da PR (rodando no momento em que este
documento foi escrito; última rodada EditMode completa: 409/409 antes das últimas edições de teste).

## 5. O que precisa de atualização

| Documento | Situação |
| --- | --- |
| `docs/GDD.md` | **Editado neste branch.** Pontos tocados: custo da lendária "enquanto escalada"; novo parágrafo "Coleção e equipe de trabalho são coisas diferentes (ECON-03)"; carta repetida = identidade já possuída; dinheiro paga salário da equipe escalada; níveis entregam vaga de **escalação** (não mais de elenco); "sem becos sem saída" (maid guardada não custa; dispensar vira fichas). **Seção 10 revisada:** novas linhas de verificação, a tabela ECON-02 marcada como "sem compras / não remedida", o "limite conhecido" atualizado e a nova subseção 10.2 com a tabela desta página. **Ainda desatualizado no GDD:** a contagem de testes na seção 12 ("257 / 292 / 17"). |
| `docs/ITCH_PAGE.md` | **Editado neste branch** (3 trechos: dinheiro paga só escaladas; repetida vira ficha e não há limite de coleção; lendária custa 210/dia só escalada). |
| `docs/DDE.md` (entregue pela PR #82) | **Revisado (v1.1)**, trazido de `origin/main` só esse arquivo: vagas por nível (3.2), coleção × equipe e custo fixo (3.7), contratação, repetidas (3.8), compatibilidade de saves (3.10), interface (4.1), estratégias (6.6), sistemas (7.4, 7.6), Situação D, emoções, matriz, decisões, evidência/riscos (16) e fontes. Estrutura DDE e as 12 rubricas preservadas. |
| `README.md` | Conferir se menciona limite de elenco. |
| `docs/art/ui-int-02a/README.md` | Histórico: cita `maids / RosterCap` do cabeçalho antigo; não foi alterado (registro de uma tarefa anterior). |

## 6. Riscos e pontos em aberto

- Mais maids guardadas = mais itens para navegar na tela Maids; paginação e filtros foram testados
  com 19 maids.
- Sem teto, o jogador pode colecionar todas as 23 identidades; o loop de "caixa" passa a depender
  mais de fichas por repetidas — vale acompanhar se fichas ficam abundantes.
- A rebalanceada (aluguel, preço de caixas) **não** foi feita; ver seção 3.
- Publicação: este pacote é **experimental**; o ZIP seguro continua sendo a versão de entrega.
