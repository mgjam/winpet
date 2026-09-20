using System.Runtime.InteropServices;
using WinPet.Desktop;
using WinPet.Domain;
using WinPet.Rendering;

namespace WinPet.Tests.Desktop;

// Explicit desktop integration diagnostic, separate from the production window.
internal static class WorkspaceChecks
{
    [DllImport("user32.dll")]
    private static extern nint WindowFromPoint(Point point);

    public static async Task<string> Run(PetWindow window, IPetVisual pet)
    {
        window.StopAnimation();
        window.RefreshWorld();
        await Task.Delay(100);
        Guid origin = VirtualDesktops.DesktopOf(window.Handle);
        Guid other = Guid.Empty;
        Native.EnumWindows((candidate, _) =>
        {
            Guid desktop = VirtualDesktops.DesktopOf(candidate);
            if (VirtualDesktops.IsCurrent(candidate) == false && desktop != Guid.Empty && desktop != origin) other = desktop;
            return other == Guid.Empty;
        }, 0);
        var lines = new List<string> { $"Window has workspace ID: {origin != Guid.Empty}", $"Initial current workspace: {VirtualDesktops.IsCurrent(window.Handle)}", $"Initial visible: {window.Visible}", $"Initial valid space: {window.Session.Available}" };
        if (window.Session.Available)
        {
            var corner = new Point((int)Math.Floor(window.Session.Position.X), (int)Math.Floor(window.Session.Position.Y));
            lines.Add($"Transparent corner passes native hit testing: {WindowFromPoint(corner) != window.Handle}");
            using var frame = PetRenderer.Frame(pet, window.Session.Frame);
            var opaque = Enumerable.Range(0, frame.Height).SelectMany(y => Enumerable.Range(0, frame.Width)
                .Select(x => new Point(x, y))).FirstOrDefault(p => frame.GetPixel(p.X, p.Y).A == 255);
            lines.Add($"Pet body receives native hit testing: {WindowFromPoint(new Point(corner.X + opaque.X, corner.Y + opaque.Y)) == window.Handle}");
        }
        if (origin == Guid.Empty)
        {
            lines.Add("Tool window is not assigned to an individual workspace; explicit move test is inapplicable.");
            lines.Add("Switching workspaces should be checked interactively.");
            return string.Join("\n", lines);
        }
        if (other != Guid.Empty)
        {
            lines.Add($"Moved own test window to inactive workspace: {VirtualDesktops.Move(window.Handle, other)}");
            await Task.Delay(300);
            lines.Add($"Window desktop matches inactive target: {VirtualDesktops.DesktopOf(window.Handle) == other}");
            lines.Add($"Current before recovery: {VirtualDesktops.IsCurrent(window.Handle)}");
            window.RefreshWorld();
            lines.Add($"Current after recovery: {VirtualDesktops.IsCurrent(window.Handle)}");
            lines.Add($"Visible after recovery: {window.Visible}");
            lines.Add($"Valid position after recovery: {window.Session.Available}");
        }
        else lines.Add("No inactive workspace window available for recovery check.");
        return string.Join("\n", lines);
    }

}
