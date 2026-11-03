# Kit modular de arte — Neko Maid Inc.

**Contrato vigente:** ChatGPT gera **imagens modulares**; Blender pode limpar fundo; Unity mantém controles, dados, textos, estados, cliques e responsive layout. **Não gerar uma tela inteira como UI clicável.** Estilo geral: Cozy Illustrated Anime Café + Ornamental Visual Novel; a paleta/materialidade final ainda depende de aprovação. [Fonte de verdade](../CURRENT_VISUAL_DIRECTION.md).

## Use nesta ordem

1. **[Tutorial passo a passo (com arquivos PNG reais)](TUTORIAL.md)** — instruções para anexar, copiar e salvar, incluindo o exemplo dos 4 botões.
2. **[Índice de arquivos já existentes no repo](references/README.md)** — nomes exatos das referências e capturas.
3. **[Ordem de geração e gates](GENERATION_ORDER.md)** — piloto 01 painel → 04 botão normal e variações → 08 Maid Card.
4. **[95 saídas nomeadas](OUTPUT_MANIFEST.md)** — nome fixo de cada PNG, link para o prompt completo correspondente. **Todos os assets novos estão PENDING**; a lista reserva nomes, não arquivos.
5. **[Prompts prontos](prompts/)** — cada prompt individual tem links diretos aos PNGs e UM bloco autossuficiente, **sem campos a substituir**.
6. **[Registro de aprovação](ASSET_REGISTRY.md)** e [ficha](ASSET_HANDOFF_TEMPLATE.md) — histórico de validação da arte real.

### Comece agora por estas três famílias

- [01 — painel grande, um prompt](prompts/01_PANEL_LARGE.md), saída `UI-001_panel_large_v01.png`.
- [04 — botão primário, quatro prompts independentes](prompts/04_BUTTON_PRIMARY.md), saídas `UI-004_button_primary_{normal,hover,pressed,disabled}_v01.png` (nomes completos clicáveis no índice).
- [08 — Maid Card compacto, um prompt](prompts/08_MAID_CARD_COMPACT.md), saída `UI-008_maid_card_compact_v01.png`.

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
    variants/       # 83 prompts individuais e específicos para variações
  references/       # lista de REFERÊNCIAS reais (sem duplicar PNGs)
  incoming/         # PR específico para candidatos selecionados
  approved/         # PR de arte aprovada
  OUTPUT_MANIFEST.md
```

Essas pastas usam `.gitkeep`. **Os PNGs ainda não foram gerados automaticamente, e os caminhos `approved/UI-...` só passam a existir após aprovação humana.** A imagem gerada pode baixar com nome aleatório; o tutorial mostra como renomeá-la. Consulte [contrato](../CURRENT_VISUAL_DIRECTION.md) e [UI-02](../../roadmap/UI-02_SCREEN_POLISH.md).

**Referência ART-01D:** preservar aconchego, molduras e visual novel; **não** reproduzir pixel art nem copiar personagens. Cenário e menus podem diferir em ornamentação, mantendo linguagem comum. O piloto deve passar nas resoluções 960×600 e 1280×720 antes de produção em escala.
