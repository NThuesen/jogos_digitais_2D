# 02 · HINA — Hina (Kuudere)

> **ARQUIVO DE PROMPTS HISTÓRICOS.** Identidade e referências de personagem permanecem úteis, mas instruções de *pixel art* não são o estilo de runtime aprovado. As nove maids já usam [Walk anime 2D ilustrado](../../maid-walk/README.md); siga a [direção vigente](../../CURRENT_VISUAL_DIRECTION.md) e não regenere frames sem uma tarefa específica.


**Pipeline v2 — HINA STYLE LOCK.** Hina é a referência-mestra de estilo já escolhida pelo responsável. Recriar só se houver necessidade; não invalidar o original aprovado.
**Marcadores exclusivos:** bob azul-noite, olhos gelo, presilha triangular prata.
**Chroma inicial da caminhada:** `#00FF00`, a confirmar nos pixels reais. Nunca sacrificar cores da personagem ao aplicar chroma.
**Não altera:** `Neko.Sim`, raridade, equipe sorteada, portrait separado ou sprites Unity já integrados.

## Anexos e ordem (OBRIGATÓRIO)

1. **PROMPT 1:** anexar **uma imagem**: o **full-body aprovado da Hina, fundo marfim**, disponível na Biblioteca/arquivos do responsável. Ela é referência de **estilo, anatomia chibi e acabamento pixel-art**, NÃO de identidade para as outras oito. **Não** anexar a antiga cena de café nem personagens adicionais. Para Hina, é também a referência de identidade.
2. **PROMPT 2:** anexar **duas imagens nesta ordem**: (A) a imagem **aprovada do próprio Hina** produzida pelo PROMPT 1 (para Hina, a aprovada original), (B) o **full-body aprovado da Hina** (âncora de estilo). Não anexar a cena do café.
3. **Tencent (vídeo):** usar apenas o PNG chroma aprovado no PROMPT 2 como imagem inicial. O prompt de vídeo e o gate de caminhada estão no [tutorial](../README.md).

**Atenção:** o PNG full-body mestre da Hina **ainda não está versionado no repositório**; deve ser anexado manualmente. Ver [HINA_STYLE_LOCK.md](../HINA_STYLE_LOCK.md). Não continuar se a imagem errada estiver anexada. Os prompts abaixo são **completos**; não colar os prompts-base junto.

## PROMPT 1 — gerar full-body no mesmo padrão artístico da Hina

```text
TASK
Generate exactly ONE original adult cat-maid full-body character design for the same 2D pixel-art chibi game as ATTACHED IMAGE A, the APPROVED HINA MASTER. Not a card, portrait crop, sheet, render of a café or written description.

REFERENCE ROLES AND PRIORITY
IMAGE A (HINA): absolute reference for shared visual language only — chibi body geometry, compact proportions, line weight/pixel clusters, shading, outline, material rendering, maid-uniform family and framing. The CHARACTER IDENTITY below controls the NEW character's hair, eyes, ears, tail, personal colors, expression, accessories and outfit variations. For HINA, IMAGE A is BOTH the already approved identity AND the group's style reference: preserve Hina's navy bob, ice-blue eyes, triangular clip, face and navy/ivory palette. Do not redesign her. Do NOT attach the café scene or another character.

SHARED STYLE LOCK — NON-NEGOTIABLE
- Replicate Hina's compact adult chibi type: approximately 2.8–3.0 heads tall from scalp to shoes (exclude cat ears); large head about one-third of non-ear height; rounded face; short torso and compact arms/legs. Not a tall, elongated anime illustration.
- Actual intentional 2D pixel-art appearance: crisp stepped dark colored contours, coherent pixel clusters, simplified surfaces, 2–3 discrete cel-shading values per material and small controlled highlights. NO smooth digital anime painting, gradient airbrushing, vectors, 3D look, fake pixel noise or postprocessed pixelation.
- Same underlying maid costume language: short/modest A-line café dress, ivory ruffled apron, visible collar/bodice/cuffs, neat hem, socks and closed service shoes. Preserve shared silhouette/complexity, but customize colors and selected details as stated in the identity block.
- Same facial language/lineweight as Hina, but original face expression and identity. Soft warm diffuse upper-left light; subtle cool shadows. Similar graphic density at the same apparent size.
- Single centered almost-front standing pose, hands/ears/feet/tail entirely visible, relaxed balanced posture; character ~80–86% of frame height; portrait ~3:4 aspect ratio if supported.
- Plain uniform warm-light ivory background; NO café scenery, shadows on ground, vignette, gradients, frames, UI, text, logos, duplicates, motion blur, watermarks or props.
- The result must visually belong to the **same character lineup as Hina** even after shrinking to 96–128 px. Same artistic type, NOT the same individual.

CHARACTER IDENTITY — CHANGE ONLY THESE APPROVED VARIABLES
Create HINA, an ORIGINAL adult Kuudere cat-maid: composed, quietly observant and technically competent. Distinctly unlike Suzu, her silhouette is sharply geometric: a precise midnight-NAVY bob cut that ends just below her jaw on the right, with a longer left face-framing lock falling to her collarbone. A slim SILVER-BLUE underlayer peeks out when her hair turns, never a whole second hairstyle. A single thin silver triangular barrette holds one lock above her right temple. Her eyes are pale icy BLUE with narrow calm lids, small understated glints and cool relaxed brows; reserved expression, subtle closed-mouth smile, no angry scowl.

Her cat ears are narrow navy-black triangles with light silver-grey inner fur, symmetrically high but matching her restrained attitude. The tail is sleek navy, fine and mostly straight, with one small silver-grey crescent-shaped tip marking; not fluffy, striped or cream. Avoid bells and pink heart motifs. Her recognizable garment is an immaculate dark-navy and ivory maid dress with a crisp cream apron, angular pleated hem, subtle powder-blue piping, white cuff edges and low-heeled black shoes. Add a compact silver collar clasp instead of a large ribbon, and no additional jewelry beyond the barrette and clasp.

Use a self-possessed standing posture, hands lightly folded at waist level, head held still, conveying a meticulous worker. Small facial markings should be simple enough to remain stable in later video. Silver underlayer and asymmetric haircut must survive the isolated walking still. Lighting remains cozy from the style anchor, but her signature cooler palette should give her a memorable contrast within a warm café without turning the character photoreal or gothic.

FINAL CHECK
If the identity block suggests longer legs, naturalistic anatomy, a different art technique or a larger/more detailed uniform, keep HINA'S SHARED CHIBI PROPORTIONS/STYLE and adapt the identity marker into them. For the HINA target, preserve the exact approved identity features from IMAGE A. Return only the rendered single full-body image.
```

