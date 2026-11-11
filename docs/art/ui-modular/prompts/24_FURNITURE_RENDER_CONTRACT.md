# 24 — Contrato comum de renderização — V01

ART-PROD-01: acabamento anime 2D ilustrado; referências pixel art só informam atmosfera.
Antes de gerar, identificar screenshot pós-PR #73 em 960×600 e 1280×720, commit,
maid de referência e objetos atuais. Capturas e calibração ainda pendentes.

## Bloco obrigatório para os oito prompts

```text
Match the current approved Hall reference: orthographic three-quarter view,
consistent camera axes and floor contact. One grid unit is the scale reference;
source PNG dimensions do not define world size. Show exactly ONE illustrated anime
2D object, genuine transparent alpha, no floor tile or baked scenery shadow.
Keep contact points readable, restrained highlights and warm coloured outlines.
Keep table seats visible and a clear front edge for seated-customer occlusion.
Do not obscure stove indicators, customer silhouettes or click targets. Tall objects
must fit the Hall and permit depth sorting. Preserve the specified footprint.
```

## Escala, pivô e orientações

Registrar no handoff: dimensões do master, bounding box alpha, footprint em unidades,
ponto de contato no piso e pivô normalizado (x,y), referência/commit, orientação e
área visual que ultrapassa o footprint. Calibrar no Salão; não inventar PPU/pivô
numérico antes do piloto. Não recortar o master para ajustar margens.

| Ordem | ID | Grade | Etapa |
|---|---|---|---|
| 1 | table_2 | 2×1 | Piloto |
| 2 | stove | 1×1 | Piloto |
| 3 | stool | 1×1 | Expansão |
| 4 | table_4 | 2×2 | Expansão |
| 5 | teabar | 2×1 | Expansão |
| 6 | dessert | 1×1 | Expansão |
| 7 | bakery | 1×2 | Expansão |
| 8 | door | 1×1 | Fixture fixa, fora da Loja |

FurnitureArt mantém suporte a largura/altura. Para table_2, teabar e bakery testar
orientação trocada antes de aprovar. Não presumir que girar a imagem preserva
perspectiva. Se falhar, produzir variante artística específica com sufixo `_rot90`
antes da versão (ex.: `UI-024_placeable_table_2_rot90_v01.png`), preservando master.
Registrar orientação da grade, pivô e teste de ambas; até três fontes adicionais.
Aprovar pilotos antes de completar seis. Integração futura em PR independente,
sem mudar preço, capacidade, efeito ou desbloqueio.
