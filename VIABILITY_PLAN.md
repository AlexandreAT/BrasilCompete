# Brasil Compete — Plano do Teste de Viabilidade dos Dados

> **Status:** documento temporário, criado em 08/10/2026.
> **Local:** raiz do repositório (`VIABILITY_PLAN.md`).
> **Para quem:** a IA que vai executar o teste de viabilidade.
> **Depois do teste:** se o resultado for positivo, este arquivo será substituído pelo plano final de implementação.

---

# 1. Como usar este documento

## 1.1. Para quem é e o que fazer primeiro

Você é a IA responsável por executar o teste de viabilidade do Brasil Compete. Leia este documento inteiro antes de qualquer ação. Em seguida, leia o projeto inteiro (seção 2). Só depois comece a implementar.

Este plano foi escrito a partir de uma análise feita sem acesso direto às APIs esportivas (o ambiente da análise bloqueava a rede). Por isso, cada informação sobre fontes indica se foi verificada ou não. O que não foi verificado precisa ser confirmado por você antes de virar premissa.

## 1.2. Ordem de trabalho

1. Ler o projeto e registrar as convenções (seção 2).
2. Entender a ideia do produto e as decisões já tomadas (seções 3 e 4).
3. Entender o objetivo, as regras e o escopo (seções 5 a 7).
4. Executar as fases na ordem (seção 10), parando apenas nos pontos de parada.
5. Manter o relatório de viabilidade sempre atualizado (seção 14).

## 1.3. Liberdade de implementação

Este documento é um guia, não uma receita. Você pode mudar tecnologia, estrutura de pastas, ordem das fases, fontes de dados, nomes ou a forma de medir, desde que todas as condições abaixo sejam verdadeiras:

* A alternativa é realmente melhor ou mais simples, e não apenas uma preferência diferente;
* Ela continua respondendo às perguntas do teste (seção 5);
* Ela respeita as regras inegociáveis (seção 6);
* A mudança fica registrada no relatório, na seção "Desvios do plano", com o motivo.

Diante de duas opções equivalentes, siga o plano.

## 1.4. Autonomia e pontos de parada

Trabalhe de forma autônoma. Não pare para pedir aprovação de decisões técnicas: decida, registre no relatório e continue.

Pare somente quando precisar de algo que apenas o usuário pode fazer, como criar uma conta, gerar uma chave de API, instalar algo na máquina dele, liberar acesso de rede ou revisar dados de validação. Antes de parar, avance tudo o que for possível sem essa ação. O formato da parada está na seção 13.

## 1.5. Precedência

* Padrões de código, organização e escrita: valem o código existente e o `PROJECT_GUIDE.md`.
* Escopo, objetivo e regras do teste: vale este documento.
* Em caso de conflito real, siga este documento para o escopo do teste e registre o conflito no relatório.

---

# 2. Fase 0 — Entender o projeto antes de escrever código

## 2.1. Leia tudo

Leia, por inteiro:

* `README.md`;
* `PROJECT_GUIDE.md`, com atenção especial às seções 3 (eventos, não jogos), 10 (princípios), 15 a 24 (entidades, DTOs, validação, datas, integrações, scraping, normalização, jobs, cache e logs) e 40 a 51 (segurança, erros, testes, nomenclatura, Clean Code, SOLID, orientações para IA e Definition of Done);
* `AGENTS.md` e `CLAUDE.md`;
* `package.json`, `app.json`, `tsconfig.json`, `eslint.config.js` e `.gitignore`;
* todos os arquivos de `app/` e `src/`;
* o histórico do Git (`git log`), para entender o padrão das mensagens de commit.

## 2.2. O que extrair

Antes de escrever código, identifique e anote:

* estrutura de pastas e onde cada tipo de arquivo vive;
* nomenclatura de arquivos, pastas, tipos, funções, componentes e constantes;
* idioma do código e idioma dos textos;
* padrões de escrita: aspas, ponto e vírgula, indentação, ordem de imports, ordem de propriedades, `type` ou `interface`, forma de exportar;
* padrões de organização e design: separação de responsabilidades, tipagem, reutilização;
* padrões de documentação;
* padrões de commit.

## 2.3. Convenções já observadas (confirme lendo o código)

Na análise, observamos o seguinte. Confirme cada item e corrija o que estiver errado:

* App em Expo SDK 54, Expo Router, React Native e TypeScript em modo `strict`, com o alias `@/` apontando para `src/`;
* Os arquivos de `app/` só fazem a ligação de rotas e reexportam páginas de `src/pages` (ex.: `export { Agenda as default } from '@/pages/Agenda/Agenda';`);
* Páginas ficam em `src/pages/<Pagina>/<Pagina>.tsx`, com componentes locais em `components/` e dados de exemplo em `*.mock.ts`; cada tela ou componente tem a própria pasta, com `.tsx` (só renderização), `types.ts`, `style.ts` e o hook `use<Nome>.ts` (seção 28 do `PROJECT_GUIDE.md`, atualizada em 09/10/2026);
* Componentes são funções com export nomeado; props e modelos usam `type`;
* O estilo usa styled-components (`styled-components/native`) em `style.ts`, com os tokens de `src/shared/themes/Theme.ts` (cores e fontes); o NativeWind foi removido em 09/10/2026;
* Chaves de estilos e props JSX ficam em ordem alfabética;
* Imports ficam em grupos separados por linha em branco: bibliotecas externas, depois `@/`, depois caminhos relativos;
* Aspas simples, ponto e vírgula e indentação de 2 espaços;
* Identificadores em inglês; textos de interface e rótulos de acessibilidade em português do Brasil;
* Commits no formato `tipo: descrição em português no presente` (ex.: `feat: aplica o estilo da tela de home do app`);
* Documentação em português, com seções numeradas e separadores `---`.

Ainda não existe código C# no repositório. Para o backend, as regras vêm do `PROJECT_GUIDE.md`: PascalCase, métodos assíncronos com sufixo `Async`, sufixos `Client`, `Normalizer`, `Mapper`, `Request` e `Response`, tipagem forte sem `dynamic`, contratos externos isolados por integração, datas em UTC, `HttpClientFactory`, resiliência, logs estruturados e xUnit.

## 2.4. Particularidades que afetam este teste

