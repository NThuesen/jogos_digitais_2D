# Suzu modular — estudo estático, gate artístico falhou

Branch: `experiment/suzu-modular-cutout-20261029`. Base remota conferida: `6d5767b8de3031423fabb22a506e49086dedad9a`.

**Atualização — prova raster:** duas iterações executadas conforme [NEXT.md](NEXT.md); resultado e fonte editável ao final. Não substitui as peças anteriores nem aprova o gate artístico.

## Resultado e limite
16 peças independentes, desenhadas originalmente com paths; nenhuma imagem antiga foi recortada ou reutilizada como peça. SVGs e gerador em `source/`, PNGs em `Assets/Experiments/SuzuModular/Parts`, harness Editor em `Assets/Experiments/SuzuModular/Editor/SuzuStudy.cs`. Não há alteração de gameplay, save, UI, cenas salvas ou Packages. Os três arquivos previamente modificados foram preservados e excluídos do commit.

**Gate: renderização estática funciona; qualidade artística insuficiente.** Não avançamos para animação ou troca de acessórios. Não é um rig final: pivôs iniciais existem, mas os membros ainda precisam de divisão cotovelo/joelho e cobertura validada em movimento. Acessório é separado, mas troca reversível não foi demonstrada.

## Evidência real e iterações
Capturas produzidas por `Camera.Render` dentro do Unity 6000.5.9f1 via Coplay `execute_code`, em cena de preview isolada. Não são capturas da Game View nem mockups. `manage_camera(screenshot)` não encontrou a câmera da cena de preview; a renderização explícita pelo Coplay funcionou.

| Versão | Hipótese/correção | Avaliação dos três problemas prioritários |
|---|---|---|
| [1](evidence/static-before.png) | Montagem inicial | Alto: olho parece óculos. Alto: cabelo achatado. Médio: uniforme pobre. |
| [2](evidence/static-02.png) | Abrir contorno do olho; laço no pescoço | Olho melhorou. Alto: rosto comprido. Alto: cabelo rígido. Médio: acabamento pobre. |
| [3](evidence/static-03.png) | Mechas iluminadas; pivôs por região | Melhora pequena no cabelo. Alto: cabelo rígido. Alto: rosto comprido. Médio: roupa plana. |
| [4](evidence/static-04.png) | Encurtar queixo, reposicionar sorriso e blush | Perfil menos pesado. Alto: linguagem vetorial distante do anchor. Alto: cabelo rígido. Médio: roupa plana. |
| [5 — 960×600](evidence/static-05-960x600.png) / [1280×720](evidence/static-05-1280x720.png) | Bordado e costura do avental | Detalhe ampliado melhorou; pouco ganho pequeno. Persistem os três defeitos da versão 4. |

Escalas: no quadro 960×600, altura visível aproximada 276/92/62 px; em 1280×720, 331/110/74 px. O tamanho 96 px foi hipótese, não medida do restaurante de produção. Contexto do restaurante **não validado**. Fonte desenhada em 96×112 e rasterizada em 192×224, com Point no harness: isso não constitui pixel art autoral de acabamento final.

Comparação: a identidade menta/âmbar/chocolate/creme/bordô aproxima Suzu D0; não alcança volume das mechas, babados e expressão da referência. O style anchor foi inspecionado, sem copiar personagens. A cauda ainda não carrega seu laço e sino; entalhe de orelha pouco legível. Essas lacunas não são aprovação estética.

Menor mudança recomendada: manter peças e harness, substituir o desenho de cabeça/cabelo por pixel art autoral em camadas na resolução-alvo, incluindo limpeza manual de clusters pela IA; redesenhar roupa com planos de sombra mais definidos. Mais traços SVG decorativos não resolvem o problema estrutural de desenho/volume. Não adicionar sistema esquelético nem dependências Unity.

## Reprodução gratuita
Python 3, CairoSVG 2.9.1 e Pillow 13.0.0 (dependência do rasterizador). Ambiente virtual externo ao projeto. Executar `python docs/art/suzu-modular/source/draw.py` na raiz: regenera SVGs e PNGs. **Fonte autoritativa: paths em draw.py**; SVGs também são editáveis, mas alterações diretas precisam ser transferidas ao gerador antes de regenerar.

No Editor existente via Coplay: `SuzuStudy.Capture("review",960,600);`, depois `SuzuStudy.Capture("review-wide",1280,720);`. `SuzuStudy.Close();` libera a cena de preview e os recursos temporários. Não lançar outro Editor. Não requer cena salva, menus, runtime ou pacote extra.

