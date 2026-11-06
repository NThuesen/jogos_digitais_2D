# Backlog de arte (estado após a rodada noturna)

## Maids: onde cada uma está

| # | Maid | Retrato | Caminhada | No jogo |
|---|---|---|---|---|
| 01–09 | Suzu, Hina, Akari, Momo, Reina, Kaede, Nene, Ichika, Yoru | aprovado | aprovada | sim |
| 10–23 | Mikan, Yuki, Kurumi, Nonoka (v02), Kaguya, Mizuki, Himari, Anzu, Kasumi, Hibari, Seira, Ruri, Chiyo, Shiori | aprovado/em uso | processada (ver `maid-walk/BATCH_10_19_STAGED.md` e manifestos) | **sim, liberadas** |
| 24–27 | Tsubaki, Mirai, Kinako, Mayu | aprovado (lote 3) | processada (lote final; ver `maid-walk/BATCH_10_19_STAGED.md`) | **sim, liberadas: 27/27** |

Catálogo completo: 27 identidades autorais liberadas, nenhuma Planned. Para uma maid futura: reservar com `Planned`, exportar frames, `process --only <id>`, copiar `*_head/_detail`, `Out(...)`, atualizar allowlist/proveniência.

## Procedural que ainda aparece no jogo (prioridade)

1. **Clientes** (`CustomerView`): figuras procedurais, um sprite estático por tipo, sem estado sentado e sem caminhada. **Escopo decidido: 8 clientes chibi** (7 tipos + 1 variante de Regular), cada um com design base + sheet de caminhada + sprite sentado. Auditoria e plano: [`CUSTOMERS_DISHES_AUDIT.md`](CUSTOMERS_DISHES_AUDIT.md). Prompts: `ui-modular/prompts/26_CUSTOMER_CHIBIS.md`.
2. **Pratos e bebidas** (`Recipe.ShapeId`/`TintHex`): 14 receitas, formas geométricas só no balão de pedido (único uso atual). **Escopo decidido: 1 ícone ilustrado por receita**, estático, transparente, derivado a 128×128. Ver a auditoria acima. Prompts: `ui-modular/prompts/27_DISH_ICONS.md`.
3. **Salão e móveis** (`RoomView`, `SpriteFactory.Furniture`): pixel/procedural contra maids ilustradas. Prompts já existem e esperam geração: `13_RESTAURANT_BACKGROUND`, `14_RESTAURANT_PROPS_SET`, `22_FLOOR_TILE`, `23_WALL_SEGMENT`, `24_FURNITURE_SINGLE` (23 móveis em `variants/`).
4. **Equipamento visível** nas maids ilustradas: hoje só aparece na UI (regra do catálogo); overlays por peça exigem arte nova (não há prompt ainda; depende de decidir o método: camadas sobre a sheet ou sheets por conjunto).
5. **HUD/UI restantes** (`15_TOP_BAR_HUD`, `16_BOTTOM_STRIP_DAY_UI`, `17_SHOP_ITEM_FRAME`, `18_BALANCE_ROW_FRAME`, `19_DIALOG_BALLOON`, `25_RARITY_CARD_FRAME`): prompts existem; ver `ui-modular/OUTPUT_MANIFEST.md` para o que já foi aprovado/integrado.

## Qualidade das caminhadas (restrições conhecidas)

- **Mizuki (aceita provisoriamente pelo dono; não regenerar agora):** passos curtos para a distância (deslize 5,1, acima das originais). Se patinar no jogo, refazer com passos mais largos.
- **Kasumi (aceita provisoriamente; não regenerar agora):** só 17 frames úteis (trecho 67–83): movimento mais curto.
- **Himari, Chiyo, Shiori:** o export trouxe matte translúcido; resolvido com `matte_cut` por maid em `tools/art/maid_walk/maids.json`. Nos próximos exports: Film → Transparent, sem sky/world.
- **Kaguya:** refeita; ciclo fecha (1,15) e perfil lateral constante.

## Custos e pendências técnicas
- **Otimização de texturas WebGL (pendente):** o build subiu de ~36 MB para 103,5 MB (data 97 MB) porque as 23 sheets vão em RGBA32 com mipmaps sem compressão (~224 MB descomprimidas se todas residentes; a vitrine F8 é o pior caso). Pico de GPU não medido. Antes de publicar: avaliar DXT5/ASTC por sheet, desligar mipmaps nas sheets, reduzir `height_px` ou descarregar sheets fora de uso; medir no navegador.
- **Elenco 3/2 (preexistente):** a partida nova começa com 3 maids e o Elenco mostra "3 / 2" (teto do café nível 1 = 2). Não introduzido pela PR #66; decidir se o time inicial ou o teto muda, sem alterar a economia sem a regra do GDD.
- 23 sheets de caminhada somam ~29 Mpx (eram ~10 Mpx para nove); só é carregada a sheet das maids na tela. O build WebGL guarda as texturas sem compressão; se o tamanho/memória incomodarem, o primeiro ajuste é comprimir as sheets (DXT5) ou reduzir `height_px` por maid.
