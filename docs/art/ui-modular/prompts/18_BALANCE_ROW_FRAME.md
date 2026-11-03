# 18_BALANCE_ROW_FRAME — Linha de receita/despesa do Balanço.

**Status:** PROMPT PREPARADO, **asset NÃO GERADO/NÃO APROVADO**. Fase: consulte [ordem e gate](../GENERATION_ORDER.md).

## Anexos

style_anchor_reference.png; UI-001/UI-002 aprovados; screenshot Balanço UI-01A com 13 indicadores.

Acesse os caminhos em [references/README.md](../references/README.md); **anexe os arquivos de imagem** no ChatGPT antes de colar o prompt.

## Campos a preencher antes de enviar

- [TARGET_ASPECT]=440:34 ou 440:28
- [VARIANT]=NORMAL | TOTAL

## PROMPT DE GERAÇÃO

Cole **antes** o bloco global em [00_GLOBAL_RULES.md](00_GLOBAL_RULES.md); em seguida, copie **somente** o texto abaixo para a geração. Um asset/estado por imagem.

```text
Create exactly ONE neutral financial-ledger row background for a cozy café management screen, variant [VARIANT]. Approximate real Unity row 440x28 / 440x34 reference units (very wide and short). Must support a LEFT dynamic item label and RIGHT dynamic value (positive or negative).

Warm illustrated paper/enamel strip with delicate thin border or shallow engraving; subtle contrasting background for data readability. NORMAL = quiet enough to repeat for many entries; TOTAL = slightly more framed and visually decisive but same geometry and family. Keep most of the center plain, no printed line items, digits, yen signs, arrows, icons or red/green fixed values.

Isolated translucent/alpha-edged row, practical rather than ornate.
```

## QA e decisões

Números grandes e moeda ¥ devem ser desenhados por texto Unity; não gerar valor numa textura.

**Registro:** [ASSET_REGISTRY.md](../ASSET_REGISTRY.md). Preserve `raw_gpt`, limpe alpha no Blender quando necessário e só integre após validação Unity e aprovação do responsável.
