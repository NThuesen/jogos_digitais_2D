# ART-02 — Uma maid completa no Unity (vertical slice visual)

**Status:** **LIBERADA para implementação experimental** pelo responsável em 2026-10-28, após conclusão da ART-01D como exploração. Aprovação restrita às referências, não aos assets finais. **Agente implementador recomendado:** Codex.
**Leia:** [estilo aprovado/artwork ART-01](ART-01_STYLE.md), [decisão](../art/MAID_VISUAL_DECISION.md), [spec](../art/MAID_VISUAL_SPEC.md), [template da identidade](../art/maids/_TEMPLATE.md), [Unity](../agent/UNITY.md). Consultar GDD apenas para arquétipo/raridade quando necessário.

## Recorte de gameplay — 2026-10-30

Integração parcial do **chibi Suzu** no caminho real de gameplay: [implementação, smoke e evidências](../art/suzu-gameplay/README.md). O recorte usa o pixel art nativo do experimento `670a594a31ff`, conforme handoff ART-02. Portrait/card continuam procedurais; o gate completo portrait + chibi e a aprovação visual da integração permanecem pendentes. **Não fazer merge nem auto-merge.**

## Hipótese
Uma única identidade visual pode sustentar **portrait atraente + chibi fofo animado**, ser reconhecida no card e restaurante e permitir troca pontual de componente sem regeneração completa. Validar isso **antes** de fixar pipeline e biblioteca.

## Primeira integração autorizada

Usar os assets existentes da [direção D](../art/art-01/D-pixel-cafe/README.md): portrait e chibi da Suzu processados. Integrar um portrait em card real e um chibi no restaurante real, começando em **96 px de altura visual como hipótese**, não tamanho final. Demonstrar uma alteração visual isolada e reversível sem reconstruir a personagem. Verificar movimento e idle no runtime e registrar honestamente as lacunas para animação completa: as três poses existentes não são um ciclo animado. Não gerar novas imagens nesta etapa.

O Canvas usa referência **1280×720** (`UiRoot.ReferenceWidth/Height`). Validar também em **960×600**: são proporções distintas (16:9 e 16:10), sem presumir equivalência de layout ou escala. Entregar capturas reais do Unity do card e restaurante em ambas as resoluções, verificando recorte, legibilidade e escala.

Sem redesign completo da Escalação, sistema novo de geração procedural, substituição de todos os sprites ou mudanças de gameplay. **Antes de qualquer merge da ART-02, o responsável deve avaliar e aprovar a primeira integração funcionando no Unity; não armar auto-merge.**

## Escopo mínimo
1. Usar **Suzu**, a identidade aprovada como referência, e produzir ficha curta com `visual_id` e referências reais (não uma ficha por instância aleatória do save).
2. Aplicar o portrait existente adequado ao card e chibi legível na escala real do restaurante. **Mesma identidade** nos dois: silhueta, cabelo, orelhas, cores e acessório marcante.
3. Reusar componentes existentes e integrar à UI/card e apresentação do restaurante em caminho isolado. Inspecionar `Assets/_Game/Scripts/Game/Ui/MaidCard.cs`, `View/MaidSpriteBuilder.cs`, `View/MaidView.cs`, `Art/ShapeRasteriser.cs` antes de projetar outro compositor.
4. Demonstrar ao menos uma **variação isolada** (acessório/cabelo) e que é possível revertê-la sem refazer o restante da personagem.
5. Mostrar chibi em `idle` e `walk` reais; demais poses (`carry/cook/tired`) devem **continuar funcionais** pelo sistema atual ou declarar explicitamente os casos ainda não atendidos, sem alegar implementação total.
6. Apresentar capturas da personagem no card e no restaurante **rodando no Unity**; diferenciar as prévias de design das capturas de integração.

## Não fixar antecipadamente
- Não exigir PNG ou SDF específico, Pillow, prefabs, rig/Unity 2D Animation, ScriptableObject, atlas ou save migration sem experimento demonstrar necessidade.
- Não reescrever o desenho dos outros personagens, não mudar economia/raridade ou serialização de `Neko.Sim`, não adicionar sistema completo de geração em massa.
- Proteger `.meta`, GUIDs, runtime, chaves de cache de sprites, referências, cenas, ferramentas de teste e performance. Se a abordagem requer alteração de arquivos protegidos pelo CI, documentar exceção e não contornar o guard.

## Aceite / decisão seguinte
- Personagem reconhecível entre portrait e chibi **sem ler nome**; card mantém nome/posição/stats/moldura de raridade como UI.
- Chibi legível e movimento/idle verificados no Canvas de referência **1280×720** e também em **960×600**, com limitações de animação declaradas. Variação isolada demonstrada.
- Capturas/curta gravação de runtime + resultado dos testes apropriados/Console, documentando limitações. No caso de ausência de Unity, **não considerar concluído e não armar auto-merge**.
- Responsável aprova ou pede iteração no resultado visual; resultado do experimento orienta futura stack. Registrar decisão técnica apenas quando existir evidência.

**Próxima etapa:** se ART-02 cumprir o gate, levar estilo às telas de UI-02; só considerar expansão de peças depois de demonstrar qualidade e manutenção.
