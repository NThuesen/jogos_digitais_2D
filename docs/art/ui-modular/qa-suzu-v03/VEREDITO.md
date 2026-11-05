# Suzu v03 — QA de retrato (Editor, 960x600 e 1280x720)

**Veredito: PASS COM AJUSTES.** Podemos produzir as outras oito no padrão v03, com os ajustes abaixo.

Nada aqui entra em produção: os recortes ficam em `Assets/_Game/Tests/PlayMode/QaArt` (fora de `Resources`), o teste `SuzuPortraitQaTests` troca só o sprite do `Portrait` depois da tela montada, e a Suzu canônica (nome + Genki) existe apenas no perfil do teste. Capturas: `comparativo_*.png` (atual → v03, pixels reais ampliados) e os frames completos.

| Contexto | Resultado |
|---|---|
| Escalação 0,62× (~93×128) | OK. Rosto, olhos, sorriso e as duas orelhas legíveis; tiara, laços e sininhos inteiros. Suave em 960×600 (rosto com ~54 px), reconhecível. |
| Elenco 0,82× (~123×169) | OK. Mesma leitura, um pouco mais nítida. Aba de posição e gema continuam fora do rosto. |
| Detalhe (protótipo no teste) | Bom. Retrato 3:4 inteiro mostra uniforme, avental, laço e rabo; sem halo nem deformação. |

Sem halos (alpha limpo, cor sangrada nas bordas), sem distorção (recortes com a proporção da fonte), moldura e textos dinâmicos intactos, clique em Escalação (remove a maid) e em Elenco (seleciona) funcionando.

## Ajustes para as outras oito
1. **Folga acima das orelhas no RAW.** A ponta da orelha direita da v03 toca a borda superior (ponta achatada). Pedir ~4% de folga em cima; a limpeza adicionou 12 px de margem, mas não recupera a ponta.
2. **Entregar derivados pré-reduzidos**, não o RAW de ~1086 px: cabeça 1,22:1 (192×157 com unsharp leve) para os cards e corpo 3:4 (480×641) para o detalhe. Reduzir 20× em tempo real borra e serrilha.
3. **Cards usam recorte de cabeça (1,22:1)**; a janela do card é paisagem, o corpo inteiro ficaria minúsculo. Detalhe usa o corpo inteiro.
4. Painel de detalhe real: o protótipo (150 px de moldura + 310 de fatos) passa 12 unidades da linha de 460; usar 138 px ou reduzir o espaçamento.

## Limpeza (tools/art/suzu_v03_prepare.py, RAW intocado)
alpha < 10 zerado, 676 manchas isoladas removidas (36 183 px), cor sangrada para fora do contorno, margem transparente de 12 px. Resultado: `UI-021_suzu_v03_clean.png`.
