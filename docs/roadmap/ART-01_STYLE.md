# ART-01 — Exploração artística do Neko Maid Inc.

**Status:** **DIREÇÃO D APROVADA PROVISORIAMENTE PARA PROTOTIPAGEM** (2026-10-28); **ART-02 continua BLOCKED até o responsável liberar explicitamente**. `style_anchor`: [`docs/art/art-01/D-pixel-cafe/`](../art/art-01/D-pixel-cafe/README.md) — referência de estilo `reference/style_anchor_reference.png`; identidade Suzu `incoming/D0_suzu_sheet.png` + `D1_portrait.png` + `D2_chibi_poses.png`; café `D3_cafe_interior.png`; kit de UI `D4_ui_kit.png`; avaliação e pendências técnicas em `EVALUATION_FINAL.md`. As propostas A/B/C (branch `art/art-01-style-exploration`) ficam como estudos. Os PNGs gerados **não** são assets finais. **Agente recomendado (original):** Claude/Claude Code (exploração e síntese visual).
**Resultado necessário:** escolher com o responsável uma direção visual **aprovada** antes de construir biblioteca de assets.
**Leia apenas:** [baseline](VISUAL_BASELINE.md), [decisão maid portrait + chibi](../art/MAID_VISUAL_DECISION.md) e [spec conceitual](../art/MAID_VISUAL_SPEC.md). O GDD completo só se uma regra de jogo afetar a proposta. Não implementar Unity nesta tarefa.

## Hipótese
A UI monocromática de marrom/madeira compete com as personagens. Uma estética de café kawaii 2D, com madeira como apoio em vez de superfície dominante, **pode** valorizar a fantasia de colecionar maids. Isso é hipótese, **não paleta decidida**.

## Trabalho solicitado ao agente
1. Propor **2–3 direções estéticas realmente distintas** (não apenas recolorações). Uma delas pode explorar creme/rosa/menta/mel + madeira secundária; ao menos uma deve testar outra solução com coerência de café e leitura.
2. Para tornar a comparação justa, usar o **mesmo brief de uma maid** nas propostas. Cada proposta deve incluir:
   - portrait expressivo/busto para card;
   - chibi do mesmo personagem em escala próxima à apresentação ingame;
   - card composto visualmente com moldura, nome e raridade separados do portrait;
   - amostra de tela de Escalação ou Elenco, mantendo hierarquia legível.
3. Mostrar pelo menos uma maquette de UI com fundos, contraste, botões e títulos. Distinguir **mockup exploratório** de captura real de Unity; não afirmar que funciona no runtime.
4. Comparar critérios de **fofura/apelo**, identidade retrato↔chibi, legibilidade em 960×600, qualidade de UI, distinção de raridade e viabilidade de produção modular por IA.
5. Registrar as prévias em arquivos acessíveis ao responsável (repositório/PR com caminhos, ou arquivos externos explicitamente referenciados) e uma nota de escolha curta: vantagens, riscos, pontos ainda não testados. Não exigir trabalho manual de edição de imagem do responsável.
6. Expor escolhas para aprovação/rejeição executiva; **não converter preferência da IA em estilo final sem aprovação**.

## Regras e constraints
- A decisão **ACCEPTED** [Portrait + Chibi](../art/MAID_VISUAL_DECISION.md) é fixa neste experimento; a stack (PNG em camadas, SDF, ferramentas, rig, ScriptableObjects) **continua aberta**.
- Não criar coleção de acessórios/uniformes, nem refazer o jogo, nem gerar dezenas de maids.
- Usar personagens originais e registrar origem/licença de assets/gerações quando pertinente; evitar copiar franquias/personagens protegidos.
- Arquivos de prévia não têm de entrar no build. Não editar `Palette.cs`, scenes, UI atual ou GDD no PR de exploração.
- Não pedir que o responsável desenhe, corte PNGs, ajuste camadas ou programe: apenas avaliar resultado.

## Gate para ART-02
**O responsável** seleciona uma direção visual, rejeita todas ou solicita iteração. Somente após decisão explícita registrar referência aprovada (`style_anchor`/caminhos) e, se a direção materialmente alterar identidade de produto, atualizar a documentação de arte/GDD no PR adequado.

**Pronto quando:** 2–3 alternativas visuais comparáveis entregues + quadro crítico de trade-offs + links de prévia + aprovação/rejeição registrada. **Merge automático de documentação não equivale a aprovação estética.**
