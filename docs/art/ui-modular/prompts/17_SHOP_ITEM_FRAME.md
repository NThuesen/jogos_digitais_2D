# 17_SHOP_ITEM_FRAME — prompt v02 (moldura universal, sem reserva de ícone)

**Próxima geração: `UI-017_shop_item_frame_v02.png` — PENDING, ainda não gerado.** `UI-017_shop_item_frame_v01.png` permanece **PENDING/REWORK para uso universal** (PR #50): tem **reserva quadrada pintada para ícone, divisor com losangos e padrão de losangos** no miolo. A Loja real (`ShopScreen.cs`) só mostra ícone na aba **Móveis**; **Roupas** e **Presentes** não têm ícone, então a reserva ficaria vazia e o texto não pode ocupá-la. A v02 é uma moldura **única para as três abas**. Não sobrescreva nem renomeie a v01; o histórico do prompt v01 está preservado no fim deste arquivo.

## 1. Anexe estes arquivos reais ao GPT Web

**Obrigatórios** (baixe o PNG original ou anexe o arquivo local; links não substituem anexos):

1. **`UI-001_panel_large_v01.png`** — [PNG aprovado real](../approved/UI-001_panel_large_v01.png), `docs/art/ui-modular/approved/UI-001_panel_large_v01.png`. Referência autoritativa de material, creme, latão e acabamento.
2. **`UI-017_shop_item_frame_v01.png`** — RAW local `art_working/ui_modular/raw_gpt/UI-017_shop_item_frame_v01.png` (**LOCAL; PENDING/REWORK; não versionado no Git**). O CLEAN equivalente está publicado na [PR #50](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/ui-modular/incoming/BATCH-2026-11-04-B3/UI-017_shop_item_frame_v01.png) (`docs/art/ui-modular/incoming/BATCH-2026-11-04-B3/UI-017_shop_item_frame_v01.png`, agora na `main`). Use **somente** para continuidade de estilo (rosewood, bordas, patas/folhas de canto) e como **NEGATIVE REFERENCE** de reserva de ícone, divisor e losangos: **não copie esses elementos**.
3. **`after_LJ-1_loja-moveis_960x600.png`** — [captura real](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/roadmap/ui-01a/evidence/after_LJ-1_loja-moveis_960x600.png), `docs/roadmap/ui-01a/evidence/after_LJ-1_loja-moveis_960x600.png`. Aba **com** ícone; proporção e posição reais das linhas.
4. **`after_LJ-3_loja-presentes_960x600.png`** — [captura real](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/roadmap/ui-01a/evidence/after_LJ-3_loja-presentes_960x600.png), `docs/roadmap/ui-01a/evidence/after_LJ-3_loja-presentes_960x600.png`. Aba **sem** ícone: o texto começa na esquerda. Layout apenas, nunca copiar a tela.

**Opcional:** [`style_anchor_reference.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/art-01/D-pixel-cafe/reference/style_anchor_reference.png) — `docs/art/art-01/D-pixel-cafe/reference/style_anchor_reference.png` (**atmosfera histórica em pixel art; não gerar pixel art**).

Se o RAW v01 local não estiver acessível, anexe o CLEAN da PR #50; se nenhum estiver, use UI-001 e as duas capturas — **não invente arquivo ou anexo futuro**.

## 2. Prompt completo — copie somente este bloco

```text
PROJECT: Neko Maid Inc. Generate exactly ONE original modular SHOP ROW FRAME sprite, version v02, for a cozy illustrated 2D anime café / ornamental visual-novel management game. Use attachments to understand style and real Unity geometry; never trace or copy screenshots.

IMAGE REFERENCE PRIORITY:
- UI-001_panel_large_v01.png (APPROVED): authoritative warm ivory/cream, restrained brass, rosewood/terracotta surface finish and border quality.
- UI-017_shop_item_frame_v01.png (PENDING/REWORK): keep its delicate rosewood border, brass inner line and small paw/leaf corner ornaments as the STYLE FAMILY ONLY. It is a NEGATIVE REFERENCE for three defects that must NOT be repeated: (1) the painted square icon reserve on the left, (2) the vertical divider with diamond ends, (3) the repeating diamond/harlequin pattern in the cream area.
- after_LJ-1_loja-moveis_960x600.png (row WITH an icon) and after_LJ-3_loja-presentes_960x600.png (row WITHOUT an icon): real Unity layout references, NOT content to copy.
Optional style_anchor_reference.png is historical pixel-art mood only: do not make pixel art.

PURPOSE: ONE universal frame used behind rows in all three Shop tabs: Furniture (rows have an icon), Clothes and Gifts (rows have NO icon). The frame must therefore work equally well with an optional icon placed by Unity on the left, or with the text expanded across the full width when there is no icon.

COMPOSITION: ONE isolated, front-facing, very wide and shallow horizontal row frame. Target visible proportion about 13.7:1 (the real row is 850 x 62 Unity reference units; it must also work at 560 x 62). Make the visible frame as shallow and wide as the canvas allows, at least 10:1; if the generator uses a wider canvas, leave the extra height as fully transparent margin above and below, keep the frame perfectly horizontal, and do not scale or distort the frame to fill the canvas. Leave at least ~3% transparent margin on every side; no ornament may touch or cross the canvas edge.

CREAM AREA (CRITICAL): one single CONTINUOUS, CLEAN, OPAQUE warm-cream band running from the left inner border to the right inner border with constant height. It must be completely empty: NO painted square or rounded-rectangle reserve for an icon, NO inset slot, NO vertical or horizontal divider lines, NO diamonds, sparkles, dots, stars, harlequin/checker pattern, vignette or large gradient, and no darker or lighter patch anywhere in the middle. Only an extremely subtle, uniform paper grain is allowed. Unity decides whether an icon, name, description, owned count and Buy button appear, and where.

BORDER AND 9-SLICE: a thin rosewood outer border with one brass inner hairline, straight and uniform along the entire length. All ornament (small paw prints and leaves) is confined to the four corner zones, each at most about one frame-height wide, so a 9-slice with fixed corners and a horizontally stretched middle never distorts any ornament. The middle section between the corner zones must be horizontally uniform: a repeatable straight border and plain cream with no motif that would stretch. The inner edge of the cream band must be a plain straight line with small simple rounded corners; NO scalloped, notched or lobed inner outline, no center ornament and no cut-out. Left and right ends are symmetrical and visually identical, so the row also works mirrored.

RIGHT SIDE: keep the right end calm, with the same border as the left: nothing painted where Unity's real Buy button will overlay. No button, plank, capsule or chip is drawn.

TRANSPARENCY: real PNG alpha outside the whole silhouette; cream interior fully opaque; no matte, checkerboard, drop-shadow backdrop or halo fringe on the outer edge.

UNITY OWNS: item icon, name, description, owned quantity, price, Buy button, hover/disabled state, tint, localization and every interaction.

STRICT NEGATIVES: no text, letters, numbers, prices, currency, icons, item art, buttons, badges, labels, characters, mockup board, screenshot, contact sheet, watermark, variants or second image. No pixel art.

OUTPUT EXACTLY ONE isolated EMPTY shop row frame, completely visible in its canvas. Intended filename: UI-017_shop_item_frame_v02.png.
```

## 3. Destino, status e critérios de QA

- RAW novo: `art_working/ui_modular/raw_gpt/UI-017_shop_item_frame_v02.png`.
- CLEAN após auditoria: `art_working/ui_modular/blender_clean/UI-017_shop_item_frame_v02.png`.
- APPROVED somente após revisão expressa do responsável: `art_working/ui_modular/approved/UI-017_shop_item_frame_v02.png`. **Nenhuma aprovação automática; nenhuma integração.** Preservar a v01 e seus arquivos.

**QA antes de aceitar a v02 (reutilizar o harness do lote B3, sem WebGL):**
1. **Alpha/geometria:** zero pixels opacos nas bordas do canvas, margens transparentes ≥3%, poeira alpha 1–3 removida só em CLEAN conservador, bbox e proporção visível medidos (alvo ≈13,7:1; aceitável ≥10:1).
2. **Área creme contínua:** inspeção e perfil de colunas do miolo (corte central de ≥30% da largura): sem reserva, divisor, losangos ou variação de tom; variação horizontal desprezível.
3. **9-slice:** ornamentos só nos cantos (≤ ~1 altura de largura); Sliced em **850×62 e 560×62** (e uma largura estreita como 380×62), sem distorção visível de pata/folha, sem borda quebrada; borders e multiplier registrados.
4. **Três abas simuladas:** linha **com ícone** (Unity, opcional, à esquerda), linha **sem ícone** com o texto expandido desde a esquerda, nome/descrição longos, contagem "possuídos" e botão Comprar nativo (150×44) à direita, estado habilitado e desabilitado; texto sem colisão com a borda; 960×600 e 1280×720; raycast/clique do botão.
5. **Limites:** estado de tinta/hover da linha real não homologado pela arte; WebGL, GPU/mouse físicos e integração permanecem **NOT RUN** até tarefa própria.

[Tutorial](../TUTORIAL.md) · [registro](../ASSET_REGISTRY.md) · [PR #50 (v01 e decisão)](https://github.com/NThuesen/jogos_digitais_2D/pull/50). **O ChatGPT não necessariamente define o nome do download; renomeie manualmente.**

---

## Histórico — prompt v01 (preservado, não use para a v02)

`UI-017_shop_item_frame_v01.png` foi gerado com o bloco abaixo e permanece PENDING/REWORK para uso universal (reserva de ícone e divisor pintados).

```text
PROJECT: Neko Maid Inc. Generate ONE ORIGINAL modular art sprite for cozy illustrated 2D anime café / ornamental visual novel management game. Use attachments to understand atmosphere and real Unity geometry, never trace or copy them. The ART-01D style anchor is historical PIXEL ART: retain its coziness, NOT its pixels. Maintain coherence with nine illustrated maids. No baked dynamic game text, prices, numbers, stats or fake UI; the exception is the static NEKO MAID INC. wordmark when explicitly requested. Unity controls clicks, responsive layout, localization and data. Output one asset only, no mockup board, watermark or checkerboard transparency. Use real PNG alpha outside when possible, or a uniform removable matte; do not erase the intended cream interior.

Design ONE very wide and shallow SHOP ROW FRAME, matching the approximate real Unity row 850x62 reference units in ratio. It is an undecorated background for a scrolling list of furniture, outfits or gifts — no item image or button drawn into it.

Provide a small quiet visual reserve on the LEFT for a separately positioned item icon; a broad cream content band in the middle for localized name/description and owned count; open clean RIGHT edge where Unity's real Buy button will overlay. Use delicate border and optional hairline divider, but no fixed-size cut-out that prevents variations.

Reusable 9-slice-friendly smooth corners/straight edges; subtle painted details, little ornament, alpha outside. Do not draw labels, numbers, currency or item art.

ONE image only. The intended saved PNG filename is UI-017_shop_item_frame_v01.png. I understand that I may need to rename the downloaded file manually.
```

Anexos originais do v01: `style_anchor_reference.png`, `after_LJ-1_loja-moveis_960x600.png`, `D4_ui_kit.png` (histórico pixel-art) e, quando existir, `UI-001_panel_large_v01.png`.