**Saída:** `02_hina_step1_fullbody_v02.png`. Aprová-la **lado a lado com a Hina na mesma altura** (proporção, contorno, pixel clusters, uniformes) antes de fazer vídeo. A imagem NÃO é um portrait detalhado do card.

## PROMPT 2 — EDITAR a maid aprovada para pose de caminhada horizontal

```text
TASK
EDIT IMAGE A into exactly ONE full-body initial frame for a RIGHTWARD, HORIZONTAL, WALK-IN-PLACE cycle in a 2D Unity café game. Not a video, sheet or collage.

REFERENCES
IMAGE A: authoritative identity, face, hair anatomy, ear/tail asymmetries, proportions, costume details and accessory placement. IMAGE B: the approved HINA master, authoritative for common pixel-art chibi style and proportions. For Hina, preserve precisely her existing approved personal design; this is an identity-preserving pose edit. Change only walking pose and background; preserve the exact character and the group's visual language.

POSE / PHYSICS
Orient face, gaze, shoulders, hips, knees and feet toward SCREEN RIGHT (a genuinely side-oriented three-quarter view, approx. 65–80° away from front). Do NOT keep face/chest pointing at the viewer while legs mime sideways walking. Show a clear, anatomically coherent stride: one foot contacts the virtual ground, the other advances far enough to create a legible weight-transfer cycle; opposing arm swing; quiet modest body bob. Avoid crossed runway legs, mincing steps, moonwalk, skating, hopping or exaggerated marching. The character stays in the same screen position; Unity translates her later. Camera fixed, no zoom.
Keep the same 2.8–3.0-head compact chibi anatomy, stepped colored outlines and clustered cel shading of IMAGE B. Do not turn into a smooth anime illustration. Maintain exact left/right asymmetry in the character's own anatomy: do NOT mirror her features. Both shoes, tail tip, ears, hands and accessories stay inside frame with generous margins.

BACKGROUND
Pure, uniform, opaque CHROMA #00FF00 from edge to edge. No gradient, soft shadow, halo, ground, horizon, vignette, reflection, transparency checkerboard, text or extra objects. Avoid chroma-color spill or damage to eyes/hair/trim. If the actual foreground overlaps the proposed chroma, select an absent color and document the change.

IDENTITY LOCK
Keep Hina's asymmetric navy bob with longer left lock, silver-blue underlayer, pale blue eyes, right-side triangular silver barrette, navy cat ears with silver interiors, slim dark tail with grey crescent tip, crisp ivory/navy apron dress and silver collar clasp. She moves with measured precision, a short balanced mid-step rather than dancing. Use pure GREEN #00FF00 chroma; her design has no saturated green foreground. No bells, new ponytails, mirrored haircut, duplicate accessories or extra props.

OUTPUT
One clean full-body STILL only. Preserve IMAGE A identity and IMAGE B artistry; no animation strip, no scene or explanation.
```

**Saída:** `02_hina_step2_walkstart_v02_green.png` (ajustar nome à cor real). A Etapa 2 deve preservar a IDENTIDADE da imagem A e o ESTILO do chibi mestre B. Não aceitar resultado de corpo alongado, pose frontal fingindo andar lateralmente ou fundo com gradiente.

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
