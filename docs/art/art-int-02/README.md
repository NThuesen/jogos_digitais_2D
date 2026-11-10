# ART-INT-02 — integração dos oito móveis funcionais

Fonte: entrega `art-prod-02c-delivery/` (CLEAN c01, aceitos artisticamente pelo responsável; **sem TECH_PASS**). Base: `main` com a infraestrutura de sorting da PR #76. Nenhuma mudança em simulação, footprints, economia, saves, contratação, progressão ou regras de assentos.

## O que entrou

| Peça | Footprint | Recurso (`Resources/Furniture/`) | Largura (tiles) | Camadas | Assentos |
|---|---|---|---|---|---|
| `table_2` v02 | 2×1 | `table_2` + `table_2_front` | 2,00 | inteira atrás, tampo+pernas à frente | 2 clientes atrás do tampo, na cadeira |
| `table_2` girada v02 | 1×2 | `table_2_rotated` (arte própria, sem girar textura) | 1,50 | uma | 2 clientes nas cadeiras da esquerda |
| `table_4` v03 | 2×2 | `table_4` + `table_4_front` | 2,00 | inteira atrás; tampo + cadeiras da frente à frente | fileira de trás atrás do tampo; fileira da frente sobre a cadeira |
| `stool` v01 | 1×1 | `stool` | 0,60 | uma | cliente sentado sobre o assento |
| `stove` v01 | 1×1 | `stove` | 0,90 | uma | — |
| `teabar` v01 | 2×1 | `teabar` | 2,00 | uma | — |
| `dessert` v01 | 1×1 | `dessert` | 0,95 | uma | — |
| `bakery` v01 | 1×2 | `bakery` | 0,95 | uma | — |

Pivô de todos: embaixo-centro, apoiado na borda frontal da pegada + `BottomLift`; 256 px por tile; PNGs gerados por `tools/art/furniture_functional/make_functional_assets.py` (confere o SHA-256 de cada origem contra o `INVENTORY.json`; recorte à área ilustrada; reamostragem com alpha pré-multiplicado). Proveniência e hashes: `docs/art/SUZU_ASSET_PROVENANCE.md`.

## O problema de sorting (e a solução mínima)

`DepthSort.ActorKey` põe quem está **dentro** da pegada logo à frente da peça inteira. Com a arte nova isso fazia todo cliente sentado ficar por cima do tampo (frame "antes" da investigação: clientes "em pé" sobre a mesa). O esquema `logical_seats.png` só mostra as células; o alinhamento visual só apareceu nos frames do Unity.

Solução, só de apresentação (a lógica do jogo não muda):
1. **Camada da frente** opcional (`IllustratedFurniture.FrontResource`): o PNG com só o que fica à frente de quem está sentado (tampo + pernas; na mesa de 4 também as cadeiras da frente). A imagem inteira vai atrás (`BackKey = borda frontal + 0,6`), a camada da frente na chave sólida de sempre (`SolidKey`).
2. **`DepthSort.SeatedKey`**: cliente sentado é chaveado pela **célula** do assento (nunca por onde é desenhado): atrás da camada da frente e à frente da imagem inteira; as fileiras `FrontRows` (cadeiras da frente da mesa de 4) ficam à frente. Sem entrada ou sem camada da frente: regra antiga, idêntica.
3. **`SeatOffset`** por assento: o cliente é *desenhado* na cadeira da arte (a célula do sim não muda). `ShiftPresenter.PositionOf` aplica; `CustomerView.SeatCell` leva a célula até o sorting.
4. Maids e qualquer um fora de um assento: regra anterior, inalterada (maid em estação/atravessando mesa continua desenhada à frente da peça).

Pixels das camadas da frente: nada é desenhado, só tornado transparente (`above_y` para a mesa de 2; dois polígonos sobre as cadeiras de trás da mesa de 4 — conferido por pré-visualização).

## Validação

Detalhes e contagens no corpo da PR. Evidências em `evidence/`:
- `before_after_*`: procedural × ilustrado, mesma sala, mesmo sorting novo, 960×600 e 1280×720, ocupada e vazia (fixture isolada `FunctionalFurnitureQaTests`: `CafeGrid` em memória, clientes e maids reais).
- `ingame_*`: cena Boot real, dia real (`FunctionalFurnitureInGameTests`), com as oito peças; mesa de 4 e banquetas em layouts próprios para o simulador sentar gente ali.

## Limitações conhecidas
- `table_4`: perspectiva diagonal do tampo sobre a grade ortogonal; os dois clientes da fileira da frente aparecem **sobre** a cadeira (as cadeiras da frente são desenhadas de costas, os clientes olham de frente). Aceitável, não perfeito.
- `table_2` girada: as cadeiras da arte distam ~0,72 tile e os assentos 1,0; o cliente de trás é rebaixado 0,3 para cair na cadeira.
- Os clientes procedurais são pequenos perto desta arte (tampo à altura dos ombros); `SeatOffset` compensa, a escala do cliente não foi tocada.
- Estações: a maid no ponto de operação fica à frente da peça inteira (regra existente), então esconde parte do fogão/forno.
- Panela do fogão e pães do forno são decoração permanente, não indicador de preparo (nota do REPORT 02C).
- Variantes giradas de `teabar`, `stove`, `dessert`, `bakery` (footprint diferente) continuam **procedurais** por desenho; só a mesa de 2 tem arte girada.
- Cliente-gato (quadrúpede) usa a mesma posição de assento; não revisado.
