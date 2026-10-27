# Neko Maid Inc. — Game Design Document

**Jogos Digitais 2026.1 · Insper · Projeto 2D**
Unity 6000.5.9f1 · 2D URP · build WebGL · entrega no itch.io

> Documento vivo e fonte de verdade para a versão atual do jogo: regras, economia,
> progressão e experiência do jogador. A versão no Milanote deve refletir este arquivo.
> Para a organização técnica, consulte `docs/ARCHITECTURE.md`.

---

## 1. Blueprint: conceito e narrativa

**Você não atende ninguém. Você escala quem atende.**

A Neko Maid Inc. é um café de maids-gato que o jogador administra como uma empresa. Ele não
serve mesas: ele monta o time que serve. Antes de abrir as portas decide quem fica na
recepção, no salão, na cozinha, no palco e na gerência — e depois assiste ao dia acontecer
sem poder interferir.

O tom é a piada central: fofura por fora, planilha por dentro. O letreiro tem orelhas de
gato e um selo de latão escrito "Inc.". As maids têm personalidade, afinidade e conversas; o
que mata o jogador é o fluxo de caixa.

**Mundo.** Um salão de bairro em algum lugar entre Akihabara e o centro de São Paulo. Seis
"casas" formam maids — Tsukimi, Hanabi, Kurogane, Shirayuki, Momiji, Hoshizora — e cada maid
carrega a casa onde se formou, um temperamento e um estilo. Quem veio da mesma casa trabalha
melhor junto; quem combina com a decoração do salão rende mais.

**Personagens.** Nove arquétipos, cada um uma *regra* e não um conjunto de números: a Genki
corre 35% mais rápido e cansa 60% mais; a Ojou-sama atrai clientes VIP e se recusa a limpar
mesa; a Dojikko derruba um prato a cada oito e o salão inteiro acha adorável.

**Arco.** Dia 1: três cartas comuns, um salão de quatro lugares, e o caixa mal cobrindo a
semana. Dia 40: um elenco escolhido, química alta, caixas douradas, e o custo fixo de um
time de lendárias que pode levar a empresa à falência se o jogador se empolgar.

## 2. Mechanics — as mecânicas do jogo

### 2.1 O laço

```
ESCALAR → ABRIR O DIA → ASSISTIR → BALANÇO → LOJA / CAIXAS → ESCALAR
```

Tudo o que o jogador faz acontece **entre** dias. Durante o dia há exatamente três
controles: velocidade 1×, 2×, 4× e pular para o fim. Nenhum deles muda o resultado.

### 2.2 As cinco posições

| Posição | O que faz | Atributo |
| --- | --- | --- |
| **Recepção** | recebe e leva à mesa | Charme, depois Salão |
| **Salão** | anota pedidos, leva pratos, limpa mesas | Salão |
| **Cozinha** | cozinha as comandas | Cozinha |
| **Palco** | não atende: segura a paciência do salão | Charme |
| **Gerência** | não atende: mais gorjeta e mais fichas | Fôlego, depois Charme |

Recepção, salão e cozinha são obrigatórias — sem qualquer uma delas o dia não abre, e o
jogo diz qual falta. Vagas crescem com o nível: começa 1/1/1 e chega a 2/3/3 + palco +
gerência.

**Fora de posição custa 35% da eficiência**, zera o bônus de química de posição e quase
dobra a chance de erro. Uma cozinheira lendária no salão rende menos que uma garçonete
comum.

### 2.3 Química

Três etiquetas por maid: **Casa** (6), **Temperamento** (3, derivado do arquétipo) e
**Origem** (4, que são os mesmos temas da decoração).

- **+3** se está na posição natural dela
- **+0 a 4** por vínculos com quem divide a posição: cada etiqueta em comum vale 1, até 2
  por vínculo
- **+3** se a Origem bate com o tema dominante do salão
- **+0 a 2** de traços

Média × 10 = **química do time, de 0 a 100**. Multiplica todos os atributos de todo mundo de
**0,75× a 1,25×** — um terço a mais de café entre uma escalação ruim e uma boa com as mesmas
cartas.

### 2.4 O dia, por dentro

```
porta → fila → mesa → pedido → comanda → estação → prato → entrega → consumo → conta → limpeza
```

Cada seta é uma tarefa que alguém precisa andar até lá para executar. As maids escolhem
sozinhas, por urgência e depois por distância, mas **só dentro da própria posição**.

### 2.5 Os três erros

| Erro | Causa | Custo |
| --- | --- | --- |
| **Prato queimado** | atributo Cozinha baixo | tempo da cozinha e ingredientes |
| **Prato derrubado** | só a Dojikko | tempo, e paciência para o salão inteiro |
| **Pedido errado** | atributo Salão baixo, fora de posição, química ruim | **35% da paciência da mesa** e a comanda refeita |

