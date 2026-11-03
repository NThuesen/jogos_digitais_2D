# UI-01A — Defect log (Loja, Balanço, Elenco)

Escopo: estabilização funcional das três telas prioritárias, sem redesign. Resoluções: **960×600** e **1280×720**.
Evidência "antes" = `origin/main` (`feac788`) medida com as regras novas; "depois" = esta branch. Capturas reais do jogo em `evidence/`
(`before_*` / `after_*`); registros brutos do Layout_Check em `report_before.jsonl` / `report_after.jsonl`.
Capturas e relatórios foram gerados com `NEKO_CAPTURE_DIR`, `NEKO_LAYOUT_REPORT` e `NEKO_LAYOUT_DUMP`, como descreve `LayoutCheckTests`.

## Causa comum — C1 reproduzida

`UiKit.Panel` usava um sprite 96×96 de retângulo arredondado desenhado a 84% do sprite e **esticado** (`Image.Type.Simple`). Todo painel largo
desenhava só ~84% do seu retângulo (8% de margem transparente por lado) e tinha cantos borrados. O conteúdo era posicionado contra o retângulo,
então texto e botões ficavam fora do painel visível. Correção: `SpriteFactory.Panel(radius)` (sprite com borda 9-slice, desenhado até a borda)
e parâmetro opt-in `exact` em `UiKit.Panel`, `UiKit.ScrollList`, `Scenery.Plaque` e `MaidCard.Build`. **Só Loja, Balanço e Elenco passam `exact: true`;**
as demais telas continuam com o desenho antigo (sem alteração visual nelas — Marco 3).

## Defeitos reproduzidos e corrigidos (9)

| # | Tela / estado | Resolução | Observado (antes) | Regra | Correção |
|---|---|---|---|---|---|
| D1 | Loja, Móveis/Roupas/Presentes (LJ-1..3) | ambas | Nome/descrição começam ~42 u **fora** do painel creme da linha (C1) | R3.1 (LJ-2: 26 hits em 960, 13 em 1280; LJ-3: 12/6) | Painéis da linha e da lista 9-sliced (`exact`) |
| D2 | Loja, Móveis e Roupas (LJ-1, LJ-2) | ambas | "No estoque: N" desenhado **sob o botão de preço** (sobreposição 36×16 u); contagem ilegível | R4.2 (14 em 960, 7 em 1280, por estado) | Rótulo movido (centro 215→165); borda direita 250 < início do botão 265 |
| D3 | Balanço com 13 chips (BL-3) | ambas | Os 5 contadores + 8 "subiu de nível" em **uma só linha**: chips além da janela (até 1063 u à direita em 960) | R2.1 (42 em 960, 18 em 1280) | Grade de 7 colunas × 2 linhas (148×54 u) |
| D4 | Balanço (BL-1..3) | ambas | Título "Dia excelente/Prejuízo", colunas, valores (`¥`) e "Caixa depois" cortados/sob a moldura da placa (C1) | R3.1 (BL-1: 38 em 960, 16 em 1280) | Placa, linhas e chips 9-sliced (`exact`) |
| D5 | Elenco (EL-1..3) | 960×600 | Grade (700 u) **sob** o painel de detalhes: 92 u de sobreposição; 28–34 textos do painel sobre a grade | R4.2 | Largura da grade 700→600 (termina em 622 < 630); linha 680→580 |
| D6 | Elenco, cartões do grid (EL-1..3) | ambas | "Salário N ¥" e linha de pips encostando/cortados pela moldura do cartão (C1) | R3.1 (50 em 960, 25 em 1280) | `MaidCard.Build(..., exact: true)` apenas no Elenco (grid e detalhe) |
| D7 | Elenco, detalhe (EL-1..3) | ambas | "Etiquetas" quebra em 2 linhas e invade a linha "Nível" (sobreposição 54×2,6) | R4.1 | `Fact(..., lines: 2)`: linha de 44 u para etiquetas |
| D8 | Elenco, detalhe (EL-2) | ambas | Atributos, traços e assinatura sob a moldura da placa; "Dispensar" fora da placa (C1) | visual (capturas) | Placa 9-sliced (`exact`) |
| D9 | Elenco, cabeçalho | 960×600 | "8 / 8 · Custo fixo por dia 1.414 ¥" quebra em 2 linhas fora da caixa de 32 u | visual (capturas) | Largura 300→460 |

Sem violações no estado "depois": EL-1..3, LJ-1..4, BL-1..3 e EM-1 passam, nas duas resoluções e no retorno a 960×600 (3 passos).

## NOT REPRODUCED / não avaliado

- Observações das capturas de outubro **não** foram assumidas; só os defeitos acima foram medidos nesta base.
- Clique/área de clique nos botões das três telas (R6.2): sem falha no "antes" nem no "depois". `NOT REPRODUCED`.
- **Não coberto** (não executado): EL-4 (grid rolado ao fim; o elenco máximo de 8 não excede a área visível), EL-5/LJ-5/LJ-6 (diálogos), BL-4
  ("O café não abriu" com a pior razão de falha), caso Balanço com `Caixa` positivo + aviso (BL-2 cai em Falência por ter caixa 40 ¥), valores "Large"
  de salário 9.999 ¥ no Elenco (o salário vem da simulação; não forçado). Os 40 estados da spec histórica **não** foram implementados; 11 estados estão registrados (EM-1 + 10 novos).

## Infraestrutura (protegida) alterada

`Assets/_Game/Tests/PlayMode/Layout/AuditStates.cs`: +10 estados (EL-1..3, LJ-1..4, BL-1..3) construídos com `SessionFactory`/`ProfileBuilder`, `LargeValues` e `Longest`.
`LayoutRules.cs`: +R3.1 (texto dentro da própria placa), R4.1 (texto × texto), R4.2 (texto sob controle alheio), R6.2 (botão ≥ 44 u e clique alcança o botão),
restritas aos estados EL/LJ/BL (a tela Empresa, UI-01B, tem achados próprios — ver abaixo). Nada foi removido ou afrouxado: R2.1 e EM-1 intactos e passando.
Tolerância de 2 u (`LineBoxSlack`) para a caixa de linha do texto; justificativa no código.

## Problemas conhecidos → UI-01B

- **Empresa (EM-1):** o wordmark "NEKO MAID INC." sobrepõe o primeiro rótulo da placa de estatísticas (22×14 u em 1280, 21,6×12,8 u em 960); R3.1 acusa 0,5–1,3 u de
  texto fora de painéis do Hub. Não corrigido (fora do escopo); as regras novas não rodam em EM-1.
- Demais telas (Escalação, Caixas, Menu, Salão, O dia, Diálogos, Falência) e `Scenery.Plank`/`UiKit.Panel` não migrados para `exact`.
- Falha pré-existente no EditMode (não relacionada): `DocsTests.TheReadmeCreditsTheOneImportedSuzuSheet` (README sem a seção "Créditos de assets"). Não tocada.
- Console: o Unity Test Framework falha o teste em erro de log não esperado; a asserção explícita `LogAssert.NoUnexpectedReceived()` do TODO do `Audit` não foi acrescentada.
