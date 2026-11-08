# ART-PROD-01 — Produção e QA

Preparação documental em 08/11/2026; baseline main 933673c, pós-PR #73.
Esta revisão não gera imagens, processa fontes, captura runtime ou integra assets.
Não alterar Neko.Sim, economia, atendimento, receitas, saves ou progressão.
Integração futura autorizada separadamente, exclusivamente na apresentação.

## Inventário e proveniência — G0 pendente

Inspecionar art_working/ui_modular/raw_gpt/ e art_working/customers/, inclusive
ignorados. Para cada arquivo registrar caminho, nome, dimensões, SHA-256,
referência/versão do prompt, origem/ferramenta, data e relação master→derivado.
Comparar Regular A e Student antes de gerar bases. Não sobrescrever originais.
Capturar Salão pós-PR #73 nas duas resoluções, com commit, data e contexto.
Identificar referências aprovadas: maid para acabamento, UI-011 cup para pratos,
Salão e objetos atuais para móveis. Não usar pixel art como acabamento.

Registrar separadamente existência (encontrado/ausente/não verificado), arte
(RAW/PENDING/REWORK/APPROVED), técnico (pendente/pass/fail), derivados e integração
(não integrada/INTEGRATED com PR). Aprovação exige responsável, data e evidência;
não deduzir aprovação pelo nome ou por PASS técnico.

## Gates por asset

| Gate | Evidência necessária |
|---|---|
| G1 PROMPT_READY | Referências identificadas, especificação sem conflito, saída definida |
| G2 RAW_READY | Fonte preservada, hash e versão, sem sobrescrita |
| G3 ART_APPROVED | Aceite artístico explícito do responsável |
| G4 TECH_PASS | Alpha, dimensões, margens, pivô, legibilidade ou frames/loop |
| G5 UNITY_PASS | Composição, escala, sorting e cliques em 960×600 e 1280×720 |
| G6 WEBGL_PASS | Chrome real, erros, clipping, import, download e memória |
| G7 INTEGRATED | PR legitimamente mergeada, proveniência e evidências |

G0 exige inventário completo e referências versionadas. Nenhum gate é declarado
concluído por esta preparação. Não produzir derivados em massa antes de G3.

## Execução futura e limites

D0 milk/omurice/parfait → Regular A completo → table_2/stove → demais pratos →
demais móveis → humanos C1–C3 → Cat separado. Reutilizar fontes aprovadas.
Máximo inicial: 45 PNGs e 8 MP4s; até 3 orientações adicionais de móveis.
Nova tentativa preserva versão anterior; revisão V02 de prompt não renomeia
automaticamente imagem ainda inexistente. Aprovar família pequena antes de escalar.

Pratos: inspecionar 20–25 px e 16 px, alpha sobre fundos claros/escuros, IDs,
variantes premium e ausência de texto. Derivar 128×128 só após master aprovado.
Clientes: identidade nas três poses, pés/contatos, loop sem salto, sem chroma,
sem cortes; usuário seleciona intervalo; guardar MP4 e frames numerados.
Móveis: encaixe, pivô, orientações, assentos/oclusão e indicadores do fogão.

Integração C0: fila→mesa→pedido→saída, cliques da mão do dono, grupo ×N,
sem teleporte, mesmos estados/durações. Só depois escalar clientes; testar tipos
nos desbloqueios reais. Comparar performance/download/memória na mesma cena,
resolução e navegador com baseline identificada; registrar números e desvios,
sem presumir orçamento ainda não medido. Consolidar WebGL por integração relevante.

GPT+usuário aprovam arte; usuário+Flow/Blender selecionam vídeos/intervalos;
Claude integra View e verifica runtime; Codex apoia processamento/QA sem duplicação.
Uma branch/PR por lote relevante. Não alterar testes, allowlist ou CI para liberar
esta documentação. Checks documentais não comprovam Unity/WebGL.
