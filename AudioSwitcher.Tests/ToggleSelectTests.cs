using AudioSwitcher.Audio;
using AudioSwitcher.Interop;

namespace AudioSwitcher.Tests;

public class ToggleSelectTests
{
    private static AudioDevice Device(string name, string id) =>
        new(name, id, DataFlow.Render, IsDefault: false, IsDefaultCommunications: false);

    private static readonly AudioDevice A = Device("Speakers", "{id-a}");
    private static readonly AudioDevice B = Device("Headphones", "{id-b}");

    [Fact]
    public void CurrentIsA_FlipsToB()
    {
        var next = AudioDeviceService.SelectToggleTarget(A.Id, A, B);
        Assert.Equal(B.Id, next.Id);
    }

    [Fact]
    public void CurrentIsB_FlipsToA()
    {
        var next = AudioDeviceService.SelectToggleTarget(B.Id, A, B);
        Assert.Equal(A.Id, next.Id);
    }

    [Fact]
    public void CurrentIsNull_LandsOnA()
    {
        var next = AudioDeviceService.SelectToggleTarget(null, A, B);
        Assert.Equal(A.Id, next.Id);
    }

    [Fact]
    public void CurrentIsUnrelatedDevice_LandsOnA()
    {
        var next = AudioDeviceService.SelectToggleTarget("{id-other}", A, B);
        Assert.Equal(A.Id, next.Id);
    }

    [Fact]
    public void CurrentMatchesA_CaseInsensitively_FlipsToB()
    {
        var next = AudioDeviceService.SelectToggleTarget("{ID-A}", A, B);
        Assert.Equal(B.Id, next.Id);
    }
}
