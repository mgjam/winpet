using System.Drawing.Drawing2D;

namespace WinPet;

internal static class CactusLaptop
{
    public static void Paint(Graphics g, double seconds, float amount)
    {
        var saved = g.Save();
        g.TranslateTransform(0, 9 * (1 - amount));
        using var edge = new Pen(Color.FromArgb(36, 70, 53), 1.6f) { LineJoin = LineJoin.Round };
        using var shell = new SolidBrush(Color.FromArgb(161, 183, 179));
        using var deck = new SolidBrush(Color.FromArgb(211, 225, 213));
        using var green = new SolidBrush(Color.FromArgb(105, 170, 107));

        // The keyboard extends toward Cacti, behind the foreground screen.
        PointF[] keyboard = [new(24, 65), new(52, 65), new(58, 80), new(18, 80)];
        g.FillPolygon(deck, keyboard);
        g.DrawPolygon(edge, keyboard);

        // Wrists peek around the lid as Cacti types, then pauses to think.
        float cycle = (float)(seconds % 5);
        float tap = cycle < 3.8f ? (float)Math.Sin(seconds * 15) : 0;
        for (int side = 0; side < 2; side++)
        {
            float x = side == 0 ? 17 : 51;
            float y = 66 - Math.Max(0, side == 0 ? tap : -tap) * 2;
            g.FillEllipse(green, x, y, 8, 6);
            g.DrawArc(edge, x, y, 8, 6, side == 0 ? 260 : 100, 230);
        }

        // We see the back of the lid; its display faces Cacti, hiding the keys.
        PointF[] lid = [new(18, 60), new(58, 60), new(55, 80), new(21, 80)];
        g.FillPolygon(shell, lid);
        g.DrawPolygon(edge, lid);
        using var highlight = new Pen(Color.FromArgb(211, 225, 213), 1.2f);
        g.DrawLine(highlight, 22, 63, 54, 63);
        g.FillEllipse(deck, 35, 68, 6, 5);
        g.FillRectangle(deck, 19, 80, 38, 2);
        g.DrawLine(edge, 19, 82, 57, 82);
        g.Restore(saved);
    }
}
