# ECON-01A — checkpoint factual

- Estado: critérios técnicos satisfeitos; revisão final/PR pendentes.
- Branch/worktree: `task/econ-01-baseline-20261029`, `neko-econ-01`. URP e ZIP da tarefa visual preservados no checkout original.
- Código de produção: `7d42ab1f7dd3981c56defdd0b3edaee08918749f`; fontes Sim/Game e regras intactas.
- Runner: `tools/econ-baseline/` (projeto net8.0, sem pacotes; inclui fontes reais Sim); instruções no README local.
- Evidência versionada: `docs/economy/econ-01a/` (relatório, agregados, 5028 dias CSV e metadados).
- Comandos: `dotnet run --project tools/econ-baseline/EconBaseline.csproj -- Logs/econ-final1`; `python3 tools/econ-baseline/report.py Logs/econ-final1`; repetir em final2 e comparar os três arquivos.
- Determinismo: duas execuções finais idênticas; SHA256 CSV `66141d3b1f45f8645ee0e060ce4c4b2e5930b4714627bcd8cae1e3da46512985`.
- Amostra: cinco políticas ×60 seeds sequenciais + mesmas políticas ×60 seeds espalhadas; 200 jogos dia 1; oito planos ×100 seeds. Até 50 dias, encerrando na falência.
- Resultados: 0/60 sobrevivem dia 7/20/50 em cada política e conjunto; medianas 4/3/4/4/3. Dia 1: perda 200/200, receita/custo medianos 119/254. Nenhum L2. Competente e Auto iguais neste elenco. Conversão de caixas sensível às seeds; valores e vieses documentados.
- Validação: simtest existente PASS 257/257, boundaries PASS (49 fontes/13 testes), duas saídas/agregações idênticas. SDK Windows 10.0.400/runtime 8.0.14, target net8.0. SDK 8 específico/Unity/WebGL NOT RUN, com motivo nos READMEs.
- Original `/tmp/neko-econ` indisponível; política reconstruída e discrepâncias documentadas. Nenhuma conclusão de Tier 1/2, raridade/ROI tardio ou playtest humano.
- Próximos passos: (1) revisão final de diff; (2) abrir PR exclusivo ECON-01A e encerrar. ECON-01B/C/D exigem outra tarefa.
