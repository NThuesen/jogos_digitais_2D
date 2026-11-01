# Pipeline de arte v2 — nove maids com o mesmo estilo chibi da Hina

**Decisão do responsável em 2026-11-01:** o resultado **full-body da Hina** (bob azul-noite, olhos gelo, presilha triangular prateada, uniforme azul-marinho/marfim, fundo marfim) foi escolhido como **padrão artístico**. Todas as nove maids devem compartilhar esse **tipo de personagem chibi pixel-art**. As identidades continuam diferentes. A Suzu anterior com anatomia mais alongada **não é o modelo estético** — preserva-se o experimento técnico já realizado, sem alterar o Unity neste PR.

**Começar aqui:** [HINA_STYLE_LOCK.md](HINA_STYLE_LOCK.md) → [índice das nove fichas](MAIDS_INDEX.md) → ficha `maids/NN_nome.md` → copiar o prompt correspondente. **Cada ficha tem PROMPT 1 e PROMPT 2 COMPLETOS**; nunca concatenar os prompts-base.

## Uma referência obrigatória — evitar ambiguidade

**A IMAGEM FULL-BODY APROVADA DA HINA é a âncora visual.** Ela foi enviada pelo responsável no ChatGPT, **mas ainda não está versionada no GitHub**. Selecione a imagem aprovada em sua Biblioteca ou arquivo local, não uma versão com chroma nem um frame do vídeo. Não afirme que há um PNG no repositório quando ele não existe. O arquivo de café [D-pixel-cafe/reference/style_anchor_reference.png](../art-01/D-pixel-cafe/reference/style_anchor_reference.png) permanece como **referência histórica do clima do jogo**, mas **NÃO deve ser anexado** nos novos Prompts 1 e 2: ele mistura personagens, cenário, texto e uma resolução de desenho diferente, permitindo deriva de estilo.

**Roles de imagens, SEM exceções:**
- **Prompt 1:** anexe **só a Hina aprovada** (IMAGEM A = estilo, corpo, traço, sombra, uniforme e framing). A ficha textual define identidade, cores, silhueta de cabelo, orelhas, cauda e acessórios. **Na própria Hina**, a imagem A é também a identidade já aprovada. Não anexar Suzu, café, outras maids ou assets de vídeo.
- **Prompt 2:** anexe nesta ordem **IMAGEM A = full-body da própria maid aprovada no Prompt 1** (identidade absoluta); **IMAGEM B = Hina aprovada** (consistência artística). Mudar apenas pose e fundo; **não redesenhar** rosto, roupa ou proporção.
- **Vídeo Tencent:** anexe o **Prompt 2 aprovado** como entrada. Não pressupor que a ferramenta aceite várias imagens.

A nova técnica se baseia nas recomendações oficiais da OpenAI de (a) papéis explícitos por referência, (b) separar invariantes e alterações, (c) prompts com objetivo/composição/estilo claros, (d) editar incrementalmente uma dimensão por vez e (e) **verificar as saídas**; não há garantia de resultado determinístico.

## Etapa 1 — geração full-body

