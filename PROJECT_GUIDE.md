# Brasil Compete — Guia Técnico, Arquitetural e de Desenvolvimento

## 1. Propósito deste documento

Este documento apresenta a visão geral do **Brasil Compete**, sua proposta de produto, arquitetura, organização de código, stack tecnológica, padrões de desenvolvimento, práticas esperadas e roadmap de evolução.

Ele deve servir como referência para:

* Desenvolvedores que entrem no projeto;
* Ferramentas de inteligência artificial usadas na implementação;
* Revisões arquiteturais;
* Planejamento de novas versões;
* Manutenção e evolução do código;
* Padronização entre API, Worker, frontend web e aplicativo mobile;
* Redução de inconsistências entre módulos, componentes, serviços e integrações externas.

Este documento não substitui o README principal. O README deve apresentar o projeto de forma resumida, contendo instruções de instalação, configuração e execução. Este arquivo funciona como um guia aprofundado de desenvolvimento.

---

# 2. Visão geral do projeto

O **Brasil Compete** é uma plataforma web e mobile criada para centralizar competições internacionais nas quais o Brasil esteja sendo representado.

O projeto não se limita a partidas tradicionais ou seleções nacionais. A plataforma deve reunir eventos envolvendo:

* Seleções brasileiras;
* Clubes brasileiros;
* Equipes brasileiras de eSports;
* Atletas individuais;
* Duplas;
* Pilotos;
* Lutadores;
* Representantes brasileiros em campeonatos internacionais.

O sistema deve responder de forma simples a perguntas como:

* Em quais eventos o Brasil compete hoje?
* Quais brasileiros competem amanhã?
* Quais finais possuem participação brasileira nesta semana?
* Onde será possível assistir?
* Quais resultados recentes foram registrados?
* Quando determinado atleta ou equipe brasileira volta a competir?

O produto deve funcionar como um **hub da participação brasileira no esporte internacional**, tendo o calendário de eventos como seu principal núcleo.

---

# 3. Conceito central: eventos, não apenas jogos

O sistema deve utilizar o conceito de **Evento Esportivo** como elemento principal.

Um evento pode representar:

* Partida;
* Luta;
* Corrida;
* Prova;
* Bateria;
* Rodada;
* Etapa;
* Classificatória;
* Qualifying;
* Treino;
* Apresentação;
* Disputa por medalha;
* Cerimônia competitiva;
* Outro formato relacionado a uma modalidade.

O termo `Jogo` pode existir como um tipo específico de evento, mas não deve ser a base da modelagem.

Fluxo conceitual principal:

```text
Modalidade
    ↓
Competição
    ↓
Temporada ou edição
    ↓
Evento
    ↓
Participantes
    ↓
Resultados
```

Um participante pode ser:

* Atleta;
* Equipe;
* Seleção;
* Clube;
* Dupla;
* Piloto;
* Lutador;
* Outro representante competitivo.

---

# 4. Objetivo atual do projeto

O objetivo inicial é construir um MVP capaz de:

* Exibir eventos internacionais com representação brasileira;
* Organizar eventos por data;
* Exibir agenda de hoje, amanhã e próximos dias;
* Permitir filtros por modalidade, competição, atleta e equipe;
* Exibir detalhes do evento;
* Informar onde assistir, quando disponível;
* Permitir cadastro e correção manual pelo painel administrativo;
* Sincronizar dados por meio de APIs externas;
* Preparar a estrutura para scraping e outras fontes;
* Permitir favoritos;
* Preparar notificações;
* Manter uma arquitetura evolutiva e demonstrável em portfólio.

A prioridade não é cobrir imediatamente todas as modalidades existentes. A primeira versão deve cobrir poucas fontes confiáveis e funcionar de maneira consistente.

---

# 5. Roadmap do produto

## 5.1. Fase inicial — Fundação técnica

### Objetivo

Construir a estrutura principal do sistema.

### Escopo

* Monólito modular;
* Banco PostgreSQL;
* Estrutura base da API;
* Entidades principais;
* Migrations;
* Swagger/OpenAPI;
* Integração com uma primeira fonte de dados;
* Worker de sincronização;
* Aplicação web inicial;
* Aplicativo mobile inicial;
* Docker;
* Logs;
* Testes básicos;
* CI/CD inicial.

---

## 5.2. MVP — Agenda brasileira

### Objetivo

Entregar uma agenda funcional de eventos internacionais envolvendo brasileiros.

### Escopo

* Eventos de hoje;
* Eventos de amanhã;
* Próximos eventos;
* Calendário mensal;
* Filtros por modalidade;
* Detalhes do evento;
* Participantes brasileiros;
* Competição;
* Horário;
* Status;
* Resultado;
* Onde assistir;
* Painel administrativo;
* Sincronização automática;
* Correções manuais;
* Tratamento de duplicidade;
* Interface responsiva;
* Aplicativo Android funcional.

### Critério de conclusão

O produto deve conseguir responder de maneira confiável:

> “Onde e quando o Brasil compete nos próximos dias?”

---

## 5.3. V1 — Personalização

### Objetivo

Transformar a agenda em uma experiência personalizada.

### Escopo

