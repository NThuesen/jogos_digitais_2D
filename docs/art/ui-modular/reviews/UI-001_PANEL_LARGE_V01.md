# Revisão técnica — UI-001 `panel_large` v01

**Estado do asset: `APPROVED` (decisão do responsável, ver [seção 6](#6-aprovação-final)); `INTEGRATED`: pendente.** A recomendação técnica das seções 1–5 foi APPROVE com restrições. O PNG aprovado está versionado em `docs/art/ui-modular/approved/UI-001_panel_large_v01.png`. Nada foi integrado ao jogo (inalterado após a aprovação): nenhuma tela, `Neko.Sim`, save, câmera, HUD, teste protegido ou workflow foi tocado.

Evidências: [`evidence/UI-001/`](evidence/UI-001/). Rodado em 2026-11-03, Unity 6000.5.9f1, Windows.

## 1. PNG técnico

| Item | Raw (`raw_gpt/`, **intacto**) | Limpo (`blender_clean/`) |
| --- | --- | --- |
| Dimensões / formato | 1536×1024, RGBA 8 bits | idem |
| SHA-256 | `d5631b7f…7f22a` | `ba300ece…ee85f8` |
| Bytes | 1 953 848 | 1 833 033 |
| Alpha | 0–**254** (nenhum pixel 255), 14,2 % = 0, 85,8 % parcial | 0–255, 83,8 % = 255, 14,3 % = 0 |
| Componentes conexos (alpha>0) | **195** (1 corpo + 194 fragmentos) | **1** |
| Corpo da moldura (alpha>128) | x 27–1509, y 45–944, 1 componente | idem |
| Buracos transparentes internos | 0 | 0 |
| Centro creme | média RGB ≈ (253, 229, 195), σ ≤ 2,3 | idem |

**Alpha real: confirmado.** O arquivo **não** precisa de recorte: o fundo fora da moldura já é alpha 0 (verificado também compondo sobre magenta: zero pixel fora do corpo).

Problemas encontrados (medidos):
1. **Alpha máximo 254**: o painel inteiro, inclusive o creme "sólido", tinha alpha 252–254 (1 272 607 px em 253), nunca 255. Efeito visual ≤ 4,5/255 sobre branco, mas viola a intenção de superfície sólida e faria painéis sobrepostos transluzirem.
2. **194 fragmentos soltos com alpha = 1** (646 px, 2–56 px do corpo). Invisíveis, mas sujam o bounding box (alpha>0: x 14–1521, y 0–1000 contra corpo 27–1509 × 45–944) e a medição de trim.
3. Halos: **nenhum**. Pixels de borda parcial (alpha 20–250) são escuros (RGB médio ≈ 100/44/54, contorno + sombra de contato), 0 pixels com luminância > 200; o RGB sob alpha 0 é rosewood escuro, sem franja clara.

Correção realizada ([`clean_alpha.py.txt`](evidence/UI-001/clean_alpha.py.txt)): só (a) alpha dos 646 px residuais → 0 e (b) alpha ≥ 250 → 255. **RGB idêntico em 100 % dos pixels; 0 pixels de antialiasing/sombra (alpha < 250) alterados; sem quantização, redução, recorte ou mudança de geometria.** Diferença visível máxima na composição: 4,5/255 sobre branco, 4,3 sobre preto. Técnica: processamento programático (Blender não ofereceria ganho mensurável). Comparação: [`before_after_light_dark.png`](evidence/UI-001/before_after_light_dark.png) (claro/escuro, raw à esquerda) e [`corner_TL_magenta_before_after.png`](evidence/UI-001/corner_TL_magenta_before_after.png).

## 2. 9-slice

Borders **derivados da análise de invariância das bandas** (não arbitrários): os cantos ocupam ≈247 px (largura) e ≈197 px (altura) a partir do corpo (limiar de cor 10), mais as margens transparentes/sombra (27 esq., 26 dir., 45 topo, 79 base). Resultado: **esq 280 / base 280 / dir 280 / topo 250** px de origem. Teste de costura (passo de cor na emenda ÷ passo típico da banda; ≈1 = invisível), 880×586, escala 0,5: bordas 280/250 → **1,41 / 1,37**; 320/290 → 1,40 / 1,79; 250/220 → 5,88 / 4,77; 200/180 → 3,08 / 4,09. Bordas menores cortam ornamentos e emendam visivelmente.

Cantos são copiados 1:1 (nenhuma deformação); só as bandas retas e o creme esticam. Patinhas e folhas ficam inteiras nos 4 cantos. Simetria: espelho esquerda/direita corr. 0,989; topo/base 0,89 (iluminação diferente), irrelevante porque o 9-slice usa os quatro cantos reais, sem espelhar. Creme esticado: textura de papel discreta, sem artefato visível.

Proporções testadas (simulação em [`nine_slice_proportions_sim.png`](evidence/UI-001/nine_slice_proportions_sim.png) e Unity real), unidades de canvas, escala de origem 0,5:

| Caso | Tamanho | Resultado |
| --- | --- | --- |
| Original 3:2 | 840×560 | OK |
| Largo ≈3,3:1 | 1080×330 | OK (altura útil só 95 u: uma linha de conteúdo) |
| Alto ≈0,73:1 | 440×600 | OK (largura útil 300 u) |
| Pequeno | 400×300 | **Moldura OK, conteúdo NÃO cabe** (útil 260×65 u) |

**Estratégia recomendada:** 9-slice integral. Alternativa de cantos separados + painel central **não foi necessária** e não foi construída. Limite de uso: não usar abaixo de ≈440×300 u; peças menores pertencem a UI-002/UI-003. Margens seguras de conteúdo (creme livre, medidas na imagem): ≈ esq/dir 70 u, topo 110 u, base 125 u a partir da borda do rect, na escala 0,5.

Configuração testada no importer: Sprite Single, **Mesh Type Full Rect**, border (L280, B280, R280, T250), PPU 100 + `Image.pixelsPerUnitMultiplier = 2` (equivale a PPU 200), Uncompressed, sem mipmaps, Clamp, Bilinear.

## 3. Unity (capturas reais, batch PlayMode, câmera → RenderTexture)

Ambiente isolado e temporário (removido; fonte em [`Ui001PanelQa.cs.txt`](evidence/UI-001/Ui001PanelQa.cs.txt)): canvas igual ao `UiRoot` (1280×720, MatchWidthOrHeight=1, Screen Space Camera), `UiKit.Label` / `UiKit.Button` reais, texto dinâmico pt-BR. Canvas medido: 1152×720 u em 960×600 e 1280×720 u em 1280×720, igual ao modelo.

| Proporção | 960×600 | 1280×720 |
| --- | --- | --- |
| 3:2 | [png](evidence/UI-001/unity_orig3x2_960x600.png) | [png](evidence/UI-001/unity_orig3x2_1280x720.png) |
| Largo | [png](evidence/UI-001/unity_wide_960x600.png) | [png](evidence/UI-001/unity_wide_1280x720.png) |
| Alto | [png](evidence/UI-001/unity_tall_960x600.png) | [png](evidence/UI-001/unity_tall_1280x720.png) |
| Pequeno (limite) | [png](evidence/UI-001/unity_small_960x600.png) | [png](evidence/UI-001/unity_small_1280x720.png) |

Resultados ([`unity_qa_log.txt`](evidence/UI-001/unity_qa_log.txt)): nos 3 tamanhos válidos × 2 resoluções, título/corpo cabem no slot, botões dentro do rect do painel, cantos sem deformação, contraste texto `#4A3B33`/creme ≈ 8,7:1. Cliques: raycast real do `GraphicRaycaster` no centro de cada botão → `Button` no topo, `onClick` disparado (Confirmar e Cancelar, 8/8 rodadas); clique na faixa da moldura não aciona botão.

Limitações: (a) cliques via `GraphicRaycaster` + `ExecuteEvents`, **não** mouse/toque do sistema operacional; (b) os botões usados são os procedurais atuais (`UiKit.Button`), não arte final (UI-004); (c) no painel alto, "Cliques" sobrepõe "Cancelar": falha do layout do teste, não do painel; (d) rodada inicial falhou por `WaitForEndOfFrame` (não existe em batch) e por layouts do teste, corrigidos no harness; (e) não rodados: EditMode/PlayMode completos (nenhum código de jogo mudou), **WebGL: NOT RUN** (sem build; o asset não está integrado), Android/APK/AAB: NOT RUN (fora de escopo), memória: textura 1536×1024 RGBA32 ≈ 6,3 MB não medida em WebGL.

## 4. Critérios

| Eixo | Resultado | Base |
| --- | --- | --- |
| A. Arte | **PASS** | Paleta rosewood/creme, patinhas/folhas, relevo suave, coerente com a direção; processamento não alterou RGB. Julgamento visual; já aprovado visualmente pelo responsável. |
| B. Transparência | **PASS** (após limpeza mínima) | Alpha real, 0 halos claros, 1 componente, creme preservado e agora 255. |
| C. Modularidade | **PASS com restrição** | 9-slice integral sem deformação em 3 proporções; inutilizável < ≈440×300 u. |
| D. Unity | **PASS** (960×600 e 1280×720) | Legibilidade, margens, composição e cliques OK; WebGL NOT RUN. |

## 5. Recomendação e pendências

**APPROVE** para uso como painel grande/médio-grande, sob: borders 280/280/280/250, escala 0,5, tamanho mínimo ≈440×300 u, conteúdo dentro das margens seguras. Pendências antes de `approved/`/`INTEGRATED`: aprovação final do responsável; decidir **trim/export** (o sprite tem margem transparente assimétrica: topo 45 e base 79 px, por causa da sombra, o que desloca o corpo visível dentro do rect), resolução/compressão para WebGL e validação em WebGL ao integrar; botões e demais peças continuam não produzidos.

## 6. Aprovação final

**Decisão:** o responsável aprovou o UI-001 `panel_large` v01 como componente visual reutilizável. **Não integrado**: nenhuma tela, `UiKit`, cena, `Neko.Sim` ou teste foi modificado.

**Arquivo aprovado:** `docs/art/ui-modular/approved/UI-001_panel_large_v01.png`, 1536×1024 RGBA, 1 833 033 bytes, SHA-256 `ba300ece650f18e8ef4e938aa84e9c9df3146396dc82a91ba4551306a9ee85f8`, **idêntico byte a byte** ao `blender_clean/` validado (conferido por SHA-256 após a cópia). Sem trim, resize, quantização ou compressão. O bruto (`d5631b7f…7f22a`) segue intacto em `raw_gpt/`, ignorado pelo Git.

**Parâmetros de importação Unity testados** (uma única configuração; ao integrar, reproduzir e revalidar):

| Parâmetro | Valor |
| --- | --- |
| Texture/Sprite | Sprite (2D and UI), Single, Mesh Type **Full Rect** |
| Border (L, B, R, T) | **280, 280, 280, 250** px de origem |
| Pixels Per Unit | 100, com `Image.pixelsPerUnitMultiplier = 2` (equivale a PPU 200: escala 0,5) |
| Image | `Image.Type.Sliced` |
| Compressão | Uncompressed (RGBA32 ≈ 6,3 MB), sem mipmaps, Clamp, Bilinear, Max Size 2048, alpha is transparency |

**Margens assimétricas do sprite** (px de origem, entre a borda do PNG e o corpo visível): esquerda 27, direita 26, topo 45, base 79 (a base inclui a sombra de contato). Em escala 0,5: 13,5 / 13 / 22,5 / 39,5 u. Como os borders incluem essas margens, o corpo visível fica deslocado ≈8,5 u para cima do centro do rect. Ao alinhar painéis a outros elementos, medir pelo corpo visível, não pelo rect. A assimetria está nos borders (topo 250, base 280) de propósito e **não deve ser "corrigida" com trim** sem nova validação.

**Margens seguras de conteúdo** (creme livre medido na imagem, tolerância de cor 45; escala 0,5; contadas a partir da borda do rect): **esquerda 70, direita 70, topo 110, base 125 u**. Esse retângulo evita o recorte em voluta dos cantos (as volutas douradas invadem o creme nos quatro cantos). Fora dele só cabem faixas centrais: entre os cantos a linha dourada reta começa ≈54–61 px de origem do corpo (topo/base) e ≈43–57 px (laterais), mas o conteúdo retangular nessas faixas não pode alcançar os cantos.

**Restrições de dimensionamento**
- Limite geométrico: soma dos borders = 280 u de largura e 265 u de altura em escala 0,5. Abaixo disso o Unity comprime os cantos proporcionalmente (não testado; não usar).
- **O tamanho mínimo útil depende do conteúdo, não só da geometria.** Área útil = tamanho − (140 u na largura, 235 u na altura). Exemplo medido: 440×600 u comporta título, 6 linhas de texto e dois botões de 145 u; 1080×330 u comporta uma única linha (útil 95 u de altura); 400×300 u tem moldura perfeita, mas útil de apenas 260×65 u e **não comporta** o conteúdo de teste. Defina o tamanho de cada painel a partir do conteúdo mais o inset acima; para painéis menores, usar UI-002/UI-003.
- Escala 0,5 é a testada. Outras escalas exigem nova captura.

**Sobreposição no painel alto:** "Cliques" sobre "Cancelar" na captura 440×600 é do **harness de QA** (rótulo de contador e botões ancorados no mesmo canto de uma área útil de 300 u de largura), **não da arte** nem do 9-slice. Está registrado aqui apenas para que a captura não seja lida como defeito do painel.

**NOT RUN (não alegar como validado):** build/execução WebGL e comportamento no navegador; compressão de textura (DXT/ASTC) e qualidade pós-compressão; memória e tamanho de download em WebGL; otimização/redução de resolução do PNG; Android/APK/AAB; EditMode/PlayMode completos do projeto (nenhum código mudou); mouse/toque reais do sistema operacional; escalas diferentes de 0,5 e outras resoluções; integração em telas reais (Loja, Balanço, Elenco etc.). Tudo isso fica para a integração real, que exigirá nova PR própria, `.meta`/GUID estáveis e captura antes/depois.
