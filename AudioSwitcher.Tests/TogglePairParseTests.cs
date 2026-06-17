using AudioSwitcher.Cli;

namespace AudioSwitcher.Tests;

public class TogglePairParseTests
{
    [Fact]
    public void TryReadPair_ReadsTwoValues_AndAdvancesIndex()
    {
        var args = new[] { "--playback", "Speakers", "Headphones" };
        var index = 0;

        var ok = CliRouter.TryReadPair(args, ref index, out var values);

        Assert.True(ok);
        Assert.Equal(new[] { "Speakers", "Headphones" }, values);
        Assert.Equal(2, index); // positioned on the second value
    }

    [Fact]
    public void TryReadPair_StopsBeforeNextFlag()
    {
        var args = new[] { "--playback", "Speakers", "--recording", "Mic" };
        var index = 0;

        var ok = CliRouter.TryReadPair(args, ref index, out var values);

        Assert.False(ok);
        Assert.Empty(values);
    }

    [Fact]
    public void TryReadPair_FailsWhenOnlyOneValueRemains()
    {
        var args = new[] { "--playback", "Speakers" };
        var index = 0;

        var ok = CliRouter.TryReadPair(args, ref index, out var values);

        Assert.False(ok);
        Assert.Empty(values);
    }

    [Fact]
    public void TryReadPair_FailsWhenNoValuesRemain()
    {
        var args = new[] { "--playback" };
        var index = 0;

        var ok = CliRouter.TryReadPair(args, ref index, out var values);

        Assert.False(ok);
        Assert.Empty(values);
    }
}
