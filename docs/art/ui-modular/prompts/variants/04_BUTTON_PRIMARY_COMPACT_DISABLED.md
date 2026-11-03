# UI-004 — BUTTON PRIMARY COMPACT DISABLED v01

**Estado:** PENDING (prompt pronto, PNG não produzido). Esta variante é da **família compacta** aprovada como direção, não da ornamental antiga em REWORK. **UMA execução = UM PNG**. Gerar em GPT Web a partir do NORMAL compacto aprovado, sem alterar assets do jogo.

## 1. Anexos — referência correta e ordem

**Obrigatório (arquivo local, não disponível como PNG aprovado no GitHub no checkpoint):**
- `art_working/ui_modular/blender_clean/UI-004_button_primary_compact_normal_v01.png` — **NORMAL COMPACTO LIMPO 2072×380, SHA-256 `c82d06142b3953a97b9654d8b7bf53e611679a6ffb4822a3b268dff239c17eb6`**. Baixe/anexe o arquivo real à conversa GPT Web. Caso tenha uma cópia com esse mesmo SHA em `art_working/ui_modular/approved/`, ela pode ser usada como fonte idêntica. **Não use o RAW 2172×724, a exportação reduzida 768×141 nem o `UI-004_button_primary_normal_v01.png` ornamental antigo.**

**Referência secundária opcional, somente para contexto de estilo:**
- [`UI-001_panel_large_v01.png`](../../approved/UI-001_panel_large_v01.png) — painel aprovado, de uso visual e não geométrico. O NORMAL COMPACTO prevalece em qualquer conflito de forma/cor.

Preferir gerar a variante em conversa com o PNG NORMAL realmente anexado e solicitar **edição da imagem de referência**, não criação independente. Um link escrito sozinho não substitui o anexo.

## 2. Prompt completo para GPT Web — copiar apenas este bloco

```text
PROJECT: Neko Maid Inc. Edit the ATTACHED APPROVED COMPACT NORMAL button sprite to create exactly ONE DISABLED interaction state (botão indisponível). This is an IMAGE EDIT of the same reusable Unity UI asset, not a brand-new button design. Use the attached compact NORMAL PNG as the geometry, surface and palette authority. The optional UI-001 approved panel is style context only. Ignore the unrelated older ornamental button family.

SOURCE MASTER: UI-004_button_primary_compact_normal_v01.png, cleaned 2072×380 RGBA. Preserve exactly the same front-facing silhouette, canvas framing, rounded corners, button proportions, transparent exterior, straight horizontal/vertical 9-slice bands, text-safe central cream surface, material, and overall alignment. This sprite must drop into the SAME Unity RectTransform and slice configuration as NORMAL. No ornamental leaves, small dots, swirls, paw marks, emblems, extra shadows outside the shape or other new decorative elements.

STATE: DISABLED.
Create a quiet, visibly inactive/resting button using modestly reduced saturation/contrast in the existing rosewood and brass border, while keeping the face uniform and clean enough for Unity's accessible dynamic label. Do NOT print a label or fake crossed-out icon. Preserve silhouette and crop. Avoid heavy greying, translucency of the cream face, muddy colors or strong opacity reduction: Unity also controls interactable/tint, so do not stack aggressive disabled effects.

VISUAL TARGET: Cozy illustrated 2D anime café / visual novel management UI. Keep the rosewood/terracotta painted frame, quiet warm cream/ivory solid face and restrained brass/gold detail. Make the state clear in the final 120×44, 150×44 and 230×54 Unity sizes without adding fine detail that disappears after scaling. No independent redesign. Keep the interior text area empty and readable; Unity overlays localized prices and button labels.

STRICT OUTPUT: ONE isolated DISABLED button, not four states, not a comparison board, not a mockup. No text, words, letters, numbers, currency, icons, fake UI, characters, watermark or checkerboard. Prefer real PNG alpha outside; NEVER remove the intended solid cream interior. Do not invent a different crop or aspect ratio. Preserve the exact source master geometry/placement as closely as image editing allows. If geometric equivalence cannot be maintained, the output is a candidate for rejection or Unity/Blender-derived tint/shading, not permission to silently alter the button system.

Save/download and rename the single PNG to UI-004_button_primary_compact_disabled_v01.png.
```

## 3. Destinos e critérios

- RAW: `art_working/ui_modular/raw_gpt/UI-004_button_primary_compact_disabled_v01.png`
- CLEAN (se necessário): `art_working/ui_modular/blender_clean/UI-004_button_primary_compact_disabled_v01.png`
- APPROVED (somente depois do responsável): `art_working/ui_modular/approved/UI-004_button_primary_compact_disabled_v01.png`
- Regeração: `_v02.png`, sem sobrescrever o `v01`.

**Antes de chamar Codex para QA:** o responsável compara lado a lado com o NORMAL em escala equivalente. Rejeitar/retrabalhar diferenças de contorno, margens, raios, banda 9-slice ou área de texto. Após a seleção: Codex compara alpha/bounding box, geometria, cores e capturas Unity 960×600 / 1280×720 nos tamanhos 120×44, 150×44 e 230×54; validar uso real do estado DISABLED e WebGL quando pertinente. A arte não substitui os controles Unity. Se o GPT não preservar a geometria, preferir derivar a variação tecnicamente do NORMAL em Unity/Blender, sujeito a revisão visual.

**Não interpretar a existência deste prompt como imagem gerada/aprovada/integrada.** Não tocar na família ornamental antiga.

[Índice UI-004](../04_BUTTON_PRIMARY.md) · [Tutorial](../../TUTORIAL.md) · [Manifesto](../../OUTPUT_MANIFEST.md) · [QA do NORMAL compacto](../../reviews/UI-004_BUTTON_PRIMARY_COMPACT_NORMAL_V01.md).
