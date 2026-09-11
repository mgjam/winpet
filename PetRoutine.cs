namespace WinPet;

// Pets opt into activities they can draw. Physics and interaction always take priority.
internal readonly record struct PetRoutine(Mood Mood, int Weight, double MinSeconds, double MaxSeconds)
{
    public static IReadOnlyList<PetRoutine> Default { get; } = Array.AsReadOnly<PetRoutine>([
        new(Mood.Walk, 3, 3, 8), new(Mood.Look, 1, 3, 8),
        new(Mood.Sit, 1, 3, 8), new(Mood.Sleep, 1, 12, 25), new(Mood.Idle, 2, 3, 8)
    ]);

    public static (Mood Mood, double Duration) Choose(IReadOnlyList<PetRoutine> routines, Mood previous, Random random)
    {
        var choices = routines.Where(r => r.Weight > 0 && r.MinSeconds > 0 &&
            r.MaxSeconds >= r.MinSeconds && double.IsFinite(r.MaxSeconds) &&
            r.Mood is not (Mood.React or Mood.Dragged or Mood.Falling)).ToArray();
        if (choices.Length == 0) return (Mood.Idle, 5);
        if (choices.Any(r => r.Mood != previous)) choices = choices.Where(r => r.Mood != previous).ToArray();
        double ticket = random.NextDouble() * choices.Sum(r => (double)r.Weight);
        var selected = choices[^1];
        foreach (var routine in choices)
        {
            ticket -= routine.Weight;
            if (ticket < 0) { selected = routine; break; }
        }
        return (selected.Mood, selected.MinSeconds + random.NextDouble() * (selected.MaxSeconds - selected.MinSeconds));
    }
}
