# Ferramentas de avaliação ART-01D (não são código do jogo)

Scripts de **avaliação visual**, usados para reproduzir `processed/` e `review/` a partir de `incoming/`. **Não** entram no build do Unity, **não** são infraestrutura do projeto e não rodam em CI. Dependências gratuitas: Python 3.12 + `pillow`, `numpy`, `scipy` (instalar num venv local, fora do repo). Fonte do mockup: `C:/Windows/Fonts/trebucbd.ttf` (provisória, Windows).

Ordem (na raiz do repositório; `R=docs/art/art-01/D-pixel-cafe`):

```
python -I $R/tools/build_cutouts_boards.py $R                                  # recortes Suzu + painéis 01-03
python -I $R/tools/ears_board.py $R/processed $R/incoming $R/review/04_orelhas_entalhe.png
python -I $R/tools/ui_extract.py $R                                            # 17 peças do D4 -> processed/ui
python -I $R/tools/ui_window.py $R                                             # molduras com miolo transparente + manifesto
python -I $R/tools/compose_lineup.py $R                                        # review/05
python -I $R/tools/compose_restaurant.py $R                                    # review/06 e 07
python -I $R/tools/compose_modularity.py $R                                    # review/08
```

Reexecutar esta cadeia reproduz byte a byte os arquivos já versionados (verificado em 2026-10-28). Parâmetros de recorte (tolerância 32, 1 px de defringe, alfa binário) e de escala (média por área) são **experimentais**. `seg.py` e `artlib.py` são bibliotecas dos demais.
