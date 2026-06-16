using AudioSwitcher.Audio;
using AudioSwitcher.Interop;

namespace AudioSwitcher.Cli;

/// <summary>Presentation helpers: labels, icons and the copy-pasteable command line.</summary>
internal static class CliFormat
{
    public const string Executable = @".\as.exe";

    public static string FlowLabel(DataFlow flow) => flow switch
    {
        DataFlow.Render => "Playback",
        DataFlow.Capture => "Recording",
        _ => "Unknown"
    };

    public static string FlowIcon(DataFlow flow) => flow switch
    {
        DataFlow.Render => "🔊",
        DataFlow.Capture => "🎙",
        _ => "🔈"
    };

    public static string TargetLabel(RoleTarget target) => target switch
    {
        RoleTarget.Multimedia => "default",
        RoleTarget.Communications => "communications",
        _ => "default and communications"
    };

    public static string? TargetFlag(RoleTarget target) => target switch
    {
        RoleTarget.Multimedia => "--default",
        RoleTarget.Communications => "--communications",
        _ => null
    };

    /// <summary>
    /// Maps the <c>--default</c> / <c>--communications</c> flags to a role slot.
    /// Each flag selects a single slot; specifying both (or neither) targets
    /// every role.
    /// </summary>
    public static RoleTarget ResolveTarget(bool defaultOnly, bool communications)
    {
        if (defaultOnly && !communications)
            return RoleTarget.Multimedia;
        if (communications && !defaultOnly)
            return RoleTarget.Communications;
        return RoleTarget.All;
    }

    /// <summary>
    /// Builds a copy-pasteable <c>as set</c> command line for the given selection.
    /// Either device name may be null to omit that flow.
    /// </summary>
    public static string BuildSetCommandLine(
        string? playbackName,
        string? recordingName,
        RoleTarget target)
    {
        var parts = new List<string> { Executable, "set" };

        if (!string.IsNullOrWhiteSpace(playbackName))
        {
            parts.Add("--playback");
            parts.Add(Quote(playbackName));
        }

        if (!string.IsNullOrWhiteSpace(recordingName))
        {
            parts.Add("--recording");
            parts.Add(Quote(recordingName));
        }

        if (TargetFlag(target) is { } flag)
            parts.Add(flag);

        return string.Join(' ', parts);
    }

    private static string Quote(string value)
    {
        var needsQuoting = value.Contains(' ') || value.Contains('"');
        if (!needsQuoting)
            return value;

        return $"\"{value.Replace("\"", "\\\"")}\"";
    }

    /// <summary>
    /// Returns the message of the deepest inner exception. Core Audio calls surface
    /// as wrapped <see cref="System.Runtime.InteropServices.COMException"/>s whose
    /// outer message is generic, so the innermost message is the most informative.
    /// </summary>
    public static string InnermostMessage(Exception ex) =>
        ex.InnerException is { } inner ? InnermostMessage(inner) : ex.Message;
}
