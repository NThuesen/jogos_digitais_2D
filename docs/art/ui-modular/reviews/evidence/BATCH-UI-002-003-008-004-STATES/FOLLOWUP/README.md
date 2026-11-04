# Continuação P0–P3 — evidências novas

Complemento das seções9–11 da revisão existente, sem duplicar os resultados anteriores. Nenhum PNG de produção publicado; arquivos de arte em art_working permanecem ignorados. Fontes em harness/ são arquivos texto de um ambiente temporário, não código integrado.

Reprodução (a partir da raiz do repositório, Python com Pillow/NumPy/SciPy, Unity6000.5.9f1 Windows/WSL):

1. Executar prepare_disabled_v02.py.txt; lê RAW v02, cria CLEAN/exports locais e prova RAW×CLEAN. Requer os exports anteriores NORMAL/HOVER/PRESSED/v01 preservados. Não modifica seus masters.
2. Restaurar a árvore harness/ em Assets/_QA_Modular_Temp/, retirando somente o último sufixo .txt. Criar Resources/ModularQA e copiar os PNGs de art_working/ui_modular/export_unity/UI_MODULAR_CONSOLIDATED_QA/.
3. Executar refine_component_exports.py.txt: derivados dos três CLEANs existentes a50%, copiados também para Resources/ModularQA. Opcional measure_geometry.py.txt para guias. final_export_manifest.json registra os exports FINAIS; as entradas local_exports em disabled_v02_audit.json são o checkpoint anterior ao refinamento dos componentes.
4. Executar run_editor_qa.sh.txt; usa o guard existente e recusa outro Unity Editor antes de executá-lo. editor_results.xml e editor_log.txt são o resultado final, após correções técnicas de amostragem/posição do salário. HOVER/PRESSED não são exercitados no teste Editor desta continuação.
5. Para reproduzir o WebGL, restaurar webgl_template.html.txt como Assets/WebGLTemplates/ModularQa/index.html e executar build_webgl_qa.sh.txt UMA VEZ. O helper temporário cria cena própria e restaura PlayerSettings em finally. Executar browser_qa.py.txt depois da saída do Editor; servidor local efêmero, sem publicação. Mouse automatizado do Chromium, não físico. Não exigir rebuild para repetir a inspeção da mesma build.
6. analyze_followup.py.txt mede contraste pontual no Editor; não certifica toda a acessibilidade. Remover somente Assets/_QA_Modular_Temp e template/cena/build de QA criados para esta execução. Conferir e reverter apenas alterações geradas pelo build em arquivos rastreados; preservar trabalho externo. Nunca mexer em saves.

Capturas nomeadas editor_* e webgl_* são imagens reais, sem edição. Os guias e disabled_raw_clean são provas estáticas e não capturas Unity. SHA-256 de todas as imagens em evidence_manifest.json. Scripts de preparação usam o diretório atual como raiz; não executá-los a partir desta pasta.

validate_evidence.py.txt confere JSON/XML, hashes de fontes/exports/evidências, links e remoção dos temporários. compare_editor_webgl.py.txt registra a amostragem de bordas, excluindo o centro do texto. Limites e vereditos ficam na revisão. UI-0080,62× requer decisão visual mesmo quando fit técnico passa. Nenhum asset novo APPROVED/INTEGRATED e nenhum merge/auto-merge.
