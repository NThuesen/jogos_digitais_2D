# Mina v01 — orientação e caminhada (V0 × V1)

Estado: **experimental, não aprovado; rascunho sem auto-merge.** Base: PR #15 (`747e677`), ainda aberto. PR #16 intocado; nenhum prompt, catálogo ou arte alterados; nenhuma imagem nova gerada. **A decisão de que a caminhada convence é do responsável, pela comparação visual — os asserts abaixo não a substituem.**

## O que mudou nesta rodada

Correções da revisão do PR #18 (só os findings comprovados):

- **Parada:** a Mina não fica mais congelada numa passada lateral ou traseira. Durante o movimento a orientação segue o deslocamento. Ao parar, termina a retenção de 120 ms e volta ao idle frontal. A troca de sprite acontece só quando o desenho muda, nunca a cada frame de uma caminhada estável.
- **Cook/Tired:** mantêm a identidade. Andando, usam a caminhada da orientação; parados, o idle. Regras de jogo e barra de humor inalteradas.
- **Carry:** o desenho único de carry (3/4 para a direita) é espelhado quando ela segue para a esquerda, e nunca anda de ré. Para o fundo usa o desenho de costas (o corpo esconde a bandeja). Para a câmera usa o carry com o último lado horizontal. O item externo continua suprimido: sem bandeja duplicada.
- **Revisão técnica (Claude Code, contexto separado):**
  - **Corrigido:** carregando em diagonal, a Mina mantinha o lado do último trecho lateral e cruzava a tela de costas para a direção. O lado agora acompanha qualquer trecho com componente horizontal clara. Há um teste novo, o probe `carry_diag_left`, e a checagem de moonwalk do harness agora cobre o carry em diagonais.
  - **Ajustado:** a checagem de oscilação de direção usava 0,3 s, mais estrito que a retenção projetada de 0,15 s, e poderia reprovar um trecho curto legítimo.
- **Lógica pura testável:** `MinaFacing` (orientação, histerese 1,25/0,8, retenções de 150/120 ms) e `MinaVisual.NameFor` (qual desenho em cada estado), cobertos por `MinaFacingTests` (EditMode).

Duas variantes de movimento, selecionadas por `NEKO_MINA_VARIANT`:

| | V0 — baseline corrigida | V1 — movimento procedural mínimo |
|---|---|---|
| Fase do passo | tempo (`|sin(11·t)|`) | distância percorrida: um passo a cada 0,45 tile |
| Oscilação vertical | até 0,07 u (≈7 px a 720p) | até 0,045 u (≈5 px) |
| Inclinação | nenhuma | balanço ±1,5° alternando por passo; inclinação lateral de 2° no sentido do movimento |
| Squash | nenhum | 2,5% no contato do pé |
| Início/parada | liga/desliga em um frame | envelope de 120 ms |

Tudo gira em torno do pivô nos pés: escala e base do sprite não mudam. Não há infraestrutura genérica de animação nem mudança na simulação.

## Harness (`MinaCapture`, Editor, batch, save isolado)

- **Relógio:** `Time.captureDeltaTime` **não** foi adotado. Medido em batch (6000.5.9f1), ele deixa `Time.unscaledDeltaTime` no valor real (0,0167 s), e `Clocks` lê exatamente esse valor. Gravar com ele mostraria o café saltando entre frames. Foi adicionado `Clocks.FixedFrame`, só no Editor, que fixa ambos os relógios em 1/25 s durante a captura. O harness falha se `Clocks.Game` e `Clocks.Ui` não seguirem esse valor.
- **Determinismo:** perfil de seed fixa (`NEKO_MINA_SEED`, padrão 1) via `GameRoot.StartIsolatedSession`, só no Editor. O hook recusa rodar sem save isolado e nunca grava um. V0 e V1 jogam o mesmo dia frame a frame, e `make_media.py` aborta se as posições divergirem.
- **Segmentos naturais** (sem mover a Mina à mão): o dia roda a 4× até a condição, depois a 1× gravando **todos os frames consecutivos**.
  - `natural_walk`: começa quando ela recebe uma tarefa. É **obrigatório** percorrer ≥ 2 tiles reais e depois ficar parada 1 s, senão a execução falha com código 1.
  - `natural_carry`: começa quando ela pega um pedido.
  - `natural_vertical`: começa quando ela anda para o fundo ou para a câmera.
- **Checagens por frame** (qualquer violação encerra com código 1):
  - parada além da retenção só com idle ou carry;
  - sem moonwalk depois de uma virada assentada (lateral nos trechos horizontais, carry também nos diagonais);
  - orientação sem voltar à anterior dentro da retenção de direção;
  - pés entre 0 e 0,08 u acima da base;
  - escala entre 97% e 103%.
- **CSV por frame:** posição, tarefa, pose, orientação, sprite, flip, lift, tilt, escala e distância.
- **Probes controlados** em seguida, com o dia suspenso, e guardados à parte. Cobrem: direita, esquerda, fundo, câmera, diagonais, parada, inversão rápida, carry nas quatro direções, em diagonal e parado, cook e tired andando e parados.
- **Falhas:** o motivo é gravado em `failure.txt` antes de sair. Antes, sair dentro do callback de log apagava a mensagem do arquivo de log.

## Evidências reais (Unity 6000.5.9f1, RenderTexture da câmera do jogo)

Quatro execuções: V0 e V1 × 1280×720 e 960×600, todas com código de saída 0. Linhas `MINA_` em [`evidence.txt`](evidence.txt); CSVs em [`evidence/`](evidence/).

