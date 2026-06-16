using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace AudioSwitcher.Interop;

/// <summary>
/// Thin wrapper over the Core Audio COM APIs: activates the MMDevice
/// enumerator and the (undocumented) PolicyConfig client, and reads the
/// handful of device properties this tool needs.
/// </summary>
internal static partial class CoreAudio
{
    private const int VtLpwstr = 31;
    private const int ClsCtxInprocServer = 1;
    private const int StgmRead = 0;
    private const int CoinitApartmentThreaded = 2;

    // PKEY_Device_FriendlyName
    private static readonly PropertyKey FriendlyNameKey = new()
    {
        FormatId = new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"),
        PropertyId = 14
    };

    private static readonly Guid ClsidDeviceEnumerator = new("BCDE0395-E52F-467C-8E3D-C4579291692E");
    private static readonly Guid IidDeviceEnumerator = new("A95664D2-9614-4F35-A746-DE8DB63617E6");
    private static readonly Guid ClsidPolicyConfig = new("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9");
    private static readonly Guid IidPolicyConfig = new("F8679F50-850A-41CF-9C72-430F290290C8");

    private static readonly StrategyBasedComWrappers Wrappers = new();

    [LibraryImport("ole32.dll")]
    private static partial int CoInitializeEx(IntPtr reserved, int coInit);

    [LibraryImport("ole32.dll")]
    private static partial int CoCreateInstance(in Guid clsid, IntPtr outer, int clsContext, in Guid iid, out IntPtr instance);

    [LibraryImport("ole32.dll")]
    private static partial int PropVariantClear(ref PropVariant value);

    [LibraryImport("ole32.dll")]
    private static partial void CoTaskMemFree(IntPtr ptr);

    static CoreAudio()
    {
        // Ignore S_FALSE / RPC_E_CHANGED_MODE — the apartment is usable either way.
        CoInitializeEx(IntPtr.Zero, CoinitApartmentThreaded);
    }

    public static IMMDeviceEnumerator CreateEnumerator() =>
        Activate<IMMDeviceEnumerator>(ClsidDeviceEnumerator, IidDeviceEnumerator);

    public static IPolicyConfig CreatePolicyConfig() =>
        Activate<IPolicyConfig>(ClsidPolicyConfig, IidPolicyConfig);

    public static string GetDeviceId(IMMDevice device)
    {
        Marshal.ThrowExceptionForHR(device.GetId(out var ptr));
        try
        {
            return Marshal.PtrToStringUni(ptr) ?? string.Empty;
        }
        finally
        {
            CoTaskMemFree(ptr);
        }
    }

    public static string GetFriendlyName(IMMDevice device)
    {
        if (device.OpenPropertyStore(StgmRead, out var store) != 0)
            return string.Empty;

        var key = FriendlyNameKey;
        if (store.GetValue(in key, out var value) != 0)
            return string.Empty;

        try
        {
            return value.VarType == VtLpwstr ? Marshal.PtrToStringUni(value.Value) ?? string.Empty : string.Empty;
        }
        finally
        {
            PropVariantClear(ref value);
        }
    }

    private static T Activate<T>(Guid clsid, Guid iid)
    {
        Marshal.ThrowExceptionForHR(CoCreateInstance(clsid, IntPtr.Zero, ClsCtxInprocServer, iid, out var ptr));
        try
        {
            return (T)Wrappers.GetOrCreateObjectForComInstance(ptr, CreateObjectFlags.None);
        }
        finally
        {
            Marshal.Release(ptr);
        }
    }
}
