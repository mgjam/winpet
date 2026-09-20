using WinPet.Desktop;
using WinPet.Domain;
using WinPet.Pets.Cactus;
using WinPet.Tests.TestDoubles;

namespace WinPet.Tests.Desktop;

internal static class PetMenuChecks
{
    public static void Run(Action<bool, string> check)
    {
        var pet = new CactusPet();
        var session = new PetSession(pet, new(100, 508), new Random(42));
        session.Observe(new World([new(0, 0, 800, 600)], []));
        using var menu = new PetMenu(pet, session);
        var water = menu.Items.OfType<ToolStripMenuItem>().Single(i => i.Text == "Water");
        PetAction? requested = null;
        menu.ActionRequested += action => requested = action;
        water.PerformClick();
        check(water.Enabled && requested == pet.Actions[0], "A menu choice requests the pet's declared action");
        var pause = menu.Items.OfType<ToolStripMenuItem>().Single(i => i.Text == "Pause");
        pause.PerformClick();
        check(session.Paused && pause.Checked && pause.Text == "Resume" && !water.Enabled,
            "Pause updates the model and disables actions from the same source of truth");
        pause.PerformClick();
        check(!session.Paused && !pause.Checked && pause.Text == "Pause" && water.Enabled,
            "Resume re-enables available actions");
        session.Observe(new World([], []));
        menu.RefreshItems();
        check(!water.Enabled && pause.Enabled && menu.Items.OfType<ToolStripMenuItem>().Last().Enabled,
            "An unavailable pet disables only its custom actions, keeping standard controls usable");
        bool space = false, quit = false;
        menu.SpaceRequested += () => space = true;
        menu.QuitRequested += () => quit = true;
        menu.Items.OfType<ToolStripMenuItem>().Single(i => i.Text == "Find desktop space").PerformClick();
        menu.Items.OfType<ToolStripMenuItem>().Single(i => i.Text == "Quit").PerformClick();
        check(space && quit, "Standard menu commands are routed to the desktop adapter");
        var plain = new CheckPet();
        using var standard = new PetMenu(plain, new PetSession(plain, PointF.Empty, new Random(42)));
        check(standard.Items.Count == 5, "A pet with no custom actions has only the standard controls");
        check(!menu.IsHandleCreated && !standard.IsHandleCreated, "Menu checks create no desktop window or tray icon");
    }
}
