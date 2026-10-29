# ART-01D — Avaliação final: composição integrada (Escalação + restaurante + kit de UI)

**Status:** direção D **aprovada provisoriamente para prototipagem** (identidade da Suzu, café D3 e linguagem ornamental D4 como referências oficiais da primeira implementação). **Os PNGs gerados continuam sem aprovação como assets finais.** Tudo abaixo é **MOCKUP** estático feito a partir de **ARTE GERADA** + recortes **PROCESSADOS**; **nada é captura Unity** e nada foi testado em runtime. Custo adicional **R$ 0**. **ART-02 só começa quando o responsável decidir.**

Parte 1 (Suzu D0–D2): [EVALUATION_D0-D2.md](EVALUATION_D0-D2.md).

## 1. Integridade dos arquivos de `incoming/`

| Arquivo | Tamanho | Estado |
| --- | --- | --- |
| `D0_suzu_sheet.png` | 1536×1024 | abre, completo; RGBA com alfa 100 % opaco |
| `D1_portrait.png` | 1024×1536 | idem |
| `D2_chibi_poses.png` | 1774×887 | idem; 3 poses numa imagem |
| `D3_cafe_interior.png` | 1672×941 (16:9) | abre, completo; **RGB sem canal alfa**; uma única imagem achatada |
| `D4_ui_kit.png` | 1536×1024 | abre, completo; **RGB sem alfa**; peças sobre padrão de losangos |
| `D5_lineup_concept.png` | 1586×992 | abre, completo; **composição inteira** (usada só como referência) |

Nenhum arquivo corrompido ou truncado. Originais **não alterados**.
**Problemas de processamento encontrados:** (a) nenhum arquivo tem transparência real; (b) D4 tem peças **sem fundo transparente** e com fundo de losangos de cor próxima da moldura (separável porque o contorno externo é bem escuro, ver §4); (c) D4 traz **marcas baked** dentro das molduras (silhueta de gatinho, patinhas, toggles); (d) D3 e D5 são imagens únicas, **sem camadas**: móveis não são separáveis.

## 2. Mockup 1 — Escalação 960×600

**Arquivo:** [`review/05_escalacao_960x600.png`](review/05_escalacao_960x600.png) · **MOCKUP**.
**Feito com:** D3 escurecido ao fundo (+ cobertura 16:9→16:10 por recorte central), peças **D4 extraídas** (faixas, molduras, pílulas, molduras de raridade), retrato e chibi da Suzu **processados**. Texto e números são **provisórios** (fonte Trebuchet no mockup, **não** a `Typeface` do jogo; "Química 62 %" e "Time promissor" são inventados só para o layout). D5 **não** foi usado como imagem final.

**Estrutura do jogo preservada** (conferida em `LineupScreen.cs`, sem editá-lo): título; botão Voltar; placa de Química (texto + barra + veredito); **cinco posições** (Recepção, Salão, Cozinha, Palco, Gerência) como **cabeçalho sobre fileira de vagas**, como em `BuildBoard`; **Banco** embaixo; linha de aviso; botões "Escalar sozinho" e "Limpar". Mostrei o estado de vagas máximas do GDD §2.2 (2/3/3 + palco + gerência) para testar o pior caso de largura. Decisão de apresentação a validar: o **chibi** ocupa a vaga (como no D5), enquanto hoje `MaidCard` mostra o portrait; escolher fica para a ART-02.
**Adições minhas, opcionais:** card de **pré-visualização** (retrato grande) e **banco com a mesma Suzu nas quatro molduras** (comum bege, rara azul, épica violeta, lendária dourada) **só para avaliar as molduras**, **não** representa estado de jogo nem cria personagens; uma única maid existe.

**Funciona**
- Tudo cabe em 960×600 com as 10 vagas na mesma linha; hierarquia clara (cartões > faixas > cenário escurecido).
- Molduras de raridade distinguíveis a 80 px (bege/azul/violeta/dourado batem com GDD §2.8).
- O café ao fundo dá calor sem competir (com escurecimento de ~50 %).

