using System.Runtime.InteropServices;

using AudioSwitcher.Interop;

namespace AudioSwitcher.Audio;

/// <summary>
/// High-level operations over the system's audio endpoints: enumerating
/// devices, resolving a device by name or id, and changing the default.
/// </summary>
internal static class AudioDeviceService
{
    /// <summary>Lists the active devices for a data flow, ordered by name.</summary>
    public static IReadOnlyList<AudioDevice> GetDevices(DataFlow flow)
    {
        var enumerator = CoreAudio.CreateEnumerator();

        var defaultId = GetDefaultDeviceId(enumerator, flow, Role.Console);
        var communicationsId = GetDefaultDeviceId(enumerator, flow, Role.Communications);

        Marshal.ThrowExceptionForHR(enumerator.EnumAudioEndpoints(flow, (int)DeviceState.Active, out var collection));
        Marshal.ThrowExceptionForHR(collection.GetCount(out var count));

        var devices = new List<AudioDevice>((int)count);
        for (uint i = 0; i < count; i++)
        {
            Marshal.ThrowExceptionForHR(collection.Item(i, out var device));

            var id = CoreAudio.GetDeviceId(device);
            var name = CoreAudio.GetFriendlyName(device).Trim();
            if (string.IsNullOrWhiteSpace(name))
                continue;

            devices.Add(new AudioDevice(
                Name: name,
                Id: id,
                Flow: flow,
                IsDefault: string.Equals(id, defaultId, StringComparison.OrdinalIgnoreCase),
                IsDefaultCommunications: string.Equals(id, communicationsId, StringComparison.OrdinalIgnoreCase)));
        }

        return devices
            .OrderBy(device => device.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>Finds a device by exact id, exact name, or unambiguous partial name match.</summary>
    public static AudioDevice? FindDevice(DataFlow flow, string nameOrId) =>
        Match(GetDevices(flow), nameOrId);

    /// <summary>
    /// Resolves a device from a candidate list. An exact id or name match always
    /// wins; otherwise a partial (substring) name match is used, but only when it
    /// is unambiguous. Returns null when nothing matches or the substring is
    /// ambiguous, so an exact match can never be shadowed by a partial one.
    /// </summary>
    internal static AudioDevice? Match(IReadOnlyList<AudioDevice> devices, string nameOrId)
    {
        var exact = devices.FirstOrDefault(device =>
            string.Equals(device.Id, nameOrId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(device.Name, nameOrId, StringComparison.OrdinalIgnoreCase));
        if (exact is not null)
            return exact;

        AudioDevice? partial = null;
        foreach (var device in devices)
        {
            if (!device.Name.Contains(nameOrId, StringComparison.OrdinalIgnoreCase))
                continue;
            if (partial is not null)
                return null; // ambiguous: more than one substring match
            partial = device;
        }

        return partial;
    }

    /// <summary>
    /// Picks which of two devices a toggle should switch to, given the id of the
    /// current default. When the current default is <paramref name="deviceA"/> we
    /// flip to <paramref name="deviceB"/>; otherwise (the current default is
    /// <paramref name="deviceB"/>, some unrelated device, or nothing) we land on
    /// <paramref name="deviceA"/>, the first listed "home" device.
    /// </summary>
    internal static AudioDevice SelectToggleTarget(string? currentDefaultId, AudioDevice deviceA, AudioDevice deviceB) =>
        string.Equals(currentDefaultId, deviceA.Id, StringComparison.OrdinalIgnoreCase) ? deviceB : deviceA;

    /// <summary>Makes <paramref name="device"/> the default for the requested role slot(s).</summary>
    public static void SetDefault(AudioDevice device, RoleTarget target = RoleTarget.All)
    {
        var config = CoreAudio.CreatePolicyConfig();
        var idPtr = Marshal.StringToCoTaskMemUni(device.Id);
        try
        {
            foreach (var role in RolesFor(target))
                Marshal.ThrowExceptionForHR(config.SetDefaultEndpoint(idPtr, role));
        }
        finally
        {
            Marshal.FreeCoTaskMem(idPtr);
        }
    }

    private static IEnumerable<Role> RolesFor(RoleTarget target) => target switch
    {
        RoleTarget.Multimedia => new[] { Role.Console, Role.Multimedia },
        RoleTarget.Communications => new[] { Role.Communications },
        _ => new[] { Role.Console, Role.Multimedia, Role.Communications }
    };

    private static string? GetDefaultDeviceId(IMMDeviceEnumerator enumerator, DataFlow flow, Role role)
    {
        try
        {
            // Returns a failure HRESULT (E_NOTFOUND) when no default exists for the role.
            if (enumerator.GetDefaultAudioEndpoint(flow, role, out var device) != 0)
                return null;
            return CoreAudio.GetDeviceId(device);
        }
        catch
        {
            return null;
        }
    }
}
