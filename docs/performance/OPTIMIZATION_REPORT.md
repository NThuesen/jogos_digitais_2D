# Auditoria e otimização WebGL — Neko Maid Inc.

Entrega de 2026-11-06, branch `perf/webgl-textures-runtime`, base `origin/main` **6c1c54d**. Build e testes completos passaram. Uma PR consolidada, sem merge ou auto-merge.

## 1. Baseline e comparação controlada

**A build caiu de 108.517.998 para 63.722.143 bytes: −41,3%.** As caminhadas empacotadas caíram de 153.172.308 para 68.510.580 bytes: **−55,3%**. São medidas de arquivos/BuildReport, não VRAM física medida.

MB abaixo significa 1.000.000 bytes; MiB significa 1.048.576 bytes. A build anterior de “103,5 MB” corresponde a aproximadamente 103,5 **MiB**.

| Métrica | Baseline main | Otimizada | Natureza da evidência |
| --- | ---: | ---: | --- |
| Build para distribuição | 108,52 MB / 103,49 MiB | 63,72 MB / 60,77 MiB | bytes reais, exclui DoNotShip |
| Download `.data.unityweb` | 97.119.765 B | 52.316.892 B | gzip real, −46,1% |
| `.wasm.unityweb` | 11.256.188 B | 11.263.242 B | bytes reais; pequeno aumento de código |
| Caminhadas no BuildReport | 153.172.308 B | 68.510.580 B | dados empacotados, −55,3% |
| Pixels dos PNGs originais | 28.721.952 | 28.721.952 | 23 sheets, sem redução de resolução |
| Texturas nativas no Editor nulo, caminhadas | 153.190.800 B, RGBA32 | não comparado nesse modo | inventário real, não VRAM |
| Backing teórico, 23 sheets e todos os mipmaps | ≈153,18 MB RGBA32 | ≈68,51 MB BC3 com padding | cálculo; não residência física |
| Inicialização local até sumir loading | 4.534 ms | 3.464 ms | uma amostra por processo Chrome novo |
| Heap JS após F8 | 172,46 MB | 120,92 MB | `performance.memory`, após GC explícito; não heap C# |
| rAF médio, 300 frames na F8 | 8,291 ms | 8,294 ms | apresentação Chrome, ≈120 Hz; não CPU/GPU isoladas |
| rAF p95 / pior intervalo | 8,5 / 9,1 ms | 8,5 / 8,8 ms | amostra curta, sem conclusão estatística |
| `CustomerView.Sync`, 10.000 chamadas | 152,875 ms | 13,537 ms | microbenchmark Editor; −91,1% neste trecho |
| Heap C#, GC por frame e VRAM física | indisponíveis | indisponíveis | não estimados como medições |

A build que existia inicialmente no disco tinha código visual anterior à main (inclusive título/layout F8 diferentes). Foi inventariada em `baseline-build.json`, mas **não foi usada como controle final**. Geramos `Build/WebGL_baseline` com `SpriteFactory`, `CustomerView` e os 23 importadores originais de `6c1c54d`, usando o mesmo Unity, release IL2CPP e gzip da candidata. Esses fontes/configurações foram restaurados ao terminar. Nenhuma worktree foi criada.

Evidências: [inventário baseline controlada](baseline-main-build.json), [inventário otimizado](optimized-build.json), [BuildReport baseline](baseline-main-packed-assets.tsv), [BuildReport otimizado](optimized-packed-assets.tsv), [inventário nativo inicial](baseline-textures.json). A estimativa inicial de 224 MB **não foi reproduzida apenas para caminhadas**; o BuildReport lista todas as texturas em 222,1 MiB na baseline, incluindo retratos/UI.

## 2. Arquitetura e gargalos encontrados

Unity **6000.5.9f1**, URP 2D, desktop WebGL. Boot constrói a apresentação em código. `Neko.Sim` continua independente de Unity; regras/economia/saves permanecem intocados. `GameSession` continua mediando alterações persistentes. As 23 identidades liberadas e quatro Planned permanecem iguais.

