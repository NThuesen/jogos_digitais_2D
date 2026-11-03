# Neko Maid Inc. — Kit modular GPT → Blender → Unity

**Estado em 03/11/2026:** INFRAESTRUTURA PREPARADA; **nenhuma imagem nova aprovada ou importada** por este PR. O estilo aprovado é **Cozy Illustrated Anime Café + Ornamental Visual Novel**, como orientação geral. Paleta final, materialidade exata, molduras, proporções e composição estão **em aprovação futura** — não tratar propostas de prompt como decisões estéticas definitivas.

**Contrato superior:** [direção visual atual](../CURRENT_VISUAL_DIRECTION.md) · [roadmap](../../ROADMAP.md) · [UI-02](../../roadmap/UI-02_SCREEN_POLISH.md).

## Começar aqui (5 passos)

1. Leia [TUTORIAL.md](TUTORIAL.md). O tutorial define como escolher o asset, onde pegar as referências e o que anexar ao ChatGPT.
2. Abra [GENERATION_ORDER.md](GENERATION_ORDER.md): comece **apenas com o piloto** `01_PANEL_LARGE`, `04_BUTTON_PRIMARY`, `08_MAID_CARD_COMPACT`; demais prompts são uma biblioteca pronta, **não autorização para gerar tudo**.
3. Em um chat de geração de imagens por asset, **anexe os arquivos reais** conforme o campo **Anexos** do prompt. Links de GitHub dentro do texto do prompt não garantem que o gerador receberá a imagem.
4. Cole o conteúdo de [00_GLOBAL_RULES.md](prompts/00_GLOBAL_RULES.md), seguido **do prompt escolhido** em [prompts/](prompts/). Preencha os parâmetros marcados `[...]` antes de enviar. Para variações, gere imagens separadas.
5. Salve a saída original em `art_working/ui_modular/raw_gpt/`; após limpeza de alpha no Blender/edição, passe a `blender_clean/`. Somente após comparação Unity e aprovação humana registre `approved` no [ASSET_REGISTRY.md](ASSET_REGISTRY.md) e faça um **PR curado** dos assets finais para `docs/art/ui-modular/approved/` ou diretamente para `Assets/_Game/Resources/`, conforme a integração planejada.

## Local vs Git

| Caminho | Função | Versionamento |
| --- | --- | --- |
| `docs/art/ui-modular/prompts/` | Especificações, prompts individuais, regras | **Git: sim** |
| `docs/art/ui-modular/references/` | Índice de referências já versionadas; imagens não duplicadas | **Git: sim** |
| `docs/art/ui-modular/incoming/` | Candidatos **selecionados para revisão compartilhada** | Git: só em PR explícito |
| `docs/art/ui-modular/approved/` | Assets visuais aprovados para futura importação; inicialmente vazio | Git: só com aprovação |
| `art_working/ui_modular/raw_gpt/` | Resposta bruta do gerador, todas as tentativas | PNG **ignorado pelo Git** |
| `art_working/ui_modular/blender_clean/` | Recortes, máscaras e PNG alfa limpos | PNG **ignorado pelo Git** |
| `art_working/ui_modular/export_unity/` | Exportação local candidata para Unity | PNG **ignorado pelo Git** |

As pastas foram criadas com `.gitkeep`. **Uma pasta vazia no GitHub não significa que os PNGs existem**. Não copiar em lote os brutos para GitHub nem usar `git add -f` para ignorados sem aprovação. Os assets gráficos já existentes em ART-01D continuam disponíveis como referências históricas.

## Critérios de pronto por asset

- **Identidade:** coerente com as nove maids ilustradas; não pixel art, não copiar a imagem de referência.
- **Modularidade:** apenas uma peça ou variações explicitamente controladas; sem UI de tela inteira, screenshots, texto/números/stats baked (exceção: logo estático).
- **Transparência:** alfa real se necessário, sem fundo xadrez falso, halo, borda verde/magenta ou perda de partes claras.
- **Técnica:** centro limpo/área segura, escala e proporções utilizáveis; dividir ornamentos de cantos antes de aplicar 9-slice.
- **Unity:** texto, preços, raridades, cliques, focus/hover/pressed/disabled, scroll e layout permanecem dinâmicos. QA em **960×600 e 1280×720** e WebGL quando integrado.
- **Recursos:** registrar dimensões e tamanho, avaliar compressão/memória no navegador antes de ampliar biblioteca de texturas.

## Papel dos agentes e decisão humana

**ChatGPT:** gerar imagens quando instruído; **Blender:** limpeza manual de fundo/chroma e recorte; **Claude Code:** direção visual, crítica, preparo/integração de arte; **Codex:** integração Unity, automação e QA. **Responsável aprova cada asset-piloto** e decisões artísticas importantes. Não há novo redesign do gameplay, simulação ou saves neste kit.
