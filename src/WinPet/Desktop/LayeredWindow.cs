using System.ComponentModel;
using System.Runtime.InteropServices;

namespace WinPet.Desktop;

// Owns the native GDI resources used to present a bitmap; knows nothing about pets.
internal static class LayeredWindow
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct Blend { public byte Operation, Flags, Opacity, AlphaFormat; }
    [DllImport("user32.dll", SetLastError = true)] private static extern nint GetDC(nint window);
    [DllImport("user32.dll")] private static extern int ReleaseDC(nint window, nint dc);
    [DllImport("gdi32.dll", SetLastError = true)] private static extern nint CreateCompatibleDC(nint dc);
    [DllImport("gdi32.dll")] private static extern bool DeleteDC(nint dc);
    [DllImport("gdi32.dll", SetLastError = true)] private static extern nint SelectObject(nint dc, nint value);
    [DllImport("gdi32.dll")] private static extern bool DeleteObject(nint value);
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UpdateLayeredWindow(nint window, nint screen, ref Point position,
        ref Size size, nint source, ref Point origin, uint colorKey, ref Blend blend, uint flags);


    public static void Draw(nint window, Bitmap frame, Point position)
    {
        nint screen = 0, memory = 0, bitmap = 0, previous = 0;
        try
        {
            screen = GetDC(0);
            if (screen == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
            memory = CreateCompatibleDC(screen);
            if (memory == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
            bitmap = frame.GetHbitmap(Color.FromArgb(0));
            previous = SelectObject(memory, bitmap);
            if (previous == 0 || previous == -1) throw new Win32Exception(Marshal.GetLastWin32Error());
            var origin = Point.Empty;
            var size = frame.Size;
            var blend = new Blend { Opacity = 255, AlphaFormat = 1 };
            if (!UpdateLayeredWindow(window, screen, ref position, ref size, memory, ref origin, 0, ref blend, 2))
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }
        finally
        {
            if (previous != 0 && previous != -1) SelectObject(memory, previous);
            if (bitmap != 0) DeleteObject(bitmap);
            if (memory != 0) DeleteDC(memory);
            if (screen != 0) ReleaseDC(0, screen);
        }
    }
}
