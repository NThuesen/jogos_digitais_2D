# 05_BUTTON_SECONDARY — Botão secundário discreto, um estado por arquivo.

**Status:** PROMPT PREPARADO, **asset NÃO GERADO/NÃO APROVADO**. Fase: consulte [ordem e gate](../GENERATION_ORDER.md).

## Anexos

style_anchor_reference.png; UI-001; UI-004_normal aprovado; outras versões de UI-004 se relevantes.

Acesse os caminhos em [references/README.md](../references/README.md); **anexe os arquivos de imagem** no ChatGPT antes de colar o prompt.

## Campos a preencher antes de enviar

- [STATE]=NORMAL | HOVER | PRESSED | DISABLED
- [TARGET_ASPECT]=aprox. 3.5:1

## PROMPT DE GERAÇÃO

Cole **antes** o bloco global em [00_GLOBAL_RULES.md](00_GLOBAL_RULES.md); em seguida, copie **somente** o texto abaixo para a geração. Um asset/estado por imagem.

```text
Generate exactly ONE SECONDARY button visual in state [STATE], part of the same family as the attached approved primary button but deliberately less visually dominant. Front-facing scalable button with compact cream/peach interior, delicate colored border, symmetric corners and clean straight bands.

Same dimensions, alpha footprint and corner geometry across states. NORMAL = calm; HOVER = slight light accent; PRESSED = inset subtle shading; DISABLED = reduced contrast without disappearance. Keep ample unadorned central area for Unity's dynamic Back/Cancel/Details label. Do not include any words, icons, four-state sheets or other buttons.
```

## QA e decisões

Não usar cor da ação destrutiva como padrão; sem efeito funcional baked.

**Registro:** [ASSET_REGISTRY.md](../ASSET_REGISTRY.md). Preserve `raw_gpt`, limpe alpha no Blender quando necessário e só integre após validação Unity e aprovação do responsável.
