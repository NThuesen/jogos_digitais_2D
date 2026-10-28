# Direção visual aprovada — Cozy Pixel-Art Anime + Visual Novel

**Estado:** ACCEPTED — direção de apresentação e linguagem visual; **não implementada** e **sem arte final aprovada**.
**Aprovada em:** 28/10/2026, pelo responsável do produto.
**Âmbito:** menus, Escalação, Elenco, momentos narrativos e apresentação do café, mantendo o gameplay 2D.
**Índice:** [GDD](../GDD.md) (produto) · [Maid Card + Chibi](MAID_VISUAL_DECISION.md) (personagens) · [Roadmap](../ROADMAP.md) (execução).

## Decisão de produto
Adotar como **norte artístico** a combinação de:
- **Cozy pixel-art anime café:** interior de café rico em detalhes, aconchegante e habitado, com plantas, madeira, luz quente, profundidade, objetos e pequenas animações ambientais. Evitar UI visualmente vazia e fundos de cor plana como destino estético.
- **Apresentação de visual novel:** portraits grandes e expressivos de maids, molduras ornamentais, diálogos e composição cênica que valorize personagens e atmosfera. Especialmente útil em menus, Elenco, Escalação, contratação/recompensas e eventuais transições entre dias.
- **Simulação 2D com chibis:** durante o turno, o jogador continua observando as maids atuarem automaticamente no restaurante, com silhuetas legíveis, movimento e expressões coerentes com seus portraits.
- **Duas escalas artísticas, uma identidade:** cenários/portraits podem ser ricos; chibis e HUD devem continuar legíveis no tamanho real. A maid deve ser reconhecível nas duas apresentações sem ler seu nome.
- **UI funcional sobre arte:** nome, stats, economia, posição, raridade e comandos são elementos dinâmicos de UI, nunca texto impresso num fundo gerado. Preservar o código e as regras do jogo onde não houver necessidade comprovada de mudança.

**Referência aspiracional:** imagem fornecida pelo responsável nesta conversa em 28/10/2026: interior de café ilustrado em pixel art, molduras rosadas, portraits de neko maids à esquerda, diálogo de visual novel e chibis no café. **A imagem não está versionada neste repositório; não alegar que ela está disponível a um agente em um clone.** Os agentes precisam solicitar/receber uma cópia acessível antes de comparar fidelidade visual. Usar suas qualidades como direção, sem copiar personagens, interface ou composição literalmente; produzir designs originais e verificar direitos/licenças dos assets.

## Como aplicar (direção, não feature pronta)
| Contexto | Tratamento visual previsto |
| --- | --- |
| Menu / Empresa | Café com cenário envolvente, detalhes ambientais, personagens em destaque e UI temática legível |
| Elenco / cards | Portraits colecionáveis expressivos, grandes o suficiente para criar vínculo; moldura, raridade e atributos na UI |
| Escalação | Atmosfera de visual novel/café + cards/posições claros e feedback de química; nunca sacrificar o uso ou os estados válidos da escalação |
| Entre dias / eventos | Potencial para cenas curtas, portraits com troca de expressão e movimentos discretos; **não aprova automaticamente novo sistema narrativo** |
| Turno / salão | Chibis no layout funcional existente, cenário com camadas e atmosfera; preservar câmera, leitura, navegação e resultados até experimento demonstrar alternativa |

## Guardrails de produção
1. **IA cria, pessoa aprova a aparência.** Não exigir que o responsável desenhe, faça slicing, rige, edite sprites, programe ou monte cenas.
2. Geração artística pode ocorrer fora do runtime; no jogo, composição e animação devem ser suficientemente modulares para uma troca pontual de cabelo, expressão, acessório ou elemento de cenário sem regenerar tudo. **Modularidade detalhada é hipótese a provar**, não afirmar que uma imagem complexa gerada por IA já vem em camadas perfeitas.
3. A solução de geração/animação/formatos **não está decidida**: PNGs, camadas, formas procedurais, pixel-art tooling, rigs, Motion Canvas, modelos de IA e outros são candidatos para o experimento. Não instalar stack inteira nem produzir biblioteca em massa antes de validar uma cena e uma maid.
4. Priorizar qualidade de rosto, legibilidade do chibi e ambiente convincente; usar detalhe e saturação para direcionar o olhar às maids. Evitar pixelização artificial que destrua expressão.
5. Validar a composição de referência do jogo em **960×600**. A inspiração enviada é widescreen; **não esticar** imagens 16:9 para 16:10. Confirmar scaling, textos, botões, mouse e desempenho WebGL no Unity real.
6. Não alterar automaticamente GDD de economia, raridade, salários, movimento, fluxo de turno ou saves. **Escalação com um posto vazio é um estado válido**, não bug. Este trabalho é direção visual, não redesign mecânico.
7. Mockups, frames gerados e capturas de Unity são evidências diferentes; nomear honestamente. CI de simulação não valida resultado estético.

## Relação com ART-01 (A/B/C)
As três propostas vetoriais A · Café Pastel, B · Quadro-Negro Doce e C · Kissaten Showa no [PR #12](https://github.com/NThuesen/jogos_digitais_2D/pull/12) são **estudos exploratórios, não o estilo final aprovado**. Suas medições de contraste e legibilidade continuam úteis. O novo norte substitui A/B/C como alvo de qualidade, **não apaga os estudos** nem autoriza integrá-los no jogo.

## Próxima validação, sem ampliar escopo
- **ART-01D (exploração):** produzir uma interpretação original e comparável da referência com **uma maid**, portrait expressivo, chibi, uma cena de café e mockup de Escalação. Apresentar alternativas de composição/qualidade, custos, limitações de geração e prévias ao responsável.
- **Gate artístico:** responsável aprova visualmente um conjunto original de referências/arte final ou solicita nova iteração. Aprovar esta **direção conceitual** não equivale a aprovar qualquer asset gerado.
- **ART-02 (Unity):** permanece bloqueada até esse gate; então integrar uma identidade com portrait e chibi e validar no Editor real, a 960×600, com evidência visual e uma mudança modular pontual.
- Somente depois do vertical slice decidir pipeline de produção e expansão da biblioteca, registrando escolhas técnicas conforme a evidência.

**Fonte de verdade:** este documento registra a decisão sobre **estilo e apresentação**; [MAID_VISUAL_DECISION.md](MAID_VISUAL_DECISION.md) registra o contrato **portrait + chibi**. Não duplicar regras em `AGENTS.md`/`CLAUDE.md`; consultar este arquivo **apenas** em tarefas de direção artística, UI ou cenas de apresentação.
