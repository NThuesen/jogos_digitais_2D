# Neko Maid Inc.

Um café de maids-gato administrado como uma empresa. Você não atende ninguém — você escala
quem atende, e depois assiste.

**Jogos Digitais 2026.1 · Insper · Projeto 2D**
Unity 6000.5.9f1 · 2D URP · WebGL

---

## O jogo

Cada dia tem quatro passos: montar a escalação, abrir as portas, assistir, e ler o balanço.
Durante o dia não há nada para clicar — a única coisa que decide o resultado é quem você pôs
em cada posição.

- **Cinco posições**: recepção, salão, cozinha, palco e gerência. Cada maid tem uma posição
  natural; fora dela perde um terço da eficiência e erra quase o dobro.
- **Química**, no sentido do Ultimate Team: casa, temperamento e origem. A química do time
  multiplica todos os atributos de 0,75× a 1,25×, então uma escalação boa com cartas comuns
  bate uma escalação ruim com lendárias.
- **Duas moedas**: dinheiro paga salário e aluguel e pode falir a empresa; fichas só compram
  caixas e são pagas por qualidade de atendimento, não por faturamento.
- **Falência é fim de jogo.** Os recordes ficam, a empresa não.

## Rodando

```sh
tools/simtest.sh                 # testes da simulação fora do editor (.NET SDK 8+)
tools/run-tests.sh EditMode      # testes EditMode dentro do Unity
tools/run-tests.sh PlayMode      # testes PlayMode, incluindo cena real
tools/synthesise-audio.sh        # regenera os 26 sons
tools/build-webgl.sh             # build WebGL, em Build/WebGL/
tools/serve.sh                   # serve a build em http://localhost:8080
tools/open-editor.sh             # abre o editor sem o Hub, checando a memória
tools/clean.sh                   # quebra o loop de crash e limpa 1,5 GB de cache
```

## Como está organizado

| Pasta | O que é |
| --- | --- |
| `Assets/_Game/Scripts/Sim` | a simulação inteira, **sem referência ao UnityEngine** |
| `Assets/_Game/Scripts/Game` | a camada Unity: arte, cena, telas, áudio, entrada |
| `Assets/_Game/Tests` | EditMode e PlayMode |
| `Assets/_Game/Resources/Audio` | os 26 `.wav` sintetizados |
| `docs/GDD.md` | o documento de design, por rubrica |
| `docs/ARCHITECTURE.md` | os limites técnicos e a organização real do jogo |
| `docs/ITCH_PAGE.md` | texto e instruções para publicar a build |
| `tools/` | os scripts acima |

## Documentação e colaboração

Este projeto tem quatro colaboradores usando Codex, Claude Code e Kiro. O trabalho de
programação e revisão é conduzido por agentes, sem aprovação humana rotineira de código.

- Uma Issue define o contrato: objetivo, aceitação, escopo e evidências esperadas.
- Uma tarefa corresponde a uma branch e um Pull Request; não fazer push na main.
- O agente implementador reporta testes realmente executados e pontos ainda não verificados.
- Um segundo agente, em sessão independente, analisa o diff real; o implementador corrige
  findings importantes ou bloqueantes e solicita nova revisão do código atualizado.
- A revisão independente documentada não equivale a um gate técnico automatizado.
  Não ativar merge autônomo antes de CI e revisão automatizada confiáveis (Fase 2).
- AGENTS.md contém as instruções compartilhadas pelas três ferramentas.
- docs/GDD.md define gameplay; docs/ARCHITECTURE.md define os limites técnicos.
  Atualize o documento correspondente no mesmo PR quando o comportamento mudar.

Templates: .github/ISSUE_TEMPLATE/task.yml e .github/PULL_REQUEST_TEMPLATE.md.

Para os testes rápidos, instale .NET SDK 8+ e execute tools/simtest.sh.
Para os testes Unity, configure UNITY_EDITOR com o editor Unity 6000.5.9f1
(no Windows, utilize Git Bash ou shell equivalente). Se faltarem ferramentas,
registre 'não executado', nunca 'aprovado'.

`Neko.Sim` não referenciar o UnityEngine é a decisão que sustenta o resto: um dia inteiro de
café roda em milissegundos num teste comum, o que é o único jeito honesto de afirmar que uma
escalação é melhor que outra.

**Uma cena, um objeto.** `Assets/_Game/Scenes/Boot.unity` contém um `GameRoot` vazio.
Câmera, salão, interface e áudio são construídos em código. Não há prefab nenhum — e num
trabalho em grupo isso significa que ninguém resolve conflito de merge em YAML com GUID.

## Créditos de assets

**Não há nenhum.**

Nenhuma imagem e nenhum som foram importados neste projeto. Não existe um único arquivo de
textura em `Assets/`.

- **Toda a arte** é gerada em tempo de execução a partir de dezessete campos de distância em
  `Assets/_Game/Scripts/Game/Art/ShapeRasteriser.cs`. Uma maid é uma pilha de camadas
  tingidas; o letreiro do menu é um conjunto de retângulos arredondados e dois triângulos.
- **Todo o áudio** é sintetizado por `tools/synthesise-audio.sh`, que descreve cada som como
  uma pilha de vozes — forma de onda, varredura de altura, envelope, filtro, vibrato — e
  escreve o `.wav`. O mesmo comando sempre produz os mesmos bytes.
- **A fonte** é a embutida do próprio Unity (`LegacyRuntime.ttf`), resolvida em tempo de
  execução. TextMeshPro não é usado: foi verificado que este editor não traz os recursos de
  TMP, e um texto de TMP sem font asset se diagrama corretamente e não desenha nada.

### Ferramentas usadas na produção

Medição e geração, não conteúdo. Nenhuma delas entra na build:

- `pixels` — fecha a paleta por L\* em vez de no olho, e lê as capturas por número
- `audio` — sintetiza e mede o conjunto de som contra a própria mediana
- `unity` — roda os testes e as capturas em modo batch
- `balance` — confere curvas

Todas fazem parte de `agent_creater`, ferramental próprio do estúdio.

### Referências de design

O laço é declaradamente derivado de dois lugares: a camada de gerenciamento de café do
**Café Mania / Diner Dash**, e o **Ultimate Team** do FIFA para escalação, química,
raridade e pacotes. Nenhum código, arte ou áudio veio de nenhum dos dois.

## Verificação

Antes de qualquer afirmação de que algo funciona:

```sh
tools/simtest.sh && tools/run-tests.sh EditMode && tools/run-tests.sh PlayMode
```

Capturas do jogo rodando ficam em `../visual_assets/frames/`, junto com o registro dos erros
que cada rodada de capturas encontrou — vários deles invisíveis para qualquer teste de
lógica.
