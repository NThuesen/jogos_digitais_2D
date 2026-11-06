# UI-021 — lote final (nove últimas maids): CLEAN, derivados e QA

Estado: **NÃO REVISADO pelo dono** (nenhum APPROVED). Não integrado em produção, nenhuma identidade liberada no `MaidCatalog`, sem WebGL: faltam as animações de caminhada.

Pipeline: o mesmo de `maid_portraits_prepare.py` / PR #61 (`clean()` e `head_box()` sem alteração) via `tools/art/maid_portraits_batch3.py`. RAWs só lidos (git-ignorados; SHA-256 em `MANIFEST_batch3.json`). Derivados 192x157 e 480x641 em `derived/` (fora de `Assets`). Auditoria numérica em `audit_batch3.json`. Evidências: `contact_sheet_27.png` (por arquétipo: original | lote 2 | lote final), `contact_sheet_new9.png`, `heads_new9.png`, `qa/` (Escalação 0,62x, Elenco 0,82x e detalhe prototipado; 960x600 e 1280x720, Editor, 2/2 testes passaram). Para o contact sheet de 27 o script lê os CLEANs do lote 2 (PR #61, ainda aberta) da pasta do worktree dela; os arquivos não são copiados para esta PR.

## Inventário (RAWs preservados, 1086x1448 RGBA)
| Maid | Arquétipo | Arquivo | SHA-256 (início) |
|---|---|---|---|
| Hibari | Genki | `UI-021_maid_portrait_hibari_neutral_v01.png` | `febb03d0fb958a45` |
| Seira | Kuudere | `UI-021_maid_portrait_seira_neutral_v01.png` | `2d79c8df2337179f` |
| Ruri | Tsundere | `UI-021_maid_portrait_ruri_neutral_v01.png` | `a4822a69f5346bc5` |
| Chiyo | Dojikko | `UI-021_maid_portrait_chiyo_neutral_v01.png` | `27e5dcef904b59cc` |
| Shiori | Ojousama | `UI-021_maid_portrait_shiori_neutral_v01.png` | `0ee759dd996f7520` |
| Tsubaki | Oneesan | `UI-021_maid_portrait_tsubaki_neutral_v01.png` | `fa30c0cee226ea76` |
| Mirai | Idol | `UI-021_maid_portrait_mirai_neutral_v01.png` | `7b53e4db9db172c1` |
| Kinako | Gourmand | `UI-021_maid_portrait_kinako_neutral_v01.png` | `821a3669a2af89b2` |
| Mayu | Mukuchi | `UI-021_maid_portrait_mayu_neutral_v01.png` | `6bfc73422fb562d6` |

## Auditoria de alpha, margens e cortes
Todos com alpha real; base do busto cortada pela tela (esperado); topo com 5–20 px de ar (Seira e Chiyo 5 px, os mais justos, sem achatamento); 16–44 fragmentos minúsculos removidos; nenhum fantasma cinza; ~2–3 % de pixels semitransparentes (borda). Único toque lateral: **Mirai**, mão levantada encostada na borda direita do RAW (~1 px), cortada também no card.

## Veredito (proposta técnica; aprovação artística é do dono)
| Maid | Veredito | Observação |
|---|---|---|
| Hibari | PASS | punho e curativo legíveis; cabelo azul-turquesa único |
| Seira | PASS | óculos e cabelo grafite legíveis |
| Ruri | PASS | orelhas e rabo de onça distintos |
| Chiyo | PASS COM RESTRIÇÕES | cabelo pêssego-claro + pele morena + orelha clara próximos de Kinako (bege) no card pequeno |
| Shiori | PASS COM RESTRIÇÕES | **maior semelhança do conjunto:** cabelo ondulado cobre-avermelhado, pele morena e olhos verdes lembram Mizuki (e o tom laranja/vermelho lembra Himari/Mikan) |
| Tsubaki | PASS COM RESTRIÇÕES | cabelo cinza-escuro sobre fundo cinza do card: contraste baixo; faz parte do grupo de cabelos escuros (Hina, Kaguya, Mayu, Yoru) |
| Mirai | PASS COM RESTRIÇÕES | mão encostada/cortada na borda direita; cabelo lilás próximo de Nonoka (diferem por trança, pele e olhos) |
| Kinako | PASS | |
| Mayu | PASS | azul-marinho próximo de Hina, mas coque, rabo branco e olhos distintos |

Nenhum REWORK obrigatório: nenhuma das nove repete rosto, penteado e silhueta de outra das 18 anteriores.

## Para decisão artística
1. **Shiori × Mizuki** (e família laranja Himari/Mikan/Anzu): se quiser mais separação, recolorir Shiori (ex.: ruivo mais escuro/borgonha com mechas, ou outro matiz) é o único caso que eu consideraria para v02; aceitar também é defensável porque penteado e acessórios diferem.
2. **Chiyo × Kinako**: pares claros e quentes; aceitar ou ajustar o tom do cabelo de uma delas.
3. **Tsubaki**: aceitar o contraste baixo no card ou esperar a moldura/fundo por arquétipo.
4. **Mirai**: aceitar o corte de ~1 px da mão ou pedir v02 com a mão recuada.

## QA no Editor
Harness existente copiado em `MaidPortraitsBatch3QaTests` (maids procedurais comuns com nome e arquétipo projetados, sem identidade; só o sprite do `Portrait` é trocado). Dois grupos, 960x600 e 1280x720: enquadramento, legibilidade e compatibilidade com as molduras OK, sem novos cortes além dos listados.
