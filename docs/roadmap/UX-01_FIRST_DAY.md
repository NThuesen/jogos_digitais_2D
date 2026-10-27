# UX-01 — Entendimento da primeira jornada

**Status:** DEFERRED; **não é correção de bug identificada**.
**Leia quando:** houver evidências de playtest do primeiro dia após UI-01 e melhorias centrais do visual.

## Pergunta de produto
O jogador compreende por que escolheu a escalação, por que as maids tiveram aquele desempenho, de onde veio o lucro/prejuízo e o que mudar amanhã?

## Observações que NÃO são bugs
- **Um posto vazio e uma maid no banco são estados válidos**, explicitamente confirmados pelo responsável em 27/10/2026. Não criar Issue para “consertar” essa escolha.
- Uma captura de **prejuízo no primeiro dia** não demonstra falha de economia ou RNG. A estratégia pode justificar prejuízo; avaliar compreensão primeiro.
- A decisão do GDD sobre turno autônomo não deve ser substituída por interação durante atendimento.

## Experimento leve quando houver tempo
1. Definir cenário reproduzível com estado inicial e time selecionado, registrar a posição de cada maid e custos antes do turno.
2. Observar se uma pessoa que não conhece a implementação consegue configurar a escalação, iniciar um dia e explicar o resultado. Não inventar amostra ou métricas.
3. Registrar pontos de confusão relativos à UI/feedback versus balanceamento. Confirmar dados no `Neko.Sim` se suspeitar de número incorreto.
4. Propor **primeiro** correção de informação, destaque ou feedback, se resolver a confusão sem mudar mecânicas.
5. Se for necessária alteração de números, expor hipótese, comparação com sementes controladas, trade-offs e decisão explícita no [GDD](../GDD.md).

**Aceite deste trabalho exploratório:** relato de observação, hipótese sustentada/descartada e proposta mínima; não declarar ajuste de balanceamento implementado sem PR e validação específicos.
