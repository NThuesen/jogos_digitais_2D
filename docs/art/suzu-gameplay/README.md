# ART-02 — integração parcial do chibi Suzu

Status: implementação parcial de chibi; **aprovação visual do responsável pendente**.
Portrait/card continuam procedurais. Não representa conclusão do gate portrait + chibi.
Sem merge ou auto-merge.

## Contrato implementado

`MaidView` usa `SuzuSprites` somente quando `Maid.Name` é exatamente `Suzu`, sem equipamento,
nas poses Idle, Walk e Carry. **Mapeamento temporário por nome**, sem `visual_id`, migração de save
ou mudança de RNG. Qualquer outra identidade usa o renderer procedural existente.

As quatro folhas originais carregam por `Resources/Suzu` também no player. São fatiadas uma vez
por view (72×80, PPU 16, pivot inferior central), com descarte dos sprites ao destruir a view.
Escala local 0,25 e offset vertical −0,69 preservam a linha dos pés do renderer anterior.
A câmera continua enquadrando a sala pelo tamanho real. Não há reconstrução de textura por frame.

Idle tem quatro quadros a 250 ms; Walk e Carry andando têm oito quadros a 110 ms, pelo `Clocks.Game`.
Carry parado usa a folha estática com braços/bandeja. O bob já está na arte e não recebe o bob procedural.
O espelhamento e a ordenação continuam seguindo o deslocamento e `Layers` da view existente.
A arte é frontal e espelhada; não há desenhos laterais/de costas.

Cook, Tired, qualquer equipamento e item específico passado por `ShowCarried` usam fallback
procedural explícito. A bandeja desenhada inclui chá/bolo genéricos, sem representar receita específica;
nesse modo o renderer externo de item fica desativado. Um item específico volta ao caminho anterior,
sem sobrepor a bandeja da folha. Acessório de cabelo e uniforme do desenho são parte da identidade,
não representam equipamento equipado. Humor, pausa, itens, outras maids e estados seguem a simulação.

[Proveniência, GUIDs e hashes](../SUZU_ASSET_PROVENANCE.md).

## Smoke reproduzível sem substituir o perfil pessoal

No Test Runner do Unity 6000.5.9f1, executar `Neko.Tests.PlayMode.SuzuGameplayTests`.
A fixture guarda o valor original de `SaveBridge.Key`, carrega Boot, aciona Nova empresa,
substitui apenas o elenco da fixture pelo seed 21, nomeia a maid de salão `Suzu`, mostra a escalação
e abre Day pelas ações reais da UI. O perfil anterior é restaurado em `UnityTearDown`, inclusive
em falha de asserção. Interromper o processo à força pode impedir teardown; fazer backup externo
antes de rodar a suíte completa, pois testes antigos não têm essa proteção.

Para capturas, definir `NEKO_SUZU_CAPTURE_DIR` como diretório absoluto acessível ao Editor e executar
`Neko.Tests.PlayMode.SuzuGameplayTests.CaptureRealShiftAtBothResolutions`.
A fixture captura a câmera real com Canvas ScreenSpaceCamera, em PlayMode, a 960×600 e 1280×720;
não usa cena experimental nem reconstrói o café offline. Os primeiros frames são descartados.
Não são screenshots da janela do Editor: são frames de runtime renderizados pelo Unity.

## Exceção de manutenção

Autorização pontual do responsável em 2026-10-30 para `DocsTests.cs` e créditos do README.
Os dois testes de ausência absoluta de assets foram substituídos por créditos e allowlist fechada
com SHA-256 dos quatro PNGs; as demais asserções de rubrica permanecem.
`protected-change-guard` continua intacto e pode falhar por manutenção de teste existente.
Isso exige tratamento manual da exceção pelo responsável, com evidências independentes,
conforme [Workflow](../../agent/WORKFLOW.md). Não contornar o guard.

## Evidência visual de runtime

Sala inicial (7×5), mesma escala de mundo nas duas resoluções:

[960×600](evidence/suzu_idle_sheet_0_right_960x600.png) ·
[1280×720](evidence/suzu_idle_sheet_0_right_1280x720.png).

Sala posterior (13×9, desbloqueio do nível 9):
[960×600](evidence/later_room_13x9_960x600.png) ·
[1280×720](evidence/later_room_13x9_1280x720.png).
A fixture reposiciona o fogão para (11,6), preservando as mesas, para que a rota normal de entrega
dure o suficiente para expor os oito quadros. A rota da sala inicial encerra o movimento antes de
completar o ciclo; isso não é tratado como um ciclo de oito quadros capturado.

Sequência completa na sala maior, a 960×600 (os mesmos arquivos existem a 1280×720):

