using System.Runtime.InteropServices;
using System.Text;

namespace WinPet.Desktop;

internal static class Native
{
    internal delegate bool EnumCallback(nint window, nint parameter);
    [StructLayout(LayoutKind.Sequential)]
    internal struct Rect { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] internal static extern bool EnumWindows(EnumCallback callback, nint parameter);
    [DllImport("user32.dll")] internal static extern bool IsWindowVisible(nint window);
    [DllImport("user32.dll")] internal static extern bool IsIconic(nint window);
    [DllImport("user32.dll")] internal static extern bool GetWindowRect(nint window, out Rect rect);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] internal static extern int GetClassName(nint window, StringBuilder name, int size);
    [DllImport("dwmapi.dll", EntryPoint = "DwmGetWindowAttribute")] internal static extern int GetFrame(nint window, int attribute, out Rect value, int size);
    [DllImport("dwmapi.dll", EntryPoint = "DwmGetWindowAttribute")] internal static extern int GetCloaked(nint window, int attribute, out int value, int size);
    [DllImport("user32.dll")] internal static extern bool SetWindowPos(nint window, nint after, int x, int y, int width, int height, uint flags);
    [DllImport("user32.dll")] internal static extern uint GetWindowThreadProcessId(nint window, out uint process);
    [DllImport("user32.dll")] internal static extern bool GetLayeredWindowAttributes(nint window, out uint color, out byte alpha, out uint flags);

    internal const int NoActivateStyle = 0x08000000;
    internal const int ToolWindowStyle = 0x00000080;
    internal const int LayeredStyle = 0x00080000;
    internal const int MouseActivateMessage = 0x21;
    internal const int NoActivateResult = 3;

    internal static void KeepTopmost(nint window) => SetWindowPos(window, -1, 0, 0, 0, 0, 0x13);
}
