using System.Drawing.Drawing2D;
using WinPet.Domain;

namespace WinPet.Pets.Cactus;

// The user's can sits beside the pot, below the right branch. All geometry stays
// inside Cacti's existing bounds and clear of the eyes, mouth, and flower.
internal static class CactusWatering
{
    public static PetAction Action { get; } = new("water", "Water",
        new CactusActivity("water", Paint, _ => new(.45f, .65f)), 7);

    private static readonly PointF Pivot = new(64, 55);
    private static readonly PointF Spout = new(49, 53);

    private static void Paint(Graphics g, double seconds, float amount)
    {
        var saved = g.Save();
        try
        {
            g.TranslateTransform(-6, 7 + 4 * (1 - amount));
            g.TranslateTransform(Pivot.X, Pivot.Y);
            g.ScaleTransform(1.22f, 1.22f);
            g.TranslateTransform(-Pivot.X, -Pivot.Y);
            float pour = Ease((seconds - 1.1) / .8) * (1 - Ease((seconds - 4.8) / .8));
            float tilt = -14 * pour;
            using var edge = CactusPalette.Outline(1.8f);
            using var enamel = new SolidBrush(CactusPalette.PropTeal);
            using var light = new Pen(CactusPalette.PropHighlight, 1.5f)
            { StartCap = LineCap.Round, EndCap = LineCap.Round };

            if (pour > .05f) PaintWater(g, seconds, pour, tilt);

            var can = g.Save();
            g.TranslateTransform(Pivot.X, Pivot.Y);
            g.RotateTransform(tilt);
            g.TranslateTransform(-Pivot.X, -Pivot.Y);

            // Open round handle, drawn behind the rounded enamel body.
            using var handle = new GraphicsPath();
            handle.AddBezier(68, 56, 75, 50, 75, 67, 69, 67);
            using var handleEdge = CactusPalette.Outline(4);
            using var handleFill = new Pen(CactusPalette.PropTeal, 1.5f);
            g.DrawPath(handleEdge, handle);
            g.DrawPath(handleFill, handle);

            using var spout = new GraphicsPath();
            spout.AddBezier(61, 66, 55, 63, 53, 57, 49, 55);
            spout.AddLine(49, 55, 51, 52);
            spout.AddBezier(51, 52, 58, 55, 58, 58, 64, 60);
            spout.CloseFigure();
            g.FillPath(enamel, spout);
            g.DrawPath(edge, spout);

            using var body = new GraphicsPath();
            body.AddBezier(59, 56, 61, 54, 68, 54, 70, 56);
            body.AddLine(70, 56, 71, 69);
            body.AddBezier(71, 69, 70, 75, 59, 75, 58, 69);
            body.CloseFigure();
            g.FillPath(enamel, body);
            g.DrawPath(edge, body);
            g.DrawBezier(light, 61, 61, 60, 67, 61, 70, 64, 71);
            g.DrawBezier(edge, 59, 56, 62, 58, 67, 58, 70, 56);

            // Flared rose makes the silhouette read as a watering can at native size.
            PointF[] rose = [new(47, 52), new(50, 49), new(53, 53), new(50, 56)];
            g.FillPolygon(enamel, rose);
            g.DrawPolygon(edge, rose);
            g.DrawLine(light, 48.5f, 52, 50, 50.5f);
            g.Restore(can);
        }
        finally { g.Restore(saved); }
    }

    private static void PaintWater(Graphics g, double seconds, float pour, float tilt)
    {
        // Use the same pivot as the can so water always begins at the moving spout.
        using var transform = new Matrix();
        transform.RotateAt(tilt, Pivot);
        PointF[] points = [Spout];
        transform.TransformPoints(points);
        var origin = points[0];
        using var water = new SolidBrush(Color.FromArgb((int)(220 * pour), CactusPalette.Water));
        using var glint = new Pen(Color.FromArgb((int)(180 * pour), CactusPalette.PropHighlight), .8f);
        for (int i = 0; i < 3; i++)
        {
            float phase = (float)((seconds * 1.7 + i / 3.0) % 1);
            float x = origin.X + (48 - origin.X) * phase + (i - 1) * .8f;
            float y = origin.Y + (59 - origin.Y) * phase * phase;
            using var drop = new GraphicsPath();
            drop.AddBezier(x, y - 1.8f, x - 2, y, x - 1.6f, y + 1.8f, x, y + 1.8f);
            drop.AddBezier(x, y + 1.8f, x + 1.6f, y + 1.8f, x + 2, y, x, y - 1.8f);
            g.FillPath(water, drop);
            g.DrawLine(glint, x - .4f, y, x - .4f, y + .6f);
        }
    }

    private static float Ease(double value)
    {
        float t = (float)Math.Clamp(value, 0, 1);
        return t * t * (3 - 2 * t);
    }
}
