using System.Drawing.Drawing2D;

namespace WinPet;

// A small, rounded prop using Cacti's own outline, pot and highlight palette.
// Only small wrists show at the neck and bow; Cacti's branches stay unchanged.
internal static class CactusViolin
{
    public static void Paint(Graphics g, double seconds, float amount)
    {
        var placement = g.Save();
        g.TranslateTransform(32, 63 + 5 * (1 - amount));
        g.RotateTransform(20 * (1 - amount));
        g.TranslateTransform(-32, -63);
        using var edge = new Pen(Color.FromArgb(36, 70, 53), 2) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round };
        using var wood = new SolidBrush(Color.FromArgb(222, 151, 110));
        using var highlight = new Pen(Color.FromArgb(248, 196, 145), 2) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        using var dark = new SolidBrush(Color.FromArgb(59, 81, 60));
        using var green = new SolidBrush(Color.FromArgb(105, 170, 107));
        float stroke = (float)Math.Sin(seconds * 1.8) * 4;
        float x = 25 + stroke * .65f, y = 75 - stroke * .76f;
        float handX = x + 5, handY = y - 6;
        var saved = g.Save();
        // Rest the lower bout below the chin, with the neck reaching sideways.
        g.TranslateTransform(43, 57);
        g.RotateTransform(88);
        g.FillRectangle(wood, -2, -23, 4, 17);
        g.DrawRectangle(edge, -2, -23, 4, 17);
        g.DrawLine(edge, -4, -21, 4, -21);
        g.FillEllipse(wood, -3, -27, 6, 6);
        g.DrawEllipse(edge, -3, -27, 6, 6);
        using var body = new GraphicsPath();
        body.AddBezier(0, -10, -8, -12, -11, -5, -6, -1);
        body.AddBezier(-6, -1, -3, 1, -4, 4, -7, 5);
        body.AddBezier(-7, 5, -13, 12, -6, 17, 0, 15);
        body.AddBezier(0, 15, 6, 17, 13, 12, 7, 5);
        body.AddBezier(7, 5, 4, 4, 3, 1, 6, -1);
        body.AddBezier(6, -1, 11, -5, 8, -12, 0, -10);
        body.CloseFigure();
        g.FillPath(wood, body);
        g.DrawPath(edge, body);
        g.DrawBezier(highlight, -6, 8, -7, 11, -4, 13, -2, 13);
        // Readable shapes at native size, rather than miniature instrument detail.
        g.FillRectangle(dark, -1.5f, -21, 3, 20);
        g.FillEllipse(dark, -2, 9, 4, 5);
        using var stringLine = new Pen(Color.FromArgb(250, 223, 176), 1);
        g.DrawLine(stringLine, 0, -20, 0, 10);
        g.DrawLine(highlight, -3, 5, 3, 5);
        using var detail = new Pen(edge.Color, 1.3f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        g.DrawArc(detail, -6, 0, 3, 5, 70, 190);
        g.DrawArc(detail, 3, 0, 3, 5, -110, 190);
        g.Restore(saved);
        g.FillEllipse(green, 59, 55, 7, 6);
        g.DrawArc(edge, 59, 55, 7, 6, -20, 240);

        // Longer strokes ease at each change of direction. The hand and bow move
        // together along the bow's own axis, keeping contact with the strings.
        // One rounded bow silhouette with a warm hair inset, matching body lines.
        g.DrawLine(edge, x, y, x + 24, y - 28);
        using var hair = new Pen(Color.FromArgb(246, 210, 162), 1) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        g.DrawLine(hair, x + 1.5f, y, x + 25.5f, y - 28);
        g.FillEllipse(green, handX - 3, handY - 2, 7, 6);
        g.DrawArc(edge, handX - 3, handY - 2, 7, 6, -30, 250);
        g.Restore(placement);
    }
}
