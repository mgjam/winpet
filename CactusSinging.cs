using System.Drawing.Drawing2D;

namespace WinPet;

internal static class CactusSinging
{
    public static void Paint(Graphics g, double seconds, float amount)
    {
        for (int i = 0; i < 3; i++)
        {
            float phase = (float)((seconds * .32 + i / 3.0) % 1);
            float x = i == 1 ? 6 : 68 + (float)Math.Sin(phase * Math.PI * 2);
            float y = 34 - phase * 28;
            float opacity = Math.Min(1, Math.Min(phase / .12f, (1 - phase) / .2f));
            var color = Color.FromArgb((int)(255 * opacity), 55, 86, 107);
            using var note = new Pen(color, 1.8f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            using var fill = new SolidBrush(color);
            g.FillEllipse(fill, x - 4, y + 7, 6, 4);
            g.DrawLine(note, x + 1, y + 8, x + 1, y);
            g.DrawBezier(note, x + 1, y, x + 5, y + 1, x + 5, y + 4, x + 3, y + 4);
        }
    }
}
