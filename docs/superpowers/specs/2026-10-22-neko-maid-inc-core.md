# Neko Maid Inc. — o core

**Substitui** o core de `2026-10-22-nekomata-cafe-design.md`. Aquele documento continua
válido para o mundo, os arquétipos, a arte e o áudio; **o loop de jogo descrito aqui é o que
vale.** O jogo passou de "gerenciar em tempo real" para **escalar um time e ver o dia
acontecer**, e mudou de nome.

**Decidido pelo dono em 2026-10-22**, nas palavras dele: *"a ideia é montar um ultimate team
(fifa), mas com maids no restaurante"*, *"o jogo vai rodar sozinho sem loop, vc faz as
alterações/melhoria entre dias"*, *"se a empresa vai a falência é gameover"*.

---

## 1. A frase

**Você não atende ninguém. Você escala quem atende.**

O dia inteiro roda sozinho. A única coisa que o jogador faz é decidir, antes do sino tocar,
**quem joga onde** — e depois assistir para descobrir se estava certo.

## 2. O laço, em quatro passos

```
ESCALAR  →  ABRIR O DIA  →  ASSISTIR  →  BALANÇO
   ↑                                        ↓
   └────────────  LOJA / CAIXAS  ←──────────┘
```

1. **Escalar.** Pôr maids nas cinco posições. Quem recepciona, quem atende o salão, quem
   cozinha, quem sobe no palco, quem fica na gerência.
2. **Abrir o dia.** Um botão. A partir daqui o jogador não tem mais nenhum controle.
3. **Assistir.** O turno roda sozinho, 75–180 segundos. Dá para olhar, acelerar ou pular.
   Não dá para interferir — e é isso que faz a escalação importar.
4. **Balanço.** Um painel de receita: o que entrou, o que saiu, o que deu errado, e o saldo.
5. **Loja.** Pontos viram caixas, caixas viram maids, moedas viram móveis e melhorias.

## 3. As cinco posições

Cada maid tem uma **posição natural**. Escalada fora dela, perde eficiência e química —
exatamente como no Ultimate Team.

| Posição | O que faz no dia | Atributo que manda |
| --- | --- | --- |
| **Recepção** | recebe quem chega e leva à mesa | Charme, depois Salão |
| **Salão** | anota pedidos, leva pratos, limpa mesas | Salão |
| **Cozinha** | cozinha as comandas | Cozinha |
| **Palco** | não executa tarefa: segura a paciência do salão inteiro | Charme |
| **Gerência** | não executa tarefa: aumenta gorjeta e pontos do dia | Fôlego, depois Charme |

Vagas por posição crescem com o nível do café: começa com **1 Recepção, 1 Salão, 1 Cozinha**
e chega a **2/3/3 + 1 Palco + 1 Gerência**.

**Fora de posição** custa 35% da eficiência e zera o bônus de posição na química. Uma
cozinheira lendária no salão rende menos que uma garçonete comum.

## 4. Química

Três etiquetas por maid, no espírito de clube/liga/nacionalidade:

| Etiqueta | O que é | Quantas |
| --- | --- | --- |
| **Casa** | onde ela se formou | 6 |
| **Temperamento** | como ela trabalha | 3 (Enérgica, Serena, Dramática) |
| **Origem** | o estilo dela | 4 (Sakura, Gótico, Estelar, Shōwa) |

**Como pontua** (0 a 10 por maid):

- **+3** se está na posição natural dela.
- **+0 a 4** pelos vínculos com quem divide a mesma posição: cada etiqueta em comum vale 1,
  até 2 por vínculo.
- **+3** se a Origem dela bate com o tema dominante da decoração do salão.

**Química do time** = média × 10, de 0 a 100. Ela multiplica os atributos efetivos de todo
mundo: `0,75 + 0,5 × química/100`, ou seja **de 0,75× a 1,25×**. Um time de lendárias mal
escalado perde para um time de comuns bem escalado — que é o ponto inteiro do gênero.

## 5. Raridade

