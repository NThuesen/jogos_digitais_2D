# 08_MAID_CARD_COMPACT — pronto para copiar, sem campos variáveis

> **ATUALIZAÇÃO 04/11/2026 — PROMPT HISTÓRICO.** O `UI-008_maid_card_compact_v01.png` já foi gerado e limpo localmente. QA Editor/WebGL na PR #44: **candidato visual em 150×206**, **REWORK em 0,62× (~93×128)**. **Não regenerar esta arte para resolver a Escalação:** a nova decisão de produto estabelece um frame simplificado separado. Use [**08_MAID_CARD_LINEUP_MINIMAL.md**](variants/08_MAID_CARD_LINEUP_MINIMAL.md), com anexos reais e prompt robusto. O PNG existente não está aprovado/integrado.


**Ainda não gerado/aprovado.** Saída: `UI-008_maid_card_compact_v01.png`.

## 1. Anexe ao ChatGPT estes arquivos EXATOS do repositório

- [`style_anchor_reference.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/art-01/D-pixel-cafe/reference/style_anchor_reference.png) — `docs/art/art-01/D-pixel-cafe/reference/style_anchor_reference.png` (**já existe no Git**).
- [`webgl_chrome_960x600.jpg`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/maid-walk/evidence_illustrated/webgl_chrome_960x600.jpg) — `docs/art/maid-walk/evidence_illustrated/webgl_chrome_960x600.jpg` (**já existe no Git**).
- [`after_ES-2_escalacao_960x600.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/roadmap/ui-01b/evidence/after_ES-2_escalacao_960x600.png) — `docs/roadmap/ui-01b/evidence/after_ES-2_escalacao_960x600.png` (**já existe no Git**).
- [`suzu_walk.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/Assets/_Game/Resources/Maids/suzu_walk.png) — `Assets/_Game/Resources/Maids/suzu_walk.png` (**já existe no Git**).

## 2. Anexe também, quando existir

- [`UI-001_panel_large_v01.png`](../approved/UI-001_panel_large_v01.png) — `docs/art/ui-modular/approved/UI-001_panel_large_v01.png` (**APPROVED, versionado no Git a partir do PR #39; referência oficial do painel**).

> Abra os links, baixe a imagem original e anexe as imagens no chat. Colar só a URL não equivale a anexar a referência. Os itens FUTUROS não são pré-requisitos existentes na main.

## 3. Prompt completo — copie o bloco inteiro

```text
PROJECT: Neko Maid Inc. Generate ONE ORIGINAL modular art sprite for cozy illustrated 2D anime café / ornamental visual novel management game. Use attachments to understand atmosphere and real Unity geometry, never trace or copy them. The ART-01D style anchor is historical PIXEL ART: retain its coziness, NOT its pixels. Maintain coherence with nine illustrated maids. No baked dynamic game text, prices, numbers, stats or fake UI; the exception is the static NEKO MAID INC. wordmark when explicitly requested. Unity controls clicks, responsive layout, localization and data. Output one asset only, no mockup board, watermark or checkerboard transparency. Use real PNG alpha outside when possible, or a uniform removable matte; do not erase the intended cream interior.

Create ONE isolated VERTICAL COMPACT MAID CARD FRAME, designed as an empty, modular collectible character card for Neko Maid Inc. Use the actual current Unity MaidCard base proportion 150 reference units wide x 206 high (ratio ~0.728), and ensure ornamentation survives reduction to 0.62x scale on the Lineup screen.

Architecture: a clearly designated upper portrait window occupying roughly the top 45–50%; an uncluttered band for a dynamic name below; restrained bottom safe region for two short stat rows, salary or rarity markers. The portrait area must be a truly transparent cut-out (so existing maid art can sit BEHIND this frame). The other information zones may use painted cream backing. Provide separate decorative border and fixed corners; no maid baked into frame.

Cozy illustrated anime visual-novel finish, subtle café/paw motif, rosewood + cream hypothesis, collectible charm, but minimal noisy detail. No rarity word, no role label, no stats, no portrait, no stickers crossing the photo window. Output only the empty frame.

ONE image only. The intended saved PNG filename is UI-008_maid_card_compact_v01.png. I understand that I may need to rename the downloaded file manually.
```

## 4. Destino com nome padronizado (não inventar outro)

- Gerado bruto: `art_working/ui_modular/raw_gpt/UI-008_maid_card_compact_v01.png`
- Corrigido no Blender, se necessário: `art_working/ui_modular/blender_clean/UI-008_maid_card_compact_v01.png`
- Aprovado, só após revisão: `art_working/ui_modular/approved/UI-008_maid_card_compact_v01.png`
- Nova tentativa: `UI-008_maid_card_compact_v02.png` (não substitua a original).

Imagem modular: as informações e interações da UI permanecem na Unity.

Veja [tutorial](../TUTORIAL.md) e [registro](../ASSET_REGISTRY.md). **O ChatGPT não necessariamente define o nome do download; renomeie manualmente.**
