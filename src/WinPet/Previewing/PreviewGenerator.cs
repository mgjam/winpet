using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using WinPet.Domain;
using WinPet.Rendering;

namespace WinPet.Previewing;

// Render any pet's supplied scene catalog using the production composer and renderer.
// Explicit sample times and cursor paths keep builds repeatable.
internal static class PreviewGenerator
{
    internal const int Fps = 20;
    internal const int Frames = 160;
    private static readonly Color Background = Color.FromArgb(245, 242, 232);

    public static void Generate(string directory, IPetVisual pet, IReadOnlyList<PreviewScene> scenes)
    {
        Directory.CreateDirectory(directory);
        using var sheet = new Bitmap(4 * 240, ((scenes.Count + 3) / 4) * 250);
        using var sg = Graphics.FromImage(sheet);
        using var titleFont = new Font("Segoe UI", 10);
        sg.Clear(Background);
        for (int index = 0; index < scenes.Count; index++)
        {
            var scene = scenes[index];
            var gaze = default(CursorAttention);
            using var gif = new GifWriter(Path.Combine(directory, scene.Id + ".gif"));
            for (int i = 0; i < Frames; i++)
            {
                double seconds = i / (double)Fps;
                PointF cursor = CursorAt(seconds, pet.AttentionOrigin);
                if (scene.TrackCursor)
                    gaze = gaze.Approach(CursorAttention.Toward(pet.AttentionOrigin, cursor), 1f / Fps);
                double previewDuration = scene.Action?.Duration ?? 7;
                var intent = scene.Transition && seconds >= previewDuration ? PetIntent.Idle : scene.Intent;
                var activity = scene.Action is not null && seconds < scene.Action.Duration
                    ? scene.Action.Activity : (scene.Transition && seconds >= previewDuration ? null : scene.Activity);
                RoutinePose? routine = activity is not null
                    ? scene.Transition ? new(seconds, previewDuration) : new(2 + seconds, 30) : null;
                using var frame = PetRenderer.Frame(pet, intent, seconds, 1, gaze, routine, activity);
                using var display = Display(frame, scene.TrackCursor ? cursor : null, pet.AttentionOrigin);
                gif.AddFrame(display, 100 / Fps);
                if (i == 40)
                {
                    frame.Save(Path.Combine(directory, scene.Id + ".png"), ImageFormat.Png);
                    int x = index % 4 * 240, y = index / 4 * 250;
                    sg.DrawImageUnscaled(display, x, y);
                    sg.DrawString(scene.Title, titleFont, Brushes.DarkSlateGray,
                        new RectangleF(x + 8, y + 212, 225, 38));
                }
            }
        }
        sheet.Save(Path.Combine(directory, "overview.png"), ImageFormat.Png);
        foreach (var action in pet.Actions) GenerateActionSequence(directory, pet, action);
        PreviewGallery.Write(directory, pet, scenes);
        Console.WriteLine($"Generated {scenes.Count} PNG/GIF pairs and gallery in {directory}");
    }

    private static void GenerateActionSequence(string directory, IPetVisual pet, PetAction action)
    {
        using var strip = new Bitmap(4 * 240, 2 * 240);
        using var g = Graphics.FromImage(strip);
        using var font = new Font("Segoe UI", 10);
        g.Clear(Background);
        for (int i = 0; i < 8; i++)
        {
            double t = i * action.Duration / 7;
            using var frame = PetRenderer.Frame(pet, PetIntent.Idle, t, 1, default, new(t, action.Duration), action.Activity);
            using var display = Display(frame, null, pet.AttentionOrigin);
            int x = i % 4 * 240, y = i / 4 * 240;
            g.DrawImageUnscaled(display, x, y);
            g.DrawString($"{action.Label} · {t:0.0}s", font, Brushes.DarkSlateGray, x + 16, y + 211);
        }
        strip.Save(Path.Combine(directory, action.Id + "-sequence.png"), ImageFormat.Png);
    }

    private static PointF CursorAt(double seconds, PointF origin)
    {
        if (seconds >= 6) return new(500, 500);
        double angle = Math.PI + seconds * Math.PI / 3;
        return new(origin.X + (float)Math.Cos(angle) * 100, origin.Y + (float)Math.Sin(angle) * 100);
    }

    private static Bitmap Display(Bitmap frame, PointF? cursor, PointF eyes)
    {
        var display = new Bitmap(240, 208);
        using var g = Graphics.FromImage(display);
        g.Clear(Background);
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        float scale = Math.Min(2, Math.Min((display.Width - 20f) / frame.Width, (display.Height - 20f) / frame.Height));
        int width = (int)(frame.Width * scale), height = (int)(frame.Height * scale);
        g.DrawImage(frame, new Rectangle((display.Width - width) / 2, 10, width, height));
        if (cursor is { } point && point.X < 300)
        {
            // Marker shows cursor direction, compressed to fit the review card.
            float x = 120 + (point.X - eyes.X) * .95f;
            float y = 96 + (point.Y - eyes.Y) * .85f;
            using var marker = new Pen(Color.FromArgb(188, 103, 78), 2);
            g.DrawEllipse(marker, x - 3, y - 3, 6, 6);
        }
        return display;
    }

}