| Raridade | Atributos iniciais | Traços | Chance na caixa comum |
| --- | --- | --- | --- |
| **Comum** | 18–32 | 0 | 78% |
| **Rara** | 28–46 | 1 | 18% |
| **Épica** | 40–62 | 2 | 3,6% |
| **Lendária** | 55–78 | 3 | 0,4% |

**Traços** são modificadores extras sorteados na abertura da caixa — *Mão de Anjo* (nunca
erra pedido), *Pé Ligeiro* (+15% de deslocamento), *Carisma* (+20% de gorjeta) e por aí. É o
que faz duas Lendárias da mesma personalidade serem cartas diferentes.

**Salário sobe com a raridade.** Uma Lendária no banco ainda custa todo dia, e é aí que a
escalação vira decisão de orçamento em vez de "use as melhores".

## 6. As duas moedas

| | Como ganha | Para que serve | Se acabar |
| --- | --- | --- | --- |
| **Moedas** | receita do dia | salários, aluguel, móveis, melhorias | **falência** |
| **Fichas** | desempenho do dia | caixas | nada acontece |

Fichas vêm da **qualidade**, não do volume: cliente satisfeito vale ficha, pedido errado
tira. Então um dia que fatura muito atropelando todo mundo rende pouca ficha, e um dia
tranquilo bem atendido rende muita. As duas moedas puxam para lados diferentes de propósito.

## 7. O balanço do dia

O painel que fecha o dia, linha por linha:

**Entrou**
- Pratos servidos (por receita, quantidade × preço)
- Gorjetas (Charme × química × ambiente)
- Bônus de meta

**Saiu**
- Salários (um por maid do elenco, escalada ou não)
- Aluguel (sobe com o tamanho do salão)
- Ingredientes (por prato feito — inclusive os errados e os queimados)

**O que deu errado**
- Pedidos errados · Pratos queimados · Clientes perdidos · Não couberam

**Resultado**
- Lucro ou prejuízo · Saldo em caixa · Fichas ganhas

## 8. Falência

**Saldo abaixo de zero no fim de um dia = fim de jogo.** O save daquela empresa é encerrado,
os recordes ficam, e o jogador começa outra empresa do zero.

Duas defesas para que isso seja justo e não cruel:

1. **Aviso de um dia.** Quando o saldo fica abaixo de dois dias de despesa fixa, o jogo
   avisa, em vermelho, antes de abrir o dia seguinte.
2. **Demissão.** Dá para dispensar uma maid a qualquer momento e recuperar parte do que ela
   custou. Sempre existe um movimento que corta a despesa.

Não há mais "dia fraco". A meta sumiu como condição de derrota; o que mata é o caixa.

## 9. O que o jogador faz, afinal

Tudo entre dias:

- **Escalação** — as cinco posições, e quem fica no banco.
- **Elenco** — ver atributos, traços, química, salário; demitir.
- **Loja** — móveis e estações (moedas); caixas (fichas).
- **Caixas** — abrir, ver o que veio, mandar para o elenco.
- **Salão** — arrumar móveis; o tema da decoração entra na química.

## 10. Identidade

**Neko Maid Inc.** — o nome é o tom: fofo por fora, planilha por dentro.

- **Logotipo**: letreiro desenhado em código, não tipografia de sistema. Uma silhueta de
  orelha de gato sobre o "I" de Inc., em cima de uma placa de café.
- **Menu**: nada de botão cinza padrão. Placa de madeira, letreiro, três maids em parada,
  luz quente, e o registro "Inc." em letra miúda — a piada visual do jogo inteiro.
- **2D**, gerado em tempo de execução, nenhum asset importado. Como antes.

## 11. O que sai do jogo anterior

- O gerente como avatar jogável, os cliques no salão, o "Apressar", o `ClickTargeting`.
- A meta como condição de derrota.
- O `ShiftResult` como "dia fraco / meta batida".

## 12. O que fica

Toda a simulação de atendimento — chegada, fila, mesa, comanda, estação, prato, pagamento,
limpeza, paciência —, os nove arquétipos como personalidade e regra, a grelha do salão e
suas métricas, a arte procedural, o conjunto de som e o save com versão e checksum.
