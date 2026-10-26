# Página do itch.io — Neko Maid Inc.

Texto pronto para colar na página do jogo. Cada bloco abaixo é um campo do itch.io; o que
está entre colchetes é instrução para quem publica, não texto da página.

---

## Título

Neko Maid Inc.

## Tagline (short description, 120 caracteres)

Você não atende ninguém. Você escala quem atende — e depois assiste.

## Classificação

- **Gênero:** Simulation
- **Tags:** `management`, `cafe`, `card-collecting`, `gacha`, `cute`, `2d`, `unity`,
  `webgl`, `singleplayer`, `no-input-gameplay`
- **Plataforma:** HTML5 (roda no navegador, nada para instalar)
- **Entrada:** mouse apenas
- **Resolução:** 1280×720, desktop
- **Idioma:** português do Brasil

---

## Descrição da página

### Um café de maids-gato administrado como uma empresa

Neko Maid Inc. é um jogo de **escalação**. Não existe nada para clicar enquanto o café está
aberto: a única coisa que decide se o dia foi bom é quem você pôs em cada posição antes de
abrir a porta.

O dia tem quatro passos, e você só age em dois deles.

1. **Escalar.** Cinco posições — Recepção, Salão, Cozinha, Palco e Gerência. Cada maid tem
   uma posição natural; fora dela ela perde um terço da eficiência e erra quase o dobro.
2. **Abrir o dia.** As portas abrem e o jogo assume.
3. **Assistir.** Os clientes entram, pedem, esperam, elogiam ou vão embora. Você olha.
4. **Ler o balanço.** Entrou, saiu, o que deu errado, e o que sobrou no caixa.

Depois disso é a loja, as caixas, e o dia seguinte.

### Química, no sentido do Ultimate Team

Toda maid carrega três etiquetas: **casa**, **temperamento** e **origem**. Duas maids na
mesma posição que compartilham etiqueta se ligam; a origem bate ou não bate com o tema do
seu café. A química do time multiplica **todos** os atributos de 0,75× a 1,25×.

É o número mais importante do jogo e o motivo de ele não ser uma corrida por cartas raras:
uma escalação bem ligada de cartas comuns bate uma escalação desconexa de lendárias, e
custa um sétimo do salário.

### Nove personalidades que mudam a conta

Genki, Kuudere, Tsundere, Dojikko, Ojou-sama, Onee-san, Idol, Gourmand e Mukuchi. Não são
retratos — são regras. A Idol dispara o próprio número no palco sem ninguém pedir; a
Dojikko derruba pratos e os clientes acham graça; a Mukuchi não fala com cliente nenhum e
é a melhor cozinheira que você vai ter.

### Duas moedas, e só uma pode te quebrar

- **Dinheiro** paga salário, aluguel e ingrediente. Se o caixa ficar em zero ou negativo
  após o balanço do dia, a empresa vai à falência: os recordes ficam, a empresa não.
- **Fichas** só compram caixas, e são pagas por **qualidade de atendimento** — cliente
  satisfeito, pedido certo, mesa girada — nunca por faturamento. Não existe como comprar
  fichas, e não existe como transformá-las em dinheiro.

### Caixas com as chances impressas na própria caixa

| Caixa | Pontos | Cartas | Chances |
| --- | --- | --- | --- |
| Simples | 40 | 1 | 78% comum · 18% rara · 3,6% épica · 0,4% lendária |
| Prata | 180 | 3 | nenhuma abaixo de rara — 62% rara · 32% épica · 6% lendária |
| Dourada | 520 | 3 | nenhuma abaixo de épica — 78% épica · 22% lendária |

As chances estão escritas na tela de compra, não no rodapé de um regulamento. Carta
repetida com o elenco cheio volta como fichas; uma caixa nunca é prejuízo total.

E a conta é honesta nos dois sentidos: a lendária custa **210 por dia** de salário contra
18 da comum. Tirar a carta boa é o começo do problema, não o fim dele.

---

## Nada aqui foi importado

Não existe um único arquivo de imagem ou de som neste projeto.

- **Toda a arte** é gerada em tempo de execução a partir de dezessete campos de distância.
  Uma maid é uma pilha de camadas tingidas; o letreiro do menu são retângulos arredondados
  e dois triângulos.
- **Todo o áudio** — 26 sons — é sintetizado por um script do repositório, que descreve cada
  som como uma pilha de vozes e escreve o `.wav`. O mesmo comando produz sempre os mesmos
  bytes.
- **A fonte** é a embutida do Unity.

O que há para creditar é de onde vem o desenho: a camada de gerenciamento do **Café Mania /
Diner Dash** e o **Ultimate Team** do FIFA. Nenhum código, arte ou áudio veio de nenhum dos
dois.

---

## Como jogar

Mouse apenas.

- **Escalação:** clique numa posição, clique na maid do banco. `Auto` preenche pelas
  posições naturais.
- **Durante o dia:** nada. `Pular` vai direto ao balanço.
- **Loja e caixas:** móveis e roupas com dinheiro, caixas com fichas.

O jogo salva automaticamente após as mudanças na empresa e ao concluir cada dia.

---

## Ficha técnica

- Unity 6000.5.9f1, 2D URP, build WebGL (Gzip com fallback de descompressão)
- A simulação inteira é uma assembly **sem referência ao UnityEngine**: um dia de café roda
  em milissegundos num teste comum, que é o único jeito honesto de afirmar que uma escalação
  é melhor que outra
- 257 testes de simulação, 292 EditMode, 17 PlayMode
- Uma cena com um objeto vazio; câmera, salão, interface e áudio construídos em código, sem
  nenhum prefab

**Jogos Digitais 2026.1 · Insper · Projeto 2D**

---

## Capturas [ordem de upload]

Da pasta `visual_assets/frames/2026-10-22_neko_maid_inc/`:

1. `00_01_menu` — a capa da página
2. `02_03_lineup` — a tela que é o jogo
3. `08_09_day_busy` — as seis escaladas trabalhando
4. `10_11_books` — o balanço
5. `04_05_boxes` — as chances impressas
6. `03_04_roster` — uma carta lida por inteiro

A primeira é a imagem de capa (`00_01_menu`): o letreiro pendurado com as orelhas, o selo
"Inc." e três maids no chão.
