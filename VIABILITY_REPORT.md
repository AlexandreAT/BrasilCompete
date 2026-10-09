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
| 2 — Identidade e entidades de referência | Pendente | | |
| 3 — Fontes sem chave | Pendente | | |
| 4 — Consolidação | Pendente | | |
| 5 — Validação da janela passada | Pendente | | |
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

---

# 5. Fontes

Preencher na fase 3.

---

# 6. Identidade

Preencher na fase 2.

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

Preencher nas fases 4 e 5.

---

# 11. Curadoria manual necessária

Preencher nas fases 3, 5 e 8.

---

# 12. Execução

* **Como rodar:** `.\backend\scripts\worker.ps1 collect --from aaaa-mm-dd --to aaaa-mm-dd` (detalhes em `backend/README.md`).
* **Fase 1:** só com a curadoria (4 eventos de teste), a execução leva 0,3 s e gera `events.json`, `discarded.json`, `run-summary.json` e `agenda.md`.

---

# 13. Modalidades fora do teste

Preencher na fase 3.

---

# 14. Riscos e limitações aceitas

* Atletas pouco conhecidos, que não aparecem nas fontes nem no Wikidata, não serão capturados (limitação aceita no plano, seção 4.1).

---

# 15. Recomendações para o plano final

Preencher na fase 8.

---

# 16. Pendências manuais

Nenhuma no momento.
