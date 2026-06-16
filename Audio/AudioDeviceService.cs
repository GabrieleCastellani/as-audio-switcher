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

    /// <summary>Finds a device by exact id, exact name, or partial name match.</summary>
    public static AudioDevice? FindDevice(DataFlow flow, string nameOrId)
    {
        return GetDevices(flow).FirstOrDefault(device =>
            string.Equals(device.Id, nameOrId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(device.Name, nameOrId, StringComparison.OrdinalIgnoreCase) ||
            device.Name.Contains(nameOrId, StringComparison.OrdinalIgnoreCase));
    }

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
