# Restrições de texto e margem segura — UI-007 v02 e UI-009 v02

**Candidatos, aprovação artística PENDING, não integrados.** Valores medidos no QA Editor da PR #49 (frames de 540 px de largura em referência 1280×720, fonte Unity `UiKit.Label` bold) e convertidos para frações do master, de modo que valham em qualquer escala. Derivam de [editor_results.json](evidence/BATCH-V02-QA-2026-11-04/editor_results.json) e das capturas. Recomendações para a futura integração, não implementação.

## UI-007 v02 (master 1733×908; uso ≈620×325)
Laço/gato ocupam o centro-topo até **≈27% da altura**; o creme útil começa em ≈19% e termina em ≈90%.

| Zona | Retângulo seguro (fração do frame) | Regra |
| --- | --- | --- |
| **Título** | x 19–81%, **y 30–40%** (1 linha ≈10%) | Sempre **abaixo do laço**; nunca no topo (24% colide). 1 linha, `Best Fit`/autosize **20→14 pt**, depois truncar com reticências. Medido: 36 caracteres a 20 pt (≈354 px) **não cabem** nos ≈335 px de 540 px de largura ⇒ ≈**≤32 caracteres a 20 pt** (≈≤37 a 620 px). O texto completo vai ao corpo ou a tooltip, nunca em 2 linhas no título. |
| **Corpo** | x 14–86%, y 42–68% | Multilinha com quebra; 2–3 linhas a 16 pt cabem folgadas; mais que isso: 14 pt mínimo e rolagem/ellipsis na última linha. |
| **Botões** | y 74–88%, centros em ±17% do centro | 2 botões ≥130×34 u cabem; 3 botões não testados. |
| **Margem segura** | ≥ 6% da largura nas laterais; ≥ 4% do topo/base do creme | Ornamentos de canto e patas ocupam ≈12% por canto. |

Evitar 9-slice neste PNG (laço central). Uso: Simple/preserveAspect com a proporção 1,91:1.

## UI-009 v02 (master 1536×1024; uso 3:2, ≈540×360)
| Zona | Retângulo no master | Regra |
| --- | --- | --- |
| **Janela do retrato** (transparente) | bbox x 77–550, y 125–735 (≈31%×60%) | Retrato **atrás** do frame, máscara no bbox. O laço cobre o topo: manter a **cabeça ≥ 18% abaixo** do topo da janela; altura do retrato 78–98% da janela, alinhado à base. Retrato real dedicado ainda não testado (usado frame de walk sheet). |
| **Nome** | x 580–1480, y 100–250 (≈59%×15%) | Autosize 20→14 pt em 1 linha; **≤ ≈26 caracteres a 20 pt**. Nome longo de 32 caracteres quebrou em 2 linhas e **tocou a borda** da placa: aceitar 2 linhas apenas ≤16 pt; senão truncar com reticências e expor o nome inteiro em Detalhes. |
| **Atributos/dados** | x 580–1480, y 270–750 | 6 linhas a 15 pt ocuparam ≈75% da altura útil: **máx. 7 linhas a 15 pt**; acima disso 13 pt mínimo ou paginar/rolar. Barras, sinais + e moedas são desenhados pela Unity; a arte não os contém. |
| **Faixa inferior** | x 50–1480, y 760–945 | Botões Unity ≥120×30 u; 2 testados com folga. |
| **Margem segura** | ≥ 5% de inset em cada placa | Evitar texto a menos de ≈3% das bordas douradas. |

## Políticas gerais para integração
1. **Autosize** com mínimo de 14 pt (legibilidade em 960×600); abaixo disso, truncar.
2. **Truncamento**: reticências na última linha permitida; nunca overflow para fora da placa; conteúdo integral acessível em outra tela/tooltip.
3. Testar sempre texto **longo** e **localizado** (PT-BR pode crescer 20–30%).
4. Confirmar no import "Alpha Is Transparency" (UI-009) e Full Rect; não aplicar borders de outras resoluções.

Aprovação artística do laço central (UI-007) e da abertura de 31% largura (UI-009) continua pendente do responsável.
