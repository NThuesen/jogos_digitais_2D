# 15 Mizuki — Oneesan | Chibi → walk still → AI video
**Status:** production PROMPTS only; no generated chibi or video is implied. **Stable identity slug:** `mizuki`. Second-wave owner-approved portrait.

**References:** Image A (required) = [Approved CLEAN in PR #61](https://github.com/NThuesen/jogos_digitais_2D/blob/dbc33fca22fa8d5796371fe0fc7e91f2f72eb693/docs/art/ui-modular/portraits/batch2/mizuki_clean.png). Image B (optional for Step 1 only) = an individually isolated currently **ILLUSTRATED** chibi walk-frame from [Hina's runtime sheet](https://github.com/NThuesen/jogos_digitais_2D/blob/main/Assets/_Game/Resources/Maids/hina_walk.png), or the V3 approved Mikan chibi pilot. B is shared size/finish reference, **not character identity**. The historical Hina PIXEL ART master is prohibited. If the approved portrait conflicts with the textual signature, **the portrait wins**.

**Identity cues:** deep warm-brown skin, long deep wine-burgundy S-wave hair sweeping over one shoulder, sea-green eyes, beauty mark near one eye, large tufted Maine Coon ears and tail; graceful cream/wine maid outfit. **Known hazard:** Preserve her elegant mature facial geometry and skin tone; do not turn her into Kaede or Akari.

## STEP 1 — full-body chibi master (image)

Attach **A = Mizuki approved premium portrait**, **B optional = illustrated chibi style reference**. Copy this block ALONE.

```text
NEKO MAID INC. — MIZUKI (Oneesan) — ILLUSTRATED FULL-BODY CHIBI V01

Create exactly ONE isolated full-body Japanese 2D anime-chibi illustration of the SAME original catgirl maid in IMAGE A. IMAGE A (approved premium portrait) is ABSOLUTE character identity truth for face, skin, hair, eyes, feline ears, tail/fur markings, costume, colors and signature ornamentation. This is a second representation of the SAME person, never a recolor or newly designed catgirl. Optional IMAGE B gives only the CURRENT GAME'S illustrated 2D chibi body type and material finish. Never copy Image B's face, hair, costume or accessory.

NAME/ARCHETYPE: Mizuki, Oneesan.
Distinctive cues from design documentation: deep warm-brown skin, long deep wine-burgundy S-wave hair sweeping over one shoulder, sea-green eyes, beauty mark near one eye, large tufted Maine Coon ears and tail; graceful cream/wine maid outfit.
Critical preservation: Preserve her elegant mature facial geometry and skin tone; do not turn her into Kaede or Akari.
If any cue contradicts the visible approved portrait, FOLLOW THE APPROVED PORTRAIT instead of reconstructing an outdated design specification.

STYLE: Premium illustrated anime 2D with clean colored expressive outlines, rich but disciplined cel-shading, clear warm café materials and delicate highlights; NOT historical pixel art, stepped pixels, 48-color quantization, 3D, plastic CGI, generic anime face, exaggerated anatomy or blurry painterly noise. Approximately 2.9–3.2 heads tall EXCLUDING cat ears: readable large head, compact body and short coherent legs, cute adult-presenting proportion. Preserve recognizability at ~100–130 screen pixels tall. Keep face/skin/silhouette PERSONAL, not cloned from Suzu or Hina.

FULL BODY: Derive a modest short café-service skirt, layered apron, stockings/socks and TWO practical closed shoes from IMAGE A's existing bodice, collar, trim and color family. Add only coherent unseen lower-body pieces. Do NOT invent extra props, hats, glasses, badges or ornamentation. Natural near-front slight three-quarter standing pose, feet stable, both arms relaxed by sides, neutral hands, expression matching the maid's personality. Do NOT copy a portrait wave, arm crossing or hand held at neckline if that would obstruct WALK animation. Dress details may simplify for gameplay readability but essential identity markers remain distinct.

FRAMING: Vertical single portrait canvas (approx 3:4); whole character from highest ear/ahoge/head ornament to soles, BOTH eyes, both hands, both shoes, skirt hem and complete cat tail visible; 5% transparent headroom and 4% transparent side margins; subject approx 80–85% canvas height. Genuine transparent alpha, NO floor, shadow, stage, chroma, scene, UI, words, title, watermark, multi-pose collage or sprite sheet.

FINAL CHECK: It must look unequivocally like Mizuki from IMAGE A at a new FULL BODY chibi scale, not an altered version of IMAGE B. Output exactly ONE beautiful isolated PNG.
```

**Save:** `art_working/15_mizuki/step1_chibi/15_mizuki_step1_chibi_v01.png`. Approve visually before Step 2.

## STEP 2 — right-facing WALK START still (image · safe framing v02)

Attach **A = Mizuki STEP 1 approved chibi**; optional B = her own portrait for checking facial identity. Copy this block ALONE.

```text
NEKO MAID INC. — MIZUKI — RIGHT-FACING 2D WALK-IN-PLACE START FRAME V02

EDIT the approved full-body Mizuki chibi in IMAGE A into exactly ONE SINGLE right-facing side/three-quarter WALK START still for extracting Unity sprite frames. IMAGE A is ABSOLUTE for actual hairstyle, facial structure, skin, clothing, accessories, ears, tail and outline quality; optional image B is her own premium portrait for identity checking. Never turn this into a new character, a sprite sheet or historical pixel art.

Identity signature to preserve: deep warm-brown skin, long deep wine-burgundy S-wave hair sweeping over one shoulder, sea-green eyes, beauty mark near one eye, large tufted Maine Coon ears and tail; graceful cream/wine maid outfit.
Special warning: Preserve her elegant mature facial geometry and skin tone; do not turn her into Kaede or Akari.

Correct right-facing locomotion: turn face, eyes, shoulders, torso, hips, knees and shoes toward SCREEN RIGHT at a genuine 65–75-degree side/three-quarter angle relative to front. One shoe clearly supports her weight slightly ahead, rear shoe is preparing to swing, opposing arm swing, physically sensible leg separation. Avoid front-facing upper body with sideways legs, crossed catwalk steps, running, tiptoe shuffling, moonwalking, idle dancing or wave. Hands free, all original accessories in their own original anatomical sides; do not mirror features.

MANDATORY COMPOSITION — FRAME THE CAMERA WIDE FIRST, THEN KEEP THE CHARACTER SMALL:
- Output a true 16:9 landscape canvas (1920x1080 if possible). FIRST fill the full canvas with the requested pure opaque chroma background. THEN place the unchanged full-body character inside it, smaller than a portrait, at approximately 70–74% of the TOTAL FRAME HEIGHT, NEVER more than 75%. Do not enlarge her to fill the image.
- HARD MEASURABLE MARGINS ON FINAL OUTPUT: Keep at least 13% of the entire canvas HEIGHT completely EMPTY above the highest character pixel (including ponytail/ahoge/cat ears) AND 13% completely EMPTY below the lowest shoe sole. At 1080 px high this means ~140 px uninterrupted solid chroma ABOVE the hairstyle and ~140 px BELOW the feet. Both are mandatory simultaneously, not interchangeable.
- Keep the torso/root near horizontal centre X=50%. Fit ALL non-background pixels, including tail, widest hair, swinging arm and hands, between X=18% and X=82% of the final canvas width: at least 18% unbroken empty chroma at LEFT and RIGHT. This is an animation source with space for future motion, NOT a cropped character close-up.
- Keep the two soles level on an IMAGINARY contact plane within the safe zone; no visible ground line, shadow or floor. If needed, uniformly SCALE DOWN the WHOLE CHIBI instead of cutting ears, hair, tail, hands or legs, or changing her original proportions and clothing. Keep her 2D art faithful to IMAGE A.
- NEVER zoom in, fill the frame, move her head against the top edge, or push shoes against the bottom edge. Prioritize the extra plain background over a larger face.
FINAL OUTPUT CHECK BEFORE RENDER: If any part of her body or accessory enters the top/bottom 13% bands or left/right 18% bands, or the full height exceeds 75% of the final 16:9 canvas, this output is INCORRECT. Recompose with MORE background and a SMALLER character, while keeping the walking pose and identity intact.

Background EXACT TARGET: completely uniform OPAQUE #0000FF pure chroma, edge-to-edge, with no gradient, floor, cast shadow, horizon, glow, checkerboard, texture, objects, text or other figures. Confirm actual chroma pixels and contrast to the real sprite colors before proceeding; the text hex is not a guarantee of real output.

Output exactly ONE still, not a video, not several views, not an animation strip. Same artistically polished 2D anime-chibi Mizuki from image A.
```

**Save:** `art_working/15_mizuki/step2_walkstart/15_mizuki_step2_walkstart_v02_blue.png`. Approve the pose, output-safe margins and chroma before Step 3. Preserve any generated v01 file as historical RAW; use v02 for new videos.

## STEP 3 — image-to-video WALK IN PLACE (Flow / Tencent / equivalent)

Attach **ONLY** the approved step-2 still. Set **16:9**. Copy this block ALONE.

```text
NEKO MAID INC. — MIZUKI — SIDE-ON 2D WALK-IN-PLACE FOR SPRITES

Animate ONLY the supplied right-facing chibi Mizuki into a technically clean, repeatable, natural RIGHT-FACING 2D WALK-IN-PLACE cycle. The input image is the character and style LOCK: same face, eyes, skin, hair geometry, ears, fur patterns, tail, outfit, apron, shoes, accessories, lighting, colored outlines and cel shading on EVERY frame. Do NOT mirror, morph, redraw, change eye side, generate extra limbs/accessories, add props or alter costume colors. Preserve her elegant mature facial geometry and skin tone; do not turn her into Kaede or Akari.

MOTION: One full alternating LEFT-then-RIGHT step pair, ideally repeated at a steady pace. Both feet clearly swing and alternately bear weight; sufficient stride length, toe lift, ankle and knee motion, natural opposing arm swing, modest skirt/hair/ear/tail secondary motion. The torso remains on the SAME screen X (character walks IN PLACE: Unity supplies translation). During each supporting-foot phase, that shoe travels BACKWARD smoothly relative to the stationary torso while the other moves FORWARD, like a proper walking cycle. NO shuffling, skating, moonwalk, dancing, marching, running, crossing legs, jumping or floating. Keep two shoes visible. Make the end pose closely resemble the opening pose if feasible; an actual good loop section will later be selected by human QA/Blender, so do not sacrifice foot contact to force an artificial snap.

CAMERA: ABSOLUTELY LOCKED single 16:9 view, no zoom, orbit, pan, reframe, shake, change of angle, perspective shift or camera cut. Keep entire character including ears, hair tip, hands, apron, both shoes and tail tip IN FRAME at exactly the input scale. She must never translate across the screen.

BACKGROUND: Retain the input's PURE solid OPAQUE #0000FF chroma as closely as possible through all frames, with no generated floor, shadow, horizon, vignette, particles, texture, gradient, text or extra characters. Avoid color spill onto the maid. No fake alpha/checkerboard.

STYLE: Illustrated high-quality Japanese 2D anime chibi, not pixel-art, CGI, 3D, photorealism or cinematic VFX. Prioritize stable character IDENTITY, consistent linework, real foot contact and useful clean sprite frames over beautiful but unusable animation.

Output one uninterrupted fixed-camera horizontal walking-in-place video.
```

**Save original video:** `art_working/15_mizuki/step3_video/15_mizuki_step3_walk_v01.mp4` (or actual supplied container/extension). Next: **select real cycle** in Blender, export numbered frames to `art_working/15_mizuki/step4_blender/walk_x/frames/`; do not assume whole video is a loop or RGBA export removes chroma.
