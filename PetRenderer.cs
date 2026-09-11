using System.ComponentModel;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace WinPet;

// The artwork's own alpha supplies both smooth edges and native mouse hit testing.
internal static class PetRenderer
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct Blend { public byte Operation, Flags, Opacity, AlphaFormat; }
    [DllImport("user32.dll")] private static extern nint GetDC(nint window);
    [DllImport("user32.dll")] private static extern int ReleaseDC(nint window, nint dc);
    [DllImport("gdi32.dll")] private static extern nint CreateCompatibleDC(nint dc);
    [DllImport("gdi32.dll")] private static extern bool DeleteDC(nint dc);
    [DllImport("gdi32.dll")] private static extern nint SelectObject(nint dc, nint value);
    [DllImport("gdi32.dll")] private static extern bool DeleteObject(nint value);
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UpdateLayeredWindow(nint window, nint screen, ref Point position,
        ref Size size, nint source, ref Point origin, uint colorKey, ref Blend blend, uint flags);

    public static Bitmap Frame(IPet pet, Mood mood, double seconds, int facing, PetGaze gaze = default, RoutinePose? routine = null)
    {
        var frame = new Bitmap(pet.Size.Width, pet.Size.Height, PixelFormat.Format32bppPArgb);
        using var graphics = Graphics.FromImage(frame);
        graphics.Clear(Color.Transparent);
        pet.Paint(graphics, pet.ComposePose(mood, seconds, facing, gaze, routine));
        return frame;
    }

    public static void Draw(nint window, IPet pet, Mood mood, double seconds, int facing, Point position, PetGaze gaze = default, RoutinePose? routine = null)
    {
        using var frame = Frame(pet, mood, seconds, facing, gaze, routine);
        nint screen = GetDC(0);
        nint memory = CreateCompatibleDC(screen);
        nint bitmap = frame.GetHbitmap(Color.FromArgb(0));
        nint previous = SelectObject(memory, bitmap);
        try
        {
            var origin = Point.Empty;
            var size = pet.Size;
            var blend = new Blend { Opacity = 255, AlphaFormat = 1 };
            if (!UpdateLayeredWindow(window, screen, ref position, ref size, memory, ref origin, 0, ref blend, 2))
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }
        finally
        {
            SelectObject(memory, previous);
            DeleteObject(bitmap);
            DeleteDC(memory);
            ReleaseDC(0, screen);
        }
    }
}
