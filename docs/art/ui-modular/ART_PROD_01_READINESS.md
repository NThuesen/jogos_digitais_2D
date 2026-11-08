# ART-PROD-01 — Prontidão para produção

## Pode começar: pratos piloto D0

[27_DISH_ICONS.md](prompts/27_DISH_ICONS.md): milk, omurice e parfait.
Anexar `docs/art/ui-modular/approved/UI-011_icon_cup_v01.png` como acabamento,
não como objeto. RAW, master aprovado e export conferem com os hashes B3.
Master inspecionado visualmente: contorno marrom, acabamento ilustrado, paleta quente.
A aprovação histórica não homologa automaticamente novos ícones a 16 px.
O bloco exige leitura principal 20–25 px e inspeção a 16 px; isso é critério futuro,
não teste de ícones ainda inexistentes. Não gerar derivados antes do aceite do master.
Os outros 11 prompts estão especificados, mas aguardam o padrão aprovado em D0.

## Clientes: texto preparado, referências de identidade pendentes

[26_CUSTOMER_CHIBIS.md](prompts/26_CUSTOMER_CHIBIS.md): seated e walk-start humanos
prontos como contrato; Regular A NÃO liberado para geração enquanto seu original
aprovado não for localizado e conferido. Não regenerar base aprovada.
Student também não encontrado nos diretórios-alvo; não presumir aprovação/ausência global.
Demais bases dependem de referência chibi aprovada identificada por arquivo/hash.
O sheet Hina integrado é candidato de acabamento, não referência de identidade;
a escolha de quadro individual ainda depende de confirmação artística.
Flow depende de walk-start aprovado. C1–C3 dependem do piloto integrado.

[26_CUSTOMER_CAT_SPECIAL.md](prompts/26_CUSTOMER_CAT_SPECIAL.md): quatro contratos
específicos preparados (base, seated felino, still quadrúpede, vídeo quadrúpede).
Não usar o prompt humano. Produção aguarda referência de acabamento e ordem C0.

## Móveis: não liberados ainda

[24_FURNITURE_RENDER_CONTRACT.md](prompts/24_FURNITURE_RENDER_CONTRACT.md) e os oito
prompts individuais têm contrato técnico, porém aguardam capturas atuais e calibração
no Salão. table_2, teabar e bakery requerem teste de ambas as orientações da grade;
`_rot90` identifica arte alternativa, não autoriza girar o bitmap 90 graus.
Sem aprovação de table_2/stove não escalar os demais seis.

## Validação desta revisão

Inventário: 83 fontes PNG locais, dimensões e SHA-256; nenhuma sobrescrita.
Status artístico histórico separado da existência e de integração.
Cat: anatomia quadrúpede e pose felina explícitas; casos Cat removidos dos blocos humanos.
Pratos: 14 IDs/descrições/cores preservados; variantes premium e bebidas diferenciadas.
Capturas Unity e QA visual a tamanho real: não executados; dependência documentada.
Não houve geração, derivados, integração, alteração de gameplay ou merge.
