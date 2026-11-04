# UI modular — aprovações do lote B3 (04/11/2026)

O responsável aprovou, por decisão expressa, **UI-011 (CAT_PAW, CLOCK, COIN, CUP, GEAR, GIFT, HEART, SHOP_BAG, STAR), UI-018 BALANCE ROW NORMAL e UI-020 LOGO WORDMARK**, com as restrições abaixo. **APPROVED não significa INTEGRATED**: nada foi integrado a telas, cenas ou importers, nenhuma imagem foi gerada e nenhuma build foi executada. **UI-017 permanece PENDING** (ver abaixo).

O PASS técnico é o QA Editor da [PR #50](https://github.com/NThuesen/jogos_digitais_2D/pull/50), **não repetido**: PlayMode batchmode, Unity 6000.5.9f1, 960×600 e 1280×720, pointer simulado. **WebGL: NOT RUN** (nenhum PASS de WebGL é declarado). Evidências: [relatório](../reviews/UI_BATCH_B3_2026_11_04.md), [capturas](../reviews/evidence/BATCH-B3-QA-2026-11-04/) e [decisão](../reviews/UI_B3_APPROVAL_2026_11_04.md). Hashes, proveniência RAW→CLEAN→export e configuração: [manifesto verificável](UI_MODULAR_B3_APPROVAL_MANIFEST.json). Masters são cópias byte a byte dos CLEANs; exports, byte a byte dos exports testados. RAWs e arquivos anteriores preservados.

| Master aprovado | Master | Export testado | Export |
| --- | --- | --- | --- |
| [CAT_PAW](UI-011_icon_cat_paw_v01.png) | 1254×1254 | [export](exports/UI-011_icon_cat_paw_v01.png) | 512×512 |
| [CLOCK](UI-011_icon_clock_v01.png) | 1254×1254 | [export](exports/UI-011_icon_clock_v01.png) | 512×512 |
| [COIN](UI-011_icon_coin_v01.png) | 1254×1254 | [export](exports/UI-011_icon_coin_v01.png) | 512×512 |
| [CUP](UI-011_icon_cup_v01.png) | 1254×1254 | [export](exports/UI-011_icon_cup_v01.png) | 512×512 |
| [GEAR](UI-011_icon_gear_v01.png) | 1254×1254 | [export](exports/UI-011_icon_gear_v01.png) | 512×512 |
| [GIFT](UI-011_icon_gift_v01.png) | 1254×1254 | [export](exports/UI-011_icon_gift_v01.png) | 512×512 |
| [HEART](UI-011_icon_heart_v01.png) | 1261×1247 | [export](exports/UI-011_icon_heart_v01.png) | 512×512 (padding quadrado) |
| [SHOP_BAG](UI-011_icon_shop_bag_v01.png) | 1254×1254 | [export](exports/UI-011_icon_shop_bag_v01.png) | 512×512 |
| [STAR](UI-011_icon_star_v01.png) | 1254×1254 | [export](exports/UI-011_icon_star_v01.png) | 512×512 |
| [UI-018 BALANCE ROW NORMAL](UI-018_balance_row_normal_v01.png) | 2172×724 | [export](exports/UI-018_balance_row_normal_v01.png) | 1082×148 |
| [UI-020 LOGO WORDMARK](UI-020_logo_wordmark_v01.png) | 1774×887 | [export](exports/UI-020_logo_wordmark_v01.png) | 866×422 |

## Restrições obrigatórias
- **CLOCK e GEAR:** preferir 32–44u; detalhes internos a 24u **não homologados**.
- **CUP:** vapor não precisa estar visível a 24u.
- **COIN:** o grão de café gravado **é** a moeda do jogo (aprovado).
- **STAR:** tonalidade mais viva aceita.
- **HEART:** preservar padding quadrado no export (master 1261×1247).
- **Ícones:** exports 512×512 sem recorte individual (escala consistente da família); testados a 24/32/44u, Simple/preserveAspect.
- **LOGO:** aprovado nos tamanhos e fundo testados (520u e 200u sobre UI-012 cover); **outros contextos exigem validação futura**.
- **UI-018:** somente **NORMAL** com a cápsula fixa de valor à direita; testado em 380×46, 520×54 e 300×40. Nenhuma outra variante (p.ex. TOTAL) ou tamanho é inferido.

## UI-017 — PENDING (recomendar v02)
Condição do responsável: aprovar só se as linhas da Loja **sempre** tiverem ícone. Inspeção do uso real (`Assets/_Game/Scripts/Game/Ui/ShopScreen.cs`, sem alterá-lo): linhas 850×62; o ícone só existe para `ShopItemKind.Placeable`; **Equipamento (roupas) e Presente não têm ícone** (`indent = 0`), e o nome começa a ≈185u da borda esquerda, deixando a reserva quadrada pintada vazia. A linha real também recebe tinta por estado (acessível/inacessível). **Condição não satisfeita ⇒ PENDING.** Recomendação v02: moldura sem reserva nem divisor pintados (ícone como filho Unity opcional) ou duas variantes (com/sem ícone); sem recorte fixo. Detalhes em [decisão](../reviews/UI_B3_APPROVAL_2026_11_04.md).

## Configuração Unity já validada (somente exports)
Sprite Single, Full Rect, PPU 100, Bilinear, Clamp, sem mipmaps, Uncompressed, alphaIsTransparency, NPOT None, maxTextureSize 4096, Read/Write desativado ([setup arquivado](../reviews/evidence/BATCH-B3-QA-2026-11-04/harness/B3QaSetup.cs.txt)). UI-018: Sliced, borders L/B/R/T 85/45/85/45 px, multiplier = 148/altura renderizada. Ícones/logo: Simple, preserveAspect. Não aplicar esses valores aos masters. Não validados: WebGL, GPU/mouse físicos, outros navegadores, compressão de bloco, memória, localização longa, telas completas, estados/variantes além do NORMAL, integração.
