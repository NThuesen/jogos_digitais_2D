# 05 · REINA — Reina (Ojou-sama)

**Estado:** Nova identidade proposta, não aprovada.
**Marcadores de identidade:** Cachos espirais platina/vinho, olhos violeta, broche de ametista, postura aristocrática.
**Chroma inicial:** **verde puro `#00FF00`**, somente após inspeção da imagem real.
**Regra:** propostas visuais não alteram as regras/atributos do arquétipo em `Neko.Sim`.

## Tutorial: o que anexar

- **PROMPT 1 (ChatGPT):** anexe a **[referência de estilo do projeto](../../art-01/D-pixel-cafe/reference/style_anchor_reference.png)**. Não anexe outra personagem.
- **PROMPT 2 (ChatGPT):** anexe **(1)** o PNG full-body da **Reina gerado no PROMPT 1** e **(2)** a **[mesma referência de estilo](../../art-01/D-pixel-cafe/reference/style_anchor_reference.png)**. O PNG da Etapa 1 manda na identidade; a arte de referência manda apenas no estilo.
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
Create REINA, an ORIGINAL adult Ojou-sama cat-maid from an elegant fictional café house. She is identified by PALE PLATINUM-BLONDE hair with two controlled spiral DRILL CURLS framing her shoulders; their lowest sections fade to a deep WINE-RED plum accent. Keep the drill curls as continuous, readable springs, not a fluffy mass or ordinary twin tails. Violet eyes with fine lashes, a gentle noble smile and chin slightly elevated communicate poise; no crown, tiara or princess dress.

Her elegant upright cat ears are platinum with very pale rose inner fur, both symmetric and smaller than Akari's ears. Tail is well groomed, platinum with a wine-colored narrow ribbon near its base and a deep burgundy tapered tip. Signature accessory is ONE oval amethyst-colored jewel brooch set into the center of her high-collar bow, combined with a thin ruffled lace maid headband; these must remain compact enough for a chibi. Nothing else sparkles like a large gemstone.

Her outfit follows café-staff practicality but is refined: cream pinafore with layered lace edge over a rich burgundy dress, gentle antique-gold piping, fitted modest long sleeves with neat cuffs, matching dark wine shoes and pale knee socks. Avoid gowns, crinolines, luxury jewelry piles or long skirts that conceal feet. Show a dignified straight standing pose with one lightly open gloved hand, the other held close to the skirt; her calm confident presence is luxurious but still affectionate, collectible, and friendly. Preserve strong separation between the light curled hair and cream apron by using colored outlines and controlled shadow bands.

If the style reference conflicts with the specified identity, preserve the CHARACTER IDENTITY and only transfer the reference's rendering language. Return only the rendered image.
```

**Saída esperada:** `05_reina_step1_fullbody_v01.png` — uma personagem original inteira, **não** portrait recortado, não card, não vídeo.

## PROMPT 2 — criar quadro inicial de caminhada (copiar inteiro)

```text
Two images are attached: IMAGE A is the already generated full-body maid from STEP 1 and is the ABSOLUTE CHARACTER IDENTITY REFERENCE; IMAGE B is the project STYLE REFERENCE and controls the general cozy pixel-art anime / visual-novel rendering language only. Do NOT copy any character or scene from IMAGE B. If the two references conflict, IMAGE A wins for face, hairstyle, ears, tail, clothing, proportions, accessory placement, color and character design.

Generate exactly ONE clean full-body still image to serve as the initial frame for short AI image-to-video WALK animation. The output is NOT a sprite sheet, animation strip, montage, scene, in-game screenshot, card or text description. Keep exactly the same maid as IMAGE A with the same recognizable face, iris colors (including left/right heterochromia if present), hair locks, ear asymmetries, tail coloration, maid uniform, seam/ribbon placements, ornaments, shoes and silhouette. No costume redesign, extra objects, prop creation, missing limbs or changes to the intended age.

She is frozen at a believable MID-STRIDE phase of a light, short, game-like walk, slightly toward screen right (the horizontal X direction) but with face and chest still mostly front-facing / gentle three-quarter view so both eyes remain visible. One foot a little forward, the opposite arm a little forward, stable hips, balanced posture, hands visible, relaxed expression appropriate to her personality. Avoid running, hopping, long strides, dancing, dramatic twisting or side-stepping with broken anatomy. Give the hair, apron and tail plausible resting positions that a video model could animate later, without motion blur. Keep the character's feet fully visible, tail tip visible, and generous empty space at all edges. The future game engine translates her position on X; this frame is for WALK-IN-PLACE motion, not traveling across the image.

Background must be a completely opaque, absolutely uniform, single flat CHROMA KEY color #00FF00 from edge to edge, without gradient, texture, lighting, horizon, checkerboard, floor line, shadow, shadow halo, reflections, vignette, props, text, watermark, UI or people. Do not introduce any glow or semi-transparent green/blue outline on the maid. Keep her original colors, especially around thin strands, ears, jewelry, skirt hems and tail. Use a crisp hard silhouette against the solid background. No environmental lighting changes.

Character-specific identity lock and chroma notes:
Reina remains the adult platinum-blonde cat-maid with TWO spiral drill curls and wine-red lower ends, violet eyes, symmetric pale ears, platinum tail with wine tip/ribbon, oval violet brooch, compact lace headband, and cream/burgundy ornate maid dress. Keep both elaborate curls structurally stable across motion and preserve visible shoes. Use flat GREEN #00FF00 background because her palette excludes neon green. The walk is graceful and small, not a runway strut, and no new crown, floating jewels or missing brooch are allowed.

Only produce the final single still image. Do not include an explanation.
```

**Saída esperada:** `05_reina_step2_walkstart_v01_green.png` (adapte ao chroma realmente usado). O vídeo/ciclo só nasce depois, na etapa Tencent.

## Revisão antes de aceitar

- [ ] Identidade da ficha preservada, inclusive lados/asimetrias
- [ ] Personagem continua original, sem copiar a maid da referência
- [ ] Etapa 1: rosto, uniforme, orelhas, cauda, braços e sapatos visíveis
- [ ] Etapa 2: mesma identidade da Etapa 1, pose de meio passo natural
- [ ] Chroma sem tons conflitantes no cabelo/olhos/roupa/acessórios, contorno limpo
- [ ] Responsável aprovou/rejeitou e nome/versão do arquivo foram registrados
