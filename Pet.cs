using System.Drawing.Drawing2D;

namespace WinPet;

internal enum Mood { Idle, Walk, Look, Sit, Sleep, React, Dragged, Falling }

// New pets supply artwork and dimensions; movement and desktop rules are shared.
internal interface IPet
{
    string Name { get; }
    Size Size { get; }
    GraphicsPath Silhouette();
    void Paint(Graphics graphics, Mood mood, double seconds, int facing);
}

internal sealed class CactusPet : IPet
{
    public string Name => "Cacti";
    public Size Size => new(76, 92);

    public GraphicsPath Silhouette()
    {
        var path = new GraphicsPath(FillMode.Winding);
        path.AddRoundedRectangle(new RectangleF(24, 9, 29, 58), new SizeF(14, 14));
        path.AddRoundedRectangle(new RectangleF(10, 30, 12, 29), new SizeF(6, 6));
        path.AddRectangle(new RectangleF(16, 48, 16, 11));
        path.AddRoundedRectangle(new RectangleF(57, 22, 11, 30), new SizeF(5, 5));
        path.AddRectangle(new RectangleF(48, 41, 14, 11));
        path.AddEllipse(39, 4, 15, 13);
        path.AddRectangle(new RectangleF(20, 62, 37, 9));
        path.AddPolygon([new(23, 69), new(54, 69), new(49, 87), new(28, 87)]);
        path.AddEllipse(26, 84, 12, 7);
        path.AddEllipse(41, 84, 12, 7);
        return path;
    }

    public void Paint(Graphics g, Mood mood, double seconds, int facing)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var outline = new Pen(Color.FromArgb(36, 70, 53), 2);
        using var green = new SolidBrush(Color.FromArgb(105, 170, 107));
        using var light = new Pen(Color.FromArgb(164, 209, 139), 2);
        using var pot = new SolidBrush(Color.FromArgb(206, 127, 90));
        using var rim = new SolidBrush(Color.FromArgb(233, 163, 118));
        using var dark = new SolidBrush(Color.FromArgb(44, 65, 51));
        using var pink = new SolidBrush(Color.FromArgb(239, 145, 160));
        using var feet = new SolidBrush(Color.FromArgb(91, 75, 57));
        float stride = mood == Mood.Walk ? (float)Math.Sin(seconds * 12) * 2 : 0;
        g.FillEllipse(feet, 27, 84 + Math.Max(0, stride), 10, 5);
        g.FillEllipse(feet, 42, 84 + Math.Max(0, -stride), 10, 5);
        using var body = new GraphicsPath(FillMode.Winding);
        body.AddBezier(25, 24, 25, 6, 52, 6, 52, 24);
        body.AddLine(52, 24, 52, 42); body.AddLine(52, 42, 58, 42);
        body.AddLine(58, 42, 58, 28); body.AddBezier(58, 28, 58, 21, 67, 21, 67, 28);
        body.AddLine(67, 28, 67, 46); body.AddBezier(67, 46, 67, 51, 63, 51, 60, 51);
        body.AddLine(60, 51, 52, 51); body.AddLine(52, 51, 52, 65);
        body.AddLine(52, 65, 25, 65); body.AddLine(25, 65, 25, 58);
        body.AddLine(25, 58, 16, 58); body.AddBezier(16, 58, 11, 58, 11, 54, 11, 50);
        body.AddLine(11, 50, 11, 36); body.AddBezier(11, 36, 11, 29, 21, 29, 21, 36);
        body.AddLine(21, 36, 21, 48); body.AddLine(21, 48, 25, 48); body.CloseFigure();
        g.FillPath(green, body);
        g.DrawPath(outline, body);
        g.DrawLine(light, 31, 21, 31, 38);
        g.DrawLine(light, 46, 20, 46, 37);
        g.DrawLine(light, 32, 54, 32, 62);
        g.DrawLine(light, 45, 54, 45, 62);
        g.FillEllipse(pink, 40, 5, 7, 7);
        g.FillEllipse(pink, 46, 5, 7, 7);
        g.FillEllipse(pink, 43, 9, 7, 7);
        g.FillEllipse(Brushes.Wheat, 44, 8, 4, 4);
        float gaze = mood == Mood.Look ? (float)Math.Sin(seconds * 1.4) * 2 : facing * 0.7f;
        bool closed = mood is Mood.Sleep or Mood.Sit || seconds % 6.3 < 0.16;
        if (closed)
        {
            g.DrawArc(outline, 30, 42, 6, 3, 0, 170);
            g.DrawArc(outline, 41, 42, 6, 3, 0, 170);
        }
        else
        {
            g.FillEllipse(dark, 32 + gaze, 41, 3, mood == Mood.Dragged ? 6 : 4);
            g.FillEllipse(dark, 43 + gaze, 41, 3, mood == Mood.Dragged ? 6 : 4);
        }
        if (mood is Mood.React or Mood.Dragged) {
            g.FillEllipse(pink, 28, 47, 6, 3); g.FillEllipse(pink, 46, 47, 5, 3);
        }
        if (mood is Mood.Falling or Mood.Dragged) g.DrawEllipse(outline, 37, 48, 4, 5);
        else g.DrawArc(outline, 36, 46, 6, 5, 5, 165);
        PointF[] potShape = [new(24, 69), new(53, 69), new(48, 86), new(29, 86)];
        g.FillPolygon(pot, potShape); g.DrawPolygon(outline, potShape);
        g.FillRectangle(rim, 21, 63, 35, 7); g.DrawRectangle(outline, 21, 63, 35, 7);
        using var detail = new Pen(Color.FromArgb(243, 181, 139), 2);
        g.DrawLine(detail, 29, 74, 31, 82);
        if (mood == Mood.React) g.DrawArc(detail, 35, 73, 10, 8, 0, 180);
    }
}
