# Área de trabalho local — frames exportados pelo Blender

**Ponto de entrada:** depois de criar a maid no ChatGPT (Etapa 1), gerar a imagem de caminhada em chroma (Etapa 2), e criar o vídeo no Tencent (Etapa 3), **salve as imagens quadro a quadro exportadas do Blender aqui (Etapa 4)**.

Esta pasta está **fora de `Assets/` do Unity** deliberadamente: frames brutos são material de produção, não sprites aprovados ou importados no jogo. Todos os diretórios de `frames/` já foram criados no repositório com arquivos `.gitkeep`. **Os PNGs e vídeos brutos são ignorados pelo Git por padrão** (ver [`.gitignore`](.gitignore)), para não poluir GitHub com milhares de arquivos; eles ficam apenas no seu computador/worktree até um fluxo explícito de revisão e integração. **Agentes em outros worktrees/máquinas não verão os PNGs ignorados.** Não tratar a criação destas pastas como inclusão de assets no jogo.

## Caminho exato para exportar do Blender

Para a Suzu andando horizontalmente na tela:

```text
<RAIZ_DO_REPOSITORIO>/art_working/01_suzu/step4_blender/walk_x/frames/
```

Outras maids usam o mesmo padrão:

```text
art_working/
  README.md
  .gitignore
  01_suzu/
    step4_blender/
      walk_x/
        frames/
          .gitkeep
          frame_0001.png     # exemplo de arquivo que o Blender gerará LOCALMENTE
          frame_0002.png
          frame_0003.png
          ...
  02_hina/step4_blender/walk_x/frames/
  03_akari/step4_blender/walk_x/frames/
  04_momo/step4_blender/walk_x/frames/
  05_reina/step4_blender/walk_x/frames/
  06_kaede/step4_blender/walk_x/frames/
  07_nene/step4_blender/walk_x/frames/
  08_ichika/step4_blender/walk_x/frames/
  09_yoru/step4_blender/walk_x/frames/
```

**Os PNGs mostrados são exemplos de nomes e ainda não existem.** Somente os nove diretórios de destino e respectivos `.gitkeep` ficam no GitHub inicialmente. Para outras animações, crie uma pasta **irmã** de `walk_x/`, por exemplo `idle/frames/`, `carry/frames/`, `walk_y/frames/`. Não misture idle e caminhada, maids diferentes, ou tentativas de vídeo diferentes na mesma sequência. Para uma nova tentativa, use uma subpasta separada como `walk_x_v02/frames/` ou limpe somente arquivos brutos daquela tentativa após backup; **não apague fontes sem confirmar**.

## Tutorial — vídeo do Tencent → imagens PNG do Blender

1. Salve o **vídeo gerado no Tencent (Etapa 3)** no computador. **Não é necessário anexar imagem ao Blender nesta etapa; a entrada é o vídeo**. Se houver opção de vídeo com cromakey, mantenha o fundo uniforme. Preserve o arquivo de origem, não edite o original.
2. No **Blender**, abra **Video Editing** (Video Sequencer), adicione o vídeo em **Add → Movie**, confira que a faixa contém os quadros desejados e estabeleça **Start** / **End** para o intervalo real de animação. Se usar o Sequencer para exportação, confira que ele está ativado no pipeline de render.
3. Em **Output Properties (ícone de impressora)**, defina resolução e **Frame Rate (FPS)** conforme a origem ou um valor escolhido conscientemente. Não reduza silenciosamente a resolução ou misture FPS entre takes.
4. Em **Output**, selecione uma pasta de **destino absoluta** que termine em `art_working/01_suzu/step4_blender/walk_x/frames/` (troque `01_suzu` pela maid correta). Como prefixo, use `frame_`, se necessário. **Não confie em `//` sem conferir onde seu arquivo `.blend` está salvo**: no Blender, `//` é relativo ao local do `.blend`, não automaticamente à raiz do Git.
5. Escolha **File Format = PNG**, **Color = RGB** se o fundo chroma ainda está pintado nos frames. Use **RGBA** somente se você realmente removeu o chroma e tem transparência real; **salvar em RGBA não remove o verde/azul por si só**.
6. Confira **Save Output** ligado, depois use **Render → Render Animation** (`Ctrl+F12`). O Blender deverá salvar uma sequência de arquivos PNG numerados na pasta escolhida, por exemplo `frame_0001.png`, `frame_0002.png`... (prefixo e padding podem variar conforme configurações).
7. Abra o **primeiro, um intermediário e o último frame** e verifique que: foram gerados na pasta da maid certa; sequência sem lacunas; mesma resolução; personagem inteiro sem cortes; fundo consistente; número de quadros/FPS corretos; sem mudanças indesejadas no rosto/roupa/cauda ou mudança da posição da câmera. Verifique a possibilidade de loop em execução, não apenas pela imagem estática.

**Se o vídeo tem fundo chroma**, os PNGs desta pasta continuarão com chroma até uma etapa explícita de remoção de fundo. Não use diretamente no Unity como sprites transparentes sem remover/validar o fundo. **Não corte, reescale, gere sprite sheet ou altere o jogo automaticamente.** O passo seguinte (recorte, seleção de quadros, transparência, slicing e integração) exige avaliação visual, técnica e aprovação.

### Quando você precisar me passar os frames

Os arquivos estão disponíveis **localmente no checkout/worktree onde foram exportados**, mas não no GitHub, pois o Git os ignora. Para outro agente trabalhar neles, compartilhe deliberadamente a pasta/ZIP por um caminho que ele possa acessar ou abra um PR separado com um conjunto **curado e aprovado** de imagens, depois de revisar tamanho, direitos/geração, qualidade e `.meta` do Unity se for importado. Não faça `git add -f` de centenas de frames sem essa decisão.

## Referências

- [Prompts completos e anexos em cada etapa](../docs/art/prompt-pipeline/README.md)
- [Índice das nove maids](../docs/art/prompt-pipeline/MAIDS_INDEX.md)
- [Blender 5.2 — Rendering Animations (documentação oficial)](https://docs.blender.org/manual/en/latest/render/output/animation.html)
- [Blender 5.2 — Output Properties (documentação oficial)](https://docs.blender.org/manual/en/5.2/render/output/properties/output.html)

**Status de validação:** apenas estrutura e documentação criada; Blender, processamento de vídeo, ciclos e integração Unity não foram executados nesta alteração.
