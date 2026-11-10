# UI-INT-05/06 — arquivos protegidos tocados

Nenhum workflow, guard (`protected-change-guard`), runner ou proteção de branch foi alterado; nenhum teste foi removido, desativado ou enfraquecido; `LayoutCheckTests`, `LayoutRules`, `AuditStates`, `KitLegibility` e `ResolutionRig` estão intactos. Os estados do Layout_Check de Salão, O dia e Balanço passam como estão.

| Arquivo | Natureza |
|---|---|
| `Assets/_Game/Tests/EditMode/Game/DocsTests.cs` | +33 entradas na allowlist fechada de proveniência (só os PNGs `hall_*`, `shift_*` e `results_*` importados; hashes em `docs/art/SUZU_ASSET_PROVENANCE.md`) |
| `Assets/_Game/Tests/PlayMode/HallShiftResultsScreenTests.cs` | arquivo novo |
| `Assets/_Game/Tests/PlayMode/RoomCostTests.cs` | limite de distância da câmera adaptado à janela do kit (ver abaixo) |

Um teste existente foi adaptado (abaixo).

## `RoomCostTests.TheStartingRoomStaysInsideItsSpriteBudget` — adaptado, não enfraquecido
O teste afirma que a câmera do dia não fica mais longe do que o enquadramento original entre as barras do HUD antigo (faixas 92/128 sobre 720). O dia agora é desenhado na abertura ilustrada do café (746×445 do mockup), uma janela menor que essa faixa, então a câmera precisa ficar mais longe (8,85 contra 4,40) para que o salão e a cabeça de uma maid na fileira de cima caibam inteiros: é o contrato do kit, não um encolhimento a mais.
- Preservado: orçamento de sprites, nenhum collider/script no salão, e a regra "a câmera não está mais longe do que o enquadramento mais próximo que ainda cabe". Sem o kit (fallback) a asserção original vale como estava.
- Trocado: com o HUD do kit, o limite é o enquadramento mínimo que cabe dentro da janela real (`RoomWindow`, com 1% de tolerância para o arredondamento da janela em pixels), calculado com a mesma folga de cabeça (0,8) e pés (0,3) do teste original.
