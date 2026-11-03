# Produção local de arte modular

**Canônico:** [tutorial](../../docs/art/ui-modular/TUTORIAL.md) · [prompts](../../docs/art/ui-modular/prompts/) · [registro](../../docs/art/ui-modular/ASSET_REGISTRY.md).

As pastas abaixo foram **criadas no Git**, mas os PNGs/`.blend` locais **são ignorados** por `art_working/.gitignore`, conforme a política existente da produção das maids.

```text
art_working/ui_modular/
  raw_gpt/       # originais da geração (NÃO sobrescrever; v01, v02...)
  blender_clean/ # fundo/alpha corrigidos; comparar com original
  approved/      # cópias locais aprovadas antes de PR
  export_unity/  # exportação local para testes Unity
```

Nomenclatura: `UI-001_panel_large_v01.png` ou `UI-004_button_primary_hover_v01.png`. O asset aprovado só passa a ser compartilhado quando um PR curado adiciona seus bytes ao repositório. Não mover automaticamente brutos para `Assets/_Game/Resources/`.
