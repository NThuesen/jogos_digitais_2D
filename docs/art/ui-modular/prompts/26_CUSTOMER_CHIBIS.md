# 26 — Clientes (7 tipos, 8 designs): chibis ilustrados para substituir o procedural

**Status:** PROMPTS apenas (escopo decidido: **8 clientes**, cada um com base + caminhada + sentado; ver [auditoria](../../CUSTOMERS_DISHES_AUDIT.md)); nenhuma imagem gerada, nada importado. Hoje os clientes são figuras procedurais (`CustomerView` + `SpriteFactory`) ao lado de maids ilustradas, o maior desencontro visual que sobrou no Salão. Os tipos vêm de `CustomerType` (`Sim/Service/CustomerTable.cs`): Regular, Student, Salaryman, Cat, Otaku, Critic, Vip. Os clientes são desenhados a 72 % do tamanho das maids (`CustomerView.CustomerScale`), então precisam ler bem a ~85–95 px de altura.

**Anexos:** **A (obrigatório, ESTILO apenas)** = um quadro individual da caminhada ilustrada atual (ex.: um recorte de `Assets/_Game/Resources/Maids/hina_walk.png`) para escala e acabamento; **nunca** copiar rosto, cabelo ou roupa dela. O retrato/walk de qualquer maid NÃO define identidade de cliente.

**Saída:** um PNG por tipo, fundo transparente real, salvo em `art_working/ui_modular/raw_gpt/` como `UI-026_customer_<tipo>_v01.png` (ex.: `UI-026_customer_student_v01.png`). Depois: CLEAN no mesmo pipeline dos retratos, e só então integração (fora deste arquivo).

## Prompt-base (copie UM bloco inteiro, trocando só o `[TIPO]`)

```text
NEKO MAID INC. — CUSTOMER CHIBI — [TIPO] — V01

Create exactly ONE isolated full-body 2D anime-chibi illustration of a CAFE CUSTOMER (a human guest, NOT a maid, NOT a catgirl, no maid uniform, no headdress, no apron) matching the finish of the attached IMAGE A (style reference only: illustrated chibi proportion, clean coloured outline, soft cel-shading, warm cozy-café palette; never copy IMAGE A's face, hair or clothes).

CUSTOMER: [TIPO + cues below].

PROPORTION: 2.8–3.1 heads tall, large readable head, compact body, short legs; legible at 85–95 screen pixels tall. Neutral, standing, three-quarter view slightly facing right, both feet visible, arms relaxed. One clear silhouette feature that identifies the type at a glance. Original character, NOT a copy of any existing anime character or real person.

STYLE: premium illustrated anime 2D, NOT pixel art, NOT 3D, NOT photoreal. Warm earthy palette with ONE accent colour for the type (given below), consistent with the maids' vinous reds and cream.

FRAMING: vertical 3:4 canvas, whole character from top of head to soles, 5 % transparent headroom and 4 % side margins, subject about 80–85 % of canvas height. Genuine transparent alpha. NO floor, shadow, scene, text, UI, watermark, collage or sprite sheet.

Output exactly ONE PNG.
```

## Tipos e sinais de leitura (acento = `AccentHex` do jogo)

| Tipo | `[TIPO]` e cues | Acento |
|---|---|---|
| Regular (`regular`) | "Everyday regular": young adult in a simple cardigan or jacket, tote bag, relaxed smile. Plain, friendly, the default guest. | `#A89078` |
| Student (`student`) | "Student": school-style blazer and skirt or trousers, backpack, a book under one arm, slightly sleepy eyes. | `#7FA8C4` |
| Salaryman (`salaryman`) | "Salaryman on a lunch break": grey-lilac suit, loosened tie, briefcase, glances at a wristwatch (hurried posture). | `#6E6A80` |
| Cat (`cat`) | "A real cat customer": an adorable chubby domestic cat sitting upright like a patron, cream coat with soft tabby patches, tiny bell collar, content half-closed eyes. A cat, NOT a catgirl; chibi-simplified, 2.0 heads of body ratio is fine. | `#C7B299` |
| Otaku (`otaku`) | "Devoted fan": enthusiastic young adult in a casual hoodie covered with small idol pins and a lanyard, tote with a charm, glowing eager eyes, a small light-stick in one hand. | `#C47FD1` |
| Critic (`critic`) | "Food critic": middle-aged person with round glasses, beret, long coat or tweed jacket, small notebook and pencil held ready, thoughtful raised eyebrow. | `#8C7B5A` |
| Vip (`vip`) | "VIP patron": elegant adult in a rich plum-and-gold coat, fur-trimmed collar, gloves, cane or fan, tasteful jewelry, serene confident posture. Must read as clearly wealthier than the others. | `#D1A54F` |

