using BrasilCompete.Worker.Commands;

namespace BrasilCompete.Worker.Tests.Commands;

public sealed class CollectCommandOptionsTests
{
    private static readonly DateOnly Today = new(2026, 10, 9);

    [Fact]
    public void Parse_ReadsTheWindowSourcesAndCacheFlag()
    {
        var options = CollectCommandOptions.Parse(
            ["--from", "2026-09-01", "--to", "2026-09-30", "--sources", "jolpica, lichess", "--no-cache"],
            Today);

        Assert.Equal(new DateOnly(2026, 9, 1), options.Window.From);
        Assert.Equal(new DateOnly(2026, 9, 30), options.Window.To);
        Assert.True(options.Sources!.SetEquals(["jolpica", "lichess"]));
        Assert.True(options.NoCache);
    }

    [Fact]
    public void Parse_DaysStartsToday()
    {
        var options = CollectCommandOptions.Parse(["--days", "14"], Today);

        Assert.Equal(Today, options.Window.From);
        Assert.Equal(new DateOnly(2026, 10, 22), options.Window.To);
    }

    [Theory]
    [InlineData("--from", "2026-09-01")]
    [InlineData("--from", "2026-09-30", "--to", "2026-09-01")]
    [InlineData("--from", "01/09/2026", "--to", "2026-09-30")]
    [InlineData("--unknown")]
    public void Parse_InvalidArguments_Throw(params string[] args) =>
        Assert.Throws<CommandLineException>(() => CollectCommandOptions.Parse(args, Today));
}
