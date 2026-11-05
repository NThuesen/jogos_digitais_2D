# UI-021 — nove retratos: CLEAN, derivados e QA (Editor, 960x600 e 1280x720)

Pipeline: `tools/art/maid_portraits_prepare.py` (RAWs intactos, SHA-256 de tudo em `MANIFEST.json`). Derivados usados pelo QA em `Assets/_Game/Tests/PlayMode/QaArt` (fora de `Resources`, nada vai para o build): cabeça 192x157 (1,22:1) e detalhe 480x641 (3:4). Evidências: `nove_retratos.png`, `cards_*.png` (Escalação 0,62x + Elenco 0,82x), `detalhe_*.png`.

| Maid | Veredito | Nota |
|---|---|---|
| Suzu | PASS COM RESTRIÇÕES | ponta da orelha direita achatada na borda superior do RAW |
| Hina | PASS | |
| Akari | PASS | pontas do rabo de cavalo tocam a borda esquerda do card |
| Momo | PASS | |
| Reina | PASS | |
| Kaede | PASS | |
| Nene | PASS | cacho direito levemente cortado no card |
| Ichika | PASS COM RESTRIÇÕES | topo do chapéu de chef toca a borda do RAW (~2 px); mancha cinza semitransparente atrás dele removida no CLEAN |
| Yoru | PASS | franja cobre um olho (desenho, igual ao sprite de caminhada) |

Nenhum REWORK. Identidade conferida contra cada `*_walk.png` (cabelo, olhos, acessórios, uniforme).
