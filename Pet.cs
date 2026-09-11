using System.Drawing.Drawing2D;

namespace WinPet;

internal enum Mood { Idle, Walk, Look, Sit, Sleep, React, Dragged, Falling, Read, Think, Sing }

// New pets supply artwork and dimensions; movement and desktop rules are shared.
internal interface IPet
{
    string Name { get; }
    Size Size { get; }
    IReadOnlyList<PetRoutine> Routines => PetRoutine.Default;
    PointF GazeOrigin => new(Size.Width / 2f, Size.Height / 2f);
    void Paint(Graphics graphics, Mood mood, double seconds, int facing, PetGaze gaze = default, RoutinePose? routine = null);
}

internal sealed class CactusPet : IPet
{
    public string Name => "Cacti";
    public Size Size => new(76, 92);
    public PointF GazeOrigin => new(38, 43);
    private const double TransitionSeconds = RoutinePose.Entrance + RoutinePose.Exit;
    public IReadOnlyList<PetRoutine> Routines { get; } = Array.AsReadOnly<PetRoutine>([
        new(Mood.Walk, 50, 5, 10), new(Mood.Idle, 38, 6, 12), new(Mood.Sleep, 23, 16, 28),
        new(Mood.Look, 4, 4, 7), new(Mood.Sit, 3, 4, 8),
        // Give the settled activity its full duration, in addition to opening/putting away.
        new(Mood.Read, 2, 19.5 + TransitionSeconds, 28.6 + TransitionSeconds),
        new(Mood.Think, 2, 18.2 + TransitionSeconds, 23.4 + TransitionSeconds),
        new(Mood.Sing, 1, 11.7 + TransitionSeconds, 15.6 + TransitionSeconds)
    ]);

    public void Paint(Graphics g, Mood mood, double seconds, int facing, PetGaze gaze = default, RoutinePose? routine = null)
    {
        float engagement = routine?.Amount ?? 1;
        double activitySeconds = routine?.Seconds ?? seconds;
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
        float usualLook = mood == Mood.Look ? (float)Math.Sin(seconds * 1.4) * 2 : facing * 0.7f;
        float eyeX = usualLook * (1 - gaze.Attention) + gaze.X * 2.5f * gaze.Attention;
        float eyeY = gaze.Y * 2 * gaze.Attention;
        if (mood == Mood.Read) { eyeX += ((float)Math.Sin(activitySeconds * 1.8) * 1.2f - eyeX) * engagement; eyeY += (2 - eyeY) * engagement; }
        if (mood == Mood.Think) { eyeX += (1.5f - eyeX) * engagement; eyeY += (-1.5f - eyeY) * engagement; }
        bool closed = mood == Mood.Sleep || (mood == Mood.Sit && gaze.Attention < 0.1f) || seconds % 6.3 < 0.16;
        if (closed)
        {
            g.DrawArc(outline, 30, 42, 6, 3, 0, 170);
            g.DrawArc(outline, 41, 42, 6, 3, 0, 170);
        }
        else
        {
            g.FillEllipse(dark, 32 + eyeX, 41 + eyeY, 3, mood == Mood.Dragged ? 6 : 4);
            g.FillEllipse(dark, 43 + eyeX, 41 + eyeY, 3, mood == Mood.Dragged ? 6 : 4);
        }
        if (mood is Mood.React or Mood.Dragged) {
            g.FillEllipse(pink, 28, 47, 6, 3); g.FillEllipse(pink, 46, 47, 5, 3);
        }
        if (mood is Mood.Falling or Mood.Dragged) g.DrawEllipse(outline, 37, 48, 4, 5);
        else if (mood == Mood.Sing)
        {
            // The smile puckers into rounded whistling lips, with a gentle breath pulse.
            float p = engagement;
            float breath = (float)Math.Sin(activitySeconds * 4) * .5f * p;
            using var lips = new Pen(outline.Color, 1.7f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            g.DrawBezier(lips, new PointF(36 + p, 48 - p),
                new PointF(37 + 6 * p + breath, 52 - 3 * p),
                new PointF(41 + 2 * p + breath, 52 - 3 * p), new PointF(42 - 5 * p, 48 + 3 * p));
        }
        else
        {
            using var smile = new Pen(outline.Color, 1.7f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            g.DrawBezier(smile, 36, 48, 37, 52, 41, 52, 42, 48);
        }
        PointF[] potShape = [new(24, 69), new(53, 69), new(48, 86), new(29, 86)];
        g.FillPolygon(pot, potShape); g.DrawPolygon(outline, potShape);
        g.FillRectangle(rim, 21, 63, 35, 7); g.DrawRectangle(outline, 21, 63, 35, 7);
        using var detail = new Pen(Color.FromArgb(243, 181, 139), 2);
        g.DrawLine(detail, 29, 74, 31, 82);
        if (mood == Mood.React) g.DrawArc(detail, 35, 73, 10, 8, 0, 180);
        if (RoutinePose.IsActivity(mood) && engagement > 0)
        {
            using var props = new Bitmap(Size.Width, Size.Height);
            using (var pg = Graphics.FromImage(props))
            {
                pg.SmoothingMode = SmoothingMode.AntiAlias;
                if (mood == Mood.Read)
                {
                    pg.TranslateTransform(38, 72 + 8 * (1 - engagement));
                    pg.ScaleTransform(.18f + .82f * engagement, .8f + .2f * engagement);
                    pg.TranslateTransform(-38, -72);
                }
                PaintRoutine(pg, mood, activitySeconds, outline);
            }
            using var attributes = new System.Drawing.Imaging.ImageAttributes();
            attributes.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix { Matrix33 = engagement });
            g.DrawImage(props, new Rectangle(Point.Empty, Size), 0, 0, Size.Width, Size.Height, GraphicsUnit.Pixel, attributes);
        }
    }

    private static void PaintRoutine(Graphics g, Mood mood, double seconds, Pen outline)
    {
        if (mood == Mood.Read)
        {
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
        }
        else if (mood == Mood.Think)
        {
            g.FillEllipse(Brushes.Ivory, 53, 3, 19, 15);
            g.DrawEllipse(outline, 53, 3, 19, 15);
            g.FillEllipse(Brushes.Ivory, 55, 19, 4, 4);
            g.FillEllipse(Brushes.Ivory, 53, 25, 3, 3);
            int dots = 1 + (int)(seconds % 3);
            float dotsLeft = 62.5f - ((dots - 1) * 5 + 2) / 2f;
            for (int i = 0; i < dots; i++) g.FillEllipse(Brushes.DarkSlateGray, dotsLeft + i * 5, 9.5f, 2, 2);
        }
        else if (mood == Mood.Sing)
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

}

