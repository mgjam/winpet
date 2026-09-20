using WinPet.Domain;
using WinPet.Tests.TestDoubles;

namespace WinPet.Tests.Domain;

internal static class PetSessionChecks
{
    public static void Run(Action<bool, string> check)
    {
        var floor = new World([new(0, 0, 800, 600)], []);
        var art = new PetActivity("custom");
        var water = new PetAction("test", "Test", art, 4);
        var pet = new CheckPet { Actions = [water] };
        PetSession Grounded()
        {
            var session = new PetSession(pet, new(100, 570), new Random(42));
            session.Observe(floor);
            return session;
        }
        var model = Grounded();
        check(model.Available && model.CanStart(water) && model.TryStart(water), "A supported pet accepts its action");
        model.Advance(.02, new(110, 580));
        var position = model.Position;
        var seconds = model.RoutineSeconds;
        var pose = model.Frame;
        model.Advance(5, new(300, 500), menuOpen: true);
        check(model.Position == position && model.RoutineSeconds == seconds && model.Frame == pose,
            "An open menu freezes motion, attention, and both animation channels");
        model.Advance(5.02, new(110, 580));
        check(Math.Abs(model.RoutineSeconds - seconds - .02) < .0001, "Closing a menu does not accumulate catch-up time");
        model.Paused = true;
        pose = model.Frame;
        model.Advance(6, new(400, 400));
        check(model.Frame == pose && !model.CanStart(water), "Explicit pause freezes the pose and blocks menu actions");
        model.Paused = false;
        model.Press(new(110, 580), 6);
        seconds = model.RoutineSeconds;
        double blink = model.AnimationSeconds;
        model.Advance(6.02, new(110, 580));
        check(model.Held && !model.Dragging && model.RoutineSeconds == seconds && model.AnimationSeconds > blink,
            "Holding freezes only routine time and keeps blinking alive");
        model.Release(6.03);
        check(model.Intent == PetIntent.React && model.ActiveAction is null && model.Velocity == PointF.Empty,
            "A grounded tap reacts and interrupts the requested action");

        model = Grounded();
        model.TryStart(water);
        model.Press(new(110, 580), 0);
        model.Advance(.02, new(114, 580));
        check(!model.Dragging, "Small pointer jitter does not pick up the pet");
        model.Advance(.04, new(140, 500));
        check(model.Dragging && model.Position == new PointF(130, 490) && model.ActiveAction is null,
            "Dragging preserves the grab offset and clears activity artwork");
        model.Release(.05);
        check(!model.Held && model.Intent == PetIntent.Falling && model.Velocity.X > 0 && model.Velocity.Y < 0 &&
            Math.Abs(model.Velocity.X) <= 650 && Math.Abs(model.Velocity.Y) <= 650, "Releasing a moving drag produces a bounded throw");
        model.Press(new(140, 500), .06);
        model.Release(.07);
        check(model.Velocity.X > 0 && model.Velocity.Y == -400, "A midair tap preserves sideways momentum and bumps upward");
        model.Press(new(140, 500), .08);
        model.Advance(.1, new(160, 470));
        model.Release(.3);
        check(model.Velocity == PointF.Empty, "A release after motion stops does not reuse a stale throw");
        model.Press(new(160, 470), .31);
        model.CancelHold();
        check(!model.Held && !model.Dragging && model.Velocity == PointF.Empty && model.Intent == PetIntent.Falling,
            "Losing mouse capture releases the held pet safely");

        model = Grounded();
        var obstacle = new World(floor.Areas, [new(140, 0, 30, 600)]);
        model.Observe(obstacle);
        model.Press(new(110, 580), 0);
        model.Advance(.02, new(210, 580));
        check(obstacle.Fits(model.Position, pet.Size) && model.Position.X <= 120,
            "Live dragging cannot pass through an application window");
        model.LeaveWorkspace();
        check(!model.Held && model.Velocity == PointF.Empty, "Leaving a workspace cancels the grab and momentum");

        var platform = new World(floor.Areas, [new(80, 300, 200, 200)]);
        model = new PetSession(pet, new(100, 270), new Random(42));
        model.Observe(platform);
        model.TryStart(water);
        model.Observe(floor);
        model.Advance(.02, PointF.Empty);
        check(model.Intent == PetIntent.Falling && model.Position.Y > 270 && model.ActiveAction is null,
            "Removing support resumes gravity and interrupts the action");
        for (int i = 2; i < 150; i++) model.Advance(i * .02, PointF.Empty);
        check(floor.Supported(model.Position, pet.Size), "The live pet lands after falling without tunneling");
        var covered = new World(floor.Areas, floor.Areas);
        model.Observe(covered);
        seconds = model.RoutineSeconds;
        model.Advance(10, PointF.Empty);
        check(!model.Available && !model.CanStart(water) && model.RoutineSeconds == seconds,
            "An unavailable desktop freezes time and disables actions");
        model.Observe(floor);
        model.Advance(10.02, PointF.Empty);
        check(model.Available && floor.Fits(model.Position, pet.Size), "The pet recovers into valid space when the desktop reappears");

        var mutable = new List<RectangleF> { new(0, 0, 800, 600) };
        var snapshot = new World(mutable, []);
        mutable.Clear();
        check(snapshot.Fits(new(100, 100), pet.Size), "A world snapshot cannot be changed through its source collections");
    }
}