* A pasta `src/` da raiz pertence ao app. Não coloque o backend lá.
* O `tsconfig.json` da raiz inclui `**/*.ts`. Se você criar qualquer arquivo TypeScript fora do app, isole-o, para não quebrar `npm run typecheck` e `npm run lint`.
* O `.gitignore` ignora `.env*.local`, mas não `.env`. Ajuste isso antes de existir qualquer segredo.
* O `AGENTS.md` exige ler a documentação do Expo SDK 54 antes de escrever código do app. Este teste não altera o app, então a regra só passa a valer se isso mudar.
* O `PROJECT_GUIDE.md` descreve a visão de longo prazo (API, PostgreSQL, Redis, Hangfire, web, painel administrativo). Neste teste, siga os padrões de código do guia, mas não construa essa infraestrutura.
* O guia cita .NET 9. Pelo índice oficial de releases do .NET, o suporte do .NET 9 termina em 10/11/2026, e o .NET 10 é LTS, com suporte até 14/11/2028. A recomendação é usar .NET 10 e registrar no relatório a sugestão de atualizar o guia.
* O guia cita FluentAssertions. A partir da versão 8, essa biblioteca é gratuita apenas para uso não comercial ou open source. Use a versão 7 (open source), uma alternativa equivalente ou nenhuma, e registre a escolha.

## 2.5. Entregável da fase 0

Crie o `VIABILITY_REPORT.md` na raiz (estrutura na seção 14) e preencha a seção "Entendimento e convenções adotadas": resuma o produto em poucas linhas e liste as convenções que você vai seguir. Não espere aprovação para continuar.

---

# 3. A ideia central do Brasil Compete

O Brasil Compete é um calendário de eventos esportivos internacionais em que há brasileiros competindo. Internacional, aqui, significa ter participantes de mais de um país, mesmo que o evento aconteça no Brasil.

A proposta é responder, de forma automática, a perguntas como "quando o Brasil compete hoje?" e "quando esse brasileiro volta a competir?", reunindo modalidades muito diferentes em uma única agenda:

* "Brasil x França" — seleção brasileira de futebol;
* "Gabriel Bortoleto (Audi) — Fórmula 1";
* "Santos x Barcelona" — clube brasileiro contra clube estrangeiro;
* "Hugo Calderano x An Jae-hyun" — tênis de mesa;
* "Luis Paulo Supi x Vincent Keymer" — xadrez;
* "Alex 'Poatan' Pereira x Ciryl Gane" — UFC;
* "LOUD x adversário — VALORANT Champions 2026" — organização brasileira de eSports representando o Brasil;
* "Gui Santos (Golden State Warriors) — Warriors x Lakers" — brasileiro em equipe estrangeira.

Esses exemplos ilustram o tipo de evento. Não assuma que esses confrontos existem ou que esses atletas continuam nesses times: verifique nas fontes.

A coleta deve ser automática, por APIs, bibliotecas, dados abertos ou leitura de páginas cujo uso automatizado seja permitido. O custo deve ser zero (o único gasto futuro previsto é a publicação na Play Store), e tudo deve respeitar a lei e os termos de uso das fontes.

---

# 4. Decisões já tomadas (não reabra neste teste)

## 4.1. Quem é brasileiro

Um atleta é considerado brasileiro quando:

* representa o Brasil (nacionalidade esportiva), e/ou
* nasceu no Brasil.

Uma equipe é considerada brasileira quando é:

* uma seleção brasileira, de qualquer modalidade ou categoria;
* um clube com sede no Brasil;
* uma organização de eSports com sede no Brasil (ex.: LOUD, FURIA).

Cidadania, sem representar o Brasil e sem ter nascido aqui, não é critério de inclusão. Mesmo assim, meça quantos atletas entrariam por esse caminho, apenas como informação.

**Limitação aceita:** atletas pouco conhecidos, que não aparecem nas fontes nem no Wikidata, não serão capturados. Isso não é um problema a resolver neste teste. Apenas meça e registre o tamanho dessa lacuna.

## 4.2. O que conta como internacional

* Competições com participantes de mais de um país entram, mesmo que aconteçam no Brasil (ex.: GP de São Paulo, Rio Open, UFC no Rio).
* Para equipes, a competição precisa reunir equipes de mais de um país: Libertadores entra; Brasileirão não. Ligas de eSports só com organizações brasileiras (ex.: CBLOL) não entram; ligas regionais com vários países (ex.: VCT Americas) entram.
* Circuitos individuais (F1, ATP, WTA, WTT, UFC, torneios internacionais de xadrez) são internacionais por natureza.
* Ligas nacionais estrangeiras (ex.: NBA, LaLiga) só entram pela visão de Indivíduos, por causa do atleta brasileiro (seção 4.3).

## 4.3. Duas visões

| Visão | O que mostra | Exemplos |
| --- | --- | --- |
| Feed principal | Equipes brasileiras em competições internacionais e atletas brasileiros de modalidades individuais | Seleção, Santos na Libertadores, LOUD no Champions, Bortoleto, Calderano, Supi, Poatan |
| Indivíduos | Atletas brasileiros jogando por equipes estrangeiras em modalidades coletivas | Gui Santos nos Warriors, jogadores brasileiros em clubes europeus, brasileiro em organização estrangeira de eSports |

Regras auxiliares sugeridas (aplique e registre os casos encontrados):

* Na F1, o piloto é o representante. Por isso, a F1 conta como modalidade individual e vai para o feed principal.
* Duplas de brasileiros (vôlei de praia, tênis) vão para o feed principal.
* Atleta que nasceu no Brasil, mas representa outro país, vai para Indivíduos.

## 4.4. Formato do evento e os quatro níveis de horário

Todo evento tem um formato:

* **Confronto:** dois lados se enfrentam ("Brasil x França", "Supi x Keymer").
* **Participação:** um brasileiro participa de algo com vários competidores ("Bortoleto no GP de São Paulo", "Calderano no WTT Champions" antes de a chave sair).

E todo evento tem exatamente um nível de precisão de horário:

