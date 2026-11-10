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
- **WebGL (Chrome, build local `tools/build-webgl.sh`)**: carrega sem erros de console; abre a empresa, inicia o dia, clique em cliente na porta (combo x1), pula o dia e chega ao balanço, em canvas 1280×720 e 960×600 (`gameplay/webgl_*.jpg`). **Limite:** a sala inicial do jogo novo não contém stage/koi_pond/window (custam moedas/nível), então a arte ilustrada **não foi exibida no navegador**; ela foi verificada no Editor/PlayMode.
- **Proveniência:** SHA-256 dos 4 PNGs conferido (arquivo de trabalho = commit = `SUZU_ASSET_PROVENANCE.md`); hashes dos 3 brutos de origem registrados lá.
- **Não executado:** compra/colocação das peças pela UI da loja no WebGL; screenshots do jogador em Android (fora de escopo).
