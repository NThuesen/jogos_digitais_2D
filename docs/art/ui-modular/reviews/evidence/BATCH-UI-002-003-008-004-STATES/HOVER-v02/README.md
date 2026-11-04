# Adendo HOVER v02 — auditoria isolada

[Veredito e limitações](../../../UI_BATCH_002_003_008_COMPACT_STATES_V01.md#7-adendo--ui-004-compact-hover-v02).

`audit_hover_v02.py.txt` lê somente o RAW v02 e as referências NORMAL limpo/aprovado e HOVER v01. Executar este script nesta localização, com Python/Pillow/NumPy/SciPy nas versões de `analysis.json`. Não executar o script do lote anterior para reproduzir este adendo.

O script verifica/limpa apenas v02, salva seu CLEAN local/ignorado e cria estas composições diagnósticas. Não altera referências ou evidências antigas; nenhuma arte nova/estado foi gerado. A normalização das bboxes de referência ocorre apenas em memória para comparação; não é export de produção.

- `v02_raw_clean_mattes.png`: mesmo recorte/escala, RAW à esquerda/CLEAN à direita, sobre claro/escuro/magenta.
- `normal_v01_v02_native_sizes.png`: simulação raster de9-slice em120×44/150×44, sem texto/controle/shader. **NÃO É UNITY.** Cortes arredondados e resampling não substituem o import/renderer final.
- `analysis.json`: hashes das três entradas, dados RAW/CLEAN, transformações, intensidade e geometria, inclusive os desvios que permanecem.
- `classification.json`: CLEAN significa candidato pronto para QA direcionado, não APPROVED/INTEGRATED.
- `evidence_manifest.json`: bytes/SHA-256 das duas composições.

Após a execução, conferir hashes das referências e do RAW. O script não executa Unity/WebGL/builds, não repete auditorias dos outros assets e não modifica o v01 ou o NORMAL.
