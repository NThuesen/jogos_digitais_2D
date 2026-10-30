# 07 · NENE — Nene (Idol)

**Estado:** Nova identidade proposta, não aprovada.
**Marcadores de identidade:** Coque lateral rosa assimétrico, mecha ciano, heterocromia azul/rosa, estrela dourada.
**Chroma inicial:** **verde puro `#00FF00`**, somente após inspeção da imagem real.
**Regra:** propostas visuais não alteram as regras/atributos do arquétipo em `Neko.Sim`.

## Tutorial: o que anexar

- **PROMPT 1 (ChatGPT):** anexe a **[referência de estilo do projeto](../../art-01/D-pixel-cafe/reference/style_anchor_reference.png)**. Não anexe outra personagem.
- **PROMPT 2 (ChatGPT):** anexe **(1)** o PNG full-body da **Nene gerado no PROMPT 1** e **(2)** a **[mesma referência de estilo](../../art-01/D-pixel-cafe/reference/style_anchor_reference.png)**. O PNG da Etapa 1 manda na identidade; a arte de referência manda apenas no estilo.
- **Tencent AI Studio (etapa posterior, opcional):** anexe o **PNG do PROMPT 2** como frame inicial; siga o [guia de vídeo, QA e nomes](../README.md#tutorial--etapa-3-tencent-ai-studio-experimento-de-vídeo). Não pressupor que a ferramenta aceite mais de uma imagem.

**Atenção:** cole **somente um dos blocos completos abaixo** por geração. Não concatene os prompts-base novamente. O fundo do PROMPT 1 é neutro; o do PROMPT 2 é chroma. Se a cor colidir com alguma peça da personagem gerada, altere a cor na instrução de chroma e registre a escolha.

## PROMPT 1 — criar a personagem (copiar inteiro)

```text
You will receive one attached STYLE REFERENCE IMAGE from our cozy pixel-art anime / visual-novel cat-maid café game. Treat it as a reference for artistic language only: warm, richly crafted atmosphere, gentle anime expressions, colored outlines, cohesive shapes, restrained pixel clusters, and a premium, charming collectible-game finish. Do NOT reproduce the pink-haired character, faces, costumes, logos, Japanese writing, café layout, UI borders, furniture, or composition shown in that image. Make a completely ORIGINAL adult cat-maid, visually part of the same fictional game.

Generate exactly ONE standalone, full-body character illustration — NOT a character sheet, multiple poses, collage, interface mockup, portrait-only crop, video frame sequence, rendered café interior or written description. She should face the viewer (very gentle three-quarter angle is allowed), standing in a relaxed balanced pose with both hands, both feet, both ears and the complete tail clearly visible within generous margins. Include sufficient space around hair and accessories for later editing and animation. One character; no duplicates, reflections, background people, animals or text.

Make this a high-quality 2D pixel-art-influenced anime game character designed for a small on-screen chibi presentation: approximately 2.5–3.25 heads tall, appealing expressive head and eyes, stable body landmarks, coherent anatomy and believable joints for a future walk cycle. Use deliberate pixel-like clusters, clean stepped edges, controlled hard/soft transitions and readable shapes rather than photographic detail, 3D rendering, glossy plastic surfaces, blur, noisy microtexture, painterly smear or artificial pixelation overlay. Warm cream/chocolate clothing materials, sophisticated pink/terracotta/rosewood accents where appropriate, colored outlines and subtle cool shadow shifts; allow the specific maid's signature palette to stand out against these shared materials. Do not require strict numeric pixel-size fidelity from the generator.

Each maid must have a memorable, non-interchangeable hair silhouette; one strong face cue; precisely described cat ears, tail and costume details; and at most one or two dominant accessories. Differentiate designs with intentional color relationships, not random decorations. Garments must read as practical yet adorable café staff clothing with covered, tasteful styling, apron, shoes and recognizable maid trim. Avoid fetish styling or revealing focus. Keep outfit geometry simple enough to remain stable across independently generated walking frames, while expressive enough to support collectible portraits later.

Composition: isolated character against a simple flat warm-neutral light backdrop that contrasts with her hair and clothing; no furniture, floor tiles, setting, vignette, cast shadow, border, UI, captions, lettering, logo or watermark. Full-body and large in frame without cropping. This is the approved-identity CANDIDATE for downstream image-to-video tests, not the final card portrait.

Character identity specification (mandatory; these details override generic visual suggestions):
Create NENE, an ORIGINAL adult Idol cat-maid: lively, stage-ready and expressive, but still a believable member of a cozy café team. To avoid resembling the PINK-haired maid in the style-reference screenshot, give Nene a clearly DIFFERENT silhouette: saturated BUBBLEGUM PINK hair arranged in ONE high asymmetric SIDE BUN on her LEFT with a single long curly pony-strand flowing from it; leave the other side short at jaw height. One unmistakable CYAN-TEAL hair streak falls from her RIGHT temple. No identical pigtails, hairstyle, face or outfit from the reference image.

Her defining facial feature is deliberate HETEROCHROMIA: RIGHT iris bright cornflower BLUE, LEFT iris soft ROSE PINK (anatomical right/left, not mirrored). Keep both eyes visible; equal size, readable color difference, joyful star-like catchlights. Her cat ears are warm pink, one slightly higher due to the bun, with light cream interiors. Tail is pink fading softly to peach with a very small TEAL ribbon close to its base, but not a huge bow that blocks movement. Signature ornament is a simple GOLD STAR COMB beside the side bun, no excessive confetti.

The costume is an idol-flavored café uniform rather than a concert costume: ivory ruffled apron, dusty-pink bodice, teal piping at collar/cuffs, one muted gold star fastening, short neat pleated hem, light socks and dark-pink shoes. Keep her body shape and garment coverage appropriate for adult café staff. Smile brightly, tilt the head a little and spread one hand in a small welcoming wave without making a big performing pose. Preserve star comb, color-locked heterochromia and teal streak as non-negotiable identifiers for animation; avoid dozens of tiny stars or sparkle particles.

If the style reference conflicts with the specified identity, preserve the CHARACTER IDENTITY and only transfer the reference's rendering language. Return only the rendered image.
```

**Saída esperada:** `07_nene_step1_fullbody_v01.png` — uma personagem original inteira, **não** portrait recortado, não card, não vídeo.

## PROMPT 2 — criar quadro inicial de caminhada (copiar inteiro)

```text
Two images are attached: IMAGE A is the already generated full-body maid from STEP 1 and is the ABSOLUTE CHARACTER IDENTITY REFERENCE; IMAGE B is the project STYLE REFERENCE and controls the general cozy pixel-art anime / visual-novel rendering language only. Do NOT copy any character or scene from IMAGE B. If the two references conflict, IMAGE A wins for face, hairstyle, ears, tail, clothing, proportions, accessory placement, color and character design.

Generate exactly ONE clean full-body still image to serve as the initial frame for short AI image-to-video WALK animation. The output is NOT a sprite sheet, animation strip, montage, scene, in-game screenshot, card or text description. Keep exactly the same maid as IMAGE A with the same recognizable face, iris colors (including left/right heterochromia if present), hair locks, ear asymmetries, tail coloration, maid uniform, seam/ribbon placements, ornaments, shoes and silhouette. No costume redesign, extra objects, prop creation, missing limbs or changes to the intended age.

She is frozen at a believable MID-STRIDE phase of a light, short, game-like walk, slightly toward screen right (the horizontal X direction) but with face and chest still mostly front-facing / gentle three-quarter view so both eyes remain visible. One foot a little forward, the opposite arm a little forward, stable hips, balanced posture, hands visible, relaxed expression appropriate to her personality. Avoid running, hopping, long strides, dancing, dramatic twisting or side-stepping with broken anatomy. Give the hair, apron and tail plausible resting positions that a video model could animate later, without motion blur. Keep the character's feet fully visible, tail tip visible, and generous empty space at all edges. The future game engine translates her position on X; this frame is for WALK-IN-PLACE motion, not traveling across the image.

Background must be a completely opaque, absolutely uniform, single flat CHROMA KEY color #00FF00 from edge to edge, without gradient, texture, lighting, horizon, checkerboard, floor line, shadow, shadow halo, reflections, vignette, props, text, watermark, UI or people. Do not introduce any glow or semi-transparent green/blue outline on the maid. Keep her original colors, especially around thin strands, ears, jewelry, skirt hems and tail. Use a crisp hard silhouette against the solid background. No environmental lighting changes.

Character-specific identity lock and chroma notes:
Nene MUST preserve right-eye BLUE and left-eye ROSE-PINK heterochromia, asymmetric BUBBLEGUM-PINK LEFT side bun with one curled loose strand, one CYAN-TEAL streak on RIGHT temple, small GOLD star comb, cream-and-dusty-pink apron dress with teal trim, and pink tail with a small teal base ribbon. Never copy the pink maid in the style reference; Nene is a different original character. Use pure GREEN #00FF00 chroma as a first experiment; check the rendered teal streak, teal piping and any greenish pixels for keying conflict. If the green removes parts of Nene, choose another vivid absent color and regenerate while preserving original garment colors. She moves with gentle upbeat stage cadence, NOT dancing or hopping. Do not switch the eye colors or mirror the design.

Only produce the final single still image. Do not include an explanation.
```

**Saída esperada:** `07_nene_step2_walkstart_v01_green.png` (adapte ao chroma realmente usado). O vídeo/ciclo só nasce depois, na etapa Tencent.

## Revisão antes de aceitar

- [ ] Identidade da ficha preservada, inclusive lados/asimetrias
- [ ] Personagem continua original, sem copiar a maid da referência
- [ ] Etapa 1: rosto, uniforme, orelhas, cauda, braços e sapatos visíveis
- [ ] Etapa 2: mesma identidade da Etapa 1, pose de meio passo natural
- [ ] Chroma sem tons conflitantes no cabelo/olhos/roupa/acessórios, contorno limpo
- [ ] Responsável aprovou/rejeitou e nome/versão do arquivo foram registrados
