# SUZU — próxima tarefa (art probe, não animação)

**Estado:** executada na branch experimental `experiment/suzu-modular-cutout-20261029`, com duas iterações raster e fonte OpenRaster de dois grupos. [Resultado/evidências](README.md#prova-raster--29102026-aguarda-aprovação-estética). Há melhoria material de desenho, mas acabamento pixel art final **FALHOU**; nova captura Unity **NÃO EXECUTADO** (ponte Windows indisponível, sem Coplay). **AGUARDA APROVAÇÃO ESTÉTICA**; nenhum asset final/pipeline foi aprovado. Execução encerrada no limite de duas iterações.

## Leia somente o necessário
- `AGENTS.md` e `docs/agent/UNITY.md` (regras obrigatórias).
- `docs/art/suzu-modular/README.md`, `evidence/static-05-960x600.png` e `source/draw.py` (diagnóstico anterior).
- `docs/art/art-01/D-pixel-cafe/reference/style_anchor_reference.png` e `incoming/D2_chibi_poses.png` (referências, **não** assets finais).

## Objetivo fechado
Obter **uma prova estática mais atraente** de Suzu em escala de jogo: cabeça/cabelo e torso/saia. Primeiro qualidade de desenho, depois modularidade. O resultado deve demonstrar volume, silhueta e detalhes coerentes com o estilo cozy pixel-art anime; não repetir refinamentos cosméticos de paths SVG do experimento reprovado.

## Execução econômica
1. Inspecione branch/status/HEAD e preserve alterações locais. Reuse apenas a captura/montagem isolada do harness; não aprove automaticamente os desenhos anteriores.
2. Verifique as capacidades **reais** de criação de arte disponíveis (incluindo se há imagem generativa acessível). **Não presuma** que Coplay gera imagens; não usar APIs/serviços pagos.
3. Se houver método viável, produza **somente** cabeça/cabelo + torso/saia, em poucos grupos de camadas, com fonte editável; compare com referência e versão 05 na escala real. Faça **no máximo duas iterações** com hipótese visível por iteração.
4. Se não houver método que atinja a qualidade esperada, **pare** e indique exatamente qual arte-fonte/entrada externa é necessária. Não faça uma sexta variante vetorial disfarçada de pixel art.

## Gates e entrega
- Comparação lado a lado: referência (não copiada), versão 05 e nova versão; evidência Unity 960×600, se houver renderização.
- Avalie rosto/cabelo, volume da roupa, coerência da paleta, legibilidade e costuras. Marque `PASSOU TECNICAMENTE / FALHOU / NÃO EXECUTADO` e `AGUARDA APROVAÇÃO ESTÉTICA`.
- Um resumo curto (até 15 linhas), caminhos das evidências, commit e problemas restantes. Só prossiga se houver melhoria **material**, não apenas mais detalhes.

**Fora de escopo:** animação, pernas/rig completo, novas maids, UI, gameplay, save, economia, packages, cenas de produção, alteração de testes/CI, merge e auto-merge. Não criar novos documentos de planejamento ou grandes sistemas. Não declarar gate artístico aprovado pelo agente.
