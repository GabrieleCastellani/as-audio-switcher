using AudioSwitcher.Audio;
using AudioSwitcher.Interop;

using Spectre.Console;

namespace AudioSwitcher.Cli;

/// <summary>
/// Flips a data flow's default device between two configured endpoints. Works
/// like <see cref="SetCommand"/> but takes two devices per flow and switches to
/// whichever one is not currently the default.
/// </summary>
internal static class ToggleCommand
{
    public static int Run(string[]? playback, string[]? recording, bool communications, bool defaultOnly)
    {
        try
        {
            var hasPlayback = playback is { Length: 2 };
            var hasRecording = recording is { Length: 2 };

            if (!hasPlayback && !hasRecording)
            {
                AnsiConsole.MarkupLine("[red]Provide two device names with --playback <a> <b> and/or --recording <a> <b>.[/]");
                return 1;
            }

            var target = CliFormat.ResolveTarget(defaultOnly, communications);

            var exitCode = 0;
            if (hasPlayback)
                exitCode |= ToggleFlow(DataFlow.Render, playback![0], playback[1], target);
            if (hasRecording)
                exitCode |= ToggleFlow(DataFlow.Capture, recording![0], recording[1], target);

            return exitCode;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(CliFormat.InnermostMessage(ex))}");
            return 1;
        }
    }

    private static int ToggleFlow(DataFlow flow, string nameA, string nameB, RoleTarget target)
    {
        var flowLabel = CliFormat.FlowLabel(flow).ToLowerInvariant();
        var devices = AudioDeviceService.GetDevices(flow);

        var deviceA = AudioDeviceService.Match(devices, nameA);
        if (deviceA is null)
            return NotMatched(flowLabel, nameA);

        var deviceB = AudioDeviceService.Match(devices, nameB);
        if (deviceB is null)
            return NotMatched(flowLabel, nameB);

        if (string.Equals(deviceA.Id, deviceB.Id, StringComparison.OrdinalIgnoreCase))
        {
            AnsiConsole.MarkupLine(
                $"[red]Both {flowLabel} names resolve to the same device ([yellow]{Markup.Escape(deviceA.Name)}[/]); nothing to toggle between.[/]");
            return 1;
        }

        var current = CurrentDefault(devices, target);
        var next = AudioDeviceService.SelectToggleTarget(current?.Id, deviceA, deviceB);

        AudioDeviceService.SetDefault(next, target);

        var fromLabel = current?.Name ?? "none";
        AnsiConsole.MarkupLine(
            $"[green]Toggled {flowLabel} {CliFormat.TargetLabel(target)} device from[/] " +
            $"[bold]{Markup.Escape(fromLabel)}[/] [green]to[/] [bold]{Markup.Escape(next.Name)}[/]");
        return 0;
    }

    /// <summary>The device currently occupying the slot the toggle decision keys off of.</summary>
    private static AudioDevice? CurrentDefault(IReadOnlyList<AudioDevice> devices, RoleTarget target) =>
        target == RoleTarget.Communications
            ? devices.FirstOrDefault(device => device.IsDefaultCommunications)
            : devices.FirstOrDefault(device => device.IsDefault);

    private static int NotMatched(string flowLabel, string nameOrId)
    {
        AnsiConsole.MarkupLine($"[red]No {flowLabel} device matched '[yellow]{Markup.Escape(nameOrId)}[/]'.[/]");
        return 1;
    }
}
