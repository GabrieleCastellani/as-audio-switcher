namespace AudioSwitcher.Audio;

/// <summary>
/// Which Windows default-device "slot" an operation targets. Windows tracks two
/// independent defaults per data flow: the multimedia default (Console +
/// Multimedia roles) and the communications default used by voice/calling apps.
/// </summary>
internal enum RoleTarget
{
    /// <summary>Console + Multimedia roles (the regular "Default Device").</summary>
    Multimedia,

    /// <summary>The Communications role (the "Default Communication Device").</summary>
    Communications,

    /// <summary>Every role at once (multimedia and communications).</summary>
    All
}
