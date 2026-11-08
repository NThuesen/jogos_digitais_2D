# ECON-02b — a sala é capacidade, não demanda

Simulação real (`Profile` → `DayPlan` → `Shift` → `Books.Close`), 100 empresas por célula (`seed × 2654435761`), 40 dias (50 para ritmo de níveis).
Dados brutos em [data/](data/); reprodução em [tools/econ-balance](../../../tools/econ-balance/README.md) (`run-all.sh`).
**Baseline** = `main` após as PRs #71–#73 (`-p:Economy=econ02`). **Candidata** = esta PR. Dois caps de elenco: *base* (main) e *+1* (PR #20, simulado, **não** integrada).
Nada aqui é diversão comprovada: é economia simulada com políticas de robô.

## 1. Diagnóstico e causa-raiz (confirmado no HEAD)

| # | Achado | Evidência |
| --- | --- | --- |
| C1 | **Demanda = `0,055 × lugares`** (+0,008/dia). Cada mesa comprada trazia a própria multidão; com os mesmos garçons, mais grupos sentados ⇒ serviço mais lento, pratos abandonados, mesas sujas, reputação perdida. Atendidos *caem* de 9,4 para 6,0 indo de 4 a 12 lugares, com maids ainda ociosas (~40–70%). | ablação nível 1; com demanda **fixa** (0,22) a queda some. |
| C2 | Mesmo uma equipe **épica** perdia dinheiro ao expandir: +fogão +2 mesas = **−22/dia** (pro) e **−65/dia** (casual) no baseline; **+293** e **+105** na candidata. | `data/roi.txt` ("EPIC") — ablação causal: só a regra de chegada mudou. |
| C3 | O gargalo do nível 1 é **1 maid de Salão** (anota + entrega + limpa; papéis são fronteiras rígidas) e **1 fogão** (um prato por vez). Lugares além de ~4 por garçom só entram em fila. | utilização, ROI por item. |
| C4 | No cap da main, `RosterCap` < vagas da escalação (nível 3: 4 maids, 5 vagas; nível 4: 5 × 6…). Contratar não preenche o que o nível abre — é a razão da PR #20. | `CafeLevelTable` × `Lineup.SlotsFor`. |
| C5 | Reputação ritmava os níveis lentos: casual chegava ao nível 3 no dia 31 e ao 4 quase nunca (2/100 até o dia 50). | `data/levels.txt`. |
| C6 | Estilo de toque pesa muito: parado satisfaz 19% dos grupos, casual ~40%, robô ~50%. Parado quebra por volta do dia 7. (Já era o desenho; ver §8.) | matriz. |

## 2. O que foi implementado (e só isso)

`Sim/Economy/DayPlan.cs`:
1. **`ArrivalsPerSecondFor` = constante `0,236/s`**, independente de lugares e do dia. O dia cresce pela duração do turno (75→180 s) e pelo cardápio, ambos já ligados ao nível. (`0,236` é a taxa que o ECON-02 já aplicava no dia 3 do salão inicial; mantém inalterada a corrente de RNG do teste protegido `NoKitchenArchetypeIsAnOutlier`, ver §7.)
2. **Reputação por cliente feliz ×4** (era ×3), `ReputationPerHappyParty`.

Não mudaram: aluguel, salários, preços, ingredientes, `BaseMistakeChance`, caixas, `CafeLevelTable`, save, conteúdo, visual, monetização. Nada toca a PR #20.

## 3. Hipóteses (ciclo: medir → alterar uma coisa → ablação → aceitar/rejeitar)

| Hip. | Mudança | Resultado | Decisão |
| --- | --- | --- | --- |
| H1 | Demanda desacoplada dos lugares | tabelas de ROI invertem (C2); pro "full" 7,0k→18,1k de caixa no dia 40; casual "none" 93%→98% | **Aceita** |
| H2 | Reputação ×3→×4 (testados ×4 e ×5) | casual: nível 3 no dia 31→19, nível 4 no 38 (73/100); ×5 deixa o robô no nível 6 no dia ~37, rápido demais | **Aceita ×4** |
| H3 | Demanda crescendo por nível (+0,01 / +0,02 por nível) | menos caixa em todas as células (pro/full −23%/−35%) sem melhorar a diferença entre decisões; só piora o excesso de fila com o mesmo time | Rejeitada |
| H4 | Demanda alta (0,32) no nível 3 | mesas viram −110 a −150/dia até para pro | Rejeitada |
| H5 | Mexer em aluguel/salário/preços | nenhuma evidência: aluguel = 15% da receita do robô e 25% da casual; as perdas vinham de C1 | Não feita (recomendação em §9) |
| H6 | Tratar poda do elenco: robô sem `Dismiss` superestimava falências com o cap +1 (lazy "hire" quebrava no dia 9) | com poda (política, não regra) a casual com +1 sobrevive 87–98% | Correção **do simulador** |

