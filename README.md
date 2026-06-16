# AudioSwitcher (as.exe)

A small, Windows-first CLI for listing and switching playback and recording devices with Spectre.Console.

## Features

- `as list` — list playback and/or recording devices, showing both the **Default** (`●`) and **Communications** (`●`) defaults
- `as set --playback <name>` — switch the default playback device
- `as set --recording <name>` — switch the default recording device
- `as set --playback <name> --recording <name>` — switch both at once
- `as set --playback <name> --communications` — switch only the *default communication device* (used by voice/calling apps)
- `as set --playback <name> --default` — switch only the multimedia default (leave communications untouched)
- `as interactive` (alias `as i`, or just run `.\as.exe` with no arguments) — guided two-column picker that builds the equivalent command line in real time

`--playback` and `--recording` can be used together to set the output and input
devices in a single command. The role flags (`--communications` / `--default`,
or neither for both) apply to whichever devices you set.

Windows tracks two independent default device slots per data flow: the regular
multimedia **Default Device** (Console + Multimedia roles) and the **Default
Communication Device** (Communications role). Use `--communications` / `--comms`
to target the communications slot, `--default` to target the multimedia slot,
or omit both to set every role at once.

## Commands

### `as list`

| Option         | Description                          |
| -------------- | ------------------------------------ |
| `--playback`   | List only playback (output) devices  |
| `--recording`  | List only recording (input) devices  |

With no option, both playback and recording devices are listed. Each table marks
the current **Default** device and the **Comms** (communications) default.

### `as set`

| Option                   | Description                                                          |
| ------------------------ | ------------------------------------------------------------------- |
| `--playback <name>`      | Name or ID of the playback (output) device to set                   |
| `--recording <name>`     | Name or ID of the recording (input) device to set                   |
| `--communications`, `--comms` | Set only the default communication device                      |
| `--default`              | Set only the multimedia default (Console + Multimedia roles)        |

Provide `--playback`, `--recording`, or both. Device matching is case-insensitive
and accepts a full name, a partial name, or the device ID. If neither
`--communications` nor `--default` is given, every role is set at once.

### `as interactive` (`as i`)

Running `.\as.exe` with **no arguments** launches this mode as well.

1. **Pick what to change** — a menu offers *Default device*, *Communication device*, or *Both (default + communication)*.
2. **Pick the devices** — a two-column screen shows **Playback** on the left and **Recording** on the right. Move with `↑/↓`, switch columns with `←/→` or `Tab`, and choose *— leave unchanged —* in a column to skip it.
3. **Copy the command** — a panel at the bottom composes the exact command in real time, prefixed with `.\as.exe`, e.g.
   `​.\as.exe set --playback "Headset Earphone" --recording "Headset Microphone" --communications`
   so you can paste it straight into a script.
4. Press `Enter` to apply (and print the final command) or `Esc` to go back.

## Project structure

The code is organised by responsibility:

| Folder      | Contents                                                                                  |
| ----------- | ----------------------------------------------------------------------------------------- |
| `Interop/`  | Native Core Audio COM glue: `NativeEnums`, `NativeTypes`, `ComInterfaces`, `CoreAudio`.    |
| `Audio/`    | Device model and logic: `AudioDevice`, `RoleTarget`, `AudioDeviceService`.                 |
| `Cli/`      | Command-line layer: `CliRouter`, `CliFormat`, and the `List`/`Set`/`Interactive` commands. |
| `Program.cs`| Entry point — hands the arguments to `CliRouter`.                                          |

## Tests

Unit tests live in `../AudioSwitcher.Tests` (xUnit) and cover the pure CLI
presentation logic in `CliFormat` (labels, icons, role-target resolution and the
copy-pasteable `set` command-line builder). The Core Audio COM layers require a
real audio endpoint and are exercised manually.

```powershell
dotnet test AudioSwitcher.slnx
```

## Usage

```powershell
as list
as list --playback
as list --recording
as set --playback "Headphones"
as set --recording "USB Microphone"
as set --playback "Headphones" --recording "USB Microphone"
as set --playback "Headset" --recording "Headset Mic" --communications
as set --playback "Speakers" --default
as interactive       # or simply: .\as.exe
```

## Publish a single self-contained binary (Native AOT)

The project is configured for **Native AOT** (`<PublishAot>true</PublishAot>`), so
`dotnet publish` produces a single, fully native `as.exe` (~3 MB) with **no .NET
runtime dependency** — nothing else needs to be copied alongside it.

```powershell
dotnet publish -c Release
```

The executable is written to `bin/x64/Release/net10.0-windows/win-x64/publish/as.exe`.

### Build prerequisites

Native AOT compiles and links to native code, so the following are required on
the build machine (not on machines that merely run `as.exe`):

- The **Desktop development with C++** workload (MSVC `link.exe`), and
- The **Windows 11 SDK** (provides `advapi32.lib`, etc.).

Publish from a *Developer Command Prompt / Developer PowerShell for VS* (or run
`vcvars64.bat` first) so the linker can find the MSVC and Windows SDK libraries.

## Releases (CI)

Pushing a semantic-version tag builds, tests and publishes `as.exe` automatically
via [`.github/workflows/release.yml`](.github/workflows/release.yml) and attaches
the binary, a zip and SHA-256 checksums to a GitHub Release.

```powershell
git tag v1.0.0
git push origin v1.0.0
```

The workflow runs on `windows-latest` (which ships the MSVC toolchain and Windows
SDK needed for Native AOT), gates the release on the test suite, and derives the
build version from the tag. It can also be triggered manually from the Actions
tab to validate the build without creating a release.


