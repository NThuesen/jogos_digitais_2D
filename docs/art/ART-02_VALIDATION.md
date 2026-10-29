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
