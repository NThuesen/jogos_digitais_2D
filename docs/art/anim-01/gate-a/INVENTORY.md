# Inventário ANIM-01 / Gate A

Estado atual: [intervenção excepcional encerrada sem aprovação](exception/README.md).

Base: PR #18 `c550e66bb1ab58f59d7d4c766094581873905ba5`, dependente do PR #15 `747e677`.

## Fontes preservadas

Originais fornecidos pelo responsável, localizados em Downloads e copiados sem alterações. Nenhum serviço externo foi usado.

| Arquivo | Dimensões | SHA-256 | Correspondência com PR #18 |
| --- | --- | --- | --- |
| [lado.png](sources/lado.png) | 1254×1254 | `76671b8ecee8ff393a42e79e864daa851d2b41d7b65d987d574c582cb795b8a7` | side |
| [costas.png](sources/costas.png) | 1216×1294 | `c172c802dc819aae7950e9732e6c0239a4b4d96f76114636945c270ee3398a79` | back |
| [v01_chibi_idle.png](sources/v01_chibi_idle.png) | 1214×1295 | `17bbed23b01c29663e05118e6ff0d0ae855aa8ccc46a3aff6b8bb3daf32ed076` | idle |
| [v01_chibi_walk.png](sources/v01_chibi_walk.png) | 1214×1295 | `3550188df131198432bf6682dc9c84d4e42c6f16196225d2761c17a1e7d9633b` | walk |
| [v01_chibi_carry.png](sources/v01_chibi_carry.png) | 1225×1284 | `c36bc314a85175d695652c46840b995cf63ae56ccf5381fa29515490eb066877` | carry |
| [v01_chibi_side_b_v2.png](sources/v01_chibi_side_b_v2.png) | 1254×1254 | `a45719109af54742a95d14b8ef391bbf29e14f0d02b2222ae5c5c039ee6e411a` | Nova referência preferencial; não usada no processamento antigo |
| [v01_chibi_back_b_v2.png](sources/v01_chibi_back_b_v2.png) | 1216×1293 | `3bd84082edfb490e96ea8ed71e305615d647b1b844634895782c8c81a1d39426` | Referência adicional; não usada no kit |

## Peças

29 sprites, 21 nós. Coordenadas e retângulos em pixels do original 1254×1254, origem superior esquerda. Nenhum redimensionamento das peças.

Cabeça inclui rosto, orelhas, franja e presilha solar permanente. A massa de cabelo traseiro visível e a cauda têm articulação própria. Garters, mangas, punhos, laço traseiro, vestido e avental são módulos de roupa separados da anatomia. Sapatos são separados da pele visível dos pés.

As reconstruções são aproximações locais: caps de sobreposição e reparos raster. Os dois patches de tecido da montagem anterior foram substituídos pela [intervenção excepcional](exception/README.md), que também fechou a geometria do sapato distante e limpou resíduos do próximo. `torso_skin` é uma base oculta reconstruída; não é um torso extraído integralmente. Regiões ocultas ainda apresentam defeitos nas poses, conforme relatório atual.

