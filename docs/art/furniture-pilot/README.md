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

## Validação
EditMode 379/379, PlayMode 104/104 (Unity 6000.5.9f1, batchmode, worktree `art-int-01`). **Não executado:** smoke WebGL no Chrome, teste de cliques/HUD em partida real, capturas de partida jogada (as capturas são de uma sala montada pela sonda). Hitboxes não foram tocadas.
