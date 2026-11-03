# 22_FLOOR_TILE — pronto para copiar, sem campos variáveis

**Ainda não gerado/aprovado.** Saída: `UI-022_floor_tile_warm_oak_v01.png`.

## 1. Anexe ao ChatGPT estes arquivos EXATOS do repositório

- [`style_anchor_reference.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/art-01/D-pixel-cafe/reference/style_anchor_reference.png) — `docs/art/art-01/D-pixel-cafe/reference/style_anchor_reference.png` (**já existe no Git**).
- [`after_SL-1_salao_960x600.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/roadmap/ui-01b/evidence/after_SL-1_salao_960x600.png) — `docs/roadmap/ui-01b/evidence/after_SL-1_salao_960x600.png` (**já existe no Git**).
- [`webgl_chrome_960x600.jpg`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/maid-walk/evidence_illustrated/webgl_chrome_960x600.jpg) — `docs/art/maid-walk/evidence_illustrated/webgl_chrome_960x600.jpg` (**já existe no Git**).

> Abra os links, baixe a imagem original e anexe as imagens no chat. Colar só a URL não equivale a anexar a referência. Os itens FUTUROS não são pré-requisitos existentes na main.

## 2. Prompt completo — copie o bloco inteiro

```text
PROJECT: Neko Maid Inc. Generate ONE ORIGINAL modular art sprite for cozy illustrated 2D anime café / ornamental visual novel management game. Use attachments to understand atmosphere and real Unity geometry, never trace or copy them. The ART-01D style anchor is historical PIXEL ART: retain its coziness, NOT its pixels. Maintain coherence with nine illustrated maids. No baked dynamic game text, prices, numbers, stats or fake UI; the exception is the static NEKO MAID INC. wordmark when explicitly requested. Unity controls clicks, responsive layout, localization and data. Output one asset only, no mockup board, watermark or checkerboard transparency. Use real PNG alpha outside when possible, or a uniform removable matte; do not erase the intended cream interior.

Generate ONE SQUARE SEAMLESS TOP-DOWN FLOOR TILE texture for an interactive orthographic 2D anime café game, material warm oak, not a scene illustration.

The image must repeat on ALL four sides seamlessly when Unity places dozens of identical tiles in a room that can change size. Soft warm painted wood/cream finish with subtle grain and enough local variation to avoid dullness, but VERY low texture contrast and no baked tables, objects, stains, arrows, UI or character shadows. No large directional sunlight gradient across a single tile. Each edge must join the opposite edge naturally and the floor perspective must be compatible with the attached actual game camera.

Export a single perfectly square tile, no perspective frame or border, no fake transparency.

ONE image only. The intended saved PNG filename is UI-022_floor_tile_warm_oak_v01.png. I understand that I may need to rename the downloaded file manually.
```

## 3. Destino com nome padronizado (não inventar outro)

- Gerado bruto: `art_working/ui_modular/raw_gpt/UI-022_floor_tile_warm_oak_v01.png`
- Corrigido no Blender, se necessário: `art_working/ui_modular/blender_clean/UI-022_floor_tile_warm_oak_v01.png`
- Aprovado, só após revisão: `art_working/ui_modular/approved/UI-022_floor_tile_warm_oak_v01.png`
- Nova tentativa: `UI-022_floor_tile_warm_oak_v02.png` (não substitua a original).

Piso de carvalho é hipótese de estilo, não aprovação final. Antes de integrar, testar tileability em mosaico 3×3.

Veja [tutorial](../TUTORIAL.md) e [registro](../ASSET_REGISTRY.md). **O ChatGPT não necessariamente define o nome do download; renomeie manualmente.**
