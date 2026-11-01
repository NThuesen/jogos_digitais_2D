# 04 · MOMO — Momo (Dojikko)

**Pipeline v2 — HINA STYLE LOCK.** Esta identidade continua candidata até aprovação visual; a Hina governa o estilo chibi comum.
**Marcadores exclusivos:** bob pêssego, ahoge, orelha direita caída.
**Chroma inicial da caminhada:** `#00FF00`, a confirmar nos pixels reais. Nunca sacrificar cores da personagem ao aplicar chroma.
**Não altera:** `Neko.Sim`, raridade, equipe sorteada, portrait separado ou sprites Unity já integrados.

## Anexos e ordem (OBRIGATÓRIO)

1. **PROMPT 1:** anexar **uma imagem**: o **full-body aprovado da Hina, fundo marfim**, disponível na Biblioteca/arquivos do responsável. Ela é referência de **estilo, anatomia chibi e acabamento pixel-art**, NÃO de identidade para as outras oito. **Não** anexar a antiga cena de café nem personagens adicionais. Para Hina, é também a referência de identidade.
2. **PROMPT 2:** anexar **duas imagens nesta ordem**: (A) a imagem **aprovada do próprio Momo** produzida pelo PROMPT 1 (para Hina, a aprovada original), (B) o **full-body aprovado da Hina** (âncora de estilo). Não anexar a cena do café.
3. **Tencent (vídeo):** usar apenas o PNG chroma aprovado no PROMPT 2 como imagem inicial. O prompt de vídeo e o gate de caminhada estão no [tutorial](../README.md).

**Atenção:** o PNG full-body mestre da Hina **ainda não está versionado no repositório**; deve ser anexado manualmente. Ver [HINA_STYLE_LOCK.md](../HINA_STYLE_LOCK.md). Não continuar se a imagem errada estiver anexada. Os prompts abaixo são **completos**; não colar os prompts-base junto.

## PROMPT 1 — gerar full-body no mesmo padrão artístico da Hina

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
Create MOMO, an ORIGINAL adult Dojikko cat-maid who is clumsy, sincere and disarmingly charming. Her rounded silhouette contrasts the angular Kuudere and dramatic Tsundere: short layered PEACH-PINK hair with a perfectly readable single CURLING AHOGE shaped like a question mark at the crown, two soft rounded locks by her cheeks and soft mauve-lilac eyes. A faint blush, petite surprised mouth and slightly uneven eyebrows communicate a sweet "oops!" personality; never show distress or pain.

Her cat ears are intentionally asymmetric: the RIGHT ear bends slightly outward and downward at the top, while the LEFT ear stands erect. Fur matches peach with warm cream inside. Her tail is softly fluffy at the peach-to-cream tip, shaped like a little comma, not striped and without bells. A single coral star patch on the front apron pocket is her key icon; optionally a tiny clean bandage-shaped embroidered motif on the pocket seam, not a realistic wound on the body. No idol star jewelry or large ribbons; keep the patch readable rather than cluttered.

Dress her in a cream pinafore over a muted dusty-rose café dress, pale-lilac trim on short sleeves, gathered but simple skirt and low brown Mary-Jane shoes with ivory ankle socks. Her uniform can show a slightly crooked apron strap to suggest clumsiness without becoming visually messy. Pose: both feet grounded, slight inward toe, one hand close to apron pocket and the other relaxed, a small apologetic smile. Prioritize the bent right ear and curled ahoge as the two big silhouette identifiers even at 64–96 pixel game scale.

FINAL CHECK
If the identity block suggests longer legs, naturalistic anatomy, a different art technique or a larger/more detailed uniform, keep HINA'S SHARED CHIBI PROPORTIONS/STYLE and adapt the identity marker into them. Do not copy Hina's distinctive personal features. Return only the rendered single full-body image.
```

**Saída:** `04_momo_step1_fullbody_v02.png`. Aprová-la **lado a lado com a Hina na mesma altura** (proporção, contorno, pixel clusters, uniformes) antes de fazer vídeo. A imagem NÃO é um portrait detalhado do card.

## PROMPT 2 — EDITAR a maid aprovada para pose de caminhada horizontal

```text
TASK
EDIT IMAGE A into exactly ONE full-body initial frame for a RIGHTWARD, HORIZONTAL, WALK-IN-PLACE cycle in a 2D Unity café game. Not a video, sheet or collage.