**Precisa de correção**
- **Chibi em vaga de 80 px = ~62 px de altura**: charme preservado, mas rosto pequeno (olhos de poucos pixels). É o menor tamanho aceitável, não o ideal.
- Painéis creme **lisos** (o preenchimento padrão do D4 foi substituído por creme chapado) ficam sem textura; o banco parece vazio quando há poucos cartões.
- Texto de 11 px (veredito, rodapé) é o limite de leitura em 960×600.
- **O canvas real do Unity usa referência 1280×720** (`UiRoot.ReferenceWidth/Height`), não 960×600 como na nota ART-02. Os tamanhos aqui precisam ser revistos nessa escala.

## 3. Mockup 2 — Restaurante com chibis a 64 / 96 / 128 px

**Arquivos:** [`06_restaurante_64px.png`](review/06_restaurante_64px.png) · [`96px`](review/06_restaurante_96px.png) · [`128px`](review/06_restaurante_128px.png) · [`06_restaurante_profundidade.png`](review/06_restaurante_profundidade.png) · comparação 1:1 + zoom 2×: [`07_restaurante_comparacao.png`](review/07_restaurante_comparacao.png). **MOCKUP** (uma maid, 3 poses estáticas; sombra de contato e posições escolhidas por mim; pose carry espelhada; sem animação).

| Altura | Rosto / acessórios | Proporção com o cenário | Espaço (largura do sprite incl. cauda ≈ 0,69×altura) |
| --- | --- | --- | --- |
| **64 px** (44 px largura) | olhos 2–3 px, sininhos viram manchas; silhueta, cabelo menta e orelhas legíveis | parece criança/gatinho frente aos bancos do balcão (~½ banco); muito piso vazio | ~9 lado a lado no corredor central (~430 px, estimado) |
| **96 px** (66 px) | rosto legível, orelhas, laços, cauda com sino visíveis | **melhor equilíbrio**: ~altura de cadeira de mesa; destaca sem dominar | ~6 lado a lado |
| **128 px** (88 px) | detalhes excelentes | ~altura de banco/balcão; **domina a cena** e aperta mesas e passagem | ~4 lado a lado |

**Perspectiva ajuda:** a variante por profundidade (64 ao fundo, 96 no meio, 128 em primeiro plano) dá leitura natural de espaço; escala fixa única é menos convincente do que escala por profundidade, mas custa decisão de engine (sorting/escala por Y).
**Recomendação para teste real (não decisão):** começar com **96 px** (faixa 80–110 px) em 960×600, e avaliar escala por profundidade no Unity.

**Coerência chibi ↔ cenário (problemas estéticos reais):**
1. **Densidade de pixel desigual:** o café tem degraus de ~3 px no canvas 960×600, o chibi tem detalhe mais fino (sub-pixel em 64–96 px). Parece um sprite "HD" sobre um fundo pixel art. *Observação visual; não medida.*
2. **Contorno do chibi escuro e grosso**, o café usa contornos suaves coloridos; a diferença aparece mais em 128 px.
3. **Iluminação:** o café é dourado quente com feixes de luz; o chibi é plano e frio por comparação. Sombra de contato ajuda, mas não integra a luz.
4. **Espelhar** a pose carry funciona, mas inverte assimetrias (cauda, entalhe da orelha) — aceitável para sprites, ruim para identidade se o entalhe voltar a ser exigido.
5. Chibis estão sempre de frente/¾; sem costas, sem perfil, sem ciclo de caminhada.

## 4. Modularidade mínima do kit D4

**Arquivo:** [`review/08_modularidade_ui.png`](review/08_modularidade_ui.png). **Componentes extraídos:** 17 em [`processed/ui/`](processed/ui/) + 7 molduras com **miolo transparente** (`*_window.png`) + [`ui_manifest.json`](processed/ui/ui_manifest.json) (tamanho, caixa interna e margens por peça).

