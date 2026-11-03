# Decisão visual — identidade da maid, chibi ilustrado e futuro portrait

**Estado:** direção de produto **ACEITA** em 03/11/2026; integração do **Walk ilustrado** das nove maids **CONCLUÍDA** nos PRs [#29](https://github.com/NThuesen/jogos_digitais_2D/pull/29) e [#30](https://github.com/NThuesen/jogos_digitais_2D/pull/30). **Portraits ilustrados, visual_id persistente e camadas intercambiáveis NÃO concluídos.**
**Referência vigente:** [CURRENT_VISUAL_DIRECTION.md](CURRENT_VISUAL_DIRECTION.md).
**Contrato conceitual futuro:** [MAID_VISUAL_SPEC.md](MAID_VISUAL_SPEC.md) (**PROPOSED**, não esquema implementado).

## Decisões aceitas

1. A identidade de cada maid deve ser reconhecível tanto no **chibi ilustrado do restaurante** quanto em um **portrait expressivo futuro no Maid Card**: cabelo, olhos, orelhas, cauda, uniforme, acessórios e cores coerentes.
2. As nove identidades do restaurante seguem **anime 2D ilustrado/chibi** com os frames originais do Blender; **não** converter sprites para pixel art, nem regenerá-los para combinar com o cenário anterior.
3. **Card = ilustração + UI dinâmica**: nome, raridade, profissão/posição, atributos e ações não devem ser texto baked em imagem.
4. Escala no restaurante ajustada para visibilidade e enquadramento, preservando detalhes na textura. Menus podem ter uma linguagem visual novel distinta da gameplay. O cenário será redesenhado para combinar com as personagens, não o contrário.
5. Qualquer mudança estrutural de identidade persistente, equipamentos ou composição de camadas exigirá uma tarefa de produto separada; não alterar regras/saves por inferência.

## Implementado hoje

- `MaidAnimCatalog` + `MaidView` + `WalkCycle`: as nove identidades ilustradas caminham em turno real. Processamento offline em `tools/art/maid_walk/maid_walk.py`, preservando cores/alpha suave; detalhes e limitações em [maid-walk/README.md](maid-walk/README.md).
- Resolução por **nome + arquétipo**; novos saves **não garantem** as nove identidades. Equipamento retorna ao renderer procedural. Outras poses reutilizam Walk/rest frame, bandeja e escurecimento; não existem nove conjuntos independentes de Idle, Carry, Cook e Tired.
- `MaidCard.cs` ainda usa composição de UI/procedural, **não um portrait ilustrado exclusivo** de cada uma das nove personagens.
- A compatibilidade visual entre portrait futuro e chibi é objetivo **a validar**, não algo já demonstrado pelo ART-04.

## Em aberto

- Portraits e expressões, integração em cartas e painéis; variantes e peças (cabelo/roupa/acessório) sem perda de fidelidade artística.
- Se/como persistir `visual_id` ou mapear cartas aleatórias a designs, mantendo serialização e comportamento existentes.
- Equipamentos sem fallback procedural, novas animações ilustradas e alterações de câmera/posição relevantes ao futuro cenário.
- Paleta, tipografia, composição dos menus e estilo final do cenário; aprovação humana com capturas reais do Unity.

**Histórico:** a decisão de outubro propunha camadas intercambiáveis completas e uma única maid experimental antes de expandir. A expansão do **Walk** aconteceu com os PRs #29/#30; isso não prova modularidade de camadas nem implementação de portraits. A antiga direção cozy pixel-art está preservada em [COZY_VISUAL_NOVEL_DIRECTION.md](COZY_VISUAL_NOVEL_DIRECTION.md) apenas como histórico.
