# ART-02 — segurança, procedência e validação

## Incidente

Na primeira implementação, a suíte PlayMode foi executada duas vezes antes de se perceber que seus testes chamam `OnNewGame`, apagando e recriando o save do Unity Editor. Um progresso anterior pode ter sido sobrescrito. Não existe cópia anterior conhecida e não se alega recuperação. O save WebGL usa armazenamento separado.

## Isolamento aprovado para estas execuções

`SaveBridge` usa somente memória no Unity Editor batch ou com `NEKO_ISOLATED_SAVE=1`. Nesse caminho `Load`, `Save`, `HasSave` e `Wipe` não acessam PlayerPrefs. Nenhuma restauração é necessária após falha ou interrupção. O player compilado mantém a persistência existente. Testes manuais no Editor habitual, sem a variável, continuam proibidos: usar batch ou iniciar um processo explicitamente isolado. Não alternar a variável dentro de uma sessão.

## Proposta de manutenção excepcional (não aplicada aos testes)

Dois testes protegidos em `DocsTests.cs` descrevem a situação antiga:

- `TheReadmeStatesThatNoAssetWasImported`: substituir a busca literal por “Não há nenhum” por exigência da seção “Créditos de assets”, referência ao inventário e registro de origem de cada entrada autorizada. O README foi corrigido para refletir as importações reais; espera-se falha desse teste antigo.
- `NoTextureOrAudioFileWasEverImported`: manter a busca de todas as extensões atuais e a exceção de áudio sintetizado existente, normalizar separadores, e exigir igualdade exata com os quatro caminhos abaixo. Verificar SHA-256 e igualdade byte a byte com os recortes fonte. Proibir caminhos duplicados, ausentes, extras e escapes do repositório. Não liberar um diretório inteiro nem qualquer PNG.

Fluxo: PR excepcional separado, autorizado pelo proprietário, com alteração mínima desses dois testes, evidência independente e casos negativos (arquivo extra, hash diferente, crédito ausente). Não enfraquecer ou contornar `protected-change-guard`; sem auto-merge. Esta proposta não autoriza alterar testes no PR #15.

| Destino | Fonte em `docs/art/art-01/D-pixel-cafe/processed/` | SHA-256 |
| --- | --- | --- |
| `Assets/_Game/Resources/Suzu/portrait.png` | `D1_portrait_cutout.png` | `8f4a549017876ba3c10e03b23582186778729e134710498f09af402dfb7a5883` |
| `Assets/_Game/Resources/Suzu/idle.png` | `D2_chibi_idle_cutout.png` | `a336511b1ee936d637b72ab2de4fb66570d7530c445085cde625a61378e43f16` |
| `Assets/_Game/Resources/Suzu/walk.png` | `D2_chibi_walk_cutout.png` | `f3f7a16b20bfdff239dbee095b5ca0e03fbd4e584b4beb6cba7270846efac88e` |
| `Assets/_Game/Resources/Suzu/carry.png` | `D2_chibi_carry_cutout.png` | `c9dbf509a2545058b145c24e575bdc422e8d5030c7f9fd890630719e3fd94249` |

## Evidência obtida no Unity 6000.5.9f1

Capturas reais de câmera em RenderTexture, com Canvas nativo e aspecto atualizado, não mockups. O fixture usa uma maid de salão do perfil isolado, renomeada Suzu apenas na memória de teste; as outras maids mantêm o fallback. Não reproduz um save do usuário.

- [Card 1280×720](art-02/captures/card_1280x720.png), [960×600](art-02/captures/card_960x600.png).
- [Card com acessório 1280×720](art-02/captures/card_accessory_1280x720.png), [960×600](art-02/captures/card_accessory_960x600.png).
- [Restaurante idle 1280×720](art-02/captures/restaurant_idle_accessory_1280x720.png), [960×600](art-02/captures/restaurant_idle_accessory_960x600.png).
- [Restaurante em movimento 1280×720](art-02/captures/restaurant_movement_1280x720.png), [960×600](art-02/captures/restaurant_movement_960x600.png).

Inspeção: portrait reconhecível, mas pequeno e com perda de detalhe; textos/posição/stats continuam nativos. Acessório é sobreposto sem regenerar imagem, e altera stats pelas regras existentes. Chibi idle e walk usam sprites distintos e deslocamento real, sem ciclo quadro a quadro. Altura nominal de 96 px em 720 px; em 600 px a altura esperada é 80 px. Suzu fica menor e mais detalhada que o fallback. O fundo antigo continua procedural; não é o café D3. Há proximidade/sobreposição com mobília e outras personagens; não se considera aprovada a integração de clipping/oclusão. Header e rodapé têm leitura limitada no layout antigo; sem redesign nesta tarefa.

O log de captura registrou `Suzu_idle` em (3.04, 1.10) e `Suzu_walk` em (3.60, 1.25) e (4.22, 1.42), além de sprites procedurais das demais maids. Sem exceções da aplicação ou erros de compilação nessa execução; há avisos de API obsoleta dos testes e mensagens do subsistema de licença/driver do Editor. Isso não equivale a Console universalmente limpo.

PlayMode isolado: 20 passaram. Impressão criptográfica local do valor de save habitual comparada antes/depois do PlayMode e da captura: idêntica. Isso confirma preservação nas novas execuções, sem alegar recuperação do incidente anterior. Logs/XML permanecem em `Logs/` (locais, não versionados).

Reprodução: iniciar Unity por operação serializada com o guard do repositório, em batch, sem `-quit`, com `NEKO_SUZU_CAPTURE_DIR` apontando para saída local e `-executeMethod Neko.Game.Core.SuzuCapture.Begin`. O hook de captura só roda no Editor batch explicitamente habilitado. Não usar o capturador antigo no ambiente habitual.

## Revisão objetiva Claude Code

HEAD revisado: `4f90b96` (diff estático fornecido explicitamente). Findings válidos corrigidos: watchdog e saída com código 1 para erros/exceções da captura; reset do aspecto automático; confirmação de idle/walk imediatamente antes de cada render; escala atualizada em Sync; SortingGroup isolando o acessório de Suzu; descarte seguro de sprites também fora de PlayMode. Revalidação: captura regenerada com sucesso após correções; testes PlayMode finais informados no PR.

Observações condicionais conferidas no repositório: imports de namespace existentes e compilação válida; Canvas ScreenSpaceCamera; assets e GUIDs presentes. O revisor não fez inspeção visual nem executou Unity. Nenhuma correção em CI ou testes protegidos. A manutenção dos dois testes de documentação permanece pendente.
