namespace WinPet;

internal static class AnimationChecks
{
    public static void Run(Action<bool, string> check)
    {
        var routine = new RoutinePose(5, 24);
        var left = new PetGaze(-1, 0, 1);
        var right = new PetGaze(1, 0, 1);
        foreach (var intent in Enum.GetValues<Mood>())
        {
            var open = PetAnimation.Compose(intent, 2, 1, left, routine);
            var blink = PetAnimation.Compose(intent, 2.8, 1, left, routine);
            var reopened = PetAnimation.Compose(intent, 3.1, 1, left, routine);
            check(intent == Mood.Sleep ? open.Face.Eyes.Openness == 0 && reopened.Face.Eyes.Openness == 0
                : open.Face.Eyes.Openness == 1 && blink.Face.Eyes.Openness == 0 && reopened.Face.Eyes.Openness == 1,
                $"{intent}: eyelids close and reopen independently, except sustained sleep");
            var lookingRight = PetAnimation.Compose(intent, 2, 1, right, routine);
            check(intent == Mood.Sleep ? open.Face.Eyes == lookingRight.Face.Eyes
                : open.Face.Eyes.X < -1.5f && lookingRight.Face.Eyes.X > 1.5f,
                $"{intent}: cursor attention takes priority when awake");
            check(open.Activity == blink.Activity && open.Face.Mouth == blink.Face.Mouth,
                $"{intent}: blinking leaves activity props and mouth alone");
        }
        var read = PetAnimation.Compose(Mood.Read, 2, 1, default, routine);
        var think = PetAnimation.Compose(Mood.Think, 2, 1, default, routine);
        check(read.Face.Eyes.Y > 1.5f && think.Face.Eyes.Y < -1,
            "Absent cursor leaves reading focused on book and thinking looking upward");
        foreach (var activity in new[] { Mood.Read, Mood.Think, Mood.Sing })
        {
            var up = PetAnimation.Compose(activity, 2, 1, new(0, -1, 1), routine);
            var down = PetAnimation.Compose(activity, 2, 1, new(0, 1, 1), routine);
            check(up.Face.Eyes.Y < -1 && down.Face.Eyes.Y > 1,
                $"{activity}: vertical attention works along with horizontal attention");
            var idle = PetAnimation.Compose(Mood.Idle, 2, 1, left);
            var entering = PetAnimation.Compose(activity, 2, 1, left, new(0, 24));
            var ending = PetAnimation.Compose(activity, 2, 1, left, new(24, 24));
            check(entering.Face.Eyes == idle.Face.Eyes && ending.Face.Eyes == idle.Face.Eyes &&
                entering.Activity.Amount == 0 && ending.Activity.Amount == 0,
                $"{activity}: entrance and exit converge to idle gaze without resetting eyelids");
        }
        var partial = PetAnimation.Compose(Mood.Think, 2.74, 1, left, routine);
        check(partial.Face.Eyes.Openness > 0 && partial.Face.Eyes.Openness < 1,
            "Blink closes progressively instead of popping between two frames");
        check(PetAnimation.Compose(Mood.Sing, 2, 1).Face.Mouth == MouthShape.Whistle &&
            PetAnimation.Compose(Mood.Falling, 2, 1).Face.Mouth == MouthShape.Surprised &&
            PetAnimation.Compose(Mood.Dragged, 2, 1, default, routine).Activity.Kind == PetActivity.None,
            "Physical interaction overrides activity props and whistling expression");
        var clock = new PetAnimationClock();
        clock.Advance(1, false, true, false);
        clock.Advance(1, false, true, true);
        check(clock.Seconds == 2 && clock.RoutineSeconds == 1, "Holding pauses the routine but keeps the blink clock alive");
        clock.Advance(1, true, true, false);
        clock.Advance(1, false, false, false);
        check(clock.Seconds == 2 && clock.RoutineSeconds == 1, "Pause and hidden desktop space freeze both animation clocks");
        var target = PetGaze.Toward(new(38, 43), new(100, 43), Mood.Think);
        var eased = default(PetGaze).Approach(target, .016f);
        check(eased.Attention > 0 && eased.Attention < target.Attention &&
            eased.Approach(default, .016f).Attention < eased.Attention,
            "Cursor arrival and departure ease continuously during activities");
        CheckRenderedEyes(check);
    }

    private static void CheckRenderedEyes(Action<bool, string> check)
    {
        Mood[] rows = [Mood.Think, Mood.Read, Mood.Sing, Mood.Walk, Mood.Sit, Mood.Sleep];
        var pet = new CactusPet();
        // Check the rendered face, not just the resolver: props must not mask eyelids.
        foreach (var intent in rows.Where(m => m != Mood.Sleep))
        {
            using var open = PetRenderer.Frame(pet, intent, 2, 1, default, new(5, 24));
            using var shut = PetRenderer.Frame(pet, intent, 2.8, 1, default, new(5, 24));
            bool changed = false;
            for (int x = 29; x < 49; x++)
            for (int y = 38; y < 47; y++) changed |= open.GetPixel(x, y) != shut.GetPixel(x, y);
            check(changed, $"{intent}: rendered eye region visibly changes during a blink");
        }
    }
}
