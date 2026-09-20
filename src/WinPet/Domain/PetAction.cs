namespace WinPet.Domain;

// A menu choice starts a finite, interruptible activity. Availability may add pet-specific rules.
internal sealed class PetAction
{
    private readonly Func<PetActionContext, bool>? availability;
    public string Id { get; }
    public string Label { get; }
    public PetActivity Activity { get; }
    public double Duration { get; }

    public PetAction(string id, string label, PetActivity activity, double duration,
        Func<PetActionContext, bool>? availability = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentNullException.ThrowIfNull(activity);
        if (!double.IsFinite(duration) || duration <= 0)
            throw new ArgumentOutOfRangeException(nameof(duration), "An action needs a finite positive duration.");
        (Id, Label, Activity, Duration, this.availability) = (id, label, activity, duration, availability);
    }

    public bool CanStart(PetActionContext context) => context.CanInteract && (availability?.Invoke(context) ?? true);
}