* Cadastro e login;
* Favoritos;
* Modalidades favoritas;
* Atletas favoritos;
* Equipes favoritas;
* Competições favoritas;
* Notificações push;
* Preferências de notificação;
* Busca global;
* Deep links;
* Histórico de eventos;
* Melhorias de acessibilidade;
* Compartilhamento de eventos.

---

## 5.4. V1.5 — Ampliação de fontes

### Objetivo

Aumentar a cobertura esportiva.

### Escopo

* Novas APIs;
* Scrapers específicos;
* Monitoramento de falhas;
* Sistema de confiança das fontes;
* Conciliação de informações divergentes;
* Dashboard de sincronizações;
* Revisão manual de dados suspeitos;
* Cobertura de novas modalidades;
* Notícias relacionadas;
* Rankings;
* Medalhas e classificações.

---

## 5.5. V2 — Processamento inteligente

### Objetivo

Utilizar inteligência artificial para auxiliar na descoberta e normalização de eventos.

### Escopo

* Interpretar notícias;
* Detectar eventos futuros;
* Identificar participantes brasileiros;
* Extrair datas e horários;
* Normalizar nomes;
* Detectar duplicidades;
* Classificar modalidades;
* Sugerir fontes;
* Gerar resumos;
* Enviar eventos incertos para revisão administrativa.

A inteligência artificial não deve publicar dados automaticamente sem validação quando a confiança da extração for baixa.

---

# 6. Stack tecnológica

## 6.1. Frontend web

* React;
* TypeScript;
* Vite;
* React Router;
* TanStack Query;
* Tailwind CSS;
* shadcn/ui;
* React Hook Form;
* Zod, quando necessário para validação no frontend;
* Cliente HTTP centralizado;
* Biblioteca de notificações visuais;
* Ferramenta de testes para componentes e fluxos.

### Responsabilidades

* Site público;
* Calendário;
* Busca;
* Página de eventos;
* Página de competições;
* Página de atletas e equipes;
* Favoritos;
* Autenticação;
* Painel administrativo;
* Dashboard de sincronização.

---

## 6.2. Aplicativo mobile

* React Native;
* TypeScript;
* Expo;
* Expo Router;
* TanStack Query;
* NativeWind;
* Expo Notifications;
* AsyncStorage ou SecureStore conforme o tipo de dado;
* EAS Build;
* EAS Update, quando aplicável.

### Responsabilidades

* Agenda diária;
* Calendário;
* Busca;
* Favoritos;
* Notificações push;
* Detalhes dos eventos;
* Compartilhamento;
* Deep linking;
* Experiência mobile principal do produto.

---

## 6.3. Backend

* C#;
* .NET 9;
* ASP.NET Core;
* Minimal APIs;
* Entity Framework Core;
* PostgreSQL;
* Npgsql;
* FluentValidation;
* Mapster;
* JWT;
* Swagger/OpenAPI;
* Serilog;
* Hangfire;
* BackgroundService;
* Redis;
* Clientes HTTP com `HttpClientFactory`;
* Polly ou mecanismos equivalentes de resiliência;
* xUnit;
* FluentAssertions;
* Moq, quando necessário.

---

## 6.4. Infraestrutura e ferramentas

* Git;
* GitHub;
* Docker;
* Docker Compose;
* GitHub Actions;
* PostgreSQL;
* Redis;
* PgAdmin;
* Mailpit;
* Swagger;
* Postman ou Bruno;
* Neon PostgreSQL;
* Upstash Redis;
* Vercel;
* Render;
* Expo EAS.

Os serviços de produção podem ser alterados caso os planos gratuitos deixem de atender ao projeto. A aplicação não deve depender de comportamentos exclusivos de um único provedor.

---

# 7. Arquitetura geral

O projeto deve utilizar um **Monólito Modular**.

A solução permanece implantada inicialmente como uma única aplicação de backend, mas o código deve ser organizado em módulos de domínio independentes.

Módulos iniciais sugeridos:

```text
Auth
Users
Sports
Competitions
Events
Participants
Calendar
Favorites
Notifications
Broadcasts
News
Search
Sync
Administration
```

Cada módulo deve concentrar:

* Entidades;
* Contratos;
* Validações;
* Casos de uso;
* Endpoints;
* Persistência específica;
* Testes.

Não devem ser criados microsserviços apenas para demonstrar arquitetura. Um módulo poderá ser extraído futuramente somente quando houver uma necessidade concreta.

---

# 8. Fluxo geral dos dados

```text
APIs externas e Scrapers
            ↓
Clientes de integração
            ↓
Worker de sincronização
            ↓
Validação e normalização
            ↓
Detecção de duplicidades
            ↓
PostgreSQL
            ↓
Invalidação ou atualização do cache
            ↓
API do Brasil Compete
            ↓
Frontend Web e Aplicativo Mobile
```

---

# 9. Fluxo de sincronização

Uma sincronização deve seguir, sempre que aplicável, o fluxo:

```text
Buscar dados externos
        ↓
Registrar execução da fonte
        ↓
Validar resposta
        ↓
Converter para modelo interno
        ↓
Normalizar nomes, datas e participantes
        ↓
Identificar possíveis duplicidades
        ↓
Criar ou atualizar registros
        ↓
Registrar alterações
        ↓
Invalidar cache relacionado
        ↓
Preparar notificações
        ↓
Registrar sucesso ou falha
```

