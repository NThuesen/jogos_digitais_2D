# Intervenção excepcional A1/A2 — resultado

**Gate A encerrado sem aprovação. Não avançar ao Gate B.**

Foi executada uma única intervenção artística sobre o HEAD verificado `228f534` do
PR draft #21. O fechamento do sapato distante melhorou e a integridade foi preservada,
mas a reconstrução de A1 ainda não mantém o acabamento da referência nas poses
exigentes. Não foi feita outra rodada de ajustes depois da inspeção do resultado.

O rig C continua o mesmo. Não há conclusão de inviabilidade de SpriteRenderers e
Transforms; o limite observado é a qualidade da reconstrução raster localizada.

## Comparações e inspeção

- [A1 antes/depois, detalhe](captures/detail_before_after_opposite_arms.png): antes à esquerda,
  intervenção à direita. O preenchimento deixa de seguir o recorte da mão, mas o painel
  e o babado reconstruídos ainda destoam do material/detalhamento adjacentes.
- [A2 antes/depois, detalhe](captures/detail_before_after_far_leg_forward.png): o sapato
  distante ganha calcanhar, lateral e sola, mantendo o tornozelo no mesmo pivô.
- Frames completos antes/acima e depois/abaixo:
  [braços](captures/before_after_opposite_arms.png),
  [perna distante](captures/before_after_far_leg_forward.png).
- [Braço ocultado — 100, 200 e 440 px](captures/arm_hidden_1280x720.png).
- [Sapato distante com a perna próxima ocultada](captures/far_shoe_only_1280x720.png)
  e [somente o pé próximo ocultado](captures/near_foot_hidden_1280x720.png).
- [Articulação simultânea extrema](captures/combined_extreme_960x600.png).
- Faixa do braço em [100 px](captures/arm_range_100.png), [200 px](captures/arm_range_200.png)
  e [440 px](captures/arm_range_440.png). Ordem por linha, da esquerda para a direita:
  −35°, −25°, −15°, −5°, 0°, +5°, +15°, +25°, +35°.
- [Casos adicionais em 960×600](captures/exception_cases_960x600.png).

Há 40 frames completos reais do Unity: sete poses originais e treze casos adicionais
em cada resolução. Os casos adicionais mostram a personagem a 100/200/440 px no mesmo
frame. A sequência de ângulos é inspeção estática, não uma animação implementada.
As folhas e recortes foram feitos por Pixels MCP a partir desses frames.

Inspeção nativa das peças, sem ampliação ou redução de produção:
[skirt.png](../../../../../Assets/_Game/Resources/Anim01GateA/skirt.png),
[far_shoe.png](../../../../../Assets/_Game/Resources/Anim01GateA/far_shoe.png),
[near_shoe.png](../../../../../Assets/_Game/Resources/Anim01GateA/near_shoe.png).
Todos os recortes usam coordenadas do original 1254×1254.

## Método localizado

Os dois `FABRIC_PATCHES` anteriores foram removidos do gerador. Não houve novo ajuste
de tamanho/posição de seus retângulos reescalados.

**A1:** construção de um painel contínuo com curvas, campo de sombra dirigido às
dobras, faixa dourada curva e seis segmentos de babado pintados. As bordas são
rasterizadas a 4× e amostradas uma vez para antialiasing. Tecido efetivamente visível
em `lado.png` foi clonado em resolução nativa, com translação `(6,-6)`, sem escala,
dentro de uma máscara explícita que exclui o outro braço. Os pixels de tecido
preservados são sobrepostos à reconstrução. Duas máscaras removem resíduos na junção
saia/coxa. O resultado não atingiu o acabamento necessário: pintura e clonagem
controladas por estes parâmetros não reproduziram adequadamente o volume e o babado.

**A2:** o couro do sapato próximo, já limpo, fornece a geometria de uma camada oculta,
com mapa de perspectiva `(0,86; 0,80)` ancorado nos tornozelos existentes. A região
acima de Y=1090 não é clonada, evitando um segundo laço. Um contraforte traseiro e um
realce curto são pintados, e a antiga borda de oclusão é removida antes de sobrepor a
frente original do sapato distante. No próximo, duas máscaras eliminam a lasca diagonal
e o resíduo vertical; nenhum outro couro é repintado.

Receita executável: [reconstruct.py](reconstruct.py), chamada por `../build_kit.py`.
Parâmetros: [reconstruction.json](reconstruction.json). Referência anterior de apenas
três peças: [baseline](baseline/) com hashes e contrato original em [baseline.json](baseline.json).
Essas cópias documentais não são novos sprites de runtime ou quadros por pose.

As máscaras autorizadas são PNGs 1254×1254: [saia](masks/authorized_skirt.png),
[sapato distante](masks/authorized_far_shoe.png), [sapato próximo](masks/authorized_near_shoe.png).
Branco permite alteração; preto preserva RGBA, inclusive pixels transparentes.

## Integridade medida

