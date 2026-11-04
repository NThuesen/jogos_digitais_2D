# UI-008 — Maid Card simplificado para Escalação (LINEUP MINIMAL)

**Decisão de produto aprovada em 04/11/2026:** duas apresentações do mesmo personagem. O `UI-008_maid_card_compact_v01.png` já gerado é **aprovado expressamente apenas para apresentação detalhada em 150×206**, mas **REWORK para uso a 0,62×**. Este prompt cria **UMA NOVA moldura simplificada**, independente, para a escalação reduzida. Não substitui nem renomeia o RAW existente. **PENDING: imagem ainda não gerada, sem QA ou aprovação.**

## 1. Anexos exatos (anexar imagens reais, não somente links)

**Obrigatórios — geometria, linguagem e contexto:**

1. **`UI-008_maid_card_compact_v01.png` — CLEAN LOCAL**, caminho `art_working/ui_modular/blender_clean/UI-008_maid_card_compact_v01.png`. Este é o **frame existente**, auditado no lote da [PR #44](https://github.com/NThuesen/jogos_digitais_2D/pull/44); a arte está **APPROVED somente em 150×206**, com [master publicado idêntico ao CLEAN](../../approved/UI-008_maid_card_compact_v01.png). Use como referência de família visual, **não copie sua densidade de ornamentos, divisões de stats ou tipografia**. Não anexe um screenshot de miniatura no lugar do PNG.
2. **`after_ES-2_escalacao_960x600.png`**, [arquivo real](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/roadmap/ui-01b/evidence/after_ES-2_escalacao_960x600.png), caminho `docs/roadmap/ui-01b/evidence/after_ES-2_escalacao_960x600.png`. Contexto de tela e espaço real, não copiar pixels.
3. **`UI-001_panel_large_v01.png`**, [arte APPROVED](../../approved/UI-001_panel_large_v01.png), caminho `docs/art/ui-modular/approved/UI-001_panel_large_v01.png`. Referência de material/paleta; a geometria da card prevalece.

**Opcionais (úteis para identidade do personagem e contexto WebGL):**

4. **`suzu_walk.png`**, [sprite sheet real](https://github.com/NThuesen/jogos_digitais_2D/blob/main/Assets/_Game/Resources/Maids/suzu_walk.png), caminho `Assets/_Game/Resources/Maids/suzu_walk.png`. Identidade visual, **NÃO desenhar Suzu dentro da moldura**.
5. **`webgl_chrome_960x600.jpg`**, [evidência real](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/maid-walk/evidence_illustrated/webgl_chrome_960x600.jpg), caminho `docs/art/maid-walk/evidence_illustrated/webgl_chrome_960x600.jpg`. Referência de contraste e estilo, não copiar UI.

**Não anexar:** `UI-009_maid_card_detail_v01.png` (ainda não existe/aprovado), versões de HOVER/PRESSED dos botões, PNGs de portraits gerados, ou qualquer arte ornamental antiga como master. Se o CLEAN local não estiver disponível, use o master aprovado publicado e confira seu [hash no manifesto](../../approved/UI_MODULAR_SIX_APPROVAL_MANIFEST.json).

## 2. Prompt robusto para GPT Web — copie SOMENTE o bloco abaixo

```text
PROJECT: Neko Maid Inc. Create exactly ONE original reusable LINEUP MINIMAL MAID CARD FRAME sprite for a cozy illustrated 2D anime café / visual novel management game.

REFERENCE HIERARCHY:
1. ATTACHED UI-008_maid_card_compact_v01.png (local CLEAN): visual family and existing card language only; it is NOT approved for tiny use and is too information-dense at 0.62x. Simplify deliberately.
2. ATTACHED after_ES-2_escalacao_960x600.png: real lineup context and available scale, NOT an image to trace.
3. ATTACHED UI-001_panel_large_v01.png: approved cream / rosewood / restrained brass surface and material cues.
Optional suzu_walk.png and webgl_chrome_960x600.jpg: character identity and WebGL contrast context only. NEVER embed a maid or a screenshot into the frame.

PRODUCT DECISION: The game uses TWO presentations of the same maid: a more detailed card for larger contexts and a MINIMAL card in the lineup. This output is ONLY the minimal lineup frame, not the detailed card, not a redesign of the game screen.

REAL UNITY GEOMETRY: Existing MaidCard.Build has a 150 x 206 reference-unit card, shown at 0.62x in the lineup (~93 x 127.72 on screen). Design a clean portrait-oriented frame compatible with that visible footprint. Keep the overall aspect ratio ~150:206 (0.728). The Unity layout may be adapted later, but do not assume more screen space than this. The frame must remain readable at 93 x 128 pixels-equivalent in a 960 x 600 viewport and at 1280 x 720. Prioritize large clear shapes and generous interior margins. Do not render a comparison board or full UI mockup.

LAYOUT: One generous TRUE TRANSPARENT portrait window, visually dominant (~65-75% of the card's height), suitable for the existing illustrated/chibi maid art placed BEHIND the frame by Unity. Beneath it, reserve ONE simple solid warm-cream nameplate region for a short dynamic character name. Optionally leave a tiny unobstructed anchor zone for ONE Unity-drawn selection/status indicator, but do not paint the indicator, its meaning, or text into the sprite. NO dedicated rows for salary, stats, traits, role, rarity, equipment, numbers or long descriptions in this minimal version; these remain in other UI areas or the detailed presentation. Keep the nameplate readable even when reduced; avoid decorative strokes across the text-safe center.

ART DIRECTION: Cozy illustrated anime café, soft warm ivory/cream, rosewood/terracotta and subtle muted brass. Preserve family resemblance with the attached detailed card and UI-001, but reduce ornament density drastically. Simple bold outer silhouette, restrained corner detail, clean visual hierarchy, no hairline filigree that disappears at 0.62x. Do not introduce pixel art, heavy gradients, thick outer glow, fake 3D perspective or glossy arcade treatment.

TECHNICAL: One FRONT-FACING isolated vertical frame. Transparent exterior and truly transparent portrait cut-out; keep the nameplate cream OPAQUE. No character inside. Preserve consistent rounded edges and margins. Do not flatten the portrait window to cream. The image is a modular sprite, not a complete screen; Unity supplies portrait, name, selection state, click/drag, focus, scaling and localization. Avoid painted elements that obstruct raycasts or create fake buttons. Use a clean PNG with true alpha whenever possible; no checkerboard, backdrop, watermark, shadows extending far outside the frame or baked UI content.

STRICT OUTPUT: EXACTLY ONE frame, no variants, no grid, no sample text, no words, letters, numbers, prices, icons, fake labels, maid portraits, screenshots or character art. No mockup, no before/after. Render at a comfortably high resolution preserving the 150:206 ratio; keep the geometry straightforward for uniform Simple/preserveAspect scaling (NOT a 9-slice card). Intended downloaded filename: UI-008_maid_card_lineup_minimal_v01.png.
```

## 3. Saída, validação e gates

- **RAW novo:** `art_working/ui_modular/raw_gpt/UI-008_maid_card_lineup_minimal_v01.png`
- **CLEAN:** `art_working/ui_modular/blender_clean/UI-008_maid_card_lineup_minimal_v01.png` (somente após limpeza/auditoria)
- **APPROVED:** `art_working/ui_modular/approved/UI-008_maid_card_lineup_minimal_v01.png` (somente após decisão explícita)
- Nova tentativa: `UI-008_maid_card_lineup_minimal_v02.png`; nunca sobrescrever v01.

**QA futuro obrigatório:** comparar com a card atual em 150×206 e **0,62× (~93×128)**; usar Suzu real atrás da janela e nomes reais/longos na Unity; verificar seleção, legibilidade do nome, alpha, máscara, sobreposição, contraste e alinhamento nas resoluções 960×600 e 1280×720. Testar telas completas/scroll/drag/foco depois de autorização de integração. Não inferir aprovação de uma captura estática. **Não gerar um novo portrait nem modificar gameplay, dados ou saves.**

[Voltar ao tutorial](../../TUTORIAL.md) · [manifesto](../../OUTPUT_MANIFEST.md) · [registro](../../ASSET_REGISTRY.md).