O sistema não deve remover imediatamente um evento apenas porque ele deixou de aparecer em uma fonte. Eventos ausentes devem ser marcados para revisão ou confirmados por execuções posteriores.

---

# 10. Princípios arquiteturais

## 10.1. Separação de responsabilidades

Cada parte do sistema deve possuir uma função clara.

* Endpoint recebe a requisição HTTP;
* Validator valida o contrato de entrada;
* Service ou Handler coordena o caso de uso;
* Repository ou camada de persistência acessa dados;
* DTO representa contratos externos;
* Entity representa dados persistidos;
* Integration Client acessa serviços externos;
* Normalizer converte dados externos para o padrão interno;
* Worker executa sincronizações;
* Hook controla lógica de interface;
* Component renderiza conteúdo;
* API Client do frontend realiza comunicação HTTP.

---

## 10.2. Reutilização

Antes de criar uma implementação nova, deve-se verificar se já existe:

* Componente visual;
* Hook;
* Serviço;
* Tipo;
* Query key;
* Cliente HTTP;
* Normalizador;
* Estratégia de sincronização;
* Validador;
* Mapper;
* Estado de loading;
* Empty state;
* Modal;
* Filtro;
* Card de evento;
* Componente de calendário.

Reutilização não significa criar componentes genéricos prematuramente. A abstração deve surgir quando houver uma repetição real ou uma regra compartilhada clara.

---

## 10.3. Baixo acoplamento

* Componentes não acessam APIs diretamente;
* Endpoints não consultam o DbContext diretamente;
* Regras de domínio não dependem de React, Expo ou infraestrutura;
* Workers não devem possuir regras duplicadas dos Services;
* Integrações externas não devem expor seus formatos diretamente para o restante do sistema;
* O frontend não deve depender de nomes internos das tabelas;
* O código de uma fonte externa não deve contaminar os demais módulos.

---

## 10.4. Tipagem forte

Evitar `any`, `dynamic` e `object` quando a estrutura for conhecida.

Devem existir tipos específicos para:

* Requests;
* Responses;
* Parâmetros de rota;
* Filtros;
* Paginação;
* Formulários;
* Respostas externas;
* Participantes;
* Eventos;
* Resultados;
* Transmissões;
* Preferências;
* Payloads de notificações;
* Dados armazenados localmente.

---

## 10.5. Evolução incremental

O sistema deve evoluir sem reescritas desnecessárias.

Novas modalidades, fontes ou tipos de evento devem ser adicionados aproveitando os contratos e estruturas existentes.

---

# 11. Organização sugerida da solução backend

```text
src/
  BrasilCompete.Api/
  BrasilCompete.Application/
  BrasilCompete.Domain/
  BrasilCompete.Infrastructure/
  BrasilCompete.Worker/

tests/
  BrasilCompete.UnitTests/
  BrasilCompete.IntegrationTests/
  BrasilCompete.ArchitectureTests/
```

Essa divisão pode ser simplificada durante o início, desde que as responsabilidades permaneçam claras.

Uma alternativa modular interna:

```text
Modules/
  Events/
    Domain/
    Application/
    Infrastructure/
    Endpoints/

  Competitions/
  Participants/
  Favorites/
  Notifications/
  Sync/
```

---

# 12. Minimal APIs e endpoints

Como o backend utiliza Minimal APIs, os endpoints substituem os Controllers tradicionais.

Responsabilidades dos endpoints:

* Receber requisições;
* Ler parâmetros;
* Obter identidade do usuário;
* Chamar o caso de uso;
* Retornar códigos HTTP adequados;
* Documentar respostas;
* Aplicar autenticação e autorização;
* Não conter regras complexas.

Exemplo conceitual:

```text
GET /api/events
    ↓
Recebe filtros
    ↓
Chama EventService ou Query Handler
    ↓
Retorna resultado paginado
```

Endpoints não devem:

* Executar consultas extensas com Entity Framework;
* Implementar sincronização;
* Normalizar dados externos;
* Conter regras de favoritos;
* Montar manualmente estruturas grandes;
* Repetir tratamento de erro.

Os endpoints devem ser agrupados por módulo usando métodos de extensão, como:

```csharp
app.MapEventEndpoints();
app.MapCompetitionEndpoints();
app.MapFavoriteEndpoints();
```

---

# 13. Services e casos de uso

Os Services ou Handlers representam os casos de uso do sistema.

Responsabilidades:

* Aplicar regras de negócio;
* Coordenar persistência;
* Validar existência de entidades;
* Aplicar permissões;
* Mapear dados;
* Normalizar filtros;
* Coordenar cache;
* Garantir integridade;
* Registrar alterações relevantes;
* Disparar eventos internos ou notificações quando necessário.

Exemplos:

```text
CreateEvent
UpdateEvent
GetEventDetails
SearchEvents
FavoriteParticipant
SynchronizeSource
NormalizeExternalEvent
ScheduleNotification
```

Services não devem:

* Renderizar mensagens de interface;
* Conter código específico de React ou Expo;
* Realizar scraping diretamente;
* Conhecer detalhes desnecessários de provedores;
* Duplicar validações mantidas em componentes próprios.

---

# 14. Repositories e Entity Framework

O padrão Repository deve ser utilizado quando ele realmente encapsular consultas ou persistência de domínio.

Não é obrigatório criar uma interface e um Repository genérico para cada entidade.

Pode-se utilizar:

