## Tarefa
Relacionado a: #

## Entrega
- **O que mudou e por quê:**
- **Critérios de aceitação satisfeitos:**
- **Decisão nova de produto/arquitetura:** nenhuma / link do requisito.

## Evidências reais
- **Neko.Sim:** executado e passou / falhou / não executado (motivo)
- **Unity EditMode e PlayMode:** executado e passou / falhou / não executado (motivo)
- **WebGL, jogo visual, cena e áudio:** validado / não se aplica / não validado (motivo)
- **Casos limite e regressões verificados:**
- **Documentos alterados se comportamento mudou:** sim / não se aplica

## Riscos e integração
- **Arquivos sensíveis (tests/CI/Unity config/scenes):** não / sim (não usar auto-merge normal)
- **Dependências de outros PRs ou alterações concorrentes:**
- **O que permanece sem validação:**

## Integração autônoma
O agente autor analisa o diff, garante que os campos acima correspondem a fatos e pode
solicitar **uma vez** auto-merge nativo, **somente** quando o ruleset da main exigir
`simulation-and-boundaries` e `protected-change-guard` e permitir 0 aprovações.
O GitHub aguarda os checks. Nenhum agente acompanha logs por polling.
**Um check verde da simulação não certifica Unity, PlayMode nem WebGL.**
Não enfraquecer testes ou scripts de validação para obter aprovação artificial.