| Quadro | Walk | Carry |
| --- | --- | --- |
| 0 | [frame](evidence/later_suzu_walk_sheet_0_left_960x600.png) | [frame](evidence/later_walk_tray_0_right_960x600.png) |
| 1 | [frame](evidence/later_suzu_walk_sheet_1_left_960x600.png) | [frame](evidence/later_walk_tray_1_left_960x600.png) |
| 2 | [frame](evidence/later_suzu_walk_sheet_2_left_960x600.png) | [frame](evidence/later_walk_tray_2_left_960x600.png) |
| 3 | [frame](evidence/later_suzu_walk_sheet_3_left_960x600.png) | [frame](evidence/later_walk_tray_3_left_960x600.png) |
| 4 | [frame](evidence/later_suzu_walk_sheet_4_left_960x600.png) | [frame](evidence/later_walk_tray_4_left_960x600.png) |
| 5 | [frame](evidence/later_suzu_walk_sheet_5_left_960x600.png) | [frame](evidence/later_walk_tray_5_left_960x600.png) |
| 6 | [frame](evidence/later_suzu_walk_sheet_6_left_960x600.png) | [frame](evidence/later_walk_tray_6_left_960x600.png) |
| 7 | [frame](evidence/later_suzu_walk_sheet_7_left_960x600.png) | [frame](evidence/later_walk_tray_7_left_960x600.png) |

Fallbacks controlados da mesma view em Day, sem alterar regras:
[Tired/humor](evidence/fallback_tired_960x600.png) ·
[Cook](evidence/fallback_cook_960x600.png) ·
[equipamento](evidence/fallback_gear_960x600.png).

Os PNGs foram copiados sem edição da saída de PlayMode. [Metadados por frame](evidence/frames.tsv)
registram o sprite efetivamente renderizado, direção, resolução, tamanho ortográfico e altura do
frame completo (inclui padding). A área alfa inicial tem 72 px de altura: com escala 0,25 / PPU 16,
mede aproximadamente **99/119 px** na sala inicial e **61/73 px** na sala maior (600/720 px de tela).

Inspeção: Point preserva a silhueta/rosto na sala inicial e reduz detalhes na sala maior;
a view não soma bob ao ciclo e mantém a linha dos pés. Não foi introduzido pixel snapping de mundo
ou câmera; a movimentação suave e o enquadramento existentes continuam, sem promessa de grade
pixel-perfect. A aprovação estética e a avaliação final de jitter pelo responsável permanecem pendentes.

## Validações

Resultados extraídos dos XMLs NUnit: [validation-results.json](validation-results.json).

| Verificação | Estado | Evidência |
| --- | --- | --- |
| `tools/simtest.sh` (.NET 8 do Windows via adaptador de caminhos local) | **passed** | 257/257; zero falhas/skips |
| `tools/run-tests.sh EditMode` | **passed** | 306/306, incluindo seleção, slicing/pacing, fallbacks e proveniência |
| `tools/run-tests.sh PlayMode` | **passed** | 23/23, incluindo Boot/Day reais, direções, carry, equipamento, humor e fim do dia |
| `python3 tools/ci/verify.py boundaries` | **passed** | 49 fontes de simulação; sem dependência Unity |
| `python3 tools/tests/test_unity_validation.py` | **passed** | 7 contratos do runner |
| PNGs/GUIDs contra `670a594a31ff` | **passed** | bytes dos quatro PNGs e GUIDs originais idênticos |
| Frames de runtime nos dois tamanhos | **passed** | 8 Walk + 8 Carry na rota longa; fallbacks e café 13×9 |
| Avaliação estética final pelo responsável | **not run** | gate aberto; portrait fora deste recorte |

Unity 6000.5.9f1 instalado no Windows. Os scripts guardados do repositório foram usados com
um adaptador local (não commitado) que traduz caminhos WSL, recusa outro Editor ativo e restaura
`nekomaidinc.profile` no Registro após cada execução. EditMode, PlayMode e build são sequenciais;
workers de importação gerados pelo próprio Editor não representam uma segunda sessão.
Nenhum outro worktree foi editado. Console de runtime verificado pelas asserções de PlayMode,
sem logs inesperados; avisos legados de API e mensagens do cliente de licença não são aprovação visual.

## WebGL final — passed

`tools/build-webgl.sh` gerou **16.496.762 bytes**, Unity 6000.5.9f1, em 6m46s na execução final
(a primeira compilação sem cache levou 13m48s). Player servido apenas em localhost, sem publicação.
Os créditos do menu também foram corrigidos: a frase antiga de ausência absoluta de imagens deixou
de ser verdadeira com esta integração. Nenhum layout/controle foi alterado.

Chromium 153.0.8010.12 / Linux / SwiftShader, perfil temporário isolado. Fluxo verificado:
Nova empresa → hub; em seguida, carregamento da fixture válida de `SaveCodec` exportada por PlayMode,
substituindo somente o PlayerPrefs descartável desse navegador → reload/Boot → hub/escalação com Suzu
→ Day → resize 960×600 / 1280×720 → pular → balanço. Nenhuma chave/perfil do navegador pessoal foi usado.
A Suzu aparece com pixel art e as outras duas maids continuam procedurais; a UI do card segue procedural.

[Game View WebGL 960×600](evidence/webgl_960x600.png) ·
[1280×720](evidence/webgl_1280x720.png) ·
[balanço](evidence/webgl_results.png) ·
[créditos finais](evidence/webgl_menu_credits.png).

Console do navegador: **zero erros/exceções** no fluxo observado.
[Resumo do smoke WebGL](webgl-validation.json).
Após o ajuste do texto de créditos, EditMode passou novamente (306) e o smoke PlayMode de telas passou (1);
a suíte completa anterior de PlayMode passou 23/23. Áudio não foi avaliado auditivamente e aprovação
estética final continua **not run / pendente**. Guard de manutenção continua independente destes resultados.