* Repository específico para agregados complexos;
* Query services para leituras;
* DbContext diretamente na infraestrutura dos casos simples;
* Projeções para DTOs em consultas;
* Specification quando houver ganho real.

Evitar estruturas como:

```text
GenericRepository<T>
```

quando elas apenas repetirem os métodos já oferecidos pelo Entity Framework.

Repositories não devem:

* Conter regras de negócio;
* Retornar objetos específicos da interface;
* Criar mensagens para usuários;
* Conhecer componentes do frontend;
* Misturar consultas de módulos sem justificativa.

---

# 15. Models e entidades principais

Entidades iniciais sugeridas:

* User;
* Sport;
* Competition;
* CompetitionEdition;
* Event;
* EventParticipant;
* Athlete;
* Team;
* Country;
* Venue;
* Result;
* Broadcast;
* Source;
* SourceExecution;
* ExternalReference;
* Favorite;
* NotificationPreference;
* Notification;
* NewsArticle.

## 15.1. Evento

O evento deve conter, conforme aplicável:

* Identificador;
* Nome;
* Tipo;
* Data e horário;
* Fuso horário original;
* Data normalizada em UTC;
* Status;
* Modalidade;
* Competição;
* Edição ou temporada;
* Fase;
* Local;
* Participantes;
* Resultado;
* Fonte;
* Referência externa;
* Indicador de participação brasileira;
* Grau de confiança;
* Data da última sincronização.

---

## 15.2. Participantes

Atletas e equipes possuem características diferentes, mas devem poder participar de eventos por meio de uma estrutura comum.

Evitar duplicar eventos específicos para atleta e para equipe.

Uma relação como `EventParticipant` pode representar:

* Participante;
* País representado;
* Papel;
* Lado ou posição;
* Ordem;
* Resultado;
* Classificação;
* Indicador de brasileiro.

---

# 16. DTOs

DTOs representam contratos da API.

Devem:

* Evitar expor entidades diretamente;
* Separar criação, edição, sincronização e leitura;
* Utilizar estruturas concretas;
* Representar corretamente campos opcionais;
* Ser compatíveis com web e mobile;
* Evitar propriedades vagas;
* Ter nomes consistentes.

Exemplos:

```text
CreateEventRequest
UpdateEventRequest
EventSummaryResponse
EventDetailsResponse
EventFilterRequest
ParticipantSummaryResponse
BroadcastResponse
PagedResponse<T>
```

O sufixo `Dto` pode ser utilizado, mas o projeto deve escolher entre `Dto`, `Request` e `Response` e manter consistência.

---

# 17. Validação

A validação deve ser feita com FluentValidation no backend.

Deve cobrir:

* Campos obrigatórios;
* Tamanhos máximos;
* Intervalos;
* Datas inválidas;
* Identificadores;
* Status permitidos;
* Tipos de eventos;
* URLs;
* Regras de filtros;
* Combinações inválidas de campos.

A validação do frontend melhora a experiência, mas não substitui a validação do backend.

---

# 18. Datas, horários e fusos

Este é um ponto crítico do projeto.

Regras obrigatórias:

* Datas de eventos devem ser armazenadas em UTC;
* O fuso original da fonte deve ser preservado quando relevante;
* A conversão para o horário do usuário deve ocorrer na apresentação;
* Não assumir que todas as fontes usam horário de Brasília;
* Eventos sem horário confirmado devem suportar horário nulo;
* Eventos com data provisória devem ser identificados;
* Alterações de horário devem atualizar notificações;
* O horário apresentado deve informar quando ainda não estiver confirmado.

Evitar armazenar datas esportivas apenas como texto.

---

# 19. Integrações externas

Cada fonte deve possuir um cliente próprio.

Exemplo:

```text
Integrations/
  ApiSports/
  Fivb/
  Itft/
  Liquipedia/
  Manual/
```

Cada integração deve conter:

* Cliente HTTP;
* Contratos externos;
* Mapper;
* Normalizador;
* Configuração;
* Tratamento de erros;
* Estratégia de autenticação;
* Testes;
* Identificação da fonte.

O restante do sistema não deve depender diretamente do formato externo.

---

# 20. Scraping

Scraping deve ser usado apenas quando:

* Não houver API adequada;
* A utilização for permitida;
* A fonte for relevante;
* O custo de manutenção for aceitável.

Cada scraper deve:

* Ser isolado;
* Possuir rate limit;
* Definir User-Agent adequadamente;
* Tratar alterações de HTML;
* Registrar falhas;
* Não bloquear todo o processo;
* Possuir testes sobre amostras quando possível;
* Evitar chamadas excessivas;
* Respeitar regras e termos da fonte.

A aplicação nunca deve depender de um único scraper sem alternativa administrativa.

---

# 21. Normalização e deduplicação

Fontes diferentes podem representar o mesmo evento de maneiras distintas.

Exemplo:

```text
Brasil x Japão
BRA vs JPN
Brazil - Japan
Seleção Brasileira vs Japan
```

A normalização deve considerar:

* Modalidade;
* Competição;
* Participantes;
* Data;
* Horário;
* Fase;
* Local;
* Identificadores externos;
* Fonte;
* Status.

Não utilizar apenas o título como critério de duplicidade.

Eventos com baixa confiança devem ser enviados para revisão administrativa.

---