| Nível | Significado | Exemplo |
| --- | --- | --- |
| Horário marcado | Data e hora definidas | Corrida da F1 às 14h |
| Data marcada | Data definida, sem hora | Confronto de tênis de mesa antes da ordem de jogos |
| Período | Só o período da competição | Torneio de 4 a 9 de novembro, ainda sem chave |
| A confirmar | Nada definido | Amistoso anunciado sem data |

Um mesmo evento pode subir de nível entre execuções (de "Período" para "Horário marcado", por exemplo). Isso é esperado e deve ficar registrado no histórico.

Nomes sugeridos no código, seguindo o padrão em inglês do guia: `EventFormat` (`Matchup`, `Participation`) e `SchedulePrecision` (`DateAndTime`, `DateOnly`, `CompetitionPeriod`, `ToBeConfirmed`). Os rótulos da interface ficam em português.

## 4.5. Exemplos completos

Exemplos ilustrativos; datas e confrontos não são reais.

| Exemplo | Visão | Formato | Precisão típica |
| --- | --- | --- | --- |
| Brasil x França, amistoso | Feed principal | Confronto | Horário marcado |
| Bortoleto (Audi) — GP de São Paulo, corrida | Feed principal | Participação | Horário marcado |
| Santos x Barcelona, amistoso anunciado | Feed principal | Confronto | A confirmar, depois Data marcada |
| Calderano no WTT Champions, antes da chave | Feed principal | Participação | Período |
| Calderano x An Jae-hyun | Feed principal | Confronto | Data marcada |
| Supi x Keymer, rodada 5 | Feed principal | Confronto | Horário marcado (o pareamento sai horas antes) |
| Poatan x Gane | Feed principal | Confronto | Data marcada ou Horário marcado |
| LOUD x adversário, VALORANT Champions 2026 | Feed principal | Confronto | Horário marcado |
| Gui Santos (Warriors) — Warriors x Lakers | Indivíduos | Confronto | Horário marcado |

"Santos x Barcelona" também mostra por que nomes não bastam: "Barcelona" pode ser o FC Barcelona (Espanha) ou o Barcelona de Guayaquil (Equador). Identifique equipes e atletas por IDs sempre que possível.

## 4.6. Restrições do produto

* Custo zero de operação.
* Sem login, sem anúncios e sem monetização (algumas fontes só permitem uso não comercial).
* Somente fatos: quem, contra quem, quando, onde, qual competição e qual fase.
* Sem logos, escudos ou fotos de atletas. Bandeiras e nomes podem ser usados.

## 4.7. Arquitetura futura (contexto, não implemente)

A arquitetura final será definida depois deste teste. A hipótese atual, ainda não decidida, é um MVP enxuto: um worker em .NET rodando agendado no GitHub Actions, gerando arquivos JSON estáticos publicados (ex.: GitHub Pages) e consumidos pelo app Expo, sem API e sem banco no início.

Por isso, escreva o código do teste de modo que adapters, normalização, identidade e modelo possam virar a base desse worker, e sem depender de banco de dados.

---

# 5. Objetivo deste teste

## 5.1. Pergunta central

> É possível montar, de forma automática, gratuita e dentro da lei e dos termos de uso das fontes, uma agenda confiável de eventos internacionais com brasileiros, nas modalidades-alvo?

## 5.2. Perguntas específicas

1. Quais fontes permitidas existem para cada modalidade, e o que cada uma entrega de fato?
2. Conseguimos identificar quem é brasileiro com a regra da seção 4.1? Qual é o tamanho da lacuna?
3. Qual é a cobertura e a exatidão (data e horário) por modalidade, comparadas a um gabarito?
4. Com quanta antecedência os eventos aparecem, e em qual nível de precisão?
5. Quão estáveis são os dados entre execuções (mudanças de horário, adversário, cancelamentos, duplicatas)?
6. Qual é o volume semanal de cada visão? A visão de Indivíduos fica utilizável sem filtro extra?
7. Quanta curadoria manual seria necessária por semana para cobrir as lacunas aceitáveis?
8. A execução cabe nos limites das fontes e roda em tempo razoável? Se testado, funciona também no GitHub Actions?

## 5.3. Modalidades-alvo

Futebol (seleções e clubes em competições internacionais), Fórmula 1, xadrez, UFC/MMA, eSports (VALORANT, CS2 e LoL), basquete NBA (visão Indivíduos), vôlei e vôlei de praia, tênis e tênis de mesa.

Outras modalidades relevantes para o Brasil (surfe, skate, judô, ginástica, atletismo, natação, basquete FIBA, handebol) ficam fora do teste. Liste-as no relatório com a fonte provável de cada uma, sem implementar.

## 5.4. Entregáveis

1. Código do teste no repositório, no padrão do projeto, com testes.
2. `VIABILITY_REPORT.md`, com métricas, achados, desvios do plano e veredito.
3. Arquivos de validação (entidades de referência e gabaritos).
4. Instruções para rodar a coleta (`backend/README.md`).

---

# 6. Regras inegociáveis

## 6.1. Uso de fontes e questões legais

1. Use apenas APIs oficiais, dados abertos ou páginas cujo acesso automatizado seja permitido pelos termos.
2. Nunca use APIs não oficiais ou obtidas por engenharia reversa, nem "só para testar": ESPN, Sofascore, Flashscore, ufcstats, HTML do vlr.gg e similares.
3. Nunca contorne login, CAPTCHA, paywall, bloqueio de IP ou desafio anti-bot. Respeite o `robots.txt`. Se uma fonte bloquear, pare de usá-la e registre.
4. Identifique as requisições com um User-Agent com nome e contato, por exemplo: `BrasilCompete-Viability/0.1 (+https://github.com/AlexandreAT/BrasilCompete)`.
5. Respeite os limites de requisição de cada fonte: requisições em série quando exigido, espera em respostas 429 conforme o `Retry-After` e cache local durante o desenvolvimento.
6. Colete somente fatos. Não copie textos, imagens, logos, escudos ou fotos.
7. Guarde a fonte, a URL e a licença de cada evento, para permitir atribuição.
8. Não use odds de apostas nem links de transmissão.
9. Não contrate nem use serviços pagos. Se a única saída for paga, registre e siga sem ela.

Contexto: a Lei 9.610/98 (art. 8º) exclui da proteção de direitos autorais "informações de uso comum tais como calendários, agendas". O risco de um projeto como este não está no fato em si, mas na forma de obtê-lo (termos de uso) e no que é copiado junto (textos, imagens, marcas). As regras acima existem para manter o projeto do lado seguro.

