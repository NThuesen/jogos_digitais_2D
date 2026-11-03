# Roadmap ativo — Neko Maid Inc.

**Atualizado em 03/11/2026.** Base integrada: [ART-03 / PR #29](https://github.com/NThuesen/jogos_digitais_2D/pull/29), [ART-04 / PR #30](https://github.com/NThuesen/jogos_digitais_2D/pull/30) e infraestrutura parcial [UI-01 / PR #31](https://github.com/NThuesen/jogos_digitais_2D/pull/31).
**Direção de arte vigente:** [anime 2D ilustrado/chibi + restaurante ilustrado futuro](art/CURRENT_VISUAL_DIRECTION.md). Planos anteriores pixel art são históricos.
**Objetivo:** estabilizar a interface antes de redesenhar o ambiente e, depois, os menus; sem modificar regras de jogo.

## Execução aprovada: estratégia B

| Marco | Entrega | Estado | Responsável sugerido | Contrato |
| --- | --- | --- | --- | --- |
| **DOC-01** | Alinhar documentação e registrar decisões vigentes, sem reescrever o histórico | CONCLUÍDO nesta entrega documental | Documentação | [Direção visual](art/CURRENT_VISUAL_DIRECTION.md) |
| **UI-01A** | Reusar o Layout_Check existente; corrigir clipping/colisões/scroll/cliques em **Loja, Balanço, Elenco**, 960×600 e 1280×720 | **CONCLUÍDO — PR #33 (9 defeitos)** | Codex | [UI-01 atual](roadmap/UI-01_LAYOUT.md) |
| **UI-01B** | Regressão das outras telas; corrigir apenas problemas bloqueantes e relatar cobertura | **CONCLUÍDO — PR #34 (9 defeitos)** | Codex | [UI-01 atual](roadmap/UI-01_LAYOUT.md) |
| **ENV-01** | Redesenhar o restaurante e mobiliário para anime 2D ilustrado/cozy, usando maids/escala atuais; sem redesign de menus | **FASE 4A — decisões estéticas em discussão**; sem assets finais aprovados | Claude Code (arte) + Codex (integração/QA) | [Direção visual](art/CURRENT_VISUAL_DIRECTION.md) |
| **UI-02** | Redesenhar menus, portraits e cards por tela com linguagem visual novel/management | PENDENTE após estabilização; assets e layouts sujeitos à aprovação | Claude Code + Codex | [UI-02](roadmap/UI-02_SCREEN_POLISH.md) |
| **UX-01** | Avaliar entendimento do primeiro dia e resultado em playtest | ADIADO | A definir | [UX-01](roadmap/UX-01_FIRST_DAY.md) |

**Concluído e preservado:** ART-03 (nove maids em turno real, PR #29), ART-04 (walk sheets ilustrados, HUD e WebGL QA, PR #30). **UI-01 concluído:** PR #31 (infraestrutura), PR #33 (Loja/Balanço/Elenco) e PR #34 (demais telas). **18 defeitos corrigidos**, 25 estados de auditoria registrados; não equivale à cobertura total dos 40 estados históricos. **Não concluído:** portraits exclusivos dos cards, animações ilustradas além do Walk, identidade visual persistida, cenário ilustrado e UI-02.

## Regras de execução

1. **Quatro entregas independentes:** documentação → UI-01 prioritário → regressão geral → redesign (restaurante antes dos menus). Manter escopo limitado e PRs separados; não presumir número de PR.
2. **UI-01A/B concluídos**: corrigiram a interface atual, sem defini-la como aparência final. Preservar as correções e o Layout_Check; UI-02 poderá alterar fontes, paleta, dimensões e composição com validação real.
3. O Layout_Check de 40 estados da [spec histórica](../.kiro/specs/ui-01-layout/tasks.md) **não** é pré-requisito de conclusão de UI-01A: expandir progressivamente. Nunca declarar cobertura que não foi executada. Cada correção precisa de defeito reproduzido e evidência antes/depois.
4. UI-01A prioriza Loja (Móveis, Roupas, Presentes), Balanço e Elenco. UI-01B confere Empresa, Escalação, Caixas, Menu, Salão, O dia, Dialogs e Falência. Corrigir regressões de input, leitura e navegação, não embelezar o tema provisório.
5. Referências de validação: **960×600 e 1280×720**, com testes em Unity real e, quando relevante, WebGL. Sim CI não comprova aparência.
6. Respeitar as mudanças de câmera e HUD do PR #30. Um turno QA com nove identidades não altera o limite de recrutamento normal. Posto vazio na escalação continua válido.
7. Não dividir alterações concorrentes em `UiKit`, `HallScreen`, `DayHud` ou `CameraRig` entre agentes; preservar `Neko.Sim`, save, branch protection e testes. Novas alterações nos testes `Layout_Check` já existentes exigem revisão de manutenção dos arquivos protegidos.
8. **Gate de produto:** usuário aprova arte final de cenário, portraits e menus antes de merge visual. Agentes podem resolver detalhes técnicos triviais; mudanças estruturais relevantes voltam ao responsável.
9. **Contrato 4A já aprovado (produção, não estética):** GPT gera assets gráficos **modulares** (fundos, peças, molduras, ícones, portraits etc.); Unity mantém layout, cliques, scroll, estados e **todos os textos/dados dinâmicos**. Nunca usar screenshot/PNG de tela inteira como UI funcional. Transparência pode ser obtida no Blender, desde que o alpha e as bordas sejam validados. Ver [contrato visual vigente](art/CURRENT_VISUAL_DIRECTION.md#decisão-de-produção-aprovada--imagens-modulares--interatividade-unity-4a).

## Documentação legada

- [ART-01 / explorações A/B/C/D](roadmap/ART-01_STYLE.md) e [direção cozy pixel-art anterior](art/COZY_VISUAL_NOVEL_DIRECTION.md): histórico de pesquisas, não estilo atual.
- [ART-02 / experimento de uma maid](roadmap/ART-02_VERTICAL_SLICE.md): experimento planejado na fase anterior, superado **apenas na integração do Walk** por ART-03/04; retratos próprios, camadas editáveis e `visual_id` **não** foram entregues.
- [Spec UI-01 de 29/10 e 02/11](../.kiro/specs/ui-01-layout/requirements.md): contrato e plano históricos que originaram os testes. Preservados para rastreabilidade, mas o escopo enxuto vigente está em [UI-01 atual](roadmap/UI-01_LAYOUT.md).

## Responsabilidades

**Codex:** implementação e validação técnica de UI-01A/UI-01B; **Claude Code:** direção/implementação artística, com revisão de capturas quando solicitado; **responsável:** aprova identidade estética e alterações de escopo. Ler apenas a nota da tarefa atribuída; [AGENTS.md](../AGENTS.md) rege branches, PRs, segurança e evidências.
