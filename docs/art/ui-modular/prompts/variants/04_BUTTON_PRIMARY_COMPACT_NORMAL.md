# UI-004 — Botão primário COMPACT NORMAL v01

**Status:** `PENDING` — prompt pronto para GPT Web; imagem ainda não gerada, limpa, aprovada ou integrada. **UM prompt = UMA imagem.** Decisão de produto (03/11/2026): botões ornamentais grandes e compactos simplificados são famílias distintas. O NORMAL ornamental anterior permanece em `REWORK`.

## 1. Anexe estes arquivos EXATOS que já existem no GitHub

Baixe os originais dos links e **anexe as imagens no ChatGPT Web**; colar apenas a URL não equivale a anexar o arquivo.

1. **Referência visual APROVADA:** [`UI-001_panel_large_v01.png`](../../approved/UI-001_panel_large_v01.png) — `docs/art/ui-modular/approved/UI-001_panel_large_v01.png`. Use a linguagem acolhedora de creme, madeira rosada/terracota e dourado discreto. O painel é aprovado como arte, **não integrado**.
2. **Uso real da Loja:** [`after_LJ-1_loja-moveis_960x600.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/roadmap/ui-01a/evidence/after_LJ-1_loja-moveis_960x600.png) — `docs/roadmap/ui-01a/evidence/after_LJ-1_loja-moveis_960x600.png`. Serve apenas para escala, contexto e densidade de interface. **Não gere a tela inteira.**
3. **Atmosfera histórica, NÃO pixel art:** [`style_anchor_reference.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/art-01/D-pixel-cafe/reference/style_anchor_reference.png) — `docs/art/art-01/D-pixel-cafe/reference/style_anchor_reference.png`. Aproveite o aconchego e não a pixelização.
4. **Exemplo do problema a EVITAR:** [`unity_sliced_1280x720.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/ui-modular/reviews/evidence/UI-004/unity_sliced_1280x720.png) — `docs/art/ui-modular/reviews/evidence/UI-004/unity_sliced_1280x720.png`. É uma matriz de QA da peça ornamental antiga: observe como a ornamentação perde leitura em tamanhos pequenos. **Não copie esta matriz ou seus botões.**

Não peça o arquivo local `UI-004_button_primary_normal_v01.png`: ele não está publicado como asset aprovado e **não é pré-requisito** para este novo prompt.

## 2. Prompt completo — copie SOMENTE o bloco abaixo

```text
PROJECT: Neko Maid Inc. Create exactly ONE ORIGINAL, isolated, production-minded UI sprite: a PRIMARY BUTTON, COMPACT FAMILY, NORMAL resting state. It belongs to a cozy illustrated 2D anime café / visual novel / management game. The provided images are REFERENCES ONLY: the approved UI-001 panel establishes the warm cohesive material language, the real Shop screenshot establishes scale/context, the style anchor contributes cozy atmosphere but NOT pixel art, and the old button QA matrix is a NEGATIVE EXAMPLE of excessive ornamentation. Do not copy, trace, reconstruct or collage any reference.

DESIGN PURPOSE: This compact button is meant to remain attractive and readable when Unity displays it at 150×44 (real Shop purchase button) and 120×44 (small navigation), with 230×54 as an additional adaptation target. Unity will add dynamic text, including currency prices, and manage interactions. Prioritize excellent small-size readability over impressive details when zoomed in.

Create a single front-facing horizontal button, approximately 3.4:1 source silhouette, visually horizontal and nearly rectangular with gently rounded, symmetrical corners. Quiet cream/warm ivory solid inner surface, a slim rosewood/terracotta-painted outer rim, restrained muted brass/gold accents, subtle layered tactile relief and restrained warm highlights. The button must look clearly clickable, polished, delicate and inviting, without looking like a modern glossy mobile-app pill or a decorative sign. Match the approved panel's visual family without duplicating its ornamentation.

CRITICAL COMPACT GEOMETRY:
- At least about 75% of the visible width must remain a clean, uninterrupted, text-safe central zone. NO leaves, swirls, paw marks, emblems, dots or pattern behind the dynamic price/text.
- Minimize end caps, decorative borders and shadow footprint so a 120×44 button does not lose its text area.
- Make border hierarchy and the outer silhouette readable at only 44 Unity UI units tall; use restrained broad shapes instead of fine hairline filigree, micro-dots or tiny ornamental motifs.
- Symmetrical left/right end caps and continuous, uncomplicated horizontal/vertical middle bands, suitable for a possible Unity 9-slice. Do not put unique decoration, gradients with hard discontinuities, or seams across stretch bands.
- Keep straight-on orthographic presentation, no perspective, tilt, excessive bevel, deep drop shadow, or fake 3D plastic.
- Exterior of the button: real transparent alpha if possible, or a uniform removable matte. The cream inner button FACE IS SOLID and must NOT be erased as background.
- No overlapping elements, clipping, halos, sheet, alternate button states, or decorative scenery.

ABSOLUTELY NO TEXT, letters, words, numbers, money labels, icons, character art, logos, watermarks, checkerboard background, full UI screens, mockup frame or second button. Do not render "Comprar", "Voltar", "R$" or any other UI label into the sprite. Do NOT create HOVER, PRESSED or DISABLED now. Unity handles those states later.

Produce exactly ONE standalone NORMAL button with generous transparent padding only if essential (prefer a tight, untrimmed continuous silhouette that is easy to inspect), centered in the image. Aim for a clean high-resolution master; exact export/import resolution, alpha, 9-slice borders, text fit and WebGL memory must be decided from actual tests, not assumed from this image-generation request.

ONE IMAGE ONLY. Intended local filename after download: UI-004_button_primary_compact_normal_v01.png. If ChatGPT downloads with a different filename, rename it manually.
```

## 3. Baixe e salve o PNG com nome EXATO

- **Bruto, sem sobrescrever outras variantes:** `art_working/ui_modular/raw_gpt/UI-004_button_primary_compact_normal_v01.png`
- **Se precisar tratar alpha/bordas:** `art_working/ui_modular/blender_clean/UI-004_button_primary_compact_normal_v01.png`
- **Somente após aprovação humana:** `art_working/ui_modular/approved/UI-004_button_primary_compact_normal_v01.png`
- Nova tentativa: trocar apenas `_v01.png` por `_v02.png`, mantendo o bruto original.

`art_working/` é ignorado pelo Git. Não publicar PNG bruto em `docs/art/ui-modular/approved/`, nem criar `.meta`/asset Unity, sem aprovação explícita.

## 4. Critérios antes de pedir aprovação da arte

- **Inspeção do PNG:** uma única peça, sem texto ou ícones, centro creme sólido, exterior em alpha real e bordas sem halo.
- **Comparação com UI-001:** estilo cozy anime ilustrado, rosewood/terracota + creme + dourado contido; sem copiar pixel art ou a ornamentação do botão grande.
- **No Unity, quando a imagem existir:** testar 150×44 com preços dinâmicos reais da Loja (não presumir que o botão diga "Comprar"), 120×44 com texto de navegação e 230×54 como tamanho adicional, em 960×600 e 1280×720. Verificar o estado `interactable`/`disabled` do componente existente, sem afirmar que a arte DISABLED existe.
- **Qualidade de uso:** sem quebra/clipping do texto, faixa central suficiente, contraste adequado, desenho percebido como clicável, hitbox e alpha consistentes. Se usar 9-slice, medir bordas e avaliar costuras no render real.
- **WebGL e memória:** `NOT RUN` até teste real. Não fixar dimensões finais de textura, compressão ou consumo de GPU por estimativa.
- **Gate:** gerar somente NORMAL agora; Claude Code critica o resultado e Codex valida tecnicamente após a seleção. Só o responsável pode marcar `APPROVED`; `INTEGRATED` exige PR própria.

## 5. Estado e relacionamentos

Esta é uma **nova variante do ID UI-004**, não um novo ID ou substituição do `UI-004_button_primary_normal_v01.png` ornamental. A peça ornamental antiga permanece `REWORK` e é candidata apenas a CTA grande (~260×88), ainda sem homologação de export 768×256/WebGL. Abas são componentes próprios. HOVER/PRESSED/DISABLED desta família compacta **ainda não possuem PNG nem prompt de geração liberado**.

[Índice UI-004](../04_BUTTON_PRIMARY.md) · [Tutorial geral](../../TUTORIAL.md) · [Registro](../../ASSET_REGISTRY.md) · [Revisão ornamental anterior](../../reviews/UI-004_BUTTON_PRIMARY_NORMAL_V01.md).
