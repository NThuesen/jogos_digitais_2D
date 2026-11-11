# Neko Maid Inc.

Um café de maids-gato administrado como uma empresa. Você escala a equipe,
acompanha o atendimento e pode intervir em oportunidades pontuais durante o expediente.

**Jogos Digitais 2026.1 · Insper · Projeto 2D**
Unity 6000.5.9f1 · 2D URP · WebGL

---

## O jogo

**Kit de produção de arte modular:** [prompts detalhados, tutorial, referências e pastas](docs/art/ui-modular/README.md). Imagens de UI geradas com GPT são peças; interações e textos dinâmicos ficam na Unity. Há integrações de interface em PRs independentes; confirmar o estado da `main` antes de declarar uma tela concluída.

**Direção artística:** **anime 2D ilustrado/chibi**, com apresentação cozy cat-café e migração progressiva de cenas e menus. [Direção visual aprovada](docs/art/CURRENT_VISUAL_DIRECTION.md) · [Roadmap](docs/ROADMAP.md).

Cada dia tem quatro passos: montar a escalação, abrir as portas, assistir, e ler o balanço.
Durante o dia, a equipe atende automaticamente, mas o jogador pode ajudar a acomodar
clientes, registrar pedidos, limpar mesas, controlar o ponto do fogão e acalmar pessoas
impacientes. O desempenho também depende da escalação e dos recursos do café.

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
| `docs/GDD.md` | referência técnica de regras e rubricas, mantida pelos testes |
| [`docs/DDE.md`](docs/DDE.md) | documento acadêmico DDE: Design, Dynamics e Experience |
| `docs/ARCHITECTURE.md` | os limites técnicos e a organização real do jogo |
| `docs/ITCH_PAGE.md` | texto e instruções para publicar a build |
| `docs/art/prompt-pipeline/README.md` | prompts completos de 9 maids, tutoriais de anexos e pipeline ChatGPT → Tencent (candidatos) |
| [`art_working/`](art_working/README.md) | destino dos frames PNG exportados pelo Blender, separado por maid e animação; arquivos brutos ignorados pelo Git |
| `tools/` | os scripts acima |

## Trabalho com agentes

`AGENTS.md` é o **ponto de entrada curto** para Codex e demais agentes: branch
automática por tarefa, PR e regras obrigatórias. As instruções detalhadas são
consultadas sob demanda:

- [Workflow de branches e PRs](docs/agent/WORKFLOW.md)
- [Unity, arte e integração](docs/agent/UNITY.md)
- [Testes e evidências](docs/agent/VALIDATION.md)
- [Roadmap visual ativo](docs/ROADMAP.md) — ler apenas a tarefa atribuída, não todas as notas

A apresentação acadêmica está no [DDE](docs/DDE.md). Mudanças nas regras do jogo
consultam [GDD](docs/GDD.md); mudanças arquiteturais
consultam [ARCHITECTURE](docs/ARCHITECTURE.md). Não é necessário ler ambos para
uma pequena alteração visual. GitHub Actions valida a simulação e os controles
automatizados; Unity/PlayMode/WebGL exigem execução real para serem considerados
validados.

`Neko.Sim` não referenciar o UnityEngine é a decisão que sustenta o resto: um dia inteiro de
café roda em milissegundos num teste comum, o que é o único jeito honesto de afirmar que uma
escalação é melhor que outra.

**Uma cena, um objeto.** `Assets/_Game/Scenes/Boot.unity` contém um `GameRoot` vazio.
Câmera, salão, interface e áudio são construídos em código. Não há prefab nenhum — e num
trabalho em grupo isso significa que ninguém resolve conflito de merge em YAML com GUID.

## Arte, áudio e créditos (Créditos de assets)

**Nove walk sheets ilustrados** são imagens originais importadas em `Assets/_Game/Resources/Maids/`, produzidas de imagens/vídeos autorais do projeto por meio do pipeline offline `tools/art/maid_walk/maid_walk.py`. As fontes brutas exportadas pelo Blender ficam fora do Git. O sprite sheet original da Suzu (`Assets/_Game/Resources/Suzu/suzu_walk_sheet.png`) permanece apenas como backup histórico. [Proveniência e hashes](docs/art/SUZU_ASSET_PROVENANCE.md) · [estado da animação e WebGL](docs/art/maid-walk/README.md).

A arte remanescente **ainda contém** elementos procedurais e móveis desenhados em código (`ShapeRasteriser`, `FurnitureArt`). O cenário atual será redesenhado para **anime 2D ilustrado/cozy**; não é referência obrigatória de pixel art. Portraits exclusivos dos Maid Cards, poses ilustradas além do Walk, equipamentos ilustrados e `visual_id` persistente **não foram entregues**.

Os 26 sons são sintetizados por `tools/synthesise-audio.sh`. A fonte de UI é a embutida no Unity (`LegacyRuntime.ttf`); TMP não é usado. Nenhum asset de terceiros foi incorporado, segundo a proveniência documentada. Não confundir assets originais importados com assets de terceiros.

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
