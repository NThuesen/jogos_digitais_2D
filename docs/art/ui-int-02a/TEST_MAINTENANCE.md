# UI-INT-02A — arquivos protegidos tocados

Nenhum workflow, guard (`protected-change-guard`), runner ou proteção de branch foi alterado; nenhum teste foi enfraquecido ou removido; `LayoutCheckTests`/`LayoutRules` intactos.

| Arquivo | Natureza |
|---|---|
| `Assets/_Game/Tests/EditMode/Game/DocsTests.cs` | +27 entradas na allowlist fechada de proveniência (só os PNGs `maids_*` importados; hashes em `docs/art/SUZU_ASSET_PROVENANCE.md`) |
| `Assets/_Game/Tests/EditMode/Game/RosterQueryTests.cs` | arquivo novo |
| `Assets/_Game/Tests/PlayMode/RosterScreenTests.cs` | arquivo novo |

O guard sinalizará esses arquivos: a exceção de merge, se necessária, deve seguir o procedimento legítimo do repositório (decisão do dono). Ao combinar com #78/#79, manter todas as listas de `DocsTests.cs` e os blocos de hash de `SUZU_ASSET_PROVENANCE.md`.
