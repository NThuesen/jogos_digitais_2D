# ECON-02 — curva de dinheiro, com e sem investimento, com e sem o cap de ECON-01B

Reprodutível com `tools/econ-balance/run-all.sh 40 40` (saída bruta em [matrix.txt](matrix.txt)). 40 empresas (seeds `i × 2654435761`), 40 dias, simulação real (`Profile` → `DayPlan` → `Shift` → `Books.Close`).

## O que mudou no código (PR #72)

Aluguel `40 + 1,2/tile` (era `60 + 2,4`), reputação por cliente feliz ×3, base de cozinha `0,8 + cook` (era `0,6`), Kuudere cozinha ×1,2, fichas por prato perfeito/combo. `BaseMistakeChance` ficou em 0,20 (um teste protegido o fixa). Motivo: no baseline ECON-01A **nenhum** café passava do dia 4.

## Estilos de jogo e política de investimento

| Estilo | Comportamento | Toques/dia |
| --- | --- | ---: |
| idle | nenhum toque | 0 |
| lazy | olha o salão a cada 4 s, acerta 30% das faixas douradas | ~20 |
| casual | a cada 1,2 s, acerta 50% das faixas | ~35 |
| pro | robô perfeito | ~55–75 |

Com **investimento** ("invest"), toda manhã: abre a melhor caixa que as fichas pagam; contrata o candidato de melhor atributo/preço quando há vaga e sobra caixa ≥ 4× custo fixo; compra uma peça por manhã (fogão, fogão, mesas só com ≥4 maids, bar de chá, mesas) com reserva ≥ 3× custo fixo; `AutoFill` da escalação. "no-invest" faz só o `AutoFill`.

## Resultados (cap atual, sem a PR #20) — sobreviventes de 40 no dia 40

| Estilo | no-invest | invest |
| --- | ---: | ---: |
| idle | 0 (mediana de quebra ≈ dia 8–10) | 0 |
| lazy | 7 | 7 |
| casual | **37** | 19 |
| pro | 40 | 34 |

## Comparação com a PR #20 (cap = vagas + 1)

| Estilo (invest) | cap atual: vivos / receita / maids no dia 40 | cap +1: vivos / receita / maids |
| --- | --- | --- |
| casual | 19 / 321 / 3,6 | 16 / 526 / 5,9 |
| pro | 34 / 379 / 4,1 | 35 / 552 / 5,8 |
| lazy | 7 / 305 / 3,6 | 0 / — / 5,2 (dia 20) |

Sem investimento o cap não muda nada (ninguém contrata). Com investimento o cap +1 **deixa contratar e abrir caixas que viram maids** (hoje, no nível 1, o cap é 2 com 3 maids, então toda caixa vira fichas): a receita sobe ~60% no dia 40, mas a folha também (95 → 150), e o jogador mais passivo quebra mais. Conclusão: a PR #20 não conflita com #72 (arquivos diferentes: ela só toca `CafeLevelTable`), melhora o teto, e torna o investimento relevante; ela aumenta o risco para quem joga pouco. Não alterei a #20.

## Conclusões

1. **A base está viável**: parado sangra devagar, casual/atento sobrevive e cresce sem comprar nada (37/40 e 40/40).
2. **Casual não precisa de cliques constantes, mas precisa de atenção**: ~35 toques em ~75 s (um a cada 2 s) estabiliza; ~20 toques (lazy) quebra a maioria até o dia 40, embora dure ~20 dias.
3. **A expansão não se paga hoje** — achado principal. No nível 1, com 3 maids, cada mesa extra **reduz** a receita (ablação, 40 seeds, dia 4: base 115; +1 fogão 135; +1 mesa 101; +2 mesas 84; 3 fogões + 2 mesas 118), porque a demanda cresce com os assentos (`0,055/s por lugar`) e o time é o mesmo: mais clientes irritados, mais reputação perdida. Fogões ajudam (+17% a +26%), mas pagam-se em ~9 dias. Mesmo com 5–6 maids e 12–15 lugares o robô perfeito fatura menos que sem expansão (≈ 550 contra 770 no dia 40). Quem "investe" quebra mais que quem não compra nada.
4. A rentabilidade da expansão não foi corrigida aqui, de propósito: mexer na demanda por assento ou no aluguel por tile para "fazer a conta fechar" seria ajuste por resultado, e exige decisão de design (ECON-02b). Esta PR só cobre a curva base.

## Limitações

- Política de investimento é uma suposição; colocação de móveis é a primeira célula livre (x ≥ 2) e pode bloquear caminhos; otimização de layout, roupas, presentes, skills e temas não foram modeladas.
- Toques do robô são mais regulares que os de uma pessoa; "diversão" não é medida por simulação.
- Sem playtest humano. Seeds iguais entre estilos (pareadas), mas o fluxo de RNG muda quando há compras/caixas.
- O cap +1 foi simulado trocando `CafeLevelTable` por uma cópia da versão da #20 (`tools/econ-balance/alt/`), não mergeando a PR.
