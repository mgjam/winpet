using WinPet.Domain;
using WinPet.Rendering;

namespace WinPet.Tests.TestDoubles;

internal sealed class CheckPet : IPetVisual
{
    public string Name => "Example";
    public Size Size { get; init; } = new(20, 30);
    public IReadOnlyList<PetRoutine> Routines { get; init; } = PetRoutine.Default;
    public IReadOnlyList<PetAction> Actions { get; init; } = [];
    public void Paint(Graphics graphics, PetFrame frame)
    {
        if (frame.Activity is not null) graphics.FillRectangle(Brushes.Red, 10, 10, 20, 20);
    }
}
