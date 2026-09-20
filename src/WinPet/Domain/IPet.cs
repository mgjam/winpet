namespace WinPet.Domain;

// Behavior and footprint only. A pet need not have a body, face, or any artwork.
internal interface IPet
{
    string Name { get; }
    Size Size { get; }
    IReadOnlyList<PetRoutine> Routines => PetRoutine.Default;
    IReadOnlyList<PetAction> Actions => Array.Empty<PetAction>();
    PointF AttentionOrigin => new(Size.Width / 2f, Size.Height / 2f);
}
