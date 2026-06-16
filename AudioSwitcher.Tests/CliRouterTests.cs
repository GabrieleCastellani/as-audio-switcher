using AudioSwitcher.Cli;

namespace AudioSwitcher.Tests;

public class CliRouterTests
{
    [Fact]
    public void Version_MatchesAssemblyVersion_NotAHardcodedConstant()
    {
        // Derived from the assembly (which the csproj <Version> feeds), so the
        // displayed version can't silently drift from the released build.
        var expected = typeof(CliRouter).Assembly.GetName().Version!.ToString(3);
        Assert.Equal(expected, CliRouter.Version);
    }
}
