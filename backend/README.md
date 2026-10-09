# Brasil Compete — Worker do teste de viabilidade

Console app em .NET 10 que coleta eventos esportivos internacionais com brasileiros, a partir de fontes
gratuitas e permitidas, e gera a agenda consolidada. Faz parte do teste descrito em
[VIABILITY_PLAN.md](../VIABILITY_PLAN.md); os resultados ficam em [VIABILITY_REPORT.md](../VIABILITY_REPORT.md).

## Requisitos

- SDK do .NET 10 ([download oficial](https://dotnet.microsoft.com/download)). Confira com `dotnet --list-sdks`.
- Windows com PowerShell (os comandos abaixo também funcionam com `dotnet` direto em outros sistemas).

Sem administrador, o SDK pode ser instalado só para o usuário, com o script oficial da Microsoft:

```powershell
Invoke-WebRequest https://dot.net/v1/dotnet-install.ps1 -OutFile dotnet-install.ps1
.\dotnet-install.ps1 -Channel 10.0 -InstallDir "$env:LOCALAPPDATA\Microsoft\dotnet"
```

Nesse caso, o `dotnet` do PATH pode continuar sendo outra versão. O script `scripts/worker.ps1` encontra
o .NET 10 sozinho.

## Como rodar

Na raiz do repositório, no PowerShell:

```powershell
# Coleta uma janela de datas (no horário de Brasília, datas inclusivas)
.\backend\scripts\worker.ps1 collect --from 2026-09-01 --to 2026-09-30

# Coleta os próximos 14 dias, a partir de hoje
.\backend\scripts\worker.ps1 collect --days 14

# Só algumas fontes, ignorando o cache
.\backend\scripts\worker.ps1 collect --days 14 --sources manual,jolpica --no-cache

# Testes
.\backend\scripts\worker.ps1 test
```

Com o .NET 10 no PATH, o equivalente é
`dotnet run --project backend/src/BrasilCompete.Worker -- collect --from 2026-09-01 --to 2026-09-30`.

Se o PowerShell bloquear scripts, rode com
`powershell -ExecutionPolicy Bypass -File .\backend\scripts\worker.ps1 ...`.

## Saídas

Cada execução grava em `backend/output/runs/<data-hora>/` e copia para `backend/output/latest/`:

| Arquivo | Conteúdo |
| --- | --- |
| `events.json` | Eventos da janela, já consolidados, com fontes, licenças e conflitos entre fontes |
| `discarded.json` | Eventos descartados e o motivo (sem brasileiro, não internacional, fora da janela) |
| `run-summary.json` | Status, contagens, requisições, 429, duração e erros por fonte |
| `agenda.md` | Agenda legível, por visão e por dia, para revisão rápida |

Também ficam fora do Git:

- `backend/.state/history.json`: histórico entre execuções (primeira aparição, mudanças e eventos que sumiram);
- `backend/.cache/`: cache das respostas das fontes, para não repetir requisições durante o desenvolvimento.

## Estrutura

```text
backend/
├── BrasilCompete.slnx
├── curation/                     eventos manuais versionados (manual-events.json)
├── validation/                   entidades de referência e gabaritos
├── scripts/                      worker.ps1 e coleta diária
├── src/BrasilCompete.Worker/
│   ├── Commands/                 linha de comando (collect)
│   ├── Configuration/            opções, caminhos e injeção de dependência
│   ├── Domain/                   SportEvent, Participant, Schedule e enums
│   ├── History/                  histórico entre execuções
│   ├── Http/                     cache, espaçamento entre requisições, resiliência e métricas
│   ├── Identity/                 regras de "quem é brasileiro"
│   ├── Integrations/<Fonte>/     um adapter por fonte (Client, contratos, Mapper, Options)
│   ├── Normalization/            textos e fusos
│   ├── Output/                   events.json, run-summary.json e agenda.md
│   ├── Pipeline/                 filtro, visão, identificadores e deduplicação
│   └── Serialization/            formato JSON
└── tests/BrasilCompete.Worker.Tests/
    └── Fixtures/                 respostas reais salvas por fonte (testes sem internet)
```

## Regras de acesso às fontes

- Requisições identificadas com `BrasilCompete-Viability/0.1 (+https://github.com/AlexandreAT/BrasilCompete)`;
- Uma requisição por vez por fonte, com intervalo mínimo configurado em `appsettings.json`;
- Respostas 429 esperam o `Retry-After` (ou o tempo configurado, quando o cabeçalho não vem);
- Cache local das respostas, com validade por fonte.

## Curadoria manual

O arquivo `curation/manual-events.json` cobre lacunas e corrige dados das fontes. A curadoria vence todas as
fontes na deduplicação. Cada entrada tem:

- `key`: identificador estável da entrada;
- `sport`, `competition`, `stage`, `format` (`Matchup` ou `Participation`) e `scope`
  (`International`, `ForeignDomestic` ou `BrazilianDomestic`);
- `schedule`: `precision` (`DateAndTime`, `DateOnly`, `CompetitionPeriod` ou `ToBeConfirmed`) e os campos da
  precisão. Horários são locais (`localTime` em `HH:mm`) com o fuso IANA do local (`timeZone`);
- `participants`: nome, tipo, país (ISO alfa-3) e, opcionalmente, `isBrazilian` e `brazilianReason` para a
  curadoria decidir a identidade;
- `sourceUrl`: página oficial consultada;
- `isTestData`: `true` nos exemplos que só exercitam o fluxo.

## Chaves de API

Nenhum segredo vai para o repositório. As chaves ficam nos User Secrets da máquina:

```powershell
dotnet user-secrets set "<Fonte>:ApiKey" "SUA_CHAVE" --id brasil-compete-worker
```

O worker também lê variáveis de ambiente (por exemplo, `Balldontlie__ApiKey`).
