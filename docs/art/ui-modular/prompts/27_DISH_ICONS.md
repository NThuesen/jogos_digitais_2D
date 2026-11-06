# 27 — Pratos e bebidas (14 receitas): ícones ilustrados

**Status:** PROMPTS apenas; nenhuma imagem gerada, nada importado. Hoje cada receita é uma forma geométrica colorida (`Recipe.ShapeId` + `TintHex` em `Sim/Service/RecipeBook.cs`): um círculo, uma cápsula, uma estrela. Estes ícones substituem esses placeholders no balão de pedido (único uso atual; bandejas e cartões de receita não existem hoje, mas devem servir a eles no futuro). Ver [auditoria](../../CUSTOMERS_DISHES_AUDIT.md).

**Anexos:** **A (obrigatório, ESTILO)** = `art_working/ui_modular/raw_gpt/UI-011_icon_cup_v01.png` (ou outro ícone `UI-011_*` aprovado) para espessura de contorno e acabamento. B opcional = `UI-011_icon_coin_v01.png`.

**Saída:** um PNG por receita, transparente, 512×512 (será derivado para 128×128 na integração; o balão é pequeno), em `art_working/ui_modular/raw_gpt/` como `UI-027_dish_<id>_v01.png` (ids na tabela). Sem texto nem preço no desenho.

## Prompt-base (copie UM bloco inteiro, trocando `[PRATO]`)

```text
NEKO MAID INC. — DISH ICON — [PRATO] — V01

Create exactly ONE isolated 2D illustrated icon of the food or drink described below, in the same finish as the attached IMAGE A (style reference only: clean warm-brown coloured outline, soft cel-shading, small specular highlights, cozy anime café feel; do not copy IMAGE A's subject).

SUBJECT: [PRATO: description below]. Shown as served: on a small plate or in a cup/glass as appropriate, three-quarter top-down view, centred, slightly cute and appetising, simplified for gameplay readability at 40–56 screen pixels.

STYLE: premium illustrated anime 2D, NOT pixel art, NOT 3D, NOT photographic. Warm cream and café palette; the dish's dominant colour (given) must dominate the silhouette. No steam smoke clutter, no hands, no characters, no text, no numbers, no UI frame, no price.

FRAMING: square 1:1 canvas, subject fills about 78 % with 8 % transparent margin on every side. Genuine transparent alpha. NO table, shadow plate beyond the dish itself, background or watermark.

Output exactly ONE PNG.
```

## Pratos

| `id` | Nome no jogo | `[PRATO]` (descrição) | Cor dominante |
|---|---|---|---|
| `milk` | Leite morno | A cup of warm milk in a cream ceramic mug with a tiny paw-print on it | `#F5EFE2` |
| `omurice` | Omurice | Classic omurice, golden omelette over rice with a ketchup swirl heart | `#F2C14E` |
| `curry` | Curry da casa | Japanese curry rice on a plate, rich brown curry, a cat-ear-shaped carrot slice | `#B5651D` |
| `ramen` | Lámen do chef | Ramen bowl with chashu, soft egg, nori and a naruto fish cake | `#C9893F` |
| `omurice_deluxe` | Omurice imperial | A grander omurice with a glossy demi-glace crown, gold-leaf sparkle and a small cat-shaped flag | `#E8A33C` |
| `matcha` | Matcha | Matcha latte in a glass cup with milk-foam cat face | `#7FA86A` |
| `bubble_tea` | Chá de bolhas | Bubble tea in a clear cup with tapioca pearls and a wide straw | `#C9A27E` |
| `royal_tea` | Chá real | Elegant porcelain teacup and saucer, violet tea, a tiny gold spoon | `#8E6FA8` |
| `pancake` | Panqueca de gato | Stack of two fluffy pancakes with a cat face drawn in syrup and butter | `#E8B974` |
| `parfait` | Parfait | Tall glass parfait with strawberries, cream and a wafer, pink layers | `#F2A6C0` |
| `sundae` | Sundae estelar | Sundae in a footed glass with a star-shaped cookie and a lilac swirl | `#B98FD1` |
| `parfait_royal` | Parfait real | Opulent parfait, taller, orchid pink, edible gold flakes and a small crown topper | `#D98FC0` |
| `catbun` | Pãozinho de gato | Round bread bun with two cat ears, a faint face, golden crust | `#E0B98A` |
| `cake` | Bolo decorado | Decorated round slice-style cake, pink frosting, strawberry and ribbon piping | `#F2C9D6` |

## Balão de pedido (opcional, 1 prompt)

Se quiser substituir também o balão procedural: use o prompt de `19_DIALOG_BALLOON.md` (já existe) e mantenha estes ícones como conteúdo separado, escalados pela Unity.