Após gerar: ver `docs/art/ui-modular/OUTPUT_MANIFEST.md` para o nome exato e pedir a revisão do dono antes de qualquer CLEAN.

## 8º cliente — Regular B (aprovado em 2026-11-06)
**Regular B** é a segunda aparência do tipo mais frequente (peso 10), com o mesmo acento `#A89078`, mas cabelo, roupa e silhueta claramente distintos de Regular A. Exemplo: jovem de jaqueta curta e fones de ouvido, sem mochila de estudante, boné de salaryman nem acessórios de otaku. Nome do arquivo: `UI-026_customer_regular_b_v01.png`. Usar o prompt-base com `[TIPO]` = "Everyday regular, alternate look: young adult, short contrasting haircut, casual short jacket, headphones around neck, friendly relaxed expression; visually distinct from Regular A.". A escolha entre Regular A/B será somente visual e determinística no runtime, sem novo `CustomerType`.

## Sentado (um por cliente, a partir do design aprovado)
Anexo A = o design base aprovado do próprio cliente (identidade); anexo B opcional = imagem de uma cadeira de café. Mesmo estilo, mesma paleta.

```text
NEKO MAID INC. — CUSTOMER SEATED — [TIPO] — V01

Same character as the attached IMAGE A (keep face, hair, outfit, colours and accessory exactly). Redraw ONLY as a SEATED pose: three-quarter view slightly facing right, sitting upright on an invisible chair, knees bent, hands resting forward as if over a table edge, relaxed expression. Show the full body down to the shoes; no chair, no table, no floor shadow drawn.

Same illustrated anime-chibi finish, 2.8–3.1 heads tall, transparent background, 3:4 canvas, subject about 80 % of the canvas, 5 % margins. No text, UI, watermark or collage. Exactly ONE PNG.
```
Saída: `UI-026_customer_<tipo>_seated_v01.png`. Para o Cat: pose de gato sentado de frente para a mesa.

## Caminhada (por cliente) — dois prompts: PNG inicial → MP4 no Google Flow

Use **estes dois blocos em sequência**, depois de aprovar o chibi base. Eles se aplicam aos **8 designs visuais** (7 CustomerType + Regular B), sem precisar trocar texto nem preencher placeholders: **a identidade vem sempre da imagem anexada**. O Cat é um gato real e precisa andar em quatro patas.

**Importante:** o STEP 2 gera **PNG 16:9 com fundo CHROMA OPACO**, não PNG com alpha. O STEP 3 recebe **somente** esse PNG no Flow e gera MP4, cujo fundo será removido depois. Evite colocar a imagem chibi 3:4 diretamente no Flow: ela não garante perfil ou margens adequadas. A pose sentada é **outra imagem**, criada separadamente na seção anterior.

- Chroma padrão: **azul puro `#0000FF`**. Se o cliente tiver detalhes azul puro que poderiam desaparecer na remoção de fundo, substitua `#0000FF` por **`#00FF00`** nos **dois prompts** antes de usar. Não escolha cor presente no personagem.
- A imagem original aprovada determina rosto, corpo, roupa, acessórios, paleta e acabamento. Não inventar outra aparência no WALK START. **A foto anexada deve ser apenas a do próprio cliente**, sem referências de outros personagens.
- Objetivo de enquadramento igual ao [V3 das maids](../../prompt-pipeline/v3/README.md): personagem de **70–74% da altura, máximo 75%**; **≥13%** vazio acima/abaixo e **≥18%** vazio à esquerda/direita. Necessário para não cortar cabelo, bolsa, mãos e sapatos ao animar. A célula final do spritesheet (~96 px de altura) será derivada **após** o vídeo; não é a resolução do STEP 2.