O catálogo JSON de caminhada é lido uma vez. `MaidAnimCatalog` resolve identidade e delega a `SpriteFactory.Sheet`, que corta e mantém os frames por recurso. As texturas são carregadas por `Resources.Load` quando necessárias; todos os arquivos em Resources entram na distribuição. A F8 usa o mesmo cache global, carrega todas as caminhadas e conserva referências após fechar. Telas ocultas são desativadas e não executam Update.

Os principais recursos empacotados eram:

| Grupo Resources | Antes | Depois |
| --- | ---: | ---: |
| Maids, incluindo catálogo | 153,18 MB | 68,52 MB |
| MaidPortraits | 41,52 MB | 41,52 MB |
| UiArt | 31,26 MB | 31,26 MB |
| Suzu de rollback | 1,39 MB | 1,39 MB |
| Audio | 0,83 MB | 0,83 MB |

Não foi encontrada criação de animações/GameObjects por desenho de caminhada. `MaidView` usa estado existente, troca frames e atualiza posição/sorting. `ShiftPresenter` cria clientes na chegada e destrói na saída, reutilizando listas auxiliares. Effects já possui pools. Não há novo sistema de pooling, atlas ou carregamento.

Um custo repetitivo foi confirmado em clientes: mesmo os hits de cache de sprites formatavam strings/cores a cada `SyncBubble()`. Há ainda formatação recorrente no `DayHud.Sync`, mas seu custo global não foi medido; ficou como próxima investigação, sem alteração especulativa.

## 3. Otimizações implementadas

### Texturas de caminhada

Somente marcar override DXT5 não bastou: as sheets NPOT com mipmaps continuaram RGBA32 no importador ([experimento](dxt-import-experiment.json)). A build agora prepara dimensões compatíveis com DXT e uma cadeia completa de mipmaps:

1. Salva os bytes originais em `Library/WebGLWalkTextureBackup`.
2. Acrescenta margem transparente **à direita e acima**, até potências de dois; mantém todos os pixels e origens dos frames.
3. Importa em **DXT5/BC3**, qualidade 100, alpha completo, sem Crunch; subtarget desktop DXT explícito.
4. Constrói e restaura os PNGs em `finally`, reimportando as fontes originais.

`SpriteFactory` reconhece a margem somente no player WebGL. Catálogo, PPU, cadência, identidade e tamanho visual dos frames não mudam. A template do pipeline mantém os overrides nas regenerações futuras. Os **23 PNGs finais foram comparados byte a byte com a main: todos iguais**. Nenhum RAW ou arquivo de `art_working/` foi alterado.

O padding aumenta os texels físicos para 51,38 Mpx, mas BC3 reduz o backing calculado para aproximadamente 68,51 MB, contra 153,18 MB RGBA32. Este é um cálculo de formato, sem overhead do driver. O Chrome confirmou `texStorage2D` BC3 (`0x83F3`) em todas as dimensões esperadas das 23 sheets, além de uma textura já existente com esse formato. Isso confirma solicitações comprimidas reais; não mede VRAM residente.

Mipmaps, Trilinear, bias e alpha suave foram mantidos. As figuras de 240 px são desenhadas menores no salão/F8; remover mipmaps poderia gerar cintilação. Read/Write já estava desligado nas sheets. Não reduzimos resolução, não alteramos desenhos, não criamos atlas entre identidades e não convertimos ilustração em pixel art.

### Runtime

O fundo do balão de clientes é resolvido na criação; o ícone somente quando estado ou referência do próximo prato mudam. O teste novo cobre mudança de prato sem mudança de estado e Bind de outro cliente. O microbenchmark usa 100 warmups e 10.000 `Sync()` reais de um cliente Ordering, em Editor `-nographics`. O ganho não representa ganho equivalente no frame inteiro. `GC.GetAllocatedBytesForCurrentThread` retornou zero nas duas versões; o contador não forneceu evidência útil de alocações nesse ambiente.

