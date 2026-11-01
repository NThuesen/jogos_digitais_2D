# Catálogo das nove maids — HINA STYLE LOCK v2

**Mesma família visual, nove identidades diferentes.** A Hina full-body aprovada pelo responsável é a referência-mestra obrigatória de **pixel art, proporção chibi e uniforme**. Cada ficha contém um **PROMPT 1 completo** (full-body) e **PROMPT 2 completo** (quadro inicial walk-x), sem precisar concatenar com prompts-base. [Guia prático de geração](README.md) · [Contrato visual e rubrica](HINA_STYLE_LOCK.md).

**Aviso:** a imagem-mestra da Hina ainda não está versionada aqui; anexe a versão full-body aprovada da Biblioteca/arquivos locais em **todas** as gerações, não o frame com chroma. A antiga imagem de cenário de café permanece referência histórica **e não deve ser usada como anexo** da pipeline v2.

| Maid / prompts | Arquétipo | Identidade exclusiva obrigatória | Chroma sugerido |
|---|---|---|---|
| [01 Suzu](maids/01_suzu.md) | Genki | Bob menta, olhos âmbar, entalhe na orelha esquerda, sinos, laço bordô | azul `#0000FF` |
| [02 Hina](maids/02_hina.md) | Kuudere | Bob azul-noite assimétrico, olhos gelo, presilha triangular prata; **master aprovado** | verde `#00FF00` |
| [03 Akari](maids/03_akari.md) | Tsundere | Ponytail carmesim lateral, ponta preta na orelha esquerda, olhos ouro | verde `#00FF00` |
| [04 Momo](maids/04_momo.md) | Dojikko | Bob pêssego, ahoge em espiral, orelha direita caída | verde `#00FF00` |
| [05 Reina](maids/05_reina.md) | Ojou-sama | Drill curls platina/vinho, broche ametista | verde `#00FF00` |
| [06 Kaede](maids/06_kaede.md) | Onee-san | Trança castanha à direita, pinta sob olho esquerdo, olhos oliva | azul `#0000FF` |
| [07 Nene](maids/07_nene.md) | Idol | Coque rosa à esquerda, mecha ciano à direita, heterocromia com lados fixos | verde `#00FF00` |
| [08 Ichika](maids/08_ichika.md) | Gourmand | Cabelo cacau com mecha creme, mini-toque de confeiteira | verde `#00FF00` |
| [09 Yoru](maids/09_yoru.md) | Mukuchi | Cabelo índigo longo cobrindo olho esquerdo, orelhas de pontas brancas | verde `#00FF00` |

**Ordem por personagem:** Hina full-body como estilo → Etapa 1 com identidade textual → **gate humano lado a lado (9–10/10)** → Etapa 2 com imagens A (maid) + B (Hina) → vídeo com pé plantado/passada correta → frames do Blender em `art_working/NN_nome/step4_blender/walk_x/frames/` → integração futura no Unity.

**Atenção:** não confundir as nove identidades ilustradas com elenco persistido no jogo; os nomes/arquetipos das cartas da simulação ainda são sorteados, e `visual_id` é decisão de integração separada. Os prompts não alteram `Neko.Sim`, UI, save, raridade, scenes ou regras de produção existentes. O experimento prévio da Suzu continua como prova técnica, não como novo padrão de estilo. Os prompts são diretrizes rigorosas mas **não garantem saída visual determinística**.
