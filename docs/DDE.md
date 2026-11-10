---
title: "Neko Maid Inc. — Documento de Design de Jogo (DDE)"
subtitle: "Design · Dynamics · Experience | Jogos Digitais — Insper"
date: "10 de novembro de 2026"
lang: pt-BR
---

**Documento de Design do Jogo — framework DDE (Design · Dynamics · Experience)**  
**Disciplina:** Jogos Digitais · Insper  
**Projeto:** jogo original 2D, gestão/estratégia e simulação de cafeteria  
**Plataforma-alvo:** navegador desktop, WebGL · **Engine:** Unity 6 (2D/URP)  
**Versão documental:** 1.0 · **Atualização:** 10/11/2026  
**Repositório:** [NThuesen/jogos_digitais_2D](https://github.com/NThuesen/jogos_digitais_2D)

> **Escopo e honestidade documental.** Este documento apresenta a concepção do jogo, as regras descritas no GDD técnico e a análise DDE da experiência pretendida. As descrições da *experiência* são **objetivos de design, não resultados de playtest humano comprovados**. A integração de novas telas ilustradas estava em andamento na data desta revisão; um mockup aprovado, uma PR aberta e uma build final publicada são estados distintos. O documento não certifica a conclusão dessa integração.

## Resumo executivo

*Neko Maid Inc.* é um jogo 2D de administração de um café de maids-gato. A fantasia de dirigir um estabelecimento acolhedor contrasta com decisões empresariais difíceis: contratar personagens com estilos distintos, formar uma equipe com boa química, distribuir funções, equilibrar atendimento e folha de pagamento, decorar o ambiente e sobreviver financeiramente. Durante o expediente, o serviço acontece de forma autônoma, mas o jogador pode intervir em situações pontuais — acomodar clientes, anotar pedidos, limpar mesas, controlar o fogão e acalmar pessoas impacientes — para melhorar o resultado.

**Proposta central:** *um café aconchegante em que contratar a maid mais rara nem sempre é a melhor decisão*. O jogo pretende transformar escolhas de composição, operação e investimento em consequências observáveis: filas, espera, satisfação, gorjetas, reputação, custos, lucro e risco de falência.

**Objetivo do jogador:** manter a cafeteria funcionando e ampliar sua capacidade, seu elenco e seu prestígio ao longo de ciclos diários, sem perder a sustentabilidade financeira.

**Gênero:** simulação de gestão com coleção de personagens, otimização de equipes e interação contextual em tempo real. **Modo:** single-player. **Entrada principal:** mouse.

## 1. O DDE como instrumento de projeto

Conforme a aula fornecida pelo professor, **Design** abrange aquilo que a equipe controla diretamente (estrutura, regras e interface); **Dynamics** descreve o comportamento efetivo decorrente da interação dessas regras; **Experience** descreve os sentidos, emoções, pensamentos e interpretações produzidos no jogador. O ponto principal é a relação causal entre as três dimensões, e não uma lista isolada de funcionalidades.

| Dimensão | Subdivisão da aula | Pergunta aplicada a Neko Maid Inc. |
|---|---|---|
| **Design** | Blueprint · Mechanics · Interface | Que mundo, regras e meios de interação são definidos? |
| **Dynamics** | Player–Player · Player–Game · Game–Game | O que realmente acontece quando as decisões e os sistemas entram em contato? |
| **Experience** | Sentidos · Emocional · Intelectual · Percepção | O que se pretende que o jogador sinta, pense e compreenda? |

**Hipótese de design:** se o jogador receber informações suficientemente claras sobre elenco, química, capacidade do salão e custos, conseguirá formular planos próprios, observar seus efeitos no expediente e sentir que os resultados — bons ou ruins — são consequências compreensíveis de suas escolhas.

# PARTE I — DESIGN

## 2. Blueprint: identidade, mundo e intenção

### 2.1 Conceito e diferencial

O universo mistura a estética afetiva dos cafés temáticos de anime com a lógica de uma pequena empresa. **Fofura por fora, planilha por dentro** é o contraste de identidade: o jogador se aproxima das personagens por seus retratos e personalidades, mas não pode ignorar salário, aluguel, custos de ingredientes ou produtividade. Diferentemente de um jogo puramente decorativo, a organização do salão interfere no funcionamento; diferentemente de um simulador financeiro abstrato, a gestão é apresentada por personagens e cenas visuais.

Três pilares de design orientam o projeto:

1. **Escalação com consequências:** cada maid possui perfil, função natural, atributos, salário, raridade e afinidades; formar o time exige escolhas, não apenas acumular raridade.
2. **Operação visível:** as maids movimentam-se e atendem; pedidos, cozinha, clientes e mesas tornam observável a relação entre planejamento e resultado.
3. **Expansão sob risco:** melhorias, coleção e reputação dão metas de continuidade, mas despesas fixas e falência impedem que o crescimento seja automático.

### 2.2 Ambientação e narrativa emergente

A cafeteria opera num universo fictício inspirado na cultura de cat-cafés e maids japonesas, com influência visual cozy/anime. A identidade narrativa é construída mais pelas rotinas e consequências do que por uma campanha linear: clientes chegam, maids demonstram comportamentos característicos, o jogador adapta a escalação e cada balanço conta como foi aquele expediente. O jogo não precisa de antagonista tradicional para produzir conflito: o principal adversário é o descompasso entre demanda, capacidade, atendimento e caixa.

Na concepção documentada, seis casas de formação — **Tsukimi, Hanabi, Kurogane, Shirayuki, Momiji e Hoshizora** — oferecem afinidades compartilhadas entre personagens. Temperamento e origem complementam essa identidade e influenciam a química e a relação com o tema do restaurante.

**Arco de evolução:** começar com uma empresa pequena e recursos restritos; aprender a escalar; conquistar reputação, novas vagas e opções; decidir se um elenco mais poderoso consegue sustentar salários maiores. Há progressão de gestão, não capítulos narrativos obrigatórios.

### 2.3 Público, formato e sessão

A proposta se dirige a jogadores interessados em gerenciamento acessível, coleção de personagens e otimização incremental, sem exigir conhecimento prévio de planilhas. O tom visual busca acolhimento, enquanto a simulação oferece profundidade opcional. O público consegue acompanhar partidas curtas e repetir o ciclo. O GDD registra expedientes da ordem de **75 a 180 segundos**, mas a duração final deve ser verificada na build entregue.

### 2.4 Personagens e agência

As maids não são apenas cartas: são agentes de serviço com atributos de **Salão, Cozinha, Charme e Fôlego** e um dos nove arquétipos comportamentais. Um arquétipo pode favorecer trabalho rápido, cozinha, atração de VIPs, palco ou auxílio ao grupo, mas também incluir uma desvantagem. O jogador é o administrador: define decisões estratégicas antes do dia e presta suporte tático quando o café abre.

### 2.5 Direção de arte e linguagem sonora

A direção visual aprovada para o produto é **anime 2D ilustrado/chibi, de atmosfera cozy**, com madeira quente, creme, rosa, detalhes dourados, elementos felinos e ornamentos de cafeteria. As personagens precisam ser reconhecíveis pelo retrato, silhueta e cores; a linguagem visual da administração deve combinar com o ambiente de jogo sem sacrificar legibilidade.

O pipeline adotado separa **fundos e ornamentos ilustrados** de **dados e interação reais no Unity**. Números, nomes, barras, preços e estados não devem estar fixados em uma imagem. A ambientação sonora documentada contempla efeitos de porta, atendimento, fogão, pratos, caixa e progresso, além de vozes estilizadas e trilhas diferentes conforme o ritmo do café. A entrega final depende de validação audiovisual no navegador.

## 3. Mechanics: regras, ações e sistemas

### 3.1 Ciclo central da partida

**Empresa → Escalação e preparação → Abrir expediente → Atendimento e intervenções → Balanço → Reinvestimento/ajustes → Próximo expediente.**

Cada passagem do ciclo fornece informação para a próxima: a escalação define a capacidade operacional; o turno revela gargalos; o balanço mede receita, custos e qualidade; o jogador responde contratando, reorganizando, comprando ou poupando. Não existem fases tradicionais: o mesmo espaço de gestão ganha complexidade com o nível e o elenco.

### 3.2 Escalação: cinco funções

| Função | Responsabilidade principal | Consequência para o plano |
|---|---|---|
| **Recepção** | Receber clientes e levá-los a mesas | Fila na entrada e velocidade de ocupação |
| **Salão** | Anotar pedidos, servir e limpar | Rotatividade de mesas, satisfação e pedidos corretos |
| **Cozinha** | Preparar as comandas | Tempo de espera, pratos entregues e desperdício |
| **Palco** | Sustentar o humor/paciência dos clientes | Tolerância a atrasos e estabilidade do serviço |
| **Gerência** | Ajudar com gorjetas e fichas | Retorno econômico e recompensa por qualidade |

O jogo começa com uma vaga em cada função operacional essencial (Recepção, Salão, Cozinha); vagas e funções adicionais são liberadas na progressão. **Sem as funções essenciais e recursos mínimos do salão, o dia não começa**: a interface deve indicar o impedimento, não deixar o jogador preso. Escalar alguém fora da posição natural acarreta perda de eficiência documentada no GDD de **35%**, além de penalidades de química e erro.

### 3.3 Química: a força do conjunto

A química combina: adequação à posição; etiquetas de **Casa, Temperamento e Origem** compartilhadas entre colegas; compatibilidade com a decoração do café; e traços individuais. O resultado agregado é mostrado ao jogador como indicador de **0 a 100**, associado a um multiplicador documentado de **0,75× a 1,25×** nos atributos. Assim, não basta avaliar uma maid isoladamente: a unidade de planejamento é o **time no salão real**.

**Exemplo de regra e consequência:** substituir uma maid rara fora da função por uma comum bem posicionada pode aumentar a eficiência, reduzir erros e diminuir a folha de pagamento. Esse é um comportamento estratégico que o sistema pretende tornar possível; o resultado exato depende da composição e dos dados daquela partida.

### 3.4 Arquétipos com trade-offs

| Arquétipo | Benefício/conduta distintiva documentada | Contrapartida ou decisão criada |
|---|---|---|
| **Genki** | Deslocamento mais rápido | Gasta ânimo mais depressa |
| **Kuudere** | Não erra pedidos | Anda mais lentamente |
| **Tsundere** | Acelera em salão cheio | Menor eficiência em cenário vazio para gorjetas |
| **Dojikko** | Gera situações cômicas por acidentes | Pode derrubar pratos e atrasar o serviço |
| **Ojou-sama** | Atrai clientes VIP | Não limpa mesas, aumentando a pressão sobre outras funções |
| **Onee-san** | Apoia colegas e compartilhamento de experiência | Valor depende de quem trabalha junto |
| **Idol** | Apresentações que recuperam paciência | Favorece uso do palco e composição adequada |
| **Gourmand** | Produção culinária veloz | Pode consumir parte dos pratos |
| **Mukuchi** | Alto desempenho de cozinha | Fraca na recepção |

Essas diferenças foram descritas no GDD como regras de simulação; a apresentação visual individual ajuda o jogador a associar personalidade à utilidade operacional. **Arquétipo não é raridade:** dois atributos separados participam da escolha.

### 3.5 Clientes, necessidades e atendimento

Há sete perfis documentados: **Cliente**, **Estudante**, **Assalariado**, **Gato**, **Fã**, **Crítico** e **VIP**. Variam em paciência, gasto, momento de desbloqueio e comportamento: estudantes toleram mais espera e gastam menos; VIPs concentram receita, mas toleram pouca demora; fãs valorizam um arquétipo escalado; críticos influenciam a percepção de reputação; gatos trazem comportamento particular e atmosfera. Essa diversidade impede que uma única escalação seja ótima para toda situação.

O serviço progride por estados: **porta → fila → mesa → pedido → comanda → cozinha → prato → entrega → consumo → pagamento → limpeza**. As personagens assumem tarefas conforme sua função, urgência e deslocamento. Pedidos errados, pratos queimados ou derrubados e clientes que desistem geram custos e resultados observáveis no balanço.

### 3.6 Participação ativa do jogador no expediente

A equipe trabalha autonomamente, mas o jogador pode atuar como suporte contextual:

| Ação contextual | Sinal de oportunidade | Efeito visado |
|---|---|---|
| Acomodar cliente | Cliente aguardando na entrada | Reduzir fila e adiantar ocupação |
| Anotar pedido | Pedido pendente/cliente sinalizando | Destravar fluxo do atendimento |
| Limpar mesa | Mesa com pratos sujos | Liberar lugar para próximo grupo |
| Controlar fogão | Medidor de preparo com faixa ideal | Evitar desperdício e obter bônus de serviço |
| Acalmar cliente | Estado de impaciência | Recuperar margem de espera |

O GDD registra **faixa ideal do fogão entre 72% e 95%**, janela de combo de 6 segundos, efeitos sobre gorjetas e limitações para que o administrador não substitua a equipe. Esses valores pertencem às regras documentadas e podem ser ajustados apenas com atualização do modelo e deste documento. O jogador pode optar por não intervir; essa escolha deve ter consequência econômica perceptível, mas não ser confundida com uma falha de controles.

### 3.7 Economia: duas moedas, uma empresa

**Dinheiro (¥)** entra pelo atendimento e sai por aluguel, salários, ingredientes, móveis e outros investimentos aplicáveis. **Fichas** recompensam a qualidade do expediente e são usadas nas caixas de contratação. O projeto documenta que as moedas **não são conversíveis** entre si: conseguir mais fichas não paga a folha e ter caixa abundante não compra diretamente sorte no recrutamento.

| Raridade | Salário diário documentado (¥) | Implicação |
|---|---:|---|
| Comum | 18 | Acessível e sustentável no começo |
| Rara | 44 | Aposta intermediária |
| Épica | 95 | Exige retorno operacional consistente |
| Lendária | 210 | Alto potencial, alto custo fixo |

O jogador pode dispensar maids; contratações e cartas competem com outros investimentos. O balanço diário explicita **receitas, despesas, lucro/prejuízo, saldo, satisfação e ocorrências**. O ponto de pressão é o caixa; cair em uma situação de insolvência pode encerrar a empresa, preservando recordes nos termos documentados.

### 3.8 Caixas, probabilidades e colecionabilidade

As três caixas usam **fichas** e apresentam probabilidades na interface, reduzindo incerteza oculta sobre as regras de obtenção:

| Caixa | Custo (fichas) | Cartas | Comum | Rara | Épica | Lendária |
|---|---:|---:|---:|---:|---:|---:|
| Simples | 40 | 1 | 78% | 18% | 3,6% | 0,4% |
| Prata | 180 | 3 | — | 62% | 32% | 6% |
| Dourada | 520 | 3 | — | — | 78% | 22% |

A caixa mais cara possui piso de raridade mais alto. A capacidade do elenco limita aquisições: a documentação registra compensação em fichas para cartas que não caibam. As chances mostradas acima são as publicadas no GDD técnico consultado; **na entrega, devem ser conferidas contra o catálogo efetivo de caixas no código**.

### 3.9 Salão, inventário e expansão

Entre expedientes, o jogador pode comprar e posicionar móveis, ajustar o layout, retirar itens permitidos e girar objetos. Mesas e estações impactam capacidade, fluxo e necessidades de serviço. A loja é organizada em **Móveis, Roupas/Equipamentos e Presentes**, com saldo e estoque reais; roupas podem afetar atributos sem necessariamente alterar o desenho do personagem.

A progressão usa **nível do café de 1 a 10**, alimentado por reputação associada ao atendimento. Desbloqueios podem ampliar espaço, vagas, receitas e itens. Não existem fases ou mapas independentes: o próprio café torna-se mais complexo. A expansão não deve ser considerada sempre lucrativa; adicionar mesas sem ampliar capacidade humana pode reduzir o desempenho.

### 3.10 Vitória, derrota, continuidade e save

**Objetivo de curto prazo:** concluir um expediente com atendimento funcional e balanço compreensível. **Objetivo de médio prazo:** melhorar reputação, montar elenco, ampliar capacidade e manter reservas. **Condição de derrota:** falência conforme as regras econômicas. Não há uma campanha de missão final única; a progressão e a busca por resultados superiores incentivam continuidade e novas tentativas.

O projeto documenta persistência de empresa com **versão e checksum**, proteção a arquivos corrompidos e uma máquina de estados para impedir transições ilegais. As alterações permanentes devem ser salvas, e estados sem equipe/mesa/estação suficiente precisam informar a correção necessária.

## 4. Interface: como o jogo comunica e recebe ações

### 4.1 Arquitetura da jornada de telas

| Tela | Informação ou decisão que oferece |
|---|---|
| Menu | Identidade do jogo e início/continuação |
| Empresa (Hub) | Caixa, fichas, custos, nível, equipe, navegação |
| Equipe do Dia | Cinco funções, vagas, banco, química e escalação |
| Maids/Elenco | Coleção, filtros, raridade, atributos e dispensa |
| Caixinhas | Preços, probabilidades, abertura e resultado |
| Lojinha | Móveis, roupas/equipamentos, presentes e compras |
| Arrumar o Salão | Planta interativa do café, estoque e posicionamento |
| Expediente | Serviço em andamento, tempo, clientes e ajudas |
| Resultado/Balanço | Receitas, despesas, desempenho, saldo e continuidade |

A composição final deve ser consistente com o tema **cozy cat-café**. Para o editor do salão foi aprovada a metáfora visual de uma **mesa de escritório com uma planta interativa**, e para o balanço um **relatório ornamental do café**. Esses desenhos são uma direção de apresentação; a planta, os clientes, as ações, os números e os botões continuam sendo elementos funcionais do Unity.

### 4.2 Controles e feedback

O mouse seleciona funções e cartas, compra itens, posiciona objetos e aciona oportunidades de ajuda durante o turno. O expediente oferece controles de velocidade **1×, 2× e 4×**, além de avanço para o resultado. O jogador deve reconhecer estados sem depender apenas de textos: sinais no cenário destacam clientes esperando, mesas sujas, ponto do fogão, impaciência e erros. HUD contextual fornece tempo, receita, atendimento, ocorrências e estado da equipe.

**Princípio de clareza:** uma ação importante deve ter (a) affordance visual, (b) resposta perceptível e (c) consequência reconhecível. Por exemplo, o prato sujo pede limpeza; o clique deve alterar a mesa; a liberação do lugar deve beneficiar o fluxo. Quando faltam pré-condições para abrir o dia, apresentar **o que falta e onde corrigir**, em vez de apenas desabilitar um botão sem contexto.

### 4.3 Inclusão, legibilidade e qualidade

A versão-alvo é WebGL desktop. As referências de QA da equipe usam **1280×720 e 960×600**. Menus e HUD devem ter tipografia legível, estados normal/hover/pressionado/desabilitado distinguíveis, hierarquia visual para valores importantes e áreas clicáveis coerentes com o cenário real. **Cor isolada não deveria ser o único indicador de risco**: texto, ícone e movimento podem complementar as mudanças de paciência/estado.

# PARTE II — DYNAMICS

## 5. Player–Player: escopo e ausência deliberada

*Neko Maid Inc.* é documentado como **single-player**. Não existe mecânica implementada de colaboração, competição, troca de cartas, ranking online ou interação simultânea entre jogadores. Portanto, **Player–Player não se aplica diretamente ao gameplay atual**. Jogadores podem comentar resultados e estratégias fora do jogo, mas essa interação social externa não deve ser apresentada como sistema multiplayer.

Essa ausência é uma escolha coerente de escopo acadêmico: concentra esforço na profundidade de Player–Game e Game–Game. Qualquer comparação de recordes entre pessoas seria uma possibilidade futura, não uma funcionalidade desta entrega.

## 6. Player–Game: como escolhas se tornam estratégia

### 6.1 Escolha sob restrição

O jogador não dispõe de orçamento, vagas e atributos ilimitados. Ao formar uma equipe, precisa decidir entre contratar uma maid poderosa e cara, manter uma mais barata que combina com a função, investir em móveis ou preservar caixa para o dia seguinte. A dinâmica central é **priorização sob risco**, e não um caminho obrigatório de upgrades.

### 6.2 Diagnóstico e adaptação

O expediente revela se a escalação funciona: a recepção pode criar fila, a cozinha atrasar comandas, o salão acumular mesas sujas ou os clientes perderem paciência. O jogador pode reagir no momento com ajudas pontuais e, principalmente, usar o balanço para mudar a próxima escalação. Essa dinâmica forma um ciclo **hipótese → execução → observação → revisão**.

### 6.3 Autonomia versus intervenção

Como as maids executam o serviço automaticamente, o jogador alterna observação gerencial e microdecisões. Um perfil mais relaxado pode acompanhar a simulação e aceitar o desempenho-base. Um jogador atento identifica oportunidades de auxílio e tenta converter tempo e precisão em qualidade de atendimento e lucro. O design limita o auxílio para preservar relevância da equipe.

### 6.4 Aprendizado por consequências

O jogo não depende apenas de tutoriais: decisões revelam relações. Colocar uma maid fora da função pode piorar erros; atrair VIPs sem capacidade para atendê-los pode gerar frustração; uma cozinha veloz não resolve uma recepção saturada; ampliar mesas pode ser prematuro. A dificuldade desejada é de **compreensão do sistema**, não de memorização arbitrária.

### 6.5 Gestão emocional e coleção

O retrato de uma personagem rara incentiva recrutamento, mas seu salário cria uma contrapartida. O jogador pode se apegar a uma maid e resistir à dispensa mesmo quando ela é cara ou mal posicionada. Essa tensão entre vínculo afetivo e viabilidade econômica é uma dinâmica desejada; não se afirma que foi observada em playtest sem evidência.

## 7. Game–Game: sistemas que interagem independentemente do jogador

### 7.1 Cadeia operacional

Chegadas de clientes geram fila; recepção ocupa mesas; salão e cozinha processam pedidos; o tempo de atendimento afeta paciência; mesas precisam ser liberadas; qualidade de serviço afeta ganhos e reputação. Um gargalo em qualquer etapa propaga-se às seguintes. As maids possuem comportamentos autônomos e interagem com as regras de prioridade, deslocamento, humor e função.

### 7.2 Economia × capacidade × satisfação

Mais mesas **aumentam capacidade física**, mas não garantem mais serviço realizado. Se o quadro de funcionários for insuficiente, a espera aumenta e parte da receita potencial desaparece. O retorno de um investimento depende da interação entre espaço, equipe, demanda e preço — e não do preço da mobília isoladamente.

**Ressalva de versão:** o repositório registra uma investigação ECON-02b e a PR #74, ainda aberta na consulta de 10/11/2026, que propõe desacoplar a taxa de chegada de clientes da quantidade de lugares. Assim, **não atribuímos automaticamente à build final** a correção proposta nessa branch. A relação causal aqui descreve a intenção de design e o problema documentado.

### 7.3 Arquétipos × clientes × posição

Uma Ojou-sama pode atrair VIPs; VIPs gastam mais, mas esperam menos; se o salão não atende a tempo, uma habilidade aparentemente vantajosa pode piorar o resultado. Uma Idol no palco pode aumentar tolerância à espera e compensar um gargalo temporário, porém ocupa uma vaga que poderia servir diretamente. O sistema favorece **sinergias contextuais**, e não uma classificação universal de melhor personagem.

### 7.4 Química × salário × longo prazo

A química amplifica atributos coletivos, enquanto a folha salarial retira dinheiro diariamente. Um conjunto de maids comuns bem alinhadas pode, em determinadas configurações, entregar melhor relação custo/resultado do que cartas raras incompatíveis. A interação cria uma decisão estratégica persistente: a equipe ótima é aquela que o café consegue **operar e financiar**, não simplesmente a de maior raridade.

### 7.5 Reputação × desbloqueios × pressão de expansão

Bom atendimento melhora reputação; reputação aumenta nível e abre opções; novas vagas e itens permitem reorganizar o serviço. O crescimento abre possibilidades, mas também pode gerar novos custos. O sistema forma um **ciclo de progresso com risco**, em vez de recompensas sem contrapartida.

## 8. Dinâmicas emergentes: três situações de jogo

**Situação A — raridade não significa eficiência.** O jogador recebe uma maid rara, mas a escala fora da posição para substituir uma comum. A eficiência da função pode cair, a química piorar e a despesa aumentar. A próxima rodada cria incentivo para comparar posição, sinergia e custo, e não olhar apenas a moldura.

**Situação B — restaurante bonito, serviço lento.** O jogador compra novas mesas e deixa o quadro inalterado. O espaço disponível cresce, mas pedidos e limpeza se acumulam. O balanço ajuda a perceber que infraestrutura sem equipe suficiente não resolve o gargalo.

**Situação C — ajuda no momento certo.** Um cliente está impaciente e a cozinha prepara um prato. O jogador observa o feedback, acalma o cliente e intervém na faixa ideal do fogão. O resultado desejado é satisfação e ganho incremental perceptíveis, sem apagar o papel da maid que cozinhou.

*As três situações são exemplos analíticos de possibilidades decorrentes das regras, não relatos de sessões reais nem garantia de um resultado numérico específico.*

# PARTE III — EXPERIENCE

## 9. Sentidos: o que o jogador vê e ouve

**Intenção sensorial:** aconchego de cafeteria anime, com personagens expressivos, cores quentes, objetos familiares, detalhes felinos e atmosfera musical leve. A experiência não deve parecer uma planilha convencional, embora apresente dados de gestão. As animações de movimento, indicadores de espera e mudanças contextuais de som devem orientar a atenção.

**Critério observável:** o jogador distingue um cliente urgente de um tranquilo, um botão ativo de um indisponível, uma maid de outra e o resultado financeiro do dia; entende o que está acontecendo mesmo quando há vários personagens na tela. Uma tela visualmente rica que dificulte os cliques ou esconda a simulação contraria esse objetivo.

## 10. Emocional: que sentimentos o jogo procura produzir

| Momento | Emoção pretendida | O que no design a provoca |
|---|---|---|
| Conhecer/obter maid | Curiosidade, afeição, surpresa | Retrato, arquétipo, raridade, revelação |
| Montar escalação | Expectativa e senso de autoria | Escolhas de funções, afinidades e custos |
| Café lotado | Tensão moderada e urgência | Filas, paciência, tempo e sinais de ajuda |
| Atendimento bem-sucedido | Alívio, satisfação e domínio | Serviço concluído, resposta imediata e ganhos |
| Abrir balanço | Curiosidade e responsabilidade | Receitas/despesas visíveis e comparação implícita com decisões |
| Comprar ou expandir | Ambição com cautela | Novas opções versus risco para o caixa |
| Falência | Frustração compreensível e vontade de tentar outra estratégia | Consequência anunciada, explicada e recuperável via novo ciclo |

A meta não é punir aleatoriamente, mas dar significado à vitória e à derrota. Uma falência sem sinalização, um pedido errado sem feedback ou um bônus impossível de compreender produziriam confusão em vez da tensão desejada.

## 11. Intelectual: hipóteses, aprendizagem e autoria

O jogo estimula comparar alternativas e raciocinar sobre restrições: **devo contratar, economizar, mudar a escalação, comprar uma estação, adicionar mesas ou ajudar mais no expediente?** A mesma decisão pode ser boa em um contexto e ruim em outro. A experiência intelectual desejada é aprender a formular um plano, executá-lo, observar indicadores e atualizar a estratégia.

O nível do café oferece uma sequência crescente de escolhas, não apenas de números. Coleção aleatória, diferentes composições e arquétipos geram diversidade de problemas; a rejogabilidade decorre de tentar construir empresas melhores com elencos e oportunidades diferentes.

**Critério de avaliação com pessoas:** após alguns expedientes, o jogador deve conseguir explicar uma decisão sua em termos de causa e efeito — por exemplo, “faltou equipe no salão” ou “a personagem era cara demais para o caixa” — e não apenas afirmar que o resultado foi aleatório.

## 12. Percepção: a interpretação que deve permanecer

O jogador deve compreender quatro mensagens centrais:

1. **Minhas decisões importam.** Uma mudança de escalação e de investimento altera o serviço e o balanço.
2. **O sistema é coerente.** Os resultados vêm de regras visíveis e consequências compreensíveis, mesmo quando há aleatoriedade.
3. **Personagens têm personalidade funcional.** A identidade de cada maid afeta o trabalho, não apenas a aparência.
4. **A empresa precisa ser sustentável.** Crescer requer equilibrar capacidade, qualidade e custo; falhar pode ser parte do aprendizado.

A experiência desejada é a de **ser responsável por um café vivo e adorável**, sem perder o controle sobre uma simulação que recompensa atenção e planejamento.

# PARTE IV — CONEXÃO DDE E VALIDAÇÃO

## 13. Matriz de rastreabilidade: Design → Dynamics → Experience

Esta matriz é o núcleo da análise. Cada linha explicita um elemento projetado, o comportamento que pode surgir e a experiência que se pretende produzir.

| Design controlado pela equipe | Dynamics esperada no jogo | Experience desejada | Como observar/verificar |
|---|---|---|---|
| Cinco funções e posições naturais | Distribuição de tarefas, gargalos e reescalação | Autoria e raciocínio estratégico | Comparar turnos com funções adequadas/inadequadas |
| Química 0–100 e bônus por afinidade | Busca por combinações, não só raridade | Descoberta e domínio | Mudanças no indicador e no desempenho de composições |
| Arquétipos com vantagens e fraquezas | Especialização e compensação entre personagens | Apego com decisões difíceis | Jogador explicar escolha e efeito de uma maid |
| Sete perfis de cliente | Pressões diferentes sobre fila, atendimento e paciência | Variedade, surpresa e tensão justa | Observar resposta a VIP, crítico, fã e estudante |
| Ajuda do dono em oportunidades visuais | Alternância entre supervisão e ação pontual | Agência e satisfação imediata | Comparar expediente sem ajuda e com intervenções válidas |
| Cozinha com faixa ideal e combo | Atenção ao tempo e priorização | Concentração e recompensa por habilidade | Feedback de acerto/erro no fogão e efeito no balanço |
| Dinheiro e fichas sem conversão | Planejamento separado de caixa e coleção | Responsabilidade e expectativa | Checar compras possíveis, saldo e decisões entre dias |
| Salários crescentes por raridade | Contratação gera risco financeiro | Ambição com cautela | Balanço após contratação, sem assumir lucro automático |
| Móveis e capacidade do salão | Espaço interage com fluxo e equipe | Sensação de construir algo próprio | Testar configurações, filas e custo de expansão |
| Balanço financeiro detalhado | Reinterpretação das decisões do turno | Aprendizado e fechamento do ciclo | Jogador reconhecer receita, custo, erro e consequência |
| Direção artística cozy e personagens chibi | Atenção aos personagens e ao ambiente | Acolhimento e vínculo | Teste de leitura, consistência e legibilidade em WebGL |
| Avisos de estado, falha e save | Recuperação sem bloquear progressão | Confiança e justiça | Testes de pré-condição, save inválido e reinício |

## 14. Jornada exemplar de um jogador

**Antes do primeiro expediente.** A pessoa observa caixa, elenco inicial, funções obrigatórias e química. Escala recepção, salão e cozinha e percebe que o número de vagas e o dinheiro são limitados. **Design:** funções e HUD. **Dynamics:** primeira decisão de alocação. **Experience:** expectativa e entendimento de responsabilidade.

**Enquanto o café funciona.** Clientes entram, pedem e esperam. Maids executam tarefas; situações contextuais pedem atenção. O jogador pode ajudar e ver o atendimento avançar. **Design:** agentes, clientes e interações. **Dynamics:** fluxo operacional e intervenção. **Experience:** urgência moderada e agência.

**No balanço.** A tela apresenta o dinheiro que entrou, os custos, ocorrências e o saldo. O jogador identifica uma hipótese de melhoria. **Design:** relatório e economia. **Dynamics:** análise pós-turno. **Experience:** aprendizado, satisfação ou frustração explicável.

**No próximo ciclo.** Com base na hipótese, ele move uma maid, poupa, contrata ou reorganiza o salão. **Design:** ferramentas de gerenciamento. **Dynamics:** experimentação acumulativa. **Experience:** domínio progressivo, autoria e vontade de continuar.

## 15. Evolução do projeto e decisões de design

O DDE deve funcionar como documento vivo. Para evidenciar decisões importantes, registramos a **trajetória** e o motivo, sem transformar propostas não integradas em recursos concluídos.

| Decisão de projeto | Problema ou alternativa | Escolha documentada e justificativa |
|---|---|---|
| Gestão por equipe com suporte ativo | Simulação totalmente passiva reduziria agência | Manter trabalho autônomo e acrescentar intervenções limitadas |
| Cinco funções, não três | Equipes reduzidas ocultavam possibilidades | Recepção, Salão, Cozinha, Palco e Gerência com papéis distintos |
| Química e posição natural | Raridade isolada tende a dominar decisões | Sinergia e adequação geram alternativas estratégicas |
| Duas moedas sem câmbio | Sorte poderia eliminar pressão de caixa | Fichas para caixas; dinheiro para operação e investimentos |
| Progressão sem fases | Gestão é um espaço persistente | Café sobe de nível e desbloqueia opções |
| Análise econômica por simulação | Primeiras curvas tinham falha de sobrevivência/pressão | Ajustes medidos, com limites e pendências de expansão registrados |
| Arte anime 2D ilustrada/cozy | Aparência procedural antiga destoava das personagens | Linguagem ilustrada coesa, sem transformar mockups em interface estática |
| Interface modular dinâmica | PNG de tela inteira fixa preços e botões | Unity mantém textos, dados, estados e cliques reais |
| Salão como planta sobre escritório | Telas genéricas não comunicavam planejamento | Metáfora administrativa aprovada, preservando viewport real |
| Resultado como relatório de café | Um exemplo visual de prejuízo não contempla todos os casos | Mesmo layout deve exibir lucro, prejuízo ou equilíbrio com números reais |

## 16. O que é evidência, intenção ou pendência

**Regras documentadas:** funções, arquétipos, clientes, moedas, preços/probabilidades de caixas, progressão, assistência e indicadores descritos no GDD do repositório. **Evidência técnica parcial:** testes de simulação, EditMode, PlayMode e builds intermediárias reportadas nas PRs e na documentação; as contagens variam conforme o commit e **não certificam a release**. **Direção de arte aprovada:** mockups e pipeline híbrido de imagens modulares + UI funcional; a integração final ainda estava em andamento. **Experiência subjetiva:** hipóteses a verificar com jogadores, não achados experimentais.

A consulta ao GitHub em **10/11/2026** confirmou PRs abertas para Hub/Caixinhas (#79), Elenco (#80) e Equipe/Lojinha (#81), com dependências empilhadas. A correção econômica ECON-02b (#74) também estava aberta. Mudanças posteriores não estão refletidas automaticamente neste documento. Não há prova aqui de publicação final no itch.io nem da integração completa de Salão, Expediente e Resultado.

### 16.1 Riscos prioritários de avaliação

- **Sincronização com o jogo final:** revalidar telas, controles, valores econômicos e estados da última build antes de entregar. Se uma regra mudar, atualizar o texto correspondente.
- **Clareza de interação real:** testar cliques dentro e fora da planta, scroll, botões, ações do expedente e interferência do HUD.
- **Balanceamento:** a expansão do salão e a viabilidade de empresas com investimento estavam sob revisão; não declarar equilíbrio definitivo sem validação.
- **Percepção do jogador:** medir se as pessoas compreendem química, duas moedas, risco dos salários e oportunidades de ajuda sem explicação externa.
- **Áudio e efeitos:** validar no WebGL real; existir arquivo ou código não garante clareza audiovisual.
- **Documento de apresentação:** não usar screenshots de mockups como provas de gameplay; indicar evidências reais do build final.

### 16.2 Roteiro curto de playtest para validar o DDE

Aplicar a 2–5 pessoas que ainda não dominem o sistema, idealmente sem instrução oral além de como iniciar o jogo. Após 2–3 expedientes, observar: (1) conseguem escalar as funções essenciais? (2) percebem por que uma maid fora da função tem consequência? (3) identificam uma chance de ajudar no turno? (4) explicam um item do balanço? (5) compreendem por que melhorar cartas pode piorar o caixa? (6) sabem o que tentar no próximo dia? Registrar resultados e não apresentá-los como realizados antes do teste.

## 17. Relação com as rubricas da disciplina

A rubrica histórica de Projeto 2D anexada ao projeto avalia 12 frentes e explicita **“Game Design Document (GDD — utilizando o DDE)”**. O DDE articula os mesmos temas de maneira causal:

| Rubrica | Onde aparece neste DDE |
|---|---|
| 1. Blueprint — conceito/narrativa | 2, 9–12 |
| 2. Mechanics | 3 |
| 3. Feedback e clareza de interação | 4.2–4.3, 9, 13 |
| 4. UI/HUD e menus | 4 |
| 5. Engajamento e imersão | 6, 8, 10–12 |
| 6. Level design e progressão | 3.9–3.10, 7.5 |
| 7. Design de som e música | 2.5, 9 |
| 8. Efeitos visuais e polimento | 2.5, 4.3, 9 |
| 9. Rejogabilidade | 11, 14 |
| 10. Balanceamento | 3.7–3.9, 7, 16 |
| 11. Estados e erros | 3.10, 13, 16 |
| 12. GDD utilizando DDE | Estrutura integral; 1, 13–17 |

O objetivo não é declarar nota ou qualidade máxima sem evidência: é disponibilizar um documento **organizado, verificável, coerente com o produto e útil para discutir decisões**.

## 18. Fontes, rastreabilidade e atualização

**Referência pedagógica principal:** `06_DDE_E_GDD.pdf`, aula *Frameworks Conceituais*, slides 8–16 — especialmente a divisão de Design/Blueprint–Mechanics–Interface (slide 14), Dynamics/Player–Player–Game (slide 15) e Experience/Sentidos–Emocional–Intelectual–Percepção (slide 16). A aula atribui o DDE a Walk, Görlich e Barrett (2017). Não foram acrescentadas regras externas não presentes nas fontes.

**Rubrica acadêmica histórica:** `Projeto_2D-2025-2.pdf`, pp. 3–9, especialmente item 12 — *GDD utilizando DDE*. A data de entrega **2025** nesse PDF é histórica e não indica prazo da turma de 2026.

**Fonte de produto:** [`docs/GDD.md`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/GDD.md) — regras e números do jogo. **Arquitetura e implementação:** [`docs/ARCHITECTURE.md`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/ARCHITECTURE.md), scripts `Neko.Sim` e `Neko.Game`. **Direção visual:** [`docs/art/CURRENT_VISUAL_DIRECTION.md`](https://github.com/NThuesen/jogos_digitais_2D/blob/main/docs/art/CURRENT_VISUAL_DIRECTION.md) e histórico de integração UI; **informações de PR** consultadas em 10/11/2026.

**Como manter vivo:** atualizar a versão e a data deste documento no mesmo ciclo em que se alterar mecânica ou apresentação relevante; sincronizar a versão do Milanote exigida pelo curso; substituir afirmações pendentes por evidência de build; incluir screenshots reais, créditos de assets, link público do itch.io e o SHA final. **Não alterar `docs/GDD.md` removendo seções técnicas antigas sem revisar seus testes de documentação**, que dependem de títulos e termos específicos.

---

**Síntese DDE:** *Desenhamos* uma equipe de maids com atributos, funções, afinidades, automação e restrições econômicas; *esperamos que surjam* escolhas de composição, adaptação a gargalos e crescimento prudente; *buscamos entregar* acolhimento, agência, descoberta e responsabilidade pelo próprio café. É essa relação entre **Design → Dynamics → Experience** que define Neko Maid Inc.