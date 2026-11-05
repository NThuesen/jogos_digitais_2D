# UI-021 — Hibari | Final wave design master | Genki

**State:** PROMPT DRAFT — no art generated or approved. Third wave of the 27-character catalog; this identity is PLANNED until a portrait and walking animation pass QA.

## Attachments (actual PNGs)

1. [Suzu v03 CLEAN](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/ui-modular/portraits/suzu_clean.png): polished premium finish and rendering reference ONLY.
2. [Original nine character lineup](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/ui-modular/portraits/nove_retratos.png): visual anti-duplication reference.
3. **Important:** add a CONTACT SHEET of the nine new second-wave portraits (Mikan, Yuki, Kurumi, Nonoka, Kaguya, Mizuki, Himari, Anzu, Kasumi) from local `raw_gpt/` after the Claude QA export. Those images are not yet published in the repository; compare to the real rendered designs, not only their prompts.
4. Optional: [real UI detail panel](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/ui-modular/portraits/integration/detail_suzu_960x600.png) for scale only.

## Copy the following prompt to the image-generation model

```text
NEKO MAID INC. — 27 UNIQUE CHARACTER ROSTER — FINAL 9 MAIDS — HIBARI V01

Create exactly ONE original, polished premium Japanese 2D anime half-body portrait of Hibari, a Genki catgirl maid for a cozy illustrated café management and collection game.

DESIGN FUNCTION: This is the definitive IDENTITY DESIGN CANDIDATE for Hibari; it must distinguish her instantly from every one of the other 26 characters. It will be the master artwork for her later animation and card crops. No procedural/recolor characters.

ATTACHMENTS: Suzu v03 clean for ART FINISH ONLY (not her face/clothes/green colors); an original-nine contact sheet and an additional contact sheet of the actual newly generated nine maids for anti-duplication. Do not recreate an existing catgirl by changing hair color.

IDENTITY / VISUAL DIRECTION
Name: Hibari
Archetype: Genki
Skin: deep sun-warmed bronze skin with soft cheek highlights.
Hair: distinctive cropped electric-cyan shag/pixie cut, asymmetric jagged fringe, one free-standing upward cowlick; no ponytail or soft rounded bob.
Eyes: clear warm golden-yellow eyes with wide alert gaze.
Cat ears and tail: seal-point Siamese cat ears: dark brown outer tips, warm beige inner fur, slim dark-tipped tail.
Face and resting expression: sporty broad smile, slightly square cute cheek contours, one small crossed adhesive bandage on the cheek.
Costume: cream and rosewood café maid dress tailored with short sky-blue piping, short fitted sleeves, a tiny brass whistle hanging under the collar, slim practical apron; still unmistakably a maid.
Pose: quick lively shoulder twist, one hand lifting a greeting with confident grin; energetic but different from Mikan's outward wave.
UNIQUE VISUAL SIGNATURE: cyan pixie/ahoge + Siamese dark ear tips + cheek bandage + brass whistle.

STRICT DISTINCTIVENESS CHECK:
Compare especially against Suzu's mint bob and Mikan's bright orange high braided ponytail.
Do NOT: No mint bob, no orange hair, no single high ponytail, no large orange hair ribbons, no generic smile cloned from Suzu or Mikan.
Create a visibly different silhouette, head/ear shape, facial structure, color balance, costume cut, accent symbol and body language. Distinction must remain obvious when viewed as a small portrait, not only at full resolution.

SHARED ART FINISH, NOT SHARED FACE:
Rich but controlled 2D anime painting matching the premium quality of Suzu v03: refined variable-weight clean linework; soft cel shading with selective painterly accents; coherent warm cafe colors; crisp cloth folds, ruffles and restrained metallic highlights; expressive eyes. Every character is a distinct adult-presenting catgirl maid rendered in a coherent anime/chibi blend. Preserve complexity in the main master, letting the game pipeline make smaller crops. Avoid exaggerated anatomy, glossy plastic lighting, pixel art, 3D CGI, photorealism and generic anime faces.

COMPOSITION:
One single isolated 3:4 portrait, ears and head accessory through approximately waist, gentle three-quarter view. Keep both cat ears and both eyes fully visible. At least 5% genuinely transparent empty headroom ABOVE the tallest ear/headdress and >=4% transparent side margins around the outermost hair. The signature feature must survive cropping the head and upper shoulders into a 1.22:1 window for tiny collectible cards. Don't flatten accessories on the canvas edge; do not crop hands in a distracting way.

TECHNICAL:
Genuine transparent-background PNG with clean alpha, isolated character, no fake checkerboard, no scenery, no panels, no text, no logo, no other character or objects floating around her.

CRITICAL: Produce a recognizable original character. In a lineup of all 27 maids she must never look like an existing maid in a different palette.

Output exactly ONE image.
Intended filename: UI-021_maid_portrait_hibari_neutral_v01.png
```

## Filing
- Candidate RAW: `art_working/ui_modular/raw_gpt/UI-021_maid_portrait_hibari_neutral_v01.png`.
- V02, if needed, is a NEW file, never overwrite v01.
- Do not mark APPROVED or RELEASED until QA and matching walk animation exist.