1. Abra a ficha, copie só o bloco **PROMPT 1**, anexe **somente** o master full-body da Hina e gere **uma imagem**. Use a mesma configuração de geração quando possível (modelo, aspecto vertical, opção de qualidade). Evite pedir várias variações em um único prompt.
2. **Comparação obrigatória:** coloque a imagem gerada lado a lado com a Hina no mesmo tamanho aparente. Inspecione **anatomia chibi de ~2,8–3 cabeças**, proporção do rosto, contorno em degraus, clusters de pixel, sombras simples, densidade de detalhes e estrutura de uniforme marfim. Uma Suzu longa estilo ilustração anime é motivo para **reprovar**.
3. Também confira **os marcadores específicos da ficha**: cores/forma do cabelo, olhos, orelha esquerda/direita, cauda, objeto de assinatura e expressão. A maid deve ser reconhecida sem ler o nome e não pode herdar cabelo/rosto/presilha da Hina.
4. Teste leitura reduzida a **96–128 px de altura de personagem**; não troque a imagem original em alta resolução por essa prévia. Use a rubrica e as falhas eliminatórias de [HINA_STYLE_LOCK.md](HINA_STYLE_LOCK.md): **9–10/10, nenhuma falha eliminatória**.
5. Se **um ponto** falhar, **edite a saída existente**, corrigindo só ele: `Change only [defect]. Preserve the character's face, hair, ears, tail, outfit, original colors, composition, Hina-locked proportions, and all other features.` Reavalie. Não gere um vídeo de uma identidade ainda reprovada.
6. Salve como `NN_nome_step1_fullbody_v02.png` (ou incremento de versão). Registre `approved / candidate / rejected`, a imagem usada de Hina, prompt v2 e motivo. Não adicionar imagens automaticamente a `Assets/`.

**Nota:** o full-body desta etapa é **chibi do restaurante**, não um retrato detalhado do Maid Card/visual novel; esses são assets distintos de mesma identidade.

## Etapa 2 — quadro inicial de caminhada

1. Selecione **uma** Etapa 1 aprovada. Em nova conversa de imagens, anexe **(A)** essa maid e **(B)** o full-body mestre da Hina.
2. Cole só o **PROMPT 2** da mesma ficha. O resultado é **um único frame**, não vídeo, strip ou sprite sheet. O corpo inteiro deve olhar e se orientar para a **DIREITA DA TELA** (vista lateral/3⁄4 genuína), com **passo de comprimento legível**, transferência de peso e contato claro do pé de apoio. Evitar rosto olhando de frente enquanto pernas parecem se deslocar de lado, cruzamento de pernas na passarela e passos minúsculos.
3. O fundo deve ser **chroma puro sólido**: por padrão azul `#0000FF` para Suzu e Kaede, verde `#00FF00` para as demais. **Inspecione os pixels reais**: alterar o chroma se sobrepõe à personagem, anotando a cor efetiva. O código hex no texto não garante perfeição do modelo.
4. Compare com a Etapa 1: preservar rosto/roupa/identidade, proporção chibi, placement de acessórios e lados anatômicos. Com vistas laterais, oclusão natural é aceitável, **inversão de lado de ear notch, trança ou heterocromia não**. Reprovar se virar ilustração anime alta.
5. Salve `NN_nome_step2_walkstart_v02_<cor>.png`, registre aprovação e só então siga ao vídeo.

## Etapa 3 — Tencent AI Studio: vídeo walk-in-place

O vídeo só é útil para Unity se tiver **pé plantado, passada suficiente, câmera fixa e loop razoável**. O experimento Suzu demonstrou que uma caminhada bonita **pode ter deslize impossível de corrigir apenas por FPS**; não repetir esse erro em lote. Usar no Tencent o PNG chroma aprovado da Etapa 2 e este **prompt-base de vídeo**:

```text
Animate the supplied SINGLE approved chibi pixel-art cat-maid into ONE natural, repeatable RIGHT-FACING horizontal WALK-IN-PLACE cycle for a 2D Unity game. Identity, colors, compact 2.8–3.0-head chibi proportions, clear stepped pixel-art outlines and clustered cel-shading must remain consistent with the attached image. Do not turn the sprite into smooth anime illustration, 3D or a realistic person.

Movement must clearly alternate TWO complete steps with genuine leg swing, visible foot lift, weight transfer, knee and ankle articulation and opposite arm swing. During each STANCE phase, one planted shoe stays fixed relative to the virtual floor and visibly moves BACKWARD relative to the torso while the other leg swings FORWARD for a sufficiently long stride; then alternate. No small shuffling, skating, moonwalk, crossed runway legs, running, hopping or floating. Modest skirt, ears, hair and tail follow with stable secondary motion. Preserve anatomy and accessory positions on every frame.

A perfect LOCKED camera, no rotation, no zoom, no camera drift. Center and retain the FULL character including ears, hands, shoes and tail tip at the same scale throughout; the character walks IN PLACE and never translates across screen (Unity handles position). Preserve an absolutely UNIFORM CHROMA background exactly as in input: no floor, shadow, gradient, texture, color spill, props, particles, text or other characters.

Make the gait PERIODIC: the ending body pose and planted-foot configuration should closely match the opening pose, forming a loop without visible snapping. Never invent extra arms, shoes, jewelry or change hair length, ears, iris sides or clothing. Prioritize consistent foot contact, readability and loop continuity over decorative movement.
```

