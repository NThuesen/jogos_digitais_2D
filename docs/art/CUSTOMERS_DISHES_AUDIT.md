# Auditoria: clientes e pratos (estado do código na main `6c1c54d`)

Só leitura do código e das docs; nada foi implementado nem gerado. Decisões do dono já fechadas: **8 clientes chibi** como escopo inicial (cada um com design base + spritesheet de caminhada + sprite sentado) e **1 ícone ilustrado por receita** (sem animação, fundo transparente).

## 1. Clientes hoje

| Item | Estado atual (fonte) |
|---|---|
| Tipos | 7 `CustomerType`: Regular, Student, Salaryman, Cat, Otaku, Critic, Vip (`Sim/Service/CustomerTable.cs`). Desbloqueio por nível do café: Regular/Student 1, Salaryman/Cat 2, Otaku 3, Critic 4, Vip 6. Peso de chegada: Regular 10, Student 6, Salaryman 5, Otaku 4, Cat 3, Critic 2, Vip 2. |
| Visual | 100 % procedural: `CustomerView` monta uma pilha de formas (sombra, avental na cor de acento, cabeça circular, olhos, e um adereço por tipo: orelhas do Cat, estrela do Vip, caderno do Critic, boné do Salaryman, coração do Otaku, mochila do Student) via `SpriteFactory.Layered("customer:<tipo>", 128)`. Um sprite estático por tipo. |
| Escala | `CustomerView.CustomerScale = 0.72` (propositalmente menor que as maids). |
| Feedback de paciência | Tinta do corpo (`Palette.ForPatience`), balanço do sprite e barra sob o corpo; não depende de arte específica, mas a tinta multiplica a cor do sprite (a arte precisa tolerar isso). |
| Estados | `Queueing, Seating, Thinking, Ordering, Waiting, Eating, Paying, Leaving, Angry` (`Customer.cs`). |
| **Estado sentado** | **Não existe.** O mesmo sprite de pé é usado em todos os estados. `PositionOf` põe o cliente na célula da cadeira (`y + 0.42`) ou na fila ao lado da porta (colunas de 4); o posicionamento é por atribuição direta a cada frame, **sem interpolação**: o cliente "teleporta" da fila para a cadeira (`Seating` dura 0,8 s só na simulação). Não há animação de caminhada. |
| Grupos | `PartySize` 1–3 existe na simulação (Student até 3), mas é **uma figura por grupo**. |
| Onde aparece | Só no Salão durante o dia (`ShiftPresenter.SyncCustomers`); o ciclo de vida da view acompanha `Shift.Customers`. Não aparece em Menu, Hub, Elenco, Escalação, Caixas nem Loja. Os testes de visão: `CustomerViewCacheTests` (EditMode). |

## 2. Pratos/receitas hoje

- **14 receitas** em `Sim/Service/RecipeBook.cs`: Stove 5 (milk, omurice, curry, ramen, omurice_deluxe), TeaBar 3 (matcha, bubble_tea, royal_tea), Dessert 4 (pancake, parfait, sundae, parfait_royal), Bakery 2 (catbun, cake). Desbloqueio do nível 1 ao 10.
- Representação: cada receita tem `ShapeId` + `TintHex` (círculo, gota, cápsula, anel, triângulo, estrela, orelha de gato, retângulo arredondado). Há colisões claras: 4 cápsulas (curry, matcha, bubble_tea, royal_tea), 2 gotas (omurice/omurice_deluxe), 2 triângulos rosa (parfait/parfait_royal); só a cor diferencia.
- **Único lugar onde o prato aparece hoje:** o balão sobre o cliente (`CustomerView.IconFor`, estados Ordering/Waiting), com ~0,25 tile de largura (cerca de 16–25 px em 960×600). Não há bandejas, cartões de receita, livro de receitas nem lista de cardápio na UI do Game; os textos de prompt que citavam "bandejas e cartões" descrevem usos futuros, não existentes.
- Os demais ícones do balão (Note = pedido a tomar, Heart = sem prato, Door = fila, hoje oculto) também são formas procedurais; ficam fora do escopo desta fase.

## 3. Recomendação de produção

