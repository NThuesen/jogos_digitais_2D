# UI-021 — Yuki (new canonical maid · Kuudere)

**Status:** design prompt for first-generation candidate; NOT yet generated or approved. This is a NEW character, not a repaint of an existing maid.

## Real reference attachments (download files and attach the images, not just URLs)

1. [`suzu_clean.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/ui-modular/portraits/suzu_clean.png) — approved Suzu v03 finish / linework / material rendering **ONLY**; never copy her appearance.
2. [`nove_retratos.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/ui-modular/portraits/nove_retratos.png) — nine existing designs as a **collision-avoidance lineup**; the new face, silhouette, hair and accessories must not resemble any of them.
3. [`detail_suzu_960x600.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/ui-modular/portraits/integration/detail_suzu_960x600.png) — optional: real scale and clarity in Unity's larger detail panel.

Source of visual direction: [CURRENT_VISUAL_DIRECTION.md](../../../CURRENT_VISUAL_DIRECTION.md). No walk sheet exists for Yuki yet: this portrait establishes the identity to be faithfully used for her eventual walk/idle animation.

## Ready-to-copy image-generation prompt

```text
PROJECT: Neko Maid Inc. — NEW MAID CHARACTER DESIGN / PREMIUM PORTRAIT — YUKI V01.

Generate EXACTLY ONE original, polished 2D anime half-body character illustration of Yuki, a newly introduced Kuudere catgirl maid in the cozy illustrated café game Neko Maid Inc.

ARTISTIC PURPOSE: This is the DESIGN MASTER for a unique, recurring, named game character. It must work both as a full high-resolution 3:4 character profile and as the source of a close-up head crop (approx. 1.22:1) on collectible cards as small as 93×128. Keep the main design complex and beautiful; cropping/downsampling will be done later in Unity's asset pipeline.

ATTACHED REFERENCES:
- Suzu v03 clean: finish, rich but controlled 2D anime rendering, cloth/hair/material shading, and polish only; do NOT copy Suzu's face, mint hair, bells, outfit cut or personality.
- Nine existing maid portraits: anti-duplication comparison of existing silhouettes and color signatures. DO NOT create a recolored existing character.
- Optional real game detail screenshot: composition/scale only; do not reproduce the screen, panels, or UI.

IDENTITY: Yuki — Kuudere.
- Skin: porcelain-beige skin with cool pink undertones, not paper-white flat skin.
- Hair and silhouette: long, straight snow-white hair with pale silver-blue shadow planes, parted cleanly off center and flowing in two long heavy lengths down her shoulders; no bob or blunt hime sidelocks.
- Eyes: clear frost-blue eyes with a composed, slightly narrow upper lid.
- Cat ears / tail: long soft white angora cat ears with fine pearly inner fur and a fluffy white tail when visible.
- Face: narrow oval face, refined straight eyebrows, serene closed-mouth expression.
- Café maid costume: understated cream-and-rosewood maid dress with desaturated slate-blue panels, silver edging, tidy narrow apron pleats and a SINGLE six-point snowflake brooch centered below the collar.
- Expression and posture: upright, calm three-quarter posture; hands gently folded rather than theatrical posing.
- Her unmistakable visual signature is: long flowing snow-white hair + cool blue eyes + silver snowflake clasp.

CHARACTER DIFFERENTIATION:
Closest existing archetype counterpart is Hina (navy-blue/violet hair, angular triangle hair clip). Also avoid visual collision with Kaguya's blue-black hime cut and Kasumi's smoky blue-grey asymmetry. Specifically: No dark-blue bob, triangular clip, long hime cut, floating magic, snow particles, heavy winter coat or imitation Hina face.
Use a different facial structure, silhouette, ear/fur markings and expression, not just new hair colors. Preserve a clearly recognizable Kuudere personality without mimicking poses of the other nine.

PREMIUM SHARED GAME STYLE:
Modern high-quality Japanese 2D anime illustration, coherent with Suzu v03's warm cozy café art direction. Smooth colored outlines with delicate weight variation; expressive eyes; layered colored hair highlights; controlled premium cel shading with light painterly material accents; intricate but organized maid-fabric folds and lace; refined small brass details. Cream and muted burgundy/rosewood remain recurring uniform-family colors, while Yuki's personal accent and costume cut distinguish her. Not pixel art, not photorealistic, not a VTuber render, not a generic doll face. No extreme highlights, exaggerated anatomy or unnecessary decorative noise.

FRAMING AND TECHNICAL RULES:
One isolated waist-up character from the tips of BOTH cat ears and headdress to roughly the waist, naturally in a slight three-quarter view, portrait-oriented canvas roughly 3:4. Both eyes and character-defining accessories must be unobscured. Keep at least 5% genuine transparent headroom above the highest ear/hair ornament, and 4% side margins for hair and tail/accessories. Do not flatten ears against the top edge. Central face must survive a small-game-card crop. Avoid having the character's signature outside the card's central head-and-shoulders area. Genuine transparent alpha outside the character; edges clean. No background, no fake checkerboard, no letters, no interface, no card frame, no logo, no watermark, no unrelated props or second character.

FINAL CHECK: Does the image read distinctly as Yuki, rather than Suzu, her established archetype counterpart, or a color-swap of the other new candidates? If not, improve the character silhouette and facial identity before rendering.

Output ONE image only. Intended basename: UI-021_maid_portrait_yuki_neutral_v01.png.
```

## Local output

- RAW candidate: `art_working/ui_modular/raw_gpt/UI-021_maid_portrait_yuki_neutral_v01.png`
- Keep any v02 as an additional file (`_v02.png`) rather than replacing v01.
- Do not copy to approved or integrate into Unity until owner visual approval and QA.
