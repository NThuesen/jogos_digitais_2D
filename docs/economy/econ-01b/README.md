# ECON-01B — capacidade estrutural do elenco

Implementação sobre `6187683fa018f00875705ff5c7c962058cd6f4ed`, branch preparada com base na main `6d5767b`. Produto aprovado: **cap = vagas desbloqueadas + 1**, ou 4/5/6/7/8/9/10/11/11/11 nos dez níveis. Integração ainda depende do mantenedor; sem auto-merge.

## Mudança e consumidores

`CafeLevelTable` deriva o cap de `Lineup.SlotsFor(level).Total + 1` ao construir os unlocks, sem uma segunda tabela de capacidades. Construtor público de CafeUnlocks, ShiftSlots, tamanho do salão, thresholds de reputação e todos os demais parâmetros permanecem iguais. Não há nova atribuição automática de maids.

`Lootbox.Open`, `ApplicantBoard.Hire` e `RosterScreen.Refresh` já consultam `Profile.Unlocks.RosterCap`, portanto não precisaram de alterações. Primeira Simples no L1 acrescenta a quarta maid ao banco; cartas que excedam o cap convertem em fichas. Uma caixa de três cartas com uma vaga adiciona a primeira e converte as outras duas, sem mudar sequência RNG/raridades. Hire aceita até o cap e recusa acima dele sem cobrar, consumir candidato ou ID. Dismiss reabre uma vaga e preserva rescisão/fichas. Todas as maids continuam recebendo salário, inclusive reservas.

A regra limita elenco total, não tamanho do banco. Vagas vazias, várias reservas e escalação manual são preservadas. GDD §2.2 explicita esse contrato; §6 não precisou mudar. Não houve tuning Tier 1/2 nem trabalho ECON-01C/D.

## Testes e save

- **PASS — 16/16 direcionados:** 15 casos novos e o teste protegido autorizado. Dez pares nível/cap; primeira caixa e salários; caixa múltipla parcialmente convertida e mesma sequência RNG; contratação no limiar/recusa/dispensa; unlocks preservando duas reservas e vagas vazias; save anterior carregado intacto.
- **PASS — 272/272 simtest completo**, zero skips/inconclusivos, build sem erros/avisos. NUnit XML verificado separadamente por `tools/ci/verify.py results`.
- **PASS — boundaries**, 49 fontes Sim e 14 arquivos de testes, sem UnityEngine; **PASS — git diff --check**.
- Ambiente: SDK Windows 10.0.400, target/runtime net8.0/8.0.14, chamado da WSL por wrapper de tradução de caminhos. Nenhum runner versionado foi modificado.

O teste de compatibilidade inclui payload congelado gerado **antes da edição** com SaveCodec da revisão inicial: `Profile.NewGame(73)`, Lineup.Clear, apenas maid 2 em Floor/0; três maids apesar do antigo cap 2. SHA256 do payload: `e69643ab4723b35db62b51324b24491e4ec39d4fec1a03594d1f82db49fc2779`. Novo código carrega como `LoadStatus.Ok`, cap 4, mesmas três maids, duas reservas e vagas vazias, e reencoda exatamente os mesmos bytes. Cap é derivado, não serializado: nenhum schema/versionamento/migração foi alterado. Não usa save pessoal.

A manutenção de `DayTests.cs` está restrita ao setup de `AFullRosterTurnsACardIntoFichasRatherThanLosingIt`: preencher até o cap antes de abrir e confirmar a lotação. Todos os asserts anteriores de resultado/conversão/fichas foram preservados. Nenhum outro teste existente foi editado.

## Comparação com ECON-01A

[comparison.json](comparison.json) contém hashes e métricas geradas por [compare.py](compare.py). O runner ECON-01A e suas evidências congeladas não foram regravados. Mesmas políticas e seeds: sequenciais 1–60 e espalhadas `i * 2654435761 mod 2^32`, i=1–60, até 50 dias/falência.

| Política/conjunto | Antes → depois |
|---|---|
| Passivo, ambos os conjuntos | 258 dias por conjunto; **todas as colunas idênticas exceto cap**. Sem compras nem novas maids, nenhum efeito econômico. |
| Caixas, sequencial | Elenco dia 1: 3 → 4; salário médio dia 1: 54 → 72; conversões totais: 61 → 0 (61 → 60 caixas). |
| Caixas, espalhado | Elenco dia 1: 3 → 4; salário médio dia 1: 54 → 77,467; conversões totais: 66 → 1 (66 → 61 caixas). |

Mediana de falência segue dia 4 e sobreviventes dia 7 seguem 0/60 nesses quatro cenários. Menos devoluções reduzem recompras; a contratação adicional aumenta folha e altera atendimento/receita. São consequências esperadas de elenco maior, sem alteração de salários, odds, preços, demanda ou outras regras. Isto não demonstra viabilidade econômica nem invalida o diagnóstico ECON-01A.

Reprodução na raiz (SDK .NET 8+ com runtime .NET 8 e Python 3):

```sh
bash tools/simtest.sh --where 'class == Neko.Tests.Sim.RosterCapacityTests || test == Neko.Tests.Sim.LootboxTests.AFullRosterTurnsACardIntoFichasRatherThanLosingIt' --noresult
bash tools/simtest.sh --result=Logs/simtest-econ01b.xml
python3 tools/ci/verify.py boundaries
python3 tools/ci/verify.py results Logs/simtest-econ01b.xml
dotnet run --project tools/econ-baseline/EconBaseline.csproj -- Logs/econ01b
python3 docs/economy/econ-01b/compare.py Logs/econ01b/days.csv
```

## Unity e integração controlada

**NOT RUN — Unity EditMode, PlayMode, tela Elenco 960×600, Console/scene wiring e WebGL.** `UNITY_EDITOR=<wrapper Windows 6000.5.9f1> bash tools/run-tests.sh EditMode` foi tentado; a verificação adicional Windows recusou porque já existe Unity aberto. Nenhum Editor concorrente foi iniciado, nenhuma sessão foi fechada e não houve polling. O guard Linux sozinho não detectaria esse processo Windows; o wrapper confirmou sua existência antes de abrir o Editor. `RosterScreen` lendo o mesmo cap é verificação estática, não evidência de runtime.

SaveBridge nesta branch grava em PlayerPrefs diretamente. Foi iniciada preparação de cópia descartável para isolamento, mas a cópia não concluiu por permissões de metadata; nenhum teste foi executado nela e nenhum ProjectSettings ou save habitual foi alterado. A validação Unity deverá ocorrer em ambiente com save isolado e sem Editor concorrente. Não declarar fluxo visual validado a partir dos testes headless.

**Bloqueio de integração esperado:** `protected-change-guard` rejeita qualquer edição de teste existente, inclusive esta exceção expressamente autorizada pelo responsável. Não alteramos workflows, runners, checks ou branch protection. Não solicitamos auto-merge. O mantenedor deve resolver a exceção legitimamente no GitHub; este relatório não substitui essa etapa nem declara o milestone integrado. CI remota não foi acompanhada por polling.
