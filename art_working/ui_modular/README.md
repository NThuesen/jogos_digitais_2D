# Pastas locais para imagens geradas — Neko Maid Inc.

**Primeiro leia:** [tutorial com anexos reais](../../docs/art/ui-modular/TUTORIAL.md) e [manifesto dos 95 nomes fixos](../../docs/art/ui-modular/OUTPUT_MANIFEST.md).

```text
art_working/ui_modular/
  raw_gpt/        # original baixado/renomeado (PNG ignorado pelo Git)
  blender_clean/  # PNG após limpeza de fundo (ignorado)
  approved/       # aprovado pelo responsável, ainda LOCAL (ignorado)
  export_unity/   # export para testes Unity (ignorado)
```

**Nomes reais de exemplos do piloto:**

- `raw_gpt/UI-001_panel_large_v01.png`
- `raw_gpt/UI-004_button_primary_normal_v01.png`
- `raw_gpt/UI-004_button_primary_hover_v01.png`
- `raw_gpt/UI-004_button_primary_pressed_v01.png`
- `raw_gpt/UI-004_button_primary_disabled_v01.png`
- `raw_gpt/UI-008_maid_card_compact_v01.png`

**Estes são DESTINOS, não PNGs já presentes.** O Git versiona os `.gitkeep`; os arquivos gerados ficam locais por política `art_working/.gitignore`. Não fazer `git add -f` nos brutos. Caso o ChatGPT baixe `image.png`, renomeie para o nome exato do prompt após baixar. Em `blender_clean/` ou `approved/` use o MESMO nome base. Repetiu a geração? `_v02.png`, sem sobrescrever a v01.

Somente PR curado e aprovado leva PNG final para `docs/art/ui-modular/approved/` ou `Assets/_Game/Resources/` com proveniência. Outros worktrees **não têm acesso** aos PNGs ignorados.
