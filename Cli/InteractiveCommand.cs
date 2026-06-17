using AudioSwitcher.Audio;
using AudioSwitcher.Interop;

using Spectre.Console;

namespace AudioSwitcher.Cli;

internal static class InteractiveCommand
{
    public static int Run()
    {
        if (Console.IsInputRedirected || Console.IsOutputRedirected)
        {
            AnsiConsole.MarkupLine("[red]Interactive mode requires an interactive terminal.[/]");
            AnsiConsole.MarkupLine($"[grey]Use[/] [yellow]{CliFormat.Executable} set --playback <name> --recording <name>[/] [grey]instead.[/]");
            return 1;
        }

        try
        {
            while (true)
            {
                var target = PromptRoleTarget();
                if (target is null)
                    return 0;

                // Enter inside the selector applies and exits; Esc returns here.
                if (RunSelector(target.Value))
                    return 0;
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(CliFormat.InnermostMessage(ex))}");
            return 1;
        }
    }

    private static RoleTarget? PromptRoleTarget()
    {
        const string defaultChoice = "Default device";
        const string commsChoice = "Communication device";
        const string bothChoice = "Both (default + communication)";
        const string exitChoice = "Exit";

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold cyan]AudioSwitcher[/] — which default do you want to change?")
                .PageSize(5)
                .AddChoices(defaultChoice, commsChoice, bothChoice, exitChoice));

        return choice switch
        {
            defaultChoice => RoleTarget.Multimedia,
            commsChoice => RoleTarget.Communications,
            bothChoice => RoleTarget.All,
            _ => null
        };
    }

    private static bool RunSelector(RoleTarget target)
    {
        var playback = AudioDeviceService.GetDevices(DataFlow.Render);
        var recording = AudioDeviceService.GetDevices(DataFlow.Capture);

        // Each column is the device list preceded by a "leave unchanged" sentinel (null).
        var playbackItems = BuildItems(playback);
        var recordingItems = BuildItems(recording);

        var playbackCursor = InitialCursor(playbackItems, target);
        var recordingCursor = InitialCursor(recordingItems, target);
        var activeColumn = 0; // 0 = playback, 1 = recording

        while (true)
        {
            Render(target, playbackItems, recordingItems, playbackCursor, recordingCursor, activeColumn);

            var key = Console.ReadKey(intercept: true).Key;
            switch (key)
            {
                case ConsoleKey.UpArrow:
                    if (activeColumn == 0)
                        playbackCursor = Wrap(playbackCursor - 1, playbackItems.Count);
                    else
                        recordingCursor = Wrap(recordingCursor - 1, recordingItems.Count);
                    break;

                case ConsoleKey.DownArrow:
                    if (activeColumn == 0)
                        playbackCursor = Wrap(playbackCursor + 1, playbackItems.Count);
                    else
                        recordingCursor = Wrap(recordingCursor + 1, recordingItems.Count);
                    break;

                case ConsoleKey.LeftArrow:
                    activeColumn = 0;
                    break;

                case ConsoleKey.RightArrow:
                    activeColumn = 1;
                    break;

                case ConsoleKey.Tab:
                    activeColumn = activeColumn == 0 ? 1 : 0;
                    break;

                case ConsoleKey.Enter:
                    // Ignore Enter until at least one device is chosen.
                    if (playbackItems[playbackCursor] is null && recordingItems[recordingCursor] is null)
                        break;
                    Apply(target, playbackItems[playbackCursor], recordingItems[recordingCursor]);
                    return true;

                case ConsoleKey.Escape:
                    return false;
            }
        }
    }

    internal static IReadOnlyList<AudioDevice?> BuildItems(IReadOnlyList<AudioDevice> devices)
    {
        var items = new List<AudioDevice?> { null };
        items.AddRange(devices);
        return items;
    }

    internal static int InitialCursor(IReadOnlyList<AudioDevice?> items, RoleTarget target)
    {
        for (var i = 1; i < items.Count; i++)
        {
            var device = items[i]!;
            var isCurrent = target == RoleTarget.Communications
                ? device.IsDefaultCommunications
                : device.IsDefault;
            if (isCurrent)
                return i;
        }

        return 0;
    }

    internal static int Wrap(int index, int count) => (index % count + count) % count;

    private static void Render(
        RoleTarget target,
        IReadOnlyList<AudioDevice?> playbackItems,
        IReadOnlyList<AudioDevice?> recordingItems,
        int playbackCursor,
        int recordingCursor,
        int activeColumn)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold cyan]AudioSwitcher — interactive[/]").LeftJustified());
        AnsiConsole.MarkupLine($"Changing: [bold]{CliFormat.TargetLabel(target)}[/] device(s)");
        AnsiConsole.WriteLine();

        var columnRows = Math.Max(playbackItems.Count, recordingItems.Count);
        var playbackPanel = BuildColumnPanel("🔊 Playback", playbackItems, playbackCursor, active: activeColumn == 0, target, columnRows);
        var recordingPanel = BuildColumnPanel("🎙 Recording", recordingItems, recordingCursor, active: activeColumn == 1, target, columnRows);
        AnsiConsole.Write(new Columns(playbackPanel, recordingPanel).Collapse());

        var command = CliFormat.BuildSetCommandLine(
            playbackItems[playbackCursor]?.Name,
            recordingItems[recordingCursor]?.Name,
            target);

        var nothingSelected = playbackItems[playbackCursor] is null && recordingItems[recordingCursor] is null;
        var commandMarkup = nothingSelected
            ? "[grey](choose at least one device)[/]"
            : $"[yellow]{Markup.Escape(command)}[/]";

        AnsiConsole.Write(new Panel(commandMarkup)
            .Header("[bold]Command (copy into your script)[/]")
            .BorderColor(Color.Grey)
            .Expand());

        AnsiConsole.MarkupLine("[grey]↑/↓ move • ←/→ or Tab switch column • Enter apply & exit • Esc back[/]");
    }

    private static Panel BuildColumnPanel(
        string header,
        IReadOnlyList<AudioDevice?> items,
        int cursor,
        bool active,
        RoleTarget target,
        int minRows)
    {
        var lines = new List<string>();
        for (var i = 0; i < items.Count; i++)
        {
            var device = items[i];
            var label = device is null ? "— leave unchanged —" : Markup.Escape(device.Name);

            if (device is not null)
            {
                if (target == RoleTarget.Communications)
                {
                    if (device.IsDefaultCommunications) label += " [blue]●[/]";
                }
                else if (device.IsDefault)
                {
                    label += " [green]●[/]";
                }
            }

            if (i == cursor)
            {
                label = active
                    ? $"[black on cyan]› {label}[/]"
                    : $"[cyan]› {label}[/]";
            }
            else
            {
                label = $"  {label}";
            }

            lines.Add(label);
        }

        while (lines.Count < minRows)
            lines.Add("  ");

        return new Panel(string.Join('\n', lines))
            .Header(active ? $"[bold cyan]{header} ◄[/]" : $"[bold]{header}[/]")
            .BorderColor(active ? Color.Cyan1 : Color.Grey)
            .Expand();
    }

    private static void Apply(RoleTarget target, AudioDevice? playback, AudioDevice? recording)
    {
        AnsiConsole.Clear();

        if (playback is not null)
        {
            AudioDeviceService.SetDefault(playback, target);
            AnsiConsole.MarkupLine($"[green]✔ Playback {CliFormat.TargetLabel(target)} →[/] [bold]{Markup.Escape(playback.Name)}[/]");
        }

        if (recording is not null)
        {
            AudioDeviceService.SetDefault(recording, target);
            AnsiConsole.MarkupLine($"[green]✔ Recording {CliFormat.TargetLabel(target)} →[/] [bold]{Markup.Escape(recording.Name)}[/]");
        }

        var command = CliFormat.BuildSetCommandLine(playback?.Name, recording?.Name, target);
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Panel($"[yellow]{Markup.Escape(command)}[/]")
            .Header("[bold]Command (copy into your script)[/]")
            .BorderColor(Color.Grey)
            .Expand());
        AnsiConsole.WriteLine();
    }
}