O pedido errado é a linha mais barulhenta do balanço de propósito: é o que uma maid no lugar
errado faz com uma mesa.

### 2.6 As nove personalidades, escritas como regras

A diversidade de maids é o apelo do jogo, então nenhum arquétipo é só um retrato: cada um é
uma alteração mensurável na simulação. Atributos iniciais na ordem **Salão · Cozinha ·
Charme · Fôlego**.

| Arquétipo | Início | O que a regra faz | Posição natural |
| --- | --- | --- | --- |
| **Genki** | 32·14·24·18 | anda 35% mais rápido e gasta ânimo 60% mais rápido | Salão |
| **Kuudere** | 18·32·14·26 | **nunca erra um pedido**, anda 20% mais devagar | Cozinha |
| **Tsundere** | 28·20·22·20 | +30% de velocidade com o salão acima de 60% cheio; −15% de gorjeta com ele vazio | Salão |
| **Dojikko** | 24·18·30·16 | derruba 12% dos pratos, e a mesa acha graça | Recepção |
| **Ojou-sama** | 16·12·38·18 | dobra a chance de VIP aparecer; **não limpa mesa** | Recepção |
| **Onee-san** | 24·24·24·22 | +15% de velocidade para quem divide a posição e passa 25% do XP dela | Gerência |
| **Idol** | 20·12·36·20 | sobe no palco sozinha a cada 90 s e devolve 70% da paciência do salão | Palco |
| **Gourmand** | 14·36·18·22 | cozinha a 1,8× e **come 20% dos pratos** | Cozinha |
| **Mukuchi** | 12·34·20·24 | recebe a 0,6×, cozinha a 1,5×, e o Charme dela **dobra na estação** | Cozinha |

Cada um tem cor de destaque e silhueta próprias — maria-chiquinha alta na Genki, tiara na
Ojou-sama, presilha de estrela na Idol — porque com seis maids no salão o jogador precisa
saber quem fez o quê sem ler nome nenhum.

A regra lida com a escalação: a Gourmand come, então ela compensa num café com cozinha
sobrando e não num apertado. A Ojou-sama atrai VIP, e VIP só espera 22 segundos — convidar
VIP sem salão para atendê-lo é um prejuízo que o jogador mesmo contratou.

### 2.7 Os sete tipos de cliente

| Cliente | Paciência | Gasta | Peso | Nível | A regra |
| --- | --- | --- | --- | --- | --- |
| **Cliente** | 45 s | 1,0× | 10 | 1 | o caso base |
| **Estudante** | 62 s | 0,7× | 6 | 1 | fica, gasta pouco, não reclama |
| **Assalariado** | 30 s | 1,4× | 5 | 2 | tem vinte minutos de almoço |
| **Gato** | 60 s | 0,4× | 3 | 2 | **nunca fica impaciente**, pede só leite e melhora o ambiente enquanto está sentado |
| **Fã** | 50 s | 1,0× | 4 | 3 | quer **um arquétipo específico**; paga 1,8× de gorjeta se ela estiver escalada |
| **Crítico** | 35 s | 1,2× | 2 | 4 | **julga**: a reputação do dia depende dele |
| **VIP** | 22 s | 3,0× | 2 | 6 | uma fortuna, e nenhuma espera |

O Fã é a peça que fecha o laço: ele transforma a escalação de hoje em receita de hoje por um
caminho que não passa por atributo nenhum. Escalar a maid da moda é uma decisão de negócio.

### 2.8 Raridade, salário e as caixas

Quatro raridades. O que elas mudam é o teto dos atributos sorteados **e o salário diário**,
e é o salário que faz da escalação uma decisão de orçamento em vez de uma lista das melhores
cartas.

| Raridade | Salário/dia | Rescisão | Cor da moldura |
| --- | --- | --- | --- |
| **Comum** | 18 | 5 fichas | bege |
| **Rara** | 44 | 20 fichas | azul |
| **Épica** | 95 | 70 fichas | violeta |
| **Lendária** | 210 | 220 fichas | dourado |

Uma lendária custa doze comuns por dia. Tirá-la de uma caixa é o começo do problema: ou o
café cresce para sustentá-la, ou ela afunda o caixa enquanto a química fica igual.

Três caixas, compradas só com fichas, com as chances **impressas na tela de compra**:

| Caixa | Pontos | Cartas | Piso | Chances |
| --- | --- | --- | --- | --- |
| **Simples** | 40 | 1 | comum | 78% · 18% · 3,6% · 0,4% |
| **Prata** | 180 | 3 | rara | — · 62% · 32% · 6% |
| **Dourada** | 520 | 3 | épica | — · — · 78% · 22% |