| Peça | Nó / pivô | Ordem | Recorte x,y,w,h | Caps reconstruídos |
| --- | --- | ---: | --- | ---: |
| `tail_base` | `tail_base` | 0 | 323,502,112,219 | 1 |
| `tail_tip` | `tail_tip` | 1 | 195,413,211,183 | 1 |
| `far_thigh` | `far_thigh` | 2 | 656,768,150,181 | 2 |
| `far_shin` | `far_shin` | 3 | 660,882,146,194 | 2 |
| `far_foot_skin` | `far_foot` | 4 | 654,1026,110,102 | 1 |
| `far_shoe` | `far_foot` | 5 | 612,1025,160,152 | 0 |
| `far_sleeve` | `far_upper_arm` | 6 | 815,592,41,42 | 0 |
| `far_garter` | `far_thigh` | 6 | 691,823,115,94 | 0 |
| `far_upper_arm` | `far_upper_arm` | 7 | 797,605,87,77 | 2 |
| `far_forearm` | `far_forearm` | 8 | 834,636,97,72 | 2 |
| `far_hand` | `far_hand` | 8 | 904,652,109,99 | 1 |
| `far_cuff` | `far_forearm` | 9 | 870,628,89,105 | 0 |
| `near_thigh` | `near_thigh` | 10 | 596,751,111,198 | 2 |
| `near_shin` | `near_shin` | 11 | 586,886,112,202 | 2 |
| `near_foot_skin` | `near_foot` | 12 | 529,1011,137,200 | 1 |
| `near_shoe` | `near_foot` | 13 | 528,1011,178,205 | 0 |
| `torso_skin` | `torso` | 14 | 686,456,119,177 | 1 |
| `near_garter` | `near_thigh` | 14 | 609,839,87,64 | 0 |
| `back_bow` | `pelvis` | 15 | 451,570,152,112 | 0 |
| `skirt` | `pelvis` | 16 | 422,613,464,264 | 0 |
| `bodice` | `torso` | 17 | 615,428,248,229 | 0 |
| `apron` | `pelvis` | 18 | 690,628,164,169 | 0 |
| `near_upper_arm` | `near_upper_arm` | 19 | 573,498,135,161 | 2 |
| `near_forearm` | `near_forearm` | 20 | 554,601,88,120 | 2 |
| `near_hand` | `near_hand` | 21 | 452,693,144,114 | 1 |
| `near_cuff` | `near_forearm` | 22 | 544,664,78,87 | 0 |
| `near_sleeve` | `near_upper_arm` | 23 | 597,453,148,161 | 0 |
| `head` | `head` | 24 | 0,0,1254,1254 | 1 |
| `ponytail` | `ponytail` | 25 | 224,96,431,464 | 1 |

## Hierarquia e pivôs

Referência de chão `(650,1215)`, PPU 1175. A conversão para Unity inverte Y. O pivô de articulação é o Transform; o sprite recortado usa pivô central e offset explícito.

| Nó | Pai | Pivô x,y |
| --- | --- | --- |
| `root` | `(harness)` | 650,1215 |
| `pelvis` | `root` | 714,792 |
| `torso` | `pelvis` | 729,607 |
| `head` | `torso` | 749,450 |
| `ponytail` | `head` | 611,230 |
| `headwear_socket` | `head` | 738,143 |
| `tail_base` | `pelvis` | 495,691 |
| `tail_tip` | `tail_base` | 365,560 |
| `near_upper_arm` | `torso` | 672,520 |
| `near_forearm` | `near_upper_arm` | 611,630 |
| `near_hand` | `near_forearm` | 573,715 |
| `far_upper_arm` | `torso` | 815,603 |
| `far_forearm` | `far_upper_arm` | 859,659 |
| `far_hand` | `far_forearm` | 924,698 |
| `near_thigh` | `pelvis` | 668,804 |
| `near_shin` | `near_thigh` | 652,917 |
| `near_foot` | `near_shin` | 612,1058 |
| `far_thigh` | `pelvis` | 744,817 |
| `far_shin` | `far_thigh` | 757,915 |
| `far_foot` | `far_shin` | 685,1048 |
| `accessory_socket` | `torso` | 780,583 |

## Poses experimentais

Ângulos são deltas locais em graus Unity, não parâmetros aprovados para caminhada. Todos os ângulos voltam a zero antes de aplicar a próxima pose.

| Pose | Deltas locais |
| --- | --- |
| `neutral` | zero |
| `near_leg_forward` | near_thigh: 24°, near_shin: -12°, near_foot: -6°, far_thigh: -12°, far_shin: 14° |
| `far_leg_forward` | far_thigh: 24°, far_shin: -12°, far_foot: -6°, near_thigh: -12°, near_shin: 14° |
| `opposite_arms` | near_upper_arm: -23°, near_forearm: 12°, far_upper_arm: 23°, far_forearm: -12° |
| `joint_flexion` | near_thigh: 10°, near_shin: -35°, far_thigh: -8°, far_shin: -28°, near_forearm: -38°, far_forearm: 35° |
| `head_rotation` | head: 7° |
| `occlusion` | near_thigh: 20°, far_thigh: -15°, near_upper_arm: 18°, near_forearm: -24°, head: -5°, ponytail: 7°, tail_base: -6°, tail_tip: 9° |

Receita exata: [build_kit.py](build_kit.py) e [recipe.json](recipe.json). Dados consumidos pelo Unity: [rig.json](../../../../Assets/_Game/Resources/Anim01GateA/rig.json). Hashes de cada PNG estão no contrato.
