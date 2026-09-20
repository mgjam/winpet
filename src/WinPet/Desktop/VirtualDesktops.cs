using System.Runtime.InteropServices;

namespace WinPet.Desktop;

// Public Windows virtual-desktop API; never switches the user's workspace.
internal static class VirtualDesktops
{
    [ComImport, Guid("A5CD92FF-29BE-454C-8D04-D82879FB3F1B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IManager
    {
        [PreserveSig] int IsWindowOnCurrentVirtualDesktop(nint window, [MarshalAs(UnmanagedType.Bool)] out bool current);
        [PreserveSig] int GetWindowDesktopId(nint window, out Guid desktop);
        [PreserveSig] int MoveWindowToDesktop(nint window, in Guid desktop);
    }

    private static readonly IManager? Manager = CreateManager();

    private static IManager? CreateManager()
    {
        try
        {
            var type = Type.GetTypeFromCLSID(new Guid("AA509086-5CA9-4C25-8F95-589D3C07B48A"));
            return type == null ? null : (IManager?)Activator.CreateInstance(type);
        }
        catch (COMException) { return null; }
    }

    public static bool? IsCurrent(nint window) => Manager != null && Manager.IsWindowOnCurrentVirtualDesktop(window, out bool current) == 0 ? current : null;
    public static Guid DesktopOf(nint window) => Manager != null && Manager.GetWindowDesktopId(window, out Guid desktop) == 0 ? desktop : Guid.Empty;
    public static bool Move(nint window, Guid desktop) => desktop != Guid.Empty && Manager != null && Manager.MoveWindowToDesktop(window, in desktop) == 0;

    public static bool FollowCurrent(nint ownWindow)
    {
        if (IsCurrent(ownWindow) != false) return true;
        bool moved = false;
        Native.EnumWindows((window, _) =>
        {
            if (window == ownWindow || !Native.IsWindowVisible(window) || Native.IsIconic(window) || IsCurrent(window) != true) return true;
            Guid desktop = DesktopOf(window);
            if (desktop == Guid.Empty || desktop == DesktopOf(ownWindow)) return true;
            moved = Move(ownWindow, desktop) && IsCurrent(ownWindow) == true;
            return !moved;
        }, 0);
        return moved;
    }
}
