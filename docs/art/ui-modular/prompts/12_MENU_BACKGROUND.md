# 12_MENU_BACKGROUND — Fundo cênico Menu (não UI funcional).

**Status:** PROMPT PREPARADO, **asset NÃO GERADO/NÃO APROVADO**. Fase: consulte [ordem e gate](../GENERATION_ORDER.md).

## Anexos

style_anchor_reference.png; captura do Menu UI-01B; UI-001 e botão aprovado (opcional).

Acesse os caminhos em [references/README.md](../references/README.md); **anexe os arquivos de imagem** no ChatGPT antes de colar o prompt.

## Campos a preencher antes de enviar

- [COMPOSITION]=wide 16:9 com safe crop 16:10
- [ZONE]=área calma para UI

## PROMPT DE GERAÇÃO

Cole **antes** o bloco global em [00_GLOBAL_RULES.md](00_GLOBAL_RULES.md); em seguida, copie **somente** o texto abaixo para a geração. Um asset/estado por imagem.

```text
Generate ONE original illustrated ANIME CAFÉ MENU BACKGROUND LAYER — a welcoming visual-novel-style café interior scene, without any interface or characters. Warm window light, wooden counter/books/plants and bakery details are welcome in background scenery, with soft atmospheric depth, elegant color transitions and coherent illustrated linework.

Landscape composition suited to a Unity 1280x720 reference scene, but with a generous safe-content region to accommodate a 960x600 view without stretching (16:9 to 16:10 adaptation by crop or responsive framing). Leave visual negative space where Unity buttons, records, title and optional character art could be placed; no essential prop should depend on the extreme 10% on either side. Do not imitate the exact camera layout or characters of the reference.

No button, panel, text, logo, game title, currency, floating icon or watermark. This is a BACKGROUND ONLY, opaque is acceptable.
```

## QA e decisões

Diferente do restaurante interativo: móveis decorativos podem ficar baked no fundo do Menu, mas não devem mascarar a UI.

**Registro:** [ASSET_REGISTRY.md](../ASSET_REGISTRY.md). Preserve `raw_gpt`, limpe alpha no Blender quando necessário e só integre após validação Unity e aprovação do responsável.
