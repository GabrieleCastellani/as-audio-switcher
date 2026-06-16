using System.Runtime.InteropServices;

namespace AudioSwitcher.Interop;

/// <summary>Win32 PROPERTYKEY: identifies a single property in a property store.</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct PropertyKey
{
    public Guid FormatId;
    public uint PropertyId;
}

/// <summary>
/// Win32 PROPVARIANT (x64 layout): a 2-byte VARTYPE, 6 bytes of padding,
/// then an 8-byte union. For VT_LPWSTR the union holds a wide-string pointer.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct PropVariant
{
    public ushort VarType;
    public ushort Reserved1;
    public ushort Reserved2;
    public ushort Reserved3;
    public IntPtr Value;
}
