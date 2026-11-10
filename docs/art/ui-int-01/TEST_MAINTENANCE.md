# UI-INT-01B — manutenção autorizada dos testes protegidos (PR #79)

Autorização do responsável: atualização limitada dos testes do Layout_Check que ficaram obsoletos com o novo Hub. **Não** é desativação nem contorno do `protected-change-guard`; a exceção de merge segue o procedimento do repositório. Nenhum workflow, guard, runner ou proteção de branch foi tocado.

## Arquivos protegidos alterados

| Arquivo | Natureza |
|---|---|
| `Assets/_Game/Tests/PlayMode/Layout/LayoutCheckTests.cs` | 3 testes atualizados (seletores/contratos), helpers trocados |
| `Assets/_Game/Tests/EditMode/Game/DocsTests.cs` | allowlist fechada com os 35 PNGs do kit (hashes em `docs/art/SUZU_ASSET_PROVENANCE.md`) — já na #79 |

Arquivos de teste **novos** (também sinalizados pelo guard): `Tests/PlayMode/UiIntegrationTests.cs`, `Tests/PlayMode/Layout/KitLegibility.cs`.
Sem alteração em `LayoutModel.cs`, `LayoutRules.cs`, `Clicks.cs`, `ResolutionRig.cs`, `SessionFactory.cs`, nem em qualquer regra (R2.1 etc.).

## O que mudou em cada teste (e por quê)

**`ClicksFindTheBaselineTargets`** — achava planks pelo rótulo "Voltar"/"Loja". Agora acha os controles pelo nome do objeto (`Back`, `Nav_Lineup`, `Nav_Roster`, `Nav_Boxes`, `Nav_Shop`, `Nav_Hall`, `Cta_OpenDay`).
- Preservado: 960×600 e 1280×720; `Clicks` por `EventSystem.RaycastAll` + pointer-click; Hit_Area com alvos de raycast; o clique no Lojinha por último troca para `ShopScreen`.
- Reforçado: **os 7 controles** (antes 2) são clicados no centro nos dois tamanhos; o clique tem de chegar ao próprio controle e disparar só o callback dele (callbacks do Hub trocados por gravadores e restaurados).
- Removido: o diagnóstico D-EM-5 (placa cobrindo "Voltar"): a placa não existe mais; "Voltar" ter Hit_Area e receber o clique cobre o objetivo.

**`ModelDescribesEmpresa`** — esperava 7 `Plank`, wordmark `TrackedLabel`, `HubScreen/Plaque` (3 placas) e o rótulo no "rosto" do plank. Agora:
- 7 Interactive_Controls `Selectable` com os nomes acima, cada um lendo seu rótulo (`Label`, filho) e sendo um elemento do modelo;
- wordmark = uma `Image` aspect-preserving; painel de economia (`EconomyPanel`) = um elemento `Image` com 8 rótulos, desenhado dentro do Viewport;
- **novo:** `KitLegibility` — todo rótulo da tela fica dentro do Viewport, dentro dos limites *desenhados* (alfa do sprite) da placa onde está, e nenhuma Image desenhada depois o cobre; tolerância `1 + VisibleShape.SampleStep` (erro documentado do modelo);
- preservado: Container da linha de química = Viewport; `Dump` (NEKO_LAYOUT_DUMP); cache de alfa × `SampleAlpha`, agora sobre o **sprite real do kit** (`economy_panel_empty`, legível na CPU); 960×600 e 1280×720.

**`ProbesAgreeWithTheRenderedFrame`** — escondia o "rosto do plank Elenco" e o anel do monograma. Agora esconde, renderiza e compara pixels com a malha prevista em **elementos reais do kit**: o backdrop (Simple), o rosto do botão Maids (sliced, `nav_pink_surface`), o painel de economia e o CTA (aspect-preserving); mantém as duas imagens locais (letterbox e sliced com borda clara), a linha de química (texto), a contagem de glifos, o raycast e o `AssertMatchesModel`, nos dois tamanhos.

## Mudanças de produção que isso exigiu
- Botão Voltar nomeado `Back` (Hub e Caixinhas); sombra removida da linha de química (o teste exige que ela não tenha mesh effect); texturas do kit com `isReadable` (o modelo precisa do alfa real para prever pixels). Nenhuma mudança visual relevante.

## Evidência
- Os 3 testes, executados **um a um**: passam (`ClicksFindTheBaselineTargets`, `ModelDescribesEmpresa`, `ProbesAgreeWithTheRenderedFrame`).
- EditMode 379/379; PlayMode 117/117 (108 anteriores, incluindo os 3 atualizados, + 9 de UI); WebGL: build OK e Hub → Lojinha → Loja no Chrome sem erros de console.
- `UiIntegrationTests` (nove): dados reais, 1–5 maids, aviso longo, **valores enormes** (cash ≈ 1,07 bi), CTA desabilitado (tint + não dispara pelo EventSystem), 7 botões por clique real, Caixinhas com fichas insuficientes (clique real não compra), compra e revelação por clique real, Voltar das Caixinhas — sempre com a legibilidade medida (`KitLegibility`), em 960×600 e 1280×720.
- Frames finais: `evidence/*.jpg` (inclui `hub_huge_*`, `hub_cta_disabled_*`).

## Integração com a #78
`DocsTests.cs` também é alterado pela #78 (10 PNGs de móveis). Ao atualizar a #79 sobre a `main` com a #78, manter **as duas** listas (10 + 35 entradas) e as linhas de hash de ambos os blocos de `SUZU_ASSET_PROVENANCE.md`. Não atualizei a base nem fiz merge.
