# Baseline visual — prints de 27/10/2026

**Read when:** UI-01 ou seleção de telas para UI-02. Observações sobre capturas de uma sessão real. As imagens foram compartilhadas na conversa, **não estão versionadas aqui**. Reproduzir os cenários no Editor antes de alterar código; distinguir observação, hipótese e confirmação.

| Tela | Observação visível | Classificação / próxima verificação |
| --- | --- | --- |
| Menu | Identidade do café reconhecível; grandes superfícies marrons e maids pequenas | Oportunidade estética, não bug comprovado |
| Empresa | Cards pequenos e pouco destaque das personagens frente ao painel econômico | Hierarquia a validar no runtime |
| Escalação | Cards/textos pequenos; posto vazio com maid no banco | **Posto vazio é legítimo, não bug**; analisar só legibilidade/hierarquia |
| Elenco | Informações apertadas, card pequeno e detalhe competindo pela área | Verificar cortes/colisões e layout |
| Caixas | Três tipos diferenciados, pouca dramaticidade na recompensa | Polimento visual posterior |
| Loja | Rótulos/descritivos pouco legíveis e sobreposição aparente com painéis | **Prioridade UI-01**: reproduzir Móveis/Roupas/Presentes |
| Salão | Métricas e cabeçalho apertados; interface com aparência instrumental | Verificar clipping; polimento posterior |
| Turno | Core loop observável; chibis e cenário com baixa expressividade visual | Prioridade visual posterior, sem alterar simulação |
| Balanço | Linhas, contadores, texto inferior e outros elementos visualmente próximos/sobrepostos | **Prioridade UI-01**; verificar também números/cálculo só se houver evidência |

## Locais de código confirmados
- `Assets/_Game/Scripts/Game/Ui/ShopScreen.cs` (Loja), `RosterScreen.cs` (Elenco), `BooksScreen.cs` (Balanço), `LineupScreen.cs` (Escalação), `HallScreen.cs` (Salão), `HubScreen.cs` (Empresa), `BoxScreen.cs` (Caixas), `DayHud.cs` (turno).
- `Assets/_Game/Scripts/Game/Ui/UiKit.cs` e `Scenery.cs`: elementos visuais compartilhados.
- `Assets/_Game/Scripts/Game/Art/Palette.cs`: cores atuais. Algumas posições e dimensões no código são fixas; **inspecionar canvas/scaling no Unity** antes de inferir causa.

## Não tratar como defeito sem evidência
- **Escalação parcialmente vazia:** escolha válida do jogador, conforme confirmação do responsável em 27/10/2026.
- **Prejuízo no primeiro dia:** uma única captura não prova dificuldade indevida ou bug econômico.
- **Paleta marrom:** avaliação estética; candidato a experimento ART-01, não correção automática de `Palette.cs`.

**Evidência esperada:** para cada defeito reproduzido, indicar estado/tela e resolução, captura *antes*, alteração mínima, captura *depois*, confirmação de legibilidade, console e testes relevantes. Se não reproduzir, registrar como não confirmado.
