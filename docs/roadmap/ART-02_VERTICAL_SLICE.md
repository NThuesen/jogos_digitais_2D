# ART-02 — Uma maid completa no Unity (vertical slice visual)

**Status:** BLOCKED até **ART-01 aprovada**. **Agente implementador recomendado:** Codex.
**Leia:** [estilo aprovado/artwork ART-01](ART-01_STYLE.md), [decisão](../art/MAID_VISUAL_DECISION.md), [spec](../art/MAID_VISUAL_SPEC.md), [template da identidade](../art/maids/_TEMPLATE.md), [Unity](../agent/UNITY.md). Consultar GDD apenas para arquétipo/raridade quando necessário.

## Hipótese
Uma única identidade visual pode sustentar **portrait atraente + chibi fofo animado**, ser reconhecida no card e restaurante e permitir troca pontual de componente sem regeneração completa. Validar isso **antes** de fixar pipeline e biblioteca.

## Escopo mínimo
1. Escolher **uma** identidade de maid conforme estilo aprovado e produzir ficha curta com `visual_id` e referências reais (não uma ficha por instância aleatória do save).
2. Produzir/aplicar portrait adequado ao card e chibi legível na escala real do restaurante. **Mesma identidade** nos dois: silhueta, cabelo, orelhas, cores e acessório marcante.
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
- Chibi legível, fofo e animado em resolução de referência 960×600. Variação modular demonstrada.
- Capturas/curta gravação de runtime + resultado dos testes apropriados/Console, documentando limitações. No caso de ausência de Unity, **não considerar concluído e não armar auto-merge**.
- Responsável aprova ou pede iteração no resultado visual; resultado do experimento orienta futura stack. Registrar decisão técnica apenas quando existir evidência.

**Próxima etapa:** se ART-02 cumprir o gate, levar estilo às telas de UI-02; só considerar expansão de peças depois de demonstrar qualidade e manutenção.
