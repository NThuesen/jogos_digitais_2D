# 19_DIALOG_BALLOON — Balão de fala visual novel modular (cauda opcional separada).

**Status:** PROMPT PREPARADO, **asset NÃO GERADO/NÃO APROVADO**. Fase: consulte [ordem e gate](../GENERATION_ORDER.md).

## Anexos

style_anchor_reference.png; UI-001 aprovado; tela de diálogo UI-01B.

Acesse os caminhos em [references/README.md](../references/README.md); **anexe os arquivos de imagem** no ChatGPT antes de colar o prompt.

## Campos a preencher antes de enviar

- [TYPE]=RECTANGULAR_PANEL | POINTER_ONLY

## PROMPT DE GERAÇÃO

Cole **antes** o bloco global em [00_GLOBAL_RULES.md](00_GLOBAL_RULES.md); em seguida, copie **somente** o texto abaixo para a geração. Um asset/estado por imagem.

```text
Create ONE illustrated visual-novel DIALOGUE BALLOON module of type [TYPE]. If RECTANGULAR_PANEL: wide rounded cream speech area with delicate rosewood border, slightly more intimate than the general modal, with plenty of blank interior for dynamically wrapped text; symmetric scalable edges. If POINTER_ONLY: just a small matching triangular/curved speech-tail ornament that can be positioned or hidden by Unity independently.

This asset is a single front-on sprite, not an interactive dialogue system or screenshot. True alpha outside. If a tail is present, ONLY generate it when POINTER_ONLY is selected, never permanently attach an asymmetric pointer to a supposed 9-slice panel.

No characters, no speech bubbles in the background, no text, no fake quotation marks.
```

## QA e decisões

Gerar painel e cauda em dois arquivos se precisar, para evitar deformação da cauda ao redimensionar.

**Registro:** [ASSET_REGISTRY.md](../ASSET_REGISTRY.md). Preserve `raw_gpt`, limpe alpha no Blender quando necessário e só integre após validação Unity e aprovação do responsável.