## 6.2. Projeto

1. Siga as convenções registradas na fase 0.
2. Não altere o app (UI, rotas ou tipos) neste teste.
3. Não construa infraestrutura que o teste não precisa: API, banco, Redis, Hangfire, painel, deploy.
4. Não use IA ou LLM no pipeline de dados.
5. Faça commits pequenos, por fase ou por fonte, no padrão do repositório. Rode build e testes antes de cada commit. Não reescreva o histórico.
6. Não altere o `PROJECT_GUIDE.md`. Proponha mudanças no relatório.

## 6.3. Segurança

1. Nenhum segredo vai para o repositório. Use .NET User Secrets (o autor já usa esse mecanismo no projeto GameHub) ou variáveis de ambiente.
2. Nunca peça ao usuário para colar uma chave no chat: ele mesmo grava a chave com o comando que você indicar.
3. Nunca escreva chaves em logs, relatórios ou mensagens de erro.
4. Ajuste o `.gitignore` para `.env`, saídas, cache, estado local e artefatos de build do .NET.

---

# 7. Fora do escopo

* Qualquer mudança no app: telas, integração com dados reais, favoritos, notificações.
* API, banco de dados, autenticação, painel administrativo, versão web, deploy e Play Store.
* Leitura de notícias e extração com IA. Se alguma modalidade só puder ser coberta por notícias, registre isso como achado.
* Fontes pagas.
* Modalidades fora da seção 5.3 (só listar).
* Categorias de acesso, como F2, F3 e Challengers menores, salvo se vierem de graça na mesma fonte.

---

# 8. O que já sabemos sobre as fontes

Legenda: **verificado** significa confirmado no repositório oficial ou na documentação, em 08/10/2026; **não verificado** significa que precisa ser confirmado por você.

| Fonte | Modalidades | Chave | O que sabemos | Papel sugerido |
| --- | --- | --- | --- | --- |
| Jolpica-F1 (`api.jolpi.ca/ergast/f1/`) | F1 | Não | Verificado: uso não comercial, dados em CC BY-NC-SA 4.0, limites de 4 req/s e 500 req/h (`TERMS.md` e `docs/rate_limits.md` do repositório) | Primária da F1 |
| API de transmissões da Lichess | Xadrez | Não | Verificado na especificação oficial: jogadores com `fed` e `fideId`; rodadas com `startsAt`; uma requisição por vez e espera de 1 minuto após 429 | Primária do xadrez |
| Wikidata (SPARQL e API) | Identidade, todas | Não | Licença CC0. Não verificado ao vivo: cobertura dos atletas brasileiros | Base de identidade |
| Wikipedia (API MediaWiki e REST) | Futebol, UFC, tênis | Não | Texto em CC BY-SA, com predefinições estruturadas (ex.: football box). Não verificado ao vivo | Primária de futebol, UFC e tênis |
| Liquipedia (API MediaWiki) | eSports | Não | Verificado: termos de API próprios, com limites rígidos e banimento se violados; conteúdo em CC BY-SA 3.0 | Primária de eSports |
| TheSportsDB v1 | Vários | Pública ("123") | Verificado: a v1 aceita a chave pública "123"; a v2 é paga. Não verificado: limites do plano grátis | Secundária, checagem cruzada |
| Web Service da FIVB | Vôlei e vôlei de praia | Não verificado | Verificado: SDK oficial no GitHub da FIVB, com modelos de partidas, torneios, jogadores e federações. Não verificado: termos e autenticação | A investigar |
| balldontlie | NBA | Sim (conta grátis) | Verificado no SDK oficial: o jogador tem `country` e `team`. Não verificado: rotas e limites do plano grátis | Primária da NBA |
| football-data.org | Ligas europeias | Sim (conta grátis) | Não verificado | Opcional: medir volume da visão Indivíduos |
| PandaScore | eSports | Sim (conta grátis) | Não verificado | Opcional: checagem cruzada |
| Curadoria manual | Qualquer | — | Arquivo versionado no repositório | Lacunas e correções |

Fontes descartadas:

* **openfootball** (CC0): atrasado demais para servir de agenda. No último commit analisado (21/09/2026), as quartas de final da Libertadores 2026, marcadas para 08 e 15/09, ainda estavam como "N.N.", e os amistosos de 2026 só tinham resultados passados. Serve como histórico ou dado de teste.
* **ESPN, Sofascore, Flashscore, ufcstats, vlr.gg**: não oficiais ou com termos que não permitem esse uso. O próprio repositório comunitário que documenta a API da ESPN avisa que um endpoint ser público não significa ter permissão para usá-lo.
* **Sites de notícias**: fora do escopo (seção 7).

---

# 9. Arquitetura sugerida para o teste

## 9.1. Tecnologia

Um console app em C# no .NET 10, com Generic Host (injeção de dependência, configuração, logs, `HttpClientFactory` e User Secrets), resiliência com `Microsoft.Extensions.Http.Resilience` e testes com xUnit. Garanta que os User Secrets sejam carregados no console app independentemente do ambiente de execução.

Motivos: é a stack do guia, mostra conhecimento de backend no portfólio e o código vira a base do worker real.

Trocar de linguagem só se justifica por um motivo forte (ex.: uma fonte essencial que só tenha cliente viável em outro ecossistema), registrado no relatório.

## 9.2. Estrutura de pastas

```text
backend/
├── BrasilCompete.slnx
├── README.md                     como rodar a coleta e o relatório
├── src/
│   └── BrasilCompete.Worker/
│       ├── Program.cs
│       ├── Domain/               Event, Participant, Schedule e enums
│       ├── Identity/             regras de "quem é brasileiro" e cliente do Wikidata
│       ├── Integrations/
│       │   ├── Jolpica/          Client, contratos externos, Mapper, Normalizer, Options
│       │   ├── Lichess/
│       │   ├── Wikipedia/
│       │   ├── Liquipedia/
│       │   ├── TheSportsDb/
│       │   ├── Fivb/
│       │   ├── Balldontlie/
│       │   └── Manual/
│       ├── Pipeline/             filtro, classificação, deduplicação e histórico
│       └── Output/               events.json, resumo da execução e agenda legível
├── tests/
│   └── BrasilCompete.Worker.Tests/
│       └── Fixtures/             respostas reais salvas por fonte
├── curation/                     eventos manuais versionados
├── validation/                   entidades de referência e gabaritos
└── scripts/                      coleta diária (PowerShell)
```

