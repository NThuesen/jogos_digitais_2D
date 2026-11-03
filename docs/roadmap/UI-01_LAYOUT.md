# UI-01 — Estabilização funcional da interface (estratégia B)

**Estado:** **CONCLUÍDO na estratégia B** — [PR #31](https://github.com/NThuesen/jogos_digitais_2D/pull/31) (infraestrutura), [PR #33](https://github.com/NThuesen/jogos_digitais_2D/pull/33) (UI-01A) e [PR #34](https://github.com/NThuesen/jogos_digitais_2D/pull/34) (UI-01B). **18 defeitos corrigidos**, 25 estados de Layout_Check registrados, 15 estados históricos ainda não implementados. O contrato abaixo permanece como histórico de escopo/aceite, não tarefas pendentes. **Responsável técnico:** Codex.
**Direção de arte:** [anime 2D ilustrado vigente](../art/CURRENT_VISUAL_DIRECTION.md). UI-01 **não** redesenha arte nem preserva visual antigo como regra permanente.
**Histórico:** a [spec original](../../.kiro/specs/ui-01-layout/requirements.md), [design](../../.kiro/specs/ui-01-layout/design.md) e [tasks](../../.kiro/specs/ui-01-layout/tasks.md) registram a proposta extensa de 40 estados; não executá-la integralmente sem readequação ao escopo aprovado aqui.

## Entrega A — UI-01 prioritário

1. Partir da `main` **após PR #30**. Preservar a infraestrutura já existente: `ResolutionRig`, `LayoutModel`, `LayoutReport`, `Clicks`, `SessionFactory`, `AuditStates` e `LayoutCheckTests`. Validar a regra R2.1 já escrita antes de completar regras essenciais de **texto, colisões, scroll e cliques**. Ampliar fixtures apenas conforme necessário.
2. Reproduzir defeitos atuais e registrar **antes**; não assumir que as capturas da versão de outubro continuam válidas depois do HUD/câmera do PR #30. Verificar em **960×600 e 1280×720**.
3. Corrigir primeiro **Loja (Móveis, Roupas, Presentes)**, **Balanço** e **Elenco**: sem clipping, sobreposição, botões obstruídos ou listas inacessíveis. Manter valores e ações reais do `Neko.Sim`. Implementar `UiKit.Panel` 9-sliced **somente se o defeito C1 realmente se reproduzir**; revisar a repercussão em todas as telas afetadas.
4. Entregar PR pequeno(s), capturas antes/depois, defect log e resultados reais de testes. O foco é **estabilidade**, não nova paleta, portraits ou mobiliário.

## Entrega B — UI-01 regressão das outras telas

Verificar **Empresa, Escalação, Caixas, Menu, Salão, O dia, Dialogs e Falência** nas duas resoluções. Corrigir somente falhas relevantes de legibilidade, navegação, scroll e interação; documentar estados não testados.

- Preservar `CameraRig.FrameRoomBetweenHud` e o ajuste dos cartões do HUD do [PR #30](https://github.com/NThuesen/jogos_digitais_2D/pull/30). Não repetir soluções baseadas na câmera anterior.
- **Salão:** nenhuma UI pode bloquear clique em tiles acessíveis. A coluna vertical de estoque de 128 unidades é **opção histórica/temporária**, não layout aprovado do futuro restaurante. Escolher a menor correção funcional justificada por captura.
- **Escalação parcialmente vazia** é estado válido, não bug. O turno especial com nove identidades pertence à QA, não ao recrutamento normal.
- Concluir e ampliar os **40 Audit_States progressivamente**, sem exigir todos como gate de UI-01A. Listar cobertura real de cada etapa, sem afirmar auditoria completa.

## Invariantes e aceite

- Não alterar regras, dinheiro, recrutamento, raridade, RNG, save, `Neko.Sim` ou entrada da simulação. Manter cliques e resultados de interface corretos.
- Tratar paleta, fonte, layout dos Maid Cards e cena atual como **provisórios**. O UI-02 e o futuro cenário podem substituí-los com aprovação visual separada.
- Proteger `.meta`, cenas, `Packages/`, `ProjectSettings/`, CI e testes; alterações nos arquivos existentes de Layout_Check do PR #31 exigem revisão de manutenção e exceção administrativa específica, **não** desativação do guard.
- Validar Unity EditMode/PlayMode e capturas comparáveis a **960×600 / 1280×720**; WebGL quando relevante. Relatar `PASS/FAIL/NOT RUN`; não confundir sim CI com aprovação visual.
- As duas entregas podem resultar em PRs distintos; não presumir número de PR nem fazer merge de mudanças visuais ainda não aprovadas.

**Depois:** [redesign de restaurante e UI-02](../ROADMAP.md), sem necessidade de polir a aparência marrom atual.
