using System.Globalization;
using System.Text.RegularExpressions;

using BrasilCompete.Worker.Domain;
using BrasilCompete.Worker.Identity;
using BrasilCompete.Worker.Integrations.Wikipedia.Wikitext;
using BrasilCompete.Worker.Normalization;

namespace BrasilCompete.Worker.Integrations.Wikipedia.Football;

/// <summary>
/// Lê os campos das predefinições de partida (<c>Football box</c>) nos formatos mais comuns da Wikipedia em inglês.
/// </summary>
public static partial class FootballBoxFields
{
    private static readonly string[] Months =
    [
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December",
    ];

    /// <summary>
    /// Data em <c>{{Start date|2026|8|11}}</c>, <c>{{dts|...}}</c>, "11 August 2026", "August 11, 2026"
    /// ou "10 October" (ano vindo do título da seção).
    /// </summary>
    public static DateOnly? ParseDate(string? wikitext, int? contextYear)
    {
        if (string.IsNullOrWhiteSpace(wikitext))
        {
            return null;
        }

        foreach (var template in WikitextReader.ReadTemplates(wikitext))
        {
            var name = template.Name.ToLowerInvariant();

            if ((name is "start date" or "start date and age" or "dts" or "date")
                && TryNumbers(template, out var numeric))
            {
                return numeric;
            }

            if (name == "dts" && template.Argument(0) is { } text && ParseText(text, contextYear) is { } fromText)
            {
                return fromText;
            }
        }

        return ParseText(WikitextReader.ToPlainText(wikitext), contextYear);
    }

    /// <summary>
    /// Horário com offset em <c>{{UTZ|21:30|-3}}</c> ou "21:30 UTC−3". Sem offset, devolve <c>null</c>:
    /// o horário nunca é inventado.
    /// </summary>
    public static (TimeOnly Time, TimeSpan Offset)? ParseTime(string? wikitext)
    {
        if (string.IsNullOrWhiteSpace(wikitext))
        {
            return null;
        }

        foreach (var template in WikitextReader.ReadTemplates(wikitext))
        {
            if (template.Name.Equals("UTZ", StringComparison.OrdinalIgnoreCase)
                && TryTime(template.Argument(0), out var time)
                && TryOffset(template.Argument(1), out var offset))
            {
                return (time, offset);
            }
        }

        var match = TimeWithOffsetPattern().Match(WikitextReader.ToPlainText(wikitext).Replace('−', '-'));

        if (!match.Success || !TryTime($"{match.Groups["hour"].Value}:{match.Groups["minute"].Value}", out var plainTime))
        {
            return null;
        }

        var hours = match.Groups["offsetHours"].Success ? int.Parse(match.Groups["offsetHours"].Value, CultureInfo.InvariantCulture) : 0;
        var minutes = match.Groups["offsetMinutes"].Success ? int.Parse(match.Groups["offsetMinutes"].Value, CultureInfo.InvariantCulture) : 0;
        var sign = match.Groups["sign"].Value == "-" ? -1 : 1;

        return (plainTime, sign * new TimeSpan(hours, minutes, 0));
    }

    /// <summary>Indica um horário escrito sem fuso (que por isso não é usado).</summary>
    public static bool HasTimeWithoutOffset(string? wikitext) =>
        !string.IsNullOrWhiteSpace(wikitext) && ParseTime(wikitext) is null && TimeOnlyPattern().IsMatch(WikitextReader.ToPlainText(wikitext));

    /// <summary>
    /// Seleções (<c>{{fb|BRA}}</c>, <c>{{fbw-rt|BRA}}</c>, <c>{{fbu|BRA|20}}</c>) ou clubes
    /// (<c>[[Clube|Nome]] {{fbaicon|ARG}}</c>). Adversário ainda indefinido ("TBD", "Winner of...") devolve <c>null</c>.
    /// </summary>
    public static Participant? ParseTeam(string? wikitext)
    {
        if (string.IsNullOrWhiteSpace(wikitext))
        {
            return null;
        }

        var templates = WikitextReader.ReadTemplates(wikitext);

        foreach (var template in templates)
        {
            var match = NationalTeamTemplatePattern().Match(template.Name.Trim());

            if (match.Success && template.Argument(0) is { Length: > 0 } code)
            {
                return NationalTeam(code, match.Groups["women"].Success, match.Groups["youth"].Success ? template.Argument(1) : null);
            }
        }

        if (WikitextReader.ReadFirstLink(wikitext) is not { } link)
        {
            return null;
        }

        var country = templates
            .FirstOrDefault(template => IconTemplatePattern().IsMatch(template.Name.Trim()))
            ?.Argument(0);

        return new Participant
        {
            Name = link.Text,
            Kind = ParticipantKind.Club,
            Country = country?.ToUpperInvariant(),
            ExternalIds = new Dictionary<string, string> { [ExternalIdKeys.EnglishWikipedia] = link.Target.Replace('_', ' ') },
        };
    }