REFERENCES
IMAGE A: authoritative identity, face, hair anatomy, ear/tail asymmetries, proportions, costume details and accessory placement. IMAGE B: the approved HINA master, authoritative for common pixel-art chibi style and proportions. Do not transplant Hina's personal design. Change only walking pose and background; preserve the exact character and the group's visual language.

POSE / PHYSICS
Orient face, gaze, shoulders, hips, knees and feet toward SCREEN RIGHT (a genuinely side-oriented three-quarter view, approx. 65–80° away from front). Do NOT keep face/chest pointing at the viewer while legs mime sideways walking. Show a clear, anatomically coherent stride: one foot contacts the virtual ground, the other advances far enough to create a legible weight-transfer cycle; opposing arm swing; quiet modest body bob. Avoid crossed runway legs, mincing steps, moonwalk, skating, hopping or exaggerated marching. The character stays in the same screen position; Unity translates her later. Camera fixed, no zoom.
Keep the same 2.8–3.0-head compact chibi anatomy, stepped colored outlines and clustered cel shading of IMAGE B. Do not turn into a smooth anime illustration. Maintain exact left/right asymmetry in the character's own anatomy: do NOT mirror her features. Both shoes, tail tip, ears, hands and accessories stay inside frame with generous margins.

BACKGROUND
Pure, uniform, opaque CHROMA #00FF00 from edge to edge. No gradient, soft shadow, halo, ground, horizon, vignette, reflection, transparency checkerboard, text or extra objects. Avoid chroma-color spill or damage to eyes/hair/trim. If the actual foreground overlaps the proposed chroma, select an absent color and document the change.

IDENTITY LOCK
Momo is the peach-pink short-haired maid with ONE curling question-mark ahoge, mauve eyes, bent RIGHT cat ear, cream-tipped fluffy comma-tail, star patch on apron pocket, dusty-rose/cream dress and simple brown Mary-Jane shoes. She walks with a cautious balanced mid-step and gentle slightly nervous expression, not falling or dropping objects. Use pure GREEN #00FF00 chroma; preserve every peach/lilac color, bent-ear direction and the location of the star patch. No additional bandages or stars outside the existing patch.

OUTPUT
One clean full-body STILL only. Preserve IMAGE A identity and IMAGE B artistry; no animation strip, no scene or explanation.
```

**Saída:** `04_momo_step2_walkstart_v02_green.png` (ajustar nome à cor real). A Etapa 2 deve preservar a IDENTIDADE da imagem A e o ESTILO do chibi mestre B. Não aceitar resultado de corpo alongado, pose frontal fingindo andar lateralmente ou fundo com gradiente.

## Gate visual antes de animar

- [ ] **Hina-locked:** altura em cabeças, silhueta, contorno em degraus, 2–3 sombras por material e detalhe visual coerentes com a Hina aprovada.
- [ ] **Identidade:** todos os marcadores específicos desta ficha estão visíveis, no lado anatômico correto; nenhuma característica da Hina foi copiada indevidamente.
- [ ] **Família do uniforme:** estrutura maid equivalente à Hina (avental marfim, vestido curto/modesto, sapato de café) com adaptações de cor/acessório específicas.
- [ ] **Composição:** um full-body, orelhas/cauda/mãos/sapatos inteiros, enquadramento consistente, sem cenário nem texto.
- [ ] **Gameplay:** legível ao reduzir para ~96–128 px de altura, sem perder marcas essenciais.
- [ ] **Walk still:** orientação efetivamente para a direita, transferência de peso e passo legível, sem redesenho ou arte borrada.
- [ ] **Chroma:** fundo uniforme de cor ausente em cabelo, olhos, roupa e acessórios.
- [ ] **Aprovação humana:** atingir pelo menos 9/10 na rubrica de [HINA_STYLE_LOCK.md](../HINA_STYLE_LOCK.md), sem falha eliminatória; registrar versão aprovada/rejeitada. Sem vídeo antes da aprovação.

**Se houver somente um defeito**, peça uma **edição direcionada** da imagem atual (mude apenas o defeito; preserve o resto) em vez de gerar uma maid do zero. Os modelos podem variar; os prompts **não garantem resultado idêntico sem QA**.
