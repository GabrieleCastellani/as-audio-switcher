using AudioSwitcher.Interop;

namespace AudioSwitcher.Audio;

/// <summary>An active audio endpoint and its current default status.</summary>
/// <param name="Name">Friendly display name.</param>
/// <param name="Id">Stable endpoint identifier used when setting defaults.</param>
/// <param name="Flow">Whether this is a playback (render) or recording (capture) device.</param>
/// <param name="IsDefault">True when this is the multimedia default device.</param>
/// <param name="IsDefaultCommunications">True when this is the communications default device.</param>
internal sealed record AudioDevice(
    string Name,
    string Id,
    DataFlow Flow,
    bool IsDefault,
    bool IsDefaultCommunications);
