using AudioSwitcher.Audio;
using AudioSwitcher.Cli;
using AudioSwitcher.Interop;

namespace AudioSwitcher.Tests;

public class InteractiveCommandTests
{
    private static AudioDevice Device(string name, bool isDefault = false, bool isComms = false) =>
        new(name, $"{{id-{name}}}", DataFlow.Render, isDefault, isComms);

    // ---- Wrap ---------------------------------------------------------------

    [Theory]
    [InlineData(0, 3, 0)]
    [InlineData(2, 3, 2)]
    [InlineData(3, 3, 0)]   // past the end wraps to start
    [InlineData(-1, 3, 2)]  // before the start wraps to end
    [InlineData(0, 1, 0)]   // single item (the sentinel-only column)
    public void Wrap_WrapsAroundBounds(int index, int count, int expected) =>
        Assert.Equal(expected, InteractiveCommand.Wrap(index, count));

    // ---- BuildItems ---------------------------------------------------------

    [Fact]
    public void BuildItems_PrependsNullSentinel()
    {
        var items = InteractiveCommand.BuildItems(new[] { Device("A"), Device("B") });

        Assert.Equal(3, items.Count);
        Assert.Null(items[0]);
        Assert.Equal("A", items[1]!.Name);
        Assert.Equal("B", items[2]!.Name);
    }

    [Fact]
    public void BuildItems_EmptyDeviceList_StillHasSentinel()
    {
        var items = InteractiveCommand.BuildItems(Array.Empty<AudioDevice>());

        Assert.Single(items);
        Assert.Null(items[0]);
    }

    // ---- InitialCursor ------------------------------------------------------

    [Fact]
    public void InitialCursor_PointsAtCurrentDefault_ForMultimediaTarget()
    {
        var items = InteractiveCommand.BuildItems(new[]
        {
            Device("A"),
            Device("B", isDefault: true),
        });

        Assert.Equal(2, InteractiveCommand.InitialCursor(items, RoleTarget.Multimedia));
    }

    [Fact]
    public void InitialCursor_PointsAtCurrentCommunications_ForCommunicationsTarget()
    {
        var items = InteractiveCommand.BuildItems(new[]
        {
            Device("A", isComms: true),
            Device("B", isDefault: true),
        });

        Assert.Equal(1, InteractiveCommand.InitialCursor(items, RoleTarget.Communications));
    }

    [Fact]
    public void InitialCursor_NoCurrentDefault_FallsBackToSentinel()
    {
        var items = InteractiveCommand.BuildItems(new[] { Device("A"), Device("B") });

        Assert.Equal(0, InteractiveCommand.InitialCursor(items, RoleTarget.Multimedia));
    }
}
