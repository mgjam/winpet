using WinPet.Domain;
using WinPet.Rendering;

namespace WinPet.Tests.Domain;

internal static class PetContractChecks
{
    public static void Run(Action<bool, string> check)
    {
        var world = new World([new(0, 0, 800, 600)], []);
        IPet definition = new SimulationPet();
        var simulation = new PetSession(definition, new(100, 568), new Random(1));
        simulation.Observe(world);
        simulation.Advance(.02, new(140, 580));
        check(simulation.Available && simulation.Frame.Intent == PetIntent.Idle &&
            simulation.Frame.Attention.Attention > 0,
            "A simulation-only definition needs no painter, face, body, or animation composer");
        check(definition.Routines.All(r => r.Intent == PetIntent.Idle),
            "The default profile does not prescribe walking, looking, or sleeping");

        foreach (var pet in new SamplePet[] { new BlobPet(), new MouthPet(), new EyesPet(), new FacePet(), new CarPet() })
        {
            var model = new PetSession(pet, new(100, 568), new Random(1));
            model.Observe(world);
            check(model.TryStart(pet.Actions[0]), $"{pet.Name}: accepts a pet-owned action");
            model.Advance(.02, new(150, 580));
            check(ReferenceEquals(model.Frame.Activity, pet.Actions[0].Activity) &&
                model.Frame.Routine?.Seconds > 0 && model.Frame.Attention.X > 0,
                $"{pet.Name}: receives activity identity, timing, and attention without anatomy");
            using var frame = PetRenderer.Frame(pet, model.Frame);
            check(frame.GetPixel(pet.Ink.X, pet.Ink.Y).A > 0 && frame.GetPixel(0, 0).A == 0,
                $"{pet.Name}: renders its own shape through the production renderer");
            var frozen = model.Frame;
            model.Paused = true;
            model.Advance(1, PointF.Empty);
            check(model.Frame == frozen, $"{pet.Name}: pause freezes its visual inputs");
            model.Paused = false;
            model.Press(new(110, 578), 1);
            model.Advance(1.02, new(140, 520));
            check(model.Intent == PetIntent.Dragged && model.Frame.Activity is null,
                $"{pet.Name}: dragging interrupts actions without requiring a physical expression");
            model.Release(1.03);
            check(model.Intent == PetIntent.Falling && model.Velocity.Y < 0,
                $"{pet.Name}: shared drag-and-throw physics does not depend on its artwork");
        }

        var car = new CarPet();
        var driving = new PetSession(car, new(100, 568), new Random(1));
        driving.Observe(world);
        for (int i = 1; i <= 150; i++) driving.Advance(i * .02, PointF.Empty);
        check(driving.Intent == PetIntent.Move && driving.Position.X != 100,
            "A car can drive using the Move state without a stride or legs");
        var charging = new PetActivity("charging");
        var dormant = new PetRoutine(PetIntent.Dormant, 1, 3, 3, charging);
        var behavior = new PetBehavior(new SimulationPet { Routines = [dormant] });
        behavior.UpdateGrounded(2, new Random(1));
        check(behavior.Intent == PetIntent.Dormant && behavior.Activity == charging,
            "Dormant objects may run their own activity; closed eyes and suppressed props are not shared policy");
        using var unlit = PetRenderer.Frame(car, new PetFrame(PetIntent.Dormant, 2, 1));
        using var lit = PetRenderer.Frame(car, new PetFrame(PetIntent.Dormant, 2, 1, new(1, 0, 1)));
        check(unlit.GetPixel(25, 14) != lit.GetPixel(25, 14),
            "An object can express attention with headlights, even while dormant");
    }

    private sealed class SimulationPet : IPet
    {
        public string Name => "Simulation only";
        public Size Size => new(32, 32);
        public IReadOnlyList<PetRoutine> Routines { get; init; } = PetRoutine.Default;
    }

    private abstract class SamplePet : IPetVisual
    {
        public abstract string Name { get; }
        public Size Size => new(32, 32);
        public abstract Point Ink { get; }
        public IReadOnlyList<PetAction> Actions { get; } = [new("activate", "Activate", new PetActivity("active"), 4)];
        public IReadOnlyList<PetRoutine> Routines => [new(PetIntent.Move, 1, 10, 10)];
        public abstract void Paint(Graphics graphics, PetFrame frame);
    }

    private sealed class BlobPet : SamplePet
    {
        public override string Name => "Faceless blob";
        public override Point Ink => new(16, 16);
        public override void Paint(Graphics graphics, PetFrame frame)
            => graphics.FillEllipse(frame.Activity is null ? Brushes.Purple : Brushes.Orange, 4, 4, 24, 24);
    }

    private sealed class MouthPet : SamplePet
    {
        public override string Name => "Mouth only";
        public override Point Ink => new(16, 19);
        public override void Paint(Graphics graphics, PetFrame frame)
            => graphics.DrawLine(Pens.Red, 8, 19, 24, 19);
    }

    private sealed class FacePet : SamplePet
    {
        public override string Name => "Face without body";
        public override Point Ink => new(10, 10);
        public override void Paint(Graphics graphics, PetFrame frame)
        {
            graphics.FillEllipse(Brushes.Black, 8, 8, 5, 5);
            graphics.FillEllipse(Brushes.Black, 20, 8, 5, 5);
            graphics.DrawLine(Pens.Red, 10, 23, 23, 23);
        }
    }

    private sealed class EyesPet : SamplePet
    {
        public override string Name => "Eyes without mouth";
        public override Point Ink => new(10, 10);
        public override void Paint(Graphics graphics, PetFrame frame)
        {
            graphics.FillEllipse(Brushes.Black, 8, 8, 5, 5);
            graphics.FillEllipse(Brushes.Black, 20, 8, 5, 5);
        }
    }

    private sealed class CarPet : SamplePet
    {
        public override string Name => "Car";
        public override Point Ink => new(16, 16);
        public override void Paint(Graphics graphics, PetFrame frame)
        {
            graphics.FillRectangle(Brushes.Blue, 4, 12, 24, 10);
            graphics.FillEllipse(Brushes.Black, 6, 20, 6, 6);
            graphics.FillEllipse(Brushes.Black, 20, 20, 6, 6);
            if (frame.Attention.Attention > .5f) graphics.FillRectangle(Brushes.Yellow, 24, 13, 3, 3);
        }
    }
}
