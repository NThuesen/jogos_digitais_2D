# MaidVisualSpec — contrato conceitual (proposto, não implementado)

**Read when:** gerar/modificar uma maid, adicionar módulos visuais ou implementar o compositor.
**Decisão de produto:** [MAID_VISUAL_DECISION.md](MAID_VISUAL_DECISION.md) (**ACCEPTED**).
**Ficha de identidade:** [maids/_TEMPLATE.md](maids/_TEMPLATE.md).
**Status deste contrato:** PROPOSED — nomes de campos e tecnologia ainda sujeitos a POC. Não confundir com classe C# ou asset existente.

## Uma fonte de identidade, duas saídas

`visual_id` identifica um **design visual estável**, não uma instância sorteada no save. Uma maid do jogador usa seus próprios dados de `Neko.Sim` (nome, arquétipo, raridade, equipamentos e atributos); a apresentação resolve a spec visual e as peças selecionadas **sem copiar esses dados para uma segunda fonte de verdade**. O mesmo conjunto de escolhas visuais alimenta:

- `portrait`: busto/meio corpo expressivo; prioridade: rosto, ornamentos, coesão com o card.
- `chibi`: personagem compacto no mapa; prioridade: silhueta, leitura, movimento, animação.

A associação entre `visual_id`, seed/variações e `Maid` ainda precisa ser desenhada e testada; não alterar save ou `Neko.Sim` antecipadamente.

## Campos de uma ficha por identidade visual

| Grupo | Campos propostos | Responsabilidade |
| --- | --- | --- |
| Identificação | `visual_id`, `status`, `style_anchor`, `archetype_hint` | localizar/avaliar uma identidade; archetype é referência visual, não regra |
| Identidade compartilhada | `skin_tone`, `hair_style`, `hair_color`, `eye_shape`, `eye_color`, `ear_style`, `tail_style`, `signature_accessory`, `uniform_theme` | aparência consistente nas duas saídas |
| Portrait | `pose`, `expression`, `portrait_layers` | composição de imagem para o card |
| Chibi | `idle`, `walk`, `carry`, `cook`, `tired`, `chibi_layers` | apresentação no restaurante e estados reutilizáveis |
| Produção | `asset_refs`, `review_previews`, `approval` | proveniência, rastreio e aceite estético |

Estes campos são um **vocabulário inicial para documentação e experimentos**; não são uma exigência de criar exatamente esse `ScriptableObject`/JSON/YAML.

## Catálogo de camadas a explorar

A ordem abaixo é **sugestão de composição**, não contrato irrevogável. Peças opcionais têm fallback claro. Variações de roupa/equipamento devem refletir dados já existentes de `Neko.Sim`.

| Portrait / Card (camadas da ilustração) | Chibi / Restaurante (camadas do sprite) |
| --- | --- |
| cabelo traseiro | sombra de contato |
| cauda / elementos traseiros | pernas / sapatos |
| corpo / pele | vestido / avental / corpo |
| uniforme / avental | braços / mangas |
| pescoço / cabeça / base do rosto | cauda |
| orelhas felinas | cabelo traseiro |
| olhos e sobrancelhas | cabeça / pele |
| boca e expressão | orelhas felinas |
| cabelo frontal / franja | franja / cabelo frontal |
| acessório de cabeça / pescoço | olhos / boca (expressão simples) |
| iluminação / realce opcional | acessório distintivo |
| *Moldura, textos, raridade e números são UI, fora do portrait* | item carregado/bandeja ligado ao estado, fora da identidade permanente |

Partes devem compartilhar referência de alinhamento dentro de cada representação. **Portrait e chibi não precisam ter o mesmo canvas ou rig**, mas devem conservar identidade (cor, silhueta, acessório). Não pressupor que todo PNG gerado terá recorte ou transparência utilizável: validar importação, recorte, sobreposição e escala.

## Regras para o agente
1. Consulte apenas a ficha visual afetada e os módulos realmente usados; não recalcule/regenere o elenco inteiro para trocar um acessório.
2. Se uma ficha ainda não tem arte aprovada, gere uma **prévia** e registre `pending`, nunca assuma aprovação.
3. Novos arquivos de arte, camadas e renderers são **futuros**, não existem automaticamente nesta especificação.
4. Ao implementar, confrontar `MaidSpriteBuilder.cs`, `MaidCard.cs`, `MaidView.cs` com os objetivos, preservando o fluxo atual até a substituição ter evidência.
5. A estética precisa de aprovação humana do **resultado visível**; o trabalho de desenho, geração, recorte, animação e código continua com agentes.
