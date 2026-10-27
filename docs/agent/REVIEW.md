# Revisão adversarial pontual — Claude Code ↔ Codex

**Read when:** responsável solicita revisão da implementação Codex ou análise de findings. **Não é revisão obrigatória de todo PR nem um agente observando CI.** Regras de branch, PR e segurança: [AGENTS.md](../../AGENTS.md).

## Evidências de entrada
- PR/diff no **HEAD atual**, Issue/escopo e critérios de aceite.
- Arquivos alterados + chamadas externas e limites de arquitetura pertinentes; screenshots/gravação Unity só quando de fato fornecidos ou obtidos da execução.
- Resultado dos testes com comando, ambiente, exit code; `not run` não equivale a `passed`.
- Nas alterações visuais, a nota da tarefa em [roadmap](../ROADMAP.md) e referências **aprovadas** (não mockups assumidos como arte integrada).

## O que o revisor deve devolver
Para **cada finding**, indicar:
1. **Severity:** `blocking` (quebra/crash/dados/CI ou requisito inviável), `important` (bug/regressão relevante ou acceptance criteria ausente), `optional` (melhoria que pode esperar); comentários puramente estilísticos não são gate.
2. **Status factual:** comprovado pela implementação/evidência, provável (como reproduzir) ou hipótese não reproduzida.
3. **Local/referência:** caminho do arquivo/trecho, estado de cena ou screenshot. Comportamento esperado vs observado.
4. **Como verificar a correção** sem aumentar testes artificialmente nem trocar expectativa para deixá-los verdes.

Se não houver finding comprovado, relatar limites da revisão em vez de fabricar problemas. **Nunca inferir bug em estado permitido de gameplay**: uma estação vazia na Escalação com maid no banco é intencional, conforme decisão do responsável.

## Devolução e fechamento
- Codex avalia cada finding: válido / parcial / inválido / decisão de produto pendente. Corrige somente os válidos e o escopo aceito; decisões materiais sobem ao responsável.
- Codex reexecuta verificações pertinentes e registra diferença observável; se mudou HEAD, não reutilizar aprovação/review antigo como evidência.
- Claude Code pode fazer nova revisão pontual da correção se solicitado; **não criar loop de agentes** nem monitoramento contínuo.
- GitHub Actions valida apenas checks declarados; arte, UI/PlayMode e WebGL precisam de evidência real. A PR só pode usar auto-merge de rotina se não for mudança protegida e tiver validação necessária.
