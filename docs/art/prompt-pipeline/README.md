# Pipeline de prompts — maids + imagem de caminhada

**Estado:** documentos de produção / **prompts candidatos**, não aprovação de assets nem implementação Unity.
**Fluxo manual escolhido:** ChatGPT (imagem da personagem) → ChatGPT (quadro isolado para caminhada) → Tencent AI Studio (vídeo curto) → avaliação e integração separadas no Unity.
**Custo adicional planejado:** R$ 0 para as duas gerações no ChatGPT já contratado. **Uso, créditos, limites e preços do Tencent devem ser verificados pelo responsável antes de cada uso; nenhuma cobrança é autorizada aqui.**

## Referência artística obrigatória

Anexe **esta imagem de referência de estilo** em **todas** as gerações do ChatGPT das etapas 1 e 2:
[referência cozy pixel-art + visual novel](../art-01/D-pixel-cafe/reference/style_anchor_reference.png) (arquivo existente no repositório).

A imagem guia **acabamento, atmosfera cozy, contornos, proporções estilizadas, paleta e linguagem de apresentação**. **Não** copie a personagem rosa, poses, personagens de fundo, cenário, interface, inscrições ou layout da referência. A referência não é uma folha de sprite nem um PNG pronto para o jogo.

Cada ficha em [MAIDS_INDEX.md](MAIDS_INDEX.md) contém **dois prompts completos, prontos para copiar e colar**. Os prompts-base em [BASE_PROMPT_STEP1_CHARACTER.md](BASE_PROMPT_STEP1_CHARACTER.md) e [BASE_PROMPT_STEP2_WALK_START.md](BASE_PROMPT_STEP2_WALK_START.md) existem para padronizar **novas** fichas; quem utilizar uma ficha pronta **não precisa combinar blocos nem colar os prompts-base outra vez**.

## Tutorial — Etapa 1: gerar uma maid

1. Abra a ficha individual da maid em `maids/`; copie **inteiramente** o bloco `PROMPT 1`.
2. Abra uma conversa de geração de imagem no ChatGPT.
3. **Anexe:** a referência artística acima. **Somente esse anexo é obrigatório**. Para Suzu, você **pode** anexar também a [sheet D0 já existente](../art-01/D-pixel-cafe/incoming/D0_suzu_sheet.png) como referência **de identidade**, se estiver disponível (a preferência é manter a Suzu consistente com esse design). Não anexe uma maid diferente.
4. Cole o prompt e peça **uma imagem apenas**: a personagem **inteira**, orelhas/cauda/mãos/pés visíveis, em proporções compactas de chibi adequadas à animação. A saída **não** é uma folha de poses, screenshot, card, colagem ou fundo de restaurante.
5. Confira a identidade da ficha: cabelos, íris, orelhas, cauda, acessório e uniforme. Refaça se o resultado copiar o rosto/personagem da referência ou trocar traços essenciais.
6. Salve localmente como `<id>_step1_fullbody_v01.png`; anote data, modelo (se conhecido) e decisão: `candidate / approved / rejected`. **Não importe automaticamente para Assets/.**

> A Etapa 1 cria a **referência integral do corpo para animação**. Um portrait de busto para o card de visual novel continua sendo um asset distinto, conforme [decisão portrait + chibi](../MAID_VISUAL_DECISION.md). Esta pipeline **não** o declara concluído.

## Tutorial — Etapa 2: quadro inicial de caminhada

1. Escolha **uma** versão aprovada ou candidata aceita para teste da Etapa 1. Abra uma conversa de geração de imagem (pode ser nova, para evitar deriva de contexto).
2. **Anexe exatamente duas referências:** (A) o PNG `<id>_step1_fullbody_vXX.png` gerado na Etapa 1 — **fonte de verdade da identidade**; (B) a [imagem de direção artística](../art-01/D-pixel-cafe/reference/style_anchor_reference.png) — **fonte de verdade do estilo**. Para a Suzu, **use também apenas esses dois anexos** nesta etapa; a D0 é opcional somente na Etapa 1.
3. Copie **inteiramente** o bloco `PROMPT 2` da **mesma maid**.
4. O resultado deve ser **um único frame**, full-body, centralizado, personagem em meio passo, com o olhar orientado para o observador e corpo ligeiramente em 3/4 para o deslocamento horizontal em tela. **Não** pedir sequência de frames/sprite sheet.
5. O fundo deve ser **chroma sólido uniforme**, com hex indicado na ficha; **se colidir** com olhos, cabelo, orelhas, uniforme, brilhos, ou a imagem tiver vazamento/variação de fundo, troque por outra cor visivelmente ausente na maid e registre a cor real. Um hex no prompt não garante precisão do modelo.
6. Revise **silhueta inteira**, área livre ao redor (cauda/cabelo não podem ser cortados), pose de braços e pernas, estabilidade de acessórios, contornos e fundo. Salve como `<id>_step2_walkstart_v01_<cor>.png`.

