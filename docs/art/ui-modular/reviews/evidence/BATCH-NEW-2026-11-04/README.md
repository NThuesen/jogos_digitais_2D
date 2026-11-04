# Evidências do novo lote modular — 04/11/2026

[Relatório consolidado](../../UI_BATCH_NEW_2026_11_04.md). `inventory.json` cobre todos os RAWs; `audit.json` contém hashes completos RAW/CLEAN/export e operações conservadoras; `classification.json` registra resultados e limites sem aprovação artística. RAW/CLEAN/export continuam locais/ignorados, com PNGs de prova selecionados versionados aqui.

`editor_results.xml` e `editor_results.json` são resultados reais do único teste direcionado válido; capturas Editor são renders reais. Clique/raycast Editor é simulado via ExecuteEvents. Capturas WebGL, quando presentes, vêm de navegador real automatizado/SwiftShader, não mouse/GPU físicos. Scripts e harness reproduzíveis estão arquivados como `.txt`; não são código integrado.

`evidence_manifest.json` registra bytes e SHA-256 de todas as evidências, exceto o próprio manifesto. Reproduzir em cache/projeto isolados; ajustar caminhos de sessão e usar fontes com os hashes registrados. Não sobrescrever RAW, CLEAN existente ou este registro de evidência. QA PASS não significa APPROVED ou INTEGRATED; sem merge/auto-merge sem revisão do responsável.
