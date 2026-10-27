# Decisão visual: Maid Card + Chibi

**Estado:** ACCEPTED — direção visual de produto, **não implementada**.
**Âmbito:** aparência de cada maid no Neko Maid Inc.; não altera regras de gameplay, raridade ou economia.
**Contrato dos dados:** [MAID_VISUAL_SPEC.md](MAID_VISUAL_SPEC.md).
**Ficha por identidade:** [maids/_TEMPLATE.md](maids/_TEMPLATE.md).
**Para implementar:** ler [nota Unity](../agent/UNITY.md) e os arquivos de código indicados abaixo, sem carregar o GDD completo salvo mudança de produto.

## Decidido
1. Toda maid deve ser reconhecível em **duas representações 2D** da mesma identidade:
   - **Portrait / Maid Card:** arte expressiva e atraente, com ênfase em rosto, cabelo, uniforme, personalidade e colecionabilidade; o card é composto pela UI do jogo.
   - **Chibi / Restaurante:** sprite compacto, fofo, silhueta legível em movimento e animações reutilizáveis (andar, esperar, servir etc.).
2. **Mesma identidade, assets/apresentações diferentes.** Cor e silhueta do cabelo, orelhas, cauda, cores, uniforme e acessório característico devem combinar entre portrait e chibi.
3. Visual composto de **camadas intercambiáveis** e parâmetros estruturados; peças podem ser trocadas isoladamente, sem redesenhar todas as maids. Produção de arte e código fica a cargo da IA; a pessoa responsável **aprova ou rejeita a aparência**, sem desenhar/editar manualmente.
4. **Card = portrait + UI nativa** (nome, raridade/moldura, posição, números). Não gerar como uma imagem única todo o texto e a moldura do card.
5. O sistema preserva a legibilidade do arquétipo e seus sinais distintivos já definidos no [GDD, §2.6](../GDD.md) e a cor da moldura de raridade no [GDD, §2.8](../GDD.md). Uma escolha estética não pode mudar as regras da simulação.

## Situação existente (verificada no código)
- `Assets/_Game/Scripts/Game/View/MaidSpriteBuilder.cs`: chibi por camadas de `ShapeSpec`; poses `Idle`, `Walk`, `Carry`, `Cook`, `Tired`; equipamento já muda camadas.
- `Assets/_Game/Scripts/Game/View/MaidView.cs`: apresenta e anima o chibi no restaurante.
- `Assets/_Game/Scripts/Game/Ui/MaidCard.cs`: monta o card na UI, **mas o portrait atual reutiliza o sprite gerado por `MaidSpriteBuilder`**.
- `Assets/_Game/Scripts/Game/Art/ShapeRasteriser.cs`: rasterização procedural existente.
- O [GDD, §8](../GDD.md) descreve o **estado atual**, com formas geradas em runtime e nenhuma imagem importada. Esta decisão documenta uma **direção futura**; não declare PNGs, portrait separado, novos renderers ou assets já existentes.

## Ainda aberto — não decidir por inferência
- Estilo final, paleta e proporções do portrait/chibi; exigir aprovação de referências visuais antes de produzir biblioteca grande.
- Técnicas e formatos de produção (imagens IA transparentes, camadas vetoriais/SDF existentes ou combinação), resolução, slicing, atlases e animação. **Python/Pillow, ScriptableObjects, prefabs e Unity 2D Animation são alternativas, não requisitos aceitos.**
- Quantidade de partes/variantes, gatilho de geração (editor/offline/runtime), regras de compatibilidade entre camadas e persistência de seeds/IDs. Resolver quando uma implementação real exigir.
- Se haverá uma ficha por personagem nomeado, por família visual ou uma spec resolvida a partir de combinações. **Não criar um arquivo Markdown por carta aleatória do jogador.**

## Experimento mínimo recomendado (não é milestone aprovado)
Gerar **uma** identidade de maid em portrait e chibi a partir da mesma spec, com uma variação de cabelo/acessório; inserir um card no layout existente e mostrar chibi andando. Avaliar lado a lado: fofura, clareza em escala real, coesão e custo de modificar uma peça. Só expandir biblioteca após aprovação visual.

## Critérios para aceitar uma implementação
- Visual no card e no restaurante corresponde à **mesma maid** sem leitura do nome.
- Camada alterada muda somente seu efeito esperado nas duas representações; sem refazer personagens inteiros.
- Pose andando/esperando mantém leitura em tamanho de jogo; texto e raridade do card continuam nítidos pela UI.
- Mudanças de arte não duplicam lógica de stats/economia nem quebram equipamento visual existente, save, cenas ou GUIDs.
- Evidência real: screenshots/capturas de card e gameplay no Unity; build/PlayMode informados como passou/falhou/não executado. CI de simulação não comprova estética.

**Quando revisitar esta decisão:** se o protótipo demonstrar que camadas degradam qualidade ou tornam portrait/chibi inconsistentes, registrar a evidência e decidir alternativa — não substituir silenciosamente a direção.