### Clientes (8)
1. **7 tipos existentes** (um design por `CustomerType`) + **1 variante de Regular** (Regular é o tipo mais frequente, peso 10; uma segunda aparência quebra a repetição sem tocar a economia). A escolha da 8ª é do dono; sem decisão, usar a variante de Regular. Sua escolha precisa de um mapeamento simples na view (por exemplo, id par/ímpar do cliente), sem novo `CustomerType`.
2. Por cliente: (a) design chibi base, (b) spritesheet de caminhada lateral, (c) sprite sentado (3/4, meio corpo visível sobre a mesa).
3. Tamanho: o jogo desenha ~85–95 px de altura; produzir a sheet de caminhada com célula ≈ 96 px de altura (não 252 px como as maids) para manter o custo de textura baixo: ~8 sheets × 0,3–0,6 Mpx.

### Pratos (14)
Um PNG transparente por receita, 1:1, gerado a 512 px e derivado para **128×128** (o balão é muito menor que 512). Ids e cores em `ui-modular/prompts/27_DISH_ICONS.md` (já batem com `RecipeBook`). Leitura a 20 px: silhueta e cor dominante têm prioridade sobre detalhes.

### Pipeline sugerido
1. **Gerar** os designs base (prompts: `26_CUSTOMER_CHIBIS.md`, `27_DISH_ICONS.md`) → aprovação do dono, um por vez.
2. **Caminhada** dos clientes pelo mesmo fluxo já usado nas maids (`docs/art/prompt-pipeline/v3/`: frame inicial → vídeo → Blender → `maid_walk.py`), exigindo adaptar o `maids.json` (ou uma cópia `customers.json`) para célula pequena e ids de cliente.
3. **Sentado**: um PNG por cliente, gerado a partir do design aprovado (prompt na seção Sentado de `26_CUSTOMER_CHIBIS.md`).
4. **Importar** em `Assets/_Game/Resources/` com proveniência SHA-256 e allowlist em `DocsTests` (lista fechada; é um teste protegido, então precisa de exceção de mantenedor).
5. **Integrar** em uma PR de código separada: `CustomerView` com fallback procedural (como as maids: se não houver sheet, mantém o visual atual) e `IconFor` lendo o ícone por id de receita (fallback: forma atual).

## 4. Dependências técnicas e pontos abertos (cliente sentado e andando)
1. **Sem estado visual sentado:** é preciso escolher o sprite por `CustomerState` (sentado em `Thinking/Ordering/Waiting/Eating/Paying`; em pé/andando em `Queueing/Seating/Leaving/Angry`).
2. **Sem movimento:** a view atribui posição direta; uma caminhada exige interpolar fila → cadeira (0,8 s de `Seating`) e cadeira → porta. A simulação não precisa mudar; é trabalho de `ShiftPresenter`/`CustomerView`.
3. **`Leaving/Angry`:** `FreeSeat` zera `SeatIndex` e `PositionOf` cai no ramo da fila, então um cliente que sai hoje é posicionado como se estivesse na fila até a view ser destruída. Verificar antes de animar a saída.
4. **Ordem de desenho e oclusão:** o sentado deve ficar atrás da borda da mesa e em frente à cadeira; hoje `Layers.Characters` ordena por y. Definir a âncora/pivô (hoje deslocamento 0,42 acima da célula).
5. **Grupos 1–3:** uma figura por grupo continua; decidir se o grupo de 2–3 mostra figuras extras (mais arte) ou apenas um número/marca.
6. **Tinta de paciência:** `_body.color` multiplica o sprite; validar com arte colorida (fica avermelhada). O Cat é "NeverImpatient" (sem tinta).
7. **Tipo Cat:** é um gato de verdade (não uma gata-maid); o sprite sentado precisa de pose de gato na cadeira.
8. **Memória/WebGL:** as sheets das maids já subiram o build a ~104 MB; o orçamento das novas texturas precisa seguir a otimização de texturas registrada no backlog (compressão, mipmaps desligados em sprites 2D, carregamento sob demanda).
9. **Testes:** `DocsTests` (allowlist/proveniência) e os QA de PlayMode que contam views de cliente podem precisar de ajuste na PR de integração.
10. **Fora de escopo agora:** `CustomerType` novos, mudança de pesos, economia, saves e recrutamento. Nada disso é necessário para trocar o visual.
