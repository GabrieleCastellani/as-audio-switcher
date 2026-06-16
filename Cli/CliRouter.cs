using System.Reflection;

using Spectre.Console;

namespace AudioSwitcher.Cli;

/// <summary>Parses the command line and dispatches to the matching command.</summary>
internal static class CliRouter
{
    // Sourced from the assembly version (driven by the csproj <Version>) so the
    // reported version always matches the released build.
    internal static readonly string Version =
        (typeof(CliRouter).Assembly.GetName().Version ?? new Version(0, 0, 0)).ToString(3);

    public static int Run(string[] args)
    {
        // No arguments → interactive mode.
        if (args.Length == 0)
            return InteractiveCommand.Run();

        var command = args[0].ToLowerInvariant();
        var rest = args.Skip(1).ToArray();

        return command switch
        {
            "list" => RunList(rest),
            "set" => RunSet(rest),
            "interactive" or "i" => InteractiveCommand.Run(),
            "-h" or "--help" or "help" => PrintHelp(),
            "-v" or "--version" or "version" => PrintVersion(),
            _ => UnknownCommand(command),
        };
    }

    private static int RunList(string[] args)
    {
        var playback = false;
        var recording = false;
        foreach (var arg in args)
        {
            switch (arg.ToLowerInvariant())
            {
                case "--playback":
                    playback = true;
                    break;
                case "--recording":
                    recording = true;
                    break;
                case "-h" or "--help":
                    return PrintHelp();
                default:
                    return UnknownOption("list", arg);
            }
        }

        return ListCommand.Run(playback, recording);
    }

    private static int RunSet(string[] args)
    {
        string? playback = null;
        string? recording = null;
        var communications = false;
        var defaultOnly = false;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "--playback":
                    if (++i >= args.Length)
                        return MissingValue("--playback");
                    playback = args[i];
                    break;
                case "--recording":
                    if (++i >= args.Length)
                        return MissingValue("--recording");
                    recording = args[i];
                    break;
                case "--communications" or "--comms":
                    communications = true;
                    break;
                case "--default":
                    defaultOnly = true;
                    break;
                case "-h" or "--help":
                    return PrintHelp();
                default:
                    return UnknownOption("set", args[i]);
            }
        }

        return SetCommand.Run(playback, recording, communications, defaultOnly);
    }

    private static int PrintVersion()
    {
        AnsiConsole.WriteLine($"as {Version}");
        return 0;
    }

    private static int UnknownCommand(string command)
    {
        AnsiConsole.MarkupLine($"[red]Unknown command '[yellow]{Markup.Escape(command)}[/]'.[/]");
        PrintHelp();
        return 1;
    }

    private static int UnknownOption(string command, string option)
    {
        AnsiConsole.MarkupLine($"[red]Unknown option '[yellow]{Markup.Escape(option)}[/]' for '{command}'.[/]");
        return 1;
    }

    private static int MissingValue(string option)
    {
        AnsiConsole.MarkupLine($"[red]Option '[yellow]{option}[/]' requires a device name.[/]");
        return 1;
    }

    private static int PrintHelp()
    {
        AnsiConsole.MarkupLine("[bold cyan]as[/] — AudioSwitcher CLI");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]USAGE[/]");
        AnsiConsole.MarkupLine("  as                                 Launch interactive mode");
        AnsiConsole.MarkupLine("  as i | interactive                 Launch interactive mode");
        AnsiConsole.MarkupLine("  as list [--playback] [--recording] List devices");
        AnsiConsole.MarkupLine("  as set [options]                   Set default device(s)");
        AnsiConsole.MarkupLine("  as --help | --version              Show help or version");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]SET OPTIONS[/]");
        AnsiConsole.MarkupLine("  --playback <name>     Playback (output) device to set");
        AnsiConsole.MarkupLine("  --recording <name>    Recording (input) device to set");
        AnsiConsole.MarkupLine("  --communications      Set only the default communication device");
        AnsiConsole.MarkupLine("  --comms               Alias of --communications");
        AnsiConsole.MarkupLine("  --default             Set only the multimedia default");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]EXAMPLES[/]");
        AnsiConsole.MarkupLine("  [grey]as set --playback \"Headphones\"[/]");
        AnsiConsole.MarkupLine("  [grey]as set --playback \"Headset\" --recording \"Headset Mic\" --communications[/]");
        return 0;
    }
}
