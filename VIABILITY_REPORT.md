# Brasil Compete — Relatório do Teste de Viabilidade dos Dados

> **Status:** em andamento. Este arquivo é a memória do teste: leia-o primeiro em qualquer sessão nova.
> **Branch:** `feat/viability-test`.
> **Plano:** [VIABILITY_PLAN.md](./VIABILITY_PLAN.md).

## Como retomar

1. Leia as seções 1 (status) e 16 (pendências manuais).
2. O .NET 10 desta máquina está instalado só para o usuário, em `%LOCALAPPDATA%\Microsoft\dotnet`. O `dotnet` do PATH aponta para o SDK 8 e não serve. Use `& "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe"` ou os scripts de `backend/scripts/`.
3. O PC é corporativo e sem administrador: só peça permissão quando for realmente necessário.

---

# 1. Status das fases

| Fase | Situação | Data | Observações |
| --- | --- | --- | --- |
| 0 — Entender o projeto | Concluída | 09/10/2026 | Convenções na seção 3 |
| 1 — Base do worker | Concluída | 09/10/2026 | 65 testes passando; `events.json` gerado a partir da curadoria |
| 2 — Identidade e entidades de referência | Concluída | 09/10/2026 | Catálogo do Wikidata com 10.331 pessoas em 10 perfis; `backend/validation/reference-entities.json` (seção 6) |
| 3 — Fontes sem chave | Concluída | 09/10/2026 | Jolpica, Lichess, Wikipedia (futebol, UFC, tênis e tênis de mesa) e Liquipedia; TheSportsDB descartado; FIVB vai para a Parada A (seção 5) |
| 4 — Consolidação | Em andamento | 09/10/2026 | Revisão dos descartados achou e corrigiu três falhas (seção 10.2) |
| 5 — Validação da janela passada | Em andamento | 09/10/2026 | Janela de 08/09 a 07/10/2026; gabarito sendo montado |
| Parada A — Chaves de API | Pendente | | |
| 6 — Fontes com chave | Pendente | | |
| 7 — Preparar a coleta da janela futura | Pendente | | |
| Parada B — Coleta e revisão | Pendente | | |
| 8 — Análise final e veredito | Pendente | | |

---

# 2. Resumo executivo e veredito

Preencher na fase 8.

---

# 3. Entendimento e convenções adotadas

## 3.1. O produto

O Brasil Compete é um app (Expo, React Native) com a agenda de eventos esportivos internacionais em que há brasileiros competindo: seleções, clubes brasileiros em competições internacionais, atletas de modalidades individuais e organizações de eSports. Há duas visões: o feed principal (equipes brasileiras e atletas de modalidades individuais) e a de Indivíduos (brasileiros em equipes estrangeiras). A coleta precisa ser automática, gratuita e dentro da lei e dos termos de uso das fontes. Este teste responde se isso é possível e com que qualidade.

## 3.2. Convenções do repositório (confirmadas lendo o código)

Correções em relação à seção 2.3 do plano:

* O estilo do app passou a usar styled-components (`styled-components/native`) em arquivos `style.ts`. O NativeWind e o `StyleSheet.create` foram removidos em 09/10/2026, a pedido do usuário, antes do início do teste.
* Cada tela ou componente tem a própria pasta, com `.tsx` (só renderização), `types.ts`, `style.ts` e o hook `use<Nome>.ts`. Os tipos ficam em `types.ts`, e não em `*.types.ts`.
* O componente raiz é `src/RootLayout/` (não pode existir `src/app` nem `src/App`, que o Expo Router usaria como raiz das rotas).

Confirmadas:

* Expo SDK 54, Expo Router, React Native e TypeScript em modo `strict`, com o alias `@/` apontando para `src/`;
* Os arquivos de `app/` só reexportam componentes de `src/`;
* Componentes são funções com export nomeado; props e modelos usam `type`;
* Chaves de estilos e props JSX em ordem alfabética;
* Imports em grupos separados por linha em branco: bibliotecas externas, `@/` e caminhos relativos;
* Aspas simples, ponto e vírgula e indentação de 2 espaços;
* Identificadores em inglês; textos de interface em português do Brasil;
* Commits no formato `tipo: descrição em português no presente` (ex.: `refactor: separa telas e componentes em tsx, types, style e hook com styled-components`);
* Documentação em português, com seções numeradas e separadores `---`.