Texturas procedurais de `SpriteFactory` passam a liberar a cópia CPU após upload no player (`Apply(false, true)`). O Editor mantém leitura para as ferramentas existentes de QA de alpha. Não foi medido separadamente o total economizado por essa mudança.

### F8 e compartilhamento

Não descarregamos texturas importadas compartilhadas ao fechar a F8. O cache continua evitando recriar frames e preserva Salão/Elenco. Não há `UnloadAsset` nem varredura global de recursos que introduza risco de sprites inválidos ou hitches ao retornar. A retenção após fechar permanece um trade-off conhecido, agora com dados comprimidos.

## 4. Qualidade e regressão visual

Inspeção real no Chrome, antes/depois em **1280×720 e 960×600**, incluindo dois instantes de caminhada em cada tamanho:

| Resolução | Original main | BC3 | Outro frame BC3 |
| --- | --- | --- | --- |
| 1280×720 | [baseline](baseline-main-1280x720.png) | [otimizada](optimized-1280x720.png) | [caminhada](optimized-1280x720-walk.png) |
| 960×600 | [baseline](baseline-main-960x600.png) | [otimizada](optimized-960x600.png) | [caminhada](optimized-960x600-walk.png) |

As 23 aparecem com rosto, olhos, cabelos, acessórios, cores e transparência preservados na escala observada. Não foram vistos blocos/halos novos que descaracterizem as personagens. BC3 é lossy: não alegamos igualdade de pixels nem aprovação em hardware que não foi testado. A F8 já usa quase toda a largura a 960×600, com extremos junto à borda, também na baseline; seu layout não foi redesenhado.

Verificados fechar/reabrir F8, [Salão](optimized-hall-after-f8.png), [Elenco](optimized-roster-after-f8.png) e [turno real](optimized-live-shift.png), com [outro instante de caminhada](optimized-live-shift-walk.png). O `3/2` preexistente do Elenco permanece intacto. Retratos/fundos não foram recomprimidos por esta política.

## 5. Validação executada

| Verificação | Resultado |
| --- | --- |
| `tools/simtest.sh`, SDK .NET Windows | **PASS: 278/278** |
| `tools/run-tests.sh EditMode` | **PASS: 349/349** |
| `tools/run-tests.sh PlayMode` | **PASS: 97/97** |
| `python3 tools/ci/verify.py boundaries` | **PASS** |
| `python3 -m unittest discover -s tools/tests -q` | **PASS: 12 testes** |
| Build WebGL baseline e otimizada | **PASS**, BuildResult.Succeeded |
| Chrome: boot, F8 23 maids, retorno às telas e turno | **PASS**, inspeção/capturas reais |
| Testes existentes, workflows, cenas e ProjectSettings | **sem mudanças na PR** |

Sem falhas, skips ou inconclusivos nas três suítes principais. [Resumo com hashes dos XML](validation.json). Os XML/logs completos estão em `Logs/` localmente. O teste novo foi acrescentado; nenhum teste protegido foi modificado.

Unity/.NET estão instalados no Windows. Usamos wrappers temporários para converter caminhos WSL em caminhos Windows, os runners/guard existentes e consulta adicional aos processos Windows para distinguir `unity mcp` do Editor. Builds/auditoria usaram `-executeMethod` com o mesmo guard, sem apagar a build anterior antes de capturar sua baseline. Uma serialização automática de batching WebGL em ProjectSettings foi retirada por não fazer parte da implementação; nenhuma configuração protegida entrou no diff.

## 6. Método de navegador e limites

Chrome 156 em modo headless, WebGL2 via ANGLE/Direct3D11 na Intel Iris Xe (ver [ambiente](browser-environment.json)), S3TC e S3TC sRGB anunciados; ASTC/ETC2 não anunciados neste equipamento. Servidor local, perfil exclusivo desta auditoria, processo Chrome reiniciado entre builds, HTTP cache e IndexedDB limpos. Unity/builds estavam encerrados durante a amostragem final. [Dados baseline](baseline-main-browser.json), [dados otimizada](optimized-browser.json).

