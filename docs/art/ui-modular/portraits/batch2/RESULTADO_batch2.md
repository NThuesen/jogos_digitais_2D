# UI-021 — segundo lote (nove novas maids): CLEAN, derivados e QA

Estado: **os nove APPROVED pelo dono** (Nonoka na v02; a v01 fica preservada como SUPERSEDED). Não integrado em produção, nenhuma identidade liberada no `MaidCatalog`, sem WebGL: faltam as animações de caminhada.

Pipeline: o mesmo de `maid_portraits_prepare.py` (`clean()` e `head_box()` reutilizados sem alteração) via `tools/art/maid_portraits_batch2.py`. RAWs só lidos (ficam em `art_working/ui_modular/raw_gpt/`, ignorado pelo git; SHA-256 em `MANIFEST_batch2.json`). Derivados 192x157 e 480x641 em `derived/` (fora de `Assets`, nada vai para o build). Auditoria numérica em `audit_batch2.json`.

Evidências: `contact_sheet_18.png` (originais em azul × novas em dourado, por arquétipo), `contact_sheet_new9.png`, `heads_new9.png`, `qa/` (Escalação 0,62x e Elenco 0,82x, grupos g0/g1, e `detail_<id>_*` com o painel de detalhe prototipado; 960x600 e 1280x720, Editor).

## Inventário (RAW preservado, 1086x1448, RGBA)
| Maid | Arquétipo | Arquivo | SHA-256 (início) |
|---|---|---|---|
| Mikan | Genki | `UI-021_maid_portrait_mikan_neutral_v01.png` | `780668c6edd50ab8` |
| Yuki | Kuudere | `UI-021_maid_portrait_yuki_neutral_v01.png` | `5e9b1ac4c548fd04` |
| Kurumi | Tsundere | `UI-021_maid_portrait_kurumi_neutral_v01.png` | `8952f8ec5f57a078` |
| Nonoka | Dojikko | `UI-021_maid_portrait_nonoka_neutral_v01.png` | `28859f8f20fc4a9a` |
| Kaguya | Ojousama | `UI-021_maid_portrait_kaguya_neutral_v01.png` | `e3c19273af38e7ff` |
| Mizuki | Oneesan | `UI-021_maid_portrait_mizuki_neutral_v01.png` | `97b93a754a59b58d` |
| Himari | Idol | `UI-021_maid_portrait_himari_neutral_v01.png` | `dd34e1e495294e6a` |
| Anzu | Gourmand | `UI-021_maid_portrait_anzu_neutral_v01.png` | `3becc3bbc0df52a2` |
| Kasumi | Mukuchi | `UI-021_maid_portrait_kasumi_neutral_v01.png` | `bdf8edffdb7b5f11` |

## Auditoria de alpha, margens e cortes
Todos os RAWs: alpha real, base do busto cortada pela tela (esperado, sem margem embaixo), nenhum corte no topo (3–13 px de ar; Mizuki 3 px e Mikan/Anzu 4 px são os mais justos, sem achatamento visível), 14–77 fragmentos minúsculos (<120 px) removidos no CLEAN, nenhum fantasma cinza semitransparente, ~2–3 % de pixels semitransparentes (borda suavizada) e ~1k pixels invisíveis com cor (tratados pelo bleed). Toque nas bordas laterais: Kurumi e Anzu ≤1 px à esquerda (irrelevante); os demais com 5–88 px de folga.

## Veredito por maid (proposta técnica, sujeita à revisão do dono)
| Maid | Veredito | Observação |
|---|---|---|
| Mikan | PASS COM RESTRIÇÕES | mão acenando encostada na borda direita do card; pose é de aceno, não neutra |
| Yuki | PASS | |
| Kurumi | PASS | mecha esquerda toca a borda do card |
| Nonoka | PASS COM RESTRIÇÕES | **decisão artística:** orelhas caídas (leem como cachorro/lop, não gato) |
| Kaguya | PASS | |
| Mizuki | PASS | mão estendida aparece só no detalhe; orelha/rabo marrom coerentes |
| Himari | PASS COM RESTRIÇÕES | ver "cabelo laranja" |
| Anzu | PASS COM RESTRIÇÕES | ver "cabelo laranja" |
| Kasumi | PASS | |

Nenhum REWORK: rosto, olhos, penteado e silhueta de cada uma são distintos das 17 outras no contact sheet.

## Para decisão artística
1. **Três cabelos laranja/âmbar** (Mikan laranja vivo, Himari coral-pêssego, Anzu âmbar cacheado), mais pele morena nas três. Diferem por olhos (verde / turquesa / âmbar), penteado e acessório, e nos cards 192x157 ainda se distinguem, mas são o trio mais próximo do elenco de 18. Se quiser mais separação, recolorir Himari (ex.: rosa/magenta) seria o ajuste de menor impacto — não feito, sem IA nesta etapa.
2. **Orelhas caídas da Nonoka**: confirmar se fica (diferencial forte) ou se pede v02 com orelha de gato.
3. **Pose da Mikan** (aceno) — confirmar se serve de âncora para o sprite de caminhada neutro.

## QA no Editor
Harness existente (`MaidPortraitsQaTests`) copiado em `MaidPortraitsBatch2QaTests`: maids procedurais comuns com o nome e o arquétipo projetados, sem identidade, só com o sprite do `Portrait` trocado. 2/2 passam; Escalação, Elenco e detalhe legíveis em 960x600 e 1280x720, sem cortes novos. Não rodei outros testes (nenhum código de produção mudou).

## Atualização: Nonoka v02 (aprovada artisticamente pelo dono)
Correção só das orelhas: de caídas (cachorro) para orelhas de gato Scottish Fold compactas; resto do desenho preservado. RAW v02 (1086x1448 RGBA, `UI-021_maid_portrait_nonoka_neutral_v02.png`, SHA-256 em `MANIFEST_batch2.json`, chave `nonoka_v02`) processado pelo mesmo pipeline (`python tools/art/maid_portraits_batch2.py nonoka:2`): CLEAN 985x1458, 54 fragmentos removidos, sem fantasmas; topo com 5 px de ar (igual à v01) e orelhas inteiras dentro da tela. A v01 (RAW, CLEAN, derivados, hash) continua intacta. Comparativos: `nonoka_v01_vs_v02_top.png`, `nonoka_v01_vs_v02_heads.png`.

QA incremental no Editor (só a Nonoka, grupo 2 do harness, 960x600 e 1280x720): 1/1 passou; capturas `qa/g2_*` e `qa/detail_nonoka_v02_*`. Os outros oito não foram reexecutados.

| Maid | Status | Restrições |
|---|---|---|
| Mikan | APPROVED | mão acenando encostada na borda direita do card |
| Yuki | APPROVED | |
| Kurumi | APPROVED | |
| Nonoka | APPROVED (v02) | nenhuma; v01 SUPERSEDED |
| Kaguya | APPROVED | |
| Mizuki | APPROVED | |
| Himari | APPROVED | cabelo coral próximo de Mikan/Anzu |
| Anzu | APPROVED | cabelo âmbar próximo de Mikan/Himari |
| Kasumi | APPROVED | |

Continua não integrado: nenhuma identidade liberada no `MaidCatalog`, sem animações de caminhada.
