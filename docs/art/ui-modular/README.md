# Kit modular de arte — Neko Maid Inc.

**Contrato vigente:** ChatGPT gera **imagens modulares**; Blender pode limpar fundo; Unity mantém controles, dados, textos, estados, cliques e responsive layout. **Não gerar uma tela inteira como UI clicável.** Estilo geral: Cozy Illustrated Anime Café + Ornamental Visual Novel; a paleta/materialidade final ainda depende de aprovação. [Fonte de verdade](../CURRENT_VISUAL_DIRECTION.md).

## Use nesta ordem

1. **[Tutorial passo a passo (com arquivos PNG reais)](TUTORIAL.md)** — instruções para anexar, copiar e salvar, incluindo o exemplo dos 4 botões.
2. **[Índice de arquivos já existentes no repo](references/README.md)** — nomes exatos das referências e capturas.
3. **[Ordem de geração e gates](GENERATION_ORDER.md)** — piloto 01 painel → 04 botão normal e variações → 08 Maid Card.
4. **[100 saídas nomeadas](OUTPUT_MANIFEST.md)** — nome fixo de cada PNG, link para o prompt completo correspondente. O manifesto reserva nomes, não arquivos gerados. Consulte o registro para distinguir UI-001 APPROVED, UI-004 ornamental REWORK e aprovações vigentes dos seis assets no registro.
5. **[Prompts prontos](prompts/)** — cada prompt individual tem links diretos aos PNGs e UM bloco autossuficiente, **sem campos a substituir**.
6. **[Registro de aprovação](ASSET_REGISTRY.md)** e [ficha](ASSET_HANDOFF_TEMPLATE.md) — histórico de validação da arte real.

### Comece agora por estas três famílias

- [01 — painel grande, um prompt](prompts/01_PANEL_LARGE.md), saída `UI-001_panel_large_v01.png`.
- [04 — botão primário, duas famílias](prompts/04_BUTTON_PRIMARY.md), incluindo os quatro prompts ornamentais históricos e os quatro compactos (NORMAL e novos HOVER/PRESSED/DISABLED); nomes exatos clicáveis no índice.
- [08 — Maid Card existente](prompts/08_MAID_CARD_COMPACT.md), `UI-008_maid_card_compact_v01.png` (APPROVED somente detalhado 150×206), e [novo frame simplificado de Escalação](prompts/variants/08_MAID_CARD_LINEUP_MINIMAL.md), `UI-008_maid_card_lineup_minimal_v01.png` (gerado/UNITY_QA; aceite artístico PENDING).

### Decisão de produto UI-004 — duas famílias (03/11/2026)### Decisão de produto — duas apresentações de Maid Card (04/11/2026)

- **Escalação a 0,62×:** gerar [**UI-008 LINEUP MINIMAL**](prompts/variants/08_MAID_CARD_LINEUP_MINIMAL.md), `UI-008_maid_card_lineup_minimal_v01.png`, com janela transparente de portrait e faixa simples de nome. [Tutorial com anexos exatos](TUTORIAL.md#passo-3b--novo-maid-card-minimalista-para-escalação-duas-apresentações-aprovadas). **PNG/CLEAN gerados, UNITY_QA; aprovação artística PENDING; não integrado.**
- **Card mais detalhado:** preservar `UI-008_maid_card_compact_v01.png` já gerado e limpo localmente, aceito visualmente apenas em 150×206 pelo responsável após o QA da PR #44; **REWORK a 0,62×**. O UI-009 expandido é uma proposta futura, não um pré-requisito. A decisão de produto não aprova a nova LINEUP MINIMAL; o aceite do frame existente é uma decisão artística expressa posterior.
- A mesma maid/dados/portrait são reutilizados em ambas as apresentações; Unity controla nome, indicadores e informações adicionais. Nenhuma cena ou lógica alterada nesta documentação.



- **Ornamental grande (~260×88):** linguagem rica para CTA de destaque. O `UI-004_button_primary_normal_v01.png` original foi gerado localmente e avaliado na [PR #40](https://github.com/NThuesen/jogos_digitais_2D/pull/40), mas segue `REWORK`, sem arte aprovada/integrada; 768×256 e WebGL continuam candidatos `NOT RUN`.
- **Compacta simplificada (altura 44–54):** para botões de compra/navegação, com foco no botão real da Loja `150×44`, `120×44` e eventual `230×54`. **NORMAL compacto já gerado:** QA na PR #42, aprovado pelo responsável; a aprovação já foi registrada na PR #42 integrada; a integração Unity é outra etapa. **Estados com QA e aprovação expressa:** [HOVER v02, PRESSED v01 e DISABLED v02 compactos](TUTORIAL.md#passo-2b--gerar-hover-pressed-e-disabled-do-botão-compacto-aprovado), com NORMAL limpo como referência exata. Os três estados agora APPROVED, sem integração; HOVER v01 e DISABLED v01 continuam REWORK.
- **Abas 210×44:** continuam peças próprias, não se transformam no botão ornamental.
- A divisão **está aprovada como direção de produto**, não como aprovação de qualquer PNG. O NORMAL compacto já foi revisado pelo responsável; os demais estados exigem suas próprias evidências e aprovação antes de integração.

**ATENÇÃO:** `00_GLOBAL_RULES.md` é agora apenas referência técnica. Os prompts já contêm as regras; basta **copiar um único bloco** do arquivo individual.

## Pastas existentes e finalidade

```text
art_working/ui_modular/
  raw_gpt/          # PNG baixado com o nome exato após renomeação (IGNORADO NO GIT)
  blender_clean/    # PNG com alpha tratado (IGNORADO)
  approved/         # cópia local após aprovação (IGNORADO)
  export_unity/     # candidato para testes (IGNORADO)
docs/art/ui-modular/
  prompts/          # 25 entradas: 12 prompts individuais + 13 índices
    variants/       # 87 prompts individuais e específicos para variações
  references/       # lista de REFERÊNCIAS reais (sem duplicar PNGs)
  incoming/         # PR específico para candidatos selecionados
  approved/         # PR de arte aprovada
  OUTPUT_MANIFEST.md
```

Essas pastas usam `.gitkeep`. **Os prompts não geram PNGs automaticamente, e os caminhos `approved/UI-...` só passam a existir após aprovação humana.** A imagem gerada pode baixar com nome aleatório; o tutorial mostra como renomeá-la. Consulte [contrato](../CURRENT_VISUAL_DIRECTION.md) e [UI-02](../../roadmap/UI-02_SCREEN_POLISH.md).

**Publicados:** UI-001 e os seis masters/exports aprovados em 04/11/2026; [arquivos, hashes, proveniência e limites](approved/README.md). COMPACT NORMAL continua local, aprovado e não integrado.

**Referência ART-01D:** preservar aconchego, molduras e visual novel; **não** reproduzir pixel art nem copiar personagens. Cenário e menus podem diferir em ornamentação, mantendo linguagem comum. O piloto deve passar nas resoluções 960×600 e 1280×720 antes de produção em escala.

## Encerramento técnico e revisão artística (04/11/2026)

- [Índice visual dos 12 PNGs individuais, exports testados e RAWs REWORK](incoming/BATCH-2026-11-04/README.md).
- [Handoff final para Claude Code](HANDOFF_CLAUDE_CODE_2026_11_04.md): status das PRs #46/#47/#48, caminhos locais, hashes, limitações e próximas tarefas.
- #47 aguarda revisão artística, sem auto-merge e sem integração. Os nove PASS técnicos não são APPROVED. UI-007/UI-009/UI-022 v01 continuam REWORK; v02 PENDING seguem os prompts já corrigidos na #48.
