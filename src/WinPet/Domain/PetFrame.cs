namespace WinPet.Domain;

// Facts supplied to any visual implementation, without prescribing anatomy or expression.
internal readonly record struct PetFrame(PetIntent Intent, double Seconds, int Facing,
    CursorAttention Attention = default, RoutinePose? Routine = null, PetActivity? Activity = null);
