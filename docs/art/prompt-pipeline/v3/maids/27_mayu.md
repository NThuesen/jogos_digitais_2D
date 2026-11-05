# 27 Mayu — Mukuchi | Chibi → walk still → AI video
**Status:** production PROMPTS only; no generated chibi or video is implied. **Stable identity slug:** `mayu`. Final-wave portrait must be approved by owner before chibi generation.

**References:** Image A (required) = Local approved premium portrait: `art_working/ui_modular/raw_gpt/UI-021_maid_portrait_mayu_neutral_vXX.png` (owner must select approved version; V01 is not automatically approved). Image B (optional for Step 1 only) = an individually isolated currently **ILLUSTRATED** chibi walk-frame from [Hina's runtime sheet](https://github.com/NThuesen/jogos_digitais_2D/blob/main/Assets/_Game/Resources/Maids/hina_walk.png), or the V3 approved Mikan chibi pilot. B is shared size/finish reference, **not character identity**. The historical Hina PIXEL ART master is prohibited. If the approved portrait conflicts with the textual signature, **the portrait wins**.

**Identity cues:** pale cool-beige skin, neat low indigo SIDE KNOT, subtle mismatched muted amber/smoke-grey irises, black ears with white tips and black tail white tip, resting hood behind shoulders. **Known hazard:** Keep both eyes visible and iris sides correct; hood is behind the head, never obscures ears.

## STEP 1 — full-body chibi master (image)

Attach **A = Mayu approved premium portrait**, **B optional = illustrated chibi style reference**. Copy this block ALONE.

```text
NEKO MAID INC. — MAYU (Mukuchi) — ILLUSTRATED FULL-BODY CHIBI V01

Create exactly ONE isolated full-body Japanese 2D anime-chibi illustration of the SAME original catgirl maid in IMAGE A. IMAGE A (approved premium portrait) is ABSOLUTE character identity truth for face, skin, hair, eyes, feline ears, tail/fur markings, costume, colors and signature ornamentation. This is a second representation of the SAME person, never a recolor or newly designed catgirl. Optional IMAGE B gives only the CURRENT GAME'S illustrated 2D chibi body type and material finish. Never copy Image B's face, hair, costume or accessory.

NAME/ARCHETYPE: Mayu, Mukuchi.
Distinctive cues from design documentation: pale cool-beige skin, neat low indigo SIDE KNOT, subtle mismatched muted amber/smoke-grey irises, black ears with white tips and black tail white tip, resting hood behind shoulders.
Critical preservation: Keep both eyes visible and iris sides correct; hood is behind the head, never obscures ears.
If any cue contradicts the visible approved portrait, FOLLOW THE APPROVED PORTRAIT instead of reconstructing an outdated design specification.

STYLE: Premium illustrated anime 2D with clean colored expressive outlines, rich but disciplined cel-shading, clear warm café materials and delicate highlights; NOT historical pixel art, stepped pixels, 48-color quantization, 3D, plastic CGI, generic anime face, exaggerated anatomy or blurry painterly noise. Approximately 2.9–3.2 heads tall EXCLUDING cat ears: readable large head, compact body and short coherent legs, cute adult-presenting proportion. Preserve recognizability at ~100–130 screen pixels tall. Keep face/skin/silhouette PERSONAL, not cloned from Suzu or Hina.

FULL BODY: Derive a modest short café-service skirt, layered apron, stockings/socks and TWO practical closed shoes from IMAGE A's existing bodice, collar, trim and color family. Add only coherent unseen lower-body pieces. Do NOT invent extra props, hats, glasses, badges or ornamentation. Natural near-front slight three-quarter standing pose, feet stable, both arms relaxed by sides, neutral hands, expression matching the maid's personality. Do NOT copy a portrait wave, arm crossing or hand held at neckline if that would obstruct WALK animation. Dress details may simplify for gameplay readability but essential identity markers remain distinct.

FRAMING: Vertical single portrait canvas (approx 3:4); whole character from highest ear/ahoge/head ornament to soles, BOTH eyes, both hands, both shoes, skirt hem and complete cat tail visible; 5% transparent headroom and 4% transparent side margins; subject approx 80–85% canvas height. Genuine transparent alpha, NO floor, shadow, stage, chroma, scene, UI, words, title, watermark, multi-pose collage or sprite sheet.

FINAL CHECK: It must look unequivocally like Mayu from IMAGE A at a new FULL BODY chibi scale, not an altered version of IMAGE B. Output exactly ONE beautiful isolated PNG.
```

**Save:** `art_working/27_mayu/step1_chibi/27_mayu_step1_chibi_v01.png`. Approve visually before Step 2.

## STEP 2 — right-facing WALK START still (image)

Attach **A = Mayu STEP 1 approved chibi**; optional B = her own portrait for checking facial identity. Copy this block ALONE.

```text
NEKO MAID INC. — MAYU — RIGHT-FACING 2D WALK-IN-PLACE START FRAME V01

EDIT the approved full-body Mayu chibi in IMAGE A into exactly ONE SINGLE right-facing side/three-quarter WALK START still for extracting Unity sprite frames. IMAGE A is ABSOLUTE for actual hairstyle, facial structure, skin, clothing, accessories, ears, tail and outline quality; optional image B is her own premium portrait for identity checking. Never turn this into a new character, a sprite sheet or historical pixel art.

Identity signature to preserve: pale cool-beige skin, neat low indigo SIDE KNOT, subtle mismatched muted amber/smoke-grey irises, black ears with white tips and black tail white tip, resting hood behind shoulders.
Special warning: Keep both eyes visible and iris sides correct; hood is behind the head, never obscures ears.

Correct right-facing locomotion: turn face, eyes, shoulders, torso, hips, knees and shoes toward SCREEN RIGHT at a genuine 65–75-degree side/three-quarter angle relative to front. One shoe clearly supports her weight slightly ahead, rear shoe is preparing to swing, opposing arm swing, physically sensible leg separation. Avoid front-facing upper body with sideways legs, crossed catwalk steps, running, tiptoe shuffling, moonwalking, idle dancing or wave. Hands free, all original accessories in their own original anatomical sides; do not mirror features.

Frame composition: one LANDSCAPE 16:9 fixed view (1920x1080 if available), character centered at ~70–82% frame height, feet and ears/tail/hair not clipped; empty lateral space to permit animation. Torso/character root stays at fixed screen X; later the Unity engine handles world movement.

Background EXACT TARGET: completely uniform OPAQUE #00FF00 pure chroma, edge-to-edge, with no gradient, floor, cast shadow, horizon, glow, checkerboard, texture, objects, text or other figures. Confirm actual chroma pixels and contrast to the real sprite colors before proceeding; the text hex is not a guarantee of real output.

Output exactly ONE still, not a video, not several views, not an animation strip. Same artistically polished 2D anime-chibi Mayu from image A.
```

**Save:** `art_working/27_mayu/step2_walkstart/27_mayu_step2_walkstart_v01_green.png`. Approve pose & chroma before Step 3.

## STEP 3 — image-to-video WALK IN PLACE (Flow / Tencent / equivalent)

Attach **ONLY** the approved step-2 still. Set **16:9**. Copy this block ALONE.

```text
NEKO MAID INC. — MAYU — SIDE-ON 2D WALK-IN-PLACE FOR SPRITES

Animate ONLY the supplied right-facing chibi Mayu into a technically clean, repeatable, natural RIGHT-FACING 2D WALK-IN-PLACE cycle. The input image is the character and style LOCK: same face, eyes, skin, hair geometry, ears, fur patterns, tail, outfit, apron, shoes, accessories, lighting, colored outlines and cel shading on EVERY frame. Do NOT mirror, morph, redraw, change eye side, generate extra limbs/accessories, add props or alter costume colors. Keep both eyes visible and iris sides correct; hood is behind the head, never obscures ears.

MOTION: One full alternating LEFT-then-RIGHT step pair, ideally repeated at a steady pace. Both feet clearly swing and alternately bear weight; sufficient stride length, toe lift, ankle and knee motion, natural opposing arm swing, modest skirt/hair/ear/tail secondary motion. The torso remains on the SAME screen X (character walks IN PLACE: Unity supplies translation). During each supporting-foot phase, that shoe travels BACKWARD smoothly relative to the stationary torso while the other moves FORWARD, like a proper walking cycle. NO shuffling, skating, moonwalk, dancing, marching, running, crossing legs, jumping or floating. Keep two shoes visible. Make the end pose closely resemble the opening pose if feasible; an actual good loop section will later be selected by human QA/Blender, so do not sacrifice foot contact to force an artificial snap.

CAMERA: ABSOLUTELY LOCKED single 16:9 view, no zoom, orbit, pan, reframe, shake, change of angle, perspective shift or camera cut. Keep entire character including ears, hair tip, hands, apron, both shoes and tail tip IN FRAME at exactly the input scale. She must never translate across the screen.

BACKGROUND: Retain the input's PURE solid OPAQUE #00FF00 chroma as closely as possible through all frames, with no generated floor, shadow, horizon, vignette, particles, texture, gradient, text or extra characters. Avoid color spill onto the maid. No fake alpha/checkerboard.

STYLE: Illustrated high-quality Japanese 2D anime chibi, not pixel-art, CGI, 3D, photorealism or cinematic VFX. Prioritize stable character IDENTITY, consistent linework, real foot contact and useful clean sprite frames over beautiful but unusable animation.

Output one uninterrupted fixed-camera horizontal walking-in-place video.
```

**Save original video:** `art_working/27_mayu/step3_video/27_mayu_step3_walk_v01.mp4` (or actual supplied container/extension). Next: **select real cycle** in Blender, export numbered frames to `art_working/27_mayu/step4_blender/walk_x/frames/`; do not assume whole video is a loop or RGBA export removes chroma.