# 22. Hangfire e BackgroundService

## 22.1. Hangfire

Usar para tarefas agendadas ou controladas, como:

* Sincronização periódica;
* Atualização de resultados;
* Geração de notificações;
* Tentativas posteriores;
* Limpeza programada;
* Atualizações de rankings.

## 22.2. BackgroundService

Usar para processos contínuos ou consumidores internos quando necessário.

Não utilizar Hangfire e BackgroundService para executar exatamente o mesmo papel.

Cada job deve:

* Ser idempotente;
* Registrar início e fim;
* Tratar cancelamento;
* Não gerar duplicidade;
* Possuir política de retry;
* Registrar falhas;
* Suportar execução manual administrativa.

---

# 23. Cache com Redis

Redis deve ser usado apenas onde houver benefício claro.

Possíveis usos:

* Eventos de hoje;
* Próximos eventos;
* Calendário resumido;
* Competições populares;
* Rankings;
* Notícias;
* Controle de concorrência de sincronizações.

Estratégia inicial recomendada:

```text
Cache-aside
```

Fluxo:

```text
Consulta cache
    ↓
Existe → retorna
    ↓
Não existe → consulta banco
    ↓
Armazena no cache
    ↓
Retorna
```

Toda atualização relevante deve invalidar as chaves relacionadas.

O sistema deve continuar funcionando caso o Redis esteja temporariamente indisponível.

---

# 24. Logging e observabilidade

Serilog deve registrar logs estruturados.

Informações importantes:

* Correlation ID;
* Nome da fonte;
* Identificador da execução;
* Duração;
* Quantidade de registros recebidos;
* Quantidade criada;
* Quantidade atualizada;
* Duplicidades;
* Falhas;
* Retry;
* Endpoint;
* Status HTTP.

Nunca registrar:

* Senhas;
* Tokens;
* Segredos;
* Dados pessoais desnecessários;
* Headers sensíveis.

---

# 25. Arquitetura do frontend web

O frontend deve ser organizado por funcionalidades.

Estrutura sugerida:

```text
src/
  app/
  routes/
  features/
    auth/
    calendar/
    events/
    competitions/
    participants/
    favorites/
    notifications/
    search/
    admin/
  components/
  services/
  hooks/
  lib/
  types/
  assets/
```

A pasta `features` deve concentrar código específico de cada domínio.

A pasta `components` deve conter elementos realmente compartilhados.

---

# 26. TanStack Query

TanStack Query deve controlar o estado proveniente do servidor.

Responsabilidades:

* Requisições;
* Cache;
* Invalidação;
* Retry;
* Paginação;
* Atualização;
* Loading;
* Refetch;
* Mutations.

Não duplicar dados de servidor em Context API ou estado global sem necessidade.

Query keys devem ser centralizadas:

```typescript
eventKeys.all
eventKeys.list(filters)
eventKeys.details(id)
eventKeys.calendar(month)
```

Após uma mutation, invalidar apenas as queries afetadas.

---

# 27. Estado local e global

Usar estado local para:

* Modal aberto;
* Aba selecionada;
* Valores temporários;
* Filtros ainda não aplicados;
* Controle de interface.

Usar Context ou outra solução global apenas para informações realmente globais, como:

* Sessão;
* Tema;
* Preferências gerais;
* Configuração de idioma.

TanStack Query não deve ser substituído por Redux para dados do servidor.

---

# 28. Componentes

Todo componente deve possuir responsabilidade clara.

Componentes visuais não devem:

* Realizar chamadas HTTP;
* Conhecer detalhes de autenticação;
* Conter regras extensas;
* Alterar diretamente o cache;
* Misturar múltiplos domínios.

Estrutura possível:

```text
EventCard/
  EventCard.tsx
  EventCard.types.ts
  EventCard.test.tsx
  index.ts
```

Como Tailwind e NativeWind serão utilizados, não é obrigatório criar um arquivo de estilo separado para cada componente.

Classes extensas e repetidas devem ser extraídas para variantes ou componentes reutilizáveis.

---

# 29. shadcn/ui e componentes compartilhados

Os componentes do shadcn/ui devem ser tratados como código do próprio projeto.

Eles podem ser adaptados, desde que:

* A acessibilidade seja preservada;
* A API do componente permaneça clara;
* As alterações sejam consistentes;
* Não exista duplicação desnecessária;
* O design system seja respeitado.

Componentes compartilhados possíveis:

* Button;
* Input;
* Select;
* Modal;
* Drawer;
* Calendar;
* DatePicker;
* EventCard;
* ParticipantAvatar;
* SportBadge;
* StatusBadge;
* EmptyState;
* ErrorState;
* Skeleton;
* Pagination;
* SearchInput;
* FilterSheet.

---

# 30. Hooks

Hooks devem controlar lógica reutilizável de interface.

Podem conter:

* Preparação de filtros;
* Controle de formulários;
* Navegação;
* Estado de modais;
* Mapeamento de respostas;
* Composição de queries;
* Regras de apresentação.

Hooks não devem:

* Conter JSX;
* Duplicar funcionalidades do TanStack Query;
* Tornar-se arquivos gigantes;
* Misturar vários domínios sem necessidade.

---

# 31. Services do frontend

A comunicação HTTP deve permanecer centralizada.

Estrutura possível:

