using WinPet.Domain;
using WinPet.Pets.Cactus;
using WinPet.Rendering;

namespace WinPet.Tests.Pets.Cactus;

internal static class AnimationChecks
{
    private static readonly (string Name, CactusActivity Activity)[] Activities = [
        ("Reading", CactusActivities.Read), ("Thinking", CactusActivities.Think),
        ("Singing", CactusActivities.Sing), ("Violin", CactusActivities.Violin),
        ("Laptop", CactusActivities.Laptop)
    ];

    public static void Run(Action<bool, string> check)
    {
        var pet = new CactusPet();
        var routine = new RoutinePose(5, 24);
        var left = new CursorAttention(-1, 0, 1);
        var right = new CursorAttention(1, 0, 1);
        var samples = Enum.GetValues<PetIntent>().Select(intent => (Name: intent.ToString(), Intent: intent, Activity: (CactusActivity?)null))
            .Concat(Activities.Select(a => (a.Name, Intent: PetIntent.Idle, Activity: (CactusActivity?)a.Activity)));
        foreach (var sample in samples)
        {
            var open = pet.ComposePose(sample.Intent, 2, 1, left, routine, sample.Activity);
            var blink = pet.ComposePose(sample.Intent, 2.8, 1, left, routine, sample.Activity);
            var reopened = pet.ComposePose(sample.Intent, 3.1, 1, left, routine, sample.Activity);
            check(sample.Intent == PetIntent.Dormant ? open.Face.Eyes.Openness == 0 && reopened.Face.Eyes.Openness == 0
                : open.Face.Eyes.Openness == 1 && blink.Face.Eyes.Openness == 0 && reopened.Face.Eyes.Openness == 1,
                $"{sample.Name}: eyelids close and reopen independently, except sustained sleep");
            var lookingRight = pet.ComposePose(sample.Intent, 2, 1, right, routine, sample.Activity);
            check(sample.Intent == PetIntent.Dormant ? open.Face.Eyes == lookingRight.Face.Eyes
                : open.Face.Eyes.X < -1.5f && lookingRight.Face.Eyes.X > 1.5f,
                $"{sample.Name}: cursor attention takes priority when awake");
            check(open.Activity == blink.Activity && open.Face.Mouth == blink.Face.Mouth,
                $"{sample.Name}: blinking leaves activity props and mouth alone");
        }
        var read = pet.ComposePose(PetIntent.Idle, 2, 1, default, routine, CactusActivities.Read);
        var think = pet.ComposePose(PetIntent.Idle, 2, 1, default, routine, CactusActivities.Think);
        check(read.Face.Eyes.Y > 1.5f && think.Face.Eyes.Y < -1,
            "Absent cursor leaves reading focused on book and thinking looking upward");
        foreach (var (name, activity) in Activities)
        {
            var up = pet.ComposePose(PetIntent.Idle, 2, 1, new(0, -1, 1), routine, activity);
            var down = pet.ComposePose(PetIntent.Idle, 2, 1, new(0, 1, 1), routine, activity);
            check(up.Face.Eyes.Y < -1 && down.Face.Eyes.Y > 1, $"{name}: vertical attention works");
            var idle = pet.ComposePose(PetIntent.Idle, 2, 1, left, null);
            var entering = pet.ComposePose(PetIntent.Idle, 2, 1, left, new(0, 24), activity);
            var ending = pet.ComposePose(PetIntent.Idle, 2, 1, left, new(24, 24), activity);
            check(entering.Face.Eyes == idle.Face.Eyes && ending.Face.Eyes == idle.Face.Eyes &&
                entering.Activity.Amount == 0 && ending.Activity.Amount == 0,
                $"{name}: entrance and exit converge to idle gaze without resetting eyelids");
            using var open = PetRenderer.Frame(pet, PetIntent.Idle, 2, 1, default, routine, activity);
            using var shut = PetRenderer.Frame(pet, PetIntent.Idle, 2.8, 1, default, routine, activity);
            bool changed = false;
            for (int x = 29; x < 49; x++)
            for (int y = 38; y < 47; y++) changed |= open.GetPixel(x, y) != shut.GetPixel(x, y);
            check(changed, $"{name}: rendered eyes visibly change during blinking");
        }
        var partial = pet.ComposePose(PetIntent.Idle, 2.74, 1, left, routine, CactusActivities.Think);
        check(partial.Face.Eyes.Openness > 0 && partial.Face.Eyes.Openness < 1, "Blink closes progressively");
        check(pet.ComposePose(PetIntent.Idle, 2, 1, default, null, CactusActivities.Sing).Face.Mouth == MouthShape.Whistle &&
            pet.ComposePose(PetIntent.Falling, 2, 1, default, null).Face.Mouth == MouthShape.Surprised &&
            pet.ComposePose(PetIntent.Dragged, 2, 1, default, routine, CactusActivities.Read).Activity.Definition is null,
            "Physical interaction overrides activity props and whistling expression");
        check(new RoutinePose(0, 8).Amount == 0 && new RoutinePose(1, 8).Amount == 1 &&
            new RoutinePose(6.8, 8).Amount == 1 && new RoutinePose(8, 8).Amount == 0,
            "Activity transition envelopes ease from and return to idle");
        var clock = new PetAnimationClock();
        clock.Advance(1, false, true, false);
        clock.Advance(1, false, true, true);
        check(clock.Seconds == 2 && clock.RoutineSeconds == 1, "Holding pauses the routine but keeps the blink clock alive");
        clock.Advance(1, true, true, false);
        clock.Advance(1, false, false, false);
        check(clock.Seconds == 2 && clock.RoutineSeconds == 1, "Pause and hidden desktop space freeze both clocks");
        var target = CursorAttention.Toward(new(38, 43), new(100, 43));
        var eased = default(CursorAttention).Approach(target, .016f);
        check(eased.Attention > 0 && eased.Attention < target.Attention &&
            eased.Approach(default, .016f).Attention < eased.Attention, "Cursor arrival and departure ease continuously");
    }
}