Cada caixa tem um **piso de raridade**: a pior carta da Dourada é épica. Carta repetida com
o elenco cheio volta como fichas, então uma caixa nunca é prejuízo total. Os pisos são
monotônicos — a caixa mais cara é sempre a melhor — e isso é um teste, não uma intenção.

Não existe comprar fichas, e não existe converter ficha em dinheiro. As duas moedas não se
tocam, o que mantém a única pressão de verdade — o fluxo de caixa — fora do alcance da
sorte.

## 3. Feedback e clareza de interação

O estado é dito pelo mundo antes de ser dito pela interface:

- A **paciência é a cor do cliente inteiro**, não uma barra: verde, âmbar, vermelho. Quem
  está quase indo embora **treme**, e treme mais quanto mais perto do fim.
- A **barra de paciência só aparece quando a pessoa está realmente esperando.**
- Um **balão** sobre a mesa mostra o próximo prato que falta, desenhado.
- **Pedido errado** sacode a câmera, solta partículas de irritação e escreve na tela.
- O **ânimo da maid** só aparece sobre a cabeça quando cai abaixo de 45%, e ela muda de pose
  quando está exausta.
- A **música troca** quando o salão passa de 70% ocupado ou faltam 20 segundos.
- Cada arquétipo tem **a própria voz** — nove blips distintos, para saber quem fez o quê de
  olhos em outro canto.

## 4. UI/HUD e menus

Nove telas, todas construídas em código, nenhuma com widget cinza padrão do Unity:

| Tela | Para quê |
| --- | --- |
| **Menu** | a identidade: placa pendurada, orelhas, selo "Inc.", três maids no salão |
| **Empresa** | caixa, fichas, custo fixo do dia, nível, e a escalação de hoje com a química |
| **Escalação** | a tela que é o jogo: cinco posições, banco, química ao vivo |
| **Elenco** | as cartas e uma lida por inteiro; dispensar |
| **Caixas** | três caixas com as chances impressas, e a abertura carta a carta |
| **Loja** | móveis e roupas, com dinheiro |
| **Salão** | arrumar móveis, com o mapa de calor de caminhada |
| **O dia** | relógio, caixa, fichas das maids, velocidade e pular |
| **Balanço** | entrou, saiu, o que deu errado, lucro, caixa |

Toda a tipografia de título usa espacejamento controlado letra a letra, porque uGUI não tem
*tracking* e tracking é a diferença entre um letreiro e um rótulo.

## 5. Engajamento e imersão

O gancho é o mesmo do Ultimate Team: **a próxima carta**. Fichas vêm de dias bem atendidos,
caixas vêm de fichas, e uma lendária muda o time de verdade. O ritmo é curto — um dia dura
de 75 a 180 segundos — e cada dia termina num número que o jogador quer melhorar.

A tensão vem de duas moedas que puxam para lados opostos: **dinheiro** paga salário e
aluguel e pode falir a empresa; **fichas** só compram caixas e são pagas por *qualidade*,
não por volume. Um dia que fatura muito atropelando todo mundo rende poucas fichas.

## 6. Level design e progressão

Não há fases. A progressão é o **nível do café** (1 a 10), ganho com reputação, que por sua
vez vem de clientes bem atendidos — nunca de faturamento.

Cada nível entrega pelo menos uma coisa: salão maior, mais uma vaga de elenco, mais uma vaga
numa posição, ou receitas, móveis e roupas novas. Um teste percorre os dez níveis e falha se
algum não trouxer nada.

**A curva foi medida, não escolhida.** Uma versão anterior tinha meta diária com expoente
1,35 sobre o dia; a medição mostrou que o faturamento de um café não muda com o dia, muda
com o tamanho — e o dia 2 era literalmente invencível. A meta saiu do jogo; o que pressiona
agora é a folha de pagamento.

## 7. Design de som e música

26 arquivos, todos sintetizados, nenhum gravado ou baixado. Balanceados contra a mediana do
próprio conjunto com `sfx_measure` até o veredito *"nothing sticks out in loudness or
brightness"*.

- **Efeitos**: sino da porta, pedido, comanda, chiado, prato, caixa registradora, moeda,
  coração, bufo, queda, subida de nível
- **Nove vozes**, uma por arquétipo
- **Três trilhas** de oito segundos em laço: menu, turno calmo, turno corrido

Três passagens de medição corrigiram um chiado sete vezes mais brilhante que o resto, dois
sons de interface construídos sobre um preset agudo demais, e um prato caindo mais baixo que
a mediana.