## 3.3. Convenções do backend do teste (C#)

Seguem o `PROJECT_GUIDE.md` (seções 15 a 24 e 40 a 51) e a regra de separação de responsabilidades do usuário, adaptada ao C#:

* Código em `backend/`, nunca em `src/` (que pertence ao app). Não há arquivos TypeScript no backend, para não afetar `npm run typecheck` e `npm run lint`;
* .NET 10, console app com Generic Host, injeção de dependência, configuração, logs, `HttpClientFactory` e User Secrets carregados em qualquer ambiente;
* PascalCase, métodos assíncronos com sufixo `Async` e `CancellationToken` propagado;
* Um tipo por arquivo, `namespace` por arquivo, `nullable` ativo, `record` para dados imutáveis;
* Cada integração em `Integrations/<Fonte>/`, com `Client` (HTTP), contratos externos isolados (sufixo `Response`), `Mapper` (contrato externo para o modelo interno), `Options` (configuração) e, quando houver regra de limpeza, `Normalizer`. O formato externo nunca sai da pasta da integração;
* Tipos sem `dynamic` nem `object` quando a estrutura é conhecida;
* Datas em UTC, sempre com o fuso ou offset original; o horário nunca é inventado;
* Logs estruturados, sem segredos;
* Testes com xUnit e as asserções do próprio xUnit, sem internet (respostas reais salvas em `Fixtures/`);
* Segredos só em User Secrets ou variáveis de ambiente;
* Requisições identificadas com `BrasilCompete-Viability/0.1 (+https://github.com/AlexandreAT/BrasilCompete)`.

## 3.4. Decisões de modelagem do worker (fase 1)