### STEP 2 — gerar uma imagem PNG pronta para animação

**Na ferramenta de imagens:** anexar **A = chibi base aprovado** (exemplo: `UI-026_customer_regular_v01.png`). Pedir **uma imagem PNG 16:9** (idealmente 1920×1080). Copiar este bloco inteiro:

```text
NEKO MAID INC. — CUSTOMER — RIGHT-FACING WALK START — IMAGE TO IMAGE V01

Using the single approved full-body illustrated chibi customer in IMAGE A as ABSOLUTE character identity reference, create EXACTLY ONE new full-body STILL image of the SAME customer, now in a technically correct right-facing WALK START pose for later image-to-video animation and 2D Unity sprites.

IDENTITY LOCK: Retain the exact approved face, eyes, hairstyle and hair accessories, skin or fur, body proportions, outfit, shoes, tote bag/backpack/other accessories, accent colors, clean colored outlines and soft cel-shading. Preserve the original side of every asymmetric detail; never mirror or redesign the customer. If this is the human Regular with a tote bag, keep the same bag attached to the same shoulder; don't make it float. Do not introduce a maid outfit, apron, catgirl ears, new accessories or another character.

PROFILE AND GAIT: Rotate the WHOLE body into a STRICT FULL SIDE PROFILE, facing SCREEN RIGHT, approximately 85–90 degrees away from front view. The face, shoulders, torso, hips, knees and shoes ALL face right, consistently. For a human customer: show a real mid-stride/contact pose, one foot ahead supporting weight and the other slightly trailing, visible separation of the shoes, natural opposing arm swing, free hands, believable body balance. Not a frontal face with sideways legs, not a neutral stand, not a runway pose, dancing or running. If IMAGE A is the REAL CAT customer, preserve realistic feline FOUR-PAW anatomy and create a side-on cat walking contact pose instead, NEVER a bipedal catgirl.

WIDE FRAME COMPOSITION IS MANDATORY: Real 16:9 landscape (1920x1080 when possible). Place the customer/root at horizontal center, X=50%. Character height from the highest hair, ear or accessory to the lowest shoe/paw: approximately 70–74% of the TOTAL image height, NEVER above 75%. Leave AT LEAST 13% of TOTAL image height as completely empty background ABOVE the highest pixel AND 13% BELOW the lowest pixel. Leave AT LEAST 18% empty background on EACH SIDE, including the tote, hair and all props. Keep a constant imaginary foot-contact plane inside the safe area. If needed, SCALE DOWN THE ENTIRE CHARACTER uniformly, instead of cropping or stretching any part.

BACKGROUND: Flat, perfectly uniform, solid OPAQUE PURE #0000FF chroma blue across the ENTIRE frame. No alpha transparency, gradients, floor, shadows, horizon, highlights, checkerboard, objects, text, UI or other characters. No chroma spill on the drawing.

STYLE: High-quality original illustrated Japanese 2D anime chibi, consistent with IMAGE A, NOT pixel art, 3D, photoreal or plastic CGI. Preserve the character at full-body scale with crisp silhouettes.

OUTPUT: Exactly ONE horizontal 16:9 PNG still. NOT a video, NOT multiple poses, NOT a spritesheet. Before rendering, ensure strict right-facing profile, original identity, BOTH shoes/paws visible, and all safety margins.
```

**Salvar, exemplo Regular:** `art_working/customers/regular/step2_walkstart/regular_step2_walkstart_v01_blue.png`. Para outro cliente, usar a pasta `art_working/customers/<id>/step2_walkstart/` com seu slug: `regular_b`, `student`, `salaryman`, `cat`, `otaku`, `critic`, `vip`. Para chroma verde, mudar sufixo para `_green.png`. **Verificar PNG e aprovar perfil/margens antes do vídeo.**

### STEP 3 — Google Flow, transformar o PNG em MP4 de caminhada

