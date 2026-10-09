using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Output;

/// <summary>Rótulos em português para a agenda legível.</summary>
public static class PortugueseLabels
{
    public static string For(Sport sport) => sport switch
    {
        Sport.Football => "Futebol",
        Sport.Formula1 => "Fórmula 1",
        Sport.Chess => "Xadrez",
        Sport.Mma => "MMA",
        Sport.EsportsValorant => "VALORANT",
        Sport.EsportsCounterStrike => "Counter-Strike 2",
        Sport.EsportsLeagueOfLegends => "League of Legends",
        Sport.Basketball => "Basquete",
        Sport.Volleyball => "Vôlei",
        Sport.BeachVolleyball => "Vôlei de praia",
        Sport.Tennis => "Tênis",
        Sport.TableTennis => "Tênis de mesa",
        _ => sport.ToString(),
    };

    public static string For(EventView view) => view switch
    {
        EventView.Main => "Feed principal",
        EventView.Individuals => "Indivíduos",
        _ => view.ToString(),
    };

    public static string For(SchedulePrecision precision) => precision switch
    {
        SchedulePrecision.DateAndTime => "Horário marcado",
        SchedulePrecision.DateOnly => "Data marcada",
        SchedulePrecision.CompetitionPeriod => "Período",
        SchedulePrecision.ToBeConfirmed => "A confirmar",
        _ => precision.ToString(),
    };

    public static string For(DiscardReason reason) => reason switch
    {
        DiscardReason.NoBrazilian => "Sem brasileiro",
        DiscardReason.NotInternational => "Não internacional",
        DiscardReason.OutOfWindow => "Fora da janela",
        _ => reason.ToString(),
    };
}
