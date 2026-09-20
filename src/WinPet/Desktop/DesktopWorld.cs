using System.Runtime.InteropServices;
using System.Text;
using WinPet.Domain;

namespace WinPet.Desktop;

// Windows desktop adapter: converts native visibility and bounds into an immutable domain snapshot.
internal static class DesktopWorld
{
    internal static bool IsFullyTransparent(bool hasAttributes, byte alpha, uint flags) => hasAttributes && (flags & 2) != 0 && alpha == 0;
    internal static bool IsPhysicalObstacle(int cloaked, bool? current, bool transparent) => cloaked == 0 && current != false && !transparent;

    internal static World Read(nint ownWindow, List<string>? diagnostics = null, nint ignoredWindow = 0)
    {
        var obstacles = new List<RectangleF>();
        Native.EnumWindows((window, _) =>
        {
            if (window == ownWindow || window == ignoredWindow || !Native.IsWindowVisible(window) || Native.IsIconic(window)) return true;
            var name = new StringBuilder(256);
            Native.GetClassName(window, name, name.Capacity);
            Native.GetCloaked(window, 14, out int cloaked, sizeof(int));
            Native.GetWindowRect(window, out var raw);
            bool layered = Native.GetLayeredWindowAttributes(window, out uint colorKey, out byte alpha, out uint flags);
            bool? current = VirtualDesktops.IsCurrent(window);
            bool transparent = IsFullyTransparent(layered, alpha, flags);
            if (diagnostics != null)
            {
                Native.GetWindowThreadProcessId(window, out uint pid);
                string processName;
                try { using var process = System.Diagnostics.Process.GetProcessById((int)pid); processName = process.ProcessName; }
                catch (ArgumentException) { processName = "exited"; }
                diagnostics.Add($"Class={name}; Process={processName}; CurrentWorkspace={current}; Transparent={transparent}; Cloaked={cloaked}; Rect={raw.Left},{raw.Top},{raw.Right},{raw.Bottom}");
            }
            if (!IsPhysicalObstacle(cloaked, current, transparent)) return true;
            if (name.ToString() is "Progman" or "WorkerW" or "Shell_TrayWnd" or "Shell_SecondaryTrayWnd") return true;
            if (Native.GetFrame(window, 9, out var rect, Marshal.SizeOf<Native.Rect>()) != 0 && !Native.GetWindowRect(window, out rect)) return true;
            if (rect.Right > rect.Left && rect.Bottom > rect.Top)
                obstacles.Add(RectangleF.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom));
            return true;
        }, 0);
        return new World(Screen.AllScreens.Select(screen => (RectangleF)screen.WorkingArea).ToArray(), obstacles);
    }
}
