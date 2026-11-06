# Backlog de arte (estado após a rodada noturna)

## Maids: onde cada uma está

| # | Maid | Retrato | Caminhada | No jogo |
|---|---|---|---|---|
| 01–09 | Suzu, Hina, Akari, Momo, Reina, Kaede, Nene, Ichika, Yoru | aprovado | aprovada | sim |
| 10–23 | Mikan, Yuki, Kurumi, Nonoka (v02), Kaguya, Mizuki, Himari, Anzu, Kasumi, Hibari, Seira, Ruri, Chiyo, Shiori | aprovado/em uso | processada (ver `maid-walk/BATCH_10_19_STAGED.md` e manifestos) | **sim, liberadas** |
| 24 | Tsubaki | pronto (lote final) | **falta** exportar frames do Blender em `art_working/24_tsubaki/…/frames/` | não (Planned) |
| 25 | Mirai | pronto | **falta** | não (Planned) |
| 26 | Kinako | pronto | **falta** | não (Planned) |
| 27 | Mayu | pronto | **falta** | não (Planned) |

Para liberar as quatro que faltam: exportar os frames, `python tools/art/maid_walk/maid_walk.py process --only tsubaki,mirai,kinako,mayu`, trocar `Planned` por `Out` no `MaidCatalog`, copiar `*_head/_detail` dos derivados de `docs/art/ui-modular/portraits/batch3/derived/` para `Resources/MaidPortraits`, e atualizar a allowlist/proveniência (`DocsTests` e `SUZU_ASSET_PROVENANCE.md`) como na PR da rodada noturna.

## Procedural que ainda aparece no jogo (prioridade)

1. **Clientes** (`CustomerView`): figuras procedurais ao lado de maids ilustradas. Prompts: `ui-modular/prompts/26_CUSTOMER_CHIBIS.md` (7 tipos).
2. **Pratos e bebidas** (`Recipe.ShapeId`/`ColorHex`): formas geométricas nos balões e bandejas. Prompts: `ui-modular/prompts/27_DISH_ICONS.md` (14 receitas).
3. **Salão e móveis** (`RoomView`, `SpriteFactory.Furniture`): pixel/procedural contra maids ilustradas. Prompts já existem e esperam geração: `13_RESTAURANT_BACKGROUND`, `14_RESTAURANT_PROPS_SET`, `22_FLOOR_TILE`, `23_WALL_SEGMENT`, `24_FURNITURE_SINGLE` (23 móveis em `variants/`).
4. **Equipamento visível** nas maids ilustradas: hoje só aparece na UI (regra do catálogo); overlays por peça exigem arte nova (não há prompt ainda; depende de decidir o método: camadas sobre a sheet ou sheets por conjunto).
5. **HUD/UI restantes** (`15_TOP_BAR_HUD`, `16_BOTTOM_STRIP_DAY_UI`, `17_SHOP_ITEM_FRAME`, `18_BALANCE_ROW_FRAME`, `19_DIALOG_BALLOON`, `25_RARITY_CARD_FRAME`): prompts existem; ver `ui-modular/OUTPUT_MANIFEST.md` para o que já foi aprovado/integrado.

## Qualidade das caminhadas (restrições conhecidas)

- **Mizuki:** passos curtos para a distância (deslize 5,1, acima das originais). Se patinar no jogo, refazer com passos mais largos.
- **Kasumi:** só 17 frames úteis (trecho 67–83): movimento mais curto.
- **Himari, Chiyo, Shiori:** o export trouxe matte translúcido; resolvido com `matte_cut` por maid em `tools/art/maid_walk/maids.json`. Nos próximos exports: Film → Transparent, sem sky/world.
- **Kaguya:** refeita; ciclo fecha (1,15) e perfil lateral constante.

## Custos
- 23 sheets de caminhada somam ~29 Mpx (eram ~10 Mpx para nove); só é carregada a sheet das maids na tela. O build WebGL guarda as texturas sem compressão; se o tamanho/memória incomodarem, o primeiro ajuste é comprimir as sheets (DXT5) ou reduzir `height_px` por maid.
