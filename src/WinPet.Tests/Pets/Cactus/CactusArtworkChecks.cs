using WinPet.Domain;
using WinPet.Pets.Cactus;
using WinPet.Rendering;

namespace WinPet.Tests.Pets.Cactus;

internal static class CactusArtworkChecks
{
    public static void Run(Action<bool, string> check)
    {
        var routinePet = new CactusPet();
        bool contained = true;
        foreach (var activity in new[] { CactusActivities.Read, CactusActivities.Think, CactusActivities.Sing, CactusActivities.Violin })
        for (int i = 0; i < 120; i++)
        {
            double t = i / 12.0;
            using var frame = PetRenderer.Frame(routinePet, PetIntent.Idle,
                t + 2, 1, default, t < 8 ? new RoutinePose(t, 8) : null, t < 8 ? activity : null);
            for (int x = 0; x < 76; x++) contained &= frame.GetPixel(x, 0).A == 0 && frame.GetPixel(x, 91).A == 0;
            for (int y = 0; y < 92; y++) contained &= frame.GetPixel(0, y).A == 0 && frame.GetPixel(75, y).A == 0;
        }
        check(contained, "Every entrance, page turn, floating note and exit stays inside the pet footprint");
        using (var transparentFrame = PetRenderer.Frame(new CactusPet(), PetIntent.Idle, 2, 1))
        {
            check(transparentFrame.GetPixel(38, 9).A == 0, "No green background above cactus outline");
            check(transparentFrame.GetPixel(0, 0).A == 0, "Empty pixels remain transparent for click-through");
            check(transparentFrame.GetPixel(38, 30).A == 255, "Cactus body remains opaque");
        }
    }
}
