using AudioSwitcher.Audio;
using AudioSwitcher.Interop;

using Spectre.Console;

namespace AudioSwitcher.Cli;

internal static class SetCommand
{
    public static int Run(string? playback, string? recording, bool communications, bool defaultOnly)
    {
        try
        {
            var hasPlayback = !string.IsNullOrWhiteSpace(playback);
            var hasRecording = !string.IsNullOrWhiteSpace(recording);

            if (!hasPlayback && !hasRecording)
            {
                AnsiConsole.MarkupLine("[red]Provide a device name with --playback <name> and/or --recording <name>.[/]");
                return 1;
            }

            var target = CliFormat.ResolveTarget(defaultOnly, communications);

            var exitCode = 0;
            if (hasPlayback)
                exitCode |= SetDefault(DataFlow.Render, playback!, target);
            if (hasRecording)
                exitCode |= SetDefault(DataFlow.Capture, recording!, target);

            return exitCode;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
            return 1;
        }
    }

    private static int SetDefault(DataFlow flow, string nameOrId, RoleTarget target)
    {
        var device = AudioDeviceService.FindDevice(flow, nameOrId);
        if (device is null)
        {
            AnsiConsole.MarkupLine($"[red]No {CliFormat.FlowLabel(flow).ToLowerInvariant()} device matched '[yellow]{Markup.Escape(nameOrId)}[/]'.[/]");
            return 1;
        }

        AudioDeviceService.SetDefault(device, target);
        AnsiConsole.MarkupLine($"[green]Switched {CliFormat.FlowLabel(flow).ToLowerInvariant()} {CliFormat.TargetLabel(target)} device to[/] [bold]{Markup.Escape(device.Name)}[/]");
        return 0;
    }
}
