# 11_ICON_PACK — Um ícone por arquivo, pack lógico via registry.

**Status:** PROMPT PREPARADO, **asset NÃO GERADO/NÃO APROVADO**. Fase: consulte [ordem e gate](../GENERATION_ORDER.md).

## Anexos

style_anchor_reference.png; painel/botão aprovados; captura de tela-alvo opcional.

Acesse os caminhos em [references/README.md](../references/README.md); **anexe os arquivos de imagem** no ChatGPT antes de colar o prompt.

## Campos a preencher antes de enviar

- [ICON]=BOOK | CAT_PAW | CUP | CLOCK | GEAR | STAR | COIN | HEART | GIFT | SHOP_BAG

## PROMPT DE GERAÇÃO

Cole **antes** o bloco global em [00_GLOBAL_RULES.md](00_GLOBAL_RULES.md); em seguida, copie **somente** o texto abaixo para a geração. Um asset/estado por imagem.

```text
Generate ONE small pictogram sprite of type [ICON] for a cozy illustrated anime café management UI. NOT a sheet of ten icons. Consistent with approved buttons/panels: clean simplified silhouette, rounded colored outlines, a restrained two- or three-tone painterly finish, well-centered in a square alpha canvas.

Treat this as an icon meant to remain distinguishable at about 24–44 Unity reference units. Minimal internal linework; avoid highly specific cultural text, brand marks, emoji style, shadows spanning outside the padding or unreadable details. Neutral visual center and a consistent baseline. No words or number.
```

## QA e decisões

Repetir separadamente para os 10 ICON; proporções idênticas para icon atlas posterior.

**Registro:** [ASSET_REGISTRY.md](../ASSET_REGISTRY.md). Preserve `raw_gpt`, limpe alpha no Blender quando necessário e só integre após validação Unity e aprovação do responsável.
