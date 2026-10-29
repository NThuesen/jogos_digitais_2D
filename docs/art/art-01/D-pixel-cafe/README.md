# ART-01D — Cozy Pixel-Art Anime Café

**Status (2026-10-28): ART-01D CONCLUÍDA COMO EXPLORAÇÃO.** O responsável aprovou direção D, identidade da Suzu, café D3 e linguagem ornamental D4 como referências da primeira implementação e **liberou explicitamente a ART-02 experimental para Codex**. Os PNGs não são assets finais. Exploração encerrada, sem novas imagens. Nada foi implementado ou validado no Unity.

**Handoff:** [ART-02](../../../roadmap/ART-02_VERTICAL_SLICE.md). Começar com os assets existentes, portrait em card real e chibi no restaurante real; 96 px é hipótese inicial. Canvas de referência 1280×720, com validação adicional em 960×600, sem equivalência presumida. Movimento/idle, variação visual isolada e capturas reais nas duas resoluções dependem da integração. O responsável avaliará o Unity antes de qualquer merge da ART-02.

**Resultados preservados:** kit D4 com 17 peças extraídas e 7 molduras com janela; contorno do chibi mais grosso/escuro e iluminação mais fria/plana que o café, além de densidade de pixels desigual. Há três poses estáticas, sem ciclo completo de animação. Originais, processados, mockups e ferramentas de reprodução permanecem preservados.

**Resultados:** [EVALUATION_D0-D2.md](EVALUATION_D0-D2.md) (Suzu, recorte, escala, identidade) · [EVALUATION_FINAL.md](EVALUATION_FINAL.md) (mockups de Escalação e restaurante, escala dos chibis, modularidade do kit, parecer) · imagens em [`review/`](review/) · componentes em [`processed/`](processed/) · reprodução em [`tools/`](tools/README.md).

**Arquivos-fonte versionados** (todos necessários para reproduzir): `reference/style_anchor_reference.png`, `incoming/D0…D5` (originais gerados, não alterados; D5 é só referência de composição). Tentativas descartadas **não** foram versionadas. O plano e as observações abaixo são histórico da exploração; não autorizam novas gerações. O fechamento acima e a nota ART-02 prevalecem sobre pendências históricas.
**Restrição definitiva:** orçamento adicional **R$ 0** (sem APIs, créditos ou assinaturas novas). Geração = ChatGPT Plus do responsável.
**Decidido vs. proposto**

| Decidido pelo responsável | Ainda **proposta / experimento** |
| --- | --- |
| Custo adicional R$ 0 (sem APIs/assinaturas); geração via ChatGPT Plus | Pixel density, contorno e iluminação do chibi vs. cenário; ciclo de caminhada; tamanho final do chibi (testar 96 px) |
| Direção D aprovada **provisoriamente para prototipagem**: identidade da Suzu, café D3 e linguagem ornamental D4 = referências oficiais (**não** PNGs finais) | Identidade em detalhes (entalhe da orelha, laço da cauda, contorno do chibi) |
| Referência = estilo aspiracional, não para copiar | Transparência: PNG alfa vs. chroma-key |
| Workflow: Claude Code prepara/processa, ChatGPT gera, responsável aprova | Resolução, redução de escala, paleta, pixel grid |
| Aprovação estética explícita antes de ART-02; sem Unity, sem gameplay, sem biblioteca | Modularização em camadas, geração local (ComfyUI) |
| Critério principal: qualidade visual próxima da referência | Todos os tamanhos e paletas numéricos deste documento |

**Referência aspiracional:** [`reference/style_anchor_reference.png`](reference/style_anchor_reference.png) — estilo a alcançar, **não** asset a copiar. A maid rosa, o layout exato, o texto e os props **não** devem ser reproduzidos.
**Prompts prontos:** [PROMPTS.md](PROMPTS.md). **Entrega das imagens:** pasta [`incoming/`](incoming/).
**Propostas A/B/C** (branch `art/art-01-style-exploration`) continuam como estudos válidos; esta direção não depende delas.

## Quem faz o quê

| Etapa | Responsável | Estado |
| --- | --- | --- |
| Análise da referência, specs, prompts | Claude Code | **feito (este documento)** |
| Geração das imagens | Responsável, via ChatGPT Plus | concluída para exploração (D0–D5) |
| Organização, recorte, chroma-key, quantização, mockups, avaliação | Claude Code | concluída para exploração |
| Aprovação estética | Responsável | concluída como referência; ART-02 liberada |

Rótulos obrigatórios em todo material: **ARTE GERADA** (saída bruta do ChatGPT), **PROCESSADO** (após recorte/paleta/escala), **MOCKUP** (composição estática fora do Unity), **UNITY** (captura real — **nenhuma existe**).

## 1. O que a referência tem e precisamos preservar

