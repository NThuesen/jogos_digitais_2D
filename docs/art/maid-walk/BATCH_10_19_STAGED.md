# Caminhadas das maids 10–27: processadas e INTEGRADAS

Este arquivo começou como o registro das dez primeiras caminhadas "staged" (PR #66) e agora cobre as 14 maids processadas e liberadas no jogo: Mikan, Yuki, Kurumi, Nonoka, Kaguya (refeita), Mizuki, Himari, Anzu, Kasumi, Hibari, Seira, Ruri, Chiyo e Shiori.

Processo: `python tools/art/maid_walk/maid_walk.py process --only <ids>` (grava a sheet em `Assets/_Game/Resources/Maids/`, atualiza `maid_walk_catalog.json`, manifestos/contact sheets/previews em `docs/art/maid-walk/<id>/`). Frames originais em `art_working/` (git-ignorados) nunca foram alterados; nenhum frame foi inventado.

## Classificação

| Maid | Frames (usados/achados) | Ciclo | Deslize | Classificação |
|---|---|---|---|---|
| Mikan | 25/25 | fecha (1,4) | 2,7 | PASS |
| Yuki | 27/27 | fecha (0,6) | 2,8 | PASS |
| Kurumi | 37/37 | fecha (1,1) | 3,5 | PASS |
| Nonoka | 34/34 | fecha (0,4) | 2,7 | PASS |
| Kaguya | refeita, READY | fecha (1,15) | 3,1 | PASS |
| Mizuki | 40/40 | fecha (1,3) | **5,1** | PASS COM RESTRIÇÕES (passos curtos; pode patinar) |
| Himari | 26/26 | fecha (0,5) | 2,6 | PASS (matte tratado, `matte_cut` 110) |
| Anzu | 30/30 | fecha (0,7) | 3,4 | PASS |
| Kasumi | 17/26 | fecha após corte | 2,9 | PASS COM RESTRIÇÕES (só trecho 67–83) |
| Hibari | 25/25 | fecha (0,9) | 2,8 | PASS |
| Seira | processada | fecha | ver manifesto | PASS |
| Ruri | processada | fecha | ver manifesto | PASS |
| Chiyo | processada | fecha | ver manifesto | PASS (matte tratado, `matte_cut` 110) |
| Shiori | processada | fecha | ver manifesto | PASS (matte tratado, `matte_cut` 110) |

Nenhuma ficou em REWORK. Valores exatos de cada uma: `docs/art/maid-walk/<id>/manifest.json`.

## Lote final: Tsubaki, Mirai, Kinako, Mayu (24–27)

Mesmo processo (`process --only tsubaki,mirai,kinako,mayu`), sheets com as configurações vigentes (DXT5 e overrides WebGL da PR #67, idênticas às das 23 anteriores). Frames originais intactos.

| Maid | Frames (usados/achados) | Numeração | Alpha / matte | Loop | Deslize | Classificação |
|---|---|---|---|---|---|---|
| Tsubaki | 24/24 | 111–134 | alpha limpo | fecha (1,37) | 2,5 | PASS |
| Mirai | 25/25 | 60–84 | matte translúcido (alpha ≈ 49–57): `matte_cut` 110 | fecha (1,93, no limite) | 3,3 | PASS COM RESTRIÇÕES (possível soluço leve na emenda do loop) |
| Kinako | 49/49 | 31–79 (sheet 7×7) | matte translúcido (alpha ≈ 52–57): `matte_cut` 110 | fecha (0,90) | 2,9 | PASS (poucos fragmentos minúsculos perto dos pés em 1 quadro) |
| Mayu | 42/45 | 0–44, usados 3–44 | matte muito fraco removido pelo corte padrão | fecha (1,22) após o corte | 4,2 | PASS (deslize dentro da faixa das originais, 2,3–4,6) |

Nenhuma em REWORK; as quatro foram liberadas, e o catálogo ficou com **27 Released / 0 Planned**. Os retratos vêm de `portraits/batch3/derived/`.

## Integração
- Retratos (`_head`/`_detail`) vêm dos derivados de `docs/art/ui-modular/portraits/batch2/` (Nonoka usa v02) e `batch3/` (Hibari, Seira, Ruri, Chiyo, Shiori). Os retratos do lote 3 foram **aprovados artisticamente pelo dono** (06/11/2026) para Hibari, Seira, Ruri, Chiyo e Shiori, mantendo as restrições documentadas.
- `MaidCatalog`: as 14 passaram de Planned a liberadas (23 no total); com o lote final (24–27) o catálogo chegou a 27 liberadas.
- Sheets carregam sob demanda (Resources); orçamento de textura do teste subiu de 12 para 30 Mpx (~29 Mpx medidos).
- Proveniência SHA-256 e allowlist de `DocsTests` atualizadas (42 arquivos).

## Aprovações do dono (06/11/2026)
- Aprovadas artisticamente: Hibari, Seira, Ruri, Chiyo e Shiori (retrato e caminhada).
- Aceitas provisoriamente, **sem regenerar agora**: Mizuki (deslizamento 5,1) e Kasumi (17 frames úteis).
- Exceção de mantenedor concedida para as alterações revisadas em `DocsTests`, `MaidCatalogTests`, `MaidAnimCatalogTests`, `DemoFlowTests`, `MaidPortraitsBatch2QaTests` e `MaidPortraitsBatch3QaTests`.

## Restrições
Mizuki (patinação) e Kasumi (movimento curto) são as duas a observar em jogo. Ver `docs/art/BACKLOG_ART.md`.
