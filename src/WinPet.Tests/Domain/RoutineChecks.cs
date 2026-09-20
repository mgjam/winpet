using WinPet.Domain;
using WinPet.Pets.Cactus;
using WinPet.Tests.TestDoubles;

namespace WinPet.Tests.Domain;

internal static class RoutineChecks
{
    public static void Run(Action<bool, string> check)
    {
        var pet = new CactusPet();
        var random = new Random(42);
        var seen = new HashSet<PetActivity>();
        var previous = PetRoutine.Idle;
        bool valid = true;
        double totalTime = 0, coreTime = 0, activityTime = 0;
        for (int i = 0; i < 100000; i++)
        {
            var choice = PetRoutine.Choose(pet.Routines, previous, random);
            var routine = choice.Routine;
            valid &= !routine.SameBehaviorAs(previous) && pet.Routines.Contains(routine) &&
                choice.Duration >= routine.MinSeconds && choice.Duration <= routine.MaxSeconds;
            totalTime += choice.Duration;
            if (routine.Activity is { } activity)
            {
                seen.Add(activity);
                activityTime += choice.Duration;
                totalTime += 2.5;
                coreTime += 2.5;
                previous = PetRoutine.Idle;
            }
            else
            {
                if (routine.Intent is PetIntent.Move or PetIntent.Idle or PetIntent.Dormant) coreTime += choice.Duration;
                previous = routine;
            }
        }
        check(valid, "Choices respect duration bounds and avoid immediate repeated behaviors");
        check(new[] { CactusActivities.Read, CactusActivities.Think, CactusActivities.Sing,
            CactusActivities.Violin, CactusActivities.Laptop }.All(seen.Contains), "All of Cacti's activities remain reachable");
        check(coreTime / totalTime > .85 && activityTime / totalTime < .15,
            $"Ordinary life dominates: core {coreTime / totalTime:P1}, activities {activityTime / totalTime:P1}");
        check(PetRoutine.Choose([], previous, random).Routine.Intent == PetIntent.Idle, "An empty profile safely idles");
        var only = new PetRoutine(PetIntent.Observe, 1, 2, 3);
        check(PetRoutine.Choose([only], only, random).Routine == only, "A single-routine profile can repeat");
        var art = new PetActivity("custom");
        var personal = new PetRoutine(PetIntent.Idle, 1, 3, 3, art);
        var customPet = new CheckPet { Routines = [personal] };
        var behavior = new PetBehavior(customPet);
        check(behavior.UpdateGrounded(2, random) && behavior.Activity == art,
            "A new autonomous activity starts without extending shared intents or dispatchers");
        check(behavior.UpdateGrounded(5, random) && behavior.Intent == PetIntent.Idle && behavior.Activity is null &&
            !behavior.UpdateGrounded(7, random), "An autonomous activity clears its artwork and rests on completion");
        check(behavior.UpdateGrounded(7.5, random) && behavior.Activity == art,
            "A pet with only an idle-body activity resumes that activity after resting");
        foreach (var create in new Func<PetRoutine>[] {
            () => new(PetIntent.Falling, 1, 2, 3), () => new(PetIntent.Dragged, 1, 2, 3),
            () => new(PetIntent.React, 1, 2, 3), () => new(PetIntent.Idle, 0, 2, 3),
            () => new(PetIntent.Idle, 1, 0, 3), () => new(PetIntent.Idle, 1, 4, 3),
            () => new(PetIntent.Idle, 1, double.NaN, 3), () => new(PetIntent.Idle, 1, 2, double.PositiveInfinity) })
        {
            bool rejected = false;
            try { create(); } catch (ArgumentException) { rejected = true; }
            check(rejected, "An invalid routine cannot enter the profile");
        }
    }
}
