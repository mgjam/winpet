using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Net;
using System.Text;

namespace WinPet;

// This catalog is the review surface for the pet. Every sample uses the production
// composer and renderer. Explicit sample times/cursor paths keep builds repeatable.
internal static class PreviewGenerator
{
    private sealed record Scene(string Id, string Group, string Title, string Description,
        Mood Intent, bool TrackCursor = false, bool Transition = false);

    private static readonly Scene[] Scenes = [
        new("on-ground", "Physical states", "On ground", "Supported, standing quietly.", Mood.Idle),
        new("in-air", "Physical states", "In air", "Airborne expression; blinking continues. Midair pokes keep this expression.", Mood.Falling),
        new("held", "Physical states", "Being held", "Picked up: wide eyes and blush, with independent blinking.", Mood.Dragged),
        new("standing", "Activities", "Standing", "Idle gaze and natural blinking.", Mood.Idle),
        new("walking", "Activities", "Walking", "Foot stride and blinking run independently.", Mood.Walk),
        new("sleeping", "Activities", "Sleeping", "Eyes stay closed, even with a nearby cursor.", Mood.Sleep, true),
        new("reading", "Activities", "Reading", "Open the book, read and turn pages, then put it away. Review clip uses a shortened hold.", Mood.Read, false, true),
        new("thinking", "Activities", "Thinking", "Thought bubble with centered dots; blink continues. Review clip uses a shortened hold.", Mood.Think, false, true),
        new("singing", "Activities", "Singing / whistling", "Puckered lips and silent notes. Review clip uses a shortened hold.", Mood.Sing, false, true),
        new("laptop", "Activities", "Writing on laptop", "Alternating typing taps and a short thinking pause. Settled writing lasts 20–28 seconds; this review shortens the hold.", Mood.Laptop, false, true),
        new("violin", "Activities", "Playing violin", "Warm wooden violin and back-and-forth bowing (silent). Settled playing lasts 18–25 seconds; this review shortens the hold.", Mood.Violin, false, true),
        new("poked", "Activities", "Being poked", "Grounded click reaction: blush and smile.", Mood.React),
        new("looking", "Activities", "Looking around", "Autonomous side-to-side gaze.", Mood.Look),
        new("sitting", "Activities", "Sitting", "Resting awake; eyelids still blink.", Mood.Sit),
        new("blinking", "Shared extras", "Blinking", "Continuous close, brief hold, reopen; independent of activity time.", Mood.Idle),
        new("cursor-tracking", "Shared extras", "Cursor tracking", "Marker moves left, up, right, down, then away; eyes ease back to their default.", Mood.Idle, true),
        new("thinking-attention", "Combinations", "Thinking + attention + blink", "Bubble, cursor gaze and eyelids coexist.", Mood.Think, true),
        new("reading-attention", "Combinations", "Reading + attention + blink", "Glances toward cursor with a slight bookward bias.", Mood.Read, true),
        new("singing-attention", "Combinations", "Whistling + attention + blink", "Mouth and notes continue while eyes track and blink.", Mood.Sing, true),
        new("laptop-attention", "Combinations", "Laptop + attention + blink", "Typing continues while eyes follow the cursor and blink.", Mood.Laptop, true),
        new("violin-attention", "Combinations", "Violin + attention + blink", "Bowing continues while eyes follow the cursor and blink.", Mood.Violin, true)
    ];

    private const int Fps = 20;
    private const int Frames = 160;
    private static readonly Color Background = Color.FromArgb(245, 242, 232);