| Segmento | Distância real | Duração gravada | Mudanças de orientação |
|---|---|---|---|
| `natural_walk` | 2,69 tiles | 13,1 s (328 frames) | 2 |
| `natural_carry` | 4,26 tiles | 11,2 s (281 frames) | 4 |
| `natural_vertical` | 3,27 tiles | 5,9 s (148 frames) | 2 |

Comparações lado a lado, rotuladas **V0 | V1**. São frames consecutivos, cada um com exatamente 40 ms de jogo (GIF a 25 fps, velocidade real), recortados em pixels nativos ao redor do trajeto. Os GIFs mostram ela saindo, chegando e parando:

| Segmento | 1280×720 | 960×600 | 1280×720 ampliado 2× |
|---|---|---|---|
| Caminhada (lado, ida e volta) | [GIF](captures/compare_natural_walk_1280x720.gif) | [GIF](captures/compare_natural_walk_960x600.gif) | [GIF](captures/compare_natural_walk_1280x720_zoom2x.gif) |
| Entrega com carry (última entrega completa) | [GIF](captures/compare_natural_carry_1280x720.gif) | [GIF](captures/compare_natural_carry_960x600.gif) | [GIF](captures/compare_natural_carry_1280x720_zoom2x.gif) |
| Fundo → mesa → estação → carry para a câmera | [GIF](captures/compare_natural_vertical_1280x720.gif) | [GIF](captures/compare_natural_vertical_960x600.gif) | — |

Probes controlados (não são movimento natural): [V0 1280×720](captures/probes_V0_1280x720.png), [V1 1280×720](captures/probes_V1_1280x720.png), [V0 960×600](captures/probes_V0_960x600.png), [V1 960×600](captures/probes_V1_960x600.png).

Medidas tiradas dos CSVs a 1280×720, com a mesma simulação nas duas variantes:

| | V0 | V1 |
|---|---|---|
| Velocidade da Mina | 1,75–2,24 tiles/s | igual |
| Picos de oscilação por segundo de movimento | 4,2–4,9, fixos no tempo | 5,3–6,1, acompanham a distância |
| Lift máximo | 0,070 u | 0,045 u |
| Inclinação | 0° | −3,5° a +3,5° |
| Queda do lift ao parar (maior salto em um frame) | 0,065 u ≈ **6,9 px** | 0,015 u ≈ **1,6 px** |
| Lift no frame da troca para idle | 0 | 0 |

## Testes

- EditMode: **falhou, 311/313**. Os 13 testes novos de `MinaFacingTests` passam. As duas falhas são `DocsTests.NoTextureOrAudioFileWasEverImported` e `DocsTests.TheReadmeStatesThatNoAssetWasImported`, os mesmos contratos documentais já incompatíveis desde o PR #15; não foram alterados.
- PlayMode: **passou, 20/20**.
- Neko.Sim standalone e WebGL: **não executados**; a simulação não foi alterada.
- **Save habitual:** todas as execuções foram em batch (`SaveBridge.Isolated`), que nunca lê nem grava PlayerPrefs. O valor `nekomaidinc.profile` do Editor tinha o mesmo hash (SHA-256 `1A7A3AE90734CC56…`, 536 bytes) antes e depois de todas as execuções da revalidação final.

## Limitações conhecidas

- A perna continua congelada em ambas as variantes: há uma pose estática por orientação. A V1 só muda como o corpo se move sobre ela.
- O carry para a câmera usa o desenho 3/4. Ele vira conforme o lado do trecho sempre que o deslocamento horizontal passa de 35% do vertical, e numa descida quase reta mantém o último lado. Para o fundo, a bandeja some atrás do corpo.
- Cook e Tired não têm ação própria.
- Na seed 1, as viagens naturais da Mina são curtas (cerca de 0,5 a 1,7 tile cada). Nenhum trajeto longo foi forçado.
- A Mina e outros personagens ainda se sobrepõem perto das estações.
- A associação é feita pelo nome, só em save isolado; equipamentos da Mina não são representados.

## Recomendação

Avaliar os GIFs **V0 | V1** a 1×. Se a V1 não convencer, ou se a melhora não justificar a complexidade, o próximo experimento é a **V2**: um segundo quadro lateral (passada oposta) alternado pela mesma fase da V1. Ele exige gerar **uma** imagem nova para a Mina, com autorização do responsável. **Não foi implementado.** Não expandir para outras identidades antes dessa decisão.

## Reprodução

1. Arte processada: `python process.py <originais> <Assets/_Game/Resources/Mina>` (inalterado).
2. Nenhum outro Editor aberto. Usar o mesmo lock de `tools/unity-guard.sh`.
3. Para cada execução, definir `NEKO_ISOLATED_SAVE=1`, `NEKO_MINA_CAPTURE_DIR=<pasta absoluta>`, `NEKO_MINA_VARIANT=V0|V1`, `NEKO_MINA_CAPTURE_SIZE=1280x720|960x600` e, opcionalmente, `NEKO_MINA_SEED`. Rodar `Unity -batchmode -projectPath <projeto> -executeMethod Neko.Game.Core.MinaCapture.Begin -logFile <log>`, sem `-quit`. Nesta sessão as capturas foram lançadas com `Start-Process` do PowerShell; o motivo de qualquer falha fica em `failure.txt` na pasta da captura.
4. Montar as mídias: `python make_media.py <pasta com V0_1280x720, V1_1280x720, V0_960x600, V1_960x600> captures`.
5. Testes: o runner protegido (`tools/run-tests.sh EditMode|PlayMode`), em batch. Nesta sessão foi usada uma cópia LF do guard e um shim de `python3`, sem alterar os scripts versionados.
