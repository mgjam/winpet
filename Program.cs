namespace WinPet;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        if (args.Contains("--diagnose"))
        {
            var diagnostics = new List<string>();
            var world = Native.ReadWorld(0, diagnostics);
            var free = world.FindSpace(PointF.Empty, new CactusPet().Size);
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "desktop-check.txt"),
                $"Monitors: {world.Areas.Count}\nVisible obstacle rectangles: {world.Obstacles.Count}\nSpace for cactus: {free.HasValue}\n" +
                string.Join("\n", world.Areas.Select(a => $"Work area: {a}")) + "\n" + string.Join("\n", diagnostics));
            return;
        }
        if (args.Contains("--self-test"))
        {
            Environment.ExitCode = PhysicsChecks.Run();
            return;
        }
        if (args.Contains("--workspace-check"))
        {
            using var pet = new PetWindow(new CactusPet());
            pet.Shown += (_, _) => pet.BeginInvoke(async () =>
            {
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "workspace-check.txt"), await pet.CheckWorkspaceFollowing());
                pet.Close();
            });
            Application.Run(pet);
            return;
        }
        using var instance = new Mutex(true, "WinPet.DesktopCompanion", out bool first);
        if (!first) return;
        Application.Run(new PetWindow(new CactusPet()));
    }
}
