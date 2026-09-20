using WinPet.Domain;
using WinPet.Rendering;
using WinPet.Tests.TestDoubles;

namespace WinPet.Tests.Domain;

internal static class PetActionChecks
{
    public static void Run(Action<bool, string> check)
    {
        var ready = new PetActionContext(false, true, false, true);
        var allowAction = true;
        var pet = new CheckPet { Size = new(76, 92), Actions = [new("glow", "Glow",
            new PetActivity("glow"), 4, _ => allowAction)] };
        var behavior = new PetBehavior(pet);
        var action = pet.Actions[0];
        var random = new Random(7);
        check(behavior.TryStart(action, ready, 10) && behavior.Activity == action.Activity,
            "A new pet's action starts without a shared mood, composer, or renderer registration");
        check(behavior.PoseAt(10)?.Amount == 0 && behavior.PoseAt(12)?.Amount == 1,
            "Actions start their own timeline and ease into their artwork");
        check(!behavior.UpdateGrounded(12, random) && behavior.ActiveAction == action,
            "Autonomous selection cannot replace an unfinished menu action");
        check(behavior.UpdateGrounded(14, random) && behavior.ActiveAction is null && behavior.Intent == PetIntent.Idle &&
            !behavior.UpdateGrounded(16, random), "Completed actions clear their artwork and rest before choosing a routine");
        foreach (var blocked in new[] { ready with { Paused = true }, ready with { Visible = false },
            ready with { Held = true }, ready with { Grounded = false } })
        {
            check(!behavior.TryStart(action, blocked, 20) && behavior.ActiveAction is null,
                $"Unavailable interaction is rejected: {blocked}");
        }
        var foreignAction = new PetAction("foreign", "Foreign", action.Activity, 4);
        check(!behavior.TryStart(foreignAction, ready, 20), "A pet cannot execute another pet's menu action");
        allowAction = false;
        check(!behavior.TryStart(action, ready, 20), "Pet-specific availability is checked at execution time");
        allowAction = true;
        foreach (var interrupt in new[] { PetIntent.React, PetIntent.Dragged, PetIntent.Falling })
        {
            behavior.TryStart(action, ready, 20);
            behavior.Interrupt(interrupt, 21, 1.8);
            check(behavior.ActiveAction is null && behavior.Activity is null && behavior.Intent == interrupt,
                $"{interrupt} cancels the action without leaving a prop behind");
        }
        behavior.TryStart(action, ready, 30);
        behavior.TryStart(action, ready, 32);
        check(behavior.PoseAt(32)?.Seconds == 0 && !behavior.UpdateGrounded(34, random),
            "Selecting an action again restarts its duration");
        var clock = new PetAnimationClock();
        behavior.TryStart(action, ready, clock.RoutineSeconds);
        clock.Advance(2, false, true, false);
        clock.Advance(10, true, true, false);
        clock.Advance(10, false, false, false);
        check(behavior.PoseAt(clock.RoutineSeconds)?.Seconds == 2 && !behavior.UpdateGrounded(clock.RoutineSeconds, random),
            "Paused and hidden time cannot consume an action's duration");

        IPetVisual artist = pet;
        using var custom = PetRenderer.Frame(artist, PetIntent.Idle, 2, 1, default, new(2, 4), action.Activity);
        check(custom.GetPixel(20, 20).R == 255 && custom.GetPixel(20, 20).A == 255,
            "The shared renderer draws a pet-defined action without a type switch");
        foreach (double duration in new[] { 0, -1, double.NaN, double.PositiveInfinity })
        {
            bool rejected = false;
            try { _ = new PetAction("bad", "Bad", action.Activity, duration); }
            catch (ArgumentOutOfRangeException) { rejected = true; }
            check(rejected, $"Invalid action duration is rejected at definition: {duration}");
        }

    }

}
