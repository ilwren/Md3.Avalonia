#if WINDOWS
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Md3.Avalonia.Gallery;

namespace Md3.Avalonia.Gallery.Desktop;

/// <summary>
/// Registers the unpackaged desktop Gallery as a Windows toast app for the current user.
/// Windows requires a Start-menu shortcut whose AppUserModelID matches the toast notifier ID.
/// This uses the Shell COM API directly and does not launch a script or helper process.
/// </summary>
internal static class WindowsToastRegistration
{
    private static readonly Guid ShellLinkClassId = new("00021401-0000-0000-C000-000000000046");
    private static readonly Guid AppUserModelIdFormatId = new("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3");
    private const uint ShellChangeCreate = 0x00000002;
    private const uint ShellChangeUpdateItem = 0x00002000;
    private const uint ShellChangePathUnicode = 0x0005;
    private const uint ShellChangeFlush = 0x1000;

    public static string RegisterForCurrentUser()
    {
        ThrowIfFailed(SetCurrentProcessExplicitAppUserModelID(GalleryNotificationIdentity.WindowsAppUserModelId));

        var executablePath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath))
            throw new InvalidOperationException("Windows could not locate the Gallery executable to register its notification shortcut.");

        var programsPath = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
        if (string.IsNullOrWhiteSpace(programsPath))
            throw new InvalidOperationException("Windows did not provide the current user's Start-menu Programs folder.");

        Directory.CreateDirectory(programsPath);
        var shortcutPath = Path.Combine(programsPath, "Material 3 for Avalonia Gallery.lnk");
        var shortcutAlreadyExisted = File.Exists(shortcutPath);
        var linkType = Type.GetTypeFromCLSID(ShellLinkClassId, throwOnError: true)
            ?? throw new InvalidOperationException("Windows ShellLink is unavailable.");
        var shellLinkObject = Activator.CreateInstance(linkType)
            ?? throw new InvalidOperationException("Windows could not create the Gallery Start-menu shortcut.");

        try
        {
            var shellLink = (IShellLinkW)shellLinkObject;
            ThrowIfFailed(shellLink.SetPath(executablePath));
            ThrowIfFailed(shellLink.SetWorkingDirectory(Path.GetDirectoryName(executablePath) ?? programsPath));
            ThrowIfFailed(shellLink.SetDescription("Material 3 for Avalonia Gallery"));
            ThrowIfFailed(shellLink.SetIconLocation(executablePath, 0));

            var propertyStore = (IPropertyStore)shellLinkObject;
            var appIdKey = new PropertyKey(AppUserModelIdFormatId, 5);
            var appIdValue = PropVariant.FromString(GalleryNotificationIdentity.WindowsAppUserModelId);
            try
            {
                ThrowIfFailed(propertyStore.SetValue(ref appIdKey, ref appIdValue));
                ThrowIfFailed(propertyStore.Commit());
            }
            finally
            {
                appIdValue.Dispose();
            }

            ((IPersistFile)shellLinkObject).Save(shortcutPath, true);
        }
        finally
        {
            if (Marshal.IsComObject(shellLinkObject))
                Marshal.FinalReleaseComObject(shellLinkObject);
        }

        if (!File.Exists(shortcutPath))
            throw new IOException("Windows did not persist the Gallery Start-menu shortcut.");

        // Notify the Windows shell immediately. Toast registration can fail if Explorer has not
        // indexed a shortcut created during this launch yet, even though the .lnk was saved.
        SHChangeNotify(
            shortcutAlreadyExisted ? ShellChangeUpdateItem : ShellChangeCreate,
            ShellChangePathUnicode | ShellChangeFlush,
            shortcutPath,
            IntPtr.Zero);

        return shortcutPath;
    }

    private static void ThrowIfFailed(int hresult)
    {
        if (hresult < 0) Marshal.ThrowExceptionForHR(hresult);
    }

    [DllImport("shell32.dll", ExactSpelling = true)]
    private static extern int SetCurrentProcessExplicitAppUserModelID(
        [MarshalAs(UnmanagedType.LPWStr)] string appId);

    [DllImport("shell32.dll", ExactSpelling = true)]
    private static extern void SHChangeNotify(
        uint eventId,
        uint flags,
        [MarshalAs(UnmanagedType.LPWStr)] string path,
        IntPtr item2);

    [ComImport]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellLinkW
    {
        [PreserveSig] int GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder file, int maxPath, IntPtr findData, uint flags);
        [PreserveSig] int GetIDList(out IntPtr itemIdList);
        [PreserveSig] int SetIDList(IntPtr itemIdList);
        [PreserveSig] int GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder name, int maxName);
        [PreserveSig] int SetDescription([MarshalAs(UnmanagedType.LPWStr)] string name);
        [PreserveSig] int GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder directory, int maxPath);
        [PreserveSig] int SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string directory);
        [PreserveSig] int GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder arguments, int maxPath);
        [PreserveSig] int SetArguments([MarshalAs(UnmanagedType.LPWStr)] string arguments);
        [PreserveSig] int GetHotkey(out short hotkey);
        [PreserveSig] int SetHotkey(short hotkey);
        [PreserveSig] int GetShowCmd(out int showCommand);
        [PreserveSig] int SetShowCmd(int showCommand);
        [PreserveSig] int GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder iconPath, int maxPath, out int iconIndex);
        [PreserveSig] int SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string iconPath, int iconIndex);
        [PreserveSig] int SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string path, uint reserved);
        [PreserveSig] int Resolve(IntPtr window, uint flags);
        [PreserveSig] int SetPath([MarshalAs(UnmanagedType.LPWStr)] string path);
    }

    [ComImport]
    [Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCf99")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IPropertyStore
    {
        [PreserveSig] int GetCount(out uint count);
        [PreserveSig] int GetAt(uint index, out PropertyKey key);
        [PreserveSig] int GetValue(ref PropertyKey key, out PropVariant value);
        [PreserveSig] int SetValue(ref PropertyKey key, ref PropVariant value);
        [PreserveSig] int Commit();
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PropertyKey
    {
        public Guid FormatId;
        public uint PropertyId;

        public PropertyKey(Guid formatId, uint propertyId)
        {
            FormatId = formatId;
            PropertyId = propertyId;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct PropVariant
    {
        [FieldOffset(0)] public ushort VariantType;
        [FieldOffset(2)] public ushort Reserved1;
        [FieldOffset(4)] public ushort Reserved2;
        [FieldOffset(6)] public ushort Reserved3;
        [FieldOffset(8)] public IntPtr PointerValue;

        public static PropVariant FromString(string value) => new()
        {
            VariantType = 31, // VT_LPWSTR
            PointerValue = Marshal.StringToCoTaskMemUni(value)
        };

        public void Dispose()
        {
            if (PointerValue == IntPtr.Zero) return;
            Marshal.FreeCoTaskMem(PointerValue);
            PointerValue = IntPtr.Zero;
        }
    }
}
#endif
