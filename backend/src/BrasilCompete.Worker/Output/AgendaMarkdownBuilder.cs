using System.Globalization;
using System.Text;

using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Pipeline;

namespace BrasilCompete.Worker.Output;

/// <summary>
/// Agenda legível, por visão e por dia no horário de Brasília, para revisão humana rápida (plano, seção 9.10).
/// </summary>
public static class AgendaMarkdownBuilder
{
    private static readonly CultureInfo Portuguese = CultureInfo.GetCultureInfo("pt-BR");

    public static string Build(PipelineResult result, DateWindow window, DateTimeOffset generatedAtUtc)
    {
        var builder = new StringBuilder();
        builder.AppendLine(CultureInfo.InvariantCulture, $"# Agenda — {window}");
        builder.AppendLine();
        builder.AppendLine(CultureInfo.InvariantCulture, $"Gerada em {BrasiliaTime.ToLocal(generatedAtUtc):dd/MM/yyyy HH:mm} (horário de Brasília). Eventos marcados com [TESTE] são exemplos da curadoria.");

        foreach (var view in Enum.GetValues<EventView>())
        {
            AppendView(builder, view, result.Events.Where(sportEvent => sportEvent.View == view).ToList());
        }

        AppendDiscarded(builder, result.Discarded);

        return builder.ToString();
    }

    private static void AppendView(StringBuilder builder, EventView view, List<SportEvent> events)
    {
        builder.AppendLine();
        builder.AppendLine(CultureInfo.InvariantCulture, $"## {PortugueseLabels.For(view)} ({events.Count})");

        if (events.Count == 0)
        {
            builder.AppendLine();
            builder.AppendLine("Nenhum evento.");

            return;
        }

        var dated = events.Where(sportEvent => sportEvent.Schedule.Date is not null);

        foreach (var day in dated.GroupBy(sportEvent => sportEvent.Schedule.Date!.Value).OrderBy(group => group.Key))
        {
            builder.AppendLine();
            builder.AppendLine(CultureInfo.InvariantCulture, $"### {day.Key.ToString("dd/MM/yyyy (dddd)", Portuguese)}");
            builder.AppendLine();

            foreach (var sportEvent in day)
            {
                builder.AppendLine(CultureInfo.InvariantCulture, $"- {FormatTime(sportEvent.Schedule)} — {Describe(sportEvent)}");
            }
        }

        AppendGroup(builder, "Períodos", events.Where(sportEvent => sportEvent.Schedule.Precision == SchedulePrecision.CompetitionPeriod), sportEvent =>
            $"{sportEvent.Schedule.PeriodStart:dd/MM} a {sportEvent.Schedule.PeriodEnd:dd/MM} — {Describe(sportEvent)}");

        AppendGroup(builder, "A confirmar", events.Where(sportEvent => sportEvent.Schedule.Precision == SchedulePrecision.ToBeConfirmed), Describe);
    }

    private static void AppendGroup(StringBuilder builder, string title, IEnumerable<SportEvent> events, Func<SportEvent, string> describe)
    {
        var list = events.ToList();

        if (list.Count == 0)
        {
            return;
        }

        builder.AppendLine();
        builder.AppendLine(CultureInfo.InvariantCulture, $"### {title}");
        builder.AppendLine();

        foreach (var sportEvent in list)
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"- {describe(sportEvent)}");
        }
    }

    private static void AppendDiscarded(StringBuilder builder, IReadOnlyList<DiscardedEvent> discarded)
    {
        builder.AppendLine();
        builder.AppendLine(CultureInfo.InvariantCulture, $"## Descartados ({discarded.Count})");
        builder.AppendLine();

        foreach (var group in discarded.GroupBy(item => item.Reason).OrderBy(group => group.Key))
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"- {PortugueseLabels.For(group.Key)}: {group.Count()}");
        }
    }

    private static string FormatTime(Schedule schedule) =>
        schedule.StartUtc is { } start ? $"{BrasiliaTime.ToLocal(start):HH:mm}" : "sem horário";

    private static string Describe(SportEvent sportEvent)
    {
        var who = sportEvent.Format == EventFormat.Matchup
            ? string.Join(" x ", sportEvent.Participants.Select(DescribeParticipant))
            : string.Join(", ", sportEvent.Participants.Select(DescribeParticipant));
        var stage = sportEvent.Stage is null ? string.Empty : $" — {sportEvent.Stage}";
        var sources = string.Join(", ", sportEvent.Sources.Select(source => source.Source).Distinct());
        var test = sportEvent.IsTestData ? "[TESTE] " : string.Empty;

        return $"{test}{who} — {PortugueseLabels.For(sportEvent.Sport)} — {sportEvent.Competition}{stage} · fontes: {sources}";
    }

    private static string DescribeParticipant(Participant participant)
    {
        var name = participant.IsBrazilian ? $"**{participant.Name}**" : participant.Name;
        var team = participant.Team is null ? string.Empty : $" ({participant.Team})";
        var brazilianMembers = participant.Members.Where(member => member.IsBrazilian).Select(member => member.Name).ToList();
        var members = brazilianMembers.Count == 0 || participant.Kind == ParticipantKind.Pair
            ? string.Empty
            : $" [com {string.Join(", ", brazilianMembers)}]";

        return $"{name}{team}{members}";
    }
}
