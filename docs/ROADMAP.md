# Roadmap ativo — Neko Maid Inc.

**Estado:** plano de trabalho aprovado em 27/10/2026; os itens abaixo ainda não são entregas concluídas.
**Objetivo deste ciclo:** tornar o core loop já jogável mais legível, acolhedor e centrado nas maids, sem adicionar mecânicas novas.
**Para agentes:** este arquivo é um índice. Leia **apenas** a nota da tarefa atribuída e seus links relevantes. A regra de branch/PR permanece em [AGENTS.md](../AGENTS.md).

## Sequência e dependências

| ID | Entrega | Estado | Dependência | Contrato da tarefa |
| --- | --- | --- | --- | --- |
| **UI-01** | Corrigir layout, clipping e legibilidade de telas existentes | PLANNED | Nenhuma; pode ocorrer em paralelo com ART-01 | [UI-01](roadmap/UI-01_LAYOUT.md) |
| **ART-01** | Explorar e comparar propostas de estilo com maid, card e chibi | PLANNED | Nenhuma; decisão estética do responsável ao final | [ART-01](roadmap/ART-01_STYLE.md) |
| **ART-02** | Integrar uma identidade portrait + chibi no Unity (vertical slice) | BLOCKED | Direção visual ART-01 **aprovada** | [ART-02](roadmap/ART-02_VERTICAL_SLICE.md) |
| **UI-02** | Evoluir UI conforme direção artística aprovada, por telas | BLOCKED | ART-01 aprovada; usar aprendizado de ART-02 | [UI-02](roadmap/UI-02_SCREEN_POLISH.md) |
| **UX-01** | Avaliar compreensão do primeiro dia/resultado sem presumir erro de balanceamento | DEFERRED | Evidências de playtest após as correções relevantes | [UX-01](roadmap/UX-01_FIRST_DAY.md) |

**PLANNED = pendente; BLOCKED = dependência não cumprida; DEFERRED = não iniciar sem evidência.** Nenhum estado é atualizado por suposição; registrar a Issue/PR e a evidência ao mudar o status. Não inventar datas ou tempo estimado.

## Responsabilidades e ciclo de correção

- **Claude / Claude Code:** ART-01 — alternativas de estilo, imagens de prévia, comparação crítica e recomendação. Não alterar gameplay, não escolher sozinho o estilo definitivo e não implantar infraestrutura de geração em massa.
- **Codex:** implementação pontual de UI-01; depois ART-02 e UI-02 conforme gates e decisões. Inspecionar estado real, criar branch automaticamente, validar e abrir PR.
- **Claude Code como revisor, quando acionado:** revisar diffs e **capturas reais** de patches Codex; classificar `blocking / important / optional`. Codex corrige apenas findings válidos e produz novas evidências. Revisão não é uma dependência automática de outra pessoa para todo PR.
- **Responsável:** aprova/rejeita **aparência e decisões de produto**; não precisa desenhar, codificar ou analisar código manualmente.

**Fluxo quando houver revisão:** Codex implementa → valida e produz evidências → Claude Code revisa implementação e captura → Codex corrige findings válidos → revalidação → PR/integração conforme proteção GitHub. Não criar IA observadora de CI.

## Restrições deste ciclo
1. **UI-01 não depende da direção artística final.** Corrigir defeitos de layout sem trocar todos os temas.
2. **ART-01 é exploração; ART-02 é implementação.** Não fazer biblioteca grande de módulos ou refactor de UI em ART-01.
3. Uma maid tem portrait para card e chibi para restaurante, com identidade consistente, conforme [decisão aceita](art/MAID_VISUAL_DECISION.md). Não fixar formatos/stack não decididos.
4. O GDD rege regras, economia e experiência; a [arquitetura](ARCHITECTURE.md) rege boundaries técnicos. Este roadmap não os substitui.
5. Para UI e gameplay, evidência **dentro do Unity na resolução de referência 960×600** (sem extrapolar para todas as telas/resoluções), incluindo capturas antes/depois. CI da simulação não verifica UI visual.
6. Não confundir preferências visuais com bugs: **deixar uma posição vazia e uma maid no banco na escalação é comportamento válido**, confirmado pelo responsável. Não corrigir como defeito.
7. Manter tarefas pequenas, sem infra nova, testes artificiais, mudanças de balanceamento ou publicações sem autorização.

**Evidência de partida:** [observações dos prints de 27/10/2026](roadmap/VISUAL_BASELINE.md). Esses prints foram fornecidos na conversa e **não são arquivos versionados**; o agente que trabalhar no Unity deve obter sua própria captura comparável.
