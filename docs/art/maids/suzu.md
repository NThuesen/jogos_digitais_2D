# Suzu — experimento ART-02

- `visual_id`: `suzu-d-experiment`; status: em avaliação, aprovação visual da integração pendente.
- Referência: [ART-01D](../art-01/D-pixel-cafe/README.md). Retrato e chibis processados copiados para `Assets/_Game/Resources/Suzu/`; originais e documentação de reprodução preservados.
- Identidade: cabelo menta, orelhas de gato, olhos âmbar, uniforme chocolate e creme, laços e sinos.
- Associação temporária: nome `Suzu` no registro de maid; o jogo sorteia nomes, portanto esse vínculo não é um identificador único e pode alcançar mais de uma maid ou arquétipos distintos. Não altera save nem simulação; requer revisão antes de ampliar o experimento.
- Idle, walk e carry usam poses estáticas separadas; o deslocamento e o bob vêm da apresentação existente, sem ciclo quadro a quadro. Cook e tired usam o sprite procedural anterior. Equipamentos de roupa, cabeça ou sapatos mantêm o fallback procedural; acessório é sobreposto separadamente e pode ser removido ao desequipá-lo.
- Escala inicial: hipótese de 96 px na altura de referência de 720 px, ainda sem avaliação visual nas duas resoluções. Capturas Unity e inspeção inicial registradas em [validação](../ART-02_VALIDATION.md); aprovação visual, escala final e integração de contorno/iluminação continuam pendentes.
- O teste EditMode `DocsTests.NoTextureOrAudioFileWasEverImported` falha porque a exigência antiga de nenhum arquivo de textura importado conflita com este experimento. Não foi modificado nem contornado.