**Cenário (café)**
- Interior em perspectiva ¾ com **profundidade em 3–4 planos** (balcão ao fundo, mesas no meio, plantas/cadeiras em primeiro plano) e pé-direito alto com vigas.
- **Luz quente narrativa:** pendentes amarelos acesos, feixes de sol diagonais pelas janelas, brilho no piso de tábuas/xadrez. Sombras em tons de marrom-arroxeado, nunca preto.
- **Densidade de detalhe cozy:** frascos de chá, plantas penduradas, quadros, ursinhos, livros, frutas, bules. Cada prop é legível sozinho (candidato a sprite separado).
- Cor complementar controlada: marrons/mel/creme dominantes, com **verde-sálvia** (plantas) e **azul-petróleo/rosa** (almofadas) como acentos.

**Personagens**
- Olhos grandes com 2–3 brilhos, íris com gradiente por dithering, cílios superiores grossos. Franja em mechas, **luz de borda** no cabelo.
- Linhas **coloridas** (marrom/rosa escuro, não preto), sombreamento em 4–6 tons com **mudança de matiz** (sombras mais frias/saturadas).
- **Expressividade variada** (feliz, atrapalhada, olhos em espiral): cada pose é um "pedaço de personalidade" que dá vontade de colecionar.
- Chibi no cenário (~40 px) mantém **cor de cabelo, orelhas, cauda e acessório** do portrait — é a mesma regra de identidade da [decisão ACCEPTED](../../MAID_VISUAL_DECISION.md).
- Portrait fica em **painel próprio com fundo desfocado/escurecido** (prateleiras), separado do cenário.

**Interface**
- Molduras **aninhadas e ornamentais** (terracota/rosewood + miolo creme), com cantos recortados, **padrão de patinhas** e **losangos (argyle)** como textura de fundo.
- Balão de fala em creme "pergaminho" com contorno angular e seta.
- Ícones pixel-art em caixas pequenas com rótulo (fita cassete, geladeira, livro, engrenagem, relógio-analógico, calendário em papel), chaves liga/desliga.
- Tipografia pixel-art pequena e nítida; hierarquia: personagens > cenário > UI.

**Paleta-alvo (aproximada; a medir por código depois):** salmão/pêssego, terracota, chocolate, creme, mel/dourado de lâmpada, sálvia, acentos azul-petróleo. É paleta **observada**, não decidida.

**Atenção honesta:** o nome do arquivo indica que a referência foi gerada pelo ChatGPT. Isso é favorável (a mesma ferramenta, em tese, chega perto), mas **modelos de imagem não produzem uma grade de pixels perfeita**: pixels de tamanhos mistos, anti-aliasing e texto inventado são comuns. Vou medir isso na referência durante o processamento e reportar, sem esconder.

## 2. Identidade da Suzu — hipótese inicial; **aprovada provisoriamente** como referência (ver avaliações)

Suzu (鈴 = sino) é só um nome já usado em `ApplicantBoard.cs`/`Lootbox.cs`; **não existe ficha visual**. A tabela abaixo é **ponto de partida para teste**, **não** característica definitiva: o cabelo menta, o sininho e os demais campos só viram identidade após o responsável avaliar o resultado gerado. Ela é **original e diferente da maid da referência** (rosa/tranças/cardigan azul):

| Campo | Proposta |
| --- | --- |
| `hair_color` | verde-menta suave (`~#8FD8C0`), sombra `~#4FA38F`, luz `~#D6F5E8` |
| `hair_style` | bob com mechas laterais longas e franja reta; duas pontas curtas amarradas com **laço de sino** |
| `eye` | grandes, levemente caídos, âmbar/dourado (`~#E8A93A`), brilhos duplos |
| `ear_style` | orelhas de gato eretas, interior creme-rosado; ponta da esquerda com pequeno corte (marca de identidade) |
| `tail_style` | cauda fina creme com ponta menta e **sininho dourado** |
| `skin_tone` | claro-quente (`~#FFE3CF`) |
| `uniform_theme` | vestido de maid chocolate, avental creme com babados, gola branca, laço **bordô** |
| `signature_accessory` | **sininho dourado** na gargantilha (portrait) / no laço do cabelo e na cauda (chibi) |
| expressão-base | sorriso tranquilo e orgulhoso, olhos meio fechados de contentamento |

Hipótese por trás do menta: contrasta com o mundo quente (leitura em cenário marrom) e não colide com um eventual fundo magenta de chroma-key. É **hipótese a testar**; se o resultado não agradar, a cor e o desenho mudam. Se preferir outra cor/cabelo antes de gerar, edite a ficha e os trechos correspondentes dos prompts.

## 3. Pipeline de produção (custo R$ 0)

