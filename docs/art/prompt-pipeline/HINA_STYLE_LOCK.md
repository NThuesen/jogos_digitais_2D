# HINA STYLE LOCK — referência visual comum das nove maids (v2)

> **PROMPT HISTÓRICO DE PRODUÇÃO — não define mais o estilo de runtime.** Este texto documenta as imagens/vídeos que deram origem às nove maids. Desde o [ART-04](../maid-walk/README.md), o jogo usa **sprites anime 2D ilustrados**, sem quantização para pixel art. A [decisão atual](../CURRENT_VISUAL_DIRECTION.md) prevalece; preserve os frames originais e não gere novamente personagens para forçar pixelização. Para novas imagens, é preciso revisar estes prompts explicitamente em tarefa futura.


**Decisão artística (2026-11-01):** a imagem **full-body aprovada da Hina** é a referência-mestra das proporções chibi, da geometria do uniforme e do acabamento pixel-art de **todas** as nove maids. O primeiro desenho da Suzu, mais alongado e próximo de ilustração anime, **não** é a referência de estilo. Isso atualiza os prompts de produção; **não substitui** automaticamente sprites ou cenas Unity existentes.

## Qual imagem usar

A Hina aprovada é a imagem fornecida pelo responsável nesta conversa: cabelo bob azul-noite, olhos gelo, presilha triangular prata, vestido azul-marinho e marfim, mãos unidas, cauda fina e fundo marfim. **Usar a imagem full-body original, NÃO o chroma verde nem um frame do vídeo.**

**O arquivo de imagem da Hina ainda não está versionado no GitHub:** o operador precisa anexá-lo da Biblioteca do ChatGPT ou de seus arquivos locais **toda vez** que gerar uma maid. Não afirmar que existe um caminho versionado para ele. Se a imagem não estiver disponível, não produzir o lote: texto sozinho não impõe a mesma identidade visual. Uma futura adição do PNG ao Git requer origem/autorizações e hash claros.

- **Etapa 1:** IMAGEM A = Hina full-body aprovada, **somente referência de estilo, proporções, pixel art e família de uniforme**. Os detalhes físicos e cores da maid alvo vêm do bloco de identidade. **Não anexar a cena de café nem outras maids.**
- **Etapa 2:** IMAGEM A = resultado aprovado da Etapa 1 da *mesma* maid (**identidade, anatomia e roupa**); IMAGEM B = Hina full-body aprovada (**estilo e proporções do grupo**). Pedir mudança de pose e fundo, não redesign. Não anexar a cena do café.
- **Etapa 3:** vídeo recebe a imagem chroma da Etapa 2. O movimento/contato do pé deve ser aceito com inspeção em vídeo, não presumido pelo texto do prompt.

## Contrato visual: não negociável

1. **Mesmo corpo chibi adulto de Hina:** alvo visual ~2,8–3,0 cabeças da cabeça até os sapatos, sem contar orelhas; cabeça perto de 1/3 da altura sem orelhas, bochechas arredondadas, torso e membros curtos, pés compactos. Proibir anatomia alongada de ilustração anime (5–8 cabeças). Números são pistas visuais, não medidas que a geração garante.
2. **Mesmo acabamento:** pixel art 2D intencional, contorno escuro colorido com degraus legíveis, grupos coerentes de pixels, 2–3 faixas de sombreamento por material, brilho discreto. Proibir pintura anime lisa, gradientes suaves, maquiagem realista, vetor sem pixel, textura ruidosa, simulação 3D e filtros de pixelização posteriores.
3. **Mesma família de uniforme:** vestido maid A-line modesto/curto, avental marfim com barra de babados, gola/botões, punhos/mangas reconhecíveis, meias e sapatos fechados. Manter silhueta e complexidade similares à Hina. Permitir alterações de cor, gola, mangas, saia e **1–2 marcas de identidade** conforme ficha; não alongar silhueta. Detalhes particulares de cada ficha continuam obrigatórios quando compatíveis.
4. **Mesmo enquadramento:** um corpo inteiro frontal quase simétrico em pé, centrado, braços separados da silhueta e ambas as mãos, orelhas, sapatos e cauda visíveis; sujeito ocupa aprox. 80–86% da altura; formato vertical ~3:4 quando disponível. Fundo marfim sólido claro, sem cenário, chão, sombra projetada, gradiente, vinheta, texto, interface ou moldura.
5. **Rosto e luz da mesma escola:** olhos anime pixelados limpos e grandes, nariz e boca discretos, luz quente difusa acima à esquerda, sombras frias contidas, contraste legível. Expressão muda com o arquétipo, mas **não copiar o rosto, cor dos olhos, cabelo ou presilha da Hina** em outras maids.
6. **Identidade única:** cabelo/orelhas/cauda/olhos/acessórios descritos na ficha prevalecem sobre esses traços particulares da Hina. Transferir **estilo e geometria de corpo**, nunca sua pessoa ou paleta.
7. **Redução de jogo:** avaliar também em ~96–128 px de altura de personagem, preservando original em alta resolução. Um personagem que só funciona em tamanho grande é rejeitado.

## QA obrigatório antes de vídeo

Ver Hina e a nova maid lado a lado **na mesma altura aparente**. Rejeitar automaticamente se: corpo alongado, outlines suaves/não pixelados, uniforme de outro estilo, falta de parte do corpo, identidade trocada, face anime detalhada em vez de chibi, cópia da Hina em vez de novo design.

**Rubrica visual (avaliação humana, não medição automática garantida):** proporção (0–2); pixel/contorno/sombreamento (0–2); uniforme/família (0–2); enquadramento e leitura a 96 px (0–2); marcadores originais da identidade (0–2). Aceitar apenas **9–10/10 e zero falhas eliminatórias**. Registrar referência, prompt/versão, saída e motivo da aprovação.

**Se um único item falhar**, fazer **edição da saída anterior**: `Change only [one problem]. Preserve identity, exact face, clothes, proportion, framing and all other features.` Repetir as invariantes críticas no edit; comparar antes/depois. Se falhar de novo, rejeitar a saída e ajustar pontualmente o prompt. Não fazer lotes de vídeos com Etapa 1 não aprovada.

**Limite:** prompts e referências minimizam variação, mas geração não é determinística; o resultado não é garantido visualmente idêntico. Portraits futuros do card são outro asset, embora partilhem a identidade.

## Referências OpenAI consultadas

- [Image prompting (API)](https://developers.openai.com/api/docs/guides/image-prompting): papel distinto de cada imagem, preservar vs mudar, edição incremental, revisar resultado.
- [Image generation models prompting guide](https://developers.openai.com/cookbook/examples/multimodal/image-gen-models-prompting-guide): formato estruturado, estilo e composição concretos, preservar identidade e usar poucos inputs com função clara.
- [OpenAI Academy — Criando imagens com ChatGPT](https://openai.com/pt-BR/academy/image-generation/): instruções claras, pequenas revisões dirigidas e comparação de saídas.

Os prompts individuais das nove fichas são **completos/autossuficientes**: não concatenar este documento ao copiar um deles.
