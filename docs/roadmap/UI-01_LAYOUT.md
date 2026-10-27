# UI-01 — Estabilizar layout e legibilidade

**Status:** PLANNED; **owner sugerido:** Codex; **pode iniciar em paralelo com ART-01**.
**Escopo:** correções de apresentação em telas já existentes, **não redesign artístico**.
**Leia antes:** [Baseline visual](VISUAL_BASELINE.md), [Unity](../agent/UNITY.md), [Validação](../agent/VALIDATION.md). Não carregar GDD inteiro se o comportamento do jogo não mudar.

## Problema
Nos prints de referência a 960×600 há texto sobrepondo painéis, informações comprimidas e valores de difícil leitura. Em especial nas telas Loja, Elenco e Balanço. Como as capturas não estão no repo, **reproduzir primeiro no Unity e registrar defeitos efetivos**, não assumir a causa por inspeção de código.

## Ordem de ataque
1. **Loja:** visitar Móveis, Roupas e Presentes com listagens reais; eliminar sobreposição nome/descrição/preço e corrigir contraste/scroll. Confirmar botão de compra visível, clicável e valor correto. Código inicial: `Assets/_Game/Scripts/Game/Ui/ShopScreen.cs`.
2. **Balanço:** reproduzir um dia concluído; separar títulos, receitas, despesas, contadores de eventos e saldo final sem cortes. Valores devem permanecer vinculados à simulação, não rearranjados ou arredondados incorretamente. Código: `BooksScreen.cs`.
3. **Elenco:** detalhes do card, atributos, salários e ações devem caber e permanecer legíveis; lista e painel de detalhe não devem se sobrepor. Código: `RosterScreen.cs`, `MaidCard.cs`.
4. **Verificação curta de regressão:** entrar em Empresa, Escalação, Caixas e Salão, registrar defeitos de layout introduzidos e corrigir só regressões causadas pelo patch. Não transformar UI-01 em refactor geral.

## Invariantes e limites
- **Escalação vazia parcialmente é estado permitido, não bug.** Preservar posição vazia, banco, química e regras atuais.
- Preservar a paleta como **temporária**; não trocar tema, fontes, sistema de cards ou gerar assets novos nesta issue.
- Evitar mudanças globais em `UiKit.cs`/`Scenery.cs` se uma correção local resolver; se mudar componente compartilhado, verificar telas que o utilizam.
- Preservar cliques, scroll, navegação, estado de sessão, save, economy e scene wiring. Não mover lógica de `Neko.Sim` para UI.
- Não alterar resolução base do jogo como forma de esconder clipping; referência obrigatória **960×600**. Outros tamanhos são exploração, não requisito novo silencioso.
- Não alterar cena/pacotes/CI/testes protegidos só para atingir o layout (exigem fluxo excepcional próprio).

## Entrega e aceite
- PR pequeno com lista de problemas **reproduzidos** e links para capturas comparáveis *antes/depois* em 960×600, identificando Loja 3 abas, Elenco e Balanço.
- Sem textos cortados/colidindo, informações e botões legíveis e utilizáveis nesses estados; nenhuma regressão de navegação.
- Logs/Console sem erros relevantes ao navegar nesses fluxos; EditMode e PlayMode pertinentes executados quando disponíveis, cada um com resultado real.
- Se qualquer ponto da captura não reproduzir, registrar **não reproduzido** e não inventar correção.
- Visual aprovado como **legibilidade corrigida**, não como arte final aprovada.

**Não fazer:** modular maid art, paleta rosa, novas telas, animações, balanceamento, onboarding ou sistemas extras.
