namespace AudioSwitcher.Interop;

/// <summary>Direction of an audio stream for an endpoint device (Win32 EDataFlow).</summary>
internal enum DataFlow
{
    /// <summary>Output/playback endpoints (speakers, headphones).</summary>
    Render = 0,

    /// <summary>Input/recording endpoints (microphones).</summary>
    Capture = 1,

    /// <summary>Both render and capture.</summary>
    All = 2
}

/// <summary>Role a default endpoint fulfils for the system (Win32 ERole).</summary>
internal enum Role
{
    /// <summary>Games, system notifications and voice commands.</summary>
    Console = 0,

    /// <summary>Music, movies and other media playback.</summary>
    Multimedia = 1,

    /// <summary>Voice communications (calling/conferencing apps).</summary>
    Communications = 2
}

/// <summary>Endpoint device state mask (Win32 DEVICE_STATE_*).</summary>
[Flags]
internal enum DeviceState
{
    Active = 0x1,
    Disabled = 0x2,
    NotPresent = 0x4,
    Unplugged = 0x8,
    All = Active | Disabled | NotPresent | Unplugged
}