## Validação
- Passou: geração local das 16 peças; compilação no Editor; montagem e capturas de câmera reais; inspeção visual das cinco versões.
- Falhou: gate artístico interno; abertura aditiva inicial com cena sem nome; screenshot de câmera de preview por nome.
- Não executado: animação, juntas durante movimento, sprite swap, espelhamento, PlayMode/EditMode suites, simulação e WebGL. Etapas posteriores bloqueadas pelo gate estático; nenhum código de produção foi alterado.
- Console consultado após compilação inicial: zero erros. Nenhuma API paga usada. Auto-merge proibido nesta entrega; aprovação estética permanece com o responsável.

## Prova raster — 29/10/2026, AGUARDA APROVAÇÃO ESTÉTICA

- Método disponível: ferramenta integrada `image_gen`, duas chamadas; sem CLI/API com chave, contratação de serviço ou instalação. Coplay não está exposto; executável Unity Windows presente, mas acesso falhou com `WSL UtilBindVsockAnyPort: socket failed 1`. Nenhum Editor foi lançado.
- Iteração 01: rosto compacto, mechas com planos de luz/sombra e dobras alternadas na saia. Iteração 02: retirar gola/laço/sino duplicados do grupo cabeça; manter o laço do torso. [Comparação das duas hipóteses](evidence/raster-iterations-960x600.png).
- [Referência D2 × captura 05 × proposta, 960×600](evidence/raster-comparison-960x600.png): montagem **offline**, recortes de referência/05 com braços/cauda originais; nova arte somente cabeça/cabelo + torso/saia. Alturas cabeça–barra 240/80/54 px com Point; 80 px é hipótese, não medição no restaurante. Anchor e D2 foram inspecionados; nenhum recorte de referência entra nas novas peças.
- Fonte raster editável: [OpenRaster, dois grupos](source/raster/suzu-probe-02.ora), [cabeça/cabelo](source/raster/head-hair.png), [torso/saia](source/raster/torso-skirt.png), [montagem](source/raster/assembly.png). Sheets originais e prompts das duas chamadas estão em `source/raster/`; não são fontes vetoriais nem camadas semânticas internas de olhos/babados.
- Reprodução: Python com Pillow, `python docs/art/suzu-modular/source/raster_probe.py`; apenas recorta, posiciona e monta fontes existentes, sem gerar/repintar arte. Canvas 920×1420, cabeça em (0,0), torso em (28,716), cabeça à frente. Regerar é determinístico a partir dos sheets salvos; chamadas generativas não são determinísticas.

| Item | Estado observado, sem aprovação artística |
|---|---|
| RGBA, dois grupos, OpenRaster e evidências | **PASSOU TECNICAMENTE**: alpha presente, camadas extraídas dos sheets, recomposição do ORA idêntica ao PNG, quadros 960×600; alterações locais preservadas por SHA-256. |
| Rosto/cabelo, roupa e paleta | Melhoria material observada frente à 05: rosto compacto e olhos expressivos, mechas com volume, saia com dobras; menta/âmbar/chocolate/creme/bordô preservados. **AGUARDA APROVAÇÃO ESTÉTICA**. |
| Legibilidade e acabamento pixel art | **FALHOU** como sprite final: grade de pixels inconsistente, ruído com Point sobretudo a 54 px; cores/contorno sobrevivem a 80 px, detalhes faciais e babados perdem definição. Não é pixel art limpa na resolução-alvo. |
| Costura estática | **PASSOU TECNICAMENTE** na [montagem ampliada do pescoço](evidence/raster-neck-seam.png): gola cobre a junção sem laço duplicado. Costuras em movimento **NÃO EXECUTADO**. |
| Unity 960×600, runtime, testes Unity/WebGL | **NÃO EXECUTADO**: ponte Windows indisponível e sem Coplay. Não houve mudança em Assets, harness, packages, cenas, gameplay ou testes. |

Encerrado após duas iterações. Não avançar para animação/rig/pipeline. Para resolver o acabamento é necessária uma entrada raster em camadas com clusters limpos na resolução-alvo (cabeça–barra ≈80 px nesta hipótese), incluindo alpha limpo e a mesma junção coberta pela gola; o responsável aprova a aparência, não precisa desenhar ou recortar. Nenhum asset final foi aprovado.
