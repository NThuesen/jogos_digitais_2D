# Roadmap ativo — Neko Maid Inc.

**Estado:** plano de trabalho aprovado em 27/10/2026; os itens abaixo ainda não são entregas concluídas.
**Objetivo deste ciclo:** tornar o core loop já jogável mais legível, acolhedor e centrado nas maids, sem adicionar mecânicas novas.
**Para agentes:** este arquivo é um índice. Leia **apenas** a nota da tarefa atribuída e seus links relevantes. A regra de branch/PR permanece em [AGENTS.md](../AGENTS.md).

## Sequência e dependências

| ID | Entrega | Estado | Dependência | Contrato da tarefa |
| --- | --- | --- | --- | --- |
| **UI-01** | Corrigir layout, clipping e legibilidade de telas existentes | PLANNED | Nenhuma; pode ocorrer em paralelo com ART-01 | [UI-01](roadmap/UI-01_LAYOUT.md) |
| **ART-01** | Explorar propostas A/B/C de maid, card e chibi | REVIEW | Estudos no [PR #12](https://github.com/NThuesen/jogos_digitais_2D/pull/12), não aprovados como estilo final | [ART-01](roadmap/ART-01_STYLE.md) |
| **ART-01D** | Explorar a direção **cozy pixel-art anime + visual novel** em arte original | PLANNED | Norte conceitual aprovado; ainda falta aprovação de assets | [Direção aceita + experimento](art/COZY_VISUAL_NOVEL_DIRECTION.md) |
| **ART-02** | Integrar uma identidade portrait + chibi no Unity (vertical slice) | BLOCKED | Prévia/identidade visual original de ART-01D **aprovada pelo responsável** | [ART-02](roadmap/ART-02_VERTICAL_SLICE.md) |
| **UI-02** | Evoluir UI conforme direção artística aprovada, por telas | BLOCKED | Assets/estilo de ART-01D aprovados; usar aprendizado de ART-02 | [UI-02](roadmap/UI-02_SCREEN_POLISH.md) |
| **UX-01** | Avaliar compreensão do primeiro dia/resultado sem presumir erro de balanceamento | DEFERRED | Evidências de playtest após as correções relevantes | [UX-01](roadmap/UX-01_FIRST_DAY.md) |

**PLANNED = pendente; REVIEW = estudos entregues para avaliação, sem aprovação final; BLOCKED = dependência não cumprida; DEFERRED = não iniciar sem evidência.** Nenhum estado é atualizado por suposição; registrar a Issue/PR e a evidência ao mudar o status. Não inventar datas ou tempo estimado.

## Responsabilidades e ciclo de correção

- **Claude / Claude Code:** ART-01D — referências originais cozy pixel-art + visual novel, imagens de prévia, comparação crítica e recomendação. Não alterar gameplay, não escolher sozinho o estilo definitivo e não implantar infraestrutura de geração em massa.
- **Codex:** implementação pontual de UI-01; depois ART-02 e UI-02 conforme gates e decisões. Inspecionar estado real, criar branch automaticamente, validar e abrir PR.
- **Claude Code como revisor, quando acionado:** revisar diffs e **capturas reais** de patches Codex; classificar `blocking / important / optional`. Codex corrige apenas findings válidos e produz novas evidências. Revisão não é uma dependência automática de outra pessoa para todo PR.
- **Responsável:** aprova/rejeita **aparência e decisões de produto**; não precisa desenhar, codificar ou analisar código manualmente.

**Fluxo quando houver revisão:** Codex implementa → valida e produz evidências → Claude Code revisa implementação e captura → Codex corrige findings válidos → revalidação → PR/integração conforme proteção GitHub. Não criar IA observadora de CI.

## Restrições deste ciclo
1. **UI-01 não depende da direção artística final.** Corrigir defeitos de layout sem trocar todos os temas.
2. **ART-01D é exploração de qualidade visual; ART-02 é implementação.** A direção geral está aprovada, mas **nenhum asset final nem stack técnica** está aprovado. Não fazer biblioteca grande de módulos ou refactor de UI nesta exploração.
3. Uma maid tem portrait para card e chibi para restaurante, com identidade consistente, conforme [decisão aceita](art/MAID_VISUAL_DECISION.md). Não fixar formatos/stack não decididos.
4. O GDD rege regras, economia e experiência; a [arquitetura](ARCHITECTURE.md) rege boundaries técnicos. Este roadmap não os substitui.
5. Para UI e gameplay, evidência **dentro do Unity na resolução de referência 960×600** (sem extrapolar para todas as telas/resoluções), incluindo capturas antes/depois. CI da simulação não verifica UI visual.
6. Não confundir preferências visuais com bugs: **deixar uma posição vazia e uma maid no banco na escalação é comportamento válido**, confirmado pelo responsável. Não corrigir como defeito.
7. Manter tarefas pequenas, sem infra nova, testes artificiais, mudanças de balanceamento ou publicações sem autorização.

**Evidência de partida:** [observações dos prints de 27/10/2026](roadmap/VISUAL_BASELINE.md). Esses prints foram fornecidos na conversa e **não são arquivos versionados**; o agente que trabalhar no Unity deve obter sua própria captura comparável.

## Trilha paralela: ECON-01A — diagnóstico econômico (autorizado em 29/10/2026)

A preparação do baseline econômico foi autorizada como tarefa **independente** dos marcos de arte/UI acima. Consultar [ECON-01A](roadmap/ECON-01_BASELINE.md) para escopo, gates e validação; checkpoint operacional em [ECON-01_HANDOFF](../.agent/ECON-01_HANDOFF.md). O documento de origem [proposal.md](../proposal.md) é uma **proposta não aprovada de números**, não uma implementação.

**Estado: READY / execução não iniciada.** Esta autorização cobre reprodução do diagnóstico, instrumentação mínima e documentação, **não** mudanças de balanceamento, produto, cenas ou integração em main. As etapas seguintes exigem branches/PRs próprios e aprovação das decisões materiais. A restrição 7 do ciclo visual permanece em vigor para as tarefas visuais.
