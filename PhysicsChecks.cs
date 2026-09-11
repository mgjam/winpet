namespace WinPet;

internal static class PhysicsChecks
{
    public static int Run()
    {
        var results = new List<string>();
        void Check(bool condition, string name) { results.Add($"{(condition ? "PASS" : "FAIL")} {name}"); }
        AnimationChecks.Run(Check);
        var routinePet = new CactusPet();
        var random = new Random(42);
        var seen = new HashSet<Mood>();
        Mood previous = Mood.Idle;
        bool validRoutines = true;
        for (int i = 0; i < 1000; i++)
        {
            var selected = PetRoutine.Choose(routinePet.Routines, previous, random);
            validRoutines &= selected.Mood != previous && routinePet.Routines.Any(r =>
                r.Mood == selected.Mood && selected.Duration >= r.MinSeconds && selected.Duration <= r.MaxSeconds);
            seen.Add(selected.Mood);
            previous = selected.Mood;
        }
        Check(validRoutines, "Autonomous routines respect pet durations and avoid immediate repeats");
        Check(new[] { Mood.Read, Mood.Think, Mood.Sing }.All(seen.Contains), "Cacti naturally reaches all three personal routines");
        Check(PetRoutine.Choose([], Mood.Read, random).Mood == Mood.Idle, "Pets without routines safely idle");
        Check(PetRoutine.Choose([new(Mood.Look, 1, 2, 3)], Mood.Look, random).Mood == Mood.Look,
            "A pet can define just one routine");
        Check(PetRoutine.Choose([new(Mood.Falling, 1, 2, 3)], Mood.Idle, random).Mood == Mood.Idle,
            "Autonomous profiles cannot select physics or interaction states");
        double totalTime = 0, coreTime = 0, activityTime = 0;
        previous = Mood.Idle;
        for (int i = 0; i < 100000; i++)
        {
            var choice = RoutinePose.IsActivity(previous) ? (Mood: Mood.Idle, Duration: 2.5)
                : PetRoutine.Choose(routinePet.Routines, previous, random);
            totalTime += choice.Duration;
            if (choice.Mood is Mood.Walk or Mood.Idle or Mood.Sleep) coreTime += choice.Duration;
            if (RoutinePose.IsActivity(choice.Mood)) activityTime += choice.Duration;
            previous = choice.Mood;
        }
        Check(coreTime / totalTime > .85 && activityTime / totalTime < .15,
            $"Ordinary life dominates by time: core {coreTime / totalTime:P1}, activities {activityTime / totalTime:P1}");
        Check(new RoutinePose(0, 8).Amount == 0 && new RoutinePose(1, 8).Amount == 1 &&
            new RoutinePose(6.8, 8).Amount == 1 && new RoutinePose(8, 8).Amount == 0,
            "Activities ease in from idle and finish in idle before the next mood");
        using (var film = new Bitmap(76 * 120, 92 * 3))
        using (var fg = Graphics.FromImage(film))
        {
            Mood[] activities = [Mood.Read, Mood.Think, Mood.Sing];
            bool contained = true;
            for (int row = 0; row < activities.Length; row++)
            for (int frameIndex = 0; frameIndex < 120; frameIndex++)
            {
                double t = frameIndex / 12.0;
                using var frame = PetRenderer.Frame(routinePet, t < 8 ? activities[row] : Mood.Idle,
                    t + 2, 1, default, t < 8 ? new RoutinePose(t, 8) : null);
                for (int x = 0; x < 76; x++) contained &= frame.GetPixel(x, 0).A == 0 && frame.GetPixel(x, 91).A == 0;
                for (int y = 0; y < 92; y++) contained &= frame.GetPixel(0, y).A == 0 && frame.GetPixel(75, y).A == 0;
                fg.DrawImageUnscaled(frame, frameIndex * 76, row * 92);
            }
            Check(contained, "Every entrance, page turn, floating note and exit stays inside the pet footprint");
            film.Save(Path.Combine(AppContext.BaseDirectory, "routine-film.png"));
        }
        using (var preview = new Bitmap(456, 140))
        using (var graphics = Graphics.FromImage(preview))
        {
            graphics.Clear(Color.FromArgb(245, 242, 232));
            using var font = new Font("Segoe UI", 8);
            Mood[] routines = [Mood.Read, Mood.Read, Mood.Think, Mood.Think, Mood.Sing, Mood.Sing];
            for (int i = 0; i < routines.Length; i++)
            {
                double seconds = i % 2 == 0 ? 2.3 : .6;
                using var frame = PetRenderer.Frame(routinePet, routines[i], seconds, 1);
                bool clearBorder = true;
                for (int x = 0; x < frame.Width; x++) clearBorder &= frame.GetPixel(x, 0).A == 0 && frame.GetPixel(x, frame.Height - 1).A == 0;
                for (int y = 0; y < frame.Height; y++) clearBorder &= frame.GetPixel(0, y).A == 0 && frame.GetPixel(frame.Width - 1, y).A == 0;
                Check(clearBorder, $"{routines[i]} frame {i % 2} stays within the collision footprint");
                graphics.DrawImageUnscaled(frame, i * 76, 8);
                graphics.DrawString(routines[i].ToString(), font, Brushes.DarkSlateGray, i * 76 + 20, 108);
            }
            preview.Save(Path.Combine(AppContext.BaseDirectory, "routines-preview.png"));
        }
        using (var transparentFrame = PetRenderer.Frame(new CactusPet(), Mood.Idle, 2, 1))
        {
            Check(transparentFrame.GetPixel(38, 9).A == 0, "No green background above cactus outline");
            Check(transparentFrame.GetPixel(0, 0).A == 0, "Empty pixels remain transparent for click-through");
            Check(transparentFrame.GetPixel(38, 30).A == 255, "Cactus body remains opaque");
            transparentFrame.Save(Path.Combine(AppContext.BaseDirectory, "clean-edges.png"));
        }
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
        float throwX = 600, throwY = 100;
        var rebound = PetMotion.Move(empty, new(770, 100), size, ref throwX, ref throwY, .035f, true, out bool bounced);
        Check(bounced && throwX < 0 && Math.Abs(throwX) < 600 && throwY == 100 && empty.Fits(rebound, size), "Right screen bounce reverses and softens horizontal velocity");
        throwX = -600; throwY = -100;
        rebound = PetMotion.Move(empty, new(2, 100), size, ref throwX, ref throwY, .035f, true, out bounced);
        Check(bounced && throwX > 0 && throwY == -100 && empty.Fits(rebound, size), "Left screen bounce preserves upward motion");
        throwX = 500; throwY = 100;
        rebound = PetMotion.Move(world, new(175, 300), size, ref throwX, ref throwY, .035f, true, out bounced);
        Check(bounced && throwX < 0 && world.Fits(rebound, size), "Thrown pet rebounds off a window side without entering it");
        throwX = -500; throwY = 100;
        rebound = PetMotion.Move(world, new(505, 300), size, ref throwX, ref throwY, .035f, true, out bounced);
        Check(bounced && throwX > 0 && world.Fits(rebound, size), "Thrown pet rebounds off the opposite window side");
        throwX = 25; throwY = 0;
        PetMotion.Move(empty, new(780, 570), size, ref throwX, ref throwY, .035f, false, out bounced);
        Check(bounced && throwX == 0 && throwY == 0, "Walking into a side still stops instead of bouncing");
        throwX = 50; throwY = 800;
        rebound = PetMotion.Move(world, new(250, 210), size, ref throwX, ref throwY, .035f, true, out _);
        Check(throwY == 0 && world.Supported(rebound, size), "Thrown pet still lands on window tops");
        var bump = PetMotion.Bump(new(240, 800));
        Check(bump.X == 240 && bump.Y < 0, "Midair tap reverses a fast fall and preserves sideways momentum");
        var repeatedBump = PetMotion.Bump(bump);
        Check(repeatedBump == bump, "Repeated taps do not stack unlimited upward speed");
        throwX = bump.X; throwY = bump.Y;
        rebound = PetMotion.Move(empty, new(100, 2), size, ref throwX, ref throwY, .035f, true, out _);
        Check(throwY == 0 && empty.Fits(rebound, size), "Bumped pet cannot escape through the top screen edge");
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
        using (var preview = new Bitmap(456, 284))
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
                cactus.Paint(graphics, PetAnimation.Compose(moods[i], 2, 1));
                graphics.DrawString(moods[i].ToString(), labelFont, Brushes.DarkSlateGray, 12, 103);
            }
            PetGaze[] looks = [new(-1, 0, 1), new(0, -1, 1), new(1, 0, 1), new(0, 1, 1), new(0, 0, 1), default];
            string[] labels = ["Look left", "Look up", "Look right", "Look down", "Near center", "Far away"];
            for (int i = 0; i < looks.Length; i++)
            {
                graphics.ResetTransform();
                graphics.TranslateTransform(i * 76, 148);
                cactus.Paint(graphics, PetAnimation.Compose(Mood.Idle, 2, 1, looks[i]));
                graphics.DrawString(labels[i], labelFont, Brushes.DarkSlateGray, 7, 103);
            }
            preview.Save(Path.Combine(AppContext.BaseDirectory, "cactus-preview.png"));
        }
        string path = Path.Combine(AppContext.BaseDirectory, "self-test-results.txt");
        File.WriteAllLines(path, results);
        return results.Any(r => r.StartsWith("FAIL")) ? 1 : 0;
    }
}
