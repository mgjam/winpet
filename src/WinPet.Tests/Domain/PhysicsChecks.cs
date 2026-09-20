using WinPet.Domain;

namespace WinPet.Tests.Domain;

internal static class PhysicsChecks
{
    public static void Run(Action<bool, string> check)
    {
        var size = new SizeF(20, 30);
        var world = new World([new(0, 0, 800, 600)], [new(200, 250, 300, 200)]);
        check(!world.Fits(new(190, 230), size), "Whole body excluded from window interior");
        var landing = world.Move(new(250, 10), size, 0, 580, out _, out bool landed);
        check(landed && Math.Abs(landing.Y - 220) < .01, "Fast fall lands on window top without tunneling");
        var side = world.Move(new(150, 280), size, 500, 0, out bool hitSide, out _);
        check(hitSide && Math.Abs(side.X - 180) < .01, "Window side blocks horizontal movement");
        var underside = world.Move(new(250, 490), size, 0, -400, out _, out bool hitUnderside);
        check(hitUnderside && Math.Abs(underside.Y - 450) < .01, "Window underside blocks upward throws");
        check(world.Supported(landing, size), "Window top supplies stable support");
        var empty = new World(world.Areas, []);
        float throwX = 600, throwY = 100;
        var rebound = PetMotion.Move(empty, new(770, 100), size, ref throwX, ref throwY, .035f, true, out bool bounced);
        check(bounced && throwX < 0 && Math.Abs(throwX) < 600 && throwY == 100 && empty.Fits(rebound, size), "Right screen bounce reverses and softens horizontal velocity");
        throwX = -600; throwY = -100;
        rebound = PetMotion.Move(empty, new(2, 100), size, ref throwX, ref throwY, .035f, true, out bounced);
        check(bounced && throwX > 0 && throwY == -100 && empty.Fits(rebound, size), "Left screen bounce preserves upward motion");
        throwX = 500; throwY = 100;
        rebound = PetMotion.Move(world, new(175, 300), size, ref throwX, ref throwY, .035f, true, out bounced);
        check(bounced && throwX < 0 && world.Fits(rebound, size), "Thrown pet rebounds off a window side without entering it");
        throwX = -500; throwY = 100;
        rebound = PetMotion.Move(world, new(505, 300), size, ref throwX, ref throwY, .035f, true, out bounced);
        check(bounced && throwX > 0 && world.Fits(rebound, size), "Thrown pet rebounds off the opposite window side");
        throwX = 25; throwY = 0;
        PetMotion.Move(empty, new(780, 570), size, ref throwX, ref throwY, .035f, false, out bounced);
        check(bounced && throwX == 0 && throwY == 0, "Walking into a side still stops instead of bouncing");
        throwX = 50; throwY = 800;
        rebound = PetMotion.Move(world, new(250, 210), size, ref throwX, ref throwY, .035f, true, out _);
        check(throwY == 0 && world.Supported(rebound, size), "Thrown pet still lands on window tops");
        var bump = PetMotion.Bump(new(240, 800));
        check(bump.X == 240 && bump.Y < 0, "Midair tap reverses a fast fall and preserves sideways momentum");
        var repeatedBump = PetMotion.Bump(bump);
        check(repeatedBump == bump, "Repeated taps do not stack unlimited upward speed");
        throwX = bump.X; throwY = bump.Y;
        rebound = PetMotion.Move(empty, new(100, 2), size, ref throwX, ref throwY, .035f, true, out _);
        check(throwY == 0 && empty.Fits(rebound, size), "Bumped pet cannot escape through the top screen edge");
        check(!empty.Supported(landing, size), "Closing supporting window resumes gravity");
        var covered = new World(world.Areas, [new(0, 0, 800, 600)]);
        check(covered.FindSpace(new(200, 200), size) == null, "Fully covered desktop hides pet");
        var safe = world.FindSpace(new(250, 300), size);
        check(safe.HasValue && world.Fits(safe.Value, size), "Moving window overlap relocates safely");
        var monitors = new World([new(-800, 0, 800, 560), new(100, 0, 800, 560)], []);
        check(monitors.Fits(new(-700, 530), size), "Negative monitor coordinates supported");
        check(!monitors.Fits(new(-10, 100), size), "Monitor gap forbidden");
        check(!monitors.Fits(new(200, 540), size), "Reserved taskbar area forbidden");
        check(empty.FindSpace(new(250, 300), size).HasValue, "Pet recovers when desktop is exposed");
        var thin = new World(world.Areas, [new(250, 200, 1, 300)]);
        var blocked = thin.Move(new(50, 250), size, 650, 0, out bool thinHit, out _);
        check(thinHit && thin.Fits(blocked, size) && blocked.X <= 230, "One-pixel obstacle blocks fast motion");
        var narrow = new World([new(0, 0, 19, 600)], []);
        check(narrow.FindSpace(PointF.Empty, size) == null, "Narrow desktop strip cannot fit the pet");
        var removedMonitor = empty.FindSpace(new(-700, 200), size);
        check(removedMonitor.HasValue && empty.Fits(removedMonitor.Value, size), "Disconnected monitor relocates pet");
    }
}
