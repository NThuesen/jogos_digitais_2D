# UI-021 — Himari (new canonical maid · Idol)

**Status:** design prompt for first-generation candidate; NOT yet generated or approved. This is a NEW character, not a repaint of an existing maid.

## Real reference attachments (download files and attach the images, not just URLs)

1. [`suzu_clean.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/ui-modular/portraits/suzu_clean.png) — approved Suzu v03 finish / linework / material rendering **ONLY**; never copy her appearance.
2. [`nove_retratos.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/ui-modular/portraits/nove_retratos.png) — nine existing designs as a **collision-avoidance lineup**; the new face, silhouette, hair and accessories must not resemble any of them.
3. [`detail_suzu_960x600.png`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/ui-modular/portraits/integration/detail_suzu_960x600.png) — optional: real scale and clarity in Unity's larger detail panel.

Source of visual direction: [CURRENT_VISUAL_DIRECTION.md](../../../CURRENT_VISUAL_DIRECTION.md). No walk sheet exists for Himari yet: this portrait establishes the identity to be faithfully used for her eventual walk/idle animation.

## Ready-to-copy image-generation prompt

```text
PROJECT: Neko Maid Inc. — NEW MAID CHARACTER DESIGN / PREMIUM PORTRAIT — HIMARI V01.

Generate EXACTLY ONE original, polished 2D anime half-body character illustration of Himari, a newly introduced Idol catgirl maid in the cozy illustrated café game Neko Maid Inc.

ARTISTIC PURPOSE: This is the DESIGN MASTER for a unique, recurring, named game character. It must work both as a full high-resolution 3:4 character profile and as the source of a close-up head crop (approx. 1.22:1) on collectible cards as small as 93×128. Keep the main design complex and beautiful; cropping/downsampling will be done later in Unity's asset pipeline.

ATTACHED REFERENCES:
- Suzu v03 clean: finish, rich but controlled 2D anime rendering, cloth/hair/material shading, and polish only; do NOT copy Suzu's face, mint hair, bells, outfit cut or personality.
- Nine existing maid portraits: anti-duplication comparison of existing silhouettes and color signatures. DO NOT create a recolored existing character.
- Optional real game detail screenshot: composition/scale only; do not reproduce the screen, panels, or UI.

IDENTITY: Himari — Idol.
- Skin: warm golden-tan skin with subtle natural highlights.
- Hair and silhouette: sunlit rose-gold hair with a CORAL undertone, swept into an elegant half-up crown twist with the lower hair forming soft outward curls around the shoulders; no pigtails or vivid pink streaks.
- Eyes: striking turquoise-blue eyes, open and sparkling but not oversized.
- Cat ears / tail: soft cream ragdoll cat ears tipped with warm caramel, plus a fluffy matching tail if visible.
- Face: rounded diamond-shaped face, defined smile lines and a confident stage-ready smile.
- Café maid costume: cream-and-rosewood maid outfit with delicate rose-gold embroidery, a SMALL stylized sunflower/starburst pin at her temple and a slim ear-mounted headset microphone; no handheld microphone.
- Expression and posture: animated three-quarter shoulder turn, one hand at the heart in a sincere greeting, not an exaggerated idol peace sign.
- Her unmistakable visual signature is: rose-gold half-up twist + turquoise eyes + sunflower starburst + delicate headset.

CHARACTER DIFFERENTIATION:
Closest existing archetype counterpart is Nene (strong magenta-pink hair and turquoise highlight). Also avoid visual collision with Momo's salmon hair and the planned Mikan orange ponytail. Specifically: No vivid magenta or turquoise hair streak, no twin tails, no Mikan-like single ponytail, no oversized stage costume, no floating stage effects.
Use a different facial structure, silhouette, ear/fur markings and expression, not just new hair colors. Preserve a clearly recognizable Idol personality without mimicking poses of the other nine.

PREMIUM SHARED GAME STYLE:
Modern high-quality Japanese 2D anime illustration, coherent with Suzu v03's warm cozy café art direction. Smooth colored outlines with delicate weight variation; expressive eyes; layered colored hair highlights; controlled premium cel shading with light painterly material accents; intricate but organized maid-fabric folds and lace; refined small brass details. Cream and muted burgundy/rosewood remain recurring uniform-family colors, while Himari's personal accent and costume cut distinguish her. Not pixel art, not photorealistic, not a VTuber render, not a generic doll face. No extreme highlights, exaggerated anatomy or unnecessary decorative noise.

FRAMING AND TECHNICAL RULES:
One isolated waist-up character from the tips of BOTH cat ears and headdress to roughly the waist, naturally in a slight three-quarter view, portrait-oriented canvas roughly 3:4. Both eyes and character-defining accessories must be unobscured. Keep at least 5% genuine transparent headroom above the highest ear/hair ornament, and 4% side margins for hair and tail/accessories. Do not flatten ears against the top edge. Central face must survive a small-game-card crop. Avoid having the character's signature outside the card's central head-and-shoulders area. Genuine transparent alpha outside the character; edges clean. No background, no fake checkerboard, no letters, no interface, no card frame, no logo, no watermark, no unrelated props or second character.

FINAL CHECK: Does the image read distinctly as Himari, rather than Suzu, her established archetype counterpart, or a color-swap of the other new candidates? If not, improve the character silhouette and facial identity before rendering.

Output ONE image only. Intended basename: UI-021_maid_portrait_himari_neutral_v01.png.
```

## Local output

- RAW candidate: `art_working/ui_modular/raw_gpt/UI-021_maid_portrait_himari_neutral_v01.png`
- Keep any v02 as an additional file (`_v02.png`) rather than replacing v01.
- Do not copy to approved or integrate into Unity until owner visual approval and QA.
