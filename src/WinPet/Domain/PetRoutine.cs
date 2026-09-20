namespace WinPet.Domain;

// Valid routine definitions carry a behavior intent and optional pet-owned activity.
internal sealed class PetRoutine
{
    public PetIntent Intent { get; }
    public PetActivity? Activity { get; }
    public int Weight { get; }
    public double MinSeconds { get; }
    public double MaxSeconds { get; }

    public PetRoutine(PetIntent intent, int weight, double minSeconds, double maxSeconds, PetActivity? activity = null)
    {
        if (!Enum.IsDefined(intent) || intent is PetIntent.React or PetIntent.Dragged or PetIntent.Falling)
            throw new ArgumentOutOfRangeException(nameof(intent), "Physical interactions cannot be autonomous routines.");
        if (weight <= 0) throw new ArgumentOutOfRangeException(nameof(weight));
        if (!double.IsFinite(minSeconds) || minSeconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(minSeconds));
        if (!double.IsFinite(maxSeconds) || maxSeconds < minSeconds)
            throw new ArgumentOutOfRangeException(nameof(maxSeconds));
        (Intent, Weight, MinSeconds, MaxSeconds, Activity) = (intent, weight, minSeconds, maxSeconds, activity);
    }

    public bool SameBehaviorAs(PetRoutine other) => Intent == other.Intent && Activity == other.Activity;

    public static PetRoutine Idle { get; } = new(PetIntent.Idle, 1, 5, 5);
    public static IReadOnlyList<PetRoutine> Default { get; } = Array.AsReadOnly<PetRoutine>([Idle]);

    public static RoutineChoice Choose(IReadOnlyList<PetRoutine> routines, PetRoutine? previous, Random random)
    {
        if (routines.Count == 0) return new(Idle, 5);
        var alternatives = routines.Where(r => previous is null || !r.SameBehaviorAs(previous)).ToArray();
        IReadOnlyList<PetRoutine> choices = alternatives.Length > 0 ? alternatives : routines;
        double ticket = random.NextDouble() * choices.Sum(r => (double)r.Weight);
        var selected = choices[^1];
        foreach (var routine in choices)
        {
            ticket -= routine.Weight;
            if (ticket < 0) { selected = routine; break; }
        }
        return new(selected, selected.MinSeconds + random.NextDouble() * (selected.MaxSeconds - selected.MinSeconds));
    }
}
