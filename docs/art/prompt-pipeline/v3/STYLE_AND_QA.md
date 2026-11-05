# V3 — Contrato de direção artística e QA

## A V2 antiga não é o padrão atual

A V2 pediu Hina full-body histórica com estética PIXEL ART; desde ART-04 os personagens usam **anime 2D ILUSTRADO**, linha suave, sombra controlada e alpha antialiasing (não pixelização/48 cores). Os prompts históricos ficam intactos para proveniência. O que se quer no V3 é conversão da **identidade premium retratada** para corpo inteiro chibi e depois vídeo.

### Referências e limitações
- **Imagem A prioritária:** retrato aprovado e real da personagem (segundo lote PR #61; terceiro lote depende de aprovação, não presuma V01). Nonoka **V02** com duas orelhas Scottish Fold felinas compactas, não as orelhas de cachorro V01.
- **Imagem B opcional:** sprite de Hina **atual ilustrado** em [runtime sheet](https://github.com/NThuesen/jogos_digitais_2D/blob/main/Assets/_Game/Resources/Maids/hina_walk.png) ou full-body Mikan V3 piloto aprovado; referência só para acabamento e proporção. **NÃO** gerar um sheet nem copiar o rosto e roupa da B.
- O retrato é meio corpo; completar pernas/saia/calçado de forma coerente sem inventar aparência, símbolos ou múltiplos acessórios. Não tornar todas as maids um recolor da Suzu/Hina/Mikan.
- Prioridade: pele, rosto, olhos, cabelo, orelhas e marcações, cauda, uniforme, acessórios e lado anatômico, expressão; para vídeo a imagem WALK START é referência absoluta. Manter sombra/material ilustrado premium e não destruir detalhes com pixel art.

### Tamanhos, ciclo e chroma
- Full-body: portrait vertical 3:4, cerca de 2.9–3.2 cabeças de altura excluindo orelhas, corpo inteiro, headroom ~5%, alpha genuíno.
- Walk still/vídeo: 16:9, personagem ~70–82% da altura, direito da tela, sapatos e cauda inteiros, root fixo. Cor `#00FF00` ou `#0000FF` apenas referência: verificar se não aparece nos olhos/cabelo/roupa, medir chroma real dos frames.
- Vídeo: um par completo esquerda-direita por ciclo, apoio de pé convincente, braço oposto, movimento leve de cabelo/roupa/cauda, câmera totalmente travada. **Antigos vídeos tinham foot sliding 2,4–5,2x**, que FPS ou `units_per_cycle` sozinho não corrige. Se reprovado, não produzir 17 vídeos semelhantes; revisar piloto e regenerar especificamente a animação.
- Blender: salvar MP4 original e frames numerados; PNG RGBA **não** remove o chroma automaticamente. Selecionar somente o trecho de loop válido, anotar FPS, evitar duplicações/pulos.
- QA futuro: `tools/art/maid_walk/maid_walk.py` pode processar novos frames, mas não alterar planilha de parâmetros, 9 sheets antigos, import Unity ou MaidCatalog sem branch/PR e aprovação. Verificar legibilidade 960×600, 1280×720 e memória real WebGL.

### Verificações por personagem
- Mikan: pose de aceno é retrato, não caminhada; braço livre no WALK.
- Nonoka: versão V02 apenas; não transformar orelhas felinas compactas em cachorro.
- Mikan / Himari / Anzu: cabelo quente nas três é decisão aceita. Preserve paletas reais em vez de recolor automático.
- Seira: óculos não desaparecem; Shiori: orelhas tortoiseshell preservam lados; Kinako: calico desigual; Ruri: bun e manchas Bengal; Mirai: trança única; Mayu: heterocromia/lado e hood baixo.
- Cabelo longo/cauda: deixar sapatos legíveis; em cada frame manter penas/orelhas/marcas sem morphing.

Tudo aqui é documentação e prompts; nenhuma geração, integração ou QA foi executado por escrever estes arquivos.
