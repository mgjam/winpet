using System.Drawing.Drawing2D;

namespace WinPet;

internal static class CactusReading
{
    public static void Paint(Graphics g, double seconds, float amount)
    {
        var saved = g.Save();
        g.TranslateTransform(38, 72 + 8 * (1 - amount));
        g.ScaleTransform(.18f + .82f * amount, .8f + .2f * amount);
        g.TranslateTransform(-38, -72);
        using var cover = new SolidBrush(Color.FromArgb(79, 132, 126));
        using var paper = new SolidBrush(Color.FromArgb(255, 239, 199));
        using var pageShadow = new SolidBrush(Color.FromArgb(230, 208, 162));
        using var edge = new Pen(Color.FromArgb(55, 87, 69), 1.6f) { LineJoin = LineJoin.Round };
        using var ink = new Pen(Color.FromArgb(185, 163, 124), .8f);
        using var binding = new GraphicsPath();
        binding.AddBezier(20, 59, 26, 57, 33, 60, 38, 62);
        binding.AddBezier(38, 62, 44, 59, 50, 57, 56, 59);
        binding.AddLine(56, 59, 56, 77);
        binding.AddBezier(56, 77, 49, 76, 43, 78, 38, 80);
        binding.AddBezier(38, 80, 32, 78, 26, 76, 20, 77);
        binding.CloseFigure();
        g.FillPath(cover, binding);
        g.DrawPath(edge, binding);
        using var pages = new GraphicsPath();
        pages.AddBezier(22, 58, 29, 57, 34, 59, 38, 62);
        pages.AddBezier(38, 62, 43, 59, 49, 57, 54, 58);
        pages.AddLine(54, 58, 54, 74);
        pages.AddBezier(54, 74, 48, 73, 42, 75, 38, 77);
        pages.AddBezier(38, 77, 32, 75, 27, 73, 22, 74);
        pages.CloseFigure();
        g.FillPath(paper, pages);
        g.DrawPath(ink, pages);
        g.FillPolygon(pageShadow, [new PointF(36, 62), new(38, 63), new(40, 62), new(39, 76), new(38, 77), new(36, 76)]);
        for (int y = 63; y <= 70; y += 4)
        {
            g.DrawBezier(ink, 25, y, 28, y, 31, y + 1, 34, y + 2);
            g.DrawBezier(ink, 42, y + 2, 45, y + 1, 49, y, 51, y);
        }
        using var bookmark = new SolidBrush(Color.FromArgb(194, 105, 91));
        g.FillPolygon(bookmark, [new PointF(46, 75), new(49, 74), new(49, 80), new(47.5f, 78.5f), new(46, 81)]);
        // A curved leaf lifts and settles, after time to actually read the page.
        double turn = (seconds + 2) % 6;
        if (turn > 4.6)
        {
            double progress = (turn - 4.6) / 1.4;
            float leafX = 38 + 16 * (float)Math.Cos(progress * Math.PI);
            float lift = 5 * (float)Math.Sin(progress * Math.PI);
            using var leaf = new GraphicsPath();
            leaf.AddBezier(38, 62, 39, 59 - lift, leafX, 57 - lift, leafX, 58);
            leaf.AddLine(leafX, 58, leafX, 74);
            leaf.AddBezier(leafX, 74, leafX, 72 - lift, 39, 74 - lift, 38, 77);
            leaf.CloseFigure();
            g.FillPath(paper, leaf);
            g.DrawPath(ink, leaf);
        }
        using var hand = new SolidBrush(Color.FromArgb(105, 170, 107));
        g.FillEllipse(hand, 18, 65, 6, 8); g.DrawArc(edge, 18, 65, 6, 8, 85, 240);
        g.FillEllipse(hand, 52, 65, 6, 8); g.DrawArc(edge, 52, 65, 6, 8, -145, 240);
        g.Restore(saved);
    }
}
