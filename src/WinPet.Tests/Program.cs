using WinPet.Desktop;
using WinPet.Pets.Cactus;
using WinPet.Previewing;
using WinPet.Tests.Desktop;
using WinPet.Tests.Previewing;

namespace WinPet.Tests;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        if (args.Length == 0) return SelfChecks.Run();
        if (args is ["--preview-check", var directory])
        {
            try
            {
                PreviewChecks.Verify(Path.GetFullPath(directory), CactusPreviewCatalog.Scenes.Select(s => s.Id),
                    PreviewGenerator.Frames, 100 / PreviewGenerator.Fps, new CactusPet().Size);
                Console.WriteLine("Preview checks passed.");
                return 0;
            }
            catch (Exception error)
            {
                Console.Error.WriteLine(error);
                return 1;
            }
        }
        if (args is ["--workspace-check"])
        {
            int exitCode = 0;
            var artwork = new CactusPet();
            using var pet = new PetWindow(artwork);
            pet.Shown += (_, _) => pet.BeginInvoke(async () =>
            {
                try
                {
                    string report = await WorkspaceChecks.Run(pet, artwork);
                    File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "workspace-check.txt"), report);
                    Console.WriteLine(report);
                }
                catch (Exception error)
                {
                    Console.Error.WriteLine(error);
                    exitCode = 1;
                }
                finally { pet.Close(); }
            });
            Application.Run(pet);
            return exitCode;
        }
        Console.Error.WriteLine("Usage: WinPet.Tests [--preview-check <directory> | --workspace-check]");
        return 2;
    }
}
