using System.Runtime.InteropServices;
using System.Text;

namespace WinPet;

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

    internal static bool IsFullyTransparent(bool hasAttributes, byte alpha, uint flags) => hasAttributes && (flags & 2) != 0 && alpha == 0;
    internal static bool IsPhysicalObstacle(int cloaked, bool? current, bool transparent) => cloaked == 0 && current != false && !transparent;

    internal static World ReadWorld(nint ownWindow, List<string>? diagnostics = null)
    {
        var obstacles = new List<RectangleF>();
        EnumWindows((window, _) =>
        {
            if (window == ownWindow || !IsWindowVisible(window) || IsIconic(window)) return true;
            var name = new StringBuilder(256);
            GetClassName(window, name, name.Capacity);
            GetCloaked(window, 14, out int cloaked, sizeof(int));
            GetWindowRect(window, out var raw);
            bool layered = GetLayeredWindowAttributes(window, out uint colorKey, out byte alpha, out uint flags);
            bool? current = VirtualDesktops.IsCurrent(window);
            bool transparent = IsFullyTransparent(layered, alpha, flags);
            if (diagnostics != null)
            {
                GetWindowThreadProcessId(window, out uint pid);
                string processName;
                try { using var process = System.Diagnostics.Process.GetProcessById((int)pid); processName = process.ProcessName; }
                catch (ArgumentException) { processName = "exited"; }
                diagnostics.Add($"Class={name}; Process={processName}; CurrentWorkspace={current}; Transparent={transparent}; Cloaked={cloaked}; Rect={raw.Left},{raw.Top},{raw.Right},{raw.Bottom}");
            }
            if (!IsPhysicalObstacle(cloaked, current, transparent)) return true;
            if (name.ToString() is "Progman" or "WorkerW" or "Shell_TrayWnd" or "Shell_SecondaryTrayWnd") return true;
            if (GetFrame(window, 9, out var rect, Marshal.SizeOf<Rect>()) != 0 && !GetWindowRect(window, out rect)) return true;
            if (rect.Right > rect.Left && rect.Bottom > rect.Top)
                obstacles.Add(RectangleF.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom));
            return true;
        }, 0);
        return new World(Screen.AllScreens.Select(screen => (RectangleF)screen.WorkingArea).ToArray(), obstacles);
    }
}
