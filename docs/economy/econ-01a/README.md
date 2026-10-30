# ECON-01A — baseline econômico reproduzível

Diagnóstico sem tuning. Código de produção na revisão `7d42ab1f7dd3981c56defdd0b3edaee08918749f` (branch preparada a partir de `ae39c2d`). [Runner e comandos](../../../tools/econ-baseline/README.md), [dados diários](days.csv), [agregados com denominadores](RESULTS.md), [hashes](metadata.json).

## Metodologia auditável

O harness original de `proposal.md` não está disponível em `/tmp/neko-econ`. Esta é uma reconstrução das políticas das seções 1–2/4, sem alegação de equivalência linha a linha ao experimento antigo. Números de Tier 1/2 não são resultados deste diagnóstico.

- Cinco políticas, seeds pareadas 1–60, até 50 dias ou falência; foco dia 1: 1–200; oito planos de compra: 1–100. Sensibilidade adicional: mesmas cinco políticas, 60 seeds `unchecked(i * 2654435761u)`, i=1–60. Mantém as seeds originais e evita depender somente dos primeiros sorteios correlacionados de seeds pequenas.
- `Profile.NewGame(seed)` sorteia o elenco com seu próprio Rng. Um **novo** `Rng(profile.Seed)` reproduz o construtor de `GameSession`. Compras de caixas e `rng.NextUInt()` de cada `DayPlan.Build` consomem esse fluxo compartilhado na ordem real; cada Shift usa sua seed própria. Não reiniciar RNG a cada dia e não continuar o RNG interno do opener.
- `Shift.Tick(0.05f)` até Finished, limite de 40000 ticks como SkipDay; `Shift.Finish()` e `Books.Close()` reais. Close aplica XP, recuperação, fichas, reputação, falência e unlocks. Readiness exige as três posições essenciais como `GameSession.WhyNotReady`. Sessão contínua, sem recarga entre dias (recarregar a sessão reiniciaria seu RNG).
- Não compila GameSession porque ele referencia SaveBridge/UI Unity. Replica somente a sequência pública auditada, não as fórmulas. Não chama persistência, PlayerPrefs, cenas ou arte.
- Sobrevivência é caixa **positivo após fechar** o dia indicado. Empresa que quebra no dia 7 não sobrevive ao dia 7. Sem imputar dias após falência, sem substituir falência por timeout. Médias das faixas são ponderadas por **dias observados**, incluindo o dia de falência; não são médias apenas dos sobreviventes nem médias por empresa.

## Políticas exatas

Todas as decisões ocorrem pela manhã. Caixa inicial, estatísticas, progressão, salários e probabilidades permanecem os reais. Desempate por ID crescente. Score para a posição natural = atributo primário + 0,4 × secundário, usando `Maid.Total`; esse é um critério explícito de seleção do jogador, não uma nova regra do jogo.

| Política | Decisões |
|---|---|
| Competente | Ordena por score e escala somente nas posições naturais. Vagas opcionais ficam vazias; mantém no máximo uma maid no banco e dispensa excedentes por score. Após dois dias consecutivos de prejuízo e caixa abaixo de 2× fixos, dispensa a mais cara entre banco/palco/gerência. Abre no máximo uma Simples por manhã somente se houver vaga vazia, 40 fichas e caixa ≥4× fixos. Refaz escalação natural. Não compra móveis. |
| Aprendiz | Rotaciona as maids escaladas uma vaga adiante nos dias 1–3; a partir do dia 4 usa Competente com o histórico real de prejuízos. |
| Passivo | Chama `Lineup.AutoFill` todo dia; nenhuma compra/dispensa. Auto faz duas passagens: naturais na ordem do elenco, depois melhor encaixe entre restantes. Não é aleatório. |
| Caixas | Compra repetidamente a mais cara disponível (Dourada ≥520, Prata ≥180, Simples ≥40 fichas), incluindo fichas devolvidas por conversão; nunca dispensa. Limite de 1000 compras por manhã falha explicitamente em vez de truncar silenciosamente. Auto após comprar e ao abrir o dia é decisão dessa política; BoxScreen real não chama Auto automaticamente. |
| Errada | Prepara naturais, depois gira IDs uma posição adiante entre vagas ocupadas, sem perder maid nem preencher banco. No L1 todas ficam fora da posição natural. |

Banco e vagas vazias são escolhas válidas conforme UX-01; não classificados como bugs. O código das políticas define comportamento em níveis altos, mas sua qualidade nesse regime não foi demonstrada, pois nenhuma empresa chegou ao L2. A distinção entre Auto e Competente precisa de experimento futuro com elenco/progressão diferentes; não se conclui falta de agência universal.

## Compras focalizadas

`purchase_0` controle; 1 planta; 2 mesa_2; 3 fogão; 4 dois fogões; 5 planta+mesa; 6 planta+fogão; 7 mesa+fogão. São sete intervenções mais controle (a proposta não publicou quais eram os oito planos). Shop.Buy real, colocação uma vez no dia 1 na primeira célula livre válida, ordem y/x, sem rotação, remoção do inventário após colocar. Falha explícita se recusar compra/colocação. Depois Auto sem novas compras. Compras reduzem caixa antes da abertura; `profit` é lucro **operacional**, não desconta investimento novamente. O campo `purchases` e `cash_start` tornam o gasto rastreável. Não otimiza layout.

