# 07_MODAL_DIALOG — Janela de confirmação sobreposta; fundo e decoração.

**Status:** PROMPT PREPARADO, **asset NÃO GERADO/NÃO APROVADO**. Fase: consulte [ordem e gate](../GENERATION_ORDER.md).

## Anexos

style_anchor_reference.png; UI-001 painel aprovado; screenshot DG-1 diálogo (UI-01B).

Acesse os caminhos em [references/README.md](../references/README.md); **anexe os arquivos de imagem** no ChatGPT antes de colar o prompt.

## Campos a preencher antes de enviar

- [TARGET_ASPECT]=aprox. 2.1:1
- [ZONE]=título, corpo, botões

## PROMPT DE GERAÇÃO

Cole **antes** o bloco global em [00_GLOBAL_RULES.md](00_GLOBAL_RULES.md); em seguida, copie **somente** o texto abaixo para a geração. Um asset/estado por imagem.

```text
Design ONE front-facing modular MODAL DIALOG graphic with an ornamental illustrated border, sturdy cream body and reserved places for (a) title line at upper center, (b) multiline body in broad central section and (c) up to two dynamic Unity action buttons along the lower edge. Do not paint any buttons or headings; just provide enough safe negative space.

Proportion close to the existing Unity dialog (620x300 reference units, ratio about 2.1:1). Neutral warm panel, delicate illustrated rosewood trim, subtle important-modal emphasis, corner ornaments away from content. Transparent outside, opaque readable cream center, no character art. Sufficient clean margin for a large localized string.

Do NOT draw a complete UI, modal shade, dummy letters or a UI screenshot.
```

## QA e decisões

Backdrop escurecido (modal shade) é componente Unity; hitboxes e bloqueio de cliques não vêm da imagem.

**Registro:** [ASSET_REGISTRY.md](../ASSET_REGISTRY.md). Preserve `raw_gpt`, limpe alpha no Blender quando necessário e só integre após validação Unity e aprovação do responsável.