As amostras JS foram feitas após GC explícito por DevTools. Navegar no mesmo processo havia retido contextos Unity anteriores e contaminado os números; essas amostras foram descartadas. Os valores finais indicam menor heap JS nessa execução, não menor heap gerenciado C# ou pico máximo. Inicialização é o intervalo entre o script inicial e o fechamento da barra de loading; uma amostra local não prevê o tempo em rede real.

`requestAnimationFrame` mede apresentação do navegador; p95 igual e ausência de piora visível sustentam estabilidade nessa amostra, não uma medição de FPS interno, CPU Timeline, GPU ou GC espontâneo. O aviso de shader FSR não suportado apareceu nas duas builds; nenhum erro/exceção JS novo foi observado. Não há benchmark controlado de 3 versus 8 versus 23 maids: menu com três, turno inicial e F8 foram vistos, mas possuem cenas/trabalho diferentes. Não atribuímos suas diferenças somente ao número de personagens. Também faltam pico/custo preciso de abertura F8, VRAM física e pausas de GC durante atendimento.

## 7. Compatibilidade, operação e trade-offs

BC3 é apropriado ao alvo desktop com S3TC. Navegadores sem suporte podem usar fallback descomprimido e perder o ganho de memória; essa compatibilidade **não foi certificada em outro hardware**. ASTC como único payload reduziria cobertura desktop; ETC2 não foi anunciado no Chrome testado; BC7 melhoraria fidelidade, mas exige BPTC e tem suporte mais restrito. Não introduzimos distribuição dupla ou detecção customizada. Referências oficiais: [compressão Web Unity 6000.5](https://docs.unity.com/en-us/engine/6000.5/manual/platform-specific/webgl/develop/texture-compression), [formatos GPU](https://docs.unity.cn/6000.5/Documentation/Manual/texture-formats-reference.html).

A preparação acrescenta reimports e escrita temporária ao build. Em encerramento normal ou exceção gerenciada, `finally` restaura as fontes. Se o processo for morto, o backup durável permanece e impede nova preparação. Recuperação: com nenhum Editor ativo, copiar os PNGs de `Library/WebGLWalkTextureBackup` para `Assets/_Game/Resources/Maids`, remover o diretório vazio e reimportar. Não editar os PNGs processados durante uma build. Nenhum backup ficou pendente nesta entrega. Procedimento e ferramentas: [tools/performance/README.md](../../tools/performance/README.md).

## 8. Próximos gargalos, por impacto

1. Medir residência real/pico F8 e perfil de turno em hardware com e sem S3TC; coletar CPU Timeline, GC.Alloc, draw calls, overdraw e Canvas.BuildBatch.
2. Retratos (41,52 MB) e UiArt (31,26 MB): avaliar separadamente compressão/residência, com close-ups; não aplicar BC3 a rostos grandes indiscriminadamente.
3. Se a retenção F8 ainda for relevante, projetar propriedade explícita dos frames entre consumidores antes de descarregar recursos; preservar retornos sem hitches.
4. Medir formatação de HUD e rebuilds de Canvas; só separar canvases ou alterar atualização com evidência de custo.
5. Comparar 3/8/23 em um harness equivalente. Não criar pooling adicional sem observar custo relevante de chegadas/saídas.

O ganho comprovado de armazenamento e o ganho localizado de CPU justificam estas mudanças. Otimizações adicionais de assets/arquitetura exigem as medições e a validação visual acima; esta PR não oferece um perfil que o ambiente não produziu.

## 9. Git

Base `6c1c54d`, branch `perf/webgl-textures-runtime`. Implementação: `9282abf` (compressão/padding/cópia CPU) e `cf599be` (cache de ícones e teste). As ferramentas e evidências estão no commit de auditoria desta PR. Link da PR acrescentado após sua criação. Sem publicação, merge ou auto-merge.
