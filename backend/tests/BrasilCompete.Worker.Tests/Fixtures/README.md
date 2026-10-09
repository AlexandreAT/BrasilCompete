# Fixtures dos testes

Respostas reais das fontes, salvas em 09/10/2026, para os testes rodarem sem internet. Cada arquivo guarda só
fatos esportivos (datas, horários, participantes); nada aqui é republicado pelo app.

| Pasta | Fonte | Endpoint | Licença dos dados |
| --- | --- | --- | --- |
| `Jolpica/` | Jolpica-F1 | `api.jolpi.ca/ergast/f1/2026/races/` e `.../driverstandings/` | CC BY-NC-SA 4.0 (uso não comercial) |
| `Lichess/` | API da Lichess | `/api/broadcast`, `/api/broadcast/{id}`, `/broadcast/{id}/players`, `/api/broadcast/-/-/{roundId}` | Dados públicos da API da Lichess |
| `Wikipedia/` | Wikipedia em inglês | `/w/rest.php/v1/page/{título}/html` (HTML do Parsoid) | CC BY-SA 4.0 |
| `Liquipedia/` | Liquipedia (API do MediaWiki) | `api.php?action=query&prop=revisions` (wikitext) | CC BY-SA 3.0 |

## Recortes

Os HTMLs da Wikipedia foram recortados nas seções usadas pelos testes, para não pesar no repositório:

- `2026_Copa_Libertadores_final_stages.html`: seções "Quarter-finals", "Semi-finals" e "Final";
- `Brazil_womens_national_football_team.html`: seção "Results and fixtures";
- `List_of_UFC_events.html`: seções "Scheduled events" e "Past events", com as 60 primeiras linhas de tabela;
- `UFC_335.html`: seção "Fight card";
- `2026_US_Open_Mens_singles.html`: chaves das seções "Finals" e "Section 1" (só as predefinições, sem o HTML renderizado).

As respostas JSON estão completas.