```text
services/
  apiClient.ts
  authService.ts
  eventService.ts
  competitionService.ts
  participantService.ts
  favoriteService.ts
  notificationService.ts
```

Responsabilidades:

* Definir URL;
* Inserir token;
* Tipar requests;
* Tipar responses;
* Tratar erros comuns;
* Aplicar interceptadores;
* Não realizar regras visuais.

Componentes não devem chamar `fetch` ou Axios diretamente.

---

# 32. Arquitetura mobile

O aplicativo deve utilizar Expo Router.

Estrutura sugerida:

```text
app/
  _layout.tsx
  (tabs)/
    index.tsx
    calendar.tsx
    discover.tsx
    favorites.tsx
    profile.tsx
  events/
    [id].tsx
  competitions/
    [id].tsx
  athletes/
    [id].tsx
  teams/
    [id].tsx
```

O código de domínio deve permanecer fora da pasta `app` quando não representar uma rota.

```text
src/
  features/
  components/
  services/
  hooks/
  types/
  lib/
```

---

# 33. Compartilhamento entre web e mobile

Web e mobile podem compartilhar:

* Contratos TypeScript;
* Tipos de API;
* Schemas;
* Constantes;
* Formatadores;
* Query keys;
* Regras puras;
* Cliente gerado a partir do OpenAPI, se adotado.

Não forçar compartilhamento de componentes visuais entre React Web e React Native quando isso prejudicar a experiência ou aumentar a complexidade.

---

# 34. Notificações

As notificações devem considerar:

* Fuso horário do usuário;
* Alteração de horário;
* Evento cancelado;
* Evento adiado;
* Início próximo;
* Resultado;
* Classificação;
* Favoritos;
* Preferências individuais.

Fluxo:

```text
Evento elegível
    ↓
Usuários interessados
    ↓
Preferências
    ↓
Agendamento
    ↓
Envio push
    ↓
Registro do resultado
```

As notificações devem ser idempotentes para impedir envio duplicado.

O payload deve conter informações suficientes para deep linking.

---

# 35. Autenticação e autorização

O sistema utiliza JWT.

Deve:

* Proteger endpoints privados;
* Validar tokens;
* Utilizar refresh token caso seja implementado;
* Diferenciar usuário comum e administrador;
* Validar permissões no backend;
* Proteger operações administrativas;
* Não confiar em informações enviadas pelo frontend;
* Armazenar tokens mobile de forma segura.

No mobile, dados sensíveis devem utilizar SecureStore, não AsyncStorage.

---

# 36. Painel administrativo

O painel administrativo deve permitir:

* Criar eventos;
* Editar eventos;
* Corrigir participantes;
* Corrigir horários;
* Gerenciar fontes;
* Visualizar sincronizações;
* Reexecutar jobs;
* Resolver duplicidades;
* Revisar dados com baixa confiança;
* Gerenciar transmissões;
* Publicar ou ocultar eventos;
* Consultar logs funcionais.

A correção manual não deve ser sobrescrita automaticamente sem uma regra explícita.

Campos modificados manualmente podem possuir um marcador como:

```text
ManualOverride
```

---

# 37. Formulários

Formulários devem:

* Utilizar componentes compartilhados;
* Ser tipados;
* Validar antes de enviar;
* Exibir mensagens claras;
* Tratar loading;
* Evitar duplicação entre criação e edição;
* Mapear corretamente dados recebidos;
* Tratar campos opcionais;
* Bloquear submissões duplicadas.

React Hook Form pode controlar formulários, e Zod pode ser utilizado quando houver ganho na validação e inferência de tipos.

---

# 38. Busca

A busca deve evoluir em fases.

## MVP

* Nome de participante;
* Competição;
* Modalidade;
* Evento.

## V1

* Sugestões;
* Resultados agrupados;
* Filtros;
* Histórico;
* Busca parcial;
* Tratamento de acentos.

## Futuro

* PostgreSQL Full Text Search;
* Similaridade;
* Busca semântica;
* Busca por contexto;
* IA.

Não adicionar Elasticsearch sem necessidade concreta.

---

# 39. Performance

Boas práticas:

* Utilizar projeções;
* Paginar resultados;
* Evitar `Include` excessivo;
* Utilizar `AsNoTracking` em leituras;
* Criar índices para filtros frequentes;
* Evitar requests duplicadas;
* Configurar corretamente `staleTime`;
* Usar listas virtualizadas no mobile;
* Comprimir imagens;
* Evitar carregar detalhes em cards resumidos;
* Aplicar cache com critério;
* Não retornar históricos extensos sem paginação.

---

# 40. Segurança

Cuidados mínimos:

* Não versionar segredos;
* Usar variáveis de ambiente;
* Validar todas as entradas;
* Proteger endpoints administrativos;
* Usar consultas parametrizadas;
* Evitar exposição de exceções;
* Aplicar rate limiting quando necessário;
* Proteger autenticação;
* Validar URLs externas;
* Sanitizar conteúdo proveniente de fontes;
* Restringir CORS;
* Atualizar dependências;
* Revisar permissões de scraping;
* Não confiar em identificadores enviados pelo cliente.

---

# 41. Tratamento de erros

Toda funcionalidade deve tratar:

