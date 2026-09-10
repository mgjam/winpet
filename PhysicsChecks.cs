namespace WinPet;

internal static class PhysicsChecks
{
    public static int Run()
    {
        var results = new List<string>();
        void Check(bool condition, string name) { results.Add($"{(condition ? "PASS" : "FAIL")} {name}"); }
        Check(Native.IsFullyTransparent(true, 0, 2), "Invisible alpha-zero NVIDIA-style overlay is excluded");
        Check(!Native.IsFullyTransparent(true, 255, 2), "Opaque layered app remains an obstacle");
        Check(!Native.IsFullyTransparent(true, 128, 2), "Partly transparent app remains an obstacle");
        Check(!Native.IsFullyTransparent(false, 0, 0), "Missing transparency attributes do not hide real apps");
        Check(!Native.IsFullyTransparent(true, 0, 1), "Color-key transparency alone does not exclude the whole app");
        Check(!Native.IsPhysicalObstacle(0, false, false), "Other workspace excluded even before cloaking updates");
        Check(!Native.IsPhysicalObstacle(2, true, false), "Cloaked window excluded during workspace transition");
        Check(Native.IsPhysicalObstacle(0, true, false), "Visible current-workspace app remains solid");
        Check(Native.IsPhysicalObstacle(0, null, false), "Unavailable workspace API preserves ordinary obstacles");
        var size = new SizeF(20, 30);
        var world = new World([new(0, 0, 800, 600)], [new(200, 250, 300, 200)]);
        Check(!world.Fits(new(190, 230), size), "Whole body excluded from window interior");
        var landing = world.Move(new(250, 10), size, 0, 580, out _, out bool landed);
        Check(landed && Math.Abs(landing.Y - 220) < .01, "Fast fall lands on window top without tunneling");
        var side = world.Move(new(150, 280), size, 500, 0, out bool hitSide, out _);
        Check(hitSide && Math.Abs(side.X - 180) < .01, "Window side blocks horizontal movement");
        var underside = world.Move(new(250, 490), size, 0, -400, out _, out bool hitUnderside);
        Check(hitUnderside && Math.Abs(underside.Y - 450) < .01, "Window underside blocks upward throws");
        Check(world.Supported(landing, size), "Window top supplies stable support");
        var empty = new World(world.Areas, []);
        Check(!empty.Supported(landing, size), "Closing supporting window resumes gravity");
        var covered = new World(world.Areas, [new(0, 0, 800, 600)]);
        Check(covered.FindSpace(new(200, 200), size) == null, "Fully covered desktop hides pet");
        var safe = world.FindSpace(new(250, 300), size);
        Check(safe.HasValue && world.Fits(safe.Value, size), "Moving window overlap relocates safely");
        var monitors = new World([new(-800, 0, 800, 560), new(100, 0, 800, 560)], []);
        Check(monitors.Fits(new(-700, 530), size), "Negative monitor coordinates supported");
        Check(!monitors.Fits(new(-10, 100), size), "Monitor gap forbidden");
        Check(!monitors.Fits(new(200, 540), size), "Reserved taskbar area forbidden");
        Check(empty.FindSpace(new(250, 300), size).HasValue, "Pet recovers when desktop is exposed");
        var thin = new World(world.Areas, [new(250, 200, 1, 300)]);
        var blocked = thin.Move(new(50, 250), size, 650, 0, out bool thinHit, out _);
        Check(thinHit && thin.Fits(blocked, size) && blocked.X <= 230, "One-pixel obstacle blocks fast motion");
        var narrow = new World([new(0, 0, 19, 600)], []);
        Check(narrow.FindSpace(PointF.Empty, size) == null, "Narrow desktop strip cannot fit the pet");
        var removedMonitor = empty.FindSpace(new(-700, 200), size);
        Check(removedMonitor.HasValue && empty.Fits(removedMonitor.Value, size), "Disconnected monitor relocates pet");
        using (var preview = new Bitmap(456, 152))
        using (var graphics = Graphics.FromImage(preview))
        {
            using var labelFont = new Font("Segoe UI", 8);
            graphics.Clear(Color.FromArgb(245, 242, 232));
            var cactus = new CactusPet();
            Mood[] moods = [Mood.Idle, Mood.Walk, Mood.Look, Mood.Sleep, Mood.React, Mood.Dragged];
            for (int i = 0; i < moods.Length; i++)
            {
                graphics.ResetTransform();
                graphics.TranslateTransform(i * 76, 16);
                cactus.Paint(graphics, moods[i], 2, 1);
                graphics.DrawString(moods[i].ToString(), labelFont, Brushes.DarkSlateGray, 12, 103);
            }
            preview.Save(Path.Combine(AppContext.BaseDirectory, "cactus-preview.png"));
        }
        string path = Path.Combine(AppContext.BaseDirectory, "self-test-results.txt");
        File.WriteAllLines(path, results);
        return results.Any(r => r.StartsWith("FAIL")) ? 1 : 0;
    }
}
