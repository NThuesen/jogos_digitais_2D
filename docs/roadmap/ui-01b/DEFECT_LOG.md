# UI-01B — Defect log (regressão funcional das demais telas)

Escopo: Empresa, Escalação, Caixas, Menu, Salão, O dia, Diálogos e Falência, em **960×600** e **1280×720** (e o retorno a 960×600 do Layout_Check).
Sem redesign. "Antes" = código de `origin/main` (`ae3dcb2`) com os novos estados/regras; "depois" = esta branch. Capturas reais do jogo em `evidence/`
(`before_*` / `after_*`); registros brutos em `report_before.jsonl` / `report_after.jsonl` (gerados com `NEKO_CAPTURE_DIR` e `NEKO_LAYOUT_REPORT`).

## Cobertura real (14 estados novos, 2 resoluções cada)

| Tela | Estados | Antes (violações) | Depois |
|---|---|---|---|
| Menu | MN-1 novo jogo; MN-2 recordes máximos; MN-3 diálogo "Nova empresa" | 0 / 0 / 0 (mas MN-2 tinha defeito visual → check novo; falha antes, passa depois) | 0 |
| Empresa | EM-1 (existente, regras agora ativas); EM-2 valores máximos + 8 maids | 45 / 108 | 0 |
| Escalação | ES-1 escalação cheia; ES-2 escalação vazia com 8 maids no banco (estado legítimo) | 117 / 50 | 0 |
| Caixas | CX-1 prateleira; CX-2 resultado após abrir uma caixa | 0 / 3 | 0 |
| Salão | SL-1 estoque cheio; SL-2 peça escolhida (ambos checam tiles cobertos por UI) | 30 / 30 | 0 |
| O dia | OD-1 dia congelado, escalação cheia | 3 | 0 |
| Falência | FL-1 | 3 | 0 |
| Diálogos | DG-1 confirmar/cancelar sobre a Empresa (bloqueio de cliques); DG-2 erro sobre o Salão | 9 / 3 | 0 |

Regras R3.1/R4.1/R4.2/R6.2 (antes restritas a EL/LJ/BL) agora valem para todos os estados. UI-01A (EL/LJ/BL) segue passando sem alteração.

## Defeitos reproduzidos e corrigidos (9)

| # | Tela | Observado (antes) | Sev. | Correção |
|---|---|---|---|---|
| D1 | Menu | Ao abrir o jogo com save, o Menu **não mostrava recordes** e o botão dizia "Abrir a empresa" em vez de "Continuar": `SetSession` rodava antes do `Build` tardio da tela. Além disso as quebras de linha eram `\\n` literais. | IMPORTANT | `MenuScreen` guarda a sessão e reaplica em `Refresh`; `\n` real. Check MN-2 (falha antes, passa depois). |
| D2 | Empresa | Wordmark "NEKO MAID INC." sob a placa de estatísticas (21,6×12,8 u em 960; R4.1) **e** botão "Voltar" com 179/257 pontos bloqueados pela placa (R6.2; o "D-EM-5" do UI-01A, que era só diagnóstico, se confirma). | IMPORTANT | Placa do balanço desce de y=-16 para -64; cartões/química deslocados 32–42 u. |
| D3 | Empresa | Rótulo "Escalação" maior que a placa do botão; cartões das maids com números/rodapé cortados pela moldura (C1). | IMPORTANT | `Scenery.Plank(..., exact)` novo e opt-in (botão 240 u); `MaidCard exact: true`. |
| D4 | Escalação | Banco e quadro com 1200 u numa janela de 1152 u: cartões das pontas cortados (R2.1); cartões com rótulos fora da moldura (C1); botões 190×42 (<44 u, R6.2); legenda do atributo sobre o número em cartões 0,62× (R4.1). | IMPORTANT | Larguras 1100; `exact` nos cartões; botões 46 u; caixas de texto do `Pip` com altura mínima de uma linha. |
| D5 | Salão | Título "Arrumar o salão" **cortado fora da janela** (placa de 1240 u, R2.1); métricas fora da placa; **13 de 117 tiles cobertos pela prateleira de estoque** (cliques em tiles bloqueados). | IMPORTANT | Placa e prateleira 1100 u, `exact`; o Salão usa `CameraRig.FrameRoomBetweenHud` (já existente, não alterado) entre barra e estoque. Estoque continua em linha horizontal rolável. |
| D6 | Falência | Rótulo "Abrir outra empresa" (325 u) mais largo que o botão desenhado (278 u). | IMPORTANT | Botão 400 u, `exact`. |
| D7 | Diálogo | Texto de 560 u fora da caixa desenhada (520 u, C1); botões com rótulo encostando na borda. | IMPORTANT | Caixa `exact`; botões 230 u. Bloqueio de cliques sobre a camada inferior verificado em MN-3/DG-1/DG-2: **sem falha**. |
| D8 | Caixas (resultado) | Nome/rodapé da carta fora da moldura (C1). | COSMETIC→corrigido | `MaidCard exact: true` no resultado. |
| D9 | O dia | Texto da barra superior (caixa do dia) fora da placa (C1). | IMPORTANT | Placa `exact`. Câmera/HUD do PR #30 e Yoru inalterados. |

