# Tutorial prático — do prompt à imagem modular aprovada

Leia antes da primeira geração. **Não precisa instalar modelos/serviços pagos.** O gerador é o ChatGPT que você já usa; Blender já fez parte do fluxo das maids; Unity monta a interface.

## 1. Abrir e baixar as referências corretas

As imagens **já estão versionadas** no repositório. Use o [índice de referências](references/README.md) para localizar e abrir os PNGs/JPGs reais. Para enviar ao gerador: **baixe a imagem original e anexe como arquivo/imagem no chat**. Não confie em apenas colar um link: isso pode não anexar a imagem.

| Finalidade | Anexo | Obrigatório? |
| --- | --- | --- |
| Identidade de atmosfera de todos os assets | `style_anchor_reference.png` de ART-01D | Sim (até termos style sheet novo aprovado) |
| Formatos e modularização de UI | `D4_ui_kit.png` + `review/08_modularidade_ui.png` | Útil como referência técnica, **não** como peça final a copiar |
| Fidelidade da maid/escala e contraste | captura WebGL ilustrada do ART-04 | Para card, portrait, HUD e cenário |
| Espaço real disponível | captura da tela Unity correspondente, do UI-01A/B | Para componente específico da tela |
| Continuidade de um kit aprovado | PNG aprovado (painel/botão/card) | **Obrigatório** após P0 para peças da mesma família |
| Consistência de uma maid | referência oficial dela, não personagem genérico da style reference | Obrigatório para gerar portrait novo, após aprovação do personagem |

**Referência original ≠ asset aprovado.** ART-01D era pixel art; mantenha aconchego, ornamentos e hierarquia, **não** os pixels, contornos quadrados ou composição literal.

## 2. Exemplo real: primeiro painel (UI-001)

1. Abra um **chat dedicado à geração de uma imagem**, de preferência novo para um asset independente.
2. Anexe o PNG original `style_anchor_reference.png`. Se quiser informar o espaço, anexe também uma captura atual da Loja, de UI-01A.
3. Abra [00_GLOBAL_RULES.md](prompts/00_GLOBAL_RULES.md) e copie o bloco `PROMPT GLOBAL`.
4. Em seguida, copie o bloco `PROMPT DE GERAÇÃO` de [01_PANEL_LARGE.md](prompts/01_PANEL_LARGE.md), substituindo os campos `[...]` pela decisão do piloto. **Não envie Markdown inteiro como instrução para desenhar o próprio documento.**
5. Solicite imagem **única e isolada**, sem mockup de tela. Se o fundo não vier realmente transparente, aceite o original apenas como **RAW**, não como asset final.
6. Salve o original sem alterações em `art_working/ui_modular/raw_gpt/UI-001_panel_large_v01.png`. Renumere em `v02` se refizer; não sobrescrever o bruto.
7. Abra no Blender para remover fundo **se necessário**, preservando sombras, preenchimento creme, bordas coloridas e vazados. Exporte RGBA para `art_working/ui_modular/blender_clean/UI-001_panel_large_v01.png`.
8. Insira provisoriamente no Unity como `Sprite (2D and UI)`; teste sobre fundos claro/escuro em 960×600 e 1280×720, incluindo áreas de conteúdo e 9-slice quando aplicável.
9. Peça avaliação do responsável com imagem do **asset isolado** e **captura real do Unity**. Só após aprovação, atualize [ASSET_REGISTRY.md](ASSET_REGISTRY.md) e prepare PR de arte curada.

## 3. Como enviar os outros prompts

