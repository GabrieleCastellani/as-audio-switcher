using AudioSwitcher.Audio;
using AudioSwitcher.Cli;
using AudioSwitcher.Interop;

namespace AudioSwitcher.Tests;

public class CliFormatTests
{
    // ---- FlowLabel ----------------------------------------------------------

    [Fact]
    public void FlowLabel_Render_IsPlayback() =>
        Assert.Equal("Playback", CliFormat.FlowLabel(DataFlow.Render));

    [Fact]
    public void FlowLabel_Capture_IsRecording() =>
        Assert.Equal("Recording", CliFormat.FlowLabel(DataFlow.Capture));

    [Fact]
    public void FlowLabel_All_IsUnknown() =>
        Assert.Equal("Unknown", CliFormat.FlowLabel(DataFlow.All));

    // ---- FlowIcon -----------------------------------------------------------

    [Fact]
    public void FlowIcon_Render_IsSpeaker() =>
        Assert.Equal("🔊", CliFormat.FlowIcon(DataFlow.Render));

    [Fact]
    public void FlowIcon_Capture_IsMicrophone() =>
        Assert.Equal("🎙", CliFormat.FlowIcon(DataFlow.Capture));

    [Fact]
    public void FlowIcon_All_IsMutedSpeaker() =>
        Assert.Equal("🔈", CliFormat.FlowIcon(DataFlow.All));

    // ---- TargetLabel --------------------------------------------------------

    [Fact]
    public void TargetLabel_Multimedia_IsDefault() =>
        Assert.Equal("default", CliFormat.TargetLabel(RoleTarget.Multimedia));

    [Fact]
    public void TargetLabel_Communications_IsCommunications() =>
        Assert.Equal("communications", CliFormat.TargetLabel(RoleTarget.Communications));

    [Fact]
    public void TargetLabel_All_IsDefaultAndCommunications() =>
        Assert.Equal("default and communications", CliFormat.TargetLabel(RoleTarget.All));

    // ---- TargetFlag ---------------------------------------------------------

    [Fact]
    public void TargetFlag_Multimedia_IsDefaultFlag() =>
        Assert.Equal("--default", CliFormat.TargetFlag(RoleTarget.Multimedia));

    [Fact]
    public void TargetFlag_Communications_IsCommunicationsFlag() =>
        Assert.Equal("--communications", CliFormat.TargetFlag(RoleTarget.Communications));

    [Fact]
    public void TargetFlag_All_IsNull_BecauseEveryRoleIsTheDefault()
    {
        Assert.Null(CliFormat.TargetFlag(RoleTarget.All));
    }

    // ---- BuildSetCommandLine ------------------------------------------------

    [Fact]
    public void BuildSetCommandLine_PlaybackOnly()
    {
        var line = CliFormat.BuildSetCommandLine("Headphones", null, RoleTarget.All);
        Assert.Equal(@".\as.exe set --playback Headphones", line);
    }

    [Fact]
    public void BuildSetCommandLine_RecordingOnly()
    {
        var line = CliFormat.BuildSetCommandLine(null, "Microphone", RoleTarget.All);
        Assert.Equal(@".\as.exe set --recording Microphone", line);
    }

    [Fact]
    public void BuildSetCommandLine_BothDevices_KeepsPlaybackBeforeRecording()
    {
        var line = CliFormat.BuildSetCommandLine("Headphones", "Microphone", RoleTarget.All);
        Assert.Equal(@".\as.exe set --playback Headphones --recording Microphone", line);
    }

    [Fact]
    public void BuildSetCommandLine_NamesWithSpaces_AreQuoted()
    {
        var line = CliFormat.BuildSetCommandLine("Headset Earphone", "Headset Microphone", RoleTarget.All);
        Assert.Equal(@".\as.exe set --playback ""Headset Earphone"" --recording ""Headset Microphone""", line);
    }

    [Fact]
    public void BuildSetCommandLine_NameWithoutSpaces_IsNotQuoted()
    {
        var line = CliFormat.BuildSetCommandLine("Speakers", null, RoleTarget.All);
        Assert.DoesNotContain("\"", line);
    }

    [Fact]
    public void BuildSetCommandLine_Communications_AppendsFlag()
    {
        var line = CliFormat.BuildSetCommandLine("Headset", null, RoleTarget.Communications);
        Assert.Equal(@".\as.exe set --playback Headset --communications", line);
    }

    [Fact]
    public void BuildSetCommandLine_Multimedia_AppendsDefaultFlag()
    {
        var line = CliFormat.BuildSetCommandLine("Speakers", null, RoleTarget.Multimedia);
        Assert.Equal(@".\as.exe set --playback Speakers --default", line);
    }

    [Fact]
    public void BuildSetCommandLine_All_OmitsRoleFlag()
    {
        var line = CliFormat.BuildSetCommandLine("Speakers", null, RoleTarget.All);
        Assert.DoesNotContain("--default", line);
        Assert.DoesNotContain("--communications", line);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BuildSetCommandLine_BlankNames_AreOmitted(string? blank)
    {
        var line = CliFormat.BuildSetCommandLine(blank, blank, RoleTarget.All);
        Assert.Equal(@".\as.exe set", line);
    }

    [Fact]
    public void BuildSetCommandLine_StartsWithExecutablePrefix()
    {
        var line = CliFormat.BuildSetCommandLine("Headphones", null, RoleTarget.All);
        Assert.StartsWith(CliFormat.Executable + " set", line);
    }

    // ---- ResolveTarget ------------------------------------------------------

    [Fact]
    public void ResolveTarget_DefaultOnly_IsMultimedia() =>
        Assert.Equal(RoleTarget.Multimedia, CliFormat.ResolveTarget(defaultOnly: true, communications: false));

    [Fact]
    public void ResolveTarget_CommunicationsOnly_IsCommunications() =>
        Assert.Equal(RoleTarget.Communications, CliFormat.ResolveTarget(defaultOnly: false, communications: true));

    [Fact]
    public void ResolveTarget_NeitherFlag_IsAll() =>
        Assert.Equal(RoleTarget.All, CliFormat.ResolveTarget(defaultOnly: false, communications: false));

    [Fact]
    public void ResolveTarget_BothFlags_IsAll() =>
        Assert.Equal(RoleTarget.All, CliFormat.ResolveTarget(defaultOnly: true, communications: true));
}