## 8. Efeitos visuais e polimento gráfico

**Nenhuma imagem é importada.** Dezessete formas geradas por campos de distância compõem
tudo — maids, clientes, móveis, ícones, o letreiro. Uma maid é uma pilha de camadas tingidas,
e é por isso que o vestuário é visível: equipar uma peça troca uma camada.

**Direção visual aprovada (ainda não implementada):** cada identidade de maid deverá ter
um **portrait expressivo para o card** e um **chibi legível para o restaurante**, com
identidade e módulos visuais consistentes entre as duas apresentações. O card permanece
composto pela UI. A decisão, os pontos em aberto e o contrato de spec por identidade
estão em [docs/art/MAID_VISUAL_DECISION.md](art/MAID_VISUAL_DECISION.md); o texto acima
descreve somente a implementação atual.


A paleta foi fechada com `pixels palette_check`: os nove acentos de arquétipo ficam numa
escada de L\* de 40 a 72, quatro pontos entre vizinhos, todos abaixo da faixa de valor da cor
de atenção e todos menos saturados que ela.

Partículas, números flutuantes, esmagamento e estica, tremor de câmera e surgimento com
overshoot — tudo em filas reaproveitadas, sem `ParticleSystem` (que exigiria um material, que
é um asset).

## 9. Rejogabilidade

- **A falência reinicia a empresa**, mas os recordes ficam — melhor dia, melhor faturamento,
  lendárias encontradas, empresas perdidas.
- As cartas são **sorteadas**: nenhuma empresa começa igual, e nenhuma coleção termina igual.
- Doze traços, nove arquétipos, quatro raridades e três etiquetas dão milhares de cartas
  distintas.
- A escalação é um quebra-cabeça com resposta diferente a cada elenco.

## 10. Balanceamento

Tudo o que é número está em `Neko.Sim`, que não referencia o UnityEngine — então um dia
inteiro roda em milissegundos num teste e 200 dias semeados cabem numa asserção.

| Afirmação | Como é verificada |
| --- | --- |
| Escalação em posição vale mais que fora dela | 12 sementes, receita comparada |
| A química vale o que anuncia | 0,75× a 1,25×, com o teste da razão |
| Nenhum arquétipo é peso morto | o pior ganha ao menos 45% do melhor |
| A caixa mais cara é sempre melhor | piso de raridade monotônico |
| Nenhuma caixa rola abaixo do próprio piso | 300 sementes por caixa |
| Fichas pagam qualidade, não volume | dia corrido rende menos ficha que dia tranquilo |
| Nenhum nível desbloqueia nada | percorre os dez |

## 11. Gerenciamento de estados e erros

- **Máquina de estados explícita**: Boot → Menu → Empresa → Dia → Balanço, com as transições
  escritas numa tabela. Uma transição ilegal é recusada e reportada, nunca lançada.
- **Nada trava.** Toda chamada de botão e o tique do dia passam por uma barreira que captura
  qualquer exceção, encerra o dia como falha explicada e devolve o jogador à empresa.
- **O save tem versão e checksum.** Arquivo truncado, byte trocado, save de versão mais
  nova, JSON de outro programa e uma sequência de zeros: todos voltam como mensagem legível
  em português e uma empresa jogável. Nenhum caminho lança.
- **Cultura invariante** em todos os números gravados, com um teste que grava em pt-BR e lê
  em invariante.
- **Sem becos sem saída.** O dia não abre se faltar recepção, salão, cozinha, mesa ou
  estação — e a mensagem diz qual tela resolve. Clientes nunca pedem prato que a cozinha não
  possa fazer. Dispensar uma maid sempre corta a despesa. Uma carta que não cabe no elenco
  vira fichas em vez de sumir.
- **A falência é avisada com um dia de antecedência**, em vermelho, antes de abrir o dia
  seguinte.

## 12. Este documento

Este GDD descreve exclusivamente a versão vigente. Mudanças de mecânicas, economia,
progressão ou UX devem ser documentadas aqui no mesmo Pull Request da implementação.
As fronteiras técnicas estão em `docs/ARCHITECTURE.md`.

**Verificação:** 257 testes de simulação (`tools/simtest.sh`, menos de um segundo), 292 de
EditMode e 17 de PlayMode dentro do editor. Sete deles leem este documento: se um arquétipo,
um tipo de cliente, uma posição, uma raridade ou uma caixa entrar no jogo e não entrar aqui,
a suíte falha com o nome que está faltando. Onze quadros do jogo rodando ficam em
`../visual_assets/frames/2026-10-22_neko_maid_inc/`, com o registro dos oito erros visuais
que as capturas encontraram.
