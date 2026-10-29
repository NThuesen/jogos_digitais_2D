# ECON-01B — handoff factual

- Estado: implementação e validação headless concluídas; revisão final/PR pendentes.
- Branch/worktree: `task/econ-01b-roster-cap-20261029`, `neko-econ-01b`; revisão inicial `6187683fa018f00875705ff5c7c962058cd6f4ed`, base main `6d5767b`.
- Produção: apenas CafeLevelTable; cap derivado de Lineup.SlotsFor(level).Total + 1 por factory privada, preservando construtor público/ShiftSlots/demais unlocks.
- Consumidores: Lootbox.Open, ApplicantBoard.Hire e RosterScreen já usam o mesmo cap; não precisaram de alterações. Nenhum preenchimento automático novo.
- Exceção usada: somente setup de LootboxTests.AFullRosterTurnsACardIntoFichasRatherThanLosingIt em DayTests.cs, mantendo asserts anteriores e adicionando confirmação de lotação.
- Testes novos: RosterCapacityTests (15 casos): dez níveis, primeira Simples, caixa múltipla/RNG, contratação/dispensa, progressão sem autoassign e fixture real do schema 2 gerada antes da mudança com cap 2.
- Validação: 16/16 direcionados; simtest completo 272/272; boundaries PASS (49 fontes/14 arquivos); verificador NUnit PASS sem skips/inconclusivos. SDK Windows 10.0.400/runtime 8.0.14.
- Baseline: runner ECON-01A intacto; dados anteriores intocados. Comparação em docs/economy/econ-01b/comparison.json: passivo e mixed_passive idênticos exceto cap; caixas adicionam quarta maid, salários sobem, falência mediana segue dia 4. Script/reprodução no relatório.
- Unity: tentativa EditMode bloqueada pelo guard Windows (Editor já aberto), sem abrir Editor concorrente. PlayMode/capturas 960×600/Console/scene wiring NOT RUN. SaveBridge desta branch não isola PlayerPrefs; cópia descartável de validação não foi concluída por permissão de metadata, nem executada. Não alterados saves ou ProjectSettings reais.
- Próximos passos: (1) diff final e relatório; (2) PR exclusivo ECON-01B, sem auto-merge. O guard deve rejeitar a mudança autorizada de teste existente; integração depende do mantenedor, sem contorno de CI. Não iniciar C/D.