**Importante sobre o eixo X:** imagem de um passo **não é uma animação**. Para o jogo mover a maid à esquerda/direita, o Unity movimenta o objeto no eixo X; a animação deve ser preferencialmente **walk-in-place**. Se o vídeo frontal/3⁄4 parecer “andar de lado”, será necessário testar uma vista lateral/3⁄4 dedicada, **não inventar que um único quadro frontal funciona em qualquer direção**. Para personagem assimétrica (orelha marcada, heterocromia), **espelhamento altera lados**.

## Tutorial — Etapa 3: Tencent AI Studio (experimento de vídeo)

1. **Anexe:** o PNG da **Etapa 2** como imagem inicial/source frame. Se o recurso específico aceitar múltiplas referências, **opcionalmente** anexe também o PNG aprovado da Etapa 1 para estabilizar identidade; não presumir que a interface suporta dois anexos.
2. Use este texto-base na ferramenta de vídeo (ajuste se a interface tiver campos separados):

```text
Animate the attached single cat-maid into a short seamless walk-in-place cycle for a 2D game. Keep the camera completely locked; keep the character centered and fully visible from ears to shoes and tail tip. Natural alternating leg steps and opposite arm swing, small tail and hair secondary motion, minimal body bob. Maintain the exact face, eye colors, hairstyle, ear asymmetry, accessories, outfit and proportions of the input frame. Keep the background a perfectly flat, uniform unchanged chroma color throughout every frame, with no ground shadow, color spill, texture, lighting shift, particles, props, text, cuts, zooms, scene movement or additional characters. No traveling across the frame: the game engine will handle horizontal position. Return a stable animation that can be looped.
```

3. Inspecione o **vídeo inteiro**: cor do chroma, estabilidade das mãos/pés, identidade do rosto, acessórios que mudam ou desaparecem, cauda cortada, câmera fixa, duração e possibilidade de loop. Geração de vídeo **pode** variar o fundo e a anatomia; o prompt não garante composição para Unity.
4. Salve `<id>_step3_walk_v01.<formato_real>` e registre `candidate / approved / rejected`. Não converta vídeos automaticamente em sprites e não altere gameplay/save para esta experiência.

## Checklist de aceite por maid

- [ ] Etapa 1: personalidade e silhueta únicas; nenhum personagem copiado da referência
- [ ] Etapa 1: mãos, sapatos, orelhas e cauda sem cortes; roupa detalhada mas legível em tamanho pequeno
- [ ] Etapa 2: a **mesma maid**, sem trocar lados/cores/acessórios; quadro único, mid-stride
- [ ] Etapa 2: chroma realmente uniforme **e distinto** de todos os componentes visuais
- [ ] Etapa 3: rosto, cor, roupa, orelhas, cauda, chroma e câmera estáveis por todo o vídeo
- [ ] Aprovação visual do responsável registrada **antes** de considerar asset final
- [ ] Quando houver integração: captura real Unity e testes apropriados; **mockup/vídeo não é validação no Unity**

## Organização dos resultados

Prompts/versionamento ficam neste diretório. Resultados de IA são arquivos **externos/candidatos** até decisão de integração; não misturar `incoming`, `processed`, `review` ou `Assets/` com imagens ainda não revisadas. Padrão sugerido fora do código de produção:

```text
art_working/
  01_suzu/step1/        # originais gerados
  01_suzu/step2/        # quadros chroma
  01_suzu/step3/        # vídeos e avaliação
```

Não criar 9 folders vazios nem comprometer centenas de vídeos no Git. Para levar resultado ao repo, registrar origens/versões/licença aplicável, aprovar e preparar um PR independente. **Este diretório de prompts não altera a direção D aceita, o GDD, a economia, os sprites existentes, a UI, o save nem a pipeline do Unity.**