* **Modelo:** `SportEvent` (e não `Event`, para não confundir com a palavra-chave `event` do C#), com `Participant`, `Schedule` e os enums do plano (`EventFormat`, `SchedulePrecision`, `EventView`). Uma equipe ou dupla lista os atletas em `Members`: é assim que "Gui Santos (Warriors)" aparece num jogo Warriors x Lakers.
* **Datas:** em eventos com horário, `startUtc` guarda o instante e `date` guarda o dia no horário de Brasília. Eventos "A confirmar" nunca ficam fora da janela.
* **Alcance da competição:** cada adapter decide se a competição é internacional, liga nacional estrangeira (só entra pela visão de Indivíduos) ou liga nacional brasileira (nunca entra), a partir de um catálogo de competições em configuração (plano, seção 9.6).
* **Visão:** em modalidades individuais (F1, xadrez, MMA, tênis, tênis de mesa, vôlei de praia), atleta que representa o Brasil vai para o feed principal, e quem só nasceu no Brasil vai para Indivíduos. Em modalidades de equipe, equipe brasileira em competição internacional vai para o feed principal, e brasileiro em equipe estrangeira vai para Indivíduos.
* **Identificador:** `modalidade:competição:participantes[:fase]:data`, determinístico. Quando o evento ganha data ou muda de dia, o histórico o reconhece pela chave sem a data e registra a mudança, em vez de contar um evento novo.
* **Deduplicação:** mesma modalidade, formato e participantes (ID do Wikidata, ou nome normalizado), com até um dia de diferença. Participações (como sessões da F1) só se juntam com a mesma fase, e dois eventos com IDs diferentes na mesma fonte nunca se juntam. A curadoria manual vence; nas demais fontes, vale o horário mais preciso, com desempate pela prioridade configurada. Horários e datas divergentes viram conflitos registrados.
* **Acesso às fontes:** cada fonte tem um cliente HTTP com, de fora para dentro, cache em disco, uma requisição por vez com intervalo mínimo, resiliência (3 tentativas, backoff exponencial, respeito ao `Retry-After`, espera configurável em 429 sem cabeçalho) e contagem de requisições e 429.

---

# 4. Desvios do plano e motivos

| # | Desvio | Motivo |
| --- | --- | --- |
| 1 | O app foi alterado antes do teste (refatoração para styled-components e nova estrutura de pastas) | Pedido explícito do usuário, em commit próprio (`dc1e5a4`), fora do escopo do teste |
| 2 | O `PROJECT_GUIDE.md` foi atualizado (styling, estrutura de componentes, .NET 10 e asserções do xUnit) em vez de só receber propostas | Orientação do usuário: não deixar documentação falsa |
| 3 | O .NET 10 foi instalado por usuário (sem administrador), fora do PATH padrão | PC corporativo sem administrador; o `dotnet` do sistema só tem o SDK 8 |
| 4 | Script `backend/scripts/worker.ps1`, além do `collect.ps1` previsto | Encontra o .NET 10 mesmo quando o `dotnet` do PATH é outra versão, e serve de base para o `collect.ps1` |
| 5 | Logs com o console do `Microsoft.Extensions.Logging`, sem Serilog | O teste não precisa de destinos de log; o código usa `ILogger`, então trocar pelo Serilog depois é só configuração |
| 6 | Saída extra `discarded.json` | Guarda os descartados com o motivo (plano, seção 9.6) para medir falsos positivos e revisar por amostragem |
| 7 | O TheSportsDB (fase 3.6) não virou adapter | O plano grátis devolve no máximo 1 a 15 resultados por consulta (seção 5.7). Não serve como segunda fonte de futebol nem de UFC; sem ele, não há checagem cruzada nessas modalidades |
| 8 | O tênis de mesa (fase 3.9) usa a Wikipedia, e não a curadoria | As páginas dos eventos da WTT na Wikipedia têm chaves no mesmo formato das do tênis, e o adapter do tênis passou a lê-las |
| 9 | No UFC, o país do lutador vem da "List of current UFC fighters" (Wikipedia), além do Wikidata | O card não traz bandeiras e o Wikidata quase não tem o país esportivo no MMA: só com ele, 9 das 19 lutas com brasileiro da janela passada se perdiam (seção 6.3) |
| 10 | Na Olimpíada de Xadrez, o evento é o confronto entre equipes por rodada ("Brasil x Botsuana", com os brasileiros como membros), e não cada tabuleiro | É assim que o público acompanha o torneio por equipes; os tabuleiros viram detalhe |
| 11 | Duplas do tênis e do tênis de mesa ficam de fora | As chaves de duplas têm dois atletas por lado; o adapter conta os confrontos ignorados na nota da fonte. Fica como lacuna medida |
| 12 | Na Liquipedia, além de `action=query`, uma requisição `action=expandtemplates` por torneio | É o único jeito de ligar o nome curto do time nas chaves ("furia", "vit") à página dele. Os termos só citam o `parse` como pesado; por cautela, essa requisição segue o mesmo intervalo de 30 s |

---

# 5. Fontes

Termos e documentação lidos em 09/10/2026. Os custos são da coleta da janela passada (08/09 a 07/10/2026); "fria" é a execução sem cache.

## 5.1. Resumo

| Fonte | Modalidades | Licença dos dados | Limite | Como o worker respeita | Custo medido |
| --- | --- | --- | --- | --- | --- |
| Jolpica-F1 | F1 | CC BY-NC-SA 4.0, uso não comercial | 4 req/s em rajada, 500 req/h | 500 ms entre requisições, cache de 6 h | 2 requisições, menos de 1 s |
| Lichess | Xadrez | API aberta (código AGPL); dados públicos das transmissões | Uma requisição por vez; após um 429, esperar um minuto | Uma por vez, 2 s entre requisições (era 1 s), 60 s após 429, cache de 6 h | 342 requisições e 12 respostas 429 em 19 min com 1 s de intervalo |
| Wikidata (WDQS) | Identidade | CC0 | 60 s por consulta, 60 s de processamento por minuto, 5 consultas paralelas | Uma consulta por vez, 2 s de intervalo, cache de 7 dias | 91 consultas para o catálogo inteiro |
| Wikipedia (REST, HTML do Parsoid) | Futebol, UFC, tênis, tênis de mesa | CC BY-SA 4.0 (o teste guarda só fatos) | Política de User-Agent da Wikimedia; sem limite fixo para uso sequencial | 1 s entre requisições, cache de 6 h | Cerca de 20 páginas por execução, poucos segundos |
| Liquipedia (API do MediaWiki) | eSports | CC BY-SA 3.0, com atribuição | 1 req a cada 2 s; `action=parse` 1 a cada 30 s; gzip e User-Agent obrigatórios | 2,1 s entre requisições, 30 s antes de cada `expandtemplates`, cache de 12 h, sem `parse` | 21 requisições em 48 s, antes do `expandtemplates`, que acrescenta 2 requisições e 30 s de espera |
| TheSportsDB (chave "123") | — | Termos indisponíveis (erro 502) | 30 req/min e 1 a 15 resultados por consulta | Não implementado (desvio 7) | 3 consultas de sondagem |
| FIVB (VIS Web Service) | Vôlei e vôlei de praia | Sem licença explícita | Não documentado | Não implementado: exige identificador de aplicação (Parada A) | — |

Com cache, a execução completa leva cerca de 40 s. Desses, 30 s são a espera de cautela do `expandtemplates` da Liquipedia, que também vale para respostas que vêm do cache. A execução fria ainda precisa ser medida com o novo intervalo da Lichess; a estimativa é de 12 a 13 minutos, dominados pela Lichess.

## 5.2. Jolpica-F1 (fórmula 1)

* **Uso:** `GET /ergast/f1/2026/races/` (calendário, com o horário de cada sessão em UTC) e `GET /ergast/f1/2026/driverstandings/` (pilotos, nacionalidade e equipe).
* **Termos:** [TERMS.md](https://github.com/jolpica/jolpica-f1/blob/main/TERMS.md) ("The data is licensed under Creative Commons Attribution-NonCommercial-ShareAlike 4.0"; uso comercial só com contato pelo admin@jolpi.ca) e [limites](https://github.com/jolpica/jolpica-f1/blob/main/docs/rate_limits.md) ("4 requests per second" e "500 requests per hour", sujeitos a mudança). Abuso pode levar a bloqueio sem aviso.
* **Impacto:** o uso não comercial combina com o produto, que é sem anúncios e sem monetização (plano, seção 4.6). Se isso mudar, a fonte sai.
* **Resultado:** 276 participações (22 pilotos × sessões) e 6 do Bortoleto na janela, todas com horário. Os nomes vêm como a fonte publica (ex.: "Bahrain Grand Prix in Malaysia").

## 5.3. Lichess (xadrez)

* **Uso:** transmissões oficiais (`/api/broadcast`, `/api/broadcast/top` para as passadas), detalhe de cada torneio (`/api/broadcast/{id}`), jogadores (`/broadcast/{id}/players`) e rodadas (`/api/broadcast/-/-/{roundId}`).
* **Termos:** [especificação da API](https://github.com/lichess-org/api/blob/master/doc/specs/lichess-api.yaml) ("Only make one request at a time"; após um 429, "waiting one minute before retrying will be sufficient") e [termos de serviço](https://lichess.org/terms-of-service).
* **Achados:** as listas mostram uma só transmissão por grupo (a Olimpíada tem várias, por faixa de mesas); o worker expande os grupos. Os jogadores só aparecem depois que o torneio os publica (2 dos 319 torneios da janela ainda não tinham). Só entram as transmissões oficiais da Lichess, não as de usuários. Na janela, 18 torneios tinham brasileiros, 16 deles internacionais: todos são partes da Olimpíada de Xadrez (aberta, feminina e para pessoas com deficiência). Os outros 2 eram torneios brasileiros, descartados corretamente.

## 5.4. Wikidata (identidade)

* **Uso:** consultas SPARQL em duas etapas (candidatos por critério, depois detalhes em lotes de 200), só pelo comando `identity`, com cache de 7 dias.
* **Termos:** dados em CC0; [limites do WDQS](https://www.mediawiki.org/wiki/Wikidata_Query_Service/User_Manual#Query_limits) e [política de User-Agent](https://foundation.wikimedia.org/wiki/Policy:Wikimedia_Foundation_User-Agent_Policy).

## 5.5. Wikipedia (futebol, UFC, tênis e tênis de mesa)

* **Uso:** `GET /w/rest.php/v1/page/{título}/html` (HTML do Parsoid). As predefinições são lidas pelo atributo `data-mw` (nome e parâmetros em JSON), sem parser de wikitext.
* **Termos:** textos em [CC BY-SA 4.0](https://en.wikipedia.org/wiki/Wikipedia:Text_of_the_Creative_Commons_Attribution-ShareAlike_4.0_International_License); o teste guarda só fatos (datas, horários, participantes) e a URL de cada página. [Etiqueta da API](https://www.mediawiki.org/wiki/API:Etiquette) e política de User-Agent da Wikimedia.
* **Páginas lidas:** fases finais da Libertadores e da Sul-Americana, páginas das seleções masculina e feminina, "List of UFC events" e o card de cada evento, "List of current UFC fighters", chaves de simples de tênis (US Open, Tóquio e China Open) e páginas de eventos da WTT (Champions Macau, Contender Panagyurishte, Star Contender Astana e China Smash).
* **Achados:**
  * As predefinições variam: "Football box", "Football box collapsible" e o redirecionamento "footballbox collapsible" (este último fazia os jogos da seleção masculina sumirem; corrigido). Páginas são renomeadas e viram redirecionamentos.
  * Times sem link nos jogos de volta são resolvidos pelos times já vistos na mesma página.
  * No tênis e no tênis de mesa, a página traz a chave, mas não o dia de cada jogo: os eventos ficam com o período do torneio.
  * A Wikipedia só tem páginas de alguns eventos da WTT de 2026 (Champions, Star Contender, Contender e China Smash). Nem todo torneio tem página.
  * O catálogo de páginas (quais torneios ler e em que período) é mantido à mão. Esse esforço entra na seção 11.

## 5.6. Liquipedia (eSports)

* **Uso:** `api.php?action=query` com wikitext (páginas dos torneios, páginas `Match:` e páginas dos times), lista de páginas por prefixo (`list=allpages`), a tabela de fusos `Module:Timezone/Data` da wiki `commons` e `action=expandtemplates` com `{{TeamPage|nome}}` para achar a página de cada time.
* **Termos:** [API Terms of Use](https://liquipedia.net/api-terms-of-use): "no more than 1 request per 2 seconds"; `action=parse` "should not exceed 1 request per 30 seconds"; gzip e User-Agent com contato; atribuição CC BY-SA 3.0. O HTML das páginas não é lido.
* **Achados:** no VALORANT, as partidas ficam em páginas `Match:`; no CS2, dentro da chave do torneio. Nas chaves, o time aparece pelo nome curto ("furia", "vit"), que antes deixava 6 dos 8 times da StarSeries sem país. A abreviação do fuso (ex.: CST) só existe na tabela do `commons`. O catálogo de torneios também é mantido à mão.

## 5.7. TheSportsDB (fase 3.6)

* **Plano grátis** ([documentação](https://www.thesportsdb.com/documentation)): chave "123", só a API v1, 30 requisições por minuto. Limites por rota: próximos e últimos jogos de um time, 1 resultado (só os jogos em casa); jogos de um dia, 3; jogos da temporada, 15; busca de times, "limited to just Arsenal" (na prática, a busca por "Flamengo" funcionou).
* **Sondagem (09/10/2026):** a temporada 2026 da Libertadores devolveu 5 jogos, todos da fase preliminar de fevereiro; os jogos do dia 16/09/2026 devolveram 3 jogos de ligas dos Estados Unidos. A página de termos respondeu 502.
* **Decisão:** não vale um adapter (desvio 7). O plano pago (100 req/min e sem os cortes) seria a forma de ter uma segunda fonte de futebol e UFC; não foi contratado.

## 5.8. FIVB, VIS Web Service (fase 3.7)

* A [documentação do VIS](https://www.fivb.org/VisSDK/VisWebService/) exige que todo cliente envie um identificador de aplicação. Ele é pedido à FIVB por e-mail (o endereço está na página de introdução da documentação) e pertence à aplicação, não à pessoa.
* Não há licença explícita nem limites publicados. O pedido do identificador também serve para perguntar se o uso é permitido.
* Vai para a Parada A. Até lá, vôlei e vôlei de praia ficam sem fonte.

---

# 6. Identidade

## 6.1. Catálogo do Wikidata

Gerado pelo comando `identity` em 09/10/2026 (91 consultas). Entra quem tem a ocupação da modalidade e pelo menos um critério: representa o Brasil (país esportivo, P1532), nasceu no Brasil (P19 no Brasil) ou tem cidadania brasileira (P27). "Só cidadania" não é critério de inclusão (plano, seção 4.1) e é contado à parte.

| Perfil | Pessoas | Representa o Brasil | Só nasceu no Brasil | Só cidadania | Representa outro país | Com artigo na Wikipedia em inglês | IDs externos |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Fórmula 1 | 36 | 7 | 27 | 2 | 0 | 36 | — |
| Xadrez | 313 | 286 | 16 | 11 | 4 | 55 | FIDE 271, Lichess 13 |
| MMA | 458 | 24 | 368 | 66 | 5 | 390 | Sherdog 429, UFC 250, Tapology 240 |
| Tênis | 239 | 103 | 92 | 44 | 9 | 195 | ATP 149, WTA 64 |
| Tênis de mesa | 48 | 9 | 29 | 10 | 1 | 37 | WTT 7 |
| Vôlei | 481 | 191 | 212 | 78 | 0 | 160 | FIVB 73 |
| Vôlei de praia | 195 | 65 | 125 | 5 | 0 | 129 | FIVB 127 |
| Basquete | 1.002 | 655 | 46 | 301 | 42 | 87 | NBA 19, Basketball Reference 21 |
| eSports | 51 | 0 | 12 | 39 | 0 | 3 | Liquipedia 40, HLTV 2 |
| Futebol | 7.508 | 4.879 | 1.591 | 1.038 | 133 | 5.966 | Transfermarkt 5.582 |
| **Total** | **10.331** | | | **1.594** | | | |

## 6.2. Lacunas

* **País esportivo (P1532) quase vazio fora do futebol, do xadrez e do basquete:** 0 de 51 no eSports, 24 de 458 no MMA, 9 de 48 no tênis de mesa. Nessas modalidades, "representa o Brasil" precisa vir da fonte (bandeira no card, federação, infobox).
* **IDs externos raros:** WTT em 7 de 48 pessoas do tênis de mesa, NBA em 19 de 1.002 do basquete. Sem ID, a ligação com a fonte é pelo artigo da Wikipedia ou pelo nome.
* **Atletas conhecidos sem país esportivo:** João Fonseca, Kerolin e Bruna Takahashi aparecem só como "nasceu no Brasil" (entram do mesmo jeito).
* **Homônimos:** Raphinha (Q28861547, Barcelona; e Q24451790, nascido em 1993), Gui Santos (o jogador da NBA e um registro só com cidadania), Alex Pereira (lutador e jogador de futebol). A busca por nome tem confiança baixa e não decide sozinha.
* **Time atual desatualizado:** o Gui Santos não tem time atual (P54 sem data de fim); a Marta aparece com três clubes "atuais". O time atual precisa vir das fontes da modalidade.
* **Equipes e organizações:** o catálogo só cobre pessoas. Clubes, seleções e organizações de eSports são identificados pela fonte (bandeira, sede no infobox). A LOUD existe no Wikidata (Q104283618), mas não está no catálogo.

## 6.3. Identidade nas coleções

Na coleta da janela passada, a fonte decidiu a nacionalidade de quase todos os participantes, e o Wikidata completou 10. Uma pessoa foi marcada como "só cidadania" e ficou de fora.

O caso do MMA mostra o limite do Wikidata: com o card (sem bandeiras) e o catálogo, 10 lutas com brasileiro foram identificadas. Com a bandeira da "List of current UFC fighters", foram 19. As 9 que faltavam eram de lutadores sem artigo na Wikipedia (Rodolfo Bellato, Elves Brener, Djorden Santos e outros), fora do catálogo, ou só com cidadania (Valesca Machado). Na janela, todos os 102 lutadores dos cards estavam no elenco atual. Ainda assim, quem for dispensado depois da luta sai da lista, e quem estreia pode demorar a entrar.

## 6.4. Entidades de referência

Em `backend/validation/reference-entities.json`: 29 entidades, com QID, IDs externos, time atual e presença na janela passada. Das 29, 18 apareceram em eventos: Bortoleto, Calderano, Takahashi, Supi, Fier, Pantoja, a seleção masculina, LOUD, FURIA, MIBR e os 8 clubes da CONMEBOL. As que não apareceram se dividem em dois grupos:

* **Sem evento na janela:** Alex Pereira, a seleção feminina e a paiN. João Fonseca e Bia Haddad Maia não aparecem nas chaves lidas; a conferência fica com o gabarito.
* **Sem fonte antes da Parada A:** vôlei, vôlei de praia e a visão Indivíduos (Gui Santos, Vinícius Júnior, Raphinha e Kerolin).

---

# 7. Cobertura e exatidão por modalidade e por visão

Preencher nas fases 5 e 8.

---

# 8. Volume por visão e por modalidade

Preencher nas fases 5 e 8.

---

# 9. Antecedência e estabilidade

Preencher na fase 8.

---

# 10. Deduplicação e conflitos entre fontes

## 10.1. Deduplicação

Na janela passada, cada modalidade teve uma única fonte. Por isso não houve junção entre fontes nem conflito. As 6 junções da primeira coleta eram o mesmo evento da F1 gerado duas vezes: as sessões configuradas no `appsettings.json` se somavam à lista padrão. Depois da correção, não sobrou nenhuma junção. Sem o TheSportsDB, a junção entre fontes só será exercitada com as fontes da Parada A (football-data.org e PandaScore) e com a curadoria.

## 10.2. Revisão dos descartados (fase 4)

Revisão por amostragem dos 1.376 descartados da janela passada, agrupados por motivo, modalidade e competição.

| Motivo | Quantidade | Revisão |
| --- | --- | --- |
| Sem brasileiro (tênis, tênis de mesa, F1, eSports) | cerca de 1.100 | Corretos: confrontos e participações de estrangeiros nas chaves e no grid |
| Sem brasileiro (MMA) | 41 → 32 | 9 eram lutas de brasileiros não identificados; corrigido com o elenco do UFC (seção 6.3) |
| Sem brasileiro (eSports) | 25 | Antes da correção, FURIA e MIBR eram descartadas: o nome curto do time não levava ao país. Corrigido com o `{{TeamPage}}` |
| Sem brasileiro (futebol) | 2 | Corretos (Cienciano x Montevideo City Torque, na Sul-Americana) |
| Não internacional (xadrez) | 250 | Corretos: dois torneios brasileiros (Manaus e São Paulo) |
| Fora da janela | 3 | Corretos: eventos de teste da curadoria |

A seleção masculina não aparecia em nenhum descarte: o leitor nem enxergava as partidas, porque a predefinição tinha outro nome (seção 5.5). Esse tipo de falha só aparece comparando com um gabarito, o que a fase 5 faz.

---

# 11. Curadoria manual necessária

## 11.1. Catálogos de competições (fase 3)

Mesmo nas fontes automáticas, alguém precisa dizer quais páginas ler:

| Catálogo | Itens na janela passada | Frequência | Esforço estimado |
| --- | --- | --- | --- |
| Torneios da Liquipedia (VALORANT, CS2, LoL) | 6 páginas | Por torneio internacional com organização brasileira | 2 a 3 minutos por torneio |
| Chaves de tênis e eventos da WTT (título e período) | 9 páginas | Por torneio; a página da chave só existe perto do início | 2 minutos por torneio |
| Páginas de futebol (fases finais da CONMEBOL e seleções) | 4 páginas | Por temporada e fase | 2 minutos por página |

Na janela passada, isso daria uns 20 itens por mês, cerca de 40 a 60 minutos mensais. O número real sai da coleta futura (fase 8).

## 11.2. Eventos que só a curadoria cobre

* **Amistosos de clubes** (ex.: "Santos x Barcelona"): nenhuma fonte do teste os traz.
* **Vôlei e vôlei de praia:** até a FIVB responder (Parada A).
* **Duplas do tênis e do tênis de mesa.**
* **Tênis e tênis de mesa fora do catálogo:** um torneio sem página na Wikipedia (ou não catalogado) só entra pela curadoria.

---

# 12. Execução

* **Como rodar:** `.\backend\scripts\worker.ps1 collect --from aaaa-mm-dd --to aaaa-mm-dd` (detalhes em `backend/README.md`).
* **Fase 1:** só com a curadoria (4 eventos de teste), a execução leva 0,3 s e gera `events.json`, `discarded.json`, `run-summary.json` e `agenda.md`.
* **Catálogo de identidade:** `.\backend\scripts\worker.ps1 identity` gera `backend/.state/identity/wikidata-catalog.json` e `backend/output/identity/summary.md` (91 consultas ao Wikidata).
* **Janela passada (fase 3):** a primeira execução fria levou 20 minutos, 19 deles na Lichess (342 requisições e 12 respostas 429, cada uma com espera de 60 s). O intervalo da Lichess subiu de 1 s para 2 s. Com cache, a execução completa leva cerca de 40 s.
* **Execução parcial:** `--sources "jolpica,lichess"` roda só as fontes indicadas (no PowerShell, a lista vai entre aspas). Nesse caso, o histórico não marca como ausentes os eventos das fontes que não rodaram.

---

# 13. Modalidades fora do teste

Só listadas (plano, seção 7). A "fonte provável" é a primeira a investigar, sem garantia de que os termos permitam o uso automatizado.

| Modalidade | Brasileiros de destaque | Fonte provável | Observação |
| --- | --- | --- | --- |
| Surfe | Circuito mundial (WSL) | Wikipedia (páginas de cada etapa do CT); site da WSL | Etapas com janela de vários dias e horário definido no dia (como o tênis: período) |
| Skate | Street e park (World Skate, SLS) | Wikipedia; sites da World Skate e da SLS | Calendário irregular |
| Judô | Circuito da IJF | Site da IJF (o próprio site usa uma API de dados; verificar termos) | Muitos atletas por evento; bom candidato a período por evento |
| Ginástica | Copas do Mundo e Mundial (FIG) | Site da FIG; Wikipedia para Mundiais | Poucos eventos por ano |
| Atletismo | Diamond League e Mundiais (World Athletics) | Site da World Athletics (calendário e listas de inscritos) | Inscrições saem perto da prova |
| Natação | Copa do Mundo e Mundiais (World Aquatics) | Site da World Aquatics; Wikipedia para Mundiais | Poucos eventos por ano |
| Basquete FIBA | Seleções (eliminatórias, AmeriCup) | Site da FIBA; Wikipedia (mesmas predefinições de jogos do futebol, em parte) | Janelas FIBA, poucas datas por ano |
| Handebol | Seleções (IHF) | Site da IHF; Wikipedia para Mundiais | Poucas datas por ano |

---

# 14. Riscos e limitações aceitas

* Atletas pouco conhecidos, que não aparecem nas fontes nem no Wikidata, não serão capturados (limitação aceita no plano, seção 4.1).
* **Dependência de páginas mantidas por voluntários (Wikipedia e Liquipedia):** formatos variam e mudam (predefinições renomeadas, redirecionamentos, partidas que mudam de lugar). Cada mudança pode exigir ajuste no leitor; os incidentes da coleta futura vão medir isso.
* **Catálogos mantidos à mão:** as páginas de torneios (tênis, tênis de mesa, eSports, futebol) precisam ser incluídas a cada temporada ou torneio. Um torneio fora do catálogo não é coletado (seção 11).
* **Sem segunda fonte nas modalidades da Wikipedia:** sem o TheSportsDB, futebol, UFC, tênis e tênis de mesa não têm checagem cruzada.
* **Elenco "atual" do UFC:** lutadores dispensados depois da luta saem da lista, e quem estreia pode demorar a entrar.
* **Arquivos locais crescem a cada execução:** uma pasta por execução em `backend/output/runs/` (cerca de 1 MB cada, quase tudo da lista de descartados) e o cache em `backend/.cache/` (82 MB depois de 14 execuções). Tudo fica fora do Git e do app, mas o produto precisará de uma regra de retenção (ex.: manter só as últimas execuções e apagar o cache vencido).

---

# 15. Recomendações para o plano final

Preencher na fase 8.

---

# 16. Pendências manuais

Nenhuma no momento.
