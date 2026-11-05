# V3 — 18 novas maids: retrato premium → chibi → vídeo → Blender

**USE ESTE V3 EM VEZ DA V2 HISTÓRICA EM PIXEL ART.** Os nove sprites atuais do jogo são **anime 2D ilustrado**, não pixel art quantizado. Esta versão mantém identidades únicas do catálogo fechado de 27 maids, usa os retratos premium aprovados como única fonte de identidade e cria full-body chibi ANTES do image-to-video. Fonte da mudança: [direção visual vigente](../../CURRENT_VISUAL_DIRECTION.md), [pipeline Walk ART-04](../../maid-walk/README.md) e [V2 histórica](../README.md).

## Abrir a ficha e copiar três prompts na ordem

| # | Maid | Arquétipo | Ficha (CHIBI, WALK START, VÍDEO) |
|---|---|---|---|
| 10 | Mikan | Genki | [Abrir 3 prompts](maids/10_mikan.md) |
| 11 | Yuki | Kuudere | [Abrir 3 prompts](maids/11_yuki.md) |
| 12 | Kurumi | Tsundere | [Abrir 3 prompts](maids/12_kurumi.md) |
| 13 | Nonoka | Dojikko | [Abrir 3 prompts](maids/13_nonoka.md) |
| 14 | Kaguya | Ojousama | [Abrir 3 prompts](maids/14_kaguya.md) |
| 15 | Mizuki | Oneesan | [Abrir 3 prompts](maids/15_mizuki.md) |
| 16 | Himari | Idol | [Abrir 3 prompts](maids/16_himari.md) |
| 17 | Anzu | Gourmand | [Abrir 3 prompts](maids/17_anzu.md) |
| 18 | Kasumi | Mukuchi | [Abrir 3 prompts](maids/18_kasumi.md) |
| 19 | Hibari | Genki | [Abrir 3 prompts](maids/19_hibari.md) |
| 20 | Seira | Kuudere | [Abrir 3 prompts](maids/20_seira.md) |
| 21 | Ruri | Tsundere | [Abrir 3 prompts](maids/21_ruri.md) |
| 22 | Chiyo | Dojikko | [Abrir 3 prompts](maids/22_chiyo.md) |
| 23 | Shiori | Ojousama | [Abrir 3 prompts](maids/23_shiori.md) |
| 24 | Tsubaki | Oneesan | [Abrir 3 prompts](maids/24_tsubaki.md) |
| 25 | Mirai | Idol | [Abrir 3 prompts](maids/25_mirai.md) |
| 26 | Kinako | Gourmand | [Abrir 3 prompts](maids/26_kinako.md) |
| 27 | Mayu | Mukuchi | [Abrir 3 prompts](maids/27_mayu.md) |

**Referências:** cada ficha vincula o CLEAN aprovado da segunda onda (na PR [#61](https://github.com/NThuesen/jogos_digitais_2D/pull/61); Nonoka é V02), e exige o retrato REAL aprovado no caso da última onda, que ainda pode estar apenas no `raw_gpt/` local. Nenhuma imagem da última onda deve ser considerada aprovada apenas porque seu prompt existe. Prioridade absoluta: **PNG de identidade aprovado > especificações textuais históricas**.

1. **Prompt 1 (GPT imagem):** Anexe **A = retrato premium aprovado daquela maid**, **B opcional = sprite chibi ilustrado atual ou piloto Mikan V3 aprovado apenas como ESTILO**. Gere UMA imagem vertical full-body 2D anime-chibi. Não copiar rosto da B, não criar pixel art.
2. **Prompt 2 (GPT imagem):** Anexe **A = CHIBI full-body aprovado**, B opcional = próprio retrato. Gere UMA imagem 16:9 horizontal com passo genuíno para DIREITA e chroma sólido `#00FF00` ou `#0000FF` especificado na ficha. Não gerar um sheet.
3. **Prompt 3 (Flow/Tencent/IA vídeo):** Anexe **SÓ a imagem Walk Start aprovada**, selecione 16:9, cole o prompt de vídeo da ficha. Dois passos alternados, caminhar NO LUGAR, câmera absolutamente fixa, personagem inteira, sem foot sliding e sem alterações de rosto/roupa. Não assuma que a ferramenta oferece um loop perfeito nem que o chroma hex sai exato.
4. **Blender:** assista o vídeo inteiro, escolha um trecho real de ciclo coerente, exporte PNGs numerados com FPS real, identifique/rejeite moonwalk e chroma contaminado. Siga o procedimento [art_working/README.md](../../../../art_working/README.md); remover chroma requer processamento explícito — salvar RGBA não o remove.

**Piloto recomendado: Mikan, de ponta a ponta.** Compare full-body com os nove chibis atuais (mesma altura em tela) e só depois use a Mikan V3 como estilo adicional para os demais 17. O velho Hina pixel-art master NÃO é referência de runtime. A foto do gameplay / sprite Hina atual não substitui a imagem da própria maid.

**Nome/pastas:** números `10` a `27` indicam apenas ordem de produção, não IdentityId persistido. Exemplo:
```text
art_working/10_mikan/step1_chibi/10_mikan_step1_chibi_v01.png
art_working/10_mikan/step2_walkstart/10_mikan_step2_walkstart_v01_blue.png
art_working/10_mikan/step3_video/10_mikan_step3_walk_v01.mp4
art_working/10_mikan/step4_blender/walk_x/frames/frame_0001.png
```
Nunca apagar RAWs/versões; Git ignora fontes, vídeos e muitos frames. Publicar apenas derivados curados depois do QA.

**Gates:** retrato aprovado → chibi aprovado → walk start aprovado → vídeo inspecionado com pé plantado → Blender frames → pipeline `tools/art/maid_walk/maid_walk.py` → QA Unity 960×600 / 1280×720 → futura liberação no MaidCatalog. Nenhum vídeo/PNG significa integração automática. Testar custo/performance WebGL: os nove walk sheets antigos totalizaram ~50,8 MB de texturas RGBA32 com mipmaps no build ART-04; mais 18 terão custo mensurável. Nenhum gasto adicional com IA de vídeo é autorizado.

[Rubrica de identidade, movimento e QA](STYLE_AND_QA.md).
