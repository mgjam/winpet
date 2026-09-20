using WinPet.Domain;

namespace WinPet.Desktop;

// A Windows view of a pet's available commands. Eligibility and pause state
// belong to the session; the menu contains no behavior or motion rules.
internal sealed class PetMenu : ContextMenuStrip
{
    private readonly PetSession session;
    private readonly ToolStripMenuItem pause;
    private readonly List<(PetAction Action, ToolStripItem Item)> actionItems = [];

    public event Action<PetAction>? ActionRequested;
    public event Action? SpaceRequested;
    public event Action? QuitRequested;

    public PetMenu(IPet pet, PetSession session)
    {
        this.session = session;
        Items.Add(new ToolStripMenuItem(pet.Name) { Enabled = false });
        foreach (var action in pet.Actions)
            actionItems.Add((action, Items.Add(action.Label, null, (_, _) => ActionRequested?.Invoke(action))));
        if (actionItems.Count > 0) Items.Add(new ToolStripSeparator());
        pause = new ToolStripMenuItem();
        pause.Click += (_, _) => { session.Paused = !session.Paused; RefreshItems(); };
        Items.Add(pause);
        Items.Add("Find desktop space", null, (_, _) => SpaceRequested?.Invoke());
        Items.Add(new ToolStripSeparator());
        Items.Add("Quit", null, (_, _) => QuitRequested?.Invoke());
        RefreshItems();
    }

    public void RefreshItems()
    {
        foreach (var (action, item) in actionItems) item.Enabled = session.CanStart(action);
        pause.Checked = session.Paused;
        pause.Text = session.Paused ? "Resume" : "Pause";
    }
}
