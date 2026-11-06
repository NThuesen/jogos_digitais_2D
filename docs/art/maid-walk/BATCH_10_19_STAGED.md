# Caminhadas das maids 10–19: processadas (staged), NÃO integradas

Frames do Blender (RGBA 1920x1080, já com alpha real, sem chroma) de Mikan, Yuki, Kurumi, Nonoka, Kaguya, Mizuki, Himari, Anzu, Kasumi e Hibari. Processados com `python tools/art/maid_walk/maid_walk.py process --stage --only <ids>`: com `--stage` a sheet fica ao lado do manifesto em `docs/art/maid-walk/<id>/<id>_walk.png`; `Assets/` (Resources, `maid_walk_catalog.json`, `.meta`) não é tocado, então nada vai para o build e nenhuma identidade é liberada. Os frames originais em `art_working/` (git-ignorados) não foram alterados ou renomeados; nenhum frame foi inventado (Kasumi usa só o trecho 67–83 que fecha o ciclo, o pipeline existente).

Nada aqui está aprovado: o status do manifesto é o do pipeline (NEEDS_PROCESSING = limpeza automática aplicada; LIMITED = ciclo não fecha) e o veredito abaixo é uma proposta técnica para a sua revisão.

| Maid | Pasta | Frames (usados/achados) | Numeração | Sheet | Status do pipeline | Ciclo | Deslize (slide) | Veredito proposto |
|---|---|---|---|---|---|---|---|---|
| Mikan | 10 | 25/25 | 77–101 | 172x252, 25 | NEEDS_PROCESSING | fecha (1,4) | 2,7 | PASS |
| Yuki | 11 | 27/27 | 77–103 | 192x252, 27 | NEEDS_PROCESSING | fecha (0,6) | 2,8 | PASS |
| Kurumi | 12 | 37/37 | 9–45 | 168x252, 37 | NEEDS_PROCESSING | fecha (1,1) | 3,5 | PASS |
| Nonoka | 13 | 34/34 | 44–77 | 168x252, 34 | NEEDS_PROCESSING | fecha (0,4) | 2,7 | PASS |
| Kaguya | 14 | 44/44 | 9–52 | 180x252, 44 | LIMITED | **não fecha (2,1)** | 3,5 | **REFAZER o vídeo** |
| Mizuki | 15 | 40/40 | 146–185 | 188x252, 40 | NEEDS_PROCESSING | fecha (1,3) | **5,1** | PASS COM RESTRIÇÕES (deslize) |
| Himari | 16 | 26/26 | 139–164 | 168x252, 26 | NEEDS_PROCESSING | fecha (0,5) | 2,6 | PASS (ver nota do matte) |
| Anzu | 17 | 30/30 | 103–132 | 156x252, 30 | NEEDS_PROCESSING | fecha (0,7) | 3,4 | PASS |
| Kasumi | 18 | 17/26 | 67–92 → 67–83 | 140x252, 17 | NEEDS_PROCESSING | fecha após o corte | 2,9 | PASS COM RESTRIÇÕES (só 17 frames úteis) |
| Hibari | 19 | 25/25 | 110–134 | 148x252, 25 | NEEDS_PROCESSING | fecha (0,9) | 2,8 | PASS |

Referência das nove originais já aceitas: deslize de 2,3 a 4,6; Kaede ficou LIMITED (2,5) como a Kaguya agora. Sem lacunas de numeração em nenhuma pasta. Os arquivos chegam como `.gitignore0077.png` (nome que o Blender gerou); o pipeline aceita `<prefixo><dígitos>.png` e não renomeia nada.

## Problemas pendentes
1. **Kaguya: refazer o vídeo.** O ciclo não fecha mesmo depois do corte (2,1× um passo normal) e, no contact sheet, a personagem gira para a câmera no meio da sequência em vez de caminhar de lado o tempo todo. Dá para usar o trecho inicial, mas não fecha loop limpo; sugiro novo take com câmera fixa e perfil lateral constante.
2. **Mizuki: patinação.** Passos curtos (pé 1,33 px/frame) para a distância percorrida: deslize 5,1, acima de todas as originais (máx. 4,6, Hina). Pode ser aceitável em tela pequena, mas é o caso mais provável de "moonwalk"; se incomodar no jogo, refazer com passos mais largos.
3. **Kasumi:** só 17 dos 26 frames fecham o ciclo; o movimento fica mais curto/rápido.
4. **Himari: matte do Blender.** O export tinha um fundo translúcido (alpha ≈ 47–53, acima do corte padrão de 40) cobrindo ~21 % do quadro, e depois ruído esparso. Adicionei a opção por maid `matte_cut` em `maids.json` (110 para a Himari). Verificado visualmente no contact sheet, que ficou limpo. Para exports futuros: ligar Film → Transparent sem sky/world e conferir o alpha do canto.
5. Nada foi testado em jogo: tamanho (~1–3 MB cada sheet RGBA, soma de 37 MB nesta PR de docs) e custo no WebGL só serão medidos na liberação.

## Para liberar (outra PR)
Mover cada sheet para `Assets/_Game/Resources/Maids/`, rodar `process` sem `--stage` (gera catálogo e `.meta`), liberar a identidade no `MaidCatalog` e fazer o QA em 960x600 e 1280x720.
