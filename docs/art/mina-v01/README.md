# Mina v01 — experimento de três orientações

Estado: experimental, não aprovado; sem auto-merge. Base: PR #15 (`747e677`), ainda aberto, não incorporado à main. PR #16 é documental e independente; nenhum prompt alterado.

## Escopo

Somente Mina com save isolado usa Front/Side/Back. Side é espelhado, com histerese entre eixos (0,8/1,25), retenção temporal de 150 ms e retenção de movimento de 120 ms. Personagens sem arte direcional mantêm o caminho anterior. Associação provisória pelo nome, sem persistência ou alteração da simulação.

Cook/Tired usam idle da mesma identidade; não são animações específicas desses estados. Regras e indicador de humor continuam existentes. Carry frontal contém comida e bandeja: o renderer de item externo é suprimido enquanto essa pose aparece. Carry lateral/costas não existe, portanto continua frontal, uma limitação a avaliar.

## Arte

Originais fornecidos em Downloads; hashes/crops em `processing.json`. `process.py` usa Python, Pillow e NumPy, autorizado pelo responsável, sem regeneração. Remoção suave do magenta, unmatting contra magenta e canvas 448×400 com altura de conteúdo 384 e base comum. Nenhuma erosão de máscara. Escala nominal de 96 px por canvas em 720 px; a igualdade de altura não prova igualdade anatômica. Portrait/sheet são referências e não precisam ser importados neste experimento de movimento.

## Evidência

Captura final concluída com código de saída 0 no Unity 6000.5.9f1. Asserts de direção e flip passaram. Capturas `natural_*` observam a simulação; `probe_*` são estímulos controlados de apresentação em memória e não comprovam navegação. RenderTexture captura a câmera real do Unity com UI, não a janela do Editor.

## Findings confirmados no código

- PR #15 retorna ao procedural em Cook/Tired.
- MaidView anterior seleciona somente flip horizontal.
- Carry original contém bandeja e comida; ShowCarried existe, mas não foram encontrados chamadores no código atual. Duplicação não foi observada no runtime anterior.
- Cinco poses estáticas não constituem ciclo de caminhada. Três orientações podem melhorar leitura espacial sem resolver deslizamento.

Aceite estético e expansão para as outras identidades permanecem pendentes.

## Reprodução

1. Processar: `python process.py <originais> <Assets/_Game/Resources/Mina>` (Pillow 13, NumPy 2.5); guardar `processing.json` neste diretório documental.
2. Verificar ausência de outros Editors com `tools/unity-editor-processes.py` no sistema do Editor e adquirir `tools/unity-guard.sh` antes de executar. Nesta sessão o guard foi carregado por uma cópia em memória sem CRLF, sem alterar o runner versionado.
3. Definir `NEKO_ISOLATED_SAVE=1` e `NEKO_MINA_CAPTURE_DIR=<diretório absoluto de capturas>` no processo Windows.
4. Unity **6000.5.9f1**: `-batchmode -projectPath <projeto> -executeMethod Neko.Game.Core.MinaCapture.Begin -logFile <log>`. Não passar `-quit`: o harness encerra ao concluir ou falhar. Nenhum PlayerPrefs é lido/escrito pelo SaveBridge nesse modo.
5. Para testes, remover a variável de captura, manter isolamento e usar o runner guardado EditMode/PlayMode. Nesta sessão os mesmos argumentos de teste foram executados via PowerShell/Start-Process com guard Linux e verificação adicional Get-Process Unity no Windows; resultados verificados por `tools/validate-unity-results.py`.

Ajuste durante inspeção: o pivô inferior exigiu deslocamento de meio canvas para conservar a altura do piso anterior. As capturas iniciais com a personagem elevada foram substituídas. Os probes suspendem o GameRoot e usam os mesmos MaidView/Worker em memória; não alteram regras da simulação nem saves persistidos.

## Resultado e limites

**Comprovado:** seleção Front/Side/Back e flip nos dois lados; retenção na diagonal e parada nos probes; mesma identidade nos fallbacks; barra de cansaço visível; Mina/Suzu no mesmo restaurante. PNGs têm canvas comum e base comum. Sem exceções da aplicação ou erros C# na captura final. Há aviso nativo de licença (token indisponível) e avisos de APIs obsoletas; não impediram execução.

**Limitações aceitáveis apenas para este experimento:** ligação por nome e ambiente isolado; carry exclusivamente frontal com comida ilustrada fixa, que não representa o prato real; Cook/Tired com idle; equipamentos da Mina não são representados. Suzu e demais maids preservam seu caminho anterior.

**Não comprovado / pendente:** caminhar convincentemente até clientes. Três vistas tornam a direção legível, mas pose congelada + bob ainda sugerem deslizamento. Parada lateral/costas conserva uma pose de passo. A igualdade de canvas evita saltos de escala programáticos; diferenças anatômicas e centro de massa entre desenhos continuam perceptíveis e sem aceite estético. Sobreposições com clientes/mobília persistem. GIFs são sequências de screenshots espaçadas, com duração ilustrativa, não gravações em tempo real. Não extrapolar para as outras 17 identidades.

**Conclusão:** arquitetura mínima reutilizável demonstrada, critério de caminhada convincente ainda não satisfeito. Não atualizar prompts nem aprovar integração automaticamente.

### Capturas reais

| Caso | 1280×720 | 960×600 |
|---|---|---|
| Direita | [captura](captures/probe_right_1280x720.png) | [captura](captures/probe_right_960x600.png) |
| Esquerda | [captura](captures/probe_left_1280x720.png) | [captura](captures/probe_left_960x600.png) |
| Fundo | [captura](captures/probe_back_1280x720.png) | [captura](captures/probe_back_960x600.png) |
| Câmera | [captura](captures/probe_front_1280x720.png) | [captura](captures/probe_front_960x600.png) |
| Diagonal | [captura](captures/probe_diagonal_1280x720.png) | [captura](captures/probe_diagonal_960x600.png) |
| Diagonal próxima | [captura](captures/probe_diagonal_near_1280x720.png) | [captura](captures/probe_diagonal_near_960x600.png) |
| Parada | [captura](captures/probe_stop_1280x720.png) | [captura](captures/probe_stop_960x600.png) |
| Carry | [captura](captures/probe_carry_1280x720.png) | [captura](captures/probe_carry_960x600.png) |
| Cook | [captura](captures/probe_cook_1280x720.png) | [captura](captures/probe_cook_960x600.png) |
| Tired | [captura](captures/probe_tired_1280x720.png) | [captura](captures/probe_tired_960x600.png) |

Sequências naturais: [1280×720](captures/natural_sequence_1280x720.gif), [960×600](captures/natural_sequence_960x600.gif).

Validação: EditMode **falhou**, 298/300; `NoTextureOrAudioFileWasEverImported` e `TheReadmeStatesThatNoAssetWasImported` (mesmos contratos documentais incompatíveis já presentes no PR #15; novos PNGs também aparecem na primeira falha). Testes protegidos intactos. Neko.Sim standalone e WebGL **não executados**.

PlayMode final: **passou, 20/20**, sem falhas/skips/inconclusivos. `git diff --check`: **passou**.
