# Suzu modular — estudo estático, gate artístico falhou

Branch: `experiment/suzu-modular-cutout-20261029`. Base remota conferida: `6d5767b8de3031423fabb22a506e49086dedad9a`.

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
