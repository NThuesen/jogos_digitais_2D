# UI-INT-01 — Hub e Caixinhas a partir do kit aprovado

Fonte: `neko-ui-kit-delivery/` (CLEAN + sprites `_v01`, mockups em `RAW/` só como referência). Nenhuma mudança em gameplay, economia, RNG, chances, preços, número de cartas, saves ou `Neko.Sim`.

## O que mudou
- `HubScreen` e `BoxScreen`: layout ilustrado montado por `KitArt` (novo) sobre os PNGs do kit. **Todo texto e número é `Text` do Unity** sobre placas em branco; nada de gameplay está em textura.
- Se algum sprite do kit faltar (`KitArt.Has`), a tela cai no layout procedural anterior (`BuildPlain` / `BuildBox`), intacto.
- Hub: fundo limpo, painel (Dia, Caixa, Fichas, Custo fixo por dia), barra de nível pela reputação real, alerta (placa só aparece com texto; texto encolhe para caber), química/multiplicador reais, cartas reais via `MaidCard.Build` (qualquer número de slots preenchidos), 5 botões (Equipe do Dia ativo, Maids, Caixinhas, Lojinha, Café), CTA "Começar Expediente" (desabilitado = tom cinza + rótulo), Voltar.
- Caixinhas: fundo limpo, título, contador de fichas, 3 cartas (painel + baú próprio + placa de nome), nome/preço/chances vindos de `Lootbox.All`/`Lootbox.Odds` (só raridades com chance > 0), botão amarelo se pagável e marrom se não (desabilitado), Voltar. Revelação e "duplicata devolve fichas" inalteradas.
- Hover/pressed/disabled: o kit traz uma imagem por botão, então os estados são *tints* da placa (`Selectable.Transition.ColorTint`) + o punch de clique existente.

## Inventário de sprites
Importados 35 PNGs byte a byte em `Resources/UiKit/` (bordas 9-slice do manifesto nos `.meta`; hashes em `SOURCES.json` e `SUZU_ASSET_PROVENANCE.md`; `tools/art/ui_kit/import_ui_kit.py`). Todos RGBA com alpha real (conferido: mín. 0, máx. 255; só `cream_surface_tile`, opaco, ficou de fora).
- **Em uso (Hub):** hub_background, economy_panel_empty, level_panel_empty, level_track_empty, level_fill, warning_blank, nav_pink_surface, nav_active_blank, cta_blank, back_blank.
- **Em uso (Caixinhas):** caixinhas_background, chest_simple/silver/gold, box_{simple,silver,gold}_panel_empty, box_name_empty (nome e contador), odds_panel_empty, title_blank, box_button_active_blank, box_button_brown_blank, back_blank, paw_gold, paw_cream.
- **Importados, ainda sem uso (P2):** economy_frame, level_frame, box_*_frame, box_name_frame, odds_frame, bow_gold/blue/cta, nav_pink_blank. `nav_pink_blank` traz um ícone fixo (confeitaria) que não serve às outras abas, então os botões rosa usam `nav_pink_surface` sem ícone.

## Validação (medida)
- EditMode **379/379**. PlayMode **110/113**: as 5 `UiIntegrationTests` novas passam (dados reais, 1–5 maids, alerta longo, 5 botões + Voltar + CTA, preços/chances das 3 caixas, compra real + revelação, 960×600 e 1280×720). As 3 falhas são testes protegidos do Layout_Check que codificam o Hub antigo (ver abaixo).
- WebGL: `tools/build-webgl.sh` OK (70 MB); no Chrome: Hub e Caixinhas ilustrados, compra real (40 fichas → carta, duplicata devolveu 5), sem erros no console.
- Evidências: `evidence/hub_vs_mockup_*.jpg`, `evidence/boxes_vs_mockup_*.jpg` (mockup à esquerda, jogo à direita) e os frames de cada estado.

## Falhas conhecidas / bloqueio de teste protegido
`ClicksFindTheBaselineTargets`, `ModelDescribesEmpresa` e `ProbesAgreeWithTheRenderedFrame` (`Tests/PlayMode/Layout/`) procuram no Hub planks com os rótulos antigos ("Voltar" como plank, "Elenco") e o caminho `HubScreen/Plaque`. Os rótulos novos (Maids, Caixinhas, Lojinha, Café) foram pedidos no brief e a placa mudou. Não alterei esses testes; pedem exceção do dono para atualizar as expectativas. As auditorias R2.1 (Audit EM-1/EM-2/CX-1) passam: o fundo é um nó `Scenery`, como o fundo antigo.

## Limitações
- Fonte: continua a fonte legada do Unity (sem TTF amigável, para não importar fonte sem licença/allowlist). Decisão do dono.
- Ícones das abas (Maids/Caixinhas/Lojinha/Café) ainda não existem; só a aba ativa tem a pata do próprio sprite.
- A barra de nível quase vazia fica discreta (o trilho é claro sobre o painel claro).
- 960×600 corta ~64 px laterais do fundo (cobre a tela, mantém a proporção); a UI fica dentro da área segura.
- Mockup tem taglines ("Mais comum"…); aqui o subtítulo é derivado dos dados (nº de cartas e raridade mínima), não copiado.
