# Prompt-base — Etapa 1 (Hina-locked chibi full-body)

> **PROMPT HISTÓRICO DE PRODUÇÃO — não define mais o estilo de runtime.** Este texto documenta as imagens/vídeos que deram origem às nove maids. Desde o [ART-04](../maid-walk/README.md), o jogo usa **sprites anime 2D ilustrados**, sem quantização para pixel art. A [decisão atual](../CURRENT_VISUAL_DIRECTION.md) prevalece; preserve os frames originais e não gere novamente personagens para forçar pixelização. Para novas imagens, é preciso revisar estes prompts explicitamente em tarefa futura.


**Uso:** contrato para novas fichas; as nove fichas atuais já incluem o texto completo. **Não** concatenar com elas. A imagem full-body aprovada da Hina deve ser anexada como **IMAGEM A**. Ver [HINA_STYLE_LOCK.md](HINA_STYLE_LOCK.md).

```text
TASK
Generate exactly ONE original adult cat-maid full-body character design for the same 2D pixel-art chibi game as ATTACHED IMAGE A, the APPROVED HINA MASTER. Not a card, portrait crop, sheet, render of a café or written description.

REFERENCE ROLES AND PRIORITY
IMAGE A (HINA): absolute reference for shared visual language only — chibi body geometry, compact proportions, line weight/pixel clusters, shading, outline, material rendering, maid-uniform family and framing. The CHARACTER IDENTITY below controls the NEW character's hair, eyes, ears, tail, personal colors, expression, accessories and outfit variations. NEVER copy Hina's navy bob, ice-blue eyes, triangle clip, face or exact palette. Do NOT attach the café scene or another character.

SHARED STYLE LOCK — NON-NEGOTIABLE
- Replicate Hina's compact adult chibi type: approximately 2.8–3.0 heads tall from scalp to shoes (exclude cat ears); large head about one-third of non-ear height; rounded face; short torso and compact arms/legs. Not a tall, elongated anime illustration.
- Actual intentional 2D pixel-art appearance: crisp stepped dark colored contours, coherent pixel clusters, simplified surfaces, 2–3 discrete cel-shading values per material and small controlled highlights. NO smooth digital anime painting, gradient airbrushing, vectors, 3D look, fake pixel noise or postprocessed pixelation.
- Same underlying maid costume language: short/modest A-line café dress, ivory ruffled apron, visible collar/bodice/cuffs, neat hem, socks and closed service shoes. Preserve shared silhouette/complexity, but customize colors and selected details as stated in the identity block.
- Same facial language/lineweight as Hina, but original face expression and identity. Soft warm diffuse upper-left light; subtle cool shadows. Similar graphic density at the same apparent size.
- Single centered almost-front standing pose, hands/ears/feet/tail entirely visible, relaxed balanced posture; character ~80–86% of frame height; portrait ~3:4 aspect ratio if supported.
- Plain uniform warm-light ivory background; NO café scenery, shadows on ground, vignette, gradients, frames, UI, text, logos, duplicates, motion blur, watermarks or props.
- The result must visually belong to the **same character lineup as Hina** even after shrinking to 96–128 px. Same artistic type, NOT the same individual.

CHARACTER IDENTITY — CHANGE ONLY THESE APPROVED VARIABLES
{{CHARACTER_IDENTITY_BLOCK}}

FINAL CHECK
If the identity block suggests longer legs, naturalistic anatomy, a different art technique or a larger/more detailed uniform, keep HINA'S SHARED CHIBI PROPORTIONS/STYLE and adapt the identity marker into them. Do not copy Hina's distinctive personal features. Return only the rendered single full-body image.
```
