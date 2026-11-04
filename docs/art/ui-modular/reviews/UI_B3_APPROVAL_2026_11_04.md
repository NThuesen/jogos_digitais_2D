# Decisão B3 — aprovações e inspeção da Loja (04/11/2026)

Autoridade: aprovação expressa do responsável na tarefa "Aprovação artística e fechamento do lote B3 | PR #50". QA reutilizado ([relatório](UI_BATCH_B3_2026_11_04.md)); nada repetido, integrado, gerado ou buildado. WebGL NOT RUN.

| Asset | Estado | Restrição resumida |
| --- | --- | --- |
| UI-011 CAT_PAW, GIFT, SHOP_BAG | **APPROVED** | Ver README B3 |
| UI-011 CLOCK, GEAR | **APPROVED** | 32–44u; 24u interno não homologado |
| UI-011 CUP | **APPROVED** | Vapor não exigido a 24u |
| UI-011 COIN | **APPROVED** | Grão de café = moeda do jogo |
| UI-011 STAR | **APPROVED** | Tom vivo aceito |
| UI-011 HEART | **APPROVED** | Padding quadrado no export |
| UI-018 BALANCE ROW NORMAL | **APPROVED (restrito)** | Só NORMAL, cápsula fixa; 380×46/520×54/300×40 |
| UI-020 LOGO WORDMARK | **APPROVED (restrito)** | 520u/200u sobre UI-012; outros contextos a validar |
| UI-017 SHOP ITEM FRAME | **PENDING — recomendar v02** | Condição "sempre com ícone" não satisfeita |

## Inspeção do uso real da Loja (somente leitura)
`ShopScreen.BuildRow`: linha `UiKit.Panel` 850×62 (9-slice, tinta por `affordable`). Ícone apenas se `item.Kind == Placeable` (sprite do móvel 60×50, texto recuado 118u). Abas: Móveis (Placeable), Roupas (Equipment) e Presentes (Gift) — **2 de 3 abas sem ícone**, onde o texto começa a ≈185u da borda e a reserva pintada à esquerda ficaria vazia. Nenhuma alteração na Loja foi feita para justificar a aprovação. O botão Comprar (150×44) e a contagem "possuídos" cabem na faixa direita testada, mas isso não supera a condição. **Veredito: PENDING; v02 sem reserva/divisor fixos.**

UI-018: não identifiquei tela de produção equivalente (o saldo atual é um `Text` no cabeçalho da Loja e em `BooksScreen`); aprovado conforme decisão do responsável, sem inferir integração.