Cada integração segue a seção 19 do guia: cliente HTTP próprio, contratos externos isolados, mapper, normalizador, configuração, tratamento de erros e identificação da fonte.

## 9.3. Fluxo

```text
Adapters (um por fonte)
    ↓
Contratos externos → Mapper → Normalizer
    ↓
Identidade (é brasileiro? por qual critério?)
    ↓
Filtro (é internacional?) e classificação (qual visão?)
    ↓
Deduplicação e merge entre fontes
    ↓
Histórico (primeira aparição e mudanças entre execuções)
    ↓
Saídas: eventos, resumo da execução e agenda legível
```

Cada adapter devolve um resultado com status (sucesso, parcial ou falha), contagens, número de requisições e duração. A falha de uma fonte nunca interrompe as outras.

## 9.4. Modelo de evento

Exemplo ilustrativo, com dados fictícios, apenas para mostrar o nível de informação esperado. Ajuste nomes e formato ao padrão do projeto:

```json
{
  "id": "esports-valorant:loud-x-equipe-exemplo:2026-09-20",
  "sport": "esports-valorant",
  "competition": "VALORANT Champions 2026",
  "stage": "Playoffs",
  "format": "Matchup",
  "view": "Main",
  "schedule": {
    "precision": "DateAndTime",
    "startUtc": "2026-09-20T17:00:00Z",
    "date": "2026-09-20",
    "periodStart": null,
    "periodEnd": null,
    "originalTimeZone": "Etc/UTC"
  },
  "participants": [
    {
      "name": "LOUD",
      "kind": "Organization",
      "country": "BRA",
      "isBrazilian": true,
      "brazilianReason": "BrazilianOrganization",
      "externalIds": { "liquipedia": "LOUD" }
    },
    {
      "name": "Equipe Exemplo",
      "kind": "Organization",
      "country": "USA",
      "isBrazilian": false,
      "externalIds": {}
    }
  ],
  "sources": [
    {
      "source": "liquipedia",
      "url": "https://liquipedia.net/valorant/...",
      "license": "CC BY-SA 3.0",
      "retrievedAtUtc": "2026-10-09T12:00:00Z"
    }
  ],
  "confidence": "High"
}
```

Motivos possíveis para `brazilianReason`: representa o Brasil, nasceu no Brasil, seleção brasileira, clube brasileiro e organização brasileira.

## 9.5. Identidade

Use camadas, nesta ordem:

1. **Nacionalidade informada pela própria fonte**, quando existir: `fed: BRA` na Lichess, `nationality: Brazilian` na Jolpica, `country` na balldontlie, federação na FIVB, país da equipe na Liquipedia, bandeira no card de luta ou na partida da Wikipedia.
2. **Wikidata**, para completar ou confirmar: país pelo qual compete (P1532), local de nascimento (P19, com país P17 igual a Brasil, Q155), cidadania (P27, só para a métrica informativa), clube (P54) e IDs externos (FIDE P1440, entre outros).
3. **Curadoria manual**, para correções.

Faça o casamento por ID externo antes do nome. Casamento só por nome deve ser marcado como baixa confiança. Registre em cada participante qual camada tomou a decisão.

Descubra as propriedades e os itens do Wikidata pela documentação ou por consulta. Não confie em IDs de memória além de Q155 (Brasil), P27, P19, P17, P54, P1532 e P1440.

## 9.6. Filtro e classificação

Aplique as seções 4.2 e 4.3. Para decidir se uma competição é internacional, um catálogo pequeno de competições-alvo por fonte, em configuração, costuma ser mais simples e transparente do que deduzir pelos participantes. Escolha e registre.

Guarde também os eventos descartados e o motivo (sem brasileiro, não internacional, fora da janela), para as métricas de falso positivo e de volume.

## 9.7. Deduplicação

O mesmo evento pode vir de mais de uma fonte, como uma partida da Libertadores presente na Wikipedia e no TheSportsDB.

* Compare modalidade, participantes (IDs antes de nomes normalizados) e data, com tolerância de um dia por causa de fuso.
* Ao juntar, mantenha todas as fontes do evento.
* Defina uma prioridade por fonte, em que a curadoria manual vence todas (o `ManualOverride` previsto no guia).
* Registre conflitos (datas ou horários diferentes entre fontes), porque isso também é métrica.
* O ID do evento deve ser determinístico, para que o histórico reconheça o mesmo evento entre execuções.

## 9.8. Horários e fusos

* Guarde tudo em UTC, junto com o fuso ou offset original.
* A exibição será em `America/Sao_Paulo` (o Brasil não tem horário de verão desde 2019).
* Escreva testes para as viradas de horário de verão de 2026: Europa em 25/10 e Estados Unidos em 01/11.
* Nunca invente horário: se a fonte não informa, a precisão desce de nível.

## 9.9. Execução, cache e histórico

* Ofereça comandos para coletar uma janela de datas e para gerar o relatório. Exemplo: `dotnet run --project backend/src/BrasilCompete.Worker -- collect --from 2026-09-01 --to 2026-09-30`.
* Use cache de respostas em disco durante o desenvolvimento, com tempo de expiração, para não repetir requisições.
* Mantenha um histórico local (fora do Git) com a primeira aparição, a última aparição e as mudanças de cada evento. É dele que saem as métricas de antecedência e estabilidade.
* Crie `backend/scripts/collect.ps1` para a coleta diária. O usuário usa Windows e PowerShell.

## 9.10. Saídas

* `events.json`: eventos da janela, já consolidados.
* `run-summary.json`: status, contagens, requisições, duração e erros por fonte.
* `agenda.md`: agenda legível, por dia e por visão, para revisão humana rápida.

As saídas brutas ficam fora do Git. O relatório e os arquivos de validação ficam no Git.

## 9.11. Testes