    private static Participant NationalTeam(string code, bool women, string? age)
    {
        var name = CountryNames.For(code);
        var suffix = (women ? " (feminino)" : string.Empty) + (age is { Length: > 0 } ? $" Sub-{age}" : string.Empty);

        return new Participant
        {
            Name = name + suffix,
            Kind = ParticipantKind.NationalTeam,
            Country = code.Trim().ToUpperInvariant(),
        };
    }

    private static bool TryNumbers(WikitextTemplate template, out DateOnly date)
    {
        date = default;
        var numbers = template.Arguments.Where(argument => !argument.Contains('=')).Take(3).ToList();

        return numbers.Count == 3
            && int.TryParse(numbers[0].Trim(), CultureInfo.InvariantCulture, out var year)
            && int.TryParse(numbers[1].Trim(), CultureInfo.InvariantCulture, out var month)
            && int.TryParse(numbers[2].Trim(), CultureInfo.InvariantCulture, out var day)
            && DateOnly.TryParseExact($"{year:0000}-{month:00}-{day:00}", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }

    private static DateOnly? ParseText(string text, int? contextYear)
    {
        var dayFirst = DayFirstPattern().Match(text);
        var monthFirst = MonthFirstPattern().Match(text);
        var match = dayFirst.Success ? dayFirst : monthFirst;

        if (!match.Success)
        {
            return null;
        }

        var month = Array.FindIndex(Months, name => name.Equals(match.Groups["month"].Value, StringComparison.OrdinalIgnoreCase)) + 1;
        var day = int.Parse(match.Groups["day"].Value, CultureInfo.InvariantCulture);
        int? year = match.Groups["year"].Success ? int.Parse(match.Groups["year"].Value, CultureInfo.InvariantCulture) : contextYear;

        return year is { } value && month > 0 && day <= DateTime.DaysInMonth(value, month)
            ? new DateOnly(value, month, day)
            : null;
    }

    private static bool TryTime(string? value, out TimeOnly time) =>
        TimeOnly.TryParseExact(value?.Trim(), ["H:mm", "HH:mm", "H.mm", "HH.mm"], CultureInfo.InvariantCulture, DateTimeStyles.None, out time);

    private static bool TryOffset(string? value, out TimeSpan offset)
    {
        offset = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim().Replace('−', '-').Replace("+", string.Empty, StringComparison.Ordinal);
        var negative = normalized.StartsWith('-');
        var parts = normalized.TrimStart('-').Split(':');

        if (!int.TryParse(parts[0], CultureInfo.InvariantCulture, out var hours))
        {
            return false;
        }

        var minutes = parts.Length > 1 && int.TryParse(parts[1], CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;
        offset = new TimeSpan(hours, minutes, 0) * (negative ? -1 : 1);

        return true;
    }

    [GeneratedRegex(@"(?<day>\d{1,2})\s+(?<month>January|February|March|April|May|June|July|August|September|October|November|December)(\s+(?<year>\d{4}))?", RegexOptions.IgnoreCase)]
    private static partial Regex DayFirstPattern();

    [GeneratedRegex(@"(?<month>January|February|March|April|May|June|July|August|September|October|November|December)\s+(?<day>\d{1,2}),?(\s+(?<year>\d{4}))?", RegexOptions.IgnoreCase)]
    private static partial Regex MonthFirstPattern();

    [GeneratedRegex(@"(?<hour>\d{1,2})[:.](?<minute>\d{2}).*?UTC\s*((?<sign>[+-])\s*(?<offsetHours>\d{1,2})(:(?<offsetMinutes>\d{2}))?)?")]
    private static partial Regex TimeWithOffsetPattern();

    [GeneratedRegex(@"\b\d{1,2}[:.]\d{2}\b")]
    private static partial Regex TimeOnlyPattern();

    [GeneratedRegex(@"^fb(?<women>w)?(?<youth>u)?(-rt)?$", RegexOptions.IgnoreCase)]
    private static partial Regex NationalTeamTemplatePattern();

    [GeneratedRegex(@"^(fbaicon|flagicon|flagdeco|fbicon)$", RegexOptions.IgnoreCase)]
    private static partial Regex IconTemplatePattern();
}