Sensibilidade à taxa (`data/sensitivity.txt`, cap base): λ = 0,22 / 0,236 / 0,26 — pro full 20,7k / 18,1k / 15,0k contra none 14,2k / 13,2k / 12,2k; casual overexpand 6% / 0% / 0% vivas; casual full lucro/dia 247 / 147 / 123 contra none 118 / 127 / 82. As conclusões qualitativas não dependem do valor exato; maior λ só aumenta a pressão.

## 4. Baseline × candidata (100 empresas, dia 40)

Sobreviventes / caixa média incluindo falidas (= 0). "invest" = política `full` (expande guiada por capacidade e contrata por função).

| Estilo · política | Baseline cap base | Candidata cap base | Baseline cap +1 | Candidata cap +1 |
| --- | ---: | ---: | ---: | ---: |
| pro · nada | 100 · 10 687 | 100 · 13 215 | 100 · 10 687 | 100 · 13 215 |
| pro · invest | 98 · 6 953 | **100 · 18 102** | 100 · 7 572 | **100 · 29 301** |
| pro · só mesas | 11 · 25 | 98 · 6 490 | 19 · 46 | 96 · 7 827 |
| pro · excesso de mesas | 1 · 4 | 88 · 5 818 | 1 · 2 | 86 · 7 347 |
| casual · nada | 93 · 2 438 | 98 · 3 695 | 93 · 2 438 | 98 · 3 695 |
| casual · invest | 84 · 1 459 | 84 · 2 736 (lucro/dia 147 vs 127) | 77 · 996 | **87 · 6 444** |
| casual · só mesas | 0 | 14 · 91 | 0 | 14 · 197 |
| casual · excesso de mesas | 0 | 0 | 0 | 0 |
| lazy · nada / invest | 20 · 160 / 29 · 254 | 23 · 277 / 19 · 251 | 20 / 6 | 23 / 11 |
| idle · qualquer | 0 | 0 | 0 | 0 |
| casual · robô "invest" do ECON-02 | 54 · 450 | 80 · 3 436 | 74 · 886 | 89 · 10 249 |
| pro · robô "invest" do ECON-02 | 87 · 3 969 | 100 · 29 304 | 96 · 4 684 | 100 · 59 321 |

Tabela completa: `data/matrix_*.txt` (9 políticas × 4 estilos × 2 caps × 2 economias).

## 5. Matriz de sobrevivência por estratégia (candidata, cap base / cap +1)

| Política | lazy | casual | pro |
| --- | ---: | ---: | ---: |
| nada | 23 / 23 | 98 / 98 | 100 / 100 |
| conservadora (fogão, contrata com reserva 8×) | 19 / 11 | 94 / 89 | 100 / 100 |
| balanceada (expande por capacidade) | 19 / 11 | 87 / 87 | 100 / 100 |
| só contratação estratégica | 20 / 11 | 94 / 89 | 100 / 100 |
| contratação excessiva (reserva 1×) | 14 / 4 | 91 / 71 | 100 / 100 |
| só mesas | 0 / 0 | 14 / 14 | 98 / 96 |
| excesso de mesas | 0 / 0 | 0 / 0 | 88 / 86 |

A ordem importa mais que os números: **gestão boa ≥ nada > contratação excessiva > mesas sem cozinha > excesso de mesas**, e a perda cresce quando o jogador toca menos.

## 6. ROI e payback por investimento (candidata, pareado, lucro/dia nos dias 2–15)

| Item (preço) | Time comum, nível 1 | Time épico, nível 1 | Nível 3 (rep 200) |
| --- | --- | --- | --- |
| Fogão (180) | pro +46/d (4 d) · casual +26 (7 d) | pro +78 (2,3 d) · casual +54 | pro +21 (9 d) · casual +7 |
| Bar de chá (380, nível 2) | ≈ 0 (+4) | — | pro +13 · casual −3 (situacional: precisa 2º cozinheiro) |
| 2 mesas (240) **sem** cozinha extra | pro −71 · casual −57 (**erro**) | pro +27 · casual −38 | pro −68 · casual −71 (**erro**) |
| Fogão + 2 mesas (420) | pro +55 (7,7 d) · casual +6 | pro **+293 (1,4 d)** · casual +105 (4 d) | pro −13 · casual −22 |
| Fogão×2 + 4 mesas (840) | pro +71 · casual +13 | pro **+374** · casual +127 | pro −27 (com 2 maids) |
| 1 maid contratada (~500) | pro +54 · casual +22 (≈10–20 d) | — | pro **+96 (≈5 d)** · casual +55 (≈9 d) |
| Maid + fogão + 2 mesas | pro **+118** · casual +21 | — | pro +74 · casual +29 |
| Palco ocupado (gerência: `skiprole`) | nível 5, time de 7 comuns: Palco ≈ +150 de receita/dia, Gerência ≈ +60/dia; nenhuma posição é inútil. | | |

Leitura: *produtivo* (fogão, maid na vaga aberta), *condicionado à equipe* (mesas: só pagam com time forte e cozinha), *situacional* (bar de chá) e *decorativo* (móveis de ambiente: fora do escopo de ROI, ver §9). Baseline: as mesas perdem em **todas** as colunas, inclusive para o time épico.

