# 15_TOP_BAR_HUD — pronto para copiar, sem campos variáveis

**Ainda não gerado/aprovado.** Saída: `UI-015_top_bar_hud_v01.png`.

## 1. Anexe ao ChatGPT estes arquivos EXATOS do repositório

- [`style_anchor_reference.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/art-01/D-pixel-cafe/reference/style_anchor_reference.png) — `docs/art/art-01/D-pixel-cafe/reference/style_anchor_reference.png` (**já existe no Git**).
- [`after_OD-1_o-dia_960x600.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/roadmap/ui-01b/evidence/after_OD-1_o-dia_960x600.png) — `docs/roadmap/ui-01b/evidence/after_OD-1_o-dia_960x600.png` (**já existe no Git**).
- [`webgl_chrome_960x600.jpg`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/maid-walk/evidence_illustrated/webgl_chrome_960x600.jpg) — `docs/art/maid-walk/evidence_illustrated/webgl_chrome_960x600.jpg` (**já existe no Git**).

## 2. Anexe também, quando existir

- [`UI-001_panel_large_v01.png`](../approved/UI-001_panel_large_v01.png) — `docs/art/ui-modular/approved/UI-001_panel_large_v01.png` (**APPROVED, versionado no Git a partir do PR #39; referência oficial do painel**).

> Abra os links, baixe a imagem original e anexe as imagens no chat. Colar só a URL não equivale a anexar a referência. Os itens FUTUROS não são pré-requisitos existentes na main.

## 3. Prompt completo — copie o bloco inteiro

```text
PROJECT: Neko Maid Inc. Generate ONE ORIGINAL modular art sprite for cozy illustrated 2D anime café / ornamental visual novel management game. Use attachments to understand atmosphere and real Unity geometry, never trace or copy them. The ART-01D style anchor is historical PIXEL ART: retain its coziness, NOT its pixels. Maintain coherence with nine illustrated maids. No baked dynamic game text, prices, numbers, stats or fake UI; the exception is the static NEKO MAID INC. wordmark when explicitly requested. Unity controls clicks, responsive layout, localization and data. Output one asset only, no mockup board, watermark or checkerboard transparency. Use real PNG alpha outside when possible, or a uniform removable matte; do not erase the intended cream interior.

Create ONE long, thin, reusable TOP SHIFT HUD PANEL graphic, matching the current 760x76 Unity reference-unit placement. Front-on horizontal illustrated frame with a restrained cream interior, thin rounded rosewood trim and subtle café ornaments at outer ends only. It is a visual backing; Unity renders the clock, till, numbers, counters, buttons and speed states.

Keep the central 85% almost unornamented; leave clear grouping space without drawing fixed clock or currency icons. Side ends should not clip at 960x600. Shape may use 9-slice only if corners and straight bands are consistent; avoid baked slots that lock a particular number of controls.

No numbers, names, fake UI, visible buttons or stats. True alpha around panel.

ONE image only. The intended saved PNG filename is UI-015_top_bar_hud_v01.png. I understand that I may need to rename the downloaded file manually.
```

## 4. Destino com nome padronizado (não inventar outro)

- Gerado bruto: `art_working/ui_modular/raw_gpt/UI-015_top_bar_hud_v01.png`
- Corrigido no Blender, se necessário: `art_working/ui_modular/blender_clean/UI-015_top_bar_hud_v01.png`
- Aprovado, só após revisão: `art_working/ui_modular/approved/UI-015_top_bar_hud_v01.png`
- Nova tentativa: `UI-015_top_bar_hud_v02.png` (não substitua a original).

Imagem modular: as informações e interações da UI permanecem na Unity.

Veja [tutorial](../TUTORIAL.md) e [registro](../ASSET_REGISTRY.md). **O ChatGPT não necessariamente define o nome do download; renomeie manualmente.**
