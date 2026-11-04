# Evidências da auditoria em lote

[Revisão e classificação consolidada](../../UI_BATCH_002_003_008_COMPACT_STATES_V01.md).

- `analysis.json`: integridade, dimensões/bytes/SHA-256, histogramas completos, bboxes, resíduos, recortes, invariância RGB e comparação proporcional dos estados.
- `classification.json`: resultado individual e indicação de QA futuro.
- `*_raw_clean_proof.png`: mesmo recorte e escala, RAW à esquerda/CLEAN à direita, sobre claro/escuro/magenta. São composições de evidência, não assets novos.
- `frames_style_comparison.png`: referências de estilo, não comparação de dimensões de runtime.
- `states_raster_comparison.png`: simulação de slice em Python. **Não é screenshot Unity**, não contém texto/controle/interação e não aprova nenhum estado.
- `evidence_manifest.json`: SHA-256/bytes das composições versionadas.

## Reprodução

Executar `python audit_batch.py.txt` **neste local do checkout**, com os seis RAWs nos caminhos exatos e as referências aprovadas presentes. Usar as versões de Python/Pillow/NumPy/SciPy registradas em `analysis.json`. O script deriva o root por sua localização, sem depender de cwd.

O script somente regrava os CLEANs de UI-002/UI-003/UI-008 e os relatórios/composições deste lote. Não regrava RAW, UI-001 ou NORMAL aprovado; a reamostragem de referência dos estados ocorre somente em memória para comparação. Não há execução Unity, builds, testes de jogo, export de estados, geração de arte ou recoloração. O padding extra de2px à direita do UI-003 é transparente e não altera os pixels existentes.

Conferir os hashes das entradas antes/depois. As classificações são pareceres do agente registrados no JSON/revisão, não são aprovadas automaticamente por medidas numéricas. Os arquivos de arte brutos/limpos permanecem ignorados no Git.