    public static void Generate(string directory)
    {
        Directory.CreateDirectory(directory);
        IPet pet = new CactusPet();
        using var sheet = new Bitmap(4 * 240, ((Scenes.Length + 3) / 4) * 250);
        using var sg = Graphics.FromImage(sheet);
        using var titleFont = new Font("Segoe UI", 10);
        sg.Clear(Background);
        for (int index = 0; index < Scenes.Length; index++)
        {
            var scene = Scenes[index];
            var gaze = default(PetGaze);
            using var gif = new GifWriter(Path.Combine(directory, scene.Id + ".gif"));
            for (int i = 0; i < Frames; i++)
            {
                double seconds = i / (double)Fps;
                PointF cursor = CursorAt(seconds);
                if (scene.TrackCursor)
                    gaze = gaze.Approach(PetGaze.Toward(pet.GazeOrigin, cursor, scene.Intent), 1f / Fps);
                var intent = scene.Transition && seconds >= 7 ? Mood.Idle : scene.Intent;
                RoutinePose? routine = RoutinePose.IsActivity(intent)
                    ? scene.Transition ? new(seconds, 7) : new(2 + seconds, 30) : null;
                using var frame = PetRenderer.Frame(pet, intent, seconds, 1, gaze, routine);
                using var display = Display(frame, scene.TrackCursor ? cursor : null, pet.GazeOrigin);
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
        WriteGallery(directory);
        PreviewChecks.Verify(directory, Scenes.Select(s => s.Id), Frames, 100 / Fps);
        Console.WriteLine($"Generated {Scenes.Length} PNG/GIF pairs and gallery in {directory}");
    }

    private static PointF CursorAt(double seconds)
    {
        if (seconds >= 6) return new(500, 500);
        double angle = Math.PI + seconds * Math.PI / 3;
        return new(38 + (float)Math.Cos(angle) * 100, 43 + (float)Math.Sin(angle) * 100);
    }

    private static Bitmap Display(Bitmap frame, PointF? cursor, PointF eyes)
    {
        var display = new Bitmap(240, 208);
        using var g = Graphics.FromImage(display);
        g.Clear(Background);
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        g.DrawImage(frame, new Rectangle(44, 10, 152, 184));
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

    private static void WriteGallery(string directory)
    {
        var html = new StringBuilder("""
            <!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width">
            <title>Cacti visual reference</title>
            <style>body{font:16px system-ui;margin:32px auto;padding:0 24px;max-width:1100px;color:#244635;background:#faf8f1}h1{margin-bottom:8px}p{line-height:1.6}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(245px,1fr));gap:20px}article{background:#f5f2e8;border:1px solid #d9dfd0;border-radius:12px;padding:16px}img{display:block;margin:auto;max-width:100%}h3{margin:12px 0 4px}article p{font-size:14px}a{color:#356e69}button{font:inherit;padding:8px 14px;margin-right:8px;cursor:pointer}nav{position:sticky;top:0;background:#faf8f1;padding:12px 0}</style>
            <h1>Cacti visual reference</h1><p>Generated from the production renderer during <code>dotnet build</code>. PNGs are native 76 × 92 pixels; animations show 2× artwork at real-time speed, 20 fps. The ring indicates cursor direction.</p>
            <p>Physical state takes priority over activities. Blinking and cursor attention are independent extras. Clips hold the pet in place to review artwork; they do not simulate desktop collisions. Activity transition clips shorten the settled hold to fit an 8-second loop; production durations are unchanged.</p>
            <nav><button onclick="setAnimated(true)">Play animations</button><button onclick="setAnimated(false)">Show stills</button><a href="overview.png">Overview sheet</a></nav>
            """);
        var md = new StringBuilder("# Cacti visual reference\n\nGenerated automatically by `dotnet build`, or `tools\\generate-previews.cmd`. Open [the gallery](index.html) for animated cards and still/animation controls. These files belong in source control; regenerate and review them with artwork changes. Do not edit them manually.\n\nPNGs: native 76 × 92, transparent. GIFs: 2× artwork on cream, 20 fps, eight-second loops. The ring represents cursor direction. Previews show artwork in place, not a desktop physics simulation. Activity transition clips use a shortened hold; production durations are unchanged.\n\n![Overview](overview.png)\n");
        foreach (var group in Scenes.GroupBy(s => s.Group))
        {
            html.Append($"<h2>{group.Key}</h2><div class=grid>");
            md.Append($"\n## {group.Key}\n\n");
            foreach (var scene in group)
            {
                html.Append($"<article><img width=240 height=208 style=object-fit:contain data-id='{scene.Id}' src='{scene.Id}.gif' alt='{WebUtility.HtmlEncode(scene.Title)}'><h3>{scene.Title}</h3><p>{scene.Description}</p><a href='{scene.Id}.png'>PNG</a> · <a href='{scene.Id}.gif'>GIF</a></article>");
                md.Append($"- **{scene.Title}** — {scene.Description} [PNG]({scene.Id}.png) · [GIF]({scene.Id}.gif)\n");
            }
            html.Append("</div>");
        }
        html.Append("<script>function setAnimated(on){document.querySelectorAll('img[data-id]').forEach(img=>{img.src=img.dataset.id+(on?'.gif':'.png');img.style.imageRendering=on?'auto':'pixelated';})}</script></html>");
        File.WriteAllText(Path.Combine(directory, "index.html"), html.ToString());
        File.WriteAllText(Path.Combine(directory, "README.md"), md.ToString());
    }
}