* Testes unitários para normalização de nomes, conversão de fuso, níveis de precisão, regras de identidade, classificação de visão e deduplicação.
* Testes de snapshot por adapter, usando respostas reais salvas em `Fixtures/`, sem rede. Inclua um README na pasta com a fonte e a licença dos trechos.
* Nenhum teste depende de internet.

---

# 10. Fases de execução

Ao final de cada fase: build e testes passando, commit no padrão e relatório atualizado (status da fase e achados).

## Fase 1 — Base do worker

* Crie a solução, o projeto do worker e o de testes.
* Configure injeção de dependência, configuração, logs, `HttpClientFactory` com resiliência e User Secrets (ainda sem segredos).
* Implemente o modelo de domínio, a saída em JSON, o histórico e os comandos.
* Implemente o adapter de curadoria manual, com 3 ou 4 eventos de exemplo marcados como dados de teste, para exercitar o fluxo completo.
* Ajuste o `.gitignore`.
* Escreva o `backend/README.md`.

**Esperado:** `dotnet build` e `dotnet test` passando, e um `events.json` gerado a partir da curadoria.

## Fase 2 — Identidade e entidades de referência

* Implemente o cliente do Wikidata (SPARQL), respeitando a política de User-Agent e os limites do serviço.
* Gere a lista de brasileiros por modalidade-alvo, com os IDs externos relevantes.
* Monte `backend/validation/reference-entities.json`, resolvendo IDs e checando a presença de cada entidade. Lista mínima (verifique se continuam ativos e onde atuam hoje):
  * **Modalidades individuais:** Gabriel Bortoleto, Hugo Calderano, Luis Paulo Supi, Alex Pereira, João Fonseca, Beatriz Haddad Maia e uma dupla brasileira de vôlei de praia;
  * **Equipes:** seleções brasileiras de futebol (masculina e feminina), seleções de vôlei, LOUD, FURIA e os clubes brasileiros ativos em competições da CONMEBOL na temporada;
  * **Visão Indivíduos:** Gui Santos e pelo menos dois jogadores brasileiros em clubes europeus, além de uma atleta do futebol feminino em clube estrangeiro.
* Acrescente outras entidades que considerar representativas e registre o critério.

**Esperado:** seção de identidade preliminar no relatório, com quantas entidades foram encontradas, quantas têm cada ID externo, quantas entrariam só por cidadania e quais são as lacunas.

## Fase 3 — Fontes sem chave

Implemente uma fonte por vez, cada uma com fixtures, testes e commit próprios. Antes de cada uma, leia a documentação e os termos de uso atuais da fonte e registre no relatório o que eles permitem e o que limitam.

**3.1. F1 (Jolpica)**

* Calendário da temporada (e da anterior, se a janela passada pedir), com horários de corrida, classificação e sprint.
* Pilotos com nacionalidade brasileira na temporada e a equipe de cada um.
* Eventos de participação, como "Gabriel Bortoleto (Audi) — GP de São Paulo — Corrida", com horário marcado.
* Decida se treinos e classificação viram eventos próprios ou detalhes da corrida. Meça o volume das duas opções.
* **Esperado:** 100% das corridas da janela, com horário correto. Esta fonte é o controle: se falhar, o problema está no pipeline.

**3.2. Xadrez (Lichess)**

* Transmissões oficiais em andamento, futuras e da janela passada.
* Para cada torneio, os jogadores. O torneio é internacional quando há duas ou mais federações. O jogador é brasileiro quando `fed` é `BRA` (confirme pelo ID FIDE no Wikidata quando possível).
* Participação por torneio (Período) ou por rodada (Horário marcado, via `startsAt`). Confronto quando a rodada já tiver pareamento ("Luis Paulo Supi x Vincent Keymer").
* Respeite a regra de uma requisição por vez.
* **Meça:** quantos torneios internacionais têm brasileiros e com quanta antecedência o pareamento aparece.

**3.3. Futebol (Wikipedia, com TheSportsDB como segunda fonte)**

* Seleções brasileiras masculina e feminina: páginas de jogos e resultados.
* Clubes brasileiros em competições internacionais da temporada: Libertadores, Sul-Americana e outras ativas na janela.
* Leia as predefinições de partida pelo HTML do Parsoid: o atributo `data-mw` traz o nome e os parâmetros da predefinição em JSON, o que evita escrever um parser de wikitext. Confirme o endpoint atual da API REST da Wikimedia.
* Identifique clubes por ID (link do clube, depois Wikidata), não por nome.
* Converta os fusos: o horário costuma vir no horário local, com offset.
* Amistosos de clubes (como "Santos x Barcelona"): meça se aparecem em alguma fonte. Se não aparecerem, vão para a curadoria.

**3.4. UFC (Wikipedia e Wikidata)**

* Lista de eventos agendados e o card anunciado de cada evento da janela.
* O lutador é brasileiro pela bandeira no próprio card e/ou pelo Wikidata.
* Confronto com Data marcada, ou Horário marcado quando o horário do card existir.
* **Meça:** quantas lutas mudam ou caem entre execuções e quantos lutadores brasileiros dos cards ficam sem identificação.

**3.5. eSports (Liquipedia)**

* Partidas de organizações brasileiras (LOUD, FURIA, MIBR, paiN e outras que as fontes indicarem) em competições internacionais de VALORANT, CS2 e LoL.
* **Caso de validação:** LOUD no VALORANT Champions 2026, na janela passada.
* Aplique a regra de internacional: CBLOL não entra; VCT Americas entra.
* Siga rigorosamente os termos da API da Liquipedia. Prefira consultas que tragam wikitext e evite as operações com limite mais restrito.
* Brasileiros em organizações estrangeiras vão para Indivíduos. Meça, sem precisar cobrir tudo.

**3.6. TheSportsDB (chave pública "123")**

* Use como fonte secundária de futebol e UFC, para checagem cruzada e deduplicação.
* Registre exatamente o que o plano grátis entrega (resultados por chamada, rotas bloqueadas). Isso indica se valeria considerar o plano pago no futuro. Não contrate.

**3.7. Vôlei (Web Service da FIVB)**

* Investigue o acesso e os termos.
* Se for permitido e não exigir cadastro: seleções brasileiras (quadra) e duplas brasileiras (praia) nos eventos da janela.
* Se exigir cadastro ou chave, inclua na Parada A, com instruções. Se os termos não permitirem, registre e siga.

