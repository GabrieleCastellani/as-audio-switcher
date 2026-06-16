using AudioSwitcher.Audio;
using AudioSwitcher.Interop;

using Spectre.Console;

namespace AudioSwitcher.Cli;

internal static class ListCommand
{
    public static int Run(bool playback, bool recording)
    {
        try
        {
            var flows = new List<DataFlow>();
            if (playback || (!playback && !recording))
                flows.Add(DataFlow.Render);
            if (recording || (!playback && !recording))
                flows.Add(DataFlow.Capture);

            foreach (var flow in flows)
            {
                var devices = AudioDeviceService.GetDevices(flow);
                if (devices.Count == 0)
                {
                    AnsiConsole.MarkupLine($"[yellow]{CliFormat.FlowIcon(flow)} No active {CliFormat.FlowLabel(flow).ToLowerInvariant()} devices found.[/]");
                    continue;
                }

                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .AddColumn("#")
                    .AddColumn("Device")
                    .AddColumn("Type")
                    .AddColumn("Default")
                    .AddColumn("Comms")
                    .AddColumn("ID");

                for (var i = 0; i < devices.Count; i++)
                {
                    var device = devices[i];
                    table.AddRow(
                        (i + 1).ToString(),
                        Markup.Escape(device.Name),
                        CliFormat.FlowLabel(flow),
                        device.IsDefault ? "[green]●[/]" : "",
                        device.IsDefaultCommunications ? "[blue]●[/]" : "",
                        Markup.Escape(device.Id));
                }

                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"[bold {(flow == DataFlow.Render ? "cyan" : "magenta")}] {CliFormat.FlowIcon(flow)} {CliFormat.FlowLabel(flow)} devices[/]");
                AnsiConsole.Write(table);
            }

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(CliFormat.InnermostMessage(ex))}");
            return 1;
        }
    }
}
