using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace AudioSwitcher.Interop;

// Native-AOT-compatible Core Audio interop using source-generated COM
// ([GeneratedComInterface]). Only the minimal slice of the MMDevice and
// PolicyConfig APIs this tool needs is declared here.
//
// Methods that are actually called get real signatures; earlier vtable slots
// are kept as opaque placeholders so the interface layout stays ABI-correct.

[GeneratedComInterface]
[Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
internal partial interface IMMDeviceEnumerator
{
    [PreserveSig]
    int EnumAudioEndpoints(DataFlow dataFlow, int stateMask, out IMMDeviceCollection devices);

    [PreserveSig]
    int GetDefaultAudioEndpoint(DataFlow dataFlow, Role role, out IMMDevice endpoint);
}

[GeneratedComInterface]
[Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E")]
internal partial interface IMMDeviceCollection
{
    [PreserveSig]
    int GetCount(out uint count);

    [PreserveSig]
    int Item(uint index, out IMMDevice device);
}

[GeneratedComInterface]
[Guid("D666063F-1587-4E43-81F1-B948E807363F")]
internal partial interface IMMDevice
{
    [PreserveSig]
    int Activate(in Guid iid, int clsCtx, IntPtr activationParams, out IntPtr instance);

    [PreserveSig]
    int OpenPropertyStore(int stgmAccess, out IPropertyStore properties);

    [PreserveSig]
    int GetId(out IntPtr id);

    [PreserveSig]
    int GetState(out int state);
}

[GeneratedComInterface]
[Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
internal partial interface IPropertyStore
{
    [PreserveSig]
    int GetCount(out uint count);

    [PreserveSig]
    int GetAt(uint index, out PropertyKey key);

    [PreserveSig]
    int GetValue(in PropertyKey key, out PropVariant value);
}

[GeneratedComInterface]
[Guid("F8679F50-850A-41CF-9C72-430F290290C8")]
internal partial interface IPolicyConfig
{
    [PreserveSig] int GetMixFormat(IntPtr deviceName, IntPtr format);
    [PreserveSig] int GetDeviceFormat(IntPtr deviceName, int useDefault, IntPtr format);
    [PreserveSig] int ResetDeviceFormat(IntPtr deviceName);
    [PreserveSig] int SetDeviceFormat(IntPtr deviceName, IntPtr endpointFormat, IntPtr mixFormat);
    [PreserveSig] int GetProcessingPeriod(IntPtr deviceName, int useDefault, IntPtr defaultPeriod, IntPtr minimumPeriod);
    [PreserveSig] int SetProcessingPeriod(IntPtr deviceName, IntPtr period);
    [PreserveSig] int GetShareMode(IntPtr deviceName, IntPtr mode);
    [PreserveSig] int SetShareMode(IntPtr deviceName, IntPtr mode);
    [PreserveSig] int GetPropertyValue(IntPtr deviceName, int fxStore, IntPtr key, IntPtr value);
    [PreserveSig] int SetPropertyValue(IntPtr deviceName, int fxStore, IntPtr key, IntPtr value);

    [PreserveSig]
    int SetDefaultEndpoint(IntPtr deviceName, Role role);

    [PreserveSig] int SetEndpointVisibility(IntPtr deviceName, int visible);
}
