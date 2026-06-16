using AudioSwitcher.Audio;
using AudioSwitcher.Interop;

namespace AudioSwitcher.Tests;

public class AudioDeviceMatchTests
{
    private static AudioDevice Device(string name, string id) =>
        new(name, id, DataFlow.Render, IsDefault: false, IsDefaultCommunications: false);

    [Fact]
    public void Match_ExactName_WinsOverEarlierSubstringMatch()
    {
        var devices = new[]
        {
            Device("ABC Headset", "{id-abc}"),
            Device("Headset", "{id-headset}"),
        };

        var match = AudioDeviceService.Match(devices, "Headset");

        Assert.NotNull(match);
        Assert.Equal("{id-headset}", match!.Id);
    }

    [Fact]
    public void Match_ExactId_IsMatchedCaseInsensitively()
    {
        var devices = new[] { Device("Speakers", "{ID-ABC}") };

        var match = AudioDeviceService.Match(devices, "{id-abc}");

        Assert.NotNull(match);
        Assert.Equal("Speakers", match!.Name);
    }

    [Fact]
    public void Match_ExactName_IsMatchedCaseInsensitively()
    {
        var devices = new[] { Device("Speakers", "{id}") };

        var match = AudioDeviceService.Match(devices, "speakers");

        Assert.NotNull(match);
        Assert.Equal("{id}", match!.Id);
    }

    [Fact]
    public void Match_SingleSubstring_IsReturned()
    {
        var devices = new[]
        {
            Device("Realtek Speakers", "{id-spk}"),
            Device("USB Microphone", "{id-mic}"),
        };

        var match = AudioDeviceService.Match(devices, "speak");

        Assert.NotNull(match);
        Assert.Equal("{id-spk}", match!.Id);
    }

    [Fact]
    public void Match_AmbiguousSubstring_ReturnsNull()
    {
        var devices = new[]
        {
            Device("Microphone (USB)", "{id-1}"),
            Device("Microphone Array", "{id-2}"),
        };

        Assert.Null(AudioDeviceService.Match(devices, "Microphone"));
    }

    [Fact]
    public void Match_NoMatch_ReturnsNull()
    {
        var devices = new[] { Device("Speakers", "{id}") };

        Assert.Null(AudioDeviceService.Match(devices, "Headphones"));
    }

    [Fact]
    public void Match_AmbiguousSubstring_StillResolvesAnExactNameMatch()
    {
        var devices = new[]
        {
            Device("Headset", "{id-exact}"),
            Device("Headset Adapter", "{id-other}"),
        };

        var match = AudioDeviceService.Match(devices, "Headset");

        Assert.NotNull(match);
        Assert.Equal("{id-exact}", match!.Id);
    }
}
