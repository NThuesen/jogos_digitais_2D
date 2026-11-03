# Regras compartilhadas (REFERÊNCIA — NÃO copiar separadamente)

**Mudança operacional:** todos os prompts executáveis `01–25` e `prompts/variants/` **já trazem suas próprias regras globais**. Para gerar imagem, copie só **um bloco completo** do prompt escolhido, sem colar este arquivo antes. [Tutorial](../TUTORIAL.md).

## Contrato invariável

- Uma geração/um prompt = **um único PNG modular**, salvo com nome exato informado no próprio arquivo; variações têm prompts próprios.
- Anexar os arquivos visuais **reais** indicados no prompt, como arquivos de imagem, não apenas URLs.
- Anexar `art_working/ui_modular/approved/UI-...png` **apenas depois** que tal arquivo realmente existir e for aprovado; não inventar um PNG por causa do nome.
- Café anime 2D ilustrado/cozy + visual novel ornamental; referência histórica ART-01D era pixel art e **não** obriga pixelização.
- União de imagens e lógica na Unity: sem preços, stats, labels, funções ou botões funcionais baked; exceção apenas para logo permanente estático.
- Fundo transparente real, se aplicável, ou matte removível e depois Blender. Preservar miolo preenchido, bordas, sombras e cortes das janelas de portrait.
- 9-slice quando geometria justificar, não deformar motivos detalhados; testar composição no Unity 960×600 / 1280×720.
- GPT pode nomear download automaticamente: **renomear depois de baixar**, conforme [OUTPUT_MANIFEST.md](../OUTPUT_MANIFEST.md).
- Nenhuma imagem aprovada ou importada na Unity automaticamente por existir um prompt.