- **Cada arquivo [01–25] em `prompts/` informa anexos, campos e proporções.** Siga essa lista em vez de anexar nove maids e todo o repo em cada solicitação.
- **Depois de aprovar o primeiro painel**, anexe seu PNG aprovado em cada prompt dependente. O gerador precisa de imagens concretas; dizer "same style" sem anexar o painel pode causar deriva.
- **Estados de botão, aba e raridades:** uma imagem/arquivo por estado ou raridade. Use o mesmo prompt com `[STATE]`/`[RARITY]` alterado; preserve ângulo e dimensões para sobreposição perfeita. Uma prancha com 4 estados serve como estudo, **não** como export final para Unity sem recorte.
- **Ícones e props:** gere um tipo por vez quando a qualidade de borda importar; não coloque 10 itens em um único quadro sem necessidade. O modelo frequentemente transforma pedidos de sprites separados em um quadro com bordas coladas.
- **Logo:** é a única exceção permitida para título literal baked; revisar manualmente a grafia "NEKO MAID INC." antes de aprovar.
- **Fundos:** podem ser opacos se forem camadas cênicas; não incluir controles/botões/texto nem móveis interativos baked. Não confundir imagem `16:10` ilustrativa com piso tileable ou cenário que precisa crescer.

## 4. Transparência no Blender — critérios, não truque único

O Blender pode remover fundo como você fez nos sprites das maids, **mas salvar como RGBA não cria alpha automaticamente**. Confira a máscara em diferentes cores de fundo e em zoom 100%. Ajuste o tratamento conforme a imagem (chroma uniforme vs. fundo com textura).

**Nunca remova o creme interno do painel** achando que ele é background. Se a peça tiver "janela" de portrait, remova **só a área indicada**. Verifique halo colorido na borda, transparência parcial preservada, sombras dentro do canvas, orelhas/fitas e áreas finas. Em caso de borda contaminada, volte ao original.

## 5. Guia de 9-slice e componentes Unity

- UI com centro repetível, bordas retas, cantos simétricos: candidato a **Sprite Editor → Border** e `UnityEngine.UI.Image.Type.Sliced`.
- Faixas longas, ângulos, joias, flores e cantos assimétricos: **separar em sprites de overlay** ou usar escala uniforme. 9-slice não cria do nada cantos que funcionem em qualquer proporção.
- **Não esticar portrait, ícone ou móveis.** Preserve aspect ratio. O menu tem valores dinâmicos e textos longos; reserve interior suficiente.
- Verifique `Button` com hitbox e estados dinâmicos na Unity. PNG de botão não faz clique sozinho.
- Importação definitiva, atlas, compressão, mipmaps e PPU são decisões técnicas testadas **depois** de conhecer a imagem final. Não fixar parâmetros por suposição.

## 6. Regra de versionamento e handoff

- `art_working/ui_modular/`: diretórios versionados, mas PNG/brutos/Blend locais e ignorados por `art_working/.gitignore`. **Não estão automaticamente visíveis a outros worktrees/Claude/Codex ou ao GitHub**.
- Para mandar um candidato a outro agente: usar `docs/art/ui-modular/incoming/` em PR específico **ou** compartilhar o caminho/arquivo de forma explícita. Candidatos não aprovados não vão direto a `Resources/`.
- Para arte aprovada: PR separado, com PNGs limpos e suas `.meta`, origem/proveniência e testes. **Não** fazer merge automaticamente de arte final sem aval visual do responsável.

## 7. Checklist de QA para cada imagem

- [ ] Arte original; nenhum personagem ou prop copiado literalmente da style reference
- [ ] Uma peça reutilizável; sem texto, números ou rótulos baked (salvo logo)
- [ ] PNG preservado (bruto), com nome e versão
- [ ] Alpha verdadeiro ou fundo opaco justificado; sem halo
- [ ] Área de conteúdo segura e dimensões coerentes
- [ ] Não esticado: bordas e ornamentos íntegros
- [ ] Captura Unity real em 960×600 e 1280×720
- [ ] Texto/controlador Unity legível; hitbox e scroll funcionam
- [ ] Impacto de textura revisado no WebGL quando integrado
- [ ] Aprovação explícita e registro no registry antes de produção em lote
