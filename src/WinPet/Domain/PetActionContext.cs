namespace WinPet.Domain;

internal readonly record struct PetActionContext(bool Paused, bool Visible, bool Held, bool Grounded)
{
    public bool CanInteract => !Paused && Visible && !Held && Grounded;
}
