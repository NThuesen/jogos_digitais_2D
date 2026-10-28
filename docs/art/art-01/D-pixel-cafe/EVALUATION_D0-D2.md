# ART-01D — Avaliação técnica dos Prompts 0, 1 e 2 (Suzu)

**Status:** avaliação objetiva de **arte gerada** (ChatGPT Plus). **Não é aprovação técnica nem autorização para ART-02** (bloqueada até a validação em contexto, abaixo). Nada aqui é Unity; custo adicional **R$ 0**. Parâmetros de recorte/escala são **testes**, não decisões.

**Decisões do responsável (2026-10-28):**
- **Identidade da Suzu:** **provisoriamente aprovada como referência artística**. Os **PNGs gerados e recortados NÃO estão aprovados como assets finais**.
- **Chibis:** não serão redesenhados à mão em pixel art e **40–48 px não está fixado**. Antes, avaliar **64, 96 e 128 px em contexto visual real**.
- **Próximo:** Prompt 3 (café); depois, **composição integrada com os chibis** para avaliar escala, legibilidade e coerência. **ART-02 permanece bloqueada** até essa validação. Sem alterações em Unity, gameplay ou UI.

**Originais:** `incoming/D0…D2` (8,6 MB) **não estão versionados neste PR**; ficam locais. Links para `incoming/` abaixo só funcionam no computador do responsável.

| Rótulo | Significa |
| --- | --- |
| **GERADA** | arquivo original em [`incoming/`](incoming/), byte a byte igual ao baixado (hash conferido), **não alterado** |
| **PROCESSADA** | recorte em [`processed/`](processed/) feito por mim a partir da GERADA |
| **MOCKUP** | composição de avaliação em [`review/`](review/) (não é captura de jogo) |

## Arquivos

| Arquivo | Prompt | Conteúdo real |
| --- | --- | --- |
| `incoming/D0_suzu_sheet.png` (1536×1024) | 0 | prancha com busto, 2 corpos inteiros, 3 expressões e 9 swatches |
| `incoming/D1_portrait.png` (1024×1536) | 1 | busto, **um** arquivo |
| `incoming/D2_chibi_poses.png` (1774×887) | 2 | **as 3 poses numa imagem só** (idle, walk, carry), não 3 arquivos |
| `processed/D1_portrait_cutout.png` | — | retrato recortado |
| `processed/D2_chibi_{idle,walk,carry}_cutout.png` | — | cada pose isolada (~540×790) |
| `review/01…04_*.png` | — | painéis de comparação (links abaixo) |

## 1. Observações sobre a geração (fatos medidos)

- **Transparência: nenhuma.** Os PNGs vêm em RGBA, mas o alfa é **100 % opaco** (0 pixels transparentes nos três). O ChatGPT ignorou tanto a Variante T (transparente) quanto a K (magenta) e entregou **fundo bege liso** (`#D8C0A9` / `#D1B49A` / `#D5BBA2`, desvio < 1). Esses são os dados do experimento de transparência: **alfa real não funcionou**; o fundo liso permitiu recorte por cor.
- **Não é pixel art verdadeira.** ~230 000 cores únicas por imagem (pixel art real teria dezenas) e **nenhuma periodicidade de grade** (pico/média ≈ 1,05–1,1). Há degraus de contorno de ~6–7 px e *dithering* de ~2 px **misturados** no mesmo desenho: aparência de pixel art, sobre uma imagem de alta resolução. Isso confirma que **reduzir não "descobre" um pixel grid que não existe**.
- **Cor pedida ≠ cor entregue.** Menta pedida `#8FD8C0` saiu `#A4CAAC` (mais acinzentada); imagem inteira com tom quente. Os swatches da sheet D0 **são** a cor real usada (medido), mas diferem do hex do prompt.
- **Poses de chibi viraram frente/¾ frontal**, não "virado para a direita" como pedido; walk e carry são poses estáticas, **não** ciclo de animação.

## 2. Isolamento (recorte)

Método: fundo por *flood-fill* a partir das bordas (tolerância 32 em soma RGB), buracos internos só se muito próximos do bege e ≥ 120 px, remoção de 1 px de contorno ainda tingido de bege. Alfa **binário** (borda dura). Scripts temporários **fora do repositório**; nada de infraestrutura nova.

| Item | Resultado |
| --- | --- |
| Retrato | 1 componente, sem fragmentos soltos; ~1,1 % do contorno ainda com tom bege (medido) |
| Chibi idle / walk | 1 componente cada; 2,0 % / 2,3 % de contorno com tom bege |
| Chibi carry | contorno 2,5 % tingido; **2 fragmentos soltos** (brilho em losango de 500 px e 1 grão de 11 px) que são FX gerados junto da pose |
| Orelhas, cauda, sininhos, mechas finas | **sem cortes visíveis**, inspecionados a 2–3× sobre magenta e fundo escuro |
| Buracos entre mechas / entre cabelo e braço | recortados corretamente (verificado visualmente) |

**Defeito real meu, corrigido:** a 1ª versão do recorte do retrato vazou para dentro do avental, porque a sombra rosada dele tem cor quase igual ao fundo e encosta na borda inferior da tela. Corrigi não semeando o preenchimento pela borda de baixo e tratando a borda da tela como não-contorno; revisado depois. **Risco que permanece:** este recorte só é seguro porque o fundo é **liso e a figura encosta pouco nele**; com o fundo menos uniforme, ou sombras de pele/creme mais próximas do bege, o método falharia. O recorte **não substitui** transparência real.