Raridade e escalação (`data/rarity_lineup.txt`, candidata, sem compras): receita por cliente 59→67→84→133 (casual, comum→lendária); abrir com lendárias quebra 82% dos casuais por salário (630/dia contra 600 de caixa) mas, quem sobrevive, lidera em tudo — risco real, pagamento real. Escalação aleatória quebra todos até o dia 8; a escalação "gananciosa por atributo" sem respeitar a posição natural também (0–1% vivos): **a escalação continua sendo a decisão central**.

## 7. Progressão e custos por nível

Mediana do dia em que cada nível é alcançado (entre as empresas que o alcançam em 50 dias) (candidata, sem compras; baseline entre parênteses):

| Estilo | N2 | N3 | N4 | N5 | N6 |
| --- | ---: | ---: | ---: | ---: | ---: |
| casual | 8 (11) | 19 (31) | 38 (48*) | 39* | — |
| pro | 6 (8) | 13 (18) | 24 (33) | 36 (—) | 48 (—) |
| pro · invest | 5 | 11 | 23 | 30 | 39 |
| lazy | 10 (13) | 25 (39) | 45 | — | — |

*poucas empresas. Custos fixos: aluguel `40 + 1,2/tile` (82 no nível 1; 180 no 10), salário por raridade 18 / 44 / 95 / 210 (+4%/nível da maid), ingredientes 22% do prato. Contratação ≈ 180 + 110×nível + 2×soma de atributos.

## 8. Impacto da PR #20 (cap = vagas + 1) e recomendação de integração

* Com a candidata, o cap +1 **não quebra a economia**: casual "nada" idêntico (98%); casual "invest" 84→87% vivos e **2,7k→6,4k** de caixa; pro "invest" 18,1k→29,3k.
* Risco: quem joga pouco e contrata sem reserva sofre mais (lazy 19→11%; casual "contratação excessiva" 91→71%). É risco de gestão real, não bug.
* Recomendação: integrar a #20 **depois** desta PR (a #20 sozinha, no baseline, mantém tudo perdendo com mesas). Sem a #20, o cap < vagas impede a equipe de crescer com os níveis, e o simulador mostra que a maior parte do ganho de expansão vem de ter mais maids na vaga aberta.

## 9. Limitações, riscos e o que precisa de playtest humano

* Política de robô é suposição; layout = primeira célula livre (x ≥ 2), que pode bloquear caminhos; não modela roupas, presentes, habilidades, temas, decoração ou otimização de layout.
* **Falta de cliques**: parado (0 toques) quebra por volta do dia 7 e *lazy* (~20 toques/dia, 1 a cada 4 s) sobrevive só ~23% — o degrau entre ~20 e ~35 toques é íngreme. Isso já existia (o GDD diz "um dia sem toques é o dia de sempre"). Casual (um toque a cada ~2 s) é estável, mas **a promessa de "sobreviver sem muitos cliques" não está resolvida para jogo muito passivo**. Opções que não testei: mais paciência base, ou a recepção sentar sozinha mais rápido. Precisa de decisão de produto e de playtest.
* Robô perfeito acumula caixa demais (13k sem comprar nada no dia 40): não há destino para dinheiro além do cap e dos móveis. Sumidouros tardios ficam para outra tarefa.
* Congestionamento: grupos além da capacidade da equipe ainda geram abandono; a candidata só deixa de **criá-los** ao comprar mesa.
* Ritmo de níveis (H2) e a taxa 0,236 são escolhas de produto sustentadas por simulação, não por jogadores. Reavaliar com telemetria/playtest.
* Seeds pareadas: dia 1 (roteirizado) e a empresa inicial são idênticos entre baseline e candidata; depois a quantidade de clientes sorteados muda, então as comparações são estatísticas (100 empresas), não por cliente. Dentro de uma mesma economia, `roi.sh` pareia empresa e dias.
* **Exceção/aviso sobre o teste protegido** `NoKitchenArchetypeIsAnOutlier`: com λ = 0,22 a razão de uma amostra de 24 saiu 1,366 (> 1,35). Medindo 1 000 amostras, a razão **real** é 1,177 (baseline) e 1,183 (candidata): não houve regressão, a amostra de 24 é só mais uma corrente de RNG. O teste não foi alterado; escolhi λ = 0,236 (taxa do ECON-02 no dia 3) para manter a corrente do teste, e declaro que isso foi uma escolha dentro da faixa 0,22–0,26 testada, não uma descoberta.

## 10. Valores finais

| Constante | Antes | Depois | Por quê |
| --- | --- | --- | --- |
| Chegadas/s | `0,055 × lugares + 0,008 × (dia−1)` (0,12–1,1) | `0,236` fixo | C1/C2 (H1) |
| Reputação por cliente feliz | 3 | 4 | C5 (H2) |

Testes novos (`Tests/EditMode/Sim/EconomyStructureTests.cs`): demanda independente de lugares/dia; reputação monótona e ≥ 0; livros fecham (`BalanceAfter = Before + Receita − Custos`); determinismo; equipe épica ganha ≥ 5% com fogão + 2 mesas; mesas sem cozinha numa equipe comum rendem menos que o controle. Dois deles falham no baseline.