**No Flow:** usar modo imagem→vídeo, anexar **apenas o PNG aprovado do STEP 2**, escolher **16:9**. Se houver opção de duração, preferir clipe contínuo de 6–8 segundos. Não pressupor alpha verdadeiro ou loop perfeito. Copiar somente este bloco:

```text
NEKO MAID INC. — CUSTOMER — RIGHT-FACING WALK IN PLACE — IMAGE TO VIDEO V01

Animate ONLY the customer shown in the supplied approved WALK-START IMAGE into a clean, natural, technically usable RIGHT-FACING 2D WALK-IN-PLACE cycle for Unity sprite frame extraction.

ABSOLUTE VISUAL LOCK: The image is the unchangeable reference for this exact customer's face, hair, eyes, clothes, shoes, skin/fur, bag/accessories, color palette, outlines, proportions, silhouette and illustrated 2D cel shading. Preserve every distinctive asymmetrical feature on the same side. The customer MUST stay in strict SIDE PROFILE facing SCREEN RIGHT throughout ALL frames. NEVER turn toward the camera, reverse, flip, re-design, change facial structure, add extra limbs, alter clothes, lose a bag or morph accessories.

REAL WALK CYCLE: Show one complete pair of ALTERNATING left/right steps with natural rhythm, ideally repeated smoothly. Feet visibly alternate between contact, lift and swing; knees and ankles bend correctly. Supporting foot travels smoothly backward relative to the stationary torso while the opposite leg swings forward; steps are long enough to convey real walking. Add modest opposing arm swing, subtle hair and clothing movement, a gently swaying tote bag that remains attached to its original shoulder, and slight natural body bob. No skating, moonwalk, tiny shuffles, feet stuck together, both legs swinging at once, crossing legs, marching, dancing, running, jumping, floating or instantaneous pose changes. If this is the REAL CAT customer, animate a believable FOUR-PAW feline walk, alternating front/hind paws naturally with a small tail sway; NEVER a human walk or catgirl.

WALK IN PLACE ONLY: Keep the character's torso/root fixed at SAME horizontal screen X as in the input. Do NOT travel across the picture. Unity will move the character through the room later. Foot contact should look coherent relative to the torso. Keep the entire character at the source image scale, fully inside the safe margins in all frames.

CAMERA: Absolutely fixed and locked, 16:9. NO zoom, crop, camera pan, tilt, orbit, tracking, cuts, parallax, reframing or changing perspective. No change of body orientation or size.

BACKGROUND: Retain exactly the same solid OPAQUE PURE #0000FF chroma blue from the source image in every frame, with no gradient, shadow, floor, horizon, decorations, checkerboard, new objects, text, vignette or particles. Avoid blue halos/spill and background flicker. Do not pretend the MP4 has alpha.

STYLE: Stable hand-illustrated 2D anime-chibi, soft cel shading and colored linework; no pixel art, 3D or CGI. Prioritize consistent character identity, clear alternating foot contacts and clean usable frames over cinematic movement. Try to end near the starting walk pose, but DO NOT abruptly snap to force a loop: a usable loop segment will be manually chosen from the video in Blender.

OUTPUT: ONE uninterrupted horizontal 16:9 image-to-video MP4 showing a right-facing walk IN PLACE.
```

**Salvar, exemplo Regular:** `art_working/customers/regular/step3_video/regular_step3_walk_v01.mp4` (ou extensão real exportada).

### STEP 4 — selecionar frames no Blender (não confundir com os prompts)

1. Assistir ao MP4 inteiro e selecionar **somente um trecho de ciclo coerente**, com alternância e perfil lateral estável; descartar take com rotação para câmera, deslizamento extremo, cortes ou deformações.
2. Remover o chroma explicitamente no Blender. Exportar como PNG RGBA **não apaga automaticamente** o fundo azul/verde.
3. Exportar PNGs numerados preservando FPS/ordem em `art_working/customers/<id>/step4_blender/walk_x/frames/`. **Nunca apagar os arquivos originais.**
4. Criar pipeline/configuração de **clientes separado de `maids.json`**, visando células de aproximadamente 96 px de altura, e medir memória/qualidade após a otimização WebGL da PR #67. QA visual em 960×600 e 1280×720 antes de integrar ao jogo.

