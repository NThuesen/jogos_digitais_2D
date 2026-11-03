# 08_MAID_CARD_COMPACT — Moldura colecionável compacta para gameplay/Elenco/Escalação; piloto P0.

**Status:** PROMPT PREPARADO, **asset NÃO GERADO/NÃO APROVADO**. Fase: consulte [ordem e gate](../GENERATION_ORDER.md).

## Anexos

style_anchor_reference.png; UI-001 aprovado; captura das maids ilustradas WebGL; captura Elenco ou Escalação.

Acesse os caminhos em [references/README.md](../references/README.md); **anexe os arquivos de imagem** no ChatGPT antes de colar o prompt.

## Campos a preencher antes de enviar

- [TARGET_ASPECT]=150:206
- [VARIANT]=COMMON_BASE

## PROMPT DE GERAÇÃO

Cole **antes** o bloco global em [00_GLOBAL_RULES.md](00_GLOBAL_RULES.md); em seguida, copie **somente** o texto abaixo para a geração. Um asset/estado por imagem.

```text
Create ONE isolated VERTICAL COMPACT MAID CARD FRAME, designed as an empty, modular collectible character card for Neko Maid Inc. Use the actual current Unity MaidCard base proportion 150 reference units wide x 206 high (ratio ~0.728), and ensure ornamentation survives reduction to 0.62x scale on the Lineup screen.

Architecture: a clearly designated upper portrait window occupying roughly the top 45–50%; an uncluttered band for a dynamic name below; restrained bottom safe region for two short stat rows, salary or rarity markers. The portrait area must be a truly transparent cut-out (so existing maid art can sit BEHIND this frame). The other information zones may use painted cream backing. Provide separate decorative border and fixed corners; no maid baked into frame.

Cozy illustrated anime visual-novel finish, subtle café/paw motif, rosewood + cream hypothesis, collectible charm, but minimal noisy detail. No rarity word, no role label, no stats, no portrait, no stickers crossing the photo window. Output only the empty frame.
```

## QA e decisões

Conferir recorte do portrait sem apagar os elementos creme do rodapé. No piloto, usar arte EXISTENTE da maid atrás; portraits novos em prompt 21 separado.

**Registro:** [ASSET_REGISTRY.md](../ASSET_REGISTRY.md). Preserve `raw_gpt`, limpe alpha no Blender quando necessário e só integre após validação Unity e aprovação do responsável.