**QA em vídeo:** ver a sequência inteira antes de levar ao Blender; atenção à estabilidade do fundo, olhos, roupa, acessórios, braços, cauda, pernas, pés e duração do loop. Confirmar trecho de ciclo contínuo em vez de presumir que todo MP4 é loop. Inspecionar na reprodução normal e em repetição. **Se o pé de apoio não atravessar a passada coerentemente e houver moonwalk, reprovar o vídeo — não confiar em correção por `UnitsPerCycle` na Unity.** Alterar pontualmente o prompt e gerar de novo; não produzir oito vídeos ruins antes de testar um.

Guardar `NN_nome_step3_walk_v02.mp4` (adaptar à extensão real). A geração de vídeo pode ter uso/créditos/preços variáveis, verificar antes de acionar; **nenhum pagamento adicional autorizado**.

## Etapa 4 — Blender e pasta local

As nove pastas já existem em [`art_working/`](../../../art_working/README.md) (com `.gitkeep`). Exemplo:

```text
art_working/
  01_suzu/step4_blender/walk_x/frames/
  02_hina/step4_blender/walk_x/frames/
  03_akari/step4_blender/walk_x/frames/
  04_momo/step4_blender/walk_x/frames/
  05_reina/step4_blender/walk_x/frames/
  06_kaede/step4_blender/walk_x/frames/
  07_nene/step4_blender/walk_x/frames/
  08_ichika/step4_blender/walk_x/frames/
  09_yoru/step4_blender/walk_x/frames/
```

Extraia quadros do **trecho de loop aprovado**, preservando numeração sequencial, alpha/chroma e FPS real medido/exportado quando disponível. O Blender exporta o vídeo; não transforme qualquer número arbitrário de frames em um ciclo sem avaliação.

**Os PNGs brutos e MP4s são ignorados pelo Git**, não aparecem automaticamente no GitHub ou em outros worktrees. Os sprites **processados/aprovados** serão assunto de integração separada no Unity, mantendo provas visuais, saves e simulação intactos.

## Critérios para considerar uma maid pronta para integração

- [ ] Etapa 1 aprovada **lado a lado com Hina** (rubrica 9–10/10) e identidade distinta preservada.
- [ ] Etapa 2 com mesma identidade/estilo e pose lateral direita com chroma funcional.
- [ ] Vídeo em loop convincente, **contato de pé e passada suficiente**; não apenas movimento bonito.
- [ ] Frames úteis do Blender em `art_working/NN_nome/step4_blender/walk_x/frames/`.
- [ ] A futura integração apresenta capturas do Unity em tamanho real e testes EditMode/PlayMode; um MP4 **não é** evidência de funcionamento no jogo.
- [ ] Aprovação visual do responsável registrada. Nenhuma alteração de sprites/saves/economia/gameplay como efeito colateral deste documento.

## Referências oficiais OpenAI

- [Image prompting — OpenAI API](https://developers.openai.com/api/docs/guides/image-prompting)
- [Image generation models prompting guide — Cookbook](https://developers.openai.com/cookbook/examples/multimodal/image-gen-models-prompting-guide)
- [Criando imagens com o ChatGPT — OpenAI Academy](https://openai.com/pt-BR/academy/image-generation/)
