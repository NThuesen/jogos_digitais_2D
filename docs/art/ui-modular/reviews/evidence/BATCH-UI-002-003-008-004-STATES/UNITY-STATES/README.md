# QA Unity direcionado — estados COMPACT

Evidências novas da seção8 da revisão existente. Fontes RAW/CLEAN/export permanecem locais e ignoradas; nenhum asset de produção publicado. NORMAL copiado integralmente do export aprovado.

Reprodução (Unity6000.5.9f1 Windows/WSL): executar prepare_states.py.txt com Python/Pillow; criar temporariamente Assets/_QA_UI004States_Temp/Editor, Tests e Textures, restaurando os arquivos .cs/.asmdef arquivados sem o sufixo .txt nos diretórios correspondentes; copiar exports normal/hover/pressed/disabled.png para a subpasta Textures. Consultar os caminhos concretos nos fontes e executar run_states_qa.sh.txt com o guard existente. run_states_test_only.sh.txt registra a execução final após configuração. Remover pasta temporária e .meta ao terminar. Não executar com outro Editor usando o projeto.

unity_states_results.xml/log.txt registram o teste final selecionado; quatro matrizes são renders reais. frame_analysis.json contém amostras e comparação de recortes do mesmo botão. summarize_frames.py.txt compõe contato e WebPs a partir dos frames locais capturados pelo harness; nearest2× e tempos escolhidos para inspeção, não latência. export_manifest.json registra fontes, hashes e transformações. evidence_manifest.json registra hashes das sete imagens publicadas.

Interação simulada ExecuteEvents/GraphicRaycaster; não mouse físico nem módulo de input completo. Sem WebGL/build, integração ou aprovação automática. Veredictos individuais e limites em verdict.json e seção8 do relatório.
