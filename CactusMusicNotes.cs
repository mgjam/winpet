using System.Drawing.Drawing2D;

namespace WinPet;

// Shared note shape and palette; each musical activity chooses its own placement.
internal static class CactusMusicNotes
{
    public static void Paint(Graphics g, float x, float y, float opacity)
    {
        var color = Color.FromArgb((int)(255 * Math.Clamp(opacity, 0, 1)), 55, 86, 107);
        using var note = new Pen(color, 1.8f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        using var fill = new SolidBrush(color);
        g.FillEllipse(fill, x - 4, y + 7, 6, 4);
        g.DrawLine(note, x + 1, y + 8, x + 1, y);
        g.DrawBezier(note, x + 1, y, x + 5, y + 1, x + 5, y + 4, x + 3, y + 4);
    }
}
