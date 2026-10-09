using System.Globalization;

using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Integrations.Wikipedia.Parsoid;
using BrasilCompete.Worker.Integrations.Wikipedia.Wikitext;

namespace BrasilCompete.Worker.Integrations.Wikipedia.Football;

/// <summary>
/// Uma predefinição de partida vira um confronto. Com um lado ainda indefinido ("TBD"), vira a participação
/// do lado conhecido. Sem data legível, o evento fica "A confirmar".
/// </summary>
public static class FootballBoxMapper
{
    public static IReadOnlyList<SportEvent> ToEvents(
        IReadOnlyList<ParsoidTemplate> boxes,
        WikipediaPageOptions page,
        int fallbackYear,
        DateTimeOffset retrievedAtUtc)
    {
        var teams = new PageTeamRegistry(boxes);

        return boxes
            .Select(box => ToEvent(box, teams, page, fallbackYear, retrievedAtUtc))
            .OfType<SportEvent>()
            .ToList();
    }

    private static SportEvent? ToEvent(
        ParsoidTemplate box,
        PageTeamRegistry teams,
        WikipediaPageOptions page,
        int fallbackYear,
        DateTimeOffset retrievedAtUtc)
    {
        var team1 = teams.Resolve(Read(box, "team1"));
        var team2 = teams.Resolve(Read(box, "team2"));
        var participants = new[] { team1, team2 }.OfType<Participant>().ToList();

        if (participants.Count == 0)
        {
            return null;
        }

        var round = Read(box, "round") is { } roundText ? Translate(WikitextReader.ToPlainText(roundText)) : null;
        var date = FootballBoxFields.ParseDate(Read(box, "date"), box.HeadingYear ?? fallbackYear);

        return new SportEvent
        {
            Sport = page.Sport,
            Competition = page.Competition ?? round ?? box.Heading ?? page.Title.Replace('_', ' '),
            Stage = page.Competition is null ? null : box.Heading,
            Format = participants.Count == 2 ? EventFormat.Matchup : EventFormat.Participation,
            Scope = page.Scope,
            Schedule = ToSchedule(date, FootballBoxFields.ParseTime(Read(box, "time"))),
            Venue = Venue(box),
            Participants = participants,
            Sources =
            [
                new Domain.SourceReference
                {
                    Source = WikipediaClient.SourceName,
                    Url = WikipediaClient.BuildArticleUrl(page.Title),
                    License = WikipediaClient.License,
                    RetrievedAtUtc = retrievedAtUtc,
                    ExternalId = $"{page.Title}#{date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "sem-data"}:{string.Join("-x-", participants.Select(participant => participant.Name))}",
                },
            ],
            Confidence = Confidence.Medium,
        };
    }

    /// <summary>"Football box", "Football box collapsible" e os redirecionamentos sem espaço ("footballbox collapsible").</summary>
    public static bool IsFootballBox(string templateName) =>
        templateName.Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .StartsWith("footballbox", StringComparison.OrdinalIgnoreCase);

    private static Schedule ToSchedule(DateOnly? date, (TimeOnly Time, TimeSpan Offset)? time)
    {
        if (date is not { } day)
        {
            return Schedule.ToBeConfirmed();
        }

        if (time is not { } kickoff)
        {
            return Schedule.OnDate(day);
        }

        var start = new DateTimeOffset(day.ToDateTime(kickoff.Time), kickoff.Offset);

        return Schedule.AtTime(start, FormatOffset(kickoff.Offset));
    }

    private static string FormatOffset(TimeSpan offset) =>
        $"UTC{(offset < TimeSpan.Zero ? "-" : "+")}{offset.Duration():hh\\:mm}";

    private static string? Venue(ParsoidTemplate box)
    {
        var stadium = Read(box, "stadium") is { } value ? WikitextReader.ToPlainText(value) : null;
        var location = Read(box, "location") is { } place ? WikitextReader.ToPlainText(place) : null;
        var parts = new[] { stadium, location }.Where(part => !string.IsNullOrWhiteSpace(part)).ToList();

        return parts.Count == 0 ? null : string.Join(", ", parts);
    }

    private static string Translate(string round) =>
        round.Equals("Friendly", StringComparison.OrdinalIgnoreCase) ? "Amistoso internacional" : round;

    private static string? Read(ParsoidTemplate box, string name) =>
        box.Parameters.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;
}
