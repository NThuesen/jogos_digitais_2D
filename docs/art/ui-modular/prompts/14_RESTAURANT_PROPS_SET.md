# 14_RESTAURANT_PROPS_SET — Catálogo de props; cada render contém UM objeto selecionado.

**Status:** PROMPT PREPARADO, **asset NÃO GERADO/NÃO APROVADO**. Fase: consulte [ordem e gate](../GENERATION_ORDER.md).

## Anexos

style_anchor_reference.png; captura Salão; ART-04 maids; piso/parede já aprovados quando disponíveis.

Acesse os caminhos em [references/README.md](../references/README.md); **anexe os arquivos de imagem** no ChatGPT antes de colar o prompt.

## Campos a preencher antes de enviar

- [PROP]=ROUND_TABLE | RECT_TABLE | CHAIR | PLANT | COUNTER_SEGMENT | CAKE_DISPLAY | SHELF | HANGING_LAMP | WALL_DECOR | RUG

## PROMPT DE GERAÇÃO

Cole **antes** o bloco global em [00_GLOBAL_RULES.md](00_GLOBAL_RULES.md); em seguida, copie **somente** o texto abaixo para a geração. Um asset/estado por imagem.

```text
Produce ONE original isolated CAFÉ PROP sprite for [PROP], in the same consistent anime illustrated perspective and softness as the attached approved room architecture and existing illustrated maids. Do not make a contact sheet or place ten props in one image.

Show its recognizable silhouette and enough volume to read in an orthographic 2D management game without perspective exaggeration. Materials: warm painted wood/porcelain/fabric/greenery when appropriate; colored outlines and subtle shadows appropriate to the attached room, restrained surface detail; clean base/footprint, no painted labels or characters. Use true alpha outside, avoid white/green chroma halos.

Align object geometry to the Unity room grid and floor contact point. Keep layer and footprint reasonable: floor props must not visually block adjacent interaction unnecessarily. Object ordering/occlusion/rotation are runtime Unity decisions.
```

## QA e decisões

Um arquivo por PROP, com pivô/footprint registrado; gerar direções alternativas separadas se gameplay realmente exigir.

**Registro:** [ASSET_REGISTRY.md](../ASSET_REGISTRY.md). Preserve `raw_gpt`, limpe alpha no Blender quando necessário e só integre após validação Unity e aprovação do responsável.
