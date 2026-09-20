using System.Drawing.Drawing2D;
using WinPet.Domain;

namespace WinPet.Pets.Cactus;

// Draws resolved facial channels, independently of activity artwork.
internal static class CactusFace
{
    public static void Paint(Graphics g, FacePose face, double activitySeconds, Pen outline, Brush dark, Brush pink)
    {
        var eyes = face.Eyes;
        float eyeX = eyes.X, eyeY = eyes.Y;
        bool closed = eyes.Openness < .08f;
        if (closed)
        {
            g.DrawArc(outline, 30, 42, 6, 3, 0, 170);
            g.DrawArc(outline, 41, 42, 6, 3, 0, 170);
        }
        else
        {
            float height = eyes.Height * eyes.Openness;
            float lidOffset = (eyes.Height - height) / 2;
            g.FillEllipse(dark, 32 + eyeX, 41 + eyeY + lidOffset, 3, height);
            g.FillEllipse(dark, 43 + eyeX, 41 + eyeY + lidOffset, 3, height);
        }
        if (face.Blush) {
            g.FillEllipse(pink, 28, 47, 6, 3); g.FillEllipse(pink, 46, 47, 5, 3);
        }
        if (face.Mouth == MouthShape.Surprised) g.DrawEllipse(outline, 37, 48, 4, 5);
        else if (face.Mouth == MouthShape.Whistle)
        {
            // The smile puckers into rounded whistling lips, with a gentle breath pulse.
            float p = face.MouthAmount;
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
    }
}