Nenhum BLOCKING foi reproduzido. Caixas (CX-1), Menu (MN-1/3) e os cliques nos botões fora de D2 não apresentaram falha.

## Cosméticos para o redesign futuro (não implementados)

- Painéis das demais telas continuam com o sprite esticado (84%) onde não houve falha medida; o redesign deve substituí-los.
- Fonte minúscula nos cartões de maid reduzidos (0,62×/0,72×) e legenda do Menu em creme 60% sobre o piso: legíveis, mas provisórios.
- O estoque do Salão é uma linha rolável provisória (a coluna de 128 unidades histórica **não** foi implementada).

## Não coberto (NOT RUN)

Dos 40 estados históricos só os 25 registrados rodam (11 do UI-01A + 14 novos). Não rodados: escalação com slot escolhido/seletor, Loja/Elenco em diálogos, Salão com peças colocadas, dia em andamento/aceleração, Falência com textos extremos, Balanço BL-4, estados de erro de save. WebGL: **NOT RUN** (nenhuma mudança exigia build). Console: sem erros inesperados (o Unity Test Framework falha o teste em `LogError`); a asserção explícita `LogAssert.NoUnexpectedReceived()` do TODO do `Audit` continua pendente.

## Testes (execução desta branch)

| Suíte | Resultado |
|---|---|
| EditMode completo | **PASS 326/326** |
| PlayMode completo | **PASS 76/76** (62 anteriores + 14 estados novos) |
| `LayoutCheckTests` reexecutado após o último ajuste de teste (check MN-2) | **PASS 32/32** (o check MN-2 falha no código antigo e passa no novo) |
| WebGL | NOT RUN |

Isolamento de saves: o `SessionFactory.Boot` guarda o save existente e o restaura (`RememberSave`/`RestoreSave`); nenhum código novo escreve save fora dele. Runner usado via `tools/run-tests.sh` com o guard de processos; o servidor `unity mcp` não foi tocado.

## Arquivos protegidos alterados (exigem exceção administrativa)

- `Assets/_Game/Tests/PlayMode/Layout/AuditStates.cs`: +14 estados (MN-1..3, EM-2, ES-1..2, CX-1..2, SL-1..2, OD-1, FL-1, DG-1..2), rotinas de build e três checks (`MenuShowsTheSavedRecords`, `DialogBlocksTheLayerBelow`, `TilesAreClickable`). Nenhuma asserção removida.
- `Assets/_Game/Tests/PlayMode/Layout/LayoutRules.cs`: `IsPriorityScreen` passa a ser verdadeiro para todos os estados (R3.1/R4.1/R4.2/R6.2 valem em todas as telas). Amplia a verificação; nada afrouxado.
- Sem alteração em workflows, `.meta` novos além dos gerados pela Unity, cenas, `Packages/` ou `ProjectSettings/`.