* Loading;
* Erro;
* Sucesso;
* Estado vazio;
* Dados incompletos;
* Horário não confirmado;
* Fonte indisponível;
* Erro de autenticação;
* Erro de validação;
* Falha de sincronização;
* Falha de cache;
* Falha de notificação;
* Conectividade mobile.

A API deve utilizar respostas consistentes, preferencialmente seguindo Problem Details.

---

# 42. Testes

## Backend

* Testes de Services;
* Testes de normalização;
* Testes de deduplicação;
* Testes de validação;
* Testes de datas e fusos;
* Testes de autorização;
* Testes de integração;
* Testes de endpoints;
* Testes de jobs;
* Testes de idempotência.

## Frontend web

* Testes de componentes;
* Testes de hooks;
* Testes de filtros;
* Testes de formulários;
* Testes de estados de erro;
* Testes de navegação.

## Mobile

* Testes de componentes críticos;
* Testes de navegação;
* Testes de armazenamento;
* Testes de deep linking;
* Testes de favoritos;
* Testes de notificações quando viável.

## E2E

Fluxos prioritários:

* Visualizar agenda;
* Filtrar eventos;
* Abrir detalhes;
* Fazer login;
* Favoritar;
* Alterar preferências;
* Corrigir evento pelo painel.

---

# 43. Docker

O ambiente de desenvolvimento deve possuir Docker Compose com:

```text
API
Worker
PostgreSQL
Redis
PgAdmin
Mailpit
```

O frontend web e mobile podem ser executados fora do Docker para facilitar hot reload.

Docker não deve ser usado apenas para aparência no portfólio. O ambiente deve ser reproduzível e documentado.

---

# 44. CI/CD

GitHub Actions deve executar:

* Restore;
* Build;
* Testes;
* Verificação de formatação;
* Lint;
* Build do frontend;
* Validação de migrations;
* Geração de artefatos;
* Deploy, quando configurado.

Pull requests não devem ser aprovadas com build ou testes críticos falhando.

Segredos devem permanecer configurados no provedor e nunca no repositório.

---

# 45. Nomenclatura

## Backend

* Classes em PascalCase;
* Interfaces iniciadas com `I`, quando utilizadas;
* Métodos em PascalCase;
* Propriedades em PascalCase;
* Métodos assíncronos terminando com `Async`;
* Requests terminando em `Request`;
* Responses terminando em `Response`;
* Validators terminando em `Validator`;
* Services terminando em `Service`;
* Repositories terminando em `Repository`;
* Jobs terminando em `Job`;
* Clients terminando em `Client`;
* Normalizadores terminando em `Normalizer`.

## Frontend e mobile

* Componentes em PascalCase;
* Hooks iniciados com `use`;
* Funções em camelCase;
* Types em PascalCase;
* Services terminando em `Service`;
* Query keys terminando em `Keys`;
* Constantes globais em UPPER_CASE;
* Evitar nomes genéricos como `data2`, `obj`, `valueTemp` ou `itemFinal`.

---

# 46. Clean Code

Regras esperadas:

* Nomes claros;
* Funções pequenas;
* Responsabilidades separadas;
* Retornos antecipados quando melhorarem a leitura;
* Ausência de código morto;
* Sem logs temporários em produção;
* Sem duplicação desnecessária;
* Comentários usados para contexto, não para explicar código confuso;
* Tratamento explícito de efeitos colaterais;
* Tipos reutilizados;
* Métodos assíncronos com cancelamento quando aplicável;
* Não esconder falhas de integração.

---

# 47. SOLID aplicado ao projeto

## Single Responsibility

* Endpoint controla HTTP;
* Service coordena negócio;
* Repository controla persistência;
* Client acessa fonte externa;
* Normalizer transforma dados;
* Worker executa processamento;
* Hook controla interface;
* Component renderiza conteúdo.

## Open/Closed

Novos esportes, tipos de evento e fontes devem ser adicionados sem reescrever todo o núcleo.

## Liskov Substitution

Implementações de contratos devem preservar o comportamento esperado.

## Interface Segregation

Evitar interfaces gigantes para fontes, Services ou Repositories.

## Dependency Inversion

Casos de uso devem depender de abstrações de infraestrutura quando isso facilitar isolamento e testes.

---

# 48. Critérios para novas funcionalidades

Antes de implementar algo, responder:

1. A funcionalidade pertence ao MVP atual?
2. Já existe componente ou regra reutilizável?
3. Ela pertence ao web, mobile, backend ou Worker?
4. Existe impacto em eventos já sincronizados?
5. Existe impacto em datas e fusos?
6. Existe impacto em cache?
7. Existe impacto em notificações?
8. Existe impacto em favoritos?
9. Existe impacto em fontes externas?
10. Existe impacto no painel administrativo?
11. Exige migration?
12. Exige atualização de contratos?
13. Exige compatibilidade com versões antigas do app?
14. Pode gerar duplicidade?
15. Precisa ser idempotente?
16. Precisa de observabilidade?
17. Como será testada?

---

# 49. Orientações para inteligências artificiais

## 49.1. Antes de alterar

Toda IA deve:

* Ler os arquivos envolvidos;
* Examinar módulos relacionados;
* Procurar padrões existentes;
* Identificar contratos;
* Identificar DTOs;
* Identificar validators;
* Identificar queries;
* Identificar componentes compartilhados;
* Identificar migrations;
* Entender o fluxo completo;
* Verificar web e mobile quando o contrato for compartilhado.

