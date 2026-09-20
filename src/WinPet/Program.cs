using WinPet.Desktop;
using WinPet.Pets.Cactus;
using WinPet.Previewing;

namespace WinPet;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        if (args.Length == 2 && args[0] == "--generate-previews")
        {
            PreviewGenerator.Generate(Path.GetFullPath(args[1]), new CactusPet(), CactusPreviewCatalog.Scenes);
            return;
        }
        if (args.Contains("--diagnose"))
        {
            var diagnostics = new List<string>();
            var world = DesktopWorld.Read(0, diagnostics);
            var free = world.FindSpace(PointF.Empty, new CactusPet().Size);
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "desktop-check.txt"),
                $"Monitors: {world.Areas.Count}\nVisible obstacle rectangles: {world.Obstacles.Count}\nSpace for cactus: {free.HasValue}\n" +
                string.Join("\n", world.Areas.Select(a => $"Work area: {a}")) + "\n" + string.Join("\n", diagnostics));
            return;
        }
        using var instance = new Mutex(true, "WinPet.DesktopCompanion", out bool first);
        if (!first) return;
        Application.Run(new PetWindow(new CactusPet()));
    }
}