**3.8. Tênis (Wikipedia)**

* Torneios ATP e WTA da janela com brasileiros na chave.
* Participação com Período antes da chave; Confronto com Data marcada depois. A falta de horário é uma limitação esperada.
* **Meça:** o atraso entre a divulgação da chave e a atualização da página.

**3.9. Tênis de mesa**

* Procure uma fonte permitida: dados abertos oficiais da WTT ou da ITTF, ou a Wikipedia. Não use APIs internas de sites.
* Se não houver, use a curadoria manual para Hugo Calderano e outros brasileiros, e meça o esforço (eventos por mês vezes minutos por evento).

## Fase 4 — Consolidação

* Finalize e teste identidade, filtro de internacional, classificação de visão, níveis de precisão, deduplicação e histórico.
* Revise os eventos descartados por amostragem, procurando descartes por engano.

## Fase 5 — Validação da janela passada e relatório preliminar

* Escolha uma janela passada de cerca de 30 dias, terminando antes da data de execução, que contenha um evento internacional de eSports com organização brasileira (de preferência o VALORANT Champions 2026). Se não for possível, acrescente uma segunda janela curta só para esse evento.
* Monte o gabarito `backend/validation/reference-past.csv` consultando calendários e resultados oficiais pontualmente, como uma pessoa faria. Não escreva coletores para essas páginas. Inclua todos os eventos das entidades de referência na janela, cada um com a URL da fonte oficial consultada.
* Colunas sugeridas: modalidade, competição, fase, participantes, visão, data, hora de Brasília, precisão esperada, URL da fonte oficial e observação.
* Rode a coleta da janela e calcule as métricas da seção 11, por modalidade e por visão.
* Escreva o relatório preliminar.

## Parada A — Chaves de API

Pare aqui e peça ao usuário as chaves necessárias, seguindo o protocolo da seção 13. Previstas:

* **balldontlie**, necessária para a NBA;
* **football-data.org**, opcional, para medir o volume da visão Indivíduos no futebol;
* **PandaScore**, opcional, para checagem cruzada de eSports;
* **FIVB**, se a fase 3.7 indicar cadastro.

Se o usuário preferir pular uma chave opcional, siga sem ela e registre o impacto.

## Fase 6 — Fontes com chave

* **6.1. NBA (balldontlie):** jogadores com `country` Brasil, depois os times, depois os jogos da janela, na visão Indivíduos ("Gui Santos (Golden State Warriors) — Warriors x Lakers"). Confirme os limites do plano grátis.
* **6.2. Futebol no exterior (football-data.org, opcional):** elencos com nacionalidade nas competições do plano grátis, depois os brasileiros, depois os jogos dos clubes. Serve só para medir o volume semanal da visão Indivíduos e testar filtros (ex.: apenas convocados da seleção nos últimos 12 meses). Não precisa cobrir tudo.
* **6.3. eSports (PandaScore, opcional):** checagem cruzada com a Liquipedia.
* Atualize as métricas e o relatório.

## Fase 7 — Preparar a coleta da janela futura

* Deixe o `backend/scripts/collect.ps1` pronto para coletar os próximos 14 dias, salvar um retrato datado e atualizar o histórico.
* Monte o gabarito candidato `backend/validation/reference-future.csv` para as entidades de referência, a partir dos calendários oficiais. Marque o que ainda não tem data ou horário definido.
* Opcional: um workflow do GitHub Actions com `schedule` e `workflow_dispatch` para rodar a coleta sozinho, usando segredos do repositório e artefatos. Isso também testa se as fontes funcionam a partir dos servidores do GitHub. Se fizer, inclua na Parada B as instruções de configuração dos segredos.

## Parada B — Coleta e revisão

Explique ao usuário, seguindo o protocolo da seção 13, que ele deve:

* rodar `backend/scripts/collect.ps1` uma vez por dia, durante 7 dias (ou ativar o workflow);
* ao fim dos 7 dias, revisar o gabarito futuro: confirmar o que aconteceu, corrigir datas e incluir eventos que faltaram;
* pedir a execução da fase 8.

Deixe marcados no gabarito os itens que precisam de conferência, para que a revisão leve menos de uma hora.

## Fase 8 — Análise final e veredito

Esta fase acontece em uma nova sessão, depois da coleta. O relatório é a memória do teste: comece lendo-o.

* Calcule antecedência, estabilidade, cobertura e exatidão na janela futura; incidentes de manutenção (parsers que quebraram); tempos de execução; problemas de limite; e esforço semanal de curadoria.
* Dê o veredito geral e por modalidade (seção 11.3).
* Escreva as recomendações para o plano final: fontes a manter e a descartar, arquitetura, atualizações sugeridas para o `PROJECT_GUIDE.md`, limitações a assumir no produto e decisões de produto que os números indicam.

---

# 11. Métricas e critérios de decisão

## 11.1. Métricas

| Métrica | Como calcular |
| --- | --- |
| Cobertura | Eventos do gabarito encontrados ÷ eventos do gabarito |
| Exatidão de data | Encontrados com data correta ÷ encontrados |
| Exatidão de horário | Corretos ÷ encontrados com "Horário marcado", após conversão de fuso |
| Falsos positivos | Eventos gerados inexistentes, sem brasileiro ou não internacionais ÷ gerados (amostra revisada) |
| Antecedência | Dias entre a primeira aparição com pelo menos "Data marcada" e a data do evento |
| Estabilidade | Mudanças por evento entre execuções e duplicatas que sobraram após a deduplicação |
| Volume | Eventos por semana, por visão e por modalidade |
| Custo operacional | Duração da execução, requisições por fonte, ocorrências de 429 e de erros |
| Curadoria | Eventos manuais necessários por semana × minutos por evento |
| Lacuna de identidade | Brasileiros presentes nas fontes, mas não identificados |

## 11.2. Metas sugeridas

São um ponto de partida. Se os dados mostrarem que alguma meta é inadequada, argumente no relatório.

