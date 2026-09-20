using WinPet.Domain;
using WinPet.Pets.Cactus;
using WinPet.Rendering;

namespace WinPet.Tests.Pets.Cactus;

internal static class CactusActionChecks
{
    public static void Run(Action<bool, string> check)
    {
        var cactus = new CactusPet();
        var water = cactus.Actions.Single();
        var falling = cactus.ComposePose(PetIntent.Falling, 2, 1, default, new(2, water.Duration), water.Activity);
        check(falling.Activity.Definition is null && falling.Face.Mouth == MouthShape.Surprised,
            "Cacti's physical poses suppress its activity art and use its surprised expression");
        check(cactus.Routines.All(r => r.Activity != water.Activity),
            "Watering is user initiated and never chosen by the autonomous profile");
        var open = cactus.ComposePose(PetIntent.Idle, 2, 1, default, new(2, water.Duration), water.Activity);
        var blink = cactus.ComposePose(PetIntent.Idle, 2.8, 1, default, new(2, water.Duration), water.Activity);
        check(open.Face.Eyes.Openness == 1 && blink.Face.Eyes.Openness == 0 && open.Activity == blink.Activity,
            "Watering retains independent blinking");
        bool faceClear = true, insideBounds = true, cleanEndpoints = true;
        using var idle = PetRenderer.Frame(cactus, PetIntent.Idle, 2, 1);
        for (int i = 0; i <= 140; i++)
        {
            double t = i / 20.0;
            using var frame = PetRenderer.Frame(cactus, PetIntent.Idle, 2, 1, default, new(t, water.Duration), water.Activity);
            // Isolate the prop: the activity deliberately changes the pet's gaze.
            using var prop = new Bitmap(cactus.Size.Width, cactus.Size.Height);
            using (var g = Graphics.FromImage(prop))
                PetActivityRenderer.Paint(g, cactus.Size, ((CactusActivity)water.Activity).Paint, t, new RoutinePose(t, water.Duration).Amount);
            for (int x = 0; x < prop.Width; x++)
            for (int y = 0; y < prop.Height; y++)
            {
                if (x >= 28 && x <= 47 && y >= 38 && y <= 51) faceClear &= prop.GetPixel(x, y).A == 0;
                if (x == 0 || y == 0 || x == prop.Width - 1 || y == prop.Height - 1)
                    insideBounds &= prop.GetPixel(x, y).A == 0;
                if (i is 0 or 140) cleanEndpoints &= frame.GetPixel(x, y) == idle.GetPixel(x, y);
            }
        }
        check(faceClear, "Watering art leaves the eyes and mouth unobstructed throughout the pour");
        check(insideBounds, "Watering art has a transparent margin throughout entry, tilt, and exit");
        check(cleanEndpoints, "Watering enters and exits with exactly the idle frame");
    }
}