| Teste | Resultado |
| --- | --- |
| Separar peças do fundo | **OK**: flood-fill a partir do contorno escuro, 17 peças limpas, inclusive a moldura dourada com brilhos |
| Molduras como overlay (miolo transparente) | **OK**: 7 molduras viraram "janelas" (conteúdo vai atrás, texto vai por cima) |
| 9-slice de `frame_wide`/`frame_large` | **OK**: cantos (patinhas) preservados, bordas esticam sem deformar; 300×74, 200×60, 120×120, 110×130 e 220×90 testados; o redimensionamento ingênuo deforma as patinhas e a espessura |
| 3-slice horizontal de `frame_label` | **OK** para faixas de título (largura segue o texto, altura fixa); **não serve** para caixas altas |
| Texto/números independentes | **OK**: nada de texto vem baked nas molduras; textos trocados sem nova arte |
| Molduras de raridade | **só escala uniforme**: esticar deforma gemas/cantos; em 64 px ainda legíveis, coroa/asas da lendária perdem detalhe |
| Ícones | legíveis até 32 px de altura; peças fixas (não recoloríveis) |
| Pílulas (botão) | texto independente; normal/pressionado são peças separadas; **sem estado desabilitado/hover** |
| Barra de toggles | **não modular**: toggles vêm desenhados na barra (não ligam/desligam sozinhos) |

**Limites reais do kit:** marcas baked no miolo (silhueta de gatinho/patinhas) — resolvido apenas nas molduras com "janela"; **sem** painel alto, aba, barra de progresso/slider, scroll, estado desabilitado, nem "pílula" vazia reutilizável; molduras não vêm em tamanhos múltiplos (reduzir suaviza o pixel art). **Nada disso foi testado como Sprite com 9-slice no Unity.**

## 5. Parecer: o que fazer com o resultado

**Funciona (reaproveitar):** identidade Suzu (retrato, 3 poses), café D3 como fundo de telas/menus, 7 molduras com janela, faixas e pílulas, ícones, molduras de raridade, 9-slice de painéis e faixas, texto independente da arte.

**Corrigir antes/durante a ART-02 (estético importante):**
1. **Casar densidade de pixel e contorno do chibi com o cenário** (gerar de novo o chibi com contorno fino? ou ajustar escala/tamanho final); é a maior incoerência visível a 96–128 px.
2. **Definir o tamanho real do chibi** em canvas 1280×720 (o do Unity), testando 96 px e escala por profundidade.
3. **Integrar a luz do café ao chibi** (tom quente, sombra) — pode ser feito por shader/tint no Unity sem nova arte.
4. **Identidade no chibi:** reintroduzir o entalhe da orelha esquerda e conferir cor do laço da cauda (ver parte 1).
5. **Ciclo de caminhada real** (idle e walk reais, exigidos pela ART-02): hoje só há poses estáticas; exige novas gerações **com necessidade agora demonstrada**.

**Pode esperar (limitação aceitável):** fonte definitiva (a do mockup é provisória), estado desabilitado/hover dos botões, toggles reais, painéis altos, barra de progresso, texturas dos painéis, café em camadas/oclusão (o D3 é uma imagem só: para gameplay, mesas e cadeiras do simulador precisam de arte própria ou posicionamento sobre o fundo), costas/perfil do chibi, outras maids.

**Riscos técnicos abertos (a medir no Unity):** importação (filtro Point vs Bilinear, compressão, mesh, 9-slice borders do manifesto); desempenho com imagens ~2–3 MP; diferença 960×600 vs 1280×720; hiato entre arte achatada do café e os móveis simulados; texto PT-BR real nas faixas (larguras variam).

## 6. Resultado

Os dois mockups mostram que o **encanto visual existe** (personagens colecionáveis, café acolhedor, interface ornamental) e que a **estrutura real da Escalação cabe** no kit D4. O que **não** foi provado: runtime, animação, coerência final de pixel e uma biblioteca de maids. O parecer é **satisfatório com ressalvas**; a decisão de liberar a **ART-02** é do responsável.
