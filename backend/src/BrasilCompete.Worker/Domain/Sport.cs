using System.Text.Json.Serialization;

namespace BrasilCompete.Worker.Domain;

public enum Sport
{
    [JsonStringEnumMemberName("football")]
    Football,

    [JsonStringEnumMemberName("formula-1")]
    Formula1,

    [JsonStringEnumMemberName("chess")]
    Chess,

    [JsonStringEnumMemberName("mma")]
    Mma,

    [JsonStringEnumMemberName("esports-valorant")]
    EsportsValorant,

    [JsonStringEnumMemberName("esports-cs2")]
    EsportsCounterStrike,

    [JsonStringEnumMemberName("esports-lol")]
    EsportsLeagueOfLegends,

    [JsonStringEnumMemberName("basketball")]
    Basketball,

    [JsonStringEnumMemberName("volleyball")]
    Volleyball,

    [JsonStringEnumMemberName("beach-volleyball")]
    BeachVolleyball,

    [JsonStringEnumMemberName("tennis")]
    Tennis,

    [JsonStringEnumMemberName("table-tennis")]
    TableTennis,
}
