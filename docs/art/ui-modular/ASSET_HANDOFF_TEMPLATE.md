# Ficha de aprovação / handoff de asset

Copie esta ficha para cada **asset efetivamente produzido**; não marque PASS sem observação.

```yaml
id: UI-001
name: panel_large
variant: v01
status: RAW # PENDING | RAW | CLEAN | UNITY_QA | APPROVED | INTEGRATED | REWORK | REJECTED
prompt_file: docs/art/ui-modular/prompts/01_PANEL_LARGE.md
prompt_version_commit: ""
reference_attachments:
  - docs/art/art-01/D-pixel-cafe/reference/style_anchor_reference.png
approved_siblings: []
raw_local_path: art_working/ui_modular/raw_gpt/UI-001_panel_large_v01.png
clean_local_path: art_working/ui_modular/blender_clean/UI-001_panel_large_v01.png
approved_repo_path: ""
unity_repo_path: ""
sha256_clean_png: ""
dimensions_px: ""
alpha_real: NOT_RUN
blender_cleanup: ""
nine_slice: NOT_EVALUATED
ppu_or_scale: NOT_EVALUATED
qa_960x600: NOT_RUN
qa_1280x720: NOT_RUN
qa_webgl_memory: NOT_RUN
reviewer_decision: PENDING
notes: ""
```

**Importante:** imagem no `art_working` é local e ignorada; referenciar caminho não torna bytes acessíveis ao GitHub. Um asset aprovado deve entrar no fluxo PR de arte com PNGs limpos, proveniência e `.meta` estáveis após integração Unity.