| Item | Resultado desta intervenção |
| --- | --- |
| Fontes originais | Sete SHA-256 conferidos; fontes não alteradas. |
| PNGs de produção não afetados | **27/27 byte a byte idênticos** a `228f534`, incluindo a referência completa. |
| `skirt` | 23.944 pixels RGBA alterados; 0 fora da máscara autorizada. |
| `far_shoe` | 7.594 pixels RGBA alterados; 0 fora da máscara autorizada. |
| `near_shoe` | 242 pixels RGBA alterados; 0 fora da máscara autorizada. |
| Rig | Mesmos 29 sprites, 21 nós, pais, pivôs e sete poses originais. Só recortes/hashes das três peças foram atualizados. |
| Reconstrução completa | **31/31 saídas idênticas** no rebuild: 30 PNGs e `rig.json`. |
| `build_kit.py --check` | **Passou**, incluindo comparação RGBA contra a baseline fora das máscaras. |

[Dados de pixels](pixel_integrity.json), [hashes do rebuild](rebuild.json).

Pixels MCP mediu **0,69%** do canvas acima de 2% contra a referência original em fundo
comum (antes: 0,58%); erro médio 0,32/255. Entre os frames Unity neutros anterior e
atual, a diferença foi **0,04%**, erro médio 0,03/255. A mudança corresponde às regiões
de tecido/calçado e não é critério de aprovação estética. A preservação fora do escopo
é demonstrada pela comparação RGBA e pelos hashes, não por essa métrica global.
[Chamadas e resultados MCP](evidence/pixels-mcp.json).

## Testes e execução

| Validação | Resultado nesta rodada |
| --- | --- |
| Unity 6000.5.9f1 / compilação | **Passou**; harness de inspeção compilado e executado. |
| Capturas PlayMode | **Concluídas com saída 0**: 40 frames em 1280×720 e 960×600, com snapshots de Transforms/ativação. Não equivale à suíte PlayMode. |
| Quatro testes do kit | **Passaram 4/4**, sem alteração do arquivo de testes. |
| EditMode completo | **Falhou: 315/317**. Somente os dois `DocsTests` conhecidos sobre importações. Sem exclusões, mudanças ou mascaramento. |
| Neko.Sim | **Não executado nesta rodada.** Código de simulação/runtime de produção não mudou. Evidência anterior: 257/257 em `228f534`. |
| Suíte PlayMode | **Não executada nesta rodada.** O capturador foi executado, mas não se alega repetição dos 20 testes. Evidência anterior: 20/20 em `228f534`. |
| WebGL / performance | **Não executados**; nenhuma afirmação de 60 FPS ou caminhada. |

[Relatório atual](evidence/validation.json) e [evidência histórica](../evidence/validation.json).
Logs/XML completos locais: `Logs/anim01/exception/unity-editor.log` e
`Logs/tests-EditMode.xml`. Testes foram executados pelo runner protegido e ponte Windows
já documentada. Não houve erro de compilação ou exceção da aplicação na captura aceita;
mensagens nativas de licença e warnings antigos permanecem no log.

## Findings remanescentes e parada

| Finding | Gravidade | Conclusão |
| --- | --- | --- |
| A1 — acabamento do tecido | **blocking** | O preenchimento é contínuo em parte da área antes vazada, mas ainda produz painel rígido, transições de textura/luz e babado incompatíveis com a referência. Não atende ao aceite. |
| Junção com tronco exposta pelo braço ocultado | **important** | A inspeção sem o braço expõe uma superfície incompleta na composição de base. Os outros módulos foram preservados; não se ampliou a reconstrução para encobrir esse limite. |
| A2 — fechamento geométrico | **important para revisão estética** | O corte de calcanhar/lateral foi fechado nos casos inspecionados e os resíduos do próximo foram removidos. A emenda de material/volume ainda merece julgamento visual; não é aprovação estética. |
| Contratos documentais de importação | **important** | Permanecem as duas falhas de EditMode, explicitamente reportadas. |

O resultado não está pronto para ser apresentado como correção aprovada. A condição
para solicitar aceite visual como candidato bem-sucedido não foi atingida. Não se
alterou o status para aprovado nem se iniciou outra rodada, cosméticos ou Gate B.
O PR permanece draft para preservar e avaliar as evidências; sem merge ou publicação.

## Reprodução

Executar os mesmos comandos de `build_kit.py` e `--check` do relatório principal.
Para repetir os casos adicionais, manter guard, Editor exato e isolamento já existentes,
definindo **também no processo Windows** `NEKO_GATE_A_EXCEPTION=1`, além de
`NEKO_ISOLATED_SAVE=1` e `NEKO_GATE_A_OUTPUT=<saída>`. Executar
`Neko.Game.View.MinaKitGateAHarness.Begin` sem `-quit` e sem `-nographics`.

Apenas o harness recebeu controles de inspeção: escalas, ativação de membros e pose
extrema. `MinaModularKit.cs`, testes, simulação, equipamentos, save, MaidView, cenas,
pacotes e CI não foram alterados. Node/MCP usou a mesma ponte temporária Node Windows
22 descrita na entrega anterior. Nenhuma instalação ou geração externa/paga foi feita.
