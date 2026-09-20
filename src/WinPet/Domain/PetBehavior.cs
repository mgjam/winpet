namespace WinPet.Domain;

// Owns one timeline for autonomous routines and requested actions.
internal sealed class PetBehavior(IPet pet)
{
    private const double LandingRest = 1.4;
    private const double ActivityRest = 2.5;
    private double started, duration, nextBehavior = 2;
    public PetIntent Intent { get; private set; } = PetIntent.Idle;
    public PetRoutine? ActiveRoutine { get; private set; } = PetRoutine.Idle;
    public PetAction? ActiveAction { get; private set; }
    public PetActivity? Activity => ActiveAction?.Activity ?? ActiveRoutine?.Activity;

    public RoutinePose? PoseAt(double now) => Activity is null ? null : new(now - started, duration);

    public bool CanStart(PetAction action, PetActionContext context)
        => pet.Actions.Contains(action) && action.CanStart(context);

    public bool TryStart(PetAction action, PetActionContext context, double now)
    {
        if (!CanStart(action, context)) return false;
        Enter(PetIntent.Idle, action.Duration, now);
        ActiveAction = action;
        return true;
    }

    public void Interrupt(PetIntent intent, double now, double seconds = 0) => Enter(intent, seconds, now);

    // The pet calls this only when supported and unpaused.
    public bool UpdateGrounded(double now, Random random)
    {
        if (Intent is PetIntent.Falling or PetIntent.Dragged) Enter(PetIntent.Rest, LandingRest, now);
        if (now < nextBehavior) return false;
        var next = Activity is not null
            ? new RoutineChoice(PetRoutine.Idle, ActivityRest)
            : PetRoutine.Choose(pet.Routines, ActiveRoutine, random);
        Enter(next.Routine.Intent, next.Duration, now);
        ActiveRoutine = next.Routine;
        return true;
    }

    private void Enter(PetIntent intent, double seconds, double now)
    {
        Intent = intent;
        ActiveAction = null;
        ActiveRoutine = pet.Routines.FirstOrDefault(r => r.Intent == intent && r.Activity is null)
            ?? (intent == PetIntent.Idle ? PetRoutine.Idle : null);
        started = now;
        duration = seconds;
        nextBehavior = now + seconds;
    }
}
