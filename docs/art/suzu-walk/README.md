# ART-02 — caminhada da Suzu no turno real

**Estado:** implementado em branch/PR de revisão; **aprovação estética do responsável pendente**. Sem merge.
Não é a Suzu "totalmente animada": só **walk** tem arte nova. Idle, carry, cook e tired reutilizam
quadros da própria caminhada (ver "Estados").

## Pipeline

```
art_working/.../walk_x/frames/*.png (30, intactos)
  → tools/art/suzu_walk/process.py            (Pillow + numpy + scipy; determinístico)
  → Assets/_Game/Resources/Suzu/suzu_walk_sheet.png   (6×5 células de 92×126, 120 px de personagem)
  → SpriteFactory.Sheet (recorta uma vez, cacheia, libera no Clear)
  → MaidView (somente Name == "Suzu" && Archetype == Genki, sem equipamento)
  → WalkCycle (fase avança pela DISTÂNCIA percorrida na tela)
```

Parâmetros, hash SHA-256 de cada PNG de origem e do sheet: [`manifest.json`](manifest.json).
Proveniência e allowlist: [`../SUZU_ASSET_PROVENANCE.md`](../SUZU_ASSET_PROVENANCE.md).
Reproduzir: `python tools/art/suzu_walk/process.py --height 120`.

## Decisões

| Tema | Decisão | Por quê (medido) |
| --- | --- | --- |
| Frames | 30 → 30, ciclo completo | Um ciclo de 2 passos; o sheet custa 1,4 MB de textura e ~180 KB de PNG. |
| Faixas no alpha | removidas (230 componentes) | 2–3 faixas finas por frame inflavam a caixa de y=149 para y=1070. |
| Piscar longo | olhos do quadro aberto mais próximo, alinhados pela franja/bochechas (frames 13–24) | 12 de 30 quadros de olho fechado repetiriam a cada passo. |
| Âncora | pé de apoio (y) + eixo da saia (x) | O vídeo deriva ±17 px; sem isso a Suzu "passeia" dentro da célula. |
| Estilo | 48 cores compartilhadas, alpha duro, Point | Casa com os móveis em pixel art; a ilustração lisa destoava. |
| Tempo | **distância**, não FPS: 1,2 unidades por ciclo (`SuzuSprites.UnitsPerCycle`) | O FPS original não existe nos PNGs. Distância = pausa/aceleração/slow-mo corretos sem código extra. |
| Parar | o ciclo corre (60 q/s) até o quadro seguinte com os dois pés no chão e segura | Evita congelar em meio ao passo; o frame de repouso é da própria sequência. |
| Idle | quadro 10 | Pés nivelados e juntos; é o menor salto em relação ao Walk. |

## Estados

| Estado | Apresentação | Observação |
| --- | --- | --- |
| Walk | ciclo de 30 quadros | Frente/¾; esquerda espelha (cauda/sininho invertem). |
| Idle | quadro 10 | estático |
| Carry | walk/idle + prato procedural nas mãos | Não há desenho carregando bandeja; é um marcador. |
| Cook | idle/walk | efeitos de vapor existentes |
| Tired | idle/walk escurecido + barra de humor | sem expressão cansada |
| Equipamento | maid volta ao visual procedural em camadas | preserva "equipamento é visível" |
| Outras maids | inalteradas | |

## Evidências (`evidence/`)

- `raw_frames_contact_sheet.png` / `processed_frames_contact_sheet.png` — antes e depois do processamento.
- `gameplay_960x600.png`, `gameplay_1280x720.png` — turno real, Suzu entre maids procedurais.
- `gameplay_walking_*.png`, `gameplay_walk.gif`, `gameplay_strip_walk_carry.png` — caminhada e carry em jogo.
- `bigger_room_point_1280x720.png` vs `bigger_room_bilinear_1280x720.png` — sala 13×9, Point × Bilinear.

## Resultados

| Verificação | Resultado |
| --- | --- |
| Unity 6000.5.9f1 EditMode completo | **PASS** — 322/322 |
| Unity PlayMode `SuzuGameplayTests` | **PASS** — 4/4 (turno real a 4×, ambos os sentidos, pausa congela o quadro, carry/tired/equipamento, skip → Results, sem logs inesperados) |
| Capturas reais 960×600 e 1280×720 | feitas (PlayMode) |
| `verify.py boundaries` | **PASS** |
| `tools/tests/test_unity_validation.py` | **FAIL no Windows (3/7)** — caminhos temporários do bash; não toca em nada deste PR; não executado em Linux |
| Suíte PlayMode completa | NOT RUN (só a classe da Suzu) |
| `tools/simtest.sh` | NOT RUN (Neko.Sim intocado) |
| Build WebGL e verificação no navegador | **NOT RUN** — a build foi interrompida pelo Claude Code por pouca memória do sistema e não foi repetida. Executar `tools/build-webgl.sh` com a máquina livre antes do merge. |

## Problemas conhecidos

1. **Deslizamento dos pés (média).** Os passos da arte são curtos (cruzados, "de passarela"); a velocidade da maid é 1,5–4 unid/s. Com 1,2 unid por ciclo há deslize residual. Alternativa: novo vídeo com passada maior.
2. **Sem quadros laterais/costas (média).** Andar para cima/baixo mostra a frente; esquerda é espelho.
3. **Sala 13×9 (baixa).** O sprite cai a ~65 px e Point deixa pixels irregulares; Bilinear suaviza mas borra.
4. **Carry (baixa/média).** Prato procedural genérico, sem pose de braço.
5. **Tired (baixa).** Só escurece.
6. **Mapeamento por nome (média, decisão aprovada).** Qualquer maid sorteada "Suzu" Genki recebe a arte; `visual_id` fica para depois.
7. **Equipamento (média).** Maid com equipamento perde a arte autoral.
8. **Aprovação estética e de movimento ao vivo (pendente).** As capturas em PlayMode batch têm tempo discreto; avaliar no Editor/WebGL.
