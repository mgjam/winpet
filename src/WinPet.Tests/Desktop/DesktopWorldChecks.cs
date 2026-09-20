using WinPet.Desktop;

namespace WinPet.Tests.Desktop;

internal static class DesktopWorldChecks
{
    public static void Run(Action<bool, string> check)
    {
        check(DesktopWorld.IsFullyTransparent(true, 0, 2), "Invisible alpha-zero NVIDIA-style overlay is excluded");
        check(!DesktopWorld.IsFullyTransparent(true, 255, 2), "Opaque layered app remains an obstacle");
        check(!DesktopWorld.IsFullyTransparent(true, 128, 2), "Partly transparent app remains an obstacle");
        check(!DesktopWorld.IsFullyTransparent(false, 0, 0), "Missing transparency attributes do not hide real apps");
        check(!DesktopWorld.IsFullyTransparent(true, 0, 1), "Color-key transparency alone does not exclude the whole app");
        check(!DesktopWorld.IsPhysicalObstacle(0, false, false), "Other workspace excluded even before cloaking updates");
        check(!DesktopWorld.IsPhysicalObstacle(2, true, false), "Cloaked window excluded during workspace transition");
        check(DesktopWorld.IsPhysicalObstacle(0, true, false), "Visible current-workspace app remains solid");
        check(DesktopWorld.IsPhysicalObstacle(0, null, false), "Unavailable workspace API preserves ordinary obstacles");
    }
}