1. **Âncora de identidade:** gerar primeiro uma *character sheet* da Suzu (frente + ¾ + expressões + cores). Todos os prompts seguintes anexam essa sheet **e** a referência de estilo no mesmo chat.
2. **Geração em peças separadas**, com **transparência como experimento, sem solução definitiva**: primeiro tentar **PNG com fundo transparente** (se o ChatGPT entregar alfa real); se vier xadrez falso, halo ou borda suja, usar **chroma-key (`#FF00FF`) como alternativa**. As duas variantes constam em [PROMPTS.md](PROMPTS.md) e, no teste inicial, comparamos **halos, bordas e qualidade do recorte** (alfa real, chroma-key e fundo liso). A escolha final fica registrada depois da evidência.
3. **Processamento por código (Claude Code), tudo experimental:** verificar se há alfa real → recorte (chroma-key se preciso) e limpeza de halo → teste de redução de escala → teste de paleta → inspeção do pixel grid. Nenhum passo é presumido bom: cada um é avaliado visualmente e os defeitos são reportados. Ferramentas gratuitas (Python/Pillow ou Node); Pillow **não está instalado** neste computador (instalação gratuita via pip, só quando houver imagens).
4. **Modularidade:** cenário em **camadas** (fundo/parede, balcão, mobília, plantas, luz, props) quando possível; **UI como kit** (moldura 9-slice, painel, balão, botão, ícones, toggles) separado de portrait e de texto — texto sempre UI nativa, como já decidido. Portrait: fundo separado do personagem; peças trocáveis (cabelo/acessório) **só se** a geração separada se mostrar consistente — não presumir.
5. **Avaliação:** montar mockup Escalação (960×600) com as peças processadas, comparar portrait↔chibi lado a lado, listar defeitos reais.

### Pixel art — parâmetros EXPERIMENTAIS (nada decidido)
**Não presumimos que reduzir uma imagem grande produz boa pixel art.** Reduzir pode gerar ruído, bordas quebradas, detalhes perdidos e pixels de tamanhos inconsistentes. Variáveis a testar e comparar visualmente, sempre contra a referência:
- **Resolução lógica:** ponto de partida apenas — mockup 960×600 (Canvas Unity de referência 1280×720); testar 480×300 a 2× e também 320×200 a 3×, preferindo escalas inteiras para evitar pixels de tamanho irregular.
- **Método de redução:** nearest-neighbor, box/area, Lanczos seguido de quantização, detecção automática de pixel grid, ou **não reduzir** (usar a imagem gerada em escala, se parecer melhor).
- **Paleta:** 16, 24, 32, 48 cores; global vs. por peça; com/sem dithering; paleta extraída vs. imposta.
- **Pixel grid:** medir o tamanho real do "pixel" na imagem gerada (inclusive na referência); se não houver grade consistente, registrar isso como limitação em vez de forçar.
- **Tamanhos-alvo iniciais (apenas para testar):** portrait ~128×160 lógicos, chibi ~40–48 px de altura, café 3:2 com recorte 16:10 para o painel.
- **Geração:** sempre em alta resolução (1536×1024 / 1024×1024). Se uma combinação de parâmetros não ficar próxima da referência, **ela é descartada** e isso é reportado.

### Limitação observada (2026-10-28)
Em **duas tentativas** de geração, o ChatGPT produziu **quadros conceituais** (pranchas compostas) em vez de **assets individuais**. Isso é **limitação observada, não entrega concluída**; nenhuma dessas imagens foi aceita como asset. Próxima rodada: **chat dedicado por prompt, um arquivo por vez**, começando pela identidade da Suzu (Prompt 0) e pelo café (Prompt 3). Gerações aprovadas para teste: Prompts 0–3. Prompts 4–5 aguardam. **Direção estética geral aprovada; imagens específicas e identidade definitiva da Suzu ainda não.**

### Limites previstos (declarados antes de testar)
- **Consistência** portrait↔chibi↔sprites de várias poses é o ponto mais difícil; pode exigir várias tentativas.
- **Spritesheet de animação** não será gerada de forma confiável; ver o plano mínimo em PROMPTS.md (poses estáticas).
- **Camadas verdadeiras** (cabelo separado do rosto) não saem de uma imagem única; peças modulares exigem gerações separadas ou aceitar a imagem como "um corpo só" nesta fase.
- Limite de gerações do ChatGPT Plus é variável e não é controlado por mim.

## 4. Geração local (opcional — ainda **não** investigar a fundo)

- Este computador tem **apenas Intel Iris Xe** (verificado); a RTX 3050 está no computador principal e **a VRAM não foi medida**. RTX 3050 existe em versões de 4, 6 e 8 GB.
- Regra do responsável: **só instalar ComfyUI depois de evidência de vantagem sobre o ChatGPT**. Critério proposto: se o ChatGPT falhar em (a) consistência chibi↔portrait ou (b) grade de pixels/transparência após ~3 rodadas, avaliar local. Primeiro passo seria só medir a VRAM (`nvidia-smi` no computador principal) — nada a instalar agora.
- Cautela de licença: modelos/LoRAs gratuitos têm licenças diferentes; registrar antes de usar.

## 5. Critério de aceite (para o responsável avaliar)

- Dá vontade de colecionar a Suzu? O chibi é reconhecível como ela sem ler o nome?
- O café parece um lugar acolhedor, rico, **não** um fundo genérico?
- A Escalação parece parte de um jogo encantador, não só funcional?
- O relatório lista defeitos reais (halo, pixels mistos, mãos, inconsistência) sem mascará-los.

## 6. Fora de escopo

Sem Unity, sem mudar `Palette.cs`/cenas/UI atual/GDD, sem gameplay, sem biblioteca de assets, sem outras maids. **ART-02 só após aprovação estética explícita.**
