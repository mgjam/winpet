namespace WinPet.Domain;

// Identity scheduled by routines and actions. Visual details belong to the pet.
internal class PetActivity
{
    public string Id { get; }

    public PetActivity(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        Id = id;
    }
}
