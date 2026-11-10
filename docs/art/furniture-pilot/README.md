# ART-INT-01 — piloto de móveis ilustrados (stage, koi_pond, window)

**Causa-raiz.** `Layers` separava `Furniture=2` de `Characters=3` em faixas de 1000: personagem sempre sobre móvel, em qualquer Y. Agora móveis sólidos e personagens compartilham uma faixa (`Layers.ActorOrder`, 40 slots/tile), chaveada pelo contato com o piso: a borda frontal da pegada do móvel (`DepthSort.SolidKey`) e os pés do personagem. Quem está **dentro** da pegada (cliente sentado, maid em estação — o sim anda em linha reta pelos móveis) fica logo à frente da peça (`DepthSort.ActorKey`). Item carregado = slot +1 do corpo.

**Arte.** `IllustratedFurniture` calibra cada PNG pela área ilustrada (largura em tiles, pivô embaixo-centro, apoio na borda frontal). Sem entrada, pegada diferente (rotação) ou PNG ausente → desenho procedural. `IllustratedFurniture.Enabled=false` desliga tudo. PNGs derivados do `raw_gpt` do Codex (não havia CLEAN) por `tools/art/furniture_pilot/make_pilot_assets.py`; o lago é dividido em base plana (imagem inteira) + elevado (mesma imagem sem a água), alinhados ao pixel.

**Capturas** (`DepthProbe.cs`, Unity real, maids em frente/atrás/ao lado/sobre cada peça): `pilot_*.png` (novo) e `procedural_*.png` (mesma sala, arte procedural, mesmo sorting novo). "Antes" com o sorting antigo: `art_working/ui_modular/qa/composition/` da branch do Codex.

## Pendências / REWORK para o Codex
- `stage`: a plataforma ocupa só ~40% da altura da arte; a pegada 2×2 fica visualmente rasa. Sem REWORK obrigatório; sem mecânica de subir (maid "sobre" o palco = dentro da pegada, desenhada à frente).
- `koi_pond`: halo marrom exigiu limpeza; cortes de água têm resíduo mínimo nas bordas (invisível pois a base está por baixo).
- `window`: peça com pés e jardineira, usada como móvel de piso livre; não virou objeto de parede (decisão de produto pendente).
- `chandelier`, `showa_poster`: não integrados (precisam de posição suspensa/parede; ver plano abaixo).

## Expansão
Mesma infraestrutura serve `cat_tower`, `sakura_tree`, `lantern_sakura`, `gothic_candelabra`, `star_lamp` (1×1/1×2 sólidos): basta PNG CLEAN + entrada na tabela. Pendentes de decisão: peças suspensas/de parede (lustre, cartaz, espelho) pedem uma faixa "wall/hang" própria.

## Validação (final)
- **EditMode** e **PlayMode** completos (Unity 6000.5.9f1, batchmode): ver contagens no PR.
- **Gameplay real** (`GameplayDepthTests`, cena Boot, novo jogo, dia de ~70 s com HUD, 3 maids, clientes, mesas, estações, `stage`, `koi_pond`, `window` girada, `table_2` e `cat_tower` girados): a cada frame, corpo e prato de cada maid e cada cliente têm a ordem esperada de `DepthSort`, e cada peça sólida (incluindo a parte elevada do lago) está na ordem da sua borda frontal; um clique em cliente na fila foi respondido. Frames: `gameplay/play_06_*`, `play_10_*` (960×600 e 1280×720, com HUD).
- **Girados / fallback:** mesa de 2 girada (1×2) e arranhador girado (2×1) desenham procedural; janela girada (1×1) mantém a arte ilustrada. `IllustratedFurniture.For` só casa a pegada original.
- **WebGL (Chrome, build local `tools/build-webgl.sh`)**: carrega sem erros de console; abre a empresa, inicia o dia, clique em cliente na porta (combo x1), pula o dia e chega ao balanço, em canvas 1280×720 e 960×600 (`gameplay/webgl_*.jpg`). A sala inicial do jogo novo não tem as peças; por isso há um build isolado `NEKO_QA_FURNITURE` (`WebGLBuilder.BuildQaFurniture` → `Build/WebGL_QA_FURNITURE`, ignorado pelo git, define ausente do build normal), no padrão de `NEKO_QA_NINE`: novo jogo em memória com layout 13×9 contendo stage, koi_pond, janela, mesas, estações, e 6 maids-só-visuais paradas à frente / sobre / atrás das peças (fora do shift, sem tocar em regra, economia, RNG ou save; o `localStorage` é da origem local do teste). **Resultado:** `stage`, `koi_pond` e `window` ilustrados aparecem no Chrome, 1280×720 e 960×600 (`gameplay/webgl_pieces_*.jpg`), sem erros de console; a maid atrás da janela é encoberta pela peça, as maids à frente e "sobre" stage/lago são desenhadas por cima. Atrás de stage/lago não há oclusão visível (a arte só passa ~0,1 tile da pegada traseira), o que está coberto pelo teste de pixels.
- **Correção P2 (Codex):** o sorting passou a usar o pé desenhado (`MaidView.Feet`/`CustomerView.Feet`), não a raiz. `RenderedOcclusionTests` renderiza o frame com/sem uma `MaidView` real e compara pixels; falhou antes da correção e passa depois, estável em 2 execuções completas seguidas.
- **Proveniência:** SHA-256 dos 4 PNGs conferido (arquivo de trabalho = commit = `SUZU_ASSET_PROVENANCE.md`); hashes dos 3 brutos de origem registrados lá.
- **Não executado:** compra/colocação das peças pela UI da loja no WebGL (a fixture a substitui); Android (fora de escopo).

**Expansão feita:** os oito móveis funcionais (mesas, banqueta, estações) entraram em [ART-INT-02](../art-int-02/README.md), incluindo a camada da frente para clientes sentados.
