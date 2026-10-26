# Arquitetura — Neko Maid Inc.

Este documento descreve a **implementação atual** do Neko Maid Inc., não o template Unity de origem. As regras do jogo, os números de balanceamento e a experiência desejada pertencem a [`GDD.md`](GDD.md). Este arquivo documenta os limites técnicos que permitem implementar e verificar essas regras.

## Plataforma e estrutura

- **Engine:** Unity 6000.5.9f1; projeto 2D com URP.
- **Entrega:** WebGL para navegador desktop, com interface em português e entrada por mouse.
- **Cena:** `Assets/_Game/Scenes/Boot.unity`, com `GameRoot`. Câmera, salão, áudio, efeitos e telas são construídos em código durante o boot.
- **Assets atuais:** sprites gerados proceduralmente em tempo de execução e áudio sintetizado pelo próprio projeto. Não há prefabs nem texturas de terceiros no fluxo descrito pelo README.

| Área | Responsabilidade |
| --- | --- |
| `Assets/_Game/Scripts/Sim` (`Neko.Sim`) | Regras de domínio: maids, escalação, química, café, atendimento, economia, progressão e formato do save. C# independente do Unity. |
| `Assets/_Game/Scripts/Game` (`Neko.Game`) | Integração Unity, ciclo de vida, cena, visualização, interfaces, áudio, entrada e ponte com a simulação. |
| `Assets/_Game/Tests` | Testes EditMode e PlayMode, incluindo verificações de documentação e da cena. |
| `Assets/_Game/Resources/Audio` | Arquivos de áudio gerados pelo script de síntese. |
| `tools/` | Execução dos testes, construção WebGL, áudio e rotinas locais de desenvolvimento. |

## Fluxo da aplicação

`GameRoot` inicializa os sistemas Unity e a sessão. `AppStateMachine` controla as transições entre **Boot → Menu → Café → Turno → Resultado → Café**, incluindo as transições permitidas de retorno.

`GameSession` reúne o perfil da empresa, a criação do turno e o balanço. O jogador altera escalação, elenco, loja e decoração **entre os dias**. Durante o turno, o atendimento é autônomo; os controles de velocidade e de pular mudam a apresentação/tempo de espera, não as decisões da simulação.

A camada visual deve **observar e representar** o estado da simulação, sem reproduzir regras de negócio em componentes de UI. Mudanças persistentes na empresa passam pela sessão e devem acionar o salvamento.

## Limites que devem continuar válidos

1. **`Neko.Sim` não referencia `UnityEngine`.** Não colocar ali `MonoBehaviour`, cena, sprites, UI, `PlayerPrefs` ou aleatoriedade do Unity.
2. **Simulação verificável:** regras e sorteios passam pelos componentes determinísticos da simulação (incluindo `Rng`). Comparações de balanceamento devem usar sementes controladas.
3. **Regras no domínio:** cálculo de química, salário, satisfação, resultados e progressão pertencem à simulação, não às telas.
4. **Salvamento com fronteira explícita:** `SaveCodec` define o formato, a versão e o checksum; `SaveBridge` integra a persistência via `PlayerPrefs` do Unity/WebGL. `GameSession.Touch()` registra alterações persistentes.
5. **Estados inválidos não viram travamentos silenciosos:** a máquina de estados recusa transições ilegais; a barreira de erros e as validações de pré-condição protegem a execução do dia.
6. **Unity é parte da entrega:** testes de lógica não substituem validação de cena, entradas, áudio, interface, renderização e build WebGL.

Esses limites documentam o projeto existente. **Não são autorização** para adicionar infraestrutura genérica, novas assemblies, gerenciadores ou frameworks sem necessidade demonstrada.

## Trabalho em quatro contas GitHub

- Uma Issue por tarefa, uma branch por tarefa e integração via Pull Request.
- O agente implementador descreve testes executados e evidências reais.
- Um agente operado sob **outra conta colaboradora** revisa o diff do HEAD atual,
  priorizando bugs, regressões, lifecycle do Unity e adulteração/afrouxamento dos testes.
- Se houver findings blocking/important, o implementador corrige e solicita revisão
  da versão atualizada. Quando estiver conforme, o agente revisor envia um review GitHub
  **APPROVE** pela própria conta, não pela conta autora.
- A revisão não exige pessoa analisando código, mas o GitHub deve exigir **1 aprovação
  de outra conta** e invalidar aprovações antigas após novos commits. O modelo que
  produziu o review não é verificável pelo GitHub; não falsificar esse trabalho.
- Os checks de GitHub Actions são acionados por evento, sem polling, e verificam
  apenas simulação/boundaries. Não substituem testes Unity, WebGL ou avaliação visual.
- Com **Allow auto-merge** ativado no repositório e o ruleset da `main` validado,
  o agente autor pode solicitar auto-merge uma vez; o GitHub esperará pela aprovação
  independente e pelo check `simulation-and-boundaries` antes de integrar.
- Alterações no GDD, na arquitetura ou em configurações Unity devem ser verificadas
  em conjunto com os arquivos correspondentes, sem scope creep.

Configuração GitHub necessária: regras de `main` somente (PR, 1 aprovação,
dismiss stale approvals, check obrigatório, bloqueio de force-push/deleções),
sem bypass; habilitar auto-merge em Settings → General → Pull Requests.

## Verificação

Com .NET SDK 8+ para os testes da simulação e Unity 6000.5.9f1 configurado em UNITY_EDITOR para os testes do Editor, o fluxo esperado para mudanças relevantes é:

```sh
tools/simtest.sh
tools/run-tests.sh EditMode
tools/run-tests.sh PlayMode
```

Para alterações que afetem a entrega ou a experiência no navegador, executar também `tools/build-webgl.sh`, abrir a build e verificar o fluxo real. Relatar explicitamente o que foi **executado**, **não executado** e **falhou**; não inferir sucesso a partir da existência de scripts ou testes.

## Documentos vigentes

| Documento | Uso |
| --- | --- |
| [`README.md`](../README.md) | Visão geral, comandos e porta de entrada para a equipe. |
| [`GDD.md`](GDD.md) | Fonte de verdade do produto e das mecânicas atuais. |
| `ARCHITECTURE.md` | Arquitetura implementada, fronteiras técnicas e prática inicial de integração. |
| [`ITCH_PAGE.md`](ITCH_PAGE.md) | Texto de publicação; deve acompanhar as mudanças do produto. |

Em caso de divergência entre documentação e comportamento observado, registrar o conflito e corrigi-lo no PR apropriado — **não inventar uma regra de precedência para mascarar a diferença**.