## 49.2. Durante a implementação

A IA deve:

* Não reescrever sem necessidade;
* Não criar arquitetura paralela;
* Não introduzir bibliotecas sem autorização;
* Não criar componentes duplicados;
* Não alterar layout sem pedido;
* Não misturar regras de negócio em endpoints;
* Não realizar chamada HTTP em componente visual;
* Manter tipagem;
* Preservar contratos existentes;
* Reutilizar query keys;
* Tratar loading e erros;
* Considerar fusos horários;
* Considerar sincronização e cache;
* Criar ou atualizar testes.

## 49.3. Depois da implementação

A IA deve:

* Executar ou revisar build;
* Revisar TypeScript;
* Revisar imports;
* Revisar warnings;
* Remover código morto;
* Validar migrations;
* Testar fluxos afetados;
* Verificar null;
* Verificar datas;
* Verificar payload;
* Verificar resposta;
* Verificar cache;
* Verificar compatibilidade;
* Informar arquivos alterados;
* Explicar decisões relevantes;
* Apontar o que não conseguiu validar.

---

# 50. O que uma IA não deve fazer

* Criar toda a funcionalidade do zero sem analisar o projeto;
* Adicionar microsserviços sem necessidade;
* Trocar a stack;
* Introduzir Redux sem justificativa;
* Usar `any` indiscriminadamente;
* Chamar APIs diretamente em componentes;
* Colocar regras de negócio em endpoints;
* Criar Repository genérico apenas por padrão;
* Alterar contratos silenciosamente;
* Editar migration sem compreender o impacto;
* Misturar formatos externos com entidades internas;
* Ignorar fuso horário;
* Sobrescrever correções manuais;
* Criar jobs não idempotentes;
* Apagar eventos porque uma fonte falhou;
* Ocultar falhas de integração;
* Alterar layout sem solicitação;
* Remover funcionalidades existentes;
* Quebrar versões publicadas do aplicativo.

---

# 51. Definition of Done

Uma funcionalidade somente deve ser considerada concluída quando os itens aplicáveis estiverem atendidos:

* Regra de negócio implementada;
* Backend implementado;
* Endpoint documentado;
* Contracts atualizados;
* Validators atualizados;
* Persistência atualizada;
* Migration criada e revisada;
* Web implementado;
* Mobile implementado, quando aplicável;
* Loading tratado;
* Erros tratados;
* Estado vazio tratado;
* Autorização validada;
* Datas e fusos validados;
* Cache considerado;
* Sincronização considerada;
* Logs adicionados quando relevantes;
* Testes criados ou atualizados;
* Build do backend funcionando;
* Build do frontend funcionando;
* TypeScript sem erros;
* Compatibilidade preservada;
* Documentação atualizada;
* Código revisado.

Nem toda tarefa exige alterações em todas as aplicações. A Definition of Done deve ser aplicada somente às partes afetadas, sem criar trabalho artificial.

---

# 52. Estrutura de referência para tarefas futuras

Toda tarefa passada para uma IA deve conter:

```text
Contexto do projeto
Objetivo
Escopo
Arquivos envolvidos
Comportamento atual
Comportamento esperado
Regras de negócio
Estrutura de dados
Fonte dos dados
Impacto em web
Impacto em mobile
Impacto em sincronização
Impacto em cache
Padrões obrigatórios
Restrições
Critérios de aceite
Testes esperados
```

---

# 53. Prioridades iniciais

1. Definir claramente o MVP;
2. Validar as primeiras fontes de dados;
3. Modelar eventos e participantes;
4. Criar o monólito modular;
5. Configurar PostgreSQL e Entity Framework;
6. Implementar a primeira sincronização;
7. Criar agenda diária;
8. Criar calendário;
9. Criar detalhes de eventos;
10. Criar painel de correções;
11. Implementar web responsivo;
12. Implementar aplicativo mobile;
13. Adicionar favoritos;
14. Adicionar notificações;
15. Melhorar cobertura esportiva.

---

# 54. Conclusão

O **Brasil Compete** não deve ser tratado como um simples calendário ou CRUD esportivo. O projeto possui três dimensões principais:

1. Agenda centralizada da representação brasileira;
2. Plataforma de integração e normalização de dados esportivos;
3. Produto personalizado com favoritos, notificações e automações.

A arquitetura deve preservar consistência, tipagem, separação de responsabilidades, observabilidade e evolução incremental.

Os padrões aproveitados do OdisseiaWiki continuam válidos quando relacionados a:

* Separação de camadas;
* Services;
* DTOs;
* Entity Framework;
* Tipagem;
* Reutilização;
* Segurança;
* Testes;
* Clean Code;
* SOLID;
* Orientações para inteligências artificiais.

Entretanto, alguns padrões foram adaptados à nova stack:

* Minimal APIs substituem Controllers tradicionais;
* TanStack Query controla o estado de servidor;
* Tailwind e NativeWind substituem arquivos obrigatórios de Styled Components;
* Repository deixa de ser obrigatório para cada entidade;
* Monólito modular substitui a divisão genérica por camadas isoladas;
* Workers, integrações, normalização, deduplicação, cache e observabilidade passam a fazer parte central da arquitetura;
* Datas, fusos horários e idempotência tornam-se requisitos fundamentais do domínio.
