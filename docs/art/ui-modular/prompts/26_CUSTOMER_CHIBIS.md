# 26 — Clientes (7 tipos): chibis ilustrados para substituir o procedural

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

## Caminhada (por cliente)
Seguir o fluxo já usado para as maids em `docs/art/prompt-pipeline/v3/` (frame inicial, vídeo, Blender, processamento), com célula de ≈ 96 px de altura. Nada de caminhada será gerado antes da aprovação do design base.
