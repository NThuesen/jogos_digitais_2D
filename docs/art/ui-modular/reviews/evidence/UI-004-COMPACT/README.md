# Evidências UI-004 COMPACT NORMAL

Capturas são renders reais, não assets aprovados. RAW/limpo/exports ficam em `art_working/`, ignorados. `analysis.json` e `geometry.json` documentam o processamento e os parâmetros. O veredito e os limites estão na [revisão](../../UI-004_BUTTON_PRIMARY_COMPACT_NORMAL_V01.md).

## Reprodução local isolada

1. Com o RAW no caminho documentado, executar `python clean_compact.py.txt` com Pillow, NumPy e SciPy, mantendo este script neste diretório. Executar `geometry.py.txt` para repetir as medidas. Isso regrava somente os derivados locais e a evidência de análise, nunca o RAW.
2. Reconstruir a árvore abaixo a partir dos arquivos `.txt` deste diretório, removendo apenas o sufixo `.txt`. Copiar os quatro `candidate_{2072,1024,768,512}.png` do export local para `Runtime/Resources/UI004CompactQa/`.

```text
Assets/_QA_UI004Compact_Temp/
  Runtime/
    CompactQa.Runtime.asmdef
    CompactQaView.cs
    CompactQaBootstrap.cs
    CompactQaBridge.jslib
    Resources/UI004CompactQa/candidate_*.png
  Tests/
    CompactQa.Tests.asmdef
    CompactQaTests.cs
  Editor/
    CompactQa.Editor.asmdef
    CompactQaSetup.cs
    CompactQaBuild.cs
```

3. Usar `run_editor_qa.sh.txt` (ajustar caminhos da máquina). Ele reutiliza `tools/unity-guard.sh`, verifica também processos Windows, configura somente importers temporários e roda apenas `CompactQaTests.Matrices`. Nenhum teste existente precisa ser editado.
4. Para WebGL, copiar `webgl_template.html.txt` para `Assets/WebGLTemplates/UI004CompactQa/index.html` e executar `build_webgl_qa.sh.txt`. O builder usa cena isolada, saída `Build/UI004CompactQA`, temporariamente desativa compressão de download e restaura as opções em `finally`. O alvo WebGL pode acrescentar automaticamente uma entrada de batching ao ProjectSettings: conferir e desfazer somente alterações produzidas pelo próprio QA após o Editor fechar, inclusive o Burst WASM rastreado em `Data/Plugins/lib_burst_generated.wasm` se regenerado; nunca incluir configurações no PR.
5. Executar `browser_qa.py.txt` com Playwright e Chromium. Ajustar o caminho do executável se necessário. Ele serve **somente localhost**, captura os dois viewports e envia eventos de mouse automatizados ao canvas; não é mouse físico. O módulo de input Unity recebe os eventos reais do navegador.
6. Encerrar os processos e remover somente o ambiente temporário criado, suas metas, template e `Build/UI004CompactQA`. Conferir `git diff`: somente documentação/evidências podem permanecer. Preservar RAW e limpo locais. Não alterar cenas ou componentes do jogo.

As opções de import são para QA sem compressão; não certificam export/import final, foco/teclado ou memória GPU residente. Não executar esses comandos simultaneamente a outro Editor.