**Limites do recorte:** o retrato está **cortado pela tela** embaixo (saia/avental) e levemente nas laterais; não existe retrato "completo". Cabelo, rosto, roupa e acessórios estão **fundidos em uma imagem** (nenhuma camada separada).

Painel: [`review/01_recorte_original_vs_processado.png`](review/01_recorte_original_vs_processado.png).

## 3. Escala do chibi (teste, **não aprovada**; foco agora: 64 / 96 / 128 px em contexto)

Cabeça ≈ metade da altura do chibi. Resultados de [`review/02_chibi_escala.png`](review/02_chibi_escala.png) (altura = ponta da orelha à sola, alfa binário):

| Altura | Leitura |
| --- | --- |
| 40 px | silhueta, cor do cabelo, orelhas e tom do vestido legíveis; **olhos viram 2–3 px**, sininhos/laços viram manchas |
| 48 px | igual, um pouco melhor; ainda **sem rosto expressivo** |
| 48 px · `nearest` | **ruim**: pontos soltos escuros e falhas (ruído de aliasing) |
| 48 px · área / lanczos | legíveis, mas **borrados**; sem pixel art nítida |
| 48 px + 24 cores | **sem melhora visível** sobre área; quantizar não resolveu |
| 64 px | rosto volta a ser legível; ainda perde acessórios finos |
| 96–128 px | boa leitura de rosto, sininhos, laços e bandeja |

Conclusão honesta: **a arte gerada não sobrevive a 40–48 px sem redesenho manual de pixel art**; isso seria "arte nova", não redução. Para esta qualidade de detalhe, **≥ 64–96 px** é onde o chibi mantém o charme. Não aprovo nem rejeito o tamanho: é decisão do responsável com o resultado visual diante dele (e depende da escala real do jogo).

## 4. Identidade retrato ↔ chibi (inconsistências reais)

Painéis: [`review/03_identidade_retrato_chibi.png`](review/03_identidade_retrato_chibi.png), [`review/04_orelhas_entalhe.png`](review/04_orelhas_entalhe.png).

**Consistente:** cabelo menta com franja reta e mechas laterais; olhos âmbar com brilho; orelhas com interior rosado; laços e sininhos nas laterais e nas pontas dos rabichos; gargantilha com sininho; laço bordô grande; vestido chocolate, avental creme com babados, mangas bufantes; cauda creme com ponta menta, laço e sininho; tiara de babados. Paleta medida (distância RGB do swatch declarado em D0 até a cor mais próxima, retrato / chibi): menta `#A4CAAC` 6 / 8, sombra menta 19 / 17, creme 8 / 2, pêssego 7 / 13, salmão 28 / 15, bordô 25 / 25, marrom 29 / 27, e **realce menta `#D7EAC7` 38 / 38** e **âmbar `#E49C50` 39 / 44**. Ou seja: menta, creme e pêssego batem bem; âmbar e realce menta **não aparecem fielmente** em nenhum dos dois (provavelmente porque a paleta medida tem 16 cores e agrupa esses tons), e a ordem de grandeza é igual no retrato e no chibi, sem diferença marcante entre eles.

**Inconsistências / defeitos:**
1. **Entalhe na orelha esquerda (marca de identidade pedida):** claro no retrato e na sheet; **ausente/ilegível nos três chibis**. A marca não está sendo carregada.
2. **Espessura do contorno:** retrato com linha fina avermelhada; chibi com contorno **escuro e grosso**. Parecem estilos diferentes.
3. **Chibi mais "limpo":** botões dourados do corpete, pulseiras e detalhes de babado do retrato/sheet aparecem simplificados ou diferentes (pulseiras vermelhas no chibi idle).
4. **Meias:** a sheet mostra meias brancas até a coxa com babado; no chibi, pernas claras sem babado visível.
5. **Cor do laço da cauda:** vermelho no retrato; **mais escuro/quase marrom** no chibi.
6. **Pose:** nenhuma pose está de lado/direita; sem costas; walk e carry são **uma pose estática cada**.
7. **Fragmentos de FX** (pose carry) e pequenos respingos/vapor perto do quadril da pose idle: elementos gerados que **não pertencem ao sprite base**.

## 5. Modularização (o que dá e o que não dá)

- **Dá para reutilizar:** retrato recortado, cada pose de chibi recortada, bandeja/bolo/xícara da pose carry **só como imagem** (não separados), cauda e sininhos como referência de design.
- **Exige geração separada:** camadas de cabelo/acessório (nada está em camadas), chibi de costas/lado/andando em ciclo, expressões do chibi, variações de roupa, assets com alfa real.
- Trocar uma peça (ex.: laço) hoje exige **regenerar a personagem inteira** e rever a identidade.

## 6. Próximo passo e pendências

- **Prompt 3 (café)** é a próxima geração: um arquivo, chat dedicado, para confirmar que cenário e personagens compartilham a direção. **O estilo do chibi (contorno grosso) vs. o retrato** deve ser julgado ao lado do café.
- Pendências para o responsável: aprovar (ou não) a identidade da Suzu; se o contorno grosso do chibi é desejado; se o tamanho em jogo pode ser ≥ 64–96 px.
- **Não** implementado: Unity, biblioteca, ART-02.