## Comparação com a proposta

| Afirmação original | Auditoria deste baseline |
|---|---|
| 0/60 vivos no dia 7, políticas distintas | Reproduzida no conjunto sequencial. Não é prova para todas as seeds possíveis. |
| Falência mediana dia 4, mais cedo dia 3 | Competente/Auto/caixas mediana 4; aprendiz/errada mediana 3. Compras antecipam falência até dia 2; o enunciado original não deve ser generalizado aos planos de compra. |
| Dia 1 perde 200/200; receita mediana 119, custos 254 | Reproduzidos. Lucro mediano medido −135; a seção 4 relata −129. A proposta também não fornece seeds para auditar essa diferença. |
| Receita média L1 dias 2+ 106, salários 57, aluguel 144, ingredientes 68, lucro −163 | Competente sequencial: 99,727 / 56,449 / 144 / 68,646 / −169,369 (n=198 dias, 60 empresas). Diferenças de seeds/política/ponderação não publicadas impedem equivalência exata. |
| Satisfação 0,14; rep/fichas 0,2/0,6 nos dias 2–5 | Competente sequencial: 0,168 / 0,260 / 0,760 (n=196 dias, 60 empresas). Não substituir diferenças por uma falsa reprodução exata. |
| Compras não salvam dia 10 | Reproduzido nos oito planos explicitados aqui, 100 seeds cada. Não prova que todo layout/móvel é inútil. |
| Simples inicial retorna média 10,9 fichas | Seeds 1–60: 60 caixas/60 conversões comuns, saldo médio 5 sem recompensa do turno; não reproduz 10,9. Nas seeds espalhadas, ver a sensibilidade abaixo; odds e fórmula intactas. |
| Picos de demanda, espera por comida e ROI/raridades em níveis altos | Não reproduzidos. Exigiriam cenários contrafactuais/controlados adicionais; não são evidências confirmadas por esta entrega. |

## Inconsistências e limites

Confirmado no código/dados: começo com 3 maids e cap 2; abrir caixa converte em fichas apesar das três vagas ocupadas. `CafeLevelTable` vs `Lineup.SlotsFor` dá cap/vagas 2/3, 3/4, 4/5, 5/6, 6/7, 6/8, 7/9, 7/10, 8/10, 8/10. É uma inconsistência observável; decidir corrigi-la pertence à ECON-01B, sem impor preencher todas as vagas.

Confirmado: demanda aumenta com o dia apesar do comentário em DayPlan dizer o contrário. Aluguel L1 é 144 contra salário inicial 54 (72,7% do custo fixo); 600 cobrem 3,03 dias fixos, divergindo do comentário “about four days” e da narrativa de semana no GDD. Ingredientes são cobrados pelo total cozido e preço médio do menu, conforme Books. Nenhuma dessas observações autoriza escolher números novos.

O possível benefício de raridades/papéis e ROI tardio não é mensurável nas trajetórias naturais deste baseline: nenhuma chega ao L2. Não foram forçados níveis, cards ou overrides para produzir conclusões tardias. Roupas, skills, presentes e juiz não foram auditados aqui. Satisfação e caixa não validam diversão, compreensão humana, Unity nem WebGL.

## Validação

- PASS: duas execuções completas com CSV idêntico byte a byte; mesmos agregados/metadados; seeds esperadas, primeiras shift seeds e fingerprints de elenco distintos por cenário; ledger conservando caixa; término dos turnos e séries.
- PASS: `tools/simtest.sh --noresult`, 257/257, sem skips/inconclusivos; SDK 10.0.400 / runtime 8.0.14. PASS: `tools/ci/verify.py boundaries` (49 fontes/13 arquivos de teste).
- NOT RUN: SDK 8 especificamente; Unity EditMode/PlayMode/WebGL e playtest humano — runner headless sem alterações de produto.
- `Neko.Sim`, `Neko.Game`, testes protegidos, CI, GDD, interfaces, cenas, arte e parâmetros econômicos intactos. ECON-01B/C/D permanecem propostas separadas.

Sensibilidade: nas 60 seeds espalhadas de Caixas houve 61 compras/conversões no primeiro dia (uma devolução permitiu recomprar), saldo médio antes da recompensa do turno 8,25 fichas. Portanto o estimador não deve supor uma única abertura por seed quando permite gastar devoluções. Nenhum dos cinco cenários mistos sobreviveu ao dia 7, e as medianas de falência permaneceram 4/3/4/4/3. O diagnóstico de sobrevivência é estável nestes dois conjuntos; estimativas de raridade/conversão não são intercambiáveis.

A saída final contém **5028 dias observados**. Duas execuções completas e agregações finais foram idênticas; SHA256 do CSV: `66141d3b1f45f8645ee0e060ce4c4b2e5930b4714627bcd8cae1e3da46512985`. Comparação pareada competente/Auto: mesmas maids escaladas e mesmos resultados em ambos os conjuntos (sem inferir equivalência para elencos maiores).
