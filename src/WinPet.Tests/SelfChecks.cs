using WinPet.Tests.Desktop;
using WinPet.Tests.Domain;
using WinPet.Tests.Pets.Cactus;

namespace WinPet.Tests;

// A small dependency-free runner: suites report facts; the runner owns reporting and exit status.
internal static class SelfChecks
{
    public static int Run()
    {
        var results = new List<string>();
        int failures = 0;
        void Check(bool passed, string name)
        {
            results.Add($"{(passed ? "PASS" : "FAIL")} {name}");
            if (!passed) Console.Error.WriteLine($"FAIL {name}");
            if (!passed) failures++;
        }
        void Suite(string name, Action<Action<bool, string>> run)
        {
            try { run((passed, message) => Check(passed, $"{name}: {message}")); }
            catch (Exception error) { Check(false, $"{name}: {error.GetType().Name}: {error.Message}"); }
        }
        Suite("Physics", PhysicsChecks.Run);
        Suite("Pet contracts", PetContractChecks.Run);
        Suite("Desktop filtering", DesktopWorldChecks.Run);
        Suite("Routines", RoutineChecks.Run);
        Suite("Animation", AnimationChecks.Run);
        Suite("Cacti artwork", CactusArtworkChecks.Run);
        Suite("Cacti actions", CactusActionChecks.Run);
        Suite("Actions", PetActionChecks.Run);
        Suite("Live pet model", PetSessionChecks.Run);
        Suite("Menu", PetMenuChecks.Run);
        File.WriteAllLines(Path.Combine(AppContext.BaseDirectory, "self-test-results.txt"), results);
        Console.WriteLine($"{results.Count} checks, {failures} failures.");
        return failures == 0 ? 0 : 1;
    }
}
