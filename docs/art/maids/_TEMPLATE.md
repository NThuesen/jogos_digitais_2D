# TEMPLATE — identidade visual de maid (não é uma personagem aprovada)

**Quando usar:** só ao criar ou revisar um *design visual* específico. Copie para `docs/art/maids/<visual_id>.md`; substitua placeholders, não registre uma ficha por carta aleatória.
**Contrato:** [MaidVisualSpec](../MAID_VISUAL_SPEC.md) (PROPOSED).
**Decisão:** [Portrait + Chibi](../MAID_VISUAL_DECISION.md) (ACCEPTED).

## Identificação e revisão
- `visual_id`: `<id-estavel-e-unico>`
- `status`: `draft | preview_ready | approved | rejected`
- `archetype_hint`: `<referência visual, não atributos de gameplay>`
- `style_anchor`: `<caminho/ID da referência estética aprovada ou pending>`
- `review_previews`: `<links/caminhos para card real e chibi no cenário>`
- `approval`: `pending | approved` — `<data/notas de avaliação>`

## Identidade compartilhada
| Elemento | Variante / parâmetro | Ref. de peça (quando existir) |
| --- | --- | --- |
| Pele | `<skin_tone>` | `<id/pendente>` |
| Cabelo (silhueta + cor) | `<hair_style / hair_color>` | `<id/pendente>` |
| Olhos (forma + cor) | `<eye_shape / eye_color>` | `<id/pendente>` |
| Orelhas | `<ear_style>` | `<id/pendente>` |
| Cauda | `<tail_style>` | `<id/pendente>` |
| Uniforme/avental | `<uniform_theme>` | `<id/pendente>` |
| Acessório assinatura | `<signature_accessory>` | `<id/pendente>` |

## Portrait para card
- `pose`: `<id/pendente>`
- `expression`: `<id/pendente>`
- `portrait_layers`: cabelo traseiro; elementos traseiros/cauda; corpo/pele; uniforme/avental; cabeça; orelhas; olhos/sobrancelhas; boca; franja; acessórios; realces opcionais.
- `asset_refs`: `<caminhos somente após arquivos serem produzidos>`
- **Não incluir na imagem**: nome, stats, posição ou moldura de raridade (pertencem à UI).

## Chibi no restaurante
- `chibi_layers`: sombra; pernas/sapatos; corpo/avental; braços; cauda; cabelo traseiro; cabeça; orelhas; franja; olhos/boca; acessório.
- Estados `idle/walk/carry/cook/tired`: `<ids de animação/pose ou pending>`
- Item carregado/bandeja: derivado da atividade, **não** parte permanente da identidade.
- `asset_refs`: `<caminhos somente após arquivos serem produzidos>`

## Critérios de aceite desta identidade
- [ ] Retrato e chibi reconhecíveis como a mesma maid sem ler nome.
- [ ] Chibi legível em escala real no restaurante e durante a animação.
- [ ] Card visualmente expressivo, sem prejudicar legibilidade da UI/raridade.
- [ ] Trocar uma peça não exige regenerar as demais.
- [ ] Prévia no Unity e aprovação estética registradas; **não marcar sem evidência**.

**Nota:** nomes/formatos de camadas são proposta para testar a decisão visual; não significam que `MaidVisualSpec` ou biblioteca de PNGs já tenham sido implementados.