| Critério | Meta |
| --- | --- |
| Cobertura do feed principal nas modalidades automáticas | ≥ 80% |
| Exatidão de data | ≥ 95% |
| Exatidão de horário | ≥ 90% |
| Falsos positivos | ≤ 5% |
| Eventos do feed principal visíveis com pelo menos "Data marcada" 3 dias antes | ≥ 70% |
| Duplicatas visíveis após a deduplicação | ≤ 2% |
| Duração de uma execução completa | ≤ 15 minutos, sem 429 sem tratamento |
| Custo | Zero, com todas as fontes usadas dentro dos termos |
| Curadoria manual | ≤ 1 hora por semana |

## 11.3. Formato do veredito

**Geral:** viável; viável com ressalvas (listadas, com o impacto no produto); ou inviável (com o motivo e os caminhos alternativos).

**Por modalidade:**

* automática;
* semiautomática (lacunas previsíveis, como falta de horário, ou curadoria leve);
* só curadoria;
* fora por enquanto.

---

# 12. O que precisará ser levantado ao final

| Item | Quem faz | Esforço estimado |
| --- | --- | --- |
| Gabarito da janela passada | IA, consultando fontes oficiais pontualmente | Incluído na fase 5 |
| 7 dias de coleta diária | Usuário (um comando por dia) ou GitHub Actions | 1 a 5 minutos por dia |
| Revisão do gabarito futuro | Usuário, com os itens a conferir marcados pela IA | Até 1 hora |
| Resumo dos termos de uso de cada fonte, com links e cláusulas relevantes | IA | Incluído nas fases 3 e 6 |
| Estimativa do esforço semanal de curadoria | IA, a partir das lacunas medidas | Incluído na fase 8 |
| Decisões de produto informadas pelos números: filtro da visão Indivíduos, modalidades só com curadoria, destino de quem nasceu no Brasil mas representa outro país | Usuário, com a recomendação da IA | Depois do relatório final |

---

# 13. Protocolo de parada e passos manuais previstos

## 13.1. Formato da mensagem de parada

```text
PARADA — preciso de você

O que já está pronto:
- resumo do que foi feito e dos commits

O que você precisa fazer (tempo estimado: X minutos):
1. Acesse <URL>.
2. <passo a passo, dizendo o que clicar e o que copiar>
3. No PowerShell, na pasta do projeto, rode:
   dotnet user-secrets set --project backend/src/BrasilCompete.Worker "<Fonte>:ApiKey" "SUA_CHAVE"
   (a chave fica só na sua máquina; não precisa me enviar)

Como me avisar: responda "feito" ou "pular <item>".

O que eu faço depois: <próximos passos>
Se você pular: <impacto no teste>
```

Antes de passar instruções ao usuário, confirme na página oficial de cada serviço que o fluxo de cadastro e o local da chave continuam iguais.

## 13.2. Passos manuais previstos

Confirme cada um antes de pedir.

* **SDK do .NET 10:** se `dotnet --list-sdks` não mostrar a versão, instalar pelo site oficial (`https://dotnet.microsoft.com/download`) ou via `winget`.
* **Acesso de rede:** se o seu ambiente bloquear as fontes, pare e explique o que precisa ser liberado, ou peça para rodar localmente. Na análise original, o ambiente bloqueava essas APIs.
* **balldontlie:** criar conta gratuita em `https://app.balldontlie.io`, copiar a chave no painel e gravá-la com `dotnet user-secrets`.
* **football-data.org (opcional):** cadastro gratuito no site; a chave chega por e-mail.
* **PandaScore (opcional):** conta gratuita; o token fica no painel.
* **GitHub Actions (opcional):** no repositório, em Settings → Secrets and variables → Actions → New repository secret, criar um segredo por chave, com os nomes que você definir.
* **Revisão do gabarito futuro:** ao fim da coleta.

---

# 14. Relatório de viabilidade (`VIABILITY_REPORT.md`)

Crie o relatório na fase 0 e atualize-o ao fim de cada fase. Ele deve permitir que outra sessão, ou outra IA, retome o trabalho sem contexto extra.

Estrutura sugerida:

1. Status das fases (tabela com fase, situação, data e observações).
2. Resumo executivo e veredito (preencher na fase 8).
3. Entendimento e convenções adotadas.
4. Desvios do plano e motivos.
5. Fontes: modalidade, chave, licença e termos (com links), limites, status, problemas e papel final.
6. Identidade: cobertura das entidades de referência, regras aplicadas, conflitos e lacuna medida.
7. Cobertura e exatidão por modalidade e por visão (janela passada e janela futura).
8. Volume por visão e por modalidade.
9. Antecedência e estabilidade.
10. Deduplicação e conflitos entre fontes.
11. Curadoria manual necessária.
12. Execução: tempos, requisições, erros, custo e GitHub Actions (se feito).
13. Modalidades fora do teste, com a fonte provável de cada uma.
14. Riscos e limitações aceitas.
15. Recomendações para o plano final.
16. Pendências manuais (o que está aguardando o usuário).

---

# 15. Definição de pronto do teste

O teste está concluído quando:

* todas as fases aplicáveis foram executadas, ou os motivos para pular estão registrados;
* o código segue as convenções do projeto, com build e testes passando e sem segredos no repositório;
* cada fonte usada tem os termos lidos, resumidos e respeitados;
* as métricas da seção 11 foram calculadas para a janela passada e para a janela futura;
* o relatório tem veredito geral e por modalidade, com recomendações para o plano final;
* as pendências manuais estão listadas, ou não existem mais.

---

# 16. Links de referência

* Jolpica-F1 (código, termos e limites): https://github.com/jolpica/jolpica-f1
* API da Lichess: https://lichess.org/api
* Wikidata Query Service: https://query.wikidata.org
* Política de User-Agent da Wikimedia: https://meta.wikimedia.org/wiki/User-Agent_policy
* API MediaWiki: https://www.mediawiki.org/wiki/API:Main_page
* Termos da API da Liquipedia: https://liquipedia.net/api-terms-of-use
* TheSportsDB: https://www.thesportsdb.com
* SDK oficial da FIVB: https://github.com/FIVB/javascript-sdk
* balldontlie: https://app.balldontlie.io
* football-data.org: https://www.football-data.org
* .NET User Secrets: https://learn.microsoft.com/aspnet/core/security/app-secrets
* Downloads do .NET: https://dotnet.microsoft.com/download
