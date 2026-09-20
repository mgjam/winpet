using WinPet.Domain;

namespace WinPet.Rendering;

// The desktop renders a definition; simulation needs only IPet.
internal interface IPetVisual : IPet
{
    void Paint(Graphics graphics, PetFrame frame);
}
