# 20_LOGO_WORDMARK — Marca estática do jogo — exceção ao texto baked.

**Status:** PROMPT PREPARADO, **asset NÃO GERADO/NÃO APROVADO**. Fase: consulte [ordem e gate](../GENERATION_ORDER.md).

## Anexos

style_anchor_reference.png; UI-001/UI-004 aprovados; screenshot Menu atual; grafia do título confirmada.

Acesse os caminhos em [references/README.md](../references/README.md); **anexe os arquivos de imagem** no ChatGPT antes de colar o prompt.

## Campos a preencher antes de enviar

- [EXACT_TEXT]=NEKO MAID INC.
- [LAYOUT]=horizontal center

## PROMPT DE GERAÇÃO

Cole **antes** o bloco global em [00_GLOBAL_RULES.md](00_GLOBAL_RULES.md); em seguida, copie **somente** o texto abaixo para a geração. Um asset/estado por imagem.

```text
Generate ONE ORIGINAL STATIC GAME TITLE LOGO, with EXACT visible wording: "NEKO MAID INC." Keep spelling, punctuation and order exact — all-caps English title; no extra subtitle. This is the one deliberate EXCEPTION where permanent branding text can be baked into artwork, subject to manual lettering QA.

Illustrated café/visual-novel identity: elegant readable lettering with subtle rosewood/cream treatment, warm inviting pastel accent, small cat-ear/teacup flourish integrated without making the title difficult to parse. Suitable for Unity Menu display over an illustrated café background, clean silhouette and transparent outside. Not a complete title screen, no backdrop, no characters and no buttons.

Avoid too much tiny serif ornament or invisible thin strokes at 960x600. If the generator cannot reliably render EXACT text, produce a textless logo emblem separately and the title must instead be typeset in Unity.
```

## QA e decisões

CONFERIR cada letra do título; caso errada, NÃO publicar logo com texto incorreto.

**Registro:** [ASSET_REGISTRY.md](../ASSET_REGISTRY.md). Preserve `raw_gpt`, limpe alpha no Blender quando necessário e só integre após validação Unity e aprovação do responsável.
